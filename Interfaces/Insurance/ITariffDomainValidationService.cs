using System.Threading.Tasks;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// رابط اعتبارسنجی قواعد دامنه برای تعرفه‌های بیمه
    /// Domain Validation Interface for Insurance Tariffs
    /// </summary>
    public interface ITariffDomainValidationService
    {
        /// <summary>
        /// اعتبارسنجی قواعد مالی تعرفه
        /// </summary>
        ServiceResult ValidateFinancialRules(InsuranceTariff tariff);

        /// <summary>
        /// اعتبارسنجی قواعد کسب‌وکار تعرفه
        /// </summary>
        Task<ServiceResult> ValidateBusinessRulesAsync(InsuranceTariff tariff);

        /// <summary>
        /// اعتبارسنجی قواعد رَوندینگ
        /// </summary>
        ServiceResult ValidateRoundingRules(InsuranceTariff tariff);

        /// <summary>
        /// اعتبارسنجی کامل تعرفه (همه قواعد)
        /// </summary>
        Task<ServiceResult> ValidateTariffAsync(InsuranceTariff tariff);
    }
}
