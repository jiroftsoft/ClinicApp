using System;

namespace ClinicApp.Models.DTOs.PromotionalEvent
{
    /// <summary>
    /// نتیجه محاسبه تخفیف از ایونت‌های تبلیغاتی
    /// </summary>
    public class DiscountResult
    {
        /// <summary>
        /// مبلغ کل تخفیف (ریال)
        /// </summary>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// شناسه ایونت تبلیغاتی که تخفیف از آن اعمال شده است
        /// اگر چند ایونت اعمال شده باشد، اولین ایونت (با بیشترین تخفیف) برگردانده می‌شود
        /// اگر هیچ تخفیفی اعمال نشده باشد، null است
        /// </summary>
        public int? PromotionalEventId { get; set; }

        /// <summary>
        /// عنوان ایونت تبلیغاتی (برای نمایش)
        /// </summary>
        public string PromotionalEventTitle { get; set; }

        /// <summary>
        /// آیا تخفیف اعمال شده است؟
        /// </summary>
        public bool HasDiscount => TotalDiscount > 0 && PromotionalEventId.HasValue;
    }
}

