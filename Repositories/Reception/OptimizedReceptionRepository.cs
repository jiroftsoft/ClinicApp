using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Base;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Repositories.Base;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Repositories.Reception
{
    /// <summary>
    /// Repository بهینه‌سازی شده برای مدیریت پذیرش‌های بیماران
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. استفاده از Compiled Queries برای عملکرد بهتر
    /// 2. بهینه‌سازی کوئری‌های دیتابیس
    /// 3. مدیریت Cache
    /// 4. Parallel Processing
    /// 5. Memory Optimization
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط دسترسی به داده پذیرش
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class OptimizedReceptionRepository : BaseRepository<Models.Entities.Reception.Reception>, IReceptionRepository
    {
        #region Fields and Constructor

        private readonly ICurrentUserService _currentUserService;
        private readonly IReceptionCacheService _cacheService;
        private readonly IReceptionPerformanceOptimizer _performanceOptimizer;

        // Compiled Queries for Performance
        private static readonly Func<ApplicationDbContext, int, Task<Models.Entities.Reception.Reception>> GetReceptionByIdCompiled =
            EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
                context.Receptions.FirstOrDefault(r => r.ReceptionId == id && !r.IsDeleted));

        private static readonly Func<ApplicationDbContext, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDateCompiled =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        private static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByPatientCompiled =
            EF.CompileAsyncQuery((ApplicationDbContext context, int patientId) =>
                context.Receptions.Where(r => r.PatientId == patientId && !r.IsDeleted));

        private static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDoctorCompiled =
            EF.CompileAsyncQuery((ApplicationDbContext context, int doctorId) =>
                context.Receptions.Where(r => r.DoctorId == doctorId && !r.IsDeleted));

        public OptimizedReceptionRepository(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService,
            IReceptionCacheService cacheService,
            IReceptionPerformanceOptimizer performanceOptimizer)
            : base(context, logger)
        {
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _performanceOptimizer = performanceOptimizer ?? throw new ArgumentNullException(nameof(performanceOptimizer));
        }

        #endregion

        #region Override Base Methods

        /// <summary>
        /// دریافت شناسه Entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>شناسه</returns>
        protected override int GetEntityId(Models.Entities.Reception.Reception entity)
        {
            return entity.ReceptionId;
        }

        #endregion

        #region Optimized CRUD Operations

        /// <summary>
        /// دریافت پذیرش با Compiled Query
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>پذیرش</returns>
        public async Task<Models.Entities.Reception.Reception> GetByIdCompiledAsync(int id)
        {
            try
            {
                _logger.Debug("دریافت پذیرش با Compiled Query. شناسه: {Id}", id);
                
                return await GetReceptionByIdCompiled(_context, id);
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
        public async Task<List<Models.Entities.Reception.Reception>> GetByDateCompiledAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها با Compiled Query. تاریخ: {Date}", date);
                
                return await GetReceptionsByDateCompiled(_context, date);
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
        public async Task<List<Models.Entities.Reception.Reception>> GetByPatientCompiledAsync(int patientId)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌های بیمار با Compiled Query. شناسه بیمار: {PatientId}", patientId);
                
                return await GetReceptionsByPatientCompiled(_context, patientId);
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
        public async Task<List<Models.Entities.Reception.Reception>> GetByDoctorCompiledAsync(int doctorId)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌های پزشک با Compiled Query. شناسه پزشک: {DoctorId}", doctorId);
                
                return await GetReceptionsByDoctorCompiled(_context, doctorId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های پزشک با Compiled Query. شناسه پزشک: {DoctorId}", doctorId);
                throw;
            }
        }

        #endregion

        #region Advanced Query Methods

        /// <summary>
        /// دریافت پذیرش‌ها با معیارهای جستجو
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetByCriteriaAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها با معیارهای جستجو. معیارها: {@Criteria}", criteria);
                
                var query = _dbSet.Where(r => !r.IsDeleted);

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

                // Apply sorting
                switch (criteria.SortBy?.ToLower())
                {
                    case "receptiondate":
                        query = criteria.SortDirection?.ToLower() == "asc" 
                            ? query.OrderBy(r => r.ReceptionDate)
                            : query.OrderByDescending(r => r.ReceptionDate);
                        break;
                    case "totalamount":
                        query = criteria.SortDirection?.ToLower() == "asc"
                            ? query.OrderBy(r => r.TotalAmount)
                            : query.OrderByDescending(r => r.TotalAmount);
                        break;
                    default:
                        query = query.OrderByDescending(r => r.ReceptionDate);
                        break;
                }

                return await query
                    .AsNoTracking()
                    .Skip(criteria.PageNumber * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها با معیارهای جستجو");
                throw;
            }
        }

        /// <summary>
        /// دریافت پذیرش‌ها با Include بهینه
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetWithIncludesAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها با Include بهینه. معیارها: {@Criteria}", criteria);
                
                var query = _dbSet
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ReceptionItems)
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

                return await query
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .Skip(criteria.PageNumber * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها با Include بهینه");
                throw;
            }
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// دریافت آمار روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        public async Task<ReceptionDailyStatsViewModel> GetDailyStatsAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار روزانه. تاریخ: {Date}", date);
                
                var receptions = await GetByDateCompiledAsync(date);
                
                var stats = new ReceptionDailyStatsViewModel
                {
                    Date = date,
                    TotalReceptions = receptions.Count,
                    CompletedReceptions = receptions.Count(r => r.Status == ReceptionStatus.Completed),
                    PendingReceptions = receptions.Count(r => r.Status == ReceptionStatus.Pending),
                    CancelledReceptions = receptions.Count(r => r.Status == ReceptionStatus.Cancelled),
                    InProgressReceptions = receptions.Count(r => r.Status == ReceptionStatus.InProgress),
                    TotalRevenue = receptions.Sum(r => r.TotalAmount),
                    AverageRevenuePerReception = receptions.Count > 0 ? receptions.Sum(r => r.TotalAmount) / receptions.Count : 0
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار روزانه. تاریخ: {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار پزشکان
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار پزشکان</returns>
        public async Task<List<ReceptionDoctorStatsViewModel>> GetDoctorStatsAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار پزشکان. تاریخ: {Date}", date);
                
                var receptions = await GetByDateCompiledAsync(date);
                
                var doctorStats = receptions
                    .GroupBy(r => r.DoctorId)
                    .Select(g => new ReceptionDoctorStatsViewModel
                    {
                        DoctorId = g.Key,
                        DoctorName = g.First().Doctor?.FullName ?? "نامشخص",
                        TotalReceptions = g.Count(),
                        CompletedReceptions = g.Count(r => r.Status == ReceptionStatus.Completed),
                        TotalRevenue = g.Sum(r => r.TotalAmount),
                        AverageRevenuePerReception = g.Count() > 0 ? g.Sum(r => r.TotalAmount) / g.Count() : 0
                    })
                    .OrderByDescending(s => s.TotalReceptions)
                    .ToList();

                return doctorStats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشکان. تاریخ: {Date}", date);
                throw;
            }
        }

        #endregion

        #region Validation Methods

        public Task<Models.Entities.Reception.Reception> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Models.Entities.Reception.Reception> GetByIdWithDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> GetAllActiveAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> GetByPatientIdAsync(int patientId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> GetByDoctorIdAsync(int doctorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> GetByDateAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<List<Models.Entities.Reception.Reception>> SearchByNationalCodeAsync(string nationalCode)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> SearchByPatientNameAsync(string patientName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> GetByStatusAsync(ReceptionStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> GetByTypeAsync(ReceptionType type)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> GetEmergencyReceptionsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPagedAsync(int? patientId, int? doctorId, ReceptionStatus? status, string searchTerm, int pageNumber,
            int pageSize)
        {
            throw new NotImplementedException();
        }

        public void Add(Reception reception)
        {
            throw new NotImplementedException();
        }

        public void Update(Reception reception)
        {
            throw new NotImplementedException();
        }

        public void Delete(Reception reception)
        {
            throw new NotImplementedException();
        }

        public DbContextTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// بررسی وجود پذیرش فعال
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="date">تاریخ</param>
        /// <returns>نتیجه بررسی</returns>
        public async Task<bool> HasActiveReceptionAsync(int patientId, DateTime date)
        {
            try
            {
                _logger.Debug("بررسی وجود پذیرش فعال. شناسه بیمار: {PatientId}, تاریخ: {Date}", patientId, date);
                
                return await _dbSet
                    .AnyAsync(r => r.PatientId == patientId && 
                                  r.ReceptionDate.Date == date.Date && 
                                  r.Status == ReceptionStatus.Pending &&
                                  !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود پذیرش فعال. شناسه بیمار: {PatientId}, تاریخ: {Date}", patientId, date);
                throw;
            }
        }

        public Task<Reception> GetLatestByPatientIdAsync(int patientId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetDailyCountAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetDoctorDailyCountAsync(int doctorId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> GetReceptionsByDateAsync(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<List<Reception>> GetReceptionsByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            throw new NotImplementedException();
        }

        public Task<Reception> UpdateReceptionAsync(Reception reception)
        {
            throw new NotImplementedException();
        }

        public Task<List<PaymentTransaction>> GetReceptionPaymentsAsync(int receptionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// دریافت تعداد پذیرش‌های پزشک در تاریخ
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>تعداد پذیرش‌ها</returns>
        public async Task<int> GetReceptionCountByDoctorAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Debug("دریافت تعداد پذیرش‌های پزشک. شناسه پزشک: {DoctorId}, تاریخ: {Date}", doctorId, date);
                
                return await _dbSet
                    .CountAsync(r => r.DoctorId == doctorId && 
                                   r.ReceptionDate.Date == date.Date && 
                                   !r.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد پذیرش‌های پزشک. شناسه پزشک: {DoctorId}, تاریخ: {Date}", doctorId, date);
                throw;
            }
        }

        #endregion

        #region Cache Integration Methods

        /// <summary>
        /// دریافت پذیرش‌ها از Cache یا Database
        /// </summary>
        /// <param name="criteria">معیارهای جستجو</param>
        /// <returns>لیست پذیرش‌ها</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsWithCacheAsync(ReceptionSearchCriteria criteria)
        {
            try
            {
                _logger.Debug("دریافت پذیرش‌ها از Cache یا Database. معیارها: {@Criteria}", criteria);
                
                // Try to get from cache first
                var cachedReceptions = await _cacheService.GetReceptionsFromCacheAsync(criteria);
                if (cachedReceptions != null && cachedReceptions.Any())
                {
                    _logger.Debug("دریافت پذیرش‌ها از Cache");
                    return cachedReceptions.Cast<Models.Entities.Reception.Reception>().ToList();
                }

                // Get from database
                var receptions = await GetByCriteriaAsync(criteria);
                
                // Cache the results
                await _cacheService.CacheReceptionsAsync(criteria, receptions);
                
                _logger.Debug("دریافت پذیرش‌ها از Database و ذخیره در Cache");
                return receptions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها از Cache یا Database");
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
                
                return await _performanceOptimizer.GetPerformanceInfoAsync();
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
                
                return await _performanceOptimizer.GetQueryStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار کوئری‌ها");
                throw;
            }
        }

        #endregion
    }
}
