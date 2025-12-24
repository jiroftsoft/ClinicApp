using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت Story
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class StoryController : BaseCMSController
    {
        private readonly IStoryService _storyService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IVideoUploadService _videoUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string StoryImageUploadPath = "~/Content/Images/stories";
        private const string StoryThumbnailUploadPath = "~/Content/Images/stories/thumbnails";
        private const string StoryVideoUploadPath = "~/Content/Videos/stories";
        private const int ThumbnailWidth = 400;
        private const int ThumbnailHeight = 400;
        private const int MaxImageWidth = 1920;
        private const int MaxImageHeight = 1080;
        private const int MaxVideoSizeInMB = 100;

        public StoryController(
            IStoryService storyService,
            ICurrentUserService currentUserService,
            IImageUploadService imageUploadService,
            IVideoUploadService videoUploadService)
        {
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
            _videoUploadService = videoUploadService ?? throw new ArgumentNullException(nameof(videoUploadService));
            _logger = Log.ForContext<StoryController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(bool includeInactive = false)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست Story ها توسط کاربر {UserId}", _currentUserService.UserId);

                var result = await _storyService.GetStoriesAsync(includeInactive);
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست Story ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<StoryIndexViewModel>());
                }

                ViewBag.IncludeInactive = includeInactive;
                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست Story ها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست Story ها");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<StoryIndexViewModel>());
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات Story - StoryId: {StoryId}", id);

                var result = await _storyService.GetStoryByIdAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات Story - StoryId: {StoryId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات Story");
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
                var model = new StoryCreateEditViewModel
                {
                    IsActive = true,
                    DisplayOrder = 0,
                    VideoType = "DirectUpload"
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد Story");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد Story");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(StoryCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد Story جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // لاگ برای دیباگ: بررسی Request
                _logger.Information("Create Action - Request.ContentType: {ContentType}, Request.Files.Count: {FileCount}",
                    Request.ContentType ?? "null", Request.Files.Count);
                
                foreach (string key in Request.Files.AllKeys)
                {
                    var file = Request.Files[key];
                    _logger.Information("Create Action - فایل پیدا شد - Key: {Key}, FileName: {FileName}, ContentLength: {ContentLength}",
                        key, file?.FileName ?? "null", file?.ContentLength ?? 0);
                }

                // پردازش آپلود تصویر Thumbnail (طبق قرارداد - استفاده از ProcessImageUpload)
                await ProcessImageUpload(model, isCreate: true);

                // پردازش آپلود ویدیو (اگر DirectUpload)
                if (model.VideoType == "DirectUpload")
                {
                    await ProcessVideoUpload(model);
                }
                else
                {
                    // پردازش Video URL (برای YouTube, Vimeo)
                    ProcessVideoUrl(model);
                }

                // تبدیل تاریخ‌های شمسی به میلادی
                model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);

                // حذف خطاهای validation برای تاریخ‌ها
                ModelState.Remove("StartDate");
                ModelState.Remove("EndDate");

                if (!ModelState.IsValid)
                {
                    // نمایش خطاهای ModelState با Toastr
                    this.AddModelStateErrorsToNotification(_logger);
                    return View(GetViewPath("Create"), model);
                }

                var result = await _storyService.CreateStoryAsync(model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد Story: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("Story با موفقیت ایجاد شد - StoryId: {StoryId}", result.Data.StoryId);
                NotificationHelper.SetSuccess(TempData, "Story با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Story");
                NotificationHelper.SetError(TempData, "خطا در ایجاد Story");
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
                _logger.Information("درخواست نمایش فرم ویرایش Story - StoryId: {StoryId}", id);

                var result = await _storyService.GetStoryByIdAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                // تبدیل به CreateEditViewModel
                var editModel = new StoryCreateEditViewModel
                {
                    StoryId = result.Data.StoryId,
                    Title = result.Data.Title,
                    Description = result.Data.Description,
                    VideoUrl = result.Data.VideoUrl,
                    VideoType = result.Data.VideoType,
                    ThumbnailUrl = result.Data.ThumbnailUrl,
                    LinkUrl = result.Data.LinkUrl,
                    ButtonText = result.Data.ButtonText,
                    IsActive = result.Data.IsActive,
                    DisplayOrder = result.Data.DisplayOrder,
                    StartDate = result.Data.StartDate,
                    EndDate = result.Data.EndDate,
                    Duration = result.Data.Duration
                };

                return View(GetViewPath("Edit"), editModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش Story - StoryId: {StoryId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش Story");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StoryCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی Story - StoryId: {StoryId}", model.StoryId);

                // دریافت Story موجود برای بررسی ThumbnailUrl قبلی
                var existingStory = await _storyService.GetStoryByIdAsync(model.StoryId);
                if (!existingStory.Success)
                {
                    NotificationHelper.SetError(TempData, "Story یافت نشد");
                    return RedirectToAction("Index");
                }

                // پردازش آپلود تصویر Thumbnail (طبق قرارداد - استفاده از ProcessImageUpload)
                // در Edit، ThumbnailFile اختیاری است (اگر ThumbnailUrl قبلاً وجود دارد)
                // ✅ استفاده از model.ThumbnailFile (از Model Binding) یا Request.Files (fallback)
                var thumbnailFile = model.ThumbnailFile ?? Request.Files["ThumbnailFile"];
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    // حذف تصویر قبلی
                    if (!string.IsNullOrEmpty(existingStory.Data.ThumbnailUrl))
                    {
                        _imageUploadService.DeleteImage(existingStory.Data.ThumbnailUrl);
                    }
                    await ProcessImageUpload(model, isCreate: false);
                }
                else
                {
                    // اگر فایل جدید انتخاب نشده، ThumbnailUrl قبلی را حفظ کن
                    if (!string.IsNullOrEmpty(existingStory.Data.ThumbnailUrl))
                    {
                        model.ThumbnailUrl = existingStory.Data.ThumbnailUrl;
                    }
                }

                // پردازش آپلود ویدیو (اگر DirectUpload و فایل جدید انتخاب شده)
                var videoFile = Request.Files["VideoFile"];
                if (model.VideoType == "DirectUpload" && videoFile != null && videoFile.ContentLength > 0)
                {
                    // حذف ویدیو قدیمی
                    if (!string.IsNullOrEmpty(existingStory.Data.VideoUrl))
                    {
                        _videoUploadService.DeleteVideo(existingStory.Data.VideoUrl);
                    }
                    await ProcessVideoUpload(model);
                }
                else if (model.VideoType != "DirectUpload")
                {
                    // پردازش Video URL (برای YouTube, Vimeo)
                    ProcessVideoUrl(model);
                }

                // تبدیل تاریخ‌های شمسی به میلادی
                model.StartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                model.EndDate = this.ParseDateFromHiddenInput("EndDate", _logger);

                // حذف خطاهای validation برای تاریخ‌ها
                ModelState.Remove("StartDate");
                ModelState.Remove("EndDate");

                if (!ModelState.IsValid)
                {
                    // نمایش خطاهای ModelState با Toastr
                    this.AddModelStateErrorsToNotification(_logger);
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _storyService.UpdateStoryAsync(model.StoryId, model);
                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی Story: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("Story با موفقیت به‌روزرسانی شد - StoryId: {StoryId}", model.StoryId);
                NotificationHelper.SetSuccess(TempData, "Story با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Story - StoryId: {StoryId}", model.StoryId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی Story");
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
                _logger.Information("درخواست حذف Story - StoryId: {StoryId}", id);

                var result = await _storyService.DeleteStoryAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "Story با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Story - StoryId: {StoryId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف Story");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Increment View Count (AJAX)

        /// <summary>
        /// افزایش تعداد بازدید Story (برای AJAX)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> IncrementViewCount(int id)
        {
            try
            {
                var result = await _storyService.IncrementViewCountAsync(id);
                if (result.Success)
                {
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید Story - StoryId: {StoryId}", id);
                return Json(new { success = false, message = "خطا در افزایش تعداد بازدید" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        #region Image & Video Upload Processing

        /// <summary>
        /// پردازش آپلود تصویر Thumbnail
        /// طبق قرارداد: استفاده از ProcessImageUpload مطابق الگوی سایر Controllerها
        /// </summary>
        private async Task ProcessImageUpload(StoryCreateEditViewModel model, bool isCreate = false)
        {
            try
            {
                // ✅ استفاده از model.ThumbnailFile (از Model Binding) یا Request.Files (fallback)
                // طبق قرارداد: اولویت با Model Binding است، اما fallback به Request.Files برای سازگاری
                var thumbnailFile = model.ThumbnailFile ?? Request.Files["ThumbnailFile"];

                // لاگ برای دیباگ
                _logger.Information("ProcessImageUpload - ThumbnailFile: {ThumbnailFile}, isCreate: {IsCreate}",
                    thumbnailFile != null 
                        ? $"نام: {thumbnailFile.FileName}, اندازه: {thumbnailFile.ContentLength}, نوع: {thumbnailFile.ContentType}" 
                        : "null", isCreate);

                // در Create، ThumbnailFile الزامی است
                if (isCreate)
                {
                    if (thumbnailFile == null)
                    {
                        _logger.Warning("ProcessImageUpload - ThumbnailFile null است در Create");
                        ModelState.AddModelError("ThumbnailFile", "تصویر Thumbnail الزامی است");
                        return;
                    }

                    if (thumbnailFile.ContentLength == 0)
                    {
                        _logger.Warning("ProcessImageUpload - ThumbnailFile ContentLength = 0 است در Create");
                        ModelState.AddModelError("ThumbnailFile", "تصویر Thumbnail الزامی است");
                        return;
                    }
                }

                // اگر فایل Thumbnail آپلود شده
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        StoryImageUploadPath,
                        StoryThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (!uploadResult.Success)
                    {
                        _logger.Warning("خطا در آپلود تصویر Thumbnail: {ErrorMessage}", uploadResult.Message);
                        NotificationHelper.SetError(TempData, uploadResult.Message);
                        ModelState.AddModelError("ThumbnailFile", uploadResult.Message);
                        return;
                    }

                    model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                    _logger.Information("تصویر Thumbnail با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر Thumbnail");
                NotificationHelper.SetError(TempData, "خطا در آپلود تصویر Thumbnail");
                ModelState.AddModelError("ThumbnailFile", "خطا در آپلود تصویر Thumbnail");
            }
        }

        /// <summary>
        /// پردازش آپلود ویدیو (فقط برای DirectUpload)
        /// </summary>
        private async Task ProcessVideoUpload(StoryCreateEditViewModel model)
        {
            try
            {
                var videoFile = Request.Files["VideoFile"];

                // در Create، VideoFile برای DirectUpload الزامی است
                if (videoFile == null || videoFile.ContentLength == 0)
                {
                    ModelState.AddModelError("VideoFile", "فایل ویدیو الزامی است");
                    return;
                }

                var videoUploadResult = _videoUploadService.UploadVideo(videoFile, StoryVideoUploadPath);
                if (!videoUploadResult.Success)
                {
                    _logger.Warning("خطا در آپلود ویدیو: {ErrorMessage}", videoUploadResult.Message);
                    NotificationHelper.SetError(TempData, videoUploadResult.Message);
                    ModelState.AddModelError("VideoFile", videoUploadResult.Message);
                    return;
                }

                model.VideoUrl = videoUploadResult.Data.VideoUrl;
                _logger.Information("ویدیو با موفقیت آپلود شد: {VideoUrl}", model.VideoUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود ویدیو");
                NotificationHelper.SetError(TempData, "خطا در آپلود ویدیو");
                ModelState.AddModelError("VideoFile", "خطا در آپلود ویدیو");
            }
        }

        #endregion

        #region Video URL Processing

        /// <summary>
        /// پردازش URL ویدیو برای YouTube و Vimeo
        /// </summary>
        private void ProcessVideoUrl(StoryCreateEditViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.VideoUrl))
            {
                ModelState.AddModelError("VideoUrl", "آدرس ویدیو الزامی است");
                return;
            }

            var videoUrl = model.VideoUrl.Trim();

            // تشخیص نوع ویدیو از URL
            if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
            {
                model.VideoType = "YouTube";
                // Extract YouTube Video ID
                var videoId = ExtractYouTubeVideoId(videoUrl);
                if (!string.IsNullOrEmpty(videoId))
                {
                    model.VideoUrl = videoId;
                }
                else
                {
                    ModelState.AddModelError("VideoUrl", "آدرس YouTube نامعتبر است");
                }
            }
            else if (videoUrl.Contains("vimeo.com"))
            {
                model.VideoType = "Vimeo";
                // Extract Vimeo Video ID
                var videoId = ExtractVimeoVideoId(videoUrl);
                if (!string.IsNullOrEmpty(videoId))
                {
                    model.VideoUrl = videoId;
                }
                else
                {
                    ModelState.AddModelError("VideoUrl", "آدرس Vimeo نامعتبر است");
                }
            }
            else if (videoUrl.Contains("aparat.com"))
            {
                model.VideoType = "Aparat";
                // برای Aparat، URL کامل را نگه می‌داریم
            }
            else
            {
                // اگر نوع مشخص نشده، به عنوان DirectUpload در نظر می‌گیریم
                model.VideoType = "DirectUpload";
            }
        }

        /// <summary>
        /// استخراج Video ID از YouTube URL
        /// </summary>
        private string ExtractYouTubeVideoId(string url)
        {
            try
            {
                // فرمت‌های مختلف YouTube URL:
                // https://www.youtube.com/watch?v=VIDEO_ID
                // https://youtu.be/VIDEO_ID
                // https://www.youtube.com/embed/VIDEO_ID

                if (url.Contains("youtu.be/"))
                {
                    var parts = url.Split(new[] { "youtu.be/" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var videoId = parts[1].Split('?')[0].Split('&')[0];
                        return videoId;
                    }
                }
                else if (url.Contains("youtube.com/watch?v="))
                {
                    var parts = url.Split(new[] { "v=" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var videoId = parts[1].Split('&')[0].Split('#')[0];
                        return videoId;
                    }
                }
                else if (url.Contains("youtube.com/embed/"))
                {
                    var parts = url.Split(new[] { "embed/" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var videoId = parts[1].Split('?')[0].Split('&')[0];
                        return videoId;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در استخراج YouTube Video ID از URL: {Url}", url);
                return null;
            }
        }

        /// <summary>
        /// استخراج Video ID از Vimeo URL
        /// </summary>
        private string ExtractVimeoVideoId(string url)
        {
            try
            {
                // فرمت‌های مختلف Vimeo URL:
                // https://vimeo.com/VIDEO_ID
                // https://player.vimeo.com/video/VIDEO_ID

                if (url.Contains("vimeo.com/"))
                {
                    var parts = url.Split(new[] { "vimeo.com/" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var videoId = parts[1].Split('?')[0].Split('/')[0];
                        // بررسی اینکه عدد است
                        if (int.TryParse(videoId, out _))
                        {
                            return videoId;
                        }
                    }
                }
                else if (url.Contains("player.vimeo.com/video/"))
                {
                    var parts = url.Split(new[] { "video/" }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        var videoId = parts[1].Split('?')[0].Split('&')[0];
                        if (int.TryParse(videoId, out _))
                        {
                            return videoId;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در استخراج Vimeo Video ID از URL: {Url}", url);
                return null;
            }
        }

        #endregion
    }
}
