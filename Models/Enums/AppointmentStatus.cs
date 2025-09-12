using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

public enum AppointmentStatus : byte
{
    [Display(Name = "در دسترس")]
    Available = 0,
    [Display(Name = "ثبت شده")]
    Scheduled = 1,
    [Display(Name = "در انتظار")]
    Pending = 2,
    [Display(Name = "انجام شده")]
    Completed = 3,
    [Display(Name = "لغو شده")]
    Cancelled = 4,
    [Display(Name = "عدم حضور")]
    NoShow = 5
}