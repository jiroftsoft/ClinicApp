using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Service مدیریت Template های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterTemplateService
    {
        Task<ServiceResult<List<NewsletterTemplateIndexViewModel>>> GetTemplatesAsync();
        Task<ServiceResult<NewsletterTemplateDetailsViewModel>> GetTemplateDetailsAsync(int templateId);
        Task<ServiceResult<NewsletterTemplateCreateEditViewModel>> GetTemplateForEditAsync(int templateId);
        Task<ServiceResult<NewsletterTemplate>> CreateTemplateAsync(NewsletterTemplateCreateEditViewModel model);
        Task<ServiceResult<NewsletterTemplate>> UpdateTemplateAsync(NewsletterTemplateCreateEditViewModel model);
        Task<ServiceResult> DeleteTemplateAsync(int templateId);
        Task<ServiceResult> ActivateTemplateAsync(int templateId);
        Task<ServiceResult> DeactivateTemplateAsync(int templateId);
        Task<ServiceResult<string>> RenderTemplateAsync(int templateId, Dictionary<string, string> variables);
        Task<ServiceResult<string>> RenderTemplateAsync(string content, Dictionary<string, string> variables);
        Task<ServiceResult<TemplateRenderResult>> RenderTemplateWithResultAsync(string content, Dictionary<string, string> variables, int? templateId = null);
    }
}

