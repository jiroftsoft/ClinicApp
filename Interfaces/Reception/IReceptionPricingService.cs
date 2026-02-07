using System.Threading.Tasks;
using System.Collections.Generic;
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
        /// ✅ بهبود یافته: برگرداندن totals و pricings برای UI
        /// </summary>
        Task<(ReceptionTotalsDto totals, List<PricingBreakdownDto> pricings)> RepriceAllAsync(int receptionId);

        /// <summary>
        /// ✅ بررسی وجود تعیین‌ست بیمه‌ای برای خدمت (قبل از افزودن/تغییر)
        /// </summary>
        /// <returns>(ok, code, message, meta) - ok=true اگر تعیین‌ست موجود است</returns>
        Task<(bool ok, string code, string message, object meta)> CheckInsuranceSetAsync(
            int serviceId, 
            int? departmentId, 
            int? doctorId, 
            int financialYearId, 
            int? basePlanId, 
            int? suppPlanId);

        /// <summary>
        /// ✅ وضعیت تعیین‌ست بیمه برای چند خدمت (برای نمایش در لیست انتخاب خدمت)
        /// </summary>
        /// <param name="serviceIds">لیست شناسه خدمات</param>
        /// <param name="basePlanId">بیمه پایه (اختیاری)</param>
        /// <param name="suppPlanId">بیمه تکمیلی (اختیاری)</param>
        /// <returns>برای هر serviceId: (hasTariffSet, warningMessage)</returns>
        Task<Dictionary<int, (bool hasTariffSet, string warning)>> GetServicesTariffStatusAsync(
            IReadOnlyList<int> serviceIds,
            int? basePlanId,
            int? suppPlanId);
    }
}

