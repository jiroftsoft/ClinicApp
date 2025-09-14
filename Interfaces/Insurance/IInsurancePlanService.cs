using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Service Interface برای مدیریت طرح‌های بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل طرح‌های بیمه (Basic, Standard, Premium, Supplementary)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. مدیریت روابط با InsuranceProvider
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار طرح‌های بیمه
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IInsurancePlanService
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت لیست طرح‌های بیمه با صفحه‌بندی و جستجو
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده طرح‌های بیمه</returns>
        Task<ServiceResult<PagedResult<InsurancePlanIndexViewModel>>> GetPlansAsync(int? providerId, string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت جزئیات طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>جزئیات طرح بیمه</returns>
        Task<ServiceResult<InsurancePlanDetailsViewModel>> GetPlanDetailsAsync(int planId);

        /// <summary>
        /// دریافت طرح بیمه برای ویرایش
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>طرح بیمه برای ویرایش</returns>
        Task<ServiceResult<InsurancePlanCreateEditViewModel>> GetPlanForEditAsync(int planId);

        /// <summary>
        /// ایجاد طرح بیمه جدید
        /// </summary>
        /// <param name="model">مدل ایجاد طرح بیمه</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<int>> CreatePlanAsync(InsurancePlanCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی طرح بیمه
        /// </summary>
        /// <param name="model">مدل به‌روزرسانی طرح بیمه</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdatePlanAsync(InsurancePlanCreateEditViewModel model);

        /// <summary>
        /// حذف نرم طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> SoftDeletePlanAsync(int planId);

        #endregion

        #region Lookup Operations

        /// <summary>
        /// دریافت طرح‌های بیمه فعال برای Lookup
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه (اختیاری)</param>
        /// <returns>لیست طرح‌های بیمه فعال</returns>
        Task<ServiceResult<List<InsurancePlanLookupViewModel>>> GetActivePlansForLookupAsync(int? providerId = null);

        /// <summary>
        /// دریافت لیست شرکت‌های بیمه برای SelectList
        /// </summary>
        /// <returns>لیست شرکت‌های بیمه فعال</returns>
        Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetActiveProvidersForLookupAsync();

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد طرح بیمه
        /// </summary>
        /// <param name="planCode">کد طرح بیمه</param>
        /// <param name="excludeId">شناسه طرح بیمه برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesPlanCodeExistAsync(string planCode, int? excludeId = null);

        /// <summary>
        /// بررسی وجود نام طرح بیمه در ارائه‌دهنده
        /// </summary>
        /// <param name="name">نام طرح بیمه</param>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه طرح بیمه برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesNameExistInProviderAsync(string name, int providerId, int? excludeId = null);

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>لیست طرح‌های بیمه ارائه‌دهنده</returns>
        Task<ServiceResult<List<InsurancePlanIndexViewModel>>> GetPlansByProviderAsync(int providerId);

        /// <summary>
        /// بررسی اعتبار طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="checkDate">تاریخ بررسی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult<bool>> IsPlanValidAsync(int planId, System.DateTime checkDate);

        #endregion
    }
}
