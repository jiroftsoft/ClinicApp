using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.Models.DTOs.PromotionalEvent;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models;
using System.Data.Entity;
using Serilog;

namespace ClinicApp.Services.Appointment
{
    /// <summary>
    /// سرویس محاسبه قیمت نوبت
    /// رعایت SRP: فقط محاسبه قیمت
    /// </summary>
    public class AppointmentPricingService
    {
        private readonly IDoctorScheduleRepository _doctorScheduleRepository;
        private readonly IPromotionalEventService _promotionalEventService;
        private readonly ApplicationDbContext _context;
        private readonly IAppSettings _appSettings;
        private readonly ILogger _logger;

        // قیمت پیش‌فرض در صورت عدم وجود تنظیمات
        private const decimal DEFAULT_CONSULTATION_FEE = 500000m; // 500,000 تومان

        public AppointmentPricingService(
            IDoctorScheduleRepository doctorScheduleRepository,
            IPromotionalEventService promotionalEventService,
            ApplicationDbContext context,
            IAppSettings appSettings,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _promotionalEventService = promotionalEventService ?? throw new ArgumentNullException(nameof(promotionalEventService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger?.ForContext<AppointmentPricingService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// محاسبه قیمت نوبت
        /// </summary>
        public async Task<AppointmentPriceResult> CalculatePriceAsync(
            int doctorId,
            int? serviceCategoryId = null,
            int? patientId = null,
            DateTime? appointmentDate = null)
        {
            try
            {
                _logger.Information("💰 PRICING: شروع محاسبه قیمت نوبت - DoctorId: {DoctorId}, ServiceCategoryId: {ServiceCategoryId}, PatientId: {PatientId}, AppointmentDate: {AppointmentDate}",
                    doctorId, serviceCategoryId, patientId, appointmentDate);

                // 1. دریافت قیمت پایه از برنامه کاری پزشک
                var basePrice = await GetBasePriceAsync(doctorId, serviceCategoryId);

                // 2. محاسبه تخفیف‌ها (با جزئیات برای PromotionalEventId و عنوان ایونت)
                var discountResult = await CalculateDiscountWithDetailsAsync(doctorId, patientId, basePrice, appointmentDate);
                var discount = discountResult.TotalDiscount;
                var promotionalEventId = discountResult.PromotionalEventId;
                var promotionalEventTitle = discountResult.PromotionalEventTitle;

                // 3. محاسبه قیمت پس از تخفیف
                var priceAfterDiscount = basePrice - discount;

                // 4. محاسبه مالیات (در حال حاضر 0% - در آینده می‌توان اضافه کرد)
                var taxRate = 0m; // 0% مالیات
                var taxAmount = priceAfterDiscount * (taxRate / 100m);

                // 5. محاسبه قیمت نهایی
                var finalPrice = priceAfterDiscount + taxAmount;

                var result = new AppointmentPriceResult
                {
                    BasePrice = basePrice,
                    DiscountAmount = discount,
                    DiscountPercentage = basePrice > 0 ? (discount / basePrice) * 100m : 0m,
                    PriceAfterDiscount = priceAfterDiscount,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    FinalPrice = finalPrice,
                    Currency = "IRR", // ریال
                    PromotionalEventId = promotionalEventId,
                    PromotionalEventTitle = promotionalEventTitle
                };

                _logger.Information("محاسبه قیمت نوبت تکمیل شد - BasePrice: {BasePrice}, Discount: {Discount}, FinalPrice: {FinalPrice}",
                    basePrice, discount, finalPrice);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت نوبت");
                throw;
            }
        }

        #region Private Methods

        /// <summary>
        /// دریافت قیمت پایه — برای مشاوره آنلاین از OnlineConsultationFee استفاده می‌شود در صورت تنظیم.
        /// </summary>
        private async Task<decimal> GetBasePriceAsync(int doctorId, int? serviceCategoryId)
        {
            try
            {
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule == null)
                {
                    _logger.Warning("قیمت پایه یافت نشد، استفاده از قیمت پیش‌فرض - DoctorId: {DoctorId}, DefaultPrice: {DefaultPrice}",
                        doctorId, DEFAULT_CONSULTATION_FEE);
                    return DEFAULT_CONSULTATION_FEE;
                }

                var isOnlineConsultation = _appSettings.OnlineConsultationServiceCategoryId.HasValue
                    && serviceCategoryId.HasValue
                    && serviceCategoryId.Value == _appSettings.OnlineConsultationServiceCategoryId.Value;

                if (isOnlineConsultation && schedule.OnlineConsultationFee > 0)
                {
                    _logger.Information("قیمت مشاوره آنلاین از DoctorSchedule - DoctorId: {DoctorId}, OnlineConsultationFee: {Fee}",
                        doctorId, schedule.OnlineConsultationFee);
                    return schedule.OnlineConsultationFee;
                }

                if (schedule.ConsultationFee > 0)
                {
                    _logger.Information("قیمت از DoctorSchedule - DoctorId: {DoctorId}, ConsultationFee: {ConsultationFee}",
                        doctorId, schedule.ConsultationFee);
                    return schedule.ConsultationFee;
                }

                _logger.Warning("قیمت پایه یافت نشد، استفاده از قیمت پیش‌فرض - DoctorId: {DoctorId}, DefaultPrice: {DefaultPrice}",
                    doctorId, DEFAULT_CONSULTATION_FEE);
                return DEFAULT_CONSULTATION_FEE;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت قیمت پایه - DoctorId: {DoctorId}", doctorId);
                return DEFAULT_CONSULTATION_FEE;
            }
        }

        /// <summary>
        /// محاسبه تخفیف‌ها با جزئیات (شامل PromotionalEventId)
        /// </summary>
        private async Task<DiscountResult> CalculateDiscountWithDetailsAsync(int doctorId, int? patientId, decimal basePrice, DateTime? appointmentDate = null)
        {
            try
            {
                _logger.Information("💰 PRICING: شروع محاسبه تخفیف با جزئیات - DoctorId: {DoctorId}, PatientId: {PatientId}, BasePrice: {BasePrice}, AppointmentDate: {AppointmentDate}",
                    doctorId, patientId, basePrice, appointmentDate);

                // ✅ محاسبه تخفیف از ایونت‌های تبلیغاتی (با جزئیات)
                var discountResult = await _promotionalEventService.CalculateDiscountWithDetailsAsync(doctorId, basePrice, appointmentDate);
                
                if (!discountResult.Success)
                {
                    _logger.Warning("⚠️ PRICING: خطا در محاسبه تخفیف از ایونت‌های تبلیغاتی: {Error}", discountResult.Message);
                    return new DiscountResult { TotalDiscount = 0m, PromotionalEventId = null, PromotionalEventTitle = null };
                }

                var result = discountResult.Data;

                // TODO: در آینده می‌توان تخفیف‌های زیر را اضافه کرد:
                // 1. تخفیف بیمه (بر اساس نوع بیمه بیمار)
                // 2. تخفیف ویژه پزشک
                // 3. تخفیف دوره‌ای (مثلاً تخفیف 10% برای اولین نوبت)
                // 4. تخفیف گروهی (مثلاً تخفیف برای اعضای خانواده)

                _logger.Information("✅ PRICING: محاسبه تخفیف تکمیل شد - DoctorId: {DoctorId}, BasePrice: {BasePrice}, TotalDiscount: {TotalDiscount}, PromotionalEventId: {PromotionalEventId}",
                    doctorId, basePrice, result.TotalDiscount, result.PromotionalEventId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING: خطا در محاسبه تخفیف - DoctorId: {DoctorId}, BasePrice: {BasePrice}", doctorId, basePrice);
                return new DiscountResult { TotalDiscount = 0m, PromotionalEventId = null, PromotionalEventTitle = null };
            }
        }

        #endregion
    }

    /// <summary>
    /// نتیجه محاسبه قیمت نوبت
    /// </summary>
    public class AppointmentPriceResult
    {
        /// <summary>
        /// قیمت پایه (قبل از تخفیف و مالیات)
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// مبلغ تخفیف
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// درصد تخفیف
        /// </summary>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// قیمت پس از تخفیف (قبل از مالیات)
        /// </summary>
        public decimal PriceAfterDiscount { get; set; }

        /// <summary>
        /// نرخ مالیات (درصد)
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// مبلغ مالیات
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// قیمت نهایی (قابل پرداخت)
        /// </summary>
        public decimal FinalPrice { get; set; }

        /// <summary>
        /// واحد پول
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// شناسه ایونت تبلیغاتی که تخفیف از آن اعمال شده است (اختیاری)
        /// </summary>
        public int? PromotionalEventId { get; set; }

        /// <summary>
        /// عنوان ایونت تبلیغاتی (برای نمایش در UI بیمار)
        /// </summary>
        public string PromotionalEventTitle { get; set; }

        /// <summary>
        /// نمایش قیمت به صورت فرمت شده
        /// </summary>
        public string GetFormattedPrice()
        {
            return $"{FinalPrice:N0} {Currency}";
        }

        /// <summary>
        /// نمایش جزئیات قیمت
        /// </summary>
        public string GetPriceBreakdown()
        {
            var breakdown = $"قیمت پایه: {BasePrice:N0} {Currency}";
            
            if (DiscountAmount > 0)
            {
                breakdown += $"\nتخفیف ({DiscountPercentage:F1}%): {DiscountAmount:N0} {Currency}";
                breakdown += $"\nقیمت پس از تخفیف: {PriceAfterDiscount:N0} {Currency}";
            }

            if (TaxAmount > 0)
            {
                breakdown += $"\nمالیات ({TaxRate:F1}%): {TaxAmount:N0} {Currency}";
            }

            breakdown += $"\nقیمت نهایی: {FinalPrice:N0} {Currency}";
            
            return breakdown;
        }
    }
}


