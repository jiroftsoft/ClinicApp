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
        
        /// <summary>
        /// نام خدمت (برای سازگاری با View)
        /// </summary>
        public string ServiceName => ServiceTitle;
        
        /// <summary>
        /// نام دسته‌بندی خدمت
        /// </summary>
        public string ServiceCategoryName { get; set; }
        
        /// <summary>
        /// وضعیت فعال/غیرفعال
        /// </summary>
        public bool IsActive { get; set; } = true;

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
                CreatedByUserName = entity.CreatedByUser?.UserName,
                ServiceCategoryName = entity.Service?.ServiceCategory?.Title,
                IsActive = !entity.IsDeleted // فرض: اگر حذف نشده باشد، فعال است
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
                
                // 🔍 DEBUG LOGGING - InsuranceProviderId calculation
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

        /// <summary>
        /// RowVersion برای مدیریت همزمانی (Concurrency Control)
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// کلید امنیتی برای جلوگیری از ارسال تکراری فرم
        /// </summary>
        [Required(ErrorMessage = "کلید امنیتی موجود نیست")]
        public string IdempotencyKey { get; set; }

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

        [Display(Name = "درصد سهم بیمار")]
        [Range(0, 100, ErrorMessage = "درصد سهم بیمار باید بین 0 تا 100 باشد.")]
        public decimal? PatientSharePercent { get; set; }

        [Display(Name = "درصد سهم بیمه")]
        [Range(0, 100, ErrorMessage = "درصد سهم بیمه باید بین 0 تا 100 باشد.")]
        public decimal? InsurerSharePercent { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاریخ شروع اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ پایان اعتبار")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "یادداشت‌های اضافی")]
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Notes { get; set; }

        // Fields for Supplementary Insurance
        [Display(Name = "بیمه پایه")]
        public int PrimaryInsurancePlanId { get; set; }

        [Display(Name = "درصد پوشش تکمیلی")]
        [Range(0, 100, ErrorMessage = "درصد پوشش باید بین 0 تا 100 باشد.")]
        public decimal SupplementaryCoveragePercent { get; set; }

        [Display(Name = "اولویت")]
        [Range(1, 10, ErrorMessage = "اولویت باید بین 1 تا 10 باشد.")]
        public int Priority { get; set; } = 5;

        // Fields for "All" selections
        [Display(Name = "همه سرفصل‌ها")]
        public bool IsAllServiceCategories { get; set; }

        [Display(Name = "همه خدمات")]
        public bool IsAllServices { get; set; }

        // Navigation Properties
        public string ServiceTitle { get; set; }
        public string InsurancePlanName { get; set; }
        public string InsuranceProviderName { get; set; }

        // SelectLists - برای سازگاری با Viewها
        public System.Web.Mvc.SelectList Departments { get; set; }
        public System.Web.Mvc.SelectList ServiceCategories { get; set; }
        public System.Web.Mvc.SelectList Services { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviders { get; set; }
        public System.Web.Mvc.SelectList InsurancePlans { get; set; }

        // Legacy SelectLists - برای سازگاری با کد قدیمی
        public System.Web.Mvc.SelectList DepartmentSelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceCategorySelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceSelectList { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviderSelectList { get; set; }
        public System.Web.Mvc.SelectList InsurancePlanSelectList { get; set; }

        public static InsuranceTariffCreateEditViewModel FromEntity(Models.Entities.Insurance.InsuranceTariff entity)
        {
            var result = new InsuranceTariffCreateEditViewModel
            {
                InsuranceTariffId = entity.InsuranceTariffId,
                RowVersion = entity.RowVersion,
                DepartmentId = entity.Service?.ServiceCategory?.DepartmentId ?? 0,
                ServiceCategoryId = entity.Service?.ServiceCategoryId ?? 0,
                ServiceId = entity.ServiceId,
                InsuranceProviderId = entity.InsurancePlan?.InsuranceProviderId ?? 0,
                InsurancePlanId = entity.InsurancePlanId ?? 0,
                TariffPrice = entity.TariffPrice ?? 0,
                // 🔍 FIX: PatientShare و InsurerShare در دیتابیس به عنوان مبلغ ذخیره می‌شوند
                PatientShare = entity.PatientShare ?? 0,
                InsurerShare = entity.InsurerShare ?? 0,
                // درصدها از مبلغ‌ها محاسبه می‌شوند - DEBUG LOGGING
                PatientSharePercent = entity.TariffPrice > 0 && entity.PatientShare.HasValue ? 
                    Math.Round((entity.PatientShare.Value / entity.TariffPrice.Value) * 100m, 2, MidpointRounding.AwayFromZero) : 0,
                InsurerSharePercent = entity.TariffPrice > 0 && entity.InsurerShare.HasValue ? 
                    Math.Round((entity.InsurerShare.Value / entity.TariffPrice.Value) * 100m, 2, MidpointRounding.AwayFromZero) : 0,
                IsActive = entity.IsActive,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Notes = entity.Notes,
                ServiceTitle = entity.Service?.Title,
                InsurancePlanName = entity.InsurancePlan?.Name,
                InsuranceProviderName = entity.InsurancePlan?.InsuranceProvider?.Name
            };
            
            // 🔍 DEBUG LOGGING - ViewModel Calculation (Console)
            Console.WriteLine($"🔍 ViewModel FromEntity - TariffId: {entity.InsuranceTariffId}");
            Console.WriteLine($"🔍   TariffPrice: {entity.TariffPrice}, PatientShare: {entity.PatientShare}, InsurerShare: {entity.InsurerShare}");
            Console.WriteLine($"🔍   PatientSharePercent: {result.PatientSharePercent}, InsurerSharePercent: {result.InsurerSharePercent}");
            
            // 🔍 ADDITIONAL DEBUG - Check if values are already percentages
            if (entity.PatientShare.HasValue && entity.InsurerShare.HasValue && entity.TariffPrice.HasValue)
            {
                var patientRatio = entity.PatientShare.Value / entity.TariffPrice.Value;
                var insurerRatio = entity.InsurerShare.Value / entity.TariffPrice.Value;
                Console.WriteLine($"🔍   Ratios: Patient={patientRatio:F4}, Insurer={insurerRatio:F4}");
                Console.WriteLine($"🔍   If PatientShare is already percentage: {entity.PatientShare.Value}% of {entity.TariffPrice.Value} = {entity.PatientShare.Value / 100m * entity.TariffPrice.Value:F0}");
            }
            
            return result;
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
        
        /// <summary>
        /// اطلاعات صفحه‌بندی برای دسترسی آسان در View
        /// </summary>
        public PagedResult<InsuranceTariffIndexViewModel> Pagination => Tariffs;

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

        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "طرح بیمه")]
        public int? InsurancePlanId { get; set; }

        [Display(Name = "خدمت")]
        public int? ServiceId { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public int? InsuranceProviderId { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        [Display(Name = "شماره صفحه")]
        public int PageNumber { get; set; } = 1;

        [Display(Name = "اندازه صفحه")]
        public int PageSize { get; set; } = 10;

        // 🚀 P0 FIX: SelectLists برای فیلترهای کامل
        public System.Web.Mvc.SelectList Departments { get; set; }
        public System.Web.Mvc.SelectList InsurancePlanSelectList { get; set; }
        public System.Web.Mvc.SelectList ServiceSelectList { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviders { get; set; }
        public System.Web.Mvc.SelectList InsuranceProviderSelectList { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار تعرفه‌های بیمه
    /// </summary>
    public class InsuranceTariffStatisticsViewModel
    {
        [Display(Name = "تعداد کل تعرفه‌های بیمه")]
        public int TotalTariffs { get; set; }

        [Display(Name = "تعرفه‌های فعال")]
        public int ActiveTariffs { get; set; }

        [Display(Name = "تعرفه‌های غیرفعال")]
        public int InactiveTariffs { get; set; }

        [Display(Name = "خدمات تحت پوشش")]
        public int TotalServices { get; set; }

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