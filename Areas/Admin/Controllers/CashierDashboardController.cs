using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Interfaces.Payment.Management;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.ViewModels.Admin.PaymentManagement;
using ClinicApp.ViewModels.Payment;
using ClinicApp.Areas.Admin.ViewModels;
using ClinicApp.Models.Entities.Payment;
using Serilog;
using PaymentStatisticsViewModel = ClinicApp.ViewModels.Admin.PaymentManagement.PaymentStatisticsViewModel;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد صندوق و منشی‌ها در پنل ادمین
    /// آمار عملیاتی، جلسات باز، اختلاف‌ها، رتبه‌بندی و دسترسی سریع.
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md، 03-Development-Contract-Quick-Guide.md
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class CashierDashboardController : Controller
    {
        private readonly ICashierPerformanceService _performanceService;
        private readonly ICashierReportService _reportService;
        private readonly IPaymentManagementService _paymentService;
        private readonly ICashSessionRepository _cashSessionRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public CashierDashboardController(
            ICashierPerformanceService performanceService,
            ICashierReportService reportService,
            IPaymentManagementService paymentService,
            ICashSessionRepository cashSessionRepository,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _cashSessionRepository = cashSessionRepository ?? throw new ArgumentNullException(nameof(cashSessionRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<CashierDashboardController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// صفحهٔ اصلی داشبورد صندوق — آمار امروز، جلسات باز، اختلاف‌ها، رتبه، منشی‌های برتر، دسترسی سریع
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("داشبورد صندوق بارگذاری شد. User: {UserId}", _currentUserService?.UserId);

                var userId = _currentUserService?.UserId ?? string.Empty;
                var todayStart = DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);

                var dashboard = new CashierDashboardViewModel
                {
                    SelectedDate = todayStart,
                    SelectedCashierId = userId
                };
                dashboard.DailyStats = await GetDailyStatsAsync(todayStart, userId);

                var topResult = await _performanceService.GetTopPerformersAsync(
                    todayStart.AddDays(-30), todayStart, topN: 5);
                if (topResult?.Success == true && topResult.Data != null)
                    dashboard.TopPerformers = topResult.Data;

                if (!string.IsNullOrEmpty(userId))
                {
                    var rankResult = await _performanceService.GetCashierRankingAsync(
                        userId, todayStart.AddDays(-30), todayStart);
                    if (rankResult?.Success == true)
                        dashboard.CurrentCashierRanking = rankResult.Data;
                }

                var todayFilter = new PaymentSearchFilter
                {
                    StartDate = todayStart,
                    EndDate = todayEnd
                };
                var todayStats = await _paymentService.GetPaymentStatisticsAsync(todayFilter);
                var openSessions = (await _cashSessionRepository.GetActiveSessionsAsync())?.ToList() ?? new List<CashSession>();
                var pendingDiscrepancyCount = await _paymentService.GetPendingDiscrepancyCountAsync();

                var openSessionDisplays = openSessions.Select(cs => new OpenSessionDisplay
                {
                    CashSessionId = cs.CashSessionId,
                    SessionNumber = cs.SessionNumber ?? $"CS{cs.CashSessionId:D6}",
                    UserName = cs.User?.UserName ?? cs.UserId ?? "-",
                    OpenedAtDisplay = PersianDateHelper.ToPersianDateTime(cs.OpenedAt, includeSeconds: false)
                }).ToList();

                var model = new CashierDashboardIndexViewModel
                {
                    Dashboard = dashboard,
                    TodayPaymentStats = todayStats ?? new PaymentStatisticsViewModel(),
                    OpenSessionsCount = openSessionDisplays.Count,
                    OpenSessions = openSessionDisplays,
                    PendingDiscrepancyCount = pendingDiscrepancyCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری داشبورد صندوق. User: {UserId}", _currentUserService?.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری داشبورد صندوق");
                return View(new CashierDashboardIndexViewModel());
            }
        }

        private async Task<CashierStatsViewModel> GetDailyStatsAsync(DateTime date, string cashierId)
        {
            try
            {
                if (string.IsNullOrEmpty(cashierId))
                    return new CashierStatsViewModel();
                var metrics = await _performanceService.GetMetricsAsync(cashierId, date);
                if (metrics?.Success != true || metrics.Data == null)
                    return new CashierStatsViewModel();
                var m = metrics.Data;
                return new CashierStatsViewModel
                {
                    TotalTransactions = m.TotalTransactions,
                    TotalAmount = m.TotalAmount,
                    SuccessRate = m.SuccessRate,
                    AverageTransactionTime = m.AverageTransactionTime,
                    DiscrepancyCount = m.DiscrepancyCount,
                    SessionsOpened = m.SessionsOpened,
                    SessionsClosed = m.SessionsClosed
                };
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در دریافت آمار روزانه. CashierId: {CashierId}, Date: {Date}", cashierId, date);
                return new CashierStatsViewModel();
            }
        }
    }
}
