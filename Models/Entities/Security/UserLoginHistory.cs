using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Security
{
    /// <summary>
    /// تاریخچه ورود کاربران سیستم
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت تمام ورودهای موفق و ناموفق
    /// 2. ذخیره IP Address و UserAgent برای امنیت
    /// 3. تشخیص Device, Browser, OS برای Anomaly Detection
    /// 4. Session Tracking برای ردیابی Session ها
    /// 5. Timestamp دقیق برای هر ورود
    /// 
    /// موارد استفاده:
    /// - Audit Trail امنیتی
    /// - تشخیص فعالیت مشکوک (IP/Device جدید)
    /// - گزارش‌گیری تاریخچه ورودها
    /// - Compliance با قوانین حریم خصوصی
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public class UserLoginHistory
    {
        /// <summary>
        /// شناسه لاگ
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است.")]
        [MaxLength(128)]
        public string UserId { get; set; }

        /// <summary>
        /// تاریخ و زمان ورود
        /// </summary>
        [Required(ErrorMessage = "تاریخ ورود الزامی است.")]
        public DateTime LoginTime { get; set; } = DateTime.Now;

        /// <summary>
        /// تاریخ و زمان خروج (nullable - در صورت خروج ثبت می‌شود)
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        [MaxLength(50, ErrorMessage = "آدرس IP نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent مرورگر
        /// </summary>
        [MaxLength(500, ErrorMessage = "User Agent نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string UserAgent { get; set; }

        /// <summary>
        /// نوع دستگاه (Mobile, Desktop, Tablet)
        /// </summary>
        [MaxLength(50, ErrorMessage = "نوع دستگاه نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string DeviceType { get; set; }

        /// <summary>
        /// نام مرورگر (Chrome, Firefox, Safari, etc.)
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام مرورگر نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string BrowserName { get; set; }

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        [MaxLength(20, ErrorMessage = "نسخه مرورگر نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string BrowserVersion { get; set; }

        /// <summary>
        /// نام سیستم عامل (Windows, iOS, Android, etc.)
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام سیستم عامل نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string OSName { get; set; }

        /// <summary>
        /// نسخه سیستم عامل
        /// </summary>
        [MaxLength(20, ErrorMessage = "نسخه سیستم عامل نمی‌تواند بیش از 20 کاراکتر باشد.")]
        public string OSVersion { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی (Optional - City, Country)
        /// </summary>
        [MaxLength(100, ErrorMessage = "موقعیت جغرافیایی نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string Location { get; set; }

        /// <summary>
        /// آیا ورود موفق بوده است؟
        /// </summary>
        [Required(ErrorMessage = "وضعیت موفقیت ورود الزامی است.")]
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// دلیل عدم موفقیت (در صورت ناموفق بودن)
        /// مثال: "Invalid OTP", "Account Locked", "Rate Limit Exceeded"
        /// </summary>
        [MaxLength(200, ErrorMessage = "دلیل عدم موفقیت نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string FailureReason { get; set; }

        /// <summary>
        /// شناسه Session (ASP.NET Session ID)
        /// </summary>
        [MaxLength(128, ErrorMessage = "شناسه Session نمی‌تواند بیش از 128 کاراکتر باشد.")]
        public string SessionId { get; set; }

        /// <summary>
        /// کلید Idempotency برای جلوگیری از ثبت duplicate
        /// این کلید توسط کلاینت تولید می‌شود (UUID/GUID)
        /// و برای هر login attempt یکتا است
        /// 
        /// ✅ استفاده: جلوگیری از ثبت چندباره login در صورت retry
        /// طبق: BEAST MODE AUDIT - Issue #2
        /// </summary>
        [MaxLength(50, ErrorMessage = "کلید Idempotency نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// تاریخ و زمان ایجاد رکورد
        /// </summary>
        [Required(ErrorMessage = "تاریخ ایجاد الزامی است.")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        #region Navigation Properties

        /// <summary>
        /// ارجاع به کاربر
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای UserLoginHistory
    /// 
    /// Indexes:
    /// - UserId (برای جستجوی لاگ‌های یک کاربر)
    /// - LoginTime (برای مرتب‌سازی زمانی)
    /// - IpAddress (برای جستجوی بر اساس IP)
    /// - IsSuccessful (برای فیلتر ورودهای موفق/ناموفق)
    /// - Composite: UserId + LoginTime (برای جستجوی بهینه)
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public class UserLoginHistoryConfig : EntityTypeConfiguration<UserLoginHistory>
    {
        public UserLoginHistoryConfig()
        {
            // ✅ Table Name
            ToTable("UserLoginHistories");

            // ✅ Primary Key
            HasKey(x => x.Id);

            // ✅ Properties Configuration
            Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_UserLoginHistory_UserId")));

            Property(x => x.LoginTime)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_UserLoginHistory_LoginTime")));

            Property(x => x.LogoutTime)
                .IsOptional();

            Property(x => x.IpAddress)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_UserLoginHistory_IpAddress")));

            Property(x => x.UserAgent)
                .IsOptional()
                .HasMaxLength(500);

            Property(x => x.DeviceType)
                .IsOptional()
                .HasMaxLength(50);

            Property(x => x.BrowserName)
                .IsOptional()
                .HasMaxLength(50);

            Property(x => x.BrowserVersion)
                .IsOptional()
                .HasMaxLength(20);

            Property(x => x.OSName)
                .IsOptional()
                .HasMaxLength(50);

            Property(x => x.OSVersion)
                .IsOptional()
                .HasMaxLength(20);

            Property(x => x.Location)
                .IsOptional()
                .HasMaxLength(100);

            Property(x => x.IsSuccessful)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_UserLoginHistory_IsSuccessful")));

            Property(x => x.FailureReason)
                .IsOptional()
                .HasMaxLength(200);

            Property(x => x.SessionId)
                .IsOptional()
                .HasMaxLength(128);

            Property(x => x.IdempotencyKey)
                .IsOptional()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_UserLoginHistory_IdempotencyKey") { IsUnique = true }));

            Property(x => x.CreatedAt)
                .IsRequired();

            // ✅ Relationships
            HasRequired(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .WillCascadeOnDelete(false);

            // ✅ Composite Index برای جستجوی بهینه
            HasIndex(x => new { x.UserId, x.LoginTime })
                .HasName("IX_UserLoginHistory_UserId_LoginTime");
        }
    }
}

