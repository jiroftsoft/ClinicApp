using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository Interface برای مدیریت طرح‌های بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل طرح‌های بیمه (Basic, Standard, Premium, Supplementary)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. مدیریت ردیابی (Audit Trail)
    /// 4. بهینه‌سازی عملکرد با AsNoTracking
    /// 5. پشتیبانی از جستجو و فیلتر بر اساس ارائه‌دهنده
    /// 6. مدیریت روابط با InsuranceProvider و PlanService
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های InsurancePlan
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IInsurancePlanRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت طرح بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه طرح بیمه</param>
        /// <returns>طرح بیمه مورد نظر</returns>
        Task<InsurancePlan> GetByIdAsync(int id);

        /// <summary>
        /// دریافت طرح بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه طرح بیمه</param>
        /// <returns>طرح بیمه با تمام روابط</returns>
        Task<InsurancePlan> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام طرح‌های بیمه
        /// </summary>
        /// <returns>لیست تمام طرح‌های بیمه</returns>
        Task<List<InsurancePlan>> GetAllAsync();

        /// <summary>
        /// دریافت طرح‌های بیمه فعال
        /// </summary>
        /// <returns>لیست طرح‌های بیمه فعال</returns>
        Task<List<InsurancePlan>> GetActiveAsync();

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>لیست طرح‌های بیمه ارائه‌دهنده</returns>
        Task<List<InsurancePlan>> GetByProviderIdAsync(int providerId);

        /// <summary>
        /// دریافت طرح‌های بیمه فعال بر اساس ارائه‌دهنده
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>لیست طرح‌های بیمه فعال ارائه‌دهنده</returns>
        Task<List<InsurancePlan>> GetActiveByProviderIdAsync(int providerId);

        /// <summary>
        /// دریافت طرح بیمه بر اساس کد طرح
        /// </summary>
        /// <param name="planCode">کد طرح بیمه</param>
        /// <returns>طرح بیمه مورد نظر</returns>
        Task<InsurancePlan> GetByPlanCodeAsync(string planCode);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد طرح بیمه
        /// </summary>
        /// <param name="planCode">کد طرح بیمه</param>
        /// <param name="excludeId">شناسه طرح بیمه برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر کد وجود داشته باشد</returns>
        Task<bool> DoesPlanCodeExistAsync(string planCode, int? excludeId = null);

        /// <summary>
        /// بررسی وجود نام طرح بیمه در ارائه‌دهنده
        /// </summary>
        /// <param name="name">نام طرح بیمه</param>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <param name="excludeId">شناسه طرح بیمه برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر نام وجود داشته باشد</returns>
        Task<bool> DoesNameExistInProviderAsync(string name, int providerId, int? excludeId = null);

        /// <summary>
        /// بررسی وجود طرح بیمه
        /// </summary>
        /// <param name="id">شناسه طرح بیمه</param>
        /// <returns>درست اگر طرح بیمه وجود داشته باشد</returns>
        Task<bool> DoesExistAsync(int id);

        /// <summary>
        /// بررسی وجود طرح‌های بیمه پایه برای ارائه‌دهنده
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <returns>درست اگر ارائه‌دهنده طرح‌های بیمه پایه داشته باشد</returns>
        Task<bool> HasPrimaryPlansAsync(int providerId);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی طرح‌های بیمه بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست طرح‌های بیمه مطابق با جستجو</returns>
        Task<List<InsurancePlan>> SearchAsync(string searchTerm);

        /// <summary>
        /// جستجوی طرح‌های بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست طرح‌های بیمه فعال مطابق با جستجو</returns>
        Task<List<InsurancePlan>> SearchActiveAsync(string searchTerm);

        /// <summary>
        /// جستجوی طرح‌های بیمه بر اساس ارائه‌دهنده و عبارت جستجو
        /// </summary>
        /// <param name="providerId">شناسه ارائه‌دهنده بیمه</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست طرح‌های بیمه مطابق با جستجو</returns>
        Task<List<InsurancePlan>> SearchByProviderAsync(int providerId, string searchTerm);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن طرح بیمه جدید
        /// </summary>
        /// <param name="plan">طرح بیمه جدید</param>
        void Add(InsurancePlan plan);

        /// <summary>
        /// به‌روزرسانی طرح بیمه
        /// </summary>
        /// <param name="plan">طرح بیمه برای به‌روزرسانی</param>
        void Update(InsurancePlan plan);

        /// <summary>
        /// حذف نرم طرح بیمه
        /// </summary>
        /// <param name="plan">طرح بیمه برای حذف</param>
        void Delete(InsurancePlan plan);

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
