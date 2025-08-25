using System.ComponentModel;

namespace ClinicApp.Core;

/// <summary>
/// دسته‌بندی خطاها برای سیستم‌های پزشکی
/// این دسته‌بندی‌ها بر اساس استانداردهای سیستم‌های پزشکی تعریف شده‌اند
/// </summary>
public enum ErrorCategory
{
    [Description("خطای عمومی")]
    General = 0,

    [Description("خطای اعتبارسنجی")]
    Validation = 1,

    [Description("مورد یافت نشد")]
    NotFound = 2,

    [Description("عدم دسترسی")]
    Unauthorized = 3,

    [Description("خطای منطق کسب‌وکار")]
    BusinessLogic = 4,

    [Description("خطای پایگاه داده")]
    Database = 5,

    [Description("خطای امنیتی")]
    Security = 6,

    [Description("خطای ارتباطی")]
    Communication = 7,

    [Description("خطای پزشکی")]
    Medical = 8,

    [Description("خطای مالی")]
    Financial = 9,
    [Description("خطای سیستم")]
    System = 10,
}