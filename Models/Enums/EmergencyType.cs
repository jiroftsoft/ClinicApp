namespace ClinicApp.Models.Enums;

/// <summary>
/// انواع اورژانس
/// </summary>
public enum EmergencyType
{
    /// <summary>
    /// اورژانس بحرانی
    /// </summary>
    Critical = 1,

    /// <summary>
    /// اورژانس پزشکی
    /// </summary>
    Medical = 2,

    /// <summary>
    /// اورژانس تصادفی
    /// </summary>
    Accident = 3,

    /// <summary>
    /// اورژانس قلبی
    /// </summary>
    Cardiac = 4,

    /// <summary>
    /// اورژانس تنفسی
    /// </summary>
    Respiratory = 5
}