using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models;
using ClinicApp.ViewModels.OnlineConsultation;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// ورود بیمار به اتاق مشاوره آنلاین تصویری (Jitsi) — بهینه‌سازی شده برای پروداکشن درمانی.
    /// </summary>
    public class OnlineConsultationController : Base.BasePatientController
    {
        private readonly IOnlineConsultationService _consultationService;
        private readonly ILogger _logger;

        public OnlineConsultationController(
            IOnlineConsultationService consultationService,
            ILogger logger,
            ClinicApp.Interfaces.ICurrentUserService currentUserService,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
        {
            _consultationService = consultationService ?? throw new System.ArgumentNullException(nameof(consultationService));
            _logger = logger?.ForContext<OnlineConsultationController>() ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET Patient/Consultation/Join/{id} یا Patient/OnlineConsultation/Join/{id} — ورود بیمار به اتاق مشاوره.
        /// همیشه صفحه Join را برمی‌گرداند؛ در صورت عدم امکان ورود، پیام امن نمایش داده می‌شود (بدون 404).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Join(int id)
        {
            if (id <= 0 || id > int.MaxValue)
            {
                _logger.Warning("OnlineConsultation Join: شناسه نوبت نامعتبر - Id: {Id}", id);
                return HttpNotFound();
            }

            var patientId = await GetCurrentPatientIdAsync();
            if (!patientId.HasValue)
            {
                _logger.Warning("OnlineConsultation Join: کاربر جاری بیمار نیست یا لاگین نشده - AppointmentId: {AppointmentId}", id);
                return new HttpUnauthorizedResult();
            }

            JoinConsultationViewModel model;
            try
            {
                model = await _consultationService.GetOrCreateRoomForPatientAsync(id, patientId.Value);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex, "OnlineConsultation Join: خطا در سرویس - AppointmentId: {AppointmentId}", id);
                model = new JoinConsultationViewModel
                {
                    AppointmentId = id,
                    CanJoin = false,
                    UserMessage = "خطا در بارگذاری اتاق. لطفاً بعداً تلاش کنید یا با پشتیبانی تماس بگیرید."
                };
            }

            if (model == null)
            {
                model = new JoinConsultationViewModel
                {
                    AppointmentId = id,
                    CanJoin = false,
                    UserMessage = "اتاق مشاوره در دسترس نیست."
                };
            }

            return View(model);
        }
    }
}
