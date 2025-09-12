using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع پذیرش
/// </summary>
public enum ReceptionType : byte
{
    [Display(Name = "عادی")]
    Normal = 0,
    [Display(Name = "اورژانس")]
    Emergency = 1,
    [Display(Name = "ویژه")]
    Special = 2,
    [Display(Name = "آنلاین")]
    Online = 3
}