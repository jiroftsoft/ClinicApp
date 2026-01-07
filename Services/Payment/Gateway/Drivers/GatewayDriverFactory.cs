using ClinicApp.Interfaces.Payment.Gateway.Drivers;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;

namespace ClinicApp.Services.Payment.Gateway.Drivers
{
    /// <summary>
    /// Factory برای ایجاد Gateway Drivers
    /// طراحی شده طبق اصول Factory Pattern
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. انتخاب Driver بر اساس GatewayType
    /// 2. پشتیبانی از چندین Gateway (ZarinPal, PayPing, etc.)
    /// 3. قابلیت توسعه برای Gateway های جدید
    /// 4. ✅ BEST PRACTICE: استفاده از PaymentGateway Entity برای تنظیمات
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class GatewayDriverFactory : IGatewayDriverFactory
    {
        #region Fields

        private readonly Dictionary<PaymentGatewayType, Func<IGatewayDriver>> _legacyDrivers; // ⚠️ DEPRECATED
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor - ثبت تمام Drivers (Legacy)
        /// ⚠️ DEPRECATED: برای سازگاری با کد قدیمی نگه داشته شده است
        /// </summary>
        public GatewayDriverFactory(
            IGatewayDriver zarinPalDriver, // ✅ ZarinPal Driver (Legacy)
            ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // ✅ ثبت Legacy Drivers در Dictionary (برای سازگاری)
            _legacyDrivers = new Dictionary<PaymentGatewayType, Func<IGatewayDriver>>
            {
                { PaymentGatewayType.ZarinPal, () => zarinPalDriver ?? throw new ArgumentNullException(nameof(zarinPalDriver)) }
                // TODO: در آینده می‌توان Drivers دیگر را اضافه کرد:
                // { PaymentGatewayType.PayPing, () => payPingDriver },
                // { PaymentGatewayType.IDPay, () => idPayDriver },
            };

            _logger.Information("✅ GatewayDriverFactory initialized - Supported Gateways: {Gateways}", 
                string.Join(", ", _legacyDrivers.Keys));
        }

        #endregion

        #region IGatewayDriverFactory Implementation

        /// <summary>
        /// ⚠️ DEPRECATED: دریافت Driver مناسب بر اساس نوع درگاه (Legacy)
        /// استفاده از GetDriver(PaymentGateway) توصیه می‌شود
        /// </summary>
        [Obsolete("Use GetDriver(PaymentGateway) instead. This method uses legacy Web.config configuration.")]
        public IGatewayDriver GetDriver(PaymentGatewayType gatewayType)
        {
            if (!_legacyDrivers.ContainsKey(gatewayType))
            {
                _logger.Error("❌ GatewayDriverFactory: GatewayType {GatewayType} پشتیبانی نمی‌شود. Supported: {Supported}",
                    gatewayType, string.Join(", ", _legacyDrivers.Keys));
                throw new NotSupportedException($"Gateway type {gatewayType} is not supported. Supported types: {string.Join(", ", _legacyDrivers.Keys)}");
            }

            try
            {
                var driver = _legacyDrivers[gatewayType]();
                _logger.Warning("⚠️ GatewayDriverFactory: استفاده از Legacy Driver برای {GatewayType} (DEPRECATED)", gatewayType);
                return driver;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ GatewayDriverFactory: خطا در ایجاد Driver برای {GatewayType}", gatewayType);
                throw;
            }
        }

        /// <summary>
        /// ✅ BEST PRACTICE: دریافت Driver مناسب بر اساس PaymentGateway Entity
        /// این متد Driver را با تنظیمات از Entity ایجاد می‌کند
        /// </summary>
        /// <param name="gateway">PaymentGateway Entity شامل تمام تنظیمات</param>
        /// <returns>Driver مناسب با تنظیمات از Entity</returns>
        public IGatewayDriver GetDriver(PaymentGateway gateway)
        {
            if (gateway == null)
                throw new ArgumentNullException(nameof(gateway));

            if (!IsSupported(gateway.GatewayType))
            {
                _logger.Error("❌ GatewayDriverFactory: GatewayType {GatewayType} پشتیبانی نمی‌شود. GatewayId: {GatewayId}",
                    gateway.GatewayType, gateway.PaymentGatewayId);
                throw new NotSupportedException($"Gateway type {gateway.GatewayType} is not supported. GatewayId: {gateway.PaymentGatewayId}");
            }

            try
            {
                IGatewayDriver driver;

                switch (gateway.GatewayType)
                {
                    case PaymentGatewayType.ZarinPal:
                        // ✅ ایجاد ZarinPalDriver با PaymentGateway Entity
                        driver = new ZarinPalDriver(gateway, _logger);
                        _logger.Information("✅ GatewayDriverFactory: ZarinPalDriver ایجاد شد از Entity - GatewayId: {GatewayId}, MerchantId: {MerchantId}, IsTestMode: {IsTestMode}",
                            gateway.PaymentGatewayId,
                            gateway.MerchantId?.Substring(0, Math.Min(8, gateway.MerchantId?.Length ?? 0)) + "...",
                            gateway.IsTestMode);
                        break;

                    case PaymentGatewayType.Simulated:
                        // ✅ ایجاد SimulatedGatewayDriver برای تست و توسعه
                        driver = new SimulatedGatewayDriver(gateway, _logger);
                        _logger.Information("✅ GatewayDriverFactory: SimulatedGatewayDriver ایجاد شد از Entity - GatewayId: {GatewayId}",
                            gateway.PaymentGatewayId);
                        break;

                    // TODO: در آینده می‌توان Drivers دیگر را اضافه کرد:
                    // case PaymentGatewayType.PayPing:
                    //     driver = new PayPingDriver(gateway, _logger);
                    //     break;
                    // case PaymentGatewayType.IDPay:
                    //     driver = new IDPayDriver(gateway, _logger);
                    //     break;

                    default:
                        _logger.Error("❌ GatewayDriverFactory: GatewayType {GatewayType} پشتیبانی نمی‌شود", gateway.GatewayType);
                        throw new NotSupportedException($"Gateway type {gateway.GatewayType} is not supported");
                }

                return driver;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ GatewayDriverFactory: خطا در ایجاد Driver برای GatewayId {GatewayId}, GatewayType {GatewayType}",
                    gateway.PaymentGatewayId, gateway.GatewayType);
                throw;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا GatewayType پشتیبانی می‌شود یا نه
        /// </summary>
        public bool IsSupported(PaymentGatewayType gatewayType)
        {
            var isSupported = gatewayType == PaymentGatewayType.ZarinPal || gatewayType == PaymentGatewayType.Simulated;
            _logger.Debug("🔍 GatewayDriverFactory: IsSupported({GatewayType}) = {IsSupported}", gatewayType, isSupported);
            return isSupported;
        }

        #endregion
    }
}

