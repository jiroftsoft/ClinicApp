using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش تخصص در فرم پذیرش
    /// </summary>
    public class SpecializationLookupViewModel
    {
        /// <summary>
        /// شناسه تخصص
        /// </summary>
        public int SpecializationId { get; set; }

        /// <summary>
        /// نام تخصص
        /// </summary>
        [Required(ErrorMessage = "نام تخصص الزامی است")]
        public string SpecializationName { get; set; }

        /// <summary>
        /// نام تخصص (alias for SpecializationName)
        /// </summary>
        public string Name 
        { 
            get => SpecializationName; 
            set => SpecializationName = value; 
        }

        /// <summary>
        /// کد تخصص
        /// </summary>
        public string SpecializationCode { get; set; }

        /// <summary>
        /// توضیحات تخصص
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال بودن تخصص
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// تعداد پزشکان
        /// </summary>
        public int DoctorsCount { get; set; }
    }
}
