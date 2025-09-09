using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای نمایش یک بیمه بیمار در لیست Index
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات کلیدی بیمه بیمار
    /// 2. مناسب برای نمایش در کارت‌های Index
    /// 3. رعایت قرارداد Factory Method Pattern
    /// 4. پشتیبانی از تاریخ شمسی
    /// 5. نمایش وضعیت بیمه (اصلی/تکمیلی)
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceIndexItemViewModel
    {
        [Display(Name = "شناسه")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام کامل بیمار")]
        public string PatientFullName { get; set; }

        [Display(Name = "کد بیمار")]
        public string PatientCode { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

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

        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string StartDateShamsi { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string EndDateShamsi { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        #region Helper Properties

        [Display(Name = "وضعیت بیمه")]
        public string InsuranceStatusText => IsPrimary ? "بیمه اصلی" : "بیمه تکمیلی";

        [Display(Name = "وضعیت فعال")]
        public string ActiveStatusText => IsActive ? "فعال" : "غیرفعال";

        [Display(Name = "وضعیت فعال")]
        public string ActiveStatusCssClass => IsActive ? "text-success" : "text-danger";

        [Display(Name = "نوع بیمه")]
        public string InsuranceTypeCssClass => IsPrimary ? "badge-primary" : "badge-secondary";

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceIndexItemViewModel FromEntity(Models.Entities.PatientInsurance entity)
        {
            if (entity == null) return null;

            return new PatientInsuranceIndexItemViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                PatientFullName = $"{entity.Patient?.FirstName} {entity.Patient?.LastName}".Trim(),
                PatientCode = entity.Patient?.PatientCode,
                PatientNationalCode = entity.Patient?.NationalCode,
                InsurancePlanId = entity.InsurancePlanId,
                PolicyNumber = entity.PolicyNumber,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                InsuranceType = entity.IsPrimary ? "اصلی" : "تکمیلی",
                IsPrimary = entity.IsPrimary,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                StartDateShamsi = entity.StartDate.ToPersianDate(),
                EndDateShamsi = entity.EndDate?.ToPersianDate(),
                IsActive = entity.IsActive,
                CoveragePercent = entity.InsurancePlan?.CoveragePercent ?? 0,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName
            };
        }
    }
}
