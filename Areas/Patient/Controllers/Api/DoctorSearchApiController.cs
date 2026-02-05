using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Infrastructure; // ✅ برای ITimeProvider
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
        private readonly ITimeProvider _timeProvider; // ✅ برای استفاده از GetIranToday()

        public DoctorSearchApiController(
            IAppointmentBookingService bookingService,
            ITimeProvider timeProvider,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
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
                
                // ✅ CRITICAL FIX: بهبود منطق parse برای تشخیص شمسی/میلادی
                // اگر سال > 2000 باشد، احتمالاً میلادی است
                // اگر سال < 2000 باشد، احتمالاً شمسی است
                var normalizedDate = date.Trim();
                var separators = new[] { '/', '-', '.', ' ' };
                var parts = normalizedDate.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length >= 3 && int.TryParse(parts[0], out int year))
                {
                    if (year > 2000)
                    {
                        // ✅ احتمالاً میلادی است
                        if (DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                        {
                            parsedDate = gregorianDate.Date;
                            _logger.Debug("تاریخ به صورت میلادی parse شد: {Date} -> {ParsedDate}", date, parsedDate.Value.ToString("yyyy/MM/dd"));
                        }
                        else
                        {
                            // اگر parse نشد، سعی می‌کنیم به صورت شمسی parse کنیم
                            parsedDate = PersianDateHelper.ParsePersianDate(date);
                            if (parsedDate.HasValue)
                            {
                                _logger.Debug("تاریخ به صورت شمسی parse شد (fallback): {Date} -> {ParsedDate}", date, parsedDate.Value.ToString("yyyy/MM/dd"));
                            }
                        }
                    }
                    else
                    {
                        // ✅ احتمالاً شمسی است
                        parsedDate = PersianDateHelper.ParsePersianDate(date);
                        if (parsedDate.HasValue)
                        {
                            _logger.Debug("تاریخ به صورت شمسی parse شد: {Date} -> {ParsedDate}", date, parsedDate.Value.ToString("yyyy/MM/dd"));
                        }
                        else
                        {
                            // اگر parse نشد، سعی می‌کنیم به صورت میلادی parse کنیم (fallback)
                            if (DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, 
                                System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                            {
                                parsedDate = gregorianDate.Date;
                                _logger.Debug("تاریخ به صورت میلادی parse شد (fallback): {Date} -> {ParsedDate}", date, parsedDate.Value.ToString("yyyy/MM/dd"));
                            }
                        }
                    }
                }
                else
                {
                    // ✅ اگر نتوانستیم سال را parse کنیم، از منطق قبلی استفاده می‌کنیم
                    if (DateTime.TryParse(date, System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out DateTime gregorianDate))
                    {
                        parsedDate = gregorianDate.Date;
                    }
                    else
                    {
                        parsedDate = PersianDateHelper.ParsePersianDate(date);
                    }
                }

                if (!parsedDate.HasValue)
                {
                    return Json(new { success = false, message = "فرمت تاریخ نامعتبر است. لطفاً از فرمت yyyy-MM-dd یا yyyy/MM/dd استفاده کنید" }, JsonRequestBehavior.AllowGet);
                }

                var appointmentDate = parsedDate.Value;

                // ✅ CRITICAL FIX: استفاده از GetIranToday() به جای DateTime.Today
                // DateTime.Today timezone-dependent است و ممکن است تاریخ اشتباه برگرداند
                var iranToday = _timeProvider.GetIranToday();
                if (appointmentDate.Date < iranToday)
                {
                    _logger.Warning("⚠️ تاریخ گذشته رد شد - تاریخ درخواست: {RequestDate}, تاریخ امروز ایران: {IranToday}",
                        appointmentDate.ToString("yyyy/MM/dd"), iranToday.ToString("yyyy/MM/dd"));
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
                // ✅ CRITICAL FIX: Log request برای دیباگ
                _logger.Debug("🔍 CheckSlotAvailability - Request: DoctorId={DoctorId}, AppointmentDate={AppointmentDate}, StartTime={StartTime}, EndTime={EndTime}",
                    request?.DoctorId ?? 0, request?.AppointmentDate.ToString("yyyy/MM/dd") ?? "null", 
                    request?.StartTime.ToString(@"hh\:mm") ?? "null", request?.EndTime.ToString(@"hh\:mm") ?? "null");
                
                // ✅ CRITICAL FIX: Log form data برای دیباگ
                var formDoctorId = Request.Form["doctorId"] ?? Request["doctorId"];
                var formAppointmentDate = Request.Form["appointmentDate"] ?? Request["appointmentDate"];
                var formStartTime = Request.Form["startTime"] ?? Request["startTime"];
                var formEndTime = Request.Form["endTime"] ?? Request["endTime"];
                _logger.Debug("🔍 CheckSlotAvailability - Form Data: doctorId={DoctorId}, appointmentDate={AppointmentDate}, startTime={StartTime}, endTime={EndTime}",
                    formDoctorId, formAppointmentDate, formStartTime, formEndTime);
                
                // ✅ اگر request null است یا DoctorId نامعتبر است، سعی می‌کنیم از form data بخوانیم
                if (request == null || request.DoctorId <= 0 || request.StartTime == TimeSpan.Zero || request.EndTime == TimeSpan.Zero)
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

                    // ✅ CRITICAL FIX: Parse زمان با پشتیبانی از فرمت hh:mm و HH:mm
                    // فرمت hh:mm (مثلاً "10:45") باید به TimeSpan تبدیل شود
                    TimeSpan startTime, endTime;
                    
                    // ✅ ابتدا سعی می‌کنیم با parse دستی (برای فرمت hh:mm)
                    var startParts = startTimeStr.Split(':');
                    var endParts = endTimeStr.Split(':');
                    
                    if (startParts.Length >= 2 && int.TryParse(startParts[0], out int startHours) && 
                        int.TryParse(startParts[1], out int startMinutes) &&
                        endParts.Length >= 2 && int.TryParse(endParts[0], out int endHours) && 
                        int.TryParse(endParts[1], out int endMinutes))
                    {
                        // ✅ اعتبارسنجی hours و minutes
                        if (startHours >= 0 && startHours < 24 && startMinutes >= 0 && startMinutes < 60 &&
                            endHours >= 0 && endHours < 24 && endMinutes >= 0 && endMinutes < 60)
                        {
                            startTime = new TimeSpan(startHours, startMinutes, 0);
                            endTime = new TimeSpan(endHours, endMinutes, 0);
                            _logger.Debug("✅ Parse موفق (manual hh:mm): StartTime='{StartTimeStr}' -> {StartTime}, EndTime='{EndTimeStr}' -> {EndTime}",
                                startTimeStr, startTime, endTimeStr, endTime);
                        }
                        else
                        {
                            return Json(new { success = false, message = "فرمت زمان نامعتبر است (مقادیر خارج از محدوده)" });
                        }
                    }
                    else if (!TimeSpan.TryParse(startTimeStr, out startTime) ||
                             !TimeSpan.TryParse(endTimeStr, out endTime))
                    {
                        // ✅ اگر parse دستی کار نکرد، سعی می‌کنیم با TimeSpan.TryParse
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

                // ✅ CRITICAL FIX: استفاده از GetIranToday() به جای DateTime.Today
                // DateTime.Today timezone-dependent است و ممکن است تاریخ اشتباه برگرداند
                var iranToday = _timeProvider.GetIranToday();
                if (request.AppointmentDate.Date < iranToday)
                {
                    _logger.Warning("⚠️ تاریخ گذشته رد شد - تاریخ درخواست: {RequestDate}, تاریخ امروز ایران: {IranToday}",
                        request.AppointmentDate.ToString("yyyy/MM/dd"), iranToday.ToString("yyyy/MM/dd"));
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" });
                }

                _logger.Debug("🔍 فراخوانی CheckSlotAvailabilityAsync - DoctorId: {DoctorId}, AppointmentDate: {AppointmentDate}, StartTime: {StartTime}, EndTime: {EndTime}",
                    request.DoctorId, request.AppointmentDate.ToString("yyyy/MM/dd"), request.StartTime, request.EndTime);
                
                var result = await _bookingService.CheckSlotAvailabilityAsync(
                    request.DoctorId,
                    request.AppointmentDate,
                    request.StartTime,
                    request.EndTime);

                _logger.Debug("✅ نتیجه CheckSlotAvailabilityAsync - Success: {Success}, IsAvailable: {IsAvailable}, Message: {Message}",
                    result.Success, result.Data, result.Message);

                if (!result.Success)
                {
                    _logger.Warning("⚠️ CheckSlotAvailabilityAsync ناموفق - DoctorId: {DoctorId}, Date: {Date}, Message: {Message}",
                        request.DoctorId, request.AppointmentDate.ToString("yyyy/MM/dd"), result.Message);
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
                _logger.Error(ex, "❌ EXCEPTION در CheckSlotAvailability - ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                    ex.GetType().Name, ex.Message, ex.StackTrace);
                return Json(new { success = false, message = $"خطای سرور: {ex.Message}" });
            }
        }

        /// <summary>
        /// دریافت قیمت نوبت (شامل تخفیف ایونت تبلیغاتی در صورت ارسال تاریخ نوبت)
        /// GET: /Patient/Api/DoctorSearch/GetAppointmentPrice
        /// ✅ AllowAnonymous: کاربران می‌توانند قبل از login قیمت نوبت را ببینند
        /// </summary>
        /// <param name="appointmentDate">تاریخ نوبت (اختیاری؛ برای اعمال صحیح تخفیف ایونت مثلاً عید نوروز)</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAppointmentPrice(
            int id,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAppointmentPriceAsync(id, serviceCategoryId, appointmentDate);

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

        /// <summary>
        /// دریافت جزئیات قیمت نوبت (پایه، تخفیف، نهایی) برای نمایش در صفحه انتخاب نوبت
        /// GET: /Patient/Api/DoctorSearch/GetAppointmentPriceBreakdown
        /// </summary>
        /// <param name="date">تاریخ نوبت به صورت شمسی (مثلاً 1404/11/17) یا میلادی</param>
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAppointmentPriceBreakdown(
            int id,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null,
            string date = null)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" }, JsonRequestBehavior.AllowGet);
                }

                // پشتیبانی از تاریخ شمسی از فرانت (مثلاً 1404/11/17)
                if (appointmentDate == null && !string.IsNullOrWhiteSpace(date))
                {
                    appointmentDate = this.ParsePersianDateSafe(date.Trim(), _logger);
                }

                var result = await _bookingService.GetAppointmentPriceBreakdownAsync(id, serviceCategoryId, appointmentDate);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "خطا در محاسبه قیمت" }, JsonRequestBehavior.AllowGet);
                }

                var d = result.Data;
                return Json(new
                {
                    success = true,
                    basePrice = d.BasePrice,
                    discountAmount = d.DiscountAmount,
                    discountPercentage = d.DiscountPercentage,
                    finalPrice = d.FinalPrice,
                    promotionalEventTitle = d.PromotionalEventTitle ?? "",
                    hasDiscount = d.HasDiscount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات قیمت نوبت - پزشک: {DoctorId}", id);
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
