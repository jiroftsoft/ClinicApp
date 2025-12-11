using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت ویدیو
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public VideoService(
            IVideoRepository videoRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _videoRepository = videoRepository ?? throw new ArgumentNullException(nameof(videoRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<VideoIndexViewModel>>> GetVideosAsync(VideoSearchViewModel search = null)
        {
            try
            {
                var videos = await _videoRepository.GetAllAsync(includeDeleted: false);
                
                var query = videos.AsQueryable();

                if (search != null)
                {
                    if (!string.IsNullOrEmpty(search.SearchTerm))
                    {
                        query = query.Where(v => v.Title.Contains(search.SearchTerm) || 
                                                 (v.Description != null && v.Description.Contains(search.SearchTerm)));
                    }

                    if (!string.IsNullOrEmpty(search.Category))
                    {
                        query = query.Where(v => v.Category == search.Category);
                    }

                    if (search.VideoType.HasValue)
                    {
                        query = query.Where(v => v.VideoType == search.VideoType.Value);
                    }

                    if (search.IsActive.HasValue)
                    {
                        query = query.Where(v => v.IsActive == search.IsActive.Value);
                    }
                }

                var viewModels = query
                    .Select(v => new VideoIndexViewModel
                    {
                        VideoId = v.VideoId,
                        Title = v.Title,
                        Description = v.Description,
                        VideoUrl = v.VideoUrl,
                        VideoType = v.VideoType,
                        VideoTypeName = GetVideoTypeName(v.VideoType),
                        ThumbnailUrl = v.ThumbnailUrl,
                        Category = v.Category,
                        Duration = v.Duration,
                        DurationFormatted = FormatDuration(v.Duration),
                        ViewCount = v.ViewCount,
                        IsActive = v.IsActive,
                        DisplayOrder = v.DisplayOrder,
                        CreatedAt = v.CreatedAt
                    })
                    .OrderBy(v => v.DisplayOrder)
                    .ThenByDescending(v => v.CreatedAt)
                    .ToList();

                return ServiceResult<List<VideoIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست ویدیوها");
                return ServiceResult<List<VideoIndexViewModel>>.Failed("خطا در دریافت لیست ویدیوها");
            }
        }

        public async Task<ServiceResult<VideoDetailsViewModel>> GetVideoDetailsAsync(int videoId)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult<VideoDetailsViewModel>.Failed("ویدیو یافت نشد");
                }

                var viewModel = new VideoDetailsViewModel
                {
                    VideoId = video.VideoId,
                    Title = video.Title,
                    Description = video.Description,
                    VideoUrl = video.VideoUrl,
                    VideoType = video.VideoType,
                    VideoTypeName = GetVideoTypeName(video.VideoType),
                    ThumbnailUrl = video.ThumbnailUrl,
                    Category = video.Category,
                    Duration = video.Duration,
                    DurationFormatted = FormatDuration(video.Duration),
                    ViewCount = video.ViewCount,
                    IsActive = video.IsActive,
                    DisplayOrder = video.DisplayOrder,
                    CreatedAt = video.CreatedAt,
                    CreatedByUserName = video.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = video.UpdatedAt,
                    UpdatedByUserName = video.UpdatedByUser?.UserName
                };

                return ServiceResult<VideoDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات ویدیو - VideoId: {VideoId}", videoId);
                return ServiceResult<VideoDetailsViewModel>.Failed("خطا در دریافت جزئیات ویدیو");
            }
        }

        public async Task<ServiceResult<VideoCreateEditViewModel>> GetVideoForEditAsync(int videoId)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult<VideoCreateEditViewModel>.Failed("ویدیو یافت نشد");
                }

                var viewModel = new VideoCreateEditViewModel
                {
                    VideoId = video.VideoId,
                    Title = video.Title,
                    Description = video.Description,
                    VideoUrl = video.VideoUrl,
                    VideoType = video.VideoType,
                    ThumbnailUrl = video.ThumbnailUrl,
                    Category = video.Category,
                    Duration = video.Duration,
                    IsActive = video.IsActive,
                    DisplayOrder = video.DisplayOrder
                };

                return ServiceResult<VideoCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ویدیو برای ویرایش - VideoId: {VideoId}", videoId);
                return ServiceResult<VideoCreateEditViewModel>.Failed("خطا در دریافت ویدیو برای ویرایش");
            }
        }

        public async Task<ServiceResult<Video>> CreateVideoAsync(VideoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد ویدیو جدید - Title: {Title}", model.Title);

                var video = new Video
                {
                    Title = model.Title,
                    Description = model.Description,
                    VideoUrl = model.VideoUrl,
                    VideoType = model.VideoType,
                    ThumbnailUrl = model.ThumbnailUrl,
                    Category = model.Category ?? "general",
                    Duration = model.Duration,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedByUserId = _currentUserService.UserId
                };

                _videoRepository.Add(video);
                await _context.SaveChangesAsync();

                _logger.Information("ویدیو با موفقیت ایجاد شد - VideoId: {VideoId}", video.VideoId);
                return ServiceResult<Video>.Successful(video, "ویدیو با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد ویدیو");
                return ServiceResult<Video>.Failed("خطا در ایجاد ویدیو");
            }
        }

        public async Task<ServiceResult<Video>> UpdateVideoAsync(VideoCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی ویدیو - VideoId: {VideoId}", model.VideoId);

                var video = await _videoRepository.GetByIdAsync(model.VideoId);
                if (video == null)
                {
                    return ServiceResult<Video>.Failed("ویدیو یافت نشد");
                }

                video.Title = model.Title;
                video.Description = model.Description;
                video.VideoUrl = model.VideoUrl;
                video.VideoType = model.VideoType;
                video.ThumbnailUrl = model.ThumbnailUrl;
                video.Category = model.Category ?? video.Category ?? "general";
                video.Duration = model.Duration;
                video.IsActive = model.IsActive;
                video.DisplayOrder = model.DisplayOrder;
                video.UpdatedByUserId = _currentUserService.UserId;
                video.UpdatedAt = DateTime.UtcNow;

                _videoRepository.Update(video);
                await _context.SaveChangesAsync();

                _logger.Information("ویدیو با موفقیت به‌روزرسانی شد - VideoId: {VideoId}", video.VideoId);
                return ServiceResult<Video>.Successful(video, "ویدیو با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ویدیو - VideoId: {VideoId}", model.VideoId);
                return ServiceResult<Video>.Failed("خطا در به‌روزرسانی ویدیو");
            }
        }

        public async Task<ServiceResult> DeleteVideoAsync(int videoId)
        {
            try
            {
                _logger.Information("حذف ویدیو - VideoId: {VideoId}", videoId);

                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult.Failed("ویدیو یافت نشد");
                }

                _videoRepository.Delete(video);
                await _context.SaveChangesAsync();

                _logger.Information("ویدیو با موفقیت حذف شد - VideoId: {VideoId}", videoId);
                return ServiceResult.Successful("ویدیو با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف ویدیو - VideoId: {VideoId}", videoId);
                return ServiceResult.Failed("خطا در حذف ویدیو");
            }
        }

        public async Task<ServiceResult> ActivateVideoAsync(int videoId)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult.Failed("ویدیو یافت نشد");
                }

                video.IsActive = true;
                video.UpdatedByUserId = _currentUserService.UserId;
                video.UpdatedAt = DateTime.UtcNow;

                _videoRepository.Update(video);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ویدیو با موفقیت فعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی ویدیو - VideoId: {VideoId}", videoId);
                return ServiceResult.Failed("خطا در فعال‌سازی ویدیو");
            }
        }

        public async Task<ServiceResult> DeactivateVideoAsync(int videoId)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult.Failed("ویدیو یافت نشد");
                }

                video.IsActive = false;
                video.UpdatedByUserId = _currentUserService.UserId;
                video.UpdatedAt = DateTime.UtcNow;

                _videoRepository.Update(video);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ویدیو با موفقیت غیرفعال شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی ویدیو - VideoId: {VideoId}", videoId);
                return ServiceResult.Failed("خطا در غیرفعال‌سازی ویدیو");
            }
        }

        public async Task<ServiceResult> UpdateDisplayOrderAsync(int videoId, int newOrder)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult.Failed("ویدیو یافت نشد");
                }

                video.DisplayOrder = newOrder;
                video.UpdatedByUserId = _currentUserService.UserId;
                video.UpdatedAt = DateTime.UtcNow;

                _videoRepository.Update(video);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful("ترتیب نمایش با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی ترتیب نمایش - VideoId: {VideoId}", videoId);
                return ServiceResult.Failed("خطا در به‌روزرسانی ترتیب نمایش");
            }
        }

        public async Task<ServiceResult<List<string>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _videoRepository.GetCategoriesAsync();
                return ServiceResult<List<string>>.Successful(categories);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی‌های ویدیو");
                return ServiceResult<List<string>>.Failed("خطا در دریافت دسته‌بندی‌های ویدیو");
            }
        }

        public async Task<ServiceResult<List<VideoHomePageViewModel>>> GetVideosForHomePageAsync(int count = 6, string category = null)
        {
            try
            {
                var videos = await _videoRepository.GetVideosForHomePageAsync(count, category);
                
                var viewModels = videos.Select(v => new VideoHomePageViewModel
                {
                    VideoId = v.VideoId,
                    Title = v.Title,
                    Description = v.Description,
                    VideoUrl = v.VideoUrl,
                    VideoType = v.VideoType,
                    ThumbnailUrl = v.ThumbnailUrl,
                    Category = v.Category,
                    Duration = v.Duration,
                    DurationFormatted = FormatDuration(v.Duration),
                    ViewCount = v.ViewCount,
                    EmbedUrl = GetEmbedUrl(v.VideoUrl, v.VideoType),
                    VideoIdFromUrl = ExtractVideoId(v.VideoUrl, v.VideoType)
                }).ToList();

                return ServiceResult<List<VideoHomePageViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ویدیوها برای صفحه اصلی");
                return ServiceResult<List<VideoHomePageViewModel>>.Failed("خطا در دریافت ویدیوها برای صفحه اصلی");
            }
        }

        public async Task<ServiceResult> IncrementViewCountAsync(int videoId)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                {
                    return ServiceResult.Failed("ویدیو یافت نشد");
                }

                video.ViewCount++;
                _videoRepository.Update(video);
                await _context.SaveChangesAsync();

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید - VideoId: {VideoId}", videoId);
                return ServiceResult.Failed("خطا در افزایش تعداد بازدید");
            }
        }

        #region Helper Methods

        private string GetVideoTypeName(VideoType videoType)
        {
            switch (videoType)
            {
                case VideoType.YouTube:
                    return "YouTube";
                case VideoType.Vimeo:
                    return "Vimeo";
                case VideoType.Aparat:
                    return "آپارات";
                case VideoType.DirectUpload:
                    return "آپلود مستقیم";
                default:
                    return "نامشخص";
            }
        }

        private string FormatDuration(int? duration)
        {
            if (!duration.HasValue || duration.Value <= 0)
                return "-";

            var timeSpan = TimeSpan.FromSeconds(duration.Value);
            if (timeSpan.Hours > 0)
            {
                return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        private string GetEmbedUrl(string videoUrl, VideoType videoType)
        {
            if (string.IsNullOrEmpty(videoUrl))
                return null;

            var videoId = ExtractVideoId(videoUrl, videoType);
            if (string.IsNullOrEmpty(videoId))
                return null;

            switch (videoType)
            {
                case VideoType.YouTube:
                    return $"https://www.youtube.com/embed/{videoId}";
                case VideoType.Vimeo:
                    return $"https://player.vimeo.com/video/{videoId}";
                case VideoType.Aparat:
                    return $"https://www.aparat.com/video/video/embed/videohash/{videoId}/vt/frame";
                case VideoType.DirectUpload:
                    return videoUrl;
                default:
                    return null;
            }
        }

        private string ExtractVideoId(string videoUrl, VideoType videoType)
        {
            if (string.IsNullOrEmpty(videoUrl))
                return null;

            switch (videoType)
            {
                case VideoType.YouTube:
                    // پشتیبانی از فرمت‌های مختلف YouTube URL
                    if (videoUrl.Contains("youtube.com/watch?v="))
                    {
                        var index = videoUrl.IndexOf("v=") + 2;
                        var endIndex = videoUrl.IndexOf("&", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    else if (videoUrl.Contains("youtu.be/"))
                    {
                        var index = videoUrl.IndexOf("youtu.be/") + 9;
                        var endIndex = videoUrl.IndexOf("?", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    else if (videoUrl.Contains("youtube.com/embed/"))
                    {
                        var index = videoUrl.IndexOf("embed/") + 6;
                        var endIndex = videoUrl.IndexOf("?", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    return null;

                case VideoType.Vimeo:
                    // پشتیبانی از فرمت‌های مختلف Vimeo URL
                    if (videoUrl.Contains("vimeo.com/"))
                    {
                        var index = videoUrl.IndexOf("vimeo.com/") + 10;
                        var endIndex = videoUrl.IndexOf("/", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    return null;

                case VideoType.Aparat:
                    // پشتیبانی از فرمت‌های مختلف Aparat URL
                    if (videoUrl.Contains("aparat.com/v/"))
                    {
                        var index = videoUrl.IndexOf("aparat.com/v/") + 13;
                        var endIndex = videoUrl.IndexOf("/", index);
                        if (endIndex == -1) endIndex = videoUrl.IndexOf("?", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    else if (videoUrl.Contains("aparat.com/video/video/embed/videohash/"))
                    {
                        var index = videoUrl.IndexOf("videohash/") + 10;
                        var endIndex = videoUrl.IndexOf("/", index);
                        if (endIndex == -1) endIndex = videoUrl.IndexOf("?", index);
                        if (endIndex == -1) endIndex = videoUrl.Length;
                        return videoUrl.Substring(index, endIndex - index);
                    }
                    return null;

                case VideoType.DirectUpload:
                    return null; // برای Direct Upload، VideoId وجود ندارد

                default:
                    return null;
            }
        }

        #endregion
    }
}

