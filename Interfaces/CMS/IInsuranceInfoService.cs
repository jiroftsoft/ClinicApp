using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت اطلاعات بیمه (Insurance Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IInsuranceInfoService
    {
        Task<ServiceResult<PagedResult<InsuranceInfoIndexViewModel>>> GetInsuranceInfosAsync(InsuranceInfoSearchViewModel filter);
        Task<ServiceResult<InsuranceInfoDetailsViewModel>> GetInsuranceInfoDetailsAsync(int insuranceInfoId);
        Task<ServiceResult<InsuranceInfoCreateEditViewModel>> GetInsuranceInfoForEditAsync(int insuranceInfoId);
        Task<ServiceResult<InsuranceInfo>> CreateInsuranceInfoAsync(InsuranceInfoCreateEditViewModel model);
        Task<ServiceResult<InsuranceInfo>> UpdateInsuranceInfoAsync(InsuranceInfoCreateEditViewModel model);
        Task<ServiceResult> DeleteInsuranceInfoAsync(int insuranceInfoId);
        Task<ServiceResult> ActivateInsuranceInfoAsync(int insuranceInfoId);
        Task<ServiceResult> DeactivateInsuranceInfoAsync(int insuranceInfoId);
        Task<ServiceResult> SetFeaturedAsync(int insuranceInfoId, bool isFeatured);
        Task<ServiceResult> IncrementViewCountAsync(int insuranceInfoId);
        Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> GetPublicInsuranceInfosAsync(string insuranceType = null);
        Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> GetFeaturedInsuranceInfosAsync(int count = 5);
        Task<ServiceResult<List<InsuranceInfoTypeViewModel>>> GetInsuranceTypesAsync();
        Task<ServiceResult<List<InsuranceInfoPublicViewModel>>> SearchInsuranceInfosAsync(string searchTerm);
        Task<ServiceResult<InsuranceInfo>> GetBySlugAsync(string slug);
    }
}

