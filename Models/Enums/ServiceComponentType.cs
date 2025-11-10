using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع جزء خدمت
/// </summary>
public enum ServiceComponentType
{
    /// <summary>
    /// جزء فنی - شامل هزینه‌های فنی (تجهیزات، مواد مصرفی)
    /// </summary>
    [Display(Name = "فنی")]
    Technical = 1,

    /// <summary>
    /// جزء حرفه‌ای - شامل هزینه‌های حرفه‌ای (دستمزد پزشک، تخصص)
    /// </summary>
    [Display(Name = "حرفه‌ای")]
    Professional = 2
}
