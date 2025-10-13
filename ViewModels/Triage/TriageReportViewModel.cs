using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Triage;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای گزارش‌گیری تریاژ
    /// </summary>
    public class TriageReportViewModel
    {
        // فیلترها
        [Display(Name = "تاریخ شروع")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);

        [Display(Name = "تاریخ پایان")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "بخش")]
        public int? DepartmentId { get; set; }

        [Display(Name = "نام بخش")]
        public string DepartmentName { get; set; }

        [Display(Name = "سطح تریاژ")]
        public TriageLevel? Level { get; set; }

        [Display(Name = "وضعیت")]
        public TriageStatus? Status { get; set; }

        [Display(Name = "نوع گزارش")]
        public ReportType ReportType { get; set; } = ReportType.Summary;

        [Display(Name = "فرمت خروجی")]
        public ExportFormat ExportFormat { get; set; } = ExportFormat.Html;

        // نتایج
        public List<TriageReportItem> ReportItems { get; set; } = new List<TriageReportItem>();
        public TriageReportSummary Summary { get; set; } = new TriageReportSummary();

        // فیلترها
        public List<LookupItemViewModel> Departments { get; set; } = new List<LookupItemViewModel>();
        public List<LookupItemViewModel> LevelOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "", Text = "همه" },
            new LookupItemViewModel { Value = "ESI1", Text = "بحرانی (ESI-1)" },
            new LookupItemViewModel { Value = "ESI2", Text = "فوری (ESI-2)" },
            new LookupItemViewModel { Value = "ESI3", Text = "عاجل (ESI-3)" },
            new LookupItemViewModel { Value = "ESI4", Text = "کم‌عاجل (ESI-4)" },
            new LookupItemViewModel { Value = "ESI5", Text = "غیرعاجل (ESI-5)" }
        };

        public List<LookupItemViewModel> StatusOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "", Text = "همه" },
            new LookupItemViewModel { Value = "Pending", Text = "در انتظار" },
            new LookupItemViewModel { Value = "InProgress", Text = "در حال انجام" },
            new LookupItemViewModel { Value = "Completed", Text = "تکمیل شده" },
            new LookupItemViewModel { Value = "Cancelled", Text = "لغو شده" }
        };

        public List<LookupItemViewModel> ReportTypeOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "Summary", Text = "خلاصه" },
            new LookupItemViewModel { Value = "Detailed", Text = "جزئیات" },
            new LookupItemViewModel { Value = "Performance", Text = "عملکرد" },
            new LookupItemViewModel { Value = "Trends", Text = "روندها" }
        };

        public List<LookupItemViewModel> ExportFormatOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "Html", Text = "HTML" },
            new LookupItemViewModel { Value = "Excel", Text = "Excel" },
            new LookupItemViewModel { Value = "Pdf", Text = "PDF" },
            new LookupItemViewModel { Value = "Csv", Text = "CSV" }
        };

        // محاسبه شده
        public int TotalItems => ReportItems.Count;
        public bool HasData => ReportItems.Count > 0;
        public string DateRangeDisplay => $"{StartDate:yyyy/MM/dd} تا {EndDate:yyyy/MM/dd}";
        public string FilterDisplay => GetFilterDisplay();
        
        private string GetFilterDisplay()
        {
            var filters = new List<string>();
            if (DepartmentId.HasValue)
                filters.Add($"بخش: {DepartmentName}");
            if (Level.HasValue)
                filters.Add($"سطح: {GetLevelDisplay(Level.Value)}");
            if (Status.HasValue)
                filters.Add($"وضعیت: {GetStatusDisplay(Status.Value)}");
            
            return filters.Count > 0 ? string.Join("، ", filters) : "بدون فیلتر";
        }
        
        private string GetLevelDisplay(TriageLevel level)
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
        
        private string GetStatusDisplay(TriageStatus status)
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

    /// <summary>
    /// آیتم گزارش تریاژ
    /// </summary>
    public class TriageReportItem
    {
        public int AssessmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string ChiefComplaint { get; set; }
        public TriageLevel Level { get; set; }
        public int Priority { get; set; }
        public TriageStatus Status { get; set; }
        public DateTime ArrivalAt { get; set; }
        public DateTime TriageStartAt { get; set; }
        public DateTime? TriageEndAt { get; set; }
        public int ReassessmentCount { get; set; }
        public string DepartmentName { get; set; }
        public string DoctorName { get; set; }
        public bool IsolationRequired { get; set; }
        public IsolationType? Isolation { get; set; }

        // محاسبه شده
        public string LevelDisplay => GetLevelDisplay(Level);
        public string StatusDisplay => GetStatusDisplay(Status);
        public TimeSpan? TotalTime => TriageEndAt.HasValue ? TriageEndAt.Value - TriageStartAt : null;
        public string TotalTimeDisplay => TotalTime?.ToString(@"hh\:mm") ?? "نامشخص";
        public bool IsCompleted => Status == TriageStatus.Completed;
        public bool HasIsolation => Isolation.HasValue;
        
        private string GetLevelDisplay(TriageLevel level)
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
        
        private string GetStatusDisplay(TriageStatus status)
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

    /// <summary>
    /// خلاصه گزارش تریاژ
    /// </summary>
    public class TriageReportSummary
    {
        public int TotalAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public int PendingAssessments { get; set; }
        public int CriticalAssessments { get; set; }
        public int HighPriorityAssessments { get; set; }
        public double AverageWaitTimeMinutes { get; set; }
        public double AverageAssessmentTimeMinutes { get; set; }
        public double CompletionRate { get; set; }
        public int IsolationCount { get; set; }
        public int ReassessmentCount { get; set; }

        // محاسبه شده
        public double CompletionPercentage => TotalAssessments > 0 ? (double)CompletedAssessments / TotalAssessments * 100 : 0;
        public double CriticalPercentage => TotalAssessments > 0 ? (double)CriticalAssessments / TotalAssessments * 100 : 0;
        public string AverageWaitTimeDisplay => $"{AverageWaitTimeMinutes:F1} دقیقه";
        public string AverageAssessmentTimeDisplay => $"{AverageAssessmentTimeMinutes:F1} دقیقه";
        public string CompletionRateDisplay => $"{CompletionRate:F1}%";
    }

    /// <summary>
    /// نوع گزارش
    /// </summary>
    public enum ReportType
    {
        Summary,
        Detailed,
        Performance,
        Trends
    }

    /// <summary>
    /// فرمت خروجی
    /// </summary>
    public enum ExportFormat
    {
        Html,
        Excel,
        Pdf,
        Csv
    }
}
