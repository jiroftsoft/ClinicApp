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
using Microsoft.AspNet.Identity;

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
    }

    /// <summary>
    /// رابط استاندارد برای ردیابی تاریخ ایجاد و به‌روزرسانی
    /// </summary>
    public interface ITrackable
    {
        DateTime CreatedAt { get; set; }
        string CreatedByUserId { get; set; }
        DateTime? UpdatedAt { get; set; }
        string UpdatedByUserId { get; set; }
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
        [Display(Name = "ثبت شده")]
        Scheduled = 0,
        [Display(Name = "انجام شده")]
        Completed = 1,
        [Display(Name = "لغو شده")]
        Cancelled = 2,
        [Display(Name = "عدم حضور")]
        NoShow = 3
    }
    #endregion

    #region ApplicationUser
    public class ApplicationUser : IdentityUser, ITrackable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public string CreatedByUserId { get; set; }

        public string UpdatedByUserId { get; set; }

        // روابط دوطرفه کامل
        public virtual ICollection<Doctor> Doctors { get; set; }
        public virtual ICollection<Patient> Patients { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // **Add custom user claims here**
            // This makes the user's first name available everywhere in the application
            userIdentity.AddClaim(new Claim("FullName", this.FullName ?? ""));
            return userIdentity;
        }
    }

    public class ApplicationUserConfig : EntityTypeConfiguration<ApplicationUser>
    {
        public ApplicationUserConfig()
        {
            ToTable("ApplicationUsers");
            HasKey(u => u.Id);

            Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            Property(u => u.LastName).IsRequired().HasMaxLength(100);
            Property(u => u.IsActive).IsRequired();
            Property(u => u.CreatedAt).IsRequired();
            Property(u => u.UpdatedAt).IsOptional();
            Property(n => n.CreatedByUserId).IsOptional();
            Property(n => n.UpdatedByUserId).IsOptional();

            // ایندکس برای عملکرد بهتر در جستجوی کاربران
            Property(u => u.UserName)
                .HasColumnAnnotation("Index", new IndexAnnotation(
                    new IndexAttribute("IX_UserName") { IsUnique = true }));

            // روابط دوطرفه کامل
            HasMany(u => u.Doctors)
                .WithRequired(d => d.ApplicationUser)
                .HasForeignKey(d => d.ApplicationUserId)
                .WillCascadeOnDelete(false);

            HasMany(u => u.Patients)
                .WithRequired(p => p.ApplicationUser)
                .HasForeignKey(p => p.ApplicationUserId)
                .WillCascadeOnDelete(false);

            HasMany(u => u.Notifications)
                .WithRequired(n => n.ApplicationUser)
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);
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
        #endregion
    }
    #endregion

    #region Clinic
    public class Clinic : ISoftDelete
    {
        public int ClinicId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }

        // روابط
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Doctor> Doctors { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public string DeletedById { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClinicConfig : EntityTypeConfiguration<Clinic>
    {
        public ClinicConfig()
        {
            ToTable("Clinics");
            HasKey(c => c.ClinicId);

            Property(c => c.Name).IsRequired().HasMaxLength(200);
            Property(c => c.Address).IsOptional().HasMaxLength(500);
            Property(c => c.PhoneNumber).IsOptional().HasMaxLength(50);

            // Soft Delete
            Property(c => c.IsDeleted).IsRequired();
            Property(c => c.DeletedAt).IsOptional();

            // روابط دوطرفه کامل
            HasMany(c => c.Departments)
                .WithRequired(d => d.Clinic)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            HasMany(c => c.Doctors)
                .WithOptional(d => d.Clinic)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);
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
        /// مثال: "دندانپزشکی"، "چشم پزشکی"، "اورولوژی"
        /// </summary>
        [Required(ErrorMessage = "نام دپارتمان الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string Name { get; set; }

        /// <summary>
        /// شناسه کلینیک مرتبط با این دپارتمان
        /// این فیلد ارتباط با جدول کلینیک‌ها را برقرار می‌کند
        /// </summary>
        public int ClinicId { get; set; }

        /// <summary>
        /// ارجاع به کلینیک مرتبط با این دپارتمان
        /// این ارتباط برای نمایش اطلاعات کلینیک در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Clinic Clinic { get; set; }

        /// <summary>
        /// لیست پزشکان مرتبط با این دپارتمان
        /// این لیست برای نمایش تمام پزشکان موجود در این دپارتمان استفاده می‌شود
        /// </summary>
        public virtual ICollection<Doctor> Doctors { get; set; }

        /// <summary>
        /// لیست دسته‌بندی‌های خدمات مرتبط با این دپارتمان
        /// این لیست برای نمایش تمام دسته‌بندی‌های خدمات موجود در این دپارتمان استفاده می‌شود
        /// </summary>
        public virtual ICollection<ServiceCategory> ServiceCategories { get; set; }

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد دپارتمان
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دپارتمان را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

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
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

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

        public bool IsActive { get; set; }
        #endregion
    }

    public class DepartmentConfig : EntityTypeConfiguration<Department>
    {
        public DepartmentConfig()
        {
            ToTable("Departments");
            HasKey(d => d.DepartmentId);

            Property(d => d.Name).IsRequired().HasMaxLength(200);

            // Soft Delete
            Property(d => d.IsDeleted).IsRequired();
            Property(d => d.DeletedAt).IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(d => d.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_CreatedByUserId")));

            Property(d => d.UpdatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_UpdatedByUserId")));

            Property(d => d.DeletedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Department_DeletedByUserId")));

            // روابط دوطرفه کامل
            HasRequired(d => d.Clinic)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            HasMany(d => d.Doctors)
                .WithOptional(doctor => doctor.Department)
                .HasForeignKey(doctor => doctor.DepartmentId)
                .WillCascadeOnDelete(false);

            // افزودن روابط با کاربران
            HasOptional(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.DeletedByUser)
                .WithMany()
                .HasForeignKey(d => d.DeletedByUserId)
                .WillCascadeOnDelete(false);
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
        public int DepartmentId { get; set; }

        /// <summary>
        /// ارجاع به دپارتمان مرتبط با این دسته‌بندی خدمات
        /// این ارتباط برای نمایش اطلاعات دپارتمان در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Department Department { get; set; }

        /// <summary>
        /// لیست خدمات پزشکی مرتبط با این دسته‌بندی
        /// این لیست برای نمایش تمام خدمات موجود در این دسته‌بندی استفاده می‌شود
        /// </summary>
        public virtual ICollection<Service> Services { get; set; }

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
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد دسته‌بندی
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربری که دسته‌بندی را ایجاد کرده است
        /// این اطلاعات برای سیستم‌های پزشکی بسیار حیاتی است
        /// </summary>
        public string CreatedByUserId { get; set; }

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

        public bool IsActive { get; set; }

        /// <summary>
        /// ارجاع به کاربر ایجاد کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ایجاد کننده ضروری است
        /// </summary>
        public virtual ApplicationUser CreatedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر ویرایش کننده
        /// این ناوبری برای دسترسی مستقیم به اطلاعات کاربر ویرایش کننده ضروری است
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }

        public ApplicationUser DeletedByUser { get; set; }
        #endregion
    }

    public class ServiceCategoryConfig : EntityTypeConfiguration<ServiceCategory>
    {
        public ServiceCategoryConfig()
        {
            ToTable("ServiceCategories");
            HasKey(sc => sc.ServiceCategoryId);

            Property(sc => sc.Title).IsRequired().HasMaxLength(200);

            // Relationship: A Department has many ServiceCategories
            HasRequired(sc => sc.Department)
                .WithMany(d => d.ServiceCategories)
                .HasForeignKey(sc => sc.DepartmentId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.CreatedByUser)
                .WithMany()
                .HasForeignKey(d => d.CreatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.UpdatedByUser)
                .WithMany()
                .HasForeignKey(d => d.UpdatedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.DeletedByUser)
                .WithMany()
                .HasForeignKey(d => d.DeletedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Service
    public class Service : ISoftDelete, ITrackable
    {
        public int ServiceId { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [Required, MaxLength(50), Index(IsUnique = true)]
        public string ServiceCode { get; set; }

        [Required, DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int ServiceCategoryId { get; set; }

        // Soft Delete & Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public string CreatedByUserId { get; set; }

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
        public virtual ICollection<ReceptionItem> ReceptionItems { get; set; }

        /// <summary>
        /// لیست تعرفه‌های بیمه‌ای مرتبط با این خدمات
        /// این لیست برای نمایش تمام تعرفه‌های بیمه‌ای موجود برای این خدمت استفاده می‌شود
        /// </summary>
        public virtual ICollection<InsuranceTariff> Tariffs { get; set; }
        #endregion
    }

    public class ServiceConfig : EntityTypeConfiguration<Service>
    {
        public ServiceConfig()
        {
            ToTable("Services");
            HasKey(s => s.ServiceId);

            Property(s => s.Title).IsRequired().HasMaxLength(250);

            Property(s => s.ServiceCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("IX_ServiceCode",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceCode") { IsUnique = true }));

            Property(s => s.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(s => s.Description).IsOptional();

            // Soft Delete & Tracking
            Property(s => s.IsDeleted).IsRequired();
            Property(s => s.DeletedAt).IsOptional();
            Property(s => s.CreatedAt).IsRequired();
            Property(s => s.UpdatedAt).IsOptional();

            // روابط دوطرفه کامل
            HasRequired(s => s.ServiceCategory)
                .WithMany(d => d.Services)
                .HasForeignKey(s => s.ServiceCategoryId)
                .WillCascadeOnDelete(false);

            HasMany(s => s.ReceptionItems)
                .WithRequired(ri => ri.Service)
                .HasForeignKey(ri => ri.ServiceId)
                .WillCascadeOnDelete(false);

            // افزودن رابطه با InsuranceTariff
            HasMany(s => s.Tariffs)
                .WithRequired(t => t.Service)
                .HasForeignKey(t => t.ServiceId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Insurance (تغییرات اصلی برای رفع خطا)
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
        public DateTime CreatedAt { get; set; }

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
        public virtual ICollection<InsuranceTariff> Tariffs { get; set; }

        /// <summary>
        /// لیست بیماران تحت پوشش این بیمه
        /// این لیست برای نمایش تمام بیمارانی که تحت پوشش این بیمه هستند استفاده می‌شود
        /// </summary>
        public virtual ICollection<Patient> Patients { get; set; }

        /// <summary>
        /// لیست پذیرش‌های مرتبط با این بیمه
        /// این لیست برای گزارش‌گیری و تحلیل‌های مالی استفاده می‌شود
        /// </summary>
        public virtual ICollection<Reception> Receptions { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل بیمه برای Entity Framework
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
                .HasMaxLength(250);

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
                .IsRequired();

            // Soft Delete
            Property(i => i.IsDeleted)
                .IsRequired();

            Property(i => i.DeletedAt)
                .IsOptional();

            // Tracking
            Property(i => i.CreatedAt)
                .IsRequired();

            Property(i => i.UpdatedAt)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(i => i.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_CreatedByUserId")));

            Property(i => i.UpdatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_UpdatedByUserId")));

            Property(i => i.DeletedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_DeletedByUserId")));

            // رابطه دوطرفه کامل (تغییر اصلی برای رفع خطا)
            // رابطه با کاربر ایجاد کننده
            HasRequired(i => i.CreatedByUser)
                .WithMany()
                .HasForeignKey(i => i.CreatedByUserId)
                .WillCascadeOnDelete(false);
            HasMany(i => i.Patients)
                .WithRequired(p => p.Insurance)
                .HasForeignKey(p => p.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(i => i.Receptions)
                .WithRequired(r => r.Insurance)
                .HasForeignKey(r => r.InsuranceId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Patient (تغییرات اصلی برای رفع خطا)
    public class Patient : ISoftDelete, ITrackable
    {
        public int PatientId { get; set; }

        [Required, MaxLength(10), Index(IsUnique = true)]
        public string NationalCode { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// توجه: در سیستم کلینیک شفا، هر بیمار حتماً باید یک بیمه داشته باشد.
        /// در صورتی که بیمار بیمه‌ای را انتخاب نکند، به طور خودکار بیمه "آزاد" به عنوان بیمه پیش‌فرض در نظر گرفته می‌شود.
        /// بیمه "آزاد" به این معنی است که بیمار تمام هزینه‌ها را خودش پرداخت می‌کند (سهم بیمه 0% و سهم بیمار 100%).
        /// </summary>
        [Required(ErrorMessage = "بیمه الزامی است.")]
        public int InsuranceId { get; set; }

        // Soft Delete & Tracking
        // Soft Delete & Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string UpdatedByUserId { get; set; }

        public DateTime? LastLoginDate { get; set; }
        // روابط
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Reception> Receptions { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; } // رابط دوطرفه اضافه شده

        /// <summary>
        /// ارجاع به بیمه
        /// این ارتباط برای نمایش اطلاعات بیمه بیمار در سیستم‌های پزشکی ضروری است
        /// </summary>
        public virtual Insurance Insurance { get; set; }

        // ارجاع به کاربران
        public virtual ApplicationUser CreatedByUser { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }
    }

    public class PatientConfig : EntityTypeConfiguration<Patient>
    {
        public PatientConfig()
        {
            ToTable("Patients");
            HasKey(p => p.PatientId);

            Property(p => p.NationalCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnAnnotation("IX_NationalCode",
                    new IndexAnnotation(new IndexAttribute("IX_NationalCode") { IsUnique = true }));

            Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            Property(p => p.LastName).IsRequired().HasMaxLength(100);
            Property(p => p.BirthDate).IsOptional();
            Property(p => p.Address).IsOptional().HasMaxLength(500);
            Property(p => p.PhoneNumber).IsOptional().HasMaxLength(50);

            // Soft Delete & Tracking
            Property(p => p.IsDeleted).IsRequired();
            Property(p => p.DeletedAt).IsOptional();
            Property(p => p.CreatedAt).IsRequired();
            Property(p => p.UpdatedAt).IsOptional();

            // روابط دوطرفه کامل
            HasRequired(p => p.ApplicationUser)
                .WithMany(u => u.Patients)
                .HasForeignKey(p => p.ApplicationUserId)
                .WillCascadeOnDelete(false);

            // تغییر InsuranceId به Required
            HasRequired(p => p.Insurance)
                .WithMany(i => i.Patients)
                .HasForeignKey(p => p.InsuranceId)
                .WillCascadeOnDelete(false);

            HasMany(p => p.Receptions)
                .WithRequired(r => r.Patient)
                .HasForeignKey(r => r.PatientId)
                .WillCascadeOnDelete(false);

            // رابط دوطرفه کامل برای Appointments
            HasMany(p => p.Appointments)
                .WithOptional(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .WillCascadeOnDelete(false);
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
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// </summary>
    public class InsuranceTariff : ITrackable
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

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد تعرفه
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime CreatedAt { get; set; }

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
        public virtual ApplicationUser DeletedByUser { get; set; }
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

        public bool IsDeleted { get; set; }
        public DateTime DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }     

        #endregion
    }

    /// <summary>
    /// پیکربندی مدل تعرفه بیمه برای Entity Framework
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
                .HasPrecision(18, 2);

            Property(t => t.PatientShare)
                .IsOptional()
                .HasPrecision(5, 2);

            Property(t => t.InsurerShare)
                .IsOptional()
                .HasPrecision(5, 2);

            // Tracking
            Property(t => t.CreatedAt)
                .IsRequired();

            Property(t => t.UpdatedAt)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(t => t.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_CreatedByUserId")));

            Property(t => t.UpdatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_InsuranceTariff_UpdatedByUserId")));

            // روابط
            HasRequired(t => t.Insurance)
                .WithMany(i => i.Tariffs)
                .HasForeignKey(t => t.InsuranceId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.Service)
                .WithMany(s => s.Tariffs)
                .HasForeignKey(t => t.ServiceId)
                .WillCascadeOnDelete(false);

            // اندیس یونیک برای جلوگیری از تکرار تعرفه برای یک بیمه و خدمت خاص
            Property(t => t.InsuranceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_Service", 1) { IsUnique = true }));

            Property(t => t.ServiceId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Insurance_Service", 2) { IsUnique = true }));
        }
    }
    #endregion

    #region Doctor
    public class Doctor : ISoftDelete, ITrackable
    {
        public int DoctorId { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(250)]
        public string Specialization { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        public int? ClinicId { get; set; }

        public int? DepartmentId { get; set; }

        // Soft Delete & Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public string CreatedByUserId { get; set; }
        public string DeletedById { get; set; }

        // روابط
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Clinic Clinic { get; set; }
        public virtual Department Department { get; set; }
        public virtual ICollection<Reception> Receptions { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public string Bio { get; set; }
    }

    public class DoctorConfig : EntityTypeConfiguration<Doctor>
    {
        public DoctorConfig()
        {
            ToTable("Doctors");
            HasKey(d => d.DoctorId);

            Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            Property(d => d.LastName).IsRequired().HasMaxLength(100);
            Property(d => d.Specialization).IsOptional().HasMaxLength(250);
            Property(d => d.PhoneNumber).IsOptional().HasMaxLength(50);

            // Soft Delete & Tracking
            Property(d => d.IsDeleted).IsRequired();
            Property(d => d.DeletedAt).IsOptional();
            Property(d => d.CreatedAt).IsRequired();
            Property(d => d.UpdatedAt).IsOptional();

            // روابط دوطرفه کامل
            HasRequired(d => d.ApplicationUser)
                .WithMany(u => u.Doctors)
                .HasForeignKey(d => d.ApplicationUserId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .WillCascadeOnDelete(false);

            HasOptional(d => d.Department)
                .WithMany(dept => dept.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .WillCascadeOnDelete(false);

            HasMany(d => d.Receptions)
                .WithRequired(r => r.Doctor)
                .HasForeignKey(r => r.DoctorId)
                .WillCascadeOnDelete(false);

            HasMany(d => d.Appointments)
                .WithRequired(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Reception
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

        // Soft Delete & Tracking
        #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }
        #endregion

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        /// </summary>
        public virtual ApplicationUser UpdatedByUser { get; set; }
        #endregion

        // روابط
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
        public virtual ICollection<ReceptionItem> ReceptionItems { get; set; }

        /// <summary>
        /// لیست تراکنش‌های مالی
        /// این لیست برای نمایش تمام پرداخت‌ها و تراکنش‌های مالی مرتبط با این پذیرش استفاده می‌شود
        /// </summary>
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } // تغییر از Payments به Transactions

        public virtual ICollection<ReceiptPrint> ReceiptPrints { get; set; }

        // محاسبه هوشمند پرداخت
        [NotMapped]
        public bool IsPaid => Transactions?.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount) >= TotalAmount;
    }

    public class ReceptionConfig : EntityTypeConfiguration<Reception>
    {
        public ReceptionConfig()
        {
            ToTable("Receptions");
            HasKey(r => r.ReceptionId);

            Property(r => r.ReceptionDate).IsRequired();

            // ویژگی‌های اصلی
            Property(r => r.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.PatientCoPay)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.InsurerShareAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(r => r.Status).IsRequired();

            // Soft Delete & Tracking
            Property(r => r.IsDeleted)
                .IsRequired();

            Property(r => r.DeletedAt)
                .IsOptional();

            // Tracking
            Property(r => r.CreatedAt)
                .IsRequired();

            Property(r => r.UpdatedAt)
                .IsOptional();

            // ایندکس‌های حیاتی برای عملکرد
            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(r => r.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_CreatedByUserId")));

            Property(r => r.UpdatedAt)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_UpdatedByUserId")));

            Property(r => r.DeletedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_Reception_DeletedByUserId")));

            Property(r => r.ReceptionDate)
                .HasColumnAnnotation("IX_ReceptionDate",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionDate")));

            Property(r => r.Status)
                .HasColumnAnnotation("IX_Status",
                    new IndexAnnotation(new IndexAttribute("IX_Status")));

            // روابط دوطرفه کامل
            HasRequired(r => r.Patient)
                .WithMany(p => p.Receptions)
                .HasForeignKey(r => r.PatientId)
                .WillCascadeOnDelete(false);

            HasRequired(r => r.Doctor)
                .WithMany(d => d.Receptions)
                .HasForeignKey(r => r.DoctorId)
                .WillCascadeOnDelete(false);

            HasMany(r => r.ReceptionItems)
                .WithRequired(ri => ri.Reception)
                .HasForeignKey(ri => ri.ReceptionId)
                .WillCascadeOnDelete(true);

            HasRequired(r => r.Insurance)
                .WithMany(i => i.Receptions)
                .HasForeignKey(r => r.InsuranceId)
                .WillCascadeOnDelete(false);

            // تغییر رابطه از Payments به Transactions
            HasMany(r => r.Transactions) // جدید
                .WithRequired(t => t.Reception) // جدید
                .HasForeignKey(t => t.ReceptionId) // جدید
                .WillCascadeOnDelete(false); // تغییر به false برای سازگاری با PaymentTransactionConfig

            HasMany(r => r.ReceiptPrints)
                .WithRequired(rp => rp.Reception)
                .HasForeignKey(rp => rp.ReceptionId)
                .WillCascadeOnDelete(true);
        }
    }
    #endregion

    #region ReceptionItem
    public class ReceptionItem : ISoftDelete
    {
        public int ReceptionItemId { get; set; }
        public int ReceptionId { get; set; }
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

        #region پیاده‌سازی ITrackable (مدیریت ردیابی)
        /// <summary>
        /// تاریخ و زمان ایجاد آیتم پذیرش
        /// این اطلاعات برای گزارش‌گیری و ردیابی در سیستم‌های پزشکی ضروری است
        /// </summary>
        public DateTime? CreatedAt { get; set; }

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
        // روابط
        public virtual Reception Reception { get; set; }
        public virtual Service Service { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
    }

    #endregion

    public class ReceptionItemConfig : EntityTypeConfiguration<ReceptionItem>
    {
        public ReceptionItemConfig()
        {
            ToTable("ReceptionItems");
            HasKey(ri => ri.ReceptionItemId);

            // ویژگی‌های اصلی
            Property(ri => ri.Quantity)
                .IsRequired();

            Property(ri => ri.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(ri => ri.PatientShareAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            Property(ri => ri.InsurerShareAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            // Tracking
            Property(ri => ri.CreatedAt)
                .IsOptional();

            Property(ri => ri.UpdatedAt)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(ri => ri.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_CreatedByUserId")));

            Property(ri => ri.UpdatedAt)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ReceptionItem_UpdatedByUserId")));

            // روابط دوطرفه کامل
            HasRequired(ri => ri.Reception)
                .WithMany(r => r.ReceptionItems)
                .HasForeignKey(ri => ri.ReceptionId)
                .WillCascadeOnDelete(true);

            HasRequired(ri => ri.Service)
                .WithMany(s => s.ReceptionItems)
                .HasForeignKey(ri => ri.ServiceId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Appointment
    public class Appointment : ISoftDelete, ITrackable
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int? PatientId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [Required, DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        [MaxLength(100)]
        public string PaymentTransactionId { get; set; }

        // Soft Delete & Tracking
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public string CreatedByUserId { get; set; }

        // روابط
        public virtual Doctor Doctor { get; set; }
        public virtual Patient Patient { get; set; }
        public string DeletedById { get; set; }
    }

    public class AppointmentConfig : EntityTypeConfiguration<Appointment>
    {
        public AppointmentConfig()
        {
            ToTable("Appointments");
            HasKey(a => a.AppointmentId);

            Property(a => a.AppointmentDate).IsRequired();

            Property(a => a.Price)
                .IsRequired() // اضافه شد
                .HasPrecision(18, 2);

            Property(a => a.PaymentTransactionId).IsOptional().HasMaxLength(100);

            // تنظیم ایندکس‌های جدید
            Property(a => a.AppointmentDate)
                .HasColumnAnnotation("IX_Appointment_Date",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_Date")));

            Property(a => a.Status)
                .HasColumnAnnotation("IX_Appointment_Status",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_Status")));

            Property(a => a.DoctorId)
                .HasColumnAnnotation("IX_Appointment_DoctorId",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_DoctorId")));

            Property(a => a.PatientId)
                .HasColumnAnnotation("IX_Appointment_PatientId",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_PatientId")));

            Property(a => a.IsDeleted)
                .HasColumnAnnotation("IX_Appointment_IsDeleted",
                    new IndexAnnotation(new IndexAttribute("IX_Appointment_IsDeleted")));

            // روابط
            HasRequired(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .WillCascadeOnDelete(false); // یا تنظیم SetNull در دیتابیس
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
                .HasPrecision(18, 2);

            Property(t => t.Method)
                .IsRequired();

            Property(t => t.Status)
                .IsRequired();

            Property(t => t.TransactionId)
                .IsOptional()
                .HasMaxLength(100);

            Property(t => t.ReferenceCode)
                .IsOptional()
                .HasMaxLength(100);

            Property(t => t.ReceiptNo)
                .IsOptional()
                .HasMaxLength(50);

            Property(t => t.Description)
                .IsOptional()
                .HasMaxLength(500);

            // Soft Delete
            Property(t => t.IsDeleted)
                .IsRequired();

            Property(t => t.DeletedAt)
                .IsOptional();

            // Tracking
            Property(t => t.CreatedAt)
                .IsRequired();

            Property(t => t.UpdatedAt)
                .IsOptional();

            Property(t => t.CreatedByUserId)
                .IsOptional();

            Property(t => t.UpdatedAt)
                .IsOptional();

            Property(t => t.DeletedByUserId)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(t => t.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CreatedByUserId")));

            Property(t => t.UpdatedAt)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_UpdatedByUserId")));

            Property(t => t.DeletedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_DeletedByUserId")));

            Property(t => t.ReceptionId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_ReceptionId")));

            Property(t => t.PosTerminalId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_PosTerminalId")));

            Property(t => t.CashSessionId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentTransaction_CashSessionId")));

            // روابط
            HasRequired(t => t.Reception)
                .WithMany(r => r.Transactions) // تغییر از Payments به Transactions
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
                .HasForeignKey(t => t.UpdatedByUserId) // <-- این درست است
                .WillCascadeOnDelete(false);
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
        public virtual ICollection<PaymentTransaction> Transactions { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل شیفت کاری صندوق برای Entity Framework
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
                .HasMaxLength(128);

            Property(cs => cs.OpenedAt)
                .IsRequired();

            Property(cs => cs.ClosedAt)
                .IsOptional();

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
                .IsRequired();

            // Soft Delete
            Property(cs => cs.IsDeleted)
                .IsRequired();

            Property(cs => cs.DeletedAt)
                .IsOptional();

            // Tracking
            Property(cs => cs.CreatedAt)
                .IsRequired();

            Property(cs => cs.UpdatedAt)
                .IsOptional();

            Property(cs => cs.CreatedByUserId)
                .IsOptional();

            Property(cs => cs.UpdatedAt)
                .IsOptional();

            Property(cs => cs.DeletedByUserId)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(cs => cs.UserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_UserId")));

            Property(cs => cs.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_CreatedByUserId")));

            Property(cs => cs.UpdatedAt)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSession_UpdatedByUserId")));

            Property(cs => cs.DeletedByUserId)
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
                .HasForeignKey(cs => cs.UpdatedByUserId) // <-- این درست است
                .WillCascadeOnDelete(false);
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
        public virtual ICollection<PaymentTransaction> Transactions { get; set; }
        #endregion
    }

    /// <summary>
    /// پیکربندی مدل دستگاه‌های پوز بانکی برای Entity Framework
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
                .HasMaxLength(100);

            Property(p => p.TerminalId)
                .IsRequired()
                .HasMaxLength(50);

            Property(p => p.MerchantId)
                .IsRequired()
                .HasMaxLength(50);

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
                .IsRequired();

            Property(p => p.IsActive)
                .IsRequired();

            Property(p => p.Protocol)
                .IsRequired();

            Property(p => p.Port)
                .IsOptional();

            // Soft Delete
            Property(p => p.IsDeleted)
                .IsRequired();

            Property(p => p.DeletedAt)
                .IsOptional();

            // Tracking
            Property(p => p.CreatedAt)
                .IsRequired();

            Property(p => p.UpdatedAt)
                .IsOptional();

            Property(p => p.CreatedByUserId)
                .IsOptional();

            Property(p => p.UpdatedAt)
                .IsOptional();

            Property(p => p.DeletedByUserId)
                .IsOptional();

            // افزودن ایندکس‌ها برای عملکرد بهتر در محیط‌های پزشکی
            Property(p => p.CreatedByUserId)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_CreatedByUserId")));

            Property(p => p.UpdatedAt)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PosTerminal_UpdatedByUserId")));

            Property(p => p.DeletedByUserId)
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
        }
    }
    #endregion

    #region ReceiptPrint
    public class ReceiptPrint : ITrackable
    {
        public int ReceiptPrintId { get; set; }

        [Required]
        public int ReceptionId { get; set; }

        [Required]
        public string ReceiptContent { get; set; }

        [Required]
        public DateTime PrintDate { get; set; } = DateTime.UtcNow;

        [MaxLength(250)]
        public string PrintedBy { get; set; }

        // کاربر چاپ‌کننده (ارتباط با ApplicationUser)
        public string PrintedByUserId { get; set; }
        public virtual ApplicationUser PrintedByUser { get; set; }

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public string CreatedByUserId { get; set; }

        // رابطه با Reception
        public virtual Reception Reception { get; set; }
    }

    public class ReceiptPrintConfig : EntityTypeConfiguration<ReceiptPrint>
    {
        public ReceiptPrintConfig()
        {
            ToTable("ReceiptPrints");
            HasKey(rp => rp.ReceiptPrintId);

            // ReceiptContent بدون محدودیت طول (nvarchar(max))
            Property(rp => rp.ReceiptContent)
                .IsRequired()
                .IsMaxLength();

            Property(rp => rp.PrintDate).IsRequired();
            Property(rp => rp.PrintedBy).IsOptional().HasMaxLength(250);

            // Tracking
            Property(rp => rp.CreatedAt).IsRequired();
            Property(rp => rp.UpdatedAt).IsOptional();

            // رابطه ReceiptPrint با Reception
            HasRequired(rp => rp.Reception)
                .WithMany(r => r.ReceiptPrints)
                .HasForeignKey(rp => rp.ReceptionId)
                .WillCascadeOnDelete(true);

            // رابطه ReceiptPrint با کاربر چاپ‌کننده (اختیاری)
            HasOptional(rp => rp.PrintedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.PrintedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    #region Notification
    public class Notification : ITrackable
    {
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } // تغییر از int? به string (برای Identity)

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; }

        [Required]
        public DateTime SentDate { get; set; }

        [Required]
        public bool IsSent { get; set; } = false;

        // Tracking
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public string CreatedByUserId { get; set; }

        // روابط
        public virtual ApplicationUser ApplicationUser { get; set; }
    }

    public class NotificationConfig : EntityTypeConfiguration<Notification>
    {
        public NotificationConfig()
        {
            ToTable("Notifications");
            HasKey(n => n.NotificationId);

            Property(n => n.UserId).IsRequired();
            Property(n => n.PhoneNumber).IsRequired().HasMaxLength(20);
            Property(n => n.Message).IsRequired().HasMaxLength(500);
            Property(n => n.SentDate).IsRequired();
            Property(n => n.IsSent).IsRequired();

            // Tracking
            Property(n => n.CreatedAt).IsRequired();
            Property(n => n.UpdatedAt).IsOptional();

            // ایندکس برای عملکرد بهتر
            Property(n => n.SentDate)
                .HasColumnAnnotation("IX_SentDate",
                    new IndexAnnotation(new IndexAttribute("IX_SentDate")));

            Property(n => n.IsSent)
                .HasColumnAnnotation("IX_IsSent",
                    new IndexAnnotation(new IndexAttribute("IX_IsSent")));

            // روابط دوطرفه کامل
            HasRequired(n => n.ApplicationUser)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);
        }
    }
    #endregion

    
    }