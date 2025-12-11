using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت ویدیوها
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class VideoController : BaseCMSController
    {
        private readonly IVideoService _videoService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVideoUploadService _videoUploadService;
        private readonly ILogger _logger;

        // Production Configuration
        private const string VideoUploadPath = "~/Content/Videos";
        private const int MaxVideoSizeInMB = 100;

        public VideoController(
            IVideoService videoService,
            ICurrentUserService currentUserService,
            IVideoUploadService videoUploadService)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _videoUploadService = videoUploadService ?? throw new ArgumentNullException(nameof(videoUploadService));
            _logger = Log.ForContext<VideoController>();
        }

        #region Index

        /// <summary>
        /// نمایش لیست ویدیوها
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(VideoSearchViewModel search = null)
        {
            try
            {
                if (search == null)
                {
                    search = new VideoSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _videoService.GetVideosAsync(search);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست ویدیوها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<VideoIndexViewModel>());
                }

                // دریافت دسته‌بندی‌ها برای فیلتر
                var categoriesResult = await _videoService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<string>();
                ViewBag.SearchModel = search;

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست ویدیوها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست ویدیوها");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<VideoIndexViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات ویدیو
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _videoService.GetVideoDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات ویدیو - VideoId: {VideoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات ویدیو");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد ویدیو جدید
        /// </summary>
        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var model = new VideoCreateEditViewModel
                {
                    IsActive = true,
                    DisplayOrder = 0,
                    VideoType = VideoType.YouTube,
                    Category = "general"
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد ویدیو");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد ویدیو");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد ویدیو جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(VideoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد ویدیو جدید توسط کاربر {UserId}", _currentUserService.UserId);

                // پردازش آپلود ویدیو (اگر DirectUpload)
                if (model.VideoType == VideoType.DirectUpload)
                {
                    ProcessVideoUpload(model);
                }
                else
                {
                    // پردازش Video URL و Extract Video ID (برای YouTube, Vimeo, Aparat)
                    ProcessVideoUrl(model);
                }

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Create"), model);
                }

                var result = await _videoService.CreateVideoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد ویدیو: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("ویدیو با موفقیت ایجاد شد - VideoId: {VideoId}", result.Data.VideoId);
                NotificationHelper.SetSuccess(TempData, "ویدیو با موفقیت ایجاد شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ویدیو");
                NotificationHelper.SetError(TempData, "خطا در ایجاد ویدیو");
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش ویدیو
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _videoService.GetVideoForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش ویدیو - VideoId: {VideoId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش ویدیو");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی ویدیو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(VideoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی ویدیو - VideoId: {VideoId}", model.VideoId);

                // پردازش آپلود ویدیو (اگر DirectUpload و فایل جدید انتخاب شده)
                if (model.VideoType == VideoType.DirectUpload && model.VideoFile != null && model.VideoFile.ContentLength > 0)
                {
                    // حذف ویدیو قدیمی (اگر وجود دارد)
                    var existingVideo = await _videoService.GetVideoDetailsAsync(model.VideoId);
                    if (existingVideo.Success && !string.IsNullOrEmpty(existingVideo.Data.VideoUrl))
                    {
                        _videoUploadService.DeleteVideo(existingVideo.Data.VideoUrl);
                    }

                    ProcessVideoUpload(model);
                }
                else if (model.VideoType != VideoType.DirectUpload)
                {
                    // پردازش Video URL و Extract Video ID (برای YouTube, Vimeo, Aparat)
                    ProcessVideoUrl(model);
                }

                if (!ModelState.IsValid)
                {
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _videoService.UpdateVideoAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی ویدیو: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("ویدیو با موفقیت به‌روزرسانی شد - VideoId: {VideoId}", model.VideoId);
                NotificationHelper.SetSuccess(TempData, "ویدیو با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ویدیو - VideoId: {VideoId}", model.VideoId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی ویدیو");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف ویدیو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _videoService.DeleteVideoAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "ویدیو با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ویدیو - VideoId: {VideoId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف ویدیو");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        /// <summary>
        /// فعال‌سازی ویدیو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _videoService.ActivateVideoAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "ویدیو با موفقیت فعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی ویدیو - VideoId: {VideoId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال‌سازی ویدیو");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// غیرفعال‌سازی ویدیو
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _videoService.DeactivateVideoAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "ویدیو با موفقیت غیرفعال شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی ویدیو - VideoId: {VideoId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال‌سازی ویدیو");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// پردازش Video URL و تشخیص نوع ویدیو
        /// </summary>
        private void ProcessVideoUrl(VideoCreateEditViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.VideoUrl))
                {
                    ModelState.AddModelError("VideoUrl", "آدرس ویدیو الزامی است.");
                    return;
                }

                var videoUrl = model.VideoUrl.Trim();

                // تشخیص نوع ویدیو بر اساس URL
                if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
                {
                    model.VideoType = VideoType.YouTube;
                    
                    // Extract Video ID برای YouTube
                    var videoId = ExtractYouTubeVideoId(videoUrl);
                    if (string.IsNullOrEmpty(videoId))
                    {
                        ModelState.AddModelError("VideoUrl", "آدرس YouTube نامعتبر است.");
                        return;
                    }

                    // تنظیم Thumbnail URL خودکار برای YouTube
                    if (string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        model.ThumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                    }
                }
                else if (videoUrl.Contains("vimeo.com"))
                {
                    model.VideoType = VideoType.Vimeo;
                    
                    // Extract Video ID برای Vimeo
                    var videoId = ExtractVimeoVideoId(videoUrl);
                    if (string.IsNullOrEmpty(videoId))
                    {
                        ModelState.AddModelError("VideoUrl", "آدرس Vimeo نامعتبر است.");
                        return;
                    }

                    // برای Vimeo، Thumbnail باید از API گرفته شود (در صورت نیاز)
                    // فعلاً از URL اصلی استفاده می‌کنیم
                }
                else if (videoUrl.Contains("aparat.com"))
                {
                    model.VideoType = VideoType.Aparat;
                    
                    // Extract Video ID برای Aparat
                    var videoId = ExtractAparatVideoId(videoUrl);
                    if (string.IsNullOrEmpty(videoId))
                    {
                        ModelState.AddModelError("VideoUrl", "آدرس آپارات نامعتبر است.");
                        return;
                    }

                    // تنظیم Thumbnail URL خودکار برای Aparat
                    if (string.IsNullOrEmpty(model.ThumbnailUrl))
                    {
                        model.ThumbnailUrl = $"https://www.aparat.com/video/video/thumb/video_h_{videoId}.jpg";
                    }
                }
                else
                {
                    // اگر نوع مشخص نشده، بر اساس VideoType انتخاب شده عمل می‌کنیم
                    if (model.VideoType == VideoType.DirectUpload)
                    {
                        ModelState.AddModelError("VideoUrl", "برای آپلود مستقیم، باید فایل ویدیو را انتخاب کنید.");
                        return;
                    }
                }

                // تنظیم VideoUrl
                model.VideoUrl = videoUrl;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش Video URL");
                ModelState.AddModelError("VideoUrl", "خطا در پردازش آدرس ویدیو");
            }
        }

        /// <summary>
        /// استخراج Video ID از YouTube URL
        /// </summary>
        private string ExtractYouTubeVideoId(string url)
        {
            try
            {
                if (url.Contains("youtube.com/watch?v="))
                {
                    var index = url.IndexOf("v=") + 2;
                    var endIndex = url.IndexOf("&", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                else if (url.Contains("youtu.be/"))
                {
                    var index = url.IndexOf("youtu.be/") + 9;
                    var endIndex = url.IndexOf("?", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                else if (url.Contains("youtube.com/embed/"))
                {
                    var index = url.IndexOf("embed/") + 6;
                    var endIndex = url.IndexOf("?", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                return null;
            }
            catch
            {
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
                if (url.Contains("vimeo.com/"))
                {
                    var index = url.IndexOf("vimeo.com/") + 10;
                    var endIndex = url.IndexOf("/", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// استخراج Video ID از Aparat URL
        /// </summary>
        private string ExtractAparatVideoId(string url)
        {
            try
            {
                // فرمت‌های مختلف Aparat URL:
                // https://www.aparat.com/v/VIDEO_ID
                // https://aparat.com/v/VIDEO_ID
                // https://www.aparat.com/video/video/embed/videohash/VIDEO_ID
                
                if (url.Contains("aparat.com/v/"))
                {
                    var index = url.IndexOf("aparat.com/v/") + 13;
                    var endIndex = url.IndexOf("/", index);
                    if (endIndex == -1) endIndex = url.IndexOf("?", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                else if (url.Contains("aparat.com/video/video/embed/videohash/"))
                {
                    var index = url.IndexOf("videohash/") + 10;
                    var endIndex = url.IndexOf("/", index);
                    if (endIndex == -1) endIndex = url.IndexOf("?", index);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(index, endIndex - index);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// پردازش آپلود ویدیو برای DirectUpload
        /// </summary>
        private void ProcessVideoUpload(VideoCreateEditViewModel model)
        {
            try
            {
                if (model.VideoFile == null || model.VideoFile.ContentLength == 0)
                {
                    ModelState.AddModelError("VideoFile", "برای آپلود مستقیم، فایل ویدیو الزامی است.");
                    return;
                }

                // آپلود ویدیو
                var uploadResult = _videoUploadService.UploadVideo(model.VideoFile, VideoUploadPath);

                if (!uploadResult.Success)
                {
                    ModelState.AddModelError("VideoFile", uploadResult.Message);
                    return;
                }

                // تنظیم VideoUrl از نتیجه آپلود
                model.VideoUrl = uploadResult.Data.VideoUrl;
                model.VideoType = VideoType.DirectUpload;

                _logger.Information("ویدیو با موفقیت آپلود شد: {VideoUrl}, Size: {FileSize}", 
                    uploadResult.Data.VideoUrl, uploadResult.Data.FileSizeFormatted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود ویدیو");
                ModelState.AddModelError("VideoFile", "خطا در آپلود ویدیو. لطفاً دوباره تلاش کنید.");
            }
        }

        #endregion
    }
}

