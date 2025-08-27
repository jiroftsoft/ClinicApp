using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        /// مدرک تحصیلی پزشک
        /// </summary>
        [Required(ErrorMessage = "مدرک تحصیلی الزامی است.")]
        [Display(Name = "مدرک تحصیلی")]
        public Degree Degree { get; set; }

        /// <summary>
        /// سال فارغ‌التحصیلی
        /// </summary>
        [Range(1350, 1410, ErrorMessage = "سال فارغ‌التحصیلی باید بین 1350 تا 1410 باشد.")]
        [Display(Name = "سال فارغ‌التحصیلی")]
        public int? GraduationYear { get; set; }

        /// <summary>
        /// دانشگاه محل تحصیل
        /// </summary>
        [StringLength(200, ErrorMessage = "نام دانشگاه نمی‌تواند بیش از 200 کاراکتر باشد.")]
        [Display(Name = "دانشگاه")]
        public string University { get; set; }

        /// <summary>
        /// جنسیت پزشک
        /// </summary>
        [Required(ErrorMessage = "جنسیت الزامی است.")]
        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        /// <summary>
        /// تاریخ تولد پزشک
        /// </summary>
        [Display(Name = "تاریخ تولد")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// آدرس منزل پزشک
        /// </summary>
        [StringLength(500, ErrorMessage = "آدرس منزل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس منزل")]
        public string HomeAddress { get; set; }

        /// <summary>
        /// آدرس مطب/کلینیک پزشک
        /// </summary>
        [StringLength(500, ErrorMessage = "آدرس مطب نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس مطب")]
        public string OfficeAddress { get; set; }

        /// <summary>
        /// تعرفه ویزیت پزشک (به تومان)
        /// </summary>
        [Range(0, 10000000, ErrorMessage = "تعرفه ویزیت باید بین 0 تا 10,000,000 تومان باشد.")]
        [Display(Name = "تعرفه ویزیت (تومان)")]
        public decimal? ConsultationFee { get; set; }

        /// <summary>
        /// سابقه کاری پزشک (به سال)
        /// </summary>
        [Range(0, 50, ErrorMessage = "سابقه کاری باید بین 0 تا 50 سال باشد.")]
        [Display(Name = "سابقه کاری (سال)")]
        public int? ExperienceYears { get; set; }

        /// <summary>
        /// عکس پروفایل پزشک
        /// </summary>
        [StringLength(500, ErrorMessage = "آدرس عکس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "عکس پروفایل")]
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// لیست تخصص‌های انتخاب شده
        /// </summary>
        [Display(Name = "تخصص‌ها")]
        public List<int> SelectedSpecializationIds { get; set; } = new List<int>();

        /// <summary>
        /// شماره تلفن پزشک
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// کد ملی پزشک
        /// </summary>
        [StringLength(10, ErrorMessage = "کد ملی نمی‌تواند بیش از 10 کاراکتر باشد.")]
        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        /// <summary>
        /// کد نظام پزشکی
        /// </summary>
        [StringLength(20, ErrorMessage = "کد نظام پزشکی نمی‌تواند بیش از 20 کاراکتر باشد.")]
        [Display(Name = "کد نظام پزشکی")]
        public string MedicalCouncilCode { get; set; }

        /// <summary>
        /// ایمیل پزشک
        /// </summary>
        [StringLength(100, ErrorMessage = "ایمیل نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

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
                Degree = doctor.Degree,
                GraduationYear = doctor.GraduationYear,
                University = doctor.University,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                HomeAddress = doctor.HomeAddress,
                OfficeAddress = doctor.OfficeAddress,
                ConsultationFee = doctor.ConsultationFee,
                ExperienceYears = doctor.ExperienceYears,
                ProfileImageUrl = doctor.ProfileImageUrl,
                PhoneNumber = doctor.PhoneNumber,
                NationalCode = doctor.NationalCode,
                MedicalCouncilCode = doctor.MedicalCouncilCode,
                Email = doctor.Email,
                Bio = doctor.Bio,
                CreatedAt = doctor.CreatedAt,
                CreatedBy = doctor.CreatedByUser?.FullName ?? doctor.CreatedByUserId,
                UpdatedAt = doctor.UpdatedAt,
                UpdatedBy = doctor.UpdatedByUser?.FullName ?? doctor.UpdatedByUserId,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDateTime(),
                SelectedSpecializationIds = doctor.Specializations?.Select(s => s.SpecializationId).ToList() ?? new List<int>()
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
                Degree = this.Degree,
                GraduationYear = this.GraduationYear,
                University = this.University,
                Gender = this.Gender,
                DateOfBirth = this.DateOfBirth,
                HomeAddress = this.HomeAddress,
                OfficeAddress = this.OfficeAddress,
                ConsultationFee = this.ConsultationFee,
                ExperienceYears = this.ExperienceYears,
                ProfileImageUrl = this.ProfileImageUrl,
                PhoneNumber = this.PhoneNumber,
                NationalCode = this.NationalCode,
                MedicalCouncilCode = this.MedicalCouncilCode,
                Email = this.Email,
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

            RuleFor(x => x.Degree)
                .IsInEnum()
                .WithMessage("مدرک تحصیلی نامعتبر است.");

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(1350, 1410)
                .When(x => x.GraduationYear.HasValue)
                .WithMessage("سال فارغ‌التحصیلی باید بین 1350 تا 1410 باشد.");

            RuleFor(x => x.University)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.University))
                .WithMessage("نام دانشگاه نمی‌تواند بیش از 200 کاراکتر باشد.");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("جنسیت نامعتبر است.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Now)
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.");

            RuleFor(x => x.HomeAddress)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.HomeAddress))
                .WithMessage("آدرس منزل نمی‌تواند بیش از 500 کاراکتر باشد.");

            RuleFor(x => x.OfficeAddress)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.OfficeAddress))
                .WithMessage("آدرس مطب نمی‌تواند بیش از 500 کاراکتر باشد.");

            RuleFor(x => x.ConsultationFee)
                .InclusiveBetween(0, 10000000)
                .When(x => x.ConsultationFee.HasValue)
                .WithMessage("تعرفه ویزیت باید بین 0 تا 10,000,000 تومان باشد.");

            RuleFor(x => x.ExperienceYears)
                .InclusiveBetween(0, 50)
                .When(x => x.ExperienceYears.HasValue)
                .WithMessage("سابقه کاری باید بین 0 تا 50 سال باشد.");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("آدرس عکس نمی‌تواند بیش از 500 کاراکتر باشد.");

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
