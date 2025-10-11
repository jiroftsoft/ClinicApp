using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات lookup پذیرش
    /// </summary>
    public class ReceptionLookupViewModel
    {
        /// <summary>
        /// شناسه
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Display(Name = "نام")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// کد
        /// </summary>
        [Display(Name = "کد")]
        public string Code { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        [Display(Name = "ترتیب")]
        public int SortOrder { get; set; }
    }
}
