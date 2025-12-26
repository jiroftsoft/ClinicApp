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
    /// سرویس گزارش‌گیری از عملکرد منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. گزارش روزانه عملکرد منشی
    /// 2. گزارش ماهانه عملکرد منشی
    /// 3. خلاصه عملکرد تمام منشی‌ها
    /// 4. مقایسه عملکرد منشی‌ها
    /// 5. Export به Excel و PDF (در فاز بعدی)
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierReportService : ICashierReportService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public CashierReportService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region GetDailyReportAsync

        public async Task<ServiceResult<CashierDailyReport>> GetDailyReportAsync(string cashierId, DateTime date)
        {
            try
            {
                _logger.Information("📊 Getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, date);

                if (string.IsNullOrWhiteSpace(cashierId))
                {
                    return ServiceResult<CashierDailyReport>.Failed("شناسه منشی الزامی است.", "VALIDATION");
                }

                var startOfDay = date.Date;
                var endOfDay = startOfDay.AddDays(1);

                // Get Cashier Info
                var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
                if (cashier == null)
                {
                    return ServiceResult<CashierDailyReport>.Failed("منشی یافت نشد.", "NOT_FOUND");
                }

                // Get Sessions
                var sessions = await _context.CashSessions
                    .Include(cs => cs.Transactions)
                    .Include(cs => cs.User)
                    .Where(cs => cs.UserId == cashierId &&
                                 cs.OpenedAt >= startOfDay &&
                                 cs.OpenedAt < endOfDay &&
                                 !cs.IsDeleted)
                    .OrderBy(cs => cs.OpenedAt)
                    .ToListAsync();

                // Get Transactions
                var transactions = sessions
                    .SelectMany(s => s.Transactions)
                    .Where(t => !t.IsDeleted)
                    .ToList();

                // Get Discrepancies
                var discrepancies = await _context.PaymentDiscrepancies
                    .Include(d => d.CashSession)
                    .Where(d => d.CashSession.UserId == cashierId &&
                                d.ReportedAt >= startOfDay &&
                                d.ReportedAt < endOfDay)
                    .ToListAsync();

                // Build Report
                var report = new CashierDailyReport
                {
                    CashierId = cashierId,
                    CashierName = cashier.UserName ?? cashier.Email ?? "نامشخص",
                    Date = date,
                    SessionsOpened = sessions.Count,
                    SessionsClosed = sessions.Count(s => s.Status == CashSessionStatus.Closed),
                    TotalTransactions = transactions.Count,
                    PosTransactions = transactions.Count(t => t.Method == PaymentMethod.POS),
                    CashTransactions = transactions.Count(t => t.Method == PaymentMethod.Cash),
                    TotalAmount = transactions.Sum(t => t.Amount),
                    PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS).Sum(t => t.Amount),
                    CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash).Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    DiscrepancyCount = discrepancies.Count,
                    TotalDiscrepancy = discrepancies.Sum(d => d.Difference)
                };

                // Calculate Performance Metrics
                if (report.TotalTransactions > 0)
                {
                    report.SuccessRate = (report.SuccessfulTransactions * 100m) / report.TotalTransactions;
                }

                // Build Session Summaries
                report.Sessions = sessions.Select(s => new CashSessionSummary
                {
                    CashSessionId = s.CashSessionId,
                    SessionNumber = s.SessionNumber,
                    OpenedAt = s.OpenedAt,
                    ClosedAt = s.ClosedAt,
                    DurationMinutes = s.ClosedAt.HasValue ? (int?)(s.ClosedAt.Value - s.OpenedAt).TotalMinutes : null,
                    OpeningBalance = s.OpeningBalance,
                    CashBalance = s.CashBalance,
                    PosBalance = s.PosBalance,
                    TransactionCount = s.Transactions?.Count(t => !t.IsDeleted) ?? 0,
                    Status = s.Status.ToString()
                }).ToList();

                // Build Discrepancy Summaries
                report.Discrepancies = discrepancies.Select(d => new DiscrepancySummary
                {
                    Id = d.Id,
                    Type = d.Type.ToString(),
                    ExpectedAmount = d.ExpectedAmount,
                    ActualAmount = d.ActualAmount,
                    Difference = d.Difference,
                    Reason = d.Reason,
                    Status = d.Status.ToString(),
                    ReportedAt = d.ReportedAt
                }).ToList();

                _logger.Information("✅ Daily report generated successfully. Cashier: {CashierId}, Transactions: {Count}", 
                    cashierId, report.TotalTransactions);

                return ServiceResult<CashierDailyReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                return ServiceResult<CashierDailyReport>.Failed("خطا در دریافت گزارش روزانه", "EXCEPTION");
            }
        }

        #endregion

        #region GetMonthlyReportAsync

        public async Task<ServiceResult<CashierMonthlyReport>> GetMonthlyReportAsync(string cashierId, int year, int month)
        {
            try
            {
                _logger.Information("📊 Getting monthly report for Cashier: {CashierId}, Year: {Year}, Month: {Month}", 
                    cashierId, year, month);

                if (string.IsNullOrWhiteSpace(cashierId))
                {
                    return ServiceResult<CashierMonthlyReport>.Failed("شناسه منشی الزامی است.", "VALIDATION");
                }

                if (month < 1 || month > 12)
                {
                    return ServiceResult<CashierMonthlyReport>.Failed("ماه باید بین 1 تا 12 باشد.", "VALIDATION");
                }

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                // Get Cashier Info
                var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
                if (cashier == null)
                {
                    return ServiceResult<CashierMonthlyReport>.Failed("منشی یافت نشد.", "NOT_FOUND");
                }

                // Get Daily Reports
                var dailyReports = new List<CashierDailyReport>();
                for (var day = startDate; day < endDate; day = day.AddDays(1))
                {
                    var dailyResult = await GetDailyReportAsync(cashierId, day);
                    if (dailyResult.Success && dailyResult.Data != null)
                    {
                        dailyReports.Add(dailyResult.Data);
                    }
                }

                // Build Monthly Report
                var report = new CashierMonthlyReport
                {
                    CashierId = cashierId,
                    CashierName = cashier.UserName ?? cashier.Email ?? "نامشخص",
                    Year = year,
                    Month = month,
                    TotalSessions = dailyReports.Sum(d => d.SessionsOpened),
                    TotalTransactions = dailyReports.Sum(d => d.TotalTransactions),
                    TotalAmount = dailyReports.Sum(d => d.TotalAmount),
                    TotalDiscrepancies = dailyReports.Sum(d => d.DiscrepancyCount),
                    DailyReports = dailyReports
                };

                // Calculate Performance Metrics
                if (dailyReports.Count > 0)
                {
                    report.AverageTransactionTime = (decimal)dailyReports.Average(d => (double)d.AverageTransactionTime);
                    report.AverageSuccessRate = (decimal)dailyReports.Average(d => (double)d.SuccessRate);

                    var bestDay = dailyReports.OrderByDescending(d => d.TotalTransactions).FirstOrDefault();
                    var worstDay = dailyReports.OrderBy(d => d.TotalTransactions).FirstOrDefault();

                    report.BestDay = bestDay?.Date;
                    report.WorstDay = worstDay?.Date;
                }

                _logger.Information("✅ Monthly report generated successfully. Cashier: {CashierId}, Total Transactions: {Count}", 
                    cashierId, report.TotalTransactions);

                return ServiceResult<CashierMonthlyReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting monthly report for Cashier: {CashierId}, Year: {Year}, Month: {Month}", 
                    cashierId, year, month);
                return ServiceResult<CashierMonthlyReport>.Failed("خطا در دریافت گزارش ماهانه", "EXCEPTION");
            }
        }

        #endregion

        #region GetAllCashiersSummaryAsync

        public async Task<ServiceResult<List<CashierSummary>>> GetAllCashiersSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Getting all cashiers summary from {FromDate} to {ToDate}", fromDate, toDate);

                if (fromDate > toDate)
                {
                    return ServiceResult<List<CashierSummary>>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                // Get all cashiers who have sessions in this period
                var cashierIds = await _context.CashSessions
                    .Where(cs => cs.OpenedAt >= fromDate &&
                                 cs.OpenedAt <= toDate &&
                                 !cs.IsDeleted)
                    .Select(cs => cs.UserId)
                    .Distinct()
                    .ToListAsync();

                var summaries = new List<CashierSummary>();

                foreach (var cashierId in cashierIds)
                {
                    var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
                    if (cashier == null) continue;

                    // Get Sessions
                    var sessions = await _context.CashSessions
                        .Include(cs => cs.Transactions)
                        .Where(cs => cs.UserId == cashierId &&
                                     cs.OpenedAt >= fromDate &&
                                     cs.OpenedAt <= toDate &&
                                     !cs.IsDeleted)
                        .ToListAsync();

                    // Get Transactions
                    var transactions = sessions
                        .SelectMany(s => s.Transactions)
                        .Where(t => !t.IsDeleted)
                        .ToList();

                    // Get Discrepancies
                    var discrepancyCount = await _context.PaymentDiscrepancies
                        .CountAsync(d => d.CashSession.UserId == cashierId &&
                                         d.ReportedAt >= fromDate &&
                                         d.ReportedAt <= toDate);

                    var summary = new CashierSummary
                    {
                        CashierId = cashierId,
                        CashierName = cashier.UserName ?? cashier.Email ?? "نامشخص",
                        SessionCount = sessions.Count,
                        TransactionCount = transactions.Count,
                        TotalAmount = transactions.Sum(t => t.Amount),
                        DiscrepancyCount = discrepancyCount
                    };

                    if (summary.TransactionCount > 0)
                    {
                        var successful = transactions.Count(t => t.Status == PaymentStatus.Success);
                        summary.SuccessRate = (successful * 100m) / summary.TransactionCount;
                    }

                    summaries.Add(summary);
                }

                // Rank cashiers by transaction count
                var ranked = summaries
                    .OrderByDescending(s => s.TransactionCount)
                    .ThenByDescending(s => s.SuccessRate)
                    .ToList();

                for (int i = 0; i < ranked.Count; i++)
                {
                    ranked[i].Rank = i + 1;
                }

                _logger.Information("✅ All cashiers summary generated successfully. Count: {Count}", ranked.Count);

                return ServiceResult<List<CashierSummary>>.Successful(ranked);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting all cashiers summary from {FromDate} to {ToDate}", fromDate, toDate);
                return ServiceResult<List<CashierSummary>>.Failed("خطا در دریافت خلاصه منشی‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region CompareCashiersAsync

        public async Task<ServiceResult<CashierPerformanceComparison>> CompareCashiersAsync(
            List<string> cashierIds, 
            DateTime fromDate, 
            DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Comparing cashiers: {CashierIds}, from {FromDate} to {ToDate}", 
                    string.Join(", ", cashierIds), fromDate, toDate);

                if (cashierIds == null || cashierIds.Count == 0)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed("حداقل یک منشی باید انتخاب شود.", "VALIDATION");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                // Get summaries for selected cashiers
                var allSummariesResult = await GetAllCashiersSummaryAsync(fromDate, toDate);
                if (!allSummariesResult.Success)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed(allSummariesResult.Message, allSummariesResult.Code);
                }

                var selectedSummaries = allSummariesResult.Data
                    .Where(s => cashierIds.Contains(s.CashierId))
                    .ToList();

                if (selectedSummaries.Count == 0)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed("هیچ داده‌ای برای منشی‌های انتخاب شده یافت نشد.", "NOT_FOUND");
                }

                var comparison = new CashierPerformanceComparison
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Cashiers = selectedSummaries,
                    TopPerformer = selectedSummaries.OrderByDescending(s => s.TransactionCount).FirstOrDefault()
                };

                if (selectedSummaries.Count > 0)
                {
                    comparison.AverageTransactionCount = (decimal)selectedSummaries.Average(s => s.TransactionCount);
                    comparison.AverageTotalAmount = (decimal)selectedSummaries.Average(s => (double)s.TotalAmount);
                    comparison.AverageSuccessRate = (decimal)selectedSummaries.Average(s => (double)s.SuccessRate);
                }

                _logger.Information("✅ Cashiers comparison generated successfully. Count: {Count}", selectedSummaries.Count);

                return ServiceResult<CashierPerformanceComparison>.Successful(comparison);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error comparing cashiers from {FromDate} to {ToDate}", fromDate, toDate);
                return ServiceResult<CashierPerformanceComparison>.Failed("خطا در مقایسه منشی‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region ExportToExcelAsync

        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            // TODO: Implement Excel export using EPPlus or ClosedXML
            _logger.Warning("⚠️ Excel export not yet implemented for Cashier: {CashierId}", cashierId);
            return ServiceResult<byte[]>.Failed("Export به Excel هنوز پیاده‌سازی نشده است.", "NOT_IMPLEMENTED");
        }

        #endregion

        #region ExportToPdfAsync

        public async Task<ServiceResult<byte[]>> ExportToPdfAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            // TODO: Implement PDF export using iTextSharp or QuestPDF
            _logger.Warning("⚠️ PDF export not yet implemented for Cashier: {CashierId}", cashierId);
            return ServiceResult<byte[]>.Failed("Export به PDF هنوز پیاده‌سازی نشده است.", "NOT_IMPLEMENTED");
        }

        #endregion
    }
}

