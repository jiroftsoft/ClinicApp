using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت صفحه "درباره ما" (About Page)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class AboutPageController : BaseCMSController
    {
        private readonly IAboutPageService _aboutPageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string AboutImageUploadPath = "~/Content/Images/about";
        private const string AboutThumbnailUploadPath = "~/Content/Images/about/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public AboutPageController(
            IAboutPageService aboutPageService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService)
        {
            _aboutPageService = aboutPageService ?? throw new ArgumentNullException(nameof(aboutPageService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<AboutPageController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(AboutPageSearchViewModel filter)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست About Pages توسط کاربر {UserId}", _currentUserService.UserId);

                if (filter == null)
                {
                    filter = new AboutPageSearchViewModel();
                }

                var result = await _aboutPageService.GetAboutPagesAsync(filter);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست About Pages: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new AboutPageIndexPageViewModel
                    {
                        PagedResult = new Interfaces.PagedResult<AboutPageIndexViewModel>
                        {
                            Items = new List<AboutPageIndexViewModel>(),
                            TotalCount = 0,
                            PageNumber = 1,
                            PageSize = 10
                        }
                    });
                }

                var viewModel = new AboutPageIndexPageViewModel
                {
                    PagedResult = result.Data
                };

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست About Pages");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست About Pages");
                return View(GetViewPath("Index"), new AboutPageIndexPageViewModel
                {
                    PagedResult = new Interfaces.PagedResult<AboutPageIndexViewModel>
                    {
                        Items = new List<AboutPageIndexViewModel>(),
                        TotalCount = 0,
                        PageNumber = 1,
                        PageSize = 10
                    }
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
                var result = await _aboutPageService.GetAboutPageDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات About Page - AboutPageId: {AboutPageId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات About Page");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            var model = new AboutPageCreateEditViewModel
            {
                IsActive = true,
                DisplayOrder = 1,
                MissionValues = new List<MissionValueViewModel>
                {
                    new MissionValueViewModel { Title = "", Description = "", Icon = "fas fa-heart" },
                    new MissionValueViewModel { Title = "", Description = "", Icon = "fas fa-stethoscope" },
                    new MissionValueViewModel { Title = "", Description = "", Icon = "fas fa-shield-alt" }
                },
                Licenses = new List<LicenseViewModel>
                {
                    new LicenseViewModel { Title = "", IssuingAuthority = "", LicenseNumber = "", ValidUntil = "" }
                },
                EthicalCommitments = new List<EthicalCommitmentViewModel>
                {
                    new EthicalCommitmentViewModel { Title = "", Description = "", Icon = "fas fa-lock" },
                    new EthicalCommitmentViewModel { Title = "", Description = "", Icon = "fas fa-shield-alt" },
                    new EthicalCommitmentViewModel { Title = "", Description = "", Icon = "fas fa-user-secret" }
                }
            };

            return View(GetViewPath("Create"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor - اجازه ارسال HTML
        public async Task<ActionResult> Create(AboutPageCreateEditViewModel model, HttpPostedFileBase heroImageFile, HttpPostedFileBase backgroundImageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید");
                    return View(GetViewPath("Create"), model);
                }

                // آپلود تصاویر
                if (heroImageFile != null && heroImageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        heroImageFile,
                        AboutImageUploadPath,
                        AboutThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);
                    
                    if (uploadResult.Success)
                    {
                        model.HeroImageUrl = uploadResult.Data.ImageUrl;
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Hero: {uploadResult.Message}");
                    }
                }

                if (backgroundImageFile != null && backgroundImageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        backgroundImageFile,
                        AboutImageUploadPath,
                        AboutThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);
                    
                    if (uploadResult.Success)
                    {
                        model.BackgroundImageUrl = uploadResult.Data.ImageUrl;
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Background: {uploadResult.Message}");
                    }
                }

                var result = await _aboutPageService.CreateAboutPageAsync(model);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                NotificationHelper.SetSuccess(TempData, "صفحه About با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد About Page");
                NotificationHelper.SetError(TempData, "خطا در ایجاد About Page");
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
                var result = await _aboutPageService.GetAboutPageForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت About Page برای ویرایش - AboutPageId: {AboutPageId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری About Page برای ویرایش");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor - اجازه ارسال HTML
        public async Task<ActionResult> Edit(AboutPageCreateEditViewModel model, HttpPostedFileBase heroImageFile, HttpPostedFileBase backgroundImageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید");
                    return View(GetViewPath("Edit"), model);
                }

                // آپلود تصاویر (فقط در صورت ارسال فایل جدید)
                if (heroImageFile != null && heroImageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        heroImageFile,
                        AboutImageUploadPath,
                        AboutThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);
                    
                    if (uploadResult.Success)
                    {
                        model.HeroImageUrl = uploadResult.Data.ImageUrl;
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Hero: {uploadResult.Message}");
                    }
                }

                if (backgroundImageFile != null && backgroundImageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        backgroundImageFile,
                        AboutImageUploadPath,
                        AboutThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);
                    
                    if (uploadResult.Success)
                    {
                        model.BackgroundImageUrl = uploadResult.Data.ImageUrl;
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Background: {uploadResult.Message}");
                    }
                }

                var result = await _aboutPageService.UpdateAboutPageAsync(model);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                NotificationHelper.SetSuccess(TempData, "صفحه About با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی About Page");
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی About Page");
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
                var result = await _aboutPageService.DeleteAboutPageAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "صفحه About با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف About Page - AboutPageId: {AboutPageId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف About Page");
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
                var result = await _aboutPageService.ActivateAboutPageAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "صفحه About فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی About Page - AboutPageId: {AboutPageId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی About Page");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _aboutPageService.DeactivateAboutPageAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "صفحه About غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی About Page - AboutPageId: {AboutPageId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی About Page");
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}
