using System;
using System.Threading.Tasks;

namespace ClinicApp.Services.Idempotency
{
    /// <summary>
    /// سرویس مدیریت Idempotency برای جلوگیری از درخواست‌های تکراری
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// بررسی و ثبت کلید Idempotency
        /// </summary>
        /// <param name="key">کلید Idempotency</param>
        /// <param name="ttlMinutes">زمان انقضا به دقیقه</param>
        /// <param name="scope">حوزه استفاده (اختیاری)</param>
        /// <returns>true اگر کلید معتبر و جدید باشد، false اگر تکراری باشد</returns>
        Task<bool> TryUseKeyAsync(string key, int ttlMinutes = 30, string scope = "default");
        
        /// <summary>
        /// حذف کلید Idempotency
        /// </summary>
        /// <param name="key">کلید Idempotency</param>
        /// <param name="scope">حوزه استفاده (اختیاری)</param>
        Task RemoveKeyAsync(string key, string scope = "default");
        
        /// <summary>
        /// پاکسازی کلیدهای منقضی شده
        /// </summary>
        /// <param name="scope">حوزه استفاده (اختیاری)</param>
        Task CleanupExpiredKeysAsync(string scope = "default");
    }
}
