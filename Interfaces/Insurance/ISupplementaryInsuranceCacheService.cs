using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Insurance;
using ClinicApp.ViewModels.Insurance.Supplementary;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Interface برای سرویس Cache بیمه تکمیلی
    /// طراحی شده برای بهبود Performance در سیستم‌های پزشکی
    /// </summary>
    public interface ISupplementaryInsuranceCacheService
    {
        /// <summary>
        /// دریافت تعرفه‌های بیمه تکمیلی از Cache یا Database
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست تعرفه‌های بیمه تکمیلی</returns>
        Task<ServiceResult<List<SupplementaryTariffViewModel>>> GetCachedSupplementaryTariffsAsync(int planId);

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی از Cache یا Database
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>تنظیمات بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementarySettings>> GetCachedSupplementarySettingsAsync(int planId);

        /// <summary>
        /// دریافت نتیجه محاسبه از Cache یا محاسبه جدید
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="primaryCoverage">میزان پوشش بیمه اصلی</param>
        /// <param name="calculationDate">تاریخ محاسبه</param>
        /// <param name="supplementaryService">سرویس بیمه تکمیلی</param>
        /// <returns>نتیجه محاسبه بیمه تکمیلی</returns>
        Task<ServiceResult<SupplementaryCalculationResult>> GetCachedCalculationResultAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            decimal primaryCoverage, 
            DateTime calculationDate,
            ISupplementaryInsuranceService supplementaryService);

        /// <summary>
        /// پاک کردن Cache
        /// </summary>
        /// <param name="cacheType">نوع Cache (اختیاری)</param>
        void ClearCache(string cacheType = null);

        /// <summary>
        /// پاک کردن Cache های منقضی شده
        /// </summary>
        void CleanExpiredCache();

        /// <summary>
        /// دریافت آمار Cache
        /// </summary>
        /// <returns>آمار Cache</returns>
        CacheStatistics GetCacheStatistics();
    }
}
