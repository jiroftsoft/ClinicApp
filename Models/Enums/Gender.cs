using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// جنسیت
/// </summary>
public enum Gender : byte
{
    [Display(Name = "مرد")]
    Male = 1,
    [Display(Name = "زن")]
    Female = 2
}