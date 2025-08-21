using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicApp
{
    #region Email Service (بهینه‌سازی شده برای محیط عملیاتی)

    /// <summary>
    /// سرویس ایمیل حرفه‌ای برای سیستم کلینیک شفا
    /// این سرویس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده است
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        private readonly ILogger _log = Log.ForContext<EmailService>();
        private readonly int _maxRetries;
        private readonly int _retryBaseDelayMs;
        private readonly int _timeoutMs;

        public EmailService()
        {
            // پیکربندی از طریق Web.config
            _maxRetries = GetInt("Email:MaxRetries", 3);
            _retryBaseDelayMs = GetInt("Email:RetryBaseDelayMs", 400);
            _timeoutMs = GetInt("Email:TimeoutMs", 15000);
        }

        public async Task SendAsync(IdentityMessage message)
        {
            // در سیستم پسورد‌لس، ممکن است نیازی به ارسال ایمیل نباشد
            var enabled = GetBool("Email:Enabled", false);
            if (!enabled)
            {
                _log.Information("Email sending is DISABLED via config. Destination: {Destination}", message?.Destination);
                return;
            }

            if (message == null)
            {
                _log.Warning("IdentityMessage is null. Email not sent.");
                return;
            }

            // اعتبارسنجی پایه
            var fromAddress = ConfigurationManager.AppSettings["Email:FromAddress"];
            if (string.IsNullOrEmpty(fromAddress))
            {
                _log.Error("Email:FromAddress is not configured in appSettings");
                return;
            }

            var smtpServer = ConfigurationManager.AppSettings["Email:SmtpServer"];
            var portStr = ConfigurationManager.AppSettings["Email:Port"];
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(portStr))
            {
                _log.Error("Email configuration is incomplete in appSettings");
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Destination) || string.IsNullOrWhiteSpace(message.Body))
            {
                _log.Warning("Destination or Body is empty for email");
                return;
            }

            // ارسال با مکانیزم Retry
            var rnd = new Random();
            Exception lastEx = null;

            for (int attempt = 1; attempt <= Math.Max(1, _maxRetries); attempt++)
            {
                using var cts = new CancellationTokenSource(_timeoutMs);
                try
                {
                    await SendInternalAsync(fromAddress, message, cts.Token);
                    _log.Information("Email sent successfully to {Destination}. Attempt: {Attempt}",
                        message.Destination, attempt);
                    return;
                }
                catch (OperationCanceledException ocex)
                {
                    lastEx = ocex;
                    _log.Error(ocex, "Email timeout/canceled. Attempt: {Attempt}, To: {Destination}",
                        attempt, message.Destination);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _log.Error(ex, "Email unexpected error. Attempt: {Attempt}, To: {Destination}",
                        attempt, message.Destination);
                }

                // اگر تلاش بعدی داریم، Backoff
                if (attempt < _maxRetries)
                {
                    int delay = ComputeBackoffDelay(attempt, _retryBaseDelayMs, rnd);
                    await Task.Delay(delay, CancellationToken.None);
                }
            }

            // اگر بعد از همه تلاش‌ها ناموفق بود
            _log.Fatal(lastEx, "Email permanently failed after {Retries} attempts. To: {Destination}",
                _maxRetries, message.Destination);
        }

        private async Task SendInternalAsync(string fromAddress, IdentityMessage message, CancellationToken ct)
        {
            var smtpServer = ConfigurationManager.AppSettings["Email:SmtpServer"];
            var portStr = ConfigurationManager.AppSettings["Email:Port"];
            var username = ConfigurationManager.AppSettings["Email:Username"];
            var password = ConfigurationManager.AppSettings["Email:Password"];
            var enableSsl = GetBool("Email:EnableSsl", true);

            var port = int.TryParse(portStr, out int p) ? p : 587;

            using var smtpClient = new SmtpClient(smtpServer, port)
            {
                Credentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)
                    ? new NetworkCredential(username, password)
                    : null,
                EnableSsl = enableSsl
            };

            using var mailMessage = new MailMessage(fromAddress, message.Destination, message.Subject, message.Body)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        #region Helper Methods

        private int ComputeBackoffDelay(int attempt, int baseDelayMs, Random rnd)
        {
            // backoff نمایی + jitter برای جلوگیری از هم‌زمانی درخواست‌ها
            double exp = Math.Pow(2, attempt - 1);
            int jitter = rnd.Next(0, baseDelayMs);
            // سقف محافظه‌کارانه
            int delay = (int)Math.Min(15000, exp * baseDelayMs + jitter);
            return delay;
        }

        private static int GetInt(string key, int defaultValue)
        {
            var val = ConfigurationManager.AppSettings[key];
            return int.TryParse(val, out var n) ? n : defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            var val = ConfigurationManager.AppSettings[key];
            return bool.TryParse(val, out var b) ? b : defaultValue;
        }

        #endregion
    }

    #endregion

    #region Identity Validators (اعتبارسنجی‌های حرفه‌ای)

    /// <summary>
    /// اعتبارسنجی کد ملی ایرانی و شماره تلفن همراه برای سیستم‌های پزشکی
    /// این اعتبارسنجی‌ها با توجه به استانداردهای سیستم‌های پزشکی ایرانی طراحی شده‌اند
    /// </summary>
    public class NationalCodeUserValidator : UserValidator<ApplicationUser>
    {
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _log = Log.ForContext<NationalCodeUserValidator>();

        public NationalCodeUserValidator(ApplicationUserManager manager) : base(manager)
        {
            _userManager = manager;
        }

        public override async Task<IdentityResult> ValidateAsync(ApplicationUser user)
        {
            var result = await base.ValidateAsync(user);
            var errors = new List<string>();

            // اعتبارسنجی کد ملی
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                errors.Add("کد ملی نمی‌تواند خالی باشد.");
            }
            else if (user.UserName.Length != 10 || !user.UserName.All(char.IsDigit))
            {
                errors.Add("کد ملی باید 10 رقمی باشد.");
            }
            else if (!IsValidIranianNationalCode(user.UserName))
            {
                errors.Add("کد ملی وارد شده معتبر نیست.");
            }
            else if (await _userManager.FindByNameAsync(user.UserName) != null)
            {
                errors.Add("کد ملی تکراری است. لطفاً کد ملی متفاوتی وارد کنید.");
            }

            // اعتبارسنجی شماره تلفن همراه
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                errors.Add("شماره تلفن همراه الزامی است.");
            }
            else
            {
                var normalizedNumber = NormalizePhoneNumber(user.PhoneNumber);
                if (!_userManager.IsValidIranianPhoneNumber(normalizedNumber))
                {
                    errors.Add("شماره تلفن همراه وارد شده معتبر نیست.");
                }
                else
                {
                    var existingUser = await _userManager.FindByPhoneNumberAsync(normalizedNumber);
                    if (existingUser != null && (user.Id == null || existingUser.Id != user.Id))
                    {
                        errors.Add("شماره تلفن همراه تکراری است. لطفاً شماره تلفن متفاوتی وارد کنید.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                _log.Warning("User validation failed for {UserName}: {Errors}",
                    user.UserName, string.Join(", ", errors));
                return IdentityResult.Failed(errors.ToArray());
            }

            return result;
        }

        /// <summary>
        /// بررسی اعتبار کد ملی ایرانی
        /// </summary>
        private bool IsValidIranianNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode))
                return false;

            // بررسی طول کد ملی
            if (nationalCode.Length != 10)
                return false;

            // بررسی اینکه تمام کاراکترها رقم هستند
            if (!nationalCode.All(char.IsDigit))
                return false;

            // بررسی خاص برای کدهای ملی که تمام ارقام یکسان دارند
            if (nationalCode.Distinct().Count() == 1)
                return false;

            // الگوریتم اعتبارسنجی کد ملی ایران
            int[] coefficients = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(nationalCode[i].ToString()) * coefficients[i];
            }

            int remainder = sum % 11;
            int controlDigit = int.Parse(nationalCode[9].ToString());

            if (remainder < 2)
            {
                return controlDigit == remainder;
            }
            else
            {
                return controlDigit == 11 - remainder;
            }
        }

        /// <summary>
        /// نرمال‌سازی شماره تلفن همراه
        /// </summary>
        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // تبدیل ارقام فارسی/عربی به انگلیسی
            var converted = new string(phoneNumber.Select(c =>
            {
                if (c >= '\u06F0' && c <= '\u06F9') // ارقام فارسی
                    return (char)(c - '\u06F0' + '0');
                if (c >= '\u0660' && c <= '\u0669') // ارقام عربی
                    return (char)(c - '\u0660' + '0');
                return c;
            }).ToArray());

            // حذف کاراکترهای غیرضروری
            var cleaned = new string(converted.Where(c => char.IsDigit(c) || c == '+').ToArray());

            // تبدیل به فرمت بین‌المللی
            if (cleaned.StartsWith("0"))
            {
                cleaned = "+98" + cleaned.Substring(1);
            }
            else if (cleaned.StartsWith("98"))
            {
                cleaned = "+" + cleaned;
            }
            else if (cleaned.StartsWith("9") && cleaned.Length == 10)
            {
                cleaned = "+98" + cleaned;
            }

            return cleaned;
        }
    }

    /// <summary>
    /// اعتبارسنجی رمز عبور برای سیستم پسورد‌لس
    /// در این سیستم، نیازی به رمز عبور نیست
    /// </summary>
    public class PasswordlessPasswordValidator : IIdentityValidator<string>
    {
        public Task<IdentityResult> ValidateAsync(string item)
        {
            return Task.FromResult(IdentityResult.Success);
        }
    }

    #endregion

    #region ApplicationUserManager (نسخه نهایی عملیاتی)

    /// <summary>
    /// مدیریت کاربران سیستم کلینیک شفا
    /// این کلاس به طور کامل سیستم پسورد‌لس را پشتیبانی می‌کند و از استانداردهای سیستم‌های پزشکی ایرانی پیروی می‌کند
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private readonly ILogger _log = Log.ForContext<ApplicationUserManager>();

        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store)
        {
            SmsService = new AsanakSmsService();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            #region User Validation (اعتبارسنجی نام کاربری)
            // استفاده از اعتبارسنجی کد ملی ایرانی
            manager.UserValidator = new NationalCodeUserValidator(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            #endregion

            #region Password Validation (غیرفعال کردن الزامات پسورد)
            // در سیستم پسورد‌لس، نیازی به رمز عبور نیست
            manager.PasswordValidator = new PasswordlessPasswordValidator();
            #endregion

            #region Account Lockout (قفل کردن حساب کاربری)
            // تنظیمات امنیتی برای سیستم‌های پزشکی
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            #endregion

            #region Two-Factor Authentication (احراز هویت دو مرحله‌ای)
            var phoneTokenProvider = new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "کد امنیتی کلینیک شفا: {0}"
            };

            // تنظیم صحیح TokenLifespan با Reflection در MVC5
            var tokenLifespanProperty = typeof(PhoneNumberTokenProvider<ApplicationUser>)
                .GetProperty("TokenLifespan", BindingFlags.NonPublic | BindingFlags.Instance);

            if (tokenLifespanProperty != null)
            {
                tokenLifespanProperty.SetValue(phoneTokenProvider, TimeSpan.FromMinutes(5));
                manager._log.Information("TokenLifespan set to 5 minutes for PhoneNumberTokenProvider");
            }
            else
            {
                manager._log.Warning("TokenLifespan property not found in PhoneNumberTokenProvider");
            }

            manager.RegisterTwoFactorProvider("Phone Code", phoneTokenProvider);
            #endregion

            #region Message Services (سرویس‌های ارسال پیام)
            manager.SmsService = new AsanakSmsService(); // سرویس پیامکی ما
            #endregion

            #region Token Provider (تولید توکن امن)
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                    dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromHours(3)
                };
                manager._log.Information("DataProtectorTokenProvider configured with 3 hours lifespan");
            }
            #endregion

            return manager;
        }

        #region Custom Methods (روش‌های سفارشی)

        /// <summary>
        /// یافتن کاربر بر اساس شماره تلفن همراه
        /// </summary>
        public async Task<ApplicationUser> FindByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                var normalizedNumber = NormalizePhoneNumber(phoneNumber);
                if (string.IsNullOrWhiteSpace(normalizedNumber))
                    return null;

                return await Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedNumber);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error finding user by phone number: {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        /// <summary>
        /// نرمال‌سازی شماره تلفن همراه
        /// </summary>
        public string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            var converted = new string(phoneNumber.Select(c =>
            {
                if (c >= '\u06F0' && c <= '\u06F9') // ارقام فارسی
                    return (char)(c - '\u06F0' + '0');
                if (c >= '\u0660' && c <= '\u0669') // ارقام عربی
                    return (char)(c - '\u0660' + '0');
                return c;
            }).ToArray());

            var cleaned = new string(converted.Where(c => char.IsDigit(c) || c == '+').ToArray());

            if (cleaned.StartsWith("0"))
                cleaned = "+98" + cleaned.Substring(1);
            else if (cleaned.StartsWith("98"))
                cleaned = "+" + cleaned;
            else if (cleaned.StartsWith("9") && cleaned.Length == 10)
                cleaned = "+98" + cleaned;

            return cleaned;
        }

        /// <summary>
        /// بررسی معتبر بودن شماره تلفن ایرانی
        /// </summary>
        public bool IsValidIranianPhoneNumber(string phoneNumber)
        {
            var normalizedNumber = NormalizePhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedNumber))
                return false;

            if (!normalizedNumber.StartsWith("+989") || normalizedNumber.Length != 13)
                return false;

            var operatorCode = normalizedNumber.Substring(4, 2);
            var validOperators = new[] { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" };

            return validOperators.Contains(operatorCode);
        }

        /// <summary>
        /// ایجاد کاربر جدید با پشتیبانی از سیستم پسورد‌لس
        /// </summary>
        public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                user.PhoneNumber = NormalizePhoneNumber(user.PhoneNumber);

                if (!IsValidIranianPhoneNumber(user.PhoneNumber))
                {
                    _log.Warning("Invalid phone number during user creation: {PhoneNumber}", user.PhoneNumber);
                    return IdentityResult.Failed("شماره تلفن همراه وارد شده معتبر نیست.");
                }

                var existingUser = await FindByPhoneNumberAsync(user.PhoneNumber);
                if (existingUser != null)
                {
                    _log.Warning("Duplicate phone number during user creation: {PhoneNumber}", user.PhoneNumber);
                    return IdentityResult.Failed("شماره تلفن همراه تکراری است. لطفاً شماره تلفن متفاوتی وارد کنید.");
                }
            }
            else
            {
                _log.Warning("Phone number is required but not provided during user creation");
                return IdentityResult.Failed("شماره تلفن همراه الزامی است.");
            }

            return await base.CreateAsync(user);
        }

        /// <summary>
        /// به‌روزرسانی کاربر با پشتیبانی از سیستم پسورد‌لس
        /// </summary>
        public override async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                user.PhoneNumber = NormalizePhoneNumber(user.PhoneNumber);

                if (!IsValidIranianPhoneNumber(user.PhoneNumber))
                {
                    _log.Warning("Invalid phone number during user update: {PhoneNumber}", user.PhoneNumber);
                    return IdentityResult.Failed("شماره تلفن همراه وارد شده معتبر نیست.");
                }

                var existingUser = await FindByPhoneNumberAsync(user.PhoneNumber);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _log.Warning("Duplicate phone number during user update: {PhoneNumber}", user.PhoneNumber);
                    return IdentityResult.Failed("شماره تلفن همراه تکراری است. لطفاً شماره تلفن متفاوتی وارد کنید.");
                }
            }
            else
            {
                _log.Warning("Phone number is required but not provided during user update");
                return IdentityResult.Failed("شماره تلفن همراه الزامی است.");
            }

            return await base.UpdateAsync(user);
        }

        /// <summary>
        /// تولید کد OTP برای تغییر شماره تلفن
        /// </summary>
        public async Task<string> GenerateChangePhoneNumberTokenAsync(string userId, string phoneNumber)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                _log.Error("User not found for generating phone number token: {UserId}", userId);
                throw new InvalidOperationException("کاربر مورد نظر یافت نشد.");
            }

            var normalizedNumber = NormalizePhoneNumber(phoneNumber);
            if (string.IsNullOrWhiteSpace(normalizedNumber))
            {
                _log.Warning("Phone number cannot be empty for generating token: {UserId}", userId);
                throw new ValidationException("شماره تلفن همراه نمی‌تواند خالی باشد.");
            }

            if (!IsValidIranianPhoneNumber(normalizedNumber))
            {
                _log.Warning("Invalid phone number for generating token: {PhoneNumber}", phoneNumber);
                throw new ValidationException("شماره تلفن همراه وارد شده معتبر نیست.");
            }

            var existingUser = await FindByPhoneNumberAsync(normalizedNumber);
            if (existingUser != null && existingUser.Id != userId)
            {
                _log.Warning("Duplicate phone number for generating token: {PhoneNumber}", phoneNumber);
                throw new ValidationException("شماره تلفن همراه تکراری است. لطفاً شماره تلفن متفاوتی وارد کنید.");
            }

            return await base.GenerateChangePhoneNumberTokenAsync(userId, normalizedNumber);
        }

        #endregion
    }

    #endregion

    #region ApplicationSignInManager (نسخه نهایی عملیاتی)

    /// <summary>
    /// مدیریت ورود کاربران سیستم کلینیک شفا
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        private readonly ILogger _log = Log.ForContext<ApplicationSignInManager>();

        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authManager)
            : base(userManager, authManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        #region Passwordless Sign-In Methods

        public async Task<SignInStatus> SignInWithOtpAsync(string identifier, string otpCode)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _log.Warning("Identifier is null or empty for OTP sign-in");
                return SignInStatus.Failure;
            }

            ApplicationUser user = null;
            if (IsNationalCode(identifier))
                user = await UserManager.FindByNameAsync(identifier);
            else if (IsPhoneNumber(identifier))
            {
                var normalizedNumber = ((ApplicationUserManager)UserManager).NormalizePhoneNumber(identifier);
                user = await ((ApplicationUserManager)UserManager).FindByPhoneNumberAsync(normalizedNumber);
            }

            if (user == null || !user.IsActive)
            {
                _log.Warning("User not found or inactive for OTP sign-in: {Identifier}", identifier);
                return SignInStatus.Failure;
            }

            var isValid = await UserManager.UserTokenProvider.ValidateAsync("Phone Code", otpCode, UserManager, user);
            if (!isValid)
            {
                _log.Warning("Invalid OTP for user: {UserId}", user.Id);
                return SignInStatus.Failure;
            }

            await SignInAsync(user, isPersistent: false, rememberBrowser: false);
            _log.Information("User signed in successfully with OTP: {UserId}", user.Id);
            return SignInStatus.Success;
        }

        public async Task<bool> SendOtpCodeAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _log.Warning("Identifier is null or empty for OTP send");
                return false;
            }

            ApplicationUser user = null;
            if (IsNationalCode(identifier))
                user = await UserManager.FindByNameAsync(identifier);
            else if (IsPhoneNumber(identifier))
            {
                var normalizedNumber = ((ApplicationUserManager)UserManager).NormalizePhoneNumber(identifier);
                user = await ((ApplicationUserManager)UserManager).FindByPhoneNumberAsync(normalizedNumber);
            }

            if (user == null || !user.IsActive)
            {
                _log.Warning("User not found or inactive for OTP send: {Identifier}", identifier);
                return false;
            }

            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(user.Id, user.PhoneNumber);
            var message = new IdentityMessage
            {
                Destination = user.PhoneNumber,
                Subject = "کد امنیتی کلینیک شفا",
                Body = $"کد امنیتی شما: {code}"
            };

            await UserManager.SmsService.SendAsync(message);
            _log.Information("OTP sent successfully to user: {UserId}", user.Id);
            return true;
        }

        #region Helper Methods

        private bool IsNationalCode(string identifier)
        {
            return identifier.Length == 10 && identifier.All(char.IsDigit);
        }

        private bool IsPhoneNumber(string identifier)
        {
            var normalized = ((ApplicationUserManager)UserManager).NormalizePhoneNumber(identifier);
            return normalized != null && normalized.StartsWith("+989") && normalized.Length == 13;
        }

        #endregion

        #endregion
    }

    #endregion



   
    
}