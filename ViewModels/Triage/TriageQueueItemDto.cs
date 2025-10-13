using System;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// DTO برای آیتم صف تریاژ
    /// </summary>
    public class TriageQueueItemDto
    {
        public int QueueId { get; set; }
        public int TriageAssessmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public TriageLevel Level { get; set; }
        public int Priority { get; set; }
        public DateTime QueueTimeUtc { get; set; }
        public DateTime? NextReassessmentDueAtUtc { get; set; }
        public int ReassessmentCount { get; set; }
        public string Status { get; set; } // Waiting/Called/Completed
        public string ChiefComplaint { get; set; }
        public bool IsOverdue { get; set; }
        public int? EstimatedWaitTimeMinutes { get; set; }
        
        // محاسبه شده
        public string LevelDisplayName => GetLevelDisplayName(Level);
        public string StatusDisplayName => GetStatusDisplayName(Status);
        public bool IsUrgent => Level == TriageLevel.ESI1 || Level == TriageLevel.ESI2;
        public TimeSpan? WaitTime => DateTime.UtcNow - QueueTimeUtc;
        
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
        
        private string GetStatusDisplayName(string status)
        {
            return status switch
            {
                "Waiting" => "در انتظار",
                "Called" => "فراخوانی شده",
                "Completed" => "تکمیل شده",
                _ => "نامشخص"
            };
        }
    }
}
