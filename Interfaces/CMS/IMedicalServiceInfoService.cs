using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت اطلاعات خدمات پزشکی (Medical Service Information)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IMedicalServiceInfoService
    {
        Task<ServiceResult<PagedResult<MedicalServiceInfoIndexViewModel>>> GetMedicalServiceInfosAsync(MedicalServiceInfoSearchViewModel filter);
        Task<ServiceResult<MedicalServiceInfoDetailsViewModel>> GetMedicalServiceInfoDetailsAsync(int medicalServiceInfoId);
        Task<ServiceResult<MedicalServiceInfoCreateEditViewModel>> GetMedicalServiceInfoForEditAsync(int medicalServiceInfoId);
        Task<ServiceResult<MedicalServiceInfo>> CreateMedicalServiceInfoAsync(MedicalServiceInfoCreateEditViewModel model);
        Task<ServiceResult<MedicalServiceInfo>> UpdateMedicalServiceInfoAsync(MedicalServiceInfoCreateEditViewModel model);
        Task<ServiceResult> DeleteMedicalServiceInfoAsync(int medicalServiceInfoId);
        Task<ServiceResult> ActivateMedicalServiceInfoAsync(int medicalServiceInfoId);
        Task<ServiceResult> DeactivateMedicalServiceInfoAsync(int medicalServiceInfoId);
        Task<ServiceResult> SetFeaturedAsync(int medicalServiceInfoId, bool isFeatured);
        Task<ServiceResult> IncrementViewCountAsync(int medicalServiceInfoId);
        Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetPublicServiceInfosAsync(int? serviceCategoryId = null);
        Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetFeaturedServiceInfosAsync(int count = 6);
        Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> GetByServiceCategoryAsync(int serviceCategoryId, int count = 10);
        Task<ServiceResult<List<MedicalServiceInfoPublicViewModel>>> SearchServiceInfosAsync(string searchTerm);
        Task<ServiceResult<MedicalServiceInfo>> GetBySlugAsync(string slug);
        Task<ServiceResult<MedicalServiceInfo>> GetByServiceIdAsync(int serviceId);
    }
}

