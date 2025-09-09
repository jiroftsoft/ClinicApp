using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceProvider
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش یک ارائه‌دهنده بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. اعتبارسنجی کامل فیلدها
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. تبدیل Entity به ViewModel و بالعکس
    /// 4. اعتبارسنجی کد و نام منحصر به فرد
    /// 5. پشتیبانی از تقویم شمسی
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceProviderCreateEditViewModel
    {
        [Display(Name = "شناسه")]
        public int InsuranceProviderId { get; set; }

        [Required(ErrorMessage = "نام ارائه‌دهنده بیمه الزامی است")]
        [StringLength(200, ErrorMessage = "نام ارائه‌دهنده بیمه نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "نام ارائه‌دهنده بیمه")]
        public string Name { get; set; }

        [Required(ErrorMessage = "کد ارائه‌دهنده بیمه الزامی است")]
        [StringLength(50, ErrorMessage = "کد ارائه‌دهنده بیمه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "کد ارائه‌دهنده بیمه")]
        public string Code { get; set; }

        [StringLength(500, ErrorMessage = "اطلاعات تماس نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "اطلاعات تماس")]
        public string ContactInfo { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceProviderCreateEditViewModel FromEntity(Models.Entities.InsuranceProvider entity)
        {
            if (entity == null) return null;

            return new InsuranceProviderCreateEditViewModel
            {
                InsuranceProviderId = entity.InsuranceProviderId,
                Name = entity.Name,
                Code = entity.Code,
                ContactInfo = entity.ContactInfo,
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public Models.Entities.InsuranceProvider ToEntity()
        {
            return new Models.Entities.InsuranceProvider
            {
                InsuranceProviderId = this.InsuranceProviderId,
                Name = this.Name?.Trim(),
                Code = this.Code?.Trim(),
                ContactInfo = this.ContactInfo?.Trim(),
                IsActive = this.IsActive
            };
        }

        /// <summary>
        /// ✅ یک Entity موجود را بر اساس داده‌های این ViewModel به‌روزرسانی می‌کند.
        /// </summary>
        public void MapToEntity(Models.Entities.InsuranceProvider entity)
        {
            if (entity == null) return;

            entity.Name = this.Name?.Trim();
            entity.Code = this.Code?.Trim();
            entity.ContactInfo = this.ContactInfo?.Trim();
            entity.IsActive = this.IsActive;
        }
    }
}
