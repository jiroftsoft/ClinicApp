using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای Service ارسال SMS خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterSmsService
    {
        Task<ServiceResult> SendNewsletterSmsAsync(NewsletterCampaign campaign, NewsletterSubscription subscription);
        Task<ServiceResult> SendVerificationSmsAsync(NewsletterSubscription subscription);
        Task<ServiceResult<string>> RenderSmsContentAsync(string content, System.Collections.Generic.Dictionary<string, string> variables);
    }
}

