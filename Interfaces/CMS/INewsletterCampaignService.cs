using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Service مدیریت Campaign های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterCampaignService
    {
        Task<ServiceResult<PagedResult<NewsletterCampaignIndexViewModel>>> GetCampaignsAsync(NewsletterCampaignSearchViewModel searchModel);
        Task<ServiceResult<NewsletterCampaignDetailsViewModel>> GetCampaignDetailsAsync(int campaignId);
        Task<ServiceResult<NewsletterCampaignCreateEditViewModel>> GetCampaignForEditAsync(int campaignId);
        Task<ServiceResult<NewsletterCampaign>> CreateCampaignAsync(NewsletterCampaignCreateEditViewModel model);
        Task<ServiceResult<NewsletterCampaign>> UpdateCampaignAsync(NewsletterCampaignCreateEditViewModel model);
        Task<ServiceResult> DeleteCampaignAsync(int campaignId);
        Task<ServiceResult<NewsletterCampaignSendViewModel>> GetCampaignForSendAsync(int campaignId);
        Task<ServiceResult> SendCampaignAsync(int campaignId, bool sendEmail, bool sendSms);
        Task<ServiceResult> ScheduleCampaignAsync(int campaignId, DateTime scheduledAt, bool sendEmail, bool sendSms);
        Task<ServiceResult> CancelScheduledCampaignAsync(int campaignId);
        Task<ServiceResult<NewsletterCampaignStatisticsViewModel>> GetCampaignStatisticsAsync(int campaignId);
        Task<ServiceResult> TrackEmailOpenAsync(int campaignId, int recipientId);
        Task<ServiceResult> TrackEmailClickAsync(int campaignId, int recipientId, string url);
        Task<ServiceResult<int>> ProcessScheduledCampaignsAsync(); // Background Job
        Task<ServiceResult<int>> EstimateRecipientsAsync(List<NewsletterCategory> categories, bool sendToAll);
        /// <summary>ارسال واقعی به صف گیرندگان و به‌روزرسانی وضعیت به Sent/Failed و SentCount. از طریق Hangfire فراخوانی می‌شود.</summary>
        Task ProcessCampaignSendQueueAsync(int campaignId, bool sendEmail, bool sendSms);
        /// <summary>ارسال مجدد برای کمپین در حال ارسال (ادامه صف) یا ناموفق (تلاش مجدد برای گیرندگان ناموفق).</summary>
        Task<ServiceResult> RetryCampaignSendAsync(int campaignId, bool sendEmail = true, bool sendSms = true);
        /// <summary>تنظیم وضعیت کمپین به ناموفق (مثلاً پس از خطای Job). برای پروداکشن ضدگلوله.</summary>
        Task MarkCampaignAsFailedAsync(int campaignId, string errorMessage = null);
        /// <summary>وضعیت کمپین را برمی‌گرداند (برای تصمیم ارسال فوری vs ارسال مجدد).</summary>
        Task<NewsletterCampaignStatus?> GetCampaignStatusAsync(int campaignId);
    }
}

