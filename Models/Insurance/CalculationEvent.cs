using System;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// رویداد محاسبه بیمه تکمیلی
    /// </summary>
    public class CalculationEvent
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// مبلغ خدمت
        /// </summary>
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// میزان پوشش بیمه اصلی
        /// </summary>
        public decimal PrimaryCoverage { get; set; }

        /// <summary>
        /// میزان پوشش بیمه تکمیلی
        /// </summary>
        public decimal SupplementaryCoverage { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        public decimal FinalPatientShare { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// مدت زمان محاسبه (به میلی‌ثانیه)
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// آیا محاسبه موفق بوده است
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام خطا (در صورت وجود)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// شناسه طرح بیمه تکمیلی
        /// </summary>
        public int? SupplementaryPlanId { get; set; }

        /// <summary>
        /// شناسه طرح بیمه اصلی
        /// </summary>
        public int? PrimaryPlanId { get; set; }

        /// <summary>
        /// نوع محاسبه
        /// </summary>
        public string CalculationType { get; set; } = "Standard";

        /// <summary>
        /// نسخه الگوریتم محاسبه
        /// </summary>
        public string AlgorithmVersion { get; set; } = "1.0";

        /// <summary>
        /// تنظیمات خاص محاسبه
        /// </summary>
        public string CalculationSettings { get; set; }

        /// <summary>
        /// یادداشت‌های اضافی
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// زمان ایجاد رویداد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// دریافت خلاصه رویداد
        /// </summary>
        /// <returns>خلاصه رویداد محاسبه</returns>
        public string GetSummary()
        {
            return $"Calculation Event - PatientId: {PatientId}, ServiceId: {ServiceId}, " +
                   $"ServiceAmount: {ServiceAmount:N0}, PrimaryCoverage: {PrimaryCoverage:N0}, " +
                   $"SupplementaryCoverage: {SupplementaryCoverage:N0}, FinalPatientShare: {FinalPatientShare:N0}, " +
                   $"Success: {Success}, Duration: {Duration}ms";
        }

        /// <summary>
        /// محاسبه درصد پوشش کل
        /// </summary>
        /// <returns>درصد پوشش کل</returns>
        public decimal GetTotalCoveragePercent()
        {
            if (ServiceAmount <= 0) return 0;
            var totalCoverage = PrimaryCoverage + SupplementaryCoverage;
            return (totalCoverage / ServiceAmount) * 100;
        }

        /// <summary>
        /// محاسبه درصد سهم بیمار
        /// </summary>
        /// <returns>درصد سهم بیمار</returns>
        public decimal GetPatientSharePercent()
        {
            if (ServiceAmount <= 0) return 0;
            return (FinalPatientShare / ServiceAmount) * 100;
        }

        /// <summary>
        /// محاسبه صرفه‌جویی بیمار
        /// </summary>
        /// <returns>مبلغ صرفه‌جویی بیمار</returns>
        public decimal GetPatientSavings()
        {
            return ServiceAmount - FinalPatientShare;
        }

        /// <summary>
        /// محاسبه کارایی بیمه تکمیلی
        /// </summary>
        /// <returns>درصد کارایی بیمه تکمیلی</returns>
        public decimal GetSupplementaryEfficiency()
        {
            if (PrimaryCoverage <= 0) return 0;
            return (SupplementaryCoverage / PrimaryCoverage) * 100;
        }
    }
}
