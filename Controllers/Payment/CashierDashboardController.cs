using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Payment;
using Serilog;

namespace ClinicApp.Controllers.Payment
{
    /// <summary>
    /// کنترلر داشبورد منشی‌ها - SRP محور
    /// مدیریت داشبورد اصلی و آمار Real-time منشی‌ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش آمار Real-time منشی‌ها
    /// 2. نمایش Top Performers
    /// 3. نمایش رتبه منشی فعلی
    /// 4. نمایش Charts و نمودارها
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class CashierDashboardController : BaseController
    {
        #region Fields

        private readonly ICashierPerformanceService _performanceService;
        private readonly ICashierReportService _reportService;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor

        public CashierDashboardController(
            ICashierPerformanceService performanceService,
            ICashierReportService reportService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region داشبورد اصلی (Main Dashboard)

        /// <summary>
        /// داشبورد اصلی منشی‌ها
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("📊 Loading Cashier Dashboard for User: {UserId}", _currentUserService.UserId);

                var model = new CashierDashboardViewModel
                {
                    SelectedDate = DateTime.Today,
                    SelectedCashierId = _currentUserService.UserId
                };

                // دریافت آمار روزانه
                var dailyStats = await GetDailyStatsAsync(DateTime.Today, _currentUserService.UserId);
                model.DailyStats = dailyStats;

                // دریافت Top Performers (30 روز گذشته)
                var topPerformers = await _performanceService.GetTopPerformersAsync(
                    DateTime.Today.AddDays(-30),
                    DateTime.Today,
                    topN: 5);
                if (topPerformers.Success)
                {
                    model.TopPerformers = topPerformers.Data;
                }
                else
                {
                    _logger.Warning("⚠️ Failed to load Top Performers: {Message}", topPerformers.Message);
                }

                // دریافت رتبه منشی فعلی
                var ranking = await _performanceService.GetCashierRankingAsync(
                    _currentUserService.UserId,
                    DateTime.Today.AddDays(-30),
                    DateTime.Today);
                if (ranking.Success)
                {
                    model.CurrentCashierRanking = ranking.Data;
                }

                _logger.Information("✅ Cashier Dashboard loaded successfully");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error loading Cashier Dashboard for User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری Dashboard");
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// دریافت آمار روزانه (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetDailyStats(DateTime date, string cashierId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(cashierId))
                {
                    cashierId = _currentUserService.UserId;
                }

                _logger.Information("📊 Getting daily stats for Cashier: {CashierId}, Date: {Date}", cashierId, date);

                var metrics = await _performanceService.GetMetricsAsync(cashierId, date);
                if (!metrics.Success)
                {
                    _logger.Warning("⚠️ Failed to get metrics: {Message}", metrics.Message);
                    return StandardJsonResponse(false, metrics.Message);
                }

                var stats = new CashierStatsViewModel
                {
                    TotalTransactions = metrics.Data.TotalTransactions,
                    TotalAmount = metrics.Data.TotalAmount,
                    SuccessRate = metrics.Data.SuccessRate,
                    AverageTransactionTime = metrics.Data.AverageTransactionTime,
                    DiscrepancyCount = metrics.Data.DiscrepancyCount,
                    SessionsOpened = metrics.Data.SessionsOpened,
                    SessionsClosed = metrics.Data.SessionsClosed
                };

                return StandardJsonResponse(true, "آمار با موفقیت دریافت شد", stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting daily stats for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                return StandardJsonResponse(false, "خطا در دریافت آمار");
            }
        }

        /// <summary>
        /// دریافت Top Performers (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetTopPerformers(DateTime fromDate, DateTime toDate, int topN = 10)
        {
            try
            {
                _logger.Information("🏆 Getting top {TopN} performers from {FromDate} to {ToDate}", topN, fromDate, toDate);

                var result = await _performanceService.GetTopPerformersAsync(fromDate, toDate, topN);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get top performers: {Message}", result.Message);
                    return StandardJsonResponse(false, result.Message);
                }

                return StandardJsonResponse(true, "منشی‌های برتر با موفقیت دریافت شدند", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting top performers");
                return StandardJsonResponse(false, "خطا در دریافت منشی‌های برتر");
            }
        }

        /// <summary>
        /// دریافت رتبه منشی (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetCashierRanking(string cashierId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(cashierId))
                {
                    cashierId = _currentUserService.UserId;
                }

                _logger.Information("📊 Getting ranking for Cashier: {CashierId} from {FromDate} to {ToDate}", 
                    cashierId, fromDate, toDate);

                var result = await _performanceService.GetCashierRankingAsync(cashierId, fromDate, toDate);
                if (!result.Success)
                {
                    _logger.Warning("⚠️ Failed to get ranking: {Message}", result.Message);
                    return StandardJsonResponse(false, result.Message);
                }

                return StandardJsonResponse(true, "رتبه منشی با موفقیت دریافت شد", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting ranking for Cashier: {CashierId}", cashierId);
                return StandardJsonResponse(false, "خطا در دریافت رتبه منشی");
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// دریافت آمار روزانه
        /// </summary>
        private async Task<CashierStatsViewModel> GetDailyStatsAsync(DateTime date, string cashierId)
        {
            try
            {
                var metrics = await _performanceService.GetMetricsAsync(cashierId, date);
                if (!metrics.Success)
                {
                    _logger.Warning("⚠️ Metrics not found for Cashier: {CashierId}, Date: {Date}", cashierId, date);
                    return new CashierStatsViewModel(); // Return empty stats
                }

                return new CashierStatsViewModel
                {
                    TotalTransactions = metrics.Data.TotalTransactions,
                    TotalAmount = metrics.Data.TotalAmount,
                    SuccessRate = metrics.Data.SuccessRate,
                    AverageTransactionTime = metrics.Data.AverageTransactionTime,
                    DiscrepancyCount = metrics.Data.DiscrepancyCount,
                    SessionsOpened = metrics.Data.SessionsOpened,
                    SessionsClosed = metrics.Data.SessionsClosed
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error getting daily stats");
                return new CashierStatsViewModel(); // Return empty stats
            }
        }

        #endregion
    }
}

