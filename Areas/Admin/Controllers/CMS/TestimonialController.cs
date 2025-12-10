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
    /// کنترلر مدیریت نظرات بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class TestimonialController : Controller
    {
        private readonly ITestimonialService _testimonialService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public TestimonialController(
            ITestimonialService testimonialService,
            ICurrentUserService currentUserService)
        {
            _testimonialService = testimonialService ?? throw new ArgumentNullException(nameof(testimonialService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<TestimonialController>();
        }

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(bool includePending = false)
        {
            try
            {
                var result = await _testimonialService.GetTestimonialsAsync(includePending);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new System.Collections.Generic.List<TestimonialIndexViewModel>());
                }

                ViewBag.IncludePending = includePending;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست نظرات");
                TempData["Error"] = "خطا در بارگذاری لیست نظرات";
                return View(new System.Collections.Generic.List<TestimonialIndexViewModel>());
            }
        }

        [HttpGet]
        public async Task<ActionResult> Pending()
        {
            try
            {
                var result = await _testimonialService.GetPendingApprovalAsync();
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View("Index", new System.Collections.Generic.List<TestimonialIndexViewModel>());
                }

                ViewBag.IncludePending = true;
                ViewBag.IsPendingPage = true;
                return View("Index", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نظرات در انتظار تایید");
                TempData["Error"] = "خطا در بارگذاری نظرات در انتظار تایید";
                return View("Index", new System.Collections.Generic.List<TestimonialIndexViewModel>());
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _testimonialService.GetTestimonialDetailsAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new TestimonialCreateEditViewModel
                {
                    IsApproved = false,
                    IsFeatured = false,
                    DisplayOrder = 0,
                    Rating = 5
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد نظر");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TestimonialCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _testimonialService.CreateTestimonialAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "نظر با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نظر");
                TempData["Error"] = "خطا در ایجاد نظر";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _testimonialService.GetTestimonialForEditAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TestimonialCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = await _testimonialService.UpdateTestimonialAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "نظر با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نظر - TestimonialId: {TestimonialId}", model.TestimonialId);
                TempData["Error"] = "خطا در به‌روزرسانی نظر";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _testimonialService.DeleteTestimonialAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نظر با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در حذف نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(int id)
        {
            try
            {
                var result = await _testimonialService.ApproveTestimonialAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نظر با موفقیت تایید شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در تایید نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reject(int id)
        {
            try
            {
                var result = await _testimonialService.RejectTestimonialAsync(id);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "نظر با موفقیت رد شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رد نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در رد نظر";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetFeatured(int id, bool isFeatured)
        {
            try
            {
                var result = await _testimonialService.SetFeaturedAsync(id, isFeatured);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isFeatured ? "نظر به عنوان ویژه تنظیم شد" : "نظر از حالت ویژه خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نظر - TestimonialId: {TestimonialId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت ویژه نظر";
                return RedirectToAction("Index");
            }
        }
    }
}

