using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Models;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل جزئیات پزشک برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ارائه جزئیات کامل پزشک شامل اطلاعات پایه، آماری و تاریخچه
    /// 2. نمایش اطلاعات مربوط به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// </summary>
    public class DoctorDetailsViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی پزشک
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل پزشک
        /// </summary>
        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن پزشک
        /// </summary>
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; }

        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن پزشک
        /// </summary>
        [Display(Name = "حذف شده است")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تخصص‌های پزشک
        /// </summary>
        [Display(Name = "تخصص‌ها")]
        public List<string> SpecializationNames { get; set; } = new List<string>();

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        [MaxLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// بیوگرافی یا توضیحات پزشک
        /// </summary>
        [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "بیوگرافی")]
        public string Bio { get; set; }

        #region فیلدهای ردیابی (Audit Trail)

        /// <summary>
        /// تاریخ و زمان ایجاد پزشک
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ و زمان ایجاد پزشک به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش پزشک
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش پزشک به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف پزشک
        /// </summary>
        [Display(Name = "تاریخ حذف")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف پزشک به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ حذف (شمسی)")]
        public string DeletedAtShamsi { get; set; }

        /// <summary>
        /// نام کاربر حذف کننده
        /// </summary>
        [Display(Name = "حذف شده توسط")]
        public string DeletedBy { get; set; }

        #endregion

        #region اطلاعات مربوط به دپارتمان‌ها

        /// <summary>
        /// تعداد دپارتمان‌هایی که پزشک در آن‌ها فعال است
        /// </summary>
        [Display(Name = "تعداد دپارتمان‌ها")]
        public int DepartmentCount { get; set; }

        /// <summary>
        /// لیست ارتباطات پزشک با دپارتمان‌ها
        /// </summary>
        [Display(Name = "دپارتمان‌ها")]
        public List<DoctorDepartmentViewModel> DoctorDepartments { get; set; } = new List<DoctorDepartmentViewModel>();

        #endregion

        #region اطلاعات مربوط به سرفصل‌های خدماتی

        /// <summary>
        /// تعداد سرفصل‌های خدماتی که پزشک می‌تواند ارائه دهد
        /// </summary>
        [Display(Name = "تعداد سرفصل‌های خدماتی")]
        public int ServiceCategoryCount { get; set; }

        /// <summary>
        /// لیست ارتباطات پزشک با سرفصل‌های خدماتی
        /// </summary>
        [Display(Name = "سرفصل‌های خدماتی")]
        public List<DoctorServiceCategoryViewModel> DoctorServiceCategories { get; set; } = new List<DoctorServiceCategoryViewModel>();

        #endregion

        #region اطلاعات مربوط به برنامه کاری

        /// <summary>
        /// برنامه کاری هفتگی پزشک
        /// </summary>
        [Display(Name = "برنامه کاری")]
        public DoctorScheduleViewModel Schedule { get; set; }

        #endregion

        /// <summary>
        /// شناسه پزشک (برای استفاده در view)
        /// </summary>
        public int Id => DoctorId;

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// شماره نظام پزشکی
        /// </summary>
        [Display(Name = "شماره نظام پزشکی")]
        public string MedicalCouncilCode { get; set; }

        /// <summary>
        /// آدرس ایمیل پزشک
        /// </summary>
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل پزشک
        /// </summary>
        [Display(Name = "تصویر پروفایل")]
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// آدرس منزل پزشک
        /// </summary>
        [Display(Name = "آدرس منزل")]
        public string Address { get; set; }

        /// <summary>
        /// شماره تماس اضطراری
        /// </summary>
        [Display(Name = "تماس اضطراری")]
        public string EmergencyContact { get; set; }

        /// <summary>
        /// شماره پروانه پزشکی
        /// </summary>
        [Display(Name = "شماره پروانه")]
        public string LicenseNumber { get; set; }

        /// <summary>
        /// اطلاعات تحصیلی پزشک
        /// </summary>
        [Display(Name = "تحصیلات")]
        public string Education { get; set; }

        /// <summary>
        /// سطح امنیتی پزشک
        /// </summary>
        [Display(Name = "سطح امنیتی")]
        public string SecurityLevel { get; set; }

        /// <summary>
        /// بیوگرافی پزشک
        /// </summary>
        [Display(Name = "بیوگرافی")]
        public string Biography { get; set; }

        /// <summary>
        /// تعداد انتسابات فعال
        /// </summary>
        public int ActiveAssignmentsCount { get; set; }

        /// <summary>
        /// اطلاعات وابستگی‌ها
        /// </summary>
        public DoctorDependencyInfo Dependencies { get; set; }

        /// <summary>
        /// انتسابات دپارتمان
        /// </summary>
        public List<LookupItemViewModel> DepartmentAssignments { get; set; } = new List<LookupItemViewModel>();

        /// <summary>
        /// انتسابات سرفصل‌های خدماتی
        /// </summary>
        public List<LookupItemViewModel> ServiceCategoryAssignments { get; set; } = new List<LookupItemViewModel>();

        /// <summary>
        /// تعداد کل نوبت‌های پزشک
        /// </summary>
        [Display(Name = "کل نوبت‌ها")]
        public int TotalAppointments { get; set; }

        /// <summary>
        /// تعداد نوبت‌های امروز پزشک
        /// </summary>
        [Display(Name = "نوبت‌های امروز")]
        public int TodayAppointments { get; set; }

        /// <summary>
        /// دپارتمان‌ها (برای backward compatibility)
        /// </summary>
        public List<DoctorDepartmentViewModel> Departments => DoctorDepartments;

        /// <summary>
        /// دسته‌بندی‌های خدماتی (برای backward compatibility)
        /// </summary>
        public List<DoctorServiceCategoryViewModel> ServiceCategories => DoctorServiceCategories;

        public int? ExperienceYears { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorDetailsViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;
            
            var viewModel = new DoctorDetailsViewModel
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                FullName = $"{doctor.FirstName} {doctor.LastName}",
                NationalCode = doctor.NationalCode,
                MedicalCouncilCode = doctor.MedicalCouncilCode,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                ProfileImageUrl = doctor.ProfileImageUrl,
                Address = doctor.HomeAddress,
                EmergencyContact = doctor.EmergencyContact,
                LicenseNumber = doctor.LicenseNumber,
                Education = $"{doctor.Degree} - {doctor.University} ({doctor.GraduationYear})",
                SecurityLevel = "متوسط", // این فیلد نیاز به پیاده‌سازی دارد
                Biography = doctor.Bio,
                SpecializationNames = doctor.DoctorSpecializations?.Where(ds => ds.Specialization != null).Select(ds => ds.Specialization.Name).ToList() ?? new List<string>(),
                IsActive = doctor.IsActive,
                IsDeleted = doctor.IsDeleted,
                CreatedAt = doctor.CreatedAt,
                CreatedBy = doctor.CreatedByUser?.FullName ?? GetUserDisplayName(doctor.CreatedByUserId),
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDateTime(),
                UpdatedAt = doctor.UpdatedAt,
                ExperienceYears = doctor.ExperienceYears,
                UpdatedBy = doctor.UpdatedByUser?.FullName ?? GetUserDisplayName(doctor.UpdatedByUserId),
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDateTime(),
                DeletedAt = doctor.DeletedAt,
                DeletedBy = doctor.DeletedByUser?.FullName ?? GetUserDisplayName(doctor.DeletedByUserId),
                DeletedAtShamsi = doctor.DeletedAt?.ToPersianDateTime(),
                DepartmentCount = doctor.DoctorDepartments?.Count(dd => 
                    dd.Department != null && 
                    !dd.Department.IsDeleted && 
                    dd.IsActive) ?? 0,
                ServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => 
                    dsc.ServiceCategory != null && 
                    !dsc.ServiceCategory.IsDeleted && 
                    dsc.IsActive) ?? 0,
                TotalAppointments = 0, // این فیلد نیاز به پیاده‌سازی دارد
                TodayAppointments = 0 // این فیلد نیاز به پیاده‌سازی دارد
            };
            
            // پر کردن لیست دپارتمان‌ها
            if (doctor.DoctorDepartments != null)
            {
                viewModel.DoctorDepartments = doctor.DoctorDepartments
                    .Where(dd => dd.Department != null && !dd.Department.IsDeleted)
                    .Select(DoctorDepartmentViewModel.FromEntity)
                    .ToList();
            }
            
            // پر کردن لیست سرفصل‌های خدماتی
            if (doctor.DoctorServiceCategories != null)
            {
                viewModel.DoctorServiceCategories = doctor.DoctorServiceCategories
                    .Where(dsc => dsc.ServiceCategory != null && !dsc.ServiceCategory.IsDeleted)
                    .Select(DoctorServiceCategoryViewModel.FromEntity)
                    .ToList();
            }
            
            return viewModel;
        }

        /// <summary>
        /// تبدیل شناسه کاربر به نام نمایشی
        /// </summary>
        private static string GetUserDisplayName(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return "سیستم";

            // شناسه‌های شناخته شده برای کاربران سیستم
            switch (userId.ToLower())
            {
                case "51a37cb8-efbf-420a-8dfd-d17e8a1c8e50":
                    return "مدیر سیستم";
                case "76549c38-562c-40ee-8a86-d8fb07ad50de":
                    return "کاربر ادمین";
                case "3020347998":
                    return "مدیر سیستم";
                case "3031945451":
                    return "سیستم";
                default:
                    // اگر شناسه GUID باشد، نام کاربری را برگردان
                    if (userId.Length > 20)
                        return "کاربر سیستم";
                    else
                        return userId;
            }
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل جزئیات پزشک
    /// </summary>
    public class DoctorDetailsViewModelValidator : AbstractValidator<DoctorDetailsViewModel>
    {
        public DoctorDetailsViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");
                
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("نام پزشک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام پزشک نمی‌تواند بیش از 100 کاراکتر باشد.");
                
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("نام خانوادگی پزشک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام خانوادگی پزشک نمی‌تواند بیش از 100 کاراکتر باشد.");
                
            RuleFor(x => x.SpecializationNames)
                .NotEmpty()
                .WithMessage("تخصص‌های پزشک الزامی است.");
                
            RuleFor(x => x.PhoneNumber)
                .Must(PersianNumberHelper.IsValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("شماره تلفن نامعتبر است.");
                
            RuleFor(x => x.Bio)
                .MaximumLength(2000)
                .WithMessage("بیوگرافی پزشک نمی‌تواند بیش از 2000 کاراکتر باشد.");
        }
    }
}
