using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// اولویت نوبت
/// </summary>
public enum AppointmentPriority : byte
{
    [Display(Name = "عادی")]
    Normal = 0,
    [Display(Name = "بالا")]
    High = 1,
    [Display(Name = "فوری")]
    Urgent = 2,
    [Display(Name = "اورژانس")]
    Emergency = 3
}