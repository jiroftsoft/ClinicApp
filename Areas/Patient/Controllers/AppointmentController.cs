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

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای مدیریت نوبت‌های بیمار
    /// </summary>
    [AllowAnonymous] // اجازه دسترسی عمومی برای مشاهده نوبت‌ها
    public class AppointmentController : Controller
    {
        private readonly IAppointmentBookingService _bookingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly IDoctorScheduleRepository _scheduleRepository;
        private readonly ILogger _logger;

        public AppointmentController(
            IAppointmentBookingService bookingService,
            ICurrentUserService currentUserService,
            IDoctorCrudService doctorCrudService,
            IDoctorScheduleRepository scheduleRepository,
            ILogger logger)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
            _logger = logger?.ForContext<AppointmentController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// صفحه عمومی نمایش نوبت‌های موجود (بدون نیاز به لاگین)
        /// GET: /Patient/Appointment/Available
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Available(
            int? doctorId = null,
            DateTime? date = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                _logger.Information("درخواست نمایش نوبت‌های موجود - DoctorId: {DoctorId}, Date: {Date}",
                    doctorId, date?.ToString("yyyy/MM/dd") ?? "همه");

                // دریافت لیست پزشکان
                var doctorsResult = await _bookingService.GetAvailableDoctorsAsync();
                if (!doctorsResult.Success)
                {
                    TempData["Error"] = "خطا در دریافت لیست پزشکان";
                    return View(new AvailableAppointmentsViewModel
                    {
                        Doctors = new List<DoctorSearchResultDto>(),
                        AvailableSlots = new List<AvailableTimeSlotDto>()
                    });
                }

                var viewModel = new AvailableAppointmentsViewModel
                {
                    Doctors = doctorsResult.Data ?? new List<DoctorSearchResultDto>(),
                    SelectedDoctorId = doctorId,
                    SelectedDate = date ?? DateTime.Now,
                    AvailableSlots = new List<AvailableTimeSlotDto>()
                };

                // اگر پزشک و تاریخ انتخاب شده، اسلات‌های موجود را دریافت کن
                if (doctorId.HasValue && date.HasValue)
                {
                    var slotsResult = await _bookingService.GetAvailableTimeSlotsAsync(doctorId.Value, date.Value);
                    if (slotsResult.Success && slotsResult.Data != null)
                    {
                        viewModel.AvailableSlots = slotsResult.Data.Where(s => s.IsAvailable).ToList();
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نوبت‌های موجود");
                TempData["Error"] = "خطا در بارگذاری صفحه";
                return View(new AvailableAppointmentsViewModel
                {
                    Doctors = new List<DoctorSearchResultDto>(),
                    AvailableSlots = new List<AvailableTimeSlotDto>()
                });
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
                    TempData["Error"] = "پزشک یافت نشد";
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
                        scheduleDetails = MapToScheduleDisplayDto(scheduleEntity);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "خطا در دریافت جزئیات برنامه کاری پزشک {DoctorId}", doctorId);
                }

                // دریافت اسلات‌های موجود
                var selectedDateValue = selectedDate ?? DateTime.Now;
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
                TempData["Error"] = "خطا در بارگذاری اطلاعات پزشک";
                return RedirectToAction("Available");
            }
        }

        /// <summary>
        /// تبدیل Entity به DTO برای نمایش برنامه کاری
        /// </summary>
        private DoctorScheduleDisplayDto MapToScheduleDisplayDto(DoctorSchedule schedule)
        {
            if (schedule == null) return null;

            var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
            var dayNamesShort = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };

            var dto = new DoctorScheduleDisplayDto
            {
                ScheduleId = schedule.ScheduleId,
                DoctorId = schedule.DoctorId,
                AppointmentDuration = schedule.AppointmentDuration,
                ConsultationFee = schedule.ConsultationFee,
                IsActive = schedule.IsActive
            };

            // تبدیل WorkDays
            if (schedule.WorkDays != null)
            {
                foreach (var workDay in schedule.WorkDays.Where(wd => wd.IsActive && !wd.IsDeleted).OrderBy(wd => wd.DayOfWeek))
                {
                    var workDayDto = new WorkDayDisplayDto
                    {
                        WorkDayId = workDay.WorkDayId,
                        DayOfWeek = workDay.DayOfWeek,
                        DayName = dayNames[workDay.DayOfWeek],
                        DayNameShort = dayNamesShort[workDay.DayOfWeek],
                        IsActive = workDay.IsActive
                    };

                    // تبدیل TimeRanges
                    if (workDay.TimeRanges != null)
                    {
                        foreach (var timeRange in workDay.TimeRanges.Where(tr => tr.IsActive && !tr.IsDeleted).OrderBy(tr => tr.StartTime))
                        {
                            workDayDto.TimeRanges.Add(new TimeRangeDisplayDto
                            {
                                TimeRangeId = timeRange.TimeRangeId,
                                StartTime = timeRange.StartTime.ToString(@"hh\:mm"),
                                EndTime = timeRange.EndTime.ToString(@"hh\:mm"),
                                DisplayTime = TimeFormatHelper.FormatTimeToPersian(timeRange.StartTime),
                                DisplayRange = TimeFormatHelper.FormatTimeRangeToPersian(timeRange.StartTime, timeRange.EndTime),
                                IsActive = timeRange.IsActive
                            });
                        }
                    }

                    dto.WorkDays.Add(workDayDto);
                }
            }

            return dto;
        }

        /// <summary>
        /// دریافت اسلات‌های زمانی برای پزشک و تاریخ مشخص
        /// GET: /Patient/Appointment/GetTimeSlots?doctorId={doctorId}&date={date}
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetTimeSlots(int doctorId, string date)
        {
            try
            {
                // تبدیل تاریخ از فرمت شمسی به میلادی
                DateTime appointmentDate;
                if (string.IsNullOrEmpty(date))
                {
                    appointmentDate = DateTime.Now;
                }
                else
                {
                    try
                    {
                        // persian-datepicker تاریخ را به فرمت YYYY/MM/DD ارسال می‌کند
                        // اما ممکن است به صورت timestamp هم ارسال شود
                        
                        // اول: بررسی timestamp (milliseconds)
                        if (long.TryParse(date, out long timestamp))
                        {
                            // تبدیل timestamp به DateTime
                            // توجه: persian-datepicker ممکن است timestamp را به صورت milliseconds ارسال کند
                            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            
                            // اگر timestamp خیلی بزرگ است (بیش از 10 رقم)، احتمالاً milliseconds است
                            // اگر کوچک است (کمتر از 10 رقم)، احتمالاً seconds است
                            if (timestamp > 9999999999)
                            {
                                // milliseconds
                                appointmentDate = epoch.AddMilliseconds(timestamp).ToLocalTime();
                            }
                            else
                            {
                                // seconds
                                appointmentDate = epoch.AddSeconds(timestamp).ToLocalTime();
                            }
                            
                            // تنظیم زمان به 00:00:00 برای مقایسه دقیق
                            appointmentDate = appointmentDate.Date;
                            
                            _logger.Information("تاریخ از timestamp تبدیل شد: {Timestamp} -> {Date}, DayOfWeek: {DayOfWeek}", 
                                timestamp, appointmentDate.ToString("yyyy/MM/dd HH:mm:ss"), appointmentDate.DayOfWeek);
                        }
                        // دوم: بررسی تاریخ شمسی (YYYY/MM/DD)
                        else if (date.Contains("/") && date.Split('/').Length == 3)
                        {
                            var parts = date.Split('/');
                            var year = int.Parse(parts[0]);
                            var month = int.Parse(parts[1]);
                            var day = int.Parse(parts[2]);
                            
                            var persianCalendar = new System.Globalization.PersianCalendar();
                            appointmentDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                            _logger.Information("تاریخ شمسی تبدیل شد: {PersianDate} -> {Date}", date, appointmentDate.ToString("yyyy/MM/dd HH:mm:ss"));
                        }
                        // سوم: استفاده از PersianDateHelper
                        else
                        {
                            try
                            {
                                appointmentDate = PersianDateHelper.ToGregorianDate(date);
                                _logger.Information("تاریخ با PersianDateHelper تبدیل شد: {PersianDate} -> {Date}", date, appointmentDate.ToString("yyyy/MM/dd HH:mm:ss"));
                            }
                            catch
                            {
                                // آخرین تلاش: parse مستقیم
                                if (DateTime.TryParse(date, out appointmentDate))
                                {
                                    _logger.Information("تاریخ به صورت مستقیم parse شد: {Date}", appointmentDate.ToString("yyyy/MM/dd HH:mm:ss"));
                                }
                                else
                                {
                                    throw new FormatException($"فرمت تاریخ نامعتبر: {date}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در تبدیل تاریخ {Date}، استفاده از تاریخ امروز. Exception: {ExceptionMessage}", date, ex.Message);
                        appointmentDate = DateTime.Now.Date; // فقط تاریخ، بدون زمان
                    }
                }

                _logger.Information("درخواست دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}, ConvertedDate: {ConvertedDate}, DayOfWeek: {DayOfWeek}",
                    doctorId, date, appointmentDate.ToString("yyyy/MM/dd HH:mm:ss"), appointmentDate.DayOfWeek);
                
                System.Diagnostics.Debug.WriteLine($"[GetTimeSlots] 🔍 DoctorId: {doctorId}, Date Input: {date}, Converted: {appointmentDate:yyyy/MM/dd}, DayOfWeek: {appointmentDate.DayOfWeek} ({(int)appointmentDate.DayOfWeek})");

                var result = await _bookingService.GetAvailableTimeSlotsAsync(doctorId, appointmentDate);
                
                if (result.Success && result.Data != null)
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
                else
                {
                    _logger.Warning("خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}, Message: {Message}",
                        doctorId, appointmentDate.ToString("yyyy/MM/dd"), result?.Message ?? "Unknown error");
                    return Json(new
                    {
                        success = false,
                        message = result?.Message ?? "خطا در دریافت اسلات‌های در دسترس"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های زمانی - DoctorId: {DoctorId}, Date: {Date}, Exception: {ExceptionMessage}, StackTrace: {StackTrace}",
                    doctorId, date ?? "null", ex.Message, ex.StackTrace);
                return Json(new
                {
                    success = false,
                    message = $"خطا در دریافت اسلات‌های در دسترس: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
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
                    TempData["Error"] = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.";
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                // دریافت نوبت‌ها
                var result = await _bookingService.GetPatientAppointmentsAsync(
                    patientId.Value,
                    startDate,
                    endDate);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "خطا در دریافت نوبت‌ها";
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
                TempData["Error"] = "خطا در بارگذاری نوبت‌ها";
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
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var result = await _bookingService.GetAppointmentDetailsAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
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
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return Json(new { success = false, message = "اطلاعات بیمار یافت نشد" });
                }

                var result = await _bookingService.CancelAppointmentAsync(id, patientId.Value);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                TempData["Success"] = "نوبت با موفقیت لغو شد";
                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو نوبت {AppointmentId}", id);
                return Json(new { success = false, message = "خطا در لغو نوبت" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// دریافت شناسه بیمار از کاربر فعلی
        /// </summary>
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
