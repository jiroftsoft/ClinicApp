using System;
using System.Net;

namespace ClinicApp.Services
{
    /// <summary>
    /// خطای درگاه پیامک (مثلاً 403 Forbidden به دلیل IP/موقعیت).
    /// برای UX حرفه‌ای OTP و پیشنهاد خاموش کردن VPN استفاده می‌شود.
    /// </summary>
    public class SmsGatewayException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        public SmsGatewayException(string message, HttpStatusCode? statusCode = null, Exception inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }
}
