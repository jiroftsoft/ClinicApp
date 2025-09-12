using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Core;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// رابط Repository برای مدیریت تاریخچه انتسابات پزشکان
    /// این رابط برای عملیات CRUD و جستجو در تاریخچه انتسابات طراحی شده است
    /// </summary>
    public interface IDoctorAssignmentHistoryRepository
    {
        #region Core CRUD Operations (عملیات اصلی CRUD)

        /// <summary>
        /// دریافت تاریخچه انتساب بر اساس شناسه
        /// </summary>
        Task<DoctorAssignmentHistory> GetByIdAsync(int id);

        /// <summary>
        /// دریافت تمام تاریخچه‌های انتساب
        /// </summary>
        Task<List<DoctorAssignmentHistory>> GetAllAsync();

        /// <summary>
        /// افزودن تاریخچه انتساب جدید
        /// </summary>
        Task<DoctorAssignmentHistory> AddAsync(DoctorAssignmentHistory history);

        /// <summary>
        /// به‌روزرسانی تاریخچه انتساب
        /// </summary>
        Task<DoctorAssignmentHistory> UpdateAsync(DoctorAssignmentHistory history);

        /// <summary>
        /// حذف نرم تاریخچه انتساب
        /// </summary>
        Task<bool> DeleteAsync(int id);

        #endregion

        /// <summary>
        /// دریافت تاریخچه انتسابات یک پزشک خاص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات پزشک</returns>
        Task<List<DoctorAssignmentHistory>> GetDoctorHistoryAsync(int doctorId, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس نوع عملیات
        /// </summary>
        /// <param name="actionType">نوع عملیات</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات</returns>
        Task<List<DoctorAssignmentHistory>> GetHistoryByActionTypeAsync(string actionType, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس سطح اهمیت
        /// </summary>
        /// <param name="importance">سطح اهمیت</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات</returns>
        Task<List<DoctorAssignmentHistory>> GetHistoryByImportanceAsync(AssignmentHistoryImportance importance, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس دپارتمان
        /// </summary>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات</returns>
        Task<List<DoctorAssignmentHistory>> GetHistoryByDepartmentAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت تاریخچه انتسابات بر اساس کاربر انجام‌دهنده
        /// </summary>
        /// <param name="performedByUserId">شناسه کاربر انجام‌دهنده</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات</returns>
        Task<List<DoctorAssignmentHistory>> GetHistoryByPerformedByAsync(string performedByUserId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// جستجوی پیشرفته در تاریخچه انتسابات
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="actionType">نوع عملیات</param>
        /// <param name="importance">سطح اهمیت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="performedByUserId">شناسه کاربر انجام‌دهنده</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست تاریخچه انتسابات</returns>
        Task<List<DoctorAssignmentHistory>> SearchHistoryAsync(string searchTerm = null, string actionType = null, AssignmentHistoryImportance? importance = null, int? departmentId = null, string performedByUserId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// دریافت آمار تاریخچه انتسابات
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار تاریخچه انتسابات</returns>
        Task<HistoryStats> GetHistoryStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// پاک‌سازی تاریخچه قدیمی
        /// </summary>
        /// <param name="olderThanDays">تاریخچه قدیمی‌تر از این تعداد روز</param>
        /// <returns>تعداد رکوردهای پاک شده</returns>
        Task<int> CleanupOldHistoryAsync(int olderThanDays = 365);
    }

    /// <summary>
    /// آمار تاریخچه انتسابات
    /// </summary>
    public class HistoryStats
    {
        public int TotalRecords { get; set; }
        public int CriticalRecords { get; set; }
        public int ImportantRecords { get; set; }
        public int NormalRecords { get; set; }
        public int SecurityRecords { get; set; }
        public int AssignmentsCount { get; set; }
        public int TransfersCount { get; set; }
        public int RemovalsCount { get; set; }
        public string MostActiveMonth { get; set; }
        public string MostActiveDoctor { get; set; }
        public string MostActiveDepartment { get; set; }
        public decimal AverageChangesPerMonth { get; set; }
        public Dictionary<string, int> ActionTypeCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DepartmentCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UserCounts { get; set; } = new Dictionary<string, int>();
    }
}
