using System.Collections.Generic;
using ClinicApp.Core;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.Insurance.Supplementary
{
    /// <summary>
    /// ViewModel برای صفحه اصلی مدیریت تعرفه‌های بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// </summary>
    public class SupplementaryTariffIndexPageViewModel
    {
        /// <summary>
        /// لیست تعرفه‌های بیمه تکمیلی
        /// </summary>
        public PagedResult<SupplementaryTariffIndexViewModel> Tariffs { get; set; }

        /// <summary>
        /// فیلترهای جستجو
        /// </summary>
        public SupplementaryTariffFilterViewModel Filter { get; set; }

        /// <summary>
        /// آمار تعرفه‌های بیمه تکمیلی
        /// </summary>
        public SupplementaryTariffStatisticsViewModel Statistics { get; set; }

        /// <summary>
        /// لیست طرح‌های بیمه برای فیلتر
        /// </summary>
        public List<SupplementaryTariffInsurancePlanViewModel> InsurancePlans { get; set; }

        /// <summary>
        /// لیست ارائه‌دهندگان بیمه برای فیلتر
        /// </summary>
        public List<SupplementaryTariffInsuranceProviderViewModel> InsuranceProviders { get; set; }

        /// <summary>
        /// لیست دپارتمان‌ها برای فیلتر
        /// </summary>
        public List<SupplementaryTariffDepartmentViewModel> Departments { get; set; }

        /// <summary>
        /// لیست خدمات برای فیلتر
        /// </summary>
        public List<SupplementaryTariffServiceViewModel> Services { get; set; }

        // Properties for backward compatibility with Index view
        /// <summary>
        /// تعداد کل خدمات (از Statistics)
        /// </summary>
        public int TotalServices => Statistics?.TotalServices ?? 0;

        /// <summary>
        /// تعداد کل تعرفه‌ها (از Statistics)
        /// </summary>
        public int TotalTariffs => Statistics?.TotalSupplementaryTariffs ?? 0;

        /// <summary>
        /// تعداد تعرفه‌های فعال (از Statistics)
        /// </summary>
        public int ActiveTariffs => Statistics?.ActiveSupplementaryTariffs ?? 0;

        /// <summary>
        /// تعداد تعرفه‌های منقضی (از Statistics)
        /// </summary>
        public int ExpiredTariffs => Statistics?.ExpiredSupplementaryTariffs ?? 0;

        /// <summary>
        /// فیلترها (از Filter)
        /// </summary>
        public SupplementaryTariffFilterViewModel Filters => Filter;

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        public SupplementaryTariffIndexPageViewModel()
        {
            Tariffs = new PagedResult<SupplementaryTariffIndexViewModel>();
            Filter = new SupplementaryTariffFilterViewModel();
            Statistics = new SupplementaryTariffStatisticsViewModel();
            InsurancePlans = new List<SupplementaryTariffInsurancePlanViewModel>();
            InsuranceProviders = new List<SupplementaryTariffInsuranceProviderViewModel>();
            Departments = new List<SupplementaryTariffDepartmentViewModel>();
            Services = new List<SupplementaryTariffServiceViewModel>();
        }

        /// <summary>
        /// سازنده با پارامترها
        /// </summary>
        public SupplementaryTariffIndexPageViewModel(
            PagedResult<SupplementaryTariffIndexViewModel> tariffs,
            SupplementaryTariffFilterViewModel filter,
            SupplementaryTariffStatisticsViewModel statistics)
        {
            Tariffs = tariffs;
            Filter = filter;
            Statistics = statistics;
            InsurancePlans = new List<SupplementaryTariffInsurancePlanViewModel>();
            InsuranceProviders = new List<SupplementaryTariffInsuranceProviderViewModel>();
            Departments = new List<SupplementaryTariffDepartmentViewModel>();
            Services = new List<SupplementaryTariffServiceViewModel>();
        }
    }

    /// <summary>
    /// ViewModel برای فیلتر تعرفه‌های بیمه تکمیلی
    /// </summary>
    public class SupplementaryTariffFilterViewModel
    {
        /// <summary>
        /// عبارت جستجو
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// شناسه طرح بیمه
        /// </summary>
        public int? InsurancePlanId { get; set; }

        /// <summary>
        /// شناسه ارائه‌دهنده بیمه
        /// </summary>
        public int? InsuranceProviderId { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// تاریخ شروع از
        /// </summary>
        public System.DateTime? StartDateFrom { get; set; }

        /// <summary>
        /// تاریخ شروع تا
        /// </summary>
        public System.DateTime? StartDateTo { get; set; }

        /// <summary>
        /// تاریخ پایان از
        /// </summary>
        public System.DateTime? EndDateFrom { get; set; }

        /// <summary>
        /// تاریخ پایان تا
        /// </summary>
        public System.DateTime? EndDateTo { get; set; }

        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// لیست خدمات برای dropdown
        /// </summary>
        public List<SupplementaryTariffServiceViewModel> Services { get; set; } = new List<SupplementaryTariffServiceViewModel>();

        /// <summary>
        /// لیست طرح‌های بیمه برای dropdown
        /// </summary>
        public List<SupplementaryTariffInsurancePlanViewModel> InsurancePlans { get; set; } = new List<SupplementaryTariffInsurancePlanViewModel>();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<SupplementaryTariffDepartmentViewModel> Departments { get; set; } = new List<SupplementaryTariffDepartmentViewModel>();

        /// <summary>
        /// لیست ارائه‌دهندگان بیمه برای dropdown
        /// </summary>
        public List<SupplementaryTariffInsuranceProviderViewModel> InsuranceProviders { get; set; } = new List<SupplementaryTariffInsuranceProviderViewModel>();
    }

    /// <summary>
    /// ViewModel برای آمار تعرفه‌های بیمه تکمیلی
    /// </summary>
    public class SupplementaryTariffStatisticsViewModel
    {
        /// <summary>
        /// تعداد کل خدمات
        /// </summary>
        public int TotalServices { get; set; }

        /// <summary>
        /// تعداد کل تعرفه‌های بیمه تکمیلی
        /// </summary>
        public int TotalSupplementaryTariffs { get; set; }

        /// <summary>
        /// تعداد تعرفه‌های فعال
        /// </summary>
        public int ActiveSupplementaryTariffs { get; set; }

        /// <summary>
        /// تعداد تعرفه‌های منقضی
        /// </summary>
        public int ExpiredSupplementaryTariffs { get; set; }

        /// <summary>
        /// تعداد تعرفه‌های آینده
        /// </summary>
        public int FutureSupplementaryTariffs { get; set; }

        /// <summary>
        /// تعداد تعرفه‌های غیرفعال
        /// </summary>
        public int InactiveSupplementaryTariffs { get; set; }

        /// <summary>
        /// درصد تعرفه‌های فعال
        /// </summary>
        public double ActivePercentage => TotalSupplementaryTariffs > 0 ? (double)ActiveSupplementaryTariffs / TotalSupplementaryTariffs * 100 : 0;

        /// <summary>
        /// درصد تعرفه‌های منقضی
        /// </summary>
        public double ExpiredPercentage => TotalSupplementaryTariffs > 0 ? (double)ExpiredSupplementaryTariffs / TotalSupplementaryTariffs * 100 : 0;

        /// <summary>
        /// درصد تعرفه‌های آینده
        /// </summary>
        public double FuturePercentage => TotalSupplementaryTariffs > 0 ? (double)FutureSupplementaryTariffs / TotalSupplementaryTariffs * 100 : 0;

        /// <summary>
        /// درصد تعرفه‌های غیرفعال
        /// </summary>
        public double InactivePercentage => TotalSupplementaryTariffs > 0 ? (double)InactiveSupplementaryTariffs / TotalSupplementaryTariffs * 100 : 0;
    }
}
