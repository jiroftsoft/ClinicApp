using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Insurance
{
    /// <summary>
    /// جزئیات بیمه تکمیلی در محاسبه ترکیبی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ذخیره اطلاعات هر بیمه تکمیلی در محاسبه
    /// 2. ردیابی اولویت و مبلغ پوشش
    /// 3. پشتیبانی از چندین بیمه تکمیلی
    /// 4. مناسب برای گزارش‌گیری و تحلیل
    /// </summary>
    public class SupplementaryInsuranceDetail
    {
        /// <summary>
        /// شناسه بیمه تکمیلی
        /// </summary>
        [Display(Name = "شناسه بیمه تکمیلی")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// مبلغ پوشش این بیمه تکمیلی
        /// </summary>
        [Display(Name = "مبلغ پوشش")]
        public decimal Coverage { get; set; }

        /// <summary>
        /// اولویت این بیمه تکمیلی
        /// </summary>
        [Display(Name = "اولویت")]
        public InsurancePriority Priority { get; set; }

        /// <summary>
        /// درصد پوشش این بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercent { get; set; }

        /// <summary>
        /// نام بیمه تکمیلی
        /// </summary>
        [Display(Name = "نام بیمه")]
        public string InsuranceName { get; set; }

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
