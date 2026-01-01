using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Core
{
    /// <summary>
    /// Database entity for OTP state persistence (Session Loss Prevention)
    /// Maps to Interfaces.OTP.OtpState (in-memory class)
    /// BEAST MODE FIX #1
    /// </summary>
    [Table("OtpStates")]
    public class OtpStateEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(88)]
        [Index("IX_OtpState_SessionId_Expiry", 1)]
        public string SessionId { get; set; }

    [Required, MaxLength(10)]
    [Index("IX_OtpState_NationalCode_Expiry", 1)]
    public string NationalCode { get; set; }

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; }

        [Required, MaxLength(255)]
        public string OtpHash { get; set; }

        [Required]
        [Index("IX_OtpState_SessionId_Expiry", 2)]
        [Index("IX_OtpState_NationalCode_Expiry", 2)]
        [Index("IX_OtpState_Expiry")]
        public DateTime ExpiryUtc { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        public int AttemptCount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }

    public class OtpStateEntityConfig : EntityTypeConfiguration<OtpStateEntity>
    {
        public OtpStateEntityConfig()
        {
            ToTable("OtpStates");
            HasKey(x => x.Id);

            Property(x => x.SessionId).IsRequired().HasMaxLength(88)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_OtpState_SessionId_Expiry", 1)));

            Property(x => x.NationalCode).IsRequired().HasMaxLength(10)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_OtpState_NationalCode_Expiry", 1)));
            
            Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);

            Property(x => x.ExpiryUtc).IsRequired()
                .HasColumnAnnotation("Index", new IndexAnnotation(new[]
                {
                    new IndexAttribute("IX_OtpState_SessionId_Expiry", 2),
                    new IndexAttribute("IX_OtpState_NationalCode_Expiry", 2),
                    new IndexAttribute("IX_OtpState_Expiry")
                }));
        }
    }
}

