using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
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
        private readonly IReceptionService _receptionService;

        public ReceptionistDashboardController(IReceptionService receptionService)
        {
            _receptionService = receptionService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("داشبورد منشی توسط {User}", User?.Identity?.Name);

                var todayStats = new WidgetTodayStatsViewModel();
                var todayAppointments = new WidgetTodayAppointmentsReceptionViewModel { Count = 0 };
                var pendingReceptions = new WidgetPendingReceptionsViewModel { Count = 0 };
                var todayDoctors = new WidgetTodayDoctorsViewModel { Count = 0 };

                var statsResult = await _receptionService.GetDailyStatsAsync(DateTime.Today).ConfigureAwait(false);
                if (statsResult?.Success == true && statsResult.Data != null)
                {
                    var s = statsResult.Data;
                    todayStats = new WidgetTodayStatsViewModel
                    {
                        ReceptionsToday = s.TotalReceptions,
                        RevenueToday = s.TotalRevenue
                    };
                    todayAppointments = new WidgetTodayAppointmentsReceptionViewModel
                    {
                        Count = s.TotalReceptions,
                        Items = new System.Collections.Generic.List<TodayAppointmentReceptionItemViewModel>()
                    };
                    pendingReceptions = new WidgetPendingReceptionsViewModel
                    {
                        Count = s.PendingReceptions,
                        Items = new System.Collections.Generic.List<PendingReceptionItemViewModel>()
                    };
                    if (s.DoctorStats != null && s.DoctorStats.Count > 0)
                    {
                        todayDoctors = new WidgetTodayDoctorsViewModel
                        {
                            Count = s.DoctorStats.Count,
                            Items = s.DoctorStats.Select(d => new TodayDoctorItemViewModel
                            {
                                DoctorName = d.DoctorName,
                                DepartmentName = d.Specialty ?? ""
                            }).ToList()
                        };
                    }
                }

                var model = new ReceptionistDashboardPageViewModel
                {
                    QuickActions = new WidgetReceptionQuickActionsViewModel(),
                    TodayAppointments = todayAppointments,
                    PendingReceptions = pendingReceptions,
                    TodayDoctors = todayDoctors,
                    TodayStats = todayStats
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
