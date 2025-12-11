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
    /// کنترلر مدیریت گالری تصاویر
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class GalleryController : BaseCMSController
    {
        private readonly IGalleryService _galleryService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public GalleryController(
            IGalleryService galleryService,
            ICurrentUserService currentUserService)
        {
            _galleryService = galleryService ?? throw new ArgumentNullException(nameof(galleryService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<GalleryController>();
        }

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(string category = null)
        {
            try
            {
                var result = await _galleryService.GetGalleryItemsAsync(category);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new System.Collections.Generic.List<GalleryItemIndexViewModel>());
                }

                var categoriesResult = await _galleryService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<string>();
                ViewBag.SelectedCategory = category;

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست گالری");
                TempData["Error"] = "خطا در بارگذاری لیست گالری";
                return View(new System.Collections.Generic.List<GalleryItemIndexViewModel>());
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _galleryService.GetGalleryItemDetailsAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات آیتم گالری";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new GalleryItemCreateEditViewModel
                {
                    IsActive = true,
                    DisplayOrder = 0,
                    Category = "clinic"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد آیتم گالری");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد آیتم گالری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GalleryItemCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _galleryService.CreateGalleryItemAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "آیتم گالری با موفقیت ایجاد شد";
                return RedirectToAction("Index", new { category = model.Category });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد آیتم گالری");
                TempData["Error"] = "خطا در ایجاد آیتم گالری";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _galleryService.GetGalleryItemForEditAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش آیتم گالری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(GalleryItemCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _galleryService.UpdateGalleryItemAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "آیتم گالری با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index", new { category = model.Category });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی آیتم گالری - GalleryItemId: {GalleryItemId}", model.GalleryItemId);
                TempData["Error"] = "خطا در به‌روزرسانی آیتم گالری";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _galleryService.DeleteGalleryItemAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "آیتم گالری با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                TempData["Error"] = "خطا در حذف آیتم گالری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _galleryService.ActivateGalleryItemAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "آیتم گالری با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                TempData["Error"] = "خطا در فعال‌سازی آیتم گالری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _galleryService.DeactivateGalleryItemAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "آیتم گالری با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی آیتم گالری";
                return RedirectToAction("Index");
            }
        }
    }
}

