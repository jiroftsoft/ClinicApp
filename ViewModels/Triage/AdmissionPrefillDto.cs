using System;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// DTO برای پری‌فیل پذیرش از تریاژ
    /// </summary>
    public class AdmissionPrefillDto
    {
        public int TriageAssessmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public TriageLevel Level { get; set; }
        public bool AnyRedFlag { get; set; }
        public IsolationType? Isolation { get; set; }
        public string ChiefComplaint { get; set; }
        public VitalBrief LastVitals { get; set; }
        public int? RecommendedDepartmentId { get; set; }
        public int? RecommendedDoctorId { get; set; }
        public string AssessmentNotes { get; set; }
        public DateTime ArrivalAtUtc { get; set; }
        public DateTime TriageStartAtUtc { get; set; }
        public DateTime? TriageEndAtUtc { get; set; }
        public int ReassessmentCount { get; set; }
        public bool IsReadyForAdmission { get; set; }
        
        // محاسبه شده
        public string LevelDisplayName => GetLevelDisplayName(Level);
        public TimeSpan? TotalAssessmentTime => TriageEndAtUtc.HasValue ? TriageEndAtUtc.Value - TriageStartAtUtc : null;
        public bool HasIsolation => Isolation.HasValue;
        public bool IsHighPriority => Level == TriageLevel.ESI1 || Level == TriageLevel.ESI2;
        
        private string GetLevelDisplayName(TriageLevel level)
        {
            return level switch
            {
                TriageLevel.ESI1 => "بحرانی (ESI-1)",
                TriageLevel.ESI2 => "فوری (ESI-2)",
                TriageLevel.ESI3 => "عاجل (ESI-3)",
                TriageLevel.ESI4 => "کم‌عاجل (ESI-4)",
                TriageLevel.ESI5 => "غیرعاجل (ESI-5)",
                _ => "نامشخص"
            };
        }
    }

    /// <summary>
    /// خلاصه علائم حیاتی
    /// </summary>
    public class VitalBrief
    {
        public int? SBP { get; set; }
        public int? DBP { get; set; }
        public int? HR { get; set; }
        public int? RR { get; set; }
        public double? TempC { get; set; }
        public int? SpO2 { get; set; }
        public int? GcsTotal { get; set; }
        public DateTime? MeasuredAtUtc { get; set; }
        public bool OnOxygen { get; set; }
        public string OxygenDevice { get; set; }
        public decimal? O2FlowLpm { get; set; }
        
        // محاسبه شده
        public bool IsNormal => 
            (SBP >= 90 && SBP <= 140) &&
            (DBP >= 60 && DBP <= 90) &&
            (HR >= 60 && HR <= 100) &&
            (RR >= 12 && RR <= 20) &&
            (TempC >= 36 && TempC <= 37.5) &&
            (SpO2 >= 95) &&
            (GcsTotal >= 13);
            
        public bool RequiresImmediateAttention =>
            (SpO2 < 90) ||
            (HR > 120 || HR < 50) ||
            (SBP < 90) ||
            (TempC > 39 || TempC < 35) ||
            (RR > 30 || RR < 10) ||
            (GcsTotal < 8);
    }
}
