using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات بیمه‌های بیمار
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات کامل بیمه بیمار
    /// 2. پشتیبانی از تاریخ شمسی
    /// 3. مناسب برای نمایش در Details و لیست‌ها
    /// 4. رعایت قرارداد Factory Method Pattern
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceViewModel
    {
        [Display(Name = "شناسه")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool IsPrimary { get; set; }

        [Display(Name = "اولویت")]
        public int Priority { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string StartDateShamsi { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string EndDateShamsi { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "کاربر ایجادکننده")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین بروزرسانی")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین بروزرسانی (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "کاربر آخرین بروزرسانی‌کننده")]
        public string UpdatedByUser { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceViewModel FromEntity(Models.Entities.PatientInsurance entity)
        {
            if (entity == null) return null;

            return new PatientInsuranceViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                InsurancePlanId = entity.InsurancePlanId,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                PolicyNumber = entity.PolicyNumber,
                IsPrimary = entity.IsPrimary,
                Priority = entity.Priority,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                StartDateShamsi = entity.StartDate.ToPersianDate(),
                EndDateShamsi = entity.EndDate?.ToPersianDate(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUser = entity.CreatedByUser != null ? 
                    $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                UpdatedByUser = entity.UpdatedByUser != null ? 
                    $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null
            };
        }

        /// <summary>
        /// ✅ یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.PatientInsurance ToEntity()
        {
            return new Models.Entities.PatientInsurance
            {
                PatientInsuranceId = this.PatientInsuranceId,
                PatientId = this.PatientId,
                InsurancePlanId = this.InsurancePlanId,
                PolicyNumber = this.PolicyNumber?.Trim(),
                IsPrimary = this.IsPrimary,
                Priority = this.Priority,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                IsActive = this.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void UpdateEntity(Models.Entities.PatientInsurance entity)
        {
            if (entity == null) return;

            entity.PolicyNumber = this.PolicyNumber?.Trim();
            entity.IsPrimary = this.IsPrimary;
            entity.Priority = this.Priority;
            entity.StartDate = this.StartDate;
            entity.EndDate = this.EndDate;
            entity.IsActive = this.IsActive;
        }
    }
}
