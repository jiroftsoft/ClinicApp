using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Repository interface for PatientEducationMaterial entity operations
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IPatientEducationMaterialRepository
    {
        Task<PatientEducationMaterial> GetByIdAsync(int materialId);
        Task<List<PatientEducationMaterial>> GetAllAsync(bool includeDeleted = false);
        Task<List<PatientEducationMaterial>> GetPublishedAsync(bool includeDeleted = false);
        Task<List<PatientEducationMaterial>> GetByCategoryAsync(PatientEducationCategory category, bool includeDeleted = false);
        Task<List<PatientEducationMaterial>> GetFeaturedAsync(int count = 10, bool includeDeleted = false);
        Task<List<PatientEducationMaterial>> SearchAsync(string searchTerm, PatientEducationCategory? category, bool? isPublished, bool? isFeatured, bool includeDeleted = false);
        Task<PatientEducationMaterial> GetBySlugAsync(string slug);
        void Add(PatientEducationMaterial material);
        void Update(PatientEducationMaterial material);
        void Delete(PatientEducationMaterial material);
        Task<bool> ExistsAsync(int materialId);
        Task IncrementDownloadCountAsync(int materialId);
        Task IncrementViewCountAsync(int materialId);
    }
}

