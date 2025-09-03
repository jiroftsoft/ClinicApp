using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;

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
public interface IDoctorScheduleRepository
{
    #region Schedule Management (مدیریت برنامه کاری)

    /// <summary>
    /// دریافت برنامه کاری پزشک
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>برنامه کاری پزشک</returns>
    Task<DoctorSchedule> GetDoctorScheduleAsync(int doctorId);

    /// <summary>
    /// دریافت برنامه کاری پزشک همراه با جزئیات
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <returns>برنامه کاری پزشک همراه با جزئیات</returns>
    Task<DoctorSchedule> GetDoctorScheduleWithDetailsAsync(int doctorId);

    /// <summary>
    /// دریافت برنامه کاری پزشک همراه با جزئیات کامل (شامل داده‌های غیرفعال)
    /// این متد فیلترهای سراسری را دور می‌زند تا تمام داده‌ها را دریافت کند
    /// </summary>
    Task<DoctorSchedule> GetDoctorScheduleWithAllDetailsAsync(int doctorId);

    /// <summary>
    /// افزودن برنامه کاری جدید برای پزشک
    /// </summary>
    /// <param name="schedule">برنامه کاری جدید</param>
    /// <returns>برنامه کاری افزوده شده</returns>
    Task<DoctorSchedule> AddDoctorScheduleAsync(DoctorSchedule schedule);

    /// <summary>
    /// به‌روزرسانی برنامه کاری پزشک
    /// </summary>
    /// <param name="schedule">برنامه کاری به‌روز شده</param>
    /// <returns>برنامه کاری به‌روز شده</returns>
    Task<DoctorSchedule> UpdateDoctorScheduleAsync(DoctorSchedule schedule);

    /// <summary>
    /// دریافت اسلات‌های زمانی خالی و قابل رزرو برای یک پزشک در یک روز مشخص
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="date">تاریخ مورد نظر</param>
    /// <returns>لیست اسلات‌های زمانی خالی</returns>
    Task<List<DoctorTimeSlot>> GetAvailableAppointmentSlotsAsync(int doctorId, DateTime date);

    /// <summary>
    /// مسدود کردن یک بازه زمانی برای پزشک (مثلا برای مرخصی یا جلسه)
    /// </summary>
    /// <param name="doctorId">شناسه پزشک</param>
    /// <param name="start">زمان شروع</param>
    /// <param name="end">زمان پایان</param>
    /// <param name="reason">دلیل مسدودیت</param>
    /// <returns>درست اگر مسدودیت با موفقیت انجام شد</returns>
    Task<bool> BlockTimeRangeForDoctorAsync(int doctorId, DateTime start, DateTime end, string reason);

    #endregion

    #region List and Search Operations

    /// <summary>
    /// دریافت تمام برنامه‌های کاری پزشکان
    /// </summary>
    /// <returns>لیست تمام برنامه‌های کاری</returns>
    Task<List<DoctorSchedule>> GetAllDoctorSchedulesAsync();

    #endregion

    #region Schedule CRUD Operations

    /// <summary>
    /// دریافت برنامه کاری بر اساس شناسه
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>برنامه کاری</returns>
    Task<DoctorSchedule> GetDoctorScheduleByIdAsync(int scheduleId);

    /// <summary>
    /// حذف برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>درست اگر حذف با موفقیت انجام شد</returns>
    Task<bool> DeleteDoctorScheduleAsync(int scheduleId);

    /// <summary>
    /// غیرفعال کردن برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>درست اگر غیرفعال‌سازی با موفقیت انجام شد</returns>
    Task<bool> DeactivateDoctorScheduleAsync(int scheduleId);

    /// <summary>
    /// فعال کردن مجدد برنامه کاری
    /// </summary>
    /// <param name="scheduleId">شناسه برنامه کاری</param>
    /// <returns>درست اگر فعال‌سازی با موفقیت انجام شد</returns>
    Task<bool> ActivateDoctorScheduleAsync(int scheduleId);

    #endregion
}