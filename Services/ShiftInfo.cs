using System;
using ClinicApp.Models.Enums;

namespace ClinicApp.Services;

/// <summary>
/// مدل اطلاعات شیفت
/// </summary>
public class ShiftInfo
{
    public ShiftType ShiftType { get; set; }
    public string DisplayName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; }
}