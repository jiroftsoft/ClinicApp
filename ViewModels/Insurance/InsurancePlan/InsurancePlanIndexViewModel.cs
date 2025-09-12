using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای نمایش طرح‌های بیمه در لیست (جدول)
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات اساسی طرح‌های بیمه
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش درصد پوشش و فرانشیز
    /// 5. نمایش اطلاعات ارائه‌دهنده بیمه
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsurancePlanIndexViewModel
    {
        [Display(Name = "شناسه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "نام طرح")]
        public string Name { get; set; }

        [Display(Name = "کد طرح")]
        public string PlanCode { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

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

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تعداد خدمات طرح")]
        public int PlanServiceCount { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsurancePlanIndexViewModel FromEntity(Models.Entities.Insurance.InsurancePlan entity)
        {
            if (entity == null) return null;

            return new InsurancePlanIndexViewModel
            {
                InsurancePlanId = entity.InsurancePlanId,
                Name = entity.Name,
                PlanCode = entity.PlanCode,
                InsuranceProviderName = entity.InsuranceProvider?.Name,
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible,
                ValidFrom = entity.ValidFrom,
                ValidTo = entity.ValidTo,
                ValidFromShamsi = entity.ValidFrom.ToPersianDate(),
                ValidToShamsi = entity.ValidTo.ToPersianDate(),
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName,
                PlanServiceCount = entity.PlanServices?.Count(ps => !ps.IsDeleted) ?? 0
            };
        }
    }
}
