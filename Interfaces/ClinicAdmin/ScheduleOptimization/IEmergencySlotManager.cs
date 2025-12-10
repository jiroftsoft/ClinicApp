using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای مدیریت زمان‌های اورژانس
    /// 
    /// مسئولیت (SRP):
    /// - مدیریت اسلات‌های اورژانس
    /// - بهینه‌سازی زمان‌های اورژانس
    /// - رزرو و آزادسازی اسلات‌های اورژانس
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط مدیریت اورژانس
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface IEmergencySlotManager
    {
        /// <summary>
        /// بهینه‌سازی زمان‌های اورژانس برای یک روز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>لیست زمان‌های اورژانس بهینه شده</returns>
        Task<ServiceResult<List<EmergencyTimeSlot>>> OptimizeEmergencyTimesAsync(int doctorId, DateTime date);

        /// <summary>
        /// محاسبه تعداد اسلات‌های اورژانس مورد نیاز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <param name="totalSlots">تعداد کل اسلات‌ها</param>
        /// <returns>تعداد اسلات‌های اورژانس پیشنهادی</returns>
        int CalculateRequiredEmergencySlots(int doctorId, DateTime date, int totalSlots);

        /// <summary>
        /// بررسی در دسترس بودن اسلات اورژانس
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <param name="time">زمان مورد نظر</param>
        /// <returns>true اگر اسلات در دسترس است</returns>
        Task<ServiceResult<bool>> IsEmergencySlotAvailableAsync(int doctorId, DateTime date, TimeSpan time);

        /// <summary>
        /// رزرو اسلات اورژانس
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <param name="priority">اولویت</param>
        /// <returns>نتیجه رزرو</returns>
        Task<ServiceResult<EmergencyTimeSlot>> ReserveEmergencySlotAsync(
            int doctorId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            EmergencyPriority priority);
    }
}

