using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClinicApp.Models.Entities;
using ClinicApp.Helpers;
using ClinicApp.Extensions;
using ClinicApp.Filters;
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
        /// تاریخ تولد پزشک (میلادی - برای ذخیره در دیتابیس)
        /// </summary>
        [Display(Name = "تاریخ تولد")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// تاریخ تولد پزشک (شمسی - برای نمایش و دریافت از کاربر)
        /// </summary>
        [Display(Name = "تاریخ تولد (شمسی)")]
        [PersianDate(
            IsRequired = false,
            MustBePastDate = true,
            ErrorMessage = "تاریخ تولد وارد شده معتبر نیست.",
            PastDateRequiredMessage = "تاریخ تولد نمی‌تواند در آینده باشد."
        )]
        public string DateOfBirthShamsi { get; set; }

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

        /// <summary>
        /// آدرس پزشک
        /// </summary>
        [StringLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        [Display(Name = "آدرس")]
        public string Address { get; set; }

        /// <summary>
        /// شماره تماس اضطراری
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره تماس اضطراری نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره تماس اضطراری")]
        public string EmergencyContact { get; set; }

        /// <summary>
        /// سطح امنیتی پزشک
        /// </summary>
        [Display(Name = "سطح امنیتی")]
        public string SecurityLevel { get; set; } = "Normal";

        /// <summary>
        /// تحصیلات پزشک
        /// </summary>
        [StringLength(100, ErrorMessage = "تحصیلات نمی‌تواند بیش از 100 کاراکتر باشد.")]
        [Display(Name = "تحصیلات")]
        public string Education { get; set; }

        /// <summary>
        /// شماره پروانه پزشکی
        /// </summary>
        [StringLength(50, ErrorMessage = "شماره پروانه نمی‌تواند بیش از 50 کاراکتر باشد.")]
        [Display(Name = "شماره پروانه")]
        public string LicenseNumber { get; set; }

        /// <summary>
        /// شناسه کلینیک مرتبط با این پزشک
        /// در سیستم فعلی، تمام پزشکان به کلینیک شفا تعلق دارند
        /// </summary>
        [Display(Name = "کلینیک")]
        public int? ClinicId { get; set; }

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
                DateOfBirthShamsi = doctor.DateOfBirth?.ToPersianDate(),
                HomeAddress = doctor.HomeAddress,
                OfficeAddress = doctor.OfficeAddress,

                ExperienceYears = doctor.ExperienceYears,
                ProfileImageUrl = doctor.ProfileImageUrl,
                PhoneNumber = doctor.PhoneNumber,
                NationalCode = doctor.NationalCode,
                MedicalCouncilCode = doctor.MedicalCouncilCode,
                Email = doctor.Email,
                Bio = doctor.Bio,
                Address = doctor.Address,
                EmergencyContact = doctor.EmergencyContact,
                SecurityLevel = doctor.SecurityLevel,
                Education = doctor.Education,
                LicenseNumber = doctor.LicenseNumber,
                ClinicId = doctor.ClinicId, // Add ClinicId to the ViewModel
                CreatedAt = doctor.CreatedAt,
                CreatedBy = doctor.CreatedByUser?.FullName ?? doctor.CreatedByUserId,
                UpdatedAt = doctor.UpdatedAt,
                UpdatedBy = doctor.UpdatedByUser?.FullName ?? doctor.UpdatedByUserId,
                CreatedAtShamsi = doctor.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = doctor.UpdatedAt?.ToPersianDateTime(),
                SelectedSpecializationIds = doctor.DoctorSpecializations?.Select(ds => ds.SpecializationId).ToList() ?? new List<int>()
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

                ExperienceYears = this.ExperienceYears,
                ProfileImageUrl = this.ProfileImageUrl,
                PhoneNumber = this.PhoneNumber,
                NationalCode = this.NationalCode,
                MedicalCouncilCode = this.MedicalCouncilCode,
                Email = this.Email,
                Bio = this.Bio,
                Address = this.Address,
                EmergencyContact = this.EmergencyContact,
                SecurityLevel = this.SecurityLevel,
                Education = this.Education,
                LicenseNumber = this.LicenseNumber,
                ClinicId = this.ClinicId, // Add ClinicId to the Entity
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
                .WithMessage("نام پزشک باید به فارسی وارد شود.")
                .MinimumLength(2)
                .WithMessage("نام پزشک باید حداقل 2 کاراکتر باشد.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("نام خانوادگی پزشک الزامی است.")
                .MaximumLength(100)
                .WithMessage("نام خانوادگی پزشک نمی‌تواند بیش از 100 کاراکتر باشد.")
                .Matches(@"^[\u0600-\u06FF\s]+$")
                .WithMessage("نام خانوادگی پزشک باید به فارسی وارد شود.")
                .MinimumLength(2)
                .WithMessage("نام خانوادگی پزشک باید حداقل 2 کاراکتر باشد.");

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
                .WithMessage("نام دانشگاه نمی‌تواند بیش از 200 کاراکتر باشد.")
                .Matches(@"^[\u0600-\u06FF\s\-\(\)]+$")
                .When(x => !string.IsNullOrEmpty(x.University))
                .WithMessage("نام دانشگاه باید به فارسی وارد شود.");

            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("جنسیت نامعتبر است.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Now)
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.")
                .GreaterThan(DateTime.Now.AddYears(-100))
                .When(x => x.DateOfBirth.HasValue)
                .WithMessage("تاریخ تولد نامعتبر است.");

            RuleFor(x => x.HomeAddress)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.HomeAddress))
                .WithMessage("آدرس منزل نمی‌تواند بیش از 500 کاراکتر باشد.");

            RuleFor(x => x.OfficeAddress)
                .MaximumLength(500)
                .WithMessage("آدرس مطب نمی‌تواند بیش از 500 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.OfficeAddress));

            RuleFor(x => x.NationalCode)
                .NotEmpty()
                .WithMessage("کد ملی الزامی است.")
                .Length(10)
                .WithMessage("کد ملی باید دقیقاً 10 رقم باشد.")
                .Matches(@"^\d{10}$")
                .WithMessage("کد ملی باید فقط شامل اعداد باشد.")
                .Must(BeValidNationalCode)
                .WithMessage("کد ملی وارد شده صحیح نیست.");

            RuleFor(x => x.MedicalCouncilCode)
                .NotEmpty()
                .WithMessage("کد نظام پزشکی الزامی است.")
                .MaximumLength(20)
                .WithMessage("کد نظام پزشکی نمی‌تواند بیش از 20 کاراکتر باشد.")
                .Matches(@"^[0-9\-]{6,8}$")
                .WithMessage("کد نظام پزشکی باید 6 تا 8 کاراکتر (اعداد و خط تیره) باشد.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("فرمت ایمیل نامعتبر است.")
                .When(x => !string.IsNullOrEmpty(x.Email))
                .MaximumLength(100)
                .WithMessage("ایمیل نمی‌تواند بیش از 100 کاراکتر باشد.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("شماره تلفن الزامی است.")
                .MaximumLength(11)
                .WithMessage("شماره تلفن نمی‌تواند بیش از 11 رقم باشد.")
                .Matches(@"^0\d{10}$")
                .WithMessage("شماره تلفن باید با 0 شروع شود و 11 رقم باشد.");

            RuleFor(x => x.SelectedSpecializationIds)
                .NotEmpty()
                .WithMessage("انتخاب حداقل یک تخصص الزامی است.")
                .Must(x => x != null && x.Count > 0)
                .WithMessage("لطفاً حداقل یک تخصص انتخاب کنید.")
                .Must(x => x == null || x.Count <= 10)
                .WithMessage("حداکثر 10 تخصص می‌توانید انتخاب کنید.");

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.EmergencyContact)
                .MaximumLength(50)
                .WithMessage("شماره تماس اضطراری نمی‌تواند بیش از 50 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContact))
                .Matches(@"^[0-9\-\(\)\s]+$")
                .When(x => !string.IsNullOrEmpty(x.EmergencyContact))
                .WithMessage("شماره تماس اضطراری نامعتبر است.");

            RuleFor(x => x.Education)
                .MaximumLength(100)
                .WithMessage("تحصیلات نمی‌تواند بیش از 100 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Education));

            RuleFor(x => x.LicenseNumber)
                .MaximumLength(50)
                .WithMessage("شماره پروانه نمی‌تواند بیش از 50 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.LicenseNumber))
                .Matches(@"^[0-9\-]+$")
                .When(x => !string.IsNullOrEmpty(x.LicenseNumber))
                .WithMessage("شماره پروانه باید شامل اعداد و خط تیره باشد.");

            RuleFor(x => x.ExperienceYears)
                .InclusiveBetween(0, 50)
                .When(x => x.ExperienceYears.HasValue)
                .WithMessage("سال‌های تجربه باید بین 0 تا 50 باشد.");

            RuleFor(x => x.Bio)
                .MaximumLength(2000)
                .WithMessage("بیوگرافی نمی‌تواند بیش از 2000 کاراکتر باشد.")
                .When(x => !string.IsNullOrEmpty(x.Bio));

            // Validation های ترکیبی
            RuleFor(x => x)
                .Must(BeValidGraduationAndExperience)
                .WithMessage("سال‌های تجربه نمی‌تواند بیشتر از فاصله سال فارغ‌التحصیلی تا حال باشد.")
                .When(x => x.GraduationYear.HasValue && x.ExperienceYears.HasValue);

            RuleFor(x => x)
                .Must(BeValidAgeAndGraduation)
                .WithMessage("سال فارغ‌التحصیلی با سن پزشک سازگار نیست.")
                .When(x => x.DateOfBirth.HasValue && x.GraduationYear.HasValue);

            // Validation اضافی برای اطمینان از منطقی بودن داده‌ها
            RuleFor(x => x)
                .Must(BeValidDateOfBirth)
                .WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x)
                .Must(BeValidGraduationYearRange)
                .WithMessage("سال فارغ‌التحصیلی باید بین 1350 تا سال جاری باشد.")
                .When(x => x.GraduationYear.HasValue);

            // اضافه کردن لاگ برای عیب‌یابی
            RuleFor(x => x)
                .Must(LogValidationInfo)
                .When(x => x.GraduationYear.HasValue);
        }

        /// <summary>
        /// بررسی صحت کد ملی
        /// </summary>
        private bool BeValidNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode) || nationalCode.Length != 10)
                return false;

            // بررسی اینکه همه کاراکترها عدد باشند
            if (!nationalCode.All(char.IsDigit))
                return false;

            // بررسی الگوریتم کد ملی
            var sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(nationalCode[i].ToString()) * (10 - i);
            }

            var remainder = sum % 11;
            var checkDigit = int.Parse(nationalCode[9].ToString());

            if (remainder < 2)
            {
                return checkDigit == remainder;
            }
            else
            {
                return checkDigit == (11 - remainder);
            }
        }

        /// <summary>
        /// بررسی سازگاری سال فارغ‌التحصیلی و تجربه
        /// </summary>
        private bool BeValidGraduationAndExperience(DoctorCreateEditViewModel model)
        {
            if (!model.GraduationYear.HasValue || !model.ExperienceYears.HasValue)
                return true;

            // محاسبه دقیق سال شمسی فعلی
            var currentPersianYear = GetCurrentPersianYear();
            
            // بررسی اینکه سال فارغ‌التحصیلی در آینده نباشد
            if (model.GraduationYear.Value > currentPersianYear)
            {
                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Graduation year {model.GraduationYear.Value} is in future. Current Persian year: {currentPersianYear}");
                return false;
            }

            // محاسبه فاصله از فارغ‌التحصیلی تا حال
            var yearsSinceGraduation = currentPersianYear - model.GraduationYear.Value;

            // سال‌های تجربه نمی‌تواند بیشتر از فاصله سال فارغ‌التحصیلی تا حال باشد
            // و حداقل باید 0 سال باشد
            var isValid = model.ExperienceYears.Value >= 0 && model.ExperienceYears.Value <= yearsSinceGraduation;
            
            // Log for debugging
            if (!isValid)
            {
                System.Diagnostics.Debug.WriteLine($"Experience validation failed: Experience={model.ExperienceYears.Value}, YearsSinceGraduation={yearsSinceGraduation}, GraduationYear={model.GraduationYear.Value}, CurrentYear={currentPersianYear}");
            }
            
            return isValid;
        }

        /// <summary>
        /// بررسی سازگاری سن و سال فارغ‌التحصیلی
        /// </summary>
        private bool BeValidAgeAndGraduation(DoctorCreateEditViewModel model)
        {
            if (!model.DateOfBirth.HasValue || !model.GraduationYear.HasValue)
                return true;

            // محاسبه سن دقیق فعلی
            var currentAge = CalculateExactAge(model.DateOfBirth.Value);
            
            // محاسبه سال شمسی تولد
            var birthPersianYear = GetPersianYearFromGregorianDate(model.DateOfBirth.Value);
            
            // محاسبه سن در زمان فارغ‌التحصیلی
            var graduationAge = model.GraduationYear.Value - birthPersianYear;

            // بررسی منطقی بودن سن فارغ‌التحصیلی
            // حداقل سن: 18 سال (برای پزشک عمومی)
            // حداکثر سن: 35 سال (برای فوق تخصص)
            var minGraduationAge = 18;
            var maxGraduationAge = 35;

            // اگر سن فعلی کمتر از سن فارغ‌التحصیلی باشد، منطقی نیست
            if (currentAge < graduationAge)
            {
                System.Diagnostics.Debug.WriteLine($"Age validation failed: CurrentAge={currentAge}, GraduationAge={graduationAge}");
                return false;
            }

            var isValid = graduationAge >= minGraduationAge && graduationAge <= maxGraduationAge;
            
            // Log for debugging
            if (!isValid)
            {
                System.Diagnostics.Debug.WriteLine($"Graduation age validation failed: GraduationAge={graduationAge}, Min={minGraduationAge}, Max={maxGraduationAge}, BirthYear={birthPersianYear}, GraduationYear={model.GraduationYear.Value}");
            }
            
            return isValid;
        }

        /// <summary>
        /// محاسبه سال شمسی فعلی به صورت دقیق - Production Ready
        /// </summary>
        private int GetCurrentPersianYear()
        {
            try
            {
                // استفاده از PersianDateHelper برای محاسبه دقیق
                var currentDate = DateTime.Now;
                var persianDate = PersianDateHelper.ToPersianDate(currentDate);
                
                // استخراج سال از تاریخ شمسی
                if (!string.IsNullOrEmpty(persianDate) && persianDate.Contains("/"))
                {
                    var parts = persianDate.Split('/');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out int year) && year > 1300 && year < 1500)
                    {
                        // Log for debugging
                        System.Diagnostics.Debug.WriteLine($"Current Persian year calculated: {year} from date: {currentDate}");
                        return year;
                    }
                }
                
                // Fallback: محاسبه تقریبی - سال 2024 میلادی تقریباً برابر با 1403 شمسی است
                var fallbackYear = currentDate.Year - 621; // 2024 - 621 = 1403
                System.Diagnostics.Debug.WriteLine($"Fallback Persian year: {fallbackYear} (calculated from {currentDate.Year})");
                return fallbackYear;
            }
            catch (Exception ex)
            {
                // Fallback: محاسبه تقریبی در صورت خطا
                var fallbackYear = DateTime.Now.Year - 621; // 2024 - 621 = 1403
                System.Diagnostics.Debug.WriteLine($"Exception in Persian year calculation: {ex.Message}. Using fallback: {fallbackYear}");
                return fallbackYear;
            }
        }

        /// <summary>
        /// محاسبه سن دقیق بر اساس تاریخ تولد
        /// </summary>
        private int CalculateExactAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            // اگر تاریخ تولد در سال جاری هنوز نرسیده، یک سال کم کنیم
            if (birthDate.Date > today.AddYears(-age))
            {
                age--;
            }
            
            return age;
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به سال شمسی
        /// </summary>
        private int GetPersianYearFromGregorianDate(DateTime gregorianDate)
        {
            try
            {
                var persianDate = PersianDateHelper.ToPersianDate(gregorianDate);
                
                if (persianDate.Contains("/"))
                {
                    var parts = persianDate.Split('/');
                    if (parts.Length >= 1 && int.TryParse(parts[0], out int year))
                    {
                        System.Diagnostics.Debug.WriteLine($"Persian year from date {gregorianDate}: {year}");
                        return year;
                    }
                }
                
                // Fallback: محاسبه تقریبی - استفاده از فرمول صحیح
                var fallbackYear = gregorianDate.Year - 621;
                System.Diagnostics.Debug.WriteLine($"Fallback Persian year from {gregorianDate}: {fallbackYear}");
                return fallbackYear;
            }
            catch (Exception ex)
            {
                // Fallback: محاسبه تقریبی در صورت خطا
                var fallbackYear = gregorianDate.Year - 621;
                System.Diagnostics.Debug.WriteLine($"Exception in Persian year conversion: {ex.Message}. Using fallback: {fallbackYear}");
                return fallbackYear;
            }
        }

        /// <summary>
        /// بررسی معتبر بودن تاریخ تولد
        /// </summary>
        private bool BeValidDateOfBirth(DoctorCreateEditViewModel model)
        {
            if (!model.DateOfBirth.HasValue)
                return true;

            // تاریخ تولد نمی‌تواند در آینده باشد
            return model.DateOfBirth.Value.Date <= DateTime.Today.Date;
        }

        /// <summary>
        /// بررسی محدوده منطقی سال فارغ‌التحصیلی - Production Ready
        /// </summary>
        private bool BeValidGraduationYearRange(DoctorCreateEditViewModel model)
        {
            if (!model.GraduationYear.HasValue)
                return true;

            try
            {
                var currentPersianYear = GetCurrentPersianYear();
                
                // سال فارغ‌التحصیلی باید بین 1350 تا سال جاری باشد
                var isValid = model.GraduationYear.Value >= 1350 && model.GraduationYear.Value <= currentPersianYear;
                
                // Log for debugging
                System.Diagnostics.Debug.WriteLine($"Graduation year range validation: {model.GraduationYear.Value} (1350 <= year <= {currentPersianYear}) = {isValid}");
                
                return isValid;
            }
            catch (Exception ex)
            {
                // در صورت خطا، فقط محدوده پایه را بررسی کنیم
                System.Diagnostics.Debug.WriteLine($"Error in graduation year validation: {ex.Message}. Using fallback validation.");
                return model.GraduationYear.Value >= 1350 && model.GraduationYear.Value <= 1410;
            }
        }

        /// <summary>
        /// لاگ اطلاعات اعتبارسنجی برای عیب‌یابی
        /// </summary>
        private bool LogValidationInfo(DoctorCreateEditViewModel model)
        {
            var currentPersianYear = GetCurrentPersianYear();
            System.Diagnostics.Debug.WriteLine($"=== VALIDATION DEBUG INFO ===");
            System.Diagnostics.Debug.WriteLine($"Current Persian Year: {currentPersianYear}");
            System.Diagnostics.Debug.WriteLine($"Graduation Year: {model.GraduationYear}");
            System.Diagnostics.Debug.WriteLine($"Experience Years: {model.ExperienceYears}");
            System.Diagnostics.Debug.WriteLine($"Date of Birth: {model.DateOfBirth}");
            if (model.DateOfBirth.HasValue)
            {
                var birthPersianYear = GetPersianYearFromGregorianDate(model.DateOfBirth.Value);
                System.Diagnostics.Debug.WriteLine($"Birth Persian Year: {birthPersianYear}");
            }
            System.Diagnostics.Debug.WriteLine($"=============================");
            return true; // این متد همیشه true برمی‌گرداند چون فقط برای لاگ است
        }
    }
}
