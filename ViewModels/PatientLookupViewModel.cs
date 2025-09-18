using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات بیمار در لیست انتخاب
    /// </summary>
    public class PatientLookupViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string FullName { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// وضعیت حذف شده بودن بیمار
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// نمایش ترکیبی نام و کد ملی
        /// </summary>
        public string DisplayText => $"{FullName} ({NationalCode})";
    }
}
