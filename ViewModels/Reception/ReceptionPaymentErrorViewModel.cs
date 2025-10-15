using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش خطای پرداخت در فرم پذیرش
    /// </summary>
    public class ReceptionPaymentErrorViewModel
    {
        /// <summary>
        /// کد خطا
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// تاریخ خطا
        /// </summary>
        public DateTime ErrorDate { get; set; }

        /// <summary>
        /// آیا خطا قابل تلاش مجدد است؟
        /// </summary>
        public bool IsRetryable { get; set; }

        /// <summary>
        /// تعداد تلاش‌ها
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// حداکثر تعداد تلاش‌ها
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// نمایش کد خطا
        /// </summary>
        public string ErrorCodeDisplay => ErrorCode ?? "نامشخص";

        /// <summary>
        /// نمایش پیام خطا
        /// </summary>
        public string ErrorMessageDisplay => ErrorMessage ?? "خطای نامشخص";

        /// <summary>
        /// نمایش تاریخ خطا (فرمات شده)
        /// </summary>
        public string ErrorDateDisplay => ErrorDate.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// نمایش وضعیت تلاش مجدد
        /// </summary>
        public string RetryStatusDisplay => IsRetryable ? "قابل تلاش مجدد" : "غیرقابل تلاش مجدد";

        /// <summary>
        /// نمایش تعداد تلاش‌ها
        /// </summary>
        public string RetryCountDisplay => $"{RetryCount}/{MaxRetries}";

        /// <summary>
        /// نمایش اطلاعات خطا (فرمات شده)
        /// </summary>
        public string ErrorInfoDisplay => $"{ErrorCodeDisplay} - {ErrorMessageDisplay}";

        /// <summary>
        /// آیا می‌توان تلاش مجدد کرد؟
        /// </summary>
        public bool CanRetry => IsRetryable && RetryCount < MaxRetries;

        /// <summary>
        /// آیا حداکثر تلاش‌ها انجام شده؟
        /// </summary>
        public bool IsMaxRetriesReached => RetryCount >= MaxRetries;

        /// <summary>
        /// نمایش وضعیت تلاش
        /// </summary>
        public string RetryStatus
        {
            get
            {
                if (IsMaxRetriesReached) return "حداکثر تلاش‌ها انجام شده";
                if (CanRetry) return "قابل تلاش مجدد";
                return "غیرقابل تلاش مجدد";
            }
        }
    }
}
