using System.ComponentModel.DataAnnotations;

namespace ClinicApp.Models.Enums
{
    /// <summary>
    /// نوع اختلاف مالی
    /// 
    /// استفاده در:
    /// - PaymentDiscrepancy Entity
    /// - گزارش‌های اختلاف
    /// - Dashboard صندوق
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public enum DiscrepancyType
    {
        /// <summary>
        /// کسری - مبلغ واقعی کمتر از مبلغ مورد انتظار
        /// مثال: موجودی صندوق 95,000 ریال است اما باید 100,000 باشد
        /// </summary>
        [Display(Name = "کسری")]
        Shortage = 1,

        /// <summary>
        /// مازاد - مبلغ واقعی بیشتر از مبلغ مورد انتظار
        /// مثال: موجودی صندوق 105,000 ریال است اما باید 100,000 باشد
        /// </summary>
        [Display(Name = "مازاد")]
        Overage = 2,

        /// <summary>
        /// عدم تطابق - تفاوت بین تراکنش ثبت شده و واقعیت
        /// مثال: تراکنش POS 50,000 ثبت شده اما در واقع 500,000 بوده
        /// </summary>
        [Display(Name = "عدم تطابق")]
        Mismatch = 3
    }
}

