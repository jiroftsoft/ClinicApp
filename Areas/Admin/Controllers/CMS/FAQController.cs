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
    /// کنترلر مدیریت سوالات متداول (FAQ)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class FAQController : Controller
    {
        private readonly IFAQService _faqService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public FAQController(
            IFAQService faqService,
            ICurrentUserService currentUserService)
        {
            _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<FAQController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(FAQSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست FAQ توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new FAQSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _faqService.GetFAQsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست FAQ: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<FAQIndexViewModel>(new System.Collections.Generic.List<FAQIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری دسته‌بندی‌ها برای فیلتر
                var categoriesResult = await _faqService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<FAQCategoryViewModel>();

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست FAQ");
                TempData["Error"] = "خطا در بارگذاری لیست FAQ";
                return View(new PagedResult<FAQIndexViewModel>(new System.Collections.Generic.List<FAQIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _faqService.GetFAQDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات FAQ";
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
                var model = new FAQCreateEditViewModel
                {
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    Category = "general"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد FAQ");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد FAQ";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Answer field
        public async Task<ActionResult> Create(FAQCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد FAQ جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _faqService.CreateFAQAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد FAQ: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("FAQ با موفقیت ایجاد شد - FAQId: {FAQId}", result.Data.FAQId);
                TempData["Success"] = "FAQ با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد FAQ");
                TempData["Error"] = "خطا در ایجاد FAQ";
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
                var result = await _faqService.GetFAQForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش FAQ";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Answer field
        public async Task<ActionResult> Edit(FAQCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی FAQ - FAQId: {FAQId}", model.FAQId);

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _faqService.UpdateFAQAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی FAQ: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("FAQ با موفقیت به‌روزرسانی شد - FAQId: {FAQId}", model.FAQId);
                TempData["Success"] = "FAQ با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی FAQ - FAQId: {FAQId}", model.FAQId);
                TempData["Error"] = "خطا در به‌روزرسانی FAQ";
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
                _logger.Information("درخواست حذف FAQ - FAQId: {FAQId}", id);

                var result = await _faqService.DeleteFAQAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "FAQ با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در حذف FAQ";
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
                var result = await _faqService.ActivateFAQAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "FAQ با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در فعال‌سازی FAQ";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _faqService.DeactivateFAQAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "FAQ با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی FAQ";
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
                var result = await _faqService.SetFeaturedAsync(id, isFeatured);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "FAQ به عنوان ویژه تنظیم شد" : "FAQ از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه FAQ - FAQId: {FAQId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه FAQ";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

