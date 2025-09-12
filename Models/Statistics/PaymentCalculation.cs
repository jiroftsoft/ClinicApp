using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// مدل محاسبه پرداخت
    /// </summary>
    public class PaymentCalculation
    {
        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        public int ReceptionId { get; set; }

        /// <summary>
        /// مبلغ پایه
        /// </summary>
        [Required]
        public decimal BaseAmount { get; set; }

        /// <summary>
        /// مبلغ تخفیف
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// مبلغ نهایی (پس از تخفیف)
        /// </summary>
        [Required]
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// کارمزد درگاه
        /// </summary>
        public decimal GatewayFee { get; set; }

        /// <summary>
        /// مبلغ کل (نهایی + کارمزد)
        /// </summary>
        [Required]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// روش پرداخت
        /// </summary>
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// تاریخ محاسبه
        /// </summary>
        public DateTime CalculatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }
    }
}
