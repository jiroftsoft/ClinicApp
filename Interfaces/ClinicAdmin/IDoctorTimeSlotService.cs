using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Admin.TimeSlotManagement;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس تخصصی برای مدیریت اسلات‌های زمانی پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل اسلات‌های زمانی (مشاهده، فیلتر، جستجو، مدیریت)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت نوبت‌دهی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از وضعیت‌های مختلف نوبت (در دسترس، رزرو شده، تکمیل شده)
    /// 
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorTimeSlotService
    {
        #region Query Operations (عملیات جستجو)

        /// <summary>
        /// دریافت اسلات‌های زمانی با فیلتر و صفحه‌بندی
        /// </summary>
        /// <param name="filter">فیلتر جستجو</param>
        /// <returns>نتیجه حاوی لیست صفحه‌بندی شده اسلات‌های زمانی</returns>
        Task<ServiceResult<PagedResult<TimeSlotIndexViewModel>>> GetTimeSlotsAsync(TimeSlotFilterViewModel filter);

        /// <summary>
        /// دریافت اسلات زمانی بر اساس شناسه
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <returns>نتیجه حاوی اسلات زمانی</returns>
        Task<ServiceResult<TimeSlotDetailsViewModel>> GetTimeSlotByIdAsync(int timeSlotId);

        /// <summary>
        /// دریافت اسلات‌های زمانی یک پزشک در یک تاریخ خاص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>نتیجه حاوی لیست اسلات‌های زمانی</returns>
        Task<ServiceResult<List<TimeSlotIndexViewModel>>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date);

        /// <summary>
        /// دریافت آمار اسلات‌های زمانی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="startDate">تاریخ شروع (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان (اختیاری)</param>
        /// <returns>نتیجه حاوی آمار اسلات‌های زمانی</returns>
        Task<ServiceResult<TimeSlotStatisticsViewModel>> GetTimeSlotStatisticsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        #endregion

        #region Management Operations (عملیات مدیریت)

        /// <summary>
        /// حذف نرم اسلات زمانی
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <returns>نتیجه عملیات حذف</returns>
        Task<ServiceResult> SoftDeleteTimeSlotAsync(int timeSlotId);

        /// <summary>
        /// تغییر وضعیت اسلات زمانی
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <param name="status">وضعیت جدید</param>
        /// <returns>نتیجه عملیات تغییر وضعیت</returns>
        Task<ServiceResult> UpdateTimeSlotStatusAsync(int timeSlotId, AppointmentStatus status);

        /// <summary>
        /// آزاد کردن اسلات رزرو شده (برای لغو نوبت)
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <returns>نتیجه عملیات آزادسازی</returns>
        Task<ServiceResult> ReleaseTimeSlotAsync(int timeSlotId);

        #endregion
    }
}

