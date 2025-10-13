using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Triage;

namespace ClinicApp.ViewModels.Triage
{
    /// <summary>
    /// ViewModel برای مدیریت صف تریاژ
    /// </summary>
    public class TriageQueueViewModel
    {
        [Display(Name = "بخش")]
        public int? DepartmentId { get; set; }

        [Display(Name = "نام بخش")]
        public string DepartmentName { get; set; }

        [Display(Name = "وضعیت صف")]
        public string Status { get; set; } = "Waiting";

        [Display(Name = "اولویت")]
        public string Priority { get; set; } = "All";

        [Display(Name = "سطح تریاژ")]
        public string Level { get; set; } = "All";

        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        // نتایج
        public List<TriageQueueItemDto> QueueItems { get; set; } = new List<TriageQueueItemDto>();
        public TriageQueueStats Stats { get; set; } = new TriageQueueStats();

        // فیلترها
        public List<LookupItemViewModel> Departments { get; set; } = new List<LookupItemViewModel>();
        public List<LookupItemViewModel> StatusOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "All", Text = "همه" },
            new LookupItemViewModel { Value = "Waiting", Text = "در انتظار" },
            new LookupItemViewModel { Value = "Called", Text = "فراخوانی شده" },
            new LookupItemViewModel { Value = "Completed", Text = "تکمیل شده" }
        };

        public List<LookupItemViewModel> PriorityOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "All", Text = "همه" },
            new LookupItemViewModel { Value = "1", Text = "اولویت 1" },
            new LookupItemViewModel { Value = "2", Text = "اولویت 2" },
            new LookupItemViewModel { Value = "3", Text = "اولویت 3" },
            new LookupItemViewModel { Value = "4", Text = "اولویت 4" },
            new LookupItemViewModel { Value = "5", Text = "اولویت 5" }
        };

        public List<LookupItemViewModel> LevelOptions { get; set; } = new List<LookupItemViewModel>
        {
            new LookupItemViewModel { Value = "All", Text = "همه" },
            new LookupItemViewModel { Value = "ESI1", Text = "بحرانی (ESI-1)" },
            new LookupItemViewModel { Value = "ESI2", Text = "فوری (ESI-2)" },
            new LookupItemViewModel { Value = "ESI3", Text = "عاجل (ESI-3)" },
            new LookupItemViewModel { Value = "ESI4", Text = "کم‌عاجل (ESI-4)" },
            new LookupItemViewModel { Value = "ESI5", Text = "غیرعاجل (ESI-5)" }
        };

        // محاسبه شده
        public int TotalItems => QueueItems.Count;
        public int CriticalItems => QueueItems.Count(q => q.Level == TriageLevel.ESI1);
        public int HighPriorityItems => QueueItems.Count(q => q.Level == TriageLevel.ESI1 || q.Level == TriageLevel.ESI2);
        public int OverdueItems => QueueItems.Count(q => q.IsOverdue);
        public bool HasOverdueItems => OverdueItems > 0;
        public string UrgencyMessage => HasOverdueItems ? $"⚠️ {OverdueItems} بیمار در انتظار بیش از حد مجاز" : "✅ همه بیماران در زمان مناسب";
    }

    /// <summary>
    /// ViewModel برای آیتم صف تریاژ
    /// </summary>
    public class TriageQueueItemViewModel
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
        public string Status { get; set; }
        public string ChiefComplaint { get; set; }
        public bool IsOverdue { get; set; }
        public int? EstimatedWaitTimeMinutes { get; set; }

        // محاسبه شده
        public string LevelDisplayName => GetLevelDisplayName(Level);
        public string StatusDisplayName => GetStatusDisplayName(Status);
        public bool IsUrgent => Level == TriageLevel.ESI1 || Level == TriageLevel.ESI2;
        public TimeSpan? WaitTime => DateTime.UtcNow - QueueTimeUtc;
        public string WaitTimeDisplay => WaitTime?.ToString(@"hh\:mm") ?? "نامشخص";
        public string UrgencyClass => IsUrgent ? "urgent" : "normal";
        public string OverdueClass => IsOverdue ? "overdue" : "ontime";
        
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
