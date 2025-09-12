namespace ClinicApp.Models.Enums;

/// <summary>
/// انواع زمان‌های استراحت
/// </summary>
public enum BreakType
{
    /// <summary>
    /// استراحت ناهار
    /// </summary>
    Lunch = 1,

    /// <summary>
    /// استراحت صبحانه
    /// </summary>
    Breakfast = 2,

    /// <summary>
    /// استراحت عصرانه
    /// </summary>
    Afternoon = 3,

    /// <summary>
    /// استراحت کوتاه
    /// </summary>
    Short = 4,

    /// <summary>
    /// استراحت اضطراری
    /// </summary>
    Emergency = 5
}