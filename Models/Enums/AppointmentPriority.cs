using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// اولویت نوبت
/// </summary>
public enum AppointmentPriority : byte
{
    [Display(Name = "پایین")]
    Low = 0,
    [Display(Name = "متوسط")]
    Medium = 1,
    [Display(Name = "بالا")]
    High = 2,
    [Display(Name = "بحرانی")]
    Critical = 3,
    [Display(Name = "عادی")]
    Normal = 4,
    [Display(Name = "فوری")]
    Urgent = 5,
    [Display(Name = "اورژانس")]
    Emergency = 6
}