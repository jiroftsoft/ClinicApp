using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای درخواست پرداخت
    /// </summary>
    public class PaymentRequestViewModel
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
        /// نوع پرداخت
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// شناسه درگاه
        /// </summary>
        public int? GatewayId { get; set; }

        /// <summary>
        /// شناسه صندوقدار
        /// </summary>
        public int? CashierId { get; set; }

        /// <summary>
        /// نام صندوقدار
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// مکان پرداخت
        /// </summary>
        public string PaymentLocation { get; set; }

        /// <summary>
        /// متادیتا
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// URL بازگشت
        /// </summary>
        public string CallbackUrl { get; set; }
    }
}