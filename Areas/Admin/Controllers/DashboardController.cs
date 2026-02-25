using System;
using System.Web.Mvc;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Admin.Dashboard;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد مرکزی پلتفرم ادمین — فقط نقش Admin. منشی به داشبورد اختصاصی خود هدایت می‌شود.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class DashboardController : Controller
    {
        private readonly ILogger _logger = Log.ForContext<DashboardController>();

        /// <summary>
        /// صفحهٔ اصلی داشبورد ادمین. منشی اجازهٔ دسترسی ندارد → ریدایرکت به داشبورد منشی.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            if (User.IsInRole(AppRoles.Receptionist) && !User.IsInRole(AppRoles.Admin))
            {
                _logger.Information("منشی به داشبورد ادمین دسترسی ندارد؛ هدایت به داشبورد منشی.");
                return RedirectToAction("Index", "ReceptionistDashboard", new { area = "Admin" });
            }

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
