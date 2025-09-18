using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس محاسبه بیمه ترکیبی (اصلی + تکمیلی)
    /// طراحی شده برای کلینیک‌های درمانی
    /// </summary>
    public interface ICombinedInsuranceCalculationService
    {
        /// <summary>
        /// محاسبه ترکیبی بیمه اصلی و تکمیلی برای یک خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه بیمه ترکیبی</returns>
        Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate);

        /// <summary>
        /// محاسبه بیمه ترکیبی برای چندین خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="serviceAmounts">لیست مبالغ خدمات</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>لیست نتایج محاسبه بیمه ترکیبی</returns>
        Task<ServiceResult<List<CombinedInsuranceCalculationResult>>> CalculateCombinedInsuranceForServicesAsync(
            int patientId, 
            List<int> serviceIds, 
            List<decimal> serviceAmounts, 
            DateTime calculationDate);
    }
}
