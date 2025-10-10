using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس محاسبه خدمات - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// اصول طراحی:
    /// ✅ Single Responsibility: فقط محاسبه قیمت خدمات
    /// ✅ Dependency Inversion: وابستگی به abstractions
    /// ✅ Clean Architecture: جداسازی کامل concerns
    /// ✅ Medical Environment: منطق‌های مخصوص محیط درمانی
    /// ✅ Error Handling: مدیریت کامل خطاها
    /// ✅ Logging: ثبت کامل عملیات
    /// </summary>
    public interface IServiceCalculationService
    {
        #region Basic Calculation Methods

        /// <summary>
        /// محاسبه قیمت پایه خدمت
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>قیمت پایه</returns>
        decimal CalculateServicePrice(Service service);

        /// <summary>
        /// محاسبه قیمت خدمت بر اساس شناسه
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>قیمت پایه</returns>
        decimal CalculateServicePrice(int serviceId, ApplicationDbContext context);

        /// <summary>
        /// محاسبه قیمت خدمت با تنظیمات مرکزی (FactorSettings)
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ محاسبه</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="financialYear">سال مالی</param>
        /// <returns>قیمت محاسبه شده</returns>
        decimal CalculateServicePriceWithFactorSettings(Service service, ApplicationDbContext context, 
            DateTime? date = null, int? departmentId = null, int? financialYear = null);

        /// <summary>
        /// محاسبه قیمت خدمت با جزئیات کامل
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ محاسبه</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="financialYear">سال مالی</param>
        /// <returns>جزئیات محاسبه</returns>
        ServiceCalculationDetails CalculateServicePriceWithDetails(Service service, ApplicationDbContext context,
            DateTime? date = null, int? departmentId = null, int? financialYear = null);

        #endregion

        #region Reception-Specific Calculation Methods

        /// <summary>
        /// محاسبه مجموع پذیرش
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceIds">لیست شناسه‌های خدمات</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>نتیجه محاسبه مجموع پذیرش</returns>
        Task<ServiceResult<decimal>> CalculateReceptionTotalAsync(int patientId, List<int> serviceIds, ApplicationDbContext context);

        /// <summary>
        /// محاسبه تخفیف
        /// </summary>
        /// <param name="totalAmount">مبلغ کل</param>
        /// <param name="discountCode">کد تخفیف</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>نتیجه محاسبه تخفیف</returns>
        Task<ServiceResult<decimal>> CalculateDiscountAsync(decimal totalAmount, string discountCode, ApplicationDbContext context);

        /// <summary>
        /// محاسبه مالیات
        /// </summary>
        /// <param name="totalAmount">مبلغ کل</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>نتیجه محاسبه مالیات</returns>
        Task<ServiceResult<decimal>> CalculateTaxAsync(decimal totalAmount, ApplicationDbContext context);

        /// <summary>
        /// محاسبه Real-time پذیرش
        /// </summary>
        /// <param name="model">مدل داده‌های پذیرش</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>نتیجه محاسبه Real-time</returns>
        Task<ServiceResult<object>> CalculateReceptionRealTimeAsync(object model, ApplicationDbContext context);

        /// <summary>
        /// محاسبه قیمت خدمت با کامپوننت‌ها (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>نتیجه محاسبه قیمت</returns>
        Task<ServiceResult<object>> CalculateServicePriceWithComponentsAsync(int serviceId, int patientId, ApplicationDbContext context);

        /// <summary>
        /// دریافت جزئیات محاسبه خدمت (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>جزئیات محاسبه</returns>
        Task<ServiceResult<object>> GetServiceCalculationDetailsAsync(int serviceId, int patientId, ApplicationDbContext context);

        /// <summary>
        /// دریافت وضعیت کامپوننت‌های خدمت (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>وضعیت کامپوننت‌ها</returns>
        Task<ServiceResult<object>> GetServiceComponentsStatusAsync(int serviceId, int patientId, ApplicationDbContext context);

        #endregion

        #region Shared Service Calculation Methods

        /// <summary>
        /// محاسبه قیمت خدمت مشترک با Override
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="overrideTechnicalFactor">ضریب فنی Override</param>
        /// <param name="overrideProfessionalFactor">ضریب حرفه‌ای Override</param>
        /// <param name="date">تاریخ محاسبه</param>
        /// <returns>نتیجه محاسبه</returns>
        Task<ServiceCalculationResult> CalculateSharedServicePriceAsync(int serviceId, int departmentId, 
            ApplicationDbContext context, decimal? overrideTechnicalFactor = null, 
            decimal? overrideProfessionalFactor = null, DateTime? date = null);

        /// <summary>
        /// بررسی اینکه آیا خدمت در دپارتمان مشترک است
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>true اگر مشترک باشد</returns>
        Task<bool> IsServiceSharedInDepartmentAsync(int serviceId, int departmentId, ApplicationDbContext context);

        #endregion

        #region Validation and Helper Methods

        /// <summary>
        /// بررسی اینکه آیا خدمت دارای اجزای کامل است
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>true اگر کامل باشد</returns>
        bool HasCompleteComponents(Service service);

        /// <summary>
        /// دریافت جزء فنی خدمت
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>جزء فنی</returns>
        ServiceComponent GetTechnicalComponent(Service service);

        /// <summary>
        /// دریافت جزء حرفه‌ای خدمت
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <returns>جزء حرفه‌ای</returns>
        ServiceComponent GetProfessionalComponent(Service service);

        /// <summary>
        /// دریافت سال مالی جاری
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>سال مالی</returns>
        int GetCurrentFinancialYear(DateTime? date = null);

        /// <summary>
        /// بررسی فریز بودن سال مالی
        /// </summary>
        /// <param name="financialYear">سال مالی</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <returns>true اگر فریز باشد</returns>
        bool IsFinancialYearFrozen(int financialYear, ApplicationDbContext context);

        #endregion

        #region Advanced Calculation Methods

        /// <summary>
        /// محاسبه قیمت با تخفیف
        /// </summary>
        /// <param name="basePrice">قیمت پایه</param>
        /// <param name="discountPercent">درصد تخفیف</param>
        /// <returns>قیمت با تخفیف</returns>
        decimal CalculateServicePriceWithDiscount(decimal basePrice, decimal discountPercent);

        /// <summary>
        /// محاسبه قیمت با مالیات
        /// </summary>
        /// <param name="basePrice">قیمت پایه</param>
        /// <param name="taxPercent">درصد مالیات</param>
        /// <returns>قیمت با مالیات</returns>
        decimal CalculateServicePriceWithTax(decimal basePrice, decimal taxPercent);

        /// <summary>
        /// محاسبه قیمت با منطق هشتگ
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ محاسبه</param>
        /// <returns>قیمت با منطق هشتگ</returns>
        decimal CalculateServicePriceWithHashtagLogic(Service service, ApplicationDbContext context, DateTime? date = null);

        /// <summary>
        /// محاسبه قیمت با Override دپارتمان
        /// </summary>
        /// <param name="service">خدمت مورد نظر</param>
        /// <param name="departmentId">شناسه دپارتمان</param>
        /// <param name="context">کانتکست دیتابیس</param>
        /// <param name="date">تاریخ محاسبه</param>
        /// <returns>قیمت با Override</returns>
        decimal CalculateServicePriceWithDepartmentOverride(Service service, int departmentId, 
            ApplicationDbContext context, DateTime? date = null);

        #endregion
    }

    /// <summary>
    /// نتیجه محاسبه خدمت مشترک
    /// </summary>
    public class ServiceCalculationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal CalculatedPrice { get; set; }
        public ServiceCalculationDetails Details { get; set; }
        public string CalculationFormula { get; set; }
        public bool HasOverride { get; set; }
        public int FinancialYear { get; set; }
    }

    /// <summary>
    /// جزئیات محاسبه خدمت
    /// </summary>
    public class ServiceCalculationDetails
    {
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public string ServiceCode { get; set; }
        public bool IsHashtagged { get; set; }
        public decimal TechnicalPart { get; set; }
        public decimal ProfessionalPart { get; set; }
        public decimal TechnicalFactor { get; set; }
        public decimal ProfessionalFactor { get; set; }
        public decimal TechnicalAmount { get; set; }
        public decimal ProfessionalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool HasDepartmentOverride { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime CalculationDate { get; set; }
    }
}
