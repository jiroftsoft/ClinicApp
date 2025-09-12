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
using System.Collections.Generic;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using DoctorDependencyInfo = ClinicApp.Models.DoctorDependencyInfo;

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
                var totalDoctors = await _context.Doctors.CountAsync(d => !d.IsDeleted);
                var activeDoctors = await _context.Doctors.CountAsync(d => d.IsActive && !d.IsDeleted);
                
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
                    TotalDoctors = totalDoctors,
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
        /// آماده‌سازی کامل ViewModel برای صفحه اصلی مدیریت انتسابات
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentIndexViewModel>> GetDoctorAssignmentIndexViewModelAsync()
        {
            try
            {
                _logger.Information("درخواست آماده‌سازی ViewModel صفحه اصلی مدیریت انتسابات");

                // اجرای همزمان تمام فراخوانی‌های دیتابیس برای بهبود عملکرد
                var statsTask = GetAssignmentStatisticsAsync();
                var departmentsTask = _doctorDepartmentService.GetDepartmentsAsSelectListAsync(true);
                var serviceCategoriesTask = _doctorServiceCategoryService.GetServiceCategoriesAsSelectListAsync(true);

                // انتظار برای تکمیل تمام عملیات
                await Task.WhenAll(statsTask, departmentsTask, serviceCategoriesTask);

                // پردازش نتایج
                var statsResult = await statsTask;
                var departmentsResult = await departmentsTask;
                var serviceCategoriesResult = await serviceCategoriesTask;

                // ایجاد ViewModel
                var viewModel = new DoctorAssignmentIndexViewModel
                {
                    PageTitle = "مدیریت انتسابات کلی پزشکان",
                    PageSubtitle = "مدیریت عملیات انتساب، انتقال و حذف انتسابات پزشکان",
                    IsDataLoaded = true,
                    IsLoading = false,
                    LoadingMessage = "",
                    LastRefreshTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                };

                // تنظیم آمار
                viewModel.Stats = statsResult.Success ? statsResult.Data : new AssignmentStatsViewModel
                {
                    TotalAssignments = 0,
                    ActiveAssignments = 0,
                    InactiveAssignments = 0,
                    AssignedDoctors = 0,
                    ActiveDepartments = 0,
                    ServiceCategories = 0,
                    CompletionPercentage = 0,
                    LastUpdate = DateTime.Now
                };

                // ایجاد فیلتر ViewModel
                viewModel.Filters = new AssignmentFilterViewModel
                {
                    Departments = departmentsResult.Success ? departmentsResult.Data : new List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>(),
                    ServiceCategories = serviceCategoriesResult.Success ? serviceCategoriesResult.Data : new List<ClinicApp.ViewModels.DoctorManagementVM.SelectListItem>()
                };

                _logger.Information("ViewModel صفحه اصلی مدیریت انتسابات با موفقیت آماده شد. TotalAssignments: {TotalAssignments}", 
                    viewModel.Stats.TotalAssignments);

                return ServiceResult<DoctorAssignmentIndexViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آماده‌سازی ViewModel صفحه اصلی مدیریت انتسابات");
                return ServiceResult<DoctorAssignmentIndexViewModel>.Failed("خطا در آماده‌سازی صفحه اصلی");
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

        /// <summary>
        /// به‌روزرسانی انتسابات پزشک از طریق EditViewModel
        /// این متد برای فرم ویرایش انتسابات طراحی شده است
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorAssignmentsFromEditAsync(DoctorAssignmentEditViewModel editModel)
        {
            try
            {
                _logger.Information("🔄 PRODUCTION LOG: شروع به‌روزرسانی انتسابات پزشک {DoctorId} از طریق EditViewModel", editModel.DoctorId);

                // اعتبارسنجی پارامترها
                if (editModel == null)
                {
                    _logger.Warning("❌ EditModel خالی است");
                    return ServiceResult.Failed("مدل ویرایش نمی‌تواند خالی باشد.");
                }

                if (editModel.DoctorId <= 0)
                {
                    _logger.Warning("❌ شناسه پزشک نامعتبر: {DoctorId}", editModel.DoctorId);
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(editModel.DoctorId);
                if (doctor == null)
                {
                    _logger.Warning("❌ پزشک با شناسه {DoctorId} یافت نشد", editModel.DoctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                _logger.Information("✅ پزشک یافت شد: {DoctorName} - {DoctorNationalCode}", doctor.FullName, doctor.NationalCode);

                // شروع تراکنش برای عملیات atomic
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. حذف انتسابات انتخاب شده
                        await RemoveSelectedAssignmentsAsync(editModel);

                        // 2. اضافه کردن دپارتمان‌های جدید
                        await AddNewDepartmentAssignmentsAsync(editModel);

                        // 3. اضافه کردن سرفصل‌های خدماتی جدید
                        await AddNewServiceCategoryAssignmentsAsync(editModel);

                        // 4. ثبت تاریخچه عملیات
                        await LogEditOperationHistoryAsync(editModel);

                        // Commit تراکنش
                        transaction.Commit();

                        _logger.Information("✅ PRODUCTION LOG: به‌روزرسانی انتسابات پزشک {DoctorId} با موفقیت انجام شد", editModel.DoctorId);
                        return ServiceResult.Successful("انتسابات پزشک با موفقیت به‌روزرسانی شد.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.Error(ex, "❌ خطا در تراکنش به‌روزرسانی انتسابات پزشک {DoctorId}", editModel.DoctorId);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در به‌روزرسانی انتسابات پزشک {DoctorId}", editModel?.DoctorId ?? 0);
                return ServiceResult.Failed("خطا در به‌روزرسانی انتسابات پزشک");
            }
        }

        /// <summary>
        /// حذف انتسابات انتخاب شده
        /// </summary>
        private async Task RemoveSelectedAssignmentsAsync(DoctorAssignmentEditViewModel editModel)
        {
            if (editModel.AssignmentsToRemove == null || !editModel.AssignmentsToRemove.Any())
            {
                _logger.Information("📝 هیچ انتسابی برای حذف انتخاب نشده است");
                return;
            }

            _logger.Information("🗑️ شروع حذف {Count} انتساب انتخاب شده", editModel.AssignmentsToRemove.Count);

            foreach (var assignmentId in editModel.AssignmentsToRemove)
            {
                // تشخیص نوع انتساب (دپارتمان یا سرفصل خدماتی)
                var departmentAssignment = editModel.DepartmentAssignments?.FirstOrDefault(d => d.Id == assignmentId);
                var serviceCategoryAssignment = editModel.ServiceCategoryAssignments?.FirstOrDefault(s => s.Id == assignmentId);

                if (departmentAssignment != null)
                {
                    _logger.Information("🗑️ حذف انتساب دپارتمان: {DepartmentName}", departmentAssignment.DepartmentName);
                    await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(editModel.DoctorId, departmentAssignment.DepartmentId);
                }
                else if (serviceCategoryAssignment != null)
                {
                    _logger.Information("🗑️ حذف انتساب سرفصل خدماتی: {ServiceCategoryName}", serviceCategoryAssignment.ServiceCategoryName);
                    await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(editModel.DoctorId, serviceCategoryAssignment.ServiceCategoryId);
                }
            }
        }

        /// <summary>
        /// اضافه کردن دپارتمان‌های جدید
        /// </summary>
        private async Task AddNewDepartmentAssignmentsAsync(DoctorAssignmentEditViewModel editModel)
        {
            if (editModel.NewDepartmentIds == null || !editModel.NewDepartmentIds.Any())
            {
                _logger.Information("📝 هیچ دپارتمان جدیدی برای اضافه کردن انتخاب نشده است");
                return;
            }

            _logger.Information("➕ شروع اضافه کردن {Count} دپارتمان جدید", editModel.NewDepartmentIds.Count);

            foreach (var departmentId in editModel.NewDepartmentIds)
            {
                var departmentAssignment = new DoctorDepartmentViewModel
                {
                    DoctorId = editModel.DoctorId,
                    DepartmentId = departmentId,
                    IsActive = true,
                    AssignmentDate = editModel.EffectiveDate ?? DateTime.Now.Date,
                    Role = "متخصص", // مقدار پیش‌فرض
                    Description = editModel.ChangeReason ?? "انتساب از طریق فرم ویرایش"
                };

                _logger.Information("➕ اضافه کردن دپارتمان با شناسه: {DepartmentId}", departmentId);
                await _doctorDepartmentService.AssignDoctorToDepartmentAsync(departmentAssignment);
            }
        }

        /// <summary>
        /// اضافه کردن سرفصل‌های خدماتی جدید
        /// </summary>
        private async Task AddNewServiceCategoryAssignmentsAsync(DoctorAssignmentEditViewModel editModel)
        {
            if (editModel.NewServiceCategoryIds == null || !editModel.NewServiceCategoryIds.Any())
            {
                _logger.Information("📝 هیچ سرفصل خدماتی جدیدی برای اضافه کردن انتخاب نشده است");
                return;
            }

            _logger.Information("➕ شروع اضافه کردن {Count} سرفصل خدماتی جدید", editModel.NewServiceCategoryIds.Count);

            foreach (var serviceCategoryId in editModel.NewServiceCategoryIds)
            {
                var serviceCategoryAssignment = new DoctorServiceCategoryViewModel
                {
                    DoctorId = editModel.DoctorId,
                    ServiceCategoryId = serviceCategoryId,
                    IsActive = true,
                    GrantedDate = editModel.EffectiveDate ?? DateTime.Now.Date,
                    AuthorizationLevel = "متوسط", // مقدار پیش‌فرض
                    CertificateNumber = $"CERT-{editModel.DoctorId}-{serviceCategoryId}-{DateTime.Now:yyyyMMdd}"
                };

                _logger.Information("➕ اضافه کردن سرفصل خدماتی با شناسه: {ServiceCategoryId}", serviceCategoryId);
                await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(serviceCategoryAssignment);
            }
        }

        /// <summary>
        /// ثبت تاریخچه عملیات ویرایش
        /// </summary>
        private async Task LogEditOperationHistoryAsync(DoctorAssignmentEditViewModel editModel)
        {
            var changes = new List<string>();

            if (editModel.AssignmentsToRemove?.Any() == true)
            {
                changes.Add($"حذف {editModel.AssignmentsToRemove.Count} انتساب");
            }

            if (editModel.NewDepartmentIds?.Any() == true)
            {
                changes.Add($"اضافه کردن {editModel.NewDepartmentIds.Count} دپارتمان");
            }

            if (editModel.NewServiceCategoryIds?.Any() == true)
            {
                changes.Add($"اضافه کردن {editModel.NewServiceCategoryIds.Count} سرفصل خدماتی");
            }

            var changeDescription = changes.Any() ? string.Join("، ", changes) : "بدون تغییر";
            var notes = !string.IsNullOrEmpty(editModel.EditNotes) ? $" یادداشت: {editModel.EditNotes}" : "";
            var reason = !string.IsNullOrEmpty(editModel.ChangeReason) ? $" دلیل: {editModel.ChangeReason}" : "";

            await _historyService.LogAssignmentOperationAsync(
                editModel.DoctorId,
                "EditAssignments",
                "ویرایش انتسابات پزشک",
                $"تغییرات: {changeDescription}{notes}{reason}",
                importance: AssignmentHistoryImportance.Important);
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

                _logger.Information("Found {Count} doctors in database", allDoctors.Count);
                
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
                
                _logger.Information("Converted to {Count} assignment items", allAssignments.Count);

                // اعمال فیلترها
                var filteredAssignments = ApplyDataTablesFilters(allAssignments, request);
                _logger.Information("After filtering: {Count} assignments", filteredAssignments.Count);

                // اعمال sorting
                var sortedAssignments = ApplyDataTablesSorting(filteredAssignments, request);
                _logger.Information("After sorting: {Count} assignments", sortedAssignments.Count);

                // اعمال pagination
                _logger.Information("Pagination parameters - Start: {Start}, Length: {Length}, Total: {Total}", 
                    request.Start, request.Length, sortedAssignments.Count);
                
                var pagedAssignments = sortedAssignments
                    .Skip(request.Start)
                    .Take(request.Length > 0 ? request.Length : 25) // Default to 25 if Length is 0
                    .ToList();
                
                _logger.Information("After pagination: {Count} assignments returned", pagedAssignments.Count);

                var response = new DataTablesResponse
                {
                    Draw = request.Draw,
                    RecordsTotal = allAssignments.Count,
                    RecordsFiltered = filteredAssignments.Count,
                    Data = pagedAssignments.Cast<object>().ToList()
                };
                
                _logger.Information("Final response - Draw: {Draw}, RecordsTotal: {RecordsTotal}, RecordsFiltered: {RecordsFiltered}, DataCount: {DataCount}", 
                    response.Draw, response.RecordsTotal, response.RecordsFiltered, response.Data.Count);

                _logger.Information("لیست انتسابات برای DataTables با موفقیت بازگردانده شد. Total: {Total}, Filtered: {Filtered}, Returned: {Returned}", 
                    response.RecordsTotal, response.RecordsFiltered, response.Data.Count);

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

        #region Helper Methods (متدهای کمکی)

        /// <summary>
        /// دریافت تمام انتسابات به صورت لیست
        /// </summary>
        private async Task<ServiceResult<List<DoctorAssignmentListItem>>> GetAllAssignmentsAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت تمام انتسابات");

                // دریافت تمام پزشکان با eager loading
                var allDoctors = await _context.Doctors
                    .Where(d => !d.IsDeleted)
                    .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
                    .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory))
                    .Include(d => d.DoctorSpecializations.Select(ds => ds.Specialization))
                    .ToListAsync();

                // تبدیل به DoctorAssignmentListItem
                var allAssignments = allDoctors.Select(d => new DoctorAssignmentListItem
                {
                    DoctorId = d.DoctorId,
                    DoctorName = $"{d.FirstName} {d.LastName}",
                    DoctorNationalCode = d.NationalCode,
                    DoctorSpecialization = d.DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "",
                    Status = "فعال", // می‌تواند از منطق کسب‌وکار تعیین شود
                    AssignmentDate = d.CreatedAt.ToString("yyyy/MM/dd"),
                    CreatedDate = d.CreatedAt,
                    Departments = d.DoctorDepartments
                        .Where(dd => !dd.IsDeleted)
                        .Select(dd => new DepartmentAssignment
                        {
                            Id = dd.DepartmentId,
                            Name = dd.Department?.Name ?? "",
                            IsActive = dd.IsActive
                        }).ToList(),
                    ServiceCategories = d.DoctorServiceCategories
                        .Where(dsc => !dsc.IsDeleted)
                        .Select(dsc => new ServiceCategoryAssignment
                        {
                            Id = dsc.ServiceCategoryId,
                            Name = dsc.ServiceCategory?.Title ?? "",
                            IsActive = dsc.IsActive
                        }).ToList()
                }).ToList();

                _logger.Information("تمام انتسابات با موفقیت دریافت شد. تعداد: {Count}", allAssignments.Count);

                return ServiceResult<List<DoctorAssignmentListItem>>.Successful(allAssignments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام انتسابات");
                return ServiceResult<List<DoctorAssignmentListItem>>.Failed("خطا در دریافت انتسابات");
            }
        }

        #endregion

        #region DataTables Methods (متدهای DataTables)

        /// <summary>
        /// دریافت انتسابات برای DataTables
        /// </summary>
        public async Task<ServiceResult<DataTablesResponse>> GetAssignmentsForDataTableAsync(
            int start, 
            int length, 
            string searchValue, 
            string departmentId, 
            string serviceCategoryId, 
            string dateFrom, 
            string dateTo)
        {
            try
            {
                _logger.Information("درخواست دریافت انتسابات برای DataTables. Start: {Start}, Length: {Length}", start, length);

                // دریافت تمام انتسابات
                var allAssignmentsResult = await GetAllAssignmentsAsync();
                if (!allAssignmentsResult.Success)
                {
                    return ServiceResult<DataTablesResponse>.Failed(allAssignmentsResult.Message);
                }

                var allAssignments = allAssignmentsResult.Data;

                // اعمال فیلترها
                var filteredAssignments = allAssignments.AsQueryable();

                // فیلتر جستجو
                if (!string.IsNullOrEmpty(searchValue))
                {
                    filteredAssignments = filteredAssignments.Where(a => 
                        a.DoctorName.Contains(searchValue) ||
                        a.DoctorNationalCode.Contains(searchValue) ||
                        a.DoctorSpecialization.Contains(searchValue) ||
                        a.Departments.Any(d => d.Name.Contains(searchValue)) ||
                        a.ServiceCategories.Any(s => s.Name.Contains(searchValue)));
                }

                // فیلتر دپارتمان
                if (!string.IsNullOrEmpty(departmentId) && int.TryParse(departmentId, out int deptId))
                {
                    filteredAssignments = filteredAssignments.Where(a => a.Departments.Any(d => d.Id == deptId));
                }

                // فیلتر سرفصل خدماتی
                if (!string.IsNullOrEmpty(serviceCategoryId) && int.TryParse(serviceCategoryId, out int serviceId))
                {
                    filteredAssignments = filteredAssignments.Where(a => a.ServiceCategories.Any(s => s.Id == serviceId));
                }

                // فیلتر تاریخ - تبدیل به List برای جلوگیری از Expression Tree
                var filteredList = filteredAssignments.ToList();
                
                if (!string.IsNullOrEmpty(dateFrom) && DateTime.TryParse(dateFrom, out DateTime fromDate))
                {
                    filteredList = filteredList.Where(a => 
                        !string.IsNullOrEmpty(a.AssignmentDate) && 
                        DateTime.TryParse(a.AssignmentDate, out DateTime assignmentDate) && 
                        assignmentDate >= fromDate).ToList();
                }

                if (!string.IsNullOrEmpty(dateTo) && DateTime.TryParse(dateTo, out DateTime toDate))
                {
                    filteredList = filteredList.Where(a => 
                        !string.IsNullOrEmpty(a.AssignmentDate) && 
                        DateTime.TryParse(a.AssignmentDate, out DateTime assignmentDate) && 
                        assignmentDate <= toDate).ToList();
                }

                // اعمال pagination
                var pagedAssignments = filteredList
                    .Skip(start)
                    .Take(length > 0 ? length : 25)
                    .ToList();

                var response = new DataTablesResponse
                {
                    Draw = 1, // این مقدار باید از درخواست دریافت شود
                    RecordsTotal = allAssignments.Count,
                    RecordsFiltered = filteredList.Count,
                    Data = pagedAssignments.Cast<object>().ToList()
                };

                _logger.Information("انتسابات برای DataTables با موفقیت بازگردانده شد. Total: {Total}, Filtered: {Filtered}", 
                    response.RecordsTotal, response.RecordsFiltered);

                return ServiceResult<DataTablesResponse>.Successful(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات برای DataTables");
                return ServiceResult<DataTablesResponse>.Failed("خطا در دریافت انتسابات");
            }
        }

        /// <summary>
        /// دریافت انتسابات فیلتر شده
        /// </summary>
        public async Task<ServiceResult<List<DoctorAssignmentListItem>>> GetFilteredAssignmentsAsync(AssignmentFilterViewModel filter)
        {
            try
            {
                _logger.Information("درخواست دریافت انتسابات فیلتر شده");

                // دریافت تمام انتسابات
                var allAssignmentsResult = await GetAllAssignmentsAsync();
                if (!allAssignmentsResult.Success)
                {
                    return ServiceResult<List<DoctorAssignmentListItem>>.Failed(allAssignmentsResult.Message);
                }

                var allAssignments = allAssignmentsResult.Data.AsQueryable();

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    allAssignments = allAssignments.Where(a => 
                        a.DoctorName.Contains(filter.SearchTerm) ||
                        a.DoctorNationalCode.Contains(filter.SearchTerm) ||
                        a.DoctorSpecialization.Contains(filter.SearchTerm));
                }

                if (filter.DepartmentId.HasValue)
                {
                    allAssignments = allAssignments.Where(a => a.Departments.Any(d => d.Id == filter.DepartmentId.Value));
                }

                if (filter.ServiceCategoryId.HasValue)
                {
                    allAssignments = allAssignments.Where(a => a.ServiceCategories.Any(s => s.Id == filter.ServiceCategoryId.Value));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    allAssignments = allAssignments.Where(a => a.Status == filter.Status);
                }

                // تبدیل به List برای جلوگیری از Expression Tree
                var allAssignmentsList = allAssignments.ToList();
                
                if (filter.DateFrom.HasValue)
                {
                    var fromDate = filter.DateFrom.Value;
                    allAssignmentsList = allAssignmentsList.Where(a => 
                        !string.IsNullOrEmpty(a.AssignmentDate) && 
                        DateTime.TryParse(a.AssignmentDate, out DateTime assignmentDate) && 
                        assignmentDate >= fromDate).ToList();
                }

                if (filter.DateTo.HasValue)
                {
                    var toDate = filter.DateTo.Value;
                    allAssignmentsList = allAssignmentsList.Where(a => 
                        !string.IsNullOrEmpty(a.AssignmentDate) && 
                        DateTime.TryParse(a.AssignmentDate, out DateTime assignmentDate) && 
                        assignmentDate <= toDate).ToList();
                }

                _logger.Information("انتسابات فیلتر شده با موفقیت بازگردانده شد. تعداد: {Count}", allAssignmentsList.Count);

                return ServiceResult<List<DoctorAssignmentListItem>>.Successful(allAssignmentsList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات فیلتر شده");
                return ServiceResult<List<DoctorAssignmentListItem>>.Failed("خطا در دریافت انتسابات فیلتر شده");
            }
        }

        #endregion
    }
}
