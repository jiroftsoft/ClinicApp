using System;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// یک ردیف گزارش «نوبت‌های رزرو شده توسط بیماران» برای منشی.
    /// </summary>
    public class PatientBookedAppointmentReportItemDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int Duration { get; set; }
        public string PatientName { get; set; }
        public string PatientPhone { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialty { get; set; }
        public string StatusDisplay { get; set; }
        public int Status { get; set; }
        public decimal Price { get; set; }
        public bool IsOnlineConsultation { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
