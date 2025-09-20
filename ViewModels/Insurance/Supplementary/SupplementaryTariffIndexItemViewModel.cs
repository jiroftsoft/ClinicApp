using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش آیتم تعرفه بیمه تکمیلی در فهرست
    /// Supplementary Tariff Index Item ViewModel
    /// </summary>
    public class SupplementaryTariffIndexItemViewModel
    {
        /// <summary>
        /// شناسه تعرفه بیمه
        /// </summary>
        [Display(Name = "شناسه تعرفه")]
        public int InsuranceTariffId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        [Display(Name = "شناسه خدمت")]
        public int ServiceId { get; set; }

        /// <summary>
        /// نام خدمت
        /// </summary>
        [Display(Name = "نام خدمت")]
        public string ServiceTitle { get; set; }

        /// <summary>
        /// کد خدمت
        /// </summary>
        [Display(Name = "کد خدمت")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        [Display(Name = "شناسه طرح بیمه")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "نام طرح بیمه")]
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// نام ارائه‌دهنده بیمه
        /// </summary>
        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        /// <summary>
        /// قیمت تعرفه
        /// </summary>
        [Display(Name = "قیمت تعرفه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? TariffPrice { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? PatientShare { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? InsurerShare { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "درصد پوشش تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal? SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// سقف پرداخت بیمه تکمیلی
        /// </summary>
        [Display(Name = "سقف پرداخت تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? SupplementaryMaxPayment { get; set; }

        /// <summary>
        /// اولویت تعرفه
        /// </summary>
        [Display(Name = "اولویت")]
        public int? Priority { get; set; }

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
        /// مبلغ باقی‌مانده پس از بیمه اصلی
        /// </summary>
        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal RemainingAmountAfterPrimary
        {
            get
            {
                var serviceAmount = TariffPrice ?? 0;
                var primaryCoverage = InsurerShare ?? 0;
                return Math.Max(0, serviceAmount - primaryCoverage);
            }
        }

        /// <summary>
        /// مبلغ پوشش بیمه تکمیلی
        /// </summary>
        [Display(Name = "پوشش تکمیلی")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal SupplementaryCoverageAmount
        {
            get
            {
                var remainingAmount = RemainingAmountAfterPrimary;
                var coveragePercent = SupplementaryCoveragePercent ?? 0;
                return (remainingAmount * coveragePercent) / 100;
            }
        }

        /// <summary>
        /// سهم نهایی بیمار
        /// </summary>
        [Display(Name = "سهم نهایی بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal FinalPatientShare
        {
            get
            {
                var remainingAmount = RemainingAmountAfterPrimary;
                var supplementaryCoverage = SupplementaryCoverageAmount;
                return Math.Max(0, remainingAmount - supplementaryCoverage);
            }
        }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجادکننده
        /// </summary>
        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// نام کاربر آخرین به‌روزرسانی‌کننده
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی‌کننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// تنظیمات خاص بیمه تکمیلی (JSON)
        /// </summary>
        [Display(Name = "تنظیمات خاص")]
        public string SupplementarySettings { get; set; }

        /// <summary>
        /// آیا تنظیمات خاص دارد
        /// </summary>
        public bool HasSpecialSettings => !string.IsNullOrEmpty(SupplementarySettings);

        /// <summary>
        /// درصد پوشش کل (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "پوشش کل")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent
        {
            get
            {
                var serviceAmount = TariffPrice ?? 0;
                if (serviceAmount == 0) return 0;
                
                var primaryCoverage = InsurerShare ?? 0;
                var supplementaryCoverage = SupplementaryCoverageAmount;
                var totalCoverage = primaryCoverage + supplementaryCoverage;
                
                return (totalCoverage / serviceAmount) * 100;
            }
        }

        /// <summary>
        /// درصد سهم بیمار
        /// </summary>
        [Display(Name = "درصد سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal PatientSharePercent
        {
            get
            {
                var serviceAmount = TariffPrice ?? 0;
                if (serviceAmount == 0) return 0;
                
                return (FinalPatientShare / serviceAmount) * 100;
            }
        }

        /// <summary>
        /// آیا تعرفه معتبر است
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
        /// متن وضعیت اعتبار
        /// </summary>
        public string ValidityText
        {
            get
            {
                if (!IsActive) return "غیرفعال";
                if (StartDate.HasValue && StartDate.Value > DateTime.Now) return "هنوز شروع نشده";
                if (EndDate.HasValue && EndDate.Value < DateTime.Now) return "منقضی شده";
                return "معتبر";
            }
        }

        /// <summary>
        /// کلاس CSS برای اعتبار
        /// </summary>
        public string ValidityCssClass
        {
            get
            {
                if (!IsActive) return "text-danger";
                if (StartDate.HasValue && StartDate.Value > DateTime.Now) return "text-warning";
                if (EndDate.HasValue && EndDate.Value < DateTime.Now) return "text-danger";
                return "text-success";
            }
        }
    }
}
