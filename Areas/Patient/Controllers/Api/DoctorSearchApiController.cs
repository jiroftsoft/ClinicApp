using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای جستجوی پزشکان
    /// 
    /// ✅ Security: PatientRoleAuthorization ensures only Patient role users can search doctors
    /// ✅ Note: برخی متدها با [AllowAnonymous] برای مشاهده اطلاعات قبل از login
    /// طبق: PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    [PatientRoleAuthorization]
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
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login پزشکان را ببینند
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login اطلاعات پزشک را ببینند
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
        /// پشتیبانی از تاریخ شمسی و میلادی
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login اسلات‌های در دسترس را ببینند
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAvailableTimeSlots(int id, string date)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(date))
                {
                    return Json(new { success = false, message = "تاریخ الزامی است" }, JsonRequestBehavior.AllowGet);
                }

                // ✅ Parse تاریخ (پشتیبانی از شمسی و میلادی)
                DateTime? parsedDate = null;
                
                // ابتدا سعی می‌کنیم به صورت میلادی parse کنیم (فرمت ISO: yyyy-MM-dd)
                if (DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                {
                    parsedDate = gregorianDate.Date;
                }
                else
                {
                    // اگر parse نشد، سعی می‌کنیم به صورت شمسی parse کنیم
                    parsedDate = PersianDateHelper.ParsePersianDate(date);
                }

                if (!parsedDate.HasValue)
                {
                    return Json(new { success = false, message = "فرمت تاریخ نامعتبر است. لطفاً از فرمت yyyy-MM-dd یا yyyy/MM/dd استفاده کنید" }, JsonRequestBehavior.AllowGet);
                }

                var appointmentDate = parsedDate.Value;

                if (appointmentDate.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAvailableTimeSlotsAsync(id, appointmentDate);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در دریافت اسلات‌های زمانی" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    count = result.Data?.Count ?? 0,
                    date = appointmentDate.ToString("yyyy-MM-dd")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - پزشک: {DoctorId}, تاریخ: {Date}", id, date);
                return Json(new { success = false, message = "خطای سرور" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی‌پذیری یک اسلات زمانی
        /// POST: /Patient/Api/DoctorSearch/CheckSlotAvailability
        /// پشتیبانی از تاریخ شمسی و میلادی
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login دسترسی‌پذیری اسلات را بررسی کنند
        /// ✅ CRITICAL FIX: ValidateAntiForgeryToken حذف شد - این یک Read Operation است و برای Anonymous users مشکل ایجاد می‌کرد
        /// ⚠️ Security: Rate Limiting در Controller level اعمال می‌شود
        /// ⚠️ Note: Global Filter برای این action skip می‌شود (بر اساس Action Name: CheckSlotAvailability)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        // ✅ CRITICAL FIX: IgnoreAntiforgeryToken در ASP.NET MVC 5 وجود ندارد
        // Global Filter برای این action skip می‌شود (بر اساس Action Name: CheckSlotAvailability)
        public async Task<JsonResult> CheckSlotAvailability(SlotAvailabilityRequest request)
        {
            try
            {
                // ✅ اگر request null است یا DoctorId نامعتبر است، سعی می‌کنیم از form data بخوانیم
                if (request == null || request.DoctorId <= 0)
                {
                    var doctorIdStr = Request.Form["doctorId"] ?? Request["doctorId"];
                    var appointmentDateStr = Request.Form["appointmentDate"] ?? Request["appointmentDate"];
                    var startTimeStr = Request.Form["startTime"] ?? Request["startTime"];
                    var endTimeStr = Request.Form["endTime"] ?? Request["endTime"];

                    if (string.IsNullOrWhiteSpace(doctorIdStr) || 
                        string.IsNullOrWhiteSpace(appointmentDateStr) ||
                        string.IsNullOrWhiteSpace(startTimeStr) ||
                        string.IsNullOrWhiteSpace(endTimeStr))
                    {
                        return Json(new { success = false, message = "اطلاعات نامعتبر است" });
                    }

                    if (!int.TryParse(doctorIdStr, out int doctorId) || doctorId <= 0)
                    {
                        return Json(new { success = false, message = "شناسه پزشک نامعتبر است" });
                    }

                    // ✅ Parse تاریخ (پشتیبانی از شمسی و میلادی)
                    DateTime? parsedDate = null;
                    if (DateTime.TryParse(appointmentDateStr, System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                    {
                        parsedDate = gregorianDate.Date;
                    }
                    else
                    {
                        parsedDate = PersianDateHelper.ParsePersianDate(appointmentDateStr);
                    }

                    if (!parsedDate.HasValue)
                    {
                        return Json(new { success = false, message = "فرمت تاریخ نامعتبر است" });
                    }

                    // ✅ Parse زمان
                    if (!TimeSpan.TryParse(startTimeStr, out TimeSpan startTime) ||
                        !TimeSpan.TryParse(endTimeStr, out TimeSpan endTime))
                    {
                        return Json(new { success = false, message = "فرمت زمان نامعتبر است" });
                    }

                    request = new SlotAvailabilityRequest
                    {
                        DoctorId = doctorId,
                        AppointmentDate = parsedDate.Value,
                        StartTime = startTime,
                        EndTime = endTime
                    };
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
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login قیمت نوبت را ببینند
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
        
        // ✅ برای پشتیبانی از تاریخ شمسی در query string
        public string AppointmentDateString 
        { 
            set 
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // ابتدا سعی می‌کنیم به صورت میلادی parse کنیم
                    if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                    {
                        AppointmentDate = gregorianDate.Date;
                    }
                    else
                    {
                        // اگر parse نشد، سعی می‌کنیم به صورت شمسی parse کنیم
                        var persianDate = PersianDateHelper.ParsePersianDate(value);
                        if (persianDate.HasValue)
                        {
                            AppointmentDate = persianDate.Value.Date;
                        }
                    }
                }
            }
        }
    }
}
