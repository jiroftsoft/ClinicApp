using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای اطلاعات درگاه پرداخت
    /// </summary>
    public class PaymentGatewayInfoViewModel
    {
        /// <summary>
        /// شناسه درگاه
        /// </summary>
        public int GatewayId { get; set; }

        /// <summary>
        /// نام درگاه
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// نام درگاه (قدیمی)
        /// </summary>
        public string GatewayName { get; set; }

        /// <summary>
        /// کد درگاه
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// کد درگاه (قدیمی)
        /// </summary>
        public string GatewayCode { get; set; }

        /// <summary>
        /// نوع درگاه
        /// </summary>
        public string GatewayType { get; set; }

        /// <summary>
        /// URL درگاه
        /// </summary>
        public string GatewayUrl { get; set; }

        /// <summary>
        /// API Endpoint
        /// </summary>
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// شناسه ترمینال
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// کلید عمومی
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// کلید خصوصی
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// سطح امنیت
        /// </summary>
        public string SecurityLevel { get; set; }

        /// <summary>
        /// ارزهای پشتیبانی شده
        /// </summary>
        public string SupportedCurrencies { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// درصد کارمزد
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal FeePercentage { get; set; }

        /// <summary>
        /// مبلغ کارمزد ثابت
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal FixedFee { get; set; }

        /// <summary>
        /// حداقل مبلغ
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MinAmount { get; set; }

        /// <summary>
        /// حداکثر مبلغ
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }
    }
}
