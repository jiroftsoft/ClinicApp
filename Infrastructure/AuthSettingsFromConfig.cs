using System.Configuration;
using ClinicApp.Interfaces.OTP;

namespace ClinicApp.Infrastructure;

/// <summary>
/// پیاده‌سازی اینترفیس تنظیمات احراز هویت با خواندن مقادیر از Web.config
/// </summary>
public class AuthSettingsFromConfig : IAuthSettings
{
    public int OtpLength => GetInt("Otp.Length", 6);
    public int OtpExpiryMinutes => GetInt("Otp.ExpiryMinutes", 2);
    public string OtpHashKey => GetStr("Otp.HashKey", "Default-Super-Secret-And-Strong-Key-For-HMACSHA256");
    public int OtpMaxSendsPerNationalCodePer5Min => GetInt("Otp.MaxSendsPerNationalCodePer5Min", 3);
    public int OtpMaxSendsPerIpPer5Min => GetInt("Otp.MaxSendsPerIpPer5Min", 10);
    public int OtpFailedMaxAttempts => GetInt("Otp.FailedMaxAttempts", 5);
    public int OtpLockoutMinutes => GetInt("Otp.LockoutMinutes", 15);

    private int GetInt(string key, int defaultValue) =>
        int.TryParse(ConfigurationManager.AppSettings[key], out var value) ? value : defaultValue;

    private string GetStr(string key, string defaultValue) =>
        ConfigurationManager.AppSettings[key] ?? defaultValue;
}