using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای جستجوی پزشکان
    /// </summary>
    [Authorize]
    public class DoctorSearchApiController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ILogger _logger;

        public DoctorSearchApiController(
            IAppointmentBookingService bookingService,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _logger = logger?.ForContext<DoctorSearchApiController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت لیست پزشکان در دسترس
        /// GET: /Patient/Api/DoctorSearch/GetAvailableDoctors
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAvailableDoctors(
            int? departmentId = null,
            string searchTerm = null)
        {
            try
            {
                var result = await _bookingService.GetAvailableDoctorsAsync(departmentId, searchTerm);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در دریافت لیست پزشکان" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    count = result.Data?.Count ?? 0
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات یک پزشک
        /// GET: /Patient/Api/DoctorSearch/GetDoctorDetails/{id}
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorDetails(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetDoctorDetailsAsync(id);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "پزشک یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", id);
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی در دسترس برای یک پزشک در یک تاریخ مشخص
        /// GET: /Patient/Api/DoctorSearch/GetAvailableTimeSlots
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAvailableTimeSlots(int id, DateTime date)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                if (date.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAvailableTimeSlotsAsync(id, date);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در دریافت اسلات‌های زمانی" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    count = result.Data?.Count ?? 0,
                    date = date.ToString("yyyy/MM/dd")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - پزشک: {DoctorId}, تاریخ: {Date}", id, date.ToString("yyyy/MM/dd"));
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی‌پذیری یک اسلات زمانی
        /// POST: /Patient/Api/DoctorSearch/CheckSlotAvailability
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckSlotAvailability(SlotAvailabilityRequest request)
        {
            try
            {
                if (request == null || request.DoctorId <= 0)
                {
                    return Json(new { success = false, message = "اطلاعات نامعتبر است" });
                }

                if (request.AppointmentDate.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" });
                }

                var result = await _bookingService.CheckSlotAvailabilityAsync(
                    request.DoctorId,
                    request.AppointmentDate,
                    request.StartTime,
                    request.EndTime);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در بررسی دسترسی‌پذیری" });
                }

                return Json(new
                {
                    success = true,
                    isAvailable = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی‌پذیری اسلات");
                return Json(new { success = false, message = "خطای سرور" });
            }
        }

        /// <summary>
        /// دریافت قیمت نوبت
        /// GET: /Patient/Api/DoctorSearch/GetAppointmentPrice
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAppointmentPrice(
            int id,
            int? serviceCategoryId = null)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAppointmentPriceAsync(id, serviceCategoryId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در محاسبه قیمت" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    price = result.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت نوبت - پزشک: {DoctorId}", id);
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }
    }

    /// <summary>
    /// مدل درخواست بررسی دسترسی‌پذیری اسلات
    /// </summary>
    public class SlotAvailabilityRequest
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
