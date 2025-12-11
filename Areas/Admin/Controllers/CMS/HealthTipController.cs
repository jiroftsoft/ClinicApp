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
    /// کنترلر مدیریت نکات سلامت (Health Tips)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class HealthTipController : BaseCMSController
    {
        private readonly IHealthTipService _healthTipService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public HealthTipController(
            IHealthTipService healthTipService,
            ICurrentUserService currentUserService)
        {
            _healthTipService = healthTipService ?? throw new ArgumentNullException(nameof(healthTipService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<HealthTipIndexViewModel>(new System.Collections.Generic.List<HealthTipIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری دسته‌بندی‌ها برای فیلتر
                var categoriesResult = await _healthTipService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<HealthTipCategoryViewModel>();

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست نکات سلامت");
                TempData["Error"] = "خطا در بارگذاری لیست نکات سلامت";
                return View(new PagedResult<HealthTipIndexViewModel>(new System.Collections.Generic.List<HealthTipIndexViewModel>(), 0, 1, 10));
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
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات نکته سلامت";
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

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد نکته سلامت");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد نکته سلامت";
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

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _healthTipService.CreateHealthTipAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد نکته سلامت: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("نکته سلامت با موفقیت ایجاد شد - HealthTipId: {HealthTipId}", result.Data.HealthTipId);
                TempData["Success"] = "نکته سلامت با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نکته سلامت");
                TempData["Error"] = "خطا در ایجاد نکته سلامت";
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
                var result = await _healthTipService.GetHealthTipForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش نکته سلامت";
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

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _healthTipService.UpdateHealthTipAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی نکته سلامت: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("نکته سلامت با موفقیت به‌روزرسانی شد - HealthTipId: {HealthTipId}", model.HealthTipId);
                TempData["Success"] = "نکته سلامت با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نکته سلامت - HealthTipId: {HealthTipId}", model.HealthTipId);
                TempData["Error"] = "خطا در به‌روزرسانی نکته سلامت";
                return View(model);
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نکته سلامت با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در حذف نکته سلامت";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نکته سلامت با موفقیت منتشر شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتشار نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در انتشار نکته سلامت";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نکته سلامت از حالت انتشار خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو انتشار نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در لغو انتشار نکته سلامت";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "نکته سلامت به عنوان ویژه تنظیم شد" : "نکته سلامت از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نکته سلامت - HealthTipId: {HealthTipId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه نکته سلامت";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

