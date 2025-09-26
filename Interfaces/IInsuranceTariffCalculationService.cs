using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس محاسبه تعرفه بیمه
    /// </summary>
    public interface IInsuranceTariffCalculationService
    {
        /// <summary>
        /// محاسبه قیمت تعرفه با استفاده از FactorSetting
        /// </summary>
        Task<decimal> CalculateTariffPriceWithFactorSettingAsync(int serviceId, decimal? currentTariffPrice, string correlationId);

        /// <summary>
        /// محاسبه مبلغ پایه خدمت بر اساس ServiceComponents و FactorSettings
        /// </summary>
        Task<decimal> CalculateServiceBasePriceAsync(int serviceId);

        /// <summary>
        /// بررسی تکراری بودن تعرفه
        /// </summary>
        Task<bool> IsTariffDuplicateAsync(int insurancePlanId, int? serviceId, bool isAllServices);
    }
}
