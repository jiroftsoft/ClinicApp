using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Serilog;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس احراز هویت با OTP به‌صورت Production-Grade:
    /// - OTP یکبارمصرف و هش‌شده
    /// - Rate Limiting و IP Throttling
    /// - Session Binding به IP/UA
    /// - اعتبارسنجی دو مرحله‌ای شماره (فرمت + پیش‌شماره مجاز)
    /// - پیام‌های فارسی و لاگ حرفه‌ای
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationSignInManager _signInManager;
        private readonly IIdentityMessageService _smsService;
        private readonly ILogger _log;

        // --- Cache برای Rate Limiting ---
        private static readonly MemoryCache _cache = MemoryCache.Default;

        // --- کلیدهای سشن (نام‌های ثابت) ---
        private const string OtpSessionKey = "OtpCodeHash";
        private const string OtpPhoneSessionKey = "OtpPhone";
        private const string OtpExpirySessionKey = "OtpExpiry";
        private const string OtpIpKey = "OtpBindIp";
        private const string OtpUaKey = "OtpBindUa";

        public AuthService(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IIdentityMessageService smsService,
            ILogger logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _smsService = smsService;
            _log = logger.ForContext<AuthService>();
        }

        #region Public API

        /// <summary>
        /// ارسال OTP امن با تمام کنترل‌های امنیتی و محدودیت‌ها.
        /// </summary>
        public async Task<bool> SendLoginOtpAsync(string phoneNumber)
        {
            var normalized = PhoneNumberHelper.NormalizeToE164(phoneNumber);

            // اعتبارسنجی مرحله 1: نرمال‌سازی/فرمت
            if (string.IsNullOrWhiteSpace(normalized))
            {
                _log.Warning("شماره موبایل نامعتبر برای درخواست OTP: {PhoneNumber}", phoneNumber);
                return false;
            }

            // اعتبارسنجی مرحله 2: پیش‌شماره مجاز
            if (!IsAllowedCountry(normalized))
            {
                _log.Warning("پیش‌شماره مجاز نیست: {PhoneE164}", normalized);
                return false;
            }

            // محدودیت نرخ برای شماره و IP
            if (ExceededRateForPhone(normalized))
            {
                _log.Warning("محدودیت ارسال OTP برای شماره رسید: {PhoneE164}", normalized);
                return false;
            }
            if (ExceededRateForIp(ClientIp))
            {
                _log.Warning("محدودیت ارسال OTP برای IP رسید: {IP}", ClientIp);
                return false;
            }

            try
            {
                var session = HttpContext.Current.Session;
                if (session == null)
                {
                    _log.Error("Session در دسترس نیست هنگام ارسال OTP.");
                    return false;
                }

                // کنترل Lockout موجود
                string lockoutEndKey = GetLockoutEndKey(normalized);
                var lockoutEnd = session[lockoutEndKey] as DateTime?;
                if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.UtcNow)
                {
                    _log.Warning("شماره در حالت قفل است: {PhoneE164}", normalized);
                    return false;
                }

                // تولید OTP امن و هش آن
                int otpLen = GetInt("Otp.Length", 6);
                var otp = GenerateSecureOtp(otpLen);
                var otpHash = HashOtp(otp, normalized);

                // محاسبه انقضا
                var expiry = DateTime.UtcNow.AddMinutes(GetInt("Otp.ExpiryMinutes", 2));

                // بایند به سشن (IP/UA) + ذخیره هش (نه خود OTP)
                BindSession(normalized, otpHash, expiry);

                // ارسال پیامک (هرگز OTP را لاگ نکن)
                var message = $"کد تأیید شما برای ورود: {otp} (اعتبار {GetInt("Otp.ExpiryMinutes", 2)} دقیقه)";
                await _smsService.SendAsync(new IdentityMessage { Destination = normalized, Body = message });

                _log.Information("OTP با موفقیت ارسال شد به {PhoneE164}", normalized);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "ارسال OTP با خطا مواجه شد برای {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        /// <summary>
        /// تأیید OTP و ورود. شامل Session Binding، یکبارمصرف بودن، Lockout و پیام‌های فارسی.
        /// </summary>
        public async Task<SignInStatus> VerifyLoginOtpAndSignInAsync(string phoneNumber, string otpCode)
        {
            var session = HttpContext.Current.Session;
            if (session == null)
            {
                _log.Error("Session در دسترس نیست هنگام تأیید OTP.");
                return SignInStatus.Failure;
            }

            var normalized = PhoneNumberHelper.NormalizeToE164(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalized) || !IsAllowedCountry(normalized))
            {
                _log.Warning("تلاش تأیید با شماره نامعتبر یا پیش‌شماره غیرمجاز: {Phone}", phoneNumber);
                return SignInStatus.Failure;
            }

            // Lockout check
            int maxFailed = GetInt("Otp.FailedMaxAttempts", 5);
            int lockoutMin = GetInt("Otp.LockoutMinutes", 15);
            string failedAttemptsKey = GetFailedAttemptsKey(normalized);
            string lockoutEndKey = GetLockoutEndKey(normalized);

            var lockoutEnd = session[lockoutEndKey] as DateTime?;
            if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.UtcNow)
            {
                _log.Warning("تلاش ورود برای شماره در حالت قفل: {Phone}", normalized);
                return SignInStatus.LockedOut;
            }

            try
            {
                // خواندن داده‌های ذخیره‌شده
                var savedHash = session[OtpSessionKey] as string;
                var savedPhone = session[OtpPhoneSessionKey] as string;
                var expiry = session[OtpExpirySessionKey] as DateTime?;

                // اعتبار Session Binding
                if (!SessionBindingValid())
                {
                    _log.Warning("Session binding نامعتبر (IP/UA mismatch) برای {Phone}", normalized);
                    RegisterFailedAttempt(session, normalized, lockoutMin, maxFailed);
                    return SignInStatus.Failure;
                }

                // صحت داده‌ها و انقضا
                if (savedHash == null || savedPhone != normalized || !expiry.HasValue || DateTime.UtcNow > expiry.Value)
                {
                    _log.Warning("OTP نامعتبر یا منقضی شده برای {Phone}", normalized);
                    RegisterFailedAttempt(session, normalized, lockoutMin, maxFailed);
                    return SignInStatus.Failure;
                }

                // مقایسه هش‌ها (مقاوم به زمان‌بندی)
                var incomingHash = HashOtp(otpCode, normalized);
                if (!SlowEquals(savedHash, incomingHash))
                {
                    RegisterFailedAttempt(session, normalized, lockoutMin, maxFailed);
                    return SignInStatus.Failure;
                }

                // پاک‌سازی کامل (یکبارمصرف)
                ClearOtpSession(session);
                session.Remove(failedAttemptsKey);
                session.Remove(lockoutEndKey);

                // ورود یا ادامه فرآیند ثبت‌نام
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalized);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return SignInStatus.Success;
                }

                // در صورت عدم وجود کاربر: می‌توان هدایت به تکمیل پروفایل/ثبت‌نام کرد
                return SignInStatus.RequiresVerification;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در تأیید OTP برای {Phone}", phoneNumber);
                return SignInStatus.Failure;
            }
        }

        #endregion

        #region Helpers (Security, Rate Limiting, Session Binding)

        private int GetInt(string key, int @default) =>
            int.TryParse(ConfigurationManager.AppSettings[key], out var v) ? v : @default;

        private string GetStr(string key, string @default) =>
            ConfigurationManager.AppSettings[key] ?? @default;

        private string ClientIp =>
            HttpContext.Current?.Request?.ServerVariables["HTTP_X_FORWARDED_FOR"]?.Split(',')[0]?.Trim()
            ?? HttpContext.Current?.Request?.UserHostAddress
            ?? "0.0.0.0";

        private string ClientUa =>
            HttpContext.Current?.Request?.UserAgent ?? "unknown";

        private bool IsAllowedCountry(string e164Phone)
        {
            var allowed = GetStr("Phone.AllowedCountryCodes", "+98").Split(',');
            return allowed.Any(c => e164Phone.StartsWith(c.Trim(), StringComparison.Ordinal));
        }

        // تولید OTP امن (عدد خالص)
        private string GenerateSecureOtp(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append((b % 10).ToString());
            return sb.ToString();
        }

        // هش OTP با HMACSHA256 و کلید از تنظیمات
        private string HashOtp(string otp, string phoneE164)
        {
            var key = Encoding.UTF8.GetBytes(GetStr("Otp.HashKey", "ChangeMe-StrongKey-64bytes"));
            using (var hmac = new HMACSHA256(key))
            {
                var data = Encoding.UTF8.GetBytes($"{phoneE164}|{otp}");
                return Convert.ToBase64String(hmac.ComputeHash(data));
            }
        }

        // مقایسه ثابت-زمان
        private bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // Rate limit per phone (پنجره ۵ دقیقه)
        private bool ExceededRateForPhone(string phoneE164)
        {
            int maxPer5 = GetInt("Otp.MaxSendsPerPhonePer5Min", 3);
            string key = $"otp:send:phone:{phoneE164}";
            int count = (int?)_cache.Get(key) ?? 0;
            if (count >= maxPer5) return true;
            _cache.Set(key, count + 1, DateTimeOffset.UtcNow.AddMinutes(5));
            return false;
        }

        // Rate limit per IP (پنجره ۵ دقیقه)
        private bool ExceededRateForIp(string ip)
        {
            int maxPer5 = GetInt("Otp.MaxSendsPerIpPer5Min", 10);
            string key = $"otp:send:ip:{ip}";
            int count = (int?)_cache.Get(key) ?? 0;
            if (count >= maxPer5) return true;
            _cache.Set(key, count + 1, DateTimeOffset.UtcNow.AddMinutes(5));
            return false;
        }

        // Session binding + ذخیره هش و انقضا
        private void BindSession(string phoneE164, string otpHash, DateTime expiryUtc)
        {
            var s = HttpContext.Current.Session;
            s[OtpPhoneSessionKey] = phoneE164;
            s[OtpSessionKey] = otpHash;
            s[OtpExpirySessionKey] = expiryUtc;
            s[OtpIpKey] = ClientIp;
            s[OtpUaKey] = ClientUa;
        }

        private bool SessionBindingValid()
        {
            var s = HttpContext.Current.Session;
            return (s[OtpIpKey] as string) == ClientIp
                && (s[OtpUaKey] as string) == ClientUa;
        }

        private void ClearOtpSession(HttpSessionState session)
        {
            session.Remove(OtpPhoneSessionKey);
            session.Remove(OtpSessionKey);
            session.Remove(OtpExpirySessionKey);
            session.Remove(OtpIpKey);
            session.Remove(OtpUaKey);
        }

        // Lockout و شمارش خطاهای OTP
        private void RegisterFailedAttempt(HttpSessionState session, string phoneE164, int lockoutMin, int maxFailed)
        {
            string failedAttemptsKey = GetFailedAttemptsKey(phoneE164);
            string lockoutEndKey = GetLockoutEndKey(phoneE164);

            int failed = (session[failedAttemptsKey] as int? ?? 0) + 1;
            session[failedAttemptsKey] = failed;

            if (failed >= maxFailed)
            {
                session[lockoutEndKey] = DateTime.UtcNow.AddMinutes(lockoutMin);
                _log.Warning("شماره {Phone} به مدت {Min} دقیقه قفل شد.", phoneE164, lockoutMin);
            }
        }

        private string GetFailedAttemptsKey(string phoneE164) => $"OtpFailedAttempts_{phoneE164}";
        private string GetLockoutEndKey(string phoneE164) => $"OtpLockoutEnd_{phoneE164}";

        #endregion
    }
}
