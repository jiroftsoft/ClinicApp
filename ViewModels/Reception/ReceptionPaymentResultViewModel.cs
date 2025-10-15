using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش نتیجه پرداخت در فرم پذیرش
    /// </summary>
    public class ReceptionPaymentResultViewModel
    {
        /// <summary>
        /// شناسه پرداخت
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// مبلغ پرداخت
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// وضعیت پرداخت
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// تاریخ پرداخت
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// آیا پرداخت موفق بود؟
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد خطا (در صورت عدم موفقیت)
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// پیام خطا (در صورت عدم موفقیت)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// نمایش مبلغ پرداخت (فرمات شده)
        /// </summary>
        public string AmountDisplay => $"{Amount:N0} ریال";

        /// <summary>
        /// نمایش روش پرداخت
        /// </summary>
        public string PaymentMethodDisplay => PaymentMethod ?? "نامشخص";

        /// <summary>
        /// نمایش وضعیت پرداخت
        /// </summary>
        public string PaymentStatusDisplay => PaymentStatus ?? "نامشخص";

        /// <summary>
        /// نمایش تاریخ پرداخت (فرمات شده)
        /// </summary>
        public string PaymentDateDisplay => PaymentDate.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// نمایش نتیجه پرداخت (فرمات شده)
        /// </summary>
        public string PaymentResultDisplay => IsSuccessful ? "موفق" : "ناموفق";

        /// <summary>
        /// نمایش اطلاعات پرداخت (فرمات شده)
        /// </summary>
        public string PaymentInfoDisplay => $"{AmountDisplay} - {PaymentMethodDisplay} - {PaymentResultDisplay}";

        /// <summary>
        /// آیا پرداخت موفق بود؟
        /// </summary>
        public bool IsPaymentSuccessful => IsSuccessful && PaymentStatus == "Success";

        /// <summary>
        /// آیا پرداخت ناموفق بود؟
        /// </summary>
        public bool IsPaymentFailed => !IsSuccessful || PaymentStatus != "Success";

        /// <summary>
        /// نمایش پیام نتیجه
        /// </summary>
        public string ResultMessage => IsSuccessful ? Message : ErrorMessage;
    }
}
