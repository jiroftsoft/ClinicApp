using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Color = System.Drawing.Color;

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
    /// 5. Export به Excel و PDF
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

                // Get Sessions (بدون Include(User) — کاربر قبلاً بارگذاری شده؛ کاهش N+1 و حجم داده)
                var sessions = await _context.CashSessions
                    .Include(cs => cs.Transactions)
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
                    SessionsClosed = sessions.Count(s => s.ClosedAt.HasValue),
                    TotalTransactions = transactions.Count,
                    PosTransactions = transactions.Count(t => t.Method == PaymentMethod.POS),
                    CashTransactions = transactions.Count(t => t.Method == PaymentMethod.Cash),
                    TotalAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    PosAmount = transactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    CashAmount = transactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    SuccessfulTransactions = transactions.Count(t => t.Status == PaymentStatus.Success),
                    FailedTransactions = transactions.Count(t => t.Status == PaymentStatus.Failed),
                    SuccessRate = transactions.Count > 0 ? (decimal)(transactions.Count(t => t.Status == PaymentStatus.Success) * 100.0 / transactions.Count) : 0,
                    AverageTransactionTime = transactions.Any(t => t.UpdatedAt.HasValue) 
                        ? (decimal)transactions.Where(t => t.UpdatedAt.HasValue).Average(t => (t.UpdatedAt.Value - t.CreatedAt).TotalSeconds) 
                        : 0,
                    DiscrepancyCount = discrepancies.Count,
                    TotalDiscrepancy = discrepancies.Sum(d => d.Difference)
                };

                // Add Session Summaries
                report.Sessions = sessions.Select(s => new CashSessionSummary
                {
                    CashSessionId = s.Id,
                    SessionNumber = s.SessionNumber,
                    OpenedAt = s.OpenedAt,
                    ClosedAt = s.ClosedAt,
                    DurationMinutes = s.ClosedAt.HasValue ? (int?)(s.ClosedAt.Value - s.OpenedAt).TotalMinutes : null,
                    OpeningBalance = s.OpeningBalance,
                    CashBalance = s.CashBalance,
                    PosBalance = s.PosBalance,
                    TransactionCount = s.Transactions.Count(t => !t.IsDeleted),
                    Status = s.ClosedAt.HasValue ? "Closed" : "Open"
                }).ToList();

                // Add Discrepancy Summaries
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

                _logger.Information("✅ Daily report generated successfully. Transactions: {Count}", report.TotalTransactions);

                return ServiceResult<CashierDailyReport>.Successful(report);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting daily report for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                return ServiceResult<CashierDailyReport>.Failed("خطا در دریافت گزارش روزانه", "EXCEPTION");
            }
        }

        /// <summary>
        /// بارگذاری گزارش‌های روزانه برای بازه با حداقل round-trip به DB (بهینه برای پروداکشن).
        /// استفاده در گزارش ماهانه، بازه‌زمانی و Export بدون حلقه روزانه.
        /// </summary>
        private async Task<List<CashierDailyReport>> GetDailyReportsForRangeInternalAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            var start = fromDate.Date;
            var end = toDate.Date.AddDays(1);

            var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
            if (cashier == null)
                return new List<CashierDailyReport>();

            var sessions = await _context.CashSessions
                .Include(cs => cs.Transactions)
                .Where(cs => cs.UserId == cashierId &&
                             cs.OpenedAt >= start &&
                             cs.OpenedAt < end &&
                             !cs.IsDeleted)
                .OrderBy(cs => cs.OpenedAt)
                .ToListAsync();

            var discrepancies = await _context.PaymentDiscrepancies
                .Include(d => d.CashSession)
                .Where(d => d.CashSession.UserId == cashierId &&
                            d.ReportedAt >= start &&
                            d.ReportedAt < end)
                .ToListAsync();

            var cashierName = cashier.UserName ?? cashier.Email ?? "نامشخص";
            var dailyReports = new List<CashierDailyReport>();

            for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
            {
                var dayEnd = date.AddDays(1);
                var daySessions = sessions.Where(s => s.OpenedAt >= date && s.OpenedAt < dayEnd).ToList();
                var dayTransactions = daySessions.SelectMany(s => s.Transactions).Where(t => !t.IsDeleted).ToList();
                var dayDiscrepancies = discrepancies.Where(d => d.ReportedAt >= date && d.ReportedAt < dayEnd).ToList();

                var report = new CashierDailyReport
                {
                    CashierId = cashierId,
                    CashierName = cashierName,
                    Date = date,
                    SessionsOpened = daySessions.Count,
                    SessionsClosed = daySessions.Count(s => s.ClosedAt.HasValue),
                    TotalTransactions = dayTransactions.Count,
                    PosTransactions = dayTransactions.Count(t => t.Method == PaymentMethod.POS),
                    CashTransactions = dayTransactions.Count(t => t.Method == PaymentMethod.Cash),
                    TotalAmount = dayTransactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    PosAmount = dayTransactions.Where(t => t.Method == PaymentMethod.POS && t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    CashAmount = dayTransactions.Where(t => t.Method == PaymentMethod.Cash && t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                    SuccessfulTransactions = dayTransactions.Count(t => t.Status == PaymentStatus.Success),
                    FailedTransactions = dayTransactions.Count(t => t.Status == PaymentStatus.Failed),
                    SuccessRate = dayTransactions.Count > 0 ? (decimal)(dayTransactions.Count(t => t.Status == PaymentStatus.Success) * 100.0 / dayTransactions.Count) : 0,
                    AverageTransactionTime = dayTransactions.Any(t => t.UpdatedAt.HasValue)
                        ? (decimal)dayTransactions.Where(t => t.UpdatedAt.HasValue).Average(t => (t.UpdatedAt.Value - t.CreatedAt).TotalSeconds)
                        : 0,
                    DiscrepancyCount = dayDiscrepancies.Count,
                    TotalDiscrepancy = dayDiscrepancies.Sum(d => d.Difference),
                    Sessions = daySessions.Select(s => new CashSessionSummary
                    {
                        CashSessionId = s.Id,
                        SessionNumber = s.SessionNumber,
                        OpenedAt = s.OpenedAt,
                        ClosedAt = s.ClosedAt,
                        DurationMinutes = s.ClosedAt.HasValue ? (int?)(s.ClosedAt.Value - s.OpenedAt).TotalMinutes : null,
                        OpeningBalance = s.OpeningBalance,
                        CashBalance = s.CashBalance,
                        PosBalance = s.PosBalance,
                        TransactionCount = s.Transactions.Count(t => !t.IsDeleted),
                        Status = s.ClosedAt.HasValue ? "Closed" : "Open"
                    }).ToList(),
                    Discrepancies = dayDiscrepancies.Select(d => new DiscrepancySummary
                    {
                        Id = d.Id,
                        Type = d.Type.ToString(),
                        ExpectedAmount = d.ExpectedAmount,
                        ActualAmount = d.ActualAmount,
                        Difference = d.Difference,
                        Reason = d.Reason,
                        Status = d.Status.ToString(),
                        ReportedAt = d.ReportedAt
                    }).ToList()
                };
                dailyReports.Add(report);
            }

            return dailyReports;
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

                // Convert Persian month to Gregorian date range
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1); // last day of month inclusive

                // Get Cashier Info
                var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
                if (cashier == null)
                {
                    return ServiceResult<CashierMonthlyReport>.Failed("منشی یافت نشد.", "NOT_FOUND");
                }

                // یک بار بارگذاری بازه ماه (بدون حلقه روزانه)
                var allDaily = await GetDailyReportsForRangeInternalAsync(cashierId, startDate, endDate);
                var dailyReports = allDaily.Where(d => d.TotalTransactions > 0).ToList();

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
                    AverageTransactionTime = dailyReports.Any() ? dailyReports.Average(d => d.AverageTransactionTime) : 0,
                    AverageSuccessRate = dailyReports.Any() ? dailyReports.Average(d => d.SuccessRate) : 0,
                    BestDay = dailyReports.OrderByDescending(d => d.TotalTransactions).FirstOrDefault()?.Date,
                    WorstDay = dailyReports.OrderBy(d => d.TotalTransactions).FirstOrDefault()?.Date,
                    DailyReports = dailyReports
                };

                _logger.Information("✅ Monthly report generated successfully. Total Transactions: {Count}", 
                    report.TotalTransactions);

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

                var rangeStart = fromDate.Date;
                var rangeEndExclusive = toDate.Date.AddDays(1);

                // 1) شناسه منشی‌هایی که در بازه حداقل یک سشن دارند
                var cashierIds = await _context.CashSessions
                    .Where(cs => cs.OpenedAt >= rangeStart &&
                                 cs.OpenedAt < rangeEndExclusive &&
                                 !cs.IsDeleted)
                    .Select(cs => cs.UserId)
                    .Distinct()
                    .ToListAsync();

                if (cashierIds.Count == 0)
                {
                    _logger.Information("✅ All cashiers summary: no sessions in range");
                    return ServiceResult<List<CashierSummary>>.Successful(new List<CashierSummary>());
                }

                // 2) بارگذاری یک‌جای کاربران (منشی‌ها)
                var users = await _context.Users
                    .Where(u => cashierIds.Contains(u.Id) && !u.IsDeleted)
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();
                var userDict = users.ToDictionary(u => u.Id);

                // 3) بارگذاری یک‌جای سشن‌ها و تراکنش‌ها در بازه
                var sessions = await _context.CashSessions
                    .Include(cs => cs.Transactions)
                    .Where(cs => cs.OpenedAt >= rangeStart &&
                                 cs.OpenedAt < rangeEndExclusive &&
                                 !cs.IsDeleted)
                    .ToListAsync();

                // 4) بارگذاری یک‌جای اختلاف‌های مالی در بازه (فیلتر منشی از طریق CashSession)
                var discrepancies = await _context.PaymentDiscrepancies
                    .Include(d => d.CashSession)
                    .Where(d => d.ReportedAt >= rangeStart &&
                                d.ReportedAt < rangeEndExclusive &&
                                d.CashSession != null &&
                                cashierIds.Contains(d.CashSession.UserId))
                    .ToListAsync();

                // 5) ساخت خلاصه به‌ازای هر منشی در حافظه
                var summaries = new List<CashierSummary>();
                foreach (var cashierId in cashierIds)
                {
                    if (!userDict.TryGetValue(cashierId, out var user))
                        continue;

                    var cashierSessions = sessions.Where(s => s.UserId == cashierId).ToList();
                    var transactions = cashierSessions
                        .SelectMany(s => s.Transactions)
                        .Where(t => !t.IsDeleted &&
                                    t.CreatedAt >= rangeStart &&
                                    t.CreatedAt < rangeEndExclusive)
                        .ToList();
                    var cashierDiscrepancies = discrepancies
                        .Where(d => d.CashSession != null && d.CashSession.UserId == cashierId)
                        .ToList();

                    summaries.Add(new CashierSummary
                    {
                        CashierId = cashierId,
                        CashierName = user.UserName ?? user.Email ?? "نامشخص",
                        SessionCount = cashierSessions.Count,
                        TransactionCount = transactions.Count,
                        TotalAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount),
                        DiscrepancyCount = cashierDiscrepancies.Count,
                        SuccessRate = transactions.Count > 0
                            ? (decimal)(transactions.Count(t => t.Status == PaymentStatus.Success) * 100.0 / transactions.Count)
                            : 0
                    });
                }

                var rankedSummaries = summaries
                    .OrderByDescending(s => s.TransactionCount)
                    .ThenByDescending(s => s.TotalAmount)
                    .ToList();

                for (int i = 0; i < rankedSummaries.Count; i++)
                {
                    rankedSummaries[i].Rank = i + 1;
                }

                _logger.Information("✅ All cashiers summary generated successfully. Count: {Count} (queries: 5 fixed)", rankedSummaries.Count);

                return ServiceResult<List<CashierSummary>>.Successful(rankedSummaries);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting all cashiers summary from {FromDate} to {ToDate}", fromDate, toDate);
                return ServiceResult<List<CashierSummary>>.Failed("خطا در دریافت خلاصه منشی‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region CompareCashiersAsync

        public async Task<ServiceResult<CashierPerformanceComparison>> CompareCashiersAsync(List<string> cashierIds, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Comparing cashiers from {FromDate} to {ToDate}", fromDate, toDate);

                if (cashierIds == null || cashierIds.Count == 0)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed("حداقل یک منشی باید انتخاب شود.", "VALIDATION");
                }

                if (fromDate > toDate)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                var distinctIds = cashierIds.Distinct().ToList();

                var allSummariesResult = await GetAllCashiersSummaryAsync(fromDate, toDate);
                if (!allSummariesResult.Success)
                {
                    return ServiceResult<CashierPerformanceComparison>.Failed(allSummariesResult.Message, allSummariesResult.Code);
                }

                var withData = allSummariesResult.Data
                    .Where(s => distinctIds.Contains(s.CashierId))
                    .ToList();

                var missingIds = distinctIds
                    .Except(withData.Select(s => s.CashierId))
                    .ToList();

                if (missingIds.Count > 0)
                {
                    var missingUsers = await _context.Users
                        .Where(u => missingIds.Contains(u.Id) && !u.IsDeleted)
                        .Select(u => new { u.Id, u.UserName, u.Email })
                        .ToListAsync();

                    foreach (var u in missingUsers)
                    {
                        withData.Add(new CashierSummary
                        {
                            CashierId = u.Id,
                            CashierName = u.UserName ?? u.Email ?? "نامشخص",
                            SessionCount = 0,
                            TransactionCount = 0,
                            TotalAmount = 0,
                            DiscrepancyCount = 0,
                            SuccessRate = 0,
                            Rank = null
                        });
                    }

                    var stillMissing = missingIds.Except(missingUsers.Select(x => x.Id)).ToList();
                    if (stillMissing.Count > 0)
                    {
                        _logger.Warning("⚠️ Compare: {Count} selected cashier(s) not found in Users: {Ids}", stillMissing.Count, string.Join(", ", stillMissing));
                    }
                }

                var ranked = withData
                    .OrderByDescending(s => s.TransactionCount)
                    .ThenByDescending(s => s.TotalAmount)
                    .ToList();

                for (int i = 0; i < ranked.Count; i++)
                {
                    ranked[i].Rank = i + 1;
                }

                var comparison = new CashierPerformanceComparison
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    Cashiers = ranked,
                    TopPerformer = ranked.FirstOrDefault(s => s.TransactionCount > 0),
                    AverageTransactionCount = ranked.Any() ? ranked.Average(s => (decimal)s.TransactionCount) : 0,
                    AverageTotalAmount = ranked.Any() ? ranked.Average(s => s.TotalAmount) : 0,
                    AverageSuccessRate = ranked.Any() ? ranked.Average(s => s.SuccessRate) : 0
                };

                _logger.Information("✅ Cashiers comparison completed successfully. Count: {Count} (with data: {WithData})", ranked.Count, ranked.Count(s => s.TransactionCount > 0));

                return ServiceResult<CashierPerformanceComparison>.Successful(comparison);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error comparing cashiers from {FromDate} to {ToDate}", fromDate, toDate);
                return ServiceResult<CashierPerformanceComparison>.Failed("خطا در مقایسه منشی‌ها", "EXCEPTION");
            }
        }

        #endregion

        #region GetRangeReportAsync

        /// <summary>
        /// گزارش بازه‌زمانی با تجمیع روزانه (برای نمایش در صفحه RangeReport)
        /// </summary>
        public async Task<ServiceResult<CashierDailyReport>> GetRangeReportAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Getting range report for Cashier: {CashierId}, From: {FromDate}, To: {ToDate}",
                    cashierId, fromDate, toDate);

                if (string.IsNullOrWhiteSpace(cashierId))
                    return ServiceResult<CashierDailyReport>.Failed("شناسه منشی الزامی است.", "VALIDATION");
                if (fromDate > toDate)
                    return ServiceResult<CashierDailyReport>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");

                var cashier = await _context.Users.FirstOrDefaultAsync(u => u.Id == cashierId);
                if (cashier == null)
                    return ServiceResult<CashierDailyReport>.Failed("منشی یافت نشد.", "NOT_FOUND");

                var dailyReports = await GetDailyReportsForRangeInternalAsync(cashierId, fromDate.Date, toDate.Date);

                if (dailyReports.Count == 0)
                {
                    return ServiceResult<CashierDailyReport>.Successful(new CashierDailyReport
                    {
                        CashierId = cashierId,
                        CashierName = cashier.UserName ?? cashier.Email ?? "نامشخص",
                        Date = fromDate
                    });
                }

                var totalTransactions = dailyReports.Sum(d => d.TotalTransactions);
                var aggregated = new CashierDailyReport
                {
                    CashierId = cashierId,
                    CashierName = cashier.UserName ?? cashier.Email ?? "نامشخص",
                    Date = fromDate,
                    SessionsOpened = dailyReports.Sum(d => d.SessionsOpened),
                    SessionsClosed = dailyReports.Sum(d => d.SessionsClosed),
                    TotalTransactions = totalTransactions,
                    PosTransactions = dailyReports.Sum(d => d.PosTransactions),
                    CashTransactions = dailyReports.Sum(d => d.CashTransactions),
                    TotalAmount = dailyReports.Sum(d => d.TotalAmount),
                    PosAmount = dailyReports.Sum(d => d.PosAmount),
                    CashAmount = dailyReports.Sum(d => d.CashAmount),
                    SuccessfulTransactions = dailyReports.Sum(d => d.SuccessfulTransactions),
                    FailedTransactions = dailyReports.Sum(d => d.FailedTransactions),
                    SuccessRate = totalTransactions > 0
                        ? (decimal)(dailyReports.Sum(d => d.SuccessfulTransactions) * 100.0 / totalTransactions)
                        : 0,
                    AverageTransactionTime = totalTransactions > 0
                        ? (decimal)dailyReports.Sum(d => d.AverageTransactionTime * d.TotalTransactions) / totalTransactions
                        : 0,
                    DiscrepancyCount = dailyReports.Sum(d => d.DiscrepancyCount),
                    TotalDiscrepancy = dailyReports.Sum(d => d.TotalDiscrepancy),
                    Sessions = dailyReports.SelectMany(d => d.Sessions ?? new List<CashSessionSummary>()).ToList(),
                    Discrepancies = dailyReports.SelectMany(d => d.Discrepancies ?? new List<DiscrepancySummary>()).ToList()
                };

                _logger.Information("✅ Range report generated. Days: {Days}, Transactions: {Count}",
                    dailyReports.Count, aggregated.TotalTransactions);

                return ServiceResult<CashierDailyReport>.Successful(aggregated);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting range report for Cashier: {CashierId}", cashierId);
                return ServiceResult<CashierDailyReport>.Failed("خطا در دریافت گزارش بازه زمانی", "EXCEPTION");
            }
        }

        #endregion

        #region GetCashiersListAsync

        /// <summary>
        /// لیست منشی‌ها (کاربران با نقش Receptionist) برای DropDown
        /// </summary>
        public async Task<List<SelectListItem>> GetCashiersListAsync()
        {
            try
            {
                var receptionistRoleId = await _context.Roles
                    .Where(r => r.Name == AppRoles.Receptionist)
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(receptionistRoleId))
                {
                    _logger.Warning("⚠️ Receptionist role not found");
                    return new List<SelectListItem>();
                }

                var cashiers = await _context.Users
                    .Where(u => u.Roles.Any(r => r.RoleId == receptionistRoleId) && !u.IsDeleted)
                    .OrderBy(u => u.UserName ?? u.Email)
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var items = cashiers
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName ?? u.Email ?? "نامشخص"
                    })
                    .ToList();

                _logger.Information("✅ Retrieved {Count} cashiers for report", items.Count);
                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting cashiers list");
                return new List<SelectListItem>();
            }
        }

        #endregion

        #region ExportToExcelAsync

        public async Task<ServiceResult<byte[]>> ExportToExcelAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Exporting to Excel - Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                if (fromDate > toDate)
                {
                    return ServiceResult<byte[]>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                byte[] excelBytes;

                // اگر cashierId = "all" باشد، خلاصه تمام منشی‌ها را Export می‌کنیم
                if (cashierId == "all" || string.IsNullOrWhiteSpace(cashierId))
                {
                    var summariesResult = await GetAllCashiersSummaryAsync(fromDate, toDate);
                    if (!summariesResult.Success)
                    {
                        return ServiceResult<byte[]>.Failed(summariesResult.Message, summariesResult.Code);
                    }

                    excelBytes = ExportAllCashiersSummaryToExcel(summariesResult.Data, fromDate, toDate);
                }
                else
                {
                    var dailyReports = (await GetDailyReportsForRangeInternalAsync(cashierId, fromDate.Date, toDate.Date))
                        .Where(d => d.TotalTransactions > 0).ToList();

                    if (dailyReports.Count == 0)
                    {
                        return ServiceResult<byte[]>.Failed("داده‌ای برای Export یافت نشد.", "NOT_FOUND");
                    }

                    excelBytes = ExportDailyReportsToExcel(dailyReports, cashierId, fromDate, toDate);
                }

                if (excelBytes == null || excelBytes.Length == 0)
                {
                    return ServiceResult<byte[]>.Failed("خطا در تولید فایل Excel", "EXPORT_ERROR");
                }

                _logger.Information("✅ Excel export completed successfully. Size: {Size} bytes", excelBytes.Length);

                return ServiceResult<byte[]>.Successful(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error exporting to Excel for Cashier: {CashierId}", cashierId);
                return ServiceResult<byte[]>.Failed("خطا در Export به Excel", "EXCEPTION");
            }
        }

        /// <summary>
        /// Export گزارش‌های روزانه به Excel
        /// </summary>
        private byte[] ExportDailyReportsToExcel(List<CashierDailyReport> reports, string cashierId, DateTime fromDate, DateTime toDate)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("گزارش روزانه");

                // Header
                var cashierName = reports.FirstOrDefault()?.CashierName ?? "نامشخص";
                sheet.Cells[1, 1].Value = $"گزارش عملکرد منشی: {cashierName}";
                sheet.Cells[1, 1, 1, 7].Merge = true;
                sheet.Cells[1, 1].Style.Font.Bold = true;
                sheet.Cells[1, 1].Style.Font.Size = 16;
                sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[2, 1].Value = $"از تاریخ: {PersianDateHelper.ToPersianDate(fromDate)}";
                sheet.Cells[2, 1, 2, 3].Merge = true;
                sheet.Cells[2, 4].Value = $"تا تاریخ: {PersianDateHelper.ToPersianDate(toDate)}";
                sheet.Cells[2, 4, 2, 7].Merge = true;

                // Column Headers
                string[] headers = { "تاریخ", "تعداد جلسات", "تعداد تراکنش‌ها", "مبلغ کل (ریال)", "نرخ موفقیت (%)", "تعداد اختلاف‌ها", "مبلغ اختلاف (ریال)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[4, i + 1].Value = headers[i];
                    sheet.Cells[4, i + 1].Style.Font.Bold = true;
                    sheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    sheet.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                var sortedReports = reports.OrderBy(r => r.Date).ToList();
                for (int i = 0; i < sortedReports.Count; i++)
                {
                    var report = sortedReports[i];
                    var row = i + 5;

                    if (row % 2 == 0)
                        sheet.Row(row).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));

                    sheet.Cells[row, 1].Value = PersianDateHelper.ToPersianDate(report.Date);
                    sheet.Cells[row, 2].Value = report.SessionsOpened;
                    sheet.Cells[row, 3].Value = report.TotalTransactions;
                    sheet.Cells[row, 4].Value = report.TotalAmount;
                    sheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[row, 5].Value = report.SuccessRate;
                    sheet.Cells[row, 5].Style.Numberformat.Format = "0.00";
                    sheet.Cells[row, 6].Value = report.DiscrepancyCount;
                    sheet.Cells[row, 7].Value = report.TotalDiscrepancy;
                    sheet.Cells[row, 7].Style.Numberformat.Format = "#,##0";

                    sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                // Summary Row
                var summaryRow = sortedReports.Count + 6;
                sheet.Cells[summaryRow, 1].Value = "جمع کل:";
                sheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                sheet.Cells[summaryRow, 2].Value = sortedReports.Sum(r => r.SessionsOpened);
                sheet.Cells[summaryRow, 3].Value = sortedReports.Sum(r => r.TotalTransactions);
                sheet.Cells[summaryRow, 4].Value = sortedReports.Sum(r => r.TotalAmount);
                sheet.Cells[summaryRow, 4].Style.Numberformat.Format = "#,##0";
                sheet.Cells[summaryRow, 5].Value = sortedReports.Average(r => r.SuccessRate);
                sheet.Cells[summaryRow, 5].Style.Numberformat.Format = "0.00";
                sheet.Cells[summaryRow, 6].Value = sortedReports.Sum(r => r.DiscrepancyCount);
                sheet.Cells[summaryRow, 7].Value = sortedReports.Sum(r => r.TotalDiscrepancy);
                sheet.Cells[summaryRow, 7].Style.Numberformat.Format = "#,##0";

                // Auto-fit columns
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Export خلاصه تمام منشی‌ها به Excel
        /// </summary>
        private byte[] ExportAllCashiersSummaryToExcel(List<CashierSummary> summaries, DateTime fromDate, DateTime toDate)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("گزارش منشی‌ها");

                // Header
                sheet.Cells[1, 1].Value = "گزارش عملکرد منشی‌ها";
                sheet.Cells[1, 1, 1, 8].Merge = true;
                sheet.Cells[1, 1].Style.Font.Bold = true;
                sheet.Cells[1, 1].Style.Font.Size = 16;
                sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                sheet.Cells[2, 1].Value = $"از تاریخ: {PersianDateHelper.ToPersianDate(fromDate)}";
                sheet.Cells[2, 1, 2, 4].Merge = true;
                sheet.Cells[2, 5].Value = $"تا تاریخ: {PersianDateHelper.ToPersianDate(toDate)}";
                sheet.Cells[2, 5, 2, 8].Merge = true;

                // Column Headers
                string[] headers = { "رتبه", "نام منشی", "تعداد جلسات", "تعداد تراکنش‌ها", "مبلغ کل (ریال)", "نرخ موفقیت (%)", "تعداد اختلاف‌ها" };
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[4, i + 1].Value = headers[i];
                    sheet.Cells[4, i + 1].Style.Font.Bold = true;
                    sheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    sheet.Cells[4, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                var sortedSummaries = summaries.OrderBy(s => s.Rank).ToList();
                for (int i = 0; i < sortedSummaries.Count; i++)
                {
                    var summary = sortedSummaries[i];
                    var row = i + 5;

                    if (row % 2 == 0)
                        sheet.Row(row).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));

                    sheet.Cells[row, 1].Value = summary.Rank ?? (i + 1);
                    sheet.Cells[row, 2].Value = summary.CashierName;
                    sheet.Cells[row, 3].Value = summary.SessionCount;
                    sheet.Cells[row, 4].Value = summary.TransactionCount;
                    sheet.Cells[row, 5].Value = summary.TotalAmount;
                    sheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[row, 6].Value = summary.SuccessRate;
                    sheet.Cells[row, 6].Style.Numberformat.Format = "0.00";
                    sheet.Cells[row, 7].Value = summary.DiscrepancyCount;

                    sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Auto-fit columns
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        #endregion

        #region ExportToPdfAsync

        public async Task<ServiceResult<byte[]>> ExportToPdfAsync(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.Information("📊 Exporting to PDF - Cashier: {CashierId}, From: {FromDate}, To: {ToDate}", 
                    cashierId, fromDate, toDate);

                if (fromDate > toDate)
                {
                    return ServiceResult<byte[]>.Failed("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.", "VALIDATION");
                }

                byte[] pdfBytes;

                // اگر cashierId = "all" باشد، خلاصه تمام منشی‌ها را Export می‌کنیم
                if (cashierId == "all" || string.IsNullOrWhiteSpace(cashierId))
                {
                    var summariesResult = await GetAllCashiersSummaryAsync(fromDate, toDate);
                    if (!summariesResult.Success)
                    {
                        return ServiceResult<byte[]>.Failed(summariesResult.Message, summariesResult.Code);
                    }

                    pdfBytes = ExportAllCashiersSummaryToPdf(summariesResult.Data, fromDate, toDate);
                }
                else
                {
                    var dailyReports = (await GetDailyReportsForRangeInternalAsync(cashierId, fromDate.Date, toDate.Date))
                        .Where(d => d.TotalTransactions > 0).ToList();

                    if (dailyReports.Count == 0)
                    {
                        return ServiceResult<byte[]>.Failed("داده‌ای برای Export یافت نشد.", "NOT_FOUND");
                    }

                    pdfBytes = ExportDailyReportsToPdf(dailyReports, fromDate, toDate);
                }

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return ServiceResult<byte[]>.Failed("خطا در تولید فایل PDF", "EXPORT_ERROR");
                }

                _logger.Information("✅ PDF export completed successfully. Size: {Size} bytes", pdfBytes.Length);

                return ServiceResult<byte[]>.Successful(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error exporting to PDF for Cashier: {CashierId}", cashierId);
                return ServiceResult<byte[]>.Failed("خطا در Export به PDF", "EXCEPTION");
            }
        }

        /// <summary>
        /// Export خلاصه تمام منشی‌ها به PDF
        /// </summary>
        private byte[] ExportAllCashiersSummaryToPdf(List<CashierSummary> summaries, DateTime fromDate, DateTime toDate)
        {
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "fonts", "Vazir.ttf");
            var fontExists = File.Exists(fontPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => fontExists ? x.FontFamily(fontPath).FontSize(11) : x.FontSize(11));

                    page.Header()
                        .Text("گزارش عملکرد منشی‌ها")
                        .FontSize(16)
                        .SemiBold()
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text($"از تاریخ: {PersianDateHelper.ToPersianDate(fromDate)}")
                                .FontSize(10);
                            column.Item().Text($"تا تاریخ: {PersianDateHelper.ToPersianDate(toDate)}")
                                .FontSize(10);
                            column.Item().PaddingTop(10);

                            // Table
                            column.Item().Table(table =>
                            {
                                // Header
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("رتبه");
                                    header.Cell().Element(CellStyle).Text("نام منشی");
                                    header.Cell().Element(CellStyle).Text("جلسات");
                                    header.Cell().Element(CellStyle).Text("تراکنش‌ها");
                                    header.Cell().Element(CellStyle).Text("مبلغ کل");
                                    header.Cell().Element(CellStyle).Text("نرخ موفقیت");
                                    header.Cell().Element(CellStyle).Text("اختلاف‌ها");
                                });

                                // Data
                                var sortedSummaries = summaries.OrderBy(s => s.Rank).ToList();
                                foreach (var summary in sortedSummaries)
                                {
                                    table.Cell().Element(CellStyle).Text((summary.Rank ?? 0).ToString());
                                    table.Cell().Element(CellStyle).Text(summary.CashierName);
                                    table.Cell().Element(CellStyle).Text(summary.SessionCount.ToString());
                                    table.Cell().Element(CellStyle).Text(summary.TransactionCount.ToString());
                                    table.Cell().Element(CellStyle).Text(summary.TotalAmount.ToString("N0"));
                                    table.Cell().Element(CellStyle).Text(summary.SuccessRate.ToString("F2") + "%");
                                    table.Cell().Element(CellStyle).Text(summary.DiscrepancyCount.ToString());
                                }
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Export گزارش‌های روزانه به PDF
        /// </summary>
        private byte[] ExportDailyReportsToPdf(List<CashierDailyReport> reports, DateTime fromDate, DateTime toDate)
        {
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "fonts", "Vazir.ttf");
            var fontExists = File.Exists(fontPath);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => fontExists ? x.FontFamily(fontPath).FontSize(11) : x.FontSize(11));

                    page.Header()
                        .Text($"گزارش عملکرد منشی: {reports.FirstOrDefault()?.CashierName ?? "نامشخص"}")
                        .FontSize(16)
                        .SemiBold()
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Item().Text($"از تاریخ: {PersianDateHelper.ToPersianDate(fromDate)}")
                                .FontSize(10);
                            column.Item().Text($"تا تاریخ: {PersianDateHelper.ToPersianDate(toDate)}")
                                .FontSize(10);
                            column.Item().PaddingTop(10);

                            // Summary
                            var totalTransactions = reports.Sum(r => r.TotalTransactions);
                            var totalAmount = reports.Sum(r => r.TotalAmount);
                            var avgSuccessRate = reports.Average(r => r.SuccessRate);

                            column.Item().Text($"تعداد کل تراکنش‌ها: {totalTransactions}")
                                .FontSize(12)
                                .SemiBold();
                            column.Item().Text($"مبلغ کل: {totalAmount:N0} ریال")
                                .FontSize(12)
                                .SemiBold();
                            column.Item().Text($"میانگین نرخ موفقیت: {avgSuccessRate:F2}%")
                                .FontSize(12)
                                .SemiBold();
                            column.Item().PaddingTop(10);

                            // Daily Reports Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("تاریخ");
                                    header.Cell().Element(CellStyle).Text("تراکنش‌ها");
                                    header.Cell().Element(CellStyle).Text("مبلغ کل");
                                    header.Cell().Element(CellStyle).Text("نرخ موفقیت");
                                    header.Cell().Element(CellStyle).Text("اختلاف‌ها");
                                });

                                foreach (var report in reports.OrderBy(r => r.Date))
                                {
                                    table.Cell().Element(CellStyle).Text(PersianDateHelper.ToPersianDate(report.Date));
                                    table.Cell().Element(CellStyle).Text(report.TotalTransactions.ToString());
                                    table.Cell().Element(CellStyle).Text(report.TotalAmount.ToString("N0"));
                                    table.Cell().Element(CellStyle).Text(report.SuccessRate.ToString("F2") + "%");
                                    table.Cell().Element(CellStyle).Text(report.DiscrepancyCount.ToString());
                                }
                            });
                        });
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Helper method for PDF cell styling
        /// </summary>
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .Padding(5)
                .AlignCenter()
                .AlignMiddle();
        }

        #endregion
    }
}
