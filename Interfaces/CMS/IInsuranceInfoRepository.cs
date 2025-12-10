using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface Repository برای عملیات داده‌ای InsuranceInfo
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public interface IInsuranceInfoRepository
    {
        Task<InsuranceInfo> GetByIdAsync(int insuranceInfoId);
        Task<List<InsuranceInfo>> GetAllAsync(bool includeDeleted = false);
        Task<List<InsuranceInfo>> GetActiveInsurancesAsync(string insuranceType = null);
        Task<List<InsuranceInfo>> GetFeaturedInsurancesAsync(int count = 5);
        Task<List<InsuranceInfo>> GetByTypeAsync(string insuranceType, int count = 10);
        Task<List<InsuranceInfo>> SearchInsurancesAsync(string searchTerm);
        Task<List<string>> GetInsuranceTypesAsync();
        Task<InsuranceInfo> GetBySlugAsync(string slug);
        void Add(InsuranceInfo insuranceInfo);
        void Update(InsuranceInfo insuranceInfo);
        void Delete(InsuranceInfo insuranceInfo);
    }
}

