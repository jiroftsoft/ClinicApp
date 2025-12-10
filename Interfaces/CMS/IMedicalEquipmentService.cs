using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface سرویس مدیریت تجهیزات پزشکی (Medical Equipment)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IMedicalEquipmentService
    {
        Task<ServiceResult<PagedResult<MedicalEquipmentIndexViewModel>>> GetMedicalEquipmentsAsync(MedicalEquipmentSearchViewModel filter);
        Task<ServiceResult<MedicalEquipmentDetailsViewModel>> GetMedicalEquipmentDetailsAsync(int medicalEquipmentId);
        Task<ServiceResult<MedicalEquipmentCreateEditViewModel>> GetMedicalEquipmentForEditAsync(int medicalEquipmentId);
        Task<ServiceResult<MedicalEquipment>> CreateMedicalEquipmentAsync(MedicalEquipmentCreateEditViewModel model);
        Task<ServiceResult<MedicalEquipment>> UpdateMedicalEquipmentAsync(MedicalEquipmentCreateEditViewModel model);
        Task<ServiceResult> DeleteMedicalEquipmentAsync(int medicalEquipmentId);
        Task<ServiceResult> ActivateMedicalEquipmentAsync(int medicalEquipmentId);
        Task<ServiceResult> DeactivateMedicalEquipmentAsync(int medicalEquipmentId);
        Task<ServiceResult> SetFeaturedAsync(int medicalEquipmentId, bool isFeatured);
        Task<ServiceResult> IncrementViewCountAsync(int medicalEquipmentId);
        Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetActiveEquipmentsAsync();
        Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetFeaturedEquipmentsAsync(int count = 6);
        Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> GetByCategoryAsync(string category);
        Task<ServiceResult<List<MedicalEquipmentPublicViewModel>>> SearchEquipmentsAsync(string searchTerm);
        Task<ServiceResult<MedicalEquipment>> GetBySlugAsync(string slug);
    }
}

