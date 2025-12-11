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
    /// کنترلر مدیریت اطلاعات بیمه (Insurance Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class InsuranceInfoController : BaseCMSController
    {
        private readonly IInsuranceInfoService _insuranceInfoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string InsuranceImageUploadPath = "~/Content/Images/insurance";
        private const string InsuranceThumbnailUploadPath = "~/Content/Images/insurance/thumbnails";
        private const int LogoWidth = 200;
        private const int LogoHeight = 200;
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public InsuranceInfoController(
            IInsuranceInfoService insuranceInfoService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService)
        {
            _insuranceInfoService = insuranceInfoService ?? throw new ArgumentNullException(nameof(insuranceInfoService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<InsuranceInfoController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(InsuranceInfoSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اطلاعات بیمه توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new InsuranceInfoSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _insuranceInfoService.GetInsuranceInfosAsync(searchModel);

                // بارگذاری انواع بیمه برای فیلتر
                var typesResult = await _insuranceInfoService.GetInsuranceTypesAsync();

                var viewModel = new InsuranceInfoAdminIndexViewModel
                {
                    InsuranceInfos = result.Success && result.Data != null
                        ? result.Data
                        : new PagedResult<InsuranceInfoIndexViewModel>(new System.Collections.Generic.List<InsuranceInfoIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize),
                    InsuranceTypes = typesResult.Success && typesResult.Data != null
                        ? typesResult.Data
                        : new System.Collections.Generic.List<InsuranceInfoTypeViewModel>()
                };

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اطلاعات بیمه: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعات بیمه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست اطلاعات بیمه");
                return View(GetViewPath("Index"), new InsuranceInfoAdminIndexViewModel
                {
                    InsuranceInfos = new PagedResult<InsuranceInfoIndexViewModel>(new System.Collections.Generic.List<InsuranceInfoIndexViewModel>(), 0, 1, 10),
                    InsuranceTypes = new System.Collections.Generic.List<InsuranceInfoTypeViewModel>()
                });
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _insuranceInfoService.GetInsuranceInfoDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات اطلاعات بیمه");
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
                var model = new InsuranceInfoCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    InsuranceType = "basic"
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اطلاعات بیمه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد اطلاعات بیمه");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Create(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد اطلاعات بیمه جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // پردازش آپلود تصاویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _insuranceInfoService.CreateInsuranceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد اطلاعات بیمه: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("اطلاعات بیمه با موفقیت ایجاد شد - InsuranceInfoId: {InsuranceInfoId}", result.Data.InsuranceInfoId);
                TempData["Success"] = "اطلاعات بیمه با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات بیمه");
                TempData["Error"] = "خطا در ایجاد اطلاعات بیمه";
                return View(model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _insuranceInfoService.GetInsuranceInfoForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش اطلاعات بیمه");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Edit(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);

                // پردازش آپلود تصاویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _insuranceInfoService.UpdateInsuranceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی اطلاعات بیمه: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("اطلاعات بیمه با موفقیت به‌روزرسانی شد - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);
                NotificationHelper.SetSuccess(TempData, "اطلاعات بیمه با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", model.InsuranceInfoId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی اطلاعات بیمه");
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
                _logger.Information("درخواست حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);

                var result = await _insuranceInfoService.DeleteInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در حذف اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _insuranceInfoService.ActivateInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در فعال‌سازی اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _insuranceInfoService.DeactivateInsuranceInfoAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اطلاعات بیمه با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی اطلاعات بیمه";
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
                var result = await _insuranceInfoService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "اطلاعات بیمه به عنوان ویژه تنظیم شد" : "اطلاعات بیمه از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات بیمه - InsuranceInfoId: {InsuranceInfoId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه اطلاعات بیمه";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر لوگو و تصویر کوچک
        /// </summary>
        private async Task ProcessImageUpload(InsuranceInfoCreateEditViewModel model)
        {
            try
            {
                var logoFile = Request.Files["LogoFile"];
                var thumbnailFile = Request.Files["ThumbnailFile"];

                // اگر لوگو آپلود شده
                if (logoFile != null && logoFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        logoFile,
                        InsuranceImageUploadPath,
                        InsuranceThumbnailUploadPath,
                        LogoWidth,
                        LogoHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود لوگو: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("LogoFile", uploadResult.Message);
                        return;
                    }

                    // حذف لوگوی قبلی در صورت وجود (فقط در Edit)
                    if (model.InsuranceInfoId > 0 && !string.IsNullOrEmpty(model.LogoUrl))
                    {
                        _imageUploadService.DeleteImage(model.LogoUrl);
                    }

                    // تنظیم مسیر لوگو
                    model.LogoUrl = uploadResult.Data.ImageUrl;

                    _logger.Information("لوگو با موفقیت آپلود شد: {LogoUrl}", model.LogoUrl);
                }

                // اگر تصویر کوچک آپلود شده
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    var thumbnailResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        InsuranceThumbnailUploadPath,
                        InsuranceThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!thumbnailResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر کوچک: {ErrorMessage}", thumbnailResult.Message);
                        NotificationHelper.SetError(TempData, thumbnailResult.Message);
                        ModelState.AddModelError("ThumbnailFile", thumbnailResult.Message);
                        return;
                    }

                    // حذف تصویر کوچک قبلی در صورت وجود (فقط در Edit)
                    if (model.InsuranceInfoId > 0 && !string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        _imageUploadService.DeleteImage(model.ThumbnailUrl);
                    }

                    // تنظیم مسیر تصویر کوچک
                    model.ThumbnailUrl = thumbnailResult.Data.ImageUrl;

                    _logger.Information("تصویر کوچک با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
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

