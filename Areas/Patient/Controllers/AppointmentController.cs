using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.DTOs.Appointment;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Patient;
using ClinicApp.ViewModels.DoctorManagementVM;
// ✅ استفاده از alias برای جلوگیری از conflict بین دو AvailableDateInfo
using AppointmentDateInfo = ClinicApp.Models.DTOs.Appointment.AvailableDateInfo;
using ClinicApp.Models.Entities.Doctor;
using Serilog;
using static ClinicApp.Helpers.NotificationHelper;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Extensions;
using Microsoft.AspNet.Identity;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای مدیریت نوبت‌های بیمار
    /// بهینه‌سازی شده طبق appointment_controller_review.md
    /// 
    /// ✅ Security: PatientRoleAuthorization enforced via BasePatientController
    /// طبق: PATIENT_AUTH_INTEGRATION_ANALYSIS.md
    /// </summary>
    public class AppointmentController : BasePatientController
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IDoctorMappingService _mappingService;
        private readonly IAppSettings _appSettings;

        public AppointmentController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IDoctorCrudService doctorCrudService,
            IDoctorScheduleRepository scheduleRepository,
            IDoctorMappingService mappingService,
            ILogger logger,
            IAppSettings appSettings = null)
            : base(logger, currentUserService) // ✅ Base Constructor
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _appSettings = appSettings ?? AppSettings.Instance; // ✅ استفاده از Singleton pattern
        }

        /// <summary>
        /// صفحه Index - هدایت به Available (برای backward compatibility)
        /// GET: /Patient/Appointment/Index
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return RedirectToAction("Available");
        }

        /// <summary>
        /// صفحه عمومی نمایش نوبت‌های موجود (بدون نیاز به لاگین)
        /// GET: /Patient/Appointment/Available
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Available(
            int? doctorId = null,
            string date = null,  // ✅ تغییر از DateTime? به string برای دریافت تاریخ شمسی
            int page = 1,
            int pageSize = 0) // ✅ 0 = استفاده از مقدار پیش‌فرض از config
        {
            try
            {
                // ✅ CRITICAL DIAGNOSTIC: Log authentication state to diagnose cookie issue
                var requestIsAuth = Request.IsAuthenticated;
                var userIdentityIsAuth = User?.Identity?.IsAuthenticated ?? false;
                var userId = User?.Identity?.GetUserId();
                var userName = User?.Identity?.GetUserName();
                var authCookie = Request.Cookies["ClinicAppAuth"];
                
                System.Diagnostics.Debug.WriteLine($"🔐 Appointment.Available - Request.IsAuthenticated: {requestIsAuth}, User.Identity.IsAuthenticated: {userIdentityIsAuth}, UserId: {userId}, UserName: {userName}");
                System.Diagnostics.Debug.WriteLine($"🔐 Appointment.Available - Cookie 'ClinicAppAuth' in Request: {(authCookie != null ? "EXISTS" : "NOT FOUND")}, Value: {(authCookie?.Value?.Substring(0, Math.Min(50, authCookie.Value?.Length ?? 0)) ?? "null")}");
                
                _logger.Information("🔐 Appointment.Available - Request.IsAuthenticated: {RequestAuth}, User.Identity.IsAuthenticated: {UserAuth}, UserId: {UserId}, CookieExists: {CookieExists}",
                    requestIsAuth, userIdentityIsAuth, userId, authCookie != null);
                
                _logger.Information("درخواست نمایش نوبت‌های موجود - DoctorId: {DoctorId}, DateString: {DateString}, Page: {Page}",
                    doctorId, date ?? "همه", page);

                // ✅ استفاده از config برای pageSize
                if (pageSize <= 0)
                {
                    pageSize = _appSettings.AppointmentDoctorsPageSize;
                }
                
                // ✅ Validation برای page
                if (page < 1)
                {
                    page = 1;
                }

                // دریافت لیست پزشکان
                var doctorsResult = await _bookingService.GetAvailableDoctorsAsync();
                if (!doctorsResult.Success)
                {
                    NotificationHelper.SetError(TempData, "خطا در دریافت لیست پزشکان");
                    return View(new AvailableAppointmentsViewModel
                    {
                        Doctors = new List<DoctorSearchResultDto>(),
                        AvailableSlots = new List<AvailableTimeSlotDto>(),
                        PageNumber = page,
                        PageSize = pageSize,
                        TotalCount = 0
                    });
                }

                // ✅ استفاده از Extension Method برای Date Parsing (حذف کد تکراری)
                var selectedDate = this.ParsePersianDateSafe(date, _logger);
                
                if (selectedDate < DateTime.Today)
                {
                    _logger.Warning("تاریخ انتخاب شده '{Date}' در گذشته است، تنظیم به امروز", 
                        selectedDate.ToString("yyyy/MM/dd"));
                    selectedDate = DateTime.Today;
                    NotificationHelper.SetWarning(TempData, "تاریخ انتخاب شده در گذشته است. لطفاً تاریخ معتبری انتخاب کنید.");
                }
                
                // ✅ فیلتر بر اساس doctorId (اگر انتخاب شده باشد)
                var allDoctors = doctorsResult.Data ?? new List<DoctorSearchResultDto>();
                if (doctorId.HasValue)
                {
                    allDoctors = allDoctors.Where(d => d.DoctorId == doctorId.Value).ToList();
                }
                
                // ✅ Pagination
                var totalCount = allDoctors.Count;
                var pagedDoctors = allDoctors
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                // ✅ دریافت تاریخ‌های نوبت موجود برای هر پزشک به صورت موازی (بهینه‌سازی عملکرد)
                // ⚠️ مهم: باید قبل از ساخت ViewModel انجام شود
                var maxDates = _appSettings.AppointmentAvailableDatesMaxCount;
                var doctorsWithDates = await Task.WhenAll(pagedDoctors.Select(async doctor =>
                {
                    if (doctor.HasActiveSchedule)
                    {
                        try
                        {
                            var availableDates = await GetAvailableDatesForDoctorAsync(doctor.DoctorId, maxDates: maxDates);
                            doctor.AvailableDates = availableDates ?? new List<AppointmentDateInfo>();
                            _logger.Debug("دریافت {Count} تاریخ نوبت موجود برای پزشک {DoctorId}", 
                                doctor.AvailableDates.Count, doctor.DoctorId);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "خطا در دریافت تاریخ‌های نوبت موجود برای پزشک {DoctorId}", doctor.DoctorId);
                            doctor.AvailableDates = new List<AppointmentDateInfo>();
                        }
                    }
                    else
                    {
                        doctor.AvailableDates = new List<AppointmentDateInfo>();
                    }
                    return doctor;
                }));
                
                // ✅ تبدیل به لیست (Task.WhenAll یک array برمی‌گرداند)
                pagedDoctors = doctorsWithDates.ToList();
                
                var viewModel = new AvailableAppointmentsViewModel
                {
                    Doctors = pagedDoctors,
                    SelectedDoctorId = doctorId,
                    SelectedDate = selectedDate,  // ✅ حالا این تاریخ صحیح است
                    AvailableSlots = new List<AvailableTimeSlotDto>(),
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                // اگر پزشک انتخاب شده، اسلات‌های موجود را دریافت کن
                if (doctorId.HasValue)
                {
                    // ✅ استفاده از selectedDate که قبلاً parse شده
                    var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId.Value, selectedDate);
                    if (slotsResult.Success && slotsResult.Data != null)
                    {
                        viewModel.AvailableSlots = slotsResult.Data.Where(s => s.IsAvailable).ToList();
                        _logger.Information("دریافت {Count} اسلات موجود برای تاریخ {Date}", 
                            viewModel.AvailableSlots.Count, selectedDate.ToString("yyyy/MM/dd"));
                    }
                    else if (!slotsResult.Success)
                    {
                        _logger.Warning("خطا در دریافت اسلات‌ها: {Message}", slotsResult.Message);
                        NotificationHelper.SetWarning(TempData, slotsResult.Message ?? "خطا در دریافت زمان‌های در دسترس");
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نوبت‌های موجود");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری صفحه");
                return View(new AvailableAppointmentsViewModel
                {
                    Doctors = new List<DoctorSearchResultDto>(),
                    AvailableSlots = new List<AvailableTimeSlotDto>(),
                    PageNumber = 1,
                    PageSize = _appSettings.AppointmentDoctorsPageSize,
                    TotalCount = 0
                });
            }
        }

        /// <summary>
        /// دریافت داده‌های نوبت‌های موجود به صورت AJAX (بدون رفرش صفحه)
        /// GET: /Patient/Appointment/GetAvailableData?doctorId={doctorId}&date={date}&searchTerm={searchTerm}
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAvailableData(int? doctorId = null, string date = null, string searchTerm = null)
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت نوبت‌های موجود - DoctorId: {DoctorId}, DateString: {DateString}, SearchTerm: {SearchTerm}",
                    doctorId, date ?? "همه", searchTerm ?? "");

                // ✅ استفاده از Extension Method برای Date Parsing (حذف کد تکراری)
                var selectedDate = this.ParsePersianDateSafe(date, _logger);
                
                // ✅ تبدیل تاریخ میلادی به شمسی برای نمایش (یک بار در ابتدا برای استفاده در کل متد)
                var persianSelectedDate = PersianDateHelper.ToPersianDate(selectedDate);

                // ✅ دریافت لیست پزشکان با فیلتر جستجو
                var doctorsResult = await _bookingService.GetAvailableDoctorsAsync(
                    departmentId: null,
                    searchTerm: searchTerm);
                if (!doctorsResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت لیست پزشکان"
                    }, JsonRequestBehavior.AllowGet);
                }

                // ✅ فیلتر 1: فیلتر بر اساس doctorId (اگر انتخاب شده باشد)
                var filteredDoctors = doctorsResult.Data ?? new List<DoctorSearchResultDto>();
                if (doctorId.HasValue)
                {
                    filteredDoctors = filteredDoctors.Where(d => d.DoctorId == doctorId.Value).ToList();
                }

                // ✅ فیلتر 2: فیلتر بر اساس تاریخ (فقط اگر تاریخ انتخاب شده باشد)
                // ⚠️ مهم: اگر تاریخ انتخاب نشده باشد، همه پزشکان را برمی‌گردانیم
                var doctorsWithAvailableSlots = new List<DoctorSearchResultDto>();
                var hasAnyAvailableSlots = false;
                
                // ✅ فقط اگر تاریخ انتخاب شده باشد، فیلتر تاریخ را اعمال می‌کنیم
                if (!string.IsNullOrWhiteSpace(date) && filteredDoctors.Any())
                {
                    _logger.Information("اعمال فیلتر تاریخ - Date: {Date}, DoctorId: {DoctorId}, TotalDoctors: {Count}",
                        persianSelectedDate, doctorId?.ToString() ?? "همه", filteredDoctors.Count);
                    
                    foreach (var doctor in filteredDoctors)
                    {
                        // بررسی اینکه آیا این پزشک در تاریخ انتخابی نوبت دارد
                        var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctor.DoctorId, selectedDate);
                        if (slotsResult.Success && slotsResult.Data != null && slotsResult.Data.Any(s => s.IsAvailable))
                        {
                            doctorsWithAvailableSlots.Add(doctor);
                            hasAnyAvailableSlots = true;
                        }
                    }
                    
                    _logger.Information("نتیجه فیلتر تاریخ - DoctorsWithSlots: {Count}, HasAnySlots: {HasAny}",
                        doctorsWithAvailableSlots.Count, hasAnyAvailableSlots);
                    
                    // ✅ اگر تاریخ انتخاب شده اما هیچ نوبتی وجود ندارد
                    if (!hasAnyAvailableSlots && filteredDoctors.Any())
                    {
                        return Json(new
                        {
                            success = true,
                            hasNoAppointments = true, // ✅ Flag برای نمایش پیام
                            message = $"در تاریخ {persianSelectedDate} نوبتی در دسترس نیست",
                            data = new
                            {
                                doctors = new List<object>(),
                                selectedDoctorId = doctorId,
                                selectedDate = persianSelectedDate,
                                availableSlots = new List<object>()
                            }
                        }, JsonRequestBehavior.AllowGet);
                    }
                    
                    // ✅ استفاده از لیست فیلتر شده (فقط اگر تاریخ انتخاب شده باشد)
                    filteredDoctors = doctorsWithAvailableSlots;
                }
                else if (string.IsNullOrWhiteSpace(date))
                {
                    // ✅ اگر تاریخ انتخاب نشده باشد، همه پزشکان را برمی‌گردانیم (بدون فیلتر تاریخ)
                    _logger.Information("تاریخ انتخاب نشده - برگرداندن همه پزشکان - DoctorId: {DoctorId}, TotalDoctors: {Count}",
                        doctorId?.ToString() ?? "همه", filteredDoctors.Count);
                }

                var availableSlots = new List<AvailableTimeSlotDto>();

                // اگر پزشک انتخاب شده، اسلات‌های موجود را دریافت کن
                if (doctorId.HasValue)
                {
                    var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId.Value, selectedDate);
                    if (slotsResult.Success && slotsResult.Data != null)
                    {
                        availableSlots = slotsResult.Data.Where(s => s.IsAvailable).ToList();
                    }
                }

                // ✅ تبدیل لیست پزشکان به anonymous type
                // استفاده از conditional operator برای جلوگیری از خطای type mismatch
                object doctorsList;
                if (filteredDoctors != null && filteredDoctors.Any())
                {
                    // ✅ دریافت تاریخ‌های نوبت موجود برای همه پزشکان به صورت موازی
                    // ⚠️ توجه: این هنوز N+1 query است اما با Task.WhenAll بهینه شده
                    // برای بهینه‌سازی بیشتر، می‌توان batch query ایجاد کرد
                    var maxDates = _appSettings.AppointmentAvailableDatesMaxCount;
                    var doctorsWithDates = await Task.WhenAll(filteredDoctors.Select(async d =>
                    {
                        // ✅ استفاده از config برای maxDates
                        var availableDates = await GetAvailableDatesForDoctorAsync(d.DoctorId, maxDates: maxDates);
                        
                        // ✅ تبدیل AvailableDateInfo به anonymous object با camelCase property names
                        var availableDatesJson = availableDates != null && availableDates.Any()
                            ? availableDates.Select(ad => new
                            {
                                persianDate = ad.PersianDate,
                                shortDate = ad.ShortDate,
                                dayName = ad.DayName,
                                dayNameShort = ad.DayNameShort,
                                startTime = ad.StartTime,
                                endTime = ad.EndTime,
                                timeRange = ad.TimeRange
                            }).Cast<object>().ToList()
                            : new List<object>();
                        
                        return new
                        {
                            doctorId = d.DoctorId,
                            fullName = d.FullName,
                            specialization = d.Specialization,
                            bio = d.Bio,
                            profileImageUrl = d.ProfileImageUrl,
                            medicalCouncilCode = d.MedicalCouncilCode,
                            experienceYears = d.ExperienceYears,
                            departmentName = d.DepartmentName,
                            hasActiveSchedule = d.HasActiveSchedule,
                            scheduleInfo = d.ScheduleInfo,
                            availableDates = availableDatesJson, // ✅ تاریخ‌های نوبت موجود با camelCase
                            isSelected = doctorId.HasValue && d.DoctorId == doctorId.Value
                        };
                    }));
                    
                    doctorsList = doctorsWithDates.ToList();
                }
                else
                {
                    doctorsList = new List<object>();
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        doctors = doctorsList,
                        selectedDoctorId = doctorId,
                        selectedDate = persianSelectedDate,
                        availableSlots = availableSlots.Select(s => new
                        {
                            startTime = s.StartTime.ToString(@"hh\:mm"),
                            endTime = s.EndTime.ToString(@"hh\:mm"),
                            displayTime = s.DisplayTime,
                            displayRange = s.DisplayRange,
                            isAvailable = s.IsAvailable,
                            duration = s.Duration
                        }).ToList()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های نوبت‌های موجود");
                return Json(new
                {
                    success = false,
                    message = "خطا در بارگذاری داده‌ها"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// نمایش جزئیات پزشک با رزومه و آمار
        /// GET: /Patient/Appointment/DoctorDetails/{doctorId}
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DoctorDetails(int doctorId, DateTime? selectedDate = null)
        {
            try
            {
                _logger.Information("درخواست جزئیات پزشک - DoctorId: {DoctorId}", doctorId);

                // دریافت جزئیات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    NotificationHelper.SetError(TempData, "پزشک یافت نشد");
                    return RedirectToAction("Available");
                }

                var doctor = doctorResult.Data;

                // دریافت برنامه کاری پزشک
                var scheduleResult = await _bookingService.GetDoctorDetailsAsync(doctorId);
                var schedule = scheduleResult.Success ? scheduleResult.Data : null;

                // دریافت جزئیات برنامه کاری (WorkDays و TimeRanges)
                DoctorScheduleDisplayDto scheduleDetails = null;
                try
                {
                    var scheduleEntity = await _scheduleRepository.GetDoctorScheduleWithDetailsAsync(doctorId);
                    if (scheduleEntity != null)
                    {
                        scheduleDetails = _mappingService.MapToScheduleDisplayDto(scheduleEntity); // ✅ استفاده از Service
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "خطا در دریافت جزئیات برنامه کاری پزشک {DoctorId}", doctorId);
                }

                // دریافت اسلات‌های موجود
                var selectedDateValue = selectedDate ?? DateTime.Today; // ✅ استفاده از Today به جای Now
                var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId, selectedDateValue);
                var availableSlots = slotsResult.Success && slotsResult.Data != null 
                    ? slotsResult.Data.Where(s => s.IsAvailable).ToList() 
                    : new List<AvailableTimeSlotDto>();

                    var viewModel = new ViewModels.Patient.DoctorDetailsViewModel
                {
                    DoctorId = doctorId,
                    Doctor = doctor,
                    Schedule = schedule,
                    ScheduleDetails = scheduleDetails,
                    AvailableSlots = availableSlots,
                    SelectedDate = selectedDateValue,
                    TotalAppointments = 0, // TODO: دریافت از سرویس آمار
                    TodayAppointments = 0, // TODO: دریافت از سرویس آمار
                    AverageRating = 0, // TODO: دریافت از سرویس آمار
                    ExperienceYears = doctor.ExperienceYears ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات پزشک {DoctorId}", doctorId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات پزشک");
                return RedirectToAction("Available");
            }
        }

        // ✅ حذف MapToScheduleDisplayDto - جابجایی به IDoctorMappingService

        /// <summary>
        /// دریافت اسلات‌های زمانی برای پزشک و تاریخ مشخص
        /// GET: /Patient/Appointment/GetTimeSlots?doctorId={doctorId}&date={date}
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        /// <summary>
        /// دریافت اسلات‌های زمانی برای پزشک و تاریخ مشخص
        /// بهینه‌سازی شده: تقسیم به Helper Methods (155 خط → 40 خط)
        /// </summary>
        public async Task<JsonResult> GetTimeSlots(int doctorId, string date)
        {
            try
            {
                var appointmentDate = ParseAppointmentDate(date); // ✅ جدا شد
                var slotsResult = await GetSlotsForDate(doctorId, appointmentDate); // ✅ جدا شد
                return CreateSlotsJsonResponse(slotsResult, doctorId, appointmentDate); // ✅ جدا شد
            }
            catch (Exception ex)
            {
                return HandleTimeSlotsError(ex, doctorId, date); // ✅ جدا شد
            }
        }

        /// <summary>
        /// Parse کردن تاریخ برای GetTimeSlots
        /// پشتیبانی از فرمت شمسی، timestamp و ISO
        /// </summary>
        private DateTime ParseAppointmentDate(string date)
        {
            // ✅ استفاده از Extension Method
            return this.ParsePersianDateSafe(date, _logger);
        }

        /// <summary>
        /// دریافت اسلات‌ها از Service
        /// </summary>
        private async Task<ServiceResult<List<AvailableTimeSlotDto>>> GetSlotsForDate(int doctorId, DateTime appointmentDate)
        {
            _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}",
                doctorId, appointmentDate.ToString("yyyy/MM/dd"));
            
            return await _bookingService.GetAvailableTimeSlotsAsync(doctorId, appointmentDate);
        }

        /// <summary>
        /// ایجاد JSON Response برای Slots
        /// </summary>
        private JsonResult CreateSlotsJsonResponse(
            ServiceResult<List<AvailableTimeSlotDto>> result,
            int doctorId,
            DateTime appointmentDate)
        {
            if (result.Success && result.Data != null && result.Data.Any())
            {
                _logger.Information("اسلات‌های زمانی با موفقیت دریافت شد - DoctorId: {DoctorId}, Count: {Count}",
                    doctorId, result.Data.Count);
                
                return Json(new
                {
                    success = true,
                    slots = result.Data.Select(s => new
                    {
                        startTime = s.StartTime.ToString(@"hh\:mm"),
                        endTime = s.EndTime.ToString(@"hh\:mm"),
                        displayTime = s.DisplayTime,
                        displayRange = s.DisplayRange,
                        isAvailable = s.IsAvailable,
                        duration = s.Duration
                    })
                }, JsonRequestBehavior.AllowGet);
            }
            
            if (result.Success && result.Data != null && !result.Data.Any())
            {
                _logger.Information("هیچ اسلاتی برای این تاریخ یافت نشد - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, appointmentDate.ToString("yyyy/MM/dd"));
                
                return Json(new
                {
                    success = true,
                    slots = new object[0],
                    message = "برای این تاریخ زمانی در دسترس نیست. لطفاً یکی از روزهای کاری پزشک را انتخاب کنید."
                }, JsonRequestBehavior.AllowGet);
            }
            
            _logger.Warning("خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}, Message: {Message}",
                doctorId, appointmentDate.ToString("yyyy/MM/dd"), result?.Message ?? "Unknown error");
            
            return Json(new
            {
                success = false,
                message = result?.Message ?? "خطا در دریافت اسلات‌های در دسترس"
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Error Handling برای GetTimeSlots
        /// </summary>
        private JsonResult HandleTimeSlotsError(Exception ex, int doctorId, string date)
        {
            _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}",
                doctorId, date ?? "null");
            
            if (ex.InnerException != null)
            {
                _logger.Error(ex.InnerException, "InnerException: {Message}",
                    ex.InnerException.Message);
            }
            
            return Json(new
            {
                success = false,
                message = $"خطا در دریافت اسلات‌های در دسترس: {ex.Message}"
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// دریافت تاریخ‌های نوبت موجود برای یک پزشک با اطلاعات کامل (حداکثر maxDates تاریخ آینده)
        /// ✅ بازنویسی کامل و حرفه‌ای برای محیط واقعی
        /// ⚠️ مهم: فقط روزهای کاری پزشک را بررسی می‌کند و اطلاعات کامل (تاریخ، روز، زمان) را برمی‌گرداند
        /// </summary>
        private async Task<List<AppointmentDateInfo>> GetAvailableDatesForDoctorAsync(int doctorId, int? maxDates = null)
        {
            try
            {
                // ✅ استفاده از config برای maxDates و daysToCheck
                var maxDatesValue = maxDates ?? _appSettings.AppointmentAvailableDatesMaxCount;
                var daysToCheck = _appSettings.AppointmentAvailableDatesDaysToCheck;
                
                _logger.Information("🔍 شروع دریافت تاریخ‌های نوبت موجود - DoctorId: {DoctorId}, MaxDates: {MaxDates}, DaysToCheck: {DaysToCheck}", 
                    doctorId, maxDatesValue, daysToCheck);

                // ✅ دریافت برنامه کاری پزشک برای بررسی روزهای کاری
                var schedule = await _scheduleRepository.GetDoctorScheduleWithDetailsAsync(doctorId);
                if (schedule == null || schedule.WorkDays == null || !schedule.WorkDays.Any(wd => wd.IsActive && !wd.IsDeleted))
                {
                    _logger.Warning("⚠️ پزشک {DoctorId} برنامه کاری فعال ندارد", doctorId);
                    return new List<AppointmentDateInfo>();
                }

                // ✅ نام روزهای هفته برای نمایش (شنبه اولین روز هفته در ایران)
                // در سیستم نمایش ایران: شنبه=0, یکشنبه=1, دوشنبه=2, سه‌شنبه=3, چهارشنبه=4, پنج‌شنبه=5, جمعه=6
                var dayNames = new[] { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه" };
                var dayNamesShort = new[] { "ش", "ی", "د", "س", "چ", "پ", "ج" };

                // ✅ دریافت روزهای کاری فعال با اطلاعات TimeRange
                var workDaysWithTimes = schedule.WorkDays
                    .Where(wd => wd.IsActive && !wd.IsDeleted && wd.TimeRanges != null && wd.TimeRanges.Any(tr => tr.IsActive && !tr.IsDeleted))
                    .Select(wd => new
                    {
                        DayOfWeek = wd.DayOfWeek, // در دیتابیس: یکشنبه=0, دوشنبه=1, ..., شنبه=6
                        TimeRanges = wd.TimeRanges
                            .Where(tr => tr.IsActive && !tr.IsDeleted)
                            .OrderBy(tr => tr.StartTime)
                            .ToList()
                    })
                    .ToList();

                _logger.Information("📋 روزهای کاری فعال پیدا شد: {Count} روز", workDaysWithTimes.Count);
                foreach (var wd in workDaysWithTimes)
                {
                    _logger.Debug("  - DayOfWeek: {DayOfWeek}, TimeRanges: {Count}", wd.DayOfWeek, wd.TimeRanges.Count);
                }

                if (!workDaysWithTimes.Any())
                {
                    _logger.Warning("⚠️ پزشک {DoctorId} روز کاری فعال با TimeRange ندارد", doctorId);
                    return new List<AppointmentDateInfo>();
                }

                var availableDates = new List<AppointmentDateInfo>();
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(daysToCheck); // ✅ استفاده از config
                var currentDate = startDate;
                var foundCount = 0;
                var checkedDays = 0;

                _logger.Information("🔍 شروع بررسی از تاریخ {StartDate} تا {EndDate}", startDate.ToString("yyyy/MM/dd"), endDate.ToString("yyyy/MM/dd"));

                while (currentDate <= endDate && foundCount < maxDatesValue)
                {
                    checkedDays++;
                    var cSharpDayOfWeek = (int)currentDate.DayOfWeek; // در C#: Sunday=0, Monday=1, ..., Saturday=6
                    
                    // ✅ تبدیل C# DayOfWeek به دیتابیس DayOfWeek
                    // در C#: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
                    // در دیتابیس: یکشنبه=0, دوشنبه=1, سه‌شنبه=2, چهارشنبه=3, پنج‌شنبه=4, جمعه=5, شنبه=6
                    // تبدیل: Sunday(0) → یکشنبه(0), Monday(1) → دوشنبه(1), ..., Saturday(6) → شنبه(6)
                    // پس: dayOfWeek در C# = dayOfWeek در دیتابیس (بدون تبدیل)
                    var dbDayOfWeek = cSharpDayOfWeek;
                    
                    _logger.Debug("📅 بررسی تاریخ: {Date}, CSharpDayOfWeek: {CSharpDayOfWeek} ({(DayOfWeek)cSharpDayOfWeek}), DbDayOfWeek: {DbDayOfWeek}", 
                        currentDate.ToString("yyyy/MM/dd"), cSharpDayOfWeek, (DayOfWeek)cSharpDayOfWeek, dbDayOfWeek);

                    var workDayInfo = workDaysWithTimes.FirstOrDefault(wd => wd.DayOfWeek == dbDayOfWeek);
                    
                    if (workDayInfo != null)
                    {
                        _logger.Debug("✅ این تاریخ یکی از روزهای کاری پزشک است - DayOfWeek: {DayOfWeek}, TimeRanges: {Count}", 
                            dbDayOfWeek, workDayInfo.TimeRanges.Count);

                        // ✅ بررسی اینکه آیا در این تاریخ نوبت موجود است
                        // ⚠️ مهم: باید واقعاً نوبت موجود باشد، نه فقط اسلات‌های ممکن
                        var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId, currentDate);
                        
                        // ✅ لاگ کامل برای دیباگ
                        var totalSlots = slotsResult?.Data?.Count ?? 0;
                        var availableSlotsCount = slotsResult?.Data?.Count(s => s.IsAvailable) ?? 0;
                        var bookedSlotsCount = slotsResult?.Data?.Count(s => !s.IsAvailable) ?? 0;
                        _logger.Debug("🔍 بررسی نوبت‌های موجود - Date: {Date}, Success: {Success}, HasData: {HasData}, TotalSlots: {TotalSlots}, AvailableSlots: {AvailableSlots}, BookedSlots: {BookedSlots}", 
                            currentDate.ToString("yyyy/MM/dd"), 
                            slotsResult?.Success ?? false,
                            slotsResult?.Data != null,
                            totalSlots,
                            availableSlotsCount,
                            bookedSlotsCount);
                        
                        // ⚠️ مهم: فقط زمانی تاریخ را اضافه می‌کنیم که واقعاً نوبت موجود باشد
                        // یعنی حداقل یک اسلات با IsAvailable = true وجود داشته باشد
                        // ✅ بررسی دقیق: باید واقعاً نوبت موجود باشد (نه فقط اسلات‌های ممکن)
                        if (slotsResult.Success && slotsResult.Data != null && slotsResult.Data.Any(s => s.IsAvailable))
                        {
                            var availableSlots = slotsResult.Data.Where(s => s.IsAvailable).ToList();
                            
                            // ✅ بررسی اضافی: اطمینان از اینکه واقعاً نوبت موجود است
                            if (availableSlots == null || availableSlots.Count == 0)
                            {
                                _logger.Warning("⚠️ هیچ نوبت موجودی در این تاریخ پیدا نشد (با وجود Any(s => s.IsAvailable) = true) - Date: {Date}, TotalSlots: {TotalSlots}", 
                                    currentDate.ToString("yyyy/MM/dd"), totalSlots);
                                currentDate = currentDate.AddDays(1);
                                continue; // به تاریخ بعدی برو
                            }
                            
                            // ✅ بررسی نهایی: اطمینان از اینکه حداقل یک اسلات واقعاً موجود است
                            // (نه فقط اسلات‌های ممکن که ممکن است همه رزرو شده باشند)
                            var trulyAvailableSlots = availableSlots.Where(s => s.IsAvailable).ToList();
                            if (trulyAvailableSlots.Count == 0)
                            {
                                _logger.Warning("⚠️ هیچ اسلات واقعاً موجودی در این تاریخ پیدا نشد - Date: {Date}, TotalSlots: {TotalSlots}, AvailableSlots: {AvailableSlots}", 
                                    currentDate.ToString("yyyy/MM/dd"), totalSlots, availableSlots.Count);
                                currentDate = currentDate.AddDays(1);
                                continue; // به تاریخ بعدی برو
                            }
                            
                            _logger.Debug("✅ نوبت موجود پیدا شد - {Count} اسلات", trulyAvailableSlots.Count);

                            // ✅ دریافت اولین TimeRange واقعی که نوبت موجود دارد
                            // ⚠️ مهم: باید از TimeRange واقعی نوبت موجود استفاده کنیم، نه اولین TimeRange روز کاری
                            var firstAvailableSlot = trulyAvailableSlots.OrderBy(s => s.StartTime).FirstOrDefault();
                            if (firstAvailableSlot != null)
                            {
                                // ✅ پیدا کردن TimeRange متناظر با این اسلات
                                var matchingTimeRange = workDayInfo.TimeRanges.FirstOrDefault(tr => 
                                    tr.StartTime <= firstAvailableSlot.StartTime && 
                                    tr.EndTime >= firstAvailableSlot.EndTime);
                                
                                // ✅ اگر TimeRange متناظر پیدا نشد، از اولین TimeRange استفاده می‌کنیم
                                var timeRangeToUse = matchingTimeRange ?? workDayInfo.TimeRanges.FirstOrDefault();
                                
                                if (timeRangeToUse != null)
                                {
                                    // تبدیل به تاریخ شمسی
                                    var persianDate = PersianDateHelper.ToPersianDate(currentDate);
                                    if (!string.IsNullOrEmpty(persianDate) && persianDate != "0000/00/00")
                                    {
                                        // ✅ استخراج روز و ماه از تاریخ شمسی (مثلاً: "08/10" از "1404/10/08")
                                        var dateParts = persianDate.Split('/');
                                        var shortDate = dateParts.Length >= 3 ? $"{dateParts[2]}/{dateParts[1]}" : persianDate;
                                        
                                        // ✅ فرمت زمان از TimeRange واقعی
                                        var startTime = TimeFormatHelper.FormatTimeToPersian(timeRangeToUse.StartTime);
                                        var endTime = TimeFormatHelper.FormatTimeToPersian(timeRangeToUse.EndTime);
                                        var timeRange = TimeFormatHelper.FormatTimeRangeToPersian(timeRangeToUse.StartTime, timeRangeToUse.EndTime);
                                        
                                        // ✅ تبدیل دیتابیس به ایران برای نمایش
                                        // دیتابیس: یکشنبه=0, دوشنبه=1, سه‌شنبه=2, چهارشنبه=3, پنج‌شنبه=4, جمعه=5, شنبه=6
                                        // ایران (شنبه اولین): شنبه=0, یکشنبه=1, دوشنبه=2, سه‌شنبه=3, چهارشنبه=4, پنج‌شنبه=5, جمعه=6
                                        // تبدیل: یکشنبه(0) → یکشنبه(1), دوشنبه(1) → دوشنبه(2), ..., شنبه(6) → شنبه(0)
                                        // فرمول: (dbDayOfWeek + 1) % 7
                                        var iranDayOfWeek = (dbDayOfWeek + 1) % 7;
                                        
                                        // ✅ بررسی تکراری نبودن: اگر این تاریخ قبلاً اضافه شده باشد، اضافه نمی‌کنیم
                                        var isDuplicate = availableDates.Any(ad => ad.PersianDate == persianDate);
                                        if (isDuplicate)
                                        {
                                            _logger.Debug("⚠️ تاریخ تکراری پیدا شد - Date: {Date}, PersianDate: {PersianDate}", 
                                                currentDate.ToString("yyyy/MM/dd"), persianDate);
                                        }
                                        else
                                        {
                                            availableDates.Add(new AppointmentDateInfo
                                            {
                                                PersianDate = persianDate,
                                                ShortDate = shortDate,
                                                DayName = dayNames[iranDayOfWeek],
                                                DayNameShort = dayNamesShort[iranDayOfWeek],
                                                StartTime = startTime,
                                                EndTime = endTime,
                                                TimeRange = timeRange
                                            });
                                            
                                            foundCount++;
                                            
                                            _logger.Information("✅ تاریخ نوبت موجود پیدا شد - DoctorId: {DoctorId}, Date: {Date}, PersianDate: {PersianDate}, DbDayOfWeek: {DbDayOfWeek}, IranDayOfWeek: {IranDayOfWeek}, Day: {Day}, Time: {Time}", 
                                                doctorId, currentDate.ToString("yyyy/MM/dd"), persianDate, dbDayOfWeek, iranDayOfWeek, dayNames[iranDayOfWeek], timeRange);
                                        }
                                    }
                                    else
                                    {
                                        _logger.Warning("⚠️ تبدیل تاریخ شمسی نامعتبر - Date: {Date}, PersianDate: {PersianDate}", 
                                            currentDate.ToString("yyyy/MM/dd"), persianDate);
                                    }
                                }
                                else
                                {
                                    _logger.Warning("⚠️ TimeRange برای این روز کاری پیدا نشد - DayOfWeek: {DayOfWeek}", dbDayOfWeek);
                                }
                            }
                            else
                            {
                                _logger.Warning("⚠️ هیچ اسلات موجودی پیدا نشد - Date: {Date}", currentDate.ToString("yyyy/MM/dd"));
                            }
                        }
                        else
                        {
                            _logger.Debug("ℹ️ در این تاریخ نوبت موجود نیست - Date: {Date}, Success: {Success}, HasData: {HasData}, HasAvailable: {HasAvailable}", 
                                currentDate.ToString("yyyy/MM/dd"), slotsResult?.Success ?? false, 
                                slotsResult?.Data != null, slotsResult?.Data?.Any(s => s.IsAvailable) ?? false);
                        }
                    }
                    else
                    {
                        _logger.Debug("ℹ️ این تاریخ روز کاری پزشک نیست - Date: {Date}, DayOfWeek: {DayOfWeek}", 
                            currentDate.ToString("yyyy/MM/dd"), dbDayOfWeek);
                    }
                    
                    currentDate = currentDate.AddDays(1);
                }

                // ✅ مرتب‌سازی نوبت‌ها بر اساس تاریخ (از قدیمی‌ترین به جدیدترین)
                availableDates = availableDates
                    .OrderBy(ad => 
                    {
                        // تبدیل تاریخ شمسی به DateTime برای مقایسه
                        if (!string.IsNullOrEmpty(ad.PersianDate))
                        {
                            try
                            {
                                var parts = ad.PersianDate.Split('/');
                                if (parts.Length >= 3 && int.TryParse(parts[0], out int year) && 
                                    int.TryParse(parts[1], out int month) && int.TryParse(parts[2], out int day))
                                {
                                    // تبدیل تاریخ شمسی به میلادی برای مقایسه
                                    var persianCalendar = new PersianCalendar();
                                    var dateTime = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                                    return dateTime;
                                }
                            }
                            catch
                            {
                                // در صورت خطا، از تاریخ امروز استفاده می‌کنیم
                            }
                        }
                        return DateTime.MaxValue; // نوبت‌های بدون تاریخ در آخر
                    })
                    .ToList();
                
                _logger.Information("✅ دریافت تاریخ‌های نوبت موجود تکمیل شد - DoctorId: {DoctorId}, Found: {Found}, Checked: {Checked}, TotalDays: {TotalDays}", 
                    doctorId, availableDates.Count, checkedDays, (endDate - startDate).Days);
                
                return availableDates;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت تاریخ‌های نوبت موجود برای پزشک {DoctorId}", doctorId);
                return new List<AppointmentDateInfo>(); // در صورت خطا، لیست خالی برمی‌گردانیم
            }
        }

        /// <summary>
        /// نمایش لیست نوبت‌های بیمار (نیاز به لاگین)
        /// GET: /Patient/Appointment/MyAppointments
        /// </summary>
        [HttpGet]
        [Authorize] // فقط برای کاربران لاگین شده
        public async Task<ActionResult> MyAppointments(
            DateTime? startDate,
            DateTime? endDate,
            AppointmentStatus? status,
            string searchTerm,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                _logger.Information("درخواست نمایش نوبت‌های بیمار - UserId: {UserId}",
                    _currentUserService.UserId);

                // دریافت شناسه بیمار از کاربر فعلی
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // دریافت نوبت‌ها
                var result = await _bookingService.GetPatientAppointmentsAsync(
                    patientId.Value,
                    startDate,
                    endDate);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message ?? "خطا در دریافت نوبت‌ها");
                    return View(new PatientAppointmentListViewModel
                    {
                        Appointments = new System.Collections.Generic.List<PatientAppointmentDto>(),
                        PageNumber = page,
                        PageSize = pageSize
                    });
                }

                // فیلتر بر اساس وضعیت
                var appointments = result.Data;
                if (status.HasValue)
                {
                    appointments = appointments.Where(a => a.Status == status.Value).ToList();
                }

                // جستجو بر اساس نام پزشک
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    appointments = appointments
                        .Where(a => a.DoctorName.ToLower().Contains(searchLower))
                        .ToList();
                }

                // Pagination
                var totalCount = appointments.Count;
                var pagedAppointments = appointments
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new ViewModels.Patient.PatientAppointmentListViewModel
                {
                    Appointments = pagedAppointments,
                    StartDateFilter = startDate,
                    EndDateFilter = endDate,
                    StatusFilter = status,
                    SearchTerm = searchTerm,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نوبت‌های بیمار");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری نوبت‌ها");
                return View(new ViewModels.Patient.PatientAppointmentListViewModel
                {
                    Appointments = new System.Collections.Generic.List<PatientAppointmentDto>(),
                    PageNumber = page,
                    PageSize = pageSize
                });
            }
        }

        /// <summary>
        /// نمایش جزئیات یک نوبت
        /// GET: /Patient/Appointment/Details/{id}
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync(); // ✅ از Base
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد"); // ✅ از Base
                }

                var result = await _bookingService.GetAppointmentDetailsAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message); // ✅ از Base
                }

                return SuccessJsonResult(result.Data); // ✅ از Base
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطا در دریافت جزئیات نوبت" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// لغو نوبت
        /// POST: /Patient/Appointment/Cancel/{id}
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Cancel(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync(); // ✅ از Base
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد"); // ✅ از Base
                }

                var result = await _bookingService.CancelAppointmentAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message); // ✅ از Base
                }

                NotificationHelper.SetSuccess(TempData, "نوبت با موفقیت لغو شد");
                return SuccessJsonResult(null, result.Message); // ✅ از Base
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطا در لغو نوبت" });
            }
        }

        // ✅ حذف GetCurrentPatientIdAsync - از BasePatientController استفاده می‌شود
    }

}
