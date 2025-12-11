using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;
using static ClinicApp.Helpers.NotificationHelper;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت نکات سلامت (Health Tips)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class HealthTipController : BaseCMSController
    {
        private readonly IHealthTipService _healthTipService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string HealthTipImageUploadPath = "~/Content/Images/health-tips";
        private const string HealthTipThumbnailUploadPath = "~/Content/Images/health-tips/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public HealthTipController(
            IHealthTipService healthTipService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService)
        {
            _healthTipService = healthTipService ?? throw new ArgumentNullException(nameof(healthTipService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<HealthTipController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(HealthTipSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست نکات سلامت توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new HealthTipSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _healthTipService.GetHealthTipsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست نکات سلامت: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyResult = new PagedResult<HealthTipIndexViewModel>(new System.Collections.Generic.List<HealthTipIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize);
                    return View(GetViewPath("Index"), emptyResult);
                }

                // بارگذاری دسته‌بندی‌ها برای فیلتر
                var categoriesResult = await _healthTipService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<HealthTipCategoryViewModel>();

                // Production-Ready: استفاده از GetViewPath برای جلوگیری از تداخل با Views/HealthTip/Index.cshtml
                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست نکات سلامت");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست نکات سلامت");
                var emptyResult = new PagedResult<HealthTipIndexViewModel>(new System.Collections.Generic.List<HealthTipIndexViewModel>(), 0, 1, 10);
                return View(GetViewPath("Index"), emptyResult);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _healthTipService.GetHealthTipDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new HealthTipCreateEditViewModel
                {
                    IsPublished = false,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    Category = "general"
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد نکته سلامت");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field
        public async Task<ActionResult> Create(HealthTipCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد نکته سلامت جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // Parse تاریخ‌ها از hidden input
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);
                model.ExpiryDate = this.ParseDateFromHiddenInput("ExpiryDate", _logger);
                
                // لاگ برای دیباگ
                _logger.Debug("مقادیر parse شده - PublishedAt: {PublishedAt}, ExpiryDate: {ExpiryDate}, IsPublished: {IsPublished}",
                    model.PublishedAt, model.ExpiryDate, model.IsPublished);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _healthTipService.CreateHealthTipAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد نکته سلامت: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("نکته سلامت با موفقیت ایجاد شد - HealthTipId: {HealthTipId}", result.Data.HealthTipId);
                NotificationHelper.SetSuccess(TempData, "نکته سلامت با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نکته سلامت");
                NotificationHelper.SetError(TempData, "خطا در ایجاد نکته سلامت");
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _healthTipService.GetHealthTipForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Content field
        public async Task<ActionResult> Edit(HealthTipCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی نکته سلامت - HealthTipId: {HealthTipId}", model.HealthTipId);

                // Parse تاریخ‌ها از hidden input
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);
                model.ExpiryDate = this.ParseDateFromHiddenInput("ExpiryDate", _logger);
                
                // لاگ برای دیباگ
                _logger.Debug("مقادیر parse شده - PublishedAt: {PublishedAt}, ExpiryDate: {ExpiryDate}, IsPublished: {IsPublished}",
                    model.PublishedAt, model.ExpiryDate, model.IsPublished);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _healthTipService.UpdateHealthTipAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی نکته سلامت: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("نکته سلامت با موفقیت به‌روزرسانی شد - HealthTipId: {HealthTipId}", model.HealthTipId);
                NotificationHelper.SetSuccess(TempData, "نکته سلامت با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نکته سلامت - HealthTipId: {HealthTipId}", model.HealthTipId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی نکته سلامت");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("درخواست حذف نکته سلامت - HealthTipId: {HealthTipId}", id);

                var result = await _healthTipService.DeleteHealthTipAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نکته سلامت با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Publish/Unpublish

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish(int id)
        {
            try
            {
                var result = await _healthTipService.PublishHealthTipAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نکته سلامت با موفقیت منتشر شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در انتشار نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Unpublish(int id)
        {
            try
            {
                var result = await _healthTipService.UnpublishHealthTipAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نکته سلامت از حالت انتشار خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در لغو انتشار نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Featured

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _healthTipService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    var message = isFeatured ? "نکته سلامت به عنوان ویژه تنظیم شد" : "نکته سلامت از حالت ویژه خارج شد";
                    NotificationHelper.SetSuccess(TempData, message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نکته سلامت - HealthTipId: {HealthTipId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم وضعیت ویژه نکته سلامت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر نکته سلامت
        /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP
        /// </summary>
        private async Task ProcessImageUpload(HealthTipCreateEditViewModel model)
        {
            try
            {
                var imageFile = Request.Files["ImageFile"];
                var thumbnailFile = Request.Files["ThumbnailFile"];

                // اگر تصویر اصلی آپلود شده
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        imageFile,
                        HealthTipImageUploadPath,
                        HealthTipThumbnailUploadPath,
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
                        HealthTipThumbnailUploadPath,
                        HealthTipThumbnailUploadPath,
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

