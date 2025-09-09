using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای اعتبارسنجی بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش نتیجه اعتبارسنجی بیمه
    /// 2. نمایش جزئیات اعتبارسنجی
    /// 3. نمایش اطلاعات بیمار و خدمت
    /// 4. نمایش وضعیت پوشش و اعتبار
    /// 5. نمایش تاریخ شمسی
    /// 
    /// نکته حیاتی: این ViewModel برای نمایش نتایج اعتبارسنجی بیمه طراحی شده است
    /// </summary>
    public class InsuranceValidationViewModel
    {
        [Display(Name = "شناسه بیمه بیمار")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "تاریخ بررسی")]
        public DateTime CheckDate { get; set; }

        [Display(Name = "وضعیت اعتبار")]
        public bool IsValid { get; set; }

        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        [Display(Name = "نام خدمت")]
        public string ServiceName { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "تاریخ بررسی")]
        public DateTime ValidationDate { get; set; }

        [Display(Name = "تاریخ بررسی (شمسی)")]
        public string ValidationDateShamsi { get; set; }

        [Display(Name = "وضعیت پوشش")]
        public bool IsCovered { get; set; }

        [Display(Name = "وضعیت اعتبار بیمه")]
        public bool IsInsuranceValid { get; set; }

        [Display(Name = "وضعیت انقضای بیمه")]
        public bool IsInsuranceExpired { get; set; }

        [Display(Name = "وضعیت واجد شرایط بودن")]
        public bool IsEligible { get; set; }

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

        [Display(Name = "تاریخ شروع بیمه")]
        public DateTime InsuranceStartDate { get; set; }

        [Display(Name = "تاریخ پایان بیمه")]
        public DateTime? InsuranceEndDate { get; set; }

        [Display(Name = "تاریخ شروع بیمه (شمسی)")]
        public string InsuranceStartDateShamsi { get; set; }

        [Display(Name = "تاریخ پایان بیمه (شمسی)")]
        public string InsuranceEndDateShamsi { get; set; }

        [Display(Name = "وضعیت اعتبارسنجی")]
        public string ValidationStatus { get; set; }

        [Display(Name = "پیام اعتبارسنجی")]
        public string ValidationMessage { get; set; }

        [Display(Name = "خطاهای اعتبارسنجی")]
        public string ValidationErrors { get; set; }
    }
}
