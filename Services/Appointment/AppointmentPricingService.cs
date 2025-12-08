using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
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
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        // قیمت پیش‌فرض در صورت عدم وجود تنظیمات
        private const decimal DEFAULT_CONSULTATION_FEE = 500000m; // 500,000 تومان

        public AppointmentPricingService(
            IDoctorScheduleRepository doctorScheduleRepository,
            ApplicationDbContext context,
            ILogger logger)
        {
            _doctorScheduleRepository = doctorScheduleRepository ?? throw new ArgumentNullException(nameof(doctorScheduleRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<AppointmentPricingService>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// محاسبه قیمت نوبت
        /// </summary>
        public async Task<AppointmentPriceResult> CalculatePriceAsync(
            int doctorId,
            int? serviceCategoryId = null,
            int? patientId = null)
        {
            try
            {
                _logger.Information("شروع محاسبه قیمت نوبت - DoctorId: {DoctorId}, ServiceCategoryId: {ServiceCategoryId}, PatientId: {PatientId}",
                    doctorId, serviceCategoryId, patientId);

                // 1. دریافت قیمت پایه از برنامه کاری پزشک
                var basePrice = await GetBasePriceAsync(doctorId, serviceCategoryId);

                // 2. محاسبه تخفیف‌ها
                var discount = await CalculateDiscountAsync(doctorId, patientId, basePrice);

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
                    Currency = "IRR" // ریال
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
        /// دریافت قیمت پایه
        /// </summary>
        private async Task<decimal> GetBasePriceAsync(int doctorId, int? serviceCategoryId)
        {
            try
            {
                // 1. تلاش برای دریافت قیمت از ServiceCategory (در صورت وجود)
                // TODO: در آینده می‌توان از ServiceCategory برای محاسبه قیمت استفاده کرد
                // فعلاً ServiceCategory دارای Price نیست، پس از ConsultationFee استفاده می‌کنیم

                // 2. دریافت قیمت از برنامه کاری پزشک
                var schedule = await _doctorScheduleRepository.GetDoctorScheduleAsync(doctorId);
                if (schedule != null && schedule.ConsultationFee > 0)
                {
                    _logger.Information("قیمت از DoctorSchedule دریافت شد - DoctorId: {DoctorId}, ConsultationFee: {ConsultationFee}",
                        doctorId, schedule.ConsultationFee);
                    return schedule.ConsultationFee;
                }

                // 3. استفاده از قیمت پیش‌فرض
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
        /// محاسبه تخفیف‌ها
        /// </summary>
        private async Task<decimal> CalculateDiscountAsync(int doctorId, int? patientId, decimal basePrice)
        {
            decimal totalDiscount = 0m;

            try
            {
                // TODO: در آینده می‌توان تخفیف‌های زیر را اضافه کرد:
                // 1. تخفیف بیمه (بر اساس نوع بیمه بیمار)
                // 2. تخفیف ویژه پزشک
                // 3. تخفیف دوره‌ای (مثلاً تخفیف 10% برای اولین نوبت)
                // 4. تخفیف گروهی (مثلاً تخفیف برای اعضای خانواده)

                // فعلاً تخفیف 0 است
                _logger.Debug("محاسبه تخفیف - DoctorId: {DoctorId}, PatientId: {PatientId}, BasePrice: {BasePrice}, Discount: {Discount}",
                    doctorId, patientId, basePrice, totalDiscount);

                return totalDiscount;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه تخفیف");
                return 0m;
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

