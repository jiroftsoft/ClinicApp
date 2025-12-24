using ClinicApp.Models.Entities.CMS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository مدیریت Story
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IStoryRepository
    {
        /// <summary>
        /// دریافت همه Story ها
        /// </summary>
        Task<List<Story>> GetAllAsync(bool includeDeleted = false);

        /// <summary>
        /// دریافت Story بر اساس ID
        /// </summary>
        Task<Story> GetByIdAsync(int storyId, bool includeDeleted = false);

        /// <summary>
        /// دریافت Story های فعال برای نمایش عمومی
        /// </summary>
        Task<List<Story>> GetActiveStoriesAsync();

        /// <summary>
        /// افزودن Story جدید
        /// </summary>
        Task<Story> AddAsync(Story story);

        /// <summary>
        /// به‌روزرسانی Story
        /// </summary>
        Task<Story> UpdateAsync(Story story);

        /// <summary>
        /// حذف نرم Story
        /// </summary>
        Task<bool> DeleteAsync(int storyId, string deletedByUserId);

        /// <summary>
        /// افزایش تعداد بازدید
        /// </summary>
        Task<bool> IncrementViewCountAsync(int storyId);
    }
}
