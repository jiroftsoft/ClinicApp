using System.ComponentModel;

namespace ClinicApp.Core;

/// <summary>
/// سطوح امنیتی برای سیستم‌های پزشکی
/// این سطوح بر اساس استانداردهای امنیتی سیستم‌های پزشکی تعریف شده‌اند
/// </summary>
public enum SecurityLevel
{
    [Description("پایین - اطلاعات عمومی")]
    Low = 1,

    [Description("متوسط - اطلاعات داخلی سیستم")]
    Medium = 2,

    [Description("بالا - اطلاعات حساس پزشکی")]
    High = 3,

    [Description("بحرانی - اطلاعات حیاتی و محرمانه پزشکی")]
    Critical = 4
}