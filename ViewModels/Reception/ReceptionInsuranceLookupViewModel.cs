using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsuranceLookupViewModel
    {
        /// <summary>
        /// شناسه بیمه
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// نام بیمه
        /// </summary>
        [Required(ErrorMessage = "نام بیمه الزامی است")]
        public string InsuranceName { get; set; }

        /// <summary>
        /// نوع بیمه
        /// </summary>
        public string InsuranceType { get; set; }

        /// <summary>
        /// وضعیت فعال بودن بیمه
        /// </summary>
        public bool IsActive { get; set; }
    }
}
