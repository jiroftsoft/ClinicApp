using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsuranceProvider
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل یک ارائه‌دهنده بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش تمام اطلاعات ارائه‌دهنده بیمه
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش اطلاعات Audit Trail
    /// 5. نمایش آمار طرح‌های بیمه
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceProviderDetailsViewModel
    {
        [Display(Name = "شناسه")]
        public int InsuranceProviderId { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [Display(Name = "کد")]
        public string Code { get; set; }

        [Display(Name = "اطلاعات تماس")]
        public string ContactInfo { get; set; }

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

        [Display(Name = "تعداد طرح‌های بیمه")]
        public int InsurancePlanCount { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceProviderDetailsViewModel FromEntity(Models.Entities.InsuranceProvider entity)
        {
            if (entity == null) return null;

            return new InsuranceProviderDetailsViewModel
            {
                InsuranceProviderId = entity.InsuranceProviderId,
                Name = entity.Name,
                Code = entity.Code,
                ContactInfo = entity.ContactInfo,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = entity.UpdatedAt?.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser?.FullName,
                UpdatedByUserName = entity.UpdatedByUser?.FullName,
                InsurancePlanCount = entity.InsurancePlans?.Count(ip => !ip.IsDeleted) ?? 0
            };
        }
    }
}
