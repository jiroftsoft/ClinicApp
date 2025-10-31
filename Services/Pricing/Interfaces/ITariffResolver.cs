using System.Threading;
using System.Threading.Tasks;

namespace ClinicApp.Services.Pricing.Interfaces
{
    /// <summary>
    /// Interface برای resolver تعرفه خدمت
    /// </summary>
    public interface ITariffResolver
    {
        /// <summary>
        /// محاسبه تعرفه مصوب خدمت
        /// مسیر 1: اگر Service.Price موجود باشد، همان استفاده می‌شود
        /// مسیر 2: اگر نبود، از ServiceComponent × FactorSetting محاسبه می‌شود
        /// </summary>
        Task<long> ResolveApprovedTariffAsync(int serviceId, int clinicId, int departmentId, int financialYearId, CancellationToken ct = default);
    }
}
