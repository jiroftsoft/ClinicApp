using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities.Doctor;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل انتسابات پزشک
    /// طراحی شده برای محیط‌های درمانی با رعایت استانداردهای پزشکی
    /// </summary>
    public class DoctorAssignmentDetailsViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        [Required(ErrorMessage = "نام پزشک الزامی است")]
        [StringLength(100, ErrorMessage = "نام پزشک نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "کد ملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد")]
        public string DoctorNationalCode { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [Display(Name = "تخصص")]
        public string DoctorSpecialization { get; set; }

        /// <summary>
        /// شماره نظام پزشکی
        /// </summary>
        [Display(Name = "شماره نظام پزشکی")]
        public string MedicalCouncilNumber { get; set; }

        /// <summary>
        /// شماره پروانه پزشک (برای سازگاری با View)
        /// </summary>
        [Display(Name = "شماره پروانه")]
        public string DoctorLicenseNumber => MedicalCouncilNumber;

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// آیا پزشک فعال است (برای سازگاری با View)
        /// </summary>
        [Display(Name = "وضعیت فعال")]
        public bool IsActive => TotalActiveAssignments > 0;

        /// <summary>
        /// تاریخ ثبت (برای سازگاری با View)
        /// </summary>
        [Display(Name = "تاریخ ثبت")]
        public DateTime? RegistrationDate => FirstAssignmentDate;

        /// <summary>
        /// تاریخ آخرین تغییر (برای سازگاری با View)
        /// </summary>
        [Display(Name = "آخرین تغییر")]
        public DateTime LastModifiedDate => LastUpdateTime;

        /// <summary>
        /// تعداد کل انتسابات فعال
        /// </summary>
        [Display(Name = "کل انتسابات فعال")]
        public int TotalActiveAssignments { get; set; }

        /// <summary>
        /// تعداد کل انتسابات (برای سازگاری با View)
        /// </summary>
        [Display(Name = "تعداد کل انتسابات")]
        public int TotalAssignments => TotalActiveAssignments;

        /// <summary>
        /// تعداد دپارتمان‌های فعال
        /// </summary>
        [Display(Name = "دپارتمان‌های فعال")]
        public int ActiveDepartmentCount { get; set; }

        /// <summary>
        /// تعداد صلاحیت‌های فعال
        /// </summary>
        [Display(Name = "صلاحیت‌های فعال")]
        public int ActiveServiceCategoryCount { get; set; }

        /// <summary>
        /// آیا پزشک چند دپارتمانه است
        /// </summary>
        [Display(Name = "نوع انتساب")]
        public bool IsMultiDepartment { get; set; }

        /// <summary>
        /// لیست انتسابات دپارتمان‌ها
        /// </summary>
        public List<DoctorDepartmentViewModel> Departments { get; set; } = new List<DoctorDepartmentViewModel>();

        /// <summary>
        /// لیست انتسابات دپارتمان‌ها (برای سازگاری با View)
        /// </summary>
        public List<DoctorDepartmentViewModel> DepartmentAssignments => Departments;

        /// <summary>
        /// لیست صلاحیت‌های خدماتی
        /// </summary>
        public List<DoctorServiceCategoryViewModel> ServiceCategories { get; set; } = new List<DoctorServiceCategoryViewModel>();

        /// <summary>
        /// لیست انتسابات سرفصل‌های خدماتی (برای سازگاری با View)
        /// </summary>
        public List<DoctorServiceCategoryViewModel> ServiceCategoryAssignments => ServiceCategories;

        /// <summary>
        /// تاریخچه انتسابات
        /// </summary>
        public List<AssignmentHistoryViewModel> History { get; set; } = new List<AssignmentHistoryViewModel>();

        /// <summary>
        /// تاریخچه انتسابات (برای سازگاری با View)
        /// </summary>
        public List<AssignmentHistoryViewModel> AssignmentHistory => History;

        /// <summary>
        /// آمار کلی برای نمایش در کارت‌ها
        /// </summary>
        public AssignmentStatsViewModel Stats { get; set; } = new AssignmentStatsViewModel();

        /// <summary>
        /// وضعیت کلی پزشک
        /// </summary>
        [Display(Name = "وضعیت")]
        public string OverallStatus { get; set; }

        /// <summary>
        /// یادداشت‌های مدیریتی
        /// </summary>
        [Display(Name = "یادداشت‌ها")]
        public string ManagementNotes { get; set; }

        /// <summary>
        /// تاریخ شروع اولین انتساب
        /// </summary>
        [Display(Name = "تاریخ شروع اولین انتساب")]
        public DateTime? FirstAssignmentDate { get; set; }

        /// <summary>
        /// تاریخ آخرین انتساب
        /// </summary>
        [Display(Name = "تاریخ آخرین انتساب")]
        public DateTime? LastAssignmentDate { get; set; }

        /// <summary>
        /// مدت زمان کل انتسابات (به روز)
        /// </summary>
        [Display(Name = "مدت زمان کل انتسابات")]
        public int TotalAssignmentDays { get; set; }

        // Helper Properties

        /// <summary>
        /// متن وضعیت کلی
        /// </summary>
        public string OverallStatusText => GetOverallStatusText();

        /// <summary>
        /// کلاس CSS برای وضعیت
        /// </summary>
        public string OverallStatusBadgeClass => GetOverallStatusBadgeClass();

        /// <summary>
        /// آیا پزشک دارای انتسابات فعال است
        /// </summary>
        public bool HasActiveAssignments => TotalActiveAssignments > 0;

        /// <summary>
        /// آیا پزشک دارای تاریخچه است
        /// </summary>
        public bool HasHistory => History != null && History.Any();

        /// <summary>
        /// تعداد کل عملیات در تاریخچه
        /// </summary>
        public int TotalHistoryItems => History?.Count ?? 0;

        /// <summary>
        /// آخرین عملیات انجام شده
        /// </summary>
        public AssignmentHistoryViewModel LastAction => History?.OrderByDescending(h => h.ActionDate).FirstOrDefault();

        /// <summary>
        /// مدت زمان به صورت خوانا
        /// </summary>
        public string TotalAssignmentDaysText => GetTotalAssignmentDaysText();

        // Private Helper Methods

        private string GetOverallStatusText()
        {
            if (TotalActiveAssignments == 0)
                return "بدون انتساب";
            
            if (IsMultiDepartment)
                return "چند دپارتمانه";
            
            return "تک دپارتمانه";
        }

        private string GetOverallStatusBadgeClass()
        {
            if (TotalActiveAssignments == 0)
                return "bg-secondary";
            
            if (IsMultiDepartment)
                return "bg-warning";
            
            return "bg-success";
        }

        private string GetTotalAssignmentDaysText()
        {
            if (TotalAssignmentDays == 0)
                return "بدون انتساب";

            if (TotalAssignmentDays < 30)
                return $"{TotalAssignmentDays} روز";

            var months = TotalAssignmentDays / 30;
            var remainingDays = TotalAssignmentDays % 30;

            if (remainingDays == 0)
                return $"{months} ماه";

            return $"{months} ماه و {remainingDays} روز";
        }

        /// <summary>
        /// Factory method برای ایجاد ViewModel از Entity
        /// </summary>
        public static DoctorAssignmentDetailsViewModel FromEntity(
            Doctor doctor,
            List<DoctorDepartmentViewModel> departments,
            List<DoctorServiceCategoryViewModel> serviceCategories,
            List<AssignmentHistoryViewModel> history)
        {
            if (doctor == null)
                return null;

            var viewModel = new DoctorAssignmentDetailsViewModel
            {
                DoctorId = doctor.DoctorId,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                DoctorNationalCode = doctor.NationalCode,
                DoctorSpecialization = GetDoctorSpecializations(doctor),
                MedicalCouncilNumber = doctor.MedicalCouncilCode,
                LastUpdateTime = DateTime.Now,
                Departments = departments ?? new List<DoctorDepartmentViewModel>(),
                ServiceCategories = serviceCategories ?? new List<DoctorServiceCategoryViewModel>(),
                History = history ?? new List<AssignmentHistoryViewModel>()
            };

            // محاسبه آمار
            viewModel.TotalActiveAssignments = 
                (departments?.Count(d => d.IsActive) ?? 0) + 
                (serviceCategories?.Count(s => s.IsActive) ?? 0);

            viewModel.ActiveDepartmentCount = departments?.Count(d => d.IsActive) ?? 0;
            viewModel.ActiveServiceCategoryCount = serviceCategories?.Count(s => s.IsActive) ?? 0;
            viewModel.IsMultiDepartment = viewModel.ActiveDepartmentCount > 1;

            // محاسبه تاریخ‌ها
            var allAssignments = new List<DateTime>();
            
            if (departments != null)
            {
                allAssignments.AddRange(departments
                    .Where(d => d.CreatedAt != default(DateTime))
                    .Select(d => d.CreatedAt));
            }

            if (serviceCategories != null)
            {
                allAssignments.AddRange(serviceCategories
                    .Where(s => s.GrantedDate.HasValue)
                    .Select(s => s.GrantedDate.Value));
            }

            if (allAssignments.Any())
            {
                viewModel.FirstAssignmentDate = allAssignments.Min();
                viewModel.LastAssignmentDate = allAssignments.Max();
                
                if (viewModel.FirstAssignmentDate.HasValue && viewModel.LastAssignmentDate.HasValue)
                {
                    viewModel.TotalAssignmentDays = (int)(viewModel.LastAssignmentDate.Value - viewModel.FirstAssignmentDate.Value).TotalDays;
                }
            }

            return viewModel;
        }

        /// <summary>
        /// دریافت تخصص‌های پزشک از رابطه Many-to-Many
        /// </summary>
        private static string GetDoctorSpecializations(Doctor doctor)
        {
            if (doctor.DoctorSpecializations == null || !doctor.DoctorSpecializations.Any())
                return "ثبت نشده";

            var specializations = doctor.DoctorSpecializations
                .Where(ds => ds.Specialization != null)
                .Select(ds => ds.Specialization.Name)
                .ToList();

            return specializations.Any() ? string.Join("، ", specializations) : "ثبت نشده";
        }
    }
}
