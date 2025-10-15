using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش خدمت در فرم پذیرش
    /// </summary>
    public class ReceptionServiceViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// توضیحات خدمت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// قیمت پایه خدمت
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// شناسه سرفصل
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// نام سرفصل
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// آیا خدمت فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا خدمت قابل انتخاب است؟
        /// </summary>
        public bool IsSelectable { get; set; } = true;

        /// <summary>
        /// نمایش نام خدمت
        /// </summary>
        public string DisplayName => $"{ServiceCode} - {ServiceName}";

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش قیمت پایه (فرمات شده)
        /// </summary>
        public string BasePriceDisplay => $"{BasePrice:N0} ریال";

        /// <summary>
        /// نمایش اطلاعات خدمت (فرمات شده)
        /// </summary>
        public string ServiceInfoDisplay => $"{ServiceCode} - {ServiceName} - {BasePrice:N0} ریال";

        /// <summary>
        /// نمایش سرفصل
        /// </summary>
        public string CategoryDisplay => CategoryName ?? "نامشخص";

        /// <summary>
        /// آیا خدمت قابل انتخاب است؟
        /// </summary>
        public bool CanBeSelected => IsActive && IsSelectable;
    }
}
