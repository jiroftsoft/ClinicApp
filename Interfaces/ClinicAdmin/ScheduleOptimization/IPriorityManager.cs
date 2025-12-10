using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization
{
    /// <summary>
    /// Interface برای مدیریت اولویت‌های نوبت‌ها
    /// 
    /// مسئولیت (SRP):
    /// - اولویت‌بندی نوبت‌ها
    /// - مدیریت نوبت‌های اورژانس
    /// - بهینه‌سازی ترتیب نوبت‌ها
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط مدیریت اولویت‌ها
    /// - Interface Segregation: Interface کوچک و متمرکز
    /// - Dependency Inversion: وابستگی به abstraction
    /// </summary>
    public interface IPriorityManager
    {
        /// <summary>
        /// بهینه‌سازی اولویت‌های نوبت‌ها برای یک روز
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>نتیجه بهینه‌سازی اولویت‌ها</returns>
        Task<ServiceResult<bool>> OptimizeAppointmentPrioritiesAsync(int doctorId, DateTime date);

        /// <summary>
        /// محاسبه اولویت یک نوبت
        /// </summary>
        /// <param name="appointment">نوبت مورد نظر</param>
        /// <returns>امتیاز اولویت (هرچه بالاتر، مهم‌تر)</returns>
        int CalculatePriority(AppointmentEntity appointment);

        /// <summary>
        /// مرتب‌سازی نوبت‌ها بر اساس اولویت
        /// </summary>
        /// <param name="appointments">لیست نوبت‌ها</param>
        /// <returns>لیست نوبت‌های مرتب شده</returns>
        List<AppointmentEntity> SortByPriority(List<AppointmentEntity> appointments);

        /// <summary>
        /// بررسی امکان جابجایی نوبت‌ها برای بهینه‌سازی
        /// </summary>
        /// <param name="appointments">لیست نوبت‌ها</param>
        /// <returns>لیست پیشنهادات جابجایی</returns>
        List<PriorityReorderingSuggestion> SuggestReordering(List<AppointmentEntity> appointments);
    }

    /// <summary>
    /// پیشنهاد جابجایی اولویت
    /// </summary>
    public class PriorityReorderingSuggestion
    {
        /// <summary>
        /// شناسه نوبت اول
        /// </summary>
        public int AppointmentId1 { get; set; }

        /// <summary>
        /// شناسه نوبت دوم
        /// </summary>
        public int AppointmentId2 { get; set; }

        /// <summary>
        /// دلیل پیشنهاد
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// امتیاز بهبود (هرچه بالاتر، بهتر)
        /// </summary>
        public int ImprovementScore { get; set; }
    }
}

