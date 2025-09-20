using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.Insurance.InsuranceTariff
{
    /// <summary>
    /// ViewModel برای نمایش لیست تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffIndexViewModel
    {
        public int InsuranceTariffId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public int InsurancePlanId { get; set; }
        public string InsurancePlanName { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal? TariffPrice { get; set; }
        public decimal? PatientShare { get; set; }
        public decimal? InsurerShare { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string CreatedByUserName { get; set; }

        public static InsuranceTariffIndexViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            return new InsuranceTariffIndexViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                ServiceId = entity.ServiceId,
                ServiceTitle = entity.Service?.Title,
                ServiceCode = entity.Service?.ServiceCode,
                InsurancePlanId = entity.InsurancePlanId ?? 0,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                TariffPrice = entity.TariffPrice,
                PatientShare = entity.PatientShare,
                InsurerShare = entity.InsurerShare,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToString("yyyy/MM/dd"), // TODO: تبدیل به شمسی
                CreatedByUserName = entity.CreatedByUser?.UserName
            };
        }
    }

    /// <summary>
    /// ViewModel برای جزئیات تعرفه بیمه
    /// </summary>
    public class InsuranceTariffDetailsViewModel
    {
        public int InsuranceTariffId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceDescription { get; set; }
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryTitle { get; set; }
        public int InsurancePlanId { get; set; }
        public string InsurancePlanName { get; set; }
        public string InsurancePlanCode { get; set; }
        public int InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal? TariffPrice { get; set; }
        public decimal? PatientShare { get; set; }
        public decimal? InsurerShare { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtShamsi { get; set; }
        public string UpdatedByUserName { get; set; }

        public static InsuranceTariffDetailsViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            return new InsuranceTariffDetailsViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                ServiceId = entity.ServiceId,
                ServiceTitle = entity.Service?.Title,
                ServiceCode = entity.Service?.ServiceCode,
                ServiceDescription = entity.Service?.Description,
                ServiceCategoryId = entity.Service?.ServiceCategoryId ?? 0,
                ServiceCategoryTitle = entity.Service?.ServiceCategory?.Title,
                InsurancePlanId = entity.InsurancePlanId ?? 0,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsurancePlanCode = entity.InsurancePlan?.PlanCode,
                InsuranceProviderId = entity.InsurancePlan?.InsuranceProviderId ?? 0,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                TariffPrice = entity.TariffPrice,
                PatientShare = entity.PatientShare,
                InsurerShare = entity.InsurerShare,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToString("yyyy/MM/dd"), // TODO: تبدیل به شمسی
                CreatedByUserName = entity.CreatedByUser?.UserName,
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt?.ToString("yyyy/MM/dd"), // TODO: تبدیل به شمسی
                UpdatedByUserName = entity.UpdatedByUser?.UserName
            };
        }
    }

    /// <summary>
    /// ViewModel برای ایجاد و ویرایش تعرفه بیمه
    /// </summary>
    public class InsuranceTariffCreateEditViewModel
    {
        public int InsuranceTariffId { get; set; }

        [Required(ErrorMessage = "انتخاب دپارتمان الزامی است.")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "سرفصل خدمت")]
        public int? ServiceCategoryId { get; set; }

        [Display(Name = "خدمت")]
        public int? ServiceId { get; set; }

        [Required(ErrorMessage = "انتخاب ارائه‌دهنده بیمه الزامی است.")]
        [Display(Name = "ارائه‌دهنده بیمه")]
        public int InsuranceProviderId { get; set; }

        [Required(ErrorMessage = "انتخاب طرح بیمه الزامی است.")]
        [Display(Name = "طرح بیمه")]
        public int InsurancePlanId { get; set; }

        [Display(Name = "قیمت تعرفه (تومان)")]
        [Range(0, double.MaxValue, ErrorMessage = "قیمت تعرفه نمی‌تواند منفی باشد.")]
        public decimal? TariffPrice { get; set; }

        [Display(Name = "سهم بیمار (تومان)")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمار نمی‌تواند منفی باشد.")]
        public decimal? PatientShare { get; set; }

        [Display(Name = "سهم بیمه (تومان)")]
        [Range(0, double.MaxValue, ErrorMessage = "سهم بیمه نمی‌تواند منفی باشد.")]
        public decimal? InsurerShare { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        // Fields for "All" selections
        [Display(Name = "همه سرفصل‌ها")]
        public bool IsAllServiceCategories { get; set; }

        [Display(Name = "همه خدمات")]
        public bool IsAllServices { get; set; }

        // Navigation Properties
        public string ServiceTitle { get; set; }
        public string InsurancePlanName { get; set; }
        public string InsuranceProviderName { get; set; }

        // SelectLists
        public System.Web.Mvc.SelectList DepartmentSelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceCategorySelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceSelectList { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviderSelectList { get; set; }
        public System.Web.Mvc.SelectList InsurancePlanSelectList { get; set; }

        public static InsuranceTariffCreateEditViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            return new InsuranceTariffCreateEditViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                DepartmentId = entity.Service?.ServiceCategory?.DepartmentId ?? 0,
                ServiceCategoryId = entity.Service?.ServiceCategoryId,
                ServiceId = entity.ServiceId,
                InsuranceProviderId = entity.InsurancePlan?.InsuranceProviderId ?? 0,
                InsurancePlanId = entity.InsurancePlanId ?? 0,
                TariffPrice = entity.TariffPrice,
                PatientShare = entity.PatientShare,
                InsurerShare = entity.InsurerShare,
                IsActive = entity.IsActive,
                ServiceTitle = entity.Service?.Title,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name
            };
        }
    }

    /// <summary>
    /// ViewModel برای صفحه اصلی تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffIndexPageViewModel
    {
        public PagedResult<InsuranceTariffIndexViewModel> Tariffs { get; set; }
        public InsuranceTariffFilterViewModel Filter { get; set; }
        public InsuranceTariffStatisticsViewModel Statistics { get; set; }

        public InsuranceTariffIndexPageViewModel()
        {
            Filter = new InsuranceTariffFilterViewModel();
            Statistics = new InsuranceTariffStatisticsViewModel();
            Tariffs = new PagedResult<InsuranceTariffIndexViewModel>();
        }

        public InsuranceTariffIndexPageViewModel(
            PagedResult<InsuranceTariffIndexViewModel> insuranceTariffs,
            InsuranceTariffFilterViewModel filter,
            InsuranceTariffStatisticsViewModel statistics)
        {
            Tariffs = insuranceTariffs;
            Filter = filter;
            Statistics = statistics;
        }
    }

    /// <summary>
    /// ViewModel برای فیلتر تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffFilterViewModel
    {
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "طرح بیمه")]
        public int? InsurancePlanId { get; set; }

        [Display(Name = "خدمت")]
        public int? ServiceId { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public int? InsuranceProviderId { get; set; }

        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "اندازه صفحه")]
        public int PageSize { get; set; } = 10;

        // SelectLists
        public System.Web.Mvc.SelectList InsurancePlanSelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceSelectList { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviderSelectList { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffStatisticsViewModel
    {
        [Display(Name = "تعداد کل تعرفه‌های بیمه")]
        public int TotalTariffs { get; set; }

        [Display(Name = "تعرفه‌های با قیمت خاص")]
        public int TariffsWithCustomPrice { get; set; }

        [Display(Name = "تعرفه‌های با سهم بیمار خاص")]
        public int TariffsWithCustomPatientShare { get; set; }

        [Display(Name = "تعرفه‌های با سهم بیمه خاص")]
        public int TariffsWithCustomInsurerShare { get; set; }

        [Display(Name = "درصد تعرفه‌های با قیمت خاص")]
        public double CustomPricePercentage => TotalTariffs > 0 ? (double)TariffsWithCustomPrice / TotalTariffs * 100 : 0;

        [Display(Name = "درصد تعرفه‌های با سهم بیمار خاص")]
        public double CustomPatientSharePercentage => TotalTariffs > 0 ? (double)TariffsWithCustomPatientShare / TotalTariffs * 100 : 0;

        [Display(Name = "درصد تعرفه‌های با سهم بیمه خاص")]
        public double CustomInsurerSharePercentage => TotalTariffs > 0 ? (double)TariffsWithCustomInsurerShare / TotalTariffs * 100 : 0;
    }

    /// <summary>
    /// ViewModel برای Lookup تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffLookupViewModel
    {
        public int InsuranceTariffId { get; set; }
        public string ServiceTitle { get; set; }
        public string InsurancePlanName { get; set; }
        public string InsuranceProviderName { get; set; }
        public decimal? TariffPrice { get; set; }
        public decimal? PatientShare { get; set; }
        public decimal? InsurerShare { get; set; }

        public static InsuranceTariffLookupViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            return new InsuranceTariffLookupViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                ServiceTitle = entity.Service?.Title,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name,
                TariffPrice = entity.TariffPrice,
                PatientShare = entity.PatientShare,
                InsurerShare = entity.InsurerShare
            };
        }
    }
}