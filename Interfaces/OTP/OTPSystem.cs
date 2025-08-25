
using System;
using System.Runtime.Caching;
using System.Web;

namespace ClinicApp.Interfaces.OTP;

public interface IOtpStateStore
{
    void SetState(OtpState state);
    OtpState GetState();
    void ClearState();
}

// 2. اینترفیس برای محدودیت نرخ (جایگزین MemoryCache مستقیم)
public interface IRateLimiter
{
    bool IsRateLimited(string key, int maxAttempts, TimeSpan period);
}

// 3. اینترفیس برای اطلاعات کلاینت (جایگزین HttpContext مستقیم)
public interface IClientInfoProvider
{
    string GetClientIpAddress();
    string GetUserAgent();
}

// 4. کلاس مدل برای نگهداری وضعیت OTP
public class OtpState
{
    public string NationalCode { get; set; }
    public string OtpHash { get; set; }
    public DateTime ExpiryUtc { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string PhoneNumber { get; set; }
}
// پیاده‌سازی با HttpSession
public class HttpSessionOtpStateStore : IOtpStateStore
{
    private const string OtpStateSessionKey = "OtpState";

    public void SetState(OtpState state) => HttpContext.Current.Session[OtpStateSessionKey] = state;
    public OtpState GetState() => HttpContext.Current.Session[OtpStateSessionKey] as OtpState;
    public void ClearState() => HttpContext.Current.Session.Remove(OtpStateSessionKey);
}

// پیاده‌سازی با MemoryCache
public class MemoryCacheRateLimiter : IRateLimiter
{
    private static readonly MemoryCache _cache = MemoryCache.Default;
    public bool IsRateLimited(string key, int maxAttempts, TimeSpan period)
    {
        int count = (int?)(_cache.Get(key)) ?? 0;
        if (count >= maxAttempts) return true;
        _cache.Set(key, count + 1, DateTimeOffset.UtcNow.Add(period));
        return false;
    }
}

// پیاده‌سازی با HttpContext
public class HttpContextClientInfoProvider : IClientInfoProvider
{
    public string GetClientIpAddress() =>
        HttpContext.Current?.Request?.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Split(',')[0]?.Trim()
        ?? HttpContext.Current?.Request?.UserHostAddress ?? "0.0.0.0";

    public string GetUserAgent() => HttpContext.Current?.Request?.UserAgent ?? "unknown";
}

    /// <summary>
    /// اینترفیس برای دسترسی به تنظیمات مربوط به احراز هویت به صورت مستقل
    /// </summary>
    public interface IAuthSettings
    {
        int OtpLength { get; }
        int OtpExpiryMinutes { get; }
        string OtpHashKey { get; }
        int OtpMaxSendsPerNationalCodePer5Min { get; }
        int OtpMaxSendsPerIpPer5Min { get; }
        int OtpFailedMaxAttempts { get; }
        int OtpLockoutMinutes { get; }
    }
