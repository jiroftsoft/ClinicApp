using ClinicApp.Core;
using ClinicApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClinicApp.Models.Entities
{
    #region Core Interfaces
    /// <summary>
    /// رابط استاندارد برای Soft Delete در سیستم‌های پزشکی
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string DeletedByUserId { get; set; }
        ApplicationUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// رابط استاندارد برای ردیابی تاریخ ایجاد و به‌روزرسانی
    /// </summary>
    public interface ITrackable
    {
        DateTime CreatedAt { get; set; }
        string CreatedByUserId { get; set; }
        ApplicationUser CreatedByUser { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedByUserId { get; set; }
        ApplicationUser UpdatedByUser { get; set; }
    }
    #endregion

    #region Enums
    /// <summary>
    /// وضعیت پذیرش
    /// </summary>
    public enum ReceptionStatus : byte
    {
        [Display(Name = "در انتظار")]
        Pending = 0,
        [Display(Name = "تکمیل شده")]
        Completed = 1,
        [Display(Name = "لغو شده")]
        Cancelled = 2,
        [Display(Name = "در حال انجام")]
        InProgress = 3,
        [Display(Name = "نیاز به پرداخت بیشتر")]
        NeedsAdditionalPayment = 4
    }

    /// <summary>
    /// روش‌های پرداخت (ادغام شده برای کل سیستم)
    /// </summary>
    public enum PaymentMethod : byte
    {
        [Display(Name = "نقدی")] // از Payment قدیمی
        Cash = 0,
        [Display(Name = "پوز")] // از Payment قدیمی
        POS = 1,
        [Display(Name = "آنلاین")] // از Payment قدیمی
        Online = 2,
        [Display(Name = "بدهی")] // جدید از PaymentTransaction
        Debt = 3,
        [Display(Name = "کارت به کارت")] // جدید از PaymentTransaction
        CardToCard = 4,
        [Display(Name = "حواله بانکی")] // جدید از PaymentTransaction
        BankTransfer = 5
    }

    /// <summary>
    /// وضعیت‌های تراکنش
    /// </summary>
    public enum PaymentStatus
    {
        [Display(Name = "در انتظار")]
        Pending = 1,
        [Display(Name = "موفق")]
        Success = 2,
        [Display(Name = "ناموفق")]
        Failed = 3,
        [Display(Name = "لغو شده")]
        Canceled = 4,
        [Display(Name = "در حال بررسی")]
        UnderReview = 5
    }

    /// <summary>
    /// وضعیت شیفت‌های صندوق
    /// </summary>
    public enum CashSessionStatus
    {
        [Display(Name = "باز")]
        Open = 1,
        [Display(Name = "بسته")]
        Closed = 2,
        [Display(Name = "در حال بررسی")]
        UnderReview = 3
    }

    /// <summary>
    /// جنسیت
    /// </summary>
    public enum Gender : byte
    {
        [Display(Name = "مرد")]
        Male = 1,
        [Display(Name = "زن")]
        Female = 2
    }

    /// <summary>
    /// مدرک تحصیلی
    /// </summary>
    public enum Degree : byte
    {
        [Display(Name = "پزشک عمومی")]
        GeneralPhysician = 1,
        [Display(Name = "متخصص")]
        Specialist = 2,
        [Display(Name = "فوق تخصص")]
        SubSpecialist = 3,
        [Display(Name = "دندانپزشک")]
        Dentist = 4,
        [Display(Name = "داروساز")]
        Pharmacist = 5

    }
    /// <summary>
    /// نوع ارائه‌دهنده پوز
    /// </summary>
    public enum PosProviderType
    {
        [Display(Name = "سامان کیش")]
        SamanKish = 1,
        [Display(Name = "آسان پرداخت")]
        AsanPardakht = 2,
        [Display(Name = "به‌پرداخت")]
        BehPardakht = 3,
        [Display(Name = "فناوا")]
        Fanava = 4,
        [Display(Name = "ایران کیش")]
        IranKish = 5,
        [Display(Name = "پرداخت آریا")]
        PardakhtAria = 6,
        [Display(Name = "ندا پی")]
        NadaPay = 7
    }

    /// <summary>
    /// پروتکل ارتباطی پوز
    /// </summary>
    public enum PosProtocol
    {
        [Display(Name = "TCP/IP")]
        Tcp = 1,
        [Display(Name = "سریال")]
        Serial = 2,
        [Display(Name = "API وب سرویس")]
        Api = 3
    }

    public enum AppointmentStatus : byte
    {
        [Display(Name = "در دسترس")]
        Available = 0,
        [Display(Name = "ثبت شده")]
        Scheduled = 1,
        [Display(Name = "در انتظار")]
        Pending = 2,
        [Display(Name = "انجام شده")]
        Completed = 3,
        [Display(Name = "لغو شده")]
        Cancelled = 4,
        [Display(Name = "عدم حضور")]
        NoShow = 5
    }
    #endregion

    #region Specialization Entity
    /// <summary>
    /// مدل تخصص‌های پزشکی - طراحی شده برای مدیریت تخصص‌ها در کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت متمرکز تخصص‌های پزشکی
    /// 2. امکان فعال/غیرفعال کردن تخصص‌ها
    /// 3. ردیابی کامل تغییرات برای امنیت سیستم
    /// 4. پشتیبانی از سیستم حذف نرم
    /// </summary>
    public class Specialization : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه تخصص
        /// </summary>
        public int SpecializationId { get; set; }

        /// <summary>
        /// نام تخصص
        /// مثال: "متخصص داخلی", "متخصص پوست", "متخصص قلب"
        /// </summary>
        [Required(ErrorMessage = "نام تخصص الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام تخصص نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات تخصص
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن تخصص
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// ترتیب نمایش تخصص
        /// </summary>
        public int DisplayOrder { get; set; } = 0;

        #region پیاده‌سازی ISoftDelete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// لیست روابط پزشک-تخصص
        /// </summary>
        public virtual ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new HashSet<DoctorSpecialization>();
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل تخصص‌ها برای Entity Framework
    /// </summary>
    public class SpecializationConfig : EntityTypeConfiguration<Specialization>
    {
        public SpecializationConfig()
        {
            ToTable("Specializations");
            HasKey(s => s.SpecializationId);

            Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_Name")));

            Property(s => s.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_IsActive")));

            Property(s => s.DisplayOrder)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_DisplayOrder")));

            // پیاده‌سازی ISoftDelete
            Property(s => s.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_IsDeleted")));

            Property(s => s.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(s => s.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_CreatedAt")));

            Property(s => s.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_CreatedByUserId")));

            Property(s => s.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_UpdatedAt")));

            Property(s => s.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_UpdatedByUserId")));

            Property(s => s.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Specialization_DeletedByUserId")));

            // روابط - حذف شده به دلیل تداخل با DoctorSpecialization entity

            HasOptional(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.DeletedByUser)
                .WithMany()
                .HasForeignKey(s => s.DeletedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region DoctorSpecialization Entity (رابطه Many-to-Many)
    /// <summary>
    /// مدل رابطه Many-to-Many بین پزشکان و تخصص‌ها
    /// این کلاس برای مدیریت رابطه بین پزشکان و تخصص‌های آن‌ها استفاده می‌شود
    /// </summary>
    public class DoctorSpecialization
    {
        /// <summary>
        /// شناسه تخصص
        /// </summary>
        public int SpecializationId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        public int DoctorId { get; set; }

        #region روابط
        /// <summary>
        /// تخصص مرتبط
        /// </summary>
        public virtual Specialization Specialization { get; set; }

        /// <summary>
        /// پزشک مرتبط
        /// </summary>
        public virtual Doctor Doctor { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل رابطه پزشک-تخصص برای Entity Framework
    /// </summary>
    public class DoctorSpecializationConfig : EntityTypeConfiguration<DoctorSpecialization>
    {
        public DoctorSpecializationConfig()
        {
            ToTable("DoctorSpecializations");
            
            // تعریف کلید مرکب
            HasKey(ds => new { ds.SpecializationId, ds.DoctorId });

            // روابط - تنظیم صحیح Foreign Keys
            HasRequired(ds => ds.Specialization)
                .WithMany(s => s.DoctorSpecializations)
                .HasForeignKey(ds => ds.SpecializationId)
                .WillCascadeOnDelete(false);

            HasRequired(ds => ds.Doctor)
                .WithMany(d => d.DoctorSpecializations)
                .HasForeignKey(ds => ds.DoctorId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region ApplicationUser

    /// <summary>
    /// مدل کاربران سیستم - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. پشتیبانی از سیستم احراز هویت ASP.NET Identity
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class ApplicationUser : IdentityUser, ISoftDelete, ITrackable
    {
        /// <summary>
        /// نام کاربر
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی کاربر
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string LastName { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد.")]
        [Index(IsUnique = true)]
        public string NationalCode { get; set; }

        // ✅ اضافه شده: شماره تلفن همراه
        [Required(ErrorMessage = "شماره تلفن همراه الزامی است.")]
        [StringLength(20, ErrorMessage = "شماره تلفن همراه نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// نام کامل کاربر
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

        /// <summary>
        /// آیا کاربر فعال است؟
        /// این فیلد برای غیرفعال کردن حساب‌های کاربری قدیمی یا مسدود شده استفاده می‌شود
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن کاربر
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف کاربر
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که کاربر را حذف کرده است
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
        /// تاریخ و زمان ایجاد کاربر
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که کاربر را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش کاربر
        /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین ویرایش را انجام داده است
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
        /// لیست بیماران مرتبط با این کاربر
        /// این لیست برای نمایش تمام بیمارانی که با این حساب کاربری مرتبط هستند استفاده می‌شود
        /// </summary>
        public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();
        public virtual ICollection<NotificationHistory> NotificationHistories { get; set; }
        /// <summary>
        /// تاریخ آخرین ورود بیمار به سیستم
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// جنسیت بیمار
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        [Required(ErrorMessage = "جنسیت الزامی است.")]
        public Gender Gender { get; set; }

        /// <summary>
        /// آدرس بیمار
        /// </summary>
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Address { get; set; }

        #endregion

        /// <summary>
        /// تولید هویت ادعا مبتنی بر کاربر
        /// این متد برای افزودن ادعاهای سفارشی به هویت کاربر استفاده می‌شود
        /// </summary>
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // افزودن ادعاهای سفارشی
            userIdentity.AddClaim(new Claim("FullName", this.FullName ?? ""));
            userIdentity.AddClaim(new Claim("UserId", this.Id));
            userIdentity.AddClaim(new Claim("IsActive", this.IsActive.ToString()));

            return userIdentity;
        }
    }

    /// <summary>
    /// پیکربندی مدل کاربران سیستم برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ApplicationUserConfig : EntityTypeConfiguration<ApplicationUser>
    {
        public ApplicationUserConfig()
        {
            ToTable("ApplicationUsers");
            HasKey(u => u.Id);



            Property(p => p.Address)
                .IsOptional()
                .HasMaxLength(500);

            // ویژگی‌های اصلی
            Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_FirstName")));

            Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_LastName")));

            Property(u => u.NationalCode)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_NationalCode") { IsUnique = true }));

            Property(u => u.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(u => u.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_IsDeleted")));

            Property(u => u.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(u => u.CreatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_CreatedAt")));

            Property(u => u.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_CreatedByUserId")));

            Property(u => u.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UpdatedAt")));

            Property(u => u.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UpdatedByUserId")));

            Property(u => u.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_DeletedByUserId")));

            Property(p => p.Gender)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_Gender")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UserName") { IsUnique = true }));

            Property(u => u.Email)
                .IsOptional()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_Email")));
            // ✅ اضافه شده: تنظیمات شماره تلفن همراه
            Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_PhoneNumber") { IsUnique = true }));

            HasMany(u => u.Patients)
                .WithRequired(p => p.ApplicationUser)
                .HasForeignKey(p => p.ApplicationUserId)
                .WillCascadeOnDelete(false);

            // ✅ اصلاح شده: رابطه با تاریخچه اطلاع‌رسانی‌ها
            HasMany(u => u.NotificationHistories)
                .WithRequired(n => n.SenderUser)
                .HasForeignKey(n => n.SenderUserId)
                .WillCascadeOnDelete(false);

            HasOptional(u => u.DeletedByUser)
                .WithMany()
                .HasForeignKey(u => u.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(u => u.CreatedByUser)
                .WithMany()
                .HasForeignKey(u => u.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(u => u.UpdatedByUser)
                .WithMany()
                .HasForeignKey(u => u.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(u => new { u.IsActive, u.IsDeleted })
                .HasName("IX_ApplicationUser_IsActive_IsDeleted");
        }
    }

    #endregion

    #region AppRoles
    /// <summary>
    /// این کلاس شامل نام تمام نقش‌های کاربردی (Roles) در سیستم است.
    /// استفاده از این کلاس به جلوگیری از اشتباهات تایپی (مانند تایپ اشتباه نام نقش) کمک می‌کند
    /// و مدیریت نقش‌ها را به صورت متمرکز و قابل نگهداری‌تر می‌سازد.
    /// </summary>
    public static class AppRoles
    {
        #region نقش‌های کاربری
        /// <summary>
        /// نقش مدیر سیستم. کاربران با این نقش دسترسی کامل به تمام بخش‌های سیستم را دارند.
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// نقش پزشک. کاربران با این نقش می‌توانند نوبت‌ها را مشاهده کنند، پرونده بیماران را ویرایش کنند و معاینات را انجام دهند.
        /// </summary>
        public const string Doctor = "Doctor";

        /// <summary>
        /// نقش منشی یا پذیرش. کاربران با این نقش مسئول ثبت بیماران، مدیریت نوبت‌ها و امور اداری هستند.
        /// </summary>
        public const string Receptionist = "Receptionist";

        /// <summary>
        /// نقش بیمار. این نقش برای استفاده در پورتال بیمار در آینده در نظر گرفته شده است.
        /// بیماران می‌توانند نوبت بگیرند، اطلاعات پرونده خود را مشاهده کنند و پیام ارسال کنند.
        /// </summary>
        public const string Patient = "Patient"; // برای استفاده در آینده (پورتال بیمار)

        /// <summary>
        /// نقش کاربر سیستم برای اجرای عملیات پس‌زمینه.
        /// این نقش برای کارهای خودکار سیستم مانند گزارش‌گیری و پشتیبان‌گیری استفاده می‌شود.
        /// </summary>
        public const string System = "System";

        #endregion
    }
    #endregion

    #region Clinic

    /// <summary>
    /// مدل کلینیک‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان → دسته‌بندی خدمات)
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با دپارتمان‌ها و پزشکان برای سازماندهی بهتر خدمات
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class Clinic : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه کلینیک
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// نام کلینیک
        /// مثال: "کلینیک تخصصی دندانپزشکی شفا"
        /// </summary>
        [Required(ErrorMessage = "نام کلینیک الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام کلینیک نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        [MaxLength(500, ErrorMessage = "آدرس کلینیک نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Address { get; set; }

        /// <summary>
        /// شماره تلفن کلینیک
        /// </summary>
        [MaxLength(50, ErrorMessage = "شماره تلفن کلینیک نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن کلینیک
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف کلینیک
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که کلینیک را حذف کرده است
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
        /// تاریخ و زمان ایجاد کلینیک
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که کلینیک را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین بروزرسانی کلینیک
        /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
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
        /// لیست دپارتمان‌های مرتبط با این کلینیک
        /// این لیست برای نمایش تمام دپارتمان‌های موجود در این کلینیک استفاده می‌شود
        /// </summary>
        public virtual ICollection<Department> Departments { get; set; } = new HashSet<Department>();

        /// <summary>
        /// لیست پزشکان مرتبط با این کلینیک
        /// این لیست برای نمایش تمام پزشکان فعال در این کلینیک استفاده می‌شود
        /// </summary>
        public virtual ICollection<Doctor> Doctors { get; set; } = new HashSet<Doctor>();

        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        #endregion
    }

    /// <summary>
    /// پیکربندی مدل کلینیک‌ها برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ClinicConfig : EntityTypeConfiguration<Clinic>
    {
        public ClinicConfig()
        {
            ToTable("Clinics");
            HasKey(c => c.ClinicId);

            // ویژگی‌های اصلی
            Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_Name")));

            Property(c => c.Address)
                .IsOptional()
                .HasMaxLength(500);

            Property(c => c.PhoneNumber)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_PhoneNumber")));

            // پیاده‌سازی ISoftDelete
            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_IsDeleted")));

            Property(c => c.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_CreatedAt")));

            Property(c => c.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_CreatedByUserId")));

            Property(c => c.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_UpdatedAt")));

            Property(c => c.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_UpdatedByUserId")));

            Property(c => c.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Clinic_DeletedByUserId")));

            // روابط
            HasMany(c => c.Departments)
                .WithRequired(d => d.Clinic)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            HasMany(c => c.Doctors)
                .WithOptional(d => d.Clinic)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.DeletedByUser)
                .WithMany()
                .HasForeignKey(c => c.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.CreatedByUser)
                .WithMany()
                .HasForeignKey(c => c.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.UpdatedByUser)
                .WithMany()
                .HasForeignKey(c => c.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(c => new { c.IsDeleted, c.CreatedAt })
                .HasName("IX_Clinic_IsDeleted_CreatedAt");
        }
    }

    #endregion

    #region Department

    /// <summary>
    /// نماینده یک دپارتمان در سیستم‌های پزشکی
    /// این کلاس تمام اطلاعات دپارتمان‌ها را شامل می‌شود و برای سیستم‌های پزشکی طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 6. **رابطه Many-to-Many با پزشکان برای انتساب چندگانه**
    /// </summary>
    public class Department : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه دپارتمان
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان
        /// مثال: "دندانپزشکی"، "چشم پزشکی"، "اورولوژی"، "تزریقات"، "اورژانس"
        /// </summary>
        [Required(ErrorMessage = "نام دپارتمان الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// شناسه کلینیک مرتبط با این دپارتمان
        /// این فیلد ارتباط با جدول کلینیک‌ها را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "کلینیک الزامی است.")]
        public int ClinicId { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن دپارتمان
        /// دپارتمان‌های غیرفعال در سیستم نوبت‌دهی نمایش داده نمی‌شوند
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن دپارتمان
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف دپارتمان
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دپارتمان را حذف کرده است
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
        /// تاریخ و زمان ایجاد دپارتمان
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که دپارتمان را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین بروزرسانی دپارتمان
        /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
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
        /// ارجاع به کلینیک مرتبط با این دپارتمان
        /// این ارتباط برای نمایش اطلاعات کلینیک در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Clinic Clinic { get; set; }

        /// <summary>
        /// لیست ارتباطات Many-to-Many با پزشکان
        /// این رابطه برای مشخص کردن اینکه کدام پزشکان در این دپارتمان فعالیت می‌کنند استفاده می‌شود
        /// مثال: دکتر احمدی هم در دپارتمان اورژانس و هم در دپارتمان تزریقات فعال است
        /// </summary>
        public virtual ICollection<DoctorDepartment> DoctorDepartments { get; set; } = new HashSet<DoctorDepartment>();

        /// <summary>
        /// لیست دسته‌بندی‌های خدمات مرتبط با این دپارتمان
        /// این لیست برای نمایش تمام دسته‌بندی‌های خدمات موجود در این دپارتمان استفاده می‌شود
        /// مثال: دپارتمان تزریقات شامل "تزریقات عضلانی"، "تزریقات وریدی" و "تزریقات زیبایی"
        /// </summary>
        public virtual ICollection<ServiceCategory> ServiceCategories { get; set; } = new HashSet<ServiceCategory>();

        public string Description { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی مدل دپارتمان‌ها برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class DepartmentConfig : EntityTypeConfiguration<Department>
    {
        public DepartmentConfig()
        {
            ToTable("Departments");
            HasKey(d => d.DepartmentId);

            // ویژگی‌های اصلی
            Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_Name")));

            Property(d => d.ClinicId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_ClinicId")));

            Property(d => d.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(d => d.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_IsDeleted")));

            Property(d => d.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_CreatedAt")));

            Property(d => d.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_CreatedByUserId")));

            Property(d => d.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_UpdatedAt")));

            Property(d => d.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_UpdatedByUserId")));

            Property(d => d.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_DeletedByUserId")));

            // روابط
            HasRequired(d => d.Clinic)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            // ✅ رابطه Many-to-Many با پزشکان
            HasMany(d => d.DoctorDepartments)
                .WithRequired(dd => dd.Department)
                .HasForeignKey(dd => dd.DepartmentId)
                .WillCascadeOnDelete(false);

            // رابطه با دسته‌بندی خدمات
            HasMany(d => d.ServiceCategories)
                .WithRequired(sc => sc.Department)
                .HasForeignKey(sc => sc.DepartmentId)
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
            HasIndex(d => new { d.ClinicId, d.IsDeleted })
                .HasName("IX_Department_ClinicId_IsDeleted");

            HasIndex(d => new { d.ClinicId, d.IsActive, d.IsDeleted })
                .HasName("IX_Department_ClinicId_IsActive_IsDeleted");
        }
    }

    #endregion

    #region ServiceCategory

    /// <summary>
    /// نماینده یک دسته‌بندی برای گروه‌بندی خدمات بالینی در سیستم‌های پزشکی
    /// این دسته‌بندی‌ها برای سازماندهی خدمات مانند "تزریقات"، "معاینات تخصصی"، "آزمایش‌ها" و سایر خدمات پزشکی استفاده می‌شوند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک -> دپارتمان -> دسته‌بندی خدمات)
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با دپارتمان‌های پزشکی برای سازماندهی بهتر خدمات
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 6. **رابطه Many-to-Many با پزشکان برای انتساب خدمات**
    /// </summary>
    public class ServiceCategory : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه دسته‌بندی خدمات پزشکی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// عنوان دسته‌بندی خدمات پزشکی
        /// مثال: "تزریقات"، "معاینات تخصصی"، "آزمایش‌های تشخیصی"
        /// </summary>
        [Required(ErrorMessage = "عنوان دسته‌بندی الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان دسته‌بندی نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Title { get; set; }

        /// <summary>
        /// شناسه دپارتمان مرتبط با این دسته‌بندی خدمات
        /// این فیلد ارتباط با جدول دپارتمان‌ها را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "دپارتمان الزامی است.")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// آیا دسته‌بندی فعال است؟
        /// این فیلد برای غیرفعال کردن دسته‌بندی‌های قدیمی یا منقضی شده استفاده می‌شود
        /// </summary>
        public bool IsActive { get; set; } = true;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن دسته‌بندی
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف دسته‌بندی
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دسته‌بندی را حذف کرده است
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
        /// تاریخ و زمان ایجاد دسته‌بندی
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که دسته‌بندی را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین بروزرسانی دسته‌بندی
        /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
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
        /// ارجاع به دپارتمان مرتبط با این دسته‌بندی خدمات
        /// این ارتباط برای نمایش اطلاعات دپارتمان در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// لیست خدمات پزشکی مرتبط با این دسته‌بندی
        /// این لیست برای نمایش تمام خدمات موجود در این دسته‌بندی استفاده می‌شود
        /// </summary>
        public virtual ICollection<Service> Services { get; set; } = new HashSet<Service>();

        /// <summary>
        /// لیست ارتباطات Many-to-Many با پزشکان
        /// این رابطه برای مشخص کردن اینکه کدام پزشکان مجاز به ارائه این دسته خدمات هستند استفاده می‌شود
        /// مثال: دکتر احمدی مجاز به ارائه خدمات "تزریقات عضلانی" است ولی مجاز به "تزریق بوتاکس" نیست
        /// </summary>
        public virtual ICollection<DoctorServiceCategory> DoctorServiceCategories { get; set; } = new HashSet<DoctorServiceCategory>();

        public string Description { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی مدل دسته‌بندی خدمات پزشکی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ServiceCategoryConfig : EntityTypeConfiguration<ServiceCategory>
    {
        public ServiceCategoryConfig()
        {
            ToTable("ServiceCategories");
            HasKey(sc => sc.ServiceCategoryId);

            // ویژگی‌های اصلی
            Property(sc => sc.Title)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_Title")));

            Property(sc => sc.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(sc => sc.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_IsDeleted")));

            Property(sc => sc.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(sc => sc.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_CreatedAt")));

            Property(sc => sc.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_CreatedByUserId")));

            Property(sc => sc.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_UpdatedAt")));

            Property(sc => sc.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_UpdatedByUserId")));

            Property(sc => sc.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(sc => sc.DepartmentId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCategory_DepartmentId")));

            // روابط
            HasRequired(sc => sc.Department)
                .WithMany(d => d.ServiceCategories)
                .HasForeignKey(sc => sc.DepartmentId)
                .WillCascadeOnDelete(false);

            // ✅ رابطه Many-to-Many با پزشکان
            HasMany(sc => sc.DoctorServiceCategories)
                .WithRequired(dsc => dsc.ServiceCategory)
                .HasForeignKey(dsc => dsc.ServiceCategoryId)
                .WillCascadeOnDelete(false);

            HasOptional(sc => sc.DeletedByUser)
                .WithMany()
                .HasForeignKey(sc => sc.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(sc => sc.CreatedByUser)
                .WithMany()
                .HasForeignKey(sc => sc.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(sc => sc.UpdatedByUser)
                .WithMany()
                .HasForeignKey(sc => sc.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(sc => new { sc.DepartmentId, sc.IsActive, sc.IsDeleted })
                .HasName("IX_ServiceCategory_DepartmentId_IsActive_IsDeleted");
        }
    }

    #endregion

    #region Service

    /// <summary>
    /// مدل خدمات پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ساختار سازمانی پزشکی (کلینیک -> دپارتمان -> دسته‌بندی خدمات -> خدمات)
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با دسته‌بندی‌های خدمات برای سازماندهی بهتر خدمات
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class Service : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه خدمات پزشکی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// عنوان خدمات پزشکی
        /// مثال: "معاینه اولیه دندانپزشکی"، "تزریق واکسن فصلی"
        /// </summary>
        [Required(ErrorMessage = "عنوان خدمات الزامی است.")]
        [MaxLength(250, ErrorMessage = "عنوان خدمات نمی‌تواند بیش از 250 کاراکتر باشد.")]
        public string Title { get; set; }

        /// <summary>
        /// کد خدمات پزشکی
        /// این کد باید منحصر به فرد باشد و برای شناسایی سریع خدمات استفاده می‌شود
        /// مثال: "DNT-001"، "EYE-105"
        /// </summary>
        [Required(ErrorMessage = "کد خدمات الزامی است.")]
        [MaxLength(50, ErrorMessage = "کد خدمات نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string ServiceCode { get; set; }

        /// <summary>
        /// قیمت پایه خدمات پزشکی
        /// </summary>
        [Required(ErrorMessage = "قیمت الزامی است.")]
        [DataType(DataType.Currency, ErrorMessage = "فرمت قیمت نامعتبر است.")]
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        /// <summary>
        /// توضیحات خدمات پزشکی
        /// </summary>
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمات مرتبط با این خدمات
        /// این فیلد ارتباط با جدول دسته‌بندی‌ها را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "دسته‌بندی خدمات الزامی است.")]
        public int ServiceCategoryId { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن خدمات
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف خدمات
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که خدمات را حذف کرده است
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
        /// تاریخ و زمان ایجاد خدمات
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که خدمات را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین بروزرسانی خدمات
        /// این اطلاعات برای ردیابی تغییرات در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آخرین بروزرسانی را انجام داده است
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
        /// ارجاع به دسته‌بندی خدمات
        /// این ارتباط برای نمایش اطلاعات دسته‌بندی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual ServiceCategory ServiceCategory { get; set; }

        /// <summary>
        /// لیست آیتم‌های پذیرش مرتبط با این خدمات
        /// این لیست برای نمایش تمام آیتم‌های پذیرش مرتبط با این خدمت استفاده می‌شود
        /// </summary>
        public virtual ICollection<ReceptionItem> ReceptionItems { get; set; } = new HashSet<ReceptionItem>();

        /// <summary>
        /// لیست تعرفه‌های بیمه‌ای مرتبط با این خدمات
        /// این لیست برای نمایش تمام تعرفه‌های بیمه‌ای موجود برای این خدمت استفاده می‌شود
        /// </summary>
        public virtual ICollection<InsuranceTariff> Tariffs { get; set; } = new HashSet<InsuranceTariff>();

        public bool IsActive { get; set; }
        public string Notes { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی مدل خدمات پزشکی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ServiceConfig : EntityTypeConfiguration<Service>
    {
        public ServiceConfig()
        {
            ToTable("Services");
            HasKey(s => s.ServiceId);

            // ویژگی‌های اصلی
            Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_Title")));

            Property(s => s.ServiceCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_ServiceCode") { IsUnique = true }));

            Property(s => s.Price)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_Price")));

            Property(s => s.Description)
                .IsOptional()
                .HasMaxLength(1000);

            // پیاده‌سازی ISoftDelete
            Property(s => s.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_IsDeleted")));

            Property(s => s.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(s => s.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_CreatedAt")));

            Property(s => s.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_CreatedByUserId")));

            Property(s => s.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_UpdatedAt")));

            Property(s => s.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_UpdatedByUserId")));

            Property(s => s.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(s => s.ServiceCategoryId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Service_ServiceCategoryId")));

            // روابط
            HasRequired(s => s.ServiceCategory)
                .WithMany(d => d.Services)
                .HasForeignKey(s => s.ServiceCategoryId)
                .WillCascadeOnDelete(false);

            HasMany(s => s.ReceptionItems)
                .WithRequired(ri => ri.Service)
                .HasForeignKey(ri => ri.ServiceId)
                .WillCascadeOnDelete(false);

            HasMany(s => s.Tariffs)
                .WithRequired(t => t.Service)
                .HasForeignKey(t => t.ServiceId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.DeletedByUser)
                .WithMany()
                .HasForeignKey(s => s.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.CreatedByUser)
                .WithMany()
                .HasForeignKey(s => s.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(s => s.UpdatedByUser)
                .WithMany()
                .HasForeignKey(s => s.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(s => new { s.ServiceCategoryId, s.IsDeleted })
                .HasName("IX_Service_ServiceCategoryId_IsDeleted");
        }
    }

    #endregion

    #region Insurance

    /// <summary>
    /// مدل بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سهم پیش‌فرض بیمه و بیمار برای تمام خدمات
    /// 2. مدیریت کامل تعرفه‌های خاص برای هر خدمت
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class Insurance : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه بیمه
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int InsuranceId { get; set; }

        /// <summary>
        /// نام بیمه
        /// مثال: "تأمین اجتماعی"، "بیمه آزاد"، "بیمه نیروهای مسلح"
        /// </summary>
        [Required(ErrorMessage = "نام بیمه الزامی است.")]
        [MaxLength(250, ErrorMessage = "نام بیمه نمی‌تواند بیش از 250 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// توضیحات بیمه
        /// مثال: "توضیحات کامل درباره شرایط و محدودیت‌های بیمه"
        /// </summary>
        [MaxLength(1000, ErrorMessage = "توضیحات بیمه نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// سهم پیش‌فرض بیمار به درصد
        /// مثال: 30 = 30%
        /// این مقدار برای تمام خدمات اعمال می‌شود مگر اینکه برای خدمت خاص تعرفه متفاوتی تعریف شده باشد
        /// </summary>
        [Required(ErrorMessage = "سهم پیش‌فرض بیمار الزامی است.")]
        [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
        public decimal DefaultPatientShare { get; set; }

        /// <summary>
        /// سهم پیش‌فرض بیمه به درصد
        /// مثال: 70 = 70%
        /// این مقدار برای تمام خدمات اعمال می‌شود مگر اینکه برای خدمت خاص تعرفه متفاوتی تعریف شده باشد
        /// </summary>
        [Required(ErrorMessage = "سهم پیش‌فرض بیمه الزامی است.")]
        [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
        public decimal DefaultInsurerShare { get; set; }

        /// <summary>
        /// آیا بیمه فعال است؟
        /// این فیلد برای غیرفعال کردن بیمه‌های قدیمی یا منقضی شده استفاده می‌شود
        /// </summary>
        public bool IsActive { get; set; } = true;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن بیمه
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف بیمه
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که بیمه را حذف کرده است
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
        /// تاریخ و زمان ایجاد بیمه
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که بیمه را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش بیمه
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که بیمه را ویرایش کرده است
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
        /// لیست تعرفه‌های بیمه
        /// این لیست برای نمایش تمام تعرفه‌های خدمات موجود برای این بیمه استفاده می‌شود
        /// </summary>
        public virtual ICollection<InsuranceTariff> Tariffs { get; set; } = new HashSet<InsuranceTariff>();

        /// <summary>
        /// لیست بیماران تحت پوشش این بیمه
        /// این لیست برای نمایش تمام بیمارانی که تحت پوشش این بیمه هستند استفاده می‌شود
        /// </summary>
        public virtual ICollection<Patient> Patients { get; set; } = new HashSet<Patient>();

        /// <summary>
        /// لیست پذیرش‌های مرتبط با این بیمه
        /// این لیست برای گزارش‌گیری و تحلیل‌های مالی استفاده می‌شود
        /// </summary>
        public virtual ICollection<Reception> Receptions { get; set; } = new HashSet<Reception>();
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل بیمه برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class InsuranceConfig : EntityTypeConfiguration<Insurance>
    {
        public InsuranceConfig()
        {
            ToTable("Insurances");
            HasKey(i => i.InsuranceId);

            // ویژگی‌های اصلی
            Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(250)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_Name")));

            Property(i => i.Description)
                .IsOptional()
                .HasMaxLength(1000);

            // سهم‌های پیش‌فرض
            Property(i => i.DefaultPatientShare)
                .IsRequired()
                .HasPrecision(5, 2);

            Property(i => i.DefaultInsurerShare)
                .IsRequired()
                .HasPrecision(5, 2);

            // وضعیت فعال بودن
            Property(i => i.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(i => i.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_IsDeleted")));

            Property(i => i.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(i => i.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_CreatedAt")));

            Property(i => i.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_CreatedByUserId")));

            Property(i => i.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_UpdatedAt")));

            Property(i => i.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_UpdatedByUserId")));

            Property(i => i.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_DeletedByUserId")));

            // روابط
            HasRequired(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(i => i.DeletedByUser)
                .WithMany()
                .HasForeignKey(i => i.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(i => i.UpdatedByUser)
                .WithMany()
                .HasForeignKey(i => i.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasMany(i => i.Tariffs)
                .WithRequired(t => t.Insurance)
                .HasForeignKey(t => t.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(i => i.Patients)
                .WithRequired(p => p.Insurance)
                .HasForeignKey(p => p.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(i => i.Receptions)
                .WithRequired(r => r.Insurance)
                .HasForeignKey(r => r.InsuranceId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(i => new { i.IsActive, i.IsDeleted })
                .HasName("IX_Insurance_IsActive_IsDeleted");
        }
    }

    #endregion

    #region Patient

    /// <summary>
    /// مدل بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 2. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 3. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 4. پشتیبانی از سیستم بیمه‌ها با توجه به نیازهای سیستم‌های درمانی
    /// 5. مدیریت صحیح کد ملی بیماران با امکان رمزنگاری
    /// </summary>
    public class Patient : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه بیمار
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// این فیلد باید منحصر به فرد باشد و برای شناسایی بیمار استفاده می‌شود
        /// </summary>
        [Required(ErrorMessage = "کد ملی الزامی است.")]
        [MaxLength(10, ErrorMessage = "کد ملی نمی‌تواند بیش از 10 کاراکتر باشد.")]
        public string NationalCode { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی بیمار
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام خانوادگی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string LastName { get; set; }

        /// <summary>
        /// تاریخ تولد بیمار
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// آدرس بیمار
        /// </summary>
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Address { get; set; }

        /// <summary>
        /// شماره تلفن بیمار
        /// </summary>
        [MaxLength(50, ErrorMessage = "شماره تلفن نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string PhoneNumber { get; set; }
        /// <summary>
        /// جنسیت بیمار
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        [Required(ErrorMessage = "جنسیت الزامی است.")]
        public Gender Gender { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// توجه: در سیستم کلینیک شفا، هر بیمار حتماً باید یک بیمه داشته باشد.
        /// در صورتی که بیمار بیمه‌ای را انتخاب نکند، به طور خودکار بیمه "آزاد" به عنوان بیمه پیش‌فرض در نظر گرفته می‌شود.
        /// بیمه "آزاد" به این معنی است که بیمار تمام هزینه‌ها را خودش پرداخت می‌کند (سهم بیمه 0% و سهم بیمار 100%).
        /// </summary>
        [Required(ErrorMessage = "بیمه الزامی است.")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود بیمار به سیستم
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن بیمار
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف بیمار
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که بیمار را حذف کرده است
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
        /// تاریخ و زمان ایجاد بیمار
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که بیمار را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش بیمار
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که بیمار را ویرایش کرده است
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
        /// شناسه کاربر مرتبط با این بیمار
        /// </summary>
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر مرتبط با این بیمار
        /// این ارتباط برای احراز هویت و دسترسی به سیستم ضروری است
        /// </summary>
        public virtual ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// ارجاع به بیمه مرتبط با این بیمار
        /// این ارتباط برای نمایش اطلاعات بیمه بیمار در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Insurance Insurance { get; set; }

        /// <summary>
        /// لیست پذیرش‌های مرتبط با این بیمار
        /// این لیست برای نمایش تمام پذیرش‌های انجام شده توسط این بیمار استفاده می‌شود
        /// </summary>
        public virtual ICollection<Reception> Receptions { get; set; } = new HashSet<Reception>();

        /// <summary>
        /// لیست نوبت‌های مرتبط با این بیمار
        /// این لیست برای نمایش تمام نوبت‌های ثبت شده توسط این بیمار استفاده می‌شود
        /// </summary>
        public virtual ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();



        #endregion
    }

    /// <summary>
    /// پیکربندی مدل بیماران برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class PatientConfig : EntityTypeConfiguration<Patient>
    {
        public PatientConfig()
        {
            ToTable("Patients");
            HasKey(p => p.PatientId);

            // ویژگی‌های اصلی
            Property(p => p.NationalCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_NationalCode") { IsUnique = true }));

            Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_FirstName")));

            Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_LastName")));

            Property(p => p.BirthDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_BirthDate")));

            Property(p => p.Address)
                .IsOptional()
                .HasMaxLength(500);

            Property(p => p.PhoneNumber)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_PhoneNumber")));

            // پیاده‌سازی ISoftDelete
            Property(p => p.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_IsDeleted")));

            Property(p => p.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_CreatedAt")));

            Property(p => p.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_CreatedByUserId")));

            Property(p => p.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_UpdatedAt")));

            Property(p => p.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_UpdatedByUserId")));

            Property(p => p.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(p => p.InsuranceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_InsuranceId")));

            Property(p => p.LastLoginDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Patient_LastLoginDate")));

            // روابط
            HasRequired(p => p.ApplicationUser)
                .WithMany(u => u.Patients)
                .HasForeignKey(p => p.ApplicationUserId)
                .WillCascadeOnDelete(false);

            HasRequired(p => p.Insurance)
                .WithMany(i => i.Patients)
                .HasForeignKey(p => p.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(p => p.Receptions)
                .WithRequired(r => r.Patient)
                .HasForeignKey(r => r.PatientId)
                .WillCascadeOnDelete(false);

            HasMany(p => p.Appointments)
                .WithOptional(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(p => new { p.LastName, p.FirstName })
                .HasName("IX_Patient_LastName_FirstName");

            HasIndex(p => new { p.InsuranceId, p.LastLoginDate })
                .HasName("IX_Patient_InsuranceId_LastLoginDate");
        }
    }

    #endregion

    #region OtpRequest and OtpRequestConfig

    /// <summary>
    /// مدل درخواست‌های کد تأیید (OTP) برای سیستم ورود بدون رمز عبور
    /// این مدل فقط از ITrackable ارث می‌برد و از ISoftDelete استفاده نمی‌کند
    /// زیرا در سیستم OTP نیازی به سیستم حذف نرم نیست
    /// </summary>
    public class OtpRequest : ITrackable
    {
        public int OtpRequestId { get; set; }

        [Required, StringLength(20)] // ✅ افزایش طول به ۲۰ کاراکتر
        public string PhoneNumber { get; set; }

        [Required]
        public string OtpCodeHash { get; set; } // هش کد در اینجا ذخیره می‌شود

        public DateTime RequestTime { get; set; } = DateTime.Now;

        public int AttemptCount { get; set; } = 1;

        public bool IsVerified { get; set; } = false;

        // پیاده‌سازی ITrackable با ارزش‌های پیش‌فرض
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string CreatedByUserId { get; set; }

        public ApplicationUser CreatedByUser { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedByUserId { get; set; }

        public ApplicationUser UpdatedByUser { get; set; }
    }

    /// <summary>
    /// پیکربندی مدل درخواست‌های کد تأیید (OTP) برای Entity Framework
    /// توجه: این مدل فقط از ITrackable ارث می‌برد و از ISoftDelete استفاده نمی‌کند
    /// زیرا در سیستم OTP نیازی به سیستم حذف نرم نیست
    /// </summary>
    public class OtpRequestConfig : EntityTypeConfiguration<OtpRequest>
    {
        public OtpRequestConfig()
        {
            ToTable("OtpRequests");
            HasKey(o => o.OtpRequestId);

            // ویژگی‌های اصلی
            Property(o => o.PhoneNumber)
                .IsRequired()
                .HasMaxLength(11)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_PhoneNumber")));

            Property(o => o.OtpCodeHash)
                .IsRequired();
            Property(p => p.PhoneNumber)
                .HasMaxLength(20) // Set the new maximum length
                .IsRequired();    // You can chain other configurations

            Property(o => o.RequestTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_RequestTime")));

            Property(o => o.AttemptCount)
                .IsRequired();

            Property(o => o.IsVerified)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_IsVerified")));

            // پیاده‌سازی ITrackable
            Property(o => o.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_CreatedAt")));

            Property(o => o.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_CreatedByUserId")));

            Property(o => o.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_UpdatedAt")));

            Property(o => o.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_OtpRequest_UpdatedByUserId")));

            // روابط
            HasOptional(o => o.CreatedByUser)
                .WithMany()
                .HasForeignKey(o => o.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.UpdatedByUser)
                .WithMany()
                .HasForeignKey(o => o.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد
            HasIndex(o => new { o.PhoneNumber, o.IsVerified, o.RequestTime })
                .HasName("IX_OtpRequest_PhoneNumber_IsVerified_RequestTime");
        }
    }

    #endregion


    #region InsuranceTariff

    /// <summary>
    /// مدل تعرفه بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از تعریف تعرفه‌های خاص برای هر خدمت
    /// 2. امکان تعریف قیمت تعرفه‌ای متفاوت از قیمت پایه
    /// 3. امکان تعریف سهم بیمار و بیمه متفاوت از سهم پیش‌فرض
    /// 4. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class InsuranceTariff : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه تعرفه بیمه
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int InsuranceTariffId { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// این فیلد ارتباط با جدول بیمه‌ها را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "بیمه الزامی است.")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// شناسه خدمت
        /// این فیلد ارتباط با جدول خدمات را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "خدمت الزامی است.")]
        public int ServiceId { get; set; }

        /// <summary>
        /// مبلغ مشخص‌شده برای خدمت تحت پوشش این بیمه (اگر null باشد، از قیمت پایه Service استفاده می‌شود).
        /// </summary>
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal? TariffPrice { get; set; }

        /// <summary>
        /// سهم بیمار به درصد (مثلاً 30 = 30%).
        /// اگر null باشد، از سهم پیش‌فرض بیمه استفاده می‌شود.
        /// </summary>
        [Range(0, 100, ErrorMessage = "سهم بیمار باید بین 0 تا 100 درصد باشد.")]
        public decimal? PatientShare { get; set; }

        /// <summary>
        /// سهم بیمه به درصد (مثلاً 70 = 70%).
        /// اگر null باشد، از سهم پیش‌فرض بیمه استفاده می‌شود.
        /// </summary>
        [Range(0, 100, ErrorMessage = "سهم بیمه باید بین 0 تا 100 درصد باشد.")]
        public decimal? InsurerShare { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن تعرفه بیمه
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف تعرفه بیمه
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که تعرفه بیمه را حذف کرده است
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
        /// تاریخ و زمان ایجاد تعرفه
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که تعرفه را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش تعرفه
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که تعرفه را ویرایش کرده است
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
        /// ارجاع به بیمه مرتبط با این تعرفه
        /// این ارتباط برای نمایش اطلاعات بیمه در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Insurance Insurance { get; set; }

        /// <summary>
        /// ارجاع به خدمت مرتبط با این تعرفه
        /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Service Service { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل تعرفه بیمه برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class InsuranceTariffConfig : EntityTypeConfiguration<InsuranceTariff>
    {
        public InsuranceTariffConfig()
        {
            ToTable("InsuranceTariffs");
            HasKey(t => t.InsuranceTariffId);

            // ویژگی‌های اصلی
            Property(t => t.TariffPrice)
                .IsOptional()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_TariffPrice")));

            Property(t => t.PatientShare)
                .IsOptional()
                .HasPrecision(5, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_PatientShare")));

            Property(t => t.InsurerShare)
                .IsOptional()
                .HasPrecision(5, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_InsurerShare")));

            // پیاده‌سازی ISoftDelete
            Property(t => t.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_IsDeleted")));

            Property(t => t.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_CreatedAt")));

            Property(t => t.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_CreatedByUserId")));

            Property(t => t.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_UpdatedAt")));

            Property(t => t.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_UpdatedByUserId")));

            Property(t => t.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(t => t.InsuranceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_InsuranceId")));

            Property(t => t.ServiceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_ServiceId")));

            // روابط
            HasRequired(t => t.Insurance)
                .WithMany(i => i.Tariffs)
                .HasForeignKey(t => t.InsuranceId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.Service)
                .WithMany(s => s.Tariffs)
                .HasForeignKey(t => t.ServiceId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.DeletedByUser)
                .WithMany()
                .HasForeignKey(t => t.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس یونیک برای جلوگیری از تکرار تعرفه برای یک بیمه و خدمت خاص
            HasIndex(t => new { t.InsuranceId, t.ServiceId })
                .IsUnique()
                .HasName("IX_InsuranceTariff_Insurance_Service_Unique");
        }
    }

    #endregion

    #region Doctor

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
        public virtual Clinic Clinic { get; set; }

        /// <summary>
        /// لیست پذیرش‌های مرتبط با این پزشک
        /// این لیست برای نمایش تمام پذیرش‌های انجام شده توسط این پزشک استفاده می‌شود
        /// </summary>
        public virtual ICollection<Reception> Receptions { get; set; } = new HashSet<Reception>();

        /// <summary>
        /// لیست نوبت‌های مرتبط با این پزشک
        /// این لیست برای نمایش تمام نوبت‌های ثبت شده برای این پزشک استفاده می‌شود
        /// </summary>
        public virtual ICollection<Appointment> Appointments { get; set; } = new HashSet<Appointment>();

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
        #endregion
    }

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

    #region DoctorSchedule
    /// <summary>
    /// مدل برنامه کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت برنامه کاری هفتگی پزشکان
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از برنامه‌ریزی نوبت‌دهی هوشمند
    /// </summary>
    public class DoctorSchedule : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه برنامه کاری
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// مدت زمان هر نوبت (به دقیقه)
        /// </summary>
        [Range(5, 120, ErrorMessage = "مدت زمان نوبت باید بین 5 تا 120 دقیقه باشد.")]
        public int AppointmentDuration { get; set; } = 30;

        /// <summary>
        /// زمان شروع پیش‌فرض روز کاری
        /// </summary>
        public TimeSpan? DefaultStartTime { get; set; }

        /// <summary>
        /// زمان پایان پیش‌فرض روز کاری
        /// </summary>
        public TimeSpan? DefaultEndTime { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن برنامه کاری
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// وضعیت حذف نرم
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر آخرین ویرایش کننده
        /// </summary>
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual Doctor Doctor { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
        public virtual ICollection<DoctorWorkDay> WorkDays { get; set; } = new List<DoctorWorkDay>();
    }

    /// <summary>
    /// کانفیگ Entity Framework برای مدل DoctorSchedule
    /// </summary>
    public class DoctorScheduleConfiguration : EntityTypeConfiguration<DoctorSchedule>
    {
        public DoctorScheduleConfiguration()
        {
            // نام جدول
            ToTable("DoctorSchedules");

            // کلید اصلی
            HasKey(ds => ds.ScheduleId);

            // پراپرتی‌های اصلی
            Property(ds => ds.ScheduleId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_ScheduleId")));

            Property(ds => ds.DoctorId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DoctorId")));

            Property(ds => ds.AppointmentDuration)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_AppointmentDuration")));

            Property(ds => ds.DefaultStartTime)
                .IsOptional();

            Property(ds => ds.DefaultEndTime)
                .IsOptional();

            Property(ds => ds.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(ds => ds.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_IsDeleted")));

            Property(ds => ds.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DeletedAt")));

            Property(ds => ds.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_DeletedByUserId")));

            // پیاده‌سازی ITrackable
            Property(ds => ds.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_CreatedAt")));

            Property(ds => ds.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_CreatedByUserId")));

            Property(ds => ds.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_UpdatedAt")));

            Property(ds => ds.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorSchedule_UpdatedByUserId")));

            // روابط
            HasRequired(ds => ds.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(ds => ds.DoctorId)
                .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

            HasOptional(ds => ds.CreatedByUser)
                .WithMany()
                .HasForeignKey(ds => ds.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(ds => ds.UpdatedByUser)
                .WithMany()
                .HasForeignKey(ds => ds.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(ds => ds.DeletedByUser)
                .WithMany()
                .HasForeignKey(ds => ds.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // روابط با WorkDays
            HasMany(ds => ds.WorkDays)
                .WithRequired(wd => wd.Schedule)
                .HasForeignKey(wd => wd.ScheduleId)
                .WillCascadeOnDelete(true); // حذف cascade برای WorkDays

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(ds => new { ds.DoctorId, ds.IsActive })
                .HasName("IX_DoctorSchedule_DoctorId_IsActive");

            HasIndex(ds => new { ds.DoctorId, ds.IsDeleted })
                .HasName("IX_DoctorSchedule_DoctorId_IsDeleted");

            HasIndex(ds => new { ds.CreatedAt, ds.IsDeleted })
                .HasName("IX_DoctorSchedule_CreatedAt_IsDeleted");

            // ایندکس منحصر به فرد برای هر پزشک فقط یک برنامه کاری فعال
            HasIndex(ds => new { ds.DoctorId, ds.IsActive, ds.IsDeleted })
                .HasName("IX_DoctorSchedule_DoctorId_IsActive_IsDeleted_Unique")
                .IsUnique();
        }
    }


    #endregion

    #region DoctorTimeRange
    /// <summary>
    /// مدل بازه زمانی کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت بازه‌های زمانی کاری در هر روز
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// </summary>
    public class DoctorTimeRange : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه بازه زمانی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int TimeRangeId { get; set; }

        /// <summary>
        /// شناسه روز کاری
        /// </summary>
        [Required(ErrorMessage = "روز کاری الزامی است.")]
        public int WorkDayId { get; set; }

        /// <summary>
        /// زمان شروع بازه
        /// </summary>
        [Required(ErrorMessage = "زمان شروع الزامی است.")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان بازه
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// نشان‌دهنده فعال بودن بازه زمانی
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// وضعیت حذف نرم
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر آخرین ویرایش کننده
        /// </summary>
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual DoctorWorkDay WorkDay { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// کانفیگ Entity Framework برای مدل DoctorTimeRange
    /// </summary>
    public class DoctorTimeRangeConfiguration : EntityTypeConfiguration<DoctorTimeRange>
    {
        public DoctorTimeRangeConfiguration()
        {
            // نام جدول
            ToTable("DoctorTimeRanges");

            // کلید اصلی
            HasKey(tr => tr.TimeRangeId);

            // پراپرتی‌های اصلی
            Property(tr => tr.TimeRangeId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_TimeRangeId")));

            Property(tr => tr.WorkDayId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_WorkDayId")));

            Property(tr => tr.StartTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_StartTime")));

            Property(tr => tr.EndTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_EndTime")));

            Property(tr => tr.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(tr => tr.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_IsDeleted")));

            Property(tr => tr.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_DeletedAt")));

            Property(tr => tr.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_DeletedByUserId")));

            // پیاده‌سازی ITrackable
            Property(tr => tr.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_CreatedAt")));

            Property(tr => tr.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_CreatedByUserId")));

            Property(tr => tr.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_UpdatedAt")));

            Property(tr => tr.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeRange_UpdatedByUserId")));

            // روابط
            HasRequired(tr => tr.WorkDay)
                .WithMany(wd => wd.TimeRanges)
                .HasForeignKey(tr => tr.WorkDayId)
                .WillCascadeOnDelete(true); // حذف cascade با WorkDay

            HasOptional(tr => tr.CreatedByUser)
                .WithMany()
                .HasForeignKey(tr => tr.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(tr => tr.UpdatedByUser)
                .WithMany()
                .HasForeignKey(tr => tr.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(tr => tr.DeletedByUser)
                .WithMany()
                .HasForeignKey(tr => tr.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(tr => new { tr.WorkDayId, tr.StartTime })
                .HasName("IX_DoctorTimeRange_WorkDayId_StartTime");

            HasIndex(tr => new { tr.WorkDayId, tr.IsActive })
                .HasName("IX_DoctorTimeRange_WorkDayId_IsActive");

            HasIndex(tr => new { tr.WorkDayId, tr.IsDeleted })
                .HasName("IX_DoctorTimeRange_WorkDayId_IsDeleted");

            HasIndex(tr => new { tr.CreatedAt, tr.IsDeleted })
                .HasName("IX_DoctorTimeRange_CreatedAt_IsDeleted");

            // ایندکس برای جستجوی بازه‌های زمانی همپوشان
            HasIndex(tr => new { tr.WorkDayId, tr.StartTime, tr.EndTime })
                .HasName("IX_DoctorTimeRange_WorkDayId_StartTime_EndTime");
        }
    }


    #endregion

    #region DoctorTimeSlot
    /// <summary>
    /// مدل اسلات زمانی نوبت‌دهی پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت اسلات‌های زمانی قابل رزرو برای نوبت‌دهی
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از وضعیت‌های مختلف نوبت (در دسترس، رزرو شده، تکمیل شده)
    /// </summary>
    public class DoctorTimeSlot : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه اسلات زمانی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int TimeSlotId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// تاریخ نوبت
        /// </summary>
        [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// زمان شروع اسلات
        /// </summary>
        [Required(ErrorMessage = "زمان شروع الزامی است.")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// زمان پایان اسلات
        /// </summary>
        [Required(ErrorMessage = "زمان پایان الزامی است.")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// مدت زمان اسلات (به دقیقه)
        /// </summary>
        [Range(5, 120, ErrorMessage = "مدت زمان اسلات باید بین 5 تا 120 دقیقه باشد.")]
        public int Duration { get; set; }

        /// <summary>
        /// وضعیت اسلات (Available, Booked, Completed, Cancelled, NoShow)
        /// </summary>
        [Required(ErrorMessage = "وضعیت اسلات الزامی است.")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Available;

        /// <summary>
        /// شناسه نوبت (در صورت رزرو شده بودن)
        /// </summary>
        public int? AppointmentId { get; set; }

        /// <summary>
        /// وضعیت حذف نرم
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر آخرین ویرایش کننده
        /// </summary>
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual Doctor Doctor { get; set; }
        public virtual Appointment Appointment { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// کانفیگ Entity Framework برای مدل DoctorTimeSlot
    /// </summary>
    public class DoctorTimeSlotConfiguration : EntityTypeConfiguration<DoctorTimeSlot>
    {
        public DoctorTimeSlotConfiguration()
        {
            // نام جدول
            ToTable("DoctorTimeSlots");

            // کلید اصلی
            HasKey(ts => ts.TimeSlotId);

            // پراپرتی‌های اصلی
            Property(ts => ts.TimeSlotId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_TimeSlotId")));

            Property(ts => ts.DoctorId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DoctorId")));

            Property(ts => ts.AppointmentDate)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_AppointmentDate")));

            Property(ts => ts.StartTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_StartTime")));

            Property(ts => ts.EndTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_EndTime")));

            Property(ts => ts.Duration)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_Duration")));

            Property(ts => ts.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_Status")));

            Property(ts => ts.AppointmentId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_AppointmentId")));

            // پیاده‌سازی ISoftDelete
            Property(ts => ts.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_IsDeleted")));

            Property(ts => ts.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DeletedAt")));

            Property(ts => ts.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_DeletedByUserId")));

            // پیاده‌سازی ITrackable
            Property(ts => ts.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_CreatedAt")));

            Property(ts => ts.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_CreatedByUserId")));

            Property(ts => ts.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_UpdatedAt")));

            Property(ts => ts.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorTimeSlot_UpdatedByUserId")));

            // روابط
            HasRequired(ts => ts.Doctor)
                .WithMany(d => d.TimeSlots)
                .HasForeignKey(ts => ts.DoctorId)
                .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

            HasOptional(ts => ts.Appointment)
                .WithMany()
                .HasForeignKey(ts => ts.AppointmentId)
                .WillCascadeOnDelete(false);

            HasOptional(ts => ts.CreatedByUser)
                .WithMany()
                .HasForeignKey(ts => ts.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(ts => ts.UpdatedByUser)
                .WithMany()
                .HasForeignKey(ts => ts.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(ts => ts.DeletedByUser)
                .WithMany()
                .HasForeignKey(ts => ts.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate })
                .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate");

            HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.Status })
                .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_Status");

            HasIndex(ts => new { ts.DoctorId, ts.IsDeleted })
                .HasName("IX_DoctorTimeSlot_DoctorId_IsDeleted");

            HasIndex(ts => new { ts.AppointmentDate, ts.Status, ts.IsDeleted })
                .HasName("IX_DoctorTimeSlot_AppointmentDate_Status_IsDeleted");

            HasIndex(ts => new { ts.CreatedAt, ts.IsDeleted })
                .HasName("IX_DoctorTimeSlot_CreatedAt_IsDeleted");

            // ایندکس برای جستجوی اسلات‌های خالی
            HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.StartTime, ts.Status })
                .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_Status");

            // ایندکس منحصر به فرد برای جلوگیری از تداخل زمانی
            HasIndex(ts => new { ts.DoctorId, ts.AppointmentDate, ts.StartTime, ts.EndTime, ts.IsDeleted })
                .HasName("IX_DoctorTimeSlot_DoctorId_AppointmentDate_StartTime_EndTime_IsDeleted_Unique")
                .IsUnique();
        }
    }


    #endregion


    #region DoctorWorkDay
    /// <summary>
    /// مدل روز کاری پزشک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت روزهای کاری هفتگی پزشکان
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// </summary>
    public class DoctorWorkDay : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه روز کاری
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int WorkDayId { get; set; }

        /// <summary>
        /// شناسه برنامه کاری پزشک
        /// </summary>
        [Required(ErrorMessage = "برنامه کاری الزامی است.")]
        public int ScheduleId { get; set; }

        /// <summary>
        /// شماره روز هفته (0 = یکشنبه، 1 = دوشنبه، ...، 6 = شنبه)
        /// </summary>
        [Range(0, 6, ErrorMessage = "شماره روز هفته باید بین 0 تا 6 باشد.")]
        public int DayOfWeek { get; set; }

        /// <summary>
        /// نشان‌دهنده فعال بودن روز کاری
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// وضعیت حذف نرم
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر آخرین ویرایش کننده
        /// </summary>
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }

        // Navigation Properties
        public virtual DoctorSchedule Schedule { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
        public virtual ICollection<DoctorTimeRange> TimeRanges { get; set; } = new List<DoctorTimeRange>();
    }

    /// <summary>
    /// کانفیگ Entity Framework برای مدل DoctorWorkDay
    /// </summary>
    public class DoctorWorkDayConfiguration : EntityTypeConfiguration<DoctorWorkDay>
    {
        public DoctorWorkDayConfiguration()
        {
            // نام جدول
            ToTable("DoctorWorkDays");

            // کلید اصلی
            HasKey(wd => wd.WorkDayId);

            // پراپرتی‌های اصلی
            Property(wd => wd.WorkDayId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_WorkDayId")));

            Property(wd => wd.ScheduleId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_ScheduleId")));

            Property(wd => wd.DayOfWeek)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DayOfWeek")));

            Property(wd => wd.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_IsActive")));

            // پیاده‌سازی ISoftDelete
            Property(wd => wd.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_IsDeleted")));

            Property(wd => wd.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DeletedAt")));

            Property(wd => wd.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_DeletedByUserId")));

            // پیاده‌سازی ITrackable
            Property(wd => wd.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_CreatedAt")));

            Property(wd => wd.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_CreatedByUserId")));

            Property(wd => wd.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_UpdatedAt")));

            Property(wd => wd.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorWorkDay_UpdatedByUserId")));

            // روابط
            HasRequired(wd => wd.Schedule)
                .WithMany(ds => ds.WorkDays)
                .HasForeignKey(wd => wd.ScheduleId)
                .WillCascadeOnDelete(true); // حذف cascade با Schedule

            HasOptional(wd => wd.CreatedByUser)
                .WithMany()
                .HasForeignKey(wd => wd.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(wd => wd.UpdatedByUser)
                .WithMany()
                .HasForeignKey(wd => wd.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(wd => wd.DeletedByUser)
                .WithMany()
                .HasForeignKey(wd => wd.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // روابط با TimeRanges
            HasMany(wd => wd.TimeRanges)
                .WithRequired(tr => tr.WorkDay)
                .HasForeignKey(tr => tr.WorkDayId)
                .WillCascadeOnDelete(true); // حذف cascade برای TimeRanges

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(wd => new { wd.ScheduleId, wd.DayOfWeek })
                .HasName("IX_DoctorWorkDay_ScheduleId_DayOfWeek");

            HasIndex(wd => new { wd.ScheduleId, wd.IsActive })
                .HasName("IX_DoctorWorkDay_ScheduleId_IsActive");

            HasIndex(wd => new { wd.ScheduleId, wd.IsDeleted })
                .HasName("IX_DoctorWorkDay_ScheduleId_IsDeleted");

            HasIndex(wd => new { wd.CreatedAt, wd.IsDeleted })
                .HasName("IX_DoctorWorkDay_CreatedAt_IsDeleted");

            // ایندکس منحصر به فرد برای هر برنامه کاری فقط یک روز کاری برای هر روز هفته
            HasIndex(wd => new { wd.ScheduleId, wd.DayOfWeek, wd.IsDeleted })
                .HasName("IX_DoctorWorkDay_ScheduleId_DayOfWeek_IsDeleted_Unique")
                .IsUnique();
        }
    }



    #endregion

    #region DoctorDepartment

    /// <summary>
    /// جدول واسط برای رابطه چند-به-چند بین پزشک و دپارتمان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ارتباط Many-to-Many بین Doctor و Department
    /// 2. مدیریت Audit کامل (کی، کی ایجاد/ویرایش کرده)
    /// 3. کلید ترکیبی (Composite Key) برای جلوگیری از تکرار
    /// 4. پشتیبانی از سناریوهای پیچیده (پزشک در چند دپارتمان)
    /// 
    /// مثال کاربرد:
    /// - دکتر احمدی: عضو دپارتمان "اورژانس" و "تزریقات"
    /// - دکتر محمدی: عضو دپارتمان "داخلی" و "اورژانس"
    /// </summary>
    public class DoctorDepartment : ITrackable
    {
        /// <summary>
        /// شناسه پزشک - بخشی از کلید ترکیبی
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// شناسه دپارتمان - بخشی از کلید ترکیبی
        /// </summary>
        [Required(ErrorMessage = "شناسه دپارتمان الزامی است.")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// نقش یا سمت پزشک در این دپارتمان (اختیاری)
        /// مثال: "رئیس دپارتمان"، "پزشک معاون"، "پزشک عادی"
        /// </summary>
        [MaxLength(100, ErrorMessage = "نقش نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Role { get; set; }

        /// <summary>
        /// وضعیت فعال بودن پزشک در این دپارتمان
        /// می‌تواند پزشک در دپارتمان عضو باشد ولی موقتاً غیرفعال باشد
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ شروع فعالیت پزشک در این دپارتمان
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان فعالیت پزشک در این دپارتمان (در صورت وجود)
        /// </summary>
        public DateTime? EndDate { get; set; }

        #region پیاده‌سازی ITrackable
        /// <summary>
        /// تاریخ و زمان ایجاد این ارتباط
        /// مهم برای ردیابی زمان اضافه شدن پزشک به دپارتمان
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که این ارتباط را ایجاد کرده
        /// مهم برای سیستم‌های پزشکی - چه کسی پزشک را به دپارتمان اضافه کرده
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش این ارتباط
        /// مهم برای ردیابی تغییرات (مثل تغییر نقش، وضعیت فعال/غیرفعال)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که این ارتباط را ویرایش کرده
        /// مهم برای سیستم‌های پزشکی - چه کسی تغییرات را اعمال کرده
        /// </summary>
        public string UpdatedByUserId { get; set; }
        #endregion

        #region Navigation Properties
        /// <summary>
        /// ارجاع به پزشک مرتبط
        /// برای دسترسی مستقیم به اطلاعات پزشک از طریق این رابطه
        /// </summary>
        public virtual Doctor Doctor { get; set; }

        /// <summary>
        /// ارجاع به دپارتمان مرتبط
        /// برای دسترسی مستقیم به اطلاعات دپارتمان از طریق این رابطه
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// برای دسترسی مستقیم به اطلاعات کاربری که این ارتباط را ایجاد کرده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// برای دسترسی مستقیم به اطلاعات کاربری که این ارتباط را ویرایش کرده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای جدول DoctorDepartment
    /// این پیکربندی با استانداردهای سیستم‌های پزشکی طراحی شده است
    /// </summary>
    public class DoctorDepartmentConfig : EntityTypeConfiguration<DoctorDepartment>
    {
        public DoctorDepartmentConfig()
        {
            // تنظیمات جدول
            ToTable("DoctorDepartments");

            // کلید ترکیبی (Composite Key)
            HasKey(dd => new { dd.DoctorId, dd.DepartmentId });

            // تنظیمات Property ها
            Property(dd => dd.DoctorId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_DoctorId")));

            Property(dd => dd.DepartmentId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_DepartmentId")));

            Property(dd => dd.Role)
                .IsOptional()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_Role")));

            Property(dd => dd.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_IsActive")));

            Property(dd => dd.StartDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_StartDate")));

            Property(dd => dd.EndDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_EndDate")));

            // تنظیمات ITrackable
            Property(dd => dd.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_CreatedAt")));

            Property(dd => dd.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_CreatedByUserId")));

            Property(dd => dd.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_UpdatedAt")));

            Property(dd => dd.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorDepartment_UpdatedByUserId")));

            // روابط اصلی
            HasRequired(dd => dd.Doctor)
                .WithMany(d => d.DoctorDepartments)
                .HasForeignKey(dd => dd.DoctorId)
                .WillCascadeOnDelete(false);

            HasRequired(dd => dd.Department)
                .WithMany(d => d.DoctorDepartments)
                .HasForeignKey(dd => dd.DepartmentId)
                .WillCascadeOnDelete(false);

            // روابط Audit
            HasOptional(dd => dd.CreatedByUser)
                .WithMany()
                .HasForeignKey(dd => dd.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(dd => dd.UpdatedByUser)
                .WithMany()
                .HasForeignKey(dd => dd.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(dd => dd.DeletedByUser)
                .WithMany()
                .HasForeignKey(dd => dd.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود کارایی
            HasIndex(dd => new { dd.DoctorId, dd.IsActive })
                .HasName("IX_DoctorDepartment_DoctorId_IsActive");

            HasIndex(dd => new { dd.DepartmentId, dd.IsActive })
                .HasName("IX_DoctorDepartment_DepartmentId_IsActive");

            HasIndex(dd => new { dd.DoctorId, dd.DepartmentId, dd.IsActive })
                .HasName("IX_DoctorDepartment_DoctorId_DepartmentId_IsActive");

            HasIndex(dd => new { dd.StartDate, dd.EndDate })
                .HasName("IX_DoctorDepartment_StartDate_EndDate");
        }
    }

    #endregion

    #region DoctorServiceCategory

    /// <summary>
    /// جدول واسط برای رابطه چند-به-چند بین پزشک و دسته‌بندی خدمات مجاز
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. کنترل دسترسی پزشکان به خدمات خاص
    /// 2. مدیریت صلاحیت‌های پزشکی (Medical Authorization)
    /// 3. کلید ترکیبی (Composite Key) برای جلوگیری از تکرار
    /// 4. پشتیبانی از سناریوهای پیچیده (صلاحیت‌های مختلف)
    /// 
    /// مثال کاربرد:
    /// - دکتر احمدی: مجاز به "تزریقات عضلانی" و "تزریقات وریدی"
    /// - دکتر محمدی: مجاز به "بررسی‌های اولیه" ولی غیرمجاز به "تزریقات زیبایی"
    /// </summary>
    public class DoctorServiceCategory : ITrackable
    {
        /// <summary>
        /// شناسه پزشک - بخشی از کلید ترکیبی
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمات - بخشی از کلید ترکیبی
        /// </summary>
        [Required(ErrorMessage = "شناسه دسته‌بندی خدمات الزامی است.")]
        public int ServiceCategoryId { get; set; }

        /// <summary>
        /// سطح صلاحیت پزشک در این دسته خدمات (اختیاری)
        /// مثال: "مبتدی"، "متوسط"، "پیشرفته"، "متخصص"
        /// </summary>
        [MaxLength(50, ErrorMessage = "سطح صلاحیت نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string AuthorizationLevel { get; set; }

        /// <summary>
        /// وضعیت فعال بودن این صلاحیت
        /// می‌تواند پزشک صلاحیت داشته باشد ولی موقتاً غیرفعال باشد
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ اعطای صلاحیت
        /// </summary>
        public DateTime? GrantedDate { get; set; }

        /// <summary>
        /// تاریخ انقضای صلاحیت (در صورت وجود)
        /// برای خدمات خاص که نیاز به تمدید دارند
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// شماره گواهی یا مجوز (اختیاری)
        /// برای خدمات خاص که نیاز به گواهی‌نامه دارند
        /// </summary>
        [MaxLength(100, ErrorMessage = "شماره گواهی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// توضیحات اضافی در مورد این صلاحیت
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Notes { get; set; }

        #region پیاده‌سازی ITrackable
        /// <summary>
        /// تاریخ و زمان ایجاد این صلاحیت
        /// مهم برای ردیابی زمان اعطای صلاحیت به پزشک
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که این صلاحیت را اعطا کرده
        /// مهم برای سیستم‌های پزشکی - چه کسی صلاحیت را اعطا کرده
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش این صلاحیت
        /// مهم برای ردیابی تغییرات (مثل تغییر سطح، تمدید انقضا)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که این صلاحیت را ویرایش کرده
        /// مهم برای سیستم‌های پزشکی - چه کسی تغییرات را اعمال کرده
        /// </summary>
        public string UpdatedByUserId { get; set; }
        #endregion

        #region Navigation Properties
        /// <summary>
        /// ارجاع به پزشک مرتبط
        /// برای دسترسی مستقیم به اطلاعات پزشک از طریق این صلاحیت
        /// </summary>
        public virtual Doctor Doctor { get; set; }

        /// <summary>
        /// ارجاع به دسته‌بندی خدمات مرتبط
        /// برای دسترسی مستقیم به اطلاعات دسته خدمات از طریق این صلاحیت
        /// </summary>
        public virtual ServiceCategory ServiceCategory { get; set; }

        /// <summary>
        /// ارجاع به کاربر اعطا کننده صلاحیت
        /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را اعطا کرده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده صلاحیت
        /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را ویرایش کرده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده صلاحیت
        /// برای دسترسی مستقیم به اطلاعات کاربری که این صلاحیت را حذف کرده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف این صلاحیت
        /// مهم برای ردیابی زمان حذف صلاحیت از پزشک
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که این صلاحیت را حذف کرده
        /// مهم برای سیستم‌های پزشکی - چه کسی صلاحیت را حذف کرده
        /// </summary>
        public string DeletedByUserId { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای جدول DoctorServiceCategory
    /// این پیکربندی با استانداردهای سیستم‌های پزشکی طراحی شده است
    /// </summary>
    public class DoctorServiceCategoryConfig : EntityTypeConfiguration<DoctorServiceCategory>
    {
        public DoctorServiceCategoryConfig()
        {
            // تنظیمات جدول
            ToTable("DoctorServiceCategories");

            // کلید ترکیبی (Composite Key)
            HasKey(dsc => new { dsc.DoctorId, dsc.ServiceCategoryId });

            // تنظیمات Property ها
            Property(dsc => dsc.DoctorId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DoctorId")));

            Property(dsc => dsc.ServiceCategoryId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_ServiceCategoryId")));

            Property(dsc => dsc.AuthorizationLevel)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_AuthorizationLevel")));

            Property(dsc => dsc.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_IsActive")));

            Property(dsc => dsc.GrantedDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_GrantedDate")));

            Property(dsc => dsc.ExpiryDate)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_ExpiryDate")));

            Property(dsc => dsc.CertificateNumber)
                .IsOptional()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CertificateNumber")));

            Property(dsc => dsc.Notes)
                .IsOptional()
                .HasMaxLength(500);

            // تنظیمات ITrackable
            Property(dsc => dsc.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CreatedAt")));

            Property(dsc => dsc.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_CreatedByUserId")));

            Property(dsc => dsc.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_UpdatedAt")));

            Property(dsc => dsc.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_UpdatedByUserId")));

            // تنظیمات ISoftDelete
            Property(dsc => dsc.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_IsDeleted")));

            Property(dsc => dsc.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DeletedAt")));

            Property(dsc => dsc.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorServiceCategory_DeletedByUserId")));

            // روابط اصلی
            HasRequired(dsc => dsc.Doctor)
                .WithMany(d => d.DoctorServiceCategories)
                .HasForeignKey(dsc => dsc.DoctorId)
                .WillCascadeOnDelete(false);

            HasRequired(dsc => dsc.ServiceCategory)
                .WithMany(sc => sc.DoctorServiceCategories)
                .HasForeignKey(dsc => dsc.ServiceCategoryId)
                .WillCascadeOnDelete(false);

            // روابط Audit
            HasOptional(dsc => dsc.CreatedByUser)
                .WithMany()
                .HasForeignKey(dsc => dsc.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(dsc => dsc.UpdatedByUser)
                .WithMany()
                .HasForeignKey(dsc => dsc.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(dsc => dsc.DeletedByUser)
                .WithMany()
                .HasForeignKey(dsc => dsc.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود کارایی
            HasIndex(dsc => new { dsc.DoctorId, dsc.IsActive })
                .HasName("IX_DoctorServiceCategory_DoctorId_IsActive");

            HasIndex(dsc => new { dsc.ServiceCategoryId, dsc.IsActive })
                .HasName("IX_DoctorServiceCategory_ServiceCategoryId_IsActive");

            HasIndex(dsc => new { dsc.DoctorId, dsc.ServiceCategoryId, dsc.IsActive })
                .HasName("IX_DoctorServiceCategory_DoctorId_ServiceCategoryId_IsActive");

            HasIndex(dsc => new { dsc.ExpiryDate, dsc.IsActive })
                .HasName("IX_DoctorServiceCategory_ExpiryDate_IsActive");

            HasIndex(dsc => new { dsc.AuthorizationLevel, dsc.IsActive })
                .HasName("IX_DoctorServiceCategory_AuthorizationLevel_IsActive");
        }
    }

    #endregion

    #region Notification

    #region Enums
    /// <summary>
    /// انواع کانال‌های اطلاع‌رسانی در سیستم کلینیک
    /// </summary>
    public enum NotificationChannelType
    {
        /// <summary>
        /// ارسال از طریق پیامک
        /// </summary>
        Sms = 1,

        /// <summary>
        /// ارسال از طریق ایمیل
        /// </summary>
        Email = 2,

        /// <summary>
        /// ارسال از طریق پوش‌نوتیفیکیشن موبایل
        /// </summary>
        AppPush = 3,

        /// <summary>
        /// ارسال داخلی از طریق پنل کاربری
        /// </summary>
        InApp = 4
    }

    /// <summary>
    /// وضعیت‌های ارسال پیام در سیستم کلینیک
    /// </summary>
    public enum NotificationStatus
    {
        /// <summary>
        /// پیام در صف ارسال قرار دارد
        /// </summary>
        Queued = 1,

        /// <summary>
        /// در حال ارسال به سرویس‌دهنده
        /// </summary>
        Sending = 2,

        /// <summary>
        /// ارسال با موفقیت انجام شده
        /// </summary>
        Sent = 3,

        /// <summary>
        /// ارسال با خطا مواجه شده
        /// </summary>
        Failed = 4,

        /// <summary>
        /// ارسال لغو شده است
        /// </summary>
        Canceled = 5,

        /// <summary>
        /// ارسال به صورت زمان‌بندی شده
        /// </summary>
        Scheduled = 6
    }

    /// <summary>
    /// سطح اهمیت عملیات‌های تاریخچه انتسابات پزشکان
    /// برای فیلتر کردن و اولویت‌بندی عملیات‌های مهم
    /// </summary>
    public enum AssignmentHistoryImportance
    {
        /// <summary>
        /// عملیات عادی و روزمره
        /// مثال: تغییر جزئی در اطلاعات
        /// </summary>
        Normal = 1,

        /// <summary>
        /// عملیات مهم که نیاز به توجه دارد
        /// مثال: تغییر نقش یا مسئولیت
        /// </summary>
        Important = 2,

        /// <summary>
        /// عملیات بحرانی که نیاز به بررسی فوری دارد
        /// مثال: حذف انتساب، تغییر دپارتمان اصلی
        /// </summary>
        Critical = 3,

        /// <summary>
        /// عملیات امنیتی که نیاز به بررسی دقیق دارد
        /// مثال: تغییرات غیرمجاز، دسترسی‌های جدید
        /// </summary>
        Security = 4
    }
    #endregion

    #region Entities
    /// <summary>
    /// تاریخچه اطلاع‌رسانی برای سیستم کلینیک شفا
    /// این موجودیت برای ذخیره تاریخچه ارسال پیام‌ها و اعلان‌ها استفاده می‌شود
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از چندین کانال ارسال (پیامک، ایمیل، اپلیکیشن)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای رعایت استانداردهای پزشکی
    /// 3. مدیریت کامل وضعیت‌های ارسال (در صف، در حال ارسال، موفق، ناموفق)
    /// 4. ارتباط با موجودیت‌های سیستم (نوبت، پذیرش، بیمار و غیره)
    /// 5. پشتیبانی از ردیابی کامل برای سیستم‌های پزشکی
    /// </summary>
    public class NotificationHistory : ITrackable, ISoftDelete
    {
        /// <summary>
        /// شناسه یکتای تاریخچه
        /// </summary>
        [Key]
        public Guid HistoryId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// شناسه یکتای اطلاع‌رسانی
        /// </summary>
        [Required]
        public Guid NotificationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// نوع کانال ارسال
        /// </summary>
        [Required]
        public NotificationChannelType ChannelType { get; set; }

        /// <summary>
        /// شماره/ایمیل گیرنده
        /// </summary>
        [Required, MaxLength(50)]
        public string Recipient { get; set; }

        /// <summary>
        /// موضوع پیام
        /// </summary>
        [MaxLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// متن اصلی پیام
        /// </summary>
        [Required, MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// زمان ارسال به سرویس‌دهنده
        /// </summary>
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعلی ارسال
        /// </summary>
        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Queued;

        /// <summary>
        /// توضیحات تکمیلی وضعیت
        /// </summary>
        [MaxLength(500)]
        public string StatusDescription { get; set; }

        /// <summary>
        /// شناسه کاربر ارسال‌کننده
        /// </summary>
        [MaxLength(128)]
        public string SenderUserId { get; set; }

        /// <summary>
        /// شناسه موجودیت سیستمی مرتبط
        /// </summary>
        [MaxLength(50)]
        public string RelatedEntityId { get; set; }

        /// <summary>
        /// نوع موجودیت مرتبط (نوبت، پذیرش، بیمار و...)
        /// </summary>
        [MaxLength(100)]
        public string RelatedEntityType { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ارسال
        /// </summary>
        [Required]
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// شناسه پیام در سرویس‌دهنده خارجی
        /// </summary>
        [MaxLength(100)]
        public string ExternalMessageId { get; set; }

        // پیاده‌سازی ISoftDelete
        [Required]
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        [MaxLength(128)]
        public string DeletedByUserId { get; set; }

        // پیاده‌سازی ITrackable
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }

        // روابط
        public virtual ApplicationUser SenderUser { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
    }

    /// <summary>
    /// الگوی اطلاع‌رسانی برای سیستم کلینیک شفا
    /// این موجودیت برای ذخیره الگوهای پیام‌های استاندارد استفاده می‌شود
    /// </summary>
    public class NotificationTemplate : ITrackable
    {
        /// <summary>
        /// کلید یکتای الگو (مقدار ثابت از NotificationTemplates)
        /// </summary>
        [Key, MaxLength(50)]
        public string Key { get; set; }

        /// <summary>
        /// عنوان نمایشی الگو
        /// </summary>
        [Required, MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// توضیحات تکمیلی الگو
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// نوع کانال مورد استفاده
        /// </summary>
        [Required]
        public NotificationChannelType ChannelType { get; set; }

        /// <summary>
        /// متن الگو برای زبان فارسی
        /// </summary>
        [Required, MaxLength(1000)]
        public string PersianTemplate { get; set; }

        /// <summary>
        /// متن الگو برای زبان انگلیسی
        /// </summary>
        [MaxLength(1000)]
        public string EnglishTemplate { get; set; }

        /// <summary>
        /// وضعیت فعال/غیرفعال بودن الگو
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        // پیاده‌سازی ITrackable
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(128)]
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(128)]
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
    }
    #endregion

    #region Configurations
    /// <summary>
    /// پیکربندی مدل تاریخچه اطلاع‌رسانی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class NotificationHistoryConfig : EntityTypeConfiguration<NotificationHistory>
    {
        public NotificationHistoryConfig()
        {
            ToTable("NotificationHistories");
            HasKey(h => h.HistoryId);

            // تنظیمات ویژگی‌های اصلی
            Property(h => h.NotificationId).IsRequired();
            Property(h => h.Recipient).IsRequired().HasMaxLength(50);
            Property(h => h.Subject).HasMaxLength(200);
            Property(h => h.Message).IsRequired().HasMaxLength(1000);
            Property(h => h.SentAt).IsRequired();
            Property(h => h.Status).IsRequired();
            Property(h => h.StatusDescription).HasMaxLength(500);
            Property(h => h.SenderUserId).HasMaxLength(128);
            Property(h => h.RelatedEntityId).HasMaxLength(50);
            Property(h => h.RelatedEntityType).HasMaxLength(100);
            Property(h => h.AttemptCount).IsRequired();
            Property(h => h.ExternalMessageId).HasMaxLength(100);

            // تنظیمات Soft Delete
            Property(h => h.IsDeleted).IsRequired();
            Property(h => h.DeletedAt).IsOptional();
            Property(h => h.DeletedByUserId).HasMaxLength(128);

            // تنظیمات ردیابی
            Property(h => h.CreatedAt).IsRequired();
            Property(h => h.CreatedByUserId).HasMaxLength(128);
            Property(h => h.UpdatedAt).IsOptional();
            Property(h => h.UpdatedByUserId).HasMaxLength(128);

            // ایجاد ایندکس‌ها برای بهینه‌سازی پرس‌وجوها
            HasIndex(h => h.SentAt).IsClustered(false).HasName("IX_NotificationHistory_SentAt");
            HasIndex(h => h.Status).IsClustered(false).HasName("IX_NotificationHistory_Status");
            HasIndex(h => h.Recipient).IsClustered(false).HasName("IX_NotificationHistory_Recipient");
            HasIndex(h => h.RelatedEntityType).IsClustered(false).HasName("IX_NotificationHistory_RelatedEntityType");
            HasIndex(h => h.IsDeleted).IsClustered(false).HasName("IX_NotificationHistory_IsDeleted");
            HasIndex(h => h.CreatedByUserId).IsClustered(false).HasName("IX_NotificationHistory_CreatedByUserId");
            HasIndex(h => h.UpdatedByUserId).IsClustered(false).HasName("IX_NotificationHistory_UpdatedByUserId");

            // ایجاد ایندکس‌های ترکیبی برای پرس‌وجوهای رایج
            HasIndex(h => new { h.SentAt, h.Status, h.IsDeleted })
                .IsClustered(false)
                .HasName("IX_NotificationHistory_SentAt_Status_IsDeleted");

            HasIndex(h => new { h.Recipient, h.Status, h.SentAt })
                .IsClustered(false)
                .HasName("IX_NotificationHistory_Recipient_Status_SentAt");

            // تنظیمات روابط
            HasOptional(h => h.SenderUser)
                .WithMany()
                .HasForeignKey(h => h.SenderUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.DeletedByUser)
                .WithMany()
                .HasForeignKey(h => h.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.CreatedByUser)
                .WithMany()
                .HasForeignKey(h => h.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .WillCascadeOnDelete(false);
        }
    }

    /// <summary>
    /// پیکربندی مدل الگوی اطلاع‌رسانی برای Entity Framework
    /// </summary>
    public class NotificationTemplateConfig : EntityTypeConfiguration<NotificationTemplate>
    {
        public NotificationTemplateConfig()
        {
            ToTable("NotificationTemplates");
            HasKey(t => t.Key);

            // تنظیمات ویژگی‌های اصلی
            Property(t => t.Key).IsRequired().HasMaxLength(50);
            Property(t => t.Title).IsRequired().HasMaxLength(200);
            Property(t => t.Description).HasMaxLength(500);
            Property(t => t.ChannelType).IsRequired();
            Property(t => t.PersianTemplate).IsRequired().HasMaxLength(1000);
            Property(t => t.EnglishTemplate).HasMaxLength(1000);
            Property(t => t.IsActive).IsRequired();

            // تنظیمات ردیابی
            Property(t => t.CreatedAt).IsRequired();
            Property(t => t.CreatedByUserId).HasMaxLength(128);
            Property(t => t.UpdatedAt).IsOptional();
            Property(t => t.UpdatedByUserId).HasMaxLength(128);

            // ایجاد ایندکس‌ها برای بهینه‌سازی
            HasIndex(t => t.ChannelType).IsClustered(false).HasName("IX_NotificationTemplate_ChannelType");
            HasIndex(t => t.IsActive).IsClustered(false).HasName("IX_NotificationTemplate_IsActive");
            HasIndex(t => t.CreatedByUserId).IsClustered(false).HasName("IX_NotificationTemplate_CreatedByUserId");
            HasIndex(t => t.UpdatedByUserId).IsClustered(false).HasName("IX_NotificationTemplate_UpdatedByUserId");

            // تنظیمات روابط
            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Constants
    /// <summary>
    /// کلیدهای ثابت برای الگوهای اطلاع‌رسانی در سیستم کلینیک
    /// </summary>
    public static class NotificationTemplates
    {
        /// <summary>
        /// الگوی ثبت‌نام بیمار جدید
        /// </summary>
        public const string Registration = "Patient_Registration";

        /// <summary>
        /// الگوی تأیید نوبت
        /// </summary>
        public const string AppointmentConfirmation = "Appointment_Confirmation";

        /// <summary>
        /// الگوی یادآوری نوبت
        /// </summary>
        public const string AppointmentReminder = "Appointment_Reminder";

        /// <summary>
        /// الگوی تبریک تولد
        /// </summary>
        public const string BirthdayWish = "Birthday_Wish";

        /// <summary>
        /// الگوی تأیید پرداخت
        /// </summary>
        public const string PaymentConfirmation = "Payment_Confirmation";

        /// <summary>
        /// الگوی تبلیغات خدمات
        /// </summary>
        public const string ServicePromotion = "Service_Promotion";

        /// <summary>
        /// الگوی تغییر برنامه کاری پزشک
        /// </summary>
        public const string DoctorScheduleChange = "Doctor_Schedule_Change";
    }
    #endregion

    #endregion


    #region Reception

    /// <summary>
    /// مدل پذیرش بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح پذیرش بیماران با توجه به استانداردهای سیستم‌های درمانی
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. محاسبه دقیق سهم بیمار و بیمه بر اساس سیستم تعرفه‌ها
    /// </summary>
    public class Reception : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه پذیرش
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ReceptionId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "بیمار الزامی است.")]
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// </summary>
        [Required(ErrorMessage = "بیمه الزامی است.")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// تاریخ پذیرش
        /// </summary>
        [Required(ErrorMessage = "تاریخ پذیرش الزامی است.")]
        public DateTime ReceptionDate { get; set; }

        /// <summary>
        /// جمع کل هزینه‌ها
        /// </summary>
        [Required(ErrorMessage = "جمع کل الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// سهم پرداختی بیمار
        /// </summary>
        [Required(ErrorMessage = "سهم پرداختی بیمار الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal PatientCoPay { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [Required(ErrorMessage = "سهم بیمه الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal InsurerShareAmount { get; set; }

        /// <summary>
        /// وضعیت پذیرش
        /// </summary>
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن پذیرش
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف پذیرش
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که پذیرش را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد پذیرش
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که پذیرش را ایجاد کرده است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش پذیرش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که پذیرش را ویرایش کرده است
        /// </summary>
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// ارجاع به بیمار
        /// این ارتباط برای نمایش اطلاعات بیمار در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Patient Patient { get; set; }

        /// <summary>
        /// ارجاع به پزشک
        /// این ارتباط برای نمایش اطلاعات پزشک در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Doctor Doctor { get; set; }

        /// <summary>
        /// ارجاع به بیمه
        /// این ارتباط برای نمایش اطلاعات بیمه در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Insurance Insurance { get; set; }

        /// <summary>
        /// لیست آیتم‌های پذیرش
        /// این لیست برای نمایش تمام خدمات ارائه شده در این پذیرش استفاده می‌شود
        /// </summary>
        public virtual ICollection<ReceptionItem> ReceptionItems { get; set; } = new HashSet<ReceptionItem>();

        /// <summary>
        /// لیست تراکنش‌های مالی
        /// این لیست برای نمایش تمام پرداخت‌ها و تراکنش‌های مالی مرتبط با این پذیرش استفاده می‌شود
        /// </summary>
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();

        /// <summary>
        /// لیست چاپ‌های رسید
        /// این لیست برای نمایش تمام چاپ‌های رسید مرتبط با این پذیرش استفاده می‌شود
        /// </summary>
        public virtual ICollection<ReceiptPrint> ReceiptPrints { get; set; } = new HashSet<ReceiptPrint>();
        #endregion

        // محاسبه هوشمند پرداخت
        [NotMapped]
        public bool IsPaid => Transactions?.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount) >= TotalAmount;
    }

    /// <summary>
    /// پیکربندی مدل پذیرش بیماران برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ReceptionConfig : EntityTypeConfiguration<Reception>
    {
        public ReceptionConfig()
        {
            ToTable("Receptions");
            HasKey(r => r.ReceptionId);

            // ویژگی‌های اصلی
            Property(r => r.ReceptionDate)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_ReceptionDate")));

            Property(r => r.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.PatientCoPay)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.InsurerShareAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_Status")));

            // پیاده‌سازی ISoftDelete
            Property(r => r.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_IsDeleted")));

            Property(r => r.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(r => r.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_CreatedAt")));

            Property(r => r.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_CreatedByUserId")));

            Property(r => r.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_UpdatedAt")));

            Property(r => r.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_UpdatedByUserId")));

            Property(r => r.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(r => r.PatientId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_PatientId")));

            Property(r => r.DoctorId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_DoctorId")));

            Property(r => r.InsuranceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_InsuranceId")));

            // روابط
            HasRequired(r => r.Patient)
                .WithMany(p => p.Receptions)
                .HasForeignKey(r => r.PatientId)
                .WillCascadeOnDelete(false);

            HasRequired(r => r.Doctor)
                .WithMany(d => d.Receptions)
                .HasForeignKey(r => r.DoctorId)
                .WillCascadeOnDelete(false);

            HasRequired(r => r.Insurance)
                .WithMany(i => i.Receptions)
                .HasForeignKey(r => r.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(r => r.ReceptionItems)
                .WithRequired(ri => ri.Reception)
                .HasForeignKey(ri => ri.ReceptionId)
                .WillCascadeOnDelete(true);

            HasMany(r => r.Transactions)
                .WithRequired(t => t.Reception)
                .HasForeignKey(r => r.ReceptionId)
                .WillCascadeOnDelete(false);

            HasMany(r => r.ReceiptPrints)
                .WithRequired(rp => rp.Reception)
                .HasForeignKey(rp => rp.ReceptionId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(r => new { r.PatientId, r.ReceptionDate, r.Status })
                .HasName("IX_Reception_PatientId_Date_Status");

            HasIndex(r => new { r.DoctorId, r.ReceptionDate, r.Status })
                .HasName("IX_Reception_DoctorId_Date_Status");

            HasIndex(r => new { r.InsuranceId, r.ReceptionDate, r.Status })
                .HasName("IX_Reception_InsuranceId_Date_Status");
        }
    }

    #endregion

    #region ReceptionItem

    /// <summary>
    /// مدل آیتم‌های پذیرش - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح آیتم‌های پذیرش با توجه به استانداردهای سیستم‌های درمانی
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. محاسبه دقیق سهم بیمار و بیمه برای هر خدمت
    /// </summary>
    public class ReceptionItem : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه آیتم پذیرش
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ReceptionItemId { get; set; }

        /// <summary>
        /// شناسه پذیرش مرتبط
        /// </summary>
        [Required(ErrorMessage = "پذیرش الزامی است.")]
        public int ReceptionId { get; set; }

        /// <summary>
        /// شناسه خدمت مرتبط
        /// </summary>
        [Required(ErrorMessage = "خدمت الزامی است.")]
        public int ServiceId { get; set; }

        /// <summary>
        /// تعداد
        /// </summary>
        [Required(ErrorMessage = "تعداد الزامی است.")]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// قیمت هر واحد
        /// </summary>
        [Required(ErrorMessage = "قیمت هر واحد الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// مبلغ سهم بیمار
        /// </summary>
        [Required(ErrorMessage = "مبلغ سهم بیمار الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal PatientShareAmount { get; set; }

        /// <summary>
        /// مبلغ سهم بیمه
        /// </summary>
        [Required(ErrorMessage = "مبلغ سهم بیمه الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal InsurerShareAmount { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن آیتم پذیرش
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف آیتم پذیرش
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آیتم پذیرش را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد آیتم پذیرش
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که آیتم پذیرش را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش آیتم پذیرش
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که آیتم پذیرش را ویرایش کرده است
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
        /// ارجاع به پذیرش مرتبط
        /// این ارتباط برای نمایش اطلاعات پذیرش در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Reception Reception { get; set; }

        /// <summary>
        /// ارجاع به خدمت مرتبط
        /// این ارتباط برای نمایش اطلاعات خدمت در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Service Service { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل آیتم‌های پذیرش برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ReceptionItemConfig : EntityTypeConfiguration<ReceptionItem>
    {
        public ReceptionItemConfig()
        {
            ToTable("ReceptionItems");
            HasKey(ri => ri.ReceptionItemId);

            // ویژگی‌های اصلی
            Property(ri => ri.Quantity)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_Quantity")));

            Property(ri => ri.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UnitPrice")));

            Property(ri => ri.PatientShareAmount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_PatientShareAmount")));

            Property(ri => ri.InsurerShareAmount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_InsurerShareAmount")));

            // پیاده‌سازی ISoftDelete
            Property(ri => ri.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_IsDeleted")));

            Property(ri => ri.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(ri => ri.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_CreatedAt")));

            Property(ri => ri.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_CreatedByUserId")));

            Property(ri => ri.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UpdatedAt")));

            Property(ri => ri.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UpdatedByUserId")));

            Property(ri => ri.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(ri => ri.ReceptionId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_ReceptionId")));

            Property(ri => ri.ServiceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_ServiceId")));

            // روابط
            HasRequired(ri => ri.Reception)
                .WithMany(r => r.ReceptionItems)
                .HasForeignKey(ri => ri.ReceptionId)
                .WillCascadeOnDelete(true);

            HasRequired(ri => ri.Service)
                .WithMany(s => s.ReceptionItems)
                .HasForeignKey(ri => ri.ServiceId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(ri => new { ri.ReceptionId, ri.ServiceId })
                .HasName("IX_ReceptionItem_ReceptionId_ServiceId");

            HasIndex(ri => new { ri.ServiceId, ri.CreatedAt })
                .HasName("IX_ReceptionItem_ServiceId_CreatedAt");
        }
    }

    #endregion

    #region Appointment

    /// <summary>
    /// مدل نوبت‌های پزشکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح نوبت‌های پزشکی با توجه به استانداردهای سیستم‌های درمانی
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی کامل
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط برای استانداردهای پزشکی
    /// 5. پشتیبانی از سیستم پرداخت و یکپارچه‌سازی با تراکنش‌های مالی
    /// </summary>
    public class Appointment : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه نوبت پزشکی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// شناسه پزشک
        /// </summary>
        [Required(ErrorMessage = "پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// شناسه بیمار (اختیاری - ممکن است نوبت توسط منشی ثبت شود)
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// تاریخ و زمان نوبت
        /// </summary>
        [Required(ErrorMessage = "تاریخ نوبت الزامی است.")]
        public DateTime AppointmentDate { get; set; }

        /// <summary>
        /// وضعیت نوبت (Scheduled, Cancelled, InProgress, NeedsAdditionalPayment)
        /// </summary>
        [Required(ErrorMessage = "وضعیت نوبت الزامی است.")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        /// <summary>
        /// مبلغ نوبت
        /// </summary>
        [Required(ErrorMessage = "مبلغ نوبت الزامی است.")]
        [DataType(DataType.Currency, ErrorMessage = "فرمت مبلغ نامعتبر است.")]
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        /// <summary>
        /// شناسه تراکنش پرداخت
        /// </summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>
        /// توضیحات نوبت
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        /// <summary>
        /// آیا بیمار جدید است؟
        /// </summary>
        public bool IsNewPatient { get; set; } = false;

        /// <summary>
        /// نام بیمار (برای نوبت‌های بدون ثبت نام)
        /// </summary>
        [MaxLength(200, ErrorMessage = "نام بیمار نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PatientName { get; set; }

        /// <summary>
        /// شماره تلفن بیمار (برای نوبت‌های بدون ثبت نام)
        /// </summary>
        [MaxLength(20, ErrorMessage = "شماره تلفن بیمار نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string PatientPhone { get; set; }

        /// <summary>
        /// شناسه دسته‌بندی خدمت
        /// </summary>
        public int? ServiceCategoryId { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن نوبت
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// تاریخ و زمان حذف نوبت
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که نوبت را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد نوبت
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که نوبت را ایجاد کرده است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش نوبت
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که نوبت را ویرایش کرده است
        /// </summary>
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// ارجاع به پزشک
        /// </summary>
        public virtual Doctor Doctor { get; set; }

        /// <summary>
        /// ارجاع به بیمار
        /// </summary>
        public virtual Patient Patient { get; set; }

        /// <summary>
        /// ارجاع به تراکنش پرداخت
        /// </summary>
        public virtual PaymentTransaction PaymentTransaction { get; set; }

        /// <summary>
        /// ارجاع به دسته‌بندی خدمت
        /// </summary>
        public virtual ServiceCategory ServiceCategory { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل نوبت‌های پزشکی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class AppointmentConfig : EntityTypeConfiguration<Appointment>
    {
        public AppointmentConfig()
        {
            ToTable("Appointments");
            HasKey(a => a.AppointmentId);

            // ویژگی‌های اصلی
            Property(a => a.AppointmentDate)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_AppointmentDate")));

            Property(a => a.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_Status")));

            Property(a => a.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(a => a.Description)
                .IsOptional()
                .HasMaxLength(500);

            // پیاده‌سازی ISoftDelete
            Property(a => a.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_IsDeleted")));

            Property(a => a.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(a => a.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_CreatedAt")));

            Property(a => a.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_CreatedByUserId")));

            Property(a => a.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_UpdatedAt")));

            Property(a => a.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_UpdatedByUserId")));

            Property(a => a.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(a => a.DoctorId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_DoctorId")));

            Property(a => a.PatientId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_PatientId")));

            Property(a => a.PaymentTransactionId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_PaymentTransactionId")));

            // روابط
            HasRequired(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.PaymentTransaction)
                .WithMany()
                .HasForeignKey(a => a.PaymentTransactionId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.DeletedByUser)
                .WithMany()
                .HasForeignKey(a => a.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.UpdatedByUser)
                .WithMany()
                .HasForeignKey(a => a.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.Status })
                .HasName("IX_Appointment_DoctorId_Date_Status");

            HasIndex(a => new { a.PatientId, a.Status, a.AppointmentDate })
                .HasName("IX_Appointment_PatientId_Status_Date");
        }
    }

    #endregion

    #region PaymentTransaction

    /// <summary>
    /// مدل تراکنش‌های مالی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از انواع روش‌های پرداخت (POS، نقدی، بدهی)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 5. پشتیبانی از انواع پروتکل‌های پوز بانکی
    /// </summary>
    public class PaymentTransaction : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه تراکنش مالی
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int PaymentTransactionId { get; set; }

        /// <summary>
        /// شناسه پذیرش مربوطه
        /// </summary>
        [Required(ErrorMessage = "پذیرش الزامی است.")]
        public int ReceptionId { get; set; }

        /// <summary>
        /// شناسه دستگاه پوز (در صورت استفاده از پوز)
        /// </summary>
        public int? PosTerminalId { get; set; }

        /// <summary>
        /// شناسه شیفت صندوق
        /// </summary>
        [Required(ErrorMessage = "شیفت صندوق الزامی است.")]
        public int CashSessionId { get; set; }

        /// <summary>
        /// مبلغ تراکنش
        /// </summary>
        [Required(ErrorMessage = "مبلغ تراکنش الزامی است.")]
        [DataType(DataType.Currency, ErrorMessage = "فرمت مبلغ نامعتبر است.")]
        [Column(TypeName = "decimal")]
        public decimal Amount { get; set; }

        /// <summary>
        /// روش پرداخت (POS / Cash / Debt)
        /// </summary>
        [Required(ErrorMessage = "روش پرداخت الزامی است.")]
        public PaymentMethod Method { get; set; }

        /// <summary>
        /// وضعیت تراکنش (Pending / Success / Failed / Canceled)
        /// </summary>
        [Required(ErrorMessage = "وضعیت تراکنش الزامی است.")]
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// شماره تراکنش بانکی
        /// </summary>
        [MaxLength(100, ErrorMessage = "شماره تراکنش نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string TransactionId { get; set; }

        /// <summary>
        /// شماره مرجع (RRN)
        /// </summary>
        [MaxLength(100, ErrorMessage = "شماره مرجع نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string ReferenceCode { get; set; }

        /// <summary>
        /// شماره قبض داخلی
        /// </summary>
        [MaxLength(50, ErrorMessage = "شماره قبض نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string ReceiptNo { get; set; }

        /// <summary>
        /// توضیحات تراکنش
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن تراکنش
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف تراکنش
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که تراکنش را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد تراکنش
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که تراکنش را ایجاد کرده است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش تراکنش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که تراکنش را ویرایش کرده است
        /// </summary>
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// ارجاع به پذیرش مربوطه
        /// </summary>
        public virtual Reception Reception { get; set; }

        /// <summary>
        /// ارجاع به دستگاه پوز (در صورت استفاده از پوز)
        /// </summary>
        public virtual PosTerminal PosTerminal { get; set; }

        /// <summary>
        /// ارجاع به شیفت صندوق
        /// </summary>
        public virtual CashSession CashSession { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل تراکنش‌های مالی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class PaymentTransactionConfig : EntityTypeConfiguration<PaymentTransaction>
    {
        public PaymentTransactionConfig()
        {
            ToTable("PaymentTransactions");
            HasKey(t => t.PaymentTransactionId);

            // ویژگی‌های اصلی
            Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Amount")));

            Property(t => t.Method)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Method")));

            Property(t => t.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_Status")));

            Property(t => t.TransactionId)
                .IsOptional()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_TransactionId")));

            Property(t => t.ReferenceCode)
                .IsOptional()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReferenceCode")));

            Property(t => t.ReceiptNo)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReceiptNo")));

            Property(t => t.Description)
                .IsOptional()
                .HasMaxLength(500);

            // پیاده‌سازی ISoftDelete
            Property(t => t.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_IsDeleted")));

            Property(t => t.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CreatedAt")));

            Property(t => t.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CreatedByUserId")));

            Property(t => t.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_UpdatedAt")));

            Property(t => t.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_UpdatedByUserId")));

            Property(t => t.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_DeletedByUserId")));

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            Property(t => t.ReceptionId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReceptionId")));

            Property(t => t.PosTerminalId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_PosTerminalId")));

            Property(t => t.CashSessionId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CashSessionId")));

            // روابط
            HasRequired(t => t.Reception)
                .WithMany(r => r.Transactions)
                .HasForeignKey(t => t.ReceptionId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.PosTerminal)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.PosTerminalId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.CashSession)
                .WithMany(cs => cs.Transactions)
                .HasForeignKey(t => t.CashSessionId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.DeletedByUser)
                .WithMany()
                .HasForeignKey(t => t.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
            HasIndex(t => new { t.CashSessionId, t.Status, t.CreatedAt })
                .HasName("IX_PaymentTransaction_CashSessionId_Status_CreatedAt");

            HasIndex(t => new { t.ReceptionId, t.Status })
                .HasName("IX_PaymentTransaction_ReceptionId_Status");
        }
    }

    #endregion

    #region CashSession

    /// <summary>
    /// مدل شیفت کاری صندوق - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت شیفت‌های صندوق برای گزارش‌گیری روزانه
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات مالی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 5. محاسبه خودکار مانده شیفت
    /// </summary>
    public class CashSession : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه شیفت صندوق
        /// </summary>
        public int CashSessionId { get; set; }

        /// <summary>
        /// شناسه کاربر (منشی صندوق‌دار)
        /// </summary>
        [Required(ErrorMessage = "منشی صندوق‌دار الزامی است.")]
        public string UserId { get; set; }

        /// <summary>
        /// تاریخ و زمان باز شدن شیفت
        /// </summary>
        [Required(ErrorMessage = "تاریخ باز شدن شیفت الزامی است.")]
        public DateTime OpenedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ و زمان بسته شدن شیفت
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// مانده اولیه شیفت
        /// </summary>
        [Required(ErrorMessage = "مانده اولیه الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal OpeningBalance { get; set; } = 0;

        /// <summary>
        /// مانده نقدی شیفت
        /// </summary>
        [Required(ErrorMessage = "مانده نقدی الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal CashBalance { get; set; } = 0;

        /// <summary>
        /// مانده پوز شیفت
        /// </summary>
        [Required(ErrorMessage = "مانده پوز الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal PosBalance { get; set; } = 0;

        /// <summary>
        /// وضعیت شیفت
        /// </summary>
        public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن شیفت
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مالی مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف شیفت
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که شیفت را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد شیفت
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که شیفت را ایجاد کرده است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش شیفت
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که شیفت را ویرایش کرده است
        /// </summary>
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// ارجاع به کاربر (منشی صندوق‌دار)
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// لیست تراکنش‌های مربوط به این شیفت
        /// </summary>
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل شیفت کاری صندوق برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class CashSessionConfig : EntityTypeConfiguration<CashSession>
    {
        public CashSessionConfig()
        {
            ToTable("CashSessions");
            HasKey(cs => cs.CashSessionId);

            // ویژگی‌های اصلی
            Property(cs => cs.UserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_UserId")));

            Property(cs => cs.OpenedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_OpenedAt")));

            Property(cs => cs.ClosedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_ClosedAt")));

            Property(cs => cs.OpeningBalance)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(cs => cs.CashBalance)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(cs => cs.PosBalance)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(cs => cs.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_Status")));

            // پیاده‌سازی ISoftDelete
            Property(cs => cs.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_IsDeleted")));

            Property(cs => cs.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(cs => cs.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_CreatedAt")));

            Property(cs => cs.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_CreatedByUserId")));

            Property(cs => cs.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_UpdatedAt")));

            Property(cs => cs.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_UpdatedByUserId")));

            Property(cs => cs.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_DeletedByUserId")));

            // روابط
            HasRequired(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .WillCascadeOnDelete(false);

            HasMany(cs => cs.Transactions)
                .WithRequired(t => t.CashSession)
                .HasForeignKey(t => t.CashSessionId)
                .WillCascadeOnDelete(false);

            HasOptional(cs => cs.DeletedByUser)
                .WithMany()
                .HasForeignKey(cs => cs.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(cs => cs.CreatedByUser)
                .WithMany()
                .HasForeignKey(cs => cs.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(cs => cs.UpdatedByUser)
                .WithMany()
                .HasForeignKey(cs => cs.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(cs => new { cs.UserId, cs.Status, cs.OpenedAt })
                .HasName("IX_CashSession_UserId_Status_OpenedAt");
        }
    }

    #endregion

    #region PosTerminal

    /// <summary>
    /// مدل دستگاه‌های پوز بانکی - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از انواع پروتکل‌های پوز (سامان کیش، آسان پرداخت، به‌پرداخت و...)
    /// 2. مدیریت اتصال به پوزها از طریق IP و MacAddress
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
    /// 4. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class PosTerminal : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه دستگاه پوز
        /// </summary>
        public int PosTerminalId { get; set; }

        /// <summary>
        /// عنوان دستگاه
        /// مثال: "پوز بانک ملی"
        /// </summary>
        [Required(ErrorMessage = "عنوان دستگاه الزامی است.")]
        [MaxLength(100, ErrorMessage = "عنوان دستگاه نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Title { get; set; }

        /// <summary>
        /// شماره ترمینال
        /// </summary>
        [Required(ErrorMessage = "شماره ترمینال الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره ترمینال نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string TerminalId { get; set; }

        /// <summary>
        /// شماره پذیرنده
        /// </summary>
        [Required(ErrorMessage = "شماره پذیرنده الزامی است.")]
        [MaxLength(50, ErrorMessage = "شماره پذیرنده نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string MerchantId { get; set; }

        /// <summary>
        /// شماره سریال دستگاه
        /// </summary>
        [MaxLength(100, ErrorMessage = "شماره سریال نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// آدرس IP دستگاه در شبکه داخلی
        /// </summary>
        [MaxLength(50, ErrorMessage = "آدرس IP نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string IpAddress { get; set; }

        /// <summary>
        /// آدرس MAC دستگاه
        /// </summary>
        [MaxLength(50, ErrorMessage = "آدرس MAC نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string MacAddress { get; set; }

        /// <summary>
        /// نوع ارائه‌دهنده پوز
        /// </summary>
        [Required(ErrorMessage = "نوع ارائه‌دهنده الزامی است.")]
        public PosProviderType Provider { get; set; }

        /// <summary>
        /// آیا دستگاه فعال است؟
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// پروتکل ارتباطی
        /// </summary>
        public PosProtocol Protocol { get; set; } = PosProtocol.Tcp;

        /// <summary>
        /// پورت ارتباطی
        /// </summary>
        public int? Port { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن دستگاه
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف دستگاه
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دستگاه را حذف کرده است
        /// </summary>
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد دستگاه
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که دستگاه را ایجاد کرده است
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش دستگاه
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دستگاه را ویرایش کرده است
        /// </summary>
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// لیست تراکنش‌های انجام شده با این دستگاه
        /// </summary>
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new HashSet<PaymentTransaction>();
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل دستگاه‌های پوز بانکی برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class PosTerminalConfig : EntityTypeConfiguration<PosTerminal>
    {
        public PosTerminalConfig()
        {
            ToTable("PosTerminals");
            HasKey(p => p.PosTerminalId);

            // ویژگی‌های اصلی
            Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_Title")));

            Property(p => p.TerminalId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_TerminalId")));

            Property(p => p.MerchantId)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_MerchantId")));

            Property(p => p.SerialNumber)
                .IsOptional()
                .HasMaxLength(100);

            Property(p => p.IpAddress)
                .IsOptional()
                .HasMaxLength(50);

            Property(p => p.MacAddress)
                .IsOptional()
                .HasMaxLength(50);

            Property(p => p.Provider)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_Provider")));

            Property(p => p.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_IsActive")));

            Property(p => p.Protocol)
                .IsRequired();

            Property(p => p.Port)
                .IsOptional();

            // پیاده‌سازی ISoftDelete
            Property(p => p.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_IsDeleted")));

            Property(p => p.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_CreatedAt")));

            Property(p => p.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_CreatedByUserId")));

            Property(p => p.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_UpdatedAt")));

            Property(p => p.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_UpdatedByUserId")));

            Property(p => p.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_DeletedByUserId")));

            // روابط
            HasMany(p => p.Transactions)
                .WithOptional(t => t.PosTerminal)
                .HasForeignKey(t => t.PosTerminalId)
                .WillCascadeOnDelete(false);

            HasOptional(p => p.DeletedByUser)
                .WithMany()
                .HasForeignKey(p => p.DeletedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(p => p.UpdatedByUser)
                .WithMany()
                .HasForeignKey(p => p.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(p => new { p.IsActive, p.Provider })
                .HasName("IX_PosTerminal_IsActive_Provider");
        }
    }

    #endregion

    #region ReceiptPrint

    /// <summary>
    /// مدل چاپ رسیدهای پذیرش در کلینیک شفا
    /// این مدل برای ذخیره اطلاعات چاپ رسیدهای پذیرش (فیش‌های پرداخت) استفاده می‌شود
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 2. مدیریت صحیح فیلدهای ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده و مدیریت ردیابی دقیق
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 5. پشتیبانی از ذخیره‌سازی محتوای کامل رسید برای مراجعات بعدی
    /// </summary>
    public class ReceiptPrint : ISoftDelete, ITrackable
    {
        /// <summary>
        /// شناسه چاپ رسید
        /// این شناسه به صورت خودکار توسط سیستم تولید می‌شود
        /// </summary>
        public int ReceiptPrintId { get; set; }

        /// <summary>
        /// شناسه پذیرش مرتبط با این رسید
        /// این فیلد ارتباط با جدول پذیرش‌ها را برقرار می‌کند
        /// </summary>
        [Required(ErrorMessage = "پذیرش الزامی است.")]
        public int ReceptionId { get; set; }

        /// <summary>
        /// محتوای کامل رسید (حداکثر 1000 کاراکتر کافی برای رسیدهای پزشکی)
        /// </summary>
        [Required(ErrorMessage = "محتوای رسید الزامی است.")]
        [MaxLength(1000, ErrorMessage = "محتوای رسید نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string ReceiptContent { get; set; }

        /// <summary>
        /// هش محتوای رسید برای ایندکس‌گذاری
        /// این فیلد برای جستجوی سریع و یکتایی استفاده می‌شود
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string ReceiptHash { get; set; }
        /// <summary>
        /// تاریخ چاپ رسید
        /// </summary>
        [Required(ErrorMessage = "تاریخ چاپ الزامی است.")]
        public DateTime PrintDate { get; set; } = DateTime.Now;

        /// <summary>
        /// نام کاربری که رسید را چاپ کرده است
        /// این فیلد برای نمایش در UI استفاده می‌شود
        /// </summary>
        [MaxLength(250, ErrorMessage = "نام کاربری چاپ‌کننده نمی‌تواند بیش از 250 کاراکتر باشد.")]
        public string PrintedBy { get; set; }

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن رسید
        /// در سیستم‌های پزشکی، حذف فیزیکی اطلاعات مجاز نیست و از سیستم حذف نرم استفاده می‌شود
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف رسید
        /// این اطلاعات برای ردیابی عملیات‌های حساس در سیستم‌های پزشکی حیاتی است
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که رسید را حذف کرده است
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
        /// تاریخ و زمان ایجاد چاپ رسید
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربری که چاپ رسید را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }
        /// <summary>
        /// شناسه کاربری که رسید را چاپ کرده است
        /// این فیلد برای ارتباط با کاربر چاپ‌کننده ضروری است
        /// </summary>
        public string PrintedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش چاپ رسید
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که چاپ رسید را ویرایش کرده است
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
        /// ارجاع به پذیرش مرتبط با این رسید
        /// این ارتباط برای نمایش اطلاعات پذیرش در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Reception Reception { get; set; }

        /// <summary>
        /// ارجاع به کاربر چاپ‌کننده
        /// این ارتباط برای نمایش اطلاعات کاربر چاپ‌کننده در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual ApplicationUser PrintedByUser { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل چاپ رسیدها برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ReceiptPrintConfig : EntityTypeConfiguration<ReceiptPrint>
    {
        public ReceiptPrintConfig()
        {
            ToTable("ReceiptPrints");
            HasKey(rp => rp.ReceiptPrintId);

            // ویژگی‌های اصلی - اصلاح شده برای رفع خطا
            Property(rp => rp.ReceiptContent)
                .IsRequired()
                .HasMaxLength(1000) // تغییر از IsMaxLength() به MaxLength(1000)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_ReceiptContent")));
            // افزودن ستون Hash برای جستجوهای پیشرفته
            Property(rp => rp.ReceiptHash)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_ReceiptHash") { IsUnique = true }));

            Property(rp => rp.PrintDate)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_PrintDate")));

            Property(rp => rp.PrintedBy)
                .IsOptional()
                .HasMaxLength(250)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_PrintedBy")));

            // پیاده‌سازی ISoftDelete
            Property(rp => rp.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_IsDeleted")));

            Property(rp => rp.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(rp => rp.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_CreatedAt")));

            Property(rp => rp.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_CreatedByUserId")));

            Property(rp => rp.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_UpdatedAt")));

            Property(rp => rp.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_UpdatedByUserId")));

            Property(rp => rp.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceiptPrint_DeletedByUserId")));

            // روابط
            HasRequired(rp => rp.Reception)
                .WithMany(r => r.ReceiptPrints)
                .HasForeignKey(rp => rp.ReceptionId)
                .WillCascadeOnDelete(false); // حتماً false برای رعایت استانداردهای پزشکی

            HasOptional(rp => rp.PrintedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.PrintedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(rp => rp.CreatedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(rp => rp.UpdatedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(rp => rp.DeletedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
            HasIndex(rp => new { rp.ReceptionId, rp.PrintDate })
                .HasName("IX_ReceiptPrint_ReceptionId_PrintDate");

            HasIndex(rp => new { rp.PrintDate, rp.IsDeleted })
                .HasName("IX_ReceiptPrint_PrintDate_IsDeleted");

            HasIndex(rp => new { rp.CreatedAt, rp.IsDeleted })
                .HasName("IX_ReceiptPrint_CreatedAt_IsDeleted");
        }
    }

    #endregion

    #region Doctor Assignment History (تاریخچه انتسابات پزشکان)

    /// <summary>
    /// موجودیت تاریخچه انتسابات پزشکان
    /// این موجودیت برای ردیابی کامل تمام تغییرات انتسابات پزشکان طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ردیابی کامل تمام عملیات انتساب (ایجاد، ویرایش، حذف، انتقال)
    /// 2. ذخیره جزئیات کامل هر عملیات برای حسابرسی
    /// 3. ارتباط با موجودیت‌های مرتبط (پزشک، دپارتمان، کاربر)
    /// 4. پشتیبانی از سیستم حذف نرم برای حفظ اطلاعات تاریخی
    /// 5. ردیابی کامل برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این موجودیت برای رعایت استانداردهای پزشکی و حسابرسی طراحی شده است
    /// </summary>
    public class DoctorAssignmentHistory : ITrackable, ISoftDelete
    {
        /// <summary>
        /// شناسه یکتای تاریخچه
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه پزشک مرتبط
        /// </summary>
        [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
        public int DoctorId { get; set; }

        /// <summary>
        /// نوع عملیات انجام شده
        /// مثال: "Create", "Update", "Delete", "Transfer", "Assign", "Remove"
        /// </summary>
        [Required(ErrorMessage = "نوع عملیات الزامی است.")]
        [MaxLength(50, ErrorMessage = "نوع عملیات نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string ActionType { get; set; }

        /// <summary>
        /// عنوان عملیات برای نمایش
        /// مثال: "انتساب به دپارتمان اورژانس", "انتقال به دپارتمان داخلی"
        /// </summary>
        [Required(ErrorMessage = "عنوان عملیات الزامی است.")]
        [MaxLength(200, ErrorMessage = "عنوان عملیات نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string ActionTitle { get; set; }

        /// <summary>
        /// توضیحات تفصیلی عملیات
        /// شامل جزئیات کامل تغییرات انجام شده
        /// </summary>
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string ActionDescription { get; set; }

        /// <summary>
        /// تاریخ و زمان انجام عملیات
        /// </summary>
        [Required(ErrorMessage = "تاریخ عملیات الزامی است.")]
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// شناسه دپارتمان مرتبط (اختیاری)
        /// برای عملیات‌هایی که مربوط به دپارتمان خاصی هستند
        /// </summary>
        public int? DepartmentId { get; set; }

        /// <summary>
        /// نام دپارتمان در زمان عملیات
        /// برای حفظ اطلاعات تاریخی حتی اگر دپارتمان حذف شود
        /// </summary>
        [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string DepartmentName { get; set; }

        /// <summary>
        /// صلاحیت‌های خدماتی مرتبط
        /// ذخیره به صورت JSON string برای حفظ اطلاعات تاریخی
        /// </summary>
        [MaxLength(2000, ErrorMessage = "صلاحیت‌های خدماتی نمی‌تواند بیش از 2000 کاراکتر باشد.")]
        public string ServiceCategories { get; set; }

        /// <summary>
        /// شناسه کاربر انجام‌دهنده عملیات
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر انجام‌دهنده الزامی است.")]
        [MaxLength(128, ErrorMessage = "شناسه کاربر نمی‌تواند بیش از 128 کاراکتر باشد.")]
        public string PerformedByUserId { get; set; }

        /// <summary>
        /// نام کاربر انجام‌دهنده
        /// برای حفظ اطلاعات تاریخی
        /// </summary>
        [MaxLength(200, ErrorMessage = "نام کاربر نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string PerformedByUserName { get; set; }

        /// <summary>
        /// یادداشت‌های اضافی
        /// برای توضیحات تکمیلی یا دلایل انجام عملیات
        /// </summary>
        [MaxLength(1000, ErrorMessage = "یادداشت نمی‌تواند بیش از 1000 کاراکتر باشد.")]
        public string Notes { get; set; }

        /// <summary>
        /// داده‌های قبلی (قبل از تغییر)
        /// ذخیره به صورت JSON string برای مقایسه
        /// </summary>
        [MaxLength(4000, ErrorMessage = "داده‌های قبلی نمی‌تواند بیش از 4000 کاراکتر باشد.")]
        public string PreviousData { get; set; }

        /// <summary>
        /// داده‌های جدید (بعد از تغییر)
        /// ذخیره به صورت JSON string برای مقایسه
        /// </summary>
        [MaxLength(4000, ErrorMessage = "داده‌های جدید نمی‌تواند بیش از 4000 کاراکتر باشد.")]
        public string NewData { get; set; }

        /// <summary>
        /// سطح اهمیت عملیات
        /// برای فیلتر کردن عملیات‌های مهم
        /// </summary>
        [Required(ErrorMessage = "سطح اهمیت الزامی است.")]
        public AssignmentHistoryImportance Importance { get; set; } = AssignmentHistoryImportance.Normal;

        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        /// <summary>
        /// نشان‌دهنده وضعیت حذف شدن رکورد
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ و زمان حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف‌کننده
        /// </summary>
        [MaxLength(128, ErrorMessage = "شناسه کاربر حذف‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
        public string DeletedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر حذف‌کننده
        /// </summary>
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد رکورد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر ایجاد‌کننده
        /// </summary>
        [MaxLength(128, ErrorMessage = "شناسه کاربر ایجاد‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد‌کننده
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// تاریخ و زمان آخرین ویرایش
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ویرایش‌کننده
        /// </summary>
        [MaxLength(128, ErrorMessage = "شناسه کاربر ویرایش‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
        public string UpdatedByUserId { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش‌کننده
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        #region روابط
        /// <summary>
        /// ارجاع به پزشک مرتبط
        /// </summary>
        public virtual Doctor Doctor { get; set; }

        /// <summary>
        /// ارجاع به دپارتمان مرتبط
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// ارجاع به کاربر انجام‌دهنده
        /// </summary>
        public virtual ApplicationUser PerformedByUser { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل تاریخچه انتسابات پزشکان برای Entity Framework
    /// </summary>
    public class DoctorAssignmentHistoryConfig : EntityTypeConfiguration<DoctorAssignmentHistory>
    {
        public DoctorAssignmentHistoryConfig()
        {
            ToTable("DoctorAssignmentHistories");
            HasKey(h => h.Id);

            // ویژگی‌های اصلی
            Property(h => h.ActionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_ActionType")));

            Property(h => h.ActionTitle)
                .IsRequired()
                .HasMaxLength(200);

            Property(h => h.ActionDescription)
                .IsOptional()
                .HasMaxLength(1000);

            Property(h => h.ActionDate)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_ActionDate")));

            Property(h => h.DepartmentName)
                .IsOptional()
                .HasMaxLength(200);

            Property(h => h.ServiceCategories)
                .IsOptional()
                .HasMaxLength(2000);

            Property(h => h.PerformedByUserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_PerformedByUserId")));

            Property(h => h.PerformedByUserName)
                .IsOptional()
                .HasMaxLength(200);

            Property(h => h.Notes)
                .IsOptional()
                .HasMaxLength(1000);

            Property(h => h.PreviousData)
                .IsOptional()
                .HasMaxLength(4000);

            Property(h => h.NewData)
                .IsOptional()
                .HasMaxLength(4000);

            Property(h => h.Importance)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_Importance")));

            // پیاده‌سازی ISoftDelete
            Property(h => h.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_IsDeleted")));

            Property(h => h.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_DeletedAt")));

            Property(h => h.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_DeletedByUserId")));

            // پیاده‌سازی ITrackable
            Property(h => h.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_CreatedAt")));

            Property(h => h.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_CreatedByUserId")));

            Property(h => h.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_UpdatedAt")));

            Property(h => h.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_UpdatedByUserId")));

            // روابط
            HasRequired(h => h.Doctor)
                .WithMany()
                .HasForeignKey(h => h.DoctorId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.Department)
                .WithMany()
                .HasForeignKey(h => h.DepartmentId)
                .WillCascadeOnDelete(false);

            HasRequired(h => h.PerformedByUser)
                .WithMany()
                .HasForeignKey(h => h.PerformedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.CreatedByUser)
                .WithMany()
                .HasForeignKey(h => h.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.UpdatedByUser)
                .WithMany()
                .HasForeignKey(h => h.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(h => h.DeletedByUser)
                .WithMany()
                .HasForeignKey(h => h.DeletedByUserId)
                .WillCascadeOnDelete(false);

            // ایندکس‌های ترکیبی برای بهبود عملکرد
            HasIndex(h => new { h.DoctorId, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_DoctorId_ActionDate");

            HasIndex(h => new { h.ActionType, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_ActionType_ActionDate");

            HasIndex(h => new { h.DepartmentId, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_DepartmentId_ActionDate");

            HasIndex(h => new { h.PerformedByUserId, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_PerformedByUserId_ActionDate");

            HasIndex(h => new { h.Importance, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_Importance_ActionDate");

            HasIndex(h => new { h.IsDeleted, h.ActionDate })
                .HasName("IX_DoctorAssignmentHistory_IsDeleted_ActionDate");
        }
    }

    #endregion

}