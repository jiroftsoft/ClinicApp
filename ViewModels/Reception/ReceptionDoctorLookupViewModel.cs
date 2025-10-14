using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش پزشک در فرم پذیرش
    /// </summary>
    public class ReceptionDoctorLookupViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Required(ErrorMessage = "نام پزشک الزامی است")]
        public string DoctorName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        public string Specialization { get; set; }

        /// <summary>
        /// وضعیت فعال بودن پزشک
        /// </summary>
        public bool IsActive { get; set; }
    }
}
