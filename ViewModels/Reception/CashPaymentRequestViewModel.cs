using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای درخواست پرداخت نقدی
    /// </summary>
    public class CashPaymentRequestViewModel
    {
        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        public int ReceptionId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// مبلغ
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// شناسه صندوقدار
        /// </summary>
        public int CashierId { get; set; }

        /// <summary>
        /// نام صندوقدار
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// مکان پرداخت
        /// </summary>
        public string PaymentLocation { get; set; }

        /// <summary>
        /// شماره رسید
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// متادیتا
        /// </summary>
        public string Metadata { get; set; }
    }
}