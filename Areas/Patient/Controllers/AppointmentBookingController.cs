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
using System.Web.UI;
using Microsoft.AspNet.Identity;
using ClinicApp.Interfaces.ClinicAdmin; // ✅ برای IDepartmentManagementService

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// ✅ ULTIMATE: Controller برای رزرو نوبت آنلاین - Enterprise-Grade
    /// 
    /// Architecture:
    /// - Extends BasePatientController for standard patient authentication
    /// - PatientRoleAuthorization ensures only Patient role users can book
    /// - Uses GetCurrentPatientIdAsync() for secure patient ID retrieval
    /// 
    /// Security:
    /// - CSRF protection on all POST actions
    /// - Rate limiting for booking attempts
    /// - State validation between booking steps
    /// 
    /// Performance:
    /// - Caching for doctor list & available slots
    /// - Optimized database queries
    /// - Transaction management for booking + payment
    /// 
    /// طبق: APPOINTMENT_BOOKING_ROADMAP.md | PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    [PatientRoleAuthorization]
    public class AppointmentBookingController : Base.BasePatientController
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly IWebPaymentService _webPaymentService;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IAppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IDepartmentManagementService _departmentService; // ✅ طبق قرارداد: Controller → Service

        public AppointmentBookingController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IWebPaymentService webPaymentService,
            IPaymentGatewayService paymentGatewayService,
            IIdempotencyService idempotencyService,
            IAppSettings appSettings,
            ApplicationDbContext context,
            IDepartmentManagementService departmentService, // ✅ طبق قرارداد: Controller → Service
            ILogger logger)
            : base(logger, currentUserService) // ✅ Call base constructor
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _webPaymentService = webPaymentService ?? throw new ArgumentNullException(nameof(webPaymentService));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _idempotencyService = idempotencyService ?? throw new ArgumentNullException(nameof(idempotencyService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        }

        /// <summary>
        /// صفحه اصلی رزرو نوبت - هدایت به انتخاب پزشک
        /// GET: /Patient/Appointment/Book
        /// </summary>
        [HttpGet]
        public ActionResult Book()
        {
            // ✅ Diagnostic: Log user info before redirect
            _logger.Information("🔍 [Book] User info - IsAuthenticated: {IsAuth}, UserId: {UserId}, IsPatientRole: {IsPatient}, UserName: {UserName}",
                User.Identity.IsAuthenticated,
                User.Identity.GetUserId(),
                User.IsInRole("Patient"),
                User.Identity.Name);
            
            return RedirectToAction("SelectDoctor");
        }

        /// <summary>
        /// ✅ Diagnostic Action: بررسی وضعیت احراز هویت و نقش کاربر
        /// GET: /Patient/Appointment/Book/CheckAuth
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // برای تست
        public JsonResult CheckAuth()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var userName = User.Identity.Name;
                var isAuthenticated = User.Identity.IsAuthenticated;
                var isPatientRole = User.IsInRole("Patient");
                var allRoles = new List<string>();
                
                if (User.Identity is System.Security.Claims.ClaimsIdentity claimsIdentity)
                {
                    allRoles = claimsIdentity.Claims
                        .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                        .Select(c => c.Value)
                        .ToList();
                }

                var patientId = GetCurrentPatientIdAsync().Result;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        isAuthenticated,
                        userId,
                        userName,
                        isPatientRole,
                        allRoles,
                        patientId,
                        message = isPatientRole 
                            ? "کاربر دارای نقش Patient است" 
                            : "کاربر نقش Patient ندارد"
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت احراز هویت");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// صفحه انتخاب پزشک
        /// GET: /Patient/Appointment/Book/SelectDoctor
        /// ✅ CRITICAL FIX: Use [Authorize] instead of manual check to ensure authentication state is synchronized
        /// </summary>
        [HttpGet]
        [Authorize]
        // ✅ FIX Issue 2: کاهش OutputCache در محیط درمانی (طبق SELECT_DOCTOR_MODULE_REVIEW.md)
        // در محیط درمانی، داده‌ها باید Real-time باشند - Cache فقط 1 دقیقه
        [OutputCache(Duration = 60, VaryByParam = "departmentId;searchTerm", Location = OutputCacheLocation.Server)]
        public async Task<ActionResult> SelectDoctor(int? departmentId, string searchTerm)
        {
            try
            {
                // ✅ FIX Issue 5: Input Validation (طبق SELECT_DOCTOR_MODULE_REVIEW.md)
                // ✅ Validate departmentId
                if (departmentId.HasValue && departmentId.Value <= 0)
                {
                    _logger.Warning("⚠️ [SelectDoctor] Invalid departmentId: {DepartmentId}", departmentId);
                    departmentId = null;
                }

                // ✅ Validate and sanitize searchTerm
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();
                    // ✅ Limit length for performance and security
                    if (searchTerm.Length > 100)
                    {
                        _logger.Warning("⚠️ [SelectDoctor] SearchTerm too long, truncating: {Length}", searchTerm.Length);
                        searchTerm = searchTerm.Substring(0, 100);
                    }
                    // ✅ Sanitize for XSS (HTML Encode will be done in View)
                    // Note: EF Core parameterized queries prevent SQL Injection
                }
                else
                {
                    searchTerm = null;
                }

                // ✅ Diagnostic: Log user info for debugging
                var userId = User.Identity.GetUserId();
                var isPatientRole = User.IsInRole("Patient");
                _logger.Information("🔍 [SelectDoctor] User info - UserId: {UserId}, IsPatientRole: {IsPatient}, DepartmentId: {DepartmentId}, SearchTerm: {SearchTerm}",
                    userId, isPatientRole, departmentId, searchTerm);

                // ✅ Validation: Check if user has Patient role (double check)
                if (!isPatientRole)
                {
                    _logger.Warning("⚠️ [SelectDoctor] User {UserId} does not have Patient role", userId);
                    NotificationHelper.SetError(TempData, "شما مجوز دسترسی به بخش رزرو نوبت را ندارید. لطفاً با حساب کاربری بیمار وارد شوید.");
                    return RedirectToAction("Login", "Account", new { area = "", returnUrl = Request.Url?.PathAndQuery });
                }

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

                // ✅ دریافت لیست دپارتمان‌های فعال برای فیلتر (طبق قرارداد: Controller → Service)
                // ✅ FIX Issue 1: انتقال DB Access از Controller به Service
                var departmentsResult = await _departmentService.GetActiveDepartmentsForPatientAsync();
                if (!departmentsResult.Success)
                {
                    _logger.Warning("⚠️ [SelectDoctor] خطا در دریافت دپارتمان‌ها: {Message}", departmentsResult.Message);
                    // Fallback: لیست خالی
                    departmentsResult = ServiceResult<List<DepartmentInfo>>.Successful(new List<DepartmentInfo>());
                }
                var departments = departmentsResult.Data ?? new List<DepartmentInfo>();

                var viewModel = new DoctorSelectionViewModel
                {
                    Doctors = result.Data,
                    SelectedDepartmentId = departmentId,
                    SearchTerm = searchTerm,
                    Departments = departments
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
        /// ✅ ULTIMATE: Bulletproof validation
        /// </summary>
        [HttpGet]
        [Authorize] // ✅ Explicit authorization (inherits PatientRoleAuthorization from controller)
        public async Task<ActionResult> SelectDate(int doctorId)
        {
            try
            {
                // ✅ DEBUGGING: Log route resolution
                _logger.Information("🔍 [SelectDate] Route resolved successfully - DoctorId: {DoctorId}, User: {UserName}, IsAuthenticated: {IsAuth}, IsPatientRole: {IsPatient}", 
                    doctorId, 
                    User.Identity?.Name ?? "NULL", 
                    User.Identity?.IsAuthenticated ?? false,
                    User.IsInRole("Patient"));
                _logger.Information("درخواست صفحه انتخاب تاریخ - DoctorId: {DoctorId}", doctorId);

                // ✅ Validation 1: DoctorId must be positive
                if (doctorId <= 0)
                {
                    _logger.Warning("DoctorId نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است");
                    return RedirectToAction("SelectDoctor");
                }

                // ✅ Validation 2: Check if patient is authenticated
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("بیمار لاگین نیست");
                    NotificationHelper.SetError(TempData, "لطفاً ابتدا وارد سیستم شوید");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Validation 3: Check if doctor exists and is active
                var doctorResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک {DoctorId} یافت نشد", doctorId);
                    NotificationHelper.SetError(TempData, "پزشک یافت نشد");
                    return RedirectToAction("SelectDoctor");
                }

                // ✅ Validation 4: Check if doctor has active schedule (accepts appointments)
                if (!doctorResult.Data.HasActiveSchedule)
                {
                    _logger.Warning("پزشک {DoctorId} برنامه کاری فعالی ندارد", doctorId);
                    NotificationHelper.SetError(TempData, "این پزشک در حال حاضر برنامه کاری فعالی ندارد");
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
        /// ✅ ULTIMATE: Bulletproof validation
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SelectTime(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست صفحه انتخاب زمان - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date.ToString("yyyy/MM/dd"));

                // ✅ Validation 1: DoctorId must be positive
                if (doctorId <= 0)
                {
                    _logger.Warning("DoctorId نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است");
                    return RedirectToAction("SelectDoctor");
                }

                // ✅ Validation 2: Check if patient is authenticated
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("بیمار لاگین نیست");
                    NotificationHelper.SetError(TempData, "لطفاً ابتدا وارد سیستم شوید");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Validation 3: Date must not be in the past
                if (date.Date < DateTime.Today)
                {
                    _logger.Warning("تاریخ {Date} در گذشته است", date.ToString("yyyy/MM/dd"));
                    NotificationHelper.SetError(TempData, "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
                    return RedirectToAction("SelectDate", new { doctorId });
                }

                // ✅ Validation 4: Date must not be too far in the future (max 90 days)
                var maxFutureDate = DateTime.Today.AddDays(90);
                if (date.Date > maxFutureDate)
                {
                    _logger.Warning("تاریخ {Date} بیش از 90 روز در آینده است", date.ToString("yyyy/MM/dd"));
                    NotificationHelper.SetError(TempData, "نمی‌توانید برای بیش از 90 روز آینده نوبت رزرو کنید");
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

                // ✅ دریافت مدت زمان نوبت از تنظیمات پزشک (DoctorSchedule) یا استفاده از مقدار پیش‌فرض
                var doctorSchedule = await _context.DoctorSchedules
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ds => ds.DoctorId == doctorId && !ds.IsDeleted);

                var appointmentDuration = doctorSchedule?.AppointmentDuration 
                    ?? _appSettings.DefaultAppointmentDurationMinutes;

                var viewModel = new TimeSlotSelectionViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    SelectedDate = date,
                    AvailableSlots = slotsResult.Data,
                    AppointmentDuration = appointmentDuration
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
        /// ✅ ULTIMATE: Bulletproof validation + Double booking prevention
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

                // ✅ Validation 1: DoctorId must be positive
                if (doctorId <= 0)
                {
                    _logger.Warning("DoctorId نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است");
                    return RedirectToAction("SelectDoctor");
                }

                // ✅ Validation 2: Check if patient is authenticated
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("بیمار لاگین نیست");
                    NotificationHelper.SetError(TempData, "لطفاً ابتدا وارد سیستم شوید");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // ✅ Validation 3: Date must not be in the past
                if (appointmentDate.Date < DateTime.Today)
                {
                    _logger.Warning("تاریخ {Date} در گذشته است", appointmentDate.ToString("yyyy/MM/dd"));
                    NotificationHelper.SetError(TempData, "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید");
                    return RedirectToAction("SelectDoctor");
                }

                // ✅ Validation 4: StartTime must be before EndTime
                if (startTime >= endTime)
                {
                    _logger.Warning("زمان شروع {StartTime} بعد از زمان پایان {EndTime} است", startTime, endTime);
                    NotificationHelper.SetError(TempData, "زمان شروع باید قبل از زمان پایان باشد");
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                // ✅ Validation 5: Time must be valid (00:00 to 23:59)
                if (startTime < TimeSpan.Zero || startTime >= TimeSpan.FromHours(24) ||
                    endTime < TimeSpan.Zero || endTime >= TimeSpan.FromHours(24))
                {
                    _logger.Warning("زمان نامعتبر - StartTime: {StartTime}, EndTime: {EndTime}", startTime, endTime);
                    NotificationHelper.SetError(TempData, "زمان انتخاب شده نامعتبر است");
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                // ✅ Validation 6: Description length check (if provided)
                if (!string.IsNullOrEmpty(description) && description.Length > 500)
                {
                    _logger.Warning("توضیحات خیلی طولانی است: {Length} کاراکتر", description.Length);
                    NotificationHelper.SetError(TempData, "توضیحات نباید بیش از 500 کاراکتر باشد");
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                // ✅ Validation 7: Check if patient already has an appointment at this time (Double Booking Prevention)
                // Note: Appointment model uses AppointmentDate (DateTime) + Duration (int minutes)
                var requestedStartDateTime = appointmentDate.Date + startTime;
                var requestedEndDateTime = appointmentDate.Date + endTime;

                var existingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.PatientId == patientId.Value &&
                                a.AppointmentDate.Year == appointmentDate.Year &&
                                a.AppointmentDate.Month == appointmentDate.Month &&
                                a.AppointmentDate.Day == appointmentDate.Day &&
                                a.Status != AppointmentStatus.Cancelled &&
                                !a.IsDeleted)
                    .ToListAsync();

                // Check overlap in memory (because we need to calculate EndTime = AppointmentDate + Duration)
                var hasOverlap = existingAppointments.Any(a =>
                {
                    var existingStartTime = a.AppointmentDate;
                    var existingEndTime = a.AppointmentDate.AddMinutes(a.Duration);

                    return (existingStartTime <= requestedStartDateTime && existingEndTime > requestedStartDateTime) ||
                           (existingStartTime < requestedEndDateTime && existingEndTime >= requestedEndDateTime) ||
                           (existingStartTime >= requestedStartDateTime && existingEndTime <= requestedEndDateTime);
                });

                if (hasOverlap)
                {
                    _logger.Warning("⚠️ DOUBLE BOOKING: بیمار {PatientId} در تاریخ {Date} زمان {Time} قبلاً نوبت دارد",
                        patientId, appointmentDate.ToString("yyyy/MM/dd"), startTime);
                    NotificationHelper.SetError(TempData, "شما در این تاریخ و زمان قبلاً نوبت دارید. لطفاً زمان دیگری انتخاب کنید");
                    return RedirectToAction("SelectTime", new { doctorId, date = appointmentDate });
                }

                // ✅ Validation 8: بررسی دسترسی‌پذیری مجدد (Race Condition Prevention)
                var availabilityCheck = await _bookingService.CheckSlotAvailabilityAsync(
                    doctorId, appointmentDate, startTime, endTime);

                if (!availabilityCheck.Success || !availabilityCheck.Data)
                {
                    _logger.Warning("⚠️ SLOT UNAVAILABLE: اسلات {DoctorId}/{Date}/{Time} دیگر در دسترس نیست",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), startTime);
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
        /// ✅ ULTIMATE: Bulletproof validation + Transaction + Double booking prevention
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

                // ✅ Validation 1: ModelState
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.Warning("ModelState نامعتبر: {Errors}", errors);
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است: " + errors });
                }

                // ✅ Validation 2: Check if patient is authenticated
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("بیمار لاگین نیست");
                    return Json(new { success = false, message = "لطفاً ابتدا وارد سیستم شوید", redirectUrl = "/Account/Login" });
                }

                // ✅ Validation 3: DoctorId must be positive
                if (model.DoctorId <= 0)
                {
                    _logger.Warning("DoctorId نامعتبر: {DoctorId}", model.DoctorId);
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است" });
                }

                // ✅ Validation 4: Date must not be in the past
                if (model.AppointmentDate.Date < DateTime.Today)
                {
                    _logger.Warning("تاریخ {Date} در گذشته است", model.AppointmentDate.ToString("yyyy/MM/dd"));
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته نوبت رزرو کنید" });
                }

                // ✅ Validation 5: StartTime must be before EndTime
                if (model.StartTime >= model.EndTime)
                {
                    _logger.Warning("زمان شروع {StartTime} بعد از زمان پایان {EndTime} است", model.StartTime, model.EndTime);
                    return Json(new { success = false, message = "زمان شروع باید قبل از زمان پایان باشد" });
                }

                // ✅ Validation 6: Double Booking Prevention (Check again in POST)
                // Note: Appointment model uses AppointmentDate (DateTime) + Duration (int minutes)
                var requestedStartDateTime = model.AppointmentDate.Date + model.StartTime;
                var requestedEndDateTime = model.AppointmentDate.Date + model.EndTime;

                var existingAppointments = await _context.Appointments
                    .AsNoTracking()
                    .Where(a => a.PatientId == patientId.Value &&
                                a.AppointmentDate.Year == model.AppointmentDate.Year &&
                                a.AppointmentDate.Month == model.AppointmentDate.Month &&
                                a.AppointmentDate.Day == model.AppointmentDate.Day &&
                                a.Status != AppointmentStatus.Cancelled &&
                                !a.IsDeleted)
                    .ToListAsync();

                // Check overlap in memory (because we need to calculate EndTime = AppointmentDate + Duration)
                var hasOverlap = existingAppointments.Any(a =>
                {
                    var existingStartTime = a.AppointmentDate;
                    var existingEndTime = a.AppointmentDate.AddMinutes(a.Duration);

                    return (existingStartTime <= requestedStartDateTime && existingEndTime > requestedStartDateTime) ||
                           (existingStartTime < requestedEndDateTime && existingEndTime >= requestedEndDateTime) ||
                           (existingStartTime >= requestedStartDateTime && existingEndTime <= requestedEndDateTime);
                });

                if (hasOverlap)
                {
                    _logger.Warning("⚠️ DOUBLE BOOKING PREVENTED: بیمار {PatientId} در تاریخ {Date} زمان {Time} قبلاً نوبت دارد",
                        patientId, model.AppointmentDate.ToString("yyyy/MM/dd"), model.StartTime);
                    return Json(new { success = false, message = "شما در این تاریخ و زمان قبلاً نوبت دارید" });
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

