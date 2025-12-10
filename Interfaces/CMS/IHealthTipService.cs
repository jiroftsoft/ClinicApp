using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت نکات سلامت (Health Tips)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IHealthTipService
    {
        Task<ServiceResult<PagedResult<HealthTipIndexViewModel>>> GetHealthTipsAsync(HealthTipSearchViewModel filter);
        Task<ServiceResult<HealthTipDetailsViewModel>> GetHealthTipDetailsAsync(int healthTipId);
        Task<ServiceResult<HealthTipCreateEditViewModel>> GetHealthTipForEditAsync(int healthTipId);
        Task<ServiceResult<HealthTip>> CreateHealthTipAsync(HealthTipCreateEditViewModel model);
        Task<ServiceResult<HealthTip>> UpdateHealthTipAsync(HealthTipCreateEditViewModel model);
        Task<ServiceResult> DeleteHealthTipAsync(int healthTipId);
        Task<ServiceResult> PublishHealthTipAsync(int healthTipId);
        Task<ServiceResult> UnpublishHealthTipAsync(int healthTipId);
        Task<ServiceResult> SetFeaturedAsync(int healthTipId, bool isFeatured);
        Task<ServiceResult> IncrementViewCountAsync(int healthTipId);
        Task<ServiceResult> IncrementShareCountAsync(int healthTipId);
        Task<ServiceResult<List<HealthTipPublicViewModel>>> GetPublicHealthTipsAsync(string category = null, int count = 10);
        Task<ServiceResult<List<HealthTipPublicViewModel>>> GetFeaturedHealthTipsAsync(int count = 5);
        Task<ServiceResult<List<HealthTipCategoryViewModel>>> GetCategoriesAsync();
        Task<ServiceResult<List<HealthTipPublicViewModel>>> SearchHealthTipsAsync(string searchTerm);
        Task<ServiceResult<HealthTip>> GetBySlugAsync(string slug);
    }
}

