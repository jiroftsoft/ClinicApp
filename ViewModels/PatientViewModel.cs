using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels
{
    /// <summary>
    /// ویومدل پایه بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 2. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی
    /// 3. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 4. رعایت استانداردهای امنیتی سیستم‌های پزشکی
    /// 5. پشتیبانی از سیستم بیمه‌ها و محاسبات مالی
    /// </summary>
    public class PatientViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => Gender == Gender.Male ? "مرد" : "زن";


        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Display(Name = "نام پزشک معالج")]
        public string DoctorName { get; set; }

        // InsuranceName حذف شد - از PatientInsurance استفاده کنید

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مانده بدهی")]
        public decimal DebtBalance { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        // فیلدهای مربوط به سیستم حذف نرم
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
        public string DeletedByUser { get; set; }

        #region Factory Methods

        /// <summary>
        /// ایجاد PatientViewModel از Entity
        /// </summary>
        /// <param name="entity">Entity بیمار</param>
        /// <returns>PatientViewModel</returns>
        public static PatientViewModel FromEntity(Patient entity)
        {
            if (entity == null) return null;

            return new PatientViewModel
            {
                PatientId = entity.PatientId,
                NationalCode = entity.NationalCode,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                FullName = $"{entity.FirstName} {entity.LastName}".Trim(),
                BirthDate = entity.BirthDate,
                BirthDateShamsi = entity.BirthDate.HasValue ? entity.BirthDate.Value.ToPersianDate() : null,
                Gender = entity.Gender,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Address = entity.Address,
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUser = entity.CreatedByUser != null ? 
                    $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt.HasValue ? 
                    entity.UpdatedAt.Value.ToPersianDateTime() : null,
                UpdatedByUser = entity.UpdatedByUser != null ? 
                    $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                LastLoginDate = entity.LastLoginDate,
                LastLoginDateShamsi = entity.LastLoginDate.HasValue ? 
                    entity.LastLoginDate.Value.ToPersianDateTime() : null,
                DoctorName = string.Empty, // فیلد DoctorName در Patient Entity موجود نیست
                DebtBalance = 0, // محاسبه در Service انجام می‌شود
                IsDeleted = entity.IsDeleted,
                DeletedAt = entity.DeletedAt,
                DeletedAtShamsi = entity.DeletedAt.HasValue ? 
                    entity.DeletedAt.Value.ToPersianDateTime() : null,
                DeletedByUser = entity.DeletedByUser != null ? 
                    $"{entity.DeletedByUser.FirstName} {entity.DeletedByUser.LastName}" : null
            };
        }

        #endregion
    }

    /// <summary>
    /// ویومدل اختصاصی برای فرم‌های ایجاد و ویرایش بیمار
    /// شامل تمام فیلدهای قابل ویرایش و قوانین اعتبارسنجی
    /// </summary>
    public class PatientCreateEditViewModel
    {
        // PatientId فقط در حالت ویرایش مقدار دارد
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "{0} باید ۱۰ رقم باشد.")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [MaxLength(150, ErrorMessage = "{0} نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "کد بیمار")]
        [MaxLength(20, ErrorMessage = "کد بیمار نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string PatientCode { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفاً {0} را وارد کنید.")]
        [RegularExpression("09[0-9]{9}", ErrorMessage = "فرمت {0} صحیح نیست.")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "آدرس ایمیل نامعتبر است.")]
        [MaxLength(256, ErrorMessage = "آدرس ایمیل نمی‌تواند بیش از 256 کاراکتر باشد.")]
        public string Email { get; set; }

        [Display(Name = "جنسیت")]
        [Required(ErrorMessage = "انتخاب {0} الزامی است.")]
        public Gender Gender { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        [PersianDate(
            IsRequired = false,
            MustBePastDate = true,
            ErrorMessage = "تاریخ تولد وارد شده معتبر نیست.",
            PastDateRequiredMessage = "تاریخ تولد نمی‌تواند در آینده باشد."
        )]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "آدرس")]
        [DataType(DataType.MultilineText)]
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string Address { get; set; }

        // InsuranceId و InsuranceName حذف شد - از PatientInsurance استفاده کنید

        [Display(Name = "نام پزشک معالج")]
        [MaxLength(200, ErrorMessage = "نام پزشک نمی‌تواند بیش از {1} کاراکتر باشد.")]
        public string DoctorName { get; set; }

        // فیلدهای مربوط به تاریخ ایجاد و ویرایش
        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }
        public bool RequireDataTables { get; set; }
        public bool RequireSelect2 { get; set; }
        public bool RequireDatePicker { get; set; }
        public bool RequireFormValidation { get; set; }
    }

    /// <summary>
    /// ویومدل سبک برای نمایش هر ردیف در لیست بیماران
    /// فقط شامل اطلاعات ضروری برای نمایش در جدول
    /// </summary>
    public class PatientIndexViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "نام کامل")]
        public string FullName { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "سن")]
        public int? Age { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "نام پزشک معالج")]
        public string DoctorName { get; set; }
        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => this.Gender == Gender.Male ? "مرد" : "زن";

        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        // InsuranceName حذف شد - از PatientInsurance استفاده کنید

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        public decimal DebtBalance { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        public int ReceptionCount { get; set; }

        // فیلدهای باندل حذف شدند طبق قرارداد FormStandards

        #region Factory Methods

        /// <summary>
        /// ایجاد PatientCreateEditViewModel از Entity
        /// </summary>
        /// <param name="entity">Entity بیمار</param>
        /// <returns>PatientCreateEditViewModel</returns>
        public static PatientCreateEditViewModel FromEntity(Patient entity)
        {
            if (entity == null) return null;

            return new PatientCreateEditViewModel
            {
                PatientId = entity.PatientId,
                NationalCode = entity.NationalCode,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                BirthDate = entity.BirthDate,
                BirthDateShamsi = entity.BirthDate.HasValue ? entity.BirthDate.Value.ToPersianDate() : null,
                Gender = entity.Gender,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Address = entity.Address,
                DoctorName = string.Empty, // فیلد DoctorName در Patient Entity موجود نیست
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime()
            };
        }

        /// <summary>
        /// تبدیل PatientCreateEditViewModel به Entity
        /// </summary>
        /// <returns>Patient Entity</returns>
        public Patient ToEntity()
        {
            return new Patient
            {
                PatientId = this.PatientId,
                NationalCode = this.NationalCode,
                FirstName = this.FirstName,
                LastName = this.LastName,
                BirthDate = this.BirthDate,
                Gender = this.Gender,
                PhoneNumber = this.PhoneNumber,
                Address = this.Address,
                Email = this.Email
                // DoctorName در Patient Entity موجود نیست - از طریق روابط دریافت می‌شود
            };
        }

        #endregion
    }

    /// <summary>
    /// مدل ویو برای جزئیات بیمار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. نمایش کامل اطلاعات بیمار با رعایت استانداردهای پزشکی
    /// 2. پشتیبانی از محیط‌های ایرانی با تاریخ شمسی و اعداد فارسی
    /// 3. رعایت استانداردهای سیستم‌های پزشکی ایران
    /// 4. امنیت بالا با عدم نمایش اطلاعات حساس
    /// 5. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 6. مدیریت کامل ردیابی (Audit Trail)
    /// </summary>
    public class PatientDetailsViewModel
    {
        public int PatientId { get; set; }

        [Display(Name = "کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Display(Name = "جنسیت")]
        public Gender Gender { get; set; }

        [Display(Name = "جنسیت")]
        public string GenderDisplay => this.Gender == Gender.Male ? "مرد" : "زن";

        [Display(Name = "نام کامل")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "تاریخ تولد")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "تاریخ تولد (شمسی)")]
        public string BirthDateShamsi { get; set; }

        [Display(Name = "آدرس")]
        public string Address { get; set; }

        [Display(Name = "شماره تلفن")]
        public string PhoneNumber { get; set; }

        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        // InsuranceId حذف شد - از PatientInsurance استفاده کنید


        [Display(Name = "نام پزشک معالج")]
        public string DoctorName { get; set; }

        // فیلدهای باندل حذف شدند طبق قرارداد FormStandards

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }

        [Display(Name = "ایجاد شده توسط")]
        public string CreatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi { get; set; }

        [Display(Name = "ویرایش شده توسط")]
        public string UpdatedByUser { get; set; }

        [Display(Name = "تاریخ آخرین ورود")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "تاریخ آخرین ورود (شمسی)")]
        public string LastLoginDateShamsi { get; set; }

        [Display(Name = "تعداد پذیرش‌ها")]
        public int ReceptionCount { get; set; }

        [Display(Name = "مانده بدهی")]
        public decimal DebtBalance { get; set; }

        [Display(Name = "مانده بدهی")]
        public string DebtBalanceFormatted => DebtBalance > 0 ?
            DebtBalance.ToString("N0") + " ریال" : "بدون بدهی";

        // فیلدهای مربوط به سیستم حذف نرم
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedAtShamsi { get; set; }
        public string DeletedByUser { get; set; }

        // بیمه‌های بیمار
        public List<PatientInsuranceIndexViewModel> PatientInsurances { get; set; } = new List<PatientInsuranceIndexViewModel>();

        // نوبت‌های بیمار
        public List<PatientAppointmentViewModel> PatientAppointments { get; set; } = new List<PatientAppointmentViewModel>();

        // پذیرش‌های بیمار
        public List<PatientReceptionViewModel> PatientReceptions { get; set; } = new List<PatientReceptionViewModel>();

        #region Factory Methods

        /// <summary>
        /// ایجاد PatientDetailsViewModel از Entity
        /// </summary>
        /// <param name="entity">Entity بیمار</param>
        /// <returns>PatientDetailsViewModel</returns>
        public static PatientDetailsViewModel FromEntity(Patient entity)
        {
            if (entity == null) return null;

            return new PatientDetailsViewModel
            {
                PatientId = entity.PatientId,
                NationalCode = entity.NationalCode,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                BirthDate = entity.BirthDate,
                BirthDateShamsi = entity.BirthDate.HasValue ? entity.BirthDate.Value.ToPersianDate() : null,
                Gender = entity.Gender,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Address = entity.Address,
                DoctorName = string.Empty, // فیلد DoctorName در Patient Entity موجود نیست
                CreatedAt = entity.CreatedAt,
                CreatedAtShamsi = entity.CreatedAt.ToPersianDateTime(),
                CreatedByUser = entity.CreatedByUser != null ? 
                    $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}" : "ناشناس",
                UpdatedAt = entity.UpdatedAt,
                UpdatedAtShamsi = entity.UpdatedAt.HasValue ? 
                    entity.UpdatedAt.Value.ToPersianDateTime() : null,
                UpdatedByUser = entity.UpdatedByUser != null ? 
                    $"{entity.UpdatedByUser.FirstName} {entity.UpdatedByUser.LastName}" : null,
                LastLoginDate = entity.LastLoginDate,
                LastLoginDateShamsi = entity.LastLoginDate.HasValue ? 
                    entity.LastLoginDate.Value.ToPersianDateTime() : null,
                ReceptionCount = 0, // محاسبه در Service انجام می‌شود
                DebtBalance = 0, // محاسبه در Service انجام می‌شود
                IsDeleted = entity.IsDeleted,
                DeletedAt = entity.DeletedAt,
                DeletedAtShamsi = entity.DeletedAt.HasValue ? 
                    entity.DeletedAt.Value.ToPersianDateTime() : null,
                DeletedByUser = entity.DeletedByUser != null ? 
                    $"{entity.DeletedByUser.FirstName} {entity.DeletedByUser.LastName}" : null
            };
        }

        #endregion
    }

    /// <summary>
    /// ویومدل برای صفحه اصلی بیماران - طبق قرارداد FormStandards
    /// </summary>
    public class PatientIndexPageViewModel
    {
        [Display(Name = "عنوان صفحه")]
        public string PageTitle { get; set; } = "مدیریت بیماران";

        [Display(Name = "تعداد کل بیماران")]
        public int TotalPatients { get; set; }

        [Display(Name = "تعداد بیماران فعال")]
        public int ActivePatients { get; set; }

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string LastUpdatedShamsi => LastUpdated.ToPersianDateTime();
    }

    /// <summary>
    /// ویومدل صفحه ایجاد/ویرایش بیمار - طبق قرارداد FormStandards
    /// </summary>
    public class PatientCreateEditPageViewModel
    {
        [Display(Name = "عنوان صفحه")]
        public string PageTitle { get; set; } = "ثبت بیمار جدید";

        [Display(Name = "زیرعنوان")]
        public string PageSubtitle { get; set; } = "اطلاعات بیمار را با دقت وارد کنید";

        [Display(Name = "فرم بیمار")]
        public PatientCreateEditViewModel FormModel { get; set; } = new PatientCreateEditViewModel();

        [Display(Name = "آیا در حالت ویرایش است")]
        public bool IsEditMode { get; set; } = false;

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string LastUpdatedShamsi => LastUpdated.ToPersianDateTime();
    }

    /// <summary>
    /// ویومدل صفحه جزئیات بیمار - طبق قرارداد FormStandards
    /// </summary>
    public class PatientDetailsPageViewModel
    {
        [Display(Name = "عنوان صفحه")]
        public string PageTitle { get; set; } = "جزئیات بیمار";

        [Display(Name = "زیرعنوان")]
        public string PageSubtitle { get; set; } = "مشاهده کامل اطلاعات بیمار";

        [Display(Name = "اطلاعات بیمار")]
        public PatientDetailsViewModel PatientInfo { get; set; } = new PatientDetailsViewModel();

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string LastUpdatedShamsi => LastUpdated.ToPersianDateTime();

        [Display(Name = "تعداد بیمه‌ها")]
        public int InsuranceCount => PatientInfo?.PatientInsurances?.Count ?? 0;

        [Display(Name = "آیا بیمه دارد")]
        public bool HasInsurance => InsuranceCount > 0;
    }

    /// <summary>
    /// ویومدل صفحه حذف بیمار - طبق قرارداد FormStandards
    /// </summary>
    public class PatientDeletePageViewModel
    {
        [Display(Name = "عنوان صفحه")]
        public string PageTitle { get; set; } = "تأیید حذف بیمار";

        [Display(Name = "زیرعنوان")]
        public string PageSubtitle { get; set; } = "این عملیات غیرقابل بازگشت است";

        [Display(Name = "اطلاعات بیمار")]
        public PatientDetailsViewModel PatientInfo { get; set; } = new PatientDetailsViewModel();

        [Display(Name = "آخرین بروزرسانی")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Display(Name = "آخرین بروزرسانی (شمسی)")]
        public string LastUpdatedShamsi => LastUpdated.ToPersianDateTime();

        [Display(Name = "تعداد بیمه‌ها")]
        public int InsuranceCount => PatientInfo?.PatientInsurances?.Count ?? 0;

        [Display(Name = "آیا بیمه دارد")]
        public bool HasInsurance => InsuranceCount > 0;

        [Display(Name = "هشدار حذف")]
        public string DeleteWarning { get; set; } = "آیا از حذف این بیمار اطمینان دارید؟ این عملیات غیرقابل بازگشت است.";

        [Display(Name = "پیام تأیید")]
        public string ConfirmationMessage { get; set; } = "برای تأیید حذف، دکمه 'حذف' را کلیک کنید.";
    }

    /// <summary>
    /// ViewModel برای نمایش نوبت‌های بیمار
    /// </summary>
    public class PatientAppointmentViewModel
    {
        [Display(Name = "شناسه نوبت")]
        public int AppointmentId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ نوبت")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "تاریخ نوبت (شمسی)")]
        public string AppointmentDateShamsi { get; set; }

        [Display(Name = "وضعیت")]
        public AppointmentStatus Status { get; set; }

        [Display(Name = "وضعیت (متن)")]
        public string StatusText { get; set; }

        [Display(Name = "قیمت")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Display(Name = "دسته‌بندی خدمت")]
        public string ServiceCategoryName { get; set; }

        [Display(Name = "یادداشت‌ها")]
        public string Notes { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }
    }

    /// <summary>
    /// ViewModel برای نمایش پذیرش‌های بیمار
    /// </summary>
    public class PatientReceptionViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        public DateTime ReceptionDate { get; set; }

        [Display(Name = "تاریخ پذیرش (شمسی)")]
        public string ReceptionDateShamsi { get; set; }

        [Display(Name = "وضعیت")]
        public ReceptionStatus Status { get; set; }

        [Display(Name = "وضعیت (متن)")]
        public string StatusText { get; set; }

        [Display(Name = "مجموع مبلغ")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "سهم بیمار")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientCoPay { get; set; }

        [Display(Name = "سهم بیمه")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsurerShareAmount { get; set; }

        [Display(Name = "ارائه‌دهنده بیمه")]
        public string InsuranceProviderName { get; set; }

        [Display(Name = "پرداخت شده")]
        public bool IsPaid { get; set; }

        [Display(Name = "تعداد خدمات")]
        public int ServicesCount { get; set; }

        [Display(Name = "تعداد پرداخت‌ها")]
        public int PaymentsCount { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi { get; set; }
    }
}