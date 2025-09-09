using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.PatientInsurance;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository Interface برای مدیریت بیمه‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل بیمه‌های بیماران (Primary و Supplementary)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. مدیریت ردیابی (Audit Trail)
    /// 4. بهینه‌سازی عملکرد با AsNoTracking
    /// 5. پشتیبانی از جستجو و فیلتر بر اساس بیمار
    /// 6. مدیریت روابط با Patient و InsurancePlan
    /// 7. مدیریت بیمه اصلی و تکمیلی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های PatientInsurance
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IPatientInsuranceRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه بیمه بیمار</param>
        /// <returns>بیمه بیمار مورد نظر</returns>
        Task<PatientInsurance> GetByIdAsync(int id);

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه بیمه بیمار</param>
        /// <returns>بیمه بیمار با تمام روابط</returns>
        Task<PatientInsurance> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام بیمه‌های بیماران
        /// </summary>
        /// <returns>لیست تمام بیمه‌های بیماران</returns>
        Task<List<PatientInsurance>> GetAllAsync();

        /// <summary>
        /// دریافت بیمه‌های بیماران فعال
        /// </summary>
        /// <returns>لیست بیمه‌های بیماران فعال</returns>
        Task<List<PatientInsurance>> GetActiveAsync();

        /// <summary>
        /// دریافت بیمه‌های بیمار بر اساس شناسه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        Task<List<PatientInsurance>> GetByPatientIdAsync(int patientId);

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار بر اساس شناسه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های فعال بیمار</returns>
        Task<List<PatientInsurance>> GetActiveByPatientIdAsync(int patientId);

        /// <summary>
        /// دریافت بیمه اصلی بیمار بر اساس شناسه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>بیمه اصلی بیمار</returns>
        Task<PatientInsurance> GetPrimaryByPatientIdAsync(int patientId);

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بیمار بر اساس شناسه بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های تکمیلی بیمار</returns>
        Task<List<PatientInsurance>> GetSupplementaryByPatientIdAsync(int patientId);

        /// <summary>
        /// دریافت بیمه‌های بیماران بر اساس شناسه طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست بیمه‌های بیماران</returns>
        Task<List<PatientInsurance>> GetByPlanIdAsync(int planId);

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شماره بیمه
        /// </summary>
        /// <param name="policyNumber">شماره بیمه</param>
        /// <returns>بیمه بیمار مورد نظر</returns>
        Task<PatientInsurance> GetByPolicyNumberAsync(string policyNumber);

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود شماره بیمه
        /// </summary>
        /// <param name="policyNumber">شماره بیمه</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر شماره بیمه وجود داشته باشد</returns>
        Task<bool> DoesPolicyNumberExistAsync(string policyNumber, int? excludeId = null);

        /// <summary>
        /// بررسی وجود بیمه اصلی برای بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر بیمه اصلی وجود داشته باشد</returns>
        Task<bool> DoesPrimaryInsuranceExistAsync(int patientId, int? excludeId = null);

        /// <summary>
        /// بررسی وجود بیمه بیمار
        /// </summary>
        /// <param name="id">شناسه بیمه بیمار</param>
        /// <returns>درست اگر بیمه بیمار وجود داشته باشد</returns>
        Task<bool> DoesExistAsync(int id);

        /// <summary>
        /// بررسی تداخل تاریخ بیمه‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="excludeId">شناسه بیمه بیمار برای حذف از بررسی (در ویرایش)</param>
        /// <returns>درست اگر تداخل وجود داشته باشد</returns>
        Task<bool> DoesDateOverlapExistAsync(int patientId, DateTime startDate, DateTime endDate, int? excludeId = null);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی بیمه‌های بیماران بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست بیمه‌های بیماران مطابق با جستجو</returns>
        Task<List<PatientInsurance>> SearchAsync(string searchTerm);

        /// <summary>
        /// جستجوی بیمه‌های بیماران فعال بر اساس عبارت جستجو
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست بیمه‌های بیماران فعال مطابق با جستجو</returns>
        Task<List<PatientInsurance>> SearchActiveAsync(string searchTerm);

        /// <summary>
        /// جستجوی بیمه‌های بیمار بر اساس شناسه بیمار و عبارت جستجو
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست بیمه‌های بیمار مطابق با جستجو</returns>
        Task<List<PatientInsurance>> SearchByPatientAsync(int patientId, string searchTerm);

        #endregion

        #region Active Insurance Operations

        /// <summary>
        /// دریافت بیمه فعال بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>بیمه فعال بیمار</returns>
        Task<ServiceResult<PatientInsurance>> GetActiveByPatientAsync(int patientId);

        #endregion

        #region Paged Operations

        /// <summary>
        /// دریافت بیمه‌های بیماران با صفحه‌بندی
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده بیمه‌های بیماران</returns>
        Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedAsync(int? patientId, string searchTerm, int pageNumber, int pageSize);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن بیمه بیمار جدید
        /// </summary>
        /// <param name="patientInsurance">بیمه بیمار جدید</param>
        void Add(PatientInsurance patientInsurance);

        /// <summary>
        /// به‌روزرسانی بیمه بیمار
        /// </summary>
        /// <param name="patientInsurance">بیمه بیمار برای به‌روزرسانی</param>
        void Update(PatientInsurance patientInsurance);

        /// <summary>
        /// حذف نرم بیمه بیمار
        /// </summary>
        /// <param name="patientInsurance">بیمه بیمار برای حذف</param>
        void Delete(PatientInsurance patientInsurance);

        #endregion

        #region Database Operations

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        /// <returns>تعداد رکوردهای تأثیرپذیرفته</returns>
        Task<int> SaveChangesAsync();

        #endregion

        #region Transaction Management

        /// <summary>
        /// شروع Transaction جدید
        /// </summary>
        /// <returns>Transaction object</returns>
        Task<System.Data.Entity.DbContextTransaction> BeginTransactionAsync();

        #endregion
    }
}
