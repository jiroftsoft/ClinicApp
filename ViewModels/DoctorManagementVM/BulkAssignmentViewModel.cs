using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class BulkAssignmentViewModel
    {
        [Required(ErrorMessage = "لطفاً حداقل یک پزشک انتخاب کنید")]
        [Display(Name = "پزشکان")]
        public List<int> DoctorIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "لطفاً دپارتمان را انتخاب کنید")]
        [Display(Name = "دپارتمان")]
        public int DepartmentId { get; set; }

        [Display(Name = "سرفصل‌های خدماتی")]
        public List<int> ServiceCategoryIds { get; set; } = new List<int>();

        [Display(Name = "وضعیت انتساب")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "اعمال به همه")]
        public bool ApplyToAll { get; set; }

        [Display(Name = "جایگزینی انتسابات موجود")]
        public bool ReplaceExisting { get; set; }

        // Available options for dropdowns
        public List<DoctorListItem> AvailableDoctors { get; set; } = new List<DoctorListItem>();
        public List<DepartmentListItem> AvailableDepartments { get; set; } = new List<DepartmentListItem>();
        public List<ServiceCategoryListItem> AvailableServiceCategories { get; set; } = new List<ServiceCategoryListItem>();

        // Validation properties
        public bool HasValidDoctors => DoctorIds != null && DoctorIds.Count > 0;
        public bool HasValidDepartment => DepartmentId > 0;
        public bool HasServiceCategories => ServiceCategoryIds != null && ServiceCategoryIds.Count > 0;

        public string GetValidationSummary()
        {
            var errors = new List<string>();

            if (!HasValidDoctors)
                errors.Add("حداقل یک پزشک باید انتخاب شود");

            if (!HasValidDepartment)
                errors.Add("دپارتمان باید انتخاب شود");

            return string.Join(", ", errors);
        }
    }

    public class DoctorListItem
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string Specialization { get; set; }
        public bool IsActive { get; set; }
        public int CurrentAssignments { get; set; }

        public string DisplayName => $"{FullName} - {NationalCode}";
    }

    public class DepartmentListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public int DoctorCount { get; set; }

        public string DisplayName => $"{Name} ({Code})";
    }

    public class ServiceCategoryListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        public string DisplayName => $"{Name} ({Code})";
    }
}
