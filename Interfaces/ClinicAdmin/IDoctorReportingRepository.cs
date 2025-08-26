using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities;

namespace ClinicApp.Interfaces.ClinicAdmin;

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
public interface IDoctorReportingRepository
{
    #region Reporting & Statistics (گزارش‌گیری و آمار)

    /// <summary>
    /// دریافت گزارش پزشکان فعال در یک بازه زمانی
    /// </summary>
    /// <param name="clinicId">شناسه کلینیک</param>
    /// <param name="departmentId">شناسه دپارتمان (اختیاری)</param>
    /// <param name="startDate">تاریخ شروع</param>
    /// <param name="endDate">تاریخ پایان</param>
    /// <returns>لیست پزشکان فعال همراه با آمار</returns>
    Task<List<Doctor>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// دریافت داده‌های داشبورد پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>داده‌های داشبورد پزشک</returns>
    Task<Doctor> GetDoctorDashboardDataAsync(int doctorId);

    #endregion

    #region Dependency Management (مدیریت وابستگی‌ها)

    /// <summary>
    /// دریافت اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>اطلاعات وابستگی‌های پزشک</returns>
    Task<DoctorDependencyInfo> GetDoctorDependencyInfoAsync(int doctorId);

    /// <summary>
    /// بررسی امکان حذف پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>درست اگر پزشک قابل حذف باشد</returns>
    Task<bool> CanDeleteDoctorAsync(int doctorId);

    #endregion
}