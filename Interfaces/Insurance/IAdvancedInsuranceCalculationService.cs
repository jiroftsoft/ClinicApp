using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس محاسبه پیشرفته بیمه
    /// </summary>
    public interface IAdvancedInsuranceCalculationService
    {
        /// <summary>
        /// محاسبه بهینه پوشش بیمه با الگوریتم پیشرفته
        /// </summary>
        /// <param name="serviceAmount">مبلغ کل خدمت</param>
        /// <param name="coverages">لیست پوشش‌های بیمه</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="advancedSettings">تنظیمات پیشرفته</param>
        /// <returns>نتیجه محاسبه پیشرفته</returns>
        Task<ServiceResult<AdvancedCalculationResult>> CalculateOptimalCoverageAsync(
            decimal serviceAmount,
            List<InsuranceCoverage> coverages,
            DateTime calculationDate,
            Dictionary<string, object> advancedSettings = null);
    }
}
