using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل داده‌های رابطه پزشک-دپارتمان برای سیستم‌های پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش جزئیات رابطه چند-به-چند بین پزشک و دپارتمان
    /// 2. مدیریت نقش‌ها و مجوزهای پزشکان در هر دپارتمان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// </summary>
    public class DoctorDepartmentViewModel
    {
        /// <summary>
        /// شناسه انتصاب (ترکیبی از DoctorId و DepartmentId)
        /// </summary>
        public string AssignmentId => $"{DoctorId}_{DepartmentId}";

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        [Display(Name = "نام دپارتمان")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// شناسه کلینیک مربوط به دپارتمان
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک مربوط به دپارتمان
        /// </summary>
        [Display(Name = "نام کلینیک")]
        public string ClinicName { get; set; }

        /// <summary>
        /// نقش پزشک در این دپارتمان
        /// مثال: "رئیس دپارتمان"، "پزشک معاون"، "پزشک عادی"
        /// </summary>
        [MaxLength(100, ErrorMessage = "نقش نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نقش")]
        public string Role { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن پزشک در این دپارتمان
        /// </summary>
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ شروع فعالیت پزشک در این دپارتمان
        /// </summary>
        [Display(Name = "تاریخ شروع")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ شروع فعالیت پزشک در این دپارتمان به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ شروع (شمسی)")]
        public string StartDateShamsi { get; set; }

        /// <summary>
        /// تاریخ پایان فعالیت پزشک در این دپارتمان (در صورت وجود)
        /// </summary>
        [Display(Name = "تاریخ پایان")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// تاریخ پایان فعالیت پزشک در این دپارتمان به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ پایان (شمسی)")]
        public string EndDateShamsi { get; set; }

        /// <summary>
        /// تاریخ انتساب پزشک به دپارتمان (معادل CreatedAt)
        /// </summary>
        [Display(Name = "تاریخ انتساب")]
        public DateTime? AssignmentDate { get; set; }

        /// <summary>
        /// تاریخ انتساب پزشک به دپارتمان به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ انتساب (شمسی)")]
        public string AssignmentDateShamsi { get; set; }

        /// <summary>
        /// توضیحات انتساب
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "توضیحات")]
        public string Description { get; set; }

        #region فیلدهای ردیابی (Audit Trail)

        /// <summary>
        /// تاریخ ایجاد رابطه پزشک-دپارتمان
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد رابطه پزشک-دپارتمان به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش رابطه پزشک-دپارتمان
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش رابطه پزشک-دپارتمان به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorDepartmentViewModel FromEntity(DoctorDepartment doctorDepartment)
        {
            if (doctorDepartment == null) return null;
            return new DoctorDepartmentViewModel
            {
                DoctorId = doctorDepartment.DoctorId,
                DoctorName = doctorDepartment.Doctor?.FullName ?? $"{doctorDepartment.Doctor?.FirstName} {doctorDepartment.Doctor?.LastName}",
                DepartmentId = doctorDepartment.DepartmentId,
                DepartmentName = doctorDepartment.Department?.Name,
                ClinicId = doctorDepartment.Department?.ClinicId ?? 0,
                ClinicName = doctorDepartment.Department?.Clinic?.Name,
                Role = doctorDepartment.Role,
                IsActive = doctorDepartment.IsActive,
                StartDate = doctorDepartment.StartDate,
                StartDateShamsi = doctorDepartment.StartDate?.ToPersianDateTime(),
                EndDate = doctorDepartment.EndDate,
                EndDateShamsi = doctorDepartment.EndDate?.ToPersianDateTime(),
                CreatedAt = doctorDepartment.CreatedAt,
                CreatedBy = doctorDepartment.CreatedByUser?.FullName ?? doctorDepartment.CreatedByUserId,
                CreatedAtShamsi = doctorDepartment.CreatedAt.ToPersianDateTime(),
                UpdatedAt = doctorDepartment.UpdatedAt,
                UpdatedBy = doctorDepartment.UpdatedByUser?.FullName ?? doctorDepartment.UpdatedByUserId,
                UpdatedAtShamsi = doctorDepartment.UpdatedAt?.ToPersianDateTime()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public DoctorDepartment ToEntity()
        {
            return new DoctorDepartment
            {
                DoctorId = this.DoctorId,
                DepartmentId = this.DepartmentId,
                Role = this.Role,
                IsActive = this.IsActive,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل رابطه پزشک-دپارتمان
    /// </summary>
    public class DoctorDepartmentViewModelValidator : AbstractValidator<DoctorDepartmentViewModel>
    {
        public DoctorDepartmentViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("شناسه دپارتمان نامعتبر است.");

            RuleFor(x => x.Role)
                .MaximumLength(100)
                .WithMessage("نقش پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Role));

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد.");

            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");
        }
    }

    /// <summary>
    /// ViewModel برای صفحه نمایش انتسابات دپارتمان پزشک
    /// </summary>
    public class DoctorDepartmentAssignmentsViewModel
    {
        /// <summary>
        /// اطلاعات پزشک
        /// </summary>
        public DoctorDetailsViewModel Doctor { get; set; } = new DoctorDetailsViewModel();

        /// <summary>
        /// لیست انتسابات دپارتمان
        /// </summary>
        public List<DoctorDepartmentViewModel> Assignments { get; set; } = new List<DoctorDepartmentViewModel>();

        /// <summary>
        /// آمار انتسابات
        /// </summary>
        public DepartmentAssignmentStatsViewModel Stats { get; set; } = new DepartmentAssignmentStatsViewModel();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> AvailableDepartments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    }

    /// <summary>
    /// ViewModel برای آمار انتسابات دپارتمان
    /// </summary>
    public class DepartmentAssignmentStatsViewModel
    {
        /// <summary>
        /// تعداد کل انتسابات
        /// </summary>
        public int TotalAssignments { get; set; }

        /// <summary>
        /// تعداد انتسابات فعال
        /// </summary>
        public int ActiveAssignments { get; set; }

        /// <summary>
        /// تعداد انتسابات غیرفعال
        /// </summary>
        public int InactiveAssignments { get; set; }

        /// <summary>
        /// تاریخ آخرین انتساب
        /// </summary>
        public DateTime? LastAssignmentDate { get; set; }
    }

    /// <summary>
    /// ViewModel برای فرم انتساب پزشک به دپارتمان
    /// </summary>
    public class DoctorDepartmentAssignFormViewModel
    {
        /// <summary>
        /// اطلاعات پزشک
        /// </summary>
        public DoctorDetailsViewModel Doctor { get; set; } = new DoctorDetailsViewModel();

        /// <summary>
        /// مدل انتساب
        /// </summary>
        public DoctorAssignmentOperationViewModel Assignment { get; set; } = new DoctorAssignmentOperationViewModel();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();
    }

    /// <summary>
    /// ViewModel برای فرم انتقال پزشک بین دپارتمان‌ها
    /// </summary>
    public class DoctorDepartmentTransferFormViewModel
    {
        /// <summary>
        /// اطلاعات پزشک
        /// </summary>
        public DoctorDetailsViewModel Doctor { get; set; } = new DoctorDetailsViewModel();

        /// <summary>
        /// مدل انتقال
        /// </summary>
        public DoctorTransferViewModel Transfer { get; set; } = new DoctorTransferViewModel();

        /// <summary>
        /// لیست دپارتمان‌ها برای dropdown
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> Departments { get; set; } = new List<System.Web.Mvc.SelectListItem>();

        /// <summary>
        /// انتسابات فعلی پزشک
        /// </summary>
        public List<DoctorDepartmentViewModel> CurrentAssignments { get; set; } = new List<DoctorDepartmentViewModel>();
    }
}
