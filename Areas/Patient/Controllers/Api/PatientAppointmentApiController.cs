using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای مدیریت نوبت‌های بیمار
    /// </summary>
    [Authorize]
    public class PatientAppointmentApiController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PatientAppointmentApiController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<PatientAppointmentApiController>() ?? throw new ArgumentNullException(nameof(logger));
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
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { success = false, message = "Unauthorized" });
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

        #region Helper Methods

        private async Task<int?> GetCurrentPatientIdAsync()
        {
            try
            {
                var patient = await _currentUserService.GetPatientInfoAsync();
                return patient?.PatientId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شناسه بیمار");
                return null;
            }
        }

        #endregion
    }
}
