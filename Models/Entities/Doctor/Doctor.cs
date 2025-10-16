using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// مدل پزشکان - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
/// 
/// ویژگی‌های کلیدی:
/// 1. مدیریت صحیح پزشکان با توجه به استانداردهای سیستم‌های درمانی
/// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
/// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
/// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
/// 5. ارتباط با کلینیک‌ها، دپارتمان‌ها و سایر موجودیت‌های سیستم
/// 6. **رابطه Many-to-Many با دپارتمان‌ها و دسته‌بندی خدمات**
/// </summary>
public class Doctor : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه پزشک
    /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// نام پزشک
    /// </summary>
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string FirstName { get; set; }

    /// <summary>
    /// نام خانوادگی پزشک
    /// </summary>
    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string LastName { get; set; }

    /// <summary>
    /// کد پزشک
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد پزشک نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string DoctorCode { get; set; }

    /// <summary>
    /// وضعیت فعال/غیرفعال بودن پزشک
    /// پزشکان غیرفعال در سیستم نوبت‌دهی نمایش داده نمی‌شوند
    /// </summary>
    [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// مدرک تحصیلی پزشک
    /// </summary>
    [Required(ErrorMessage = "مدرک تحصیلی الزامی است.")]
    public Degree Degree { get; set; }

    /// <summary>
    /// سال فارغ‌التحصیلی
    /// </summary>
    [Range(1350, 1410, ErrorMessage = "سال فارغ‌التحصیلی باید بین 1350 تا 1410 باشد.")]
    public int? GraduationYear { get; set; }

    /// <summary>
    /// دانشگاه محل تحصیل
    /// </summary>
    [MaxLength(200, ErrorMessage = "نام دانشگاه نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string University { get; set; }

    /// <summary>
    /// جنسیت پزشک
    /// </summary>
    [Required(ErrorMessage = "جنسیت الزامی است.")]
    public Gender Gender { get; set; }

    /// <summary>
    /// تاریخ تولد پزشک
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// آدرس منزل پزشک
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس منزل نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string HomeAddress { get; set; }

    /// <summary>
    /// آدرس مطب/کلینیک پزشک
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس مطب نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string OfficeAddress { get; set; }



    /// <summary>
    /// سابقه کاری پزشک (به سال)
    /// </summary>
    [Range(0, 50, ErrorMessage = "سابقه کاری باید بین 0 تا 50 سال باشد.")]
    public int? ExperienceYears { get; set; }

    /// <summary>
    /// عکس پروفایل پزشک
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس عکس نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string ProfileImageUrl { get; set; }

    /// <summary>
    /// شماره تلفن پزشک
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
    [RegularExpression(@"^(\+98|0)?9\d{9}$", ErrorMessage = "شماره تلفن باید در فرمت صحیح وارد شود.")]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// کد ملی پزشک
    /// </summary>
    [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیش از 10 کاراکتر باشد.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "کد ملی باید 10 رقم باشد.")]
    public string NationalCode { get; set; }

    /// <summary>
    /// کد نظام پزشکی
    /// </summary>
    [MaxLength(20, ErrorMessage = "کد نظام پزشکی نمی‌تواند بیش از 20 کاراکتر باشد.")]
    [RegularExpression(@"^[0-9\-]{6,8}$", ErrorMessage = "کد نظام پزشکی باید 6 تا 8 کاراکتر (اعداد و خط تیره) باشد.")]
    public string MedicalCouncilCode { get; set; }

    /// <summary>
    /// ایمیل پزشک
    /// </summary>
    [MaxLength(100, ErrorMessage = "ایمیل نمی‌تواند بیش از 100 کاراکتر باشد.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
    public string Email { get; set; }

    /// <summary>
    /// شناسه کلینیک مربوطه (اختیاری)
    /// این فیلد برای مشخص کردن کلینیک اصلی پزشک استفاده می‌شود
    /// </summary>
    public int? ClinicId { get; set; }

    /// <summary>
    /// بیوگرافی یا توضیحات پزشک
    /// شامل سوابق تحصیلی، تجربیات کاری و سایر اطلاعات مهم
    /// </summary>
    [MaxLength(2000, ErrorMessage = "توضیحات نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string Bio { get; set; }

    /// <summary>
    /// آدرس پزشک
    /// </summary>
    [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Address { get; set; }

    /// <summary>
    /// شماره تماس اضطراری
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره تماس اضطراری نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string EmergencyContact { get; set; }

    /// <summary>
    /// سطح امنیتی پزشک
    /// </summary>
    [MaxLength(20, ErrorMessage = "سطح امنیتی نمی‌تواند بیش از 20 کاراکتر باشد.")]
    public string SecurityLevel { get; set; } = "Normal";

    /// <summary>
    /// تحصیلات پزشک
    /// </summary>
    [MaxLength(100, ErrorMessage = "تحصیلات نمی‌تواند بیش از 100 کاراکتر باشد.")]
    public string Education { get; set; }

    /// <summary>
    /// شماره پروانه پزشکی
    /// </summary>
    [MaxLength(50, ErrorMessage = "شماره پروانه نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string LicenseNumber { get; set; }

    /// <summary>
    /// نام کامل پزشک
    /// این ویژگی برای نمایش در UI بسیار مفید است
    /// </summary>
    [NotMapped]
    public string FullName
    {
        get
        {
            return $"{FirstName} {LastName}".Trim();
        }
    }

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن پزشک
    /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف پزشک
    /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پزشک را حذف کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر حذف کننده ضروری است
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد پزشک
    /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربری که پزشک را ایجاد کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش پزشک
    /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربری که پزشک را ویرایش کرده است
    /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش کننده
    /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به کلینیک مرتبط با این پزشک
    /// این ارتباط برای نمایش اطلاعات کلینیک در سیستم‌های پزشکی ضروری است
    /// </summary>
    public virtual Clinic.Clinic Clinic { get; set; }

    /// <summary>
    /// لیست پذیرش‌های مرتبط با این پزشک
    /// این لیست برای نمایش تمام پذیرش‌های انجام شده توسط این پزشک استفاده می‌شود
    /// </summary>
    public virtual ICollection<Reception.Reception> Receptions { get; set; } = new HashSet<Reception.Reception>();

    /// <summary>
    /// لیست نوبت‌های مرتبط با این پزشک
    /// این لیست برای نمایش تمام نوبت‌های ثبت شده برای این پزشک استفاده می‌شود
    /// </summary>
    public virtual ICollection<Appointment.Appointment> Appointments { get; set; } = new HashSet<Appointment.Appointment>();

    /// <summary>
    /// لیست ارتباطات Many-to-Many با دپارتمان‌ها
    /// این رابطه برای مشخص کردن اینکه پزشک در کدام دپارتمان‌ها فعالیت می‌کند استفاده می‌شود
    /// مثال: دکتر احمدی هم در دپارتمان اورژانس و هم در دپارتمان تزریقات فعال است
    /// </summary>
    public virtual ICollection<DoctorDepartment> DoctorDepartments { get; set; } = new HashSet<DoctorDepartment>();

    /// <summary>
    /// لیست ارتباطات Many-to-Many با دسته‌بندی خدمات
    /// این رابطه برای مشخص کردن اینکه پزشک مجاز به ارائه کدام دسته خدمات است استفاده می‌شود
    /// مثال: دکتر احمدی مجاز به "تزریقات عضلانی" است ولی مجاز به "تزریق بوتاکس" نیست
    /// </summary>
    public virtual ICollection<DoctorServiceCategory> DoctorServiceCategories { get; set; } = new HashSet<DoctorServiceCategory>();

    /// <summary>
    /// لیست برنامه‌های کاری پزشک
    /// این رابطه برای مدیریت برنامه کاری هفتگی پزشک استفاده می‌شود
    /// </summary>
    public virtual ICollection<DoctorSchedule> Schedules { get; set; } = new HashSet<DoctorSchedule>();

    /// <summary>
    /// لیست اسلات‌های زمانی نوبت‌دهی پزشک
    /// این رابطه برای مدیریت اسلات‌های قابل رزرو استفاده می‌شود
    /// </summary>
    public virtual ICollection<DoctorTimeSlot> TimeSlots { get; set; } = new HashSet<DoctorTimeSlot>();

    /// <summary>
    /// لیست روابط پزشک-تخصص (Many-to-Many)
    /// این رابطه برای مشخص کردن تخصص‌های پزشک استفاده می‌شود
    /// </summary>
    public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new HashSet<DoctorSpecialization>();

    /// <summary>
    /// لیست برنامه‌های کاری پزشک
    /// این رابطه برای مدیریت برنامه‌های کاری هفتگی پزشک استفاده می‌شود
    /// </summary>
    public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new HashSet<DoctorSchedule>();

    /// <summary>
    /// تخصص اصلی پزشک (برای دسترسی مستقیم)
    /// </summary>
    [NotMapped]
    public string SpecializationName
    {
        get
        {
            return DoctorSpecializations?.FirstOrDefault()?.Specialization?.Name ?? "";
        }
    }

    /// <summary>
    /// لیست نام‌های تخصص‌های پزشک (برای دسترسی مستقیم)
    /// </summary>
    [NotMapped]
    public IEnumerable<string> SpecializationNames
    {
        get
        {
            return DoctorSpecializations?.Select(ds => ds.Specialization?.Name).Where(name => !string.IsNullOrEmpty(name)) ?? Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// لیست ارزیابی‌های تریاژ انجام شده توسط این پزشک (به عنوان پزشک پیشنهادی)
    /// این لیست برای نمایش تمام ارزیابی‌های تریاژ که این پزشک به عنوان پزشک پیشنهادی در آن‌ها ذکر شده است
    /// </summary>
    public virtual ICollection<Triage.TriageAssessment> RecommendedTriageAssessments { get; set; } = new HashSet<Triage.TriageAssessment>();
    #endregion
}

#region Doctor

/// <summary>
/// پیکربندی مدل پزشکان برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class DoctorConfig : EntityTypeConfiguration<Doctor>
{
    public DoctorConfig()
    {
        ToTable("Doctors");
        HasKey(d => d.DoctorId);

        // ویژگی‌های اصلی
        Property(d => d.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_FirstName")));

        Property(d => d.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_LastName")));

        Property(d => d.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_IsActive")));

        Property(d => d.Degree)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_Degree")));

        Property(d => d.GraduationYear)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_GraduationYear")));

        Property(d => d.University)
            .IsOptional()
            .HasMaxLength(200)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_University")));

        Property(d => d.Gender)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_Gender")));

        Property(d => d.DateOfBirth)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_DateOfBirth")));

        Property(d => d.HomeAddress)
            .IsOptional()
            .HasMaxLength(500);

        Property(d => d.OfficeAddress)
            .IsOptional()
            .HasMaxLength(500);

        Property(d => d.ExperienceYears)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_ExperienceYears")));

        Property(d => d.ProfileImageUrl)
            .IsOptional()
            .HasMaxLength(500);

        Property(d => d.PhoneNumber)
            .IsOptional()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_PhoneNumber")));

        Property(d => d.NationalCode)
            .IsOptional()
            .HasMaxLength(10)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_NationalCode")));

        Property(d => d.MedicalCouncilCode)
            .IsOptional()
            .HasMaxLength(20)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_MedicalCouncilCode")));

        Property(d => d.Email)
            .IsOptional()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_Email")));

        Property(d => d.Bio)
            .IsOptional()
            .HasMaxLength(2000);

        Property(d => d.Address)
            .IsOptional()
            .HasMaxLength(500);

        Property(d => d.EmergencyContact)
            .IsOptional()
            .HasMaxLength(50);

        Property(d => d.SecurityLevel)
            .IsOptional()
            .HasMaxLength(20);

        Property(d => d.Education)
            .IsOptional()
            .HasMaxLength(100);

        Property(d => d.LicenseNumber)
            .IsOptional()
            .HasMaxLength(50);

        // پیاده‌سازی ISoftDelete
        Property(d => d.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_IsDeleted")));

        Property(d => d.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(d => d.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_CreatedAt")));

        Property(d => d.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_CreatedByUserId")));

        Property(d => d.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_UpdatedAt")));

        Property(d => d.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_UpdatedByUserId")));

        Property(d => d.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_DeletedByUserId")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(d => d.ClinicId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Doctor_ClinicId")));

        // روابط

        HasOptional(d => d.Clinic)
            .WithMany(c => c.Doctors)
            .HasForeignKey(d => d.ClinicId)
            .WillCascadeOnDelete(false);

        // ✅ روابط Many-to-Many جدید
        HasMany(d => d.DoctorDepartments)
            .WithRequired(dd => dd.Doctor)
            .HasForeignKey(dd => dd.DoctorId)
            .WillCascadeOnDelete(false);

        HasMany(d => d.DoctorServiceCategories)
            .WithRequired(dsc => dsc.Doctor)
            .HasForeignKey(dsc => dsc.DoctorId)
            .WillCascadeOnDelete(false);

        // روابط عملیاتی
        HasMany(d => d.Receptions)
            .WithRequired(r => r.Doctor)
            .HasForeignKey(r => r.DoctorId)
            .WillCascadeOnDelete(false);

        HasMany(d => d.Appointments)
            .WithRequired(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .WillCascadeOnDelete(false);

        // رابطه Many-to-Many با تخصص‌ها
        HasMany(d => d.DoctorSpecializations)
            .WithRequired(ds => ds.Doctor)
            .HasForeignKey(ds => ds.DoctorId)
            .WillCascadeOnDelete(false);

        // روابط Audit
        HasOptional(d => d.DeletedByUser)
            .WithMany()
            .HasForeignKey(d => d.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(d => d.CreatedByUser)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(d => d.UpdatedByUser)
            .WithMany()
            .HasForeignKey(d => d.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(d => new { d.LastName, d.FirstName })
            .HasName("IX_Doctor_LastName_FirstName");

        HasIndex(d => new { d.ClinicId, d.IsActive, d.IsDeleted })
            .HasName("IX_Doctor_ClinicId_IsActive_IsDeleted");

        HasIndex(d => new { d.University, d.IsActive, d.IsDeleted })
            .HasName("IX_Doctor_University_IsActive_IsDeleted");
    }
}

#endregion