namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع جزء خدمت
/// </summary>
public enum ServiceComponentType
{
    /// <summary>
    /// جزء فنی - شامل هزینه‌های فنی (تجهیزات، مواد مصرفی)
    /// </summary>
    Technical = 1,

    /// <summary>
    /// جزء حرفه‌ای - شامل هزینه‌های حرفه‌ای (دستمزد پزشک، تخصص)
    /// </summary>
    Professional = 2
}
