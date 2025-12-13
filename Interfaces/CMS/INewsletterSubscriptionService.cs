using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Service مدیریت اشتراک‌های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterSubscriptionService
    {
        Task<ServiceResult<PagedResult<NewsletterSubscriptionIndexViewModel>>> GetSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel);
        Task<ServiceResult<NewsletterSubscriptionDetailsViewModel>> GetSubscriptionDetailsAsync(int subscriptionId);
        Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionAsync(PublicNewsletterSubscriptionViewModel model, string ipAddress = null, string userAgent = null);
        Task<ServiceResult<NewsletterSubscription>> CreateSubscriptionByAdminAsync(NewsletterSubscriptionCreateEditViewModel model);
        Task<ServiceResult<NewsletterSubscription>> UpdateSubscriptionAsync(NewsletterSubscriptionCreateEditViewModel model);
        Task<ServiceResult> DeleteSubscriptionAsync(int subscriptionId);
        Task<ServiceResult> ActivateSubscriptionAsync(int subscriptionId);
        Task<ServiceResult> DeactivateSubscriptionAsync(int subscriptionId);
        Task<ServiceResult> VerifySubscriptionAsync(string verificationToken);
        Task<ServiceResult> UnsubscribeAsync(string unsubscribeToken);
        Task<ServiceResult> ImportSubscriptionsAsync(List<NewsletterSubscriptionCreateEditViewModel> subscriptions);
        Task<ServiceResult<byte[]>> ExportSubscriptionsAsync(NewsletterSubscriptionSearchViewModel searchModel);
        Task<ServiceResult<NewsletterStatisticsViewModel>> GetStatisticsAsync();
        Task<ServiceResult<int>> GetActiveCountAsync();
        Task<ServiceResult<int>> GetVerifiedCountAsync();
    }
}

