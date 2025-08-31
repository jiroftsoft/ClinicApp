using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Collections.Generic; // Added missing import for List
using ClinicApp.Models;
using ClinicApp.Models.Entities; // Added missing import for ApplicationDbContext

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت انتسابات پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتسابات
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 7. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 8. عملیات ترکیبی و انتقال پزشکان بین دپارتمان‌ها
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorAssignmentService : IDoctorAssignmentService
    {
        private readonly IDoctorAssignmentRepository _doctorAssignmentRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        private readonly IDoctorAssignmentHistoryService _historyService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorAssignmentsViewModel> _validator;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public DoctorAssignmentService(
            IDoctorAssignmentRepository doctorAssignmentRepository,
            IDoctorCrudRepository doctorRepository,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorAssignmentHistoryService historyService,
            ICurrentUserService currentUserService,
            IValidator<DoctorAssignmentsViewModel> validator,
            ApplicationDbContext context)
        {
            _doctorAssignmentRepository = doctorAssignmentRepository ?? throw new ArgumentNullException(nameof(doctorAssignmentRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<DoctorAssignmentService>();
        }

        #region Assignment Management (مدیریت انتسابات)

        /// <summary>
        /// به‌روزرسانی کامل انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی)
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorAssignmentsAsync(int doctorId, DoctorAssignmentsViewModel assignments)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی انتسابات پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                if (assignments == null)
                {
                    return ServiceResult.Failed("مدل انتسابات نمی‌تواند خالی باشد.");
                }

                // تنظیم شناسه پزشک در مدل
                assignments.DoctorId = doctorId;

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(assignments);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل انتسابات پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // اعتبارسنجی سطح بالا
                var validationCheck = await ValidateAssignmentCompatibilityAsync(assignments);
                if (!validationCheck.Success)
                {
                    return validationCheck;
                }

                // به‌روزرسانی انتسابات دپارتمان‌ها
                await UpdateDepartmentAssignmentsAsync(doctorId, assignments.DoctorDepartments);

                // به‌روزرسانی انتسابات سرفصل‌های خدماتی
                await UpdateServiceCategoryAssignmentsAsync(doctorId, assignments.DoctorServiceCategories);

                // ثبت تاریخچه عملیات
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    "UpdateAssignments",
                    "به‌روزرسانی انتسابات پزشک",
                    "انتسابات پزشک به‌روزرسانی شد",
                    importance: AssignmentHistoryImportance.Important);

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت به‌روزرسانی شد", doctorId);

                return ServiceResult.Successful("انتسابات پزشک با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در به‌روزرسانی انتسابات پزشک");
            }
        }

        /// <summary>
        /// دریافت تمام انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی)
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت انتسابات پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorAssignmentsViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorAssignmentsViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت انتسابات دپارتمان‌ها
                var departmentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
                var doctorDepartments = departmentAssignmentsResult.Success ? departmentAssignmentsResult.Data.Items : new List<DoctorDepartmentViewModel>();

                // دریافت انتسابات سرفصل‌های خدماتی
                var serviceCategoryAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                var doctorServiceCategories = serviceCategoryAssignmentsResult.Success ? serviceCategoryAssignmentsResult.Data.Items : new List<DoctorServiceCategoryViewModel>();

                // ایجاد مدل انتسابات
                var assignments = new DoctorAssignmentsViewModel
                {
                    DoctorId = doctorId,
                    DoctorDepartments = doctorDepartments,
                    DoctorServiceCategories = doctorServiceCategories
                };

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت دریافت شد. تعداد دپارتمان‌ها: {DepartmentCount}, تعداد سرفصل‌ها: {ServiceCategoryCount}", 
                    doctorId, doctorDepartments.Count, doctorServiceCategories.Count);

                return ServiceResult<DoctorAssignmentsViewModel>.Successful(assignments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorAssignmentsViewModel>.Failed("خطا در دریافت انتسابات پزشک");
            }
        }

        /// <summary>
        /// انتساب همزمان پزشک به دپارتمان و سرفصل‌های خدماتی مرتبط
        /// </summary>
        public async Task<ServiceResult> AssignDoctorToDepartmentWithServicesAsync(int doctorId, int departmentId, List<int> serviceCategoryIds)
        {
            try
            {
                _logger.Information("درخواست انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} با سرفصل‌های خدماتی", doctorId, departmentId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0 || departmentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک یا دپارتمان نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // انتساب به دپارتمان
                var departmentAssignment = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    IsActive = true
                };

                var departmentResult = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(departmentAssignment);
                if (!departmentResult.Success)
                {
                    return departmentResult;
                }

                // ثبت تاریخچه انتساب به دپارتمان
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    "AssignToDepartment",
                    "انتساب پزشک به دپارتمان",
                    $"پزشک به دپارتمان {departmentId} انتساب داده شد",
                    departmentId,
                    importance: AssignmentHistoryImportance.Important);

                // انتساب سرفصل‌های خدماتی
                if (serviceCategoryIds != null && serviceCategoryIds.Any())
                {
                    foreach (var serviceCategoryId in serviceCategoryIds)
                    {
                        var serviceAssignment = new DoctorServiceCategoryViewModel
                        {
                            DoctorId = doctorId,
                            ServiceCategoryId = serviceCategoryId,
                            IsActive = true
                        };

                        var serviceResult = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(serviceAssignment);
                        if (!serviceResult.Success)
                        {
                            _logger.Warning("خطا در انتساب سرفصل خدماتی {ServiceCategoryId} به پزشک {DoctorId}", serviceCategoryId, doctorId);
                        }
                    }

                    // ثبت تاریخچه انتساب سرفصل‌های خدماتی
                    var serviceCategoriesJson = string.Join(",", serviceCategoryIds);
                    await _historyService.LogAssignmentOperationAsync(
                        doctorId,
                        "AssignServiceCategories",
                        "انتساب سرفصل‌های خدماتی",
                        $"سرفصل‌های خدماتی {serviceCategoriesJson} به پزشک انتساب داده شد",
                        departmentId,
                        serviceCategoriesJson,
                        importance: AssignmentHistoryImportance.Important);
                }

                _logger.Information("پزشک {DoctorId} با موفقیت به دپارتمان {DepartmentId} انتساب داده شد", doctorId, departmentId);
                return ServiceResult.Successful("پزشک با موفقیت به دپارتمان انتساب داده شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                return ServiceResult.Failed("خطا در انتساب پزشک به دپارتمان");
            }
        }

        /// <summary>
        /// انتقال پزشک بین دپارتمان‌ها با حفظ صلاحیت‌های خدماتی
        /// </summary>
        public async Task<ServiceResult> TransferDoctorBetweenDepartmentsAsync(int doctorId, int fromDepartmentId, int toDepartmentId, bool preserveServiceCategories = true)
        {
            try
            {
                _logger.Information("درخواست انتقال پزشک {DoctorId} از دپارتمان {FromDepartmentId} به دپارتمان {ToDepartmentId}", 
                    doctorId, fromDepartmentId, toDepartmentId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0 || fromDepartmentId <= 0 || toDepartmentId <= 0)
                {
                    return ServiceResult.Failed("شناسه‌های وارد شده نامعتبر هستند.");
                }

                if (fromDepartmentId == toDepartmentId)
                {
                    return ServiceResult.Failed("دپارتمان مبدا و مقصد نمی‌توانند یکسان باشند.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت سرفصل‌های خدماتی فعلی (در صورت نیاز به حفظ)
                List<int> currentServiceCategories = new List<int>();
                if (preserveServiceCategories)
                {
                    var currentAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                    if (currentAssignmentsResult.Success)
                    {
                        currentServiceCategories = currentAssignmentsResult.Data.Items
                            .Where(sc => sc.IsActive)
                            .Select(sc => sc.ServiceCategoryId)
                            .ToList();
                    }
                }

                // حذف انتساب از دپارتمان فعلی
                var removeResult = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, fromDepartmentId);
                if (!removeResult.Success)
                {
                    _logger.Warning("خطا در حذف انتساب پزشک {DoctorId} از دپارتمان {FromDepartmentId}", doctorId, fromDepartmentId);
                }

                // ثبت تاریخچه حذف از دپارتمان
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    "RemoveFromDepartment",
                    "حذف از دپارتمان",
                    $"پزشک از دپارتمان {fromDepartmentId} حذف شد",
                    fromDepartmentId,
                    importance: AssignmentHistoryImportance.Important);

                // انتساب به دپارتمان جدید
                var newDepartmentAssignment = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = toDepartmentId,
                    IsActive = true
                };

                var assignResult = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(newDepartmentAssignment);
                if (!assignResult.Success)
                {
                    return assignResult;
                }

                // ثبت تاریخچه انتساب به دپارتمان جدید
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    "TransferToDepartment",
                    "انتقال به دپارتمان جدید",
                    $"پزشک به دپارتمان {toDepartmentId} منتقل شد",
                    toDepartmentId,
                    importance: AssignmentHistoryImportance.Critical);

                // بازانتساب سرفصل‌های خدماتی (در صورت نیاز)
                if (preserveServiceCategories && currentServiceCategories.Any())
                {
                    foreach (var serviceCategoryId in currentServiceCategories)
                    {
                        var serviceAssignment = new DoctorServiceCategoryViewModel
                        {
                            DoctorId = doctorId,
                            ServiceCategoryId = serviceCategoryId,
                            IsActive = true
                        };

                        await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(serviceAssignment);
                    }
                }

                _logger.Information("پزشک {DoctorId} با موفقیت از دپارتمان {FromDepartmentId} به دپارتمان {ToDepartmentId} منتقل شد", 
                    doctorId, fromDepartmentId, toDepartmentId);

                return ServiceResult.Successful("پزشک با موفقیت بین دپارتمان‌ها منتقل شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتقال پزشک {DoctorId} بین دپارتمان‌ها", doctorId);
                return ServiceResult.Failed("خطا در انتقال پزشک بین دپارتمان‌ها");
            }
        }

        /// <summary>
        /// حذف کامل تمام انتسابات یک پزشک
        /// </summary>
        public async Task<ServiceResult> RemoveAllDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست حذف کامل انتسابات پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وابستگی‌ها
                var dependencies = await _doctorAssignmentRepository.GetDoctorDependenciesAsync(doctorId);
                if (!dependencies.CanBeDeleted)
                {
                    _logger.Warning("پزشک {DoctorId} دارای وابستگی‌های فعال است و قابل حذف نیست: {ErrorMessage}", doctorId, dependencies.DeletionErrorMessage);
                    return ServiceResult.Failed(dependencies.DeletionErrorMessage);
                }

                // حذف انتسابات دپارتمان‌ها
                var departmentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
                if (departmentAssignmentsResult.Success)
                {
                    foreach (var assignment in departmentAssignmentsResult.Data.Items)
                    {
                        await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, assignment.DepartmentId);
                    }
                }

                // حذف انتسابات سرفصل‌های خدماتی
                var serviceCategoryAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                if (serviceCategoryAssignmentsResult.Success)
                {
                    foreach (var assignment in serviceCategoryAssignmentsResult.Data.Items)
                    {
                        await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, assignment.ServiceCategoryId);
                    }
                }

                // ثبت تاریخچه حذف کامل انتسابات
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    "RemoveAllAssignments",
                    "حذف کامل انتسابات",
                    "تمام انتسابات پزشک حذف شد",
                    importance: AssignmentHistoryImportance.Security);

                _logger.Information("تمام انتسابات پزشک {DoctorId} با موفقیت حذف شد", doctorId);
                return ServiceResult.Successful("تمام انتسابات پزشک با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در حذف انتسابات پزشک");
            }
        }

        #endregion

        #region Assignment History (تاریخچه انتسابات)

        /// <summary>
        /// دریافت تاریخچه انتسابات یک پزشک
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentHistory>>> GetDoctorAssignmentHistoryAsync(int doctorId, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست دریافت تاریخچه انتسابات پزشک {DoctorId}. Page: {Page}, PageSize: {PageSize}", 
                    doctorId, page, pageSize);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت تاریخچه از سرویس تاریخچه
                var historyResult = await _historyService.GetDoctorHistoryAsync(doctorId, page, pageSize);
                if (!historyResult.Success)
                {
                    _logger.Warning("خطا در دریافت تاریخچه پزشک {DoctorId}: {Message}", doctorId, historyResult.Message);
                    return ServiceResult<List<DoctorAssignmentHistory>>.Failed(historyResult.Message);
                }

                _logger.Information("تاریخچه انتسابات پزشک {DoctorId} با موفقیت دریافت شد. تعداد رکوردها: {Count}", 
                    doctorId, historyResult.Data?.Count ?? 0);

                return ServiceResult<List<DoctorAssignmentHistory>>.Successful(historyResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<List<DoctorAssignmentHistory>>.Failed("خطا در دریافت تاریخچه انتسابات");
            }
        }

        /// <summary>
        /// دریافت آمار تاریخچه انتسابات
        /// </summary>
        public async Task<ServiceResult<DashboardHistoryStats>> GetAssignmentHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار تاریخچه انتسابات. StartDate: {StartDate}, EndDate: {EndDate}", 
                    startDate, endDate);

                // دریافت آمار از سرویس تاریخچه
                var statsResult = await _historyService.GetHistoryStatsAsync(startDate, endDate);
                if (!statsResult.Success)
                {
                    _logger.Warning("خطا در دریافت آمار تاریخچه: {Message}", statsResult.Message);
                    return ServiceResult<DashboardHistoryStats>.Failed(statsResult.Message);
                }

                // تبدیل به DashboardHistoryStats
                var dashboardStats = new DashboardHistoryStats
                {
                    TotalRecords = statsResult.Data.TotalRecords,
                    CriticalRecords = statsResult.Data.CriticalRecords,
                    ImportantRecords = statsResult.Data.ImportantRecords,
                    ActionTypeCounts = statsResult.Data.ActionTypeCounts,
                    DepartmentCounts = statsResult.Data.DepartmentCounts
                };

                _logger.Information("آمار تاریخچه انتسابات با موفقیت دریافت شد. TotalRecords: {TotalRecords}", 
                    dashboardStats.TotalRecords);

                return ServiceResult<DashboardHistoryStats>.Successful(dashboardStats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار تاریخچه انتسابات");
                return ServiceResult<DashboardHistoryStats>.Failed("خطا در دریافت آمار تاریخچه");
            }
        }

        #endregion

        #region Statistics and Reporting (آمار و گزارش‌گیری)

        /// <summary>
        /// دریافت آمار کلی انتسابات پزشکان
        /// </summary>
        public async Task<ServiceResult<AssignmentStatsViewModel>> GetAssignmentStatisticsAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت آمار کلی انتسابات پزشکان");

                // محاسبه آمار از دیتابیس
                var totalDoctors = await _doctorRepository.GetAllDoctorsCountAsync();
                var activeDoctors = await _doctorRepository.GetActiveDoctorsCountAsync();
                
                // محاسبه آمار انتسابات
                var totalDepartmentAssignments = await _context.DoctorDepartments.CountAsync(dd => !dd.IsDeleted);
                var activeDepartmentAssignments = await _context.DoctorDepartments.CountAsync(dd => dd.IsActive && !dd.IsDeleted);
                var inactiveDepartmentAssignments = totalDepartmentAssignments - activeDepartmentAssignments;

                var totalServiceCategoryAssignments = await _context.DoctorServiceCategories.CountAsync(dsc => !dsc.IsDeleted);
                var activeServiceCategoryAssignments = await _context.DoctorServiceCategories.CountAsync(dsc => dsc.IsActive && !dsc.IsDeleted);

                // محاسبه آمار دپارتمان‌ها و سرفصل‌های خدماتی
                var activeDepartments = await _context.Departments.CountAsync(d => d.IsActive && !d.IsDeleted);
                var serviceCategories = await _context.ServiceCategories.CountAsync(sc => sc.IsActive && !sc.IsDeleted);

                // محاسبه درصد تکمیل
                var completionPercentage = totalDoctors > 0 ? (decimal)activeDoctors / totalDoctors * 100 : 0;

                var stats = new AssignmentStatsViewModel
                {
                    TotalAssignments = totalDepartmentAssignments + totalServiceCategoryAssignments,
                    ActiveAssignments = activeDepartmentAssignments + activeServiceCategoryAssignments,
                    InactiveAssignments = inactiveDepartmentAssignments + (totalServiceCategoryAssignments - activeServiceCategoryAssignments),
                    AssignedDoctors = activeDoctors,
                    ActiveDepartments = activeDepartments,
                    ServiceCategories = serviceCategories,
                    CompletionPercentage = completionPercentage,
                    LastUpdate = DateTime.Now
                };

                _logger.Information("آمار کلی انتسابات پزشکان با موفقیت محاسبه شد. TotalAssignments: {TotalAssignments}, ActiveAssignments: {ActiveAssignments}", 
                    stats.TotalAssignments, stats.ActiveAssignments);

                return ServiceResult<AssignmentStatsViewModel>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه آمار کلی انتسابات پزشکان");
                return ServiceResult<AssignmentStatsViewModel>.Failed("خطا در محاسبه آمار");
            }
        }

        /// <summary>
        /// دریافت اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
        /// </summary>
        public async Task<ServiceResult<DoctorDependencyInfo>> GetDoctorDependenciesAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست بررسی وابستگی‌های پزشک {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorDependencyInfo>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorDependencyInfo>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت اطلاعات وابستگی‌ها
                var dependencies = await _doctorAssignmentRepository.GetDoctorDependenciesAsync(doctorId);

                _logger.Information("وابستگی‌های پزشک {DoctorId} با موفقیت بررسی شد. CanBeDeleted: {CanBeDeleted}, AppointmentCount: {AppointmentCount}", 
                    doctorId, dependencies.CanBeDeleted, dependencies.AppointmentCount);

                return ServiceResult<DoctorDependencyInfo>.Successful(dependencies);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وابستگی‌های پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorDependencyInfo>.Failed("خطا در بررسی وابستگی‌های پزشک");
            }
        }

        /// <summary>
        /// دریافت تعداد انتسابات فعال یک پزشک
        /// </summary>
        public async Task<ServiceResult<int>> GetActiveAssignmentsCountAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت تعداد انتسابات فعال پزشک {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<int>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<int>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت تعداد انتسابات فعال
                var count = await _doctorAssignmentRepository.GetActiveAssignmentsCountAsync(doctorId);

                _logger.Information("تعداد انتسابات فعال پزشک {DoctorId}: {Count}", doctorId, count);

                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد انتسابات فعال پزشک {DoctorId}", doctorId);
                return ServiceResult<int>.Failed("خطا در دریافت تعداد انتسابات فعال");
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// اعتبارسنجی سازگاری انتسابات
        /// </summary>
        private async Task<ServiceResult> ValidateAssignmentCompatibilityAsync(DoctorAssignmentsViewModel assignments)
        {
            try
            {
                // بررسی تداخل دپارتمان‌ها
                var departmentIds = assignments.DoctorDepartments?.Select(d => d.DepartmentId).Distinct().ToList() ?? new List<int>();
                if (departmentIds.Count != (assignments.DoctorDepartments?.Count ?? 0))
                {
                    return ServiceResult.Failed("انتساب تکراری به دپارتمان‌ها مجاز نیست.");
                }

                // بررسی تداخل سرفصل‌های خدماتی
                var serviceCategoryIds = assignments.DoctorServiceCategories?.Select(s => s.ServiceCategoryId).Distinct().ToList() ?? new List<int>();
                if (serviceCategoryIds.Count != (assignments.DoctorServiceCategories?.Count ?? 0))
                {
                    return ServiceResult.Failed("انتساب تکراری به سرفصل‌های خدماتی مجاز نیست.");
                }

                // بررسی سازگاری سرفصل‌های خدماتی با دپارتمان‌ها
                // این بخش می‌تواند بر اساس قوانین کسب‌وکار توسعه یابد

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی سازگاری انتسابات");
                return ServiceResult.Failed("خطا در اعتبارسنجی انتسابات");
            }
        }

        /// <summary>
        /// به‌روزرسانی انتسابات دپارتمان‌ها
        /// </summary>
        private async Task UpdateDepartmentAssignmentsAsync(int doctorId, List<DoctorDepartmentViewModel> newAssignments)
        {
            if (newAssignments == null) return;

            // دریافت انتسابات فعلی
            var currentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
            var currentAssignments = currentAssignmentsResult.Success ? currentAssignmentsResult.Data.Items : new List<DoctorDepartmentViewModel>();

            // شناسایی انتسابات جدید
            var newDepartmentIds = newAssignments.Select(a => a.DepartmentId).ToList();
            var currentDepartmentIds = currentAssignments.Select(a => a.DepartmentId).ToList();

            // حذف انتسابات غیرضروری
            var assignmentsToRemove = currentAssignments.Where(a => !newDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToRemove)
            {
                await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, assignment.DepartmentId);
            }

            // افزودن انتسابات جدید
            var assignmentsToAdd = newAssignments.Where(a => !currentDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToAdd)
            {
                await _doctorDepartmentService.AssignDoctorToDepartmentAsync(assignment);
            }

            // به‌روزرسانی انتسابات موجود
            var assignmentsToUpdate = newAssignments.Where(a => currentDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToUpdate)
            {
                await _doctorDepartmentService.UpdateDoctorDepartmentAssignmentAsync(assignment);
            }
        }

        /// <summary>
        /// به‌روزرسانی انتسابات سرفصل‌های خدماتی
        /// </summary>
        private async Task UpdateServiceCategoryAssignmentsAsync(int doctorId, List<DoctorServiceCategoryViewModel> newAssignments)
        {
            if (newAssignments == null) return;

            // دریافت انتسابات فعلی
            var currentAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
            var currentAssignments = currentAssignmentsResult.Success ? currentAssignmentsResult.Data.Items : new List<DoctorServiceCategoryViewModel>();

            // شناسایی انتسابات جدید
            var newServiceCategoryIds = newAssignments.Select(a => a.ServiceCategoryId).ToList();
            var currentServiceCategoryIds = currentAssignments.Select(a => a.ServiceCategoryId).ToList();

            // حذف انتسابات غیرضروری
            var assignmentsToRemove = currentAssignments.Where(a => !newServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToRemove)
            {
                await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, assignment.ServiceCategoryId);
            }

            // افزودن انتسابات جدید
            var assignmentsToAdd = newAssignments.Where(a => !currentServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToAdd)
            {
                await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(assignment);
            }

            // به‌روزرسانی انتسابات موجود
            var assignmentsToUpdate = newAssignments.Where(a => currentServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToUpdate)
            {
                await _doctorServiceCategoryService.UpdateDoctorServiceCategoryAsync(assignment);
            }
        }

        #endregion

        #region DataTables Support

        /// <summary>
        /// دریافت لیست انتسابات برای DataTables با pagination و filtering
        /// </summary>
        public async Task<ServiceResult<DataTablesResponse>> GetAssignmentsForDataTablesAsync(DataTablesRequest request)
        {
            try
            {
                _logger.Information("درخواست دریافت لیست انتسابات برای DataTables. Draw: {Draw}, Start: {Start}, Length: {Length}", 
                    request.Draw, request.Start, request.Length);

                // دریافت تمام پزشکان با eager loading برای Entity Framework 6
                var allDoctors = await _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .ToListAsync();

                // Eager load nested properties for Entity Framework 6
                foreach (var doctor in allDoctors)
                {
                    if (doctor.DoctorDepartments != null)
                    {
                        foreach (var deptAssignment in doctor.DoctorDepartments)
                        {
                            if (deptAssignment.DepartmentId > 0)
                            {
                                deptAssignment.Department = await _context.Departments
                                    .FirstOrDefaultAsync(d => d.DepartmentId == deptAssignment.DepartmentId);
                            }
                        }
                    }

                    if (doctor.DoctorServiceCategories != null)
                    {
                        foreach (var serviceAssignment in doctor.DoctorServiceCategories)
                        {
                            if (serviceAssignment.ServiceCategoryId > 0)
                            {
                                serviceAssignment.ServiceCategory = await _context.ServiceCategories
                                    .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == serviceAssignment.ServiceCategoryId);
                            }
                        }
                    }
                }

                // تبدیل به ViewModel
                var allAssignments = allDoctors.Select(d => new DoctorAssignmentListItem
                {
                    Id = d.DoctorId,
                    DoctorId = d.DoctorId,
                    DoctorName = d.FirstName + " " + d.LastName,
                    DoctorNationalCode = d.NationalCode ?? "",
                    DoctorSpecialization = d.Education ?? "",
                    Status = d.IsActive ? "active" : "inactive",
                    AssignmentDate = d.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                    LastModifiedDate = d.UpdatedAt?.ToString("yyyy/MM/dd HH:mm") ?? "-",
                    ModifiedBy = d.UpdatedByUserId ?? "",
                    IsActive = d.IsActive,
                    Departments = d.DoctorDepartments
                        .Where(dd => !dd.IsDeleted)
                        .Select(dd => new DepartmentAssignment
                        {
                            Id = dd.DepartmentId,
                            Name = dd.Department?.Name ?? "",
                            Code = dd.Department?.DepartmentId.ToString() ?? "",
                            Role = dd.Role ?? "",
                            StartDate = dd.StartDate?.ToString("yyyy/MM/dd") ?? "",
                            IsActive = dd.IsActive
                        }).ToList(),
                    ServiceCategories = d.DoctorServiceCategories
                        .Where(dsc => !dsc.IsDeleted)
                        .Select(dsc => new ServiceCategoryAssignment
                        {
                            Id = dsc.ServiceCategoryId,
                            Name = dsc.ServiceCategory?.Title ?? "",
                            Code = dsc.ServiceCategory?.ServiceCategoryId.ToString() ?? "",
                            AuthorizationLevel = dsc.AuthorizationLevel ?? "",
                            GrantedDate = dsc.GrantedDate?.ToString("yyyy/MM/dd") ?? "",
                            CertificateNumber = dsc.CertificateNumber ?? "",
                            IsActive = dsc.IsActive
                        }).ToList()
                }).ToList();

                // اعمال فیلترها
                var filteredAssignments = ApplyDataTablesFilters(allAssignments, request);

                // اعمال sorting
                var sortedAssignments = ApplyDataTablesSorting(filteredAssignments, request);

                // اعمال pagination
                var pagedAssignments = sortedAssignments
                    .Skip(request.Start)
                    .Take(request.Length)
                    .ToList();

                var response = new DataTablesResponse
                {
                    Draw = request.Draw,
                    RecordsTotal = allAssignments.Count,
                    RecordsFiltered = filteredAssignments.Count,
                    Assignments = pagedAssignments
                };

                _logger.Information("لیست انتسابات برای DataTables با موفقیت بازگردانده شد. Total: {Total}, Filtered: {Filtered}, Returned: {Returned}", 
                    response.RecordsTotal, response.RecordsFiltered, response.Assignments.Count);

                return ServiceResult<DataTablesResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست انتسابات برای DataTables");
                return ServiceResult<DataTablesResponse>.Failed("خطا در دریافت لیست انتسابات");
            }
        }

        /// <summary>
        /// اعمال فیلترهای DataTables
        /// </summary>
        private List<DoctorAssignmentListItem> ApplyDataTablesFilters(List<DoctorAssignmentListItem> assignments, DataTablesRequest request)
        {
            var filtered = assignments.AsEnumerable();

            // Global search
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                var searchValue = request.Search.Value.ToLower();
                filtered = filtered.Where(a => 
                    a.DoctorName.ToLower().Contains(searchValue) ||
                    a.DoctorNationalCode.Contains(searchValue) ||
                    a.DoctorSpecialization?.ToLower().Contains(searchValue) == true
                );
            }

            // Column-specific search
            foreach (var column in request.Columns.Where(c => !string.IsNullOrEmpty(c.Search?.Value)))
            {
                var searchValue = column.Search.Value.ToLower();
                switch (column.Data?.ToLower())
                {
                    case "doctorname":
                        filtered = filtered.Where(a => a.DoctorName.ToLower().Contains(searchValue));
                        break;
                    case "doctornationalcode":
                        filtered = filtered.Where(a => a.DoctorNationalCode.Contains(searchValue));
                        break;
                    case "status":
                        filtered = filtered.Where(a => a.Status.ToLower().Contains(searchValue));
                        break;
                }
            }

            return filtered.ToList();
        }

        /// <summary>
        /// اعمال sorting DataTables
        /// </summary>
        private List<DoctorAssignmentListItem> ApplyDataTablesSorting(List<DoctorAssignmentListItem> assignments, DataTablesRequest request)
        {
            var sorted = assignments.AsEnumerable();

            foreach (var order in request.Order)
            {
                var column = request.Columns.ElementAtOrDefault(order.Column);
                if (column == null) continue;

                var isAscending = order.Dir?.ToLower() == "asc";
                switch (column.Data?.ToLower())
                {
                    case "doctorname":
                        sorted = isAscending ? sorted.OrderBy(a => a.DoctorName) : sorted.OrderByDescending(a => a.DoctorName);
                        break;
                    case "doctornationalcode":
                        sorted = isAscending ? sorted.OrderBy(a => a.DoctorNationalCode) : sorted.OrderByDescending(a => a.DoctorNationalCode);
                        break;
                    case "status":
                        sorted = isAscending ? sorted.OrderBy(a => a.Status) : sorted.OrderByDescending(a => a.Status);
                        break;
                    case "assignmentdate":
                        sorted = isAscending ? sorted.OrderBy(a => a.AssignmentDate) : sorted.OrderByDescending(a => a.AssignmentDate);
                        break;
                    default:
                        sorted = isAscending ? sorted.OrderBy(a => a.DoctorName) : sorted.OrderByDescending(a => a.DoctorName);
                        break;
                }
            }

            return sorted.ToList();
        }

        #endregion
    }
}
