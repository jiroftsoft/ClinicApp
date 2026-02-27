using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository دسته‌صورت‌حساب بیمه
    /// </summary>
    public interface IInsuranceBatchRepository
    {
        Task<InsuranceBatch> GetByIdAsync(int id);
        Task<InsuranceBatch> GetByBatchNumberAsync(string batchNumber);
        Task<List<InsuranceBatch>> GetByProviderIdAsync(int providerId, int pageSize = 50);
        Task<InsuranceBatch> AddAsync(InsuranceBatch entity);
        Task<InsuranceBatch> UpdateAsync(InsuranceBatch entity);
        Task<bool> SoftDeleteAsync(int id, string deletedByUserId);
    }
}
