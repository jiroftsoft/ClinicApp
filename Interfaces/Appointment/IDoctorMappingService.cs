using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.Interfaces.Appointment
{
    /// <summary>
    /// سرویس Mapping برای تبدیل Entity به DTO
    /// طبق appointment_controller_review.md - فاز 1
    /// رفع نقض SRP: جابجایی Business Logic از Controller به Service
    /// </summary>
    public interface IDoctorMappingService
    {
        /// <summary>
        /// تبدیل DoctorSchedule Entity به DTO
        /// </summary>
        /// <param name="schedule">DoctorSchedule Entity</param>
        /// <returns>DoctorScheduleDisplayDto</returns>
        DoctorScheduleDisplayDto MapToScheduleDisplayDto(DoctorSchedule schedule);
    }
}

