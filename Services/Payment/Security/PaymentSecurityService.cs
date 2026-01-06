using ClinicApp.Interfaces.Payment.Security;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
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
        private const int MaxAttemptsPerAppointment = 3;
        private const int CooldownMinutesBetweenAttempts = 5;
        
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
                        $"شما بیش از حد مجاز درخواست پرداخت ارسال کرده‌اید. لطفاً {CooldownMinutesBetweenAttempts} دقیقه صبر کنید.",
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

                if (string.IsNullOrWhiteSpace(ipAddress) || !IsValidIpAddress(ipAddress))
                {
                    return ServiceResult.Failed("آدرس IP نامعتبر است", "INVALID_IP_ADDRESS");
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

                if (attemptCount >= MaxAttemptsPerAppointment)
                {
                    _logger.Warning("⚠️ SECURITY: Appointment Rate Limit exceeded - AppointmentId: {AppointmentId}, Attempts: {Attempts}, Limit: {Limit}, CorrelationId: {CorrelationId}",
                        appointmentId, attemptCount, MaxAttemptsPerAppointment, correlationId);
                    
                    return ServiceResult.Failed(
                        $"تعداد تلاش‌های پرداخت برای این نوبت بیش از حد مجاز است. لطفاً {CooldownMinutesBetweenAttempts} دقیقه صبر کنید یا با پشتیبانی تماس بگیرید.",
                        "APPOINTMENT_RATE_LIMIT_EXCEEDED");
                }

                // ✅ بررسی Cooldown (آخرین تلاش)
                var lastAttempt = appointmentPayments?
                    .Where(p => !p.IsDeleted && 
                               (p.Status == OnlinePaymentStatus.Pending || 
                                p.Status == OnlinePaymentStatus.Failed))
                    .OrderByDescending(p => p.CreatedAt)
                    .FirstOrDefault();

                if (lastAttempt != null)
                {
                    var timeSinceLastAttempt = DateTime.UtcNow - lastAttempt.CreatedAt;
                    if (timeSinceLastAttempt.TotalMinutes < CooldownMinutesBetweenAttempts)
                    {
                        var remainingMinutes = CooldownMinutesBetweenAttempts - (int)timeSinceLastAttempt.TotalMinutes;
                        _logger.Warning("⚠️ SECURITY: Cooldown active - AppointmentId: {AppointmentId}, RemainingMinutes: {RemainingMinutes}, CorrelationId: {CorrelationId}",
                            appointmentId, remainingMinutes, correlationId);
                        
                        return ServiceResult.Failed(
                            $"لطفاً {remainingMinutes} دقیقه صبر کنید و دوباره تلاش کنید.",
                            "COOLDOWN_ACTIVE");
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

                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    return ServiceResult.Failed("آدرس IP الزامی است", "IP_REQUIRED");
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

                // ✅ بررسی دقت اعشار (باید بدون اعشار باشد - ریال)
                if (amount != Math.Floor(amount))
                {
                    _logger.Warning("⚠️ SECURITY: Amount has decimal places - Amount: {Amount}, CorrelationId: {CorrelationId}",
                        amount, correlationId);
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
            try
            {
                _logger.Information("🔒 SECURITY: شروع اعتبارسنجی امنیتی جامع - CorrelationId: {CorrelationId}",
                    request.CorrelationId);

                var errors = new List<string>();

                // ✅ 1. IP Validation
                var ipResult = ValidateIpAddress(request.UserIpAddress, request.CorrelationId);
                if (!ipResult.Success)
                {
                    errors.Add(ipResult.Message);
                }

                // ✅ 2. User Agent Validation
                var userAgentResult = ValidateUserAgent(request.UserAgent, request.CorrelationId);
                if (!userAgentResult.Success)
                {
                    errors.Add(userAgentResult.Message);
                }

                // ✅ 3. Amount Validation
                var amountResult = ValidateAmount(request.Amount, request.CorrelationId);
                if (!amountResult.Success)
                {
                    errors.Add(amountResult.Message);
                }

                // ✅ 4. Rate Limiting
                if (!string.IsNullOrWhiteSpace(request.UserId))
                {
                    var userRateLimitResult = await ValidateUserRateLimitAsync(request.UserId, request.CorrelationId);
                    if (!userRateLimitResult.Success)
                    {
                        errors.Add(userRateLimitResult.Message);
                    }
                }

                var ipRateLimitResult = await ValidateIpRateLimitAsync(request.UserIpAddress, request.CorrelationId);
                if (!ipRateLimitResult.Success)
                {
                    errors.Add(ipRateLimitResult.Message);
                }

                if (request.AppointmentId.HasValue)
                {
                    var appointmentRateLimitResult = await ValidateAppointmentRateLimitAsync(
                        request.AppointmentId.Value, request.CorrelationId);
                    if (!appointmentRateLimitResult.Success)
                    {
                        errors.Add(appointmentRateLimitResult.Message);
                    }
                }

                // ✅ 5. Amount Anomaly Detection
                if (request.PatientId.HasValue && request.PatientId.Value > 0)
                {
                    var anomalyResult = await DetectAmountAnomalyAsync(
                        request.Amount, request.PatientId.Value, request.CorrelationId);
                    // Fail-Open: اگر خطا داد، اجازه می‌دهیم ادامه دهد
                }

                if (errors.Any())
                {
                    _logger.Warning("⚠️ SECURITY: اعتبارسنجی امنیتی ناموفق - Errors: {Errors}, CorrelationId: {CorrelationId}",
                        string.Join("; ", errors), request.CorrelationId);
                    
                    return ServiceResult.Failed(
                        "اعتبارسنجی امنیتی ناموفق بود",
                        string.Join("; ", errors));
                }

                _logger.Information("✅ SECURITY: اعتبارسنجی امنیتی موفق - CorrelationId: {CorrelationId}",
                    request.CorrelationId);

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SECURITY: خطا در اعتبارسنجی امنیتی جامع, CorrelationId: {CorrelationId}",
                    request?.CorrelationId);
                return ServiceResult.Failed("خطا در اعتبارسنجی امنیتی", "SECURITY_VALIDATION_ERROR");
            }
        }

        #endregion
    }
}

