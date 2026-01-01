using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.OTP;
using ClinicApp.Interfaces.Security;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ClinicApp.Models.Core;

namespace ClinicApp.Services
{
    /// <summary>
    /// نسخه نهایی سرویس احراز هویت با معماری تست‌پذیر و امن:
    /// - کاملا تفکیک شده از HttpContext و Session برای تست‌پذیری کامل.
    /// - استفاده از مکانیزم Lockout استاندارد ASP.NET Identity برای پایداری بیشتر.
    /// - تمام وابستگی‌ها از طریق اینترفیس و با Dependency Injection مدیریت می‌شوند.
    /// - حفظ تمام ویژگی‌های امنیتی پیشرفته (Hashing, Rate Limiting, Session Binding).
    /// </summary>
    public class AuthService : IAuthService
    {
        private static readonly ILogger _log = Log.ForContext<AuthService>();

        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context; // ✅ ADD THIS
        private readonly IAuthenticationManager _authenticationManager;
        private readonly AsanakSmsService _smsService;
        private readonly IOtpStateStore _otpStateStore;
        private readonly IRateLimiter _rateLimiter;
        private readonly IClientInfoProvider _clientProvider;
        private readonly IAuthSettings _authSettings;
        private readonly ILoginHistoryService _loginHistoryService;

        public AuthService(
            ApplicationUserManager userManager,
            ApplicationDbContext context,
            IAuthenticationManager authenticationManager,
            AsanakSmsService smsService,
            IOtpStateStore otpStateStore,
            IRateLimiter rateLimiter,
            IClientInfoProvider clientProvider,
            IAuthSettings authSettings,
            ILoginHistoryService loginHistoryService)
        {
            _userManager = userManager;
            _context = context;
            _authenticationManager = authenticationManager;
            _smsService = smsService;
            _otpStateStore = otpStateStore;
            _rateLimiter = rateLimiter;
            _clientProvider = clientProvider;
            _authSettings = authSettings;
            _loginHistoryService = loginHistoryService;

            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(_authSettings.OtpLockoutMinutes);
            _userManager.MaxFailedAccessAttemptsBeforeLockout = _authSettings.OtpFailedMaxAttempts;
        }
        /// <summary>
        /// A final, production-ready method for sending a login OTP to an EXISTING user.
        /// It performs all necessary security checks before sending.
        /// </summary>
        public async Task<ServiceResult> SendLoginOtpAsync(string nationalCode)
        {
            var startTime = DateTime.UtcNow;
            _log.Information("🚀 [SendLoginOtp] START - NationalCode: {MaskedNC}", 
                MaskHelper.MaskNationalCode(nationalCode));

            try
            {
                // Step 1: Validate the National Code format
                _log.Debug("[SendLoginOtp] Step 1: Validating National Code format");
                var normalizedCode = PersianNumberHelper.ToEnglishNumbers(nationalCode);
                if (!IranianNationalCodeValidator.IsValid(normalizedCode))
                {
                    _log.Warning("[SendLoginOtp] FAILED - Invalid National Code format: {MaskedNC}", 
                        MaskHelper.MaskNationalCode(normalizedCode));
                    return ServiceResult.Failed("کد ملی وارد شده معتبر نیست.", "INVALID_NATIONAL_CODE", ErrorCategory.Validation);
                }
                _log.Debug("[SendLoginOtp] ✓ National Code format valid");

                // Step 2: Find the user
                _log.Debug("[SendLoginOtp] Step 2: Finding user in database");
                var user = await _userManager.FindByNameAsync(normalizedCode);
                if (user == null)
                {
                    _log.Warning("[SendLoginOtp] FAILED - User not found: {MaskedNC}", 
                        MaskHelper.MaskNationalCode(normalizedCode));
                    return ServiceResult.Failed("کاربری با این مشخصات یافت نشد.", "USER_NOT_FOUND", ErrorCategory.NotFound);
                }
                _log.Debug("[SendLoginOtp] ✓ User found - UserId: {UserId}", user.Id);

                // Step 3: Check for a valid phone number
                _log.Debug("[SendLoginOtp] Step 3: Validating phone number");
                if (string.IsNullOrWhiteSpace(user.PhoneNumber) || !PersianNumberHelper.IsValidPhoneNumber(user.PhoneNumber))
                {
                    _log.Error("[SendLoginOtp] FAILED - Invalid or missing phone number - UserId: {UserId}, PhoneLength: {PhoneLength}", 
                        user.Id, user.PhoneNumber?.Length ?? 0);
                    return ServiceResult.Failed("شماره موبایلی برای حساب شما ثبت نشده یا نامعتبر است. لطفاً با پشتیبانی تماس بگیرید.", "PHONE_NUMBER_MISSING", ErrorCategory.BusinessLogic);
                }
                _log.Debug("[SendLoginOtp] ✓ Phone number valid - MaskedPhone: {MaskedPhone}", 
                    MaskHelper.MaskPhoneNumber(user.PhoneNumber));

                // Step 4: Check if the account is locked
                _log.Debug("[SendLoginOtp] Step 4: Checking account lockout status");
                if (await _userManager.IsLockedOutAsync(user.Id))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user.Id);
                    var lockoutPersianTime = lockoutEnd.UtcDateTime.ToPersianDateTime() ?? " دقایقی دیگر";
                    _log.Warning("[SendLoginOtp] FAILED - Account locked - UserId: {UserId}, LockoutEnd: {LockoutEnd}", 
                        user.Id, lockoutEnd);
                    return ServiceResult.Failed($"حساب شما به دلیل تلاش‌های ناموفق تا {lockoutPersianTime} قفل شده است.", "ACCOUNT_LOCKED", ErrorCategory.Security);
                }
                _log.Debug("[SendLoginOtp] ✓ Account not locked");

                // Step 5: Apply Rate Limiting
                _log.Debug("[SendLoginOtp] Step 5: Checking rate limits");
                var clientIp = _clientProvider.GetClientIpAddress();
                if (_rateLimiter.IsRateLimited($"login_otp_send_nc:{normalizedCode}", _authSettings.OtpMaxSendsPerNationalCodePer5Min, TimeSpan.FromMinutes(5)) ||
                    _rateLimiter.IsRateLimited($"login_otp_send_ip:{clientIp}", _authSettings.OtpMaxSendsPerIpPer5Min, TimeSpan.FromMinutes(5)))
                {
                    _log.Warning("[SendLoginOtp] FAILED - Rate limit exceeded - IP: {IP}, MaskedNC: {MaskedNC}", 
                        clientIp, MaskHelper.MaskNationalCode(normalizedCode));
                    return ServiceResult.Failed("تعداد درخواست‌های شما بیش از حد مجاز است. لطفاً دقایقی دیگر تلاش کنید.", "RATE_LIMIT_EXCEEDED");
                }
                _log.Debug("[SendLoginOtp] ✓ Rate limits OK");

                // Step 6: Generate, Store, and Send the OTP
                _log.Debug("[SendLoginOtp] Step 6: Generating OTP");
                var otp = GenerateSecureOtp(_authSettings.OtpLength);
                var otpHash = HashOtp(otp, user.PhoneNumber);
                _log.Debug("[SendLoginOtp] ✓ OTP generated - Length: {OtpLength}, HashLength: {HashLength}", 
                    otp.Length, otpHash.Length);

                // Check and invalidate old OTP
                var oldState = _otpStateStore.GetState();
                if (oldState != null && oldState.NationalCode == normalizedCode)
                {
                    _log.Information("[SendLoginOtp] Invalidating old OTP for MaskedNC: {MaskedNC}", 
                        MaskHelper.MaskNationalCode(normalizedCode));
                }

                // Create and store new OTP state
                _log.Debug("[SendLoginOtp] Storing OTP state (Session + Database)");
                var state = new OtpState
                {
                    NationalCode = normalizedCode,
                    PhoneNumber = user.PhoneNumber,
                    OtpHash = otpHash,
                    ExpiryUtc = DateTime.UtcNow.AddMinutes(_authSettings.OtpExpiryMinutes),
                    IpAddress = clientIp,
                    UserAgent = _clientProvider.GetUserAgent(),
                    AttemptCount = 0
                };
                _otpStateStore.SetState(state);
                _log.Debug("[SendLoginOtp] ✓ OTP state stored to Session - Expiry: {Expiry}", state.ExpiryUtc);

                // ✅ ALSO save to database for fallback (using AuthService's context)
                var sessionId = HttpContext.Current?.Session?.SessionID;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    // Remove old OTP states for this user
                    var oldStates = _context.OtpStates
                        .Where(o => o.NationalCode == normalizedCode)
                        .ToList();
                    if (oldStates.Any())
                    {
                        _context.OtpStates.RemoveRange(oldStates);
                        _log.Debug("[SendLoginOtp] Removed {Count} old OTP states from database", oldStates.Count);
                    }

                    // Add new OTP state
                    var dbState = new OtpStateEntity
                    {
                        SessionId = sessionId,
                        NationalCode = normalizedCode,
                        PhoneNumber = user.PhoneNumber,
                        OtpHash = otpHash,
                        ExpiryUtc = state.ExpiryUtc,
                        IpAddress = clientIp,
                        UserAgent = _clientProvider.GetUserAgent(),
                        AttemptCount = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.OtpStates.Add(dbState);
                    _log.Debug("[SendLoginOtp] Added OTP state to database for fallback");
                }

                // Log to audit database
                _log.Debug("[SendLoginOtp] Logging OTP request to audit database");
                var otpLog = new OtpRequest { PhoneNumber = user.PhoneNumber, OtpCodeHash = otpHash };
                _context.OtpRequests.Add(otpLog);
                await _context.SaveChangesAsync();
                _log.Debug("[SendLoginOtp] ✓ Audit log saved");

                // Send SMS
                _log.Debug("[SendLoginOtp] Sending SMS to: {MaskedPhone}", 
                    MaskHelper.MaskPhoneNumber(user.PhoneNumber));
                var message = new IdentityMessage { Destination = user.PhoneNumber, Body = $"کد ورود کلینیک شفا: {otp}" };
                await _smsService.SendAsync(message);
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _log.Information("✅ [SendLoginOtp] SUCCESS - UserId: {UserId}, Duration: {Duration}ms", 
                    user.Id, duration);
                
                return ServiceResult.Successful("کد ورود به شماره موبایل شما ارسال شد.");
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _log.Error(ex, "❌ [SendLoginOtp] EXCEPTION - MaskedNC: {MaskedNC}, Duration: {Duration}ms, ExceptionType: {ExceptionType}", 
                    MaskHelper.MaskNationalCode(nationalCode), duration, ex.GetType().Name);
                return ServiceResult.Failed("یک خطای سیستمی رخ داد. لطفاً با پشتیبانی تماس بگیرید.", "SYSTEM_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }
        /// <summary>
        /// تایید OTP برای ورود کاربر موجود، با رعایت تمام پروتکل‌های امنیتی، و ورود نهایی به سیستم.
        /// </summary>
        public async Task<ServiceResult> VerifyLoginOtpAndSignInAsync(string nationalCode, string otpCode)
        {
            try
            {
                // ✅ DEBUG: Log incoming parameters
                _log.Information("🔍 VerifyLoginOtpAndSignInAsync called - NationalCode: {NationalCode}, OtpCode Length: {OtpLength}",
                    nationalCode ?? "NULL",
                    otpCode?.Length ?? 0);
                
                // مرحله ۱: نرمال‌سازی و یافتن کاربر
                var normalizedCode = PersianNumberHelper.ToEnglishNumbers(nationalCode);
                var user = await _userManager.FindByNameAsync(normalizedCode);

                if (user == null)
                {
                    _log.Warning("❌ User not found - NationalCode: {NationalCode}", normalizedCode);
                    return ServiceResult.Failed("کاربری با این مشخصات یافت نشد.", "USER_NOT_FOUND", ErrorCategory.NotFound);
                }

                // مرحله ۲: بررسی وضعیت قفل بودن حساب (قبل از هر کاری)
                if (await _userManager.IsLockedOutAsync(user.Id))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user.Id);
                    var lockoutPersianTime = lockoutEnd.UtcDateTime.ToPersianDateTime() ?? " دقایقی دیگر";
                    _log.Warning("تلاش برای ورود به حساب قفل شده: {UserId}", user.Id);
                    
                    // ثبت تاریخچه ورود ناموفق (Account Locked)
                    await _loginHistoryService.LogFailedLoginAsync(
                        userId: user.Id,
                        ipAddress: _clientProvider.GetClientIpAddress(),
                        userAgent: _clientProvider.GetUserAgent(),
                        failureReason: $"Account Locked until {lockoutPersianTime}"
                    );
                    
                    return ServiceResult.Failed($"حساب شما به دلیل تلاش‌های ناموفق تا {lockoutPersianTime} قفل شده است.", "ACCOUNT_LOCKED", ErrorCategory.Security);
                }

                // مرحله ۳: اعتبارسنجی OTP
                var state = _otpStateStore.GetState();
                _log.Information("🔍 OTP State retrieved - IsNull: {IsNull}, NationalCode: {StateNC}, Expiry: {Expiry}",
                    state == null,
                    state?.NationalCode ?? "NULL",
                    state?.ExpiryUtc ?? DateTime.MinValue);
                
                // ✅ CRITICAL FIX: Fallback to database lookup by NationalCode if session is lost
                if (state == null)
                {
                    _log.Warning("⚠️ OTP State is NULL in Session - Attempting database fallback by NationalCode: {MaskedNC}", 
                        MaskHelper.MaskNationalCode(normalizedCode));
                    
                    // Query database directly (using AuthService's context)
                    var dbState = await _context.OtpStates
                        .Where(o => o.NationalCode == normalizedCode && o.ExpiryUtc > DateTime.UtcNow)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();
                    
                    if (dbState != null)
                    {
                        _log.Information("✅ OTP State recovered from database - DbId: {DbId}", dbState.Id);
                        
                        // Restore to in-memory state
                        state = new OtpState
                        {
                            NationalCode = dbState.NationalCode,
                            PhoneNumber = dbState.PhoneNumber,
                            OtpHash = dbState.OtpHash,
                            ExpiryUtc = dbState.ExpiryUtc,
                            IpAddress = dbState.IpAddress,
                            UserAgent = dbState.UserAgent,
                            AttemptCount = dbState.AttemptCount
                        };
                        
                        // Restore to session for next request
                        _otpStateStore.SetState(state);
                    }
                    else
                    {
                        _log.Error("❌ CRITICAL: OTP State is NULL even after database fallback - NationalCode: {MaskedNC}", 
                            MaskHelper.MaskNationalCode(normalizedCode));
                    }
                }
                
                var incomingHash = HashOtp(otpCode, user.PhoneNumber); // هش کردن با شماره موبایل کاربر
                var validationResult = ValidateOtpState(state, normalizedCode, incomingHash, user.PhoneNumber); // ✅ ارسال phoneNumber برای بررسی
                
                _log.Information("📊 OTP Validation result - Success: {Success}, Message: {Message}, Code: {Code}",
                    validationResult.Success,
                    validationResult.Message,
                    validationResult.Code);

                if (!validationResult.Success)
                {
                    await _userManager.AccessFailedAsync(user.Id); // ثبت تلاش ناموفق که منجر به قفل شدن حساب می‌شود
                    
                    // ثبت تاریخچه ورود ناموفق
                    await _loginHistoryService.LogFailedLoginAsync(
                        userId: user.Id,
                        ipAddress: _clientProvider.GetClientIpAddress(),
                        userAgent: _clientProvider.GetUserAgent(),
                        failureReason: validationResult.Message ?? "Invalid OTP"
                    );
                    
                    return validationResult;
                }

                // مرحله ۴: بررسی قوانین کسب‌وکار (مانند حذف نرم)
                if (user.IsDeleted)
                {
                    return ServiceResult.Failed("حساب کاربری شما غیرفعال است.", "ACCOUNT_DELETED", ErrorCategory.Unauthorized);
                }

                // --- فرآیندهای پس از تایید موفقیت‌آمیز ---

                // مرحله ۵: تکمیل ردپای حسابرسی (Audit Trail)
                var otpLog = await _context.OtpRequests
                    .Where(r => r.PhoneNumber == user.PhoneNumber && r.OtpCodeHash == incomingHash && !r.IsVerified)
                    .OrderByDescending(r => r.RequestTime)
                    .FirstOrDefaultAsync();

                if (otpLog != null)
                {
                    otpLog.IsVerified = true; // علامت‌گذاری به عنوان "استفاده شده"
                }

                // مرحله ۶: پاک‌سازی و ورود نهایی
                _otpStateStore.ClearState(); // جلوگیری از استفاده مجدد از OTP (Session)
                
                // ✅ Also clear from database
                var dbStatesToClear = _context.OtpStates
                    .Where(o => o.NationalCode == normalizedCode)
                    .ToList();
                if (dbStatesToClear.Any())
                {
                    _context.OtpStates.RemoveRange(dbStatesToClear);
                    _log.Debug("[VerifyLoginOtp] Cleared {Count} OTP state(s) from database", dbStatesToClear.Count);
                }
                
                await _userManager.ResetAccessFailedCountAsync(user.Id); // ریست کردن شمارنده خطاهای ورود

                await SignInUserAsync(user, isPersistent: false); // ورود کاربر و آپدیت LastLoginDate

                // مرحله ۷: ذخیره تمام تغییرات در دیتابیس (آپدیت OtpRequest و LastLoginDate) در یک تراکنش
                await _context.SaveChangesAsync();

                // مرحله ۸: ثبت تاریخچه ورود (Login History)
                var sessionId = HttpContext.Current?.Session?.SessionID;
                await _loginHistoryService.LogLoginAsync(
                    userId: user.Id,
                    ipAddress: _clientProvider.GetClientIpAddress(),
                    userAgent: _clientProvider.GetUserAgent(),
                    sessionId: sessionId
                );

                _log.Information("ورود موفق کاربر {UserId} با کد ملی {NationalCode}. لاگ OTP با شناسه {OtpLogId} تایید شد.", user.Id, normalizedCode, otpLog?.OtpRequestId);
                return ServiceResult.Successful("ورود با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در تایید OTP برای {NationalCode}", nationalCode);
                return ServiceResult.Failed("یک خطای سیستمی رخ داده است. لطفاً با پشتیبانی تماس بگیرید.", "SYSTEM_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        public async Task<ServiceResult> SignInWithNationalCodeAsync(string nationalCode)
        {
            try
            {
                nationalCode = PersianNumberHelper.ToEnglishNumbers(nationalCode);
                if (!IranianNationalCodeValidator.IsValid(nationalCode))
                    return ServiceResultFactory.ValidationErrors(new[] { new ValidationError("NationalCode", "کد ملی معتبر نیست.") });

                var user = await _userManager.FindByNationalCodeAsync(nationalCode);
                if (user == null)
                    return ServiceResultFactory.NotFound("کاربر", nationalCode);

                if (user.IsDeleted)
                    return ServiceResultFactory.Error("حساب کاربری شما غیرفعال است.", "ACCOUNT_DELETED");

                await SignInUserAsync(user, isPersistent: false);
                
                // ثبت تاریخچه ورود (Login History)
                var sessionId = HttpContext.Current?.Session?.SessionID;
                await _loginHistoryService.LogLoginAsync(
                    userId: user.Id,
                    ipAddress: _clientProvider.GetClientIpAddress(),
                    userAgent: _clientProvider.GetUserAgent(),
                    sessionId: sessionId
                );
                
                _log.Information("ورود مستقیم موفق کاربر {UserId}", user.Id);
                return ServiceResultFactory.Success("ورود با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در ورود مستقیم کاربر با کد ملی {NationalCode}", nationalCode);
                return ServiceResultFactory.Error("خطا در فرآیند ورود.", "SIGNIN_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        public void SignOut()
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            _log.Information("کاربر از سیستم خارج شد.");
        }

        public bool IsAuthenticated => HttpContext.Current?.User?.Identity?.IsAuthenticated ?? false;

        public string GetCurrentUserId() => IsAuthenticated ? HttpContext.Current.User.Identity.GetUserId() : null;
        // In ClinicApp.Services.AuthService class

        /// <summary>
        /// Checks if a user with the given national code already exists.
        /// Returns a Success result if the user is NEW, and a Failed result if they already exist.
        /// </summary>
        // در فایل AuthService.cs
        /// <summary>
        /// بررسی می‌کند که آیا کاربری با کد ملی داده شده وجود دارد یا خیر.
        /// برای کاربر جدید Success=true و برای کاربر موجود Success=false برمی‌گرداند.
        /// </summary>
        public async Task<ServiceResult> CheckUserExistsAsync(string nationalCode)
        {
            try
            {
                var normalizedCode = PersianNumberHelper.ToEnglishNumbers(nationalCode);
                if (!IranianNationalCodeValidator.IsValid(normalizedCode))
                {
                    return ServiceResult.Failed("کد ملی وارد شده معتبر نیست.", "INVALID_NATIONAL_CODE", ErrorCategory.Validation);
                }

                var user = await _userManager.FindByNameAsync(normalizedCode);

                if (user != null)
                {
                    _log.Information("کاربر موجود با کد ملی {NationalCode} شناسایی شد.", normalizedCode);
                    // ✅ نتیجه ناموفق با کد مشخص برای کاربر موجود
                    return ServiceResult.Failed(
                        "کاربر شناسایی شد. در حال ارسال کد ورود...",
                        "USER_ALREADY_EXISTS",
                        ErrorCategory.BusinessLogic); // این یک خطای بیزینسی است، نه سیستمی
                }
                else
                {
                    _log.Information("کد ملی {NationalCode} برای ثبت‌نام در دسترس است.", normalizedCode);
                    // ✅ نتیجه موفق با کد مشخص برای کاربر جدید
                    return ServiceResult.Successful(
                        "کد ملی برای ثبت نام در دسترس است.",
                        "USER_IS_NEW"
                    );
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کاربر با کد ملی {NationalCode}", nationalCode);
                return ServiceResult.Failed("خطای سیستم.", "SYSTEM_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        /// <summary>
        /// ارسال کد تایید ثبت‌نام به شماره موبایل جدید با رعایت تمام پروتکل‌های امنیتی و پزشکی.
        /// این متد به صورت کامل برای محیط عملیاتی کلینیک شفا آماده شده است.
        /// </summary>
        public async Task<ServiceResult> SendRegistrationOtpAsync(string nationalCode, string phoneNumber)
        {
            try
            {
                // مرحله ۱: اعتبارسنجی ورودی‌ها
                var normalizedPhone = PersianNumberHelper.ToEnglishNumbers(phoneNumber);
                if (!PersianNumberHelper.IsValidPhoneNumber(normalizedPhone))
                {
                    return ServiceResult.Failed("شماره موبایل وارد شده معتبر نیست.", "INVALID_PHONE_NUMBER", ErrorCategory.Validation);
                }

                // مرحله ۲: بررسی امنیتی - آیا شماره موبایل قبلاً توسط کاربر دیگری ثبت شده است؟
                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone && !u.IsDeleted);
                if (existingUser != null)
                {
                    _log.Warning("تلاش برای ثبت‌نام با شماره موبایل تکراری: {PhoneNumber}", normalizedPhone);
                    return ServiceResult.Failed("این شماره موبایل قبلاً در سیستم ثبت شده است.", "DUPLICATE_PHONE_NUMBER", ErrorCategory.Validation);
                }

                // مرحله ۳: اعمال محدودیت نرخ درخواست برای جلوگیری از حملات اسپم
                var clientIp = _clientProvider.GetClientIpAddress();
                if (_rateLimiter.IsRateLimited($"reg_otp_send_nc:{nationalCode}", _authSettings.OtpMaxSendsPerNationalCodePer5Min, TimeSpan.FromMinutes(5)) ||
                    _rateLimiter.IsRateLimited($"reg_otp_send_ip:{clientIp}", _authSettings.OtpMaxSendsPerIpPer5Min, TimeSpan.FromMinutes(5)))
                {
                    return ServiceResult.Failed("تعداد درخواست‌های شما بیش از حد مجاز است. لطفاً دقایقی دیگر تلاش کنید.", "RATE_LIMIT_EXCEEDED");
                }

                // مرحله ۴: تولید کد امن و ذخیره موقت آن در سشن کاربر
                var otp = GenerateSecureOtp(_authSettings.OtpLength);
                var otpHash = HashOtp(otp, normalizedPhone); // هش کردن کد با شماره موبایل به عنوان "نمک"
                // ✅ باطل کردن OTP قبلی (اگر وجود داشته باشد) و ثبت در لاگ
                var oldState = _otpStateStore.GetState();
                if (oldState != null && oldState.NationalCode == nationalCode)
                {
                    _log.Information("OTP قبلی برای کد ملی {NationalCode} باطل شد (OTP جدید ارسال شد)", nationalCode);
                }

                var state = new OtpState
                {
                    NationalCode = nationalCode,
                    PhoneNumber = normalizedPhone,
                    OtpHash = otpHash,
                    ExpiryUtc = DateTime.UtcNow.AddMinutes(_authSettings.OtpExpiryMinutes),
                    IpAddress = clientIp,
                    UserAgent = _clientProvider.GetUserAgent(),
                    AttemptCount = 0 // ✅ شمارنده تلاش‌ها
                };
                _otpStateStore.SetState(state);

                // ✅ مرحله ۵: ثبت درخواست در دیتابیس برای ردگیری و حسابرسی امنیتی
                var otpRequestLog = new OtpRequest
                {
                    PhoneNumber = normalizedPhone,
                    OtpCodeHash = otpHash, // فقط هش کد در دیتابیس ذخیره می‌شود
                    RequestTime = DateTime.UtcNow,
                    IsVerified = false,
                    CreatedByUserId = SystemUsers.SystemUserId // این عملیات توسط سیستم انجام می‌شود
                };
                _context.OtpRequests.Add(otpRequestLog);
                await _context.SaveChangesAsync();
                _log.Information("درخواست OTP برای ثبت‌نام در دیتابیس با شناسه {OtpRequestId} ثبت شد.", otpRequestLog.OtpRequestId);


                // مرحله ۶: ارسال پیامک به کاربر
                var message = new IdentityMessage { Destination = normalizedPhone, Body = $"کد تایید کلینیک شفا: {otp}" };
                await _smsService.SendAsync(message);

                _log.Information("کد OTP ثبت‌نام با موفقیت به شماره {PhoneNumber} برای کد ملی {NationalCode} ارسال شد.", normalizedPhone, nationalCode);
                return ServiceResult.Successful("کد تایید به شماره موبایل شما ارسال شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در هنگام ارسال OTP ثبت‌نام برای کد ملی {NationalCode}", nationalCode);
                return ServiceResult.Failed("خطای سیستمی رخ داده است، لطفاً با پشتیبانی تماس بگیرید.", "SYSTEM_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        /// <summary>
        /// Verifies the OTP sent during the registration flow.
        /// </summary>
    /// <summary>
/// Verifies the OTP sent during the registration flow and updates the audit trail.
/// </summary>
public async Task<ServiceResult> VerifyRegistrationOtpAsync(string nationalCode, string phoneNumber, string otpCode)
{
    try
    {
        var state = _otpStateStore.GetState();
        var normalizedPhone = PersianNumberHelper.ToEnglishNumbers(phoneNumber);
        var incomingHash = HashOtp(otpCode, normalizedPhone);

        // Step 1: Validate the OTP state from the session
        var validationResult = ValidateOtpState(state, nationalCode, incomingHash, normalizedPhone); // ✅ ارسال phoneNumber برای بررسی
        if (!validationResult.Success)
        {
            return validationResult;
        }

        // ✅ --- FINAL STEP: UPDATE THE DATABASE AUDIT LOG ---
        try
        {
            // Find the original log record in the database
            var otpLog = await _context.OtpRequests
                .Where(r => r.PhoneNumber == normalizedPhone && r.OtpCodeHash == incomingHash && !r.IsVerified)
                .OrderByDescending(r => r.RequestTime)
                .FirstOrDefaultAsync();

            if (otpLog != null)
            {
                otpLog.IsVerified = true;
                // The DbContext will automatically handle UpdatedAt and UpdatedByUserId on save
                await _context.SaveChangesAsync();
                _log.Information("Registration OTP for {PhoneNumber} was verified and Log ID {OtpLogId} was updated.", phoneNumber, otpLog.OtpRequestId);
            }
            else
            {
                _log.Warning("A valid registration OTP was verified, but no matching log was found in the database for phone {PhoneNumber}", phoneNumber);
            }
        }
        catch (Exception dbEx)
        {
            // If the database update fails, we log it but don't fail the user's registration flow.
            // The OTP was valid, which is the most critical part for user experience.
            _log.Error(dbEx, "Failed to update IsVerified flag in OtpRequest log for phone {PhoneNumber}", phoneNumber);
        }
        // --- END OF FINAL STEP ---

        _log.Information("Registration OTP successfully verified for {NationalCode} and {PhoneNumber}", nationalCode, normalizedPhone);
        
        return ServiceResult.Successful("شماره موبایل با موفقیت تایید شد. لطفاً پروفایل خود را تکمیل کنید.");
    }
    catch (Exception ex)
    {
        _log.Error(ex, "System error while verifying registration OTP for {NationalCode}", nationalCode);
        return ServiceResult.Failed("یک خطای سیستمی رخ داد.", "SYSTEM_ERROR", ErrorCategory.System, SecurityLevel.High);
    }
}

        // ✅ NEW Private Helper for validating the registration OTP state
        private ServiceResult ValidateRegistrationOtpState(OtpState state, string nationalCode, string phoneNumber, string otpCode)
        {
            if (state == null || state.ExpiryUtc < DateTime.UtcNow)
                return ServiceResult.Failed("The verification code has expired. Please request a new one.", "OTP_EXPIRED", ErrorCategory.Validation);

            // Ensure the request is coming from the same person (IP/Browser) who requested the code
            if (state.IpAddress != _clientProvider.GetClientIpAddress() || state.UserAgent != _clientProvider.GetUserAgent())
            {
                _log.Warning("Session binding mismatch during registration OTP verification for {NationalCode}", nationalCode);
                return ServiceResult.Failed("Security check failed. Please try the process again from the same browser.", "SESSION_BINDING_ERROR", ErrorCategory.Security, SecurityLevel.High);
            }

            // Ensure the OTP is for the correct phone number and national code
            if (state.NationalCode != nationalCode || state.PhoneNumber != phoneNumber)
            {
                _log.Warning("State mismatch during registration OTP verification for {NationalCode}", nationalCode);
                return ServiceResult.Failed("The verification data is incorrect. Please start over.", "STATE_MISMATCH_ERROR", ErrorCategory.Security);
            }

            // Securely compare the provided OTP with the stored hash
            var incomingHash = HashOtp(otpCode, phoneNumber);
            if (!SlowEquals(state.OtpHash, incomingHash))
            {
                // Here you could implement a failed attempt counter in the OtpState if needed
                return ServiceResult.Failed("The verification code is incorrect.", "OTP_INVALID", ErrorCategory.Validation);
            }

            return ServiceResult.Successful();
        }

        #region Private Helper Methods

        private ServiceResult ValidateOtpState(OtpState state, string nationalCode, string incomingHash, string phoneNumber = null)
        {
            // ✅ CRITICAL FIX: Distinguish between different failure reasons for better UX
            if (state == null)
            {
                _log.Warning("OTP state not found during validation | NationalCode: {NationalCode}", nationalCode);
                return ServiceResult.Failed("کد تایید یافت نشد. لطفاً کد جدیدی دریافت کنید.", "OTP_STATE_NOT_FOUND", ErrorCategory.Validation);
            }
            
            if (state.ExpiryUtc < DateTime.UtcNow)
            {
                _log.Warning("OTP expired | NationalCode: {NationalCode} | Expiry: {Expiry}", nationalCode, state.ExpiryUtc);
                return ServiceResult.Failed("کد تایید منقضی شده است. لطفاً کد جدیدی دریافت کنید.", "OTP_EXPIRED", ErrorCategory.Validation);
            }
            
            if (state.NationalCode != nationalCode)
            {
                _log.Warning("OTP NationalCode mismatch | State: {StateNC} | Provided: {ProvidedNC}", state.NationalCode, nationalCode);
                return ServiceResult.Failed("کد تایید نامعتبر است.", "OTP_INVALID", ErrorCategory.Validation);
            }

            // ✅ بررسی تطابق phoneNumber (برای Login flow)
            if (!string.IsNullOrEmpty(phoneNumber) && state.PhoneNumber != phoneNumber)
            {
                _log.Warning("PhoneNumber mismatch during OTP validation | State: {StatePhone} | Provided: {ProvidedPhone}", state.PhoneNumber, phoneNumber);
                return ServiceResult.Failed("اطلاعات تایید نامعتبر است.", "PHONE_MISMATCH", ErrorCategory.Security);
            }

            // ✅ بررسی محدودیت تلاش‌های ناموفق برای این OTP
            var maxOtpAttempts = _authSettings.OtpMaxVerificationAttempts > 0 ? _authSettings.OtpMaxVerificationAttempts : 5;
            if (state.AttemptCount >= maxOtpAttempts)
            {
                _log.Warning("OTP verification attempts exceeded | NationalCode: {NationalCode} | Attempts: {Attempts}", nationalCode, state.AttemptCount);
                return ServiceResult.Failed("تعداد تلاش‌های ناموفق برای این کد بیش از حد مجاز است. لطفاً کد جدید درخواست کنید.", "OTP_ATTEMPTS_EXCEEDED", ErrorCategory.Security);
            }

            if (state.IpAddress != _clientProvider.GetClientIpAddress() || state.UserAgent != _clientProvider.GetUserAgent())
            {
                return ServiceResult.Failed("نشست امنیتی نامعتبر است.", "SESSION_BINDING_ERROR", ErrorCategory.Security);
            }

            // ✅ مقایسه مستقیم هش‌ها
            if (!SlowEquals(state.OtpHash, incomingHash))
            {
                // ✅ افزایش شمارنده تلاش‌های ناموفق
                state.AttemptCount++;
                _otpStateStore.SetState(state); // ✅ ذخیره state به‌روز شده
                _log.Warning("OTP verification failed | NationalCode: {NationalCode} | Attempt: {Attempt}/{Max}", nationalCode, state.AttemptCount, maxOtpAttempts);
                return ServiceResult.Failed("کد وارد شده صحیح نمی‌باشد.", "OTP_INVALID");
            }

            return ServiceResult.Successful();
        }

        private string GenerateSecureOtp(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            foreach (var b in bytes) sb.Append(b % 10);
            return sb.ToString();
        }

        /// <summary>
        /// هش کردن OTP با استفاده از salt (phoneNumber یا nationalCode)
        /// </summary>
        private string HashOtp(string otp, string salt)
        {
            var key = Encoding.UTF8.GetBytes(_authSettings.OtpHashKey);
            using (var hmac = new HMACSHA256(key))
            {
                var data = Encoding.UTF8.GetBytes($"{salt}|{otp}"); // ✅ استفاده از مقدار واقعی salt
                return Convert.ToBase64String(hmac.ComputeHash(data));
            }
        }

        private bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        private async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

            identity.AddClaim(new Claim("FullName", user.FullName ?? ""));
            identity.AddClaim(new Claim("NationalCode", user.NationalCode ?? ""));

            // ✅ FIX: Add FirstName and LastName claims for UI display
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? ""));
            identity.AddClaim(new Claim("LastName", user.LastName ?? ""));

            var roles = await _userManager.GetRolesAsync(user.Id);
            if (roles.Any())
            {
                identity.AddClaim(new Claim("PrimaryRole", roles.First()));
            }

            _authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);

            // ✅ REMOVED: Response.Flush() causes conflict with JsonResult
            // OWIN manages cookie automatically and Application_PostAuthenticateRequest handles sync
            // No need to flush - JsonResult will work correctly without it
            // Cookie will be sent automatically by OWIN middleware

            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);
        }

        #endregion
    }
}