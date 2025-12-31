using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel برای نمایش پذیرش‌ها در EMR
    /// Single Responsibility: نمایش داده‌های پذیرش در EMR
    /// </summary>
    public class MedicalRecordReceptionViewModel
    {
        public int ReceptionId { get; set; }
        public string ReceptionNumber { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string ClinicName { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string ReceptionDateShamsi { get; set; }
        public string ReceptionTime { get; set; }
        public Models.Enums.ReceptionStatus Status { get; set; }
        public string StatusText { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsurerShare { get; set; }
        public string Notes { get; set; }
        public bool IsEmergency { get; set; }
    }
}

