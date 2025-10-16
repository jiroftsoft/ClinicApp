using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای روش‌های پرداخت
    /// </summary>
    public class PaymentMethodViewModel
    {
        /// <summary>
        /// شناسه روش پرداخت
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// شناسه روش پرداخت (قدیمی)
        /// </summary>
        public int MethodId { get; set; }

        /// <summary>
        /// نام روش پرداخت
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// نام روش پرداخت (قدیمی)
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// کد روش پرداخت
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// کد روش پرداخت (قدیمی)
        /// </summary>
        public string MethodCode { get; set; }

        /// <summary>
        /// نوع روش پرداخت
        /// </summary>
        public string MethodType { get; set; }

        /// <summary>
        /// حداقل مبلغ
        /// </summary>
        public decimal MinAmount { get; set; }

        /// <summary>
        /// حداکثر مبلغ
        /// </summary>
        public decimal MaxAmount { get; set; }

        /// <summary>
        /// درصد کارمزد
        /// </summary>
        public decimal FeePercentage { get; set; }

        /// <summary>
        /// کارمزد ثابت
        /// </summary>
        public decimal FixedFee { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// آیا فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا نیاز به درگاه دارد
        /// </summary>
        public bool RequiresGateway { get; set; }

        /// <summary>
        /// شناسه درگاه
        /// </summary>
        public int? GatewayId { get; set; }

        /// <summary>
        /// نام درگاه
        /// </summary>
        public string GatewayName { get; set; }

        /// <summary>
        /// ترتیب نمایش
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
