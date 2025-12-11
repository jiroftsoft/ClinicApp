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
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string GalleryImageUploadPath = "~/Content/Images/gallery";
        private const string GalleryThumbnailUploadPath = "~/Content/Images/gallery/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public GalleryController(
            IGalleryService galleryService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService)
        {
            _galleryService = galleryService ?? throw new ArgumentNullException(nameof(galleryService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<GalleryController>();
        }

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(string category = null)
        {
            try
            {
                var result = await _galleryService.GetGalleryItemsAsync(category);
                var items = result.Success 
                    ? result.Data 
                    : new System.Collections.Generic.List<GalleryItemIndexViewModel>();

                var categoriesResult = await _galleryService.GetCategoriesAsync();
                var categories = categoriesResult.Success 
                    ? categoriesResult.Data 
                    : new System.Collections.Generic.List<string>();

                // Create strongly-typed ViewModel
                var viewModel = new GalleryIndexViewModel
                {
                    Items = items,
                    Categories = categories,
                    SelectedCategory = category
                };

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست گالری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست گالری");
                return View(GetViewPath("Index"), new GalleryIndexViewModel
                {
                    Items = new System.Collections.Generic.List<GalleryItemIndexViewModel>(),
                    Categories = new System.Collections.Generic.List<string>(),
                    SelectedCategory = category
                });
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات آیتم گالری");
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

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد آیتم گالری");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد آیتم گالری");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GalleryItemCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد آیتم گالری جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // حذف خطای ImageUrl از ModelState (چون بعد از آپلود تنظیم می‌شود)
                ModelState.Remove("ImageUrl");
                ModelState.Remove("ThumbnailUrl");

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _galleryService.CreateGalleryItemAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد آیتم گالری: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("آیتم گالری با موفقیت ایجاد شد - GalleryItemId: {GalleryItemId}", result.Data.GalleryItemId);
                NotificationHelper.SetSuccess(TempData, "آیتم گالری با موفقیت ایجاد شد");
                return RedirectToAction("Index", new { category = model.Category });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد آیتم گالری");
                NotificationHelper.SetError(TempData, "خطا در ایجاد آیتم گالری");
                return View(GetViewPath("Create"), model);
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش آیتم گالری");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(GalleryItemCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی آیتم گالری - GalleryItemId: {GalleryItemId}", model.GalleryItemId);

                // حذف خطای ImageUrl از ModelState (چون ممکن است بعد از آپلود تنظیم شود)
                ModelState.Remove("ImageUrl");
                ModelState.Remove("ThumbnailUrl");

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _galleryService.UpdateGalleryItemAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی آیتم گالری: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("آیتم گالری با موفقیت به‌روزرسانی شد - GalleryItemId: {GalleryItemId}", model.GalleryItemId);
                NotificationHelper.SetSuccess(TempData, "آیتم گالری با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index", new { category = model.Category });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی آیتم گالری - GalleryItemId: {GalleryItemId}", model.GalleryItemId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی آیتم گالری");
                return View(GetViewPath("Edit"), model);
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "آیتم گالری با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف آیتم گالری");
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "آیتم گالری با موفقیت فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی آیتم گالری");
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "آیتم گالری با موفقیت غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی آیتم گالری - GalleryItemId: {GalleryItemId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی آیتم گالری");
                return RedirectToAction("Index");
            }
        }

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر گالری
        /// </summary>
        private async Task ProcessImageUpload(GalleryItemCreateEditViewModel model)
        {
            try
            {
                var imageFile = Request.Files["ImageFile"];
                var thumbnailFile = Request.Files["ThumbnailFile"];

                // در Create، تصویر الزامی است
                bool isCreate = model.GalleryItemId == 0;
                
                // اگر در Create هستیم و تصویری آپلود نشده
                if (isCreate && (imageFile == null || imageFile.ContentLength == 0))
                {
                    ModelState.AddModelError("ImageFile", "تصویر الزامی است.");
                    return;
                }

                // اگر تصویر اصلی آپلود شده
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        imageFile,
                        GalleryImageUploadPath,
                        GalleryThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("ImageFile", uploadResult.Message);
                        return;
                    }

                    // تنظیم مسیر تصویر اصلی
                    model.ImageUrl = uploadResult.Data.ImageUrl;

                    // اگر thumbnail جداگانه آپلود نشده، از thumbnail خودکار استفاده کن
                    if (thumbnailFile == null || thumbnailFile.ContentLength == 0)
                    {
                        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                    }

                    _logger.Information("تصویر با موفقیت آپلود شد: {ImageUrl}, Thumbnail: {ThumbnailUrl}",
                        model.ImageUrl, model.ThumbnailUrl);
                }

                // اگر thumbnail جداگانه آپلود شده
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    var thumbnailResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        GalleryThumbnailUploadPath,
                        GalleryThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        ThumbnailWidth,
                        ThumbnailHeight);

                    if (!thumbnailResult.Success)
                    {
                        _logger.Warning("خطا در آپلود thumbnail: {ErrorMessage}", thumbnailResult.Message);
                        NotificationHelper.SetError(TempData, thumbnailResult.Message);
                        ModelState.AddModelError("ThumbnailFile", thumbnailResult.Message);
                        return;
                    }

                    model.ThumbnailUrl = thumbnailResult.Data.ImageUrl;
                    _logger.Information("Thumbnail جداگانه با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر");
                NotificationHelper.SetError(TempData, "خطا در آپلود تصویر");
                ModelState.AddModelError("", "خطا در آپلود تصویر");
            }
        }

        #endregion
    }
}

