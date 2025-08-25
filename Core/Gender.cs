using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Core;

/// <summary>
/// جنسیت بیمار.
/// </summary>
public enum Gender
{
    [Display(Name = "مرد")]
    Male = 1,

    [Display(Name = "زن")]
    Female = 2
}