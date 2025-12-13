using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository مدیریت Template های خبرنامه
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface INewsletterTemplateRepository
    {
        Task<NewsletterTemplate> GetByIdAsync(int templateId);
        Task<List<NewsletterTemplate>> GetAllAsync(bool includeDeleted = false);
        Task<List<NewsletterTemplate>> GetActiveAsync(bool includeDeleted = false);
        void Add(NewsletterTemplate template);
        void Update(NewsletterTemplate template);
        void Delete(NewsletterTemplate template);
        Task<bool> ExistsAsync(int templateId);
    }
}

