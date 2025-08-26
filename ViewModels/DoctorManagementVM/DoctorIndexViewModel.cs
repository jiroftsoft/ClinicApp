using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل داده‌های پزشک برای نمایش در لیست - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش اطلاعات کلیدی پزشکان برای تصمیم‌گیری سریع
    /// 2. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 3. رعایت استانداردهای امنیتی سیستم‌های پزشکی ایران
    /// 4. پشتیبانی از سیستم‌های Load Balanced و محیط‌های Production
    /// </summary>
    public class DoctorIndexViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// این شناسه برای عملیات‌های بعدی مانند ویرایش و حذف ضروری است
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی پزشک
        /// </summary>
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل پزشک (نام + نام خانوادگی)
        /// </summary>
        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تعداد دپارتمان‌هایی که پزشک در آن‌ها فعال است
        /// </summary>
        [Display(Name = "تعداد دپارتمان‌ها")]
        public int DepartmentCount { get; set; }

        /// <summary>
        /// تعداد سرفصل‌های خدماتی که پزشک می‌تواند ارائه دهد
        /// </summary>
        [Display(Name = "تعداد سرفصل‌های خدماتی")]
        public int ServiceCategoryCount { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن پزشک
        /// </summary>
        [Display(Name = "وضعیت")]
        public bool IsActive { get; set; }

        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن پزشک
        /// </summary>
        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ ایجاد پزشک
        /// </summary>
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        [Display(Name = "ایجاد شده توسط")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد پزشک به فرمت شمسی
        /// </summary>
        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorIndexViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;
            return new DoctorIndexViewModel
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                FullName = $"{doctor.FirstName} {doctor.LastName}",
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                DepartmentCount = doctor.DoctorDepartments?.Count(dd => 
                    dd.Department != null && 
                    !dd.Department.IsDeleted && 
                    dd.IsActive) ?? 0,
                ServiceCategoryCount = doctor.DoctorServiceCategories?.Count(dsc => 
                    dsc.ServiceCategory != null && 
                    !dsc.ServiceCategory.IsDeleted && 
                    dsc.IsActive) ?? 0,
                IsActive = doctor.IsActive,
                IsDeleted = doctor.IsDeleted,
                CreatedAt = doctor.CreatedAt,
                CreatedBy = doctor.CreatedByUser?.FullName ?? doctor.CreatedByUserId,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDateTime()
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل نمایش لیست پزشکان
    /// </summary>
    public class DoctorIndexViewModelValidator : AbstractValidator<DoctorIndexViewModel>
    {
        public DoctorIndexViewModelValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("شناسه پزشک نامعتبر است.");

            RuleFor(x => x.FullName)
                .NotEmpty()
                .WithMessage("نام کامل پزشک نمی‌تواند خالی باشد.");
                
            RuleFor(x => x.DepartmentCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد دپارتمان‌ها نمی‌تواند منفی باشد.");
                
            RuleFor(x => x.ServiceCategoryCount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("تعداد سرفصل‌های خدماتی نمی‌تواند منفی باشد.");
        }
    }
}
