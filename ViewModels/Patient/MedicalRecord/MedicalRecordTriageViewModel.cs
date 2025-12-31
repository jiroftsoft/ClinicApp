using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel برای نمایش تریاژ در EMR
    /// Single Responsibility: نمایش داده‌های تریاژ در EMR
    /// </summary>
    public class MedicalRecordTriageViewModel
    {
        public int TriageAssessmentId { get; set; }
        public string AssessmentNumber { get; set; }
        public string AssessorName { get; set; }
        public Models.Enums.TriageLevel Level { get; set; }
        public string LevelText { get; set; }
        public int? EsiScore { get; set; }
        public int? News2Score { get; set; }
        public int? PewsScore { get; set; }
        public string ChiefComplaint { get; set; }
        public DateTime ArrivalAt { get; set; }
        public string ArrivalAtShamsi { get; set; }
        public DateTime TriageStartAt { get; set; }
        public string TriageStartAtShamsi { get; set; }
        public DateTime? TriageEndAt { get; set; }
        public string TriageEndAtShamsi { get; set; }
        
        // Vital Signs
        public MedicalRecordVitalSignsViewModel VitalSigns { get; set; }
    }
    
    /// <summary>
    /// ViewModel برای علائم حیاتی
    /// </summary>
    public class MedicalRecordVitalSignsViewModel
    {
        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public int? RespiratoryRate { get; set; }
        public int? OxygenSaturation { get; set; }
        public int? PainLevel { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? BMI { get; set; }
        public int? GcsTotal { get; set; }
    }
}

