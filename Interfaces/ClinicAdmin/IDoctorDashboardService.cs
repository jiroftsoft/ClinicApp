using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Repositories.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس تخصصی برای داشبورد پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تمرکز صرف بر نمایش و مدیریت داده‌های داشبورد
    /// 2. رعایت استانداردهای پزشکی ایران در نمایش اطلاعات
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای نمایشی
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorDashboardService
    {
        #region Dashboard Data (داده‌های داشبورد)

        /// <summary>
        /// دریافت داده‌های داشبورد اصلی پزشکان
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <returns>نتیجه حاوی داده‌های داشبورد اصلی</returns>
        Task<ServiceResult<DoctorDashboardIndexViewModel>> GetDashboardDataAsync(int? clinicId = null, int? departmentId = null);

        /// <summary>
        /// دریافت جزئیات کامل یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه حاوی جزئیات کامل پزشک</returns>
        Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorDetailsAsync(int doctorId);

        /// <summary>
        /// دریافت لیست انتسابات یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه حاوی لیست انتسابات پزشک</returns>
        Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId);

        #endregion

        #region Search & Filter (جستجو و فیلتر)

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="specializationId">شناسه تخصص (اختیاری)</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه حاوی لیست پزشکان</returns>
        Task<ServiceResult<DoctorSearchResultViewModel>> SearchDoctorsAsync(string searchTerm = null, int? clinicId = null, int? departmentId = null, int? specializationId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت پزشکان بر اساس فیلتر
        /// </summary>
        /// <param name="filters">فیلترهای اعمال شده</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه حاوی لیست پزشکان</returns>
        Task<ServiceResult<DoctorSearchResultViewModel>> GetDoctorsByFilterAsync(DoctorFilterViewModel filters, int page = 1, int pageSize = 20);

        #endregion

        #region Statistics & Analytics (آمار و تحلیل)

        /// <summary>
        /// دریافت آمار کلی داشبورد
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>نتیجه حاوی آمار کلی</returns>
        Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync(int? clinicId = null);

        /// <summary>
        /// دریافت آمار پزشکان فعال
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <returns>نتیجه حاوی آمار پزشکان فعال</returns>
        Task<ServiceResult<ActiveDoctorsStatsViewModel>> GetActiveDoctorsStatsAsync(int? clinicId = null, int? departmentId = null);

        /// <summary>
        /// دریافت آمار انتسابات
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>نتیجه حاوی آمار انتسابات</returns>
        Task<ServiceResult<AssignmentStatsViewModel>> GetAssignmentStatsAsync(int? clinicId = null);

        #endregion

        #region Quick Actions (عملیات سریع)

        /// <summary>
        /// دریافت عملیات سریع برای پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه حاوی عملیات سریع</returns>
        Task<ServiceResult<List<QuickActionViewModel>>> GetQuickActionsAsync(int doctorId);

        /// <summary>
        /// بررسی وضعیت پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه حاوی وضعیت پزشک</returns>
        Task<ServiceResult<DoctorStatusViewModel>> GetDoctorStatusAsync(int doctorId);

        #endregion

        #region Notifications & Alerts (اعلان‌ها و هشدارها)

        /// <summary>
        /// دریافت اعلان‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه حاوی اعلان‌های پزشک</returns>
        Task<ServiceResult<List<DoctorNotificationViewModel>>> GetDoctorNotificationsAsync(int doctorId);

        /// <summary>
        /// دریافت هشدارهای سیستم
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>نتیجه حاوی هشدارهای سیستم</returns>
        Task<ServiceResult<List<SystemAlertViewModel>>> GetSystemAlertsAsync(int? clinicId = null);

        #endregion
    }

    #region Supporting ViewModels (ViewModels پشتیبان)

    /// <summary>
    /// مدل داشبورد اصلی پزشکان
    /// </summary>
    public class DoctorDashboardIndexViewModel
    {
        /// <summary>
        /// آمار کلی
        /// </summary>
        public DashboardStatsViewModel Stats { get; set; } = new DashboardStatsViewModel();

        /// <summary>
        /// پزشکان اخیر
        /// </summary>
        public List<DoctorSummaryViewModel> RecentDoctors { get; set; } = new List<DoctorSummaryViewModel>();

        /// <summary>
        /// انتسابات اخیر
        /// </summary>
        public List<RecentAssignmentViewModel> RecentAssignments { get; set; } = new List<RecentAssignmentViewModel>();

        /// <summary>
        /// هشدارهای سیستم
        /// </summary>
        public List<SystemAlertViewModel> SystemAlerts { get; set; } = new List<SystemAlertViewModel>();

        /// <summary>
        /// فیلترهای فعال
        /// </summary>
        public DoctorFilterViewModel ActiveFilters { get; set; } = new DoctorFilterViewModel();
    }

    /// <summary>
    /// مدل آمار کلی داشبورد
    /// </summary>
    public class DashboardStatsViewModel
    {
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int InactiveDoctors { get; set; }
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalServiceCategories { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    /// <summary>
    /// مدل آمار پزشکان فعال
    /// </summary>
    public class ActiveDoctorsStatsViewModel
    {
        public int TotalActiveDoctors { get; set; }
        public int DoctorsWithAssignments { get; set; }
        public int DoctorsWithoutAssignments { get; set; }
        public Dictionary<string, int> DepartmentDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SpecializationDistribution { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// مدل نتیجه جستجوی پزشکان
    /// </summary>
    public class DoctorSearchResultViewModel
    {
        public List<DoctorSummaryViewModel> Doctors { get; set; } = new List<DoctorSummaryViewModel>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public DoctorFilterViewModel AppliedFilters { get; set; } = new DoctorFilterViewModel();
    }

    /// <summary>
    /// مدل خلاصه پزشک
    /// </summary>
    public class DoctorSummaryViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string MedicalCouncilCode { get; set; }
        public List<string> SpecializationNames { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public int ActiveAssignmentsCount { get; set; }
        public string Status { get; set; }
        public DateTime LastActivity { get; set; }
    }

    /// <summary>
    /// مدل انتساب اخیر
    /// </summary>
    public class RecentAssignmentViewModel
    {
        public string AssignmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string ActionType { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string PerformedBy { get; set; }
    }

    /// <summary>
    /// مدل عملیات سریع
    /// </summary>
    public class QuickActionViewModel
    {
        public string ActionName { get; set; }
        public string ActionTitle { get; set; }
        public string ActionUrl { get; set; }
        public string IconClass { get; set; }
        public string ColorClass { get; set; }
        public bool IsEnabled { get; set; }
        public string Tooltip { get; set; }
    }

    /// <summary>
    /// مدل وضعیت پزشک
    /// </summary>
    public class DoctorStatusViewModel
    {
        public int DoctorId { get; set; }
        public string Status { get; set; }
        public string StatusClass { get; set; }
        public bool HasActiveAssignments { get; set; }
        public bool HasActiveSchedule { get; set; }
        public bool HasPendingAppointments { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// مدل اعلان پزشک
    /// </summary>
    public class DoctorNotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string ActionUrl { get; set; }
    }

    /// <summary>
    /// مدل هشدار سیستم
    /// </summary>
    public class SystemAlertViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAcknowledged { get; set; }
        public string ActionUrl { get; set; }
    }

    #endregion
}
