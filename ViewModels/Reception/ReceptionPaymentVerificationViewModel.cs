using System;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش تأیید پرداخت در فرم پذیرش
    /// </summary>
    public class ReceptionPaymentVerificationViewModel
    {
        /// <summary>
        /// شناسه پرداخت
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// آیا پرداخت تأیید شد؟
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// تاریخ تأیید
        /// </summary>
        public DateTime VerificationDate { get; set; }

        /// <summary>
        /// وضعیت تأیید
        /// </summary>
        public string VerificationStatus { get; set; }

        /// <summary>
        /// پیام تأیید
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد تأیید
        /// </summary>
        public string VerificationCode { get; set; }

        /// <summary>
        /// نمایش وضعیت تأیید
        /// </summary>
        public string VerificationStatusDisplay => VerificationStatus ?? "نامشخص";

        /// <summary>
        /// نمایش تاریخ تأیید (فرمات شده)
        /// </summary>
        public string VerificationDateDisplay => VerificationDate.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// نمایش نتیجه تأیید (فرمات شده)
        /// </summary>
        public string VerificationResultDisplay => IsVerified ? "تأیید شد" : "تأیید نشد";

        /// <summary>
        /// نمایش اطلاعات تأیید (فرمات شده)
        /// </summary>
        public string VerificationInfoDisplay => $"{VerificationResultDisplay} - {VerificationDateDisplay}";

        /// <summary>
        /// آیا پرداخت تأیید شد؟
        /// </summary>
        public bool IsPaymentVerified => IsVerified && VerificationStatus == "Verified";

        /// <summary>
        /// آیا پرداخت تأیید نشد؟
        /// </summary>
        public bool IsPaymentNotVerified => !IsVerified || VerificationStatus != "Verified";
    }
}
