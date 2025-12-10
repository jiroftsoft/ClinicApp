using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای MedicalServiceInfo
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IMedicalServiceInfoRepository
    {
        Task<MedicalServiceInfo> GetByIdAsync(int medicalServiceInfoId);
        Task<MedicalServiceInfo> GetByServiceIdAsync(int serviceId);
        Task<List<MedicalServiceInfo>> GetAllAsync(bool includeDeleted = false);
        Task<List<MedicalServiceInfo>> GetActiveServiceInfosAsync(int? serviceCategoryId = null);
        Task<List<MedicalServiceInfo>> GetFeaturedServiceInfosAsync(int count = 6);
        Task<List<MedicalServiceInfo>> GetByServiceCategoryAsync(int serviceCategoryId, int count = 10);
        Task<List<MedicalServiceInfo>> SearchServiceInfosAsync(string searchTerm);
        Task<MedicalServiceInfo> GetBySlugAsync(string slug);
        void Add(MedicalServiceInfo medicalServiceInfo);
        void Update(MedicalServiceInfo medicalServiceInfo);
        void Delete(MedicalServiceInfo medicalServiceInfo);
    }
}

