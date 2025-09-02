using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.Interfaces.ClinicAdmin
{
    /// <summary>
    /// رابط سرویس مدیریت رزروهای اورژانس
    /// </summary>
    public interface IEmergencyBookingService
    {
        /// <summary>
        /// بررسی امکان رزرو اورژانس
        /// </summary>
        Task<ServiceResult<bool>> CanBookEmergencyAsync(int doctorId, DateTime date, TimeSpan time, EmergencyPriority priority);

        /// <summary>
        /// رزرو نوبت اورژانس
        /// </summary>
        Task<ServiceResult<EmergencyBookingResult>> BookEmergencyAsync(EmergencyBookingRequest request);

        /// <summary>
        /// لغو نوبت اورژانس
        /// </summary>
        Task<ServiceResult<bool>> CancelEmergencyAsync(int emergencyBookingId, string cancellationReason);

        /// <summary>
        /// دریافت لیست نوبت‌های اورژانس
        /// </summary>
        Task<ServiceResult<List<EmergencyBooking>>> GetEmergencyBookingsAsync(int doctorId, DateTime? date = null, EmergencyPriority? priority = null);

        /// <summary>
        /// دریافت آمار نوبت‌های اورژانس
        /// </summary>
        Task<ServiceResult<EmergencyBookingStatistics>> GetEmergencyStatisticsAsync(int doctorId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// دریافت اولویت‌های اورژانس
        /// </summary>
        Task<ServiceResult<List<EmergencyPriority>>> GetEmergencyPrioritiesAsync();

        /// <summary>
        /// تنظیم اولویت اورژانس
        /// </summary>
        Task<ServiceResult<bool>> SetEmergencyPriorityAsync(int emergencyBookingId, EmergencyPriority priority);

        /// <summary>
        /// ارسال اعلان اورژانس
        /// </summary>
        Task<ServiceResult<bool>> SendEmergencyNotificationAsync(int emergencyBookingId, NotificationChannelType channel);

        /// <summary>
        /// بررسی تعارضات اورژانس
        /// </summary>
        Task<ServiceResult<List<EmergencyConflict>>> CheckEmergencyConflictsAsync(int doctorId, DateTime date, TimeSpan time);

        /// <summary>
        /// حل تعارضات اورژانس
        /// </summary>
        Task<ServiceResult<bool>> ResolveEmergencyConflictsAsync(int doctorId, DateTime date, List<EmergencyConflict> conflicts);

        /// <summary>
        /// دریافت گزارش اورژانس
        /// </summary>
        Task<ServiceResult<EmergencyReport>> GetEmergencyReportAsync(int doctorId, DateTime startDate, DateTime endDate);
    }
}
