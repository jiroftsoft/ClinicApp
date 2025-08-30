using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class AssignmentFilterViewModel
    {
        [Display(Name = "جستجو")]
        public string SearchTerm { get; set; }

        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "صلاحیت خدماتی")]
        public int? ServiceCategoryId { get; set; }

        [Display(Name = "وضعیت")]
        public string Status { get; set; }

        [Display(Name = "تاریخ از")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "تاریخ تا")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "نوع انتساب")]
        public string AssignmentType { get; set; }

        // Available options for dropdowns
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ServiceCategories { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AssignmentTypes { get; set; } = new List<SelectListItem>();

        public AssignmentFilterViewModel()
        {
            // Initialize status options
            Statuses = new List<SelectListItem>
            {
                new SelectListItem { Text = "همه", Value = "" },
                new SelectListItem { Text = "فعال", Value = "active" },
                new SelectListItem { Text = "غیرفعال", Value = "inactive" },
                new SelectListItem { Text = "انتظار تایید", Value = "pending" }
            };

            // Initialize assignment type options
            AssignmentTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "همه", Value = "" },
                new SelectListItem { Text = "تک دپارتمان", Value = "single" },
                new SelectListItem { Text = "چند دپارتمان", Value = "multiple" }
            };
        }

        // Helper methods
        public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                                 DepartmentId.HasValue || 
                                 !string.IsNullOrEmpty(Status) || 
                                 DateFrom.HasValue || 
                                 DateTo.HasValue || 
                                 !string.IsNullOrEmpty(AssignmentType);

        public string GetFilterSummary()
        {
            var filters = new List<string>();
            
            if (!string.IsNullOrEmpty(SearchTerm))
                filters.Add($"جستجو: {SearchTerm}");
            
            if (DepartmentId.HasValue)
                filters.Add($"دپارتمان: {Departments.FirstOrDefault(d => d.Value == DepartmentId.ToString())?.Text}");
            
            if (!string.IsNullOrEmpty(Status))
                filters.Add($"وضعیت: {Statuses.FirstOrDefault(s => s.Value == Status)?.Text}");
            
            if (DateFrom.HasValue)
                filters.Add($"از تاریخ: {DateFrom.Value.ToString("yyyy/MM/dd")}");
            
            if (DateTo.HasValue)
                filters.Add($"تا تاریخ: {DateTo.Value.ToString("yyyy/MM/dd")}");
            
            if (!string.IsNullOrEmpty(AssignmentType))
                filters.Add($"نوع: {AssignmentTypes.FirstOrDefault(t => t.Value == AssignmentType)?.Text}");

            return string.Join(" | ", filters);
        }
    }

    public class SelectListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
