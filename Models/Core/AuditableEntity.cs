using System;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Models.Entities;

namespace ClinicApp.Models.Core;

/// <summary>
/// کلاس پایه برای تمام موجودیت‌های قابل ردیابی و حذف نرم
/// این کلاس تمام پراپرتی‌های Audit و Soft Delete را فراهم می‌کند
/// </summary>
public abstract class AuditableEntity : ISoftDelete, ITrackable
{
    [ForeignKey(nameof(CreatedByUser))]
    public string CreatedByUserId { get; set; } = null!;
    public virtual ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(UpdatedByUser))]
    public string? UpdatedByUserId { get; set; }
    public virtual ApplicationUser? UpdatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(DeletedByUser))]
    public string? DeletedByUserId { get; set; }
    public virtual ApplicationUser? DeletedByUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}