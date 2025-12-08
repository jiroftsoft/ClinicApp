using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces;
using ClinicApp.Models.DTOs.Appointment;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای رزرو نوبت
    /// </summary>
    [Authorize]
    public class AppointmentBookingApiController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public AppointmentBookingApiController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<AppointmentBookingApiController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// رزرو نوبت
        /// POST: /Patient/Api/AppointmentBooking/ReserveAppointment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ReserveAppointment(AppointmentBookingRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "اطلاعات رزرو نامعتبر است" });
                }

                // اعتبارسنجی
                if (request.DoctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" });
                }

                if (request.PatientId <= 0)
                {
                    // دریافت شناسه بیمار از کاربر فعلی
                    var patientId = await GetCurrentPatientIdAsync();
                    if (patientId == null)
                    {
                        return Json(new { success = false, message = "Unauthorized" });
                    }
                    request.PatientId = patientId.Value;
                }

                if (request.AppointmentDate.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" });
                }

                // بررسی حداقل زمان رزرو (2 ساعت قبل)
                var minimumBookingTime = DateTime.Now.AddHours(2);
                var appointmentDateTime = request.AppointmentDate.Date.Add(request.StartTime);
                if (appointmentDateTime < minimumBookingTime)
                {
                    return Json(new { success = false, message = "نوبت باید حداقل 2 ساعت قبل از زمان نوبت رزرو شود" });
                }

                var result = await _bookingService.ReserveAppointmentAsync(request);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در رزرو نوبت" });
                }

                return Json(new
                {
                    success = true,
                    message = "نوبت با موفقیت رزرو شد",
                    data = new
                    {
                        appointmentId = result.Data?.AppointmentId,
                        appointmentDate = result.Data?.AppointmentDate,
                        doctorId = result.Data?.DoctorId,
                        price = result.Data?.Price
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رزرو نوبت");
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
