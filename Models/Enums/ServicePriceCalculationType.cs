namespace ClinicApp.Models.Enums;

/// <summary>
/// نوع محاسبه قیمت خدمات
/// </summary>
public enum ServicePriceCalculationType
{
    /// <summary>
    /// قیمت ثابت - قیمت از قبل تعریف شده
    /// </summary>
    Fixed = 1,

    /// <summary>
    /// محاسبه بر اساس اجزای فنی و حرفه‌ای
    /// قیمت = (کای فنی × کای حرفه‌ای)
    /// </summary>
    ComponentBased = 2
}
