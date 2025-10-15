using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش cascade loading در فرم پذیرش
    /// </summary>
    public class ReceptionCascadeViewModel
    {
        /// <summary>
        /// شناسه کلینیک انتخاب شده
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// شناسه دپارتمان انتخاب شده
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// شناسه پزشک انتخاب شده
        /// </summary>
        public int? DoctorId { get; set; }

        /// <summary>
        /// کلینیک انتخاب شده
        /// </summary>
        public ReceptionClinicViewModel Clinic { get; set; }

        /// <summary>
        /// لیست دپارتمان‌های کلینیک
        /// </summary>
        public List<ReceptionDepartmentViewModel> Departments { get; set; } = new List<ReceptionDepartmentViewModel>();

        /// <summary>
        /// لیست پزشکان دپارتمان
        /// </summary>
        public List<ReceptionDoctorViewModel> Doctors { get; set; } = new List<ReceptionDoctorViewModel>();

        /// <summary>
        /// پزشک انتخاب شده
        /// </summary>
        public ReceptionDoctorViewModel SelectedDoctor { get; set; }

        /// <summary>
        /// تاریخ بارگذاری
        /// </summary>
        public DateTime LoadDate { get; set; }

        /// <summary>
        /// آیا کلینیک انتخاب شده؟
        /// </summary>
        public bool IsClinicSelected => ClinicId > 0;

        /// <summary>
        /// آیا دپارتمان انتخاب شده؟
        /// </summary>
        public bool IsDepartmentSelected => DepartmentId.HasValue && DepartmentId.Value > 0;

        /// <summary>
        /// آیا پزشک انتخاب شده؟
        /// </summary>
        public bool IsDoctorSelected => DoctorId.HasValue && DoctorId.Value > 0;

        /// <summary>
        /// تعداد دپارتمان‌ها
        /// </summary>
        public int DepartmentCount => Departments?.Count ?? 0;

        /// <summary>
        /// تعداد پزشکان
        /// </summary>
        public int DoctorCount => Doctors?.Count ?? 0;

        /// <summary>
        /// نمایش وضعیت انتخاب
        /// </summary>
        public string SelectionStatus
        {
            get
            {
                if (!IsClinicSelected) return "کلینیک انتخاب نشده";
                if (!IsDepartmentSelected) return "دپارتمان انتخاب نشده";
                if (!IsDoctorSelected) return "پزشک انتخاب نشده";
                return "انتخاب کامل";
            }
        }

        /// <summary>
        /// نمایش اطلاعات cascade (فرمات شده)
        /// </summary>
        public string CascadeInfoDisplay
        {
            get
            {
                var parts = new List<string>();
                
                if (Clinic != null) parts.Add($"کلینیک: {Clinic.ClinicName}");
                if (IsDepartmentSelected && Departments.Any(d => d.DepartmentId == DepartmentId))
                {
                    var department = Departments.First(d => d.DepartmentId == DepartmentId);
                    parts.Add($"دپارتمان: {department.DepartmentName}");
                }
                if (IsDoctorSelected && SelectedDoctor != null)
                {
                    parts.Add($"پزشک: {SelectedDoctor.FullName}");
                }
                
                return string.Join(" | ", parts);
            }
        }

        /// <summary>
        /// آیا cascade کامل است؟
        /// </summary>
        public bool IsComplete => IsClinicSelected && IsDepartmentSelected && IsDoctorSelected;

        /// <summary>
        /// درصد تکمیل cascade
        /// </summary>
        public int CompletionPercentage
        {
            get
            {
                var steps = 0;
                if (IsClinicSelected) steps++;
                if (IsDepartmentSelected) steps++;
                if (IsDoctorSelected) steps++;
                return (steps * 100) / 3;
            }
        }
    }
}
