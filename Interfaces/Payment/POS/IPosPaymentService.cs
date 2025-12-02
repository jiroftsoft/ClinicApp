using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Payment;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment.POS
{
    /// <summary>
    /// Service Interface for POS Payment Processing
    /// 
    /// مسئولیت: مدیریت منطق کسب‌وکار پرداخت POS
    /// 
    /// اصول طراحی:
    /// ✅ Single Responsibility: فقط منطق پرداخت POS
    /// ✅ Separation of Concerns: جدا از Device Communication
    /// ✅ High Testability: Interface قابل Mock کردن
    /// ✅ Production-Ready: آماده برای استفاده در Production
    /// 
    /// استفاده:
    /// - ماژول پذیرش (Reception)
    /// - ماژول صندوق (Cashier)
    /// - سایر ماژول‌های پرداخت
    /// </summary>
    public interface IPosPaymentService
    {
        /// <summary>
        /// پردازش پرداخت POS
        /// 
        /// این متد کل فرایند پرداخت را مدیریت می‌کند:
        /// 1. اعتبارسنجی درخواست
        /// 2. دریافت ترمینال
        /// 3. پردازش پرداخت با Retry Logic
        /// 4. ثبت تراکنش در دیتابیس
        /// 5. Logging کامل
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه پرداخت</returns>
        Task<ServiceResult<PosPaymentResult>> ProcessPaymentAsync(PosPaymentRequest request);

        /// <summary>
        /// اعتبارسنجی درخواست پرداخت
        /// </summary>
        /// <param name="request">درخواست پرداخت</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ServiceResult> ValidatePaymentRequestAsync(PosPaymentRequest request);

        /// <summary>
        /// دریافت اطلاعات ترمینال برای پرداخت
        /// </summary>
        /// <param name="terminalId">شناسه ترمینال (اختیاری - اگر null باشد، ترمینال پیش‌فرض استفاده می‌شود)</param>
        /// <returns>اطلاعات ترمینال</returns>
        Task<ServiceResult<PosTerminal>> GetTerminalForPaymentAsync(int? terminalId = null);

        /// <summary>
        /// ثبت تراکنش پرداخت در دیتابیس
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="paymentResult">نتیجه پرداخت</param>
        /// <returns>نتیجه ثبت</returns>
        Task<ServiceResult> RegisterPaymentTransactionAsync(int receptionId, PosPaymentResult paymentResult);
    }

    /// <summary>
    /// درخواست پرداخت POS
    /// </summary>
    public class PosPaymentRequest
    {
        /// <summary>
        /// شناسه پذیرش (برای تست می‌تواند 0 باشد)
        /// </summary>
        public int ReceptionId { get; set; }

        /// <summary>
        /// مبلغ پرداخت به ریال
        /// </summary>
        public decimal AmountIRR { get; set; }

        /// <summary>
        /// شناسه ترمینال (اختیاری - اگر null باشد، ترمینال پیش‌فرض استفاده می‌شود)
        /// </summary>
        public int? TerminalId { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// توضیحات اضافی
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// نتیجه پرداخت POS
    /// </summary>
    public class PosPaymentResult
    {
        /// <summary>
        /// آیا پرداخت موفق بود؟
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// RRN (Retrieval Reference Number)
        /// </summary>
        public string RRN { get; set; }

        /// <summary>
        /// Trace Number
        /// </summary>
        public string TraceNo { get; set; }

        /// <summary>
        /// Terminal ID
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// آخرین 4 رقم کارت
        /// </summary>
        public string CardLast4 { get; set; }

        /// <summary>
        /// مبلغ پرداخت
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// پیام
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد خطا (در صورت خطا)
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Operation ID برای Tracking
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// زمان پردازش به میلی‌ثانیه
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// تعداد تلاش‌ها
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// آیا توسط کاربر لغو شد؟
        /// </summary>
        public bool IsCanceled { get; set; }
    }
}

