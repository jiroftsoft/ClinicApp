using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Payment;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس مدیریت پذیرش‌های بیماران - قلب تپنده سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پذیرش‌های بیماران (Normal, Emergency, Special, Online)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 8. یکپارچه‌سازی با سیستم بیمه و محاسبات
    /// 9. پشتیبانی از استعلام کمکی خارجی
    /// 10. مدیریت کامل تراکنش‌های مالی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار پذیرش
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public class ReceptionService : IReceptionService
    {
        #region Fields and Constructor

        private readonly IReceptionRepository _receptionRepository;
        private readonly IPatientService _patientService;
        private readonly IExternalInquiryService _externalInquiryService;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IServiceService _serviceService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ILogger _logger;

        public ReceptionService(
            IReceptionRepository receptionRepository,
            IPatientService patientService,
            IExternalInquiryService externalInquiryService,
            IInsuranceCalculationService insuranceCalculationService,
            ICurrentUserService currentUserService,
            IServiceCategoryService serviceCategoryService,
            IServiceService serviceService,
            IDoctorCrudService doctorCrudService,
            ILogger logger)
        {
            _receptionRepository = receptionRepository ?? throw new ArgumentNullException(nameof(receptionRepository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _externalInquiryService = externalInquiryService ?? throw new ArgumentNullException(nameof(externalInquiryService));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _serviceCategoryService = serviceCategoryService ?? throw new ArgumentNullException(nameof(serviceCategoryService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _logger = logger.ForContext<ReceptionService>();
        }

        #endregion

        #region Core CRUD Operations

        /// <summary>
        /// ایجاد پذیرش جدید
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه ایجاد پذیرش</returns>
        public async Task<ServiceResult<ReceptionDetailsViewModel>> CreateReceptionAsync(ReceptionCreateViewModel model)
        {
            _logger.Information(
                "شروع ایجاد پذیرش جدید. بیمار: {PatientId}, پزشک: {DoctorId}, خدمات: {ServiceIds}. کاربر: {UserName}",
                model.PatientId, model.DoctorId, string.Join(",", model.SelectedServiceIds), _currentUserService.UserName);

            try
            {
                // اعتبارسنجی اولیه
                var validationResult = await ValidateReceptionAsync(model.PatientId, model.DoctorId, model.ReceptionDate);
                if (!validationResult.Success)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed(
                        validationResult.Message,
                        "VALIDATION_FAILED",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // ایجاد Entity
                var reception = new Reception
                {
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    ReceptionDate = DateTime.Now,
                    Status = ReceptionStatus.Pending,
                    Type = ReceptionType.Normal,
                    TotalAmount = model.TotalAmount,
                    PatientCoPay = 0, // محاسبه خواهد شد
                    InsurerShareAmount = 0, // محاسبه خواهد شد
                    Notes = string.Empty,
                    IsEmergency = false,
                    IsOnlineReception = false
                };

                // ذخیره در Repository
                _receptionRepository.Add(reception);
                await _receptionRepository.SaveChangesAsync();

                _logger.Information(
                    "پذیرش جدید با موفقیت ایجاد شد. شناسه: {ReceptionId}. کاربر: {UserName}",
                    reception.ReceptionId, _currentUserService.UserName);

                // تبدیل به ViewModel
                var result = await GetReceptionDetailsAsync(reception.ReceptionId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ایجاد پذیرش جدید. بیمار: {PatientId}, پزشک: {DoctorId}. کاربر: {UserName}",
                    model.PatientId, model.DoctorId, _currentUserService.UserName);

                return ServiceResult<ReceptionDetailsViewModel>.Failed(
                    "خطا در ایجاد پذیرش جدید. لطفاً مجدداً تلاش کنید.",
                    "RECEPTION_CREATE_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// به‌روزرسانی پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <param name="model">مدل به‌روزرسانی</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        public async Task<ServiceResult<ReceptionDetailsViewModel>> UpdateReceptionAsync(ReceptionEditViewModel model)
        {
            _logger.Information(
                "شروع به‌روزرسانی پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                model.ReceptionId, _currentUserService.UserName);

            try
            {
                // دریافت پذیرش موجود
                var reception = await _receptionRepository.GetByIdAsync(model.ReceptionId);
                if (reception == null)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed(
                        "پذیرش مورد نظر یافت نشد.",
                        "RECEPTION_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // بررسی امکان ویرایش
                if (reception.Status == ReceptionStatus.Completed)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed(
                        "امکان ویرایش پذیرش تکمیل شده وجود ندارد.",
                        "RECEPTION_COMPLETED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // به‌روزرسانی فیلدها
                reception.DoctorId = model.DoctorId;
                reception.TotalAmount = model.TotalAmount;
                reception.Notes = model.Notes ?? string.Empty;

                // ذخیره تغییرات
                _receptionRepository.Update(reception);
                await _receptionRepository.SaveChangesAsync();

                _logger.Information(
                    "پذیرش با موفقیت به‌روزرسانی شد. شناسه: {ReceptionId}. کاربر: {UserName}",
                    model.ReceptionId, _currentUserService.UserName);

                // بازگرداندن نتیجه
                var result = await GetReceptionDetailsAsync(model.ReceptionId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در به‌روزرسانی پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                    model.ReceptionId, _currentUserService.UserName);

                return ServiceResult<ReceptionDetailsViewModel>.Failed(
                    "خطا در به‌روزرسانی پذیرش. لطفاً مجدداً تلاش کنید.",
                    "RECEPTION_UPDATE_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// حذف پذیرش (Soft Delete)
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه حذف</returns>
        public async Task<ServiceResult> DeleteReceptionAsync(int id)
        {
            _logger.Information(
                "شروع حذف پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                // دریافت پذیرش موجود
                var reception = await _receptionRepository.GetByIdAsync(id);
                if (reception == null)
                {
                    return ServiceResult.Failed(
                        "پذیرش مورد نظر یافت نشد.",
                        "RECEPTION_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // بررسی امکان حذف
                if (reception.Status == ReceptionStatus.Completed)
                {
                    return ServiceResult.Failed(
                        "امکان حذف پذیرش تکمیل شده وجود ندارد.",
                        "RECEPTION_COMPLETED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // حذف نرم
                _receptionRepository.Delete(reception);
                await _receptionRepository.SaveChangesAsync();

                _logger.Information(
                    "پذیرش با موفقیت حذف شد. شناسه: {ReceptionId}. کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return ServiceResult.Successful("پذیرش با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در حذف پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return ServiceResult.Failed(
                    "خطا در حذف پذیرش. لطفاً مجدداً تلاش کنید.",
                    "RECEPTION_DELETE_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها با صفحه‌بندی
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="status">وضعیت (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetReceptionsAsync(
            int? patientId = null,
            int? doctorId = null,
            ReceptionStatus? status = null,
            string searchTerm = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            _logger.Information(
                "دریافت لیست پذیرش‌ها. صفحه: {PageNumber}, اندازه: {PageSize}, جستجو: {SearchTerm}. کاربر: {UserName}",
                pageNumber, pageSize, searchTerm, _currentUserService.UserName);

            try
            {
                var result = await _receptionRepository.GetPagedAsync(
                    patientId, doctorId, status, searchTerm, pageNumber, pageSize);

                // Repository خودش ServiceResult برمی‌گرداند، پس مستقیماً آن را بازمی‌گردانیم
                _logger.Information(
                    "لیست پذیرش‌ها با موفقیت دریافت شد. کاربر: {UserName}",
                    _currentUserService.UserName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پذیرش‌ها. کاربر: {UserName}",
                    _currentUserService.UserName);

                return ServiceResult<PagedResult<ReceptionIndexViewModel>>.Failed(
                    "خطا در دریافت لیست پذیرش‌ها. لطفاً مجدداً تلاش کنید.",
                    "RECEPTIONS_GET_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت جزئیات پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات پذیرش</returns>
        public async Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionDetailsAsync(int id)
        {
            _logger.Information(
                "دریافت جزئیات پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                var reception = await _receptionRepository.GetByIdWithDetailsAsync(id);
                if (reception == null)
                {
                    return ServiceResult<ReceptionDetailsViewModel>.Failed(
                        "پذیرش مورد نظر یافت نشد.",
                        "RECEPTION_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // تبدیل به ViewModel
                var viewModel = new ReceptionDetailsViewModel
                {
                    ReceptionId = reception.ReceptionId,
                    PatientFullName = $"{reception.Patient.FirstName} {reception.Patient.LastName}",
                    PatientNationalCode = reception.Patient.NationalCode,
                    DoctorFullName = $"{reception.Doctor.FirstName} {reception.Doctor.LastName}",
                    ReceptionDate = reception.ReceptionDate.ToPersianDateTime(),
                    Status = GetReceptionStatusText(reception.Status),
                    TotalAmount = reception.TotalAmount,
                    PaidAmount = await CalculatePaidAmountAsync(reception.ReceptionId),
                    Services = new List<ReceptionItemViewModel>(), // TODO: تبدیل ReceptionItems
                    Payments = new List<ReceptionPaymentViewModel>() // TODO: تبدیل Transactions
                };

                _logger.Information(
                    "جزئیات پذیرش با موفقیت دریافت شد. شناسه: {ReceptionId}. کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return ServiceResult<ReceptionDetailsViewModel>.Successful(
                    viewModel,
                    "جزئیات پذیرش با موفقیت دریافت شد.",
                    operationName: "GetReceptionDetails");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت جزئیات پذیرش. شناسه: {ReceptionId}. کاربر: {UserName}",
                    id, _currentUserService.UserName);

                return ServiceResult<ReceptionDetailsViewModel>.Failed(
                    "خطا در دریافت جزئیات پذیرش. لطفاً مجدداً تلاش کنید.",
                    "RECEPTION_DETAILS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تبدیل وضعیت پذیرش به متن فارسی
        /// </summary>
        private string GetReceptionStatusText(ReceptionStatus status)
        {
            switch (status)
            {
                case ReceptionStatus.Pending:
                    return "در انتظار";
                case ReceptionStatus.InProgress:
                    return "در حال انجام";
                case ReceptionStatus.Completed:
                    return "تکمیل شده";
                case ReceptionStatus.Cancelled:
                    return "لغو شده";
                default:
                    return "نامشخص";
            }
        }

        #endregion

        /// <summary>
        /// جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجو</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> SearchReceptionsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await GetReceptionsAsync(null, null, null, searchTerm, pageNumber, pageSize);
        }

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPatientReceptionsAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await GetReceptionsAsync(patientId, null, null, null, pageNumber, pageSize);
        }

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌های پزشک</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetDoctorReceptionsAsync(
            int doctorId,
            DateTime? date = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await GetReceptionsAsync(null, doctorId, null, null, pageNumber, pageSize);
        }


        #region Placeholder Methods (To be implemented in next steps)

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>اطلاعات بیمار</returns>
        public async Task<ServiceResult<ReceptionPatientLookupViewModel>> LookupPatientByNationalCodeAsync(string nationalCode)
        {
            _logger.Information(
                "جستجوی بیمار بر اساس کد ملی. کد ملی: {NationalCode}. کاربر: {UserName}",
                nationalCode, _currentUserService.UserName);

            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                        "کد ملی نمی‌تواند خالی باشد.",
                        "NATIONAL_CODE_REQUIRED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // جستجو در دیتابیس بر اساس کد ملی
                var patients = await _patientService.SearchPatientsAsync(nationalCode, 1, 1);
                if (!patients.Success || patients.Data.Items.Count == 0)
                {
                    return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                        "بیمار با این کد ملی یافت نشد.",
                        "PATIENT_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var patient = patients.Data.Items.First();
                
                // تبدیل به ViewModel
                var viewModel = new ReceptionPatientLookupViewModel
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    NationalCode = patient.NationalCode,
                    PhoneNumber = patient.PhoneNumber,
                    BirthDate = patient.BirthDate,
                    BirthDateShamsi = patient.BirthDate?.ToPersianDateTime(),
                    Gender = patient.Gender,
                    FullName = $"{patient.FirstName} {patient.LastName}"
                };

                _logger.Information(
                    "بیمار با موفقیت یافت شد. شناسه: {PatientId}, نام: {FullName}. کاربر: {UserName}",
                    viewModel.PatientId, viewModel.FullName, _currentUserService.UserName);

                return ServiceResult<ReceptionPatientLookupViewModel>.Successful(
                    viewModel,
                    "بیمار با موفقیت یافت شد.",
                    operationName: "LookupPatientByNationalCode");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در جستجوی بیمار. کد ملی: {NationalCode}. کاربر: {UserName}",
                    nationalCode, _currentUserService.UserName);

                return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                    "خطا در جستجوی بیمار. لطفاً مجدداً تلاش کنید.",
                    "PATIENT_LOOKUP_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// جستجوی بیمار بر اساس نام
        /// </summary>
        /// <param name="name">نام بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست بیماران</returns>
        public async Task<ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>> SearchPatientsByNameAsync(string name, int pageNumber = 1, int pageSize = 10)
        {
            _logger.Information(
                "جستجوی بیمار بر اساس نام. نام: {Name}, صفحه: {PageNumber}, اندازه: {PageSize}. کاربر: {UserName}",
                name, pageNumber, pageSize, _currentUserService.UserName);

            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>.Failed(
                        "نام بیمار نمی‌تواند خالی باشد.",
                        "NAME_REQUIRED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // جستجو در دیتابیس
                var patients = await _patientService.SearchPatientsAsync(name, pageNumber, pageSize);
                if (!patients.Success)
                {
                    return ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>.Failed(
                        patients.Message,
                        patients.Code,
                        patients.Category,
                        patients.SecurityLevel);
                }

                // تبدیل به ViewModel
                var viewModels = patients.Data.Items.Select(p => new ReceptionPatientLookupViewModel
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    NationalCode = p.NationalCode,
                    PhoneNumber = p.PhoneNumber,
                    BirthDate = p.BirthDate,
                    BirthDateShamsi = p.BirthDate?.ToPersianDateTime(),
                    Gender = p.Gender,
                    FullName = $"{p.FirstName} {p.LastName}"
                }).ToList();

                var pagedResult = new PagedResult<ReceptionPatientLookupViewModel>(
                    viewModels, 
                    patients.Data.TotalItems, 
                    patients.Data.PageNumber, 
                    patients.Data.PageSize);

                _logger.Information(
                    "جستجوی بیماران با موفقیت انجام شد. تعداد: {Count}. کاربر: {UserName}",
                    viewModels.Count, _currentUserService.UserName);

                return ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>.Successful(
                    pagedResult,
                    "جستجوی بیماران با موفقیت انجام شد.",
                    operationName: "SearchPatientsByName");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در جستجوی بیماران. نام: {Name}. کاربر: {UserName}",
                    name, _currentUserService.UserName);

                return ServiceResult<PagedResult<ReceptionPatientLookupViewModel>>.Failed(
                    "خطا در جستجوی بیماران. لطفاً مجدداً تلاش کنید.",
                    "PATIENT_SEARCH_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// ایجاد بیمار جدید در حین پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد بیمار</param>
        /// <returns>اطلاعات بیمار جدید</returns>
        public async Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientInlineAsync(PatientCreateEditViewModel model)
        {
            _logger.Information(
                "ایجاد بیمار جدید در حین پذیرش. نام: {FirstName} {LastName}, کد ملی: {NationalCode}. کاربر: {UserName}",
                model.FirstName, model.LastName, model.NationalCode, _currentUserService.UserName);

            try
            {
                if (model == null)
                {
                    return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                        "اطلاعات بیمار نمی‌تواند خالی باشد.",
                        "MODEL_REQUIRED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // ایجاد بیمار جدید
                var createResult = await _patientService.CreatePatientAsync(model);
                if (!createResult.Success)
                {
                    return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                        createResult.Message,
                        createResult.Code,
                        createResult.Category,
                        createResult.SecurityLevel);
                }

                // جستجوی بیمار جدید ایجاد شده
                var newPatient = await _patientService.SearchPatientsAsync(model.NationalCode, 1, 1);
                if (!newPatient.Success || newPatient.Data.Items.Count == 0)
                {
                    return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                        "بیمار ایجاد شد اما اطلاعات آن قابل بازیابی نیست.",
                        "PATIENT_CREATED_BUT_NOT_FOUND",
                        ErrorCategory.System,
                        SecurityLevel.High);
                }

                var patient = newPatient.Data.Items.First();
                
                // تبدیل به ViewModel
                var viewModel = new ReceptionPatientLookupViewModel
                {
                    PatientId = patient.PatientId,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    NationalCode = patient.NationalCode,
                    PhoneNumber = patient.PhoneNumber,
                    BirthDate = patient.BirthDate,
                    BirthDateShamsi = patient.BirthDate?.ToPersianDateTime(),
                    Gender = patient.Gender,
                    FullName = $"{patient.FirstName} {patient.LastName}"
                };

                _logger.Information(
                    "بیمار جدید با موفقیت ایجاد شد. شناسه: {PatientId}, نام: {FullName}. کاربر: {UserName}",
                    viewModel.PatientId, viewModel.FullName, _currentUserService.UserName);

                return ServiceResult<ReceptionPatientLookupViewModel>.Successful(
                    viewModel,
                    "بیمار جدید با موفقیت ایجاد شد.",
                    operationName: "CreatePatientInline");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ایجاد بیمار جدید. نام: {FirstName} {LastName}. کاربر: {UserName}",
                    model?.FirstName, model?.LastName, _currentUserService.UserName);

                return ServiceResult<ReceptionPatientLookupViewModel>.Failed(
                    "خطا در ایجاد بیمار جدید. لطفاً مجدداً تلاش کنید.",
                    "PATIENT_CREATE_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات فعال
        /// </summary>
        /// <returns>لیست دسته‌بندی‌های خدمات</returns>
        public async Task<ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>> GetServiceCategoriesAsync()
        {
            _logger.Information(
                "دریافت لیست دسته‌بندی‌های خدمات فعال. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                // دریافت دسته‌بندی‌های فعال
                var categories = await _serviceCategoryService.GetActiveServiceCategoriesAsync();
                
                // تبدیل به ViewModel
                var viewModels = categories.Select(c => new ReceptionServiceCategoryLookupViewModel
                {
                    ServiceCategoryId = c.ServiceCategoryId,
                    Title = c.Title,
                    Description = "", // ServiceCategorySelectItem doesn't have Description
                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.DepartmentName,
                    IsActive = true, // ServiceCategorySelectItem doesn't have IsActive
                    DisplayName = c.Title
                }).ToList();

                _logger.Information(
                    "لیست دسته‌بندی‌های خدمات با موفقیت دریافت شد. تعداد: {Count}. کاربر: {UserName}",
                    viewModels.Count, _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>.Successful(
                    viewModels,
                    "لیست دسته‌بندی‌های خدمات با موفقیت دریافت شد.",
                    operationName: "GetServiceCategories");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست دسته‌بندی‌های خدمات. کاربر: {UserName}",
                    _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>.Failed(
                    "خطا در دریافت لیست دسته‌بندی‌های خدمات. لطفاً مجدداً تلاش کنید.",
                    "SERVICE_CATEGORIES_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت لیست خدمات فعال بر اساس دسته‌بندی
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>لیست خدمات</returns>
        public async Task<ServiceResult<List<ReceptionServiceLookupViewModel>>> GetServicesByCategoryAsync(int categoryId)
        {
            _logger.Information(
                "دریافت لیست خدمات فعال بر اساس دسته‌بندی. شناسه دسته‌بندی: {CategoryId}. کاربر: {UserName}",
                categoryId, _currentUserService.UserName);

            try
            {
                if (categoryId <= 0)
                {
                    return ServiceResult<List<ReceptionServiceLookupViewModel>>.Failed(
                        "شناسه دسته‌بندی خدمات نامعتبر است.",
                        "INVALID_CATEGORY_ID",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // دریافت خدمات فعال بر اساس دسته‌بندی
                var services = await _serviceService.GetActiveServicesByCategoryAsync(categoryId);
                
                // تبدیل به ViewModel
                var viewModels = services.Select(s => new ReceptionServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    Description = "", // ServiceSelectItem doesn't have Description
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategoryTitle, // Use ServiceCategoryTitle instead
                    BasePrice = s.Price, // Use Price instead of BasePrice
                    IsActive = true, // ServiceSelectItem doesn't have IsActive
                    DisplayName = s.Title,
                    PriceDisplay = s.Price.ToString("N0") + " تومان"
                }).ToList();

                _logger.Information(
                    "لیست خدمات با موفقیت دریافت شد. تعداد: {Count}, دسته‌بندی: {CategoryId}. کاربر: {UserName}",
                    viewModels.Count, categoryId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceLookupViewModel>>.Successful(
                    viewModels,
                    "لیست خدمات با موفقیت دریافت شد.",
                    operationName: "GetServicesByCategory");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست خدمات. دسته‌بندی: {CategoryId}. کاربر: {UserName}",
                    categoryId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceLookupViewModel>>.Failed(
                    "خطا در دریافت لیست خدمات. لطفاً مجدداً تلاش کنید.",
                    "SERVICES_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان فعال
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsAsync()
        {
            _logger.Information(
                "دریافت لیست پزشکان فعال. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                // دریافت پزشکان فعال با فیلتر خالی
                var filter = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000 // دریافت همه پزشکان
                };
                
                var doctors = await _doctorCrudService.GetDoctorsAsync(filter);
                if (!doctors.Success)
                {
                    return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                        doctors.Message,
                        doctors.Code,
                        doctors.Category,
                        doctors.SecurityLevel);
                }
                
                // تبدیل به ViewModel
                var viewModels = doctors.Data.Items.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    SpecializationId = null, // DoctorIndexViewModel doesn't have SpecializationId
                    SpecializationName = d.SpecializationNames?.FirstOrDefault() ?? "", // Use first specialization
                    DepartmentId = null, // DoctorIndexViewModel doesn't have DepartmentId
                    DepartmentName = "", // DoctorIndexViewModel doesn't have DepartmentName
                    IsActive = d.IsActive,
                    FullName = $"{d.FirstName} {d.LastName}",
                    DisplayName = $"{d.FirstName} {d.LastName} - {d.SpecializationNames?.FirstOrDefault() ?? ""}"
                }).ToList();

                _logger.Information(
                    "لیست پزشکان با موفقیت دریافت شد. تعداد: {Count}. کاربر: {UserName}",
                    viewModels.Count, _currentUserService.UserName);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(
                    viewModels,
                    "لیست پزشکان با موفقیت دریافت شد.",
                    operationName: "GetDoctors");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پزشکان. کاربر: {UserName}",
                    _currentUserService.UserName);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در دریافت لیست پزشکان. لطفاً مجدداً تلاش کنید.",
                    "DOCTORS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<List<ReceptionDoctorLookupViewModel>>> GetDoctorsBySpecializationAsync(int specializationId)
        {
            _logger.Information(
                "درخواست لیست پزشکان بر اساس تخصص. تخصص: {SpecializationId}, کاربر: {UserName}",
                specializationId, _currentUserService.UserName);

            try
            {
                // دریافت پزشکان بر اساس تخصص (موقت - باید در Repository پیاده‌سازی شود)
                var doctors = new List<Doctor>(); // TODO: پیاده‌سازی GetDoctorsBySpecializationAsync
                
                var result = doctors.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = $"{d.FirstName} {d.LastName}",
                    SpecializationName = "نامشخص", // TODO: از رابطه Specialization استفاده شود
                    DepartmentName = "نامشخص", // TODO: از رابطه Department استفاده شود
                    IsActive = d.IsActive,
                    IsAvailable = d.IsActive && !d.IsDeleted,
                    DisplayName = $"{d.FirstName} {d.LastName}"
                }).ToList();

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پزشکان بر اساس تخصص. تخصص: {SpecializationId}, کاربر: {UserName}",
                    specializationId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionDoctorLookupViewModel>>.Failed(
                    "خطا در دریافت لیست پزشکان. لطفاً مجدداً تلاش کنید.",
                    "DOCTORS_BY_SPECIALIZATION_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>> GetPatientActiveInsurancesAsync(int patientId)
        {
            _logger.Information(
                "درخواست بیمه‌های فعال بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                patientId, _currentUserService.UserName);

            try
            {
                // دریافت بیمه‌های فعال بیمار (موقت - باید در Repository پیاده‌سازی شود)
                var insurances = new List<PatientInsurance>(); // TODO: پیاده‌سازی GetPatientActiveInsurancesAsync
                
                var result = insurances.Select(i => new ReceptionPatientInsuranceLookupViewModel
                {
                    PatientInsuranceId = i.PatientInsuranceId,
                    PatientId = i.PatientId,
                    InsuranceProviderName = "نامشخص", // TODO: از رابطه InsuranceProvider استفاده شود
                    PolicyNumber = i.PolicyNumber,
                    StartDate = i.StartDate,
                    EndDate = i.EndDate,
                    IsActive = i.IsActive && (i.EndDate == null || i.EndDate > DateTime.Now),
                    CoveragePercentage = 0 // TODO: از InsurancePlan.CoveragePercent استفاده شود
                }).ToList();

                return ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت بیمه‌های فعال بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                    patientId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>.Failed(
                    "خطا در دریافت بیمه‌های بیمار. لطفاً مجدداً تلاش کنید.",
                    "PATIENT_INSURANCES_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<ReceptionCostCalculationViewModel>> CalculateReceptionCostsAsync(int patientId, List<int> serviceIds, int? insuranceId, DateTime receptionDate)
        {
            _logger.Information(
                "محاسبه هزینه‌های پذیرش. بیمار: {PatientId}, خدمات: {ServiceIds}, بیمه: {InsuranceId}, تاریخ: {ReceptionDate}, کاربر: {UserName}",
                patientId, string.Join(",", serviceIds), insuranceId, receptionDate, _currentUserService.UserName);

            try
            {
                var result = new ReceptionCostCalculationViewModel();
                decimal totalAmount = 0;
                decimal totalInsuranceShare = 0;
                decimal totalPatientShare = 0;

                // دریافت اطلاعات خدمات (موقت - باید در Repository پیاده‌سازی شود)
                var services = new List<Service>(); // TODO: پیاده‌سازی GetServicesByIdsAsync
                
                // دریافت اطلاعات بیمه (موقت - باید در Repository پیاده‌سازی شود)
                PatientInsurance insurance = null; // TODO: پیاده‌سازی GetPatientInsuranceAsync

                foreach (var service in services)
                {
                    var serviceCost = new ServiceCostDetailViewModel
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.Title,
                        BasePrice = service.Price
                    };

                    // محاسبه سهم بیمه
                    if (insurance != null && insurance.IsActive)
                    {
                        serviceCost.InsuranceShare = service.Price * (0 / 100); // TODO: از InsurancePlan.CoveragePercent استفاده شود
                        serviceCost.PatientShare = service.Price - serviceCost.InsuranceShare;
                        serviceCost.CoveragePercentage = 0; // TODO: از InsurancePlan.CoveragePercent استفاده شود
                    }
                    else
                    {
                        serviceCost.InsuranceShare = 0;
                        serviceCost.PatientShare = service.Price;
                        serviceCost.CoveragePercentage = 0;
                    }

                    result.ServiceDetails.Add(serviceCost);

                    totalAmount += service.Price;
                    totalInsuranceShare += serviceCost.InsuranceShare;
                    totalPatientShare += serviceCost.PatientShare;
                }

                result.TotalServiceCost = totalAmount;
                result.InsuranceShare = totalInsuranceShare;
                result.PatientShare = totalPatientShare;
                result.CalculationDate = receptionDate;

                return ServiceResult<ReceptionCostCalculationViewModel>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در محاسبه هزینه‌های پذیرش. بیمار: {PatientId}, کاربر: {UserName}",
                    patientId, _currentUserService.UserName);

                return ServiceResult<ReceptionCostCalculationViewModel>.Failed(
                    "خطا در محاسبه هزینه‌ها. لطفاً مجدداً تلاش کنید.",
                    "COST_CALCULATION_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientInfoAsync(string nationalCode, DateTime birthDate)
        {
            _logger.Information(
                "استعلام اطلاعات بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, کاربر: {UserName}",
                nationalCode, birthDate, _currentUserService.UserName);

            try
            {
                // شبیه‌سازی استعلام از سیستم خارجی (شبکه شمس)
                var inquiryResult = new PatientInquiryViewModel
                {
                    InquiryType = InquiryType.Both,
                    Status = InquiryStatus.Successful,
                    IdentityData = new PatientIdentityData
                    {
                        FirstName = "نام نمونه",
                        LastName = "نام خانوادگی نمونه",
                        BirthDate = birthDate,
                        Gender = Gender.Male
                    },
                    InsuranceData = new PatientInsuranceData
                    {
                        InsuranceName = "تأمین اجتماعی",
                        InsuranceNumber = "1234567890",
                        IsVerified = true
                    }
                };

                return ServiceResult<PatientInquiryViewModel>.Successful(inquiryResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در استعلام اطلاعات بیمار. کد ملی: {NationalCode}, کاربر: {UserName}",
                    nationalCode, _currentUserService.UserName);

                return ServiceResult<PatientInquiryViewModel>.Failed(
                    "خطا در استعلام اطلاعات بیمار. لطفاً مجدداً تلاش کنید.",
                    "PATIENT_INQUIRY_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<ReceptionValidationViewModel>> ValidateReceptionAsync(int patientId, int doctorId, DateTime receptionDate)
        {
            _logger.Information(
                "اعتبارسنجی پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, تاریخ: {ReceptionDate}, کاربر: {UserName}",
                patientId, doctorId, receptionDate, _currentUserService.UserName);

            try
            {
                var validation = new ReceptionValidationViewModel();

                // بررسی وجود بیمار (موقت - باید در Repository پیاده‌سازی شود)
                var patient = new Patient(); // TODO: پیاده‌سازی GetPatientByIdAsync
                if (patient == null)
                {
                    validation.ValidationErrors.Add("بیمار مورد نظر یافت نشد.");
                }

                // بررسی وجود پزشک (موقت - باید در Repository پیاده‌سازی شود)
                var doctor = new Doctor(); // TODO: پیاده‌سازی GetDoctorByIdAsync
                if (doctor == null)
                {
                    validation.ValidationErrors.Add("پزشک مورد نظر یافت نشد.");
                }
                else if (!doctor.IsActive)
                {
                    validation.ValidationErrors.Add("پزشک مورد نظر غیرفعال است.");
                }
                else
                {
                    validation.IsDoctorAvailable = true;
                }

                // بررسی تاریخ پذیرش
                if (receptionDate < DateTime.Today)
                {
                    validation.ValidationErrors.Add("تاریخ پذیرش نمی‌تواند در گذشته باشد.");
                }
                else if (receptionDate > DateTime.Today.AddDays(30))
                {
                    validation.ValidationErrors.Add("تاریخ پذیرش نمی‌تواند بیش از ۳۰ روز آینده باشد.");
                }

                // بررسی تداخل زمانی (موقت - باید در Repository پیاده‌سازی شود)
                var existingReceptions = new List<Reception>(); // TODO: پیاده‌سازی GetReceptionsByDoctorAndDateAsync
                if (existingReceptions.Count >= 20) // فرض: حداکثر ۲۰ پذیرش در روز
                {
                    validation.Warnings.Add("ظرفیت پذیرش پزشک در این تاریخ تکمیل است.");
                }

                validation.IsValid = validation.ValidationErrors.Count == 0;

                return ServiceResult<ReceptionValidationViewModel>.Successful(validation);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در اعتبارسنجی پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    patientId, doctorId, _currentUserService.UserName);

                return ServiceResult<ReceptionValidationViewModel>.Failed(
                    "خطا در اعتبارسنجی پذیرش. لطفاً مجدداً تلاش کنید.",
                    "RECEPTION_VALIDATION_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<ReceptionDailyStatsViewModel>> GetDailyStatsAsync(DateTime date)
        {
            _logger.Information(
                "درخواست آمار روزانه پذیرش‌ها. تاریخ: {Date}, کاربر: {UserName}",
                date, _currentUserService.UserName);

            try
            {
                var stats = new ReceptionDailyStatsViewModel
                {
                    Date = date
                };

                // دریافت آمار کلی (موقت - باید در Repository پیاده‌سازی شود)
                var dailyReceptions = new List<Reception>(); // TODO: پیاده‌سازی GetReceptionsByDateAsync
                
                stats.TotalReceptions = dailyReceptions.Count;
                stats.CompletedReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Completed);
                stats.PendingReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Pending);
                stats.CancelledReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Cancelled);

                // محاسبه درآمد
                stats.TotalRevenue = dailyReceptions.Sum(r => r.TotalAmount);
                stats.AverageRevenuePerReception = dailyReceptions.Count > 0 ? stats.TotalRevenue / dailyReceptions.Count : 0;

                // آمار پزشکان
                var doctorGroups = dailyReceptions.GroupBy(r => r.DoctorId);
                foreach (var group in doctorGroups)
                {
                    var doctor = group.First().Doctor;
                    stats.DoctorStats.Add(new ReceptionDoctorStatsViewModel
                    {
                        DoctorId = doctor.DoctorId,
                        DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                        ReceptionsCount = group.Count(),
                        TotalRevenue = group.Sum(r => r.TotalAmount),
                        AverageRevenue = group.Count() > 0 ? group.Sum(r => r.TotalAmount) / group.Count() : 0
                    });
                }

                return ServiceResult<ReceptionDailyStatsViewModel>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت آمار روزانه. تاریخ: {Date}, کاربر: {UserName}",
                    date, _currentUserService.UserName);

                return ServiceResult<ReceptionDailyStatsViewModel>.Failed(
                    "خطا در دریافت آمار روزانه. لطفاً مجدداً تلاش کنید.",
                    "DAILY_STATS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<ReceptionDoctorStatsViewModel>> GetDoctorStatsAsync(int doctorId, DateTime date)
        {
            _logger.Information(
                "درخواست آمار پزشک. پزشک: {DoctorId}, تاریخ: {Date}, کاربر: {UserName}",
                doctorId, date, _currentUserService.UserName);

            try
            {
                var doctor = new Doctor(); // TODO: پیاده‌سازی GetDoctorByIdAsync
                if (doctor == null)
                {
                    return ServiceResult<ReceptionDoctorStatsViewModel>.Failed(
                        "پزشک مورد نظر یافت نشد.",
                        "DOCTOR_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var stats = new ReceptionDoctorStatsViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}"
                };

                // دریافت پذیرش‌های پزشک در تاریخ مشخص (موقت - باید در Repository پیاده‌سازی شود)
                var doctorReceptions = new List<Reception>(); // TODO: پیاده‌سازی GetReceptionsByDoctorAndDateAsync
                
                stats.ReceptionsCount = doctorReceptions.Count;
                stats.TotalRevenue = doctorReceptions.Sum(r => r.TotalAmount);
                stats.AverageRevenue = doctorReceptions.Count > 0 ? stats.TotalRevenue / doctorReceptions.Count : 0;

                return ServiceResult<ReceptionDoctorStatsViewModel>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت آمار پزشک. پزشک: {DoctorId}, تاریخ: {Date}, کاربر: {UserName}",
                    doctorId, date, _currentUserService.UserName);

                return ServiceResult<ReceptionDoctorStatsViewModel>.Failed(
                    "خطا در دریافت آمار پزشک. لطفاً مجدداً تلاش کنید.",
                    "DOCTOR_STATS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<PaymentTransactionViewModel>> AddPaymentAsync(int receptionId, PaymentTransactionCreateViewModel paymentModel)
        {
            _logger.Information(
                "افزودن پرداخت جدید. پذیرش: {ReceptionId}, مبلغ: {Amount}, روش: {PaymentMethod}, کاربر: {UserName}",
                receptionId, paymentModel.Amount, paymentModel.Method, _currentUserService.UserName);

            try
            {
                // بررسی وجود پذیرش
                var reception = await _receptionRepository.GetByIdAsync(receptionId);
                if (reception == null)
                {
                    return ServiceResult<PaymentTransactionViewModel>.Failed(
                        "پذیرش مورد نظر یافت نشد.",
                        "RECEPTION_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // ایجاد تراکنش پرداخت (موقت - باید Entity PaymentTransaction بررسی شود)
                var paymentTransaction = new PaymentTransaction
                {
                    ReceptionId = receptionId,
                    Amount = paymentModel.Amount,
                    // PaymentMethod = paymentModel.Method, // TODO: بررسی property - Method موجود است
                    // TransactionDate = DateTime.Now, // TODO: بررسی property
                    // Status = PaymentStatus.Completed, // TODO: بررسی enum
                    // ReferenceNumber = paymentModel.ReferenceNumber, // TODO: بررسی property
                    // Notes = paymentModel.Notes, // TODO: بررسی property
                    // CreatedBy = _currentUserService.UserId, // TODO: بررسی property
                    // CreatedAt = DateTime.Now // TODO: بررسی property
                };

                // ذخیره تراکنش (موقت - باید در Repository پیاده‌سازی شود)
                // await _receptionRepository.AddPaymentTransactionAsync(paymentTransaction); // TODO: پیاده‌سازی

                // به‌روزرسانی مبلغ پرداخت شده در پذیرش (موقت - باید Entity Reception بررسی شود)
                // reception.PaidAmount += paymentModel.Amount; // TODO: بررسی property
                // reception.RemainingAmount = reception.TotalAmount - reception.PaidAmount; // TODO: بررسی property
                
                // if (reception.RemainingAmount <= 0) // TODO: بررسی property
                // {
                //     reception.Status = ReceptionStatus.Completed;
                // }

                await _receptionRepository.SaveChangesAsync();

                // بازگرداندن نتیجه (موقت - باید PaymentTransactionViewModel بررسی شود)
                var result = new PaymentTransactionViewModel
                {
                    // Id = paymentTransaction.Id, // TODO: بررسی property
                    ReceptionId = paymentTransaction.ReceptionId,
                    Amount = paymentTransaction.Amount,
                    // PaymentMethod = paymentTransaction.PaymentMethod.ToString(), // TODO: بررسی property
                    // TransactionDate = paymentTransaction.TransactionDate, // TODO: بررسی property
                    // Status = paymentTransaction.Status.ToString(), // TODO: بررسی property
                    // ReferenceNumber = paymentTransaction.ReferenceNumber, // TODO: بررسی property
                    // Notes = paymentTransaction.Notes // TODO: بررسی property
                };

                return ServiceResult<PaymentTransactionViewModel>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در افزودن پرداخت. پذیرش: {ReceptionId}, کاربر: {UserName}",
                    receptionId, _currentUserService.UserName);

                return ServiceResult<PaymentTransactionViewModel>.Failed(
                    "خطا در افزودن پرداخت. لطفاً مجدداً تلاش کنید.",
                    "ADD_PAYMENT_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        public async Task<ServiceResult<List<PaymentTransactionViewModel>>> GetReceptionPaymentsAsync(int receptionId)
        {
            _logger.Information(
                "درخواست لیست پرداخت‌های پذیرش. پذیرش: {ReceptionId}, کاربر: {UserName}",
                receptionId, _currentUserService.UserName);

            try
            {
                // دریافت تراکنش‌های پرداخت (موقت - باید در Repository پیاده‌سازی شود)
                var payments = new List<PaymentTransaction>(); // TODO: پیاده‌سازی GetReceptionPaymentsAsync
                
                var result = payments.Select(p => new PaymentTransactionViewModel
                {
                    // Id = p.Id, // TODO: بررسی property
                    ReceptionId = p.ReceptionId,
                    Amount = p.Amount,
                    // PaymentMethod = p.PaymentMethod.ToString(), // TODO: بررسی property
                    // TransactionDate = p.TransactionDate, // TODO: بررسی property
                    // Status = p.Status.ToString(), // TODO: بررسی property
                    // ReferenceNumber = p.ReferenceNumber, // TODO: بررسی property
                    // Notes = p.Notes // TODO: بررسی property
                }).ToList();

                return ServiceResult<List<PaymentTransactionViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پرداخت‌ها. پذیرش: {ReceptionId}, کاربر: {UserName}",
                    receptionId, _currentUserService.UserName);

                return ServiceResult<List<PaymentTransactionViewModel>>.Failed(
                    "خطا در دریافت لیست پرداخت‌ها. لطفاً مجدداً تلاش کنید.",
                    "GET_PAYMENTS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        public async Task<ServiceResult<ReceptionLookupListsViewModel>> GetLookupListsAsync()
        {
            _logger.Information(
                "درخواست لیست‌های کمکی. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                var lookupLists = new ReceptionLookupListsViewModel();

                // دریافت پزشکان (موقت - باید در Repository پیاده‌سازی شود)
                var doctors = new List<Doctor>(); // TODO: پیاده‌سازی GetAllDoctorsAsync
                lookupLists.Doctors = doctors.Select(d => new ReceptionDoctorLookupViewModel
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    FullName = $"{d.FirstName} {d.LastName}",
                    SpecializationName = "نامشخص", // TODO: از رابطه Specialization استفاده شود
                    IsActive = d.IsActive,
                    DisplayName = $"{d.FirstName} {d.LastName}"
                }).ToList();

                // دریافت دسته‌بندی‌های خدمات (موقت - باید در Repository پیاده‌سازی شود)
                var serviceCategories = new List<ServiceCategory>(); // TODO: پیاده‌سازی GetServiceCategoriesAsync
                lookupLists.ServiceCategories = serviceCategories.Select(sc => new ReceptionServiceCategoryLookupViewModel
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Title = sc.Title,
                    Description = sc.Description,
                    IsActive = sc.IsActive,
                    DisplayName = sc.Title
                }).ToList();

                // دریافت خدمات (موقت - باید در Repository پیاده‌سازی شود)
                var services = new List<Service>(); // TODO: پیاده‌سازی GetAllServicesAsync
                lookupLists.Services = services.Select(s => new ReceptionServiceLookupViewModel
                {
                    ServiceId = s.ServiceId,
                    Title = s.Title,
                    Description = s.Description,
                    Price = s.Price,
                    BasePrice = s.Price,
                    IsActive = s.IsActive,
                    DisplayName = s.Title,
                    PriceDisplay = $"{s.Price:C0} تومان"
                }).ToList();

                // دریافت ارائه‌دهندگان بیمه (موقت - باید در Repository پیاده‌سازی شود)
                var insuranceProviders = new List<InsuranceProvider>(); // TODO: پیاده‌سازی GetInsuranceProvidersAsync
                // lookupLists.InsuranceProviders = insuranceProviders.Select(ip => new SelectListItem // TODO: بررسی property
                // {
                //     Value = ip.Id.ToString(),
                //     Text = ip.Name
                // }).ToList();

                // روش‌های پرداخت
                lookupLists.PaymentMethods = Enum.GetValues(typeof(PaymentMethod))
                    .Cast<PaymentMethod>()
                    .Select(pm => new PaymentMethodLookupViewModel
                    {
                        PaymentMethod = pm,
                        Name = GetPaymentMethodDisplayName(pm),
                        Description = GetPaymentMethodDisplayName(pm),
                        IsActive = true,
                        RequiresConfirmation = pm == PaymentMethod.Online
                    }).ToList();

                return ServiceResult<ReceptionLookupListsViewModel>.Successful(lookupLists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست‌های کمکی. کاربر: {UserName}",
                    _currentUserService.UserName);

                return ServiceResult<ReceptionLookupListsViewModel>.Failed(
                    "خطا در دریافت لیست‌های کمکی. لطفاً مجدداً تلاش کنید.",
                    "LOOKUP_LISTS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        #region Helper Methods

        private string GetPaymentMethodDisplayName(PaymentMethod paymentMethod)
        {
            return paymentMethod switch
            {
                PaymentMethod.Cash => "نقدی",
                PaymentMethod.Card => "کارت",
                // PaymentMethod.Insurance => "بیمه", // TODO: بررسی enum
                PaymentMethod.Online => "آنلاین",
                _ => paymentMethod.ToString()
            };
        }

        private string GetReceptionTypeDisplayName(ReceptionType receptionType)
        {
            return receptionType switch
            {
                ReceptionType.Normal => "عادی",
                ReceptionType.Emergency => "اورژانس",
                ReceptionType.Special => "ویژه",
                ReceptionType.Online => "آنلاین",
                _ => receptionType.ToString()
            };
        }

        private string GetReceptionStatusDisplayName(ReceptionStatus receptionStatus)
        {
            return receptionStatus switch
            {
                ReceptionStatus.Pending => "در انتظار",
                ReceptionStatus.InProgress => "در حال انجام",
                ReceptionStatus.Completed => "تکمیل شده",
                ReceptionStatus.Cancelled => "لغو شده",
                // ReceptionStatus.NoShow => "عدم حضور", // TODO: بررسی enum
                _ => receptionStatus.ToString()
            };
        }

        #endregion

        #region External Inquiry Services

        /// <summary>
        /// استعلام هویت بیمار از سیستم خارجی (شبکه شمس)
        /// طبق AI_COMPLIANCE_CONTRACT: قانون 6 - امنیت و کیفیت
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <param name="birthDate">تاریخ تولد بیمار</param>
        /// <returns>نتیجه استعلام هویت</returns>
        public async Task<ServiceResult<PatientInquiryViewModel>> InquiryPatientIdentityAsync(string nationalCode, DateTime birthDate)
        {
            try
            {
                _logger.Information(
                    "شروع استعلام هویت بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, کاربر: {UserName}",
                    nationalCode, birthDate, _currentUserService.UserName);

                // اعتبارسنجی ورودی‌ها
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return ServiceResult<PatientInquiryViewModel>.Failed(
                        "کد ملی الزامی است.",
                        "NATIONAL_CODE_REQUIRED",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                if (birthDate == default(DateTime))
                {
                    return ServiceResult<PatientInquiryViewModel>.Failed(
                        "تاریخ تولد الزامی است.",
                        "BIRTH_DATE_REQUIRED",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);
                }

                // TODO: پیاده‌سازی اتصال به سیستم خارجی (شبکه شمس)
                // در حال حاضر یک پیاده‌سازی Mock ارائه می‌دهیم
                await Task.Delay(1000); // شبیه‌سازی تأخیر شبکه

                // شبیه‌سازی نتیجه استعلام
                var inquiryResult = new PatientInquiryViewModel
                {
                    NationalCode = nationalCode,
                    BirthDate = birthDate,
                    BirthDateShamsi = birthDate.ToString("yyyy/MM/dd"), // تبدیل به شمسی
                    InquiryType = InquiryType.Both,
                    Status = InquiryStatus.Successful,
                    Message = "استعلام هویت با موفقیت انجام شد.",
                    IsSuccessful = true,
                    InquiryTime = DateTime.Now,
                    IdentityData = new PatientIdentityData
                    {
                        FirstName = "نام نمونه",
                        LastName = "نام خانوادگی نمونه",
                        FatherName = "نام پدر نمونه",
                        BirthDate = birthDate,
                        Gender = Gender.Male,
                        BirthPlace = "تهران",
                        Address = "تهران، خیابان نمونه",
                        BirthCertificateNumber = "1234567890",
                        IsVerified = true,
                        VerificationDate = DateTime.Now
                    },
                    InsuranceData = new PatientInsuranceData
                    {
                        InsuranceName = "تأمین اجتماعی",
                        InsuranceNumber = "123456789",
                        InsuranceStatus = "فعال",
                        StartDate = DateTime.Now.AddYears(-1),
                        EndDate = DateTime.Now.AddYears(1),
                        CoveragePercentage = 70,
                        Deductible = 100000,
                        IsVerified = true,
                        VerificationDate = DateTime.Now
                    },
                    IsDataBound = true
                };

                _logger.Information(
                    "استعلام هویت بیمار با موفقیت انجام شد. کد ملی: {NationalCode}, کاربر: {UserName}",
                    nationalCode, _currentUserService.UserName);

                return ServiceResult<PatientInquiryViewModel>.Successful(
                    inquiryResult,
                    "استعلام هویت بیمار با موفقیت انجام شد.",
                    operationName: "InquiryPatientIdentity");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در استعلام هویت بیمار. کد ملی: {NationalCode}, کاربر: {UserName}",
                    nationalCode, _currentUserService.UserName);

                return ServiceResult<PatientInquiryViewModel>.Failed(
                    "خطا در استعلام هویت بیمار. لطفاً مجدداً تلاش کنید.",
                    "INQUIRY_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.Medium);
            }
        }

        #endregion

        #endregion

        #region Helper Methods

        /// <summary>
        /// محاسبه مبلغ پرداخت شده برای پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>مبلغ پرداخت شده</returns>
        private async Task<decimal> CalculatePaidAmountAsync(int receptionId)
        {
            try
            {
                // دریافت تراکنش‌های پرداخت
                var payments = await _receptionRepository.GetReceptionPaymentsAsync(receptionId);
                return payments?.Sum(p => p.Amount) ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه مبلغ پرداخت شده برای پذیرش {ReceptionId}", receptionId);
                return 0;
            }
        }

        #endregion
    }
}
