using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش خدمات در فرم پذیرش
    /// </summary>
    public class ServiceLookupViewModel
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
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

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
        /// آیا نیاز به تخصص دارد
        /// </summary>
        public bool RequiresSpecialization { get; set; }

        /// <summary>
        /// آیا نیاز به پزشک دارد
        /// </summary>
        public bool RequiresDoctor { get; set; }

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// عنوان خدمت (برای سازگاری)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// قیمت خدمت (برای سازگاری)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمت (برای سازگاری)
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی خدمت (برای سازگاری)
        /// </summary>
        public string ServiceCategoryTitle { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش دسته‌بندی خدمات
    /// </summary>
    public class ServiceCategoryLookupViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}