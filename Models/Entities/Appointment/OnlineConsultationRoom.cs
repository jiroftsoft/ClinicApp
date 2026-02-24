using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Core;

namespace ClinicApp.Models.Entities.Appointment;

/// <summary>
/// اتاق مشاوره آنلاین تصویری (Jitsi) برای یک نوبت
/// </summary>
public class OnlineConsultationRoom : ITrackable
{
    public int RoomId { get; set; }
    [Required]
    public int AppointmentId { get; set; }
    [Required]
    [MaxLength(256)]
    public string RoomName { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    #region ITrackable
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public virtual ApplicationUser CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    public virtual Appointment Appointment { get; set; }
}

public class OnlineConsultationRoomConfig : EntityTypeConfiguration<OnlineConsultationRoom>
{
    public OnlineConsultationRoomConfig()
    {
        ToTable("OnlineConsultationRooms");
        HasKey(r => r.RoomId);
        Property(r => r.AppointmentId)
            .IsRequired()
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_OnlineConsultationRoom_AppointmentId") { IsUnique = true }));
        Property(r => r.RoomName)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_OnlineConsultationRoom_RoomName") { IsUnique = true }));
        Property(r => r.StartedAt).IsOptional();
        Property(r => r.EndedAt).IsOptional();
        Property(r => r.CreatedAt).IsRequired();
        Property(r => r.CreatedByUserId).IsOptional();
        Property(r => r.UpdatedAt).IsOptional();
        Property(r => r.UpdatedByUserId).IsOptional();

        HasRequired(r => r.Appointment)
            .WithMany()
            .HasForeignKey(r => r.AppointmentId)
            .WillCascadeOnDelete(false);
        HasOptional(r => r.CreatedByUser)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .WillCascadeOnDelete(false);
        HasOptional(r => r.UpdatedByUser)
            .WithMany()
            .HasForeignKey(r => r.UpdatedByUserId)
            .WillCascadeOnDelete(false);
    }
}
