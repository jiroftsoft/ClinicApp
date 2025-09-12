using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceProvider
{
    /// <summary>
    /// ViewModel برای نمایش ارائه‌دهندگان بیمه در Lookup Lists (Dropdown)
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات اساسی برای انتخاب
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. بهینه‌سازی برای عملکرد
    /// 4. مناسب برای Select2 و Dropdown
    /// 5. نمایش کد و نام
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsuranceProviderLookupViewModel
    {
        [Display(Name = "شناسه")]
        public int InsuranceProviderId { get; set; }

        [Display(Name = "نام")]
        public string Name { get; set; }

        [Display(Name = "کد")]
        public string Code { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsuranceProviderLookupViewModel FromEntity(Models.Entities.Insurance.InsuranceProvider entity)
        {
            if (entity == null) return null;

            return new InsuranceProviderLookupViewModel
            {
                InsuranceProviderId = entity.InsuranceProviderId,
                Name = entity.Name,
                Code = entity.Code
            };
        }
    }
}
