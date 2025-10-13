using System;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// آمار صف تریاژ
    /// </summary>
    public class TriageQueueStats
    {
        public int TotalInQueue { get; set; }
        public int WaitingCount { get; set; }
        public int CalledCount { get; set; }
        public int CompletedCount { get; set; }
        public int CriticalCount { get; set; }
        public int OverdueCount { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public int MaxWaitTimeMinutes { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
