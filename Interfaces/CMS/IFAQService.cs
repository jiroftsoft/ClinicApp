using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت سوالات متداول (FAQ)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IFAQService
    {
        Task<ServiceResult<PagedResult<FAQIndexViewModel>>> GetFAQsAsync(FAQSearchViewModel filter);
        Task<ServiceResult<FAQDetailsViewModel>> GetFAQDetailsAsync(int faqId);
        Task<ServiceResult<FAQCreateEditViewModel>> GetFAQForEditAsync(int faqId);
        Task<ServiceResult<FAQ>> CreateFAQAsync(FAQCreateEditViewModel model);
        Task<ServiceResult<FAQ>> UpdateFAQAsync(FAQCreateEditViewModel model);
        Task<ServiceResult> DeleteFAQAsync(int faqId);
        Task<ServiceResult> ActivateFAQAsync(int faqId);
        Task<ServiceResult> DeactivateFAQAsync(int faqId);
        Task<ServiceResult> SetFeaturedAsync(int faqId, bool isFeatured);
        Task<ServiceResult> IncrementViewCountAsync(int faqId);
        Task<ServiceResult<List<FAQPublicViewModel>>> GetPublicFAQsAsync(string category = null);
        Task<ServiceResult<List<FAQPublicViewModel>>> GetFeaturedFAQsAsync(int count = 5);
        Task<ServiceResult<List<FAQCategoryViewModel>>> GetCategoriesAsync();
        Task<ServiceResult<List<FAQPublicViewModel>>> SearchFAQsAsync(string searchTerm);
        Task<ServiceResult<FAQ>> GetBySlugAsync(string slug);
    }
}

