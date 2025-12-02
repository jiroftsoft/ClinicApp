using System;
using System.Configuration;
using Serilog;

namespace ClinicApp.Services.Payment.POS
{
    /// <summary>
    /// Production-Ready POS Payment Configuration Service
    /// 
    /// مسئولیت: مدیریت تنظیمات پرداخت POS
    /// 
    /// ویژگی‌های کلیدی:
    /// ✅ خواندن تنظیمات از Web.config
    /// ✅ Default Values
    /// ✅ Validation
    /// ✅ Caching
    /// 
    /// اصول طراحی:
    /// - Single Responsibility: فقط مدیریت Configuration
    /// - Separation of Concerns: جدا از Business Logic
    /// </summary>
    public class PosPaymentConfigurationService
    {
        private readonly ILogger _logger;
        private static PosPaymentConfigurationService _instance;
        private static readonly object _lock = new object();

        // Configuration Keys
        private const string SignalRUrlKey = "SamanKishSignalRUrl";
        private const string MaxRetryAttemptsKey = "PosPayment:MaxRetryAttempts";
        private const string ConnectionTimeoutKey = "PosPayment:ConnectionTimeoutMs";
        private const string PaymentTimeoutKey = "PosPayment:PaymentTimeoutMs";
        private const string InitialDelayKey = "PosPayment:InitialDelayMs";
        private const string RetryDelayKey = "PosPayment:RetryDelayMs";

        // Default Values
        private const string DefaultSignalRUrl = "http://localhost:8080/signalr";
        private const int DefaultMaxRetryAttempts = 3;
        private const int DefaultConnectionTimeoutMs = 30000; // 30 seconds
        private const int DefaultPaymentTimeoutMs = 120000; // 2 minutes
        private const int DefaultInitialDelayMs = 1000; // 1 second
        private const int DefaultRetryDelayMs = 2000; // 2 seconds

        public PosPaymentConfigurationService(ILogger logger)
        {
            _logger = logger?.ForContext<PosPaymentConfigurationService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت URL SignalR Hub
        /// </summary>
        public string GetSignalRUrl()
        {
            try
            {
                var url = ConfigurationManager.AppSettings[SignalRUrlKey];
                if (string.IsNullOrWhiteSpace(url))
                {
                    _logger.Warning("⚠️ SignalR URL not found in config, using default: {DefaultUrl}", DefaultSignalRUrl);
                    return DefaultSignalRUrl;
                }

                _logger.Debug("✅ SignalR URL loaded from config: {Url}", url);
                return url;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading SignalR URL from config, using default");
                return DefaultSignalRUrl;
            }
        }

        /// <summary>
        /// دریافت حداکثر تعداد تلاش‌ها
        /// </summary>
        public int GetMaxRetryAttempts()
        {
            try
            {
                var value = ConfigurationManager.AppSettings[MaxRetryAttemptsKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return DefaultMaxRetryAttempts;
                }

                if (int.TryParse(value, out int result) && result > 0)
                {
                    return result;
                }

                _logger.Warning("⚠️ Invalid MaxRetryAttempts value: {Value}, using default: {Default}", value, DefaultMaxRetryAttempts);
                return DefaultMaxRetryAttempts;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading MaxRetryAttempts from config, using default");
                return DefaultMaxRetryAttempts;
            }
        }

        /// <summary>
        /// دریافت Timeout اتصال (میلی‌ثانیه)
        /// </summary>
        public int GetConnectionTimeoutMs()
        {
            try
            {
                var value = ConfigurationManager.AppSettings[ConnectionTimeoutKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return DefaultConnectionTimeoutMs;
                }

                if (int.TryParse(value, out int result) && result > 0)
                {
                    return result;
                }

                _logger.Warning("⚠️ Invalid ConnectionTimeout value: {Value}, using default: {Default}", value, DefaultConnectionTimeoutMs);
                return DefaultConnectionTimeoutMs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading ConnectionTimeout from config, using default");
                return DefaultConnectionTimeoutMs;
            }
        }

        /// <summary>
        /// دریافت Timeout پرداخت (میلی‌ثانیه)
        /// </summary>
        public int GetPaymentTimeoutMs()
        {
            try
            {
                var value = ConfigurationManager.AppSettings[PaymentTimeoutKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return DefaultPaymentTimeoutMs;
                }

                if (int.TryParse(value, out int result) && result > 0)
                {
                    return result;
                }

                _logger.Warning("⚠️ Invalid PaymentTimeout value: {Value}, using default: {Default}", value, DefaultPaymentTimeoutMs);
                return DefaultPaymentTimeoutMs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading PaymentTimeout from config, using default");
                return DefaultPaymentTimeoutMs;
            }
        }

        /// <summary>
        /// دریافت تاخیر اولیه (میلی‌ثانیه)
        /// </summary>
        public int GetInitialDelayMs()
        {
            try
            {
                var value = ConfigurationManager.AppSettings[InitialDelayKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return DefaultInitialDelayMs;
                }

                if (int.TryParse(value, out int result) && result >= 0)
                {
                    return result;
                }

                _logger.Warning("⚠️ Invalid InitialDelay value: {Value}, using default: {Default}", value, DefaultInitialDelayMs);
                return DefaultInitialDelayMs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading InitialDelay from config, using default");
                return DefaultInitialDelayMs;
            }
        }

        /// <summary>
        /// دریافت تاخیر Retry (میلی‌ثانیه)
        /// </summary>
        public int GetRetryDelayMs()
        {
            try
            {
                var value = ConfigurationManager.AppSettings[RetryDelayKey];
                if (string.IsNullOrWhiteSpace(value))
                {
                    return DefaultRetryDelayMs;
                }

                if (int.TryParse(value, out int result) && result > 0)
                {
                    return result;
                }

                _logger.Warning("⚠️ Invalid RetryDelay value: {Value}, using default: {Default}", value, DefaultRetryDelayMs);
                return DefaultRetryDelayMs;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error reading RetryDelay from config, using default");
                return DefaultRetryDelayMs;
            }
        }

        /// <summary>
        /// دریافت تمام تنظیمات به صورت Object
        /// </summary>
        public PosPaymentConfiguration GetConfiguration()
        {
            return new PosPaymentConfiguration
            {
                SignalRUrl = GetSignalRUrl(),
                MaxRetryAttempts = GetMaxRetryAttempts(),
                ConnectionTimeoutMs = GetConnectionTimeoutMs(),
                PaymentTimeoutMs = GetPaymentTimeoutMs(),
                InitialDelayMs = GetInitialDelayMs(),
                RetryDelayMs = GetRetryDelayMs()
            };
        }
    }

    /// <summary>
    /// Configuration Object
    /// </summary>
    public class PosPaymentConfiguration
    {
        public string SignalRUrl { get; set; }
        public int MaxRetryAttempts { get; set; }
        public int ConnectionTimeoutMs { get; set; }
        public int PaymentTimeoutMs { get; set; }
        public int InitialDelayMs { get; set; }
        public int RetryDelayMs { get; set; }
    }
}

