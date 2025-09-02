using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin;

/// <summary>
/// اینترفیس تخصصی برای مدیریت برنامه کاری پزشکان در سیستم کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. تمرکز صرف بر مدیریت برنامه‌های کاری پزشکان
/// 2. رعایت استانداردهای پزشکی ایران در برنامه‌ریزی نوبت‌دهی
/// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
/// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
/// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
/// 
/// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
/// </summary>
public interface IDoctorScheduleService
{
    #region Scheduling & Availability (برنامه‌ریزی و زمان‌های در دسترس)

    /// <summary>
    /// تنظیم یا به‌روزرسانی برنامه کاری هفتگی یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="schedule">مدل برنامه کاری.</param>
    /// <returns>نتیجه عملیات تنظیم برنامه.</returns>
    Task<ServiceResult> SetDoctorScheduleAsync(int doctorId, DoctorScheduleViewModel schedule);

    /// <summary>
    /// دریافت برنامه کاری هفتگی یک پزشک.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <returns>نتیجه حاوی برنامه کاری پزشک.</returns>
    Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleAsync(int doctorId);

    /// <summary>
    /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه).
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="start">زمان شروع.</param>
    /// <param name="end">زمان پایان.</param>
    /// <param name="reason">دلیل مسدودیت.</param>
    /// <returns>نتیجه عملیات مسدودیت.</returns>
    Task<ServiceResult> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason);

    /// <summary>
    /// محاسبه و بازگرداندن تمام اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص.
    /// </summary>
    /// <param name="doctorId">شناسه پزشک.</param>
    /// <param name="date">تاریخ مورد نظر.</param>
    /// <returns>نتیجه حاوی اسلات‌های زمانی خالی.</returns>
    Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date);

    #endregion

    #region List and Search Operations

    /// <summary>
    /// دریافت لیست تمام برنامه‌های کاری پزشکان با صفحه‌بندی
    /// </summary>
    /// <param name="searchTerm">عبارت جستجو</param>
    /// <param name="pageNumber">شماره صفحه</param>
    /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
    /// <returns>نتیجه حاوی لیست صفحه‌بندی شده برنامه‌های کاری</returns>
    Task<ServiceResult<PagedResult<DoctorScheduleViewModel>>> GetAllDoctorSchedulesAsync(string searchTerm, int pageNumber, int pageSize);

    #endregion

    #region Schedule Management Operations

    /// <summary>
    /// دریافت برنامه کاری بر اساس شناسه
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>نتیجه حاوی برنامه کاری</returns>
    Task<ServiceResult<DoctorScheduleViewModel>> GetDoctorScheduleByIdAsync(int scheduleId);

    /// <summary>
    /// حذف برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>نتیجه عملیات حذف</returns>
    Task<ServiceResult> DeleteDoctorScheduleAsync(int scheduleId);

    /// <summary>
    /// غیرفعال کردن برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>نتیجه عملیات غیرفعال‌سازی</returns>
    Task<ServiceResult> DeactivateDoctorScheduleAsync(int scheduleId);

    /// <summary>
    /// فعال کردن مجدد برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>نتیجه عملیات فعال‌سازی</returns>
    Task<ServiceResult> ActivateDoctorScheduleAsync(int scheduleId);

    #endregion
}