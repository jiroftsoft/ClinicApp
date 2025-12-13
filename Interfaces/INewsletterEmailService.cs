using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای Service ارسال ایمیل خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterEmailService
    {
        Task<ServiceResult> SendNewsletterAsync(NewsletterCampaign campaign, NewsletterSubscription subscription);
        Task<ServiceResult> SendVerificationEmailAsync(NewsletterSubscription subscription);
        Task<ServiceResult> SendUnsubscribeConfirmationAsync(NewsletterSubscription subscription);
        Task<ServiceResult<string>> RenderContentAsync(string content, Dictionary<string, string> variables);
        string GenerateTrackingPixelUrl(int campaignId, int recipientId);
        string GenerateClickTrackingUrl(int campaignId, int recipientId, string originalUrl);
    }
}

