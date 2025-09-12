using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// مدل فیلترهای جستجوی پرداخت
    /// </summary>
    public class PaymentSearchFilters
    {
        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        public int? ReceptionId { get; set; }

        /// <summary>
        /// شناسه قرار ملاقات
        /// </summary>
        public int? AppointmentId { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        public PaymentMethod? PaymentMethod { get; set; }

        /// <summary>
        /// وضعیت پرداخت
        /// </summary>
        public PaymentStatus? PaymentStatus { get; set; }

        /// <summary>
        /// حداقل مبلغ
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// حداکثر مبلغ
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// نام بیمار
        /// </summary>
        public string PatientName { get; set; }

        /// <summary>
        /// نام پزشک
        /// </summary>
        public string DoctorName { get; set; }

        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// کد مرجع
        /// </summary>
        public string ReferenceCode { get; set; }

        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// اندازه صفحه
        /// </summary>
        public int PageSize { get; set; } = 50;
    }
}
