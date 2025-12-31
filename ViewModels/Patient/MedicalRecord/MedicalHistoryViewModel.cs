using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel برای نمایش تاریخچه پزشکی
    /// Single Responsibility: نمایش داده‌های تاریخچه پزشکی
    /// </summary>
    public class MedicalHistoryViewModel
    {
        public int MedicalHistoryId { get; set; }
        public int PatientId { get; set; }
        public Models.Enums.MedicalHistoryType Type { get; set; }
        public string TypeText { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public string StartDateShamsi { get; set; }
        public DateTime? EndDate { get; set; }
        public string EndDateShamsi { get; set; }
        public bool IsActive { get; set; }
        public string Severity { get; set; }
        public string DoctorName { get; set; }
        public string MedicalCenter { get; set; }
        public string Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtShamsi { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtShamsi { get; set; }
    }
}

