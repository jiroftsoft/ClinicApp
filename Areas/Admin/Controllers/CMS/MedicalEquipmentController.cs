using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;
using System.Collections.Generic;
using System.Web;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت تجهیزات پزشکی (Medical Equipment)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class MedicalEquipmentController : BaseCMSController
    {
        private readonly IMedicalEquipmentService _medicalEquipmentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string EquipmentImageUploadPath = "~/Content/Images/equipment";
        private const string EquipmentThumbnailUploadPath = "~/Content/Images/equipment/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public MedicalEquipmentController(
            IMedicalEquipmentService medicalEquipmentService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            IImageUploadService imageUploadService)
        {
            _medicalEquipmentService = medicalEquipmentService ?? throw new ArgumentNullException(nameof(medicalEquipmentService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<MedicalEquipmentController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(MedicalEquipmentSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست تجهیزات پزشکی توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new MedicalEquipmentSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _medicalEquipmentService.GetMedicalEquipmentsAsync(searchModel);

                // بارگذاری دسته‌بندی‌ها برای فیلتر
                var categories = await GetCategoriesAsync();

                // بارگذاری وضعیت‌ها برای فیلتر
                var statuses = GetStatuses();

                var viewModel = new MedicalEquipmentAdminIndexViewModel
                {
                    MedicalEquipments = result.Success && result.Data != null
                        ? result.Data
                        : new PagedResult<MedicalEquipmentIndexViewModel>(new List<MedicalEquipmentIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize),
                    Categories = categories,
                    Statuses = statuses
                };

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست تجهیزات پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تجهیزات پزشکی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست تجهیزات پزشکی");
                return View(GetViewPath("Index"), new MedicalEquipmentAdminIndexViewModel
                {
                    MedicalEquipments = new PagedResult<MedicalEquipmentIndexViewModel>(new List<MedicalEquipmentIndexViewModel>(), 0, 1, 10),
                    Categories = new List<MedicalEquipmentCategoryViewModel>(),
                    Statuses = GetStatuses()
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
                var result = await _medicalEquipmentService.GetMedicalEquipmentDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات تجهیز پزشکی");
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
                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();

                var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                {
                    Model = new MedicalEquipmentCreateEditViewModel
                    {
                        IsActive = true,
                        IsFeatured = false,
                        DisplayOrder = 0,
                        Status = "Active"
                    },
                    Categories = categories,
                    Statuses = statuses
                };

                return View(GetViewPath("Create"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد تجهیز پزشکی");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد تجهیز پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in TechnicalSpecifications field
        public async Task<ActionResult> Create(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد تجهیز پزشکی جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // Parse تاریخ‌ها از hidden input
                model.PurchaseDate = this.ParseDateFromHiddenInput("PurchaseDate", _logger);
                model.InstallationDate = this.ParseDateFromHiddenInput("InstallationDate", _logger);
                model.WarrantyExpiryDate = this.ParseDateFromHiddenInput("WarrantyExpiryDate", _logger);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();

                if (!ModelState.IsValid)
                {
                    var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                    {
                        Model = model,
                        Categories = categories,
                        Statuses = statuses
                    };
                    return View(GetViewPath("Create"), pageViewModel);
                }

                var result = await _medicalEquipmentService.CreateMedicalEquipmentAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد تجهیز پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    
                    var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                    {
                        Model = model,
                        Categories = categories,
                        Statuses = statuses
                    };
                    return View(GetViewPath("Create"), pageViewModel);
                }

                _logger.Information("تجهیز پزشکی با موفقیت ایجاد شد - MedicalEquipmentId: {MedicalEquipmentId}", result.Data.MedicalEquipmentId);
                NotificationHelper.SetSuccess(TempData, "تجهیز پزشکی با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تجهیز پزشکی");
                NotificationHelper.SetError(TempData, "خطا در ایجاد تجهیز پزشکی");
                
                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();
                var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                {
                    Model = model,
                    Categories = categories,
                    Statuses = statuses
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
                var result = await _medicalEquipmentService.GetMedicalEquipmentForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();

                var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                {
                    Model = result.Data,
                    Categories = categories,
                    Statuses = statuses
                };

                return View(GetViewPath("Edit"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش تجهیز پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in TechnicalSpecifications field
        public async Task<ActionResult> Edit(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);

                // Parse تاریخ‌ها از hidden input
                model.PurchaseDate = this.ParseDateFromHiddenInput("PurchaseDate", _logger);
                model.InstallationDate = this.ParseDateFromHiddenInput("InstallationDate", _logger);
                model.WarrantyExpiryDate = this.ParseDateFromHiddenInput("WarrantyExpiryDate", _logger);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();

                if (!ModelState.IsValid)
                {
                    var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                    {
                        Model = model,
                        Categories = categories,
                        Statuses = statuses
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                var result = await _medicalEquipmentService.UpdateMedicalEquipmentAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی تجهیز پزشکی: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    
                    var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                    {
                        Model = model,
                        Categories = categories,
                        Statuses = statuses
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                _logger.Information("تجهیز پزشکی با موفقیت به‌روزرسانی شد - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);
                NotificationHelper.SetSuccess(TempData, "تجهیز پزشکی با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", model.MedicalEquipmentId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی تجهیز پزشکی");
                
                var categories = await GetCategoriesAsync();
                var statuses = GetStatuses();
                var pageViewModel = new MedicalEquipmentCreateEditPageViewModel
                {
                    Model = model,
                    Categories = categories,
                    Statuses = statuses
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
                _logger.Information("درخواست حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);

                var result = await _medicalEquipmentService.DeleteMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "تجهیز پزشکی با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف تجهیز پزشکی");
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
                var result = await _medicalEquipmentService.ActivateMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "تجهیز پزشکی با موفقیت فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی تجهیز پزشکی");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _medicalEquipmentService.DeactivateMedicalEquipmentAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "تجهیز پزشکی با موفقیت غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی تجهیز پزشکی");
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
                var result = await _medicalEquipmentService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, isFeatured ? "تجهیز پزشکی به عنوان ویژه تنظیم شد" : "تجهیز پزشکی از حالت ویژه خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه تجهیز پزشکی - MedicalEquipmentId: {MedicalEquipmentId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم وضعیت ویژه تجهیز پزشکی");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        private string GetCategoryDisplayName(string category)
        {
            return category switch
            {
                "Imaging" => "تصویربرداری",
                "Laboratory" => "آزمایشگاه",
                "Surgery" => "جراحی",
                "Monitoring" => "مانیتورینگ",
                "Therapy" => "درمانی",
                "Diagnostic" => "تشخیصی",
                "Emergency" => "اورژانس",
                "Rehabilitation" => "توانبخشی",
                _ => category ?? "عمومی"
            };
        }

        private async Task<List<MedicalEquipmentCategoryViewModel>> GetCategoriesAsync()
        {
            var categories = await _context.Set<MedicalEquipment>()
                .Where(e => !e.IsDeleted)
                .Select(e => e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var categoriesList = categories.Select(c => new MedicalEquipmentCategoryViewModel
            {
                Category = c,
                DisplayName = GetCategoryDisplayName(c)
            }).ToList();

            // اگر دسته‌بندی‌ای وجود ندارد، لیست پیش‌فرض
            if (!categoriesList.Any())
            {
                categoriesList = new List<MedicalEquipmentCategoryViewModel>
                {
                    new MedicalEquipmentCategoryViewModel { Category = "Imaging", DisplayName = "تصویربرداری" },
                    new MedicalEquipmentCategoryViewModel { Category = "Laboratory", DisplayName = "آزمایشگاه" },
                    new MedicalEquipmentCategoryViewModel { Category = "Surgery", DisplayName = "جراحی" },
                    new MedicalEquipmentCategoryViewModel { Category = "Monitoring", DisplayName = "مانیتورینگ" },
                    new MedicalEquipmentCategoryViewModel { Category = "Therapy", DisplayName = "درمانی" },
                    new MedicalEquipmentCategoryViewModel { Category = "Diagnostic", DisplayName = "تشخیصی" },
                    new MedicalEquipmentCategoryViewModel { Category = "Emergency", DisplayName = "اورژانس" },
                    new MedicalEquipmentCategoryViewModel { Category = "Rehabilitation", DisplayName = "توانبخشی" }
                };
            }

            return categoriesList;
        }

        private List<MedicalEquipmentStatusViewModel> GetStatuses()
        {
            return new List<MedicalEquipmentStatusViewModel>
            {
                new MedicalEquipmentStatusViewModel { Status = "Active", DisplayName = "فعال" },
                new MedicalEquipmentStatusViewModel { Status = "Maintenance", DisplayName = "تعمیر" },
                new MedicalEquipmentStatusViewModel { Status = "Inactive", DisplayName = "غیرفعال" }
            };
        }

        #endregion

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر تجهیز پزشکی
        /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP
        /// </summary>
        private async Task ProcessImageUpload(MedicalEquipmentCreateEditViewModel model)
        {
            try
            {
                var imageFile = Request.Files["ImageFile"];

                // اگر تصویر اصلی آپلود شده
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        imageFile,
                        EquipmentImageUploadPath,
                        EquipmentThumbnailUploadPath,
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
                    if (model.MedicalEquipmentId > 0 && !string.IsNullOrEmpty(model.ImageUrl))
                    {
                        _imageUploadService.DeleteImage(model.ImageUrl);
                    }

                    // تنظیم مسیر تصویر
                    model.ImageUrl = uploadResult.Data.ImageUrl;

                    _logger.Information("تصویر با موفقیت آپلود شد: {ImageUrl}", model.ImageUrl);
                }

                // پردازش تصاویر اضافی (Multiple Files)
                var existingImages = new List<string>();
                
                // خواندن تصاویر موجود از hidden field (که توسط JavaScript به‌روزرسانی شده)
                if (!string.IsNullOrEmpty(model.ImageUrls))
                {
                    try
                    {
                        existingImages = System.Web.Helpers.Json.Decode<List<string>>(model.ImageUrls) ?? new List<string>();
                        _logger.Debug("تصاویر موجود از hidden field: {Count} تصویر", existingImages.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در decode کردن JSON تصاویر موجود");
                        existingImages = new List<string>();
                    }
                }
                
                // جمع‌آوری تمام فایل‌های AdditionalImages
                // در ASP.NET MVC، وقتی multiple file input داریم، باید از AllKeys استفاده کنیم
                var uploadedImageUrls = new List<string>();
                var additionalImageFiles = new List<HttpPostedFileBase>();
                
                // پیدا کردن تمام فایل‌هایی که نامشان "AdditionalImages" است (با یا بدون index)
                foreach (string key in Request.Files.AllKeys)
                {
                    if (key != null && (key.StartsWith("AdditionalImages[", StringComparison.OrdinalIgnoreCase) || 
                        key.Equals("AdditionalImages", StringComparison.OrdinalIgnoreCase)))
                    {
                        var file = Request.Files[key];
                        if (file != null && file.ContentLength > 0)
                        {
                            additionalImageFiles.Add(file);
                            _logger.Debug("فایل AdditionalImages پیدا شد: {Key}, Size: {Size}", key, file.ContentLength);
                        }
                    }
                }
                
                // اگر فایل جدیدی آپلود شده
                if (additionalImageFiles.Any())
                {
                    for (int i = 0; i < additionalImageFiles.Count; i++)
                    {
                        var file = additionalImageFiles[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                                file,
                                EquipmentImageUploadPath,
                                EquipmentThumbnailUploadPath,
                                ThumbnailWidth,
                                ThumbnailHeight,
                                MaxImageWidth,
                                MaxImageHeight);

                            if (uploadResult.Success)
                            {
                                uploadedImageUrls.Add(uploadResult.Data.ImageUrl);
                                _logger.Information("تصویر اضافی با موفقیت آپلود شد: {ImageUrl}", uploadResult.Data.ImageUrl);
                            }
                            else
                            {
                                _logger.Warning("خطا در آپلود تصویر اضافی {Index}: {ErrorMessage}", i, uploadResult.Message);
                            }
                        }
                    }

                    // ترکیب تصاویر موجود و جدید
                    if (uploadedImageUrls.Any())
                    {
                        var allImages = existingImages.Union(uploadedImageUrls).ToList();
                        var jsonArray = "[" + string.Join(",", allImages.Select(url => "\"" + url + "\"")) + "]";
                        model.ImageUrls = jsonArray;
                        _logger.Information("تعداد {Count} تصویر جدید آپلود شد. کل تصاویر: {Total}", uploadedImageUrls.Count, allImages.Count);
                    }
                    else if (existingImages.Any())
                    {
                        // اگر فایل جدیدی آپلود نشد اما تصاویر موجود داریم، آن‌ها را حفظ کن
                        var jsonArray = "[" + string.Join(",", existingImages.Select(url => "\"" + url + "\"")) + "]";
                        model.ImageUrls = jsonArray;
                        _logger.Debug("هیچ فایل جدیدی آپلود نشد. تصاویر موجود حفظ شدند: {Count} تصویر", existingImages.Count);
                    }
                }
                else if (existingImages.Any())
                {
                    // اگر هیچ فایل جدیدی آپلود نشد اما تصاویر موجود داریم، آن‌ها را حفظ کن
                    var jsonArray = "[" + string.Join(",", existingImages.Select(url => "\"" + url + "\"")) + "]";
                    model.ImageUrls = jsonArray;
                    _logger.Debug("هیچ فایل جدیدی آپلود نشد. تصاویر موجود حفظ شدند: {Count} تصویر", existingImages.Count);
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
