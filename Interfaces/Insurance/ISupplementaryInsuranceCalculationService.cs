using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس محاسبه بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public interface ISupplementaryInsuranceCalculationService
    {
        /// <summary>
        /// محاسبه بیمه تکمیلی بر اساس باقی‌مانده بیمه اصلی
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryInsuranceCoverage">پوشش بیمه اصلی</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementaryInsuranceCalculationResult>> CalculateSupplementaryInsuranceAsync(
            int patientId,
            int serviceId,
            decimal serviceAmount,
            decimal primaryInsuranceCoverage,
            DateTime calculationDate);

        /// <summary>
        /// محاسبه بیمه تکمیلی برای چندین خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمت</param>
        /// <param name="serviceAmounts">لیست مبالغ خدمات</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>لیست نتایج محاسبه بیمه تکمیلی</returns>
        Task<ServiceResult<List<SupplementaryInsuranceCalculationResult>>> CalculateSupplementaryInsuranceForServicesAsync(
            int patientId,
            List<int> serviceIds,
            List<decimal> serviceAmounts,
            DateTime calculationDate);

        /// <summary>
        /// محاسبه بیمه تکمیلی برای سناریو خاص (بدون وابستگی به بیمار)
        /// </summary>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryCoverage">پوشش بیمه اصلی</param>
        /// <param name="supplementaryCoveragePercent">درصد پوشش بیمه تکمیلی</param>
        /// <param name="supplementaryMaxPayment">سقف پرداخت بیمه تکمیلی</param>
        /// <returns>نتیجه محاسبه بیمه تکمیلی</returns>
        SupplementaryInsuranceCalculationResult CalculateForSpecificScenario(
            decimal serviceAmount,
            decimal primaryCoverage,
            decimal supplementaryCoveragePercent,
            decimal? supplementaryMaxPayment = null);
    }
}
