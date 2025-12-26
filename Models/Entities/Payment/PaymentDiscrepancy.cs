using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Payment
{
    /// <summary>
    /// اختلاف‌های مالی
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت اختلاف‌های مالی (کسری، مازاد، عدم تطابق)
    /// 2. ارتباط با جلسه صندوق و تراکنش پرداخت
    /// 3. ثبت مبلغ مورد انتظار و واقعی
    /// 4. دلیل و راه‌حل اختلاف
    /// 5. ردیابی کامل (گزارش‌دهنده، حل‌کننده)
    /// 
    /// موارد استفاده:
    /// - تطبیق صندوق
    /// - شناسایی اختلاف‌ها
    /// - رفع و پیگیری اختلاف‌ها
    /// - گزارش‌گیری اختلاف‌های مالی
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class PaymentDiscrepancy
    {
        /// <summary>
        /// شناسه اختلاف
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// شناسه جلسه صندوق
        /// </summary>
        [Required(ErrorMessage = "شناسه جلسه صندوق الزامی است.")]
        public int CashSessionId { get; set; }

        /// <summary>
        /// شناسه تراکنش پرداخت (اختیاری)
        /// در صورتی که اختلاف مربوط به یک تراکنش خاص باشد
        /// </summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>
        /// نوع اختلاف (Shortage, Overage, Mismatch)
        /// </summary>
        [Required(ErrorMessage = "نوع اختلاف الزامی است.")]
        public DiscrepancyType Type { get; set; }

        /// <summary>
        /// مبلغ مورد انتظار
        /// </summary>
        [Required(ErrorMessage = "مبلغ مورد انتظار الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal ExpectedAmount { get; set; }

        /// <summary>
        /// مبلغ واقعی
        /// </summary>
        [Required(ErrorMessage = "مبلغ واقعی الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// تفاوت (ActualAmount - ExpectedAmount)
        /// </summary>
        [Required(ErrorMessage = "تفاوت الزامی است.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal")]
        public decimal Difference { get; set; }

        /// <summary>
        /// دلیل اختلاف
        /// </summary>
        [MaxLength(500, ErrorMessage = "دلیل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Reason { get; set; }

        /// <summary>
        /// راه‌حل و نتیجه
        /// </summary>
        [MaxLength(500, ErrorMessage = "راه‌حل نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Resolution { get; set; }

        /// <summary>
        /// وضعیت اختلاف (Pending, Resolved, Escalated)
        /// </summary>
        [Required(ErrorMessage = "وضعیت اختلاف الزامی است.")]
        public DiscrepancyStatus Status { get; set; } = DiscrepancyStatus.Pending;

        /// <summary>
        /// شناسه کاربر گزارش‌دهنده
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر گزارش‌دهنده الزامی است.")]
        [MaxLength(128)]
        public string ReportedByUserId { get; set; }

        /// <summary>
        /// تاریخ گزارش
        /// </summary>
        [Required(ErrorMessage = "تاریخ گزارش الزامی است.")]
        public DateTime ReportedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// شناسه کاربر حل‌کننده
        /// </summary>
        [MaxLength(128)]
        public string ResolvedByUserId { get; set; }

        /// <summary>
        /// تاریخ حل
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        #region Navigation Properties

        /// <summary>
        /// ارجاع به جلسه صندوق
        /// </summary>
        public virtual CashSession CashSession { get; set; }

        /// <summary>
        /// ارجاع به تراکنش پرداخت
        /// </summary>
        public virtual PaymentTransaction PaymentTransaction { get; set; }

        /// <summary>
        /// ارجاع به کاربر گزارش‌دهنده
        /// </summary>
        public virtual ApplicationUser ReportedByUser { get; set; }

        /// <summary>
        /// ارجاع به کاربر حل‌کننده
        /// </summary>
        public virtual ApplicationUser ResolvedByUser { get; set; }

        #endregion
    }

    /// <summary>
    /// پیکربندی Entity Framework برای PaymentDiscrepancy
    /// 
    /// Indexes:
    /// - CashSessionId (برای جستجوی اختلاف‌های یک جلسه)
    /// - Status (برای فیلتر بر اساس وضعیت)
    /// - ReportedAt (برای مرتب‌سازی زمانی)
    /// - Composite: CashSessionId + Status (برای جستجوی بهینه)
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class PaymentDiscrepancyConfig : EntityTypeConfiguration<PaymentDiscrepancy>
    {
        public PaymentDiscrepancyConfig()
        {
            // ✅ Table Name
            ToTable("PaymentDiscrepancies");

            // ✅ Primary Key
            HasKey(x => x.Id);

            // ✅ Properties Configuration
            Property(x => x.Type)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentDiscrepancy_Type")));

            Property(x => x.ExpectedAmount)
                .IsRequired()
                .HasPrecision(18, 0);

            Property(x => x.ActualAmount)
                .IsRequired()
                .HasPrecision(18, 0);

            Property(x => x.Difference)
                .IsRequired()
                .HasPrecision(18, 0);

            Property(x => x.Reason)
                .IsOptional()
                .HasMaxLength(500);

            Property(x => x.Resolution)
                .IsOptional()
                .HasMaxLength(500);

            Property(x => x.Status)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentDiscrepancy_Status")));

            Property(x => x.ReportedByUserId)
                .IsRequired()
                .HasMaxLength(128)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentDiscrepancy_ReportedByUserId")));

            Property(x => x.ReportedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentDiscrepancy_ReportedAt")));

            Property(x => x.ResolvedByUserId)
                .IsOptional()
                .HasMaxLength(128);

            Property(x => x.ResolvedAt)
                .IsOptional();

            Property(x => x.CashSessionId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_PaymentDiscrepancy_CashSessionId")));

            Property(x => x.PaymentTransactionId)
                .IsOptional();

            // ✅ Relationships
            HasRequired(x => x.CashSession)
                .WithMany()
                .HasForeignKey(x => x.CashSessionId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.PaymentTransaction)
                .WithMany()
                .HasForeignKey(x => x.PaymentTransactionId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.ReportedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReportedByUserId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .WillCascadeOnDelete(false);

            // ✅ Composite Indexes برای جستجوی بهینه
            HasIndex(x => new { x.CashSessionId, x.Status })
                .HasName("IX_PaymentDiscrepancy_CashSessionId_Status");

            HasIndex(x => new { x.Status, x.ReportedAt })
                .HasName("IX_PaymentDiscrepancy_Status_ReportedAt");
        }
    }
}

