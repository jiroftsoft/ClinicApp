using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.InsurancePlan;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel بهینه شده برای ایجاد تعرفه گروهی بیمه تکمیلی
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
        /// شناسه‌های دپارتمان‌های انتخاب شده
        /// </summary>
        [Required(ErrorMessage = "انتخاب حداقل یک دپارتمان الزامی است.")]
        [Display(Name = "دپارتمان‌ها")]
        public List<int> SelectedDepartmentIds { get; set; } = new List<int>();

        /// <summary>
        /// شناسه‌های دسته‌بندی‌های انتخاب شده
        /// </summary>
        [Required(ErrorMessage = "انتخاب حداقل یک سرفصل الزامی است.")]
        [Display(Name = "سرفصل‌ها")]
        public List<int> SelectedServiceCategoryIds { get; set; } = new List<int>();

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

        // Navigation Properties
        public List<InsurancePlanLookupViewModel> PrimaryInsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public List<InsurancePlanLookupViewModel> SupplementaryInsurancePlans { get; set; } = new List<InsurancePlanLookupViewModel>();
        public List<DepartmentLookupViewModel> Departments { get; set; } = new List<DepartmentLookupViewModel>();
        public List<ServiceCategoryLookupViewModel> ServiceCategories { get; set; } = new List<ServiceCategoryLookupViewModel>();
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