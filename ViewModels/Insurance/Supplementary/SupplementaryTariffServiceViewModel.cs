using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش خدمت در تعرفه بیمه تکمیلی
    /// Supplementary Tariff Service ViewModel
    /// </summary>
    public class SupplementaryTariffServiceViewModel
    {
        /// <summary>
        /// شناسه خدمت
        /// </summary>
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Display(Name = "نام خدمت")]
        [Required(ErrorMessage = "نام خدمت الزامی است.")]
        [StringLength(200, ErrorMessage = "نام خدمت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string ServiceTitle { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        [Display(Name = "کد خدمت")]
        [StringLength(50, ErrorMessage = "کد خدمت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمت
        /// </summary>
        [Display(Name = "دسته‌بندی خدمت")]
        public int? ServiceCategoryId { get; set; }

        /// <summary>
        /// نام دسته‌بندی خدمت
        /// </summary>
        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        /// <summary>
        /// شناسه بخش
        /// </summary>
        [Display(Name = "بخش")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// نام بخش
        /// </summary>
        [Display(Name = "بخش")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// قیمت پایه خدمت
        /// </summary>
        [Display(Name = "قیمت پایه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// قیمت فنی خدمت
        /// </summary>
        [Display(Name = "قیمت فنی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TechnicalPrice { get; set; }

        /// <summary>
        /// قیمت حرفه‌ای خدمت
        /// </summary>
        [Display(Name = "قیمت حرفه‌ای")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? ProfessionalPrice { get; set; }

        /// <summary>
        /// قیمت کل خدمت
        /// </summary>
        [Display(Name = "قیمت کل")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalPrice
        {
            get
            {
                var basePrice = BasePrice ?? 0;
                var technicalPrice = TechnicalPrice ?? 0;
                var professionalPrice = ProfessionalPrice ?? 0;
                return basePrice + technicalPrice + professionalPrice;
            }
        }

        /// <summary>
        /// واحد خدمت
        /// </summary>
        [Display(Name = "واحد")]
        [StringLength(50, ErrorMessage = "واحد خدمت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string Unit { get; set; }

        /// <summary>
        /// توضیحات خدمت
        /// </summary>
        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText => IsActive ? "فعال" : "غیرفعال";

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string StatusCssClass => IsActive ? "badge-success" : "badge-secondary";

        /// <summary>
        /// آیا خدمت دارای قیمت است
        /// </summary>
        public bool HasPrice => TotalPrice > 0;

        /// <summary>
        /// نمایش کامل خدمت
        /// </summary>
        public string FullDisplayName
        {
            get
            {
                var display = ServiceTitle;
                if (!string.IsNullOrEmpty(ServiceCode))
                {
                    display += $" ({ServiceCode})";
                }
                if (!string.IsNullOrEmpty(DepartmentName))
                {
                    display += $" - {DepartmentName}";
                }
                return display;
            }
        }

        /// <summary>
        /// نمایش کوتاه خدمت
        /// </summary>
        public string ShortDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(ServiceCode))
                {
                    return $"{ServiceCode} - {ServiceTitle}";
                }
                return ServiceTitle;
            }
        }

        /// <summary>
        /// آیا خدمت دارای دسته‌بندی است
        /// </summary>
        public bool HasCategory => !string.IsNullOrEmpty(ServiceCategoryName);

        /// <summary>
        /// آیا خدمت دارای بخش است
        /// </summary>
        public bool HasDepartment => !string.IsNullOrEmpty(DepartmentName);

        /// <summary>
        /// آیا خدمت دارای توضیحات است
        /// </summary>
        public bool HasDescription => !string.IsNullOrEmpty(Description);

        /// <summary>
        /// نمایش قیمت فرمت شده
        /// </summary>
        public string FormattedPrice => TotalPrice.ToString("N0") + " تومان";

        /// <summary>
        /// نمایش قیمت پایه فرمت شده
        /// </summary>
        public string FormattedBasePrice => (BasePrice ?? 0).ToString("N0") + " تومان";

        /// <summary>
        /// نمایش قیمت فنی فرمت شده
        /// </summary>
        public string FormattedTechnicalPrice => (TechnicalPrice ?? 0).ToString("N0") + " تومان";

        /// <summary>
        /// نمایش قیمت حرفه‌ای فرمت شده
        /// </summary>
        public string FormattedProfessionalPrice => (ProfessionalPrice ?? 0).ToString("N0") + " تومان";

        /// <summary>
        /// درصد قیمت فنی از کل
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TechnicalPricePercent
        {
            get
            {
                if (TotalPrice == 0) return 0;
                var technicalPrice = TechnicalPrice ?? 0;
                return (technicalPrice / TotalPrice) * 100;
            }
        }

        /// <summary>
        /// درصد قیمت حرفه‌ای از کل
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal ProfessionalPricePercent
        {
            get
            {
                if (TotalPrice == 0) return 0;
                var professionalPrice = ProfessionalPrice ?? 0;
                return (professionalPrice / TotalPrice) * 100;
            }
        }

        /// <summary>
        /// درصد قیمت پایه از کل
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal BasePricePercent
        {
            get
            {
                if (TotalPrice == 0) return 0;
                var basePrice = BasePrice ?? 0;
                return (basePrice / TotalPrice) * 100;
            }
        }
    }
}
