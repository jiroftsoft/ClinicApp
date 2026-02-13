using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;
using ClinicApp.Models.DTOs.Appointment;

namespace ClinicApp.Interfaces.Appointment
{
    /// <summary>
    /// Repository Interface برای مدیریت نوبت‌های پزشکی
    /// </summary>
    public interface IAppointmentRepository
    {
        /// <summary>
        /// آمار شمارش نوبت‌های یک بیمار (فقط COUNT، بدون بارگذاری موجودیت) — Real-Time، بدون کش.
        /// </summary>
        Task<PatientAppointmentCountsDto> GetPatientAppointmentCountsAsync(int patientId, DateTime asOf);

        /// <summary>
        /// دریافت نوبت‌های بیمار
        /// </summary>
        Task<List<Models.Entities.Appointment.Appointment>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// دریافت نوبت بر اساس شناسه
        /// </summary>
        Task<Models.Entities.Appointment.Appointment> GetAppointmentByIdAsync(int appointmentId);

        /// <summary>
        /// ایجاد نوبت جدید
        /// </summary>
        Task<Models.Entities.Appointment.Appointment> CreateAppointmentAsync(Models.Entities.Appointment.Appointment appointment);

        /// <summary>
        /// به‌روزرسانی وضعیت نوبت
        /// </summary>
        Task<bool> UpdateAppointmentStatusAsync(
            int appointmentId,
            Models.Enums.AppointmentStatus status);

        /// <summary>
        /// بررسی دسترسی‌پذیری اسلات زمانی
        /// </summary>
        Task<bool> CheckSlotAvailabilityAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime);

        /// <summary>
        /// دریافت نوبت‌های یک پزشک در یک تاریخ مشخص
        /// </summary>
        Task<List<Models.Entities.Appointment.Appointment>> GetDoctorAppointmentsByDateAsync(
            int doctorId,
            DateTime date);

        /// <summary>
        /// بررسی تداخل نوبت‌های بیمار (Double Booking Prevention)
        /// ✅ CRITICAL: با Locking برای جلوگیری از Race Condition
        /// </summary>
        Task<bool> HasOverlappingPatientAppointmentAsync(
            int patientId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime);
    }
}

