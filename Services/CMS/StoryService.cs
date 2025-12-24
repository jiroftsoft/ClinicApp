using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Services.CMS
{
    /// <summary>
    /// سرویس مدیریت Story
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _storyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public StoryService(
            IStoryRepository storyRepository,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger)
        {
            _storyRepository = storyRepository ?? throw new ArgumentNullException(nameof(storyRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult<List<StoryIndexViewModel>>> GetStoriesAsync(bool includeInactive = false)
        {
            try
            {
                var stories = await _storyRepository.GetAllAsync(includeDeleted: false);
                
                if (!includeInactive)
                {
                    stories = stories.Where(s => s.IsActive).ToList();
                }

                var viewModels = stories.Select(s => new StoryIndexViewModel
                {
                    StoryId = s.StoryId,
                    Title = s.Title,
                    Description = s.Description,
                    ThumbnailUrl = s.ThumbnailUrl,
                    VideoUrl = s.VideoUrl,
                    VideoType = s.VideoType,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    IsActive = s.IsActive,
                    DisplayOrder = s.DisplayOrder,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    ViewCount = s.ViewCount,
                    Duration = s.Duration
                }).OrderBy(s => s.DisplayOrder)
                  .ThenByDescending(s => s.StartDate.HasValue ? s.StartDate.Value : DateTime.MinValue)
                  .ThenByDescending(s => s.StoryId)
                  .ToList();

                return ServiceResult<List<StoryIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست Story ها");
                return ServiceResult<List<StoryIndexViewModel>>.Failed("خطا در دریافت لیست Story ها");
            }
        }

        public async Task<ServiceResult<StoryDetailsViewModel>> GetStoryByIdAsync(int storyId)
        {
            try
            {
                var story = await _storyRepository.GetByIdAsync(storyId);
                if (story == null)
                {
                    return ServiceResult<StoryDetailsViewModel>.Failed("Story یافت نشد");
                }

                var viewModel = new StoryDetailsViewModel
                {
                    StoryId = story.StoryId,
                    Title = story.Title,
                    Description = story.Description,
                    ThumbnailUrl = story.ThumbnailUrl,
                    VideoUrl = story.VideoUrl,
                    VideoType = story.VideoType,
                    LinkUrl = story.LinkUrl,
                    ButtonText = story.ButtonText,
                    IsActive = story.IsActive,
                    DisplayOrder = story.DisplayOrder,
                    StartDate = story.StartDate,
                    EndDate = story.EndDate,
                    ViewCount = story.ViewCount,
                    Duration = story.Duration,
                    CreatedAt = story.CreatedAt,
                    CreatedByUserName = story.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = story.UpdatedAt,
                    UpdatedByUserName = story.UpdatedByUser?.UserName
                };

                return ServiceResult<StoryDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات Story - StoryId: {StoryId}", storyId);
                return ServiceResult<StoryDetailsViewModel>.Failed("خطا در دریافت جزئیات Story");
            }
        }

        public async Task<ServiceResult<List<StoryPublicViewModel>>> GetActiveStoriesForPublicAsync()
        {
            try
            {
                var stories = await _storyRepository.GetActiveStoriesAsync();

                var viewModels = stories.Select(s => new StoryPublicViewModel
                {
                    StoryId = s.StoryId,
                    Title = s.Title,
                    Description = s.Description,
                    ThumbnailUrl = s.ThumbnailUrl,
                    VideoUrl = s.VideoUrl,
                    VideoType = s.VideoType,
                    LinkUrl = s.LinkUrl,
                    ButtonText = s.ButtonText,
                    Duration = s.Duration
                }).ToList();

                return ServiceResult<List<StoryPublicViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت Story های فعال برای نمایش عمومی");
                return ServiceResult<List<StoryPublicViewModel>>.Failed("خطا در دریافت Story ها");
            }
        }

        public async Task<ServiceResult<StoryDetailsViewModel>> CreateStoryAsync(StoryCreateEditViewModel model)
        {
            try
            {
                _logger.Information("ایجاد Story جدید - Title: {Title}", model.Title);

                var story = new Story
                {
                    Title = model.Title,
                    Description = model.Description,
                    VideoUrl = model.VideoUrl,
                    VideoType = model.VideoType ?? "DirectUpload",
                    ThumbnailUrl = model.ThumbnailUrl,
                    LinkUrl = model.LinkUrl,
                    ButtonText = model.ButtonText,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Duration = model.Duration,
                    CreatedByUserId = _currentUserService.UserId
                };

                var createdStory = await _storyRepository.AddAsync(story);

                _logger.Information("Story با موفقیت ایجاد شد - StoryId: {StoryId}", createdStory.StoryId);
                
                var viewModel = new StoryDetailsViewModel
                {
                    StoryId = createdStory.StoryId,
                    Title = createdStory.Title,
                    Description = createdStory.Description,
                    ThumbnailUrl = createdStory.ThumbnailUrl,
                    VideoUrl = createdStory.VideoUrl,
                    VideoType = createdStory.VideoType,
                    LinkUrl = createdStory.LinkUrl,
                    ButtonText = createdStory.ButtonText,
                    IsActive = createdStory.IsActive,
                    DisplayOrder = createdStory.DisplayOrder,
                    StartDate = createdStory.StartDate,
                    EndDate = createdStory.EndDate,
                    ViewCount = createdStory.ViewCount,
                    Duration = createdStory.Duration,
                    CreatedAt = createdStory.CreatedAt
                };

                return ServiceResult<StoryDetailsViewModel>.Successful(viewModel, "Story با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Story");
                return ServiceResult<StoryDetailsViewModel>.Failed("خطا در ایجاد Story");
            }
        }

        public async Task<ServiceResult<StoryDetailsViewModel>> UpdateStoryAsync(int storyId, StoryCreateEditViewModel model)
        {
            try
            {
                _logger.Information("به‌روزرسانی Story - StoryId: {StoryId}", storyId);

                var story = await _storyRepository.GetByIdAsync(storyId);
                if (story == null)
                {
                    return ServiceResult<StoryDetailsViewModel>.Failed("Story یافت نشد");
                }

                story.Title = model.Title;
                story.Description = model.Description;
                story.VideoUrl = model.VideoUrl;
                story.VideoType = model.VideoType ?? story.VideoType ?? "DirectUpload";
                story.ThumbnailUrl = model.ThumbnailUrl;
                story.LinkUrl = model.LinkUrl;
                story.ButtonText = model.ButtonText;
                story.IsActive = model.IsActive;
                story.DisplayOrder = model.DisplayOrder;
                story.StartDate = model.StartDate;
                story.EndDate = model.EndDate;
                story.Duration = model.Duration;
                story.UpdatedByUserId = _currentUserService.UserId;
                story.UpdatedAt = DateTime.Now;

                var updatedStory = await _storyRepository.UpdateAsync(story);

                _logger.Information("Story با موفقیت به‌روزرسانی شد - StoryId: {StoryId}", updatedStory.StoryId);

                var viewModel = new StoryDetailsViewModel
                {
                    StoryId = updatedStory.StoryId,
                    Title = updatedStory.Title,
                    Description = updatedStory.Description,
                    ThumbnailUrl = updatedStory.ThumbnailUrl,
                    VideoUrl = updatedStory.VideoUrl,
                    VideoType = updatedStory.VideoType,
                    LinkUrl = updatedStory.LinkUrl,
                    ButtonText = updatedStory.ButtonText,
                    IsActive = updatedStory.IsActive,
                    DisplayOrder = updatedStory.DisplayOrder,
                    StartDate = updatedStory.StartDate,
                    EndDate = updatedStory.EndDate,
                    ViewCount = updatedStory.ViewCount,
                    Duration = updatedStory.Duration,
                    CreatedAt = updatedStory.CreatedAt,
                    UpdatedAt = updatedStory.UpdatedAt
                };

                return ServiceResult<StoryDetailsViewModel>.Successful(viewModel, "Story با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Story - StoryId: {StoryId}", storyId);
                return ServiceResult<StoryDetailsViewModel>.Failed("خطا در به‌روزرسانی Story");
            }
        }

        public async Task<ServiceResult<bool>> DeleteStoryAsync(int storyId)
        {
            try
            {
                _logger.Information("حذف Story - StoryId: {StoryId}", storyId);

                var result = await _storyRepository.DeleteAsync(storyId, _currentUserService.UserId);
                if (!result)
                {
                    return ServiceResult<bool>.Failed("Story یافت نشد");
                }

                _logger.Information("Story با موفقیت حذف شد - StoryId: {StoryId}", storyId);
                return ServiceResult<bool>.Successful(true, "Story با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Story - StoryId: {StoryId}", storyId);
                return ServiceResult<bool>.Failed("خطا در حذف Story");
            }
        }

        public async Task<ServiceResult<bool>> IncrementViewCountAsync(int storyId)
        {
            try
            {
                var result = await _storyRepository.IncrementViewCountAsync(storyId);
                if (!result)
                {
                    return ServiceResult<bool>.Failed("Story یافت نشد");
                }

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزایش تعداد بازدید Story - StoryId: {StoryId}", storyId);
                return ServiceResult<bool>.Failed("خطا در افزایش تعداد بازدید");
            }
        }
    }
}
