using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment.POS
{
    /// <summary>
    /// Repository Interface برای مدیریت جلسات نقدی
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت CRUD جلسات نقدی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل جلسات نقدی
    /// 2. پشتیبانی از Soft Delete برای حفظ اطلاعات
    /// 3. بهینه‌سازی برای عملکرد بالا
    /// </summary>
    public interface ICashSessionRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت جلسه نقدی بر اساس شناسه
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>جلسه نقدی یا null</returns>
        Task<CashSession> GetByIdAsync(int sessionId);

        /// <summary>
        /// دریافت تمام جلسات نقدی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست جلسات</returns>
        Task<IEnumerable<CashSession>> GetAllAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// افزودن جلسه نقدی جدید
        /// </summary>
        /// <param name="session">جلسه نقدی</param>
        /// <returns>جلسه اضافه شده</returns>
        Task<CashSession> AddAsync(CashSession session);

        /// <summary>
        /// به‌روزرسانی جلسه نقدی
        /// </summary>
        /// <param name="session">جلسه نقدی</param>
        /// <returns>جلسه به‌روزرسانی شده</returns>
        Task<CashSession> UpdateAsync(CashSession session);

        /// <summary>
        /// حذف نرم جلسه نقدی
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> SoftDeleteAsync(int sessionId, string deletedByUserId);

        #endregion

        #region Query Operations

        /// <summary>
        /// دریافت جلسات فعال
        /// </summary>
        /// <returns>لیست جلسات فعال</returns>
        Task<IEnumerable<CashSession>> GetActiveSessionsAsync();

        /// <summary>
        /// دریافت جلسات بر اساس کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست جلسات</returns>
        Task<IEnumerable<CashSession>> GetByUserIdAsync(string userId);

        /// <summary>
        /// دریافت جلسات بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>لیست جلسات</returns>
        Task<IEnumerable<CashSession>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت جلسات بر اساس وضعیت
        /// </summary>
        /// <param name="status">وضعیت جلسه</param>
        /// <returns>لیست جلسات</returns>
        Task<IEnumerable<CashSession>> GetByStatusAsync(CashSessionStatus status);

        /// <summary>
        /// جستجوی جلسات نقدی
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست جلسات</returns>
        Task<IEnumerable<CashSession>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 50);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود جلسه
        /// </summary>
        /// <param name="sessionId">شناسه جلسه</param>
        /// <returns>true اگر وجود دارد</returns>
        Task<bool> ExistsAsync(int sessionId);

        /// <summary>
        /// بررسی وجود جلسه فعال برای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>true اگر جلسه فعال وجود دارد</returns>
        Task<bool> HasActiveSessionAsync(string userId);

        /// <summary>
        /// دریافت تعداد جلسات
        /// </summary>
        /// <returns>تعداد جلسات</returns>
        Task<int> GetCountAsync();

        #endregion

        #region Statistics Operations

        /// <summary>
        /// دریافت آمار جلسات نقدی
        /// </summary>
        /// <returns>آمار جلسات</returns>
        Task<CashSessionStatistics> GetStatisticsAsync();

        /// <summary>
        /// دریافت آمار جلسات بر اساس تاریخ
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>آمار جلسات</returns>
        Task<CashSessionStatistics> GetStatisticsAsync(DateTime startDate, DateTime endDate);

        #endregion
    }

    /// <summary>
    /// آمار جلسات نقدی
    /// </summary>
    public class CashSessionStatistics
    {
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int CancelledSessions { get; set; }
        public decimal TotalInitialCash { get; set; }
        public decimal TotalFinalCash { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalDifference { get; set; }
        public decimal AverageSessionDuration { get; set; }
        public DateTime? LastSessionDate { get; set; }
    }
}