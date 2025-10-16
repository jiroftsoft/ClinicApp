using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش اطلاعات پرداخت
    /// </summary>
    public class PaymentInfoViewModel
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// مبلغ قابل پرداخت
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PayableAmount { get; set; }

        /// <summary>
        /// مبلغ کل
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// سهم بیمه
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal InsuranceShare { get; set; }

        /// <summary>
        /// سهم بیمار
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal PatientShare { get; set; }

        /// <summary>
        /// ارز
        /// </summary>
        public string Currency { get; set; } = "IRR";

        /// <summary>
        /// اطلاعات درگاه
        /// </summary>
        public string GatewayInfo { get; set; }

        /// <summary>
        /// اطلاعات درگاه (ViewModel)
        /// </summary>
        public PaymentGatewayInfoViewModel GatewayInfoViewModel { get; set; }

        /// <summary>
        /// روش‌های پرداخت موجود
        /// </summary>
        public List<string> AvailablePaymentMethods { get; set; } = new List<string>();

        /// <summary>
        /// روش‌های پرداخت موجود (ViewModel)
        /// </summary>
        public List<PaymentMethodViewModel> AvailablePaymentMethodViewModels { get; set; } = new List<PaymentMethodViewModel>();

        /// <summary>
        /// آیا پرداخت فعال است
        /// </summary>
        public bool IsPaymentEnabled { get; set; }

        /// <summary>
        /// آیا می‌تواند آنلاین پرداخت کند
        /// </summary>
        public bool CanPayOnline { get; set; }

        /// <summary>
        /// آیا می‌تواند نقدی پرداخت کند
        /// </summary>
        public bool CanPayCash { get; set; }

        /// <summary>
        /// پیام وضعیت
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
