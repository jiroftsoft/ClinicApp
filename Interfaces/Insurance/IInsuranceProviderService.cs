using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Service Interface برای مدیریت ارائه‌دهندگان بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ارائه‌دهندگان بیمه (SSO, FREE, MILITARY, HEALTH, SUPPLEMENTARY)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار ارائه‌دهندگان بیمه
    /// ✅ Separation of Concerns: Repository layer فقط دسترسی به داده
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer منطق کسب‌وکار
    /// </summary>
    public interface IInsuranceProviderService
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت لیست ارائه‌دهندگان بیمه با صفحه‌بندی و جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده ارائه‌دهندگان بیمه</returns>
        Task<ServiceResult<PagedResult<InsuranceProviderIndexViewModel>>> GetProvidersAsync(string searchTerm, int pageNumber, int pageSize);

        /// <summary>
        /// دریافت جزئیات ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>جزئیات ارائه‌دهنده بیمه</returns>
        Task<ServiceResult<InsuranceProviderDetailsViewModel>> GetProviderDetailsAsync(int providerId);

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه برای ویرایش
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>ارائه‌دهنده بیمه برای ویرایش</returns>
        Task<ServiceResult<InsuranceProviderCreateEditViewModel>> GetProviderForEditAsync(int providerId);

        /// <summary>
        /// ایجاد ارائه‌دهنده بیمه جدید
        /// </summary>
        /// <param name="model">مدل ایجاد ارائه‌دهنده بیمه</param>
        /// <returns>نتیجه ایجاد</returns>
        Task<ServiceResult<int>> CreateProviderAsync(InsuranceProviderCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="model">مدل به‌روزرسانی ارائه‌دهنده بیمه</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<ServiceResult> UpdateProviderAsync(InsuranceProviderCreateEditViewModel model);

        /// <summary>
        /// حذف نرم ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>نتیجه حذف</returns>
        Task<ServiceResult> SoftDeleteProviderAsync(int providerId);

        #endregion

        #region Lookup Operations

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه فعال برای Lookup
        /// </summary>
        /// <returns>لیست ارائه‌دهندگان بیمه فعال</returns>
        Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetActiveProvidersForLookupAsync();

        /// <summary>
        /// دریافت بیمه‌گذاران پایه
        /// </summary>
        /// <returns>لیست بیمه‌گذاران پایه</returns>
        Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetPrimaryInsuranceProvidersAsync();

        /// <summary>
        /// دریافت بیمه‌گذاران تکمیلی
        /// </summary>
        /// <returns>لیست بیمه‌گذاران تکمیلی</returns>
        Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetSupplementaryInsuranceProvidersAsync();

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="code">کد ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه ارائه‌دهنده بیمه برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesCodeExistAsync(string code, int? excludeId = null);

        /// <summary>
        /// بررسی وجود نام ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="name">نام ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه ارائه‌دهنده بیمه برای حذف از بررسی</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> DoesNameExistAsync(string name, int? excludeId = null);

        #endregion
    }
}
