using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Payment.Gateway.Drivers
{
    /// <summary>
    /// Factory Interface برای ایجاد Gateway Drivers
    /// طراحی شده طبق اصول Factory Pattern
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. انتخاب Driver بر اساس GatewayType
    /// 2. پشتیبانی از چندین Gateway
    /// 3. قابلیت توسعه برای Gateway های جدید
    /// 4. ✅ BEST PRACTICE: استفاده از PaymentGateway Entity برای تنظیمات
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public interface IGatewayDriverFactory
    {
        /// <summary>
        /// دریافت Driver مناسب بر اساس نوع درگاه
        /// ⚠️ DEPRECATED: استفاده از GetDriver(PaymentGateway) توصیه می‌شود
        /// </summary>
        /// <param name="gatewayType">نوع درگاه پرداخت</param>
        /// <returns>Driver مناسب</returns>
        /// <exception cref="NotSupportedException">اگر GatewayType پشتیبانی نشود</exception>
        IGatewayDriver GetDriver(PaymentGatewayType gatewayType);

        /// <summary>
        /// ✅ BEST PRACTICE: دریافت Driver مناسب بر اساس PaymentGateway Entity
        /// این متد تنظیمات Gateway را از Entity می‌خواند (MerchantId, GatewayUrl, IsTestMode)
        /// </summary>
        /// <param name="gateway">PaymentGateway Entity شامل تمام تنظیمات</param>
        /// <returns>Driver مناسب با تنظیمات از Entity</returns>
        /// <exception cref="ArgumentNullException">اگر gateway null باشد</exception>
        /// <exception cref="NotSupportedException">اگر GatewayType پشتیبانی نشود</exception>
        IGatewayDriver GetDriver(PaymentGateway gateway);

        /// <summary>
        /// بررسی اینکه آیا GatewayType پشتیبانی می‌شود یا نه
        /// </summary>
        /// <param name="gatewayType">نوع درگاه پرداخت</param>
        /// <returns>true اگر پشتیبانی می‌شود، در غیر این صورت false</returns>
        bool IsSupported(PaymentGatewayType gatewayType);
    }
}

