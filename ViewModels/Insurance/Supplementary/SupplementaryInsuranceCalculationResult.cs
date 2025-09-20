using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// نتیجه محاسبه بیمه تکمیلی - طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class SupplementaryInsuranceCalculationResult
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
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// پوشش بیمه اصلی
        /// </summary>
        [Display(Name = "پوشش بیمه اصلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PrimaryInsuranceCoverage { get; set; }

        /// <summary>
        /// پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "پوشش بیمه تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SupplementaryInsuranceCoverage { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal FinalPatientShare { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        [Display(Name = "تاریخ محاسبه")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// آیا کاملاً پوشش داده شده است؟
        /// </summary>
        [Display(Name = "کاملاً پوشش داده شده")]
        public bool IsFullyCovered { get; set; }

        /// <summary>
        /// شناسه تعرفه
        /// </summary>
        public int? TariffId { get; set; }

        /// <summary>
        /// شناسه طرح بیمه تکمیلی
        /// </summary>
        public int? SupplementaryPlanId { get; set; }

        /// <summary>
        /// درصد پوشش کل (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "درصد پوشش کل")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent => ServiceAmount > 0 ? ((PrimaryInsuranceCoverage + SupplementaryInsuranceCoverage) / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد پوشش بیمه اصلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه اصلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal PrimaryCoveragePercent => ServiceAmount > 0 ? (PrimaryInsuranceCoverage / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش بیمه تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal SupplementaryCoveragePercent => ServiceAmount > 0 ? (SupplementaryInsuranceCoverage / ServiceAmount) * 100 : 0;

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        [Display(Name = "درصد سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal PatientSharePercent => ServiceAmount > 0 ? (FinalPatientShare / ServiceAmount) * 100 : 0;

        /// <summary>
        /// مبلغ باقی‌مانده پس از بیمه اصلی
        /// </summary>
        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmountAfterPrimary => ServiceAmount - PrimaryInsuranceCoverage;

        /// <summary>
        /// آیا بیمه تکمیلی اعمال شده است؟
        /// </summary>
        [Display(Name = "بیمه تکمیلی اعمال شده")]
        public bool IsSupplementaryApplied => SupplementaryInsuranceCoverage > 0;

        /// <summary>
        /// صرفه‌جویی بیمار از بیمه تکمیلی
        /// </summary>
        [Display(Name = "صرفه‌جویی از بیمه تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientSavings => RemainingAmountAfterPrimary - FinalPatientShare;
    }
}
