using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Repositories.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای داشبورد پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل منطق کسب‌وکار داشبورد پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در منطق کسب‌وکار
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای کسب‌وکار
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorDashboardService : IDoctorDashboardService
    {
        private readonly IDoctorDashboardRepository _dashboardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public DoctorDashboardService(
            IDoctorDashboardRepository dashboardRepository,
            ICurrentUserService currentUserService)
        {
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<DoctorDashboardService>();
        }

        #region Dashboard Data (داده‌های داشبورد)

        /// <summary>
        /// دریافت داده‌های داشبورد اصلی پزشکان
        /// </summary>
        public async Task<ServiceResult<DoctorDashboardIndexViewModel>> GetDashboardDataAsync(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست دریافت داده‌های داشبورد اصلی توسط کاربر {UserId}", _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<DoctorDashboardIndexViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                if (departmentId.HasValue && departmentId.Value <= 0)
                {
                    return ServiceResult<DoctorDashboardIndexViewModel>.Failed("شناسه دپارتمان نامعتبر است.");
                }

                // دریافت داده‌های داشبورد
                var dashboardData = await _dashboardRepository.GetDashboardDataAsync(clinicId, departmentId);

                _logger.Information("داده‌های داشبورد اصلی با موفقیت دریافت شد. کلینیک: {ClinicId}, دپارتمان: {DepartmentId}", 
                    clinicId?.ToString() ?? "همه", departmentId?.ToString() ?? "همه");

                return ServiceResult<DoctorDashboardIndexViewModel>.Successful(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های داشبورد اصلی");
                return ServiceResult<DoctorDashboardIndexViewModel>.Failed("خطا در دریافت داده‌های داشبورد");
            }
        }

        /// <summary>
        /// دریافت جزئیات کامل یک پزشک
        /// </summary>
        public async Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت جزئیات پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorDetailsViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // دریافت جزئیات پزشک
                var doctorDetails = await _dashboardRepository.GetDoctorDetailsAsync(doctorId);

                _logger.Information("جزئیات پزشک {DoctorId} با موفقیت دریافت شد", doctorId);

                return ServiceResult<DoctorDetailsViewModel>.Successful(doctorDetails);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning("پزشک {DoctorId} یافت نشد: {Message}", doctorId, ex.Message);
                return ServiceResult<DoctorDetailsViewModel>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorDetailsViewModel>.Failed("خطا در دریافت جزئیات پزشک");
            }
        }

        /// <summary>
        /// دریافت لیست انتسابات یک پزشک
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت انتسابات پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorAssignmentsViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // دریافت انتسابات پزشک
                var assignments = await _dashboardRepository.GetDoctorAssignmentsAsync(doctorId);

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت دریافت شد. تعداد انتسابات فعال: {ActiveCount}", 
                    doctorId, assignments.TotalActiveAssignments);

                return ServiceResult<DoctorAssignmentsViewModel>.Successful(assignments);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning("پزشک {DoctorId} یافت نشد: {Message}", doctorId, ex.Message);
                return ServiceResult<DoctorAssignmentsViewModel>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorAssignmentsViewModel>.Failed("خطا در دریافت انتسابات پزشک");
            }
        }

        #endregion

        #region Search & Filter (جستجو و فیلتر)

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        public async Task<ServiceResult<DoctorSearchResultViewModel>> SearchDoctorsAsync(string searchTerm = null, int? clinicId = null, int? departmentId = null, int? specializationId = null, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست جستجوی پزشکان توسط کاربر {UserId}. جستجو: {SearchTerm}, صفحه: {Page}", 
                    _currentUserService.UserId, searchTerm ?? "بدون فیلتر", page);

                // اعتبارسنجی پارامترها
                if (page <= 0)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("شماره صفحه نامعتبر است.");
                }

                if (pageSize <= 0 || pageSize > 100)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("اندازه صفحه نامعتبر است.");
                }

                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                if (departmentId.HasValue && departmentId.Value <= 0)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("شناسه دپارتمان نامعتبر است.");
                }

                if (specializationId.HasValue && specializationId.Value <= 0)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("شناسه تخصص نامعتبر است.");
                }

                // جستجوی پزشکان
                var searchResult = await _dashboardRepository.SearchDoctorsAsync(searchTerm, clinicId, departmentId, specializationId, page, pageSize);

                _logger.Information("جستجوی پزشکان با موفقیت انجام شد. تعداد نتایج: {TotalCount}, صفحه: {Page}", 
                    searchResult.TotalCount, page);

                return ServiceResult<DoctorSearchResultViewModel>.Successful(searchResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پزشکان");
                return ServiceResult<DoctorSearchResultViewModel>.Failed("خطا در جستجوی پزشکان");
            }
        }

        /// <summary>
        /// دریافت پزشکان بر اساس فیلتر
        /// </summary>
        public async Task<ServiceResult<DoctorSearchResultViewModel>> GetDoctorsByFilterAsync(DoctorFilterViewModel filters, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست دریافت پزشکان بر اساس فیلتر توسط کاربر {UserId}. صفحه: {Page}", 
                    _currentUserService.UserId, page);

                // اعتبارسنجی پارامترها
                if (filters == null)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("فیلترهای اعمال شده نامعتبر است.");
                }

                if (page <= 0)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("شماره صفحه نامعتبر است.");
                }

                if (pageSize <= 0 || pageSize > 100)
                {
                    return ServiceResult<DoctorSearchResultViewModel>.Failed("اندازه صفحه نامعتبر است.");
                }

                // جستجو بر اساس فیلتر
                var searchResult = await _dashboardRepository.SearchDoctorsAsync(
                    filters.SearchTerm, 
                    filters.ClinicId, 
                    filters.DepartmentId, 
                    filters.SpecializationId, 
                    page, 
                    pageSize);

                _logger.Information("دریافت پزشکان بر اساس فیلتر با موفقیت انجام شد. تعداد نتایج: {TotalCount}", 
                    searchResult.TotalCount);

                return ServiceResult<DoctorSearchResultViewModel>.Successful(searchResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پزشکان بر اساس فیلتر");
                return ServiceResult<DoctorSearchResultViewModel>.Failed("خطا در دریافت پزشکان بر اساس فیلتر");
            }
        }

        #endregion

        #region Statistics & Analytics (آمار و تحلیل)

        /// <summary>
        /// دریافت آمار کلی داشبورد
        /// </summary>
        public async Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync(int? clinicId = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار کلی داشبورد توسط کاربر {UserId}. کلینیک: {ClinicId}", 
                    _currentUserService.UserId, clinicId?.ToString() ?? "همه");

                // اعتبارسنجی پارامترها
                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<DashboardStatsViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                // دریافت آمار
                var stats = await _dashboardRepository.GetDashboardStatsAsync(clinicId);

                _logger.Information("آمار کلی داشبورد با موفقیت دریافت شد. کل پزشکان: {TotalDoctors}, پزشکان فعال: {ActiveDoctors}", 
                    stats.TotalDoctors, stats.ActiveDoctors);

                return ServiceResult<DashboardStatsViewModel>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار کلی داشبورد");
                return ServiceResult<DashboardStatsViewModel>.Failed("خطا در دریافت آمار کلی داشبورد");
            }
        }

        /// <summary>
        /// دریافت آمار پزشکان فعال
        /// </summary>
        public async Task<ServiceResult<ActiveDoctorsStatsViewModel>> GetActiveDoctorsStatsAsync(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار پزشکان فعال توسط کاربر {UserId}. کلینیک: {ClinicId}, دپارتمان: {DepartmentId}", 
                    _currentUserService.UserId, clinicId?.ToString() ?? "همه", departmentId?.ToString() ?? "همه");

                // اعتبارسنجی پارامترها
                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<ActiveDoctorsStatsViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                if (departmentId.HasValue && departmentId.Value <= 0)
                {
                    return ServiceResult<ActiveDoctorsStatsViewModel>.Failed("شناسه دپارتمان نامعتبر است.");
                }

                // دریافت آمار پزشکان فعال
                var stats = await _dashboardRepository.GetActiveDoctorsStatsAsync(clinicId, departmentId);

                _logger.Information("آمار پزشکان فعال با موفقیت دریافت شد. کل پزشکان فعال: {TotalActiveDoctors}", 
                    stats.TotalActiveDoctors);

                return ServiceResult<ActiveDoctorsStatsViewModel>.Successful(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشکان فعال");
                return ServiceResult<ActiveDoctorsStatsViewModel>.Failed("خطا در دریافت آمار پزشکان فعال");
            }
        }

        /// <summary>
        /// دریافت آمار انتسابات
        /// </summary>
        public async Task<ServiceResult<AssignmentStatsViewModel>> GetAssignmentStatsAsync(int? clinicId = null)
        {
            try
            {
                _logger.Information("درخواست دریافت آمار انتسابات توسط کاربر {UserId}. کلینیک: {ClinicId}", 
                    _currentUserService.UserId, clinicId?.ToString() ?? "همه");

                // اعتبارسنجی پارامترها
                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<AssignmentStatsViewModel>.Failed("شناسه کلینیک نامعتبر است.");
                }

                // دریافت آمار انتسابات از داشبورد
                var dashboardStats = await _dashboardRepository.GetDashboardStatsAsync(clinicId);

                var assignmentStats = new AssignmentStatsViewModel
                {
                    TotalAssignments = dashboardStats.TotalAssignments,
                    ActiveAssignments = dashboardStats.ActiveAssignments,
                    InactiveAssignments = dashboardStats.TotalAssignments - dashboardStats.ActiveAssignments,
                    CompletionPercentage = dashboardStats.TotalAssignments > 0 ? 
                        (decimal)((double)dashboardStats.ActiveAssignments / dashboardStats.TotalAssignments * 100) : 0,
                    LastUpdate = dashboardStats.LastUpdate
                };

                _logger.Information("آمار انتسابات با موفقیت دریافت شد. کل انتسابات: {TotalAssignments}, انتسابات فعال: {ActiveAssignments}", 
                    assignmentStats.TotalAssignments, assignmentStats.ActiveAssignments);

                return ServiceResult<AssignmentStatsViewModel>.Successful(assignmentStats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار انتسابات");
                return ServiceResult<AssignmentStatsViewModel>.Failed("خطا در دریافت آمار انتسابات");
            }
        }

        #endregion

        #region Quick Actions (عملیات سریع)

        /// <summary>
        /// دریافت عملیات سریع برای پزشک
        /// </summary>
        public async Task<ServiceResult<List<QuickActionViewModel>>> GetQuickActionsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت عملیات سریع برای پزشک {DoctorId} توسط کاربر {UserId}", 
                    doctorId, _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<QuickActionViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctorDetails = await _dashboardRepository.GetDoctorDetailsAsync(doctorId);
                if (doctorDetails == null)
                {
                    return ServiceResult<List<QuickActionViewModel>>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // ایجاد لیست عملیات سریع
                var quickActions = new List<QuickActionViewModel>
                {
                    new QuickActionViewModel
                    {
                        ActionName = "ViewDetails",
                        ActionTitle = "مشاهده جزئیات",
                        ActionUrl = $"/Admin/DoctorDashboard/Details/{doctorId}",
                        IconClass = "fas fa-eye",
                        ColorClass = "btn-info",
                        IsEnabled = true,
                        Tooltip = "مشاهده جزئیات کامل پزشک"
                    },
                    new QuickActionViewModel
                    {
                        ActionName = "ViewAssignments",
                        ActionTitle = "مشاهده انتسابات",
                        ActionUrl = $"/Admin/DoctorDashboard/Assignments/{doctorId}",
                        IconClass = "fas fa-link",
                        ColorClass = "btn-primary",
                        IsEnabled = true,
                        Tooltip = "مشاهده انتسابات پزشک"
                    },
                    new QuickActionViewModel
                    {
                        ActionName = "AssignToDepartment",
                        ActionTitle = "انتساب به دپارتمان",
                        ActionUrl = $"/Admin/DoctorDepartment/AssignToDepartment/{doctorId}",
                        IconClass = "fas fa-plus",
                        ColorClass = "btn-success",
                        IsEnabled = true,
                        Tooltip = "انتساب پزشک به دپارتمان"
                    },
                    new QuickActionViewModel
                    {
                        ActionName = "AssignToServiceCategory",
                        ActionTitle = "انتساب به دسته‌بندی خدمات",
                        ActionUrl = $"/Admin/DoctorServiceCategory/AssignToServiceCategory/{doctorId}",
                        IconClass = "fas fa-certificate",
                        ColorClass = "btn-warning",
                        IsEnabled = true,
                        Tooltip = "انتساب پزشک به دسته‌بندی خدمات"
                    },
                    new QuickActionViewModel
                    {
                        ActionName = "ViewSchedule",
                        ActionTitle = "مشاهده برنامه کاری",
                        ActionUrl = $"/Admin/DoctorSchedule/AssignSchedule/{doctorId}",
                        IconClass = "fas fa-calendar",
                        ColorClass = "btn-secondary",
                        IsEnabled = true,
                        Tooltip = "مشاهده و تنظیم برنامه کاری پزشک"
                    }
                };

                _logger.Information("عملیات سریع برای پزشک {DoctorId} با موفقیت دریافت شد. تعداد عملیات: {ActionCount}", 
                    doctorId, quickActions.Count);

                return ServiceResult<List<QuickActionViewModel>>.Successful(quickActions);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning("پزشک {DoctorId} یافت نشد: {Message}", doctorId, ex.Message);
                return ServiceResult<List<QuickActionViewModel>>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت عملیات سریع برای پزشک {DoctorId}", doctorId);
                return ServiceResult<List<QuickActionViewModel>>.Failed("خطا در دریافت عملیات سریع");
            }
        }

        /// <summary>
        /// بررسی وضعیت پزشک
        /// </summary>
        public async Task<ServiceResult<DoctorStatusViewModel>> GetDoctorStatusAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست بررسی وضعیت پزشک {DoctorId} توسط کاربر {UserId}", 
                    doctorId, _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorStatusViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // دریافت جزئیات پزشک
                var doctorDetails = await _dashboardRepository.GetDoctorDetailsAsync(doctorId);
                if (doctorDetails == null)
                {
                    return ServiceResult<DoctorStatusViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت انتسابات پزشک
                var assignments = await _dashboardRepository.GetDoctorAssignmentsAsync(doctorId);

                // تعیین وضعیت
                var status = doctorDetails.IsActive ? "فعال" : "غیرفعال";
                var statusClass = doctorDetails.IsActive ? "text-success" : "text-danger";

                var warnings = new List<string>();
                var recommendations = new List<string>();

                // بررسی هشدارها
                if (!doctorDetails.IsActive)
                {
                    warnings.Add("پزشک غیرفعال است");
                    recommendations.Add("فعال‌سازی حساب کاربری پزشک");
                }

                if (assignments.TotalActiveAssignments == 0)
                {
                    warnings.Add("هیچ انتساب فعالی ندارد");
                    recommendations.Add("انتساب پزشک به دپارتمان");
                }

                if (assignments.ActiveServiceCategoryCount == 0)
                {
                    warnings.Add("هیچ صلاحیت خدماتی ندارد");
                    recommendations.Add("اعطای صلاحیت‌های خدماتی");
                }

                var doctorStatus = new DoctorStatusViewModel
                {
                    DoctorId = doctorId,
                    Status = status,
                    StatusClass = statusClass,
                    HasActiveAssignments = assignments.TotalActiveAssignments > 0,
                    HasActiveSchedule = false, // در آینده از DoctorScheduleRepository استفاده شود
                    HasPendingAppointments = false, // در آینده از AppointmentRepository استفاده شود
                    Warnings = warnings,
                    Recommendations = recommendations
                };

                _logger.Information("وضعیت پزشک {DoctorId} با موفقیت بررسی شد. وضعیت: {Status}, تعداد هشدارها: {WarningCount}", 
                    doctorId, status, warnings.Count);

                return ServiceResult<DoctorStatusViewModel>.Successful(doctorStatus);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning("پزشک {DoctorId} یافت نشد: {Message}", doctorId, ex.Message);
                return ServiceResult<DoctorStatusViewModel>.Failed(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorStatusViewModel>.Failed("خطا در بررسی وضعیت پزشک");
            }
        }

        #endregion

        #region Notifications & Alerts (اعلان‌ها و هشدارها)

        /// <summary>
        /// دریافت اعلان‌های پزشک
        /// </summary>
        public async Task<ServiceResult<List<DoctorNotificationViewModel>>> GetDoctorNotificationsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت اعلان‌های پزشک {DoctorId} توسط کاربر {UserId}", 
                    doctorId, _currentUserService.UserId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<List<DoctorNotificationViewModel>>.Failed("شناسه پزشک نامعتبر است.");
                }

                // فعلاً یک لیست خالی برمی‌گردانیم
                // در آینده می‌توان از جدول DoctorNotifications استفاده کرد
                var notifications = new List<DoctorNotificationViewModel>();

                _logger.Information("اعلان‌های پزشک {DoctorId} با موفقیت دریافت شد. تعداد اعلان‌ها: {NotificationCount}", 
                    doctorId, notifications.Count);

                return ServiceResult<List<DoctorNotificationViewModel>>.Successful(notifications);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اعلان‌های پزشک {DoctorId}", doctorId);
                return ServiceResult<List<DoctorNotificationViewModel>>.Failed("خطا در دریافت اعلان‌های پزشک");
            }
        }

        /// <summary>
        /// دریافت هشدارهای سیستم
        /// </summary>
        public async Task<ServiceResult<List<SystemAlertViewModel>>> GetSystemAlertsAsync(int? clinicId = null)
        {
            try
            {
                _logger.Information("درخواست دریافت هشدارهای سیستم توسط کاربر {UserId}. کلینیک: {ClinicId}", 
                    _currentUserService.UserId, clinicId?.ToString() ?? "همه");

                // اعتبارسنجی پارامترها
                if (clinicId.HasValue && clinicId.Value <= 0)
                {
                    return ServiceResult<List<SystemAlertViewModel>>.Failed("شناسه کلینیک نامعتبر است.");
                }

                // فعلاً یک لیست خالی برمی‌گردانیم
                // در آینده می‌توان از جدول SystemAlerts استفاده کرد
                var alerts = new List<SystemAlertViewModel>();

                _logger.Information("هشدارهای سیستم با موفقیت دریافت شد. تعداد هشدارها: {AlertCount}", alerts.Count);

                return ServiceResult<List<SystemAlertViewModel>>.Successful(alerts);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت هشدارهای سیستم");
                return ServiceResult<List<SystemAlertViewModel>>.Failed("خطا در دریافت هشدارهای سیستم");
            }
        }

        #endregion
    }
}
