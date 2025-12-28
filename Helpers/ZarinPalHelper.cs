using Serilog;
using System;
using System.Configuration;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای خواندن تنظیمات ZarinPal از Web.config
    /// طراحی شده طبق اصول SRP - مسئولیت: مدیریت تنظیمات ZarinPal
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public static class ZarinPalHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(ZarinPalHelper));

        #region Configuration Keys

        private const string MerchantIdKey = "ZarinpalMerchantId";
        private const string IsSandboxKey = "Zarinpal:IsSandbox";
        private const string RequestUrlKey = "Zarinpal:RequestUrl";
        private const string VerifyUrlKey = "Zarinpal:VerifyUrl";
        private const string StartPayUrlKey = "Zarinpal:StartPayUrl";
        private const string StatusUrlKey = "Zarinpal:StatusUrl";

        #endregion

        #region Default Values (Sandbox)

        private const string DefaultSandboxRequestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
        private const string DefaultSandboxVerifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";
        private const string DefaultSandboxStartPayUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
        private const string DefaultSandboxStatusUrl = "https://sandbox.zarinpal.com/pg/v4/payment/status.json";

        private const string DefaultProductionRequestUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
        private const string DefaultProductionVerifyUrl = "https://api.zarinpal.com/pg/v4/payment/verify.json";
        private const string DefaultProductionStartPayUrl = "https://www.zarinpal.com/pg/StartPay/";
        private const string DefaultProductionStatusUrl = "https://api.zarinpal.com/pg/v4/payment/status.json";

        #endregion

        #region Get Merchant ID

        /// <summary>
        /// دریافت Merchant ID از Web.config
        /// </summary>
        /// <returns>Merchant ID</returns>
        public static string GetMerchantId()
        {
            try
            {
                var merchantId = ConfigurationManager.AppSettings[MerchantIdKey];
                
                if (string.IsNullOrWhiteSpace(merchantId))
                {
                    _logger.Warning("⚠️ ZarinPal: Merchant ID در Web.config یافت نشد. Key: {Key}", MerchantIdKey);
                    throw new ConfigurationErrorsException($"ZarinPal Merchant ID در Web.config یافت نشد. Key: {MerchantIdKey}");
                }

                _logger.Debug("✅ ZarinPal: Merchant ID خوانده شد - Length: {Length}", merchantId.Length);
                return merchantId.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن Merchant ID از Web.config");
                throw;
            }
        }

        #endregion

        #region Get Is Sandbox

        /// <summary>
        /// بررسی اینکه آیا در حالت Sandbox هستیم یا Production
        /// </summary>
        /// <returns>true = Sandbox, false = Production</returns>
        public static bool IsSandbox()
        {
            try
            {
                var isSandboxValue = ConfigurationManager.AppSettings[IsSandboxKey];
                
                if (string.IsNullOrWhiteSpace(isSandboxValue))
                {
                    _logger.Warning("⚠️ ZarinPal: IsSandbox در Web.config یافت نشد. مقدار پیش‌فرض (true) استفاده می‌شود.");
                    return true; // پیش‌فرض: Sandbox
                }

                var isSandbox = isSandboxValue.Trim().ToLower() == "true";
                _logger.Debug("✅ ZarinPal: IsSandbox = {IsSandbox}", isSandbox);
                return isSandbox;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن IsSandbox از Web.config");
                return true; // پیش‌فرض: Sandbox
            }
        }

        #endregion

        #region Get URLs

        /// <summary>
        /// دریافت URL درخواست پرداخت
        /// </summary>
        /// <returns>Request URL</returns>
        public static string GetRequestUrl()
        {
            try
            {
                var url = ConfigurationManager.AppSettings[RequestUrlKey];
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    // استفاده از URL پیش‌فرض بر اساس Sandbox/Production
                    url = IsSandbox() ? DefaultSandboxRequestUrl : DefaultProductionRequestUrl;
                    _logger.Debug("✅ ZarinPal: Request URL از پیش‌فرض استفاده شد - {Url}", url);
                }
                else
                {
                    _logger.Debug("✅ ZarinPal: Request URL از Web.config خوانده شد - {Url}", url);
                }

                return url.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن Request URL");
                return IsSandbox() ? DefaultSandboxRequestUrl : DefaultProductionRequestUrl;
            }
        }

        /// <summary>
        /// دریافت URL تأیید پرداخت
        /// </summary>
        /// <returns>Verify URL</returns>
        public static string GetVerifyUrl()
        {
            try
            {
                var url = ConfigurationManager.AppSettings[VerifyUrlKey];
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = IsSandbox() ? DefaultSandboxVerifyUrl : DefaultProductionVerifyUrl;
                    _logger.Debug("✅ ZarinPal: Verify URL از پیش‌فرض استفاده شد - {Url}", url);
                }
                else
                {
                    _logger.Debug("✅ ZarinPal: Verify URL از Web.config خوانده شد - {Url}", url);
                }

                return url.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن Verify URL");
                return IsSandbox() ? DefaultSandboxVerifyUrl : DefaultProductionVerifyUrl;
            }
        }

        /// <summary>
        /// دریافت URL شروع پرداخت (Redirect)
        /// </summary>
        /// <returns>Start Pay URL</returns>
        public static string GetStartPayUrl()
        {
            try
            {
                var url = ConfigurationManager.AppSettings[StartPayUrlKey];
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = IsSandbox() ? DefaultSandboxStartPayUrl : DefaultProductionStartPayUrl;
                    _logger.Debug("✅ ZarinPal: StartPay URL از پیش‌فرض استفاده شد - {Url}", url);
                }
                else
                {
                    _logger.Debug("✅ ZarinPal: StartPay URL از Web.config خوانده شد - {Url}", url);
                }

                return url.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن StartPay URL");
                return IsSandbox() ? DefaultSandboxStartPayUrl : DefaultProductionStartPayUrl;
            }
        }

        /// <summary>
        /// دریافت URL بررسی وضعیت پرداخت
        /// </summary>
        /// <returns>Status URL</returns>
        public static string GetStatusUrl()
        {
            try
            {
                var url = ConfigurationManager.AppSettings[StatusUrlKey];
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = IsSandbox() ? DefaultSandboxStatusUrl : DefaultProductionStatusUrl;
                    _logger.Debug("✅ ZarinPal: Status URL از پیش‌فرض استفاده شد - {Url}", url);
                }
                else
                {
                    _logger.Debug("✅ ZarinPal: Status URL از Web.config خوانده شد - {Url}", url);
                }

                return url.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ZarinPal: خطا در خواندن Status URL");
                return IsSandbox() ? DefaultSandboxStatusUrl : DefaultProductionStatusUrl;
            }
        }

        #endregion
    }
}

