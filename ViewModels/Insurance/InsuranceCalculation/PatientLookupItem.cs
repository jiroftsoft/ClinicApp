using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات بیمار در لیست انتخاب
    /// </summary>
    public class PatientLookupItem
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام کامل بیمار
        /// </summary>
        [Display(Name = "نام بیمار")]
        public string Name { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalId { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// وضعیت فعال بودن بیمار
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش ترکیبی نام و کد ملی
        /// </summary>
        public string DisplayText => $"{Name} ({NationalId})";
    }
}
