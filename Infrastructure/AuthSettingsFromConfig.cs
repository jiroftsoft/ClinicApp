using System;
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
    // ✅ OPTIMIZATION: Changed from 5 minutes to 10 minutes per checklist
    public int OtpMaxSendsPerNationalCodePer10Min => GetInt("Otp.MaxSendsPerNationalCodePer10Min", 3);
    public int OtpMaxSendsPerIpPer10Min => GetInt("Otp.MaxSendsPerIpPer10Min", 10);
    
    // ✅ Backward compatibility (deprecated - use Per10Min)
    [Obsolete("Use OtpMaxSendsPerNationalCodePer10Min instead")]
    public int OtpMaxSendsPerNationalCodePer5Min => OtpMaxSendsPerNationalCodePer10Min;
    
    [Obsolete("Use OtpMaxSendsPerIpPer10Min instead")]
    public int OtpMaxSendsPerIpPer5Min => OtpMaxSendsPerIpPer10Min;
    public int OtpFailedMaxAttempts => GetInt("Otp.FailedMaxAttempts", 5);
    public int OtpLockoutMinutes => GetInt("Otp.LockoutMinutes", 15);
    public int OtpMaxVerificationAttempts => GetInt("Otp.MaxVerificationAttempts", 5); // ✅ حداکثر تلاش برای تایید یک OTP

    private int GetInt(string key, int defaultValue) =>
        int.TryParse(ConfigurationManager.AppSettings[key], out var value) ? value : defaultValue;

    private string GetStr(string key, string defaultValue) =>
        ConfigurationManager.AppSettings[key] ?? defaultValue;
}