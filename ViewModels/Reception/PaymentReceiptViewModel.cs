using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای رسید پرداخت
    /// </summary>
    public class PaymentReceiptViewModel
    {
        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// شناسه تراکنش (string)
        /// </summary>
        public string TransactionIdString { get; set; }

        /// <summary>
        /// شناسه پرداخت
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// شماره رسید
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// تاریخ پرداخت
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// تاریخ پرداخت (شمسی)
        /// </summary>
        public string PaymentDateShamsi { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ تکمیل
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// مبلغ پرداخت
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Amount { get; set; }

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
        /// روش پرداخت
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// کد ملی بیمار
        /// </summary>
        public string PatientNationalCode { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// شماره پذیرش
        /// </summary>
        public string ReceptionNumber { get; set; }

        /// <summary>
        /// وضعیت پرداخت
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// کد مرجع
        /// </summary>
        public string ReferenceCode { get; set; }

        /// <summary>
        /// کد پیگیری
        /// </summary>
        public string TrackingCode { get; set; }

        /// <summary>
        /// محتوای رسید
        /// </summary>
        public string ReceiptContent { get; set; }

        /// <summary>
        /// QR Code
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// نام کلینیک
        /// </summary>
        public string ClinicName { get; set; }

        /// <summary>
        /// آدرس کلینیک
        /// </summary>
        public string ClinicAddress { get; set; }

        /// <summary>
        /// تلفن کلینیک
        /// </summary>
        public string ClinicPhone { get; set; }

        /// <summary>
        /// نام صندوقدار
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// اطلاعات اضافی
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// یادداشت‌ها
        /// </summary>
        public string Notes { get; set; }
    }
}
