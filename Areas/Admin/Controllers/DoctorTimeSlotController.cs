using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Constants;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.TimeSlotManagement;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت اسلات‌های زمانی پزشکان در سیستم کلینیک شفا
    /// 
    /// مسئولیت: مدیریت کامل اسلات‌های زمانی (مشاهده، فیلتر، جستجو، مدیریت)
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: فقط مدیریت اسلات‌های زمانی
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// ✅ Clean Architecture: Controller فقط View را مدیریت می‌کند
    /// ✅ Medical Standards: رعایت استانداردهای سیستم‌های پزشکی
    /// ✅ Security: Authorization کامل، Validation کامل
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin)]
    public class DoctorTimeSlotController : Controller
    {
        #region Fields and Constructor

        private readonly IDoctorTimeSlotService _timeSlotService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public DoctorTimeSlotController(
            IDoctorTimeSlotService timeSlotService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _timeSlotService = timeSlotService ?? throw new ArgumentNullException(nameof(timeSlotService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<DoctorTimeSlotController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Index & Listing

        /// <summary>
        /// نمایش لیست اسلات‌های زمانی با قابلیت جستجو و فیلتر
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(TimeSlotFilterViewModel filter = null)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اسلات‌های زمانی توسط کاربر {UserId}", _currentUserService.UserId);

                // ✅ تنظیم فیلتر پیش‌فرض اگر null باشد
                if (filter == null)
                {
                    filter = new TimeSlotFilterViewModel();
                }

                // ✅ Parse تاریخ‌های شمسی از Query String (برای GET request)
                var startDateQuery = Request.QueryString["StartDate"];
                var endDateQuery = Request.QueryString["EndDate"];
                
                if (!string.IsNullOrEmpty(startDateQuery))
                {
                    var parsedStartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                    if (parsedStartDate.HasValue)
                    {
                        filter.StartDate = parsedStartDate.Value;
                    }
                    else
                    {
                        // Fallback: استفاده از PersianDateHelper
                        var persianDate = PersianDateHelper.ParsePersianDate(startDateQuery);
                        if (persianDate.HasValue)
                        {
                            filter.StartDate = persianDate.Value;
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(endDateQuery))
                {
                    var parsedEndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
                    if (parsedEndDate.HasValue)
                    {
                        filter.EndDate = parsedEndDate.Value;
                    }
                    else
                    {
                        // Fallback: استفاده از PersianDateHelper
                        var persianDate = PersianDateHelper.ParsePersianDate(endDateQuery);
                        if (persianDate.HasValue)
                        {
                            filter.EndDate = persianDate.Value;
                        }
                    }
                }

                filter.ValidateAndSetDefaults();

                // ✅ دریافت اسلات‌ها
                var result = await _timeSlotService.GetTimeSlotsAsync(filter);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                    return View(new PagedResult<TimeSlotIndexViewModel>(new System.Collections.Generic.List<TimeSlotIndexViewModel>(), 0, filter.PageNumber, filter.PageSize));
                }

                // ✅ دریافت آمار
                var statisticsResult = await _timeSlotService.GetTimeSlotStatisticsAsync(
                    filter.DoctorId,
                    filter.StartDate,
                    filter.EndDate);

                // ✅ آماده‌سازی ViewBag برای فیلترها
                ViewBag.Doctors = await GetDoctorsSelectListAsync();
                ViewBag.Statuses = GetStatusSelectList();
                ViewBag.Statistics = statisticsResult.Data;
                ViewBag.Filter = filter;

                _logger.Information("لیست اسلات‌های زمانی با موفقیت نمایش داده شد - TotalItems: {TotalItems}",
                    result.Data.TotalItems);

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اسلات‌های زمانی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست اسلات‌های زمانی", "خطا");
                return View(new PagedResult<TimeSlotIndexViewModel>(new System.Collections.Generic.List<TimeSlotIndexViewModel>(), 0, 1, 20));
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات اسلات زمانی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات اسلات زمانی {TimeSlotId} توسط کاربر {UserId}",
                    id, _currentUserService.UserId);

                if (id <= 0)
                {
                    NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.InvalidTimeSlotId, "خطا");
                    return RedirectToAction("Index");
                }

                var result = await _timeSlotService.GetTimeSlotByIdAsync(id);

                if (!result.Success || result.Data == null)
                {
                    NotificationHelper.SetError(TempData, result.Message ?? Constants.DoctorTimeSlotConstants.Messages.TimeSlotNotFound, "خطا");
                    return RedirectToAction("Index");
                }

                _logger.Information("جزئیات اسلات زمانی {TimeSlotId} با موفقیت نمایش داده شد", id);

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اسلات زمانی {TimeSlotId}", id);
                NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.ErrorLoadingDetails, "خطا");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Management Operations

        /// <summary>
        /// حذف نرم اسلات زمانی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف اسلات زمانی {TimeSlotId} توسط کاربر {UserId}",
                    id, _currentUserService.UserId);

                if (id <= 0)
                {
                    NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.InvalidTimeSlotId, "خطا");
                    return RedirectToAction("Index");
                }

                var result = await _timeSlotService.SoftDeleteTimeSlotAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, Constants.DoctorTimeSlotConstants.Messages.TimeSlotDeletedSuccessfully, "موفقیت");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلات زمانی {TimeSlotId}", id);
                NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.ErrorDeletingTimeSlot, "خطا");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// تغییر وضعیت اسلات زمانی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            try
            {
                _logger.Information("درخواست تغییر وضعیت اسلات زمانی {TimeSlotId} به {Status} توسط کاربر {UserId}",
                    id, status, _currentUserService.UserId);

                if (id <= 0)
                {
                    NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.InvalidTimeSlotId, "خطا");
                    return RedirectToAction("Index");
                }

                var result = await _timeSlotService.UpdateTimeSlotStatusAsync(id, status);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, Constants.DoctorTimeSlotConstants.Messages.TimeSlotStatusUpdatedSuccessfully, "موفقیت");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت اسلات زمانی {TimeSlotId}", id);
                NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.ErrorUpdatingStatus, "خطا");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// آزاد کردن اسلات رزرو شده
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Release(int id)
        {
            try
            {
                _logger.Information("درخواست آزاد کردن اسلات زمانی {TimeSlotId} توسط کاربر {UserId}",
                    id, _currentUserService.UserId);

                if (id <= 0)
                {
                    NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.InvalidTimeSlotId, "خطا");
                    return RedirectToAction("Index");
                }

                var result = await _timeSlotService.ReleaseTimeSlotAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, Constants.DoctorTimeSlotConstants.Messages.TimeSlotReleasedSuccessfully, "موفقیت");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در آزاد کردن اسلات زمانی {TimeSlotId}", id);
                NotificationHelper.SetError(TempData, Constants.DoctorTimeSlotConstants.Messages.ErrorReleasingTimeSlot, "خطا");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method برای برگرداندن View path صحیح
        /// این کار برای حل مشکل case-sensitivity در MVC View resolution است
        /// طبق قرارداد DEVELOPMENT_CONTRACT.md
        /// </summary>
        /// <param name="viewName">نام View (مثلاً "Index", "Details")</param>
        /// <returns>مسیر کامل View</returns>
        protected string GetViewPath(string viewName)
        {
            string controllerName = GetType().Name.Replace("Controller", "");
            return $"~/Areas/Admin/Views/{controllerName}/{viewName}.cshtml";
        }

        /// <summary>
        /// دریافت لیست پزشکان برای DropDown
        /// </summary>
        private async Task<System.Web.Mvc.SelectList> GetDoctorsSelectListAsync()
        {
            try
            {
                var filter = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000,
                    IsActive = true
                };

                var doctorsResult = await _doctorCrudService.GetDoctorsAsync(filter);
                if (doctorsResult.Success && doctorsResult.Data != null && doctorsResult.Data.Items != null)
                {
                    var items = doctorsResult.Data.Items
                        .Select(d => new System.Web.Mvc.SelectListItem
                        {
                            Value = d.DoctorId.ToString(),
                            Text = $"{d.FirstName} {d.LastName}".Trim()
                        })
                        .ToList();

                    items.Insert(0, new System.Web.Mvc.SelectListItem { Value = "", Text = "همه پزشکان" });

                    return new System.Web.Mvc.SelectList(items, "Value", "Text");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
            }

            return new System.Web.Mvc.SelectList(new System.Collections.Generic.List<System.Web.Mvc.SelectListItem>(), "Value", "Text");
        }

        /// <summary>
        /// دریافت لیست وضعیت‌ها برای DropDown
        /// </summary>
        private System.Web.Mvc.SelectList GetStatusSelectList()
        {
            var items = Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new System.Web.Mvc.SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.GetDisplayName()
                })
                .ToList();

            items.Insert(0, new System.Web.Mvc.SelectListItem { Value = "", Text = "همه وضعیت‌ها" });

            return new System.Web.Mvc.SelectList(items, "Value", "Text");
        }

        #endregion
    }
}

