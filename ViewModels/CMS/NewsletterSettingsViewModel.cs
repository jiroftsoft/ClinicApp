using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.CMS
{
    /// <summary>
    /// نمایش تنظیمات ایمیل و پیامک (خواندن از DB یا Web.config).
    /// </summary>
    public class NewsletterSettingsViewModel
    {
        public List<SettingItemViewModel> EmailSettings { get; set; } = new List<SettingItemViewModel>();
        public List<SettingItemViewModel> SmsSettings { get; set; } = new List<SettingItemViewModel>();
    }

    public class SettingItemViewModel
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public bool IsMasked { get; set; }
    }

    /// <summary>
    /// مدل ویرایش تنظیمات ارسال — ذخیره در DB (پروداکشن).
    /// رمز خالی = تغییر نده (نگه‌داشتن مقدار فعلی).
    /// </summary>
    public class NewsletterSettingsEditViewModel
    {
        [Display(Name = "آدرس فرستنده (From)")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست")]
        public string EmailFromAddress { get; set; }

        [Display(Name = "نام نمایشی فرستنده")]
        public string EmailNoReplyDisplayName { get; set; }

        [Display(Name = "پیشوند موضوع ایمیل")]
        public string EmailSubjectPrefix { get; set; }

        [Display(Name = "رونوشت مخفی (BCC)")]
        public string EmailBccAddresses { get; set; }

        [Display(Name = "سرور SMTP")]
        [Required(ErrorMessage = "سرور SMTP الزامی است")]
        public string EmailSmtpServer { get; set; }

        [Display(Name = "پورت")]
        [Required(ErrorMessage = "پورت الزامی است")]
        public string EmailPort { get; set; }

        [Display(Name = "نام کاربری SMTP")]
        public string EmailUsername { get; set; }

        [Display(Name = "رمز SMTP (خالی = بدون تغییر)")]
        public string EmailPassword { get; set; }

        [Display(Name = "فعال بودن ارسال ایمیل")]
        public bool EmailEnabled { get; set; }

        [Display(Name = "استفاده از SSL/TLS")]
        public bool EmailEnableSsl { get; set; } = true;

        [Display(Name = "تعداد تلاش مجدد")]
        public int EmailMaxRetries { get; set; } = 3;

        [Display(Name = "زمان قطع (میلی‌ثانیه)")]
        public int EmailTimeoutMs { get; set; } = 15000;

        [Display(Name = "نام کاربری درگاه پیامک")]
        public string SmsUsername { get; set; }

        [Display(Name = "رمز درگاه پیامک (خالی = بدون تغییر)")]
        public string SmsPassword { get; set; }

        [Display(Name = "شماره فرستنده")]
        public string SmsSourceNumber { get; set; }

        [Display(Name = "فعال بودن ارسال پیامک")]
        public bool SmsEnabled { get; set; } = true;

        [Display(Name = "زمان قطع (میلی‌ثانیه)")]
        public int SmsTimeoutMs { get; set; } = 15000;

        [Display(Name = "تعداد تلاش مجدد")]
        public int SmsMaxRetries { get; set; } = 3;
    }
}
