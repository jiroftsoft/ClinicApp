namespace ClinicApp.Models.Enums;

/// <summary>
/// شدت تعارضات اورژانس
/// </summary>
public enum EmergencyConflictSeverity
{
    /// <summary>
    /// تعارض کم - قابل حل
    /// </summary>
    Low = 1,

    /// <summary>
    /// تعارض متوسط - نیاز به بررسی
    /// </summary>
    Medium = 2,

    /// <summary>
    /// تعارض بالا - نیاز به حل فوری
    /// </summary>
    High = 3,

    /// <summary>
    /// تعارض بحرانی - غیرقابل حل
    /// </summary>
    Critical = 4
}