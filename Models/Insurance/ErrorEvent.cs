using System;

namespace ClinicApp.Models.Insurance
{
    /// <summary>
    /// رویداد خطا در سیستم بیمه تکمیلی
    /// </summary>
    public class ErrorEvent
    {
        /// <summary>
        /// نوع خطا
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Stack Trace خطا
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// شناسه بیمار (در صورت وجود)
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// شناسه خدمت (در صورت وجود)
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// شناسه کلینیک
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// سطح خطا (Error, Warning, Critical)
        /// </summary>
        public string Severity { get; set; } = "Error";

        /// <summary>
        /// کد خطا
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// زمان وقوع خطا
        /// </summary>
        public DateTime ErrorTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// زمان ایجاد رویداد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// اطلاعات اضافی خطا
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// شناسه Session
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// IP Address کاربر
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent مرورگر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// URL درخواست
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// HTTP Method
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// شناسه درخواست
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// آیا خطا حل شده است
        /// </summary>
        public bool IsResolved { get; set; } = false;

        /// <summary>
        /// زمان حل خطا
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// کاربر حل کننده خطا
        /// </summary>
        public string ResolvedBy { get; set; }

        /// <summary>
        /// توضیحات حل خطا
        /// </summary>
        public string ResolutionNotes { get; set; }

        /// <summary>
        /// تعداد تکرار خطا
        /// </summary>
        public int OccurrenceCount { get; set; } = 1;

        /// <summary>
        /// آخرین زمان تکرار خطا
        /// </summary>
        public DateTime LastOccurrence { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// دریافت خلاصه خطا
        /// </summary>
        /// <returns>خلاصه رویداد خطا</returns>
        public string GetSummary()
        {
            return $"Error Event - Type: {ErrorType}, Message: {Message}, " +
                   $"PatientId: {PatientId}, ServiceId: {ServiceId}, " +
                   $"Severity: {Severity}, OccurrenceCount: {OccurrenceCount}, " +
                   $"IsResolved: {IsResolved}";
        }

        /// <summary>
        /// بررسی آیا خطا بحرانی است
        /// </summary>
        /// <returns>آیا خطا بحرانی است</returns>
        public bool IsCritical()
        {
            return Severity?.ToLower() == "critical";
        }

        /// <summary>
        /// بررسی آیا خطا هشدار است
        /// </summary>
        /// <returns>آیا خطا هشدار است</returns>
        public bool IsWarning()
        {
            return Severity?.ToLower() == "warning";
        }

        /// <summary>
        /// بررسی آیا خطا تکراری است
        /// </summary>
        /// <returns>آیا خطا تکراری است</returns>
        public bool IsRecurring()
        {
            return OccurrenceCount > 1;
        }

        /// <summary>
        /// محاسبه مدت زمان حل نشدن خطا
        /// </summary>
        /// <returns>مدت زمان حل نشدن خطا (به ساعت)</returns>
        public double GetUnresolvedDurationHours()
        {
            if (IsResolved && ResolvedAt.HasValue)
            {
                return (ResolvedAt.Value - ErrorTime).TotalHours;
            }
            return (DateTime.UtcNow - ErrorTime).TotalHours;
        }

        /// <summary>
        /// به‌روزرسانی تعداد تکرار خطا
        /// </summary>
        public void UpdateOccurrence()
        {
            OccurrenceCount++;
            LastOccurrence = DateTime.UtcNow;
        }

        /// <summary>
        /// حل کردن خطا
        /// </summary>
        /// <param name="resolvedBy">کاربر حل کننده</param>
        /// <param name="resolutionNotes">توضیحات حل</param>
        public void Resolve(string resolvedBy, string resolutionNotes = null)
        {
            IsResolved = true;
            ResolvedAt = DateTime.UtcNow;
            ResolvedBy = resolvedBy;
            ResolutionNotes = resolutionNotes;
        }
    }
}
