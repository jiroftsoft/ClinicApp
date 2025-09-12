using System;
using ClinicApp.Models.Entities;

namespace ClinicApp.Models.Core;

/// <summary>
/// رابط استاندارد برای Soft Delete در سیستم‌های پزشکی
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    string DeletedByUserId { get; set; }
    ApplicationUser DeletedByUser { get; set; }
}