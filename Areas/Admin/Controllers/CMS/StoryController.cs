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
        public async Task<ActionResult> Create(StoryCreateEditViewModel model, HttpPostedFileBase thumbnailFile, HttpPostedFileBase videoFile)
        {
            try
            {
                _logger.Information("درخواست ایجاد Story جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // پردازش آپلود تصویر Thumbnail
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

                    if (uploadResult.Success)
                    {
                        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                        _logger.Information("تصویر Thumbnail با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Thumbnail: {uploadResult.Message}");
                        ModelState.AddModelError("ThumbnailFile", uploadResult.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError("ThumbnailFile", "تصویر Thumbnail الزامی است");
                }

                // پردازش آپلود ویدیو (اگر DirectUpload)
                if (model.VideoType == "DirectUpload")
                {
                    if (videoFile != null && videoFile.ContentLength > 0)
                    {
                        var videoUploadResult = _videoUploadService.UploadVideo(videoFile, StoryVideoUploadPath);
                        if (videoUploadResult.Success)
                        {
                            model.VideoUrl = videoUploadResult.Data.VideoUrl;
                            _logger.Information("ویدیو با موفقیت آپلود شد: {VideoUrl}", model.VideoUrl);
                        }
                        else
                        {
                            NotificationHelper.SetWarning(TempData, $"خطا در آپلود ویدیو: {videoUploadResult.Message}");
                            ModelState.AddModelError("VideoFile", videoUploadResult.Message);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("VideoFile", "فایل ویدیو الزامی است");
                    }
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
        public async Task<ActionResult> Edit(StoryCreateEditViewModel model, HttpPostedFileBase thumbnailFile, HttpPostedFileBase videoFile)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی Story - StoryId: {StoryId}", model.StoryId);

                // پردازش آپلود تصویر Thumbnail (فقط در صورت ارسال فایل جدید)
                if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
                {
                    // حذف تصویر قبلی
                    var existingStory = await _storyService.GetStoryByIdAsync(model.StoryId);
                    if (existingStory.Success && !string.IsNullOrEmpty(existingStory.Data.ThumbnailUrl))
                    {
                        _imageUploadService.DeleteImage(existingStory.Data.ThumbnailUrl);
                    }

                    var uploadResult = _imageUploadService.UploadImageWithThumbnail(
                        thumbnailFile,
                        StoryImageUploadPath,
                        StoryThumbnailUploadPath,
                        ThumbnailWidth,
                        ThumbnailHeight,
                        MaxImageWidth,
                        MaxImageHeight);

                    if (uploadResult.Success)
                    {
                        model.ThumbnailUrl = uploadResult.Data.ThumbnailUrl;
                        _logger.Information("تصویر Thumbnail جدید با موفقیت آپلود شد: {ThumbnailUrl}", model.ThumbnailUrl);
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود تصویر Thumbnail: {uploadResult.Message}");
                        ModelState.AddModelError("ThumbnailFile", uploadResult.Message);
                    }
                }

                // پردازش آپلود ویدیو (اگر DirectUpload و فایل جدید انتخاب شده)
                if (model.VideoType == "DirectUpload" && videoFile != null && videoFile.ContentLength > 0)
                {
                    // حذف ویدیو قدیمی
                    var existingStory = await _storyService.GetStoryByIdAsync(model.StoryId);
                    if (existingStory.Success && !string.IsNullOrEmpty(existingStory.Data.VideoUrl))
                    {
                        _videoUploadService.DeleteVideo(existingStory.Data.VideoUrl);
                    }

                    var videoUploadResult = _videoUploadService.UploadVideo(videoFile, StoryVideoUploadPath);
                    if (videoUploadResult.Success)
                    {
                        model.VideoUrl = videoUploadResult.Data.VideoUrl;
                        _logger.Information("ویدیو جدید با موفقیت آپلود شد: {VideoUrl}", model.VideoUrl);
                    }
                    else
                    {
                        NotificationHelper.SetWarning(TempData, $"خطا در آپلود ویدیو: {videoUploadResult.Message}");
                        ModelState.AddModelError("VideoFile", videoUploadResult.Message);
                    }
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

        #region Helper Methods

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
