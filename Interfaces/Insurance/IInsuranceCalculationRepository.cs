using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.Insurance
{
    /// <summary>
    /// Repository Interface برای مدیریت محاسبات بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل محاسبات بیمه (محاسبه سهم بیمه، سهم بیمار، فرانشیز)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 3. مدیریت ردیابی (Audit Trail)
    /// 4. بهینه‌سازی عملکرد با AsNoTracking
    /// 5. پشتیبانی از جستجو و فیلتر بر اساس بیمار، خدمت، طرح بیمه
    /// 6. مدیریت تاریخچه محاسبات برای حسابرسی
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت داده‌های InsuranceCalculation
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Repository layer فقط دسترسی به داده
    /// </summary>
    public interface IInsuranceCalculationRepository
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت محاسبه بیمه بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه محاسبه بیمه</param>
        /// <returns>محاسبه بیمه مورد نظر</returns>
        Task<InsuranceCalculation> GetByIdAsync(int id);

        /// <summary>
        /// دریافت محاسبه بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        /// <param name="id">شناسه محاسبه بیمه</param>
        /// <returns>محاسبه بیمه با تمام روابط</returns>
        Task<InsuranceCalculation> GetByIdWithDetailsAsync(int id);

        /// <summary>
        /// دریافت تمام محاسبات بیمه
        /// </summary>
        /// <returns>لیست تمام محاسبات بیمه</returns>
        Task<List<InsuranceCalculation>> GetAllAsync();

        /// <summary>
        /// دریافت محاسبات بیمه معتبر
        /// </summary>
        /// <returns>لیست محاسبات بیمه معتبر</returns>
        Task<List<InsuranceCalculation>> GetValidCalculationsAsync();

        /// <summary>
        /// افزودن محاسبه بیمه جدید
        /// </summary>
        /// <param name="calculation">محاسبه بیمه جدید</param>
        /// <returns>محاسبه بیمه افزوده شده</returns>
        Task<InsuranceCalculation> AddAsync(InsuranceCalculation calculation);

        /// <summary>
        /// به‌روزرسانی محاسبه بیمه موجود
        /// </summary>
        /// <param name="calculation">محاسبه بیمه به‌روزرسانی شده</param>
        /// <returns>محاسبه بیمه به‌روزرسانی شده</returns>
        Task<InsuranceCalculation> UpdateAsync(InsuranceCalculation calculation);

        /// <summary>
        /// حذف نرم محاسبه بیمه
        /// </summary>
        /// <param name="id">شناسه محاسبه بیمه</param>
        /// <returns>وضعیت حذف</returns>
        Task<bool> SoftDeleteAsync(int id);

        #endregion

        #region Search and Filter Operations

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست محاسبات بیمه بیمار</returns>
        Task<List<InsuranceCalculation>> GetByPatientIdAsync(int patientId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس خدمت
        /// </summary>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>لیست محاسبات بیمه خدمت</returns>
        Task<List<InsuranceCalculation>> GetByServiceIdAsync(int serviceId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس طرح بیمه
        /// </summary>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <returns>لیست محاسبات بیمه طرح</returns>
        Task<List<InsuranceCalculation>> GetByPlanIdAsync(int planId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس بیمه بیمار
        /// </summary>
        /// <param name="patientInsuranceId">شناسه بیمه بیمار</param>
        /// <returns>لیست محاسبات بیمه بیمار</returns>
        Task<List<InsuranceCalculation>> GetByPatientInsuranceIdAsync(int patientInsuranceId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس پذیرش
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>لیست محاسبات بیمه پذیرش</returns>
        Task<List<InsuranceCalculation>> GetByReceptionIdAsync(int receptionId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس قرار ملاقات
        /// </summary>
        /// <param name="appointmentId">شناسه قرار ملاقات</param>
        /// <returns>لیست محاسبات بیمه قرار ملاقات</returns>
        Task<List<InsuranceCalculation>> GetByAppointmentIdAsync(int appointmentId);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس نوع محاسبه
        /// </summary>
        /// <param name="calculationType">نوع محاسبه</param>
        /// <returns>لیست محاسبات بیمه نوع مشخص</returns>
        Task<List<InsuranceCalculation>> GetByCalculationTypeAsync(string calculationType);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس تاریخ
        /// </summary>
        /// <param name="fromDate">تاریخ شروع</param>
        /// <param name="toDate">تاریخ پایان</param>
        /// <returns>لیست محاسبات بیمه در بازه زمانی</returns>
        Task<List<InsuranceCalculation>> GetByDateRangeAsync(System.DateTime fromDate, System.DateTime toDate);

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس وضعیت اعتبار
        /// </summary>
        /// <param name="isValid">وضعیت اعتبار</param>
        /// <returns>لیست محاسبات بیمه با وضعیت مشخص</returns>
        Task<List<InsuranceCalculation>> GetByValidityAsync(bool isValid);

        #endregion

        #region Advanced Operations

        /// <summary>
        /// دریافت آمار محاسبات بیمه
        /// </summary>
        /// <returns>آمار کلی محاسبات بیمه</returns>
        Task<object> GetCalculationStatisticsAsync();

        /// <summary>
        /// دریافت محاسبات بیمه با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>محاسبات بیمه با صفحه‌بندی</returns>
        Task<(List<InsuranceCalculation> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// جستجوی پیشرفته محاسبات بیمه
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="serviceId">شناسه خدمت (اختیاری)</param>
        /// <param name="planId">شناسه طرح بیمه (اختیاری)</param>
        /// <param name="isValid">وضعیت اعتبار (اختیاری)</param>
        /// <param name="fromDate">تاریخ شروع (اختیاری)</param>
        /// <param name="toDate">تاریخ پایان (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتایج جستجو با صفحه‌بندی</returns>
        Task<(List<InsuranceCalculation> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            int? patientId = null,
            int? serviceId = null,
            int? planId = null,
            bool? isValid = null,
            System.DateTime? fromDate = null,
            System.DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// بررسی وجود محاسبه بیمه برای ترکیب مشخص
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <param name="planId">شناسه طرح بیمه</param>
        /// <param name="receptionId">شناسه پذیرش (اختیاری)</param>
        /// <returns>وضعیت وجود محاسبه</returns>
        Task<bool> ExistsAsync(int patientId, int serviceId, int planId, int? receptionId = null);

        /// <summary>
        /// دریافت آخرین محاسبه بیمه برای بیمار و خدمت
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="serviceId">شناسه خدمت</param>
        /// <returns>آخرین محاسبه بیمه</returns>
        Task<InsuranceCalculation> GetLatestByPatientAndServiceAsync(int patientId, int serviceId);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// افزودن چندین محاسبه بیمه
        /// </summary>
        /// <param name="calculations">لیست محاسبات بیمه</param>
        /// <returns>لیست محاسبات بیمه افزوده شده</returns>
        Task<List<InsuranceCalculation>> AddRangeAsync(List<InsuranceCalculation> calculations);

        /// <summary>
        /// به‌روزرسانی وضعیت اعتبار چندین محاسبه
        /// </summary>
        /// <param name="calculationIds">لیست شناسه‌های محاسبه</param>
        /// <param name="isValid">وضعیت اعتبار جدید</param>
        /// <returns>تعداد محاسبات به‌روزرسانی شده</returns>
        Task<int> UpdateValidityAsync(List<int> calculationIds, bool isValid);

        /// <summary>
        /// حذف نرم چندین محاسبه بیمه
        /// </summary>
        /// <param name="calculationIds">لیست شناسه‌های محاسبه</param>
        /// <returns>تعداد محاسبات حذف شده</returns>
        Task<int> SoftDeleteRangeAsync(List<int> calculationIds);

        #endregion
    }
}
