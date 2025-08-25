using System.ComponentModel;

namespace ClinicApp.Core;

/// <summary>
/// وضعیت عملیات برای سیستم‌های پزشکی
/// این وضعیت‌ها بر اساس استانداردهای سیستم‌های پزشکی تعریف شده‌اند
/// </summary>
public enum OperationStatus
{
    [Description("در انتظار")]
    Pending = 0,

    [Description("در حال انجام")]
    InProgress = 1,

    [Description("تکمیل شده")]
    Completed = 2,

    [Description("ناموفق")]
    Failed = 3,

    [Description("لغو شده")]
    Canceled = 4,

    [Description("معلق")]
    Suspended = 5
}