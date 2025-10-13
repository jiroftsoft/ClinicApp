using System;
using System.Collections.Generic;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Triage;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای داشبورد تریاژ
    /// </summary>
    public class TriageDashboardViewModel
    {
        // آمار کلی
        public TriageDailyStats DailyStats { get; set; } = new TriageDailyStats();
        public TriageQueueStats QueueStats { get; set; } = new TriageQueueStats();

        // لیست‌های مهم
        public List<TriageQueueItemDto> UrgentQueue { get; set; } = new List<TriageQueueItemDto>();
        public List<TriageQueueItemDto> OverdueQueue { get; set; } = new List<TriageQueueItemDto>();
        public List<TriageSummaryDto> RecentAssessments { get; set; } = new List<TriageSummaryDto>();
        public List<TriageAlert> ActiveAlerts { get; set; } = new List<TriageAlert>();

        // فیلترها
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public int? SelectedDepartmentId { get; set; }
        public string SelectedDepartmentName { get; set; }

        // وضعیت سیستم
        public bool IsSystemHealthy { get; set; } = true;
        public string SystemStatusMessage { get; set; } = "سیستم در وضعیت عادی";
        public List<string> SystemWarnings { get; set; } = new List<string>();

        // محاسبه شده
        public int TotalActiveAssessments => DailyStats.TotalAssessments - DailyStats.CompletedAssessments;
        public int CriticalPatients => DailyStats.CriticalLevel + DailyStats.HighLevel;
        public double CompletionRate => DailyStats.TotalAssessments > 0 ? 
            (double)DailyStats.CompletedAssessments / DailyStats.TotalAssessments * 100 : 0;
        public bool HasUrgentPatients => UrgentQueue.Count > 0;
        public bool HasOverduePatients => OverdueQueue.Count > 0;
        public bool HasActiveAlerts => ActiveAlerts.Count > 0;

        // پیغام‌های وضعیت
        public string UrgencyMessage => HasUrgentPatients ? 
            $"⚠️ {UrgentQueue.Count} بیمار بحرانی در انتظار" : "✅ هیچ بیمار بحرانی در انتظار نیست";
        
        public string OverdueMessage => HasOverduePatients ? 
            $"🚨 {OverdueQueue.Count} بیمار در انتظار بیش از حد مجاز" : "✅ همه بیماران در زمان مناسب";
        
        public string AlertMessage => HasActiveAlerts ? 
            $"🔔 {ActiveAlerts.Count} هشدار فعال" : "✅ هیچ هشدار فعالی وجود ندارد";

        // کلاس‌های CSS
        public string SystemStatusClass => IsSystemHealthy ? "success" : "danger";
        public string UrgencyClass => HasUrgentPatients ? "danger" : "success";
        public string OverdueClass => HasOverduePatients ? "warning" : "success";
        public string AlertClass => HasActiveAlerts ? "warning" : "success";
    }

    /// <summary>
    /// آمار روزانه تریاژ
    /// </summary>
    public class TriageDailyStats
    {
        public DateTime Date { get; set; }
        public int TotalAssessments { get; set; }
        public int CriticalLevel { get; set; }
        public int HighLevel { get; set; }
        public int MediumLevel { get; set; }
        public int LowLevel { get; set; }
        public int VeryLowLevel { get; set; }
        public int CompletedAssessments { get; set; }
        public int PendingAssessments { get; set; }
        public int AverageWaitTimeMinutes { get; set; }
        public int AverageAssessmentTimeMinutes { get; set; }

        // محاسبه شده
        public int TotalLevels => CriticalLevel + HighLevel + MediumLevel + LowLevel + VeryLowLevel;
        public double CriticalPercentage => TotalAssessments > 0 ? (double)CriticalLevel / TotalAssessments * 100 : 0;
        public double HighPercentage => TotalAssessments > 0 ? (double)HighLevel / TotalAssessments * 100 : 0;
        public double CompletionPercentage => TotalAssessments > 0 ? (double)CompletedAssessments / TotalAssessments * 100 : 0;
        public string WaitTimeDisplay => $"{AverageWaitTimeMinutes} دقیقه";
        public string AssessmentTimeDisplay => $"{AverageAssessmentTimeMinutes} دقیقه";
    }

    /// <summary>
    /// هشدار تریاژ
    /// </summary>
    public class TriageAlert
    {
        public int AlertId { get; set; }
        public int AssessmentId { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
        public TriageLevel Severity { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // محاسبه شده
        public string SeverityDisplay => GetSeverityDisplay(Severity);
        public string AlertClass => GetAlertClass(Severity);
        public string TimeDisplay => GetTimeDisplay(CreatedAt);
        public bool IsUrgent => Severity == TriageLevel.ESI1 || Severity == TriageLevel.ESI2;
        
        private string GetSeverityDisplay(TriageLevel severity)
        {
            return severity switch
            {
                TriageLevel.ESI1 => "بحرانی",
                TriageLevel.ESI2 => "فوری",
                TriageLevel.ESI3 => "عاجل",
                TriageLevel.ESI4 => "کم‌عاجل",
                TriageLevel.ESI5 => "غیرعاجل",
                _ => "نامشخص"
            };
        }
        
        private string GetAlertClass(TriageLevel severity)
        {
            return severity switch
            {
                TriageLevel.ESI1 => "alert-danger",
                TriageLevel.ESI2 => "alert-warning",
                TriageLevel.ESI3 => "alert-info",
                TriageLevel.ESI4 => "alert-success",
                TriageLevel.ESI5 => "alert-secondary",
                _ => "alert-light"
            };
        }
        
        private string GetTimeDisplay(DateTime createdAt)
        {
            var timeSpan = DateTime.UtcNow - createdAt;
            if (timeSpan.TotalMinutes < 1)
                return "همین الان";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";
            return $"{(int)timeSpan.TotalDays} روز پیش";
        }
    }
}
