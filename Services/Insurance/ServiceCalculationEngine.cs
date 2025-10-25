using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// موتور محاسبه خدمات - طبق مصوبه 1404
    /// 
    /// فرمول اصلی:
    /// UnitPriceIRR = (K_Technical × K_Professional) × ServiceBasePrice
    /// 
    /// قوانین:
    /// 1. Groups 1-7 (# hashed): K_Technical × K_Professional
    /// 2. Non-hashed: K_Technical × K_Professional  
    /// 3. Rounding: به ریال (بدون اعشار)
    /// 4. Rule Engine: برای exceptions
    /// </summary>
    public class ServiceCalculationEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public ServiceCalculationEngine(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Core Calculation Methods

        /// <summary>
        /// محاسبه UnitPriceIRR طبق فرمول اصلی
        /// </summary>
        public async Task<decimal> CalculateUnitPriceIRRAsync(int serviceId, int financialYear)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه UnitPriceIRR - ServiceId: {ServiceId}, FinancialYear: {FinancialYear}", 
                    serviceId, financialYear);

                // دریافت اطلاعات خدمت
                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد - ServiceId: {ServiceId}", serviceId);
                    return 0m;
                }

                // دریافت ضرایب
                var factors = await GetActiveFactorsAsync(service.IsHashtagged, financialYear);
                if (factors == null)
                {
                    _logger.Warning("🏥 MEDICAL: ضرایب یافت نشد - ServiceId: {ServiceId}, IsHashtagged: {IsHashtagged}", 
                        serviceId, service.IsHashtagged);
                    return service.Price; // Fallback
                }

                // محاسبه بر اساس اجزای خدمت
                decimal unitPriceIRR;
                if (service.ServiceComponents?.Any() == true)
                {
                    unitPriceIRR = CalculateWithServiceComponents(service, factors);
                }
                else
                {
                    unitPriceIRR = CalculateWithBasePrice(service, factors);
                }

                // اعمال Rule Engine
                unitPriceIRR = ApplyRuleEngine(service, unitPriceIRR, factors);

                // گرد کردن به ریال
                unitPriceIRR = Math.Round(unitPriceIRR, 0, MidpointRounding.AwayFromZero);

                _logger.Information("🏥 MEDICAL: محاسبه UnitPriceIRR تکمیل شد - ServiceId: {ServiceId}, UnitPriceIRR: {UnitPriceIRR}", 
                    serviceId, unitPriceIRR);

                return unitPriceIRR;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه UnitPriceIRR - ServiceId: {ServiceId}", serviceId);
                return 0m;
            }
        }

        /// <summary>
        /// محاسبه با اجزای خدمت (ServiceComponents)
        /// </summary>
        private decimal CalculateWithServiceComponents(Service service, FactorPair factors)
        {
            decimal totalPrice = 0m;

            foreach (var component in service.ServiceComponents)
            {
                decimal componentPrice = 0m;

                if (component.ComponentType == ServiceComponentType.Technical)
                {
                    componentPrice = component.Coefficient * factors.TechnicalFactor;
                }
                else if (component.ComponentType == ServiceComponentType.Professional)
                {
                    componentPrice = component.Coefficient * factors.ProfessionalFactor;
                }

                totalPrice += componentPrice;
            }

            return totalPrice;
        }

        /// <summary>
        /// محاسبه با قیمت پایه (BasePrice)
        /// </summary>
        private decimal CalculateWithBasePrice(Service service, FactorPair factors)
        {
            // فرمول اصلی: BasePrice × K_Technical × K_Professional
            return service.Price * factors.TechnicalFactor * factors.ProfessionalFactor;
        }

        #endregion

        #region Factor Management

        /// <summary>
        /// دریافت ضرایب فعال
        /// </summary>
        private async Task<FactorPair> GetActiveFactorsAsync(bool isHashtagged, int financialYear)
        {
            try
            {
                var currentDate = DateTime.Now;

                // دریافت کای فنی
                var technicalFactor = await _context.FactorSettings
                    .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                               fs.IsHashtagged == isHashtagged &&
                               fs.FinancialYear == financialYear &&
                               fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                               fs.EffectiveFrom <= currentDate &&
                               (fs.EffectiveTo == null || fs.EffectiveTo >= currentDate))
                    .OrderByDescending(fs => fs.EffectiveFrom)
                    .FirstOrDefaultAsync();

                // دریافت کای حرفه‌ای
                var professionalFactor = await _context.FactorSettings
                    .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                               fs.IsHashtagged == isHashtagged &&
                               fs.FinancialYear == financialYear &&
                               fs.IsActive && !fs.IsDeleted && !fs.IsFrozen &&
                               fs.EffectiveFrom <= currentDate &&
                               (fs.EffectiveTo == null || fs.EffectiveTo >= currentDate))
                    .OrderByDescending(fs => fs.EffectiveFrom)
                    .FirstOrDefaultAsync();

                if (technicalFactor == null || professionalFactor == null)
                {
                    _logger.Warning("🏥 MEDICAL: ضرایب یافت نشد - IsHashtagged: {IsHashtagged}, FinancialYear: {FinancialYear}", 
                        isHashtagged, financialYear);
                    return null;
                }

                return new FactorPair
                {
                    TechnicalFactor = technicalFactor.Value,
                    ProfessionalFactor = professionalFactor.Value,
                    TechnicalFactorId = technicalFactor.FactorSettingId,
                    ProfessionalFactorId = professionalFactor.FactorSettingId
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت ضرایب - IsHashtagged: {IsHashtagged}, FinancialYear: {FinancialYear}", 
                    isHashtagged, financialYear);
                return null;
            }
        }

        #endregion

        #region Rule Engine

        /// <summary>
        /// اعمال Rule Engine برای exceptions
        /// </summary>
        private decimal ApplyRuleEngine(Service service, decimal calculatedPrice, FactorPair factors)
        {
            try
            {
                // Rule 1: Groups 1-7 (# hashed) - اعمال ضریب اضافی
                if (service.IsHashtagged)
                {
                    calculatedPrice = ApplyHashedRules(service, calculatedPrice, factors);
                }

                // Rule 2: Service-specific exceptions
                calculatedPrice = ApplyServiceSpecificRules(service, calculatedPrice);

                // Rule 3: Minimum/Maximum limits
                calculatedPrice = ApplyLimits(service, calculatedPrice);

                return calculatedPrice;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در Rule Engine - ServiceId: {ServiceId}", service.ServiceId);
                return calculatedPrice;
            }
        }

        /// <summary>
        /// اعمال قوانین هشتگ‌دار (Groups 1-7)
        /// </summary>
        private decimal ApplyHashedRules(Service service, decimal calculatedPrice, FactorPair factors)
        {
            // TODO: پیاده‌سازی قوانین خاص Groups 1-7
            // مثال: ضریب اضافی برای خدمات خاص
            
            return calculatedPrice;
        }

        /// <summary>
        /// اعمال قوانین خاص خدمت
        /// </summary>
        private decimal ApplyServiceSpecificRules(Service service, decimal calculatedPrice)
        {
            // TODO: پیاده‌سازی قوانین خاص هر خدمت
            // مثال: تخفیف برای خدمات خاص
            
            return calculatedPrice;
        }

        /// <summary>
        /// اعمال محدودیت‌های حداقل/حداکثر
        /// </summary>
        private decimal ApplyLimits(Service service, decimal calculatedPrice)
        {
            // حداقل قیمت: 1000 ریال
            if (calculatedPrice < 1000m)
            {
                calculatedPrice = 1000m;
            }

            // حداکثر قیمت: 100,000,000 ریال
            if (calculatedPrice > 100000000m)
            {
                calculatedPrice = 100000000m;
            }

            return calculatedPrice;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// جفت ضرایب فنی و حرفه‌ای
        /// </summary>
        public class FactorPair
        {
            public decimal TechnicalFactor { get; set; }
            public decimal ProfessionalFactor { get; set; }
            public int TechnicalFactorId { get; set; }
            public int ProfessionalFactorId { get; set; }
        }

        #endregion
    }
}
