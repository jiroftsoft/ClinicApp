using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک بیمه بیمار
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تمام اطلاعات بیمه بیمار
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش اطلاعات Audit Trail
    /// 5. نمایش اطلاعات کامل بیمار و طرح بیمه
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceDetailsViewModel
    {
        [Display(Name = "شناسه")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد بیمار")]
        public string PatientCode { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

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

        [Display(Name = "فرانشیز")]
        public decimal Deductible { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "تاریخ به‌روزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "به‌روزرساننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceDetailsViewModel FromEntity(Models.Entities.PatientInsurance entity)
        {
            if (entity == null) return null;

            return new PatientInsuranceDetailsViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                PatientName = $"{entity.Patient?.FirstName} {entity.Patient?.LastName}",
                PatientNationalCode = entity.Patient?.NationalCode,
                PolicyNumber = entity.PolicyNumber,
                InsurancePlanId = entity.InsurancePlanId,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                InsuranceType = entity.IsPrimary ? "اصلی" : "تکمیلی",
                IsPrimary = entity.IsPrimary,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                StartDateShamsi = entity.StartDate.ToPersianDate(),
                EndDateShamsi = entity.EndDate?.ToPersianDate(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName,
                UpdatedByUserName = entity.UpdatedByUser?.FullName
            };
        }
    }
}
