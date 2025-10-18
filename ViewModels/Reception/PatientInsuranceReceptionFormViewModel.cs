using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای ذخیره اطلاعات بیمه بیمار در فرم پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از بیمه پایه و تکمیلی
    /// 2. اعتبارسنجی جامع
    /// 3. مناسب برای Real-Time Updates
    /// 4. Production Ready
    /// </summary>
    public class PatientInsuranceReceptionFormViewModel
    {
        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه بیمار نامعتبر است")]
        public int PatientId { get; set; }

        [Display(Name = "شناسه بیمه پایه")]
        public int? PrimaryInsuranceId { get; set; }

        [Display(Name = "شماره بیمه پایه")]
        [StringLength(50, ErrorMessage = "شماره بیمه پایه نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string PrimaryPolicyNumber { get; set; }

        [Display(Name = "شماره کارت بیمه پایه")]
        [StringLength(50, ErrorMessage = "شماره کارت بیمه پایه نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string PrimaryCardNumber { get; set; }

        [Display(Name = "شناسه بیمه تکمیلی")]
        public int? SupplementaryInsuranceId { get; set; }

        [Display(Name = "شماره بیمه تکمیلی")]
        [StringLength(50, ErrorMessage = "شماره بیمه تکمیلی نمی‌تواند بیش از 50 کاراکتر باشد")]
        public string SupplementaryPolicyNumber { get; set; }

        [Display(Name = "تاریخ انقضای بیمه تکمیلی")]
        [DataType(DataType.Date)]
        public DateTime? SupplementaryExpiryDate { get; set; }

        [Display(Name = "تاریخ ذخیره")]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        [Display(Name = "کاربر ذخیره‌کننده")]
        public string SavedBy { get; set; }

        /// <summary>
        /// بررسی اعتبارسنجی کامل
        /// </summary>
        public bool IsValid()
        {
            return PatientId > 0 && 
                   (PrimaryInsuranceId.HasValue || SupplementaryInsuranceId.HasValue);
        }

        /// <summary>
        /// دریافت پیام اعتبارسنجی
        /// </summary>
        public string GetValidationMessage()
        {
            if (PatientId <= 0)
                return "شناسه بیمار نامعتبر است";
            
            if (!PrimaryInsuranceId.HasValue && !SupplementaryInsuranceId.HasValue)
                return "حداقل یک بیمه (پایه یا تکمیلی) باید انتخاب شود";
            
            return null;
        }
    }
}
