using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// انواع مختلف استثناهای برنامه کاری
/// </summary>
public enum ExceptionType : byte
{
    [Display(Name = "تعطیلات رسمی")]
    PublicHoliday = 0,
    [Display(Name = "مرخصی")]
    Vacation = 1,
    [Display(Name = "مریضی")]
    SickLeave = 2,
    [Display(Name = "سفر کاری")]
    BusinessTrip = 3,
    [Display(Name = "کنفرانس")]
    Conference = 4,
    [Display(Name = "آموزش")]
    Training = 5,
    [Display(Name = "تعطیلات")]
    Holiday = 6,
    [Display(Name = "سایر")]
    Other = 7
}