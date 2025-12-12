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
    public class TestimonialController : BaseCMSController
    {
        private readonly ITestimonialService _testimonialService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string TestimonialImageUploadPath = "~/Content/Images/testimonials";
        private const string TestimonialThumbnailUploadPath = "~/Content/Images/testimonials/thumbnails";
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private const int MaxImageWidth = 1920; // Full HD
        private const int MaxImageHeight = 1080; // Full HD

        public TestimonialController(
            ITestimonialService testimonialService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService)
        {
            _testimonialService = testimonialService ?? throw new ArgumentNullException(nameof(testimonialService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<TestimonialIndexViewModel>());
                }

                ViewBag.IncludePending = includePending;
                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست نظرات");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست نظرات");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<TestimonialIndexViewModel>());
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<TestimonialIndexViewModel>());
                }

                ViewBag.IncludePending = true;
                ViewBag.IsPendingPage = true;
                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش نظرات در انتظار تایید");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری نظرات در انتظار تایید");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<TestimonialIndexViewModel>());
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات نظر");
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

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد نظر");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد نظر");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TestimonialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد نظر جدید");

                // پردازش آپلود تصویر
                await ProcessImageUpload(model, isEdit: false);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _testimonialService.CreateTestimonialAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد نظر: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("نظر با موفقیت ایجاد شد - TestimonialId: {TestimonialId}", result.Data?.TestimonialId ?? 0);
                NotificationHelper.SetSuccess(TempData, "نظر با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد نظر");
                NotificationHelper.SetError(TempData, "خطا در ایجاد نظر");
                return View(GetViewPath("Create"), model);
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
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش نظر");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TestimonialCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی نظر - TestimonialId: {TestimonialId}", model.TestimonialId);

                // پردازش آپلود تصویر
                await ProcessImageUpload(model, isEdit: true);

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _testimonialService.UpdateTestimonialAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی نظر: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("نظر با موفقیت به‌روزرسانی شد - TestimonialId: {TestimonialId}", model.TestimonialId);
                NotificationHelper.SetSuccess(TempData, "نظر با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی نظر - TestimonialId: {TestimonialId}", model.TestimonialId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی نظر");
                return View(GetViewPath("Edit"), model);
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نظر با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف نظر");
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نظر با موفقیت تایید شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تایید نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در تایید نظر");
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "نظر با موفقیت رد شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در رد نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در رد نظر");
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
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, isFeatured ? "نظر به عنوان ویژه تنظیم شد" : "نظر از حالت ویژه خارج شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت ویژه نظر - TestimonialId: {TestimonialId}", id);
                NotificationHelper.SetError(TempData, "خطا در تنظیم وضعیت ویژه نظر");
                return RedirectToAction("Index");
            }
        }

        #region Image Upload

        /// <summary>
        /// پردازش آپلود تصویر نظر
        /// </summary>
        private async Task ProcessImageUpload(TestimonialCreateEditViewModel model, bool isEdit = false)
        {
            try
            {
                var photoFile = Request.Files["PhotoFile"];

                // اگر تصویر آپلود شده
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    // حذف تصویر قبلی در صورت ویرایش
                    if (isEdit && !string.IsNullOrEmpty(model.PhotoUrl))
                    {
                        var deleteResult = _imageUploadService.DeleteImage(model.PhotoUrl);
                        if (deleteResult.Success)
                        {
                            _logger.Information("تصویر قبلی حذف شد: {PhotoUrl}", model.PhotoUrl);
                        }
                    }

                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        photoFile,
                        TestimonialImageUploadPath,
                        TestimonialThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("PhotoFile", uploadResult.Message);
                        return;
                    }

                    // تنظیم مسیر تصویر
                    model.PhotoUrl = uploadResult.Data.ImageUrl;

                    _logger.Information("تصویر با موفقیت آپلود شد: {PhotoUrl}", model.PhotoUrl);
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

