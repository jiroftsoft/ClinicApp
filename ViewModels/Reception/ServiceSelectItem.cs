using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای انتخاب خدمات در فرم پذیرش
    /// </summary>
    public class ServiceSelectItem
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
        public string ServiceCode { get; set; }

        /// <summary>
        /// قیمت پایه
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// نام دسته‌بندی
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// قیمت نمایشی
        /// </summary>
        public string PriceDisplay { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا نیاز به تخصص دارد
        /// </summary>
        public bool RequiresSpecialization { get; set; }

        /// <summary>
        /// آیا نیاز به پزشک دارد
        /// </summary>
        public bool RequiresDoctor { get; set; }
    }
}
