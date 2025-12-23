using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت صفحه "درباره ما" (About Page)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IAboutPageService
    {
        Task<ServiceResult<Interfaces.PagedResult<AboutPageIndexViewModel>>> GetAboutPagesAsync(AboutPageSearchViewModel filter);
        Task<ServiceResult<AboutPageDetailsViewModel>> GetAboutPageDetailsAsync(int aboutPageId);
        Task<ServiceResult<AboutPageCreateEditViewModel>> GetAboutPageForEditAsync(int aboutPageId);
        Task<ServiceResult<AboutPage>> CreateAboutPageAsync(AboutPageCreateEditViewModel model);
        Task<ServiceResult<AboutPage>> UpdateAboutPageAsync(AboutPageCreateEditViewModel model);
        Task<ServiceResult> DeleteAboutPageAsync(int aboutPageId);
        Task<ServiceResult> ActivateAboutPageAsync(int aboutPageId);
        Task<ServiceResult> DeactivateAboutPageAsync(int aboutPageId);
        Task<ServiceResult<AboutPagePublicViewModel>> GetActiveAboutPageAsync();
        Task<ServiceResult<AboutPage>> GetBySlugAsync(string slug);
    }
}
