using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.DoctorManagementVM;
using System;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس تخصصی برای گزارش‌گیری پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تمرکز صرف بر گزارش‌گیری و آمار پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در گزارش‌گیری
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای گزارش‌گیری
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorReportingService
    {
        #region Specialized Queries (پرس‌وجوهای تخصصی)

        /// <summary>
        /// بررسی دسترسی پزشک به یک دسته‌بندی خدمات خاص.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمات.</param>
        /// <returns>نتیجه حاوی اطلاعات دسترسی پزشک به دسته‌بندی خدمات.</returns>
        Task<ServiceResult<bool>> HasAccessToServiceCategoryAsync(int doctorId, int serviceCategoryId);

        /// <summary>
        /// بررسی دسترسی پزشک به یک خدمت خاص.
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <param name="serviceId">شناسه خدمت.</param>
        /// <returns>نتیجه حاوی اطلاعات دسترسی پزشک به خدمت.</returns>
        Task<ServiceResult<bool>> HasAccessToServiceAsync(int doctorId, int serviceId);

        /// <summary>
        /// دریافت لیست پزشکان فعال در یک بازه زمانی برای گزارش‌گیری.
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک.</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری).</param>
        /// <param name="startDate">تاریخ شروع.</param>
        /// <param name="endDate">تاریخ پایان.</param>
        /// <returns>نتیجه حاوی اطلاعات گزارش پزشکان فعال.</returns>
        Task<ServiceResult<ActiveDoctorsReportViewModel>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate);

        #endregion

        #region Data & Reporting (داده و گزارش‌گیری)

        /// <summary>
        /// دریافت داده‌های کلیدی برای داشبورد یک پزشک (مانند تعداد نوبت‌های امروز).
        /// </summary>
        /// <param name="doctorId">شناسه پزشک.</param>
        /// <returns>نتیجه حاوی داده‌های داشبورد پزشک.</returns>
        Task<ServiceResult<DoctorDashboardViewModel>> GetDoctorDashboardDataAsync(int doctorId);

        #endregion
    }
}