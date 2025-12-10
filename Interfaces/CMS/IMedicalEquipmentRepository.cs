using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای MedicalEquipment
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IMedicalEquipmentRepository
    {
        Task<MedicalEquipment> GetByIdAsync(int medicalEquipmentId);
        Task<List<MedicalEquipment>> GetAllAsync(bool includeDeleted = false);
        Task<List<MedicalEquipment>> GetActiveEquipmentsAsync();
        Task<List<MedicalEquipment>> GetFeaturedEquipmentsAsync(int count = 6);
        Task<List<MedicalEquipment>> GetByCategoryAsync(string category);
        Task<List<MedicalEquipment>> SearchEquipmentsAsync(string searchTerm);
        Task<MedicalEquipment> GetBySlugAsync(string slug);
        void Add(MedicalEquipment medicalEquipment);
        void Update(MedicalEquipment medicalEquipment);
        void Delete(MedicalEquipment medicalEquipment);
    }
}

