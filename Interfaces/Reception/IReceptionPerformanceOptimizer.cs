using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای بهینه‌سازی عملکرد ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. استفاده از Compiled Queries
    /// 2. بهینه‌سازی کوئری‌های دیتابیس
    /// 3. مدیریت Cache
    /// 4. بهینه‌سازی Memory Usage
    /// 5. Parallel Processing
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط بهینه‌سازی عملکرد
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public interface IReceptionPerformanceOptimizer
    {
        #region Optimized Query Methods

        /// <summary>
        /// دریافت بهینه پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست بهینه پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsOptimizedAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// دریافت جزئیات بهینه پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات بهینه پذیرش</returns>
        Task<ReceptionComponentViewModels.ReceptionDetailsViewModel> GetReceptionDetailsOptimizedAsync(int id);

        /// <summary>
        /// دریافت لیست بهینه پذیرش‌ها برای نمایش
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست بهینه پذیرش‌ها</returns>
        Task<List<ReceptionComponentViewModels.ReceptionIndexViewModel>> GetReceptionsIndexOptimizedAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// دریافت آمار بهینه پذیرش‌ها
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار بهینه</returns>
        Task<ReceptionDailyStatsViewModel> GetDailyStatsOptimizedAsync(DateTime date);

        #endregion

        #region Compiled Query Methods

        /// <summary>
        /// دریافت پذیرش با Compiled Query
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>پذیرش</returns>
        Task<Models.Entities.Reception.Reception> GetReceptionByIdCompiledAsync(int id);

        /// <summary>
        /// دریافت پذیرش‌ها با Compiled Query
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateCompiledAsync(DateTime date);

        /// <summary>
        /// دریافت پذیرش‌های بیمار با Compiled Query
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByPatientCompiledAsync(int patientId);

        /// <summary>
        /// دریافت پذیرش‌های پزشک با Compiled Query
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorCompiledAsync(int doctorId);

        #endregion

        #region Projection Methods

        /// <summary>
        /// دریافت فیلدهای محدود پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست فیلدهای محدود</returns>
        Task<List<ReceptionProjectionViewModel>> GetReceptionsProjectionAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// دریافت فیلدهای محدود پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>فیلدهای محدود پذیرش</returns>
        Task<ReceptionProjectionViewModel> GetReceptionProjectionAsync(int id);

        #endregion

        #region Caching Methods

        /// <summary>
        /// دریافت پذیرش‌ها از Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها از Cache</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsFromCacheAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// ذخیره پذیرش‌ها در Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <param name="receptions">لیست پذیرش‌ها</param>
        /// <returns>نتیجه ذخیره</returns>
        Task<bool> CacheReceptionsAsync(ReceptionSearchCriteria criteria, List<Models.Entities.Reception.Reception> receptions);

        /// <summary>
        /// پاک کردن Cache پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearReceptionsCacheAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// پاک کردن تمام Cache
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearAllCacheAsync();

        #endregion

        #region Parallel Processing Methods

        /// <summary>
        /// دریافت موازی پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsParallelAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// دریافت موازی جزئیات پذیرش‌ها
        /// </summary>
        /// <param name="ids">شناسه‌های پذیرش‌ها</param>
        /// <returns>لیست جزئیات پذیرش‌ها</returns>
        Task<List<ReceptionComponentViewModels.ReceptionDetailsViewModel>> GetReceptionsDetailsParallelAsync(List<int> ids);

        /// <summary>
        /// دریافت موازی آمار پذیرش‌ها
        /// </summary>
        /// <param name="dates">تاریخ‌ها</param>
        /// <returns>لیست آمار</returns>
        Task<List<ReceptionDailyStatsViewModel>> GetDailyStatsParallelAsync(List<DateTime> dates);

        #endregion

        #region Memory Optimization Methods

        /// <summary>
        /// بهینه‌سازی استفاده از حافظه
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>نتیجه بهینه‌سازی</returns>
        Task<bool> OptimizeMemoryUsageAsync(ReceptionSearchCriteria criteria);

        /// <summary>
        /// پاک کردن حافظه غیرضروری
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        Task<bool> ClearUnnecessaryMemoryAsync();

        /// <summary>
        /// بررسی استفاده از حافظه
        /// </summary>
        /// <returns>اطلاعات استفاده از حافظه</returns>
        Task<MemoryUsageInfo> GetMemoryUsageInfoAsync();

        #endregion

        #region Performance Monitoring Methods

        /// <summary>
        /// دریافت اطلاعات عملکرد
        /// </summary>
        /// <returns>اطلاعات عملکرد</returns>
        Task<PerformanceInfo> GetPerformanceInfoAsync();

        /// <summary>
        /// دریافت آمار کوئری‌ها
        /// </summary>
        /// <returns>آمار کوئری‌ها</returns>
        Task<QueryStatistics> GetQueryStatisticsAsync();

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        /// <returns>آمار Cache</returns>
        Task<CacheStatistics> GetCacheStatisticsAsync();

        #endregion

        #region Bulk Operations Methods

        /// <summary>
        /// دریافت دسته‌ای پذیرش‌ها
        /// </summary>
        /// <param name="ids">شناسه‌های پذیرش‌ها</param>
        /// <returns>لیست پذیرش‌ها</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsBulkAsync(List<int> ids);

        /// <summary>
        /// به‌روزرسانی دسته‌ای پذیرش‌ها
        /// </summary>
        /// <param name="receptions">لیست پذیرش‌ها</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<bool> UpdateReceptionsBulkAsync(List<Models.Entities.Reception.Reception> receptions);

        /// <summary>
        /// حذف دسته‌ای پذیرش‌ها
        /// </summary>
        /// <param name="ids">شناسه‌های پذیرش‌ها</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> DeleteReceptionsBulkAsync(List<int> ids);

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// معیارهای جستجوی پذیرش
    /// </summary>
    public class ReceptionSearchCriteria
    {
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public ReceptionStatus? Status { get; set; }
        public ReceptionType? Type { get; set; }
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "ReceptionDate";
        public string SortDirection { get; set; } = "DESC";
    }

    /// <summary>
    /// مدل نمایش فیلدهای محدود پذیرش
    /// </summary>
    public class ReceptionProjectionViewModel
    {
        public int ReceptionId { get; set; }
        public string ReceptionNumber { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime ReceptionDate { get; set; }
        public ReceptionStatus Status { get; set; }
        public ReceptionType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PatientCoPay { get; set; }
        public decimal InsurerShareAmount { get; set; }
    }

    /// <summary>
    /// اطلاعات استفاده از حافظه
    /// </summary>
    public class MemoryUsageInfo
    {
        public long TotalMemory { get; set; }
        public long UsedMemory { get; set; }
        public long AvailableMemory { get; set; }
        public double MemoryUsagePercentage { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// اطلاعات عملکرد
    /// </summary>
    public class PerformanceInfo
    {
        public TimeSpan AverageResponseTime { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// آمار کوئری‌ها
    /// </summary>
    public class QueryStatistics
    {
        public int TotalQueries { get; set; }
        public int CompiledQueries { get; set; }
        public int RegularQueries { get; set; }
        public TimeSpan AverageQueryTime { get; set; }
        public TimeSpan TotalQueryTime { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// آمار Cache
    /// </summary>
    public class CacheStatistics
    {
        public int TotalCacheEntries { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public double CacheHitRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
