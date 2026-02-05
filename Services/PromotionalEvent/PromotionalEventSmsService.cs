using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.PromotionalEvent;
using ClinicApp.Models.Enums;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Services.PromotionalEvent
{
    /// <summary>
    /// سرویس ارسال پیامک ایونت تبلیغاتی به مشتریان (آسانک)
    /// - دریافت ایونت، ساخت متن (حداکثر ۱۶۰ کاراکتر)، دریافت شماره‌ها بر اساس مخاطب، ارسال، لاگ و مدیریت خطا
    /// </summary>
    public class PromotionalEventSmsService : IPromotionalEventSmsService
    {
        private const int MaxSmsLength = 160;
        private const string DefaultClinicName = "کلینیک شفا";
        private const string AppointmentUrlText = "رزرو: /Patient/Appointment/Available";

        private static readonly Regex PersianArabicDigits = new Regex("[\u06F0-\u06F9\u0660-\u0669]", RegexOptions.Compiled);
        private static readonly Regex NonDigitExceptPlus = new Regex(@"(?!^\+)[^\d]", RegexOptions.Compiled);

        private readonly IPromotionalEventRepository _eventRepository;
        private readonly IIdentityMessageService _smsService;
        private readonly ApplicationDbContext _context;
        private readonly INewsletterSubscriptionRepository _newsletterRepository;
        private readonly ILogger _logger;

        public PromotionalEventSmsService(
            IPromotionalEventRepository eventRepository,
            IIdentityMessageService smsService,
            ApplicationDbContext context,
            INewsletterSubscriptionRepository newsletterRepository,
            ILogger logger)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _newsletterRepository = newsletterRepository ?? throw new ArgumentNullException(nameof(newsletterRepository));
            _logger = logger?.ForContext<PromotionalEventSmsService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<PromotionalEventSmsSendResult>> SendEventSmsToCustomersAsync(
            int eventId,
            PromotionalEventAudience audience,
            string customMessage = null)
        {
            try
            {
                var ev = await _eventRepository.GetByIdAsync(eventId);
                if (ev == null || ev.IsDeleted)
                {
                    return ServiceResult<PromotionalEventSmsSendResult>.Failed("ایونت تبلیغاتی یافت نشد.");
                }

                string body = !string.IsNullOrWhiteSpace(customMessage)
                    ? TruncateToSmsLength(customMessage.Trim())
                    : BuildDefaultMessage(ev);

                var phoneNumbers = await GetPhoneNumbersByAudienceAsync(audience);
                if (phoneNumbers == null || phoneNumbers.Count == 0)
                {
                    return ServiceResult<PromotionalEventSmsSendResult>.Failed("هیچ شماره‌ای برای ارسال یافت نشد.");
                }

                int sent = 0;
                int failed = 0;
                foreach (var phone in phoneNumbers)
                {
                    try
                    {
                        var message = new IdentityMessage
                        {
                            Destination = phone,
                            Body = body
                        };
                        await _smsService.SendAsync(message);
                        sent++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        _logger.Warning(ex, "خطا در ارسال پیامک ایونت به شماره {Phone}", MaskPhone(phone));
                    }
                }

                var result = new PromotionalEventSmsSendResult
                {
                    TotalRecipients = phoneNumbers.Count,
                    SentCount = sent,
                    FailedCount = failed,
                    MessageBody = body
                };

                _logger.Information(
                    "ارسال پیامک ایونت {EventId} به {Audience}: ارسال‌شده={Sent}, ناموفق={Failed}, کل={Total}",
                    eventId, audience, sent, failed, phoneNumbers.Count);

                return ServiceResult<PromotionalEventSmsSendResult>.Successful(
                    result,
                    sent > 0
                        ? $"پیامک به {sent} مخاطب ارسال شد." + (failed > 0 ? $" ({failed} مورد ناموفق)" : "")
                        : "ارسال انجام نشد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال پیامک ایونت {EventId}", eventId);
                return ServiceResult<PromotionalEventSmsSendResult>.Failed("خطا در ارسال پیامک ایونت.");
            }
        }

        /// <inheritdoc />
        public async Task<PromotionalEventAudienceCountsDto> GetAudienceCountsAsync()
        {
            try
            {
                var patientPhonesTask = GetPatientPhoneNumbersNormalizedAsync();
                var newsletterPhonesTask = GetNewsletterPhoneNumbersNormalizedAsync();
                await Task.WhenAll(patientPhonesTask, newsletterPhonesTask);

                var patientSet = new HashSet<string>(await patientPhonesTask, StringComparer.Ordinal);
                var newsletterSet = new HashSet<string>(await newsletterPhonesTask, StringComparer.Ordinal);
                var bothSet = new HashSet<string>(patientSet, StringComparer.Ordinal);
                foreach (var p in newsletterSet)
                    bothSet.Add(p);

                return new PromotionalEventAudienceCountsDto
                {
                    PatientsWithPhoneCount = patientSet.Count,
                    NewsletterSubscribersCount = newsletterSet.Count,
                    BothCount = bothSet.Count
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد مخاطبان پیامک ایونت");
                return new PromotionalEventAudienceCountsDto();
            }
        }

        private async Task<List<string>> GetPhoneNumbersByAudienceAsync(PromotionalEventAudience audience)
        {
            var patientPhones = await GetPatientPhoneNumbersNormalizedAsync();
            var newsletterPhones = await GetNewsletterPhoneNumbersNormalizedAsync();

            switch (audience)
            {
                case PromotionalEventAudience.PatientsWithPhone:
                    return patientPhones;
                case PromotionalEventAudience.NewsletterSubscribers:
                    return newsletterPhones;
                case PromotionalEventAudience.Both:
                    var set = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var p in patientPhones) set.Add(p);
                    foreach (var p in newsletterPhones) set.Add(p);
                    return set.ToList();
                default:
                    return new List<string>();
            }
        }

        /// <summary>
        /// شماره موبایل بیماران (اعتبارسنجی و نرمال‌سازی، حذف تکراری)
        /// </summary>
        private async Task<List<string>> GetPatientPhoneNumbersNormalizedAsync()
        {
            var raw = await _context.Patients
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.PhoneNumber != null && p.PhoneNumber != "")
                .Select(p => p.PhoneNumber)
                .ToListAsync();

            return NormalizeAndDedupe(raw);
        }

        /// <summary>
        /// شماره مشترکین خبرنامه فعال و تأییدشده (دارای شماره)
        /// </summary>
        private async Task<List<string>> GetNewsletterPhoneNumbersNormalizedAsync()
        {
            var subscribers = await _newsletterRepository.GetActiveAndVerifiedAsync(includeDeleted: false);
            var raw = subscribers
                .Where(n => !string.IsNullOrWhiteSpace(n.PhoneNumber))
                .Select(n => n.PhoneNumber)
                .ToList();

            return NormalizeAndDedupe(raw);
        }

        private static List<string> NormalizeAndDedupe(List<string> raw)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in raw)
            {
                var normalized = NormalizeMsisdn(s);
                if (!string.IsNullOrEmpty(normalized) && IsLikelyPhone(normalized))
                    set.Add(normalized);
            }
            return set.ToList();
        }

        private static string NormalizeMsisdn(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            string converted = PersianArabicDigits.Replace(input, m =>
            {
                char ch = m.Value[0];
                int digit = ch >= '\u06F0' && ch <= '\u06F9' ? ch - '\u06F0' : ch - '\u0660';
                return digit.ToString(System.Globalization.CultureInfo.InvariantCulture);
            });
            converted = NonDigitExceptPlus.Replace(converted, string.Empty).Trim();

            if (converted.StartsWith("0098")) converted = "+98" + converted.Substring(4);
            if (converted.StartsWith("098")) converted = "+98" + converted.Substring(3);
            if (converted.StartsWith("98") && !converted.StartsWith("+")) converted = "+98" + converted.Substring(2);
            if (converted.StartsWith("0") && converted.Length >= 10)
                converted = "+98" + converted.Substring(1);
            if (!converted.StartsWith("+") && converted.Length == 10 && converted.StartsWith("9"))
                converted = "+98" + converted;

            return converted;
        }

        private static bool IsLikelyPhone(string msisdn)
        {
            if (string.IsNullOrWhiteSpace(msisdn)) return false;
            return Regex.IsMatch(msisdn, @"^\+\d{8,15}$");
        }

        private static string BuildDefaultMessage(Models.Entities.PromotionalEvent.PromotionalEvent ev)
        {
            string clinic = DefaultClinicName;
            string title = ev.Title ?? "ایونت تبلیغاتی";
            string discountPart = ev.DiscountType == DiscountType.Percentage
                ? $"{ev.DiscountValue:N0}٪"
                : $"{ev.DiscountValue:N0} ریال";
            string endDate = PersianDateHelper.ToPersianDate(ev.EndDate);
            string text = $"«{clinic}» {title}. تخفیف {discountPart}. تا {endDate}. {AppointmentUrlText}";
            return TruncateToSmsLength(text);
        }

        private static string TruncateToSmsLength(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= MaxSmsLength ? text : text.Substring(0, MaxSmsLength - 3) + "...";
        }

        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 6) return "***";
            return phone.Substring(0, 4) + "***" + phone.Substring(phone.Length - 2);
        }
    }
}
