using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Insurance
{
    /// <summary>
    /// جزئیات محاسبه پوشش بیمه
    /// </summary>
    public class InsuranceCoverageDetail
    {
        /// <summary>
        /// شناسه بیمه
        /// </summary>
        [Display(Name = "شناسه بیمه")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// نام بیمه
        /// </summary>
        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

        /// <summary>
        /// اولویت بیمه
        /// </summary>
        [Display(Name = "اولویت")]
        public InsurancePriority Priority { get; set; }

        /// <summary>
        /// درصد پوشش
        /// </summary>
        [Display(Name = "درصد پوشش")]
        public decimal Percentage { get; set; }

        /// <summary>
        /// حداکثر مبلغ پرداخت
        /// </summary>
        [Display(Name = "حداکثر مبلغ")]
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// حداقل مبلغ پرداخت
        /// </summary>
        [Display(Name = "حداقل مبلغ")]
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// مبلغ محاسبه شده
        /// </summary>
        [Display(Name = "مبلغ محاسبه شده")]
        public decimal CalculatedCoverage { get; set; }

        /// <summary>
        /// مبلغ واقعی پوشش
        /// </summary>
        [Display(Name = "مبلغ واقعی پوشش")]
        public decimal ActualCoverage { get; set; }

        /// <summary>
        /// آیا این بیمه اعمال شده؟
        /// </summary>
        [Display(Name = "اعمال شده")]
        public bool IsApplied { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Notes { get; set; }
    }
}
