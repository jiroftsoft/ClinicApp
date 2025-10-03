using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای ایجاد و ویرایش تعرفه بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class SupplementaryTariffCreateEditViewModel
    {
        /// <summary>
        /// شناسه تعرفه بیمه تکمیلی (برای ویرایش)
        /// </summary>
        public int? InsuranceTariffId { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        [Required(ErrorMessage = "انتخاب دپارتمان الزامی است.")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// شناسه سرفصل خدمت
        /// </summary>
        [Display(Name = "سرفصل خدمت")]
        public int? ServiceCategoryId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        [Required(ErrorMessage = "انتخاب خدمت الزامی است.")]
        [Display(Name = "خدمت")]
        public int ServiceId { get; set; }

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        [Required(ErrorMessage = "انتخاب ارائه‌دهنده بیمه الزامی است.")]
        [Display(Name = "ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        /// <summary>
        /// شناسه بیمه پایه
        /// </summary>
        [Required(ErrorMessage = "انتخاب بیمه پایه الزامی است.")]
        [Display(Name = "بیمه پایه")]
        public int PrimaryInsurancePlanId { get; set; }

        /// <summary>
        /// شناسه طرح بیمه تکمیلی
        /// </summary>
        [Required(ErrorMessage = "انتخاب طرح بیمه تکمیلی الزامی است.")]
        [Display(Name = "طرح بیمه تکمیلی")]
        public int InsurancePlanId { get; set; }

        /// <summary>
        /// قیمت تعرفه (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "قیمت تعرفه (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت تعرفه نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "قیمت تعرفه باید عدد صحیح مثبت باشد")]
        public decimal? TariffPrice { get; set; }

        /// <summary>
        /// سهم بیمار (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "سهم بیمار (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "سهم بیمار باید عدد صحیح مثبت باشد")]
        public decimal? PatientShare { get; set; }

        /// <summary>
        /// سهم بیمه (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "سهم بیمه (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "سهم بیمه باید عدد صحیح مثبت باشد")]
        public decimal? InsurerShare { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی (با اعشار)
        /// </summary>
        [Display(Name = "درصد پوشش تکمیلی")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد.")]
        [RegularExpression(@"^(0|[1-9]\d*)(\.\d{1,2})?$", ErrorMessage = "درصد پوشش باید عدد مثبت باشد (حداکثر 2 رقم اعشار)")]
        public decimal? SupplementaryCoveragePercent { get; set; }

        /// <summary>
        /// سقف پرداخت بیمه تکمیلی (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "سقف پرداخت تکمیلی (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "سقف پرداخت نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "سقف پرداخت باید عدد صحیح مثبت باشد")]
        public decimal? SupplementaryMaxPayment { get; set; }

        /// <summary>
        /// فرانشیز بیمه تکمیلی (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "فرانشیز تکمیلی (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "فرانشیز تکمیلی نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "فرانشیز تکمیلی باید عدد صحیح مثبت باشد")]
        public decimal? SupplementaryDeductible { get; set; }

        /// <summary>
        /// حداقل پرداخت بیمار (ریال - بدون اعشار)
        /// </summary>
        [Display(Name = "حداقل پرداخت بیمار (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "حداقل پرداخت بیمار نمی‌تواند منفی باشد.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "حداقل پرداخت بیمار باید عدد صحیح مثبت باشد")]
        public decimal? MinPatientCopay { get; set; }

        /// <summary>
        /// تنظیمات خاص بیمه تکمیلی (JSON)
        /// </summary>
        [Display(Name = "تنظیمات خاص")]
        [StringLength(2000, ErrorMessage = "تنظیمات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string SupplementarySettings { get; set; }

        /// <summary>
        /// لیست دپارتمان‌ها برای DropDown
        /// </summary>
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// لیست بیمه‌های پایه برای DropDown
        /// </summary>
        public List<SelectListItem> PrimaryInsurancePlans { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// لیست بیمه‌های تکمیلی برای DropDown
        /// </summary>
        public List<SelectListItem> InsurancePlans { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// متادیتای طرح‌های بیمه پایه برای استفاده در JS (Strongly Typed)
        /// </summary>
        public List<InsurancePlanMetaViewModel> PrimaryInsurancePlansMeta { get; set; } = new List<InsurancePlanMetaViewModel>();

        /// <summary>
        /// متادیتای طرح‌های بیمه تکمیلی برای استفاده در JS (Strongly Typed)
        /// </summary>
        public List<InsurancePlanMetaViewModel> InsurancePlansMeta { get; set; } = new List<InsurancePlanMetaViewModel>();

        /// <summary>
        /// نام خدمت (فقط برای نمایش)
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// کد خدمت (فقط برای نمایش)
        /// </summary>
        public string ServiceCode { get; set; }

        /// <summary>
        /// اولویت تعرفه
        /// </summary>
        [Display(Name = "اولویت")]
        [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد.")]
        public int? Priority { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        [Display(Name = "تاریخ شروع اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        [Display(Name = "تاریخ پایان اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// نام خدمت (برای نمایش)
        /// </summary>
        public string ServiceTitle { get; set; }

        /// <summary>
        /// نام طرح بیمه (برای نمایش)
        /// </summary>
        public string InsurancePlanName { get; set; }

        /// <summary>
        /// نام بیمه پایه (برای نمایش)
        /// </summary>
        public string PrimaryInsurancePlanName { get; set; }

        /// <summary>
        /// نام ارائه‌دهنده بیمه (برای نمایش)
        /// </summary>
        public string InsuranceProviderName { get; set; }

        /// <summary>
        /// نام دپارتمان (برای نمایش)
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// نام سرفصل خدمت (برای نمایش)
        /// </summary>
        public string ServiceCategoryTitle { get; set; }

        /// <summary>
        /// آیا تعرفه منقضی شده است؟
        /// </summary>
        public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.Now;

        /// <summary>
        /// آیا تعرفه در آینده فعال می‌شود؟
        /// </summary>
        public bool IsFuture => StartDate.HasValue && StartDate.Value > DateTime.Now;

        /// <summary>
        /// آیا تعرفه در حال حاضر معتبر است؟
        /// </summary>
        public bool IsCurrentlyValid => IsActive && !IsExpired && !IsFuture;

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

        /// <summary>
        /// درصد پوشش کل (اصلی + تکمیلی)
        /// </summary>
        [Display(Name = "درصد پوشش کل")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal TotalCoveragePercent
        {
            get
            {
                if (TariffPrice.HasValue && TariffPrice.Value > 0)
                {
                    var primaryPercent = InsurerShare.HasValue ? (InsurerShare.Value / TariffPrice.Value) * 100 : 0;
                    var supplementaryPercent = SupplementaryCoveragePercent ?? 0;
                    return Math.Min(primaryPercent + supplementaryPercent, 100);
                }
                return 0;
            }
        }

        /// <summary>
        /// مبلغ باقی‌مانده پس از بیمه اصلی
        /// </summary>
        [Display(Name = "مبلغ باقی‌مانده")]
        [DisplayFormat(DataFormatString = "{0:N0} تومان")]
        public decimal RemainingAmountAfterPrimary
        {
            get
            {
                if (TariffPrice.HasValue && InsurerShare.HasValue)
                {
                    return Math.Max(0, TariffPrice.Value - InsurerShare.Value);
                }
                return TariffPrice ?? 0;
            }
        }

        /// <summary>
        /// آیا بیمه تکمیلی اعمال شده است؟
        /// </summary>
        [Display(Name = "بیمه تکمیلی اعمال شده")]
        public bool IsSupplementaryApplied => SupplementaryCoveragePercent.HasValue && SupplementaryCoveragePercent.Value > 0;
    }

    /// <summary>
    /// ViewModel سبک برای متادیتای طرح بیمه جهت استفاده Strongly Typed در ویو
    /// </summary>
    public class InsurancePlanMetaViewModel
    {
        public int InsurancePlanId { get; set; }
        public string Name { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal Deductible { get; set; } // Rial
    }
}
