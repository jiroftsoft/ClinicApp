using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس Repository برای داشبورد پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تعریف قراردادهای دسترسی به داده‌های داشبورد
    /// 2. رعایت استانداردهای پزشکی ایران در دسترسی به داده
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای داده‌ای
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorDashboardRepository
    {
        #region Dashboard Data (داده‌های داشبورد)

        /// <summary>
        /// دریافت داده‌های داشبورد اصلی
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <returns>داده‌های داشبورد اصلی</returns>
        Task<DoctorDashboardIndexViewModel> GetDashboardDataAsync(int? clinicId = null, int? departmentId = null);

        /// <summary>
        /// دریافت جزئیات کامل پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>جزئیات کامل پزشک</returns>
        Task<DoctorDetailsViewModel> GetDoctorDetailsAsync(int doctorId);

        /// <summary>
        /// دریافت لیست انتسابات پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست انتسابات پزشک</returns>
        Task<DoctorAssignmentsViewModel> GetDoctorAssignmentsAsync(int doctorId);

        #endregion

        #region Search & Filter (جستجو و فیلتر)

        /// <summary>
        /// جستجوی پزشکان
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <param name="specializationId">شناسه تخصص (اختیاری)</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه جستجوی پزشکان</returns>
        Task<DoctorSearchResultViewModel> SearchDoctorsAsync(string searchTerm = null, int? clinicId = null, int? departmentId = null, int? specializationId = null, int page = 1, int pageSize = 20);

        #endregion

        #region Statistics & Analytics (آمار و تحلیل)

        /// <summary>
        /// دریافت آمار کلی داشبورد
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <returns>آمار کلی داشبورد</returns>
        Task<DashboardStatsViewModel> GetDashboardStatsAsync(int? clinicId = null);

        /// <summary>
        /// دریافت آمار پزشکان فعال
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک (اختیاری)</param>
        /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
        /// <returns>آمار پزشکان فعال</returns>
        Task<ActiveDoctorsStatsViewModel> GetActiveDoctorsStatsAsync(int? clinicId = null, int? departmentId = null);

        #endregion
    }
}
