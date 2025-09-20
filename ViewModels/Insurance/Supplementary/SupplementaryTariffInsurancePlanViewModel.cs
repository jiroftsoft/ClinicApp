using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش طرح بیمه در تعرفه بیمه تکمیلی
    /// Supplementary Tariff Insurance Plan ViewModel
    /// </summary>
    public class SupplementaryTariffInsurancePlanViewModel
    {
        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "نام طرح بیمه")]
        [Required(ErrorMessage = "نام طرح بیمه الزامی است.")]
        [StringLength(200, ErrorMessage = "نام طرح بیمه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        [Display(Name = "ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// نام ارائه‌دهنده بیمه
        /// </summary>
        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        /// <summary>
        /// کد طرح بیمه
        /// </summary>
        [Display(Name = "کد طرح بیمه")]
        [StringLength(50, ErrorMessage = "کد طرح بیمه نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string InsurancePlanCode { get; set; }

        /// <summary>
        /// نوع بیمه
        /// </summary>
        [Display(Name = "نوع بیمه")]
        public string InsuranceType { get; set; }

        /// <summary>
        /// درصد پوشش پایه
        /// </summary>
        [Display(Name = "درصد پوشش پایه")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal? BaseCoveragePercent { get; set; }

        /// <summary>
        /// سقف پرداخت پایه
        /// </summary>
        [Display(Name = "سقف پرداخت پایه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? BaseMaxPayment { get; set; }

        /// <summary>
        /// درصد پوشش تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal? SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// سقف پرداخت تکمیلی
        /// </summary>
        [Display(Name = "سقف پرداخت تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? SupplementaryMaxPayment { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// متن وضعیت
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsActive)
                    return "غیرفعال";
                
                if (StartDate.HasValue && StartDate.Value > DateTime.Now)
                    return "آینده";
                
                if (EndDate.HasValue && EndDate.Value < DateTime.Now)
                    return "منقضی شده";
                
                return "فعال";
            }
        }

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string StatusCssClass
        {
            get
            {
                if (!IsActive)
                    return "badge-secondary";
                
                if (StartDate.HasValue && StartDate.Value > DateTime.Now)
                    return "badge-warning";
                
                if (EndDate.HasValue && EndDate.Value < DateTime.Now)
                    return "badge-danger";
                
                return "badge-success";
            }
        }

        /// <summary>
        /// آیا طرح بیمه معتبر است
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!IsActive) return false;
                
                var now = DateTime.Now;
                
                if (StartDate.HasValue && StartDate.Value > now) return false;
                if (EndDate.HasValue && EndDate.Value < now) return false;
                
                return true;
            }
        }

        /// <summary>
        /// نمایش کامل طرح بیمه
        /// </summary>
        public string FullDisplayName
        {
            get
            {
                var display = InsurancePlanName;
                if (!string.IsNullOrEmpty(InsuranceProviderName))
                {
                    display += $" - {InsuranceProviderName}";
                }
                if (!string.IsNullOrEmpty(InsurancePlanCode))
                {
                    display += $" ({InsurancePlanCode})";
                }
                return display;
            }
        }

        /// <summary>
        /// نمایش کوتاه طرح بیمه
        /// </summary>
        public string ShortDisplayName
        {
            get
            {
                if (!string.IsNullOrEmpty(InsurancePlanCode))
                {
                    return $"{InsurancePlanCode} - {InsurancePlanName}";
                }
                return InsurancePlanName;
            }
        }

        /// <summary>
        /// آیا طرح بیمه دارای کد است
        /// </summary>
        public bool HasCode => !string.IsNullOrEmpty(InsurancePlanCode);

        /// <summary>
        /// آیا طرح بیمه دارای ارائه‌دهنده است
        /// </summary>
        public bool HasProvider => !string.IsNullOrEmpty(InsuranceProviderName);

        /// <summary>
        /// آیا طرح بیمه دارای تاریخ شروع است
        /// </summary>
        public bool HasStartDate => StartDate.HasValue;

        /// <summary>
        /// آیا طرح بیمه دارای تاریخ پایان است
        /// </summary>
        public bool HasEndDate => EndDate.HasValue;

        /// <summary>
        /// آیا طرح بیمه دارای پوشش پایه است
        /// </summary>
        public bool HasBaseCoverage => BaseCoveragePercent.HasValue && BaseCoveragePercent.Value > 0;

        /// <summary>
        /// آیا طرح بیمه دارای پوشش تکمیلی است
        /// </summary>
        public bool HasSupplementaryCoverage => SupplementaryCoveragePercent.HasValue && SupplementaryCoveragePercent.Value > 0;

        /// <summary>
        /// آیا طرح بیمه دارای سقف پرداخت پایه است
        /// </summary>
        public bool HasBaseMaxPayment => BaseMaxPayment.HasValue && BaseMaxPayment.Value > 0;

        /// <summary>
        /// آیا طرح بیمه دارای سقف پرداخت تکمیلی است
        /// </summary>
        public bool HasSupplementaryMaxPayment => SupplementaryMaxPayment.HasValue && SupplementaryMaxPayment.Value > 0;

        /// <summary>
        /// نمایش درصد پوشش پایه فرمت شده
        /// </summary>
        public string FormattedBaseCoveragePercent => (BaseCoveragePercent ?? 0).ToString("F2") + "%";

        /// <summary>
        /// نمایش درصد پوشش تکمیلی فرمت شده
        /// </summary>
        public string FormattedSupplementaryCoveragePercent => (SupplementaryCoveragePercent ?? 0).ToString("F2") + "%";

        /// <summary>
        /// نمایش سقف پرداخت پایه فرمت شده
        /// </summary>
        public string FormattedBaseMaxPayment => (BaseMaxPayment ?? 0).ToString("N0") + " تومان";

        /// <summary>
        /// نمایش سقف پرداخت تکمیلی فرمت شده
        /// </summary>
        public string FormattedSupplementaryMaxPayment => (SupplementaryMaxPayment ?? 0).ToString("N0") + " تومان";

        /// <summary>
        /// درصد پوشش کل (پایه + تکمیلی)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent
        {
            get
            {
                var baseCoverage = BaseCoveragePercent ?? 0;
                var supplementaryCoverage = SupplementaryCoveragePercent ?? 0;
                return baseCoverage + supplementaryCoverage;
            }
        }

        /// <summary>
        /// سقف پرداخت کل (پایه + تکمیلی)
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalMaxPayment
        {
            get
            {
                var baseMaxPayment = BaseMaxPayment ?? 0;
                var supplementaryMaxPayment = SupplementaryMaxPayment ?? 0;
                return baseMaxPayment + supplementaryMaxPayment;
            }
        }

        /// <summary>
        /// نمایش درصد پوشش کل فرمت شده
        /// </summary>
        public string FormattedTotalCoveragePercent => TotalCoveragePercent.ToString("F2") + "%";

        /// <summary>
        /// نمایش سقف پرداخت کل فرمت شده
        /// </summary>
        public string FormattedTotalMaxPayment => TotalMaxPayment.ToString("N0") + " تومان";

        /// <summary>
        /// آیا طرح بیمه دارای محدودیت زمانی است
        /// </summary>
        public bool HasTimeLimit => StartDate.HasValue || EndDate.HasValue;

        /// <summary>
        /// آیا طرح بیمه دارای محدودیت مالی است
        /// </summary>
        public bool HasFinancialLimit => HasBaseMaxPayment || HasSupplementaryMaxPayment;

        /// <summary>
        /// آیا طرح بیمه دارای پوشش کامل است
        /// </summary>
        public bool HasFullCoverage => TotalCoveragePercent >= 100;

        /// <summary>
        /// آیا طرح بیمه دارای پوشش جزئی است
        /// </summary>
        public bool HasPartialCoverage => TotalCoveragePercent > 0 && TotalCoveragePercent < 100;

        /// <summary>
        /// آیا طرح بیمه بدون پوشش است
        /// </summary>
        public bool HasNoCoverage => TotalCoveragePercent == 0;
    }
}
