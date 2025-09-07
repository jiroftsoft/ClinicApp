using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل پیش‌نمایش انتساب پزشک
    /// </summary>
    public class AssignmentPreviewViewModel
    {
        /// <summary>
        /// اطلاعات پزشک
        /// </summary>
        public DoctorInfoViewModel Doctor { get; set; }

        /// <summary>
        /// لیست دپارتمان‌های انتخاب شده
        /// </summary>
        public List<DepartmentAssignmentViewModel> DepartmentAssignments { get; set; } = new List<DepartmentAssignmentViewModel>();

        /// <summary>
        /// لیست سرفصل‌های خدماتی انتخاب شده
        /// </summary>
        public List<ServiceCategoryAssignmentViewModel> ServiceCategoryAssignments { get; set; } = new List<ServiceCategoryAssignmentViewModel>();

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        /// <summary>
        /// اعمال فوری
        /// </summary>
        [Display(Name = "اعمال فوری")]
        public bool ApplyImmediately { get; set; }

        /// <summary>
        /// تاریخ پیش‌نمایش
        /// </summary>
        [Display(Name = "تاریخ پیش‌نمایش")]
        public DateTime PreviewDate { get; set; } = DateTime.Now;

        /// <summary>
        /// تعداد کل انتسابات
        /// </summary>
        public int TotalAssignments => DepartmentAssignments.Count + ServiceCategoryAssignments.Count;
    }






}
