using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel برای نمایش نوبت‌های پزشکی در EMR
    /// Single Responsibility: نمایش داده‌های نوبت در EMR
    /// </summary>
    public class MedicalRecordAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentDateShamsi { get; set; }
        public string AppointmentTime { get; set; }
        public Models.Enums.AppointmentStatus Status { get; set; }
        public string StatusText { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public bool IsNewPatient { get; set; }
        public string ServiceCategory { get; set; }
        public int? Duration { get; set; }
    }
}

