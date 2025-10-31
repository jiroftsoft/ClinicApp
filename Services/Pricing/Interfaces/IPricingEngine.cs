using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Services.Pricing.Models;

namespace ClinicApp.Services.Pricing.Interfaces
{
    /// <summary>
    /// Interface برای موتور محاسبه قیمت‌گذاری
    /// </summary>
    public interface IPricingEngine
    {
        /// <summary>
        /// پیش‌محاسبه قیمت خدمت با شکستن سهم‌ها
        /// </summary>
        Task<QuoteResultDto> QuoteAsync(QuoteRequestDto request, CancellationToken ct = default);

        /// <summary>
        /// محاسبه مجدد همه آیتم‌های یک پذیرش (Reprice-on-change)
        /// </summary>
        Task RepriceReceptionAsync(int receptionId, CancellationToken ct = default);
    }
}
