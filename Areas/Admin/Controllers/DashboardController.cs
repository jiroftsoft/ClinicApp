using System;
using System.Web.Mvc;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Admin.Dashboard;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد مرکزی پلتفرم ادمین — گرید ویجت‌ها، آمادهٔ اتصال به سرویس‌ها در فاز بعد
    /// طبق Docs/ADMIN_PLATFORM_ARCHITECTURE.md
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class DashboardController : Controller
    {
        private readonly ILogger _logger = Log.ForContext<DashboardController>();

        /// <summary>
        /// صفحهٔ اصلی داشبورد با ویجت‌های نمایشی؛ دادهٔ واقعی در فاز بعد از سرویس‌ها پر می‌شود.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                _logger.Information("درخواست نمایش داشبورد پلتفرم ادمین توسط کاربر {User}", User?.Identity?.Name);

                var model = new DashboardPageViewModel
                {
                    TodayAppointments = new WidgetTodayAppointmentsViewModel
                    {
                        Count = 0,
                        Items = new System.Collections.Generic.List<TodayAppointmentItemViewModel>()
                    },
                    PatientStats = new WidgetPatientStatsViewModel
                    {
                        TodayCount = 0,
                        WeekCount = 0,
                        MonthCount = 0
                    },
                    FinancialOverview = new WidgetFinancialOverviewViewModel
                    {
                        TodayRevenue = 0,
                        WeekRevenue = 0
                    },
                    ShowQuickActions = true
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری داشبورد پلتفرم ادمین");
                return View(new DashboardPageViewModel());
            }
        }
    }
}
