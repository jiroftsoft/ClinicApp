using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک طرح بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تمام اطلاعات طرح بیمه
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش اطلاعات Audit Trail
    /// 5. نمایش آمار خدمات طرح
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsurancePlanDetailsViewModel
    {
        [Display(Name = "شناسه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "نام طرح")]
        public string Name { get; set; }

        [Display(Name = "کد طرح")]
        public string PlanCode { get; set; }

        [Display(Name = "شناسه ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "فرانشیز")]
        public decimal Deductible { get; set; }

        [Display(Name = "تاریخ شروع")]
        public DateTime ValidFrom { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? ValidTo { get; set; }

        [Display(Name = "تاریخ شروع (شمسی)")]
        public string ValidFromShamsi { get; set; }

        [Display(Name = "تاریخ پایان (شمسی)")]
        public string ValidToShamsi { get; set; }

        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

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

        [Display(Name = "تعداد خدمات طرح")]
        public int PlanServiceCount { get; set; }

        [Display(Name = "خدمات طرح")]
        public List<PlanServiceViewModel> PlanServices { get; set; } = new List<PlanServiceViewModel>();

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsurancePlanDetailsViewModel FromEntity(Models.Entities.InsurancePlan entity)
        {
            if (entity == null) return null;

            return new InsurancePlanDetailsViewModel
            {
                InsurancePlanId = entity.InsurancePlanId,
                Name = entity.Name,
                PlanCode = entity.PlanCode,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider?.Name,
                Description = entity.Description,
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible,
                ValidFrom = entity.ValidFrom,
                ValidTo = entity.ValidTo,
                ValidFromShamsi = entity.ValidFrom.ToPersianDate(),
                ValidToShamsi = entity.ValidTo.ToPersianDate(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName,
                UpdatedByUserName = entity.UpdatedByUser?.FullName,
                PlanServiceCount = entity.PlanServices?.Count(ps => !ps.IsDeleted) ?? 0,
                PlanServices = entity.PlanServices?.Where(ps => !ps.IsDeleted)
                    .Select(ps => PlanServiceViewModel.FromEntity(ps))
                    .ToList() ?? new List<PlanServiceViewModel>()
            };
        }
    }
}
