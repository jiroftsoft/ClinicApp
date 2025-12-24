using ClinicApp.ViewModels.CMS;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Service مدیریت Story
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IStoryService
    {
        /// <summary>
        /// دریافت همه Story ها
        /// </summary>
        Task<ServiceResult<List<StoryIndexViewModel>>> GetStoriesAsync(bool includeInactive = false);

        /// <summary>
        /// دریافت Story بر اساس ID
        /// </summary>
        Task<ServiceResult<StoryDetailsViewModel>> GetStoryByIdAsync(int storyId);

        /// <summary>
        /// دریافت Story های فعال برای نمایش عمومی
        /// </summary>
        Task<ServiceResult<List<StoryPublicViewModel>>> GetActiveStoriesForPublicAsync();

        /// <summary>
        /// ایجاد Story جدید
        /// </summary>
        Task<ServiceResult<StoryDetailsViewModel>> CreateStoryAsync(StoryCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی Story
        /// </summary>
        Task<ServiceResult<StoryDetailsViewModel>> UpdateStoryAsync(int storyId, StoryCreateEditViewModel model);

        /// <summary>
        /// حذف Story
        /// </summary>
        Task<ServiceResult<bool>> DeleteStoryAsync(int storyId);

        /// <summary>
        /// افزایش تعداد بازدید
        /// </summary>
        Task<ServiceResult<bool>> IncrementViewCountAsync(int storyId);
    }
}
