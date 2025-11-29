using ClinicApp.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Entities.Clinic
{
    /// <summary>
    /// مدل حساب بانکی کلینیک - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت حساب بانکی هر کلینیک (شماره شبا)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات
    /// 3. مدیریت ردیابی (Audit Trail) برای رعایت استانداردهای پزشکی
    /// 4. رابطه One-to-One با Clinic
    /// 5. Validation کامل شماره شبا
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط اطلاعات حساب بانکی
    /// ✅ Separation of Concerns: جدا از Clinic Entity
    /// ✅ Audit Trail: ردیابی کامل تغییرات
    /// </summary>
    public class ClinicBankAccount : AuditableEntity
    {
        /// <summary>
        /// شناسه حساب بانکی
        /// </summary>
        public int ClinicBankAccountId { get; set; }

        /// <summary>
        /// شناسه کلینیک (Foreign Key)
        /// رابطه One-to-One با Clinic
        /// </summary>
        [Required(ErrorMessage = "شناسه کلینیک الزامی است.")]
        [Index("IX_ClinicBankAccount_ClinicId", IsUnique = true)]
        public int ClinicId { get; set; }

        /// <summary>
        /// ارجاع به کلینیک
        /// </summary>
        [ForeignKey(nameof(ClinicId))]
        public virtual Clinic Clinic { get; set; }

        /// <summary>
        /// شماره شبا (IBAN)
        /// فرمت: IR + 24 رقم
        /// مثال: IR123456789012345678901234
        /// </summary>
        [Required(ErrorMessage = "شماره شبا الزامی است.")]
        [MaxLength(26, ErrorMessage = "شماره شبا باید 26 کاراکتر باشد.")]
        [MinLength(26, ErrorMessage = "شماره شبا باید 26 کاراکتر باشد.")]
        [RegularExpression(@"^IR\d{24}$", ErrorMessage = "فرمت شماره شبا نامعتبر است. باید با IR شروع شود و 24 رقم داشته باشد.")]
        public string IbanNumber { get; set; }

        /// <summary>
        /// نام بانک
        /// مثال: "بانک ملی ایران"
        /// </summary>
        [Required(ErrorMessage = "نام بانک الزامی است.")]
        [MaxLength(100, ErrorMessage = "نام بانک نمی‌تواند بیش از 100 کاراکتر باشد.")]
        public string BankName { get; set; }

        /// <summary>
        /// شماره حساب
        /// </summary>
        [MaxLength(50, ErrorMessage = "شماره حساب نمی‌تواند بیش از 50 کاراکتر باشد.")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// نام صاحب حساب
        /// </summary>
        [Required(ErrorMessage = "نام صاحب حساب الزامی است.")]
        [MaxLength(200, ErrorMessage = "نام صاحب حساب نمی‌تواند بیش از 200 کاراکتر باشد.")]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// آیا این حساب پیش‌فرض است؟
        /// در حال حاضر One-to-One است، اما برای آینده آماده است
        /// </summary>
        public bool IsDefault { get; set; } = true;

        /// <summary>
        /// آیا حساب فعال است؟
        /// </summary>
        [Required(ErrorMessage = "وضعیت فعال بودن الزامی است.")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        [MaxLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
        public string Description { get; set; }
    }

    /// <summary>
    /// پیکربندی مدل حساب بانکی کلینیک برای Entity Framework
    /// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
    /// </summary>
    public class ClinicBankAccountConfig : EntityTypeConfiguration<ClinicBankAccount>
    {
        public ClinicBankAccountConfig()
        {
            ToTable("ClinicBankAccounts");
            HasKey(c => c.ClinicBankAccountId);

            // ویژگی‌های اصلی
            Property(c => c.ClinicId)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_ClinicId") { IsUnique = true }));

            Property(c => c.IbanNumber)
                .IsRequired()
                .HasMaxLength(26)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_IbanNumber")));

            Property(c => c.BankName)
                .IsRequired()
                .HasMaxLength(100);

            Property(c => c.AccountNumber)
                .IsOptional()
                .HasMaxLength(50);

            Property(c => c.AccountHolderName)
                .IsRequired()
                .HasMaxLength(200);

            Property(c => c.IsDefault)
                .IsRequired();

            Property(c => c.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_IsActive")));

            Property(c => c.Description)
                .IsOptional()
                .HasMaxLength(500);

            // پیاده‌سازی ISoftDelete
            Property(c => c.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_IsDeleted")));

            Property(c => c.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_DeletedAt")));

            // پیاده‌سازی ITrackable
            Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_CreatedAt")));

            Property(c => c.CreatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_CreatedByUserId")));

            Property(c => c.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_UpdatedAt")));

            Property(c => c.UpdatedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_UpdatedByUserId")));

            Property(c => c.DeletedByUserId)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ClinicBankAccount_DeletedByUserId")));

            // روابط
            // رابطه One-to-One: هر Clinic می‌تواند یک ClinicBankAccount داشته باشد
            // استفاده از HasRequired().WithMany() با Index Unique برای شبیه‌سازی One-to-One
            // چون ClinicId غیرقابل null است، رابطه باید Required باشد (EF Rule)
            // Index Unique روی ClinicId تضمین می‌کند که هر Clinic فقط یک ClinicBankAccount داشته باشد
            HasRequired(c => c.Clinic)
                .WithMany()
                .HasForeignKey(c => c.ClinicId)
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

            // ایندکس‌های ترکیبی برای بهبود عملکرد
            HasIndex(c => new { c.ClinicId, c.IsActive, c.IsDeleted })
                .HasName("IX_ClinicBankAccount_ClinicId_IsActive_IsDeleted");

            HasIndex(c => new { c.IsActive, c.IsDeleted })
                .HasName("IX_ClinicBankAccount_IsActive_IsDeleted");
        }
    }
}

