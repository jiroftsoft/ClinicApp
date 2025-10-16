using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای انتخاب دسته‌بندی خدمات در فرم پذیرش
    /// </summary>
    public class ServiceCategorySelectItem
    {
        /// <summary>
        /// شناسه دسته‌بندی
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// نام دسته‌بندی
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// کد دسته‌بندی
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نام نمایشی
        /// </summary>
        public string DisplayName { get; set; }
    }
}
