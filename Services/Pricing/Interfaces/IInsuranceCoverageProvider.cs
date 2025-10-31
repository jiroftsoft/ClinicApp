using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Services.Pricing.Models;

namespace ClinicApp.Services.Pricing.Interfaces
{
    /// <summary>
    /// Interface برای provider قواعد پوشش بیمه
    /// </summary>
    public interface IInsuranceCoverageProvider
    {
        /// <summary>
        /// دریافت قاعده پوشش بیمه پایه
        /// </summary>
        Task<CoverageRule> GetPrimaryRuleAsync(int insurancePlanId, int serviceId, int departmentId, int doctorId, int financialYearId, CancellationToken ct = default);

        /// <summary>
        /// دریافت قاعده پوشش بیمه تکمیلی
        /// </summary>
        Task<CoverageRule> GetSupplementaryRuleAsync(int insurancePlanId, int serviceId, int departmentId, int doctorId, int financialYearId, CancellationToken ct = default);
    }
}
