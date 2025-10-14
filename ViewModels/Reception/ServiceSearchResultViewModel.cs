using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نتایج جستجوی خدمت
    /// </summary>
    public class ServiceSearchResultViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Required(ErrorMessage = "نام خدمت الزامی است")]
        public string ServiceName { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        [Required(ErrorMessage = "کد خدمت الزامی است")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// قیمت خدمت
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// نام دسته‌بندی
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// توضیحات خدمت
        /// </summary>
        public string Description { get; set; }
    }
}
