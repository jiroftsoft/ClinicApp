using System.ComponentModel;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// دسته‌بندی‌های خطاهای سیستم‌های پزشکی
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>
        /// خطای عمومی
        /// </summary>
        [Description("خطای عمومی")]
        General = 0,

        /// <summary>
        /// خطای مربوط به داده‌های ورودی
        /// </summary>
        [Description("خطای ورودی")]
        InputValidation = 1,

        /// <summary>
        /// خطای مربوط به دسترسی
        /// </summary>
        [Description("خطای دسترسی")]
        Authorization = 2,

        /// <summary>
        /// خطای مربوط به امنیت
        /// </summary>
        [Description("خطای امنیتی")]
        Security = 3,

        /// <summary>
        /// خطای مربوط به پایگاه داده
        /// </summary>
        [Description("خطای پایگاه داده")]
        Database = 4,

        /// <summary>
        /// خطای مربوط به پزشکی
        /// </summary>
        [Description("خطای پزشکی")]
        Medical = 5,

        /// <summary>
        /// خطای مربوط به حسابداری
        /// </summary>
        [Description("خطای حسابداری")]
        Accounting = 6,

        /// <summary>
        /// خطای مربوط به پردازش
        /// </summary>
        [Description("خطای پردازش")]
        Processing = 7,

        /// <summary>
        /// خطای مربوط به سخت‌افزار
        /// </summary>
        [Description("خطای سخت‌افزار")]
        Hardware = 8,

        /// <summary>
        /// خطای مربوط به شبکه
        /// </summary>
        [Description("خطای شبکه")]
        Network = 9
    }

    /// <summary>
    /// سطوح امنیتی برای مدیریت اطلاعات در سیستم‌های پزشکی
    /// </summary>
    public enum SecurityLevel
    {
        /// <summary>
        /// سطح امنیتی پایین (اطلاعات عمومی)
        /// </summary>
        [Description("پایین")]
        Low = 0,

        /// <summary>
        /// سطح امنیتی متوسط (اطلاعات داخلی)
        /// </summary>
        [Description("متوسط")]
        Medium = 1,

        /// <summary>
        /// سطح امنیتی بالا (اطلاعات حساس)
        /// </summary>
        [Description("بالا")]
        High = 2,

        /// <summary>
        /// سطح امنیتی بسیار بالا (اطلاعات بسیار حساس پزشکی)
        /// </summary>
        [Description("بسیار بالا")]
        Critical = 3
    }

    /// <summary>
    /// وضعیت‌های عملیات در سیستم‌های پزشکی
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// در انتظار اجرا
        /// </summary>
        [Description("در انتظار")]
        Pending = 0,

        /// <summary>
        /// در حال اجرا
        /// </summary>
        [Description("در حال اجرا")]
        InProgress = 1,

        /// <summary>
        /// با موفقیت انجام شد
        /// </summary>
        [Description("موفق")]
        Completed = 2,

        /// <summary>
        /// با خطا مواجه شد
        /// </summary>
        [Description("ناموفق")]
        Failed = 3,

        /// <summary>
        /// لغو شد
        /// </summary>
        [Description("لغو شده")]
        Cancelled = 4,

        /// <summary>
        /// در انتظار تأیید
        /// </summary>
        [Description("در انتظار تأیید")]
        AwaitingApproval = 5,

        /// <summary>
        /// تأیید شد
        /// </summary>
        [Description("تأیید شده")]
        Approved = 6,

        /// <summary>
        /// رد شد
        /// </summary>
        [Description("رد شده")]
        Rejected = 7,

        /// <summary>
        /// در انتظار پردازش
        /// </summary>
        [Description("در انتظار پردازش")]
        AwaitingProcessing = 8
    }
}