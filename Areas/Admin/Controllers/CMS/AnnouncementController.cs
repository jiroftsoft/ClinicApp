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
    public class AnnouncementController : Controller
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
                    return View(new System.Collections.Generic.List<AnnouncementIndexViewModel>());
                }

                ViewBag.IncludeInactive = includeInactive;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعیه‌ها");
                TempData["Error"] = "خطا در بارگذاری لیست اطلاعیه‌ها";
                return View(new System.Collections.Generic.List<AnnouncementIndexViewModel>());
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

                return View(result.Data);
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

                return View(model);
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
        public async Task<ActionResult> Create(AnnouncementCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _announcementService.CreateAnnouncementAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اطلاعیه با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعیه");
                TempData["Error"] = "خطا در ایجاد اطلاعیه";
                return View(model);
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

                return View(result.Data);
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
        public async Task<ActionResult> Edit(AnnouncementCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _announcementService.UpdateAnnouncementAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اطلاعیه با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعیه - AnnouncementId: {AnnouncementId}", model.AnnouncementId);
                TempData["Error"] = "خطا در به‌روزرسانی اطلاعیه";
                return View(model);
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

