using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.PromotionalEvent
{
    /// <summary>
    /// سرویس ارسال پیامک ایونت تبلیغاتی به مشتریان (آسانک)
    /// </summary>
    public interface IPromotionalEventSmsService
    {
        /// <summary>
        /// ارسال پیامک ایونت به مخاطبان انتخاب‌شده
        /// </summary>
        /// <param name="eventId">شناسه ایونت</param>
        /// <param name="audience">نوع مخاطب (بیماران، مشترکین خبرنامه، هر دو)</param>
        /// <param name="customMessage">متن سفارشی؛ در صورت null از قالب پیش‌فرض استفاده می‌شود (حداکثر ۱۶۰ کاراکتر)</param>
        /// <returns>نتیجه با تعداد ارسال‌شده و خطا</returns>
        Task<ServiceResult<PromotionalEventSmsSendResult>> SendEventSmsToCustomersAsync(
            int eventId,
            PromotionalEventAudience audience,
            string customMessage = null);

        /// <summary>
        /// دریافت تعداد تقریبی مخاطب برای هر گزینه (برای صفحه تأیید ارسال)
        /// </summary>
        Task<PromotionalEventAudienceCountsDto> GetAudienceCountsAsync();
    }

    /// <summary>
    /// نتیجه ارسال پیامک ایونت
    /// </summary>
    public class PromotionalEventSmsSendResult
    {
        public int SentCount { get; set; }
        public int FailedCount { get; set; }
        public int TotalRecipients { get; set; }
        public string MessageBody { get; set; }
    }

    /// <summary>
    /// تعداد مخاطب برای هر نوع (برای نمایش در صفحه تأیید)
    /// </summary>
    public class PromotionalEventAudienceCountsDto
    {
        public int PatientsWithPhoneCount { get; set; }
        public int NewsletterSubscribersCount { get; set; }
        public int BothCount { get; set; }
    }
}
