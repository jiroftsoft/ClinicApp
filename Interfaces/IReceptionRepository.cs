using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Repository Interface برای مدیریت پذیرش‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پذیرش‌های بیماران (Normal, Emergency, Special, Online)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. مدیریت ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 4. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 5. پشتیبانی از جستجو و فیلتر بر اساس بیمار، پزشک، تاریخ
    /// 6. مدیریت روابط با Patient، Doctor، PatientInsurance
    /// 7. مدیریت انواع پذیرش و اولویت‌ها
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های Reception
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IReceptionRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>پذیرش مورد نظر</returns>
        Task<Models.Entities.Reception.Reception> GetByIdAsync(int id);

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>پذیرش با جزئیات کامل</returns>
        Task<Models.Entities.Reception.Reception> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام پذیرش‌های فعال
        /// </summary>
        /// <returns>لیست پذیرش‌های فعال</returns>
        Task<List<Models.Entities.Reception.Reception>> GetAllActiveAsync();

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByPatientIdAsync(int patientId);

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست پذیرش‌های پزشک</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByDoctorIdAsync(int doctorId);

        /// <summary>
        /// دریافت پذیرش‌های تاریخ مشخص
        /// </summary>
        /// <param name="date">تاریخ پذیرش</param>
        /// <returns>لیست پذیرش‌های تاریخ مشخص</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByDateAsync(DateTime date);

        /// <summary>
        /// دریافت پذیرش‌های بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>لیست پذیرش‌های بازه زمانی</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس کد ملی بیمار
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        Task<List<Models.Entities.Reception.Reception>> SearchByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس نام بیمار
        /// </summary>
        /// <param name="patientName">نام بیمار</param>
        /// <returns>لیست پذیرش‌های بیمار</returns>
        Task<List<Models.Entities.Reception.Reception>> SearchByPatientNameAsync(string patientName);

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس وضعیت
        /// </summary>
        /// <param name="status">وضعیت پذیرش</param>
        /// <returns>لیست پذیرش‌های با وضعیت مشخص</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByStatusAsync(ReceptionStatus status);

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس نوع
        /// </summary>
        /// <param name="type">نوع پذیرش</param>
        /// <returns>لیست پذیرش‌های با نوع مشخص</returns>
        Task<List<Models.Entities.Reception.Reception>> GetByTypeAsync(ReceptionType type);

        /// <summary>
        /// جستجوی پذیرش‌های اورژانس
        /// </summary>
        /// <returns>لیست پذیرش‌های اورژانس</returns>
        Task<List<Models.Entities.Reception.Reception>> GetEmergencyReceptionsAsync();

        #endregion

        #region Paged Operations

        /// <summary>
        /// دریافت پذیرش‌ها با صفحه‌بندی
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="status">وضعیت پذیرش (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده پذیرش‌ها</returns>
        Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPagedAsync(
            int? patientId, 
            int? doctorId, 
            ReceptionStatus? status, 
            string searchTerm, 
            int pageNumber, 
            int pageSize);

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن پذیرش جدید
        /// </summary>
        /// <param name="reception">پذیرش جدید</param>
        void Add(Models.Entities.Reception.Reception reception);

        /// <summary>
        /// به‌روزرسانی پذیرش
        /// </summary>
        /// <param name="reception">پذیرش برای به‌روزرسانی</param>
        void Update(Models.Entities.Reception.Reception reception);

        /// <summary>
        /// حذف نرم پذیرش
        /// </summary>
        /// <param name="reception">پذیرش برای حذف</param>
        void Delete(Models.Entities.Reception.Reception reception);

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
        /// <returns>Transaction جدید</returns>
        System.Data.Entity.DbContextTransaction BeginTransaction();

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// بررسی وجود پذیرش فعال برای بیمار در تاریخ مشخص
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="date">تاریخ</param>
        /// <returns>true اگر پذیرش فعال وجود دارد</returns>
        Task<bool> HasActiveReceptionAsync(int patientId, DateTime date);

        /// <summary>
        /// دریافت آخرین پذیرش بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>آخرین پذیرش بیمار</returns>
        Task<Models.Entities.Reception.Reception> GetLatestByPatientIdAsync(int patientId);

        /// <summary>
        /// شمارش پذیرش‌های روز
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>تعداد پذیرش‌های روز</returns>
        Task<int> GetDailyCountAsync(DateTime date);

        /// <summary>
        /// شمارش پذیرش‌های پزشک در تاریخ مشخص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>تعداد پذیرش‌های پزشک</returns>
        Task<int> GetDoctorDailyCountAsync(int doctorId, DateTime date);

        /// <summary>
        /// Get receptions by date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>List of receptions</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateAsync(DateTime date);

        /// <summary>
        /// Get receptions by doctor and date
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="date">Date</param>
        /// <returns>List of receptions</returns>
        Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorAndDateAsync(int doctorId, DateTime date);

        /// <summary>
        /// Update reception entity
        /// </summary>
        /// <param name="reception">Reception entity</param>
        /// <returns>Updated reception</returns>
        Task<Models.Entities.Reception.Reception> UpdateReceptionAsync(Models.Entities.Reception.Reception reception);

        /// <summary>
        /// Get reception payments
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <returns>List of payment transactions</returns>
        Task<List<PaymentTransaction>> GetReceptionPaymentsAsync(int receptionId);

        /// <summary>
        /// دریافت تعداد پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>تعداد پذیرش‌ها</returns>
        Task<int> GetReceptionCountByDoctorAsync(int doctorId);

        #endregion
    }
}
