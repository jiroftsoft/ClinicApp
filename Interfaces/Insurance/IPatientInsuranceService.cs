using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Service Interface برای مدیریت بیمه‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل بیمه‌های بیماران (Primary و Supplementary)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. مدیریت روابط با Patient و InsurancePlan
    /// 8. مدیریت بیمه اصلی و تکمیلی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار بیمه‌های بیماران
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IPatientInsuranceService
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت لیست بیمه‌های بیماران با صفحه‌بندی و جستجو
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده بیمه‌های بیماران</returns>
        Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPatientInsurancesAsync(int? patientId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت لیست بیمه‌های بیماران با فیلترهای کامل
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <param name="isPrimary">نوع بیمه (اصلی/تکمیلی)</param>
        /// <param name="isActive">وضعیت فعال</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده بیمه‌های بیماران</returns>
        Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPatientInsurancesWithFiltersAsync(
            int? patientId = null, 
            string searchTerm = "", 
            int? providerId = null, 
            bool? isPrimary = null, 
            bool? isActive = null, 
            int pageNumber = 1, 
            int pageSize = 10);

        /// <summary>
        /// دریافت لیست بیمه‌های بیماران با صفحه‌بندی و جستجو (بهینه‌سازی شده)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="isPrimary">نوع بیمه (اصلی/تکمیلی)</param>
        /// <param name="isActive">وضعیت فعال</param>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده بیمه‌های بیماران</returns>
        Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedAsync(
            string searchTerm = null,
            int? providerId = null,
            int? planId = null,
            bool? isPrimary = null,
            bool? isActive = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// دریافت جزئیات بیمه بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>جزئیات بیمه بیمار</returns>
        Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPatientInsuranceDetailsAsync(int patientInsuranceId);

        /// <summary>
        /// دریافت وضعیت بیمه بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>وضعیت بیمه بیمار</returns>
        Task<ServiceResult<object>> GetPatientInsuranceStatusForReceptionAsync(int patientId);

        /// <summary>
        /// دریافت بیمه‌های بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        Task<ServiceResult<object>> GetPatientInsurancesForReceptionAsync(int patientId);

        /// <summary>
        /// دریافت بیمه بیمار برای ویرایش
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>بیمه بیمار برای ویرایش</returns>
        Task<ServiceResult<PatientInsuranceCreateEditViewModel>> GetPatientInsuranceForEditAsync(int patientInsuranceId);

        /// <summary>
        /// ایجاد بیمه بیمار جدید
        /// </summary>
        /// <param name="model">مدل ایجاد بیمه بیمار</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<int>> CreatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی بیمه بیمار
        /// </summary>
        /// <param name="model">مدل به‌روزرسانی بیمه بیمار</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);

        /// <summary>
        /// حذف نرم بیمه بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> SoftDeletePatientInsuranceAsync(int patientInsuranceId);

        /// <summary>
        /// متد debug برای بررسی تعداد رکوردها
        /// </summary>
        /// <returns>تعداد کل رکوردهای بیمه‌های بیماران</returns>
        Task<ServiceResult<int>> GetTotalRecordsCountAsync();
        Task<ServiceResult<List<object>>> GetSimpleListAsync();

        #endregion

        #region Lookup Operations

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار برای Lookup
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های فعال بیمار</returns>
        Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActivePatientInsurancesForLookupAsync(int patientId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود شماره بیمه
        /// </summary>
        /// <param name="policyNumber">شماره بیمه</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesPolicyNumberExistAsync(string policyNumber, int? excludeId = null);

        /// <summary>
        /// بررسی وجود بیمه اصلی برای بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesPrimaryInsuranceExistAsync(int patientId, int? excludeId = null);

        /// <summary>
        /// بررسی تداخل تاریخ بیمه‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesDateOverlapExistAsync(int patientId, System.DateTime startDate, System.DateTime endDate, int? excludeId = null);

        /// <summary>
        /// اعتبارسنجی کامل بیمه بیمار (منطق کسب‌وکار)
        /// </summary>
        /// <param name="model">مدل بیمه بیمار</param>
        /// <returns>نتیجه اعتبارسنجی با لیست خطاها</returns>
        Task<ServiceResult<Dictionary<string, string>>> ValidatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model);

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت بیمه‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetPatientInsurancesByPatientAsync(int patientId);

        /// <summary>
        /// دریافت فقط بیمه‌های تکمیلی بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های تکمیلی بیمار</returns>
        Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetSupplementaryInsurancesByPatientAsync(int patientId);

        /// <summary>
        /// دریافت بیمه اصلی بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>بیمه اصلی بیمار</returns>
        Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPrimaryInsuranceByPatientAsync(int patientId);


        /// <summary>
        /// تنظیم بیمه اصلی بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>نتیجه تنظیم</returns>
        Task<ServiceResult> SetPrimaryInsuranceAsync(int patientInsuranceId);

        /// <summary>
        /// بررسی اعتبار بیمه بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <param name="checkDate">تاریخ بررسی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientInsuranceId, System.DateTime checkDate);

        #endregion

        #region Combined Insurance Calculation Methods

        /// <summary>
        /// محاسبه بیمه ترکیبی برای بیمار و خدمت مشخص
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="serviceAmount">مبلغ خدمت</param>
        /// <param name="calculationDate">تاریخ محاسبه (اختیاری)</param>
        /// <returns>نتیجه محاسبه بیمه ترکیبی</returns>
        Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceForPatientAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime? calculationDate = null);

        /// <summary>
        /// دریافت اطلاعات بیمه‌های فعال بیمار (اصلی + تکمیلی)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های فعال و تکمیلی</returns>
        Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActiveAndSupplementaryByPatientIdAsync(int patientId);

        /// <summary>
        /// بررسی وجود خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> ServiceExistsAsync(int serviceId);

        /// <summary>
        /// دریافت شماره بیمه پایه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>شماره بیمه پایه بیمار</returns>
        Task<ServiceResult<string>> GetPrimaryInsurancePolicyNumberAsync(int patientId);

        /// <summary>
        /// بررسی وجود بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> PatientExistsAsync(int patientId);

        /// <summary>
        /// بررسی وجود بیمه ترکیبی برای بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>آیا بیمار بیمه ترکیبی دارد یا نه</returns>
        Task<ServiceResult<bool>> HasCombinedInsuranceAsync(int patientId);

        #endregion

        #region Supplementary Insurance Management

        /// <summary>
        /// افزودن بیمه تکمیلی به رکورد بیمه پایه موجود
        /// </summary>
        /// <param name="model">مدل بیمه تکمیلی</param>
        /// <returns>نتیجه افزودن بیمه تکمیلی</returns>
        Task<ServiceResult<int>> AddSupplementaryInsuranceToExistingAsync(PatientInsuranceCreateEditViewModel model);

        #endregion

        #region Statistics Methods

        /// <summary>
        /// دریافت تعداد بیمه‌های فعال
        /// </summary>
        /// <returns>تعداد بیمه‌های فعال</returns>
        Task<int> GetActiveInsurancesCountAsync();

        /// <summary>
        /// دریافت تعداد بیمه‌های منقضی
        /// </summary>
        /// <returns>تعداد بیمه‌های منقضی</returns>
        Task<int> GetExpiredInsurancesCountAsync();

        #endregion
    }
}
