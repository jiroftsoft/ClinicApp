namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش جزئیات قیمت نوبت (قیمت پایه، تخفیف، قیمت نهایی) در UI بیمار
    /// </summary>
    public class AppointmentPriceBreakdownDto
    {
        /// <summary>قیمت پایه (ریال)</summary>
        public decimal BasePrice { get; set; }

        /// <summary>مبلغ تخفیف (ریال)</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>درصد تخفیف</summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>قیمت نهایی پس از تخفیف (ریال)</summary>
        public decimal FinalPrice { get; set; }

        /// <summary>عنوان ایونت تبلیغاتی (در صورت اعمال تخفیف)</summary>
        public string PromotionalEventTitle { get; set; }

        /// <summary>آیا تخفیف اعمال شده است؟</summary>
        public bool HasDiscount => DiscountAmount > 0;
    }
}
