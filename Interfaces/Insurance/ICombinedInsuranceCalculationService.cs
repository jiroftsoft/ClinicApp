using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Patient;
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
        /// محاسبه بیمه بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="receptionDate">تاریخ پذیرش</param>
        /// <returns>نتیجه محاسبه بیمه</returns>
        Task<ServiceResult<object>> CalculatePatientInsuranceForReceptionAsync(int patientId, System.Collections.Generic.List<int> serviceIds, System.DateTime receptionDate);

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

        /// <summary>
        /// محاسبه پیشرفته بیمه ترکیبی با در نظر گیری تنظیمات خاص
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="customSettings">تنظیمات خاص (اختیاری)</param>
        /// <returns>نتیجه محاسبه پیشرفته بیمه ترکیبی</returns>
        Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateAdvancedCombinedInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate,
            Dictionary<string, object> customSettings = null);

        /// <summary>
        /// محاسبه مقایسه‌ای بیمه‌های مختلف
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="insurancePlanIds">لیست شناسه‌های طرح‌های بیمه برای مقایسه (اختیاری)</param>
        /// <returns>لیست نتایج مقایسه‌ای بیمه‌ها</returns>
        Task<ServiceResult<List<CombinedInsuranceCalculationResult>>> CompareInsuranceOptionsAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime calculationDate,
            List<int> insurancePlanIds = null);

        /// <summary>
        /// دریافت بیمه‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        Task<ServiceResult<List<PatientInsurance>>> GetPatientInsurancesAsync(int patientId);

        /// <summary>
        /// دریافت لیست بیماران فعال برای محاسبه بیمه
        /// </summary>
        /// <returns>لیست بیماران فعال</returns>
        Task<ServiceResult<List<PatientLookupItem>>> GetActivePatientsAsync();

        /// <summary>
        /// دریافت لیست خدمات فعال برای محاسبه بیمه
        /// </summary>
        /// <returns>لیست خدمات فعال</returns>
        Task<ServiceResult<List<ServiceLookupItem>>> GetActiveServicesAsync();
    }
}
