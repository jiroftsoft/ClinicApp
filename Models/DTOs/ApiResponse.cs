using System.Collections.Generic;

namespace ClinicApp.Models.DTOs
{
    /// <summary>
    /// مدل استاندارد پاسخ API
    /// </summary>
    /// <typeparam name="T">نوع داده</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// وضعیت موفقیت عملیات
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// پیام پاسخ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// شناسه همبستگی برای ردیابی
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// داده‌های پاسخ
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// لیست خطاها (در صورت وجود)
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// زمان اجرا به میلی‌ثانیه
        /// </summary>
        public long? Duration { get; set; }

        /// <summary>
        /// ایجاد پاسخ موفق
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "عملیات با موفقیت انجام شد", string correlationId = null, long? duration = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                CorrelationId = correlationId,
                Duration = duration
            };
        }

        /// <summary>
        /// ایجاد پاسخ خطا
        /// </summary>
        public static ApiResponse<T> CreateError(string message, string correlationId = null, List<string> errors = null, long? duration = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                CorrelationId = correlationId,
                Errors = errors ?? new List<string>(),
                Duration = duration
            };
        }
    }

    /// <summary>
    /// مدل استاندارد پاسخ API بدون داده
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// ایجاد پاسخ موفق بدون داده
        /// </summary>
        public static ApiResponse CreateSuccess(string message = "عملیات با موفقیت انجام شد", string correlationId = null, long? duration = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                CorrelationId = correlationId,
                Duration = duration
            };
        }

        /// <summary>
        /// ایجاد پاسخ خطا بدون داده
        /// </summary>
        public static ApiResponse CreateError(string message, string correlationId = null, List<string> errors = null, long? duration = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                CorrelationId = correlationId,
                Errors = errors ?? new List<string>(),
                Duration = duration
            };
        }
    }
}
