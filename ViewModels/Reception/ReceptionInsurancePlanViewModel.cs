using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش طرح بیمه در فرم پذیرش
    /// </summary>
    public class ReceptionInsurancePlanViewModel
    {
        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// توضیحات طرح بیمه
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// درصد پوشش بیمه
        /// </summary>
        public decimal? CoveragePercent { get; set; }

        /// <summary>
        /// فرانشیز بیمه
        /// </summary>
        public decimal? Deductible { get; set; }

        /// <summary>
        /// آیا طرح بیمه فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// نمایش نام طرح بیمه (فرمات شده)
        /// </summary>
        public string DisplayName => $"{Name}";

        /// <summary>
        /// نمایش درصد پوشش (فرمات شده)
        /// </summary>
        public string CoveragePercentDisplay => CoveragePercent.HasValue ? $"{CoveragePercent.Value}%" : "نامشخص";

        /// <summary>
        /// نمایش فرانشیز (فرمات شده)
        /// </summary>
        public string DeductibleDisplay => Deductible.HasValue ? $"{Deductible.Value:N0} ریال" : "نامشخص";

        /// <summary>
        /// نمایش وضعیت فعال
        /// </summary>
        public string StatusDisplay => IsActive ? "فعال" : "غیرفعال";
    }
}
