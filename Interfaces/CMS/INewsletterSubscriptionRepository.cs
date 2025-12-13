using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository مدیریت اشتراک‌های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterSubscriptionRepository
    {
        Task<NewsletterSubscription> GetByIdAsync(int subscriptionId);
        Task<NewsletterSubscription> GetByEmailAsync(string email);
        Task<NewsletterSubscription> GetByVerificationTokenAsync(string token);
        Task<NewsletterSubscription> GetByUnsubscribeTokenAsync(string token);
        Task<List<NewsletterSubscription>> GetAllAsync(bool includeDeleted = false);
        Task<List<NewsletterSubscription>> GetActiveAsync(bool includeDeleted = false);
        Task<List<NewsletterSubscription>> GetActiveAndVerifiedAsync(bool includeDeleted = false);
        Task<List<NewsletterSubscription>> GetByCategoriesAsync(List<NewsletterCategory> categories, bool includeDeleted = false);
        Task<List<NewsletterSubscription>> GetBySourceAsync(NewsletterSubscriptionSource source, bool includeDeleted = false);
        Task<List<NewsletterSubscription>> SearchAsync(string searchTerm, bool? isActive, bool? isVerified, NewsletterSubscriptionSource? source, NewsletterCategory? category, bool includeDeleted = false);
        void Add(NewsletterSubscription subscription);
        void Update(NewsletterSubscription subscription);
        void Delete(NewsletterSubscription subscription);
        Task<bool> ExistsAsync(string email);
    }
}

