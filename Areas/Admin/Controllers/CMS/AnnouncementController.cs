using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت اطلاعیه‌ها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class AnnouncementController : BaseCMSController
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public AnnouncementController(
            IAnnouncementService announcementService,
            ICurrentUserService currentUserService)
        {
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<AnnouncementController>();
        }

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(bool includeInactive = false)
        {
            try
            {
                var result = await _announcementService.GetAnnouncementsAsync(includeInactive);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<AnnouncementIndexViewModel>());
                }

                ViewBag.IncludeInactive = includeInactive;
                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعیه‌ها");
                TempData["Error"] = "خطا در بارگذاری لیست اطلاعیه‌ها";
                return View(GetViewPath("Index"), new System.Collections.Generic.List<AnnouncementIndexViewModel>());
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _announcementService.GetAnnouncementDetailsAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new AnnouncementCreateEditViewModel
                {
                    IsActive = true,
                    IsImportant = false,
                    DisplayOrder = 0,
                    Type = "info",
                    TargetAudience = "all"
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اطلاعیه");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field (CKEditor)
        public async Task<ActionResult> Create(AnnouncementCreateEditViewModel model)
        {
            try
            {
                // لاگ تمام Form values برای دیباگ
                _logger.Debug("Form values - StartDate: {StartDate}, StartDate_Hidden: {StartDateHidden}, EndDate: {EndDate}, EndDate_Hidden: {EndDateHidden}",
                    Request.Form["StartDate"], Request.Form["StartDate_Hidden"], Request.Form["EndDate"], Request.Form["EndDate_Hidden"]);
                
                // تبدیل تاریخ‌های شمسی به میلادی از hidden inputs (استفاده از Extension Method)
                model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
                
                _logger.Information("📊 مدل قبل از ذخیره - StartDate: {StartDate}, EndDate: {EndDate}", model.StartDate, model.EndDate);

                // حذف خطاهای validation برای تاریخ‌ها (چون به صورت دستی تبدیل می‌کنیم)
                ModelState.Remove("StartDate");
                ModelState.Remove("EndDate");

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _announcementService.CreateAnnouncementAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(GetViewPath("Create"), model);
                }

                TempData["Success"] = "اطلاعیه با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعیه");
                TempData["Error"] = "خطا در ایجاد اطلاعیه";
                return View(GetViewPath("Create"), model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _announcementService.GetAnnouncementForEditAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field (CKEditor)
        public async Task<ActionResult> Edit(AnnouncementCreateEditViewModel model)
        {
            try
            {
                // لاگ تمام Form values برای دیباگ
                _logger.Debug("Form values - StartDate: {StartDate}, StartDate_Hidden: {StartDateHidden}, EndDate: {EndDate}, EndDate_Hidden: {EndDateHidden}",
                    Request.Form["StartDate"], Request.Form["StartDate_Hidden"], Request.Form["EndDate"], Request.Form["EndDate_Hidden"]);
                
                // تبدیل تاریخ‌های شمسی به میلادی از hidden inputs
                var startDateHidden = Request.Form["StartDate_Hidden"];
                if (!string.IsNullOrEmpty(startDateHidden))
                {
                    // TryParse با CultureInfo.InvariantCulture برای ISO format
                    // تاریخ از JavaScript به صورت local date ارسال شده (بدون timezone)
                    if (DateTime.TryParse(startDateHidden, System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out DateTime startDate))
                    {
                        // تاریخ به صورت Unspecified است، به عنوان local در نظر می‌گیریم
                        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Local);
                        
                        // فقط تاریخ را نگه دار (بدون زمان)
                        model.StartDate = startDate.Date;
                        _logger.Information("✅ تاریخ شروع از hidden input: {StartDate}", model.StartDate);
                    }
                    else
                    {
                        _logger.Warning("⚠️ خطا در parse کردن تاریخ شروع از hidden input: {StartDateHidden}", startDateHidden);
                        var startDatePersian = Request.Form["StartDate"];
                        if (!string.IsNullOrEmpty(startDatePersian))
                        {
                            model.StartDate = Helpers.PersianDateHelper.ParsePersianDate(startDatePersian);
                            _logger.Information("✅ تاریخ شروع از PersianDateHelper: {StartDate}", model.StartDate);
                        }
                        else
                        {
                            model.StartDate = null;
                            _logger.Debug("⚠️ تاریخ شروع null تنظیم شد");
                        }
                    }
                }
                else
                {
                    _logger.Debug("⚠️ Hidden input برای تاریخ شروع خالی است");
                    var startDatePersian = Request.Form["StartDate"];
                    if (!string.IsNullOrEmpty(startDatePersian))
                    {
                        model.StartDate = Helpers.PersianDateHelper.ParsePersianDate(startDatePersian);
                        _logger.Information("✅ تاریخ شروع از input شمسی: {StartDate}", model.StartDate);
                    }
                    else
                    {
                        model.StartDate = null;
                        _logger.Debug("⚠️ تاریخ شروع null تنظیم شد");
                    }
                }

                var endDateHidden = Request.Form["EndDate_Hidden"];
                if (!string.IsNullOrEmpty(endDateHidden))
                {
                    // TryParse با CultureInfo.InvariantCulture برای ISO format
                    // تاریخ از JavaScript به صورت local date ارسال شده (بدون timezone)
                    if (DateTime.TryParse(endDateHidden, System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, out DateTime endDate))
                    {
                        // تاریخ به صورت Unspecified است، به عنوان local در نظر می‌گیریم
                        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Local);
                        
                        // فقط تاریخ را نگه دار (بدون زمان)
                        model.EndDate = endDate.Date;
                        _logger.Information("✅ تاریخ پایان از hidden input: {EndDate}", model.EndDate);
                    }
                    else
                    {
                        _logger.Warning("⚠️ خطا در parse کردن تاریخ پایان از hidden input: {EndDateHidden}", endDateHidden);
                        var endDatePersian = Request.Form["EndDate"];
                        if (!string.IsNullOrEmpty(endDatePersian))
                        {
                            model.EndDate = Helpers.PersianDateHelper.ParsePersianDate(endDatePersian);
                            _logger.Information("✅ تاریخ پایان از PersianDateHelper: {EndDate}", model.EndDate);
                        }
                        else
                        {
                            model.EndDate = null;
                            _logger.Debug("⚠️ تاریخ پایان null تنظیم شد");
                        }
                    }
                }
                else
                {
                    _logger.Debug("⚠️ Hidden input برای تاریخ پایان خالی است");
                    var endDatePersian = Request.Form["EndDate"];
                    if (!string.IsNullOrEmpty(endDatePersian))
                    {
                        model.EndDate = Helpers.PersianDateHelper.ParsePersianDate(endDatePersian);
                        _logger.Information("✅ تاریخ پایان از input شمسی: {EndDate}", model.EndDate);
                    }
                    else
                    {
                        model.EndDate = null;
                        _logger.Debug("⚠️ تاریخ پایان null تنظیم شد");
                    }
                }
                
                _logger.Information("📊 مدل قبل از ذخیره - StartDate: {StartDate}, EndDate: {EndDate}", model.StartDate, model.EndDate);

                // حذف خطاهای validation برای تاریخ‌ها
                ModelState.Remove("StartDate");
                ModelState.Remove("EndDate");

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _announcementService.UpdateAnnouncementAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(GetViewPath("Edit"), model);
                }

                TempData["Success"] = "اطلاعیه با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعیه - AnnouncementId: {AnnouncementId}", model.AnnouncementId);
                TempData["Error"] = "خطا در به‌روزرسانی اطلاعیه";
                return View(GetViewPath("Edit"), model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _announcementService.DeleteAnnouncementAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعیه با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در حذف اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _announcementService.ActivateAnnouncementAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعیه با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در فعال‌سازی اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _announcementService.DeactivateAnnouncementAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعیه با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی اطلاعیه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetImportant(int id, bool isImportant)
        {
            try
            {
                var result = await _announcementService.SetImportantAsync(id, isImportant);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isImportant ? "اطلاعیه به عنوان مهم تنظیم شد" : "اطلاعیه از حالت مهم خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت مهم اطلاعیه - AnnouncementId: {AnnouncementId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت مهم اطلاعیه";
                return RedirectToAction("Index");
            }
        }
    }
}

