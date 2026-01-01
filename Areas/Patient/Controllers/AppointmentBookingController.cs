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
using ClinicApp.Services.Idempotency; // ✅ Idempotency
using System.Data.Entity;
using ClinicApp.Filters;
using Serilog;
using System.Linq;
using System.Collections.Generic;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای رزرو نوبت آنلاین
    /// 
    /// ✅ Security: PatientRoleAuthorization ensures only Patient role users can book appointments
    /// طبق: PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    [PatientRoleAuthorization]
    public class AppointmentBookingController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWebPaymentService _webPaymentService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IIdempotencyService _idempotencyService; // ✅ Idempotency
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public AppointmentBookingController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IWebPaymentService webPaymentService,
            IPaymentGatewayService paymentGatewayService,
            IIdempotencyService idempotencyService, // ✅ Idempotency
            ApplicationDbContext context,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _webPaymentService = webPaymentService ?? throw new ArgumentNullException(nameof(webPaymentService));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _idempotencyService = idempotencyService ?? throw new ArgumentNullException(nameof(idempotencyService));
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
        /// ✅ CRITICAL FIX: Use [Authorize] instead of manual check to ensure authentication state is synchronized
        /// </summary>
        [HttpGet]
        [Authorize] // ✅ CRITICAL FIX: Let MVC authorization middleware handle authentication (ensures cookie is validated)
        public async Task<ActionResult> SelectDoctor(int? departmentId, string searchTerm)
        {
            try
            {
                _logger.Information("درخواست صفحه انتخاب پزشک - DepartmentId: {DepartmentId}, SearchTerm: {SearchTerm}",
                    departmentId, searchTerm);

                // ✅ CRITICAL FIX: Removed manual authentication check
                // [Authorize] attribute ensures User.Identity.IsAuthenticated is true before action executes
                // This prevents race condition between cookie set and validation

                var result = await _bookingService.GetAvailableDoctorsAsync(departmentId, searchTerm);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message ?? "خطا در دریافت لیست پزشکان");
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
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
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
                    NotificationHelper.SetError(TempData, "پزشک یافت نشد");
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
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
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
                    NotificationHelper.SetError(TempData, "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
                    return RedirectToAction("SelectDate", new { doctorId });
                }

                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    NotificationHelper.SetError(TempData, "پزشک یافت نشد");
                    return RedirectToAction("SelectDoctor");
                }

                var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId, date);
                if (!slotsResult.Success)
                {
                    NotificationHelper.SetError(TempData, slotsResult.Message ?? "خطا در دریافت اسلات‌های زمانی");
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
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
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
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // بررسی دسترسی‌پذیری مجدد
                var availabilityCheck = await _bookingService.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                if (!availabilityCheck.Success || !availabilityCheck.Data)
                {
                    NotificationHelper.SetError(TempData, "این زمان دیگر در دسترس نیست. لطفاً زمان دیگری انتخاب کنید");
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    NotificationHelper.SetError(TempData, "پزشک یافت نشد");
                    return RedirectToAction("SelectDoctor");
                }

                var priceResult = await _bookingService.GetAppointmentPriceAsync(doctorId, serviceCategoryId);
                if (!priceResult.Success)
                {
                    NotificationHelper.SetError(TempData, priceResult.Message ?? "خطا در محاسبه قیمت");
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
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
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
                    NotificationHelper.SetError(TempData, "اطلاعات وارد شده نامعتبر است");
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
                NotificationHelper.SetSuccess(TempData, "نوبت با موفقیت رزرو شد. لطفاً برای تکمیل رزرو، پرداخت را انجام دهید.");
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
        public async Task<ActionResult> ProcessPayment(int appointmentId, string paymentMethod = "online", string idempotencyKey = null)
        {
            try
            {
                _logger.Information("💰 PAYMENT REQUEST: درخواست پردازش پرداخت - AppointmentId: {AppointmentId}, Method: {Method}, IdempotencyKey: {IdempotencyKey}",
                    appointmentId, paymentMethod, idempotencyKey);

                // ✅ 0. Idempotency Check (جلوگیری از درخواست‌های تکراری)
                if (string.IsNullOrEmpty(idempotencyKey))
                {
                    idempotencyKey = $"payment_{appointmentId}_{_currentUserService.UserId}_{DateTime.UtcNow:yyyyMMddHHmm}";
                }

                var idempotencyKeyFull = $"appointment_payment_{idempotencyKey}";
                var canProcess = await _idempotencyService.TryUseKeyAsync(idempotencyKeyFull, ttlMinutes: 30, scope: "appointment_payment");

                if (!canProcess)
                {
                    _logger.Warning("⚠️ PAYMENT REQUEST: درخواست تکراری - AppointmentId: {AppointmentId}, IdempotencyKey: {IdempotencyKey}",
                        appointmentId, idempotencyKey);

                    // ✅ بررسی اینکه آیا OnlinePayment قبلاً ایجاد شده است
                    var existingPayment = await _context.OnlinePayments
                        .FirstOrDefaultAsync(op => op.AppointmentId == appointmentId && 
                                                   op.Status == OnlinePaymentStatus.Pending && 
                                                   !op.IsDeleted);

                    if (existingPayment != null && !string.IsNullOrEmpty(existingPayment.PaymentUrl))
                    {
                        _logger.Information("✅ PAYMENT REQUEST: بازگرداندن PaymentUrl موجود - OnlinePaymentId: {OnlinePaymentId}",
                            existingPayment.OnlinePaymentId);

                        return Json(new
                        {
                            success = true,
                            paymentUrl = existingPayment.PaymentUrl,
                            paymentToken = existingPayment.PaymentToken,
                            message = "در حال هدایت به درگاه پرداخت..."
                        });
                    }

                    return Json(new { success = false, message = "درخواست پرداخت در حال پردازش است. لطفاً صبر کنید." });
                }

                // 1. دریافت نوبت و بررسی دسترسی (Read-Only Query)
                var appointment = await _context.Appointments
                    .AsNoTracking() // ✅ برای Read-Only Query
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

                // ✅ 5. استفاده از Transaction برای اطمینان از یکپارچگی داده‌ها
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 5.1. ایجاد OnlinePayment record
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
                            CreatedAt = DateTime.UtcNow, // ✅ استفاده از UtcNow
                            IsDeleted = false
                        };

                        _context.OnlinePayments.Add(onlinePayment);
                        await _context.SaveChangesAsync();

                        // ✅ Post-Save Verification
                        var saved = await _context.OnlinePayments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePayment.OnlinePaymentId);

                        if (saved == null)
                        {
                            _logger.Error("❌ VERIFY: OnlinePayment ذخیره نشد! - OnlinePaymentId: {OnlinePaymentId}",
                                onlinePayment.OnlinePaymentId);
                            transaction.Rollback();
                            return Json(new { success = false, message = "خطا در ذخیره اطلاعات پرداخت" });
                        }

                        _logger.Information("✅ VERIFY: OnlinePayment با موفقیت ذخیره شد - OnlinePaymentId: {OnlinePaymentId}, Amount: {Amount}",
                            saved.OnlinePaymentId, saved.Amount);

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

                        // 7. فراخوانی سرویس پرداخت (خارج از Transaction - API Call)
                        var paymentResult = await _webPaymentService.CreatePaymentRequestAsync(paymentRequest);

                        if (!paymentResult.Success || paymentResult.Data == null)
                        {
                            _logger.Error("❌ PAYMENT REQUEST: خطا در ایجاد درخواست پرداخت - {ErrorMessage}",
                                paymentResult.Message);

                            // ✅ به‌روزرسانی وضعیت OnlinePayment به Failed
                            onlinePayment.Status = OnlinePaymentStatus.Failed;
                            onlinePayment.ErrorMessage = paymentResult.Message ?? "خطا در ایجاد درخواست پرداخت";
                            onlinePayment.UpdatedAt = DateTime.UtcNow;
                            onlinePayment.UpdatedByUserId = _currentUserService.UserId;
                            await _context.SaveChangesAsync();
                            transaction.Commit(); // ✅ Commit برای ذخیره وضعیت Failed

                            return Json(new { success = false, message = paymentResult.Message ?? "خطا در ایجاد درخواست پرداخت" });
                        }

                        var gatewayResponse = paymentResult.Data;

                        if (!gatewayResponse.Success || string.IsNullOrEmpty(gatewayResponse.PaymentUrl))
                        {
                            _logger.Error("❌ PAYMENT REQUEST: درگاه پرداخت پاسخ نامعتبر داد - ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                                gatewayResponse.ErrorCode, gatewayResponse.ErrorMessage);

                            // ✅ به‌روزرسانی وضعیت OnlinePayment به Failed
                            onlinePayment.Status = OnlinePaymentStatus.Failed;
                            onlinePayment.ErrorMessage = gatewayResponse.ErrorMessage ?? "خطا در درگاه پرداخت";
                            onlinePayment.ErrorCode = gatewayResponse.ErrorCode;
                            onlinePayment.UpdatedAt = DateTime.UtcNow;
                            onlinePayment.UpdatedByUserId = _currentUserService.UserId;
                            await _context.SaveChangesAsync();
                            transaction.Commit(); // ✅ Commit برای ذخیره وضعیت Failed

                            return Json(new { success = false, message = gatewayResponse.ErrorMessage ?? "خطا در درگاه پرداخت" });
                        }

                        // 8. به‌روزرسانی OnlinePayment با PaymentToken
                        onlinePayment.PaymentToken = gatewayResponse.PaymentToken;
                        onlinePayment.GatewayTransactionId = gatewayResponse.GatewayTransactionId;
                        onlinePayment.PaymentUrl = gatewayResponse.PaymentUrl;
                        onlinePayment.PaymentStartDate = DateTime.UtcNow;
                        onlinePayment.UpdatedAt = DateTime.UtcNow;
                        onlinePayment.UpdatedByUserId = _currentUserService.UserId;
                        await _context.SaveChangesAsync();

                        // ✅ Post-Save Verification
                        var verified = await _context.OnlinePayments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePayment.OnlinePaymentId && 
                                                       op.PaymentToken == gatewayResponse.PaymentToken);

                        if (verified == null)
                        {
                            _logger.Error("❌ VERIFY: OnlinePayment به‌روزرسانی نشد! - OnlinePaymentId: {OnlinePaymentId}",
                                onlinePayment.OnlinePaymentId);
                            transaction.Rollback();
                            return Json(new { success = false, message = "خطا در به‌روزرسانی اطلاعات پرداخت" });
                        }

                        transaction.Commit(); // ✅ Commit موفق

                        _logger.Information("✅ PAYMENT REQUEST: درخواست پرداخت با موفقیت ایجاد شد - OnlinePaymentId: {OnlinePaymentId}, PaymentUrl: {PaymentUrl}",
                            verified.OnlinePaymentId, verified.PaymentUrl);

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
                        transaction.Rollback();
                        _logger.Error(ex, "❌ PAYMENT REQUEST: خطا در Transaction - AppointmentId: {AppointmentId}", appointmentId);
                        throw; // Re-throw برای catch block اصلی
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش پرداخت - AppointmentId: {AppointmentId}", appointmentId);
                return Json(new { success = false, message = "خطا در پردازش پرداخت. لطفاً دوباره تلاش کنید" });
            }
        }

        /// <summary>
        /// Callback از درگاه پرداخت (ZarinPal Format)
        /// GET: /Patient/Appointment/Book/PaymentCallback?Status=OK&Authority=xxx
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // درگاه ممکن است از خارج از سیستم فراخوانی شود
        public async Task<ActionResult> PaymentCallback(
            string Status, // ZarinPal: Status (OK/NOK)
            string Authority) // ZarinPal: Authority (PaymentToken)
        {
            try
            {
                _logger.Information("💰 PAYMENT CALLBACK: دریافت Callback از درگاه - Status: {Status}, Authority: {Authority}",
                    Status, Authority);

                // ✅ ZarinPal Callback Format: ?Status=OK&Authority=xxx
                // اگر Status یا Authority خالی باشد، از QueryString بخوان
                if (string.IsNullOrEmpty(Status))
                {
                    Status = Request.QueryString["Status"];
                }
                if (string.IsNullOrEmpty(Authority))
                {
                    Authority = Request.QueryString["Authority"];
                }

                // 1. دریافت OnlinePayment بر اساس Authority (PaymentToken)
                if (string.IsNullOrEmpty(Authority))
                {
                    _logger.Warning("⚠️ PAYMENT CALLBACK: Authority در Callback موجود نیست");
                    NotificationHelper.SetError(TempData, "اطلاعات پرداخت نامعتبر است");
                    return RedirectToAction("PaymentError", new { message = "اطلاعات پرداخت نامعتبر است" });
                }

                var onlinePayment = await _context.OnlinePayments
                    .Include(op => op.Appointment)
                    .Include("Appointment.Doctor")
                    .Include(op => op.PaymentGateway)
                    .Include(op => op.Patient)
                    .FirstOrDefaultAsync(op => op.PaymentToken == Authority && !op.IsDeleted);

                if (onlinePayment == null)
                {
                    _logger.Warning("⚠️ PAYMENT CALLBACK: OnlinePayment با Authority {Authority} یافت نشد", Authority);
                    NotificationHelper.SetError(TempData, "پرداخت یافت نشد");
                    return RedirectToAction("PaymentError", new { message = "پرداخت یافت نشد" });
                }

                // 2. بررسی دسترسی بیمار (اگر کاربر لاگین کرده است)
                if (User.Identity.IsAuthenticated)
                {
                    var patient = await _currentUserService.GetPatientInfoAsync();
                    if (patient == null || patient.PatientId != onlinePayment.PatientId)
                    {
                        _logger.Warning("⚠️ PAYMENT CALLBACK: دسترسی غیرمجاز به OnlinePayment {OnlinePaymentId} توسط بیمار {PatientId}",
                            onlinePayment.OnlinePaymentId, patient?.PatientId);
                        NotificationHelper.SetError(TempData, "شما اجازه دسترسی به این پرداخت را ندارید");
                        return RedirectToAction("PaymentError", new { message = "دسترسی غیرمجاز" });
                    }
                }

                // 3. ✅ ساخت PaymentCallbackData از ZarinPal Callback Format
                var callbackData = new PaymentCallbackData
                {
                    PaymentToken = Authority, // Authority = PaymentToken در ZarinPal
                    TransactionId = Authority, // Authority = TransactionId
                    ReferenceCode = Request.QueryString["RefId"], // RefId (اختیاری)
                    Status = Status, // OK یا NOK
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

                _logger.Debug("📋 PAYMENT CALLBACK: CallbackData ساخته شد - PaymentToken: {PaymentToken}, Status: {Status}",
                    callbackData.PaymentToken, callbackData.Status);

                // 4. پردازش Callback
                var callbackResult = await _webPaymentService.ProcessPaymentCallbackAsync(
                    onlinePayment.PaymentGateway.GatewayType,
                    callbackData);

                if (!callbackResult.Success || callbackResult.Data == null)
                {
                    _logger.Error("خطا در پردازش Callback - {ErrorMessage}",
                        callbackResult.Message);

                    NotificationHelper.SetError(TempData, callbackResult.Message ?? "خطا در پردازش پرداخت");
                    return RedirectToAction("MyAppointments", "Appointment");
                }

                var result = callbackResult.Data;

                // 5. ✅ به‌روزرسانی Appointment.Status و OnlinePayment
                if (result.Success && result.Status == OnlinePaymentStatus.Successful)
                {
                    // ✅ دریافت مجدد OnlinePayment برای Update (نه AsNoTracking)
                    var onlinePaymentForUpdate = await _context.OnlinePayments
                        .Include(op => op.Appointment)
                        .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePayment.OnlinePaymentId && !op.IsDeleted);

                    if (onlinePaymentForUpdate == null)
                    {
                        _logger.Error("❌ PAYMENT CALLBACK: OnlinePayment برای Update یافت نشد - OnlinePaymentId: {OnlinePaymentId}",
                            onlinePayment.OnlinePaymentId);
                        NotificationHelper.SetError(TempData, "پرداخت یافت نشد");
                        return RedirectToAction("PaymentError", new { message = "پرداخت یافت نشد" });
                    }

                    // استفاده از Transaction برای اطمینان از یکپارچگی داده‌ها
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            // به‌روزرسانی OnlinePayment
                            onlinePaymentForUpdate.Status = OnlinePaymentStatus.Successful;
                            onlinePaymentForUpdate.GatewayTransactionId = result.GatewayTransactionId; // RefId
                            onlinePaymentForUpdate.GatewayReferenceCode = callbackData.ReferenceCode ?? result.GatewayTransactionId;
                            onlinePaymentForUpdate.PaymentCompletionDate = DateTime.UtcNow;
                            onlinePaymentForUpdate.UpdatedAt = DateTime.UtcNow;
                            onlinePaymentForUpdate.UpdatedByUserId = _currentUserService.UserId ?? "System";

                            // به‌روزرسانی Appointment
                            var appointment = onlinePaymentForUpdate.Appointment;
                            if (appointment != null)
                            {
                                appointment.Status = AppointmentStatus.Scheduled;
                                appointment.PaymentTransactionId = result.PaymentTransactionId;
                                appointment.UpdatedAt = DateTime.UtcNow;
                                appointment.UpdatedByUserId = _currentUserService.UserId ?? "System";
                            }

                            await _context.SaveChangesAsync();

                            // ✅ Post-Save Verification
                            var verifiedPayment = await _context.OnlinePayments
                                .AsNoTracking()
                                .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentForUpdate.OnlinePaymentId && 
                                                           op.Status == OnlinePaymentStatus.Successful);

                            var verifiedAppointment = appointment != null
                                ? await _context.Appointments
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId && 
                                                              a.Status == AppointmentStatus.Scheduled)
                                : null;

                            if (verifiedPayment == null)
                            {
                                _logger.Error("❌ VERIFY: OnlinePayment ذخیره نشد! - OnlinePaymentId: {OnlinePaymentId}",
                                    onlinePaymentForUpdate.OnlinePaymentId);
                                transaction.Rollback();
                                NotificationHelper.SetError(TempData, "خطا در ذخیره اطلاعات پرداخت");
                                return RedirectToAction("PaymentError", new { message = "خطا در ذخیره اطلاعات پرداخت" });
                            }

                            if (appointment != null && verifiedAppointment == null)
                            {
                                _logger.Error("❌ VERIFY: Appointment به‌روزرسانی نشد! - AppointmentId: {AppointmentId}",
                                    appointment.AppointmentId);
                                transaction.Rollback();
                                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی نوبت");
                                return RedirectToAction("PaymentError", new { message = "خطا در به‌روزرسانی نوبت" });
                            }

                            transaction.Commit();

                            _logger.Information("✅ VERIFY: OnlinePayment و Appointment با موفقیت ذخیره شدند - OnlinePaymentId: {OnlinePaymentId}, AppointmentId: {AppointmentId}, RefId: {RefId}",
                                verifiedPayment.OnlinePaymentId, verifiedAppointment?.AppointmentId, result.GatewayTransactionId);

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
                                        if (verifiedAppointment != null)
                                        {
                                            await notificationService.SendPaymentConfirmationAsync(verifiedAppointment.AppointmentId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex, "خطا در ارسال اعلان پرداخت - AppointmentId: {AppointmentId}",
                                            verifiedAppointment?.AppointmentId);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.Warning(ex, "خطا در ایجاد سرویس اعلان - AppointmentId: {AppointmentId}",
                                    verifiedAppointment?.AppointmentId);
                            }

                            // ✅ Redirect به صفحه موفقیت
                            return RedirectToAction("PaymentSuccess", new 
                            { 
                                appointmentId = verifiedAppointment?.AppointmentId,
                                onlinePaymentId = verifiedPayment.OnlinePaymentId,
                                refId = result.GatewayTransactionId
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            _logger.Error(ex, "❌ PAYMENT CALLBACK: خطا در ذخیره اطلاعات پرداخت");
                            throw;
                        }
                    }
                }
                else
                {
                    // ✅ دریافت مجدد OnlinePayment برای Update
                    var onlinePaymentForUpdate = await _context.OnlinePayments
                        .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePayment.OnlinePaymentId && !op.IsDeleted);

                    if (onlinePaymentForUpdate != null)
                    {
                        // ✅ به‌روزرسانی OnlinePayment به Failed
                        onlinePaymentForUpdate.Status = OnlinePaymentStatus.Failed;
                        onlinePaymentForUpdate.ErrorMessage = result.ErrorMessage;
                        onlinePaymentForUpdate.ErrorCode = callbackData.ErrorCode;
                        onlinePaymentForUpdate.PaymentCompletionDate = DateTime.UtcNow;
                        onlinePaymentForUpdate.UpdatedAt = DateTime.UtcNow;
                        onlinePaymentForUpdate.UpdatedByUserId = _currentUserService.UserId ?? "System";
                        await _context.SaveChangesAsync();

                        // ✅ Post-Save Verification
                        var verified = await _context.OnlinePayments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(op => op.OnlinePaymentId == onlinePaymentForUpdate.OnlinePaymentId && 
                                                       op.Status == OnlinePaymentStatus.Failed);

                        if (verified == null)
                        {
                            _logger.Error("❌ VERIFY: OnlinePayment به‌روزرسانی نشد! - OnlinePaymentId: {OnlinePaymentId}",
                                onlinePaymentForUpdate.OnlinePaymentId);
                        }
                        else
                        {
                            _logger.Information("✅ VERIFY: OnlinePayment به‌روزرسانی شد - OnlinePaymentId: {OnlinePaymentId}, Status: {Status}",
                                verified.OnlinePaymentId, verified.Status);
                        }
                    }

                    _logger.Warning("⚠️ PAYMENT CALLBACK: پرداخت ناموفق - OnlinePaymentId: {OnlinePaymentId}, Status: {Status}, Error: {Error}",
                        onlinePayment.OnlinePaymentId, result.Status, result.ErrorMessage);

                    // ✅ Redirect به صفحه خطا
                    return RedirectToAction("PaymentError", new 
                    { 
                        message = result.ErrorMessage ?? "پرداخت ناموفق بود",
                        appointmentId = onlinePayment.AppointmentId,
                        onlinePaymentId = onlinePayment.OnlinePaymentId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PAYMENT CALLBACK: خطا در پردازش Callback پرداخت");
                return RedirectToAction("PaymentError", new { message = "خطا در پردازش پرداخت. لطفاً با پشتیبانی تماس بگیرید" });
            }
        }

        /// <summary>
        /// ✅ صفحه موفقیت پرداخت
        /// GET: /Patient/Appointment/Book/PaymentSuccess
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PaymentSuccess(int? appointmentId, int? onlinePaymentId, string refId)
        {
            try
            {
                _logger.Information("✅ PAYMENT SUCCESS: نمایش صفحه موفقیت - AppointmentId: {AppointmentId}, OnlinePaymentId: {OnlinePaymentId}, RefId: {RefId}",
                    appointmentId, onlinePaymentId, refId);

                var viewModel = new PaymentSuccessViewModel
                {
                    AppointmentId = appointmentId,
                    OnlinePaymentId = onlinePaymentId,
                    RefId = refId
                };

                // دریافت اطلاعات نوبت (اگر موجود باشد)
                if (appointmentId.HasValue)
                {
                    var appointment = await _context.Appointments
                        .Include(a => a.Doctor)
                        .Include(a => a.Patient)
                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId.Value && !a.IsDeleted);

                    if (appointment != null)
                    {
                        viewModel.DoctorName = appointment.Doctor?.FullName ?? "نامشخص";
                        viewModel.AppointmentDate = appointment.AppointmentDate;
                        viewModel.PatientName = appointment.Patient?.FullName ?? "نامشخص";
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PAYMENT SUCCESS: خطا در نمایش صفحه موفقیت");
                return RedirectToAction("PaymentError", new { message = "خطا در نمایش اطلاعات" });
            }
        }

        /// <summary>
        /// ✅ صفحه خطای پرداخت
        /// GET: /Patient/Appointment/Book/PaymentError
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> PaymentError(string message, int? appointmentId, int? onlinePaymentId)
        {
            try
            {
                _logger.Warning("⚠️ PAYMENT ERROR: نمایش صفحه خطا - Message: {Message}, AppointmentId: {AppointmentId}, OnlinePaymentId: {OnlinePaymentId}",
                    message, appointmentId, onlinePaymentId);

                var viewModel = new PaymentErrorViewModel
                {
                    ErrorMessage = message ?? "پرداخت ناموفق بود",
                    AppointmentId = appointmentId,
                    OnlinePaymentId = onlinePaymentId
                };

                // دریافت اطلاعات نوبت (اگر موجود باشد)
                if (appointmentId.HasValue)
                {
                    var appointment = await _context.Appointments
                        .Include(a => a.Doctor)
                        .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId.Value && !a.IsDeleted);

                    if (appointment != null)
                    {
                        viewModel.DoctorName = appointment.Doctor?.FullName ?? "نامشخص";
                        viewModel.AppointmentDate = appointment.AppointmentDate;
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PAYMENT ERROR: خطا در نمایش صفحه خطا");
                return View(new PaymentErrorViewModel { ErrorMessage = "خطا در نمایش اطلاعات" });
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

