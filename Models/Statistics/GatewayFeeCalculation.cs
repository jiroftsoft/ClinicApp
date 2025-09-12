using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Statistics
{
    /// <summary>
    /// مدل محاسبه کارمزد درگاه
    /// </summary>
    public class GatewayFeeCalculation
    {
        /// <summary>
        /// شناسه درگاه
        /// </summary>
        public int GatewayId { get; set; }

        /// <summary>
        /// نام درگاه
        /// </summary>
        public string GatewayName { get; set; }

        /// <summary>
        /// مبلغ اصلی
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// کارمزد ثابت
        /// </summary>
        public decimal FixedFee { get; set; }

        /// <summary>
        /// کارمزد درصدی
        /// </summary>
        public decimal PercentageFee { get; set; }

        /// <summary>
        /// کل کارمزد
        /// </summary>
        [Required]
        public decimal TotalFee { get; set; }

        /// <summary>
        /// کارمزد محاسبه شده
        /// </summary>
        public decimal CalculatedFee { get; set; }

        /// <summary>
        /// مبلغ خالص (پس از کسر کارمزد)
        /// </summary>
        [Required]
        public decimal NetAmount { get; set; }

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
