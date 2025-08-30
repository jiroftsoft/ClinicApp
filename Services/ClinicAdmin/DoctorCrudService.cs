using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
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
        private readonly ISpecializationService _specializationService;
        private readonly IDoctorReportingRepository _doctorReportingRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorCreateEditViewModel> _validator;
        private readonly ILogger _logger;
        private readonly IClinicRepository _clinicRepository; // اضافه کردن repository کلینیک
        private readonly ApplicationDbContext _context; // اضافه کردن context برای دسترسی مستقیم به دیتابیس

        public DoctorCrudService(
            IDoctorCrudRepository doctorRepository,
            ISpecializationService specializationService,
            IDoctorReportingRepository doctorReportingRepository,
            ICurrentUserService currentUserService,
            IValidator<DoctorCreateEditViewModel> validator,
            IClinicRepository clinicRepository,
            ApplicationDbContext context) // اضافه کردن dependency
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _specializationService = specializationService ?? throw new ArgumentNullException(nameof(specializationService));
            _doctorReportingRepository = doctorReportingRepository ?? throw new ArgumentNullException(nameof(doctorReportingRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _clinicRepository = clinicRepository ?? throw new ArgumentNullException(nameof(clinicRepository)); // تخصیص dependency
            _context = context ?? throw new ArgumentNullException(nameof(context)); // تخصیص dependency
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
                var totalCount = await _doctorRepository.GetAllDoctorsCountAsync();

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

                // بررسی تکراری نبودن پزشک (بر اساس کد ملی و کد نظام پزشکی)
                var nationalCodeExists = await _doctorRepository.DoesNationalCodeExistAsync(model.NationalCode);
                if (nationalCodeExists)
                {
                    _logger.Warning("تلاش برای ایجاد پزشک تکراری با کد ملی: {NationalCode}", model.NationalCode);
                    return ServiceResult<Doctor>.Failed("پزشکی با این کد ملی قبلاً ثبت شده است.");
                }

                var medicalCouncilCodeExists = await _doctorRepository.DoesMedicalCouncilCodeExistAsync(model.MedicalCouncilCode);
                if (medicalCouncilCodeExists)
                {
                    _logger.Warning("تلاش برای ایجاد پزشک تکراری با کد نظام پزشکی: {MedicalCouncilCode}", model.MedicalCouncilCode);
                    return ServiceResult<Doctor>.Failed("پزشکی با این کد نظام پزشکی قبلاً ثبت شده است.");
                }

                // تبدیل ViewModel به Entity
                var doctor = model.ToEntity();
                
                // تنظیم اطلاعات ردیابی
                var currentUserId = await GetValidUserIdAsync();
                
                doctor.CreatedByUserId = currentUserId;
                doctor.UpdatedByUserId = currentUserId;
                doctor.CreatedAt = DateTime.Now;
                doctor.UpdatedAt = DateTime.Now;
                doctor.IsDeleted = false;

                // تنظیم کلینیک پیش‌فرض (شفا) اگر انتخاب نشده باشد
                if (!doctor.ClinicId.HasValue)
                {
                    // در اینجا باید شناسه کلینیک شفا را تنظیم کنیم
                    // برای حال حاضر، از مقدار 1 استفاده می‌کنیم (فرض بر این است که شفا اولین کلینیک است)
                    var shifaClinic = await _clinicRepository.GetByNameAsync("کلینیک شفا");
                    if (shifaClinic != null)
                    {
                        doctor.ClinicId = shifaClinic.ClinicId;
                        _logger.Information("پزشک جدید به کلینیک شفا (شناسه: {ClinicId}) تخصیص داده شد", doctor.ClinicId);
                    }
                    else
                    {
                        _logger.Warning("کلینیک شفا پیدا نشد. پزشک به کلینیک شفا تخصیص داده نشد.");
                    }
                }

                // ذخیره در دیتابیس
                var createdDoctor = await _doctorRepository.AddAsync(doctor);

                // به‌روزرسانی تخصص‌ها
                if (model.SelectedSpecializationIds != null && model.SelectedSpecializationIds.Any())
                {
                    var updateResult = await _specializationService.UpdateDoctorSpecializationsAsync(createdDoctor.DoctorId, model.SelectedSpecializationIds);
                    if (!updateResult.Success)
                    {
                        _logger.Warning("خطا در به‌روزرسانی تخصص‌های پزشک: {Error}", updateResult.Message);
                    }
                }

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
                var nationalCodeExists = await _doctorRepository.DoesNationalCodeExistAsync(model.NationalCode, model.DoctorId);
                if (nationalCodeExists)
                {
                    _logger.Warning("تلاش برای به‌روزرسانی پزشک تکراری با کد ملی: {NationalCode}", model.NationalCode);
                    return ServiceResult<Doctor>.Failed("پزشکی با این کد ملی قبلاً ثبت شده است.");
                }

                var medicalCouncilCodeExists = await _doctorRepository.DoesMedicalCouncilCodeExistAsync(model.MedicalCouncilCode, model.DoctorId);
                if (medicalCouncilCodeExists)
                {
                    _logger.Warning("تلاش برای به‌روزرسانی پزشک تکراری با کد نظام پزشکی: {MedicalCouncilCode}", model.MedicalCouncilCode);
                    return ServiceResult<Doctor>.Failed("پزشکی با این کد نظام پزشکی قبلاً ثبت شده است.");
                }

                // بررسی معتبر بودن کاربر فعلی
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.Error("شناسه کاربر خالی است. UserId: {UserId}", currentUserId);
                    
                    // Fallback: استفاده از شناسه Admin واقعی از دیتابیس
                    _logger.Warning("استفاده از شناسه Admin واقعی از دیتابیس به عنوان fallback");
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
                    if (adminUser != null)
                    {
                        currentUserId = adminUser.Id;
                        _logger.Information("شناسه کاربر Admin از دیتابیس: {UserId}", currentUserId);
                    }
                    else
                    {
                        _logger.Error("کاربر Admin در دیتابیس یافت نشد. استفاده از شناسه System");
                        currentUserId = "System";
                        _logger.Information("شناسه کاربر System: {UserId}", currentUserId);
                    }
                }
                
                // بررسی محیط توسعه
                if (_currentUserService.IsDevelopmentEnvironment())
                {
                    _logger.Information("محیط توسعه تشخیص داده شد. استفاده از کاربر Admin توسعه با شناسه: {UserId} برای ویرایش پزشک", currentUserId);
                }
                else
                {
                    _logger.Information("محیط تولید تشخیص داده شد. استفاده از کاربر احراز هویت شده با شناسه: {UserId} برای ویرایش پزشک", currentUserId);
                }
                
                // به‌روزرسانی فیلدها
                existingDoctor.FirstName = model.FirstName;
                existingDoctor.LastName = model.LastName;
                existingDoctor.Degree = model.Degree;
                existingDoctor.GraduationYear = model.GraduationYear;
                existingDoctor.University = model.University;
                existingDoctor.Gender = model.Gender;
                existingDoctor.DateOfBirth = model.DateOfBirth;
                existingDoctor.HomeAddress = model.HomeAddress;
                existingDoctor.OfficeAddress = model.OfficeAddress;
                existingDoctor.ExperienceYears = model.ExperienceYears;
                
                // به‌روزرسانی تصویر پروفایل فقط اگر مقدار جدید ارائه شده باشد
                if (!string.IsNullOrEmpty(model.ProfileImageUrl))
                {
                    existingDoctor.ProfileImageUrl = model.ProfileImageUrl;
                    _logger.Information("تصویر پروفایل پزشک {DoctorId} به‌روزرسانی شد", model.DoctorId);
                }
                else
                {
                    _logger.Information("تصویر پروفایل پزشک {DoctorId} تغییر نکرده، مقدار قبلی حفظ می‌شود", model.DoctorId);
                }
                
                existingDoctor.PhoneNumber = model.PhoneNumber;
                existingDoctor.NationalCode = model.NationalCode;
                existingDoctor.MedicalCouncilCode = model.MedicalCouncilCode;
                existingDoctor.Email = model.Email;
                existingDoctor.Bio = model.Bio;
                existingDoctor.IsActive = model.IsActive;
                existingDoctor.UpdatedAt = DateTime.Now;
                existingDoctor.UpdatedByUserId = currentUserId;

                // ذخیره تغییرات
                var updatedDoctor = await _doctorRepository.UpdateAsync(existingDoctor);

                // به‌روزرسانی تخصص‌ها
                if (model.SelectedSpecializationIds != null)
                {
                    var updateResult = await _specializationService.UpdateDoctorSpecializationsAsync(model.DoctorId, model.SelectedSpecializationIds);
                    if (!updateResult.Success)
                    {
                        _logger.Warning("خطا در به‌روزرسانی تخصص‌های پزشک: {Error}", updateResult.Message);
                    }
                }

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
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.Error("شناسه کاربر خالی است. UserId: {UserId}", currentUserId);
                    
                    // Fallback: استفاده از شناسه Admin واقعی از دیتابیس
                    _logger.Warning("استفاده از شناسه Admin واقعی از دیتابیس به عنوان fallback");
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
                    if (adminUser != null)
                    {
                        currentUserId = adminUser.Id;
                        _logger.Information("شناسه کاربر Admin از دیتابیس: {UserId}", currentUserId);
                    }
                    else
                    {
                        _logger.Error("کاربر Admin در دیتابیس یافت نشد. استفاده از شناسه System");
                        currentUserId = "System";
                        _logger.Information("شناسه کاربر System: {UserId}", currentUserId);
                    }
                }
                
                // بررسی محیط توسعه
                if (_currentUserService.IsDevelopmentEnvironment())
                {
                    _logger.Information("محیط توسعه تشخیص داده شد. استفاده از کاربر Admin توسعه با شناسه: {UserId} برای حذف پزشک", currentUserId);
                }
                else
                {
                    _logger.Information("محیط تولید تشخیص داده شد. استفاده از کاربر احراز هویت شده با شناسه: {UserId} برای حذف پزشک", currentUserId);
                }
                
                // حذف انتصابات فعال به دپارتمان‌ها
                var departmentAssignments = await _context.DoctorDepartments
                    .Where(dd => dd.DoctorId == doctorId && dd.IsActive && !dd.IsDeleted)
                    .ToListAsync();
                
                foreach (var assignment in departmentAssignments)
                {
                    assignment.IsActive = false;
                    assignment.IsDeleted = true;
                    assignment.DeletedAt = DateTime.Now;
                    assignment.DeletedByUserId = currentUserId;
                }
                
                // حذف انتصابات فعال به سرفصل‌های خدماتی
                var serviceCategoryAssignments = await _context.DoctorServiceCategories
                    .Where(dsc => dsc.DoctorId == doctorId && dsc.IsActive)
                    .ToListAsync();
                
                foreach (var assignment in serviceCategoryAssignments)
                {
                    assignment.IsActive = false;
                    assignment.IsDeleted = true;
                    assignment.DeletedAt = DateTime.Now;
                    assignment.DeletedByUserId = currentUserId;
                }
                
                // حذف نرم پزشک
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

                // بررسی معتبر بودن کاربر فعلی
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
                    currentUserId = adminUser?.Id ?? "System";
                }
                
                var restored = await _doctorRepository.RestoreAsync(doctorId, currentUserId);
                if (!restored)
                {
                    _logger.Warning("بازیابی پزشک با شناسه {DoctorId} ناموفق بود", doctorId);
                    return ServiceResult.Failed("بازیابی پزشک ناموفق بود");
                }

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت بازیابی شد", doctorId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در بازیابی پزشک");
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس کد ملی
        /// </summary>
        public async Task<ServiceResult<Doctor>> GetDoctorByNationalCodeAsync(string nationalCode)
        {
            try
            {
                if (string.IsNullOrEmpty(nationalCode))
                {
                    return ServiceResult<Doctor>.Failed("کد ملی نمی‌تواند خالی باشد");
                }

                var doctor = await _doctorRepository.GetByNationalCodeAsync(nationalCode);
                if (doctor == null)
                {
                    return ServiceResult<Doctor>.Successful(null);
                }

                return ServiceResult<Doctor>.Successful(doctor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پزشک با کد ملی: {NationalCode}", nationalCode);
                return ServiceResult<Doctor>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        /// <summary>
        /// دریافت پزشک بر اساس کد نظام پزشکی
        /// </summary>
        public async Task<ServiceResult<Doctor>> GetDoctorByMedicalCouncilCodeAsync(string medicalCouncilCode)
        {
            try
            {
                if (string.IsNullOrEmpty(medicalCouncilCode))
                {
                    return ServiceResult<Doctor>.Failed("کد نظام پزشکی نمی‌تواند خالی باشد");
                }

                var doctor = await _doctorRepository.GetByMedicalCouncilCodeAsync(medicalCouncilCode);
                if (doctor == null)
                {
                    return ServiceResult<Doctor>.Successful(null);
                }

                return ServiceResult<Doctor>.Successful(doctor);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پزشک با کد نظام پزشکی: {MedicalCouncilCode}", medicalCouncilCode);
                return ServiceResult<Doctor>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        /// <summary>
        /// فعال کردن یک پزشک
        /// </summary>
        public async Task<ServiceResult> ActivateDoctorAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست فعال‌سازی پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای فعال‌سازی یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                if (doctor.IsActive)
                {
                    _logger.Information("پزشک با شناسه {DoctorId} قبلاً فعال است", doctorId);
                    return ServiceResult.Successful();
                }

                // فعال‌سازی پزشک
                doctor.IsActive = true;
                doctor.UpdatedAt = DateTime.Now;
                
                // بررسی معتبر بودن کاربر فعلی
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
                    currentUserId = adminUser?.Id ?? "System";
                }
                doctor.UpdatedByUserId = currentUserId;

                await _doctorRepository.UpdateAsync(doctor);

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت فعال شد", doctorId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در فعال‌سازی پزشک");
            }
        }

        /// <summary>
        /// غیرفعال کردن یک پزشک
        /// </summary>
        public async Task<ServiceResult> DeactivateDoctorAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست غیرفعال‌سازی پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} برای غیرفعال‌سازی یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                if (!doctor.IsActive)
                {
                    _logger.Information("پزشک با شناسه {DoctorId} قبلاً غیرفعال است", doctorId);
                    return ServiceResult.Successful();
                }

                // بررسی وابستگی‌های فعال
                var canDeactivate = await _doctorReportingRepository.CanDeleteDoctorAsync(doctorId);
                if (!canDeactivate)
                {
                    _logger.Warning("امکان غیرفعال‌سازی پزشک {DoctorId} به دلیل وجود وابستگی‌های فعال وجود ندارد", doctorId);
                    return ServiceResult.Failed("امکان غیرفعال‌سازی پزشک به دلیل وجود وابستگی‌های فعال وجود ندارد.");
                }

                // غیرفعال‌سازی پزشک
                doctor.IsActive = false;
                doctor.UpdatedAt = DateTime.Now;
                
                // بررسی معتبر بودن کاربر فعلی
                var currentUserId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
                    currentUserId = adminUser?.Id ?? "System";
                }
                doctor.UpdatedByUserId = currentUserId;

                await _doctorRepository.UpdateAsync(doctor);

                _logger.Information("پزشک با شناسه {DoctorId} با موفقیت غیرفعال شد", doctorId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی پزشک");
            }
        }

        #endregion

        #region Specialization Operations

        /// <summary>
        /// دریافت لیست تخصص‌های فعال برای نمایش در فرم‌ها
        /// </summary>
        public async Task<ServiceResult<List<Specialization>>> GetActiveSpecializationsAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تخصص‌های فعال");

                var specializationsResult = await _specializationService.GetActiveSpecializationsAsync();
                if (!specializationsResult.Success)
                {
                    _logger.Error("خطا در دریافت تخصص‌های فعال: {Error}", specializationsResult.Message);
                    return ServiceResult<List<Specialization>>.Failed("خطا در دریافت لیست تخصص‌های فعال");
                }
                var specializations = specializationsResult.Data;
                
                _logger.Information("لیست تخصص‌های فعال با موفقیت دریافت شد. تعداد: {Count}", specializations.Count);

                return ServiceResult<List<Specialization>>.Successful(specializations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تخصص‌های فعال");
                return ServiceResult<List<Specialization>>.Failed("خطا در دریافت لیست تخصص‌های فعال");
            }
        }

        #endregion

        #region Helper Methods (روش‌های کمکی)

        /// <summary>
        /// دریافت شناسه کاربر معتبر برای عملیات‌های ردیابی
        /// </summary>
        private async Task<string> GetValidUserIdAsync()
        {
            var currentUserId = _currentUserService.UserId;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                return currentUserId;
            }

            // Fallback: استفاده از شناسه Admin واقعی از دیتابیس
            _logger.Warning("استفاده از شناسه Admin واقعی از دیتابیس به عنوان fallback");
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3020347998");
            if (adminUser != null)
            {
                _logger.Information("شناسه کاربر Admin از دیتابیس: {UserId}", adminUser.Id);
                return adminUser.Id;
            }

            // Fallback: بررسی کاربر System
            _logger.Error("کاربر Admin در دیتابیس یافت نشد. بررسی کاربر System");
            var systemUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "3031945451");
            if (systemUser != null)
            {
                _logger.Information("شناسه کاربر System از دیتابیس: {UserId}", systemUser.Id);
                return systemUser.Id;
            }

            // Fallback: ایجاد کاربر System
            _logger.Error("هیچ کاربر Admin یا System در دیتابیس یافت نشد. ایجاد کاربر System");
            var newSystemUser = new ApplicationUser
            {
                UserName = "3031945451",
                NationalCode = "3031945451",
                Email = "system@clinic.com",
                PhoneNumber = "09022487373",
                PhoneNumberConfirmed = true,
                FirstName = "System",
                LastName = "Shefa Clinic",
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedByUserId = "System" // خودش
            };
            
            _context.Users.Add(newSystemUser);
            await _context.SaveChangesAsync();
            
            _logger.Information("کاربر System ایجاد شد با شناسه: {UserId}", newSystemUser.Id);
            return newSystemUser.Id;
        }

        #endregion
    }
}
