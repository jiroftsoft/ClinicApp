using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.InsuranceCalculation
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات خدمت در لیست انتخاب
    /// </summary>
    public class ServiceLookupItem
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Display(Name = "نام خدمت")]
        public string Name { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// دسته‌بندی خدمت
        /// </summary>
        [Display(Name = "دسته‌بندی")]
        public string Category { get; set; }

        /// <summary>
        /// قیمت پایه خدمت
        /// </summary>
        [Display(Name = "قیمت پایه")]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// وضعیت فعال بودن خدمت
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش ترکیبی نام، دسته‌بندی و قیمت
        /// </summary>
        public string DisplayText => $"{Name} ({Category}) - {BasePrice:N0} تومان";
    }
}
