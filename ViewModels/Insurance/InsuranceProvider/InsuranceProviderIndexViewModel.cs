using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;

namespace ClinicApp.ViewModels.Insurance.InsuranceProvider
{
    /// <summary>
    /// ViewModel برای نمایش ارائه‌دهندگان بیمه در لیست (جدول)
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات اساسی ارائه‌دهندگان بیمه
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. نمایش تاریخ شمسی
    /// 4. نمایش وضعیت فعال/غیرفعال
    /// 5. نمایش اطلاعات کاربر ایجادکننده
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceProviderIndexViewModel
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

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceProviderIndexViewModel FromEntity(Models.Entities.Insurance.InsuranceProvider entity)
        {
            if (entity == null) return null;

            return new InsuranceProviderIndexViewModel
            {
                InsuranceProviderId = entity.InsuranceProviderId,
                Name = entity.Name,
                Code = entity.Code,
                ContactInfo = entity.ContactInfo,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = entity.CreatedByUser != null ? $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}".Trim() : null
            };
        }
    }
}
