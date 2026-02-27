using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Insurance
{
    /// <summary>
    /// دسته‌صورت‌حساب بیمه — گروه‌بندی مطالبات ارسالی به بیمه برای تسویه
    /// </summary>
    public class InsuranceBatch : ISoftDelete, ITrackable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Index("UX_InsuranceBatch_BatchNumber", IsUnique = true)]
        public string BatchNumber { get; set; }

        public int InsuranceProviderId { get; set; }

        public DateTime SubmissionDate { get; set; }

        public DateTime? SettlementDate { get; set; }

        [Required]
        public decimal TotalClaimed { get; set; }

        [Required]
        public decimal TotalApproved { get; set; }

        [Required]
        public decimal TotalDeduction { get; set; }

        public BatchStatus Status { get; set; }

        #region Navigation

        public virtual InsuranceProvider InsuranceProvider { get; set; }
        public virtual ICollection<InsuranceClaim> Claims { get; set; }

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

    public class InsuranceBatchConfig : EntityTypeConfiguration<InsuranceBatch>
    {
        public InsuranceBatchConfig()
        {
            ToTable("InsuranceBatches");
            HasKey(b => b.Id);

            Property(b => b.BatchNumber).IsRequired().HasMaxLength(50);
            Property(b => b.SubmissionDate).IsRequired();
            Property(b => b.SettlementDate).IsOptional();
            Property(b => b.TotalClaimed).HasPrecision(18, 0);
            Property(b => b.TotalApproved).HasPrecision(18, 0);
            Property(b => b.TotalDeduction).HasPrecision(18, 0);
            Property(b => b.Status).IsRequired();

            Property(b => b.IsDeleted).IsRequired();
            Property(b => b.DeletedAt).IsOptional();
            Property(b => b.DeletedByUserId).IsOptional().HasMaxLength(128);
            Property(b => b.CreatedAt).IsRequired();
            Property(b => b.CreatedByUserId).IsOptional().HasMaxLength(128);
            Property(b => b.UpdatedAt).IsOptional();
            Property(b => b.UpdatedByUserId).IsOptional().HasMaxLength(128);

            HasRequired(b => b.InsuranceProvider)
                .WithMany()
                .HasForeignKey(b => b.InsuranceProviderId)
                .WillCascadeOnDelete(false);

            HasOptional(b => b.CreatedByUser).WithMany().HasForeignKey(b => b.CreatedByUserId).WillCascadeOnDelete(false);
            HasOptional(b => b.UpdatedByUser).WithMany().HasForeignKey(b => b.UpdatedByUserId).WillCascadeOnDelete(false);
            HasOptional(b => b.DeletedByUser).WithMany().HasForeignKey(b => b.DeletedByUserId).WillCascadeOnDelete(false);

            HasIndex(b => new { b.InsuranceProviderId, b.SubmissionDate }).HasName("IX_InsuranceBatch_Provider_Submission");
            HasIndex(b => new { b.Status, b.IsDeleted }).HasName("IX_InsuranceBatch_Status_Deleted");
        }
    }
}
