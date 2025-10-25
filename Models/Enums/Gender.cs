using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// جنسیت
/// </summary>
public enum Gender : byte
{
    [Display(Name = "نامشخص")]
    Unknown = 0,
    [Display(Name = "مرد")]
    Male = 1,
    [Display(Name = "زن")]
    Female = 2
}