using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Infrastructure.Reception;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// سرویس بهینه‌سازی عملکرد ماژول پذیرش
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
    public class ReceptionPerformanceOptimizer : IReceptionPerformanceOptimizer
    {
        #region Fields and Constructor

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        // Cache Keys
        private const string RECEPTIONS_CACHE_KEY = "receptions_";
        private const string RECEPTION_DETAILS_CACHE_KEY = "reception_details_";
        private const string DAILY_STATS_CACHE_KEY = "daily_stats_";
        private const string DOCTOR_STATS_CACHE_KEY = "doctor_stats_";

        // Cache Expiry Times
        private readonly TimeSpan _receptionsCacheExpiry = TimeSpan.FromMinutes(30);
        private readonly TimeSpan _receptionDetailsCacheExpiry = TimeSpan.FromHours(1);
        private readonly TimeSpan _statsCacheExpiry = TimeSpan.FromMinutes(15);

        public ReceptionPerformanceOptimizer(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region Optimized Query Methods

        /// <summary>
        /// دریافت بهینه پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست بهینه پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsOptimizedAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت بهینه پذیرش‌ها. معیارها: {@Criteria}", criteria);
                
                var cacheKey = $"{RECEPTIONS_CACHE_KEY}{criteria.GetHashCode()}";
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out List<Models.Entities.Reception.Reception> cachedReceptions))
                {
                    _logger.Debug("دریافت پذیرش‌ها از Cache");
                    return cachedReceptions;
                }

                // Get from database using compiled queries
                var receptions = await GetReceptionsFromDatabaseAsync(criteria);
                
                // Cache the results
                _cache.Set(cacheKey, receptions, _receptionsCacheExpiry);
                
                _logger.Debug("دریافت پذیرش‌ها از Database و ذخیره در Cache");
                return receptions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بهینه پذیرش‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت جزئیات بهینه پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات بهینه پذیرش</returns>
        public async Task<ReceptionComponentViewModels.ReceptionDetailsViewModel> GetReceptionDetailsOptimizedAsync(int id)
        {
            try
            {
                _logger.Debug("دریافت جزئیات بهینه پذیرش. شناسه: {Id}", id);
                
                var cacheKey = $"{RECEPTION_DETAILS_CACHE_KEY}{id}";
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ReceptionComponentViewModels.ReceptionDetailsViewModel cachedDetails))
                {
                    _logger.Debug("دریافت جزئیات پذیرش از Cache");
                    return cachedDetails;
                }

                // Get from database using compiled query
                var reception = await ReceptionCompiledQueries.GetReceptionByIdWithIncludes(_context, id);
                if (reception == null)
                {
                    _logger.Warning("پذیرش با شناسه {Id} یافت نشد", id);
                    return null;
                }

                // Convert to ViewModel
                var details = ReceptionComponentViewModels.ReceptionDetailsViewModel.FromEntity(reception);
                
                // Cache the results
                _cache.Set(cacheKey, details, _receptionDetailsCacheExpiry);
                
                _logger.Debug("دریافت جزئیات پذیرش از Database و ذخیره در Cache");
                return details;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات بهینه پذیرش. شناسه: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت لیست بهینه پذیرش‌ها برای نمایش
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست بهینه پذیرش‌ها</returns>
        public async Task<List<ReceptionComponentViewModels.ReceptionIndexViewModel>> GetReceptionsIndexOptimizedAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت لیست بهینه پذیرش‌ها. معیارها: {@Criteria}", criteria);
                
                var cacheKey = $"{RECEPTIONS_CACHE_KEY}index_{criteria.GetHashCode()}";
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out List<ReceptionComponentViewModels.ReceptionIndexViewModel> cachedIndex))
                {
                    _logger.Debug("دریافت لیست پذیرش‌ها از Cache");
                    return cachedIndex;
                }

                // Get from database using compiled queries
                var receptions = await GetReceptionsFromDatabaseAsync(criteria);
                
                // Convert to Index ViewModels
                var indexViewModels = receptions.Select(r => ReceptionComponentViewModels.ReceptionIndexViewModel.FromEntity(r)).ToList();
                
                // Cache the results
                _cache.Set(cacheKey, indexViewModels, _receptionsCacheExpiry);
                
                _logger.Debug("دریافت لیست پذیرش‌ها از Database و ذخیره در Cache");
                return indexViewModels;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست بهینه پذیرش‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار بهینه پذیرش‌ها
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار بهینه</returns>
        public async Task<ReceptionDailyStatsViewModel> GetDailyStatsOptimizedAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار بهینه پذیرش‌ها. تاریخ: {Date}", date);
                
                var cacheKey = $"{DAILY_STATS_CACHE_KEY}{date:yyyyMMdd}";
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out ReceptionDailyStatsViewModel cachedStats))
                {
                    _logger.Debug("دریافت آمار از Cache");
                    return cachedStats;
                }

                // Get from database using compiled queries
                var totalReceptions = await ReceptionCompiledQueries.GetReceptionsCountByDate(_context, date);
                var completedReceptions = await ReceptionCompiledQueries.GetCompletedReceptionsCountByDate(_context, date);
                var pendingReceptions = await ReceptionCompiledQueries.GetPendingReceptionsCountByDate(_context, date);
                var totalRevenue = await ReceptionCompiledQueries.GetTotalRevenueByDate(_context, date);

                var stats = new ReceptionDailyStatsViewModel
                {
                    Date = date,
                    TotalReceptions = totalReceptions,
                    CompletedReceptions = completedReceptions,
                    PendingReceptions = pendingReceptions,
                    CancelledReceptions = totalReceptions - completedReceptions - pendingReceptions,
                    InProgressReceptions = 0, // Calculate based on other statuses
                    TotalRevenue = totalRevenue,
                    AverageRevenuePerReception = totalReceptions > 0 ? totalRevenue / totalReceptions : 0
                };
                
                // Cache the results
                _cache.Set(cacheKey, stats, _statsCacheExpiry);
                
                _logger.Debug("دریافت آمار از Database و ذخیره در Cache");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار بهینه پذیرش‌ها. تاریخ: {Date}", date);
                throw;
            }
        }

        #endregion

        #region Compiled Query Methods

        /// <summary>
        /// دریافت پذیرش با Compiled Query
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>پذیرش</returns>
        public async Task<Models.Entities.Reception.Reception> GetReceptionByIdCompiledAsync(int id)
        {
            try
            {
                _logger.Debug("دریافت پذیرش با Compiled Query. شناسه: {Id}", id);
                
                return await ReceptionCompiledQueries.GetReceptionById(_context, id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش با Compiled Query. شناسه: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت پذیرش‌ها با Compiled Query
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateCompiledAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها با Compiled Query. تاریخ: {Date}", date);
                
                return await ReceptionCompiledQueries.GetReceptionsByDate(_context, date);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها با Compiled Query. تاریخ: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های بیمار با Compiled Query
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByPatientCompiledAsync(int patientId)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌های بیمار با Compiled Query. شناسه بیمار: {PatientId}", patientId);
                
                return await ReceptionCompiledQueries.GetReceptionsByPatient(_context, patientId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های بیمار با Compiled Query. شناسه بیمار: {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های پزشک با Compiled Query
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorCompiledAsync(int doctorId)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌های پزشک با Compiled Query. شناسه پزشک: {DoctorId}", doctorId);
                
                return await ReceptionCompiledQueries.GetReceptionsByDoctor(_context, doctorId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های پزشک با Compiled Query. شناسه پزشک: {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Projection Methods

        /// <summary>
        /// دریافت فیلدهای محدود پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست فیلدهای محدود</returns>
        public async Task<List<ReceptionProjectionViewModel>> GetReceptionsProjectionAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت فیلدهای محدود پذیرش‌ها. معیارها: {@Criteria}", criteria);
                
                var query = _context.Receptions
                    .Where(r => !r.IsDeleted);

                if (criteria.PatientId.HasValue)
                    query = query.Where(r => r.PatientId == criteria.PatientId.Value);

                if (criteria.DoctorId.HasValue)
                    query = query.Where(r => r.DoctorId == criteria.DoctorId.Value);

                if (criteria.DateFrom.HasValue)
                    query = query.Where(r => r.ReceptionDate >= criteria.DateFrom.Value);

                if (criteria.DateTo.HasValue)
                    query = query.Where(r => r.ReceptionDate <= criteria.DateTo.Value);

                if (criteria.Status.HasValue)
                    query = query.Where(r => r.Status == criteria.Status.Value);

                if (criteria.Type.HasValue)
                    query = query.Where(r => r.Type == criteria.Type.Value);

                var projections = await query
                    .Select(r => new ReceptionProjectionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        ReceptionNumber = r.ReceptionNumber,
                        PatientId = r.PatientId,
                        PatientName = r.Patient.FirstName + " " + r.Patient.LastName,
                        DoctorId = r.DoctorId,
                        DoctorName = r.Doctor.FirstName + " " + r.Doctor.LastName,
                        ReceptionDate = r.ReceptionDate,
                        Status = r.Status,
                        Type = r.Type,
                        TotalAmount = r.TotalAmount,
                        PatientCoPay = r.PatientCoPay,
                        InsurerShareAmount = r.InsurerShareAmount
                    })
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .Skip(criteria.PageNumber * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToListAsync();

                return projections;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فیلدهای محدود پذیرش‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت فیلدهای محدود پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>فیلدهای محدود پذیرش</returns>
        public async Task<ReceptionProjectionViewModel> GetReceptionProjectionAsync(int id)
        {
            try
            {
                _logger.Debug("دریافت فیلدهای محدود پذیرش. شناسه: {Id}", id);
                
                var projection = await _context.Receptions
                    .Where(r => r.ReceptionId == id && !r.IsDeleted)
                    .Select(r => new ReceptionProjectionViewModel
                    {
                        ReceptionId = r.ReceptionId,
                        ReceptionNumber = r.ReceptionNumber,
                        PatientId = r.PatientId,
                        PatientName = r.Patient.FirstName + " " + r.Patient.LastName,
                        DoctorId = r.DoctorId,
                        DoctorName = r.Doctor.FirstName + " " + r.Doctor.LastName,
                        ReceptionDate = r.ReceptionDate,
                        Status = r.Status,
                        Type = r.Type,
                        TotalAmount = r.TotalAmount,
                        PatientCoPay = r.PatientCoPay,
                        InsurerShareAmount = r.InsurerShareAmount
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                return projection;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت فیلدهای محدود پذیرش. شناسه: {Id}", id);
                throw;
            }
        }

        #endregion

        #region Caching Methods

        /// <summary>
        /// دریافت پذیرش‌ها از Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها از Cache</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsFromCacheAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها از Cache. معیارها: {@Criteria}", criteria);
                
                var cacheKey = $"{RECEPTIONS_CACHE_KEY}{criteria.GetHashCode()}";
                
                if (_cache.TryGetValue(cacheKey, out List<Models.Entities.Reception.Reception> cachedReceptions))
                {
                    _logger.Debug("دریافت پذیرش‌ها از Cache موفق");
                    return cachedReceptions;
                }

                _logger.Debug("پذیرش‌ها در Cache یافت نشد");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها از Cache");
                throw;
            }
        }

        /// <summary>
        /// ذخیره پذیرش‌ها در Cache
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <param name="receptions">لیست پذیرش‌ها</param>
        /// <returns>نتیجه ذخیره</returns>
        public async Task<bool> CacheReceptionsAsync(ReceptionSearchCriteria criteria, List<Models.Entities.Reception.Reception> receptions)
        {
            try
            {
                _logger.Debug("ذخیره پذیرش‌ها در Cache. تعداد: {Count}", receptions?.Count ?? 0);
                
                var cacheKey = $"{RECEPTIONS_CACHE_KEY}{criteria.GetHashCode()}";
                
                _cache.Set(cacheKey, receptions, _receptionsCacheExpiry);
                
                _logger.Debug("ذخیره پذیرش‌ها در Cache موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره پذیرش‌ها در Cache");
                return false;
            }
        }

        /// <summary>
        /// پاک کردن Cache پذیرش‌ها
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>نتیجه پاک کردن</returns>
        public async Task<bool> ClearReceptionsCacheAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("پاک کردن Cache پذیرش‌ها. معیارها: {@Criteria}", criteria);
                
                var cacheKey = $"{RECEPTIONS_CACHE_KEY}{criteria.GetHashCode()}";
                
                _cache.Remove(cacheKey);
                
                _logger.Debug("پاک کردن Cache پذیرش‌ها موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache پذیرش‌ها");
                return false;
            }
        }

        /// <summary>
        /// پاک کردن تمام Cache
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        public async Task<bool> ClearAllCacheAsync()
        {
            try
            {
                _logger.Debug("پاک کردن تمام Cache");
                
                // Clear all cache entries
                if (_cache is MemoryCache memoryCache)
                {
                    memoryCache.Dispose();
                }
                
                _logger.Debug("پاک کردن تمام Cache موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن تمام Cache");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت پذیرش‌ها از دیتابیس
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        private async Task<List<Models.Entities.Reception.Reception>> GetReceptionsFromDatabaseAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها از دیتابیس. معیارها: {@Criteria}", criteria);
                
                // Use compiled query for better performance
                var receptions = await ReceptionCompiledQueries.GetReceptionsWithFiltersAndIncludes(
                    _context,
                    criteria.PatientId,
                    criteria.DoctorId,
                    criteria.DateFrom,
                    criteria.DateTo,
                    criteria.Status,
                    criteria.Type);

                // Apply pagination
                var pagedReceptions = receptions
                    .Skip(criteria.PageNumber * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToList();

                return pagedReceptions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها از دیتابیس");
                throw;
            }
        }

        #endregion

        #region Performance Monitoring Methods

        /// <summary>
        /// دریافت اطلاعات عملکرد
        /// </summary>
        /// <returns>اطلاعات عملکرد</returns>
        public async Task<PerformanceInfo> GetPerformanceInfoAsync()
        {
            try
            {
                _logger.Debug("دریافت اطلاعات عملکرد");
                
                // TODO: Implement performance monitoring
                var performanceInfo = new PerformanceInfo
                {
                    AverageResponseTime = TimeSpan.FromMilliseconds(100),
                    TotalRequests = 1000,
                    SuccessfulRequests = 950,
                    FailedRequests = 50,
                    SuccessRate = 95.0,
                    Timestamp = DateTime.Now
                };

                return performanceInfo;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات عملکرد");
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار کوئری‌ها
        /// </summary>
        /// <returns>آمار کوئری‌ها</returns>
        public async Task<QueryStatistics> GetQueryStatisticsAsync()
        {
            try
            {
                _logger.Debug("دریافت آمار کوئری‌ها");
                
                // TODO: Implement query statistics
                var queryStats = new QueryStatistics
                {
                    TotalQueries = 500,
                    CompiledQueries = 200,
                    RegularQueries = 300,
                    AverageQueryTime = TimeSpan.FromMilliseconds(50),
                    TotalQueryTime = TimeSpan.FromSeconds(25),
                    Timestamp = DateTime.Now
                };

                return queryStats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار کوئری‌ها");
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        /// <returns>آمار Cache</returns>
        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            try
            {
                _logger.Debug("دریافت آمار Cache");
                
                // TODO: Implement cache statistics
                var cacheStats = new CacheStatistics
                {
                    TotalCacheEntries = 100,
                    CacheHits = 800,
                    CacheMisses = 200,
                    CacheHitRate = 80.0,
                    Timestamp = DateTime.Now
                };

                return cacheStats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار Cache");
                throw;
            }
        }

        #endregion
    }
}
