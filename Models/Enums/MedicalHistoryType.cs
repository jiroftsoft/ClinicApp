using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع تاریخچه پزشکی
/// </summary>
public enum MedicalHistoryType : byte
{
    [Display(Name = "بیماری")]
    Disease = 0,
    [Display(Name = "جراحی")]
    Surgery = 1,
    [Display(Name = "آسیب")]
    Injury = 2,
    [Display(Name = "دارو")]
    Medication = 3,
    [Display(Name = "آلرژی")]
    Allergy = 4,
    [Display(Name = "سابقه خانوادگی")]
    FamilyHistory = 5,
    [Display(Name = "سایر")]
    Other = 6
}