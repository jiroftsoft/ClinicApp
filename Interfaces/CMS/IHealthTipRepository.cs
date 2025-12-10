using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای HealthTip
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IHealthTipRepository
    {
        Task<HealthTip> GetByIdAsync(int healthTipId);
        Task<List<HealthTip>> GetAllAsync(bool includeDeleted = false);
        Task<List<HealthTip>> GetPublishedTipsAsync(string category = null, int count = 10);
        Task<List<HealthTip>> GetFeaturedTipsAsync(int count = 5);
        Task<List<HealthTip>> GetByCategoryAsync(string category, int count = 10);
        Task<List<HealthTip>> GetActiveTipsAsync(int count = 10); // بدون تاریخ انقضا یا تاریخ انقضا در آینده
        Task<List<HealthTip>> SearchTipsAsync(string searchTerm);
        Task<List<string>> GetCategoriesAsync();
        Task<HealthTip> GetBySlugAsync(string slug);
        void Add(HealthTip healthTip);
        void Update(HealthTip healthTip);
        void Delete(HealthTip healthTip);
    }
}

