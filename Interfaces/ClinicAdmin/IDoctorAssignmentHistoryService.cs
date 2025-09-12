using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;
using ClinicApp.Repositories.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// رابط سرویس برای مدیریت تاریخچه انتسابات پزشکان
    /// این رابط برای عملیات تجاری و منطق کسب‌وکار طراحی شده است
    /// </summary>
    public interface IDoctorAssignmentHistoryService
    {
        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت تاریخچه انتساب بر اساس شناسه
        /// </summary>
        Task<ServiceResult<DoctorAssignmentHistory>> GetByIdAsync(int id);

        /// <summary>
        /// دریافت تمام تاریخچه‌های انتساب
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetAllAsync();

        /// <summary>
        /// افزودن تاریخچه انتساب جدید
        /// </summary>
        Task<ServiceResult<DoctorAssignmentHistory>> AddAsync(DoctorAssignmentHistory history);

        /// <summary>
        /// به‌روزرسانی تاریخچه انتساب
        /// </summary>
        Task<ServiceResult<DoctorAssignmentHistory>> UpdateAsync(DoctorAssignmentHistory history);

        /// <summary>
        /// حذف نرم تاریخچه انتساب
        /// </summary>
        Task<ServiceResult<bool>> DeleteAsync(int id);

        #endregion

        #region Specialized Operations (عملیات تخصصی)

        /// <summary>
        /// دریافت تاریخچه انتسابات یک پزشک خاص
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetDoctorHistoryAsync(int doctorId, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس نوع عملیات
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByActionTypeAsync(string actionType, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس سطح اهمیت
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByImportanceAsync(AssignmentHistoryImportance importance, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس دپارتمان
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByDepartmentAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetHistoryByPerformedByAsync(string performedByUserId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// جستجوی پیشرفته در تاریخچه انتسابات
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> SearchHistoryAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        #endregion

        #region Statistics & Reporting (آمار و گزارش‌گیری)

        /// <summary>
        /// دریافت آمار تاریخچه انتسابات
        /// </summary>
        Task<ServiceResult<HistoryStats>> GetHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// دریافت آمار تاریخچه برای نمایش
        /// </summary>
        Task<ServiceResult<AssignmentHistoryStatisticsViewModel>> GetHistoryStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// دریافت آمار تاریخچه برای داشبورد
        /// </summary>
        Task<ServiceResult<DashboardHistoryStats>> GetDashboardStatsAsync();

        /// <summary>
        /// دریافت گزارش تغییرات بحرانی
        /// </summary>
        Task<ServiceResult<List<DoctorAssignmentHistory>>> GetCriticalChangesAsync(DateTime? startDate = null, DateTime? endDate = null);

        #endregion

        #region Maintenance Operations (عملیات نگهداری)

        /// <summary>
        /// پاک‌سازی تاریخچه قدیمی
        /// </summary>
        Task<ServiceResult<int>> CleanupOldHistoryAsync(int olderThanDays = 365);

        /// <summary>
        /// آرشیو تاریخچه‌های قدیمی
        /// </summary>
        Task<ServiceResult<bool>> ArchiveOldHistoryAsync(DateTime cutoffDate);

        #endregion

        #region Business Logic Operations (عملیات منطق کسب‌وکار)

        /// <summary>
        /// ثبت خودکار تاریخچه برای عملیات انتساب
        /// </summary>
        Task<ServiceResult<bool>> LogAssignmentOperationAsync(int doctorId, string actionType, string actionTitle, string actionDescription, int? departmentId = null, string serviceCategories = null, string notes = null, AssignmentHistoryImportance importance = AssignmentHistoryImportance.Normal);

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات پزشک
        /// </summary>
        Task<ServiceResult<bool>> LogDoctorChangeAsync(int doctorId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null);

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات دپارتمان
        /// </summary>
        Task<ServiceResult<bool>> LogDepartmentChangeAsync(int doctorId, int departmentId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null);

        /// <summary>
        /// ثبت خودکار تاریخچه برای تغییرات صلاحیت‌های خدماتی
        /// </summary>
        Task<ServiceResult<bool>> LogServiceCategoryChangeAsync(int doctorId, string actionType, string actionTitle, string previousData = null, string newData = null, string notes = null);

        #endregion

        #region Export Operations (عملیات خروجی)

        /// <summary>
        /// خروجی Excel تاریخچه انتسابات
        /// </summary>
        Task<ServiceResult<byte[]>> ExportToExcelAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// خروجی PDF تاریخچه انتسابات
        /// </summary>
        Task<ServiceResult<byte[]>> ExportToPdfAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null);

        #endregion
    }

    /// <summary>
    /// آمار داشبورد تاریخچه انتسابات
    /// </summary>
    public class DashboardHistoryStats
    {
        public int TotalRecords { get; set; }
        public int TodayRecords { get; set; }
        public int ThisWeekRecords { get; set; }
        public int ThisMonthRecords { get; set; }
        public int CriticalRecords { get; set; }
        public int ImportantRecords { get; set; }
        public Dictionary<string, int> ActionTypeCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DepartmentCounts { get; set; } = new Dictionary<string, int>();
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    }

    /// <summary>
    /// فعالیت‌های اخیر
    /// </summary>
    public class RecentActivity
    {
        public int Id { get; set; }
        public string ActionTitle { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string PerformedByUserName { get; set; }
        public DateTime ActionDate { get; set; }
        public AssignmentHistoryImportance Importance { get; set; }
    }
}
