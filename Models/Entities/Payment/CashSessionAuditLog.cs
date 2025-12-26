using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Payment
{
    /// <summary>
    /// لاگ تغییرات جلسات صندوق
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت تمام تغییرات جلسات صندوق (باز کردن، بستن، تعدیل، لغو)
    /// 2. ذخیره مقادیر قبل و بعد (Old/New Value) به صورت JSON
    /// 3. ثبت دلیل تغییر (Reason)
    /// 4. ثبت اطلاعات کاربر انجام‌دهنده (User, IP, UserAgent)
    /// 5. Timestamp دقیق برای هر تغییر
    /// 
    /// موارد استفاده:
    /// - Audit Trail مالی
    /// - ردیابی اقدامات منشی‌ها
    /// - تشخیص تغییرات غیرمجاز
    /// - گزارش‌گیری تاریخچه تغییرات
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashSessionAuditLog
    {
        /// <summary>
        /// شناسه لاگ
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه جلسه صندوق
        /// </summary>
        [Required(ErrorMessage = "شناسه جلسه صندوق الزامی است.")]
        public int CashSessionId { get; set; }

        /// <summary>
        /// نوع اقدام (Open, Close, Adjust, Cancel, UpdateBalance, etc.)
        /// </summary>
        [Required(ErrorMessage = "نوع اقدام الزامی است.")]
        [MaxLength(50, ErrorMessage = "نوع اقدام نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string Action { get; set; }

        /// <summary>
        /// مقدار قبلی (JSON)
        /// مثال: {"CashBalance": 100000, "Status": "Open"}
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string OldValue { get; set; }

        /// <summary>
        /// مقدار جدید (JSON)
        /// مثال: {"CashBalance": 150000, "Status": "Closed"}
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string NewValue { get; set; }

        /// <summary>
        /// دلیل تغییر
        /// مثال: "Session opened by cashier", "Manual adjustment approved by manager"
        /// </summary>
        [MaxLength(500, ErrorMessage = "دلیل تغییر نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Reason { get; set; }

        /// <summary>
        /// شناسه کاربر انجام‌دهنده
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است.")]
        [MaxLength(128)]
        public string PerformedByUserId { get; set; }

        /// <summary>
        /// تاریخ و زمان انجام اقدام
        /// </summary>
        [Required(ErrorMessage = "تاریخ انجام اقدام الزامی است.")]
        public DateTime PerformedAt { get; set; } = DateTime.Now;

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

        #region Navigation Properties

        /// <summary>
        /// ارجاع به جلسه صندوق
        /// </summary>
        public virtual CashSession CashSession { get; set; }

        /// <summary>
        /// ارجاع به کاربر انجام‌دهنده
        /// </summary>
        public virtual ApplicationUser PerformedByUser { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای CashSessionAuditLog
    /// 
    /// Indexes:
    /// - CashSessionId (برای جستجوی لاگ‌های یک جلسه)
    /// - PerformedByUserId (برای جستجوی لاگ‌های یک کاربر)
    /// - PerformedAt (برای مرتب‌سازی زمانی)
    /// - Composite: CashSessionId + PerformedAt (برای جستجوی بهینه)
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashSessionAuditLogConfig : EntityTypeConfiguration<CashSessionAuditLog>
    {
        public CashSessionAuditLogConfig()
        {
            // ✅ Table Name
            ToTable("CashSessionAuditLogs");

            // ✅ Primary Key
            HasKey(x => x.Id);

            // ✅ Properties Configuration
            Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSessionAuditLog_Action")));

            Property(x => x.OldValue)
                .IsOptional()
                .HasColumnType("nvarchar(max)");

            Property(x => x.NewValue)
                .IsOptional()
                .HasColumnType("nvarchar(max)");

            Property(x => x.Reason)
                .IsOptional()
                .HasMaxLength(500);

            Property(x => x.PerformedByUserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSessionAuditLog_PerformedByUserId")));

            Property(x => x.PerformedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSessionAuditLog_PerformedAt")));

            Property(x => x.IpAddress)
                .IsOptional()
                .HasMaxLength(50);

            Property(x => x.UserAgent)
                .IsOptional()
                .HasMaxLength(500);

            Property(x => x.CashSessionId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_CashSessionAuditLog_CashSessionId")));

            // ✅ Relationships
            HasRequired(x => x.CashSession)
                .WithMany()
                .HasForeignKey(x => x.CashSessionId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.PerformedByUser)
                .WithMany()
                .HasForeignKey(x => x.PerformedByUserId)
                .WillCascadeOnDelete(false);

            // ✅ Composite Index برای جستجوی بهینه
            HasIndex(x => new { x.CashSessionId, x.PerformedAt })
                .HasName("IX_CashSessionAuditLog_CashSessionId_PerformedAt");
        }
    }
}

