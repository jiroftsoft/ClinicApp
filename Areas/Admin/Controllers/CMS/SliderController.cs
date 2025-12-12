using System;
using System.Collections.Generic;
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
    /// کنترلر مدیریت اسلایدرها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class SliderController : BaseCMSController
    {
        private readonly ISliderService _sliderService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string SliderImageUploadPath = "~/Content/Images/sliders";
        private const string SliderThumbnailUploadPath = "~/Content/Images/sliders/thumbnails";
        private const int ThumbnailWidth = 400;
        private const int ThumbnailHeight = 250;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public SliderController(
            ISliderService sliderService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            IImageUploadService imageUploadService)
        {
            _sliderService = sliderService ?? throw new ArgumentNullException(nameof(sliderService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _logger = Log.ForContext<SliderController>();
        }

        #region Index

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(string position = null)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اسلایدرها توسط کاربر {UserId}", _currentUserService.UserId);

                var result = await _sliderService.GetSlidersAsync(position);
                
                var positions = GetPositions();
                
                var viewModel = new SliderAdminIndexViewModel
                {
                    Sliders = result.Success && result.Data != null ? result.Data : new List<SliderIndexViewModel>(),
                    SelectedPosition = position,
                    Positions = positions
                };

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اسلایدرها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return View(GetViewPath("Index"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اسلایدرها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست اسلایدرها");
                return View(GetViewPath("Index"), new SliderAdminIndexViewModel
                {
                    Sliders = new List<SliderIndexViewModel>(),
                    SelectedPosition = position,
                    Positions = GetPositions()
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
                var result = await _sliderService.GetSliderDetailsAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اسلایدر - SliderId: {SliderId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات اسلایدر");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create(string position = null)
        {
            try
            {
                var model = new SliderCreateEditViewModel
                {
                    IsActive = true,
                    DisplayOrder = 0,
                    Position = position ?? "hero"
                };

                var positions = GetPositions();
                
                var pageViewModel = new SliderCreateEditPageViewModel
                {
                    Model = model,
                    Positions = positions
                };

                return View(GetViewPath("Create"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اسلایدر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد اسلایدر");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SliderCreateEditViewModel model)
        {
            try
            {
                _logger.Information("شروع ایجاد اسلایدر - Title: {Title}, Position: {Position}", model.Title, model.Position);
                
                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    var errorCount = ModelState.Values.SelectMany(v => v.Errors).Count();
                    _logger.Warning("ModelState نامعتبر است. تعداد خطاها: {ErrorCount}", errorCount);
                    foreach (var error in ModelState)
                    {
                        foreach (var err in error.Value.Errors)
                        {
                            _logger.Warning("خطای ModelState - Key: {Key}, Error: {Error}", error.Key, err.ErrorMessage);
                        }
                    }
                    
                    var positions = GetPositions();
                    var pageViewModel = new SliderCreateEditPageViewModel
                    {
                        Model = model,
                        Positions = positions
                    };
                    NotificationHelper.SetError(TempData, "لطفاً خطاهای فرم را برطرف کنید.");
                    return View(GetViewPath("Create"), pageViewModel);
                }

                // Parse کردن تاریخ‌ها از hidden input
                ParseDateFromHiddenInput(model, "StartDate");
                ParseDateFromHiddenInput(model, "EndDate");

                var result = await _sliderService.CreateSliderAsync(model);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    var positions = GetPositions();
                    var pageViewModel = new SliderCreateEditPageViewModel
                    {
                        Model = model,
                        Positions = positions
                    };
                    return View(GetViewPath("Create"), pageViewModel);
                }

                NotificationHelper.SetSuccess(TempData, "اسلایدر با موفقیت ایجاد شد");
                return RedirectToAction("Index", new { position = model.Position });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اسلایدر");
                NotificationHelper.SetError(TempData, "خطا در ایجاد اسلایدر");
                var positions = GetPositions();
                var pageViewModel = new SliderCreateEditPageViewModel
                {
                    Model = model,
                    Positions = positions
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
                var result = await _sliderService.GetSliderForEditAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var positions = GetPositions();
                
                var pageViewModel = new SliderCreateEditPageViewModel
                {
                    Model = result.Data,
                    Positions = positions
                };

                return View(GetViewPath("Edit"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اسلایدر - SliderId: {SliderId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش اسلایدر");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SliderCreateEditViewModel model)
        {
            try
            {
                // پردازش آپلود تصویر
                await ProcessImageUpload(model);

                if (!ModelState.IsValid)
                {
                    var positions = GetPositions();
                    var pageViewModel = new SliderCreateEditPageViewModel
                    {
                        Model = model,
                        Positions = positions
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                // Parse کردن تاریخ‌ها از hidden input
                ParseDateFromHiddenInput(model, "StartDate");
                ParseDateFromHiddenInput(model, "EndDate");

                var result = await _sliderService.UpdateSliderAsync(model);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    var positions = GetPositions();
                    var pageViewModel = new SliderCreateEditPageViewModel
                    {
                        Model = model,
                        Positions = positions
                    };
                    return View(GetViewPath("Edit"), pageViewModel);
                }

                NotificationHelper.SetSuccess(TempData, "اسلایدر با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index", new { position = model.Position });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اسلایدر - SliderId: {SliderId}", model.SliderId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی اسلایدر");
                var positions = GetPositions();
                var pageViewModel = new SliderCreateEditPageViewModel
                {
                    Model = model,
                    Positions = positions
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
                var result = await _sliderService.DeleteSliderAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اسلایدر با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلایدر - SliderId: {SliderId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف اسلایدر");
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
                var result = await _sliderService.ActivateSliderAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اسلایدر با موفقیت فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اسلایدر - SliderId: {SliderId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی اسلایدر");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _sliderService.DeactivateSliderAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "اسلایدر با موفقیت غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اسلایدر - SliderId: {SliderId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی اسلایدر");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        private List<SliderPositionViewModel> GetPositions()
        {
            return new List<SliderPositionViewModel>
            {
                new SliderPositionViewModel { Value = "hero", DisplayName = "Hero (صفحه اصلی)" },
                new SliderPositionViewModel { Value = "sidebar", DisplayName = "Sidebar" },
                new SliderPositionViewModel { Value = "footer", DisplayName = "Footer" }
            };
        }

        /// <summary>
        /// پردازش آپلود تصویر اسلایدر
        /// طراحی شده برای محیط Production درمانی با رعایت اصول SRP
        /// </summary>
        private async Task ProcessImageUpload(SliderCreateEditViewModel model)
        {
            try
            {
                var imageFile = Request.Files["ImageFile"];
                var isEdit = model.SliderId > 0; // اگر SliderId > 0 باشد یعنی Edit است
                var hasUploadedFile = imageFile != null && imageFile.ContentLength > 0;
                var hasExistingImage = !string.IsNullOrEmpty(model.ImageUrl);

                // اگر تصویر اصلی آپلود شده
                if (hasUploadedFile)
                {
                    // اعتبارسنجی فایل
                    var validationResult = _imageUploadService.ValidateImageFile(imageFile);
                    if (!validationResult.Success)
                    {
                        _logger.Warning("خطا در اعتبارسنجی تصویر: {ErrorMessage}", validationResult.Message);
                        ModelState.AddModelError("ImageUrl", validationResult.Message);
                        return;
                    }

                    // حذف تصویر قدیمی در صورت وجود (فقط در Edit)
                    if (isEdit && hasExistingImage)
                    {
                        try
                        {
                            _imageUploadService.DeleteImage(model.ImageUrl, model.ThumbnailUrl);
                            _logger.Information("تصویر قدیمی حذف شد: {ImageUrl}", model.ImageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "خطا در حذف تصویر قدیمی: {ImageUrl}", model.ImageUrl);
                            // خطا در حذف تصویر قدیمی نباید باعث توقف فرآیند شود
                        }
                    }

                    // آپلود تصویر جدید
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        imageFile,
                        SliderImageUploadPath,
                        SliderThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                        ModelState.AddModelError("ImageUrl", uploadResult.Message);
                        return;
                    }

                    // به‌روزرسانی مسیر تصاویر
                    model.ImageUrl = uploadResult.Data.ImageUrl;
                    model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                    _logger.Information("تصویر با موفقیت آپلود شد: {ImageUrl}", model.ImageUrl);
                }
                else
                {
                    // اگر فایل جدیدی آپلود نشده
                    if (!isEdit)
                    {
                        // در Create: تصویر الزامی است
                        ModelState.AddModelError("ImageUrl", "تصویر الزامی است. لطفاً یک تصویر انتخاب کنید.");
                        _logger.Warning("در Create، تصویر آپلود نشده است");
                    }
                    else
                    {
                        // در Edit: اگر تصویر قدیمی وجود دارد، مشکلی نیست
                        // اگر تصویر قدیمی وجود ندارد و فایل جدید هم آپلود نشده، خطا
                        if (!hasExistingImage)
                        {
                            ModelState.AddModelError("ImageUrl", "تصویر الزامی است. لطفاً یک تصویر انتخاب کنید یا تصویر قبلی را حفظ کنید.");
                            _logger.Warning("در Edit، تصویر قدیمی وجود ندارد و فایل جدید هم آپلود نشده است - SliderId: {SliderId}", model.SliderId);
                        }
                        else
                        {
                            _logger.Information("در Edit، تصویر قدیمی حفظ می‌شود: {ImageUrl}", model.ImageUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر اسلایدر - SliderId: {SliderId}", model.SliderId);
                ModelState.AddModelError("ImageUrl", "خطا در پردازش تصویر. لطفاً دوباره تلاش کنید.");
            }
        }

        private void ParseDateFromHiddenInput(SliderCreateEditViewModel model, string propertyName)
        {
            try
            {
                var hiddenInputName = $"{propertyName}Persian";
                var persianDateValue = Request.Form[hiddenInputName];
                
                if (!string.IsNullOrEmpty(persianDateValue))
                {
                    var dateValue = PersianDateHelper.ParsePersianDate(persianDateValue);
                    if (dateValue.HasValue)
                    {
                        var property = typeof(SliderCreateEditViewModel).GetProperty(propertyName);
                        if (property != null && property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(model, dateValue.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در Parse کردن تاریخ {PropertyName}", propertyName);
            }
        }

        #endregion
    }
}

