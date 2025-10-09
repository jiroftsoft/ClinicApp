using System.ComponentModel.DataAnnotations;

namespace ViewModels.Insurance.PatientInsurance
{
    /// <summary>
    /// ViewModel برای افزودن بیمه تکمیلی به رکورد موجود
    /// </summary>
    public class AddSupplementaryInsuranceViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه صندوق بیمه تکمیلی
        /// </summary>
        [Required(ErrorMessage = "صندوق بیمه تکمیلی الزامی است")]
        [Display(Name = "صندوق بیمه تکمیلی")]
        public int SupplementaryInsuranceProviderId { get; set; }

        /// <summary>
        /// شناسه طرح بیمه تکمیلی
        /// </summary>
        [Required(ErrorMessage = "طرح بیمه تکمیلی الزامی است")]
        [Display(Name = "طرح بیمه تکمیلی")]
        public int SupplementaryInsurancePlanId { get; set; }

        /// <summary>
        /// شماره معرفی نامه (اختیاری)
        /// </summary>
        [StringLength(100, ErrorMessage = "شماره معرفی نامه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [RegularExpression(@"^[A-Za-z0-9\-_]*$", ErrorMessage = "شماره معرفی نامه فقط می‌تواند شامل حروف انگلیسی، اعداد، خط تیره و زیرخط باشد")]
        [Display(Name = "شماره معرفی نامه")]
        public string SupplementaryPolicyNumber { get; set; }

        /// <summary>
        /// شماره کارت بیمه (اختیاری)
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره کارت بیمه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره کارت بیمه")]
        public string CardNumber { get; set; }

        /// <summary>
        /// اولویت بیمه
        /// </summary>
        [Required(ErrorMessage = "اولویت بیمه الزامی است")]
        [Display(Name = "اولویت بیمه")]
        public int Priority { get; set; } = 2; // پیش‌فرض: بیمه تکمیلی اول

        /// <summary>
        /// آیا فعال است
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;
    }
}
