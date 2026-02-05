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
using System.Text.RegularExpressions;
using ClinicApp.Constants;
using ClinicApp.Extensions; // ✅ برای استفاده از DateTimeExtensions

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
        public async Task<ActionResult> Index(
            string searchTerm = "", 
            int page = DoctorScheduleConstants.Defaults.PageNumber, 
            int pageSize = DoctorScheduleConstants.Defaults.PageSize, 
            int? doctorId = null, 
            string dayOfWeek = null, 
            string isActive = null)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست برنامه‌های کاری پزشکان. Page: {Page}, PageSize: {PageSize}, DoctorId: {DoctorId}, DayOfWeek: {DayOfWeek}, IsActive: {IsActive}", 
                    page, pageSize, doctorId, dayOfWeek, isActive);

                // ✅ تبدیل string isActive به bool? با استفاده از Constants (Strongly-typed)
                // طبق: DEVELOPMENT_CONTRACT.md - Strongly-Typed Development
                bool? isActiveFilter = null;
                if (!string.IsNullOrWhiteSpace(isActive))
                {
                    if (isActive == DoctorScheduleConstants.FilterValues.Active)
                    {
                        isActiveFilter = true;
                    }
                    else if (isActive == DoctorScheduleConstants.FilterValues.Inactive)
                    {
                        isActiveFilter = false;
                    }
                }

                // دریافت لیست برنامه‌های کاری
                var result = await _doctorScheduleService.GetAllDoctorSchedulesAsync(searchTerm, page, pageSize, doctorId, dayOfWeek, isActiveFilter);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست برنامه‌های کاری: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message, "خطا در دریافت لیست");
                    return View(new PagedResult<DoctorScheduleViewModel>(new List<DoctorScheduleViewModel>(), 0, page, pageSize));
                }

                _logger.Information("لیست برنامه‌های کاری با موفقیت بازیابی شد. TotalItems: {TotalItems}", result.Data.TotalItems);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست برنامه‌های کاری پزشکان");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست برنامه‌های کاری", "خطا");
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
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است.", "خطا");
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    _logger.Warning("پزشک یافت نشد: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "پزشک مورد نظر یافت نشد.", "خطا");
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
                NotificationHelper.SetError(TempData, "خطا در بارگذاری برنامه کاری پزشک", "خطا");
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

        /// <summary>
        /// تولید دستی اسلات‌های زمانی برای یک پزشک
        /// 
        /// ✅ منطق: برنامه هفتگی است و منشی برای تاریخ‌های خاص برنامه تنظیم می‌کند
        /// - منشی می‌تواند برای هفته آینده یا تاریخ‌های خاص (مثلاً 25-26) برنامه تنظیم کند
        /// - اسلات‌ها فقط برای همان تاریخ خاص تولید می‌شوند (نه برای چند هفته آینده)
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="scheduleId">شناسه برنامه کاری</param>
        /// <param name="targetDate">تاریخ هدف برای تولید اسلات (اختیاری - فرمت: yyyy-MM-dd)</param>
        [HttpPost]
        public async Task<JsonResult> GenerateTimeSlots(int doctorId, int scheduleId, string targetDate = null)
        {
            try
            {
                DateTime? targetDateTime = null;
                if (!string.IsNullOrWhiteSpace(targetDate))
                {
                    if (DateTime.TryParse(targetDate, out var parsedDate))
                    {
                        targetDateTime = parsedDate.Date;
                    }
                    else
                    {
                        return Json(new { success = false, message = "فرمت تاریخ نامعتبر است. فرمت صحیح: yyyy-MM-dd" }, JsonRequestBehavior.AllowGet);
                    }
                }

                _logger.Information("درخواست تولید دستی اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}, TargetDate: {TargetDate}",
                    doctorId, scheduleId, targetDateTime?.ToString("yyyy/MM/dd") ?? "null (اولین روز کاری)");

                if (doctorId <= 0 || scheduleId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک یا برنامه کاری نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                // تولید و ذخیره اسلات‌های زمانی از طریق Service
                var result = await _doctorScheduleService.GenerateAndSaveTimeSlotsAsync(doctorId, scheduleId, targetDateTime);

                if (!result.Success)
                {
                    _logger.Warning("خطا در تولید اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}, Message: {Message}",
                        doctorId, scheduleId, result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _logger.Information("اسلات‌های زمانی با موفقیت تولید شدند - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}",
                    doctorId, scheduleId);

                return Json(new { success = true, message = "اسلات‌های زمانی با موفقیت تولید شدند." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید اسلات‌های زمانی - DoctorId: {DoctorId}, ScheduleId: {ScheduleId}",
                    doctorId, scheduleId);
                return Json(new { success = false, message = $"خطا در تولید اسلات‌های زمانی: {ex.Message}" }, JsonRequestBehavior.AllowGet);
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
                _logger.Error("خطا در بارگذاری نمای کلی برنامه‌های کاری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری نمای کلی برنامه‌های کاری", "خطا");
                return View(new ScheduleOverviewViewModel());
            }
        }

        #endregion

        #region AJAX Operations

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

        /// <summary>
        /// بازگرداندن نمایش مبلغ حق ویزیت برای راهنمای منشی (تومان + مبلغ به حروف).
        /// منطق نمایش در MoneyDisplayHelper (SRP).
        /// </summary>
        [HttpGet]
        public JsonResult GetConsultationFeeDisplay(decimal? amountRial)
        {
            try
            {
                var rial = amountRial ?? 0;
                if (rial < 0) rial = 0;
                var tomanDisplay = MoneyDisplayHelper.FormatTomanDisplay(rial);
                var amountInWords = MoneyDisplayHelper.AmountToWordsToman(rial);
                return Json(new { success = true, tomanDisplay, amountInWords }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در نمایش مبلغ حق ویزیت");
                return Json(new { success = false, tomanDisplay = "", amountInWords = "" }, JsonRequestBehavior.AllowGet);
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
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است", "خطا");
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    _logger.Warning("خطا در دریافت اطلاعات پزشک: {ErrorMessage}", doctorResult.Message);
                    NotificationHelper.SetError(TempData, doctorResult.Message, "خطا");
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                var doctor = doctorResult.Data;

                // ✅ دریافت برنامه کاری موجود با Error Handling کامل
                DoctorScheduleViewModel model;
                try
                {
                    _logger.Information("🔍 [AssignSchedule GET] شروع دریافت برنامه کاری پزشک {DoctorId}", doctorId.Value);
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] 🔍 شروع دریافت برنامه کاری پزشک {doctorId.Value}");
                    
                    var scheduleResult = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId.Value);
                    
                    _logger.Information("🔍 [AssignSchedule GET] نتیجه GetDoctorScheduleAsync: Success={Success}, DataIsNull={DataIsNull}, Message={Message}", 
                        scheduleResult.Success, scheduleResult.Data == null, scheduleResult.Message ?? "null");
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] نتیجه GetDoctorScheduleAsync: Success={scheduleResult.Success}, DataIsNull={scheduleResult.Data == null}, Message={scheduleResult.Message ?? "null"}");
                    
                    // ✅ بررسی خطا در Service
                    if (!scheduleResult.Success)
                    {
                        _logger.Warning("❌ [AssignSchedule GET] خطا در دریافت برنامه کاری پزشک {DoctorId}: {Message}", doctorId.Value, scheduleResult.Message);
                        System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] ❌ خطا در دریافت: {scheduleResult.Message}");
                        
                        // ✅ نمایش پیغام خطا به کاربر
                        _logger.Warning("خطا در دریافت برنامه کاری: {ErrorMessage}", scheduleResult.Message);
                        NotificationHelper.SetError(TempData, scheduleResult.Message ?? "خطا در دریافت برنامه کاری پزشک. لطفاً دوباره تلاش کنید.", "خطا");
                        
                        // ✅ ایجاد مدل جدید برای ادامه کار
                        model = new DoctorScheduleViewModel
                        {
                            DoctorId = doctorId.Value,
                            AppointmentDuration = 30,
                            WorkDays = new List<WorkDayViewModel>()
                        };
                    }
                    else if (scheduleResult.Success && scheduleResult.Data != null)
                    {
                        model = scheduleResult.Data;
                        _logger.Information("✅ [AssignSchedule GET] برنامه کاری پزشک {DoctorId} با موفقیت دریافت شد. WorkDays: {WorkDaysCount}", 
                            doctorId.Value, model.WorkDays?.Count ?? 0);
                        System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] ✅ برنامه کاری پزشک {doctorId.Value} با موفقیت دریافت شد. WorkDays: {model.WorkDays?.Count ?? 0}");
                    }
                    else
                    {
                        // ✅ اگر برنامه کاری وجود ندارد یا null است، یک مدل جدید ایجاد می‌کنیم
                        _logger.Information("ℹ️ [AssignSchedule GET] برنامه کاری برای پزشک {DoctorId} یافت نشد یا null است. ایجاد مدل جدید", doctorId.Value);
                        System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] ℹ️ برنامه کاری برای پزشک {doctorId.Value} یافت نشد یا null است. ایجاد مدل جدید");
                        model = new DoctorScheduleViewModel
                        {
                            DoctorId = doctorId.Value,
                            AppointmentDuration = 30,
                            WorkDays = new List<WorkDayViewModel>()
                        };
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // ✅ مدیریت خطاهای عملیاتی (مثل خطاهای Repository)
                    _logger.Warning(ex, "خطای عملیاتی در دریافت برنامه کاری پزشک {DoctorId}: {Message}", doctorId.Value, ex.Message);
                    
                    // ✅ نمایش پیغام خطا به کاربر
                    _logger.Error("خطا در دریافت برنامه کاری پزشک");
                    NotificationHelper.SetError(TempData, "خطا در دریافت برنامه کاری پزشک. لطفاً دوباره تلاش کنید.", "خطا");
                    
                    model = new DoctorScheduleViewModel
                    {
                        DoctorId = doctorId.Value,
                        AppointmentDuration = 30,
                        WorkDays = new List<WorkDayViewModel>()
                    };
                }
                catch (Exception ex)
                {
                    // ✅ در صورت خطا، یک مدل جدید ایجاد می‌کنیم و خطا را با جزئیات کامل لاگ می‌کنیم
                    _logger.Error(ex, "خطا در دریافت برنامه کاری پزشک {DoctorId}. ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                        doctorId.Value, ex.GetType().Name, ex.Message, ex.StackTrace);
                    
                    if (ex.InnerException != null)
                    {
                        _logger.Error(ex.InnerException, "InnerException برای DoctorId {DoctorId}: {Message}, Type: {Type}", 
                            doctorId.Value, ex.InnerException.Message, ex.InnerException.GetType().Name);
                    }
                    
                    // ✅ نمایش پیغام خطا به کاربر (بدون نمایش جزئیات فنی)
                    _logger.Error("خطا در دریافت برنامه کاری پزشک");
                    NotificationHelper.SetError(TempData, "خطا در دریافت برنامه کاری پزشک. لطفاً دوباره تلاش کنید.", "خطا");
                    
                    model = new DoctorScheduleViewModel
                    {
                        DoctorId = doctorId.Value,
                        AppointmentDuration = 30,
                        WorkDays = new List<WorkDayViewModel>()
                    };
                }

                // اطمینان از اینکه WorkDays null نباشد
                if (model.WorkDays == null)
                {
                    model.WorkDays = new List<WorkDayViewModel>();
                }

                // ✅ همیشه تمام 7 روز هفته را اضافه می‌کنیم (نه فقط وقتی خالی است)
                // ✅ این کار باعث می‌شود که کاربر بتواند روزهای جدید را اضافه کند
                // ✅ DayOfWeek: 0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه (مطابق با Entity)
                var daysOfWeek = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
                
                // ✅ ایجاد Dictionary از WorkDays موجود بر اساس DayOfWeek برای جستجوی سریع
                var existingWorkDaysDict = model.WorkDays
                    .Where(wd => wd.DayOfWeek >= 0 && wd.DayOfWeek < 7)
                    .ToDictionary(wd => wd.DayOfWeek, wd => wd);
                
                // ✅ ایجاد لیست جدید با تمام 7 روز هفته
                var allWorkDays = new List<WorkDayViewModel>();
                
                for (int i = 0; i < 7; i++)
                {
                    if (existingWorkDaysDict.TryGetValue(i, out var existingWorkDay))
                    {
                        // ✅ اگر WorkDay موجود است، از آن استفاده می‌کنیم
                        allWorkDays.Add(existingWorkDay);
                    }
                    else
                    {
                        // ✅ اگر WorkDay موجود نیست، یک WorkDay جدید با IsActive = false ایجاد می‌کنیم
                        allWorkDays.Add(new WorkDayViewModel
                        {
                            DayOfWeek = i,
                            DayName = daysOfWeek[i],
                            IsActive = false,
                            TimeRanges = new List<TimeRangeViewModel>()
                        });
                    }
                }
                
                // ✅ جایگزین کردن WorkDays با لیست کامل
                model.WorkDays = allWorkDays;
                
                _logger.Information("✅ [AssignSchedule GET] تمام 7 روز هفته اضافه شد. WorkDaysCount: {WorkDaysCount}", model.WorkDays.Count);
                System.Diagnostics.Debug.WriteLine($"[AssignSchedule GET] ✅ تمام 7 روز هفته اضافه شد. WorkDaysCount: {model.WorkDays.Count}");

                ViewBag.Doctor = doctor;

                // ✅ اضافه کردن اطلاعات امروز برای راهنمایی منشی
                var today = DateTime.Today;
                var todayPersianDate = PersianDateHelper.ToPersianDate(today);
                var todayDayOfWeekName = PersianDateHelper.GetPersianDayOfWeekName(today.DayOfWeek);
                ViewBag.TodayPersianDate = todayPersianDate;
                ViewBag.TodayDayOfWeekName = todayDayOfWeekName;
                ViewBag.Today = today;

                // ✅ محاسبه تاریخ هر روز هفته در هفته جاری (برای نمایش کنار نام روز)
                // ✅ هفته شمسی از شنبه شروع می‌شود (مطابق time.ir)
                var weekStart = today.StartOfWeek(DayOfWeek.Saturday);
                var weekDates = new Dictionary<int, string>(); // DayOfWeek -> PersianDate
                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var persianDate = PersianDateHelper.ToPersianDate(date);
                    var dayOfWeek = (int)date.DayOfWeek;
                    weekDates[dayOfWeek] = persianDate;
                }
                ViewBag.WeekDates = weekDates;

                _logger.Information("فرم تنظیم برنامه کاری پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم تنظیم برنامه کاری پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                _logger.Error("خطا در بارگذاری فرم تنظیم برنامه کاری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم تنظیم برنامه کاری", "خطا");
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
                _logger.Information("🔍 [AssignSchedule POST] شروع درخواست تنظیم برنامه کاری پزشک {DoctorId} - عملیات: {Operation} - AJAX: {IsAjax}", 
                    model.DoctorId, operation, isAjax);
                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] 🔍 شروع درخواست تنظیم برنامه کاری پزشک {model.DoctorId}");
                
                // ✅ لاگ جزئیات Model قبل از فیلتر - با لاگ Request.Form برای دیباگ
                if (Request.Form != null)
                {
                    _logger.Information("🔍 [AssignSchedule POST] Request.Form Keys: {FormKeys}", string.Join(", ", Request.Form.AllKeys.Where(k => k != null && (k.Contains("TimeRanges") || k.Contains("Default")))));
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] 🔍 Request.Form Keys: {string.Join(", ", Request.Form.AllKeys.Where(k => k != null && (k.Contains("TimeRanges") || k.Contains("Default"))))}");
                    
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key != null && (key.Contains("TimeRanges") || key.Contains("Default")))
                        {
                            _logger.Information("🔍 [AssignSchedule POST] Form[{Key}] = {Value}", key, Request.Form[key]);
                            System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] 🔍 Form[{key}] = {Request.Form[key]}");
                        }
                    }
                }
                
                // ✅ محاسبه DayName از DayOfWeek برای تمام WorkDays (قبل از validation)
                // این کار برای اطمینان از اینکه DayName همیشه از DayOfWeek محاسبه می‌شود
                // ✅ DayOfWeek: 0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه (مطابق با Entity)
                var dayNames = new[] { "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه" };
                if (model.WorkDays != null)
                {
                    foreach (var workDay in model.WorkDays)
                    {
                        // ✅ محاسبه DayName از DayOfWeek اگر خالی باشد یا با DayOfWeek همخوانی نداشته باشد
                        if (workDay.DayOfWeek >= 0 && workDay.DayOfWeek < dayNames.Length)
                        {
                            var calculatedDayName = dayNames[workDay.DayOfWeek];
                            if (string.IsNullOrEmpty(workDay.DayName) || workDay.DayName != calculatedDayName)
                            {
                                workDay.DayName = calculatedDayName;
                                _logger.Information("✅ [AssignSchedule POST] DayName محاسبه شد: DayOfWeek={DayOfWeek}, DayName={DayName}", 
                                    workDay.DayOfWeek, workDay.DayName);
                                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] ✅ DayName محاسبه شد: DayOfWeek={workDay.DayOfWeek}, DayName={workDay.DayName}");
                            }
                        }
                    }
                }
                
                // ✅ لاگ جزئیات Model قبل از فیلتر
                if (model.WorkDays != null)
                {
                    _logger.Information("🔍 [AssignSchedule POST] قبل از فیلتر - WorkDaysCount: {WorkDaysCount}", model.WorkDays.Count);
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] قبل از فیلتر - WorkDaysCount: {model.WorkDays.Count}");
                    
                    foreach (var workDay in model.WorkDays)
                    {
                        _logger.Information("🔍 [AssignSchedule POST] WorkDay {DayOfWeek} (DayName: {DayName}, IsActive: {IsActive}) - TimeRangesCount: {TimeRangesCount}", 
                            workDay.DayOfWeek, workDay.DayName, workDay.IsActive, workDay.TimeRanges?.Count ?? 0);
                        System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] WorkDay {workDay.DayOfWeek} (DayName: {workDay.DayName}, IsActive: {workDay.IsActive}) - TimeRangesCount: {workDay.TimeRanges?.Count ?? 0}");
                        
                        if (workDay.TimeRanges != null)
                        {
                            foreach (var timeRange in workDay.TimeRanges)
                            {
                                _logger.Information("🔍 [AssignSchedule POST] TimeRange: StartTime={StartTime}, EndTime={EndTime}, StartTimeString={StartTimeString}, EndTimeString={EndTimeString}", 
                                    timeRange.StartTime, timeRange.EndTime, timeRange.StartTimeString, timeRange.EndTimeString);
                                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] TimeRange: StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}, StartTimeString={timeRange.StartTimeString}, EndTimeString={timeRange.EndTimeString}");
                            }
                        }
                    }
                }

                // ✅ اعتبارسنجی با FluentValidation (قبل از فیلتر) برای نمایش پیغام خطای دقیق
                // این کار باعث می‌شود که اگر EndTime < StartTime باشد، پیغام خطای مناسب نمایش داده شود
                _logger.Information("🔍 [AssignSchedule POST] شروع Validation (قبل از فیلتر)");
                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] 🔍 شروع Validation (قبل از فیلتر)");
                
                var validationResultBeforeFilter = await _scheduleValidator.ValidateAsync(model);
                
                if (!validationResultBeforeFilter.IsValid)
                {
                    // ✅ بررسی خطاهای مربوط به ترتیب زمان (EndTime < StartTime)
                    var timeOrderErrors = validationResultBeforeFilter.Errors
                        .Where(e => e.ErrorCode == "INVALID_TIME_ORDER")
                        .ToList();
                    
                    if (timeOrderErrors.Any())
                    {
                        // ✅ بهبود پیغام خطا با جزئیات بیشتر (شامل نام روز هفته و زمان‌های واقعی)
                        var errorMessages = timeOrderErrors.Select(e => 
                        {
                            // استخراج نام روز و TimeRange از PropertyName (مثلاً WorkDays[0].TimeRanges[0].EndTime)
                            var propertyName = e.PropertyName ?? "";
                            var dayIndexMatch = Regex.Match(propertyName, @"WorkDays\[(\d+)\]");
                            var timeRangeIndexMatch = Regex.Match(propertyName, @"TimeRanges\[(\d+)\]");
                            
                            if (dayIndexMatch.Success && int.TryParse(dayIndexMatch.Groups[1].Value, out int dayIndex))
                            {
                                var workDay = model.WorkDays?.ElementAtOrDefault(dayIndex);
                                if (workDay != null && !string.IsNullOrEmpty(workDay.DayName))
                                {
                                    // ✅ استخراج TimeRange برای نمایش زمان‌های واقعی
                                    if (timeRangeIndexMatch.Success && int.TryParse(timeRangeIndexMatch.Groups[1].Value, out int timeRangeIndex))
                                    {
                                        var timeRange = workDay.TimeRanges?.ElementAtOrDefault(timeRangeIndex);
                                        if (timeRange != null && timeRange.StartTime != TimeSpan.Zero && timeRange.EndTime != TimeSpan.Zero)
                                        {
                                            var startTimeStr = $"{timeRange.StartTime.Hours:D2}:{timeRange.StartTime.Minutes:D2}";
                                            var endTimeStr = $"{timeRange.EndTime.Hours:D2}:{timeRange.EndTime.Minutes:D2}";
                                            return $"{workDay.DayName}: ❌ زمان پایان ({endTimeStr}) باید بعد از زمان شروع ({startTimeStr}) باشد.";
                                        }
                                    }
                                    
                                    return $"{workDay.DayName}: {e.ErrorMessage}";
                                }
                            }
                            
                            return e.ErrorMessage;
                        });
                        
                        var errorMessage = string.Join(" ", errorMessages);
                        _logger.Warning("❌ [AssignSchedule POST] خطای ترتیب زمان: {Errors}", errorMessage);
                        
                        if (isAjax)
                            return Json(new { success = false, message = errorMessage });
                        
                        _logger.Warning("خطا در اعتبارسنجی: {ErrorMessage}", errorMessage);
                        NotificationHelper.SetError(TempData, $"خطا در اعتبارسنجی: {errorMessage}", "خطای اعتبارسنجی");
                        return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                    }
                }
                
                // ✅ فیلتر کردن TimeRange های خالی بعد از Validation اولیه
                // این کار برای حذف TimeRange های نامعتبر انجام می‌شود
                if (model.WorkDays != null)
                {
                    foreach (var workDay in model.WorkDays)
                    {
                        if (workDay.TimeRanges != null)
                        {
                            var beforeCount = workDay.TimeRanges.Count;
                            
                            // ✅ حذف TimeRange های خالی (با StartTime یا EndTime = TimeSpan.Zero)
                            // ✅ همچنین حذف TimeRange های نامعتبر (EndTime <= StartTime)
                            workDay.TimeRanges = workDay.TimeRanges
                                .Where(tr => tr != null && 
                                            tr.StartTime != TimeSpan.Zero && 
                                            tr.EndTime != TimeSpan.Zero &&
                                            tr.StartTime < tr.EndTime) // ✅ فقط TimeRange های معتبر
                                .ToList();
                            
                            var afterCount = workDay.TimeRanges.Count;
                            
                            if (beforeCount != afterCount)
                            {
                                _logger.Information("🔍 [AssignSchedule POST] WorkDay {DayOfWeek}: TimeRanges فیلتر شد. قبل: {BeforeCount}, بعد: {AfterCount}", 
                                    workDay.DayOfWeek, beforeCount, afterCount);
                                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] WorkDay {workDay.DayOfWeek}: TimeRanges فیلتر شد. قبل: {beforeCount}, بعد: {afterCount}");
                            }
                            
                            // ✅ لاگ TimeRange های باقی‌مانده
                            foreach (var timeRange in workDay.TimeRanges)
                            {
                                _logger.Information("✅ [AssignSchedule POST] TimeRange معتبر: StartTime={StartTime}, EndTime={EndTime}, Duration={Duration} دقیقه", 
                                    timeRange.StartTime, timeRange.EndTime, (timeRange.EndTime - timeRange.StartTime).TotalMinutes);
                                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] ✅ TimeRange معتبر: StartTime={timeRange.StartTime}, EndTime={timeRange.EndTime}, Duration={(timeRange.EndTime - timeRange.StartTime).TotalMinutes} دقیقه");
                            }
                        }
                    }
                }
                
                // ✅ پردازش تاریخ‌های خاص از Request.Form
                if (model.SpecificDates == null)
                {
                    model.SpecificDates = new List<SpecificDateViewModel>();
                }
                
                // ✅ استخراج تاریخ‌های خاص از Request.Form
                var specificDateKeys = Request.Form?.AllKeys?.Where(k => k != null && k.StartsWith("SpecificDates[") && k.Contains("].PersianDate"))?.ToList() ?? new List<string>();
                foreach (var key in specificDateKeys)
                {
                    // ✅ استخراج index از key (مثلاً: "SpecificDates[0].PersianDate" -> 0)
                    var match = System.Text.RegularExpressions.Regex.Match(key, @"SpecificDates\[(\d+)\]\.PersianDate");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
                    {
                        var persianDate = Request.Form[key]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(persianDate))
                        {
                            // ✅ خواندن StartTime, EndTime, AppointmentDuration
                            var startTimeKey = $"SpecificDates[{index}].StartTime";
                            var endTimeKey = $"SpecificDates[{index}].EndTime";
                            var durationKey = $"SpecificDates[{index}].AppointmentDuration";
                            
                            var startTimeStr = Request.Form[startTimeKey]?.ToString()?.Trim();
                            var endTimeStr = Request.Form[endTimeKey]?.ToString()?.Trim();
                            var durationStr = Request.Form[durationKey]?.ToString()?.Trim();
                            
                            if (TimeSpan.TryParse(startTimeStr, out TimeSpan startTime) && 
                                TimeSpan.TryParse(endTimeStr, out TimeSpan endTime) &&
                                int.TryParse(durationStr, out int duration))
                            {
                                model.SpecificDates.Add(new SpecificDateViewModel
                                {
                                    PersianDate = persianDate,
                                    StartTime = startTime,
                                    EndTime = endTime,
                                    AppointmentDuration = duration > 0 ? duration : 30
                                });
                                
                                _logger.Information("✅ [AssignSchedule POST] تاریخ خاص اضافه شد: {PersianDate}, StartTime={StartTime}, EndTime={EndTime}, Duration={Duration}", 
                                    persianDate, startTime, endTime, duration);
                            }
                        }
                    }
                }
                
                _logger.Information("🔍 [AssignSchedule POST] بعد از فیلتر TimeRange های خالی. WorkDaysCount: {WorkDaysCount}, TotalTimeRangesCount: {TotalTimeRangesCount}, SpecificDatesCount: {SpecificDatesCount}", 
                    model.WorkDays?.Count ?? 0,
                    model.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0,
                    model.SpecificDates?.Count ?? 0);
                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] بعد از فیلتر TimeRange های خالی. WorkDaysCount: {model.WorkDays?.Count ?? 0}, TotalTimeRangesCount: {model.WorkDays?.Sum(w => w.TimeRanges?.Count ?? 0) ?? 0}, SpecificDatesCount: {model.SpecificDates?.Count ?? 0}");

                // ✅ بررسی ModelState بعد از فیلتر
                if (!ModelState.IsValid)
                {
                    var errorMessage = "اطلاعات وارد شده نامعتبر است";
                    _logger.Warning("مدل برنامه کاری نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    
                    if (isAjax)
                        return Json(new { success = false, message = errorMessage });
                    
                    NotificationHelper.SetError(TempData, errorMessage, "خطا");
                    return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }
                
                // اعتبارسنجی با FluentValidation (بعد از فیلتر TimeRange های خالی)
                _logger.Information("🔍 [AssignSchedule POST] شروع Validation");
                System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] 🔍 شروع Validation");
                
                var validationResult = await _scheduleValidator.ValidateAsync(model);
                
                if (!validationResult.IsValid)
                {
                    _logger.Warning("❌ [AssignSchedule POST] Validation ناموفق بود. تعداد خطاها: {ErrorCount}", validationResult.Errors.Count);
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] ❌ Validation ناموفق بود. تعداد خطاها: {validationResult.Errors.Count}");
                    
                    // ✅ لاگ تمام خطاهای Validation
                    foreach (var error in validationResult.Errors)
                    {
                        _logger.Warning("❌ [AssignSchedule POST] Validation Error: PropertyName={PropertyName}, ErrorMessage={ErrorMessage}, ErrorCode={ErrorCode}", 
                            error.PropertyName, error.ErrorMessage, error.ErrorCode);
                        System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] ❌ Validation Error: PropertyName={error.PropertyName}, ErrorMessage={error.ErrorMessage}, ErrorCode={error.ErrorCode}");
                    }
                    
                    // ✅ فیلتر کردن خطاهای مربوط به TimeRange های خالی
                    var relevantErrors = validationResult.Errors
                        .Where(e => !e.ErrorMessage.Contains("زمان شروع الزامی است") && 
                                    !e.ErrorMessage.Contains("زمان پایان الزامی است"))
                        .ToList();
                    
                    if (relevantErrors.Any())
                    {
                        var errors = string.Join(", ", relevantErrors.Select(e => e.ErrorMessage));
                        var errorMessage = $"خطا در اعتبارسنجی: {errors}";
                        _logger.Warning("❌ [AssignSchedule POST] اعتبارسنجی برنامه کاری پزشک {DoctorId} ناموفق بود: {Errors}", model.DoctorId, errors);
                        
                        if (isAjax)
                            return Json(new { success = false, message = errorMessage });
                        
                        NotificationHelper.SetError(TempData, errorMessage, "خطا");
                        return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                    }
                }
                else
                {
                    _logger.Information("✅ [AssignSchedule POST] Validation موفق بود");
                    System.Diagnostics.Debug.WriteLine($"[AssignSchedule POST] ✅ Validation موفق بود");
                }

                // ✅ تنظیم برنامه کاری پزشک با Error Handling کامل
                ServiceResult result;
                try
                {
                    result = await _doctorScheduleService.SetDoctorScheduleAsync(model.DoctorId, model);
                }
                catch (InvalidOperationException ex)
                {
                    // ✅ مدیریت خطاهای عملیاتی (مثل تداخل بازه‌های زمانی)
                    _logger.Warning(ex, "خطای عملیاتی در تنظیم برنامه کاری پزشک {DoctorId}: {Message}", model.DoctorId, ex.Message);
                    result = ServiceResult.Failed(ex.Message);
                }
                catch (Exception ex)
                {
                    // ✅ مدیریت خطاهای غیرمنتظره
                    _logger.Error(ex, "خطای غیرمنتظره در تنظیم برنامه کاری پزشک {DoctorId}", model.DoctorId);
                    result = ServiceResult.Failed("خطا در انجام عملیات تنظیم برنامه کاری. لطفاً دوباره تلاش کنید.");
                }

                if (!result.Success)
                {
                    _logger.Warning("تنظیم برنامه کاری پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    
                    if (isAjax)
                        return Json(new { success = false, message = result.Message });
                    
                    _logger.Error("خطا در ذخیره برنامه کاری: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                    return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                var successMessage = operation == "create" ? "برنامه کاری پزشک با موفقیت ایجاد شد" : 
                                   operation == "update" ? "برنامه کاری پزشک با موفقیت به‌روزرسانی شد" :
                                   "برنامه کاری پزشک با موفقیت تنظیم شد";
                
                _logger.Information("تنظیم برنامه کاری پزشک {DoctorId} با موفقیت انجام شد - عملیات: {Operation}", model.DoctorId, operation);
                
                if (isAjax)
                    return Json(new { success = true, message = successMessage });
                
                NotificationHelper.SetSuccess(TempData, successMessage, "موفقیت");
                // ✅ Redirect به Index به جای Schedule برای جلوگیری از خطای GetDoctorScheduleAsync
                return RedirectToAction("Index", "DoctorSchedule");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم برنامه کاری پزشک {DoctorId} - عملیات: {Operation}", model.DoctorId, operation);
                var errorMessage = "خطا در انجام عملیات تنظیم برنامه کاری";
                
                if (isAjax)
                    return Json(new { success = false, message = errorMessage });
                
                NotificationHelper.SetError(TempData, errorMessage, "خطا");
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
                    _logger.Warning("شناسه پزشک نامعتبر: {DoctorId}", doctorId);
                    NotificationHelper.SetError(TempData, "شناسه پزشک نامعتبر است", "خطا");
                    return RedirectToAction("Index", "DoctorSchedule");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    _logger.Warning("خطا در دریافت اطلاعات پزشک: {ErrorMessage}", doctorResult.Message);
                    NotificationHelper.SetError(TempData, doctorResult.Message, "خطا");
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
                _logger.Error(ex, "خطا در بارگذاری فرم مسدود کردن بازه زمانی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم مسدود کردن بازه زمانی", "خطا");
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
                    _logger.Warning("اطلاعات وارد شده نامعتبر");
                    NotificationHelper.SetError(TempData, "اطلاعات وارد شده نامعتبر است", "خطای اعتبارسنجی");
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                // ترکیب تاریخ و زمان
                var startDateTime = model.StartDate.Date.Add(model.StartTime);
                var endDateTime = model.EndDate.Date.Add(model.EndTime);

                // بررسی منطقی بودن بازه زمانی
                if (startDateTime >= endDateTime)
                {
                    _logger.Warning("زمان شروع باید قبل از زمان پایان باشد");
                    NotificationHelper.SetError(TempData, "زمان شروع باید قبل از زمان پایان باشد", "خطای اعتبارسنجی");
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                // مسدود کردن بازه زمانی
                var result = await _doctorScheduleService.BlockTimeRangeForDoctorAsync(
                    model.DoctorId, startDateTime, endDateTime, model.Reason);

                if (!result.Success)
                {
                    _logger.Warning("مسدود کردن بازه زمانی پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    _logger.Error("خطا در ذخیره برنامه کاری: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                    return RedirectToAction("BlockTimeRange", new { doctorId = model.DoctorId });
                }

                _logger.Information("مسدود کردن بازه زمانی پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                NotificationHelper.SetSuccess(TempData, "بازه زمانی با موفقیت مسدود شد", "موفقیت");
                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مسدود کردن بازه زمانی پزشک {DoctorId}", model.DoctorId);
                _logger.Error(ex, "خطا در انجام عملیات مسدود کردن بازه زمانی");
                NotificationHelper.SetError(TempData, "خطا در انجام عملیات مسدود کردن بازه زمانی", "خطا");
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
                                NotificationHelper.SetSuccess(TempData, "برنامه کاری با موفقیت حذف شد", "موفقیت");
                                return RedirectToAction("Index");
                            }
                            _logger.Error("خطا در حذف برنامه کاری: {ErrorMessage}", deleteResult.Message);
                            NotificationHelper.SetError(TempData, deleteResult.Message, "خطا");
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    case "activate":
                        if (model.Id > 0)
                        {
                            var activateResult = await _doctorScheduleService.ActivateDoctorScheduleAsync(model.Id);
                            if (activateResult.Success)
                            {
                                NotificationHelper.SetSuccess(TempData, "برنامه کاری با موفقیت فعال شد", "موفقیت");
                                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                            }
                            _logger.Error("خطا در فعال کردن برنامه کاری: {ErrorMessage}", activateResult.Message);
                            NotificationHelper.SetError(TempData, activateResult.Message, "خطا");
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    case "deactivate":
                        if (model.Id > 0)
                        {
                            var deactivateResult = await _doctorScheduleService.DeactivateDoctorScheduleAsync(model.Id);
                            if (deactivateResult.Success)
                            {
                                NotificationHelper.SetSuccess(TempData, "برنامه کاری با موفقیت غیرفعال شد", "موفقیت");
                                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                            }
                            _logger.Error("خطا در غیرفعال کردن برنامه کاری: {ErrorMessage}", deactivateResult.Message);
                            NotificationHelper.SetError(TempData, deactivateResult.Message, "خطا");
                            return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                        }
                        break;
                    
                    default:
                        _logger.Warning("عملیات نامعتبر: {Action}", action);
                        NotificationHelper.SetError(TempData, "عملیات نامعتبر است", "خطا");
                        return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
                }

                _logger.Error("خطا در انجام عملیات");
                NotificationHelper.SetError(TempData, "خطا در انجام عملیات", "خطا");
                return RedirectToAction("AssignSchedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در مدیریت برنامه کاری برای پزشک {DoctorId} - عملیات: {Action}", model.DoctorId, action);
                _logger.Error("خطا در انجام عملیات");
                NotificationHelper.SetError(TempData, "خطا در انجام عملیات", "خطا");
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
                    _logger.Warning("شناسه برنامه کاری نامعتبر: {ScheduleId}", scheduleId);
                    NotificationHelper.SetError(TempData, "شناسه برنامه کاری نامعتبر است.", "خطا");
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(scheduleId);
                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("برنامه کاری یافت نشد: {ScheduleId}", scheduleId);
                    NotificationHelper.SetError(TempData, "برنامه کاری مورد نظر یافت نشد.", "خطا");
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش برنامه کاری {ScheduleId}", scheduleId);
                _logger.Error(ex, "خطا در بارگذاری برنامه کاری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری برنامه کاری", "خطا");
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
                if (!schedule.Success || schedule.Data == null)
                {
                    return Json(new { success = false, message = schedule.Message ?? "برنامه کاری مورد نظر یافت نشد." });
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
                if (!schedule.Success || schedule.Data == null)
                {
                    return Json(new { success = false, message = schedule.Message ?? "برنامه کاری مورد نظر یافت نشد." });
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
                    _logger.Warning("شناسه برنامه کاری نامعتبر: {ScheduleId}", id);
                    NotificationHelper.SetError(TempData, "شناسه برنامه کاری نامعتبر است.", "خطا");
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("برنامه کاری یافت نشد: {ScheduleId}", id);
                    NotificationHelper.SetError(TempData, "برنامه کاری مورد نظر یافت نشد.", "خطا");
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
                _logger.Error(ex, "خطا در بارگذاری جزئیات برنامه کاری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات برنامه کاری", "خطا");
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
                    _logger.Warning("شناسه برنامه کاری نامعتبر: {ScheduleId}", id);
                    NotificationHelper.SetError(TempData, "شناسه برنامه کاری نامعتبر است.", "خطا");
                    return RedirectToAction("Index");
                }

                // دریافت برنامه کاری
                var result = await _doctorScheduleService.GetDoctorScheduleByIdAsync(id);
                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("برنامه کاری یافت نشد: {ScheduleId}", id);
                    NotificationHelper.SetError(TempData, "برنامه کاری مورد نظر یافت نشد.", "خطا");
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
                _logger.Error(ex, "خطا در بارگذاری برنامه کاری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری برنامه کاری", "خطا");
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
                    WeeklyHours = scheduleResult.Data?.WeeklyHours ?? 0,
                    // جزئیات کامل WorkDays و TimeRanges
                    WorkDaysDetails = scheduleResult.Data?.WorkDays?.Select(w => new
                    {
                        Id = w.Id,
                        DayOfWeek = w.DayOfWeek,
                        DayOfWeekForCalendar = w.DayOfWeekForCalendar,
                        DayName = w.DayName,
                        IsActive = w.IsActive,
                        TimeRangesCount = w.TimeRanges?.Count ?? 0,
                        TimeRanges = w.TimeRanges?.Select(t => new
                        {
                            Id = t.Id,
                            StartTime = t.StartTime.ToString(@"hh\:mm"),
                            EndTime = t.EndTime.ToString(@"hh\:mm"),
                            StartTimeString = t.StartTimeString,
                            EndTimeString = t.EndTimeString,
                            IsActive = t.IsActive
                        }).ToList()
                    }).ToList()
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
