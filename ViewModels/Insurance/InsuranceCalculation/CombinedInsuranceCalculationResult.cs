using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// نتیجه محاسبه بیمه ترکیبی (اصلی + تکمیلی)
    /// طراحی شده برای کلینیک‌های درمانی
    /// </summary>
    public class CombinedInsuranceCalculationResult
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
        /// مبلغ کل خدمت
        /// </summary>
        [Display(Name = "مبلغ کل خدمت")]
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// شناسه بیمه اصلی
        /// </summary>
        public int PrimaryInsuranceId { get; set; }

        /// <summary>
        /// پوشش بیمه اصلی
        /// </summary>
        [Display(Name = "پوشش بیمه اصلی")]
        public decimal PrimaryCoverage { get; set; }

        /// <summary>
        /// درصد پوشش بیمه اصلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه اصلی")]
        public decimal PrimaryCoveragePercent { get; set; }

        /// <summary>
        /// شناسه بیمه تکمیلی (در صورت وجود)
        /// </summary>
        public int? SupplementaryInsuranceId { get; set; }

        /// <summary>
        /// پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "پوشش بیمه تکمیلی")]
        public decimal SupplementaryCoverage { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه تکمیلی")]
        public decimal SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        public decimal FinalPatientShare { get; set; }

        /// <summary>
        /// کل پوشش بیمه (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "کل پوشش بیمه")]
        public decimal TotalInsuranceCoverage { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// آیا بیمه تکمیلی دارد؟
        /// </summary>
        [Display(Name = "بیمه تکمیلی")]
        public bool HasSupplementaryInsurance { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Notes { get; set; }

        /// <summary>
        /// درصد کل پوشش بیمه
        /// </summary>
        [Display(Name = "درصد کل پوشش")]
        public decimal TotalCoveragePercent => ServiceAmount > 0 ? (TotalInsuranceCoverage / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        [Display(Name = "درصد سهم بیمار")]
        public decimal PatientSharePercent => ServiceAmount > 0 ? (FinalPatientShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// صرفه‌جویی بیمار از بیمه تکمیلی
        /// </summary>
        [Display(Name = "صرفه‌جویی از بیمه تکمیلی")]
        public decimal SupplementarySavings => HasSupplementaryInsurance ? SupplementaryCoverage : 0;

        /// <summary>
        /// فرمت مبلغ برای نمایش
        /// </summary>
        public string FormattedServiceAmount => ServiceAmount.ToString("N0") + " تومان";

        /// <summary>
        /// فرمت پوشش بیمه اصلی برای نمایش
        /// </summary>
        public string FormattedPrimaryCoverage => PrimaryCoverage.ToString("N0") + " تومان";

        /// <summary>
        /// فرمت پوشش بیمه تکمیلی برای نمایش
        /// </summary>
        public string FormattedSupplementaryCoverage => SupplementaryCoverage.ToString("N0") + " تومان";

        /// <summary>
        /// فرمت سهم نهایی بیمار برای نمایش
        /// </summary>
        public string FormattedFinalPatientShare => FinalPatientShare.ToString("N0") + " تومان";

        /// <summary>
        /// فرمت کل پوشش بیمه برای نمایش
        /// </summary>
        public string FormattedTotalInsuranceCoverage => TotalInsuranceCoverage.ToString("N0") + " تومان";

        /// <summary>
        /// وضعیت پوشش (کامل، ناقص، بدون پوشش)
        /// </summary>
        public string CoverageStatus
        {
            get
            {
                if (TotalCoveragePercent >= 100)
                    return "پوشش کامل";
                else if (TotalCoveragePercent > 0)
                    return "پوشش ناقص";
                else
                    return "بدون پوشش";
            }
        }

        /// <summary>
        /// رنگ وضعیت پوشش
        /// </summary>
        public string CoverageStatusColor
        {
            get
            {
                if (TotalCoveragePercent >= 100)
                    return "success";
                else if (TotalCoveragePercent > 0)
                    return "warning";
                else
                    return "danger";
            }
        }
    }
}
