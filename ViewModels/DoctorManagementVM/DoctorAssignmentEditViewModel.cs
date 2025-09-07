using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Extensions;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل ویرایش انتسابات پزشک - طراحی شده برای فرم ویرایش حرفه‌ای
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش کامل اطلاعات پزشک و انتسابات موجود
    /// 2. امکان ویرایش انتسابات دپارتمان و سرفصل‌های خدماتی
    /// 3. مدیریت تغییرات با ردیابی کامل
    /// 4. اعتبارسنجی جامع برای محیط پزشکی
    /// </summary>
    public class DoctorAssignmentEditViewModel
    {
        #region اطلاعات پزشک

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است")]
        [Range(1, int.MaxValue, ErrorMessage = "شناسه پزشک نامعتبر است")]
        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        [Display(Name = "نام کامل")]
        public string DoctorName { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        [Display(Name = "کد ملی")]
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
        /// وضعیت فعال بودن پزشک
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ ثبت پزشک
        /// </summary>
        [Display(Name = "تاریخ ثبت")]
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region آمار انتسابات

        /// <summary>
        /// تعداد کل انتسابات فعال
        /// </summary>
        [Display(Name = "کل انتسابات فعال")]
        public int TotalActiveAssignments { get; set; }

        /// <summary>
        /// تعداد دپارتمان‌های فعال
        /// </summary>
        [Display(Name = "دپارتمان‌های فعال")]
        public int ActiveDepartmentCount { get; set; }

        /// <summary>
        /// تعداد سرفصل‌های خدماتی فعال
        /// </summary>
        [Display(Name = "سرفصل‌های خدماتی فعال")]
        public int ActiveServiceCategoryCount { get; set; }

        /// <summary>
        /// آیا پزشک در چند دپارتمان فعال است
        /// </summary>
        [Display(Name = "چند دپارتمانه")]
        public bool IsMultiDepartment { get; set; }

        #endregion

        #region انتسابات دپارتمان

        /// <summary>
        /// لیست انتسابات دپارتمان موجود
        /// </summary>
        [Display(Name = "انتسابات دپارتمان")]
        public List<DoctorDepartmentEditViewModel> DepartmentAssignments { get; set; } = new List<DoctorDepartmentEditViewModel>();

        /// <summary>
        /// لیست دپارتمان‌های موجود برای انتخاب
        /// </summary>
        [Display(Name = "دپارتمان‌های موجود")]
        public List<SelectListItem> AvailableDepartments { get; set; } = new List<SelectListItem>();

        #endregion

        #region انتسابات سرفصل خدماتی

        /// <summary>
        /// لیست انتسابات سرفصل‌های خدماتی موجود
        /// </summary>
        [Display(Name = "انتسابات سرفصل‌های خدماتی")]
        public List<DoctorServiceCategoryEditViewModel> ServiceCategoryAssignments { get; set; } = new List<DoctorServiceCategoryEditViewModel>();

        /// <summary>
        /// لیست سرفصل‌های خدماتی موجود برای انتخاب
        /// </summary>
        [Display(Name = "سرفصل‌های خدماتی موجود")]
        public List<SelectListItem> AvailableServiceCategories { get; set; } = new List<SelectListItem>();

        #endregion

        #region تغییرات جدید

        /// <summary>
        /// دپارتمان‌های جدید برای اضافه کردن
        /// </summary>
        [Display(Name = "دپارتمان‌های جدید")]
        public List<int> NewDepartmentIds { get; set; } = new List<int>();

        /// <summary>
        /// سرفصل‌های خدماتی جدید برای اضافه کردن
        /// </summary>
        [Display(Name = "سرفصل‌های خدماتی جدید")]
        public List<int> NewServiceCategoryIds { get; set; } = new List<int>();

        /// <summary>
        /// انتساباتی که باید حذف شوند
        /// </summary>
        [Display(Name = "انتسابات برای حذف")]
        public List<int> AssignmentsToRemove { get; set; } = new List<int>();

        #endregion

        #region یادداشت‌ها و توضیحات

        /// <summary>
        /// یادداشت‌های ویرایش
        /// </summary>
        [MaxLength(1000, ErrorMessage = "یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد")]
        [Display(Name = "یادداشت‌های ویرایش")]
        public string EditNotes { get; set; }

        /// <summary>
        /// دلیل تغییرات
        /// </summary>
        [MaxLength(500, ErrorMessage = "دلیل تغییرات نمی‌تواند بیش از 500 کاراکتر باشد")]
        [Display(Name = "دلیل تغییرات")]
        public string ChangeReason { get; set; }

        /// <summary>
        /// آیا تغییرات فوری اعمال شوند
        /// </summary>
        [Display(Name = "اعمال فوری")]
        public bool ApplyImmediately { get; set; } = true;

        /// <summary>
        /// تاریخ اعمال تغییرات
        /// </summary>
        [Display(Name = "تاریخ اعمال")]
        public DateTime? EffectiveDate { get; set; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// ایجاد ViewModel از DoctorAssignmentDetailsViewModel
        /// </summary>
        public static DoctorAssignmentEditViewModel FromDetailsViewModel(DoctorAssignmentDetailsViewModel details)
        {
            if (details == null) return null;

            return new DoctorAssignmentEditViewModel
            {
                DoctorId = details.DoctorId,
                DoctorName = details.DoctorName,
                DoctorNationalCode = details.DoctorNationalCode,
                DoctorSpecialization = details.DoctorSpecialization,
                MedicalCouncilNumber = details.MedicalCouncilNumber,
                IsActive = details.IsActive,
                RegistrationDate = details.RegistrationDate,
                LastModifiedDate = details.LastModifiedDate,
                TotalActiveAssignments = details.TotalActiveAssignments,
                ActiveDepartmentCount = details.ActiveDepartmentCount,
                ActiveServiceCategoryCount = details.ActiveServiceCategoryCount,
                IsMultiDepartment = details.IsMultiDepartment,
                DepartmentAssignments = details.DepartmentAssignments?.Select(d => new DoctorDepartmentEditViewModel
                {
                    Id = d.Id,
                    DepartmentId = d.DepartmentId,
                    DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode,
                    Role = d.Role,
                    IsActive = d.IsActive,
                    AssignmentDate = d.AssignmentDate,
                    CreatedAt = d.CreatedAt
                }).ToList() ?? new List<DoctorDepartmentEditViewModel>(),
                ServiceCategoryAssignments = details.ServiceCategoryAssignments?.Select(s => new DoctorServiceCategoryEditViewModel
                {
                    Id = s.Id,
                    ServiceCategoryId = s.ServiceCategoryId,
                    ServiceCategoryName = s.ServiceCategoryName ?? s.ServiceCategoryTitle ?? "نامشخص",
                    ServiceCategoryCode = s.ServiceCategoryCode,
                    AuthorizationLevel = s.AuthorizationLevel,
                    IsActive = s.IsActive,
                    GrantedDate = s.GrantedDate,
                    CertificateNumber = s.CertificateNumber
                }).ToList() ?? new List<DoctorServiceCategoryEditViewModel>()
            };
        }

        #endregion
    }

    /// <summary>
    /// مدل ویرایش انتساب دپارتمان
    /// </summary>
    public class DoctorDepartmentEditViewModel
    {
        /// <summary>
        /// شناسه انتساب
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه دپارتمان
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// کد دپارتمان
        /// </summary>
        public string DepartmentCode { get; set; }

        /// <summary>
        /// نقش در دپارتمان
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ انتساب
        /// </summary>
        public DateTime? AssignmentDate { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آیا برای حذف انتخاب شده
        /// </summary>
        public bool IsSelectedForRemoval { get; set; }

        /// <summary>
        /// یادداشت‌های ویرایش
        /// </summary>
        public string EditNotes { get; set; }
    }

    /// <summary>
    /// مدل ویرایش انتساب سرفصل خدماتی
    /// </summary>
    public class DoctorServiceCategoryEditViewModel
    {
        /// <summary>
        /// شناسه انتساب
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه سرفصل خدماتی
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// نام سرفصل خدماتی
        /// </summary>
        public string ServiceCategoryName { get; set; }

        /// <summary>
        /// کد سرفصل خدماتی
        /// </summary>
        public string ServiceCategoryCode { get; set; }

        /// <summary>
        /// سطح دسترسی
        /// </summary>
        public string AuthorizationLevel { get; set; }

        /// <summary>
        /// وضعیت فعال
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ اعطا
        /// </summary>
        public DateTime? GrantedDate { get; set; }

        /// <summary>
        /// شماره گواهی
        /// </summary>
        public string CertificateNumber { get; set; }

        /// <summary>
        /// آیا برای حذف انتخاب شده
        /// </summary>
        public bool IsSelectedForRemoval { get; set; }

        /// <summary>
        /// یادداشت‌های ویرایش
        /// </summary>
        public string EditNotes { get; set; }
    }

    /// <summary>
    /// ولیدیتور برای مدل ویرایش انتسابات
    /// </summary>
    public class DoctorAssignmentEditViewModelValidator : AbstractValidator<DoctorAssignmentEditViewModel>
    {
        public DoctorAssignmentEditViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است");

            RuleFor(x => x.EditNotes)
                .MaximumLength(1000)
                .WithMessage("یادداشت‌ها نمی‌تواند بیش از 1000 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.EditNotes));

            RuleFor(x => x.ChangeReason)
                .MaximumLength(500)
                .WithMessage("دلیل تغییرات نمی‌تواند بیش از 500 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.ChangeReason));

            RuleFor(x => x.EffectiveDate)
                .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("تاریخ اعمال نمی‌تواند در گذشته باشد")
                .When(x => x.EffectiveDate.HasValue);

            // اعتبارسنجی حداقل یک تغییر
            RuleFor(x => x)
                .Must(HaveAtLeastOneChange)
                .WithMessage("لطفاً حداقل یک تغییر انتخاب کنید");

            // اعتبارسنجی دپارتمان‌های جدید
            RuleFor(x => x.NewDepartmentIds)
                .Must(BeValidDepartmentIds)
                .WithMessage("دپارتمان‌های انتخاب شده نامعتبر هستند")
                .When(x => x.NewDepartmentIds != null && x.NewDepartmentIds.Any());

            // اعتبارسنجی سرفصل‌های خدماتی جدید
            RuleFor(x => x.NewServiceCategoryIds)
                .Must(BeValidServiceCategoryIds)
                .WithMessage("سرفصل‌های خدماتی انتخاب شده نامعتبر هستند")
                .When(x => x.NewServiceCategoryIds != null && x.NewServiceCategoryIds.Any());

            // اعتبارسنجی انتسابات برای حذف
            RuleFor(x => x.AssignmentsToRemove)
                .Must(BeValidAssignmentIds)
                .WithMessage("انتسابات انتخاب شده برای حذف نامعتبر هستند")
                .When(x => x.AssignmentsToRemove != null && x.AssignmentsToRemove.Any());
        }

        /// <summary>
        /// بررسی وجود حداقل یک تغییر
        /// </summary>
        private bool HaveAtLeastOneChange(DoctorAssignmentEditViewModel model)
        {
            var hasNewDepartments = model.NewDepartmentIds?.Any() == true;
            var hasNewServiceCategories = model.NewServiceCategoryIds?.Any() == true;
            var hasRemovals = model.AssignmentsToRemove?.Any() == true;

            return hasNewDepartments || hasNewServiceCategories || hasRemovals;
        }

        /// <summary>
        /// اعتبارسنجی شناسه‌های دپارتمان
        /// </summary>
        private bool BeValidDepartmentIds(List<int> departmentIds)
        {
            return departmentIds?.All(id => id > 0) == true;
        }

        /// <summary>
        /// اعتبارسنجی شناسه‌های سرفصل‌های خدماتی
        /// </summary>
        private bool BeValidServiceCategoryIds(List<int> serviceCategoryIds)
        {
            return serviceCategoryIds?.All(id => id > 0) == true;
        }

        /// <summary>
        /// اعتبارسنجی شناسه‌های انتسابات برای حذف
        /// </summary>
        private bool BeValidAssignmentIds(List<int> assignmentIds)
        {
            return assignmentIds?.All(id => id > 0) == true;
        }
    }
}
