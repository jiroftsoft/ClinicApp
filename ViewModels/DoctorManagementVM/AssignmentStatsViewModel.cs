using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class AssignmentStatsViewModel
    {
        [Display(Name = "کل انتسابات")]
        public int TotalAssignments { get; set; }

        [Display(Name = "انتسابات فعال")]
        public int ActiveAssignments { get; set; }

        [Display(Name = "انتسابات غیرفعال")]
        public int InactiveAssignments { get; set; }

        [Display(Name = "پزشکان انتساب شده")]
        public int AssignedDoctors { get; set; }

        [Display(Name = "کل پزشکان")]
        public int TotalDoctors { get; set; }

        [Display(Name = "دپارتمان‌های فعال")]
        public int ActiveDepartments { get; set; }

        [Display(Name = "صلاحیت‌های خدماتی")]
        public int ServiceCategories { get; set; }

        [Display(Name = "درصد تکمیل")]
        public decimal CompletionPercentage { get; set; }

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdate { get; set; }

        // Calculated properties
        public string CompletionPercentageFormatted => $"{CompletionPercentage:F1}%";
        public string LastUpdateFormatted => LastUpdate.ToString("yyyy/MM/dd HH:mm");
        public bool HasActiveAssignments => ActiveAssignments > 0;
        public bool HasInactiveAssignments => InactiveAssignments > 0;
    }
}
