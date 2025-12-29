using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// اینترفیس تخصصی برای مدیریت اسلات‌های زمانی پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل اسلات‌های زمانی (مشاهده، فیلتر، جستجو)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت نوبت‌دهی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از وضعیت‌های مختلف نوبت (در دسترس، رزرو شده، تکمیل شده)
    /// 
    /// نکته حیاتی: این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده است
    /// </summary>
    public interface IDoctorTimeSlotRepository
    {
        #region Query Operations (عملیات جستجو)

        /// <summary>
        /// دریافت اسلات‌های زمانی با فیلتر و صفحه‌بندی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک (اختیاری - اگر null باشد، همه پزشکان)</param>
        /// <param name="startDate">تاریخ شروع (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان (اختیاری)</param>
        /// <param name="status">وضعیت اسلات (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <returns>لیست اسلات‌های زمانی صفحه‌بندی شده</returns>
        Task<(List<DoctorTimeSlot> Items, int TotalCount)> GetTimeSlotsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppointmentStatus? status = null,
            int pageNumber = 1,
            int pageSize = 20,
            string searchTerm = null);

        /// <summary>
        /// دریافت اسلات زمانی بر اساس شناسه
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <returns>اسلات زمانی یا null</returns>
        Task<DoctorTimeSlot> GetTimeSlotByIdAsync(int timeSlotId);

        /// <summary>
        /// دریافت اسلات‌های زمانی یک پزشک در یک تاریخ خاص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>لیست اسلات‌های زمانی</returns>
        Task<List<DoctorTimeSlot>> GetTimeSlotsByDoctorAndDateAsync(int doctorId, DateTime date);

        /// <summary>
        /// دریافت آمار اسلات‌های زمانی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="startDate">تاریخ شروع (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان (اختیاری)</param>
        /// <returns>آمار اسلات‌های زمانی</returns>
        Task<TimeSlotStatistics> GetTimeSlotStatisticsAsync(
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        #endregion

        #region Management Operations (عملیات مدیریت)

        /// <summary>
        /// حذف نرم اسلات زمانی
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف کننده</param>
        /// <returns>درست اگر حذف با موفقیت انجام شد</returns>
        Task<bool> SoftDeleteTimeSlotAsync(int timeSlotId, string deletedByUserId);

        /// <summary>
        /// تغییر وضعیت اسلات زمانی
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <param name="status">وضعیت جدید</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>درست اگر تغییر وضعیت با موفقیت انجام شد</returns>
        Task<bool> UpdateTimeSlotStatusAsync(int timeSlotId, AppointmentStatus status, string updatedByUserId);

        /// <summary>
        /// آزاد کردن اسلات رزرو شده (برای لغو نوبت)
        /// </summary>
        /// <param name="timeSlotId">شناسه اسلات زمانی</param>
        /// <param name="updatedByUserId">شناسه کاربر به‌روزرسانی کننده</param>
        /// <returns>درست اگر آزادسازی با موفقیت انجام شد</returns>
        Task<bool> ReleaseTimeSlotAsync(int timeSlotId, string updatedByUserId);

        #endregion
    }

    /// <summary>
    /// آمار اسلات‌های زمانی
    /// </summary>
    public class TimeSlotStatistics
    {
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int BookedSlots { get; set; }
        public int CompletedSlots { get; set; }
        public int CancelledSlots { get; set; }
        public int NoShowSlots { get; set; }
        public int DeletedSlots { get; set; }
    }
}

