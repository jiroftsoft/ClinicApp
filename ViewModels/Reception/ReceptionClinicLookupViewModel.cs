using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش کلینیک در فرم پذیرش
    /// </summary>
    public class ReceptionClinicLookupViewModel
    {
        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        [Required(ErrorMessage = "نام کلینیک الزامی است")]
        public string ClinicName { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// شماره تلفن کلینیک
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// وضعیت فعال بودن کلینیک
        /// </summary>
        public bool IsActive { get; set; }
    }
}
