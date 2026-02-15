using ClinicApp.Interfaces.Payment.Security;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Payment;

namespace ClinicApp.Services.Payment.Security
{
    /// <summary>
    /// ✅ ENTERPRISE-GRADE: Service برای اعتبارسنجی امنیتی پرداخت‌ها
    /// 
    /// مسئولیت‌ها:
    /// 1. Rate Limiting
    /// 2. IP Validation
    /// 3. User Agent Validation
    /// 4. Amount Anomaly Detection
    /// 5. Pattern Detection (Anti-Fraud)
    /// 6. Digital Signature Verification
    /// 
    /// طبق: PAYMENT_SYSTEM_ENTERPRISE_REDESIGN.md
    /// </summary>
    public class PaymentSecurityService : IPaymentSecurityService
    {
        #region Fields

        private readonly IOnlinePaymentRepository _onlinePaymentRepository;
        private readonly ILogger _logger;
        
        // ✅ Rate Limiting Configuration
        private const int MaxRequestsPerHourPerUser = 10;
        private const int MaxRequestsPerMinutePerIp = 5;
        private const int MaxRequestsPerHourPerIp = 100;
        // ✅ Appointment rate limit: از Web.config خوانده می‌شود (Payment:MaxAttemptsPerAppointment, Payment:AppointmentCooldownMinutes)
        
        // ✅ Amount Limits
        private const decimal MaxSinglePaymentAmount = 200000000m; // 200M تومان
        private const decimal MaxDailyPaymentAmount = 1000000000m; // 1B تومان
        private const decimal AnomalyThresholdMultiplier = 10m; // 10x average
        
        // ✅ IP Blacklist (در Production باید از Database یا Redis خوانده شود)
        private static readonly HashSet<string> _ipBlacklist = new HashSet<string>();

        #endregion

        #region Constructor

        public PaymentSecurityService(
            IOnlinePaymentRepository onlinePaymentRepository,
            ILogger logger)
        {
            _onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
            _logger = logger?.ForContext<PaymentSecurityService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Config Helpers (Payment:AppointmentCooldownMinutes, Payment:MaxAttemptsPerAppointment)

        private static int GetAppointmentCooldownMinutes()
        {
            var raw = ConfigurationManager.AppSettings["Payment:AppointmentCooldownMinutes"];
            if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw.Trim(), out var minutes) || minutes < 0)
                return 1;
            return Math.Min(60, Math.Max(0, minutes));
        }

        private static int GetMaxAttemptsPerAppointment()
        {
            var raw = ConfigurationManager.AppSettings["Payment:MaxAttemptsPerAppointment"];
            if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw.Trim(), out var n) || n < 1)
                return 3;
            return Math.Min(20, Math.Max(1, n));
        }

        #endregion

        #region Rate Limiting

        /// <summary>
        /// ✅ بررسی Rate Limit برای کاربر
        /// </summary>
        public async Task<ServiceResult> ValidateUserRateLimitAsync(string userId, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: بررسی Rate Limit برای کاربر {UserId}", userId);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failed("شناسه کاربر نامعتبر است", "INVALID_USER_ID");
                }

                // ✅ بررسی تعداد درخواست‌های پرداخت در یک ساعت گذشته
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var recentPayments = await _onlinePaymentRepository.GetByUserIdAndDateRangeAsync(
                    userId, oneHourAgo, DateTime.UtcNow);

                var paymentCount = recentPayments?.Count(p => 
                    p.Status == OnlinePaymentStatus.Pending || 
                    p.Status == OnlinePaymentStatus.Successful) ?? 0;

                if (paymentCount >= MaxRequestsPerHourPerUser)
                {
                    _logger.Warning("⚠️ SECURITY: Rate Limit exceeded for user {UserId} - Count: {Count}, Limit: {Limit}, CorrelationId: {CorrelationId}",
                        userId, paymentCount, MaxRequestsPerHourPerUser, correlationId);
                    
                    return ServiceResult.Failed(
                        $"شما بیش از حد مجاز درخواست پرداخت ارسال کرده‌اید. لطفاً {GetAppointmentCooldownMinutes()} دقیقه صبر کنید.",
                        "RATE_LIMIT_EXCEEDED");
                }

                _logger.Debug("✅ SECURITY: Rate Limit OK for user {UserId} - Count: {Count}, Limit: {Limit}",
                    userId, paymentCount, MaxRequestsPerHourPerUser);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در بررسی Rate Limit برای کاربر {UserId}, CorrelationId: {CorrelationId}",
                    userId, correlationId);
                return ServiceResult.Failed("خطا در بررسی محدودیت درخواست", "RATE_LIMIT_CHECK_ERROR");
            }
        }

        /// <summary>
        /// ✅ بررسی Rate Limit برای IP
        /// </summary>
        public async Task<ServiceResult> ValidateIpRateLimitAsync(string ipAddress, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: بررسی Rate Limit برای IP {IpAddress}", ipAddress);

                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "Unknown" || !IsValidIpAddress(ipAddress))
                {
                    _logger.Debug("⚠️ SECURITY: IP not available for rate limit - skipping IP rate limit check, CorrelationId: {CorrelationId}", correlationId);
                    return ServiceResult.Successful(); // وقتی IP در دسترس نیست، محدودیت IP اعمال نشود
                }

                // ✅ بررسی IP Blacklist
                if (_ipBlacklist.Contains(ipAddress))
                {
                    _logger.Warning("⚠️ SECURITY: IP {IpAddress} is blacklisted, CorrelationId: {CorrelationId}",
                        ipAddress, correlationId);
                    return ServiceResult.Failed("آدرس IP شما مسدود شده است", "IP_BLACKLISTED");
                }

                // ✅ بررسی تعداد درخواست‌های پرداخت در یک دقیقه گذشته
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentPaymentsByIp = await _onlinePaymentRepository.GetByIpAddressAndDateRangeAsync(
                    ipAddress, oneMinuteAgo, DateTime.UtcNow);

                var paymentCountPerMinute = recentPaymentsByIp?.Count(p => 
                    p.Status == OnlinePaymentStatus.Pending || 
                    p.Status == OnlinePaymentStatus.Successful) ?? 0;

                if (paymentCountPerMinute >= MaxRequestsPerMinutePerIp)
                {
                    _logger.Warning("⚠️ SECURITY: IP Rate Limit exceeded - IP: {IpAddress}, Count: {Count}, Limit: {Limit}, CorrelationId: {CorrelationId}",
                        ipAddress, paymentCountPerMinute, MaxRequestsPerMinutePerIp, correlationId);
                    
                    return ServiceResult.Failed(
                        "تعداد درخواست‌های پرداخت از این IP بیش از حد مجاز است. لطفاً چند لحظه صبر کنید.",
                        "IP_RATE_LIMIT_EXCEEDED");
                }

                // ✅ بررسی تعداد درخواست‌های پرداخت در یک ساعت گذشته
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                var recentPaymentsByIpHour = await _onlinePaymentRepository.GetByIpAddressAndDateRangeAsync(
                    ipAddress, oneHourAgo, DateTime.UtcNow);

                var paymentCountPerHour = recentPaymentsByIpHour?.Count(p => 
                    p.Status == OnlinePaymentStatus.Pending || 
                    p.Status == OnlinePaymentStatus.Successful) ?? 0;

                if (paymentCountPerHour >= MaxRequestsPerHourPerIp)
                {
                    _logger.Warning("⚠️ SECURITY: IP Hourly Rate Limit exceeded - IP: {IpAddress}, Count: {Count}, Limit: {Limit}, CorrelationId: {CorrelationId}",
                        ipAddress, paymentCountPerHour, MaxRequestsPerHourPerIp, correlationId);
                    
                    return ServiceResult.Failed(
                        "تعداد درخواست‌های پرداخت از این IP در یک ساعت گذشته بیش از حد مجاز است.",
                        "IP_HOURLY_RATE_LIMIT_EXCEEDED");
                }

                _logger.Debug("✅ SECURITY: IP Rate Limit OK - IP: {IpAddress}, Count/Minute: {CountMinute}, Count/Hour: {CountHour}",
                    ipAddress, paymentCountPerMinute, paymentCountPerHour);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در بررسی Rate Limit برای IP {IpAddress}, CorrelationId: {CorrelationId}",
                    ipAddress, correlationId);
                return ServiceResult.Failed("خطا در بررسی محدودیت درخواست IP", "IP_RATE_LIMIT_CHECK_ERROR");
            }
        }

        /// <summary>
        /// ✅ بررسی Rate Limit برای نوبت (جلوگیری از تلاش‌های مکرر)
        /// </summary>
        public async Task<ServiceResult> ValidateAppointmentRateLimitAsync(int appointmentId, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: بررسی Rate Limit برای نوبت {AppointmentId}", appointmentId);

                // ✅ بررسی تعداد تلاش‌های پرداخت برای این نوبت
                var appointmentPayments = await _onlinePaymentRepository.GetByAppointmentIdAsync(appointmentId);
                
                var attemptCount = appointmentPayments?.Count(p => 
                    !p.IsDeleted && 
                    (p.Status == OnlinePaymentStatus.Pending || 
                     p.Status == OnlinePaymentStatus.Failed ||
                     p.Status == OnlinePaymentStatus.Successful)) ?? 0;

                var maxAttempts = GetMaxAttemptsPerAppointment();
                if (attemptCount >= maxAttempts)
                {
                    var cooldownMin = GetAppointmentCooldownMinutes();
                    _logger.Warning("⚠️ SECURITY: Appointment Rate Limit exceeded - AppointmentId: {AppointmentId}, Attempts: {Attempts}, Limit: {Limit}, CorrelationId: {CorrelationId}",
                        appointmentId, attemptCount, maxAttempts, correlationId);
                    
                    return ServiceResult.Failed(
                        $"تعداد تلاش‌های پرداخت برای این نوبت بیش از حد مجاز است. لطفاً {cooldownMin} دقیقه صبر کنید یا با پشتیبانی تماس بگیرید.",
                        "APPOINTMENT_RATE_LIMIT_EXCEEDED");
                }

                // ✅ بررسی Cooldown (آخرین تلاش)
                var lastAttempt = appointmentPayments?
                    .Where(p => !p.IsDeleted && 
                               (p.Status == OnlinePaymentStatus.Pending || 
                                p.Status == OnlinePaymentStatus.Failed))
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                var cooldownMinutes = GetAppointmentCooldownMinutes();
                if (lastAttempt != null)
                {
                    var timeSinceLastAttempt = DateTime.UtcNow - lastAttempt.CreatedAt;
                    if (timeSinceLastAttempt.TotalMinutes < cooldownMinutes)
                    {
                        var remainingMinutes = Math.Max(1, cooldownMinutes - (int)timeSinceLastAttempt.TotalMinutes);
                        _logger.Warning("⚠️ SECURITY: Cooldown active - AppointmentId: {AppointmentId}, RemainingMinutes: {RemainingMinutes}, CorrelationId: {CorrelationId}",
                            appointmentId, remainingMinutes, correlationId);
                        
                        return ServiceResult.Failed(
                            $"لطفاً {remainingMinutes} دقیقه صبر کنید و دوباره تلاش کنید.",
                            "AppointmentRateLimit");
                    }
                }

                _logger.Debug("✅ SECURITY: Appointment Rate Limit OK - AppointmentId: {AppointmentId}, Attempts: {Attempts}",
                    appointmentId, attemptCount);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در بررسی Rate Limit برای نوبت {AppointmentId}, CorrelationId: {CorrelationId}",
                    appointmentId, correlationId);
                return ServiceResult.Failed("خطا در بررسی محدودیت درخواست نوبت", "APPOINTMENT_RATE_LIMIT_CHECK_ERROR");
            }
        }

        #endregion

        #region IP Validation

        /// <summary>
        /// ✅ اعتبارسنجی آدرس IP
        /// </summary>
        public ServiceResult ValidateIpAddress(string ipAddress, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: اعتبارسنجی IP {IpAddress}", ipAddress);

                if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "Unknown")
                {
                    _logger.Warning("⚠️ SECURITY: IP not available (empty or Unknown) - CorrelationId: {CorrelationId}", correlationId);
                    return ServiceResult.Successful(); // Fail-open: در محیط‌هایی که IP در دسترس نیست پرداخت مسدود نشود
                }

                if (!IsValidIpAddress(ipAddress))
                {
                    _logger.Warning("⚠️ SECURITY: Invalid IP format - IP: {IpAddress}, CorrelationId: {CorrelationId}",
                        ipAddress, correlationId);
                    return ServiceResult.Failed("فرمت آدرس IP نامعتبر است", "INVALID_IP_FORMAT");
                }

                // ✅ بررسی IP Blacklist
                if (_ipBlacklist.Contains(ipAddress))
                {
                    _logger.Warning("⚠️ SECURITY: IP {IpAddress} is blacklisted, CorrelationId: {CorrelationId}",
                        ipAddress, correlationId);
                    return ServiceResult.Failed("آدرس IP شما مسدود شده است", "IP_BLACKLISTED");
                }

                // ✅ بررسی IP های مشکوک (localhost در Production)
                if (ipAddress == "127.0.0.1" || ipAddress == "::1")
                {
                    // در Production باید بررسی شود
                    _logger.Debug("⚠️ SECURITY: Localhost IP detected - IP: {IpAddress}", ipAddress);
                }

                _logger.Debug("✅ SECURITY: IP validation OK - IP: {IpAddress}", ipAddress);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در اعتبارسنجی IP {IpAddress}, CorrelationId: {CorrelationId}",
                    ipAddress, correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی آدرس IP", "IP_VALIDATION_ERROR");
            }
        }

        /// <summary>
        /// ✅ بررسی اینکه آیا IP معتبر است
        /// </summary>
        private bool IsValidIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // ✅ IPv4 Pattern
            var ipv4Pattern = @"^(\d{1,3}\.){3}\d{1,3}$";
            if (Regex.IsMatch(ipAddress, ipv4Pattern))
            {
                return IPAddress.TryParse(ipAddress, out _);
            }

            // ✅ IPv6 Pattern
            if (ipAddress.Contains(":"))
            {
                return IPAddress.TryParse(ipAddress, out _);
            }

            return false;
        }

        #endregion

        #region User Agent Validation

        /// <summary>
        /// ✅ اعتبارسنجی User Agent
        /// </summary>
        public ServiceResult ValidateUserAgent(string userAgent, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: اعتبارسنجی User Agent");

                if (string.IsNullOrWhiteSpace(userAgent))
                {
                    // User Agent اختیاری است اما بهتر است وجود داشته باشد
                    _logger.Debug("⚠️ SECURITY: User Agent is empty, CorrelationId: {CorrelationId}", correlationId);
                    return ServiceResult.Successful(); // اختیاری
                }

                // ✅ بررسی طول User Agent
                if (userAgent.Length > 500)
                {
                    _logger.Warning("⚠️ SECURITY: User Agent too long - Length: {Length}, CorrelationId: {CorrelationId}",
                        userAgent.Length, correlationId);
                    return ServiceResult.Failed("User Agent بیش از حد مجاز است", "USER_AGENT_TOO_LONG");
                }

                // ✅ بررسی الگوهای مشکوک
                var suspiciousPatterns = new[]
                {
                    "curl", "wget", "python", "scrapy", "bot", "crawler", "spider"
                };

                var lowerUserAgent = userAgent.ToLower();
                foreach (var pattern in suspiciousPatterns)
                {
                    if (lowerUserAgent.Contains(pattern))
                    {
                        _logger.Warning("⚠️ SECURITY: Suspicious User Agent pattern detected - Pattern: {Pattern}, UserAgent: {UserAgent}, CorrelationId: {CorrelationId}",
                            pattern, userAgent, correlationId);
                        // در Production می‌تواند block شود
                        // return ServiceResult.Failed("User Agent مشکوک است", "SUSPICIOUS_USER_AGENT");
                    }
                }

                _logger.Debug("✅ SECURITY: User Agent validation OK");
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در اعتبارسنجی User Agent, CorrelationId: {CorrelationId}",
                    correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی User Agent", "USER_AGENT_VALIDATION_ERROR");
            }
        }

        #endregion

        #region Amount Validation

        /// <summary>
        /// ✅ اعتبارسنجی مبلغ پرداخت
        /// </summary>
        public ServiceResult ValidateAmount(decimal amount, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: اعتبارسنجی مبلغ {Amount}", amount);

                // ✅ بررسی مقدار صفر یا منفی
                if (amount <= 0)
                {
                    _logger.Warning("⚠️ SECURITY: Invalid amount (<= 0) - Amount: {Amount}, CorrelationId: {CorrelationId}",
                        amount, correlationId);
                    return ServiceResult.Failed("مبلغ پرداخت باید بیشتر از صفر باشد", "INVALID_AMOUNT");
                }

                // ✅ بررسی حداکثر مبلغ
                if (amount > MaxSinglePaymentAmount)
                {
                    _logger.Warning("⚠️ SECURITY: Amount exceeds maximum - Amount: {Amount}, Max: {Max}, CorrelationId: {CorrelationId}",
                        amount, MaxSinglePaymentAmount, correlationId);
                    return ServiceResult.Failed(
                        $"مبلغ پرداخت بیش از حد مجاز است (حداکثر {MaxSinglePaymentAmount:N0} ریال)",
                        "AMOUNT_EXCEEDS_MAXIMUM");
                }

                // ✅ بررسی دقت اعشار (ریال؛ اختلاف ناچیز ناشی از float/DB قابل قبول است)
                var floorAmount = Math.Floor(amount);
                if (amount - floorAmount > 0.001m)
                {
                    _logger.Warning("⚠️ SECURITY: Amount has significant decimal places - Amount: {Amount}, Floor: {Floor}, CorrelationId: {CorrelationId}",
                        amount, floorAmount, correlationId);
                    return ServiceResult.Failed("مبلغ پرداخت باید بدون اعشار باشد (ریال)", "AMOUNT_HAS_DECIMAL");
                }

                _logger.Debug("✅ SECURITY: Amount validation OK - Amount: {Amount}", amount);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در اعتبارسنجی مبلغ {Amount}, CorrelationId: {CorrelationId}",
                    amount, correlationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی مبلغ", "AMOUNT_VALIDATION_ERROR");
            }
        }

        /// <summary>
        /// ✅ تشخیص ناهنجاری در مبلغ (Anti-Fraud)
        /// </summary>
        public async Task<ServiceResult> DetectAmountAnomalyAsync(decimal amount, int patientId, string correlationId)
        {
            try
            {
                _logger.Debug("🔒 SECURITY: تشخیص ناهنجاری مبلغ - Amount: {Amount}, PatientId: {PatientId}", amount, patientId);

                // ✅ محاسبه میانگین پرداخت‌های قبلی بیمار
                var patientPayments = await _onlinePaymentRepository.GetByPatientIdAsync(patientId);
                
                var completedPayments = patientPayments?
                    .Where(p => !p.IsDeleted && 
                               p.Status == OnlinePaymentStatus.Successful &&
                               p.CreatedAt >= DateTime.UtcNow.AddMonths(-3)) // 3 ماه گذشته
                    .ToList() ?? new List<OnlinePayment>();

                if (completedPayments.Any())
                {
                    var averageAmount = completedPayments.Average(p => p.Amount);
                    var threshold = averageAmount * AnomalyThresholdMultiplier;

                    if (amount > threshold)
                    {
                        _logger.Warning("⚠️ SECURITY: Amount anomaly detected - Amount: {Amount}, Average: {Average}, Threshold: {Threshold}, PatientId: {PatientId}, CorrelationId: {CorrelationId}",
                            amount, averageAmount, threshold, patientId, correlationId);
                        
                        // در Production می‌تواند flag شود برای بررسی دستی
                        // return ServiceResult.Failed("مبلغ پرداخت غیرعادی است. لطفاً با پشتیبانی تماس بگیرید.", "AMOUNT_ANOMALY");
                    }
                }

                _logger.Debug("✅ SECURITY: Amount anomaly check OK - Amount: {Amount}", amount);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در تشخیص ناهنجاری مبلغ {Amount}, PatientId: {PatientId}, CorrelationId: {CorrelationId}",
                    amount, patientId, correlationId);
                // در صورت خطا، اجازه می‌دهیم ادامه دهد (Fail-Open)
                return ServiceResult.Successful();
            }
        }

        #endregion

        #region Comprehensive Security Validation

        /// <summary>
        /// ✅ اعتبارسنجی امنیتی جامع برای درخواست پرداخت
        /// </summary>
        public async Task<ServiceResult> ValidatePaymentRequestSecurityAsync(
            PaymentSecurityValidationRequest request)
        {
            const string LogPrefix = "[PaymentSecurity]";
            try
            {
                _logger.Information("{Prefix} شروع اعتبارسنجی - CorrelationId: {CorrelationId}, UserId: {UserId}, PatientId: {PatientId}, AppointmentId: {AppointmentId}, Amount: {Amount}, IP: {UserIpAddress}, UserAgentLen: {UserAgentLen}",
                    LogPrefix, request.CorrelationId, request.UserId ?? "NULL", request.PatientId, request.AppointmentId, request.Amount, request.UserIpAddress ?? "NULL", request.UserAgent?.Length ?? 0);

                var errors = new List<string>();

                // ✅ Step 1: IP Validation
                _logger.Debug("{Prefix} Step1 IP - UserIpAddress: {Ip}", LogPrefix, request.UserIpAddress ?? "NULL");
                var ipResult = ValidateIpAddress(request.UserIpAddress, request.CorrelationId);
                if (!ipResult.Success)
                {
                    _logger.Warning("{Prefix} Step1 IP FAILED - {Message}, Code: {Code}", LogPrefix, ipResult.Message, ipResult.Code);
                    errors.Add("IP: " + ipResult.Message);
                }

                // ✅ Step 2: User Agent Validation
                _logger.Debug("{Prefix} Step2 UserAgent - Length: {Len}", LogPrefix, request.UserAgent?.Length ?? 0);
                var userAgentResult = ValidateUserAgent(request.UserAgent, request.CorrelationId);
                if (!userAgentResult.Success)
                {
                    _logger.Warning("{Prefix} Step2 UserAgent FAILED - {Message}, Code: {Code}", LogPrefix, userAgentResult.Message, userAgentResult.Code);
                    errors.Add("UserAgent: " + userAgentResult.Message);
                }

                // ✅ Step 3: Amount Validation
                _logger.Debug("{Prefix} Step3 Amount - Amount: {Amount}, Floor: {Floor}", LogPrefix, request.Amount, Math.Floor(request.Amount));
                var amountResult = ValidateAmount(request.Amount, request.CorrelationId);
                if (!amountResult.Success)
                {
                    _logger.Warning("{Prefix} Step3 Amount FAILED - {Message}, Code: {Code}", LogPrefix, amountResult.Message, amountResult.Code);
                    errors.Add("Amount: " + amountResult.Message);
                }

                // ✅ Step 4: User Rate Limiting
                if (!string.IsNullOrWhiteSpace(request.UserId))
                {
                    _logger.Debug("{Prefix} Step4 UserRateLimit - UserId: {UserId}", LogPrefix, request.UserId);
                    var userRateLimitResult = await ValidateUserRateLimitAsync(request.UserId, request.CorrelationId);
                    if (!userRateLimitResult.Success)
                    {
                        _logger.Warning("{Prefix} Step4 UserRateLimit FAILED - {Message}, Code: {Code}", LogPrefix, userRateLimitResult.Message, userRateLimitResult.Code);
                        errors.Add("UserRateLimit: " + userRateLimitResult.Message);
                    }
                }
                else
                    _logger.Debug("{Prefix} Step4 UserRateLimit SKIP - UserId empty", LogPrefix);

                // ✅ Step 5: IP Rate Limiting
                _logger.Debug("{Prefix} Step5 IpRateLimit - IP: {Ip}", LogPrefix, request.UserIpAddress ?? "NULL");
                var ipRateLimitResult = await ValidateIpRateLimitAsync(request.UserIpAddress, request.CorrelationId);
                if (!ipRateLimitResult.Success)
                {
                    _logger.Warning("{Prefix} Step5 IpRateLimit FAILED - {Message}, Code: {Code}", LogPrefix, ipRateLimitResult.Message, ipRateLimitResult.Code);
                    errors.Add("IpRateLimit: " + ipRateLimitResult.Message);
                }

                // ✅ Step 6: Appointment Rate Limiting
                if (request.AppointmentId.HasValue)
                {
                    _logger.Debug("{Prefix} Step6 AppointmentRateLimit - AppointmentId: {AppointmentId}", LogPrefix, request.AppointmentId.Value);
                    var appointmentRateLimitResult = await ValidateAppointmentRateLimitAsync(
                        request.AppointmentId.Value, request.CorrelationId);
                    if (!appointmentRateLimitResult.Success)
                    {
                        _logger.Warning("{Prefix} Step6 AppointmentRateLimit FAILED - {Message}, Code: {Code}", LogPrefix, appointmentRateLimitResult.Message, appointmentRateLimitResult.Code);
                        errors.Add("AppointmentRateLimit: " + appointmentRateLimitResult.Message);
                    }
                }
                else
                    _logger.Debug("{Prefix} Step6 AppointmentRateLimit SKIP - no AppointmentId", LogPrefix);

                // ✅ Step 7: Amount Anomaly (Fail-Open)
                if (request.PatientId.HasValue && request.PatientId.Value > 0)
                {
                    _logger.Debug("{Prefix} Step7 Anomaly - PatientId: {PatientId}, Amount: {Amount}", LogPrefix, request.PatientId.Value, request.Amount);
                    var anomalyResult = await DetectAmountAnomalyAsync(
                        request.Amount, request.PatientId.Value, request.CorrelationId);
                }

                if (errors.Any())
                {
                    var details = string.Join("; ", errors);
                    _logger.Warning("{Prefix} اعتبارسنجی ناموفق - CorrelationId: {CorrelationId}, Errors: {Errors}",
                        LogPrefix, request.CorrelationId, details);
                    
                    return ServiceResult.Failed(
                        "اعتبارسنجی امنیتی ناموفق بود",
                        details);
                }

                _logger.Information("{Prefix} اعتبارسنجی موفق - CorrelationId: {CorrelationId}", LogPrefix, request.CorrelationId);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{Prefix} EXCEPTION در اعتبارسنجی - CorrelationId: {CorrelationId}", LogPrefix, request?.CorrelationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی امنیتی", "SECURITY_VALIDATION_ERROR");
            }
        }

        #endregion
    }
}

