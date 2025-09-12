using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// اولویت‌های اورژانس
/// </summary>
/// <summary>
/// اولویت‌های اورژانس
/// </summary>
public enum EmergencyPriority : byte
{
    [Display(Name = "کم")]
    Low = 0,
    [Display(Name = "متوسط")]
    Medium = 1,
    [Display(Name = "زیاد")]
    High = 2,
    [Display(Name = "بحرانی")]
    Critical = 3
}