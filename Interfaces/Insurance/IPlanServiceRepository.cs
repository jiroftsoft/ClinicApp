using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository Interface برای مدیریت خدمات طرح‌های بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل خدمات طرح‌های بیمه (Copay, CoverageOverride)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. مدیریت ردیابی (Audit Trail)
    /// 4. بهینه‌سازی عملکرد با AsNoTracking
    /// 5. پشتیبانی از جستجو و فیلتر بر اساس طرح و دسته‌بندی خدمات
    /// 6. مدیریت روابط با InsurancePlan و ServiceCategory
    /// 7. محاسبه پوشش بیمه برای خدمات مختلف
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های PlanService
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IPlanServiceRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه خدمات طرح بیمه</param>
        /// <returns>خدمات طرح بیمه مورد نظر</returns>
        Task<PlanService> GetByIdAsync(int id);

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه خدمات طرح بیمه</param>
        /// <returns>خدمات طرح بیمه با تمام روابط</returns>
        Task<PlanService> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام خدمات طرح‌های بیمه
        /// </summary>
        /// <returns>لیست تمام خدمات طرح‌های بیمه</returns>
        Task<List<PlanService>> GetAllAsync();

        /// <summary>
        /// دریافت خدمات طرح‌های بیمه فعال
        /// </summary>
        /// <returns>لیست خدمات طرح‌های بیمه فعال</returns>
        Task<List<PlanService>> GetActiveAsync();

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس شناسه طرح
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست خدمات طرح بیمه</returns>
        Task<List<PlanService>> GetByPlanIdAsync(int planId);

        /// <summary>
        /// دریافت خدمات طرح بیمه فعال بر اساس شناسه طرح
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست خدمات طرح بیمه فعال</returns>
        Task<List<PlanService>> GetActiveByPlanIdAsync(int planId);

        /// <summary>
        /// دریافت خدمات طرح‌های بیمه بر اساس شناسه دسته‌بندی خدمات
        /// </summary>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>لیست خدمات طرح‌های بیمه</returns>
        Task<List<PlanService>> GetByServiceCategoryIdAsync(int serviceCategoryId);

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس طرح و دسته‌بندی خدمات
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>خدمات طرح بیمه مورد نظر</returns>
        Task<PlanService> GetByPlanAndCategoryAsync(int planId, int serviceCategoryId);

        /// <summary>
        /// دریافت خدمات طرح بیمه بر اساس طرح و دسته‌بندی خدمات (برای محاسبات بیمه)
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <returns>خدمات طرح بیمه مورد نظر</returns>
        Task<ServiceResult<PlanService>> GetByPlanAndServiceCategoryAsync(int planId, int serviceCategoryId);

        /// <summary>
        /// دریافت پیکربندی بیمه بر اساس ServiceId
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>پیکربندی بیمه برای خدمت</returns>
        Task<ServiceResult<PlanService>> GetByPlanAndServiceAsync(int planId, int serviceId);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود خدمات طرح بیمه برای طرح و دسته‌بندی
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات</param>
        /// <param name="excludeId">شناسه خدمات طرح بیمه برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر خدمات طرح بیمه وجود داشته باشد</returns>
        Task<bool> DoesPlanServiceExistAsync(int planId, int serviceCategoryId, int? excludeId = null);

        /// <summary>
        /// بررسی وجود خدمات طرح بیمه
        /// </summary>
        /// <param name="id">شناسه خدمات طرح بیمه</param>
        /// <returns>درست اگر خدمات طرح بیمه وجود داشته باشد</returns>
        Task<bool> DoesExistAsync(int id);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی خدمات طرح‌های بیمه بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست خدمات طرح‌های بیمه مطابق با جستجو</returns>
        Task<List<PlanService>> SearchAsync(string searchTerm);

        /// <summary>
        /// جستجوی خدمات طرح‌های بیمه فعال بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست خدمات طرح‌های بیمه فعال مطابق با جستجو</returns>
        Task<List<PlanService>> SearchActiveAsync(string searchTerm);

        /// <summary>
        /// جستجوی خدمات طرح بیمه بر اساس شناسه طرح و عبارت جستجو
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست خدمات طرح بیمه مطابق با جستجو</returns>
        Task<List<PlanService>> SearchByPlanAsync(int planId, string searchTerm);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن خدمات طرح بیمه جدید
        /// </summary>
        /// <param name="planService">خدمات طرح بیمه جدید</param>
        void Add(PlanService planService);

        /// <summary>
        /// به‌روزرسانی خدمات طرح بیمه
        /// </summary>
        /// <param name="planService">خدمات طرح بیمه برای به‌روزرسانی</param>
        void Update(PlanService planService);

        /// <summary>
        /// حذف نرم خدمات طرح بیمه
        /// </summary>
        /// <param name="planService">خدمات طرح بیمه برای حذف</param>
        void Delete(PlanService planService);

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
