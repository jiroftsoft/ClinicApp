using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Extensions;
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
    /// سرویس تخصصی برای مدیریت صلاحیت‌های خدماتی پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت رابطه چند-به-چند پزشک-دسته‌بندی خدمات
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت صلاحیت‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 7. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorServiceCategoryService : IDoctorServiceCategoryService
    {
        private readonly IDoctorServiceCategoryRepository _doctorServiceCategoryRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorServiceCategoryViewModel> _validator;
        private readonly ILogger _logger;

        public DoctorServiceCategoryService(
            IDoctorServiceCategoryRepository doctorServiceCategoryRepository,
            IDoctorCrudRepository doctorRepository,
            IServiceCategoryRepository serviceCategoryRepository,
            ICurrentUserService currentUserService,
            IValidator<DoctorServiceCategoryViewModel> validator
            )
        {
            _doctorServiceCategoryRepository = doctorServiceCategoryRepository ?? throw new ArgumentNullException(nameof(doctorServiceCategoryRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _serviceCategoryRepository = serviceCategoryRepository ?? throw new ArgumentNullException(nameof(serviceCategoryRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = Log.ForContext<DoctorServiceCategoryService>();
        }

        #region Doctor-ServiceCategory Management (مدیریت انتصاب پزشک به سرفصل‌های خدماتی)

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات مجاز برای یک پزشک
        /// </summary>
        public async Task<ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>> GetServiceCategoriesForDoctorAsync(int doctorId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("درخواست دریافت دسته‌بندی‌های خدمات پزشک با شناسه: {DoctorId}, صفحه: {PageNumber}", doctorId, pageNumber);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت دسته‌بندی‌های خدمات پزشک
                var doctorServiceCategories = await _doctorServiceCategoryRepository.GetDoctorServiceCategoriesAsync(doctorId, searchTerm, pageNumber, pageSize);
                var totalCount = await _doctorServiceCategoryRepository.GetDoctorServiceCategoriesCountAsync(doctorId, searchTerm);

                // تبدیل به ViewModel
                var doctorServiceCategoryViewModels = doctorServiceCategories.Select(DoctorServiceCategoryViewModel.FromEntity).ToList();

                // ایجاد نتیجه صفحه‌بندی شده
                var pagedResult = new PagedResult<DoctorServiceCategoryViewModel>(
                    doctorServiceCategoryViewModels,
                    totalCount,
                    pageNumber,
                    pageSize
                ).WithMedicalInfo(containsSensitiveData: true, SecurityLevel.High);

                _logger.Information("دسته‌بندی‌های خدمات پزشک با شناسه {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", doctorId, doctorServiceCategoryViewModels.Count);

                return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های خدمات پزشک {DoctorId}", doctorId);
                return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Failed("خطا در دریافت دسته‌بندی‌های خدمات پزشک");
            }
        }

        /// <summary>
        /// اعطا کردن صلاحیت ارائه یک دسته‌بندی خدمات به یک پزشک
        /// </summary>
        public async Task<ServiceResult> GrantServiceCategoryToDoctorAsync(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", model.ServiceCategoryId, model.DoctorId);

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل اعطای صلاحیت دسته‌بندی خدمات ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(model.DoctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", model.DoctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وجود دسته‌بندی خدمات
                var serviceCategory = await _serviceCategoryRepository.GetByIdAsync(model.ServiceCategoryId);
                if (serviceCategory == null)
                {
                    _logger.Warning("دسته‌بندی خدمات با شناسه {ServiceCategoryId} یافت نشد", model.ServiceCategoryId);
                    return ServiceResult.Failed("دسته‌بندی خدمات مورد نظر یافت نشد.");
                }

                // بررسی وجود صلاحیت قبلی
                var existingPermission = await _doctorServiceCategoryRepository.GetDoctorServiceCategoryAsync(model.DoctorId, model.ServiceCategoryId);
                if (existingPermission != null)
                {
                    _logger.Warning("پزشک {DoctorId} قبلاً صلاحیت دسته‌بندی خدمات {ServiceCategoryId} را دارد", model.DoctorId, model.ServiceCategoryId);
                    return ServiceResult.Failed("این پزشک قبلاً صلاحیت این دسته‌بندی خدمات را دارد.");
                }

                // تبدیل ViewModel به Entity
                var doctorServiceCategory = model.ToEntity();
                
                // تنظیم اطلاعات ردیابی
                var currentUserId = _currentUserService.UserId;
                doctorServiceCategory.CreatedByUserId = currentUserId;
                doctorServiceCategory.UpdatedByUserId = currentUserId;
                doctorServiceCategory.CreatedAt = DateTime.Now;
                doctorServiceCategory.UpdatedAt = DateTime.Now;

                // تنظیم تاریخ اعطا (در صورت عدم تعیین)
                if (!doctorServiceCategory.GrantedDate.HasValue)
                {
                    doctorServiceCategory.GrantedDate = DateTime.Now;
                }

                // ذخیره در دیتابیس
                await _doctorServiceCategoryRepository.AddDoctorServiceCategoryAsync(doctorServiceCategory);

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} با موفقیت به پزشک {DoctorId} اعطا شد", model.ServiceCategoryId, model.DoctorId);

                return ServiceResult.Successful("صلاحیت دسته‌بندی خدمات با موفقیت اعطا شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", model.ServiceCategoryId, model.DoctorId);
                return ServiceResult.Failed("خطا در اعطای صلاحیت دسته‌بندی خدمات");
            }
        }

        /// <summary>
        /// لغو صلاحیت ارائه یک دسته‌بندی خدمات از یک پزشک
        /// </summary>
        public async Task<ServiceResult> RevokeServiceCategoryFromDoctorAsync(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);

                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک یا دسته‌بندی خدمات نامعتبر است.");
                }

                // بررسی وجود صلاحیت
                var existingPermission = await _doctorServiceCategoryRepository.GetDoctorServiceCategoryAsync(doctorId, serviceCategoryId);
                if (existingPermission == null)
                {
                    _logger.Warning("صلاحیت پزشک {DoctorId} برای دسته‌بندی خدمات {ServiceCategoryId} یافت نشد", doctorId, serviceCategoryId);
                    return ServiceResult.Failed("صلاحیت مورد نظر یافت نشد.");
                }

                // بررسی وجود نوبت‌های فعال (این بررسی در repository انجام می‌شود)
                // لغو صلاحیت (حذف)
                await _doctorServiceCategoryRepository.DeleteDoctorServiceCategoryAsync(existingPermission);

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId} با موفقیت لغو شد", serviceCategoryId, doctorId);

                return ServiceResult.Successful("صلاحیت دسته‌بندی خدمات با موفقیت لغو شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);
                return ServiceResult.Failed("خطا در لغو صلاحیت دسته‌بندی خدمات");
            }
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات صلاحیت ارائه دسته‌بندی خدمات توسط پزشک
        /// طبق AI_COMPLIANCE_CONTRACT: اعتبارسنجی کامل و مدیریت خطا
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorServiceCategoryAsync(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی صلاحیت خدماتی {AssignmentId} توسط کاربر {UserId}", 
                    model.AssignmentId, _currentUserService.UserId);

                // اعتبارسنجی اولیه
                if (model == null)
                {
                    _logger.Warning("مدل null دریافت شد. کاربر: {UserId}", _currentUserService.UserId);
                    return ServiceResult.Failed("اطلاعات ارسالی نامعتبر است.");
                }

                if (model.DoctorId <= 0 || model.ServiceCategoryId <= 0)
                {
                    _logger.Warning("شناسه‌های نامعتبر. DoctorId: {DoctorId}, ServiceCategoryId: {ServiceCategoryId}, کاربر: {UserId}", 
                        model.DoctorId, model.ServiceCategoryId, _currentUserService.UserId);
                    return ServiceResult.Failed("شناسه پزشک یا دسته‌بندی خدمات نامعتبر است.");
                }

                // اعتبارسنجی فیلدهای اجباری
                if (string.IsNullOrWhiteSpace(model.AuthorizationLevel))
                {
                    _logger.Warning("سطح صلاحیت خالی است. کاربر: {UserId}", _currentUserService.UserId);
                    return ServiceResult.Failed("سطح صلاحیت الزامی است.");
                }

                if (!model.GrantedDate.HasValue)
                {
                    _logger.Warning("تاریخ اعطا خالی است. کاربر: {UserId}", _currentUserService.UserId);
                    return ServiceResult.Failed("تاریخ اعطا الزامی است.");
                }

                // اعتبارسنجی تاریخ‌ها
                if (model.ExpiryDate.HasValue && model.ExpiryDate.Value <= model.GrantedDate.Value)
                {
                    _logger.Warning("تاریخ انقضا باید بعد از تاریخ اعطا باشد. کاربر: {UserId}", _currentUserService.UserId);
                    return ServiceResult.Failed("تاریخ انقضا باید بعد از تاریخ اعطا باشد.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(model.DoctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد. کاربر: {UserId}", model.DoctorId, _currentUserService.UserId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وجود دسته‌بندی خدمات
                var serviceCategory = await _serviceCategoryRepository.GetByIdAsync(model.ServiceCategoryId);
                if (serviceCategory == null)
                {
                    _logger.Warning("دسته‌بندی خدمات با شناسه {ServiceCategoryId} یافت نشد. کاربر: {UserId}", 
                        model.ServiceCategoryId, _currentUserService.UserId);
                    return ServiceResult.Failed("دسته‌بندی خدمات مورد نظر یافت نشد.");
                }

                // بررسی وجود صلاحیت
                var existingPermission = await _doctorServiceCategoryRepository.GetDoctorServiceCategoryAsync(model.DoctorId, model.ServiceCategoryId);
                if (existingPermission == null)
                {
                    _logger.Warning("صلاحیت خدماتی {AssignmentId} یافت نشد. کاربر: {UserId}", 
                        model.AssignmentId, _currentUserService.UserId);
                    return ServiceResult.Failed("صلاحیت مورد نظر یافت نشد.");
                }

                // بررسی تغییرات
                var hasChanges = false;
                var changes = new List<string>();

                if (existingPermission.AuthorizationLevel != model.AuthorizationLevel)
                {
                    existingPermission.AuthorizationLevel = model.AuthorizationLevel;
                    changes.Add($"سطح صلاحیت: {model.AuthorizationLevel}");
                    hasChanges = true;
                }

                if (existingPermission.CertificateNumber != model.CertificateNumber)
                {
                    existingPermission.CertificateNumber = model.CertificateNumber;
                    changes.Add($"شماره گواهی: {model.CertificateNumber}");
                    hasChanges = true;
                }

                if (existingPermission.IsActive != model.IsActive)
                {
                    existingPermission.IsActive = model.IsActive;
                    changes.Add($"وضعیت فعال: {(model.IsActive ? "فعال" : "غیرفعال")}");
                    hasChanges = true;
                }

                if (existingPermission.GrantedDate != model.GrantedDate)
                {
                    existingPermission.GrantedDate = model.GrantedDate;
                    changes.Add($"تاریخ اعطا: {model.GrantedDate?.ToString("yyyy/MM/dd")}");
                    hasChanges = true;
                }

                if (existingPermission.ExpiryDate != model.ExpiryDate)
                {
                    existingPermission.ExpiryDate = model.ExpiryDate;
                    changes.Add($"تاریخ انقضا: {model.ExpiryDate?.ToString("yyyy/MM/dd")}");
                    hasChanges = true;
                }

                if (existingPermission.Notes != model.Notes)
                {
                    existingPermission.Notes = model.Notes;
                    changes.Add($"توضیحات: {model.Notes}");
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    _logger.Information("هیچ تغییری در صلاحیت خدماتی {AssignmentId} اعمال نشد. کاربر: {UserId}", 
                        model.AssignmentId, _currentUserService.UserId);
                    return ServiceResult.Successful("هیچ تغییری اعمال نشد.");
                }

                // به‌روزرسانی اطلاعات
                existingPermission.UpdatedAt = DateTime.UtcNow;
                existingPermission.UpdatedByUserId = _currentUserService.UserId;

                await _doctorServiceCategoryRepository.UpdateDoctorServiceCategoryAsync(existingPermission);
                await _doctorServiceCategoryRepository.SaveChangesAsync();

                _logger.Information("صلاحیت خدماتی {AssignmentId} با موفقیت به‌روزرسانی شد. تغییرات: {Changes}, کاربر: {UserId}", 
                    model.AssignmentId, string.Join(", ", changes), _currentUserService.UserId);

                return ServiceResult.Successful("صلاحیت خدماتی با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی صلاحیت خدماتی {AssignmentId}. کاربر: {UserId}", 
                    model?.AssignmentId, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در به‌روزرسانی صلاحیت خدماتی");
            }
        }

        /// <summary>
        /// دریافت لیست تمام دسته‌بندی‌های خدمات فعال برای استفاده در لیست‌های کشویی
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetAllServiceCategoriesAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تمام دسته‌بندی‌های خدمات فعال");

                var serviceCategories = await _serviceCategoryRepository.GetAllActiveServiceCategoriesAsync();

                var lookupItems = serviceCategories.Select(sc => new LookupItemViewModel
                {
                    Id = sc.ServiceCategoryId,
                    Name = sc.Title,
                    Description = $"{sc.Department.Name} - {sc.Description}"
                }).ToList();

                _logger.Information("لیست تمام دسته‌بندی‌های خدمات فعال با موفقیت دریافت شد. تعداد: {Count}", lookupItems.Count);

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تمام دسته‌بندی‌های خدمات فعال");
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در دریافت لیست دسته‌بندی‌های خدمات");
            }
        }

        /// <summary>
        /// دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی (برای فیلتر "همه پزشکان")
        /// </summary>
        public async Task<ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>> GetAllDoctorServiceCategoriesAsync(string searchTerm, int? doctorId, int? serviceCategoryId, bool? isActive, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("درخواست دریافت همه انتصابات پزشکان به سرفصل‌های خدماتی. صفحه: {PageNumber}, اندازه: {PageSize}", pageNumber, pageSize);

                if (pageNumber <= 0) pageNumber = 1;
                if (pageSize <= 0) pageSize = 10;

                // دریافت داده‌ها از Repository
                var doctorServiceCategories = await _doctorServiceCategoryRepository.GetAllDoctorServiceCategoriesAsync(searchTerm, doctorId, serviceCategoryId, isActive, pageNumber, pageSize);
                var totalCount = await _doctorServiceCategoryRepository.GetAllDoctorServiceCategoriesCountAsync(searchTerm, doctorId, serviceCategoryId, isActive);

                // تبدیل به ViewModel
                var viewModels = doctorServiceCategories.Select(dsc => new DoctorServiceCategoryViewModel
                {
                    DoctorId = dsc.DoctorId,
                    DoctorName = $"{dsc.Doctor?.FirstName} {dsc.Doctor?.LastName}".Trim(),
                    ServiceCategoryId = dsc.ServiceCategoryId,
                    ServiceCategoryName = dsc.ServiceCategory?.Title,
                    DepartmentName = dsc.ServiceCategory?.Department?.Name,
                    AuthorizationLevel = dsc.AuthorizationLevel,
                    CertificateNumber = dsc.CertificateNumber,
                    IsActive = dsc.IsActive,
                    GrantedDate = dsc.GrantedDate,
                    GrantedDateShamsi = dsc.GrantedDate?.ToPersianDateTime(),
                    ExpiryDate = dsc.ExpiryDate,
                    ExpiryDateShamsi = dsc.ExpiryDate?.ToPersianDateTime(),
                    Notes = dsc.Notes,
                    CreatedAt = dsc.CreatedAt,
                    CreatedAtShamsi = dsc.CreatedAt.ToPersianDateTime(),
                    CreatedBy = $"{dsc.CreatedByUser?.FirstName} {dsc.CreatedByUser?.LastName}".Trim()
                }).ToList();

                var pagedResult = new PagedResult<DoctorServiceCategoryViewModel>(viewModels, totalCount, pageNumber, pageSize);

                _logger.Information("لیست همه انتصابات پزشکان به سرفصل‌های خدماتی با موفقیت دریافت شد. تعداد: {Count}", viewModels.Count);

                return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست همه انتصابات پزشکان به سرفصل‌های خدماتی");
                return ServiceResult<PagedResult<DoctorServiceCategoryViewModel>>.Failed("خطا در دریافت لیست انتصابات");
            }
        }

        #endregion

        #region Department Management (مدیریت دپارتمان‌ها)

        /// <summary>
        /// دریافت دپارتمان‌های مرتبط با پزشک
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetDoctorDepartmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت دپارتمان‌های پزشک {DoctorId}", doctorId);

                var departments = await _doctorServiceCategoryRepository.GetDoctorDepartmentsAsync(doctorId);

                var lookupItems = departments.Select(d => new LookupItemViewModel
                {
                    Id = d.DepartmentId,
                    Name = d.Name
                }).ToList();

                _logger.Information("دپارتمان‌های پزشک {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", doctorId, lookupItems.Count);

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دپارتمان‌های پزشک {DoctorId}", doctorId);
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در دریافت دپارتمان‌های پزشک");
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدماتی مرتبط با دپارتمان
        /// </summary>
        public async Task<ServiceResult<List<LookupItemViewModel>>> GetServiceCategoriesByDepartmentAsync(int departmentId)
        {
            try
            {
                _logger.Information("درخواست دریافت سرفصل‌های خدماتی دپارتمان {DepartmentId}", departmentId);

                var serviceCategories = await _doctorServiceCategoryRepository.GetServiceCategoriesByDepartmentAsync(departmentId);

                var lookupItems = serviceCategories.Select(sc => new LookupItemViewModel
                {
                    Id = sc.ServiceCategoryId,
                    Name = sc.Title
                }).ToList();

                _logger.Information("سرفصل‌های خدماتی دپارتمان {DepartmentId} با موفقیت دریافت شد. تعداد: {Count}", departmentId, lookupItems.Count);

                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت سرفصل‌های خدماتی دپارتمان {DepartmentId}", departmentId);
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطا در دریافت سرفصل‌های خدماتی دپارتمان");
            }
        }

        /// <summary>
        /// دریافت لیست سرفصل‌های خدماتی به صورت SelectListItem برای استفاده در View
        /// </summary>
        public async Task<ServiceResult<List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>>> GetServiceCategoriesAsSelectListAsync(bool addAllOption = false)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست سرفصل‌های خدماتی به صورت SelectList");

                var serviceCategoriesResult = await GetAllServiceCategoriesAsync();
                if (!serviceCategoriesResult.Success)
                {
                    return ServiceResult<List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>>.Failed(serviceCategoriesResult.Message);
                }

                var selectList = serviceCategoriesResult.Data.Select(sc => new ClinicApp.ViewModels.DoctorManagementVM.SelectListItem
                {
                    Text = sc.Name,
                    Value = sc.Id.ToString(),
                    Selected = false
                }).ToList();

                if (addAllOption)
                {
                    selectList.Insert(0, new ClinicApp.ViewModels.DoctorManagementVM.SelectListItem
                    {
                        Text = "همه سرفصل‌های خدماتی",
                        Value = "",
                        Selected = true
                    });
                }

                _logger.Information("لیست سرفصل‌های خدماتی به صورت SelectList با موفقیت دریافت شد. تعداد: {Count}", selectList.Count);

                return ServiceResult<List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>>.Successful(selectList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست سرفصل‌های خدماتی به صورت SelectList");
                return ServiceResult<List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>>.Failed("خطا در دریافت لیست سرفصل‌های خدماتی");
            }
        }

        #endregion

    }
}
