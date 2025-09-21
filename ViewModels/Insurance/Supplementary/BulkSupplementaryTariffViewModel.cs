using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.InsurancePlan;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای ایجاد تعرفه گروهی بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا - Production Ready
    /// </summary>
    public class BulkSupplementaryTariffViewModel
    {
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
        /// نوع انتخاب خدمات
        /// </summary>
        [Required(ErrorMessage = "نوع انتخاب خدمات الزامی است.")]
        [Display(Name = "نوع انتخاب")]
        public BulkSelectionType SelectionType { get; set; }

        /// <summary>
        /// شناسه‌های دپارتمان‌های انتخاب شده
        /// </summary>
        [Display(Name = "دپارتمان‌ها")]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();

        /// <summary>
        /// شناسه‌های دسته‌بندی‌های انتخاب شده
        /// </summary>
        [Display(Name = "دسته‌بندی‌ها")]
        public List<int> SelectedServiceCategoryIds { get; set; } = new List<int>();

        /// <summary>
        /// شناسه‌های خدمات انتخاب شده
        /// </summary>
        [Display(Name = "خدمات")]
        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        /// <summary>
        /// محدوده قیمت حداقل
        /// </summary>
        [Display(Name = "حداقل قیمت")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// محدوده قیمت حداکثر
        /// </summary>
        [Display(Name = "حداکثر قیمت")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// درصد پوشش بیمه تکمیلی
        /// </summary>
        [Required(ErrorMessage = "درصد پوشش الزامی است.")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد پوشش تکمیلی")]
        public decimal SupplementaryCoveragePercent { get; set; } = 90;

        /// <summary>
        /// اولویت تعرفه
        /// </summary>
        [Required(ErrorMessage = "اولویت الزامی است.")]
        [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد")]
        [Display(Name = "اولویت")]
        public int Priority { get; set; } = 5;

        /// <summary>
        /// نوع محاسبه قیمت
        /// </summary>
        [Required(ErrorMessage = "نوع محاسبه قیمت الزامی است.")]
        [Display(Name = "نوع محاسبه قیمت")]
        public PriceCalculationType PriceCalculationType { get; set; } = PriceCalculationType.Auto;

        /// <summary>
        /// قیمت پیش‌فرض (در صورت انتخاب Fixed)
        /// </summary>
        [Display(Name = "قیمت پیش‌فرض")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت نمی‌تواند منفی باشد")]
        public decimal? DefaultPrice { get; set; }

        /// <summary>
        /// ضریب قیمت (در صورت انتخاب Multiplier)
        /// </summary>
        [Display(Name = "ضریب قیمت")]
        [Range(0.1, 10, ErrorMessage = "ضریب باید بین 0.1 تا 10 باشد")]
        public decimal? PriceMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// فعال بودن تعرفه
        /// </summary>
        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// تنظیمات پیشرفته
        /// </summary>
        [Display(Name = "تنظیمات پیشرفته")]
        public BulkTariffAdvancedSettings AdvancedSettings { get; set; } = new BulkTariffAdvancedSettings();

        // Navigation Properties
        public List<InsurancePlanLookupViewModel> PrimaryInsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public List<InsurancePlanLookupViewModel> SupplementaryInsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public List<DepartmentLookupViewModel> Departments { get; set; } = new List<DepartmentLookupViewModel>();
        public List<ServiceCategoryLookupViewModel> ServiceCategories { get; set; } = new List<ServiceCategoryLookupViewModel>();
        public List<ServiceLookupViewModel> Services { get; set; } = new List<ServiceLookupViewModel>();
    }

    /// <summary>
    /// نوع انتخاب خدمات برای تعرفه گروهی
    /// </summary>
    public enum BulkSelectionType
    {
        [Display(Name = "همه خدمات")]
        AllServices = 1,
        
        [Display(Name = "بر اساس دپارتمان")]
        ByDepartment = 2,
        
        [Display(Name = "بر اساس دسته‌بندی")]
        ByServiceCategory = 3,
        
        [Display(Name = "بر اساس محدوده قیمت")]
        ByPriceRange = 4,
        
        [Display(Name = "انتخاب دستی")]
        ManualSelection = 5
    }

    /// <summary>
    /// نوع محاسبه قیمت
    /// </summary>
    public enum PriceCalculationType
    {
        [Display(Name = "خودکار (بر اساس قیمت خدمات)")]
        Auto = 1,
        
        [Display(Name = "ثابت")]
        Fixed = 2,
        
        [Display(Name = "ضریبی")]
        Multiplier = 3,
        
        [Display(Name = "محدوده")]
        Range = 4
    }

    /// <summary>
    /// تنظیمات پیشرفته تعرفه گروهی
    /// </summary>
    public class BulkTariffAdvancedSettings
    {
        /// <summary>
        /// اعمال تنظیمات به تعرفه‌های موجود
        /// </summary>
        [Display(Name = "اعمال به تعرفه‌های موجود")]
        public bool ApplyToExistingTariffs { get; set; } = false;

        /// <summary>
        /// حذف تعرفه‌های موجود قبل از ایجاد جدید
        /// </summary>
        [Display(Name = "حذف تعرفه‌های موجود")]
        public bool DeleteExistingTariffs { get; set; } = false;

        /// <summary>
        /// ایجاد نسخه پشتیبان قبل از تغییرات
        /// </summary>
        [Display(Name = "ایجاد نسخه پشتیبان")]
        public bool CreateBackup { get; set; } = true;

        /// <summary>
        /// ارسال اعلان پس از تکمیل
        /// </summary>
        [Display(Name = "ارسال اعلان")]
        public bool SendNotification { get; set; } = true;

        /// <summary>
        /// محدودیت تعداد خدمات
        /// </summary>
        [Display(Name = "حداکثر تعداد خدمات")]
        [Range(1, 1000, ErrorMessage = "تعداد خدمات باید بین 1 تا 1000 باشد")]
        public int MaxServicesLimit { get; set; } = 100;
    }

    /// <summary>
    /// ViewModel برای نمایش نتایج تعرفه گروهی
    /// </summary>
    public class BulkTariffResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalServices { get; set; }
        public int CreatedTariffs { get; set; }
        public int UpdatedTariffs { get; set; }
        public int DeletedTariffs { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public DateTime ProcessedAt { get; set; }
        public TimeSpan ProcessingTime { get; set; }
    }
}
