using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت مطالب آموزشی بیماران
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IPatientEducationMaterialService
    {
        Task<ServiceResult<PagedResult<PatientEducationMaterialIndexViewModel>>> GetMaterialsAsync(PatientEducationMaterialSearchViewModel searchModel);
        Task<ServiceResult<List<PatientEducationMaterialIndexViewModel>>> GetPublishedMaterialsAsync(int count = 10);
        Task<ServiceResult<List<PatientEducationMaterialIndexViewModel>>> GetFeaturedMaterialsAsync(int count = 5);
        Task<ServiceResult<PatientEducationMaterialDetailsViewModel>> GetMaterialDetailsAsync(int materialId);
        Task<ServiceResult<PatientEducationMaterialDetailsViewModel>> GetMaterialBySlugAsync(string slug);
        Task<ServiceResult<PatientEducationMaterialCreateEditViewModel>> GetMaterialForEditAsync(int materialId);
        Task<ServiceResult<PatientEducationMaterial>> CreateMaterialAsync(PatientEducationMaterialCreateEditViewModel model);
        Task<ServiceResult<PatientEducationMaterial>> UpdateMaterialAsync(PatientEducationMaterialCreateEditViewModel model);
        Task<ServiceResult> DeleteMaterialAsync(int materialId);
        Task<ServiceResult> PublishMaterialAsync(int materialId);
        Task<ServiceResult> UnpublishMaterialAsync(int materialId);
        Task<ServiceResult> SetFeaturedAsync(int materialId, bool isFeatured);
        Task<ServiceResult> IncrementDownloadCountAsync(int materialId);
        Task<ServiceResult> IncrementViewCountAsync(int materialId);
    }
}

