namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش سرفصل خدمت در فرم پذیرش
    /// </summary>
    public class ReceptionServiceCategoryViewModel
    {
        /// <summary>
        /// شناسه سرفصل
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// نام سرفصل
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// توضیحات سرفصل
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// آیا سرفصل فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تعداد خدمات
        /// </summary>
        public int ServiceCount { get; set; }

        /// <summary>
        /// نمایش نام سرفصل
        /// </summary>
        public string DisplayName => CategoryName;

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// نمایش اطلاعات سرفصل (فرمات شده)
        /// </summary>
        public string CategoryInfoDisplay => $"{CategoryName} - {Description}";

        /// <summary>
        /// نمایش تعداد خدمات
        /// </summary>
        public string ServiceCountDisplay => $"{ServiceCount} خدمت";
    }
}
