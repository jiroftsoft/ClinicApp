using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using FluentValidation;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    /// <summary>
    /// مدل داده‌های پزشک برای ایجاد و ویرایش - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از ساختار سازمانی پزشکی (کلینیک → دپارتمان → پزشک)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// </summary>
    public class DoctorCreateEditViewModel
    {
        /// <summary>
        /// شناسه پزشک
        /// در حالت ایجاد جدید، این مقدار صفر است
        /// </summary>
        public int DoctorId { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است.")]
        [StringLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی پزشک
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
        [StringLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن پزشک
        /// </summary>
        [Display(Name = "فعال است")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تخصص پزشک
        /// </summary>
        [StringLength(250, ErrorMessage = "تخصص نمی‌تواند بیش از 250 کاراکتر باشد.")]
        [Display(Name = "تخصص")]
        public string Specialization { get; set; }

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// بیوگرافی یا توضیحات پزشک
        /// </summary>
        [StringLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        [Display(Name = "بیوگرافی")]
        public string Bio { get; set; }

        #region فیلدهای ردیابی (Audit Trail)

        /// <summary>
        /// تاریخ و زمان ایجاد پزشک
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// نام کاربر ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش پزشک
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نام کاربر آخرین ویرایش کننده
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد پزشک به فرمت شمسی
        /// </summary>
        public string CreatedAtShamsi { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش پزشک به فرمت شمسی
        /// </summary>
        public string UpdatedAtShamsi { get; set; }

        #endregion

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی یک Entity می‌سازد.
        /// </summary>
        public static DoctorCreateEditViewModel FromEntity(Doctor doctor)
        {
            if (doctor == null) return null;
            return new DoctorCreateEditViewModel
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                IsActive = doctor.IsActive,
                Specialization = doctor.Specialization,
                PhoneNumber = doctor.PhoneNumber,
                Bio = doctor.Bio,
                CreatedAt = doctor.CreatedAt,
                CreatedBy = doctor.CreatedByUser?.FullName ?? doctor.CreatedByUserId,
                UpdatedAt = doctor.UpdatedAt,
                UpdatedBy = doctor.UpdatedByUser?.FullName ?? doctor.UpdatedByUserId,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDateTime()
            };
        }

        /// <summary>
        /// ✅ تبدیل ViewModel به Entity برای ذخیره در دیتابیس
        /// </summary>
        public Doctor ToEntity()
        {
            return new Doctor
            {
                DoctorId = this.DoctorId,
                FirstName = this.FirstName,
                LastName = this.LastName,
                IsActive = this.IsActive,
                Specialization = this.Specialization,
                PhoneNumber = this.PhoneNumber,
                Bio = this.Bio,
                CreatedAt = this.CreatedAt,
                CreatedByUserId = this.CreatedBy,
                UpdatedAt = this.UpdatedAt,
                UpdatedByUserId = this.UpdatedBy
            };
        }
    }

    /// <summary>
    /// ولیدیتور برای مدل ایجاد و ویرایش پزشک
    /// </summary>
    public class DoctorCreateEditViewModelValidator : AbstractValidator<DoctorCreateEditViewModel>
    {
        public DoctorCreateEditViewModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("نام پزشک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام پزشک باید به فارسی وارد شود.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("نام خانوادگی پزشک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام خانوادگی پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام خانوادگی پزشک باید به فارسی وارد شود.");

            RuleFor(x => x.Specialization)
                .MaximumLength(250)
                .WithMessage("تخصص پزشک نمی‌تواند بیش از 250 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50)
                .WithMessage("شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")
                .Must(PersianNumberHelper.IsValidPhoneNumber)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("شماره تلفن نامعتبر است.");

            RuleFor(x => x.Bio)
                .MaximumLength(2000)
                .WithMessage("بیوگرافی پزشک نمی‌تواند بیش از 2000 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Bio));
        }
    }
}
