using System;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای درخواست رزرو نوبت
    /// </summary>
    public class AppointmentBookingRequestDto
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? ServiceCategoryId { get; set; }
        public string Description { get; set; }
        public int PatientId { get; set; }
    }
}

