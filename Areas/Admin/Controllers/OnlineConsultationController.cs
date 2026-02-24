using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.ViewModels.OnlineConsultation;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// ورود پزشک/ادمین به اتاق مشاوره آنلاین تصویری (Jitsi)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class OnlineConsultationController : Controller
    {
        private readonly IOnlineConsultationService _consultationService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger _logger;

        public OnlineConsultationController(
            IOnlineConsultationService consultationService,
            IAppointmentRepository appointmentRepository,
            ILogger logger)
        {
            _consultationService = consultationService ?? throw new System.ArgumentNullException(nameof(consultationService));
            _appointmentRepository = appointmentRepository ?? throw new System.ArgumentNullException(nameof(appointmentRepository));
            _logger = logger?.ForContext<OnlineConsultationController>() ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET Admin/OnlineConsultation/Join/123 — ورود به اتاق مشاوره (پزشک/ادمین). اعتبارسنجی و بازه زمانی.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Join(int id)
        {
            if (id <= 0 || id > int.MaxValue)
            {
                _logger.Warning("Admin OnlineConsultation Join: شناسه نوبت نامعتبر");
                return HttpNotFound();
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null || appointment.IsDeleted || !appointment.IsOnlineConsultation)
            {
                return HttpNotFound();
            }

            var model = await _consultationService.GetOrCreateRoomForDoctorAsync(id, appointment.DoctorId);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View("~/Areas/Patient/Views/OnlineConsultation/Join.cshtml", model);
        }
    }
}
