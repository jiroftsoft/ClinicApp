using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Insurance
{
    /// <summary>
    /// مطالبه بیمه — ثبت مبلغ ارسالی به بیمه، تأیید، کسورات و واریز نهایی
    /// </summary>
    public class InsuranceClaim : ISoftDelete, ITrackable
    {
        public int Id { get; set; }

        public int PatientId { get; set; }

        public int InsurancePlanId { get; set; }

        /// <summary>دسته‌صورت‌حساب (برای گروه‌بندی)</summary>
        public int? BatchId { get; set; }

        [Required]
        public decimal ClaimedAmount { get; set; }

        [Required]
        public decimal ApprovedAmount { get; set; }

        [Required]
        public decimal DeductionAmount { get; set; }

        [Required]
        public decimal FinalSettlement { get; set; }

        public DateTime SubmissionDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public ClaimStatus Status { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        /// <summary>جزئیات کسورات (JSON)</summary>
        [MaxLength(2000)]
        public string DeductionDetails { get; set; }

        /// <summary>تراکنش نهایی واریز (در صورت وجود)</summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>ارتباط با پذیرش (برای ردیابی)</summary>
        public int? ReceptionId { get; set; }

        #region Navigation

        public virtual Patient.Patient Patient { get; set; }
        public virtual InsurancePlan InsurancePlan { get; set; }
        public virtual InsuranceBatch Batch { get; set; }
        public virtual PaymentTransaction PaymentTransaction { get; set; }
        public virtual Reception.Reception Reception { get; set; }

        #endregion

        #region ISoftDelete

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedByUserId { get; set; }
        public virtual ApplicationUser DeletedByUser { get; set; }

        #endregion

        #region ITrackable

        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public virtual ApplicationUser UpdatedByUser { get; set; }

        #endregion
    }

    public class InsuranceClaimConfig : EntityTypeConfiguration<InsuranceClaim>
    {
        public InsuranceClaimConfig()
        {
            ToTable("InsuranceClaims");
            HasKey(c => c.Id);

            Property(c => c.ClaimedAmount).HasPrecision(18, 0);
            Property(c => c.ApprovedAmount).HasPrecision(18, 0);
            Property(c => c.DeductionAmount).HasPrecision(18, 0);
            Property(c => c.FinalSettlement).HasPrecision(18, 0);
            Property(c => c.SubmissionDate).IsRequired();
            Property(c => c.ApprovalDate).IsOptional();
            Property(c => c.PaymentDate).IsOptional();
            Property(c => c.Status).IsRequired();
            Property(c => c.RejectionReason).IsOptional().HasMaxLength(500);
            Property(c => c.DeductionDetails).IsOptional().HasMaxLength(2000);

            Property(c => c.IsDeleted).IsRequired();
            Property(c => c.DeletedAt).IsOptional();
            Property(c => c.DeletedByUserId).IsOptional().HasMaxLength(128);
            Property(c => c.CreatedAt).IsRequired();
            Property(c => c.CreatedByUserId).IsOptional().HasMaxLength(128);
            Property(c => c.UpdatedAt).IsOptional();
            Property(c => c.UpdatedByUserId).IsOptional().HasMaxLength(128);

            HasRequired(c => c.Patient)
                .WithMany()
                .HasForeignKey(c => c.PatientId)
                .WillCascadeOnDelete(false);

            HasRequired(c => c.InsurancePlan)
                .WithMany()
                .HasForeignKey(c => c.InsurancePlanId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.Batch)
                .WithMany(b => b.Claims)
                .HasForeignKey(c => c.BatchId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.PaymentTransaction)
                .WithMany()
                .HasForeignKey(c => c.PaymentTransactionId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.Reception)
                .WithMany()
                .HasForeignKey(c => c.ReceptionId)
                .WillCascadeOnDelete(false);

            HasOptional(c => c.CreatedByUser).WithMany().HasForeignKey(c => c.CreatedByUserId).WillCascadeOnDelete(false);
            HasOptional(c => c.UpdatedByUser).WithMany().HasForeignKey(c => c.UpdatedByUserId).WillCascadeOnDelete(false);
            HasOptional(c => c.DeletedByUser).WithMany().HasForeignKey(c => c.DeletedByUserId).WillCascadeOnDelete(false);

            HasIndex(c => new { c.InsurancePlanId, c.SubmissionDate }).HasName("IX_InsuranceClaim_Plan_Submission");
            HasIndex(c => new { c.Status, c.IsDeleted }).HasName("IX_InsuranceClaim_Status_Deleted");
            HasIndex(c => c.BatchId).HasName("IX_InsuranceClaim_BatchId");
        }
    }
}
