using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository مدیریت Campaign های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterCampaignRepository
    {
        Task<NewsletterCampaign> GetByIdAsync(int campaignId);
        Task<List<NewsletterCampaign>> GetAllAsync(bool includeDeleted = false);
        Task<List<NewsletterCampaign>> GetByStatusAsync(NewsletterCampaignStatus status, bool includeDeleted = false);
        Task<List<NewsletterCampaign>> GetScheduledAsync(bool includeDeleted = false);
        Task<List<NewsletterCampaign>> SearchAsync(string searchTerm, NewsletterCampaignStatus? status, DateTime? fromDate, DateTime? toDate, bool includeDeleted = false);
        void Add(NewsletterCampaign campaign);
        void Update(NewsletterCampaign campaign);
        void Delete(NewsletterCampaign campaign);
        Task<bool> ExistsAsync(int campaignId);
    }
}

