namespace ClinicApp.Models.Enums;

/// <summary>
/// وضعیت‌های تعادل بار کاری پزشکان
/// </summary>
public enum WorkloadBalanceStatus
{
    /// <summary>
    /// بار کاری سبک - امکان افزایش تعداد نوبت‌ها
    /// </summary>
    Light = 1,

    /// <summary>
    /// بار کاری متعادل - وضعیت مطلوب
    /// </summary>
    Balanced = 2,

    /// <summary>
    /// بار کاری سنگین - نیاز به بهینه‌سازی
    /// </summary>
    Heavy = 3,

    /// <summary>
    /// بار کاری بیش از حد - نیاز به کاهش فوری
    /// </summary>
    Overloaded = 4,

    /// <summary>
    /// روز کاری تعریف نشده
    /// </summary>
    NoWorkDay = 5
}