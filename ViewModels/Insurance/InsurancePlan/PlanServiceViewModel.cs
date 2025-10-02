using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای نمایش خدمات یک طرح بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات خدمات طرح بیمه
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش اطلاعات Audit Trail
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PlanServiceViewModel
    {
        [Display(Name = "شناسه")]
        public int PlanServiceId { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "شناسه دسته‌بندی خدمت")]
        public int ServiceCategoryId { get; set; }

        [Display(Name = "نام دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "سهم بیمار")]
        public decimal? Copay { get; set; }

        [Display(Name = "پوشش خاص")]
        public decimal? CoverageOverride { get; set; }

        [Display(Name = "تحت پوشش")]
        public bool IsCovered { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PlanServiceViewModel FromEntity(PlanService entity)
        {
            if (entity == null) return null;

            return new PlanServiceViewModel
            {
                PlanServiceId = entity.PlanServiceId,
                InsurancePlanId = entity.InsurancePlanId,
                ServiceCategoryId = entity.ServiceCategoryId,
                ServiceCategoryName = entity.ServiceCategory?.Title,
                Copay = entity.PatientSharePercent,
                CoverageOverride = entity.CoverageOverride,
                IsCovered = entity.IsCovered,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName
            };
        }
    }
}
