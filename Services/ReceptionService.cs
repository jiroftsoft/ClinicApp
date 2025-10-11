using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Reception;
using ReceptionIndexViewModel = ClinicApp.ViewModels.Reception.ReceptionIndexViewModel;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Services;
using ClinicApp.Services.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;
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
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IPatientService _patientService;
        private readonly IExternalInquiryService _externalInquiryService;
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IServiceService _serviceService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly IDoctorDepartmentRepository _doctorDepartmentRepository;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IServiceCalculationService _serviceCalculationService;
        

        public ReceptionService(
            IReceptionRepository receptionRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IPatientService patientService,
            IExternalInquiryService externalInquiryService,
            IInsuranceCalculationService insuranceCalculationService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IPatientInsuranceService patientInsuranceService,
            ICurrentUserService currentUserService,
            IServiceCategoryService serviceCategoryService,
            IServiceService serviceService,
            IDoctorCrudService doctorCrudService,
            IDoctorDepartmentRepository doctorDepartmentRepository,
            ILogger logger,
            ApplicationDbContext context,
            IServiceCalculationService serviceCalculationService)
        {
            _receptionRepository = receptionRepository ?? throw new ArgumentNullException(nameof(receptionRepository));
            _paymentTransactionRepository = paymentTransactionRepository ?? throw new ArgumentNullException(nameof(paymentTransactionRepository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _externalInquiryService = externalInquiryService ?? throw new ArgumentNullException(nameof(externalInquiryService));
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _serviceCategoryService = serviceCategoryService ?? throw new ArgumentNullException(nameof(serviceCategoryService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _doctorDepartmentRepository = doctorDepartmentRepository ?? throw new ArgumentNullException(nameof(doctorDepartmentRepository));
            _logger = logger.ForContext<ReceptionService>();
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
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
                var reception = new Models.Entities.Reception.Reception
                {
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    ReceptionDate = model.ReceptionDate,
                    Status = ReceptionStatus.Pending,
                    Type = model.Type,
                    TotalAmount = model.TotalAmount,
                    PatientCoPay = model.PatientShare,
                    InsurerShareAmount = model.InsuranceShare,
                    Notes = model.Notes ?? string.Empty,
                    IsEmergency = model.IsEmergency,
                    IsOnlineReception = model.IsOnlineReception,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // ذخیره در Repository
                _receptionRepository.Add(reception);
                await _receptionRepository.SaveChangesAsync();

                // ایجاد ReceptionItems
                if (model.SelectedServiceIds != null && model.SelectedServiceIds.Any())
                {
                    try
                    {
                        var services = await _serviceService.GetActiveServicesAsync();
                        if (services == null || !services.Any())
                        {
                            _logger.Warning("هیچ خدمت فعالی یافت نشد");
                            return ServiceResult<ReceptionDetailsViewModel>.Failed(
                                "هیچ خدمت فعالی یافت نشد. لطفاً با مدیر سیستم تماس بگیرید.",
                                "NO_ACTIVE_SERVICES",
                                ErrorCategory.Validation,
                                SecurityLevel.Low);
                        }

                        var selectedServices = services.Where(s => model.SelectedServiceIds.Contains(s.ServiceId));
                        if (selectedServices != null && selectedServices.Any())
                        {
                            foreach (var service in selectedServices)
                        {
                            var receptionItem = new ReceptionItem
                            {
                                ReceptionId = reception.ReceptionId,
                                ServiceId = service.ServiceId,
                                Quantity = 1,
                                UnitPrice = service.Price,
                                CreatedByUserId = _currentUserService.UserId,
                                CreatedAt = DateTime.Now,
                                IsDeleted = false
                            };
                            // افزودن ReceptionItem به Reception
                            reception.ReceptionItems.Add(receptionItem);
                        }
                        await _receptionRepository.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در ایجاد ReceptionItems");
                        return ServiceResult<ReceptionDetailsViewModel>.Failed(
                            "خطا در ایجاد آیتم‌های پذیرش. لطفاً مجدداً تلاش کنید.",
                            "RECEPTION_ITEMS_ERROR",
                            ErrorCategory.System,
                            SecurityLevel.Medium);
                    }
                }

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

                return ServiceResult<bool>.CreateSuccess(true, "پذیرش با موفقیت حذف شد.");
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
                    "خطا در دریافت لیست پذیرش‌ها. جزئیات: {ExceptionMessage}, StackTrace: {StackTrace}, کاربر: {UserName}",
                    ex.Message, ex.StackTrace, _currentUserService.UserName);

                return ServiceResult<PagedResult<ReceptionIndexViewModel>>.Failed(
                    $"خطا در دریافت لیست پذیرش‌ها: {ex.Message}",
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

                // محاسبه مبلغ پرداخت شده به صورت همزمان
                var paidAmountTask = CalculatePaidAmountAsync(reception.ReceptionId);
                
                // تبدیل به ViewModel
                var viewModel = new ReceptionDetailsViewModel
                {
                    ReceptionId = reception.ReceptionId,
                    PatientFullName = $"{reception.Patient.FirstName} {reception.Patient.LastName}",
                    PatientNationalCode = reception.Patient.NationalCode,
                    PatientPhoneNumber = reception.Patient.PhoneNumber,
                    DoctorFullName = $"{reception.Doctor.FirstName} {reception.Doctor.LastName}",
                    DoctorSpecialization = "نامشخص", // TODO: Get specialization from Doctor entity
                    ReceptionDate = reception.ReceptionDate.ToPersianDateTime(),
                    Status = GetReceptionStatusText(reception.Status),
                    Type = GetReceptionTypeDisplayName(reception.Type),
                    TotalAmount = reception.TotalAmount,
                    PaidAmount = await paidAmountTask, // استفاده از await در انتها
                    InsuranceShare = reception.InsurerShareAmount,
                    PatientShare = reception.PatientCoPay,
                    Notes = reception.Notes,
                    CreatedAt = reception.CreatedAt,
                    UpdatedAt = reception.UpdatedAt,
                    CreatedBy = reception.CreatedByUser?.UserName ?? "سیستم",
                    Services = reception.ReceptionItems?.Select(ri => new ReceptionItemViewModel
                    {
                        ServiceName = ri.Service?.Title ?? "نامشخص",
                        ServiceCategory = ri.Service?.ServiceCategory?.Title ?? "نامشخص",
                        ServiceTitle = ri.Service?.Title ?? "نامشخص",
                        Quantity = ri.Quantity,
                        Price = ri.UnitPrice
                    }).ToList() ?? new List<ReceptionItemViewModel>(),
                    Payments = reception.Transactions?.Where(t => !t.IsDeleted).Select(t => new ReceptionPaymentViewModel
                    {
                        Id = t.PaymentTransactionId,
                        Amount = t.Amount,
                        PaymentMethod = t.Method.ToString(),
                        TransactionDate = t.CreatedAt,
                        Status = t.Status.ToString(),
                        ReferenceNumber = t.ReferenceCode,
                        Notes = t.Description
                    }).ToList() ?? new List<ReceptionPaymentViewModel>()
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

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>اطلاعات پذیرش</returns>
        public async Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionByIdAsync(int id)
        {
            return await GetReceptionDetailsAsync(id);
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
        /// جستجوی پذیرش‌ها با مدل (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجو</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> SearchReceptionsAsync(
            ReceptionSearchViewModel model,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await SearchReceptionsAsync(model.SearchTerm ?? "", pageNumber, pageSize);
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
        /// دریافت تاریخچه پذیرش‌های بیمار (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>تاریخچه پذیرش‌های بیمار</returns>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPatientReceptionHistoryAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await GetPatientReceptionsAsync(patientId, pageNumber, pageSize);
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
        /// جستجوی بیمار بر اساس کد ملی (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>اطلاعات بیمار</returns>
        public async Task<ServiceResult<ReceptionPatientLookupViewModel>> SearchPatientByNationalCodeAsync(string nationalCode)
        {
            return await LookupPatientByNationalCodeAsync(nationalCode);
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
        /// ایجاد بیمار جدید (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="model">مدل ایجاد بیمار</param>
        /// <returns>اطلاعات بیمار جدید</returns>
        public async Task<ServiceResult<ReceptionPatientLookupViewModel>> CreatePatientAsync(PatientCreateEditViewModel model)
        {
            return await CreatePatientInlineAsync(model);
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
                // دریافت پزشکان بر اساس تخصص
                var doctorFilter = new DoctorSearchViewModel
                {
                    SpecializationId = specializationId,
                    PageNumber = 1,
                    PageSize = 1000
                };
                var doctorsResult = await _doctorCrudService.GetDoctorsAsync(doctorFilter);
                var doctors = doctorsResult.Success ? doctorsResult.Data.Items.Select(d => new Doctor
                {
                    DoctorId = d.DoctorId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    IsActive = d.IsActive,
                    IsDeleted = false
                }).ToList() : new List<Doctor>();
                
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

        /// <summary>
        /// دریافت دپارتمان‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست دپارتمان‌های پزشک</returns>
        public async Task<ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>> GetDoctorDepartmentsAsync(int doctorId)
        {
            _logger.Information(
                "درخواست دپارتمان‌های پزشک. پزشک: {DoctorId}, کاربر: {UserName}",
                doctorId, _currentUserService.UserName);

            try
            {
                if (doctorId <= 0)
                {
                    return ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>.Failed(
                        "شناسه پزشک نامعتبر است.",
                        "INVALID_DOCTOR_ID",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // دریافت دپارتمان‌های پزشک از دیتابیس
                var doctorDepartments = await _doctorDepartmentRepository.GetDoctorDepartmentsAsync(doctorId, "", 1, 100);
                
                var departments = doctorDepartments.Select(dd => new ReceptionDoctorDepartmentLookupViewModel
                {
                    DepartmentId = dd.DepartmentId,
                    DepartmentName = dd.Department?.Name ?? "نامشخص",
                    DepartmentDescription = dd.Department?.Description ?? "",
                    DoctorId = dd.DoctorId,
                    DoctorName = dd.Doctor?.FirstName + " " + dd.Doctor?.LastName ?? "نامشخص",
                    IsActive = dd.IsActive,
                    JoinDate = dd.StartDate,
                    JoinDateShamsi = dd.StartDate?.ToString("yyyy/MM/dd") ?? "",
                    Role = dd.Role ?? "پزشک",
                    DisplayName = dd.Department?.Name ?? "نامشخص"
                }).ToList();

                _logger.Information(
                    "دپارتمان‌های پزشک با موفقیت دریافت شد. تعداد: {Count}, پزشک: {DoctorId}. کاربر: {UserName}",
                    departments.Count, doctorId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>.Successful(
                    departments,
                    "دپارتمان‌های پزشک با موفقیت دریافت شد.",
                    operationName: "GetDoctorDepartments");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت دپارتمان‌های پزشک. پزشک: {DoctorId}. کاربر: {UserName}",
                    doctorId, _currentUserService.UserName);

                return ServiceResult<List<ReceptionDoctorDepartmentLookupViewModel>>.Failed(
                    "خطا در دریافت دپارتمان‌های پزشک. لطفاً مجدداً تلاش کنید.",
                    "DOCTOR_DEPARTMENTS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدمات بر اساس دپارتمان‌ها
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان‌ها</param>
        /// <returns>لیست سرفصل‌های خدمات</returns>
        public async Task<ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>> GetServiceCategoriesByDepartmentsAsync(List<int> departmentIds)
        {
            _logger.Information(
                "درخواست سرفصل‌های خدمات بر اساس دپارتمان‌ها. دپارتمان‌ها: {DepartmentIds}, کاربر: {UserName}",
                string.Join(",", departmentIds), _currentUserService.UserName);

            try
            {
                if (departmentIds == null || !departmentIds.Any())
                {
                    return ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>.Failed(
                        "شناسه‌های دپارتمان الزامی است.",
                        "INVALID_DEPARTMENT_IDS",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // دریافت سرفصل‌های فعال
                var categories = await _serviceCategoryService.GetActiveServiceCategoriesAsync();
                
                // فیلتر کردن بر اساس دپارتمان‌ها
                var filteredCategories = categories.Where(c => departmentIds.Contains(c.DepartmentId)).ToList();
                
                // تبدیل به ViewModel
                var viewModels = filteredCategories.Select(c => new ReceptionServiceCategoryLookupViewModel
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
                    "سرفصل‌های خدمات بر اساس دپارتمان‌ها با موفقیت دریافت شد. تعداد: {Count}, دپارتمان‌ها: {DepartmentIds}. کاربر: {UserName}",
                    viewModels.Count, string.Join(",", departmentIds), _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>.Successful(
                    viewModels,
                    "سرفصل‌های خدمات بر اساس دپارتمان‌ها با موفقیت دریافت شد.",
                    operationName: "GetServiceCategoriesByDepartments");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت سرفصل‌های خدمات بر اساس دپارتمان‌ها. دپارتمان‌ها: {DepartmentIds}. کاربر: {UserName}",
                    string.Join(",", departmentIds), _currentUserService.UserName);

                return ServiceResult<List<ReceptionServiceCategoryLookupViewModel>>.Failed(
                    "خطا در دریافت سرفصل‌های خدمات. لطفاً مجدداً تلاش کنید.",
                    "SERVICE_CATEGORIES_BY_DEPARTMENTS_ERROR",
                    ErrorCategory.System,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دپارتمان‌ها (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان‌ها</param>
        /// <returns>لیست خدمات</returns>
        public async Task<ServiceResult<List<ReceptionServiceLookupViewModel>>> GetServicesByDepartmentsAsync(List<int> departmentIds)
        {
            // TODO: پیاده‌سازی منطق دریافت خدمات بر اساس دپارتمان‌ها
            return ServiceResult<List<ReceptionServiceLookupViewModel>>.Successful(new List<ReceptionServiceLookupViewModel>());
        }

        public async Task<ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>> GetPatientActiveInsurancesAsync(int patientId)
        {
            _logger.Information(
                "درخواست بیمه‌های فعال بیمار. بیمار: {PatientId}, کاربر: {UserName}",
                patientId, _currentUserService.UserName);

            try
            {
                // 🏥 MEDICAL: استفاده از PatientInsuranceService برای دریافت بیمه‌های فعال
                var patientInsurancesResult = await _patientInsuranceService.GetActiveAndSupplementaryByPatientIdAsync(patientId);
                
                if (!patientInsurancesResult.Success)
                {
                    _logger.Warning("خطا در دریافت بیمه‌های فعال بیمار. بیمار: {PatientId}, خطا: {Error}, کاربر: {UserName}",
                        patientId, patientInsurancesResult.Message, _currentUserService.UserName);
                    
                    return ServiceResult<List<ReceptionPatientInsuranceLookupViewModel>>.Failed(
                        "خطا در دریافت بیمه‌های بیمار. لطفاً مجدداً تلاش کنید.",
                        "PATIENT_INSURANCES_ERROR",
                        ErrorCategory.System,
                        SecurityLevel.Medium);
                }

                var result = patientInsurancesResult.Data.Select(pi => new ReceptionPatientInsuranceLookupViewModel
                {
                    PatientInsuranceId = pi.PatientInsuranceId,
                    PatientId = pi.PatientId,
                    InsuranceProviderName = pi.InsuranceProviderName ?? "نامشخص",
                    PolicyNumber = pi.PolicyNumber,
                    StartDate = pi.StartDate,
                    EndDate = pi.EndDate,
                    IsActive = pi.IsActive && (pi.EndDate == null || pi.EndDate > DateTime.Now),
                    CoveragePercentage = pi.CoveragePercent
                }).ToList();

                _logger.Information("🏥 MEDICAL: دریافت بیمه‌های فعال بیمار موفق. بیمار: {PatientId}, تعداد: {Count}, کاربر: {UserName}",
                    patientId, result.Count, _currentUserService.UserName);

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
                if (serviceIds == null || serviceIds.Count == 0)
                {
                    return ServiceResult<ReceptionCostCalculationViewModel>.Failed(
                        "حداقل یک خدمت باید انتخاب شود.",
                        "NO_SERVICES_SELECTED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                var result = new ReceptionCostCalculationViewModel
                {
                    ServiceDetails = new List<ServiceCostDetailViewModel>(),
                    CalculationDate = receptionDate
                };

                decimal totalAmount = 0;
                decimal totalInsuranceShare = 0;
                decimal totalPatientShare = 0;

                // دریافت اطلاعات خدمات
                var allServices = await _serviceService.GetActiveServicesAsync();
                var services = allServices.Where(s => serviceIds.Contains(s.ServiceId)).ToList();
                if (services == null || services.Count == 0)
                {
                    return ServiceResult<ReceptionCostCalculationViewModel>.Failed(
                        "خدمات انتخاب شده یافت نشد.",
                        "SERVICES_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // 🏥 MEDICAL: بررسی وجود بیمه ترکیبی برای بیمار
                bool hasCombinedInsurance = false;
                if (insuranceId.HasValue)
                {
                    var combinedInsuranceCheck = await _patientInsuranceService.HasCombinedInsuranceAsync(patientId);
                    if (combinedInsuranceCheck.Success)
                    {
                        hasCombinedInsurance = combinedInsuranceCheck.Data;
                        _logger.Information("🏥 MEDICAL: بررسی بیمه ترکیبی - PatientId: {PatientId}, HasCombined: {HasCombined}, User: {UserName}",
                            patientId, hasCombinedInsurance, _currentUserService.UserName);
                    }
                }

                foreach (var serviceItem in services)
                {
                    // دریافت خدمت کامل از دیتابیس
                    var service = _context.Services
                        .Include(s => s.ServiceComponents)
                        .FirstOrDefault(s => s.ServiceId == serviceItem.ServiceId && !s.IsDeleted);

                    if (service == null)
                        continue;

                    // محاسبه قیمت واقعی خدمت با استفاده از ServiceCalculationService
                    var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                        service, _context, receptionDate);

                    var serviceCost = new ServiceCostDetailViewModel
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.Title,
                        BasePrice = calculatedPrice // استفاده از قیمت محاسبه شده
                    };

                    // 🏥 MEDICAL: محاسبه سهم بیمه با استفاده از CombinedInsuranceCalculationService
                    if (insuranceId.HasValue)
                    {
                        try
                        {
                            if (hasCombinedInsurance)
                            {
                                // استفاده از CombinedInsuranceCalculationService برای محاسبه ترکیبی پیشرفته
                                var combinedResult = await _combinedInsuranceCalculationService.CalculateAdvancedCombinedInsuranceAsync(
                                    patientId, service.ServiceId, calculatedPrice, receptionDate, GetAdvancedCalculationSettings());

                                if (combinedResult.Success)
                                {
                                    var combinedData = combinedResult.Data;
                                    serviceCost.InsuranceShare = combinedData.TotalInsuranceCoverage;
                                    serviceCost.PatientShare = combinedData.FinalPatientShare;
                                    serviceCost.CoveragePercentage = combinedData.TotalCoveragePercent;
                                    
                                    _logger.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی پیشرفته موفق - ServiceId: {ServiceId}, PrimaryCoverage: {PrimaryCoverage}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                                        service.ServiceId, combinedData.PrimaryCoverage, combinedData.SupplementaryCoverage, combinedData.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                                }
                                else
                                {
                                    // Fallback به محاسبه عادی در صورت خطا
                                    _logger.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی، استفاده از محاسبه عادی - ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                                        service.ServiceId, combinedResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                                    
                                    // محاسبه عادی بیمه
                                    serviceCost.InsuranceShare = 0;
                                    serviceCost.PatientShare = calculatedPrice;
                                    serviceCost.CoveragePercentage = 0;
                                }
                            }
                            else
                            {
                                // محاسبه عادی بیمه (بدون بیمه ترکیبی)
                                serviceCost.InsuranceShare = 0;
                                serviceCost.PatientShare = calculatedPrice;
                                serviceCost.CoveragePercentage = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی - ServiceId: {ServiceId}, PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                                service.ServiceId, patientId, _currentUserService.UserName, _currentUserService.UserId);
                            
                            // Fallback به محاسبه عادی
                            serviceCost.InsuranceShare = 0;
                            serviceCost.PatientShare = calculatedPrice;
                            serviceCost.CoveragePercentage = 0;
                        }
                    }
                    else
                    {
                        serviceCost.InsuranceShare = 0;
                        serviceCost.PatientShare = calculatedPrice;
                        serviceCost.CoveragePercentage = 0;
                    }

                    result.ServiceDetails.Add(serviceCost);

                    totalAmount += calculatedPrice;
                    totalInsuranceShare += serviceCost.InsuranceShare;
                    totalPatientShare += serviceCost.PatientShare;
                }

                result.TotalServiceCost = totalAmount;
                result.InsuranceShare = totalInsuranceShare;
                result.PatientShare = totalPatientShare;

                _logger.Information(
                    "محاسبه هزینه‌های پذیرش با موفقیت انجام شد. بیمار: {PatientId}, مجموع: {TotalAmount}, سهم بیمه: {InsuranceShare}, سهم بیمار: {PatientShare}, کاربر: {UserName}",
                    patientId, totalAmount, totalInsuranceShare, totalPatientShare, _currentUserService.UserName);

                return ServiceResult<ReceptionCostCalculationViewModel>.Successful(
                    result,
                    "محاسبه هزینه‌های پذیرش با موفقیت انجام شد.",
                    operationName: "CalculateReceptionCosts");
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
                var validation = new ReceptionValidationViewModel
                {
                    ValidationErrors = new List<string>(),
                    Warnings = new List<string>()
                };

                // بررسی وجود بیمار
                var patientResult = await _patientService.GetPatientDetailsAsync(patientId);
                var patient = patientResult.Success ? patientResult.Data : null;
                if (patient == null)
                {
                    validation.ValidationErrors.Add("بیمار مورد نظر یافت نشد.");
                }
                else if (patient.IsDeleted)
                {
                    validation.ValidationErrors.Add("بیمار مورد نظر حذف شده است.");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                var doctor = doctorResult.Success ? doctorResult.Data : null;
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

                // بررسی تداخل زمانی - تعداد پذیرش‌های پزشک در تاریخ مشخص
                if (doctor != null && receptionDate >= DateTime.Today)
                {
                    var existingReceptions = await _receptionRepository.GetReceptionsByDoctorAndDateAsync(doctorId, receptionDate);
                    var maxReceptionsPerDay = 20; // حداکثر ۲۰ پذیرش در روز
                    
                    if (existingReceptions.Count >= maxReceptionsPerDay)
                    {
                        validation.Warnings.Add($"ظرفیت پذیرش پزشک در این تاریخ تکمیل است. ({existingReceptions.Count}/{maxReceptionsPerDay})");
                    }
                    else if (existingReceptions.Count >= maxReceptionsPerDay * 0.8) // 80% ظرفیت
                    {
                        validation.Warnings.Add($"ظرفیت پذیرش پزشک در این تاریخ تقریباً تکمیل است. ({existingReceptions.Count}/{maxReceptionsPerDay})");
                    }
                }

                // بررسی پذیرش‌های قبلی بیمار در همان روز
                if (patient != null && receptionDate >= DateTime.Today)
                {
                    var patientReceptions = await _receptionRepository.GetReceptionsByDateAsync(receptionDate);
                    var patientReceptionsToday = patientReceptions.Where(r => r.PatientId == patientId && !r.IsDeleted).ToList();
                    
                    if (patientReceptionsToday.Count > 0)
                    {
                        validation.Warnings.Add($"بیمار در این تاریخ قبلاً پذیرش دارد. ({patientReceptionsToday.Count} پذیرش)");
                    }
                }

                validation.IsValid = validation.ValidationErrors.Count == 0;

                _logger.Information(
                    "اعتبارسنجی پذیرش انجام شد. بیمار: {PatientId}, پزشک: {DoctorId}, معتبر: {IsValid}, خطاها: {ErrorCount}, هشدارها: {WarningCount}, کاربر: {UserName}",
                    patientId, doctorId, validation.IsValid, validation.ValidationErrors.Count, validation.Warnings.Count, _currentUserService.UserName);

                return ServiceResult<ReceptionValidationViewModel>.Successful(
                    validation,
                    validation.IsValid ? "اعتبارسنجی پذیرش با موفقیت انجام شد." : "اعتبارسنجی پذیرش با خطا مواجه شد.",
                    operationName: "ValidateReception");
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
                    Date = date,
                    DoctorStats = new List<ReceptionDoctorStatsViewModel>()
                };

                // دریافت آمار کلی - TODO: بهینه‌سازی با query های جداگانه
                var dailyReceptions = await _receptionRepository.GetReceptionsByDateAsync(date);
                
                stats.TotalReceptions = dailyReceptions.Count;
                stats.CompletedReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Completed);
                stats.PendingReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Pending);
                stats.CancelledReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.Cancelled);
                stats.InProgressReceptions = dailyReceptions.Count(r => r.Status == ReceptionStatus.InProgress);

                // محاسبه درآمد
                stats.TotalRevenue = dailyReceptions.Sum(r => r.TotalAmount);
                stats.AverageRevenuePerReception = dailyReceptions.Count > 0 ? stats.TotalRevenue / dailyReceptions.Count : 0;

                // آمار پزشکان
                var doctorGroups = dailyReceptions
                    .Where(r => r.Doctor != null)
                    .GroupBy(r => r.DoctorId)
                    .ToList();

                foreach (var group in doctorGroups)
                {
                    var doctor = group.First().Doctor;
                    var doctorReceptions = group.ToList();
                    
                    stats.DoctorStats.Add(new ReceptionDoctorStatsViewModel
                    {
                        DoctorId = doctor.DoctorId,
                        DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                        ReceptionsCount = doctorReceptions.Count,
                        TotalRevenue = doctorReceptions.Sum(r => r.TotalAmount),
                        AverageRevenue = doctorReceptions.Count > 0 ? doctorReceptions.Sum(r => r.TotalAmount) / doctorReceptions.Count : 0,
                        CompletedReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Completed),
                        PendingReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Pending),
                        CancelledReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Cancelled)
                    });
                }

                // آمار انواع پذیرش
                stats.EmergencyReceptions = dailyReceptions.Count(r => r.IsEmergency);
                stats.OnlineReceptions = dailyReceptions.Count(r => r.IsOnlineReception);
                stats.NormalReceptions = dailyReceptions.Count(r => !r.IsEmergency && !r.IsOnlineReception);

                // آمار روش‌های پرداخت
                var paymentGroups = dailyReceptions
                    .Where(r => r.Transactions != null && r.Transactions.Any())
                    .SelectMany(r => r.Transactions)
                    .GroupBy(t => t.Method)
                    .ToList();

                stats.CashPayments = paymentGroups.Where(g => g.Key == PaymentMethod.Cash).Sum(g => g.Sum(t => t.Amount));
                stats.CardPayments = paymentGroups.Where(g => g.Key == PaymentMethod.Card).Sum(g => g.Sum(t => t.Amount));
                stats.OnlinePayments = paymentGroups.Where(g => g.Key == PaymentMethod.Online).Sum(g => g.Sum(t => t.Amount));
                stats.InsurancePayments = paymentGroups.Where(g => g.Key == PaymentMethod.Insurance).Sum(g => g.Sum(t => t.Amount));

                _logger.Information(
                    "آمار روزانه پذیرش‌ها با موفقیت دریافت شد. تاریخ: {Date}, کل پذیرش‌ها: {TotalReceptions}, درآمد کل: {TotalRevenue}, کاربر: {UserName}",
                    date, stats.TotalReceptions, stats.TotalRevenue, _currentUserService.UserName);

                return ServiceResult<ReceptionDailyStatsViewModel>.Successful(
                    stats,
                    "آمار روزانه پذیرش‌ها با موفقیت دریافت شد.",
                    operationName: "GetDailyStats");
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
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                var doctor = doctorResult.Success ? doctorResult.Data : null;
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
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    Date = date
                };

                // دریافت پذیرش‌های پزشک در تاریخ مشخص
                var doctorReceptions = await _receptionRepository.GetReceptionsByDoctorAndDateAsync(doctorId, date);
                
                stats.ReceptionsCount = doctorReceptions.Count;
                stats.TotalRevenue = doctorReceptions.Sum(r => r.TotalAmount);
                stats.AverageRevenue = doctorReceptions.Count > 0 ? stats.TotalRevenue / doctorReceptions.Count : 0;

                // آمار تفصیلی وضعیت‌ها
                stats.CompletedReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Completed);
                stats.PendingReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Pending);
                stats.CancelledReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.Cancelled);
                stats.InProgressReceptions = doctorReceptions.Count(r => r.Status == ReceptionStatus.InProgress);

                // آمار انواع پذیرش
                stats.EmergencyReceptions = doctorReceptions.Count(r => r.IsEmergency);
                stats.OnlineReceptions = doctorReceptions.Count(r => r.IsOnlineReception);
                stats.NormalReceptions = doctorReceptions.Count(r => !r.IsEmergency && !r.IsOnlineReception);

                // آمار روش‌های پرداخت
                var paymentGroups = doctorReceptions
                    .Where(r => r.Transactions != null && r.Transactions.Any())
                    .SelectMany(r => r.Transactions)
                    .GroupBy(t => t.Method)
                    .ToList();

                stats.CashPayments = paymentGroups.Where(g => g.Key == PaymentMethod.Cash).Sum(g => g.Sum(t => t.Amount));
                stats.CardPayments = paymentGroups.Where(g => g.Key == PaymentMethod.Card).Sum(g => g.Sum(t => t.Amount));
                stats.OnlinePayments = paymentGroups.Where(g => g.Key == PaymentMethod.Online).Sum(g => g.Sum(t => t.Amount));
                stats.InsurancePayments = paymentGroups.Where(g => g.Key == PaymentMethod.Insurance).Sum(g => g.Sum(t => t.Amount));

                // محاسبه درصدها
                if (stats.ReceptionsCount > 0)
                {
                    stats.CompletionRate = (decimal)stats.CompletedReceptions / stats.ReceptionsCount * 100;
                    stats.CancellationRate = (decimal)stats.CancelledReceptions / stats.ReceptionsCount * 100;
                    stats.EmergencyRate = (decimal)stats.EmergencyReceptions / stats.ReceptionsCount * 100;
                }

                _logger.Information(
                    "آمار پزشک با موفقیت دریافت شد. پزشک: {DoctorId}, تاریخ: {Date}, پذیرش‌ها: {ReceptionsCount}, درآمد: {TotalRevenue}, کاربر: {UserName}",
                    doctorId, date, stats.ReceptionsCount, stats.TotalRevenue, _currentUserService.UserName);

                return ServiceResult<ReceptionDoctorStatsViewModel>.Successful(
                    stats,
                    "آمار پزشک با موفقیت دریافت شد.",
                    operationName: "GetDoctorStats");
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

                // بررسی وضعیت پذیرش
                if (reception.Status == ReceptionStatus.Cancelled)
                {
                    return ServiceResult<PaymentTransactionViewModel>.Failed(
                        "امکان افزودن پرداخت به پذیرش لغو شده وجود ندارد.",
                        "RECEPTION_CANCELLED",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // بررسی مبلغ پرداخت
                if (paymentModel.Amount <= 0)
                {
                    return ServiceResult<PaymentTransactionViewModel>.Failed(
                        "مبلغ پرداخت باید بزرگتر از صفر باشد.",
                        "INVALID_PAYMENT_AMOUNT",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // محاسبه مبلغ باقی‌مانده
                var currentPaidAmount = await CalculatePaidAmountAsync(receptionId);
                var remainingAmount = reception.TotalAmount - currentPaidAmount;

                if (paymentModel.Amount > remainingAmount)
                {
                    return ServiceResult<PaymentTransactionViewModel>.Failed(
                        $"مبلغ پرداخت نمی‌تواند بیش از مبلغ باقی‌مانده باشد. مبلغ باقی‌مانده: {remainingAmount:N0} تومان",
                        "PAYMENT_AMOUNT_EXCEEDS_REMAINING",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // ایجاد تراکنش پرداخت
                var paymentTransaction = new PaymentTransaction
                {
                    ReceptionId = receptionId,
                    Amount = paymentModel.Amount,
                    Method = paymentModel.Method,
                    CreatedAt = DateTime.Now,
                    Status = PaymentStatus.Completed,
                    ReferenceCode = paymentModel.ReferenceNumber ?? Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    Description = paymentModel.Notes ?? string.Empty,
                    CreatedByUserId = _currentUserService.UserId,
                    IsDeleted = false
                };

                // ذخیره تراکنش
                await _paymentTransactionRepository.AddAsync(paymentTransaction);

                // به‌روزرسانی وضعیت پذیرش در صورت تکمیل پرداخت
                var newPaidAmount = currentPaidAmount + paymentModel.Amount;
                if (newPaidAmount >= reception.TotalAmount)
                {
                    reception.Status = ReceptionStatus.Completed;
                    reception.UpdatedAt = DateTime.Now;
                    reception.UpdatedByUserId = _currentUserService.UserId;
                }

                await _receptionRepository.SaveChangesAsync();

                // بازگرداندن نتیجه
                var result = new PaymentTransactionViewModel
                {
                    Id = paymentTransaction.PaymentTransactionId,
                    ReceptionId = paymentTransaction.ReceptionId,
                    Amount = paymentTransaction.Amount,
                    PaymentMethod = paymentTransaction.Method.ToString(),
                    TransactionDate = paymentTransaction.CreatedAt,
                    Status = paymentTransaction.Status.ToString(),
                    ReferenceNumber = paymentTransaction.ReferenceCode,
                    Notes = paymentTransaction.Description
                };

                _logger.Information(
                    "پرداخت جدید با موفقیت افزوده شد. پذیرش: {ReceptionId}, مبلغ: {Amount}, تراکنش: {TransactionId}, کاربر: {UserName}",
                    receptionId, paymentModel.Amount, paymentTransaction.PaymentTransactionId, _currentUserService.UserName);

                return ServiceResult<PaymentTransactionViewModel>.Successful(
                    result,
                    "پرداخت جدید با موفقیت افزوده شد.",
                    operationName: "AddPayment");
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
                // بررسی وجود پذیرش
                var reception = await _receptionRepository.GetByIdAsync(receptionId);
                if (reception == null)
                {
                    return ServiceResult<List<PaymentTransactionViewModel>>.Failed(
                        "پذیرش مورد نظر یافت نشد.",
                        "RECEPTION_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // دریافت تراکنش‌های پرداخت
                var payments = await _receptionRepository.GetReceptionPaymentsAsync(receptionId);
                
                var result = payments
                    .Where(p => !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PaymentTransactionViewModel
                    {
                        Id = p.PaymentTransactionId,
                        ReceptionId = p.ReceptionId,
                        Amount = p.Amount,
                        PaymentMethod = p.Method.ToString(),
                        TransactionDate = p.CreatedAt,
                        Status = p.Status.ToString(),
                        ReferenceNumber = p.ReferenceCode,
                        Notes = p.Description
                    }).ToList();

                _logger.Information(
                    "لیست پرداخت‌های پذیرش با موفقیت دریافت شد. پذیرش: {ReceptionId}, تعداد پرداخت‌ها: {PaymentCount}, کاربر: {UserName}",
                    receptionId, result.Count, _currentUserService.UserName);

                return ServiceResult<List<PaymentTransactionViewModel>>.Successful(
                    result,
                    "لیست پرداخت‌های پذیرش با موفقیت دریافت شد.",
                    operationName: "GetReceptionPayments");
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
                var lookupLists = new ReceptionLookupListsViewModel
                {
                    Doctors = new List<ReceptionDoctorLookupViewModel>(),
                    ServiceCategories = new List<ReceptionServiceCategoryLookupViewModel>(),
                    Services = new List<ReceptionServiceLookupViewModel>(),
                    PaymentMethods = new List<PaymentMethodLookupViewModel>()
                };

                // دریافت پزشکان
                var doctorsResult = await GetDoctorsAsync();
                if (doctorsResult.Success)
                {
                    lookupLists.Doctors = doctorsResult.Data;
                }

                // دریافت دسته‌بندی‌های خدمات
                var serviceCategoriesResult = await GetServiceCategoriesAsync();
                if (serviceCategoriesResult.Success)
                {
                    lookupLists.ServiceCategories = serviceCategoriesResult.Data;
                }

                // دریافت خدمات (همه خدمات فعال)
                var allServices = await _serviceService.GetActiveServicesAsync();
                if (allServices != null && allServices.Any())
                {
                    lookupLists.Services = allServices.Select(s => new ReceptionServiceLookupViewModel
                    {
                        ServiceId = s.ServiceId,
                        Title = s.Title,
                        Description = s.Title ?? string.Empty,
                        ServiceCategoryId = s.ServiceCategoryId,
                        ServiceCategoryName = s.ServiceCategoryTitle ?? string.Empty,
                        BasePrice = s.Price,
                        IsActive = true, // All services from GetActiveServicesAsync are active
                        DisplayName = s.Title,
                        PriceDisplay = $"{s.Price:N0} تومان"
                    }).ToList();
                }

                // روش‌های پرداخت
                lookupLists.PaymentMethods = Enum.GetValues(typeof(PaymentMethod))
                    .Cast<PaymentMethod>()
                    .Select(pm => new PaymentMethodLookupViewModel
                    {
                        Value = pm,
                        Text = GetPaymentMethodDisplayName(pm),
                        Description = GetPaymentMethodDisplayName(pm),
                        DisplayName = GetPaymentMethodDisplayName(pm)
                    }).ToList();

                _logger.Information(
                    "لیست‌های کمکی با موفقیت دریافت شد. پزشکان: {DoctorCount}, دسته‌بندی‌ها: {CategoryCount}, خدمات: {ServiceCount}, روش‌های پرداخت: {PaymentMethodCount}, کاربر: {UserName}",
                    lookupLists.Doctors.Count, lookupLists.ServiceCategories.Count, lookupLists.Services.Count, lookupLists.PaymentMethods.Count, _currentUserService.UserName);

                return ServiceResult<ReceptionLookupListsViewModel>.Successful(
                    lookupLists,
                    "لیست‌های کمکی با موفقیت دریافت شد.",
                    operationName: "GetLookupLists");
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

                // جستجو در دیتابیس محلی (7 هزار رکورد بیمار)
                var existingPatient = await _patientService.SearchPatientsAsync(nationalCode, 1, 1);
                if (existingPatient.Success && existingPatient.Data.Items.Any())
                {
                    var patient = existingPatient.Data.Items.First();
                    _logger.Information(
                        "بیمار موجود در دیتابیس لوکال یافت شد. کد ملی: {NationalCode}, نام: {FullName}, کاربر: {UserName}",
                        nationalCode, patient.FullName, _currentUserService.UserName);

                    // بازگرداندن اطلاعات بیمار موجود از دیتابیس لوکال
                    var inquiryResult = new PatientInquiryViewModel
                    {
                        NationalCode = nationalCode,
                        BirthDate = birthDate,
                        BirthDateShamsi = birthDate.ToString("yyyy/MM/dd"),
                        InquiryType = InquiryType.Both,
                        Status = InquiryStatus.Successful,
                        Message = "بیمار در دیتابیس لوکال موجود است.",
                        IsSuccessful = true,
                        InquiryTime = DateTime.Now,
                        IdentityData = new PatientIdentityData
                        {
                            FirstName = patient.FirstName,
                            LastName = patient.LastName,
                            BirthDate = patient.BirthDate,
                            Gender = patient.Gender,
                            Address = patient.Address,
                            IsVerified = true,
                            VerificationDate = DateTime.Now
                        },
                        InsuranceData = new PatientInsuranceData
                        {
                            InsuranceName = "بیمه موجود در دیتابیس لوکال",
                            InsuranceStatus = "فعال",
                            IsVerified = true,
                            VerificationDate = DateTime.Now
                        },
                        IsDataBound = true
                    };

                    return ServiceResult<PatientInquiryViewModel>.Successful(
                        inquiryResult,
                        "بیمار در دیتابیس لوکال موجود است.",
                        operationName: "InquiryPatientIdentity");
                }

                // TODO: پیاده‌سازی اتصال به سیستم خارجی (شبکه شمس)
                // در انتظار مجوز وزارت بهداشت و دسترسی به شبکه شمس
                // فعلاً از Mock Data استفاده می‌کنیم
                await Task.Delay(500); // شبیه‌سازی تأخیر کوتاه

                _logger.Information(
                    "بیمار در دیتابیس لوکال یافت نشد. کد ملی: {NationalCode}, کاربر: {UserName}",
                    nationalCode, _currentUserService.UserName);

                // شبیه‌سازی نتیجه استعلام برای بیمار جدید
                var newPatientInquiryResult = new PatientInquiryViewModel
                {
                    NationalCode = nationalCode,
                    BirthDate = birthDate,
                    BirthDateShamsi = birthDate.ToString("yyyy/MM/dd"), // تبدیل به شمسی
                    InquiryType = InquiryType.Both,
                    Status = InquiryStatus.Successful,
                    Message = "بیمار جدید - اطلاعات از دیتابیس لوکال دریافت نشد. لطفاً اطلاعات را دستی وارد کنید.",
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
                    newPatientInquiryResult,
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

        #region Enhanced Error Handling

        /// <summary>
        /// مدیریت پیشرفته خطاها با جزئیات بیشتر
        /// </summary>
        /// <typeparam name="T">نوع نتیجه</typeparam>
        /// <param name="ex">خطای رخ داده</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="context">اطلاعات اضافی</param>
        /// <returns>ServiceResult با جزئیات خطا</returns>
        private ServiceResult<T> HandleEnhancedError<T>(Exception ex, string operationName, object context = null)
        {
            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var errorContext = new
            {
                ErrorId = errorId,
                Operation = operationName,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                Context = context
            };

            // تشخیص نوع خطا و مدیریت مناسب
            switch (ex)
            {
                case ArgumentNullException argEx:
                    _logger.Warning("خطای ورودی نامعتبر در {Operation}. خطا: {ErrorId}, ورودی: {ArgumentName}, کاربر: {UserName}",
                        operationName, errorId, argEx.ParamName, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        $"ورودی نامعتبر: {argEx.ParamName}",
                        "INVALID_INPUT",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);

                case ArgumentException argEx:
                    _logger.Warning("خطای اعتبارسنجی در {Operation}. خطا: {ErrorId}, پیام: {Message}, کاربر: {UserName}",
                        operationName, errorId, argEx.Message, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        $"خطای اعتبارسنجی: {argEx.Message}",
                        "VALIDATION_ERROR",
                        ErrorCategory.Validation,
                        SecurityLevel.Low);

                case UnauthorizedAccessException authEx:
                    _logger.Warning("خطای دسترسی در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        "شما مجوز انجام این عملیات را ندارید.",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.System,
                        SecurityLevel.High);

                case TimeoutException timeoutEx:
                    _logger.Error(timeoutEx, "خطای زمان‌بندی در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        "عملیات بیش از حد انتظار طول کشید. لطفاً مجدداً تلاش کنید.",
                        "OPERATION_TIMEOUT",
                        ErrorCategory.System,
                        SecurityLevel.Medium);

                case InvalidOperationException invalidOpEx:
                    _logger.Error(invalidOpEx, "خطای عملیات نامعتبر در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        $"عملیات نامعتبر: {invalidOpEx.Message}",
                        "INVALID_OPERATION",
                        ErrorCategory.System,
                        SecurityLevel.Medium);

                default:
                    _logger.Error(ex, "خطای غیرمنتظره در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return ServiceResult<T>.Failed(
                        "خطای غیرمنتظره‌ای رخ داده است. لطفاً با پشتیبانی تماس بگیرید.",
                        "UNEXPECTED_ERROR",
                        ErrorCategory.System,
                        SecurityLevel.High);
            }
        }

        /// <summary>
        /// اعتبارسنجی پیشرفته ورودی‌ها
        /// </summary>
        /// <param name="input">ورودی برای اعتبارسنجی</param>
        /// <param name="validationRules">قوانین اعتبارسنجی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private ServiceResult<bool> ValidateInputAdvanced<T>(T input, Dictionary<string, Func<T, bool>> validationRules)
        {
            var errors = new List<string>();

            foreach (var rule in validationRules)
            {
                try
                {
                    if (!rule.Value(input))
                    {
                        errors.Add(rule.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning("خطا در اعتبارسنجی {RuleName}: {Message}, کاربر: {UserName}",
                        rule.Key, ex.Message, _currentUserService.UserName);
                    errors.Add($"خطا در اعتبارسنجی {rule.Key}");
                }
            }

            if (errors.Any())
            {
                return ServiceResult<bool>.Failed(
                    $"خطاهای اعتبارسنجی: {string.Join(", ", errors)}",
                    "VALIDATION_FAILED",
                    ErrorCategory.Validation,
                    SecurityLevel.Low);
            }

            return ServiceResult<bool>.Successful(true);
        }

        /// <summary>
        /// دریافت تنظیمات پیشرفته برای محاسبات بیمه
        /// </summary>
        private Dictionary<string, object> GetAdvancedCalculationSettings()
        {
            try
            {
                var settings = new Dictionary<string, object>();
                
                // تنظیمات پیش‌فرض
                settings["enableAdvancedCalculation"] = true;
                settings["includeTimeRestrictions"] = true;
                settings["enableDiscounts"] = true;
                
                // تنظیمات خاص کلینیک
                var currentHour = DateTime.Now.Hour;
                if (currentHour >= 8 && currentHour <= 18)
                {
                    settings["businessHours"] = true;
                }
                else
                {
                    settings["businessHours"] = false;
                    settings["emergencySurcharge"] = 0.1m; // 10% اضافه در ساعات غیراداری
                }
                
                // تنظیمات تخفیف
                var dayOfWeek = DateTime.Now.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    settings["weekendDiscount"] = 0.05m; // 5% تخفیف در آخر هفته
                }
                
                _logger.Information("🏥 MEDICAL: تنظیمات پیشرفته محاسبه بیمه بارگذاری شد - BusinessHours: {BusinessHours}, WeekendDiscount: {WeekendDiscount}. User: {UserName} (Id: {UserId})",
                    settings["businessHours"], settings.ContainsKey("weekendDiscount") ? settings["weekendDiscount"] : 0, _currentUserService.UserName, _currentUserService.UserId);
                
                return settings;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تنظیمات پیشرفته محاسبه بیمه");
                return new Dictionary<string, object>(); // تنظیمات خالی در صورت خطا
            }
        }

        /// <summary>
        /// محاسبه مقایسه‌ای بیمه‌های مختلف برای بیمار
        /// </summary>
        public async Task<ServiceResult<List<CombinedInsuranceCalculationResult>>> CompareInsuranceOptionsForPatientAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate,
            List<int> alternativePlanIds = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: شروع مقایسه گزینه‌های بیمه برای بیمار - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                var comparisonResult = await _combinedInsuranceCalculationService.CompareInsuranceOptionsAsync(
                    patientId, serviceId, serviceAmount, calculationDate, alternativePlanIds);

                if (comparisonResult.Success)
                {
                    _logger.Information("🏥 MEDICAL: مقایسه گزینه‌های بیمه تکمیل شد - OptionsCount: {OptionsCount}. User: {UserName} (Id: {UserId})",
                        comparisonResult.Data.Count, _currentUserService.UserName, _currentUserService.UserId);
                }

                return comparisonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در مقایسه گزینه‌های بیمه - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<CombinedInsuranceCalculationResult>>.Failed("خطا در مقایسه گزینه‌های بیمه");
            }
        }

        /// <summary>
        /// جستجوی بیماران
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجوی بیماران</returns>
        public async Task<ServiceResult<PagedResult<PatientDetailsViewModel>>> SearchPatientsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Debug("جستجوی بیماران. عبارت جستجو: {SearchTerm}, صفحه: {PageNumber}, اندازه: {PageSize}", 
                    searchTerm, pageNumber, pageSize);

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<PagedResult<PatientDetailsViewModel>>.Failed("عبارت جستجو نمی‌تواند خالی باشد");
                }

                var patients = await _context.Patients
                    .Where(p => !p.IsDeleted && 
                        (p.FirstName.Contains(searchTerm) || 
                         p.LastName.Contains(searchTerm) || 
                         p.NationalCode.Contains(searchTerm) ||
                         p.PhoneNumber.Contains(searchTerm)))
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var totalCount = await _context.Patients
                    .CountAsync(p => !p.IsDeleted && 
                        (p.FirstName.Contains(searchTerm) || 
                         p.LastName.Contains(searchTerm) || 
                         p.NationalCode.Contains(searchTerm) ||
                         p.PhoneNumber.Contains(searchTerm)));

                var patientViewModels = patients.Select(p => new PatientDetailsViewModel
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    NationalCode = p.NationalCode,
                    PhoneNumber = p.PhoneNumber,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender,
                    IsActive = !p.IsDeleted
                }).ToList();

                var pagedResult = new PagedResult<PatientDetailsViewModel>(
                    patientViewModels, 
                    totalCount, 
                    pageNumber, 
                    pageSize);

                _logger.Information("جستجوی بیماران موفق. تعداد نتایج: {Count}, کاربر: {UserName}", 
                    patientViewModels.Count, _currentUserService.UserName);

                return ServiceResult<PagedResult<PatientDetailsViewModel>>.CreateSuccess(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیماران");
                return ServiceResult<PagedResult<PatientDetailsViewModel>>.Failed("خطا در جستجوی بیماران");
            }
        }

        #endregion
    }
}
