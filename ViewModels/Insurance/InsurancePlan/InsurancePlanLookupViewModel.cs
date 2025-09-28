using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsurancePlan
{
    /// <summary>
    /// ViewModel برای نمایش طرح‌های بیمه در Lookup Lists (Dropdown)
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات اساسی برای انتخاب
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. بهینه‌سازی برای عملکرد
    /// 4. مناسب برای Select2 و Dropdown
    /// 5. نمایش کد، نام و درصد پوشش
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class InsurancePlanLookupViewModel
    {
        [Display(Name = "شناسه")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// Id property for consistency with other lookup ViewModels
        /// </summary>
        public int Id => InsurancePlanId;

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

        /// <summary>
        /// Value برای SelectList (شناسه طرح)
        /// </summary>
        public int Value => InsurancePlanId;

        /// <summary>
        /// Text برای SelectList (نام طرح + کد)
        /// </summary>
        public string Text => $"{Name} ({PlanCode})";

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static InsurancePlanLookupViewModel FromEntity(Models.Entities.Insurance.InsurancePlan entity)
        {
            if (entity == null) return null;

            return new InsurancePlanLookupViewModel
            {
                InsurancePlanId = entity.InsurancePlanId,
                Name = entity.Name,
                PlanCode = entity.PlanCode,
                InsuranceProviderName = entity.InsuranceProvider?.Name,
                CoveragePercent = entity.CoveragePercent,
                Deductible = entity.Deductible
            };
        }
    }
}
