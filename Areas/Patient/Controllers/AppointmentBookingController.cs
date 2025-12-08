using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment.Web;
using ClinicApp.Interfaces.Payment.Gateway;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.ViewModels.Patient;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Appointment;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Appointment;
using ClinicApp.Services;
using System.Data.Entity;
using ClinicApp.Filters;
using Serilog;
using System.Linq;
using System.Collections.Generic;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای رزرو نوبت آنلاین
    /// </summary>
    [Authorize]
    public class AppointmentBookingController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebPaymentService _webPaymentService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AppointmentBookingController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IWebPaymentService webPaymentService,
            IPaymentGatewayService paymentGatewayService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _webPaymentService = webPaymentService ?? throw new ArgumentNullException(nameof(webPaymentService));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<AppointmentBookingController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// صفحه اصلی رزرو نوبت - هدایت به انتخاب پزشک
        /// GET: /Patient/Appointment/Book
        /// </summary>
        [HttpGet]
        public ActionResult Book()
        {
            return RedirectToAction("SelectDoctor");
        }

        /// <summary>
        /// صفحه انتخاب پزشک
        /// GET: /Patient/Appointment/Book/SelectDoctor
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SelectDoctor(int? departmentId, string searchTerm)
        {
            try
            {
                _logger.Information("درخواست صفحه انتخاب پزشک - DepartmentId: {DepartmentId}, SearchTerm: {SearchTerm}",
                    departmentId, searchTerm);

                var result = await _bookingService.GetAvailableDoctorsAsync(departmentId, searchTerm);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "خطا در دریافت لیست پزشکان";
                    return View(new DoctorSelectionViewModel
                    {
                        Doctors = new List<DoctorSearchResultDto>(),
                        Departments = new List<DepartmentInfo>()
                    });
                }

                // TODO: دریافت لیست دپارتمان‌ها برای فیلتر
                var viewModel = new DoctorSelectionViewModel
                {
                    Doctors = result.Data,
                    SelectedDepartmentId = departmentId,
                    SearchTerm = searchTerm,
                    Departments = new System.Collections.Generic.List<DepartmentInfo>() // TODO: دریافت از سرویس
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه انتخاب پزشک");
                TempData["Error"] = "خطا در بارگذاری صفحه";
                return View(new DoctorSelectionViewModel
                {
                    Doctors = new List<DoctorSearchResultDto>(),
                    Departments = new List<DepartmentInfo>()
                });
            }
        }

        /// <summary>
        /// صفحه انتخاب تاریخ
        /// GET: /Patient/Appointment/Book/SelectDate/{doctorId}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SelectDate(int doctorId)
        {
            try
            {
                _logger.Information("درخواست صفحه انتخاب تاریخ - DoctorId: {DoctorId}", doctorId);

                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک یافت نشد";
                    return RedirectToAction("SelectDoctor");
                }

                var viewModel = new DateSelectionViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    DoctorSpecialization = doctorResult.Data.Specialization
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه انتخاب تاریخ");
                TempData["Error"] = "خطا در بارگذاری صفحه";
                return RedirectToAction("SelectDoctor");
            }
        }

        /// <summary>
        /// صفحه انتخاب زمان
        /// GET: /Patient/Appointment/Book/SelectTime/{doctorId}/{date}
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SelectTime(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست صفحه انتخاب زمان - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                if (date.Date < DateTime.Today)
                {
                    TempData["Error"] = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید";
                    return RedirectToAction("SelectDate", new { doctorId });
                }

                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک یافت نشد";
                    return RedirectToAction("SelectDoctor");
                }

                var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId, date);
                if (!slotsResult.Success)
                {
                    TempData["Error"] = slotsResult.Message ?? "خطا در دریافت اسلات‌های زمانی";
                    return RedirectToAction("SelectDate", new { doctorId });
                }

                var viewModel = new TimeSlotSelectionViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    SelectedDate = date,
                    AvailableSlots = slotsResult.Data,
                    AppointmentDuration = 30 // TODO: از تنظیمات پزشک دریافت شود
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه انتخاب زمان");
                TempData["Error"] = "خطا در بارگذاری صفحه";
                return RedirectToAction("SelectDate", new { doctorId });
            }
        }

        /// <summary>
        /// صفحه تایید و پرداخت
        /// GET: /Patient/Appointment/Book/Confirm
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ConfirmBooking(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? serviceCategoryId = null,
            string description = null)
        {
            try
            {
                _logger.Information("درخواست صفحه تایید رزرو - DoctorId: {DoctorId}, Date: {Date}, Time: {StartTime}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime);

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    TempData["Error"] = "اطلاعات بیمار یافت نشد";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // بررسی دسترسی‌پذیری مجدد
                var availabilityCheck = await _bookingService.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                if (!availabilityCheck.Success || !availabilityCheck.Data)
                {
                    TempData["Error"] = "این زمان دیگر در دسترس نیست. لطفاً زمان دیگری انتخاب کنید";
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک یافت نشد";
                    return RedirectToAction("SelectDoctor");
                }

                var priceResult = await _bookingService.GetAppointmentPriceAsync(doctorId, serviceCategoryId);
                if (!priceResult.Success)
                {
                    TempData["Error"] = priceResult.Message ?? "خطا در محاسبه قیمت";
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                var viewModel = new AppointmentBookingViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    DoctorSpecialization = doctorResult.Data.Specialization,
                    AppointmentDate = appointmentDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = priceResult.Data,
                    ServiceCategoryId = serviceCategoryId,
                    Description = description
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه تایید رزرو");
                TempData["Error"] = "خطا در بارگذاری صفحه";
                return RedirectToAction("SelectDoctor");
            }
        }

        /// <summary>
        /// رزرو نوبت
        /// POST: /Patient/Appointment/Book/Reserve
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppointmentRateLimit(5, 60)] // حداکثر 5 رزرو در ساعت
        public async Task<ActionResult> Reserve(AppointmentBookingViewModel model)
        {
            try
            {
                _logger.Information("درخواست رزرو نوبت - DoctorId: {DoctorId}, Date: {Date}, Time: {StartTime}",
                    model.DoctorId, model.AppointmentDate.ToString("yyyy/MM/dd"), model.StartTime);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("SelectDoctor");
                }

                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد" });
                }

                var request = new AppointmentBookingRequestDto
                {
                    DoctorId = model.DoctorId,
                    AppointmentDate = model.AppointmentDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    ServiceCategoryId = model.ServiceCategoryId,
                    Description = model.Description,
                    PatientId = patientId.Value
                };

                var result = await _bookingService.ReserveAppointmentAsync(request);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                // TODO: در آینده پرداخت را از اینجا انجام می‌دهیم
                // فعلاً نوبت رزرو می‌شود و پرداخت بعداً انجام می‌شود
                TempData["Success"] = "نوبت با موفقیت رزرو شد. لطفاً برای تکمیل رزرو، پرداخت را انجام دهید.";
                return Json(new
                {
                    success = true,
                    message = "نوبت با موفقیت رزرو شد",
                    appointmentId = result.Data?.AppointmentId,
                    requiresPayment = true,
                    paymentUrl = Url.Action("ProcessPayment", new { appointmentId = result.Data?.AppointmentId })
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رزرو نوبت");
                return Json(new { success = false, message = "خطا در رزرو نوبت. لطفاً دوباره تلاش کنید" });
            }
        }

        /// <summary>
        /// پردازش پرداخت
        /// POST: /Patient/Appointment/Book/ProcessPayment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppointmentRateLimit(10, 60)] // حداکثر 10 درخواست پرداخت در ساعت
        public async Task<ActionResult> ProcessPayment(int appointmentId, string paymentMethod = "online")
        {
            try
            {
                _logger.Information("درخواست پردازش پرداخت - AppointmentId: {AppointmentId}, Method: {Method}",
                    appointmentId, paymentMethod);

                // 1. دریافت نوبت و بررسی دسترسی
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && !a.IsDeleted);

                if (appointment == null)
                {
                    _logger.Warning("نوبت {AppointmentId} یافت نشد", appointmentId);
                    return Json(new { success = false, message = "نوبت یافت نشد" });
                }

                var patient = await _currentUserService.GetPatientInfoAsync();
                if (patient == null || patient.PatientId != appointment.PatientId)
                {
                    _logger.Warning("دسترسی غیرمجاز به نوبت {AppointmentId} توسط بیمار {PatientId}",
                        appointmentId, patient?.PatientId);
                    return Json(new { success = false, message = "شما اجازه دسترسی به این نوبت را ندارید" });
                }

                // 2. بررسی وضعیت نوبت
                if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Pending)
                {
                    _logger.Warning("نوبت {AppointmentId} در وضعیت قابل پرداخت نیست. وضعیت: {Status}",
                        appointmentId, appointment.Status);
                    return Json(new { success = false, message = "این نوبت در وضعیت قابل پرداخت نیست" });
                }

                // 3. دریافت درگاه پیش‌فرض
                var defaultGatewayResult = await _paymentGatewayService.GetDefaultPaymentGatewayAsync();
                if (!defaultGatewayResult.Success || defaultGatewayResult.Data == null)
                {
                    _logger.Error("درگاه پرداخت پیش‌فرض یافت نشد");
                    return Json(new { success = false, message = "درگاه پرداخت در دسترس نیست. لطفاً با پشتیبانی تماس بگیرید" });
                }

                var gateway = defaultGatewayResult.Data;

                // 4. بررسی PatientId
                if (!appointment.PatientId.HasValue)
                {
                    _logger.Warning("نوبت {AppointmentId} دارای PatientId نیست", appointmentId);
                    return Json(new { success = false, message = "نوبت معتبر نیست" });
                }

                // 5. ایجاد OnlinePayment record
                var onlinePayment = new OnlinePayment
                {
                    PaymentGatewayId = gateway.PaymentGatewayId,
                    AppointmentId = appointmentId,
                    PatientId = appointment.PatientId.Value,
                    PaymentType = OnlinePaymentType.Appointment,
                    Status = OnlinePaymentStatus.Pending,
                    Amount = appointment.Price,
                    Description = $"پرداخت نوبت - پزشک: {appointment.Doctor?.FullName ?? "نامشخص"}",
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.OnlinePayments.Add(onlinePayment);
                await _context.SaveChangesAsync();

                _logger.Information("OnlinePayment ایجاد شد - OnlinePaymentId: {OnlinePaymentId}, Amount: {Amount}",
                    onlinePayment.OnlinePaymentId, onlinePayment.Amount);

                // 6. ایجاد درخواست پرداخت
                var callbackUrl = Url.Action("PaymentCallback", "AppointmentBooking", new { area = "Patient" }, Request.Url.Scheme);
                var userIpAddress = Request.UserHostAddress;
                var userAgent = Request.UserAgent;

                var paymentRequest = new CreatePaymentRequest
                {
                    OnlinePaymentId = onlinePayment.OnlinePaymentId,
                    GatewayType = gateway.GatewayType,
                    Amount = appointment.Price,
                    Description = $"پرداخت نوبت - {appointment.Doctor?.FullName ?? "نامشخص"}",
                    CallbackUrl = callbackUrl,
                    UserIpAddress = userIpAddress,
                    UserAgent = userAgent,
                    AdditionalData = new Dictionary<string, string>
                    {
                        { "AppointmentId", appointmentId.ToString() },
                        { "PatientId", appointment.PatientId.Value.ToString() },
                        { "DoctorId", appointment.DoctorId.ToString() }
                    }
                };

                // 7. فراخوانی سرویس پرداخت
                var paymentResult = await _webPaymentService.CreatePaymentRequestAsync(paymentRequest);

                if (!paymentResult.Success || paymentResult.Data == null)
                {
                    _logger.Error("خطا در ایجاد درخواست پرداخت - {ErrorMessage}",
                        paymentResult.Message);
                    
                    // به‌روزرسانی وضعیت OnlinePayment
                    onlinePayment.Status = OnlinePaymentStatus.Failed;
                    onlinePayment.Description = $"خطا در ایجاد درخواست پرداخت: {paymentResult.Message}";
                    await _context.SaveChangesAsync();

                    return Json(new { success = false, message = paymentResult.Message ?? "خطا در ایجاد درخواست پرداخت" });
                }

                var gatewayResponse = paymentResult.Data;

                if (!gatewayResponse.Success || string.IsNullOrEmpty(gatewayResponse.PaymentUrl))
                {
                    _logger.Error("درگاه پرداخت پاسخ نامعتبر داد - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                        gatewayResponse.ErrorCode, gatewayResponse.ErrorMessage);

                    onlinePayment.Status = OnlinePaymentStatus.Failed;
                    onlinePayment.Description = $"خطا در درگاه: {gatewayResponse.ErrorMessage}";
                    await _context.SaveChangesAsync();

                    return Json(new { success = false, message = gatewayResponse.ErrorMessage ?? "خطا در درگاه پرداخت" });
                }

                // 8. به‌روزرسانی OnlinePayment با PaymentToken
                onlinePayment.PaymentToken = gatewayResponse.PaymentToken;
                onlinePayment.GatewayTransactionId = gatewayResponse.GatewayTransactionId;
                await _context.SaveChangesAsync();

                _logger.Information("درخواست پرداخت با موفقیت ایجاد شد - OnlinePaymentId: {OnlinePaymentId}, PaymentUrl: {PaymentUrl}",
                    onlinePayment.OnlinePaymentId, gatewayResponse.PaymentUrl);

                // 9. هدایت به درگاه پرداخت
                return Json(new
                {
                    success = true,
                    paymentUrl = gatewayResponse.PaymentUrl,
                    paymentToken = gatewayResponse.PaymentToken,
                    message = "در حال هدایت به درگاه پرداخت..."
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت - AppointmentId: {AppointmentId}", appointmentId);
                return Json(new { success = false, message = "خطا در پردازش پرداخت. لطفاً دوباره تلاش کنید" });
            }
        }

        /// <summary>
        /// Callback از درگاه پرداخت
        /// GET: /Patient/Appointment/Book/PaymentCallback
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // درگاه ممکن است از خارج از سیستم فراخوانی شود
        public async Task<ActionResult> PaymentCallback(
            string token,
            string status,
            string authority,
            string refId = null)
        {
            try
            {
                _logger.Information("دریافت Callback از درگاه - Token: {Token}, Status: {Status}, Authority: {Authority}",
                    token, status, authority);

                // 1. دریافت OnlinePayment بر اساس PaymentToken
                if (string.IsNullOrEmpty(token))
                {
                    _logger.Warning("PaymentToken در Callback موجود نیست");
                    TempData["Error"] = "اطلاعات پرداخت نامعتبر است";
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                var onlinePayment = await _context.OnlinePayments
                    .Include(op => op.Appointment)
                    .Include(op => op.PaymentGateway)
                    .FirstOrDefaultAsync(op => op.PaymentToken == token && !op.IsDeleted);

                if (onlinePayment == null)
                {
                    _logger.Warning("OnlinePayment با PaymentToken {Token} یافت نشد", token);
                    TempData["Error"] = "پرداخت یافت نشد";
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                // 2. بررسی دسترسی بیمار (اگر کاربر لاگین کرده است)
                if (User.Identity.IsAuthenticated)
                {
                    var patient = await _currentUserService.GetPatientInfoAsync();
                    if (patient == null || patient.PatientId != onlinePayment.PatientId)
                    {
                        _logger.Warning("دسترسی غیرمجاز به OnlinePayment {OnlinePaymentId} توسط بیمار {PatientId}",
                            onlinePayment.OnlinePaymentId, patient?.PatientId);
                        TempData["Error"] = "شما اجازه دسترسی به این پرداخت را ندارید";
                        return RedirectToAction("MyAppointments", "Appointment");
                    }
                }

                // 3. ساخت PaymentCallbackData از QueryString
                var callbackData = new PaymentCallbackData
                {
                    PaymentToken = token,
                    TransactionId = authority,
                    ReferenceCode = refId,
                    Status = status,
                    AdditionalData = new Dictionary<string, string>()
                };

                // اضافه کردن تمام QueryString parameters به AdditionalData
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        callbackData.AdditionalData[key] = Request.QueryString[key];
                    }
                }

                // 4. پردازش Callback
                var callbackResult = await _webPaymentService.ProcessPaymentCallbackAsync(
                    onlinePayment.PaymentGateway.GatewayType,
                    callbackData);

                if (!callbackResult.Success || callbackResult.Data == null)
                {
                    _logger.Error("خطا در پردازش Callback - {ErrorMessage}",
                        callbackResult.Message);

                    TempData["Error"] = callbackResult.Message ?? "خطا در پردازش پرداخت";
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                var result = callbackResult.Data;

                // 5. به‌روزرسانی Appointment.Status
                if (result.Success && result.Status == OnlinePaymentStatus.Success)
                {
                    var appointment = await _context.Appointments
                        .FirstOrDefaultAsync(a => a.AppointmentId == onlinePayment.AppointmentId && !a.IsDeleted);

                    if (appointment != null)
                    {
                        appointment.Status = AppointmentStatus.Scheduled;
                        appointment.PaymentTransactionId = result.PaymentTransactionId;
                        appointment.UpdatedAt = DateTime.Now;
                        appointment.UpdatedByUserId = _currentUserService.UserId;

                        await _context.SaveChangesAsync();

                        _logger.Information("نوبت {AppointmentId} با موفقیت پرداخت شد - OnlinePaymentId: {OnlinePaymentId}",
                            appointment.AppointmentId, onlinePayment.OnlinePaymentId);

                        // ارسال اعلان پرداخت موفق (به صورت Async - بدون انتظار)
                        try
                        {
                            var notificationService = new AppointmentNotificationService(
                                _context,
                                new EmailService(),
                                new AsanakSmsService(),
                                _logger);

                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    await notificationService.SendPaymentConfirmationAsync(appointment.AppointmentId);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex, "خطا در ارسال اعلان پرداخت - AppointmentId: {AppointmentId}",
                                        appointment.AppointmentId);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "خطا در ایجاد سرویس اعلان - AppointmentId: {AppointmentId}",
                                appointment.AppointmentId);
                        }

                        TempData["Success"] = "پرداخت با موفقیت انجام شد. نوبت شما رزرو شد.";
                        return RedirectToAction("MyAppointments", "Appointment");
                    }
                }
                else
                {
                    _logger.Warning("پرداخت ناموفق - OnlinePaymentId: {OnlinePaymentId}, Status: {Status}, Error: {Error}",
                        onlinePayment.OnlinePaymentId, result.Status, result.ErrorMessage);

                    TempData["Error"] = result.ErrorMessage ?? "پرداخت ناموفق بود";
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                // Fallback
                TempData["Error"] = "خطا در پردازش پرداخت";
                return RedirectToAction("MyAppointments", "Appointment");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش Callback پرداخت");
                TempData["Error"] = "خطا در پردازش پرداخت. لطفاً با پشتیبانی تماس بگیرید";
                return RedirectToAction("MyAppointments", "Appointment");
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

