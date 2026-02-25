using System;
using System.Web.Mvc;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.Admin.ReceptionistDashboard;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// داشبورد اختصاصی نقش منشی — قابل کاستومایز کامل.
    /// فقط دسترسی Admin و Receptionist.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    public class ReceptionistDashboardController : Controller
    {
        private readonly ILogger _logger = Log.ForContext<ReceptionistDashboardController>();

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                _logger.Information("داشبورد منشی توسط {User}", User?.Identity?.Name);

                var model = new ReceptionistDashboardPageViewModel
                {
                    QuickActions = new WidgetReceptionQuickActionsViewModel(),
                    TodayAppointments = new WidgetTodayAppointmentsReceptionViewModel { Count = 0 },
                    PendingReceptions = new WidgetPendingReceptionsViewModel { Count = 0 },
                    TodayDoctors = new WidgetTodayDoctorsViewModel { Count = 0 },
                    TodayStats = new WidgetTodayStatsViewModel()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری داشبورد منشی");
                return View(new ReceptionistDashboardPageViewModel());
            }
        }
    }
}
