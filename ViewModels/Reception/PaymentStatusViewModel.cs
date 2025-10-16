using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش وضعیت پرداخت
    /// </summary>
    public class PaymentStatusViewModel
    {
        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// شناسه پرداخت
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// وضعیت پرداخت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// توضیحات وضعیت
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// پیام
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// آخرین به‌روزرسانی
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ تکمیل
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// مبلغ
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// شماره رسید
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// آیا قابل لغو است
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// آیا قابل بازگشت است
        /// </summary>
        public bool CanRefund { get; set; }

        /// <summary>
        /// تاریخچه وضعیت
        /// </summary>
        public List<PaymentStatusHistoryViewModel> StatusHistory { get; set; } = new List<PaymentStatusHistoryViewModel>();
    }

    /// <summary>
    /// ViewModel برای تاریخچه وضعیت پرداخت
    /// </summary>
    public class PaymentStatusHistoryViewModel
    {
        /// <summary>
        /// وضعیت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// توضیحات وضعیت
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// تاریخ تغییر
        /// </summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// تغییر دهنده
        /// </summary>
        public string ChangedBy { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        public string Notes { get; set; }
    }
}
