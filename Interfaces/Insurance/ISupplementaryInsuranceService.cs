using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس بیمه تکمیلی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبات پیچیده بیمه تکمیلی
    /// 2. مدیریت تنظیمات تخصصی
    /// 3. پشتیبانی از سناریوهای مختلف
    /// 4. رعایت استانداردهای پزشکی ایران
    /// </summary>
    public interface ISupplementaryInsuranceService
    {
        #region Supplementary Insurance Calculation

        /// <summary>
        /// محاسبه بیمه تکمیلی با تنظیمات خاص
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryCoverage">پوشش بیمه اصلی</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementaryCalculationResult>> CalculateSupplementaryInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate);

        #endregion

        #region Supplementary Settings Management

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>تنظیمات بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementarySettings>> GetSupplementarySettingsAsync(int planId);

        /// <summary>
        /// به‌روزرسانی تنظیمات بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="settings">تنظیمات جدید</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdateSupplementarySettingsAsync(int planId, SupplementarySettings settings);

        #endregion

        #region Supplementary Tariff Management

        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه تکمیلی</returns>
        Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetSupplementaryTariffsAsync(int planId);

        #endregion

        #region Advanced Supplementary Insurance Calculation

        /// <summary>
        /// محاسبه پیشرفته بیمه تکمیلی با الگوریتم‌های پیچیده
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryCoverage">میزان پوشش بیمه اصلی</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="advancedSettings">تنظیمات پیشرفته (اختیاری)</param>
        /// <returns>نتیجه محاسبه پیشرفته بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementaryCalculationResult>> CalculateAdvancedSupplementaryInsuranceAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            Dictionary<string, object> advancedSettings = null);

        /// <summary>
        /// محاسبه مقایسه‌ای بیمه‌های تکمیلی مختلف
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryCoverage">میزان پوشش بیمه اصلی</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="supplementaryPlanIds">لیست شناسه‌های طرح‌های بیمه تکمیلی برای مقایسه (اختیاری)</param>
        /// <returns>لیست نتایج مقایسه‌ای بیمه‌های تکمیلی</returns>
        Task<ServiceResult<List<SupplementaryCalculationResult>>> CompareSupplementaryInsuranceOptionsAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            List<int> supplementaryPlanIds = null);

        #endregion
    }
}
