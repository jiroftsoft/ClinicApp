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
    /// کنترلر مدیریت مطالب آموزشی بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class PatientEducationMaterialController : BaseCMSController
    {
        private readonly IPatientEducationMaterialService _materialService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IDocumentUploadService _documentUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string MaterialDocumentUploadPath = "~/Content/Documents/patient-education";
        private const string MaterialImageUploadPath = "~/Content/Images/patient-education";
        private const string MaterialThumbnailUploadPath = "~/Content/Images/patient-education/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920;
        private const int MaxImageHeight = 1080;

        public PatientEducationMaterialController(
            IPatientEducationMaterialService materialService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService,
            IDocumentUploadService documentUploadService)
        {
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _documentUploadService = documentUploadService ?? throw new ArgumentNullException(nameof(documentUploadService));
            _logger = Log.ForContext<PatientEducationMaterialController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(PatientEducationMaterialSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست مطالب آموزشی توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                {
                    searchModel = new PatientEducationMaterialSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _materialService.GetMaterialsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست مطالب آموزشی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyResult = new PagedResult<PatientEducationMaterialIndexViewModel>(new System.Collections.Generic.List<PatientEducationMaterialIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize);
                    var emptyPageViewModel = new PatientEducationMaterialIndexPageViewModel
                    {
                        Materials = emptyResult,
                        SearchModel = searchModel
                    };
                    return View(GetViewPath("Index"), emptyPageViewModel);
                }

                var pageViewModel = new PatientEducationMaterialIndexPageViewModel
                {
                    Materials = result.Data,
                    SearchModel = searchModel
                };

                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست مطالب آموزشی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست مطالب آموزشی");
                var emptyResult = new PagedResult<PatientEducationMaterialIndexViewModel>(new System.Collections.Generic.List<PatientEducationMaterialIndexViewModel>(), 0, 1, 10);
                var emptyPageViewModel = new PatientEducationMaterialIndexPageViewModel
                {
                    Materials = emptyResult,
                    SearchModel = new PatientEducationMaterialSearchViewModel { PageNumber = 1, PageSize = 10 }
                };
                return View(GetViewPath("Index"), emptyPageViewModel);
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new PatientEducationMaterialCreateEditViewModel
                {
                    IsPublished = false,
                    IsFeatured = false,
                    DisplayOrder = 0
                };
                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد مطلب آموزشی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Create(PatientEducationMaterialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد مطلب آموزشی جدید - Title: {Title}", model.Title);

                // Parse تاریخ انتشار
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);

                // پردازش آپلود فایل مستندات
                await ProcessDocumentUpload(model);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً خطاهای موجود در فرم را برطرف کنید.");
                    return View(GetViewPath("Create"), model);
                }

                var result = await _materialService.CreateMaterialAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد مطلب آموزشی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("مطلب آموزشی با موفقیت ایجاد شد - MaterialId: {MaterialId}", result.Data.PatientEducationMaterialId);
                NotificationHelper.SetSuccess(TempData, "مطلب آموزشی با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد مطلب آموزشی");
                NotificationHelper.SetError(TempData, "خطا در ایجاد مطلب آموزشی");
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
                var result = await _materialService.GetMaterialForEditAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش مطلب آموزشی - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(PatientEducationMaterialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی مطلب آموزشی - MaterialId: {MaterialId}", model.PatientEducationMaterialId);

                // Parse تاریخ انتشار
                model.PublishedAt = this.ParseDateFromHiddenInput("PublishedAt", _logger);

                // پردازش آپلود فایل مستندات
                await ProcessDocumentUpload(model);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً خطاهای موجود در فرم را برطرف کنید.");
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _materialService.UpdateMaterialAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی مطلب آموزشی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("مطلب آموزشی با موفقیت به‌روزرسانی شد - MaterialId: {MaterialId}", model.PatientEducationMaterialId);
                NotificationHelper.SetSuccess(TempData, "مطلب آموزشی با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی مطلب آموزشی - MaterialId: {MaterialId}", model.PatientEducationMaterialId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی مطلب آموزشی");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _materialService.GetMaterialDetailsAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات مطلب آموزشی - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات");
                return RedirectToAction("Index");
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
                var result = await _materialService.DeleteMaterialAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مطلب آموزشی با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف مطلب آموزشی - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف مطلب آموزشی");
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
                var result = await _materialService.PublishMaterialAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مطلب آموزشی با موفقیت منتشر شد");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار مطلب آموزشی - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در انتشار مطلب آموزشی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Unpublish(int id)
        {
            try
            {
                var result = await _materialService.UnpublishMaterialAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "مطلب آموزشی با موفقیت از حالت انتشار خارج شد");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار مطلب آموزشی - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در لغو انتشار مطلب آموزشی");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Set Featured

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _materialService.SetFeaturedAsync(id, isFeatured);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ویژه - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم ویژه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region File Upload Processing

        private async Task ProcessDocumentUpload(PatientEducationMaterialCreateEditViewModel model)
        {
            try
            {
                var documentFile = Request.Files["DocumentFile"];
                
                if (documentFile != null && documentFile.ContentLength > 0)
                {
                    var uploadResult = _documentUploadService.UploadDocument(
                        documentFile,
                        MaterialDocumentUploadPath);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود فایل مستندات: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("DocumentFile", uploadResult.Message);
                        return;
                    }

                    // حذف فایل قدیمی در صورت وجود
                    if (!string.IsNullOrEmpty(model.FileUrl))
                    {
                        _documentUploadService.DeleteDocument(model.FileUrl);
                    }

                    model.FileUrl = uploadResult.Data.FileUrl;
                    model.FileName = uploadResult.Data.OriginalFileName;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود فایل مستندات");
                NotificationHelper.SetError(TempData, "خطا در آپلود فایل مستندات");
                ModelState.AddModelError("", "خطا در آپلود فایل مستندات");
            }
        }

        private async Task ProcessImageUpload(PatientEducationMaterialCreateEditViewModel model)
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
                        MaterialImageUploadPath,
                        MaterialThumbnailUploadPath,
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
                    
                    model.ImageUrl = uploadResult.Data.ImageUrl;
                    
                    // اگر thumbnail جداگانه آپلود نشده، از thumbnail خودکار استفاده کن
                    if (thumbnailFile == null || thumbnailFile.ContentLength == 0)
                    {
                        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                    }
                }
                
                // اگر thumbnail جداگانه آپلود شده
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    var thumbnailResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        MaterialThumbnailUploadPath,
                        MaterialThumbnailUploadPath,
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

