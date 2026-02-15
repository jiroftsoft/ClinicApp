using System;
using System.Web;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای ساخت URL های پرداخت (CallbackUrl, SuccessUrl, ErrorUrl)
    /// طراحی شده طبق اصول SRP - مسئولیت: ساخت URL های پرداخت
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. استفاده از PaymentBaseUrl از تنظیمات (اگر تنظیم شده باشد)
    /// 2. Fallback به Request.Url (اگر BaseUrl تنظیم نشده باشد)
    /// 3. پشتیبانی از Development و Production
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public static class PaymentUrlHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(PaymentUrlHelper));

        /// <summary>
        /// ساخت CallbackUrl کامل برای درگاه‌های پرداخت
        /// 
        /// منطق:
        /// 1. اگر PaymentBaseUrl در Web.config تنظیم شده باشد، از آن استفاده می‌شود
        /// 2. در غیر این صورت، از Request.Url استفاده می‌شود (Fallback)
        /// 
        /// مثال:
        /// - Production: https://yourdomain.com/Patient/AppointmentBooking/PaymentCallback
        /// - Development: http://localhost:3560/Patient/AppointmentBooking/PaymentCallback
        /// </summary>
        /// <param name="relativePath">مسیر نسبی (مثلاً: /Patient/AppointmentBooking/PaymentCallback)</param>
        /// <param name="request">HttpRequestBase برای Fallback</param>
        /// <param name="appSettings">تنظیمات سیستم (اختیاری - اگر null باشد، از Instance استفاده می‌شود)</param>
        /// <returns>URL کامل (مثلاً: https://yourdomain.com/Patient/AppointmentBooking/PaymentCallback)</returns>
        public static string BuildPaymentCallbackUrl(string relativePath, HttpRequestBase request, IAppSettings appSettings = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    _logger.Warning("⚠️ PaymentUrlHelper: relativePath خالی است");
                    throw new ArgumentException("relativePath cannot be null or empty", nameof(relativePath));
                }

                if (request == null)
                {
                    _logger.Warning("⚠️ PaymentUrlHelper: request null است");
                    throw new ArgumentNullException(nameof(request));
                }

                // ✅ استفاده از appSettings یا Instance
                appSettings = appSettings ?? AppSettings.Instance;

                // ✅ STEP 0: وقتی درخواست از localhost است (توسعه)، همیشه از Request.Url استفاده کن
                // تا پس از پرداخت در سندباکس زرین‌پال به همان localhost برگردد و درگاه به درگاه وصل شود
                var host = request.Url?.Host ?? "";
                var isLocalhost = host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host == "127.0.0.1";
                if (isLocalhost)
                {
                    var scheme = request.Url.Scheme;
                    var port = request.Url.Port != 80 && request.Url.Port != 443 ? $":{request.Url.Port}" : "";
                    var localUrl = $"{scheme}://{host}{port}{relativePath}";
                    _logger.Information("✅ PaymentUrlHelper: درخواست از localhost است؛ CallbackUrl از Request.Url ساخته شد - {CallbackUrl}", localUrl);
                    return localUrl;
                }

                // ✅ STEP 1: بررسی PaymentBaseUrl از تنظیمات
                var baseUrl = appSettings.PaymentBaseUrl;
                
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    // ✅ استفاده از BaseUrl از تنظیمات
                    var fullUrl = $"{baseUrl.TrimEnd('/')}{relativePath}";
                    _logger.Information("✅ PaymentUrlHelper: CallbackUrl از PaymentBaseUrl ساخته شد - {CallbackUrl}", fullUrl);
                    return fullUrl;
                }

                // ✅ STEP 2: Fallback به Request.Url
                var fallbackScheme = request.Url.Scheme;
                var fallbackPort = request.Url.Port != 80 && request.Url.Port != 443 ? $":{request.Url.Port}" : "";
                var fallbackUrl = $"{fallbackScheme}://{host}{fallbackPort}{relativePath}";
                
                _logger.Warning("⚠️ PaymentUrlHelper: PaymentBaseUrl تنظیم نشده است، استفاده از Request.Url (Fallback) - {CallbackUrl}", fallbackUrl);
                return fallbackUrl;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PaymentUrlHelper: خطا در ساخت CallbackUrl - relativePath: {RelativePath}", relativePath);
                throw;
            }
        }

        /// <summary>
        /// ساخت URL کامل برای درگاه‌های پرداخت (عمومی)
        /// </summary>
        /// <param name="relativePath">مسیر نسبی</param>
        /// <param name="request">HttpRequestBase</param>
        /// <param name="appSettings">تنظیمات سیستم (اختیاری)</param>
        /// <returns>URL کامل</returns>
        public static string BuildPaymentUrl(string relativePath, HttpRequestBase request, IAppSettings appSettings = null)
        {
            return BuildPaymentCallbackUrl(relativePath, request, appSettings);
        }
    }
}

