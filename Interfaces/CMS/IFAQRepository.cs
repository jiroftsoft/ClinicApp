using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای FAQ
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IFAQRepository
    {
        Task<FAQ> GetByIdAsync(int faqId);
        Task<List<FAQ>> GetAllAsync(bool includeDeleted = false);
        Task<List<FAQ>> GetActiveFAQsAsync(string category = null);
        Task<List<FAQ>> GetFeaturedFAQsAsync(int count = 5);
        Task<List<FAQ>> GetByCategoryAsync(string category, int count = 10);
        Task<List<FAQ>> SearchFAQsAsync(string searchTerm);
        Task<List<string>> GetCategoriesAsync();
        Task<FAQ> GetBySlugAsync(string slug);
        void Add(FAQ faq);
        void Update(FAQ faq);
        void Delete(FAQ faq);
    }
}

