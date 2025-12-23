using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای AboutPage
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IAboutPageRepository
    {
        Task<AboutPage> GetByIdAsync(int aboutPageId);
        Task<List<AboutPage>> GetAllAsync(bool includeDeleted = false);
        Task<AboutPage> GetActiveAboutPageAsync();
        Task<AboutPage> GetBySlugAsync(string slug);
        void Add(AboutPage aboutPage);
        void Update(AboutPage aboutPage);
        void Delete(AboutPage aboutPage);
    }
}
