using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Linq;
using System.Collections.Generic; // Added for List

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت برنامه‌های کاری پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت برنامه‌های کاری و زمان‌بندی پزشکان
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorScheduleController : Controller
    {
        private readonly IDoctorScheduleService _doctorScheduleService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorScheduleViewModel> _scheduleValidator;
        private readonly ILogger _logger;

        public DoctorScheduleController(
            IDoctorScheduleService doctorScheduleService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorScheduleViewModel> scheduleValidator
            )
        {
            _doctorScheduleService = doctorScheduleService ?? throw new ArgumentNullException(nameof(doctorScheduleService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _scheduleValidator = scheduleValidator ?? throw new ArgumentNullException(nameof(scheduleValidator));
            _logger = Log.ForContext<DoctorScheduleController>();
        }

        #region Index and List Operations

        /// <summary>
        /// نمایش لیست برنامه‌های کاری پزشکان
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست برنامه‌های کاری پزشکان. Page: {Page}, PageSize: {PageSize}", page, pageSize);

                // دریافت لیست برنامه‌های کاری
                var result = await _doctorScheduleService.GetAllDoctorSchedulesAsync(searchTerm, page, pageSize);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<DoctorScheduleViewModel>(new List<DoctorScheduleViewModel>(), 0, page, pageSize));
                }

                _logger.Information("لیست برنامه‌های کاری با موفقیت بازیابی شد. TotalItems: {TotalItems}", result.Data.TotalItems);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست برنامه‌های کاری پزشکان");
                TempData["Error"] = "خطا در بارگذاری لیست برنامه‌های کاری";
                return View(new PagedResult<DoctorScheduleViewModel>(new List<DoctorScheduleViewModel>(), 0, page, pageSize));
            }
        }

        #endregion

        #region Schedule Management

        /// <summary>
        /// نمایش برنامه کاری پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Schedule(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش برنامه کاری پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // دریافت برنامه کاری
                var scheduleResult = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId);
                
                // ایجاد ViewModel با اطلاعات پزشک
                var viewModel = scheduleResult.Success && scheduleResult.Data != null 
                    ? scheduleResult.Data 
                    : new DoctorScheduleViewModel
                    {
                        DoctorId = doctorId,
                        DoctorName = doctorResult.Data.FullName,
                        NationalCode = doctorResult.Data.NationalCode,
                        MedicalCouncilCode = doctorResult.Data.MedicalCouncilCode,
                        SpecializationNames = doctorResult.Data.SpecializationNames ?? new List<string>(),
                        WorkDays = new List<WorkDayViewModel>(),
                        AppointmentDuration = 30
                    };

                // اگر برنامه کاری موجود است، اطلاعات پزشک را به‌روزرسانی کن
                if (scheduleResult.Success && scheduleResult.Data != null)
                {
                    viewModel.DoctorName = doctorResult.Data.FullName;
                    viewModel.NationalCode = doctorResult.Data.NationalCode;
                    viewModel.MedicalCouncilCode = doctorResult.Data.MedicalCouncilCode;
                    viewModel.SpecializationNames = doctorResult.Data.SpecializationNames ?? new List<string>();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش برنامه کاری پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری برنامه کاری پزشک";
                return RedirectToAction("Index", "DoctorSchedule");
            }
        }

        #endregion

        #region Time Blocking

        /// <summary>
        /// مسدود کردن بازه زمانی برای پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BlockTime(int doctorId, DateTime startTime, DateTime endTime, string reason)
        {
            try
            {
                _logger.Information("درخواست مسدود کردن بازه زمانی برای پزشک {DoctorId} از {StartTime} تا {EndTime} توسط کاربر {UserId}", 
                    doctorId, startTime, endTime, _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });
                }

                if (startTime >= endTime)
                {
                    return Json(new { success = false, message = "زمان شروع باید قبل از زمان پایان باشد." });
                }

                if (startTime < DateTime.Now)
                {
                    return Json(new { success = false, message = "نمی‌توانید زمان‌های گذشته را مسدود کنید." });
                }

                // مسدود کردن بازه زمانی
                var result = await _doctorScheduleService.BlockTimeRangeForDoctorAsync(doctorId, startTime, endTime, reason ?? "مسدود شده توسط مدیر");

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("بازه زمانی برای پزشک {DoctorId} با موفقیت مسدود شد", doctorId);

                return Json(new { success = true, message = "بازه زمانی با موفقیت مسدود شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مسدود کردن بازه زمانی برای پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در مسدود کردن بازه زمانی" });
            }
        }

        #endregion

        #region Available Slots

        /// <summary>
        /// دریافت اسلات‌های در دسترس برای نوبت‌دهی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AvailableSlots(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("درخواست دریافت اسلات‌های در دسترس برای پزشک {DoctorId} در تاریخ {Date} توسط کاربر {UserId}", 
                    doctorId, date.ToString("yyyy/MM/dd"), _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                if (date.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای تاریخ‌های گذشته اسلات دریافت کنید." }, JsonRequestBehavior.AllowGet);
                }

                // دریافت اسلات‌های در دسترس
                var result = await _doctorScheduleService.GetAvailableAppointmentSlotsAsync(doctorId, date);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var slots = result.Data?.Select(slot => new
                {
                    startTime = slot.StartTime.ToString("HH:mm"),
                    endTime = slot.EndTime.ToString("HH:mm"),
                    isAvailable = slot.IsAvailable,
                    status = slot.Status
                });

                return Json(new { success = true, data = slots }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اسلات‌های در دسترس برای پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت اسلات‌های در دسترس" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Schedule Overview

        /// <summary>
        /// نمایش نمای کلی برنامه‌های کاری
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Overview(int? clinicId = null, int? departmentId = null)
        {
            try
            {
                _logger.Information("درخواست نمایش نمای کلی برنامه‌های کاری توسط کاربر {UserId}", _currentUserService.UserId);

                // ایجاد ViewModel برای Overview
                var overviewModel = new ScheduleOverviewViewModel
                {
                    ClinicId = clinicId,
                    DepartmentId = departmentId,
                    TotalDoctors = 0, // در آینده از Service دریافت شود
                    ActiveSchedules = 0, // در آینده از Service دریافت شود
                    TotalAppointments = 0 // در آینده از Service دریافت شود
                };

                return View(overviewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نمای کلی برنامه‌های کاری");
                TempData["Error"] = "خطا در بارگذاری نمای کلی برنامه‌های کاری";
                return View(new ScheduleOverviewViewModel());
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// عملیات سریع برنامه کاری (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuickScheduleOperation(DoctorScheduleViewModel model, string operation = "create")
        {
            try
            {
                _logger.Information("درخواست عملیات سریع برنامه کاری برای پزشک {DoctorId} - عملیات: {Operation}", 
                    model.DoctorId, operation);

                // استفاده از AssignSchedule بهبود یافته
                return await AssignSchedule(model, true, operation);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در عملیات سریع برنامه کاری برای پزشک {DoctorId}", model.DoctorId);
                return Json(new { success = false, message = "خطا در انجام عملیات" });
            }
        }

        /// <summary>
        /// دریافت برنامه کاری پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorSchedule(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت برنامه کاری پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت برنامه کاری پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی در دسترس بودن پزشک در زمان مشخص (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorAvailability(int doctorId, DateTime dateTime)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                if (dateTime < DateTime.Now)
                {
                    return Json(new { success = false, message = "نمی‌توانید برای زمان‌های گذشته بررسی کنید." }, JsonRequestBehavior.AllowGet);
                }

                // دریافت اسلات‌های در دسترس برای آن روز
                var result = await _doctorScheduleService.GetAvailableAppointmentSlotsAsync(doctorId, dateTime.Date);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                // بررسی اینکه آیا زمان مورد نظر در دسترس است
                var timeOfDay = dateTime.TimeOfDay;
                var isAvailable = result.Data?.Any(slot => 
                    slot.StartTime <= timeOfDay && 
                    slot.EndTime > timeOfDay && 
                    slot.IsAvailable) ?? false;

                return Json(new { success = true, isAvailable = isAvailable }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی در دسترس بودن پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در بررسی در دسترس بودن پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Assignment Operations

        /// <summary>
        /// نمایش فرم تنظیم برنامه کاری پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AssignSchedule(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم تنظیم برنامه کاری پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                var doctor = doctorResult.Data;

                // دریافت برنامه کاری موجود
                var scheduleResult = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId.Value);
                var model = scheduleResult.Success && scheduleResult.Data != null ? scheduleResult.Data : new DoctorScheduleViewModel
                {
                    DoctorId = doctorId.Value,
                    AppointmentDuration = 30,
                    WorkDays = new List<WorkDayViewModel>()
                };

                // اطمینان از اینکه WorkDays null نباشد
                if (model.WorkDays == null)
                {
                    model.WorkDays = new List<WorkDayViewModel>();
                }

                // تنظیم روزهای هفته
                if (!model.WorkDays.Any())
                {
                    var daysOfWeek = new[] { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه" };
                    for (int i = 0; i < 7; i++)
                    {
                        model.WorkDays.Add(new WorkDayViewModel
                        {
                            DayOfWeek = i,
                            DayName = daysOfWeek[i],
                            IsActive = false,
                            TimeRanges = new List<TimeRangeViewModel>()
                        });
                    }
                }

                ViewBag.Doctor = doctor;

                _logger.Information("فرم تنظیم برنامه کاری پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم تنظیم برنامه کاری پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم تنظیم برنامه کاری";
                return RedirectToAction("Index", "DoctorSchedule");
            }
        }

        /// <summary>
        /// پردازش تنظیم برنامه کاری پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignSchedule(DoctorScheduleViewModel model, bool isAjax = false, string operation = "create")
        {
            try
            {
                _logger.Information("درخواست تنظیم برنامه کاری پزشک {DoctorId} - عملیات: {Operation} - AJAX: {IsAjax}", 
                    model.DoctorId, operation, isAjax);

                if (!ModelState.IsValid)
                {
                    var errorMessage = "اطلاعات وارد شده نامعتبر است";
                    _logger.Warning("مدل برنامه کاری نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    
                    if (isAjax)
                        return Json(new { success = false, message = errorMessage });
                    
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _scheduleValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    var errorMessage = $"خطا در اعتبارسنجی: {errors}";
                    _logger.Warning("اعتبارسنجی برنامه کاری پزشک {DoctorId} ناموفق بود: {Errors}", model.DoctorId, errors);
                    
                    if (isAjax)
                        return Json(new { success = false, message = errorMessage });
                    
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                // تنظیم برنامه کاری پزشک
                var result = await _doctorScheduleService.SetDoctorScheduleAsync(model.DoctorId, model);

                if (!result.Success)
                {
                    _logger.Warning("تنظیم برنامه کاری پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    
                    if (isAjax)
                        return Json(new { success = false, message = result.Message });
                    
                    TempData["Error"] = result.Message;
                    return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                var successMessage = operation == "create" ? "برنامه کاری پزشک با موفقیت ایجاد شد" : 
                                   operation == "update" ? "برنامه کاری پزشک با موفقیت به‌روزرسانی شد" :
                                   "برنامه کاری پزشک با موفقیت تنظیم شد";
                
                _logger.Information("تنظیم برنامه کاری پزشک {DoctorId} با موفقیت انجام شد - عملیات: {Operation}", model.DoctorId, operation);
                
                if (isAjax)
                    return Json(new { success = true, message = successMessage });
                
                TempData["Success"] = successMessage;
                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم برنامه کاری پزشک {DoctorId} - عملیات: {Operation}", model.DoctorId, operation);
                var errorMessage = "خطا در انجام عملیات تنظیم برنامه کاری";
                
                if (isAjax)
                    return Json(new { success = false, message = errorMessage });
                
                TempData["Error"] = errorMessage;
                return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// نمایش فرم مسدود کردن بازه زمانی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> BlockTimeRange(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم مسدود کردن بازه زمانی پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                var doctor = doctorResult.Data;

                var model = new BlockTimeRangeViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    StartTime = new TimeSpan(9, 0, 0), // 9:00 AM
                    EndTime = new TimeSpan(17, 0, 0),  // 5:00 PM
                    Reason = ""
                };

                ViewBag.Doctor = doctor;

                _logger.Information("فرم مسدود کردن بازه زمانی پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم مسدود کردن بازه زمانی پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم مسدود کردن بازه زمانی";
                return RedirectToAction("Index", "DoctorSchedule");
            }
        }

        /// <summary>
        /// پردازش مسدود کردن بازه زمانی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BlockTimeRange(BlockTimeRangeViewModel model)
        {
            try
            {
                _logger.Information("درخواست مسدود کردن بازه زمانی پزشک {DoctorId} از {StartDate} تا {EndDate}", 
                    model.DoctorId, model.StartDate, model.EndDate);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل مسدود کردن بازه زمانی نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                // ترکیب تاریخ و زمان
                var startDateTime = model.StartDate.Date.Add(model.StartTime);
                var endDateTime = model.EndDate.Date.Add(model.EndTime);

                // بررسی منطقی بودن بازه زمانی
                if (startDateTime >= endDateTime)
                {
                    TempData["Error"] = "زمان شروع باید قبل از زمان پایان باشد";
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                // مسدود کردن بازه زمانی
                var result = await _doctorScheduleService.BlockTimeRangeForDoctorAsync(
                    model.DoctorId, startDateTime, endDateTime, model.Reason);

                if (!result.Success)
                {
                    _logger.Warning("مسدود کردن بازه زمانی پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["Error"] = result.Message;
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                _logger.Information("مسدود کردن بازه زمانی پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["Success"] = "بازه زمانی با موفقیت مسدود شد";
                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مسدود کردن بازه زمانی پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات مسدود کردن بازه زمانی";
                return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Comprehensive Schedule Management

        /// <summary>
        /// مدیریت جامع برنامه کاری (ایجاد، ویرایش، حذف)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageSchedule(DoctorScheduleViewModel model, string action = "create")
        {
            try
            {
                _logger.Information("درخواست مدیریت برنامه کاری برای پزشک {DoctorId} - عملیات: {Action}", 
                    model.DoctorId, action);

                switch (action.ToLower())
                {
                    case "create":
                    case "update":
                        return await AssignSchedule(model, false, action);
                    
                    case "delete":
                        if (model.Id > 0)
                        {
                            var deleteResult = await _doctorScheduleService.DeleteDoctorScheduleAsync(model.Id);
                            if (deleteResult.Success)
                            {
                                TempData["Success"] = "برنامه کاری با موفقیت حذف شد";
                                return RedirectToAction("Index");
                            }
                            TempData["Error"] = deleteResult.Message;
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    case "activate":
                        if (model.Id > 0)
                        {
                            var activateResult = await _doctorScheduleService.ActivateDoctorScheduleAsync(model.Id);
                            if (activateResult.Success)
                            {
                                TempData["Success"] = "برنامه کاری با موفقیت فعال شد";
                                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                            }
                            TempData["Error"] = activateResult.Message;
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    case "deactivate":
                        if (model.Id > 0)
                        {
                            var deactivateResult = await _doctorScheduleService.DeactivateDoctorScheduleAsync(model.Id);
                            if (deactivateResult.Success)
                            {
                                TempData["Success"] = "برنامه کاری با موفقیت غیرفعال شد";
                                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                            }
                            TempData["Error"] = deactivateResult.Message;
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    default:
                        TempData["Error"] = "عملیات نامعتبر است";
                        return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                TempData["Error"] = "خطا در انجام عملیات";
                return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت برنامه کاری برای پزشک {DoctorId} - عملیات: {Action}", model.DoctorId, action);
                TempData["Error"] = "خطا در انجام عملیات";
                return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تبدیل نام روز هفته به شماره
        /// </summary>
        private int ConvertDayOfWeekToNumber(string dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case "شنبه": return 6;
                case "یکشنبه": return 0;
                case "دوشنبه": return 1;
                case "سه‌شنبه": return 2;
                case "چهارشنبه": return 3;
                case "پنج‌شنبه": return 4;
                case "جمعه": return 5;
                default: return 0;
            }
        }

        #endregion

        #region Schedule CRUD Operations

        /// <summary>
        /// ویرایش برنامه کاری
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> EditSchedule(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست ویرایش برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    TempData["Error"] = "شناسه برنامه کاری نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(scheduleId);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = "برنامه کاری مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش برنامه کاری {ScheduleId}", scheduleId);
                TempData["Error"] = "خطا در بارگذاری برنامه کاری";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف برنامه کاری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveSchedule(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست حذف برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return Json(new { success = false, message = "شناسه برنامه کاری نامعتبر است." });
                }

                // حذف برنامه کاری
                var result = await _doctorScheduleService.DeleteDoctorScheduleAsync(scheduleId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت حذف شد", scheduleId);

                return Json(new { success = true, message = "برنامه کاری با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف برنامه کاری {ScheduleId}", scheduleId);
                return Json(new { success = false, message = "خطا در حذف برنامه کاری" });
            }
        }

        /// <summary>
        /// غیرفعال کردن برنامه کاری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeactivateSchedule(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست غیرفعال کردن برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return Json(new { success = false, message = "شناسه برنامه کاری نامعتبر است." });
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleService.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return Json(new { success = false, message = "برنامه کاری مورد نظر یافت نشد." });
                }

                // غیرفعال کردن برنامه کاری
                var result = await _doctorScheduleService.DeactivateDoctorScheduleAsync(scheduleId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت غیرفعال شد", scheduleId);

                return Json(new { success = true, message = "برنامه کاری با موفقیت غیرفعال شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن برنامه کاری {ScheduleId}", scheduleId);
                return Json(new { success = false, message = "خطا در غیرفعال کردن برنامه کاری" });
            }
        }

        /// <summary>
        /// فعال کردن مجدد برنامه کاری
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateSchedule(int scheduleId)
        {
            try
            {
                _logger.Information("درخواست فعال کردن مجدد برنامه کاری {ScheduleId} توسط کاربر {UserId}", scheduleId, _currentUserService.UserId);

                if (scheduleId <= 0)
                {
                    return Json(new { success = false, message = "شناسه برنامه کاری نامعتبر است." });
                }

                // بررسی وجود برنامه کاری
                var schedule = await _doctorScheduleService.GetDoctorScheduleByIdAsync(scheduleId);
                if (schedule == null)
                {
                    return Json(new { success = false, message = "برنامه کاری مورد نظر یافت نشد." });
                }

                // فعال کردن مجدد برنامه کاری
                var result = await _doctorScheduleService.ActivateDoctorScheduleAsync(scheduleId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("برنامه کاری {ScheduleId} با موفقیت فعال شد", scheduleId);

                return Json(new { success = true, message = "برنامه کاری با موفقیت فعال شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن مجدد برنامه کاری {ScheduleId}", scheduleId);
                return Json(new { success = false, message = "خطا در فعال کردن مجدد برنامه کاری" });
            }
        }

        #endregion

        #region View and Edit Operations

        /// <summary>
        /// نمایش جزئیات برنامه کاری
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات برنامه کاری {ScheduleId} توسط کاربر {UserId}", id, _currentUserService.UserId);

                if (id <= 0)
                {
                    TempData["Error"] = "شناسه برنامه کاری نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = "برنامه کاری مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک و اضافه کردن به ViewModel
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(result.Data.DoctorId);
                if (doctorResult.Success && doctorResult.Data != null)
                {
                    result.Data.DoctorName = doctorResult.Data.FullName;
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات برنامه کاری {ScheduleId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات برنامه کاری";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش برنامه کاری (سازگار با View)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.Information("درخواست ویرایش برنامه کاری {ScheduleId} توسط کاربر {UserId}", id, _currentUserService.UserId);

                if (id <= 0)
                {
                    TempData["Error"] = "شناسه برنامه کاری نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = "برنامه کاری مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک و اضافه کردن به ViewModel
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(result.Data.DoctorId);
                if (doctorResult.Success && doctorResult.Data != null)
                {
                    result.Data.DoctorName = doctorResult.Data.FullName;
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش برنامه کاری {ScheduleId}", id);
                TempData["Error"] = "خطا در بارگذاری برنامه کاری";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Debug Actions (فقط برای تست)

        /// <summary>
        /// اکشن دیباگ برای بررسی داده‌های برنامه کاری پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DebugSchedule(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دیباگ برنامه کاری پزشک {DoctorId}", doctorId);

                // دریافت برنامه کاری
                var scheduleResult = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId);
                
                var debugInfo = new
                {
                    Success = scheduleResult.Success,
                    Message = scheduleResult.Message,
                    HasData = scheduleResult.Data != null,
                    DoctorId = scheduleResult.Data?.DoctorId,
                    DoctorName = scheduleResult.Data?.DoctorName,
                    WorkDaysCount = scheduleResult.Data?.WorkDays?.Count ?? 0,
                    ActiveWorkDaysCount = scheduleResult.Data?.WorkDays?.Count(w => w.IsActive) ?? 0,
                    TotalTimeRanges = scheduleResult.Data?.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0,
                    ActiveTimeRanges = scheduleResult.Data?.WorkDays?.Sum(w => w.TimeRanges?.Count(t => t.IsActive) ?? 0) ?? 0,
                    TotalSchedules = scheduleResult.Data?.TotalSchedules ?? 0,
                    ActiveSchedules = scheduleResult.Data?.ActiveSchedules ?? 0,
                    TotalTimeSlots = scheduleResult.Data?.TotalTimeSlots ?? 0,
                    WeeklyHours = scheduleResult.Data?.WeeklyHours ?? 0
                };

                return Json(debugInfo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دیباگ برنامه کاری پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
