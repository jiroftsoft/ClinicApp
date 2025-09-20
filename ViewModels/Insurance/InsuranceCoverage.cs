using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Insurance
{
    /// <summary>
    /// اطلاعات پوشش بیمه برای محاسبه پیشرفته
    /// </summary>
    public class InsuranceCoverage
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
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد")]
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
        /// آیا این بیمه فعال است؟
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }
    }
}
