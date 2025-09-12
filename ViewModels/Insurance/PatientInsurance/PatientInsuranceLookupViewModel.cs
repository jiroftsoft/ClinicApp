using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای نمایش بیمه‌های بیماران در Lookup Lists (Dropdown)
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات اساسی برای انتخاب
    /// 2. پشتیبانی از Factory Method Pattern
    /// 3. بهینه‌سازی برای عملکرد
    /// 4. مناسب برای Select2 و Dropdown
    /// 5. نمایش شماره بیمه و نوع
    /// 
    /// نکته حیاتی: این ViewModel طبق قرارداد Factory Method Pattern طراحی شده است
    /// </summary>
    public class PatientInsuranceLookupViewModel
    {
        [Display(Name = "شناسه")]
        public int PatientInsuranceId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "شماره بیمه")]
        public string PolicyNumber { get; set; }

        [Display(Name = "درصد پوشش")]
        public decimal CoveragePercent { get; set; }

        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; }

        [Display(Name = "بیمه اصلی")]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static PatientInsuranceLookupViewModel FromEntity(Models.Entities.Patient.PatientInsurance entity)
        {
            if (entity == null) return null;

            return new PatientInsuranceLookupViewModel
            {
                PatientInsuranceId = entity.PatientInsuranceId,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName,
                InsurancePlanId = entity.InsurancePlanId,
                PolicyNumber = entity.PolicyNumber,
                CoveragePercent = entity.InsurancePlan?.CoveragePercent ?? 0,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                InsuranceType = entity.IsPrimary ? "اصلی" : "تکمیلی",
                IsPrimary = entity.IsPrimary
            };
        }
    }
}
