using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository مدیریت Recipients Campaign
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterCampaignRecipientRepository
    {
        Task<NewsletterCampaignRecipient> GetByIdAsync(int recipientId);
        Task<List<NewsletterCampaignRecipient>> GetByCampaignIdAsync(int campaignId);
        Task<List<NewsletterCampaignRecipient>> GetBySubscriptionIdAsync(int subscriptionId);
        Task<NewsletterCampaignRecipient> GetByCampaignAndSubscriptionAsync(int campaignId, int subscriptionId);
        void Add(NewsletterCampaignRecipient recipient);
        void Update(NewsletterCampaignRecipient recipient);
        Task BulkInsertAsync(List<NewsletterCampaignRecipient> recipients);
        Task<int> GetCountByCampaignIdAsync(int campaignId);
        Task<int> GetOpenedCountByCampaignIdAsync(int campaignId);
        Task<int> GetClickedCountByCampaignIdAsync(int campaignId);
    }
}

