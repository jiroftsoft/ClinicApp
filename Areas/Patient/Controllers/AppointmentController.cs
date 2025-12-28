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
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Patient;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities.Doctor;
using Serilog;
using static ClinicApp.Helpers.NotificationHelper;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Extensions;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای مدیریت نوبت‌های بیمار
    /// بهینه‌سازی شده طبق appointment_controller_review.md
    /// </summary>
    [AllowAnonymous] // اجازه دسترسی عمومی برای مشاهده نوبت‌ها
    public class AppointmentController : BasePatientController
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly IDoctorMappingService _mappingService;

        public AppointmentController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IDoctorCrudService doctorCrudService,
            IDoctorScheduleRepository scheduleRepository,
            IDoctorMappingService mappingService,
            ILogger logger)
            : base(logger, currentUserService) // ✅ Base Constructor
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
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
            int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست نمایش نوبت‌های موجود - DoctorId: {DoctorId}, DateString: {DateString}",
                    doctorId, date ?? "همه");

                // دریافت لیست پزشکان
                var doctorsResult = await _bookingService.GetAvailableDoctorsAsync();
                if (!doctorsResult.Success)
                {
                    NotificationHelper.SetError(TempData, "خطا در دریافت لیست پزشکان");
                    return View(new AvailableAppointmentsViewModel
                    {
                        Doctors = new List<DoctorSearchResultDto>(),
                        AvailableSlots = new List<AvailableTimeSlotDto>()
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
                
                var viewModel = new AvailableAppointmentsViewModel
                {
                    Doctors = doctorsResult.Data ?? new List<DoctorSearchResultDto>(),
                    SelectedDoctorId = doctorId,
                    SelectedDate = selectedDate,  // ✅ حالا این تاریخ صحیح است
                    AvailableSlots = new List<AvailableTimeSlotDto>()
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
                    AvailableSlots = new List<AvailableTimeSlotDto>()
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

                // ✅ استفاده از Extension Method برای Date Parsing (حذف کد تکراری)
                var selectedDate = this.ParsePersianDateSafe(date, _logger);

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

                // ✅ تبدیل تاریخ میلادی به شمسی برای نمایش
                var persianSelectedDate = PersianDateHelper.ToPersianDate(selectedDate);

                // ✅ تبدیل لیست پزشکان به anonymous type
                // استفاده از conditional operator برای جلوگیری از خطای type mismatch
                object doctorsList;
                if (doctorsResult.Data != null && doctorsResult.Data.Any())
                {
                    doctorsList = doctorsResult.Data.Select(d => new
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
                        isSelected = doctorId.HasValue && d.DoctorId == doctorId.Value
                    }).ToList();
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
