using System;
using System.Collections.Generic;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// DTO برای خلاصه ارزیابی تریاژ
    /// </summary>
    public class TriageSummaryDto
    {
        public int AssessmentId { get; set; }
        public int PatientId { get; set; }
        public TriageLevel Level { get; set; }
        public int Priority { get; set; }
        public TriageStatus Status { get; set; }
        public string ChiefComplaint { get; set; }
        public DateTime ArrivalAt { get; set; }
        public DateTime TriageStartAt { get; set; }
        public DateTime? TriageEndAt { get; set; }
        public bool IsOpen { get; set; }
        public List<string> RedFlags { get; set; } = new List<string>();
        public bool IsolationRequired { get; set; }
        public TriageVitalSigns LastVitalSigns { get; set; }
        public int ProtocolsCount { get; set; }
        public int ReassessmentCount { get; set; }
        public DateTime? LastReassessmentAt { get; set; }
        
        // محاسبه شده
        public TimeSpan? TotalAssessmentTime => TriageEndAt.HasValue ? TriageEndAt.Value - TriageStartAt : null;
        public string LevelDisplayName => GetLevelDisplayName(Level);
        public string StatusDisplayName => GetStatusDisplayName(Status);
        
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
        
        private string GetStatusDisplayName(TriageStatus status)
        {
            return status switch
            {
                TriageStatus.Pending => "در انتظار",
                TriageStatus.InProgress => "در حال انجام",
                TriageStatus.Completed => "تکمیل شده",
                TriageStatus.Cancelled => "لغو شده",
                _ => "نامشخص"
            };
        }
    }
}
