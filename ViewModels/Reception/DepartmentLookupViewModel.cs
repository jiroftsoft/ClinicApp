using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش دپارتمان در فرم پذیرش
    /// </summary>
    public class DepartmentLookupViewModel
    {
        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        [Required(ErrorMessage = "نام دپارتمان الزامی است")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// توضیحات دپارتمان
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// کد دپارتمان
        /// </summary>
        public string DepartmentCode { get; set; }

        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// وضعیت فعال بودن دپارتمان
        /// </summary>
        public bool IsActive { get; set; }
    }
}
