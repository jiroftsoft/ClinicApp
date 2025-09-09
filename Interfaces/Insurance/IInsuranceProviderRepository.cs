using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository Interface برای مدیریت ارائه‌دهندگان بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ارائه‌دهندگان بیمه (SSO, FREE, MILITARY, HEALTH, SUPPLEMENTARY)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. مدیریت ردیابی (Audit Trail)
    /// 4. بهینه‌سازی عملکرد با AsNoTracking
    /// 5. پشتیبانی از جستجو و فیلتر
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های InsuranceProvider
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IInsuranceProviderRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>ارائه‌دهنده بیمه مورد نظر</returns>
        Task<InsuranceProvider> GetByIdAsync(int id);

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>ارائه‌دهنده بیمه با تمام روابط</returns>
        Task<InsuranceProvider> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام ارائه‌دهندگان بیمه
        /// </summary>
        /// <returns>لیست تمام ارائه‌دهندگان بیمه</returns>
        Task<List<InsuranceProvider>> GetAllAsync();

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه فعال
        /// </summary>
        /// <returns>لیست ارائه‌دهندگان بیمه فعال</returns>
        Task<List<InsuranceProvider>> GetActiveAsync();

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه بر اساس کد
        /// </summary>
        /// <param name="code">کد ارائه‌دهنده بیمه</param>
        /// <returns>ارائه‌دهنده بیمه مورد نظر</returns>
        Task<InsuranceProvider> GetByCodeAsync(string code);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="code">کد ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه ارائه‌دهنده بیمه برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر کد وجود داشته باشد</returns>
        Task<bool> DoesCodeExistAsync(string code, int? excludeId = null);

        /// <summary>
        /// بررسی وجود نام ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="name">نام ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه ارائه‌دهنده بیمه برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر نام وجود داشته باشد</returns>
        Task<bool> DoesNameExistAsync(string name, int? excludeId = null);

        /// <summary>
        /// بررسی وجود ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="id">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>درست اگر ارائه‌دهنده بیمه وجود داشته باشد</returns>
        Task<bool> DoesExistAsync(int id);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی ارائه‌دهندگان بیمه بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست ارائه‌دهندگان بیمه مطابق با جستجو</returns>
        Task<List<InsuranceProvider>> SearchAsync(string searchTerm);

        /// <summary>
        /// جستجوی ارائه‌دهندگان بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست ارائه‌دهندگان بیمه فعال مطابق با جستجو</returns>
        Task<List<InsuranceProvider>> SearchActiveAsync(string searchTerm);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن ارائه‌دهنده بیمه جدید
        /// </summary>
        /// <param name="provider">ارائه‌دهنده بیمه جدید</param>
        void Add(InsuranceProvider provider);

        /// <summary>
        /// به‌روزرسانی ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="provider">ارائه‌دهنده بیمه برای به‌روزرسانی</param>
        void Update(InsuranceProvider provider);

        /// <summary>
        /// حذف نرم ارائه‌دهنده بیمه
        /// </summary>
        /// <param name="provider">ارائه‌دهنده بیمه برای حذف</param>
        void Delete(InsuranceProvider provider);

        #endregion

        #region Database Operations

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        /// <returns>تعداد رکوردهای تأثیرپذیرفته</returns>
        Task<int> SaveChangesAsync();

        #endregion
    }
}
