using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای درخواست پرداخت در فرم پذیرش
    /// </summary>
    public class ReceptionPaymentRequestViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        public int PatientId { get; set; }

        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        [Required(ErrorMessage = "شناسه پذیرش الزامی است")]
        public int ReceptionId { get; set; }

        /// <summary>
        /// مبلغ پرداخت
        /// </summary>
        [Required(ErrorMessage = "مبلغ پرداخت الزامی است")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ پرداخت باید بیشتر از صفر باشد")]
        public decimal Amount { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        [Required(ErrorMessage = "روش پرداخت الزامی است")]
        public string PaymentMethod { get; set; } = "POS";

        /// <summary>
        /// شماره کارت
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// تاریخ انقضای کارت
        /// </summary>
        public string CardExpiryDate { get; set; }

        /// <summary>
        /// CVV کارت
        /// </summary>
        public string CardCVV { get; set; }

        /// <summary>
        /// نام دارنده کارت
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// توضیحات پرداخت
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// تاریخ درخواست پرداخت
        /// </summary>
        public DateTime RequestDate { get; set; } = DateTime.Now;

        /// <summary>
        /// نمایش مبلغ پرداخت (فرمات شده)
        /// </summary>
        public string AmountDisplay => $"{Amount:N0} ریال";

        /// <summary>
        /// نمایش روش پرداخت
        /// </summary>
        public string PaymentMethodDisplay => PaymentMethod ?? "نامشخص";

        /// <summary>
        /// نمایش اطلاعات درخواست پرداخت (فرمات شده)
        /// </summary>
        public string PaymentRequestInfoDisplay => $"{AmountDisplay} - {PaymentMethodDisplay}";

        /// <summary>
        /// آیا درخواست پرداخت معتبر است؟
        /// </summary>
        public bool IsValid => PatientId > 0 && ReceptionId > 0 && Amount > 0 && !string.IsNullOrWhiteSpace(PaymentMethod);
    }
}
