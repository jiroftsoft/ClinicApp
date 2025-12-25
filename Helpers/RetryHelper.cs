using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class for retry operations
    /// کلاس کمکی برای تلاش مجدد
    /// </summary>
    public static class RetryHelper
    {
        #region Synchronous Retry

        /// <summary>
        /// Retries an operation multiple times before failing
        /// تلاش مجدد برای اجرای عملیات
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="maxRetries">Maximum retry attempts (default: 3)</param>
        /// <param name="delayMs">Delay between retries in milliseconds (default: 1000)</param>
        /// <returns>Result of operation</returns>
        /// <example>
        /// var result = RetryHelper.Retry(() => CallExternalApi(), 3, 2000);
        /// </example>
        public static T Retry<T>(Func<T> operation, int maxRetries = 3, int delayMs = 1000)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (maxRetries < 1)
                throw new ArgumentException("Maximum retries must be at least 1", nameof(maxRetries));

            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i == maxRetries - 1)
                        throw;

                    if (delayMs > 0)
                        Thread.Sleep(delayMs);
                }
            }

            throw new Exception($"Operation failed after {maxRetries} retries", lastException);
        }

        /// <summary>
        /// Retries a void operation
        /// تلاش مجدد برای عملیات بدون بازگشت
        /// </summary>
        public static void Retry(Action operation, int maxRetries = 3, int delayMs = 1000)
        {
            Retry(() =>
            {
                operation();
                return true;
            }, maxRetries, delayMs);
        }

        #endregion

        #region Async Retry

        /// <summary>
        /// Retries an async operation multiple times
        /// تلاش مجدد برای عملیات Async
        /// </summary>
        public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMs = 1000)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (maxRetries < 1)
                throw new ArgumentException("Maximum retries must be at least 1", nameof(maxRetries));

            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i == maxRetries - 1)
                        throw;

                    if (delayMs > 0)
                        await Task.Delay(delayMs);
                }
            }

            throw new Exception($"Operation failed after {maxRetries} retries", lastException);
        }

        /// <summary>
        /// Retries an async void operation
        /// تلاش مجدد برای عملیات Async بدون بازگشت
        /// </summary>
        public static async Task RetryAsync(Func<Task> operation, int maxRetries = 3, int delayMs = 1000)
        {
            await RetryAsync(async () =>
            {
                await operation();
                return true;
            }, maxRetries, delayMs);
        }

        #endregion

        #region Exponential Backoff

        /// <summary>
        /// Retries with exponential backoff delay
        /// تلاش مجدد با تاخیر افزایشی
        /// </summary>
        public static T RetryWithExponentialBackoff<T>(Func<T> operation, int maxRetries = 3, int initialDelayMs = 1000)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            Exception lastException = null;
            int delay = initialDelayMs;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (i == maxRetries - 1)
                        throw;

                    Thread.Sleep(delay);
                    delay *= 2; // Exponential backoff
                }
            }

            throw new Exception($"Operation failed after {maxRetries} retries", lastException);
        }

        #endregion

        #region Conditional Retry

        /// <summary>
        /// Retries only for specific exception types
        /// تلاش مجدد فقط برای خطاهای خاص
        /// </summary>
        public static T RetryOn<T, TException>(Func<T> operation, int maxRetries = 3, int delayMs = 1000) 
            where TException : Exception
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            Exception lastException = null;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return operation();
                }
                catch (TException ex)
                {
                    lastException = ex;

                    if (i == maxRetries - 1)
                        throw;

                    if (delayMs > 0)
                        Thread.Sleep(delayMs);
                }
            }

            throw new Exception($"Operation failed after {maxRetries} retries", lastException);
        }

        #endregion
    }
}
