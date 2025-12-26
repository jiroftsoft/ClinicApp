using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment
{
    /// <summary>
    /// سرویس محاسبه و مدیریت متریک‌های عملکرد منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه خودکار متریک‌های روزانه
    /// 2. ذخیره متریک‌ها در دیتابیس
    /// 3. دریافت متریک‌های ذخیره شده
    /// 4. شناسایی بهترین عملکردها
    /// 5. پشتیبانی از Scheduled Jobs
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierPerformanceService : ICashierPerformanceService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public CashierPerformanceService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region CalculateDailyMetricsAsync

        public async Task<ServiceResult<CashierPerformanceMetrics>> CalculateDailyMetricsAsync(string cashierId, DateTime date)
        {
            try
            {
                _logger.Information("📊 Calculating daily metrics for Cashier: {CashierId}, Date: {Date}", cashierId, date);

                if (string.IsNullOrEmpty(cashierId))
                {
                    return ServiceResult<CashierPerformanceMetrics>.Failed("شناسه منشی نامعتبر است.", "VALIDATION");
                }

                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                // دریافت جلسات صندوق
                var sessions = await _context.CashSessions
                    .Include(cs => cs.Transactions)
                    .Where(cs => cs.UserId == cashierId &&
                                 cs.OpenedAt >= startOfDay &&
                                 cs.OpenedAt < endOfDay &&
                                 !cs.IsDeleted)
                    .ToListAsync();

                // دریافت تراکنش‌ها
                var allTransactions = sessions
                    .SelectMany(s => s.Transactions ?? new List<PaymentTransaction>())
                    .Where(t => !t.IsDeleted)
                    .ToList();

                // دریافت اختلاف‌ها
                var sessionIds = sessions.Select(s => s.CashSessionId).ToList();
                var discrepancies = await _context.PaymentDiscrepancies
                    .Where(d => sessionIds.Contains(d.CashSessionId))
                    .ToListAsync();

                // محاسبه متریک‌ها
                var metrics = new CashierPerformanceMetrics
                {
                    CashierId = cashierId,
                    Date = startOfDay,
                    CreatedAt = DateTime.Now
                };

                // Transaction Metrics
                metrics.TotalTransactions = allTransactions.Count;
                metrics.PosTransactions = allTransactions.Count(t => t.Method == PaymentMethod.POS);
                metrics.CashTransactions = allTransactions.Count(t => t.Method == PaymentMethod.Cash);
                metrics.TotalAmount = allTransactions.Sum(t => t.Amount);
                metrics.PosAmount = allTransactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount);
                metrics.CashAmount = allTransactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount);

                // Performance Metrics
                metrics.SuccessfulTransactions = allTransactions.Count(t => t.Status == PaymentStatus.Success);
                metrics.FailedTransactions = allTransactions.Count(t => t.Status == PaymentStatus.Failed || t.Status == PaymentStatus.Canceled);
                metrics.SuccessRate = metrics.TotalTransactions > 0
                    ? (decimal)(metrics.SuccessfulTransactions * 100.0 / metrics.TotalTransactions)
                    : 0;

                // محاسبه زمان میانگین تراکنش (از زمان ایجاد Reception تا زمان Finalize)
                // برای سادگی، از زمان ایجاد تراکنش تا زمان به‌روزرسانی استفاده می‌کنیم
                var transactionsWithTime = allTransactions
                    .Where(t => t.UpdatedAt.HasValue && t.CreatedAt != default(DateTime))
                    .ToList();

                if (transactionsWithTime.Any())
                {
                    var totalSeconds = transactionsWithTime
                        .Sum(t => (t.UpdatedAt.Value - t.CreatedAt).TotalSeconds);
                    metrics.AverageTransactionTime = (decimal)(totalSeconds / (double)transactionsWithTime.Count);
                }

                // Discrepancy Metrics
                metrics.DiscrepancyCount = discrepancies.Count;
                metrics.TotalDiscrepancy = discrepancies.Sum(d => Math.Abs(d.Difference));

                // Session Metrics
                metrics.SessionsOpened = sessions.Count;
                metrics.SessionsClosed = sessions.Count(s => s.ClosedAt.HasValue);

                // محاسبه مدت زمان میانگین جلسات
                var closedSessions = sessions.Where(s => s.ClosedAt.HasValue).ToList();
                if (closedSessions.Any())
                {
                    var totalDuration = closedSessions
                        .Sum(s => (s.ClosedAt.Value - s.OpenedAt).TotalSeconds);
                    var avgSeconds = totalDuration / (double)closedSessions.Count;
                    metrics.AverageSessionDuration = TimeSpan.FromSeconds(avgSeconds);
                }

                // بررسی وجود رکورد قبلی
                var existingMetrics = await _context.CashierPerformanceMetrics
                    .FirstOrDefaultAsync(m => m.CashierId == cashierId && m.Date == startOfDay);

                if (existingMetrics != null)
                {
                    // به‌روزرسانی رکورد موجود
                    existingMetrics.TotalTransactions = metrics.TotalTransactions;
                    existingMetrics.PosTransactions = metrics.PosTransactions;
                    existingMetrics.CashTransactions = metrics.CashTransactions;
                    existingMetrics.TotalAmount = metrics.TotalAmount;
                    existingMetrics.PosAmount = metrics.PosAmount;
                    existingMetrics.CashAmount = metrics.CashAmount;
                    existingMetrics.AverageTransactionTime = metrics.AverageTransactionTime;
                    existingMetrics.SuccessfulTransactions = metrics.SuccessfulTransactions;
                    existingMetrics.FailedTransactions = metrics.FailedTransactions;
                    existingMetrics.SuccessRate = metrics.SuccessRate;
                    existingMetrics.DiscrepancyCount = metrics.DiscrepancyCount;
                    existingMetrics.TotalDiscrepancy = metrics.TotalDiscrepancy;
                    existingMetrics.SessionsOpened = metrics.SessionsOpened;
                    existingMetrics.SessionsClosed = metrics.SessionsClosed;
                    existingMetrics.AverageSessionDuration = metrics.AverageSessionDuration;
                    existingMetrics.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    _logger.Information("✅ Daily metrics updated for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                    return ServiceResult<CashierPerformanceMetrics>.Successful(existingMetrics);
                }
                else
                {
                    // ایجاد رکورد جدید
                    _context.CashierPerformanceMetrics.Add(metrics);
                    await _context.SaveChangesAsync();

                    _logger.Information("✅ Daily metrics calculated and saved for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                    return ServiceResult<CashierPerformanceMetrics>.Successful(metrics);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error calculating daily metrics for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                return ServiceResult<CashierPerformanceMetrics>.Failed("خطا در محاسبه متریک‌های روزانه منشی.", "EXCEPTION");
            }
        }

        #endregion

        #region CalculateAllCashiersDailyMetricsAsync

        public async Task<ServiceResult<int>> CalculateAllCashiersDailyMetricsAsync(DateTime date)
        {
            try
            {
                _logger.Information("📊 Calculating daily metrics for all cashiers, Date: {Date}", date);

                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                // دریافت تمام منشی‌هایی که در این تاریخ جلسه داشته‌اند
                var cashierIds = await _context.CashSessions
                    .Where(cs => cs.OpenedAt >= startOfDay &&
                                 cs.OpenedAt < endOfDay &&
                                 !cs.IsDeleted)
                    .Select(cs => cs.UserId)
                    .Distinct()
                    .ToListAsync();

                int successCount = 0;
                int failCount = 0;

                foreach (var cashierId in cashierIds)
                {
                    var result = await CalculateDailyMetricsAsync(cashierId, date);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        _logger.Warning("⚠️ Failed to calculate metrics for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                    }
                }

                _logger.Information("✅ Calculated metrics for {SuccessCount} cashiers, Failed: {FailCount}, Date: {Date}", 
                    successCount, failCount, date);

                return ServiceResult<int>.Successful(successCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error calculating daily metrics for all cashiers, Date: {Date}", date);
                return ServiceResult<int>.Failed("خطا در محاسبه متریک‌های روزانه تمام منشی‌ها.", "EXCEPTION");
            }
        }

        #endregion

        #region GetMetricsAsync

        public async Task<ServiceResult<CashierPerformanceMetrics>> GetMetricsAsync(string cashierId, DateTime date)
        {
            try
            {
                _logger.Information("📊 Getting metrics for Cashier: {CashierId}, Date: {Date}", cashierId, date);

                if (string.IsNullOrEmpty(cashierId))
                {
                    return ServiceResult<CashierPerformanceMetrics>.Failed("شناسه منشی نامعتبر است.", "VALIDATION");
                }

                var startOfDay = date.Date;

                var metrics = await _context.CashierPerformanceMetrics
                    .Include(m => m.Cashier)
                    .FirstOrDefaultAsync(m => m.CashierId == cashierId && m.Date == startOfDay);

                if (metrics == null)
                {
                    // اگر متریک وجود ندارد، محاسبه کن
                    _logger.Information("📊 Metrics not found, calculating for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                    return await CalculateDailyMetricsAsync(cashierId, date);
                }

                return ServiceResult<CashierPerformanceMetrics>.Successful(metrics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting metrics for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                return ServiceResult<CashierPerformanceMetrics>.Failed("خطا در دریافت متریک‌های منشی.", "EXCEPTION");
            }
        }

        #endregion

        #region GetMetricsRangeAsync

        public async Task<ServiceResult<List<CashierPerformanceMetrics>>> GetMetricsRangeAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Getting metrics range for Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                if (string.IsNullOrEmpty(cashierId))
                {
                    return ServiceResult<List<CashierPerformanceMetrics>>.Failed("شناسه منشی نامعتبر است.", "VALIDATION");
                }

                var startOfDay = fromDate.Date;
                var endOfDay = toDate.Date.AddDays(1);

                var metrics = await _context.CashierPerformanceMetrics
                    .Include(m => m.Cashier)
                    .Where(m => m.CashierId == cashierId &&
                                m.Date >= startOfDay &&
                                m.Date < endOfDay)
                    .OrderBy(m => m.Date)
                    .ToListAsync();

                return ServiceResult<List<CashierPerformanceMetrics>>.Successful(metrics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting metrics range for Cashier: {CashierId}", cashierId);
                return ServiceResult<List<CashierPerformanceMetrics>>.Failed("خطا در دریافت متریک‌های بازه زمانی منشی.", "EXCEPTION");
            }
        }

        #endregion

        #region GetTopPerformersAsync

        public async Task<ServiceResult<List<CashierRanking>>> GetTopPerformersAsync(DateTime fromDate, DateTime toDate, int topN = 10, string sortBy = "TotalTransactions")
        {
            try
            {
                _logger.Information("🏆 Getting top {TopN} performers, From: {FromDate}, To: {ToDate}, SortBy: {SortBy}", 
                    topN, fromDate, toDate, sortBy);

                var startOfDay = fromDate.Date;
                var endOfDay = toDate.Date.AddDays(1);

                // دریافت متریک‌ها در بازه زمانی
                var metrics = await _context.CashierPerformanceMetrics
                    .Include(m => m.Cashier)
                    .Where(m => m.Date >= startOfDay && m.Date < endOfDay)
                    .ToListAsync();

                // گروه‌بندی بر اساس منشی و محاسبه مجموع
                var aggregatedMetrics = metrics
                    .GroupBy(m => m.CashierId)
                    .Select(g => new
                    {
                        CashierId = g.Key,
                        CashierName = g.First().Cashier?.UserName ?? g.First().Cashier?.Email ?? "نامشخص",
                        TotalTransactions = g.Sum(m => m.TotalTransactions),
                        TotalAmount = g.Sum(m => m.TotalAmount),
                        SuccessRate = g.Average(m => m.SuccessRate),
                        DiscrepancyCount = g.Sum(m => m.DiscrepancyCount),
                        TotalDiscrepancyAmount = g.Sum(m => m.TotalDiscrepancy),
                        AverageTransactionTime = g.Average(m => m.AverageTransactionTime),
                        SessionsCount = g.Sum(m => m.SessionsOpened)
                    })
                    .ToList();

                // مرتب‌سازی
                var orderedMetrics = sortBy.ToLower() switch
                {
                    "totalamount" => aggregatedMetrics.OrderByDescending(m => m.TotalAmount),
                    "successrate" => aggregatedMetrics.OrderByDescending(m => m.SuccessRate),
                    "totaltransactions" => aggregatedMetrics.OrderByDescending(m => m.TotalTransactions),
                    _ => aggregatedMetrics.OrderByDescending(m => m.TotalTransactions)
                };

                // تبدیل به CashierRanking
                var rankings = orderedMetrics
                    .Take(topN)
                    .Select((m, index) => new CashierRanking
                    {
                        CashierId = m.CashierId,
                        CashierName = m.CashierName,
                        Rank = index + 1,
                        TotalTransactions = m.TotalTransactions,
                        TotalAmount = m.TotalAmount,
                        SuccessRate = m.SuccessRate,
                        DiscrepancyCount = m.DiscrepancyCount,
                        TotalDiscrepancyAmount = m.TotalDiscrepancyAmount,
                        AverageTransactionTime = m.AverageTransactionTime,
                        SessionsCount = m.SessionsCount,
                        OverallScore = CalculateOverallScore(m.TotalTransactions, m.SuccessRate, m.DiscrepancyCount)
                    })
                    .ToList();

                _logger.Information("✅ Retrieved {Count} top performers", rankings.Count);

                return ServiceResult<List<CashierRanking>>.Successful(rankings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting top performers");
                return ServiceResult<List<CashierRanking>>.Failed("خطا در دریافت منشی‌های برتر.", "EXCEPTION");
            }
        }

        #endregion

        #region GetCashierRankingAsync

        public async Task<ServiceResult<CashierRanking>> GetCashierRankingAsync(string cashierId, DateTime fromDate, DateTime toDate, string sortBy = "TotalTransactions")
        {
            try
            {
                _logger.Information("📊 Getting ranking for Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                // دریافت تمام منشی‌های برتر
                var topPerformersResult = await GetTopPerformersAsync(fromDate, toDate, 1000, sortBy);
                if (!topPerformersResult.Success)
                {
                    return ServiceResult<CashierRanking>.Failed(topPerformersResult.Message, topPerformersResult.Code);
                }

                // پیدا کردن رتبه منشی مورد نظر
                var ranking = topPerformersResult.Data
                    .FirstOrDefault(r => r.CashierId == cashierId);

                if (ranking == null)
                {
                    return ServiceResult<CashierRanking>.Failed("منشی در لیست رتبه‌بندی یافت نشد.", "NOT_FOUND");
                }

                return ServiceResult<CashierRanking>.Successful(ranking);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting ranking for Cashier: {CashierId}", cashierId);
                return ServiceResult<CashierRanking>.Failed("خطا در دریافت رتبه منشی.", "EXCEPTION");
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// محاسبه امتیاز کلی منشی
        /// </summary>
        private decimal CalculateOverallScore(int totalTransactions, decimal successRate, int discrepancyCount)
        {
            // فرمول: (تعداد تراکنش * 0.4) + (نرخ موفقیت * 0.4) - (تعداد اختلاف * 10)
            var transactionScore = totalTransactions * 0.4m;
            var successScore = successRate * 0.4m;
            var discrepancyPenalty = discrepancyCount * 10m;

            return transactionScore + successScore - discrepancyPenalty;
        }

        #endregion
    }
}

