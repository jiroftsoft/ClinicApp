using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت انتصاب پزشکان به دپارتمان‌ها در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت رابطه چند-به-چند پزشک-دپارتمان
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتصاب‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 7. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorDepartmentService : IDoctorDepartmentService
    {
        private readonly IDoctorDepartmentRepository _doctorDepartmentRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorDepartmentViewModel> _validator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorDepartmentService(
            IDoctorDepartmentRepository doctorDepartmentRepository,
            IDoctorCrudRepository doctorRepository,
            IDepartmentRepository departmentRepository,
            ICurrentUserService currentUserService,
            IValidator<DoctorDepartmentViewModel> validator,
            IMapper mapper)
        {
            _doctorDepartmentRepository = doctorDepartmentRepository ?? throw new ArgumentNullException(nameof(doctorDepartmentRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorDepartmentService>();
        }

        #region Doctor-Department Management (مدیریت انتصاب پزشک به دپارتمان)

        /// <summary>
        /// دریافت لیست پزشکان فعال برای استفاده در لیست‌های کشویی (Dropdowns)
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveDoctorsForLookupAsync(int? clinicId, int? departmentId)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست پزشکان فعال برای lookup - کلینیک: {ClinicId}, دپارتمان: {DepartmentId}", clinicId, departmentId);

                // دریافت پزشکان فعال
                var doctors = await _doctorDepartmentRepository.GetActiveDoctorsForDepartmentLookupAsync(departmentId ?? 0);

                // تبدیل به ViewModel
                var lookupItems = doctors.Select(doctor => LookupItemViewModel.FromEntity(
                    doctor.DoctorId,
                    $"{doctor.FirstName} {doctor.LastName}",
                    doctor.University
                )).ToList();

                _logger.Information("لیست پزشکان فعال برای lookup با موفقیت دریافت شد. تعداد: {Count}", lookupItems.Count);

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان فعال برای lookup");
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در دریافت لیست پزشکان فعال");
            }
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌های مرتبط با یک پزشک
        /// </summary>
        public async Task<ServiceResult<PagedResult<DoctorDepartmentViewModel>>> GetDepartmentsForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("درخواست دریافت دپارتمان‌های پزشک با شناسه: {DoctorId}, صفحه: {PageNumber}", doctorId, pageNumber);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<PagedResult<DoctorDepartmentViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<PagedResult<DoctorDepartmentViewModel>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت دپارتمان‌های پزشک
                var doctorDepartments = await _doctorDepartmentRepository.GetDoctorDepartmentsAsync(doctorId, searchTerm, pageNumber, pageSize);
                var totalCount = await _doctorDepartmentRepository.GetDoctorDepartmentsCountAsync(doctorId, searchTerm);

                // تبدیل به ViewModel
                var doctorDepartmentViewModels = doctorDepartments.Select(DoctorDepartmentViewModel.FromEntity).ToList();

                // ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<DoctorDepartmentViewModel>(
                    doctorDepartmentViewModels,
                    totalCount,
                    pageNumber,
                    pageSize
                ).WithMedicalInfo(containsSensitiveData: true, SecurityLevel.High);

                _logger.Information("دپارتمان‌های پزشک با شناسه {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", doctorId, doctorDepartmentViewModels.Count);

                return ServiceResult<PagedResult<DoctorDepartmentViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دپارتمان‌های پزشک {DoctorId}", doctorId);
                return ServiceResult<PagedResult<DoctorDepartmentViewModel>>.Failed("خطا در دریافت دپارتمان‌های پزشک");
            }
        }

        /// <summary>
        /// انتصاب یک پزشک به یک دپارتمان با مشخص کردن نقش و سایر جزئیات
        /// </summary>
        public async Task<ServiceResult> AssignDoctorToDepartmentAsync(DoctorDepartmentViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", model.DoctorId, model.DepartmentId);

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل انتصاب پزشک به دپارتمان ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(model.DoctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", model.DoctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وجود دپارتمان
                var department = await _departmentRepository.GetByIdAsync(model.DepartmentId);
                if (department == null)
                {
                    _logger.Warning("دپارتمان با شناسه {DepartmentId} یافت نشد", model.DepartmentId);
                    return ServiceResult.Failed("دپارتمان مورد نظر یافت نشد.");
                }

                // بررسی وجود انتصاب قبلی
                var existingAssignment = await _doctorDepartmentRepository.GetDoctorDepartmentAsync(model.DoctorId, model.DepartmentId);
                if (existingAssignment != null)
                {
                    _logger.Warning("پزشک {DoctorId} قبلاً به دپارتمان {DepartmentId} انتصاب شده است", model.DoctorId, model.DepartmentId);
                    return ServiceResult.Failed("این پزشک قبلاً به این دپارتمان انتصاب شده است.");
                }

                // تبدیل ViewModel به Entity
                var doctorDepartment = model.ToEntity();
                
                // تنظیم اطلاعات ردیابی
                var currentUserId = _currentUserService.UserId;
                doctorDepartment.CreatedByUserId = currentUserId;
                doctorDepartment.UpdatedByUserId = currentUserId;
                doctorDepartment.CreatedAt = DateTime.Now;
                doctorDepartment.UpdatedAt = DateTime.Now;
                doctorDepartment.IsDeleted = false;

                // تنظیم تاریخ شروع (در صورت عدم تعیین)
                if (!doctorDepartment.StartDate.HasValue)
                {
                    doctorDepartment.StartDate = DateTime.Now;
                }

                // ذخیره در دیتابیس
                await _doctorDepartmentRepository.AddDoctorDepartmentAsync(doctorDepartment);

                _logger.Information("پزشک {DoctorId} با موفقیت به دپارتمان {DepartmentId} انتصاب شد", model.DoctorId, model.DepartmentId);

                return ServiceResult.Successful("پزشک با موفقیت به دپارتمان انتصاب شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتصاب پزشک {DoctorId} به دپارتمان {DepartmentId}", model.DoctorId, model.DepartmentId);
                return ServiceResult.Failed("خطا در انتصاب پزشک به دپارتمان");
            }
        }

        /// <summary>
        /// لغو انتصاب یک پزشک از یک دپارتمان
        /// </summary>
        public async Task<ServiceResult> RevokeDoctorFromDepartmentAsync(int doctorId, int departmentId)
        {
            try
            {
                _logger.Information("درخواست لغو انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId}", doctorId, departmentId);

                if (doctorId <= 0 || departmentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک یا دپارتمان نامعتبر است.");
                }

                // بررسی وجود انتصاب
                var existingAssignment = await _doctorDepartmentRepository.GetDoctorDepartmentAsync(doctorId, departmentId);
                if (existingAssignment == null)
                {
                    _logger.Warning("انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} یافت نشد", doctorId, departmentId);
                    return ServiceResult.Failed("انتساب مورد نظر یافت نشد.");
                }

                // بررسی وجود نوبت‌های فعال (این بررسی در repository انجام می‌شود)
                // لغو انتصاب (حذف)
                await _doctorDepartmentRepository.DeleteDoctorDepartmentAsync(existingAssignment);

                _logger.Information("انتساب پزشک {DoctorId} از دپارتمان {DepartmentId} با موفقیت لغو شد", doctorId, departmentId);

                return ServiceResult.Successful("انتساب پزشک با موفقیت لغو شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتصاب پزشک {DoctorId} از دپارتمان {DepartmentId}", doctorId, departmentId);
                return ServiceResult.Failed("خطا در لغو انتصاب پزشک");
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات انتصاب پزشک به دپارتمان (نقش، وضعیت فعال/غیرفعال و ...)
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorDepartmentAssignmentAsync(DoctorDepartmentViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی انتصاب پزشک {DoctorId} در دپارتمان {DepartmentId}", model.DoctorId, model.DepartmentId);

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل به‌روزرسانی انتصاب پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود انتصاب
                var existingAssignment = await _doctorDepartmentRepository.GetDoctorDepartmentAsync(model.DoctorId, model.DepartmentId);
                if (existingAssignment == null)
                {
                    _logger.Warning("انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} یافت نشد", model.DoctorId, model.DepartmentId);
                    return ServiceResult.Failed("انتساب مورد نظر یافت نشد.");
                }

                // به‌روزرسانی فیلدها
                existingAssignment.Role = model.Role;
                existingAssignment.IsActive = model.IsActive;
                existingAssignment.StartDate = model.StartDate;
                existingAssignment.EndDate = model.EndDate;
                existingAssignment.UpdatedAt = DateTime.Now;
                existingAssignment.UpdatedByUserId = _currentUserService.UserId;

                // ذخیره تغییرات
                await _doctorDepartmentRepository.UpdateDoctorDepartmentAsync(existingAssignment);

                _logger.Information("انتساب پزشک {DoctorId} در دپارتمان {DepartmentId} با موفقیت به‌روزرسانی شد", model.DoctorId, model.DepartmentId);

                return ServiceResult.Successful("اطلاعات انتساب پزشک با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتصاب پزشک {DoctorId} در دپارتمان {DepartmentId}", model.DoctorId, model.DepartmentId);
                return ServiceResult.Failed("خطا در به‌روزرسانی انتصاب پزشک");
            }
        }

        #endregion
    }
}
