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
    /// کنترلر مدیریت اسلایدرها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ISliderService _sliderService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public SliderController(
            ISliderService sliderService,
            ICurrentUserService currentUserService)
        {
            _sliderService = sliderService ?? throw new ArgumentNullException(nameof(sliderService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<SliderController>();
        }

        #region Index

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(string position = null)
        {
            try
            {
                var result = await _sliderService.GetSlidersAsync(position);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new System.Collections.Generic.List<SliderIndexViewModel>());
                }

                ViewBag.Position = position;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اسلایدرها");
                TempData["Error"] = "خطا در بارگذاری لیست اسلایدرها";
                return View(new System.Collections.Generic.List<SliderIndexViewModel>());
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
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اسلایدر - SliderId: {SliderId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات اسلایدر";
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

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد اسلایدر");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد اسلایدر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SliderCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _sliderService.CreateSliderAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اسلایدر با موفقیت ایجاد شد";
                return RedirectToAction("Index", new { position = model.Position });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد اسلایدر");
                TempData["Error"] = "خطا در ایجاد اسلایدر";
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
                var result = await _sliderService.GetSliderForEditAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اسلایدر - SliderId: {SliderId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش اسلایدر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SliderCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _sliderService.UpdateSliderAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اسلایدر با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index", new { position = model.Position });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی اسلایدر - SliderId: {SliderId}", model.SliderId);
                TempData["Error"] = "خطا در به‌روزرسانی اسلایدر";
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
                var result = await _sliderService.DeleteSliderAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اسلایدر با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف اسلایدر - SliderId: {SliderId}", id);
                TempData["Error"] = "خطا در حذف اسلایدر";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اسلایدر با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی اسلایدر - SliderId: {SliderId}", id);
                TempData["Error"] = "خطا در فعال‌سازی اسلایدر";
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
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "اسلایدر با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی اسلایدر - SliderId: {SliderId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی اسلایدر";
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

