using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// رابط سرویس مدیریت دسترسی‌پذیری نوبت‌ها
    /// </summary>
    public interface IAppointmentAvailabilityService
    {
        /// <summary>
        /// دریافت تاریخ‌های در دسترس برای یک پزشک
        /// </summary>
        Task<ServiceResult<List<DateTime>>> GetAvailableDatesAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت اسلات‌های زمانی در دسترس برای یک تاریخ مشخص
        /// </summary>
        Task<ServiceResult<List<TimeSlotViewModel>>> GetAvailableTimeSlotsAsync(int doctorId, DateTime date);

        /// <summary>
        /// بررسی در دسترس بودن یک اسلات
        /// </summary>
        Task<ServiceResult<bool>> IsSlotAvailableAsync(int slotId);

        /// <summary>
        /// رزرو موقت یک اسلات
        /// </summary>
        Task<ServiceResult<bool>> ReserveSlotAsync(int slotId, int patientId, TimeSpan reservationDuration);

        /// <summary>
        /// آزاد کردن یک اسلات رزرو شده
        /// </summary>
        Task<ServiceResult<bool>> ReleaseSlotAsync(int slotId);

        /// <summary>
        /// تولید اسلات‌های هفتگی برای یک پزشک
        /// </summary>
        Task<ServiceResult<bool>> GenerateWeeklySlotsAsync(int doctorId, DateTime weekStart);

        /// <summary>
        /// تولید اسلات‌های ماهانه برای یک پزشک
        /// </summary>
        Task<ServiceResult<bool>> GenerateMonthlySlotsAsync(int doctorId, DateTime monthStart);
    }
}
