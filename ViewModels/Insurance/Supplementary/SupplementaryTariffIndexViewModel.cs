using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای نمایش آیتم تعرفه بیمه تکمیلی در لیست
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class SupplementaryTariffIndexViewModel
    {
        /// <summary>
        /// شناسه تعرفه بیمه تکمیلی
        /// </summary>
        public int InsuranceTariffId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
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
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// نام طرح بیمه
        /// </summary>
        [Display(Name = "طرح بیمه")]
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
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? TariffPrice { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal? PatientShare { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
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
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
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
        /// تاریخ ایجاد
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجادکننده
        /// </summary>
        [Display(Name = "ایجادکننده")]
        public string CreatedByUserName { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        [Display(Name = "آخرین به‌روزرسانی")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر آخرین به‌روزرسانی‌کننده
        /// </summary>
        [Display(Name = "به‌روزرسانی‌کننده")]
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// آیا تعرفه منقضی شده است؟
        /// </summary>
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;

        /// <summary>
        /// آیا تعرفه در آینده فعال می‌شود؟
        /// </summary>
        public bool IsFuture => StartDate.HasValue && StartDate.Value > DateTime.Now;

        /// <summary>
        /// وضعیت تعرفه (فعال/غیرفعال/منقضی/آینده)
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsActive) return "غیرفعال";
                if (IsExpired) return "منقضی";
                if (IsFuture) return "آینده";
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
                if (!IsActive) return "badge-secondary";
                if (IsExpired) return "badge-danger";
                if (IsFuture) return "badge-warning";
                return "badge-success";
            }
        }
    }
}
