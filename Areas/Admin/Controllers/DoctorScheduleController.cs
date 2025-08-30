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

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت برنامه‌های کاری پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت برنامه‌های کاری و زمان‌بندی پزشکان
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
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
                    return RedirectToAction("Index", "Doctor");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت برنامه کاری
                var scheduleResult = await _doctorScheduleService.GetDoctorScheduleAsync(doctorId);
                if (!scheduleResult.Success)
                {
                    TempData["Error"] = scheduleResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.Doctor = doctorResult.Data;
                return View(scheduleResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش برنامه کاری پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری برنامه کاری پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// به‌روزرسانی برنامه کاری پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateSchedule(DoctorScheduleViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی برنامه کاری پزشک {DoctorId} توسط کاربر {UserId}", model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _scheduleValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                }

                // به‌روزرسانی برنامه کاری
                var result = await _doctorScheduleService.SetDoctorScheduleAsync(model.DoctorId, model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
                }

                _logger.Information("برنامه کاری پزشک {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                TempData["Success"] = "برنامه کاری پزشک با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی برنامه کاری پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در به‌روزرسانی برنامه کاری پزشک";
                return RedirectToAction("Schedule", new { doctorId = model.DoctorId });
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

                // این متد می‌تواند لیست پزشکان با برنامه‌های کاری را نمایش دهد
                // فعلاً یک نمونه ساده
                ViewBag.ClinicId = clinicId;
                ViewBag.DepartmentId = departmentId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نمای کلی برنامه‌های کاری");
                TempData["Error"] = "خطا در بارگذاری نمای کلی برنامه‌های کاری";
                return View();
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

        #endregion
    }
}
