using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// مدرک تحصیلی
/// </summary>
public enum Degree : byte
{
    [Display(Name = "پزشک عمومی")]
    GeneralPhysician = 1,
    [Display(Name = "متخصص")]
    Specialist = 2,
    [Display(Name = "فوق تخصص")]
    SubSpecialist = 3,
    [Display(Name = "دندانپزشک")]
    Dentist = 4,
    [Display(Name = "داروساز")]
    Pharmacist = 5

}