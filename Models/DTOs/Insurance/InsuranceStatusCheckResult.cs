using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Insurance
{
    /// <summary>
    /// نتیجه بررسی جامع وضعیت بیمه - برای استفاده در ماژول‌های مختلف
    /// 
    /// این DTO برای نمایش هشدارهای واضح به منشی‌ها در فرم پذیرش طراحی شده است
    /// </summary>
    public class InsuranceStatusCheckResult
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// آیا بیمه معتبر است؟
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// وضعیت کلی بیمه
        /// </summary>
        public InsuranceStatusType Status { get; set; }

        /// <summary>
        /// پیام اصلی برای نمایش به کاربر
        /// </summary>
        public string MainMessage { get; set; }

        /// <summary>
        /// پیام تفصیلی (برای نمایش در tooltip یا modal)
        /// </summary>
        public string DetailedMessage { get; set; }

        /// <summary>
        /// آیا هشدار انقضا وجود دارد؟
        /// </summary>
        public bool HasExpiryWarning { get; set; }

        /// <summary>
        /// آیا بیمه منقضی شده است؟
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// تعداد روزهای باقی‌مانده تا انقضا (منفی اگر منقضی شده)
        /// </summary>
        public int? DaysUntilExpiry { get; set; }

        /// <summary>
        /// تاریخ انقضای بیمه پایه
        /// </summary>
        public DateTime? PrimaryInsuranceExpiryDate { get; set; }

        /// <summary>
        /// تاریخ انقضای بیمه تکمیلی
        /// </summary>
        public DateTime? SupplementaryInsuranceExpiryDate { get; set; }

        /// <summary>
        /// اطلاعات بیمه پایه
        /// </summary>
        public InsuranceStatusDetail PrimaryInsurance { get; set; }

        /// <summary>
        /// اطلاعات بیمه تکمیلی
        /// </summary>
        public InsuranceStatusDetail SupplementaryInsurance { get; set; }

        /// <summary>
        /// لیست هشدارها (برای نمایش به منشی)
        /// </summary>
        public List<InsuranceStatusAlert> Alerts { get; set; } = new List<InsuranceStatusAlert>();

        /// <summary>
        /// آیا می‌توان پذیرش را ادامه داد؟
        /// </summary>
        public bool CanProceedWithReception { get; set; }

        /// <summary>
        /// توصیه‌ها برای کاربر
        /// </summary>
        public List<string> Recommendations { get; set; } = new List<string>();

        /// <summary>
        /// تاریخ بررسی
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// نوع وضعیت بیمه
    /// </summary>
    public enum InsuranceStatusType
    {
        /// <summary>
        /// معتبر - همه چیز درست است
        /// </summary>
        Valid = 0,

        /// <summary>
        /// منقضی شده - نیاز به تمدید
        /// </summary>
        Expired = 1,

        /// <summary>
        /// در حال انقضا - هشدار (کمتر از 30 روز)
        /// </summary>
        ExpiringSoon = 2,

        /// <summary>
        /// بیمه پایه وجود ندارد
        /// </summary>
        MissingPrimaryInsurance = 3,

        /// <summary>
        /// بیمه غیرفعال است
        /// </summary>
        Inactive = 4,

        /// <summary>
        /// خطا در بررسی
        /// </summary>
        Error = 5
    }

    /// <summary>
    /// جزئیات وضعیت یک بیمه
    /// </summary>
    public class InsuranceStatusDetail
    {
        /// <summary>
        /// آیا این بیمه وجود دارد؟
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// شناسه بیمه
        /// </summary>
        public int? InsuranceId { get; set; }

        /// <summary>
        /// نام بیمه
        /// </summary>
        public string InsuranceName { get; set; }

        /// <summary>
        /// شماره بیمه
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// آیا فعال است؟
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا منقضی شده است؟
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// تعداد روزهای باقی‌مانده
        /// </summary>
        public int? DaysRemaining { get; set; }

        /// <summary>
        /// وضعیت (برای نمایش)
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// کلاس CSS برای نمایش (success, warning, danger)
        /// </summary>
        public string StatusClass { get; set; }
    }

    /// <summary>
    /// هشدار وضعیت بیمه (برای نمایش به منشی)
    /// </summary>
    public class InsuranceStatusAlert
    {
        /// <summary>
        /// نوع هشدار
        /// </summary>
        public AlertType Type { get; set; }

        /// <summary>
        /// شدت هشدار
        /// </summary>
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// عنوان هشدار
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// پیام هشدار
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// آیا باید به صورت modal نمایش داده شود؟
        /// </summary>
        public bool ShowAsModal { get; set; }

        /// <summary>
        /// آیا باید پذیرش را متوقف کند؟
        /// </summary>
        public bool BlockReception { get; set; }

        /// <summary>
        /// آیکون برای نمایش
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// کلاس CSS برای نمایش
        /// </summary>
        public string CssClass { get; set; }
    }

    /// <summary>
    /// نوع هشدار
    /// </summary>
    public enum AlertType
    {
        Expired,
        ExpiringSoon,
        Missing,
        Inactive,
        InvalidDateRange,
        Other
    }

    /// <summary>
    /// شدت هشدار
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// اطلاعاتی - فقط اطلاع‌رسانی
        /// </summary>
        Info = 0,

        /// <summary>
        /// هشدار - نیاز به توجه
        /// </summary>
        Warning = 1,

        /// <summary>
        /// بحرانی - باید اقدام شود
        /// </summary>
        Critical = 2
    }
}

