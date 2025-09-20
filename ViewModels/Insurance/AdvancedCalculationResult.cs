using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance
{
    /// <summary>
    /// نتیجه محاسبه پیشرفته بیمه
    /// </summary>
    public class AdvancedCalculationResult
    {
        /// <summary>
        /// مبلغ کل خدمت
        /// </summary>
        [Display(Name = "مبلغ کل خدمت")]
        public decimal ServiceAmount { get; set; }

        /// <summary>
        /// کل پوشش بیمه
        /// </summary>
        [Display(Name = "کل پوشش بیمه")]
        public decimal TotalCoverage { get; set; }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        public decimal FinalPatientShare { get; set; }

        /// <summary>
        /// درصد پوشش کل
        /// </summary>
        [Display(Name = "درصد پوشش کل")]
        public decimal CoveragePercentage { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// جزئیات پوشش‌های بیمه
        /// </summary>
        [Display(Name = "جزئیات پوشش‌ها")]
        public List<InsuranceCoverageDetail> Coverages { get; set; } = new List<InsuranceCoverageDetail>();

        /// <summary>
        /// تعداد بیمه‌های اعمال شده
        /// </summary>
        [Display(Name = "تعداد بیمه‌های اعمال شده")]
        public int AppliedInsurancesCount => Coverages.Count;

        /// <summary>
        /// آیا محاسبه موفق بوده؟
        /// </summary>
        [Display(Name = "محاسبه موفق")]
        public bool IsSuccessful => TotalCoverage > 0;

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Notes { get; set; }
    }
}
