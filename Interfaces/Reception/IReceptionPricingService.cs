using System.Threading.Tasks;
using ClinicApp.Controllers.Api;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// ✅ سرویس محاسبه قیمت‌گذاری پذیرش
    /// ارائه خروجی متحدالشکل برای UI
    /// </summary>
    public interface IReceptionPricingService
    {
        /// <summary>
        /// محاسبه جزئیات قیمت یک آیتم
        /// </summary>
        Task<PricingBreakdownDto> PriceItemAsync(int receptionId, int receptionItemId);

        /// <summary>
        /// محاسبه جمع‌های پذیرش (مجموع همه آیتم‌ها)
        /// </summary>
        Task<ReceptionTotalsDto> CalculateTotalsAsync(int receptionId);

        /// <summary>
        /// محاسبه مجدد همه آیتم‌های پذیرش (برای زمانی که بیمه تغییر می‌کند)
        /// </summary>
        Task RepriceAllAsync(int receptionId);
    }
}

