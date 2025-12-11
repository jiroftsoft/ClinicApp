using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت اطلاعات خدمات پزشکی (Medical Service Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class MedicalServiceInfoController : BaseCMSController
    {
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string ServiceImageUploadPath = "~/Content/Images/services";
        private const string ServiceThumbnailUploadPath = "~/Content/Images/services/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public MedicalServiceInfoController(
            IMedicalServiceInfoService medicalServiceInfoService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            IImageUploadService imageUploadService)
        {
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<MedicalServiceInfoController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(MedicalServiceInfoSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اطلاعات خدمات پزشکی توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new MedicalServiceInfoSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _medicalServiceInfoService.GetMedicalServiceInfosAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اطلاعات خدمات پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    
                    var emptyViewModel = new MedicalServiceInfoAdminIndexViewModel
                    {
                        MedicalServiceInfos = new PagedResult<MedicalServiceInfoIndexViewModel>(new List<MedicalServiceInfoIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize),
                        Services = new List<MedicalServiceInfoServiceViewModel>(),
                        ServiceCategories = new List<MedicalServiceInfoCategoryViewModel>(),
                        SearchModel = searchModel
                    };
                    return View(GetViewPath("Index"), emptyViewModel);
                }

                // بارگذاری دسته‌بندی‌های خدمات برای فیلتر
                var serviceCategories = await _context.ServiceCategories
                    .Where(sc => !sc.IsDeleted)
                    .OrderBy(sc => sc.Title)
                    .Select(sc => new MedicalServiceInfoCategoryViewModel
                    {
                        ServiceCategoryId = sc.ServiceCategoryId,
                        Title = sc.Title
                    })
                    .ToListAsync();

                // بارگذاری خدمات برای فیلتر
                var services = await _context.Services
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Title)
                    .Select(s => new MedicalServiceInfoServiceViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceTitle = s.Title,
                        ServiceCode = s.ServiceCode,
                        ServiceCategoryTitle = s.ServiceCategory != null ? s.ServiceCategory.Title : ""
                    })
                    .ToListAsync();

                var viewModel = new MedicalServiceInfoAdminIndexViewModel
                {
                    MedicalServiceInfos = result.Data,
                    Services = services,
                    ServiceCategories = serviceCategories,
                    SearchModel = searchModel
                };

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اطلاعات خدمات پزشکی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست اطلاعات خدمات پزشکی");
                
                var emptyViewModel = new MedicalServiceInfoAdminIndexViewModel
                {
                    MedicalServiceInfos = new PagedResult<MedicalServiceInfoIndexViewModel>(new List<MedicalServiceInfoIndexViewModel>(), 0, 1, 10),
                    Services = new List<MedicalServiceInfoServiceViewModel>(),
                    ServiceCategories = new List<MedicalServiceInfoCategoryViewModel>(),
                    SearchModel = new MedicalServiceInfoSearchViewModel { PageNumber = 1, PageSize = 10 }
                };
                return View(GetViewPath("Index"), emptyViewModel);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _medicalServiceInfoService.GetMedicalServiceInfoDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات اطلاعات خدمت پزشکی");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                // بارگذاری لیست خدمات برای dropdown
                var services = await GetServicesAsync();

                var model = new MedicalServiceInfoCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0
                };

                var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                {
                    Model = model,
                    Services = services
                };

                return View(GetViewPath("Create"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اطلاعات خدمت پزشکی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد اطلاعات خدمت پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Create(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد اطلاعات خدمت پزشکی جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                var services = await GetServicesAsync();

                if (!ModelState.IsValid)
                {
                    var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                    {
                        Model = model,
                        Services = services
                    };
                    return View(GetViewPath("Create"), pageViewModel);
                }

                var result = await _medicalServiceInfoService.CreateMedicalServiceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد اطلاعات خدمت پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    
                    var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                    {
                        Model = model,
                        Services = services
                    };
                    return View(GetViewPath("Create"), pageViewModel);
                }

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت ایجاد شد - MedicalServiceInfoId: {MedicalServiceInfoId}", result.Data.MedicalServiceInfoId);
                NotificationHelper.SetSuccess(TempData, "اطلاعات خدمت پزشکی با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اطلاعات خدمت پزشکی");
                NotificationHelper.SetError(TempData, "خطا در ایجاد اطلاعات خدمت پزشکی");
                
                var services = await GetServicesAsync();
                var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                {
                    Model = model,
                    Services = services
                };
                return View(GetViewPath("Create"), pageViewModel);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _medicalServiceInfoService.GetMedicalServiceInfoForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                // بارگذاری لیست خدمات برای dropdown
                var services = await GetServicesAsync();

                var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                {
                    Model = result.Data,
                    Services = services
                };

                return View(GetViewPath("Edit"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش اطلاعات خدمت پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in FullDescription field
        public async Task<ActionResult> Edit(MedicalServiceInfoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                var services = await GetServicesAsync();

                if (!ModelState.IsValid)
                {
                    var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                    {
                        Model = model,
                        Services = services
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                var result = await _medicalServiceInfoService.UpdateMedicalServiceInfoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی اطلاعات خدمت پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    
                    var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                    {
                        Model = model,
                        Services = services
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                _logger.Information("اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);
                NotificationHelper.SetSuccess(TempData, "اطلاعات خدمت پزشکی با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", model.MedicalServiceInfoId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی اطلاعات خدمت پزشکی");
                
                var services = await GetServicesAsync();
                var pageViewModel = new MedicalServiceInfoCreateEditPageViewModel
                {
                    Model = model,
                    Services = services
                };
                return View(GetViewPath("Edit"), pageViewModel);
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
                _logger.Information("درخواست حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);

                var result = await _medicalServiceInfoService.DeleteMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اطلاعات خدمت پزشکی با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف اطلاعات خدمت پزشکی");
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
                var result = await _medicalServiceInfoService.ActivateMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اطلاعات خدمت پزشکی با موفقیت فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی اطلاعات خدمت پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _medicalServiceInfoService.DeactivateMedicalServiceInfoAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اطلاعات خدمت پزشکی با موفقیت غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی اطلاعات خدمت پزشکی");
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
                var result = await _medicalServiceInfoService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, isFeatured ? "اطلاعات خدمت پزشکی به عنوان ویژه تنظیم شد" : "اطلاعات خدمت پزشکی از حالت ویژه خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی - MedicalServiceInfoId: {MedicalServiceInfoId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم وضعیت ویژه اطلاعات خدمت پزشکی");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت لیست خدمات برای dropdown
        /// </summary>
        private async Task<List<MedicalServiceInfoServiceViewModel>> GetServicesAsync()
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.ServiceCategory)
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .ToListAsync();

                return services
                    .OrderBy(s => s.ServiceCategory != null ? s.ServiceCategory.Title : "")
                    .ThenBy(s => s.Title)
                    .Select(s => new MedicalServiceInfoServiceViewModel
                    {
                        ServiceId = s.ServiceId,
                        ServiceTitle = (s.ServiceCategory != null ? s.ServiceCategory.Title + " - " : "") + s.Title + " (" + s.ServiceCode + ")",
                        ServiceCode = s.ServiceCode,
                        ServiceCategoryTitle = s.ServiceCategory != null ? s.ServiceCategory.Title : ""
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست خدمات");
                return new List<MedicalServiceInfoServiceViewModel>();
            }
        }

        #endregion

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر اطلاعات خدمت پزشکی
        /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP
        /// </summary>
        private async Task ProcessImageUpload(MedicalServiceInfoCreateEditViewModel model)
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
                        ServiceImageUploadPath,
                        ServiceThumbnailUploadPath,
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

                    // حذف تصویر قبلی در صورت وجود (فقط در Edit)
                    if (model.MedicalServiceInfoId > 0 && !string.IsNullOrEmpty(model.ImageUrl))
                    {
                        _imageUploadService.DeleteImage(model.ImageUrl);
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
                        ServiceThumbnailUploadPath,
                        ServiceThumbnailUploadPath,
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

                    // حذف thumbnail قبلی در صورت وجود (فقط در Edit)
                    if (model.MedicalServiceInfoId > 0 && !string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        _imageUploadService.DeleteImage(model.ThumbnailUrl);
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

