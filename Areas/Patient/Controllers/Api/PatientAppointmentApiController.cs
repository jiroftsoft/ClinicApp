using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Filters;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای مدیریت نوبت‌های بیمار.
    /// ✅ از BasePatientController ارث می‌برد تا GetCurrentPatientIdAsync با صفحه نوبت‌های من یکسان باشد (رفع Unauthorized در جزئیات).
    /// </summary>
    [PatientRoleAuthorization]
    public class PatientAppointmentApiController : BasePatientController
    {
        private readonly IAppointmentBookingService _bookingService;

        public PatientAppointmentApiController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            ILogger logger,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
        }

        /// <summary>
        /// دریافت لیست نوبت‌های بیمار
        /// GET: /Patient/Api/PatientAppointment/GetAppointments
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAppointments(
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppointmentStatus? status = null)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetPatientAppointmentsAsync(patientId.Value, startDate, endDate);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در دریافت نوبت‌ها" }, JsonRequestBehavior.AllowGet);
                }

                // فیلتر بر اساس وضعیت
                var appointments = result.Data;
                if (status.HasValue)
                {
                    appointments = appointments.Where(a => a.Status == status.Value).ToList();
                }

                return Json(new
                {
                    success = true,
                    data = appointments,
                    count = appointments.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های بیمار");
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت جزئیات یک نوبت
        /// GET: /Patient/Api/PatientAppointment/GetAppointmentDetails/{id}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAppointmentDetails(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAppointmentDetailsAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// لغو نوبت
        /// POST: /Patient/Api/PatientAppointment/CancelAppointment/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CancelAppointment(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید." });
                }

                var result = await _bookingService.CancelAppointmentAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطای سرور" });
            }
        }

    }
}
