using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای نمایش نتیجه محاسبه بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش نتیجه محاسبه بیمه
    /// 2. نمایش جزئیات محاسبه
    /// 3. نمایش اطلاعات بیمار و خدمت
    /// 4. نمایش سهم بیمه و بیمار
    /// 5. نمایش تاریخ شمسی
    /// 
    /// نکته حیاتی: این ViewModel برای نمایش نتایج محاسبات بیمه طراحی شده است
    /// </summary>
    public class InsuranceCalculationResultViewModel
    {
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "تاریخ محاسبه")]
        public DateTime CalculationDate { get; set; }

        [Display(Name = "تاریخ محاسبه (شمسی)")]
        public string CalculationDateShamsi { get; set; }

        [Display(Name = "مبلغ کل خدمت")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "مبلغ فرانشیز")]
        public decimal DeductibleAmount { get; set; }

        [Display(Name = "مبلغ قابل پوشش")]
        public decimal CoverableAmount { get; set; }

        [Display(Name = "درصد پوشش بیمه")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "مبلغ پوشش بیمه")]
        public decimal InsuranceCoverage { get; set; }

        [Display(Name = "مبلغ پرداخت بیمار")]
        public decimal PatientPayment { get; set; }

        [Display(Name = "فرانشیز")]
        public decimal Deductible { get; set; }

        [Display(Name = "Copay")]
        public decimal Copay { get; set; }

        [Display(Name = "Coverage Override")]
        public decimal? CoverageOverride { get; set; }

        [Display(Name = "سهم بیمه")]
        public decimal InsuranceShare { get; set; }

        [Display(Name = "سهم بیمار")]
        public decimal PatientShare { get; set; }

        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool IsPrimary { get; set; }

        [Display(Name = "وضعیت محاسبه")]
        public string CalculationStatus { get; set; }

        [Display(Name = "پیام محاسبه")]
        public string CalculationMessage { get; set; }
    }
}
