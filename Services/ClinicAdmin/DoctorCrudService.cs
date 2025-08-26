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
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس اصلی برای عملیات CRUD پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل عملیات اصلی ایجاد، خواندن، به‌روزرسانی و حذف
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 7. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorCrudService : IDoctorCrudService
    {
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IDoctorReportingRepository _doctorReportingRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorCreateEditViewModel> _validator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorCrudService(
            IDoctorCrudRepository doctorRepository,
            IDoctorReportingRepository doctorReportingRepository,
            ICurrentUserService currentUserService,
            IValidator<DoctorCreateEditViewModel> validator,
            IMapper mapper)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _doctorReportingRepository = doctorReportingRepository ?? throw new ArgumentNullException(nameof(doctorReportingRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorCrudService>();
        }

        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت لیست پزشکان با قابلیت صفحه‌بندی و جستجو
        /// </summary>
        public async Task<ServiceResult<PagedResult<DoctorIndexViewModel>>> GetDoctorsAsync(DoctorSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست پزشکان با فیلتر: {@Filter}", filter);

                // اعتبارسنجی فیلتر
                if (filter == null)
                {
                    filter = new DoctorSearchViewModel();
                }

                // تنظیم مقادیر پیش‌فرض
                if (filter.PageNumber <= 0) filter.PageNumber = 1;
                if (filter.PageSize <= 0) filter.PageSize = 10;

                // دریافت پزشکان از repository
                var doctors = await _doctorRepository.SearchDoctorsAsync(filter);
                var totalCount = await _doctorRepository.GetDoctorsCountAsync(filter);

                // تبدیل به ViewModel
                var doctorViewModels = doctors.Select(DoctorIndexViewModel.FromEntity).ToList();

                // ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<DoctorIndexViewModel>(
                    doctorViewModels, 
                    totalCount, 
                    filter.PageNumber, 
                    filter.PageSize
                ).WithMedicalInfo(containsSensitiveData: true, SecurityLevel.High);

                _logger.Information("لیست پزشکان با موفقیت دریافت شد. تعداد: {Count}", doctorViewModels.Count);

                return ServiceResult<PagedResult<DoctorIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
                return ServiceResult<PagedResult<DoctorIndexViewModel>>.Failed("خطا در دریافت لیست پزشکان");
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل یک پزشک برای نمایش
        /// </summary>
        public async Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت جزئیات پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorDetailsViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // دریافت پزشک با جزئیات کامل
                var doctor = await _doctorRepository.GetByIdWithDetailsAsync(doctorId);

                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorDetailsViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // تبدیل به ViewModel
                var doctorDetailsViewModel = DoctorDetailsViewModel.FromEntity(doctor);

                _logger.Information("جزئیات پزشک با شناسه {DoctorId} با موفقیت دریافت شد", doctorId);

                return ServiceResult<DoctorDetailsViewModel>.Successful(doctorDetailsViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorDetailsViewModel>.Failed("خطا در دریافت جزئیات پزشک");
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک پزشک برای پر کردن فرم ویرایش
        /// </summary>
        public async Task<ServiceResult<DoctorCreateEditViewModel>> GetDoctorForEditAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت اطلاعات پزشک برای ویرایش با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorCreateEditViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // دریافت پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);

                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای ویرایش یافت نشد", doctorId);
                    return ServiceResult<DoctorCreateEditViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // تبدیل به ViewModel
                var doctorEditViewModel = DoctorCreateEditViewModel.FromEntity(doctor);

                _logger.Information("اطلاعات پزشک با شناسه {DoctorId} برای ویرایش با موفقیت دریافت شد", doctorId);

                return ServiceResult<DoctorCreateEditViewModel>.Successful(doctorEditViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک برای ویرایش {DoctorId}", doctorId);
                return ServiceResult<DoctorCreateEditViewModel>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        /// <summary>
        /// ایجاد یک پزشک جدید بر اساس اطلاعات ورودی
        /// </summary>
        public async Task<ServiceResult<Doctor>> CreateDoctorAsync(DoctorCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد پزشک جدید: {@Model}", model);

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل ایجاد پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult<Doctor>.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی تکراری نبودن پزشک (بر اساس نام و نام خانوادگی)
                // این بررسی در repository انجام می‌شود

                // تبدیل ViewModel به Entity
                var doctor = model.ToEntity();
                
                // تنظیم اطلاعات ردیابی
                var currentUserId = _currentUserService.UserId;
                doctor.CreatedByUserId = currentUserId;
                doctor.UpdatedByUserId = currentUserId;
                doctor.CreatedAt = DateTime.Now;
                doctor.UpdatedAt = DateTime.Now;
                doctor.IsDeleted = false;

                // ذخیره در دیتابیس
                var createdDoctor = await _doctorRepository.AddAsync(doctor);

                _logger.Information("پزشک جدید با شناسه {DoctorId} با موفقیت ایجاد شد", createdDoctor.DoctorId);

                return ServiceResult<Doctor>.Successful(createdDoctor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پزشک جدید");
                return ServiceResult<Doctor>.Failed("خطا در ایجاد پزشک جدید");
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات یک پزشک موجود
        /// </summary>
        public async Task<ServiceResult<Doctor>> UpdateDoctorAsync(DoctorCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی پزشک با شناسه: {DoctorId}", model.DoctorId);

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل به‌روزرسانی پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult<Doctor>.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var existingDoctor = await _doctorRepository.GetByIdAsync(model.DoctorId);
                if (existingDoctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای به‌روزرسانی یافت نشد", model.DoctorId);
                    return ServiceResult<Doctor>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی تکراری نبودن پزشک (به جز خودش)
                // این بررسی در repository انجام می‌شود

                // به‌روزرسانی فیلدها
                existingDoctor.FirstName = model.FirstName;
                existingDoctor.LastName = model.LastName;
                existingDoctor.Specialization = model.Specialization;
                existingDoctor.PhoneNumber = model.PhoneNumber;
                existingDoctor.Bio = model.Bio;
                existingDoctor.IsActive = model.IsActive;
                existingDoctor.UpdatedAt = DateTime.Now;
                existingDoctor.UpdatedByUserId = _currentUserService.UserId;

                // ذخیره تغییرات
                var updatedDoctor = await _doctorRepository.UpdateAsync(existingDoctor);

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                return ServiceResult<Doctor>.Successful(updatedDoctor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پزشک {DoctorId}", model.DoctorId);
                return ServiceResult<Doctor>.Failed("خطا در به‌روزرسانی پزشک");
            }
        }

        /// <summary>
        /// حذف نرم یک پزشک با بررسی قوانین کسب‌وکار
        /// </summary>
        public async Task<ServiceResult> SoftDeleteDoctorAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست حذف نرم پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای حذف یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی امکان حذف
                var canDelete = await _doctorReportingRepository.CanDeleteDoctorAsync(doctorId);
                if (!canDelete)
                {
                    _logger.Warning("امکان حذف پزشک {DoctorId} به دلیل وجود وابستگی‌های فعال وجود ندارد", doctorId);
                    return ServiceResult.Failed("امکان حذف پزشک به دلیل وجود وابستگی‌های فعال وجود ندارد.");
                }

                // حذف نرم
                var currentUserId = _currentUserService.UserId;
                await _doctorRepository.SoftDeleteAsync(doctorId, currentUserId);

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت حذف شد", doctorId);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در حذف پزشک");
            }
        }

        /// <summary>
        /// بازیابی یک پزشک حذف شده
        /// </summary>
        public async Task<ServiceResult> RestoreDoctorAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست بازیابی پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای بازیابی یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                if (!doctor.IsDeleted)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} قبلاً حذف نشده است", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر قبلاً حذف نشده است.");
                }

                // بازیابی
                var currentUserId = _currentUserService.UserId;
                await _doctorRepository.RestoreAsync(doctorId, currentUserId);

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت بازیابی شد", doctorId);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در بازیابی پزشک");
            }
        }

        #endregion
    }
}
