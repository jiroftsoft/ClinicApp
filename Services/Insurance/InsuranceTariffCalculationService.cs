using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس محاسبه تعرفه بیمه - جداسازی منطق محاسبه از Controller
    /// </summary>
    public class InsuranceTariffCalculationService : IInsuranceTariffCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFactorSettingService _factorSettingService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ILogger _logger;

        public InsuranceTariffCalculationService(
            ApplicationDbContext context,
            IFactorSettingService factorSettingService,
            IServiceCalculationService serviceCalculationService,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// محاسبه قیمت تعرفه با استفاده از FactorSetting - منطق یکسان
        /// </summary>
        public async Task<decimal> CalculateTariffPriceWithFactorSettingAsync(int serviceId, decimal? currentTariffPrice, string correlationId)
        {
            try
            {
                // اگر قیمت فعلی موجود است، از آن استفاده کن
                if (currentTariffPrice.HasValue && currentTariffPrice.Value > 0)
                {
                    _logger.Debug("🏥 MEDICAL: استفاده از قیمت تعرفه موجود - Price: {Price}, CorrelationId: {CorrelationId}",
                        currentTariffPrice.Value, correlationId);
                    return currentTariffPrice.Value;
                }

                // دریافت سال مالی فعلی
                var currentFinancialYear = await GetCurrentFinancialYearAsync(DateTime.Now);
                
                // دریافت خدمت از دیتابیس
                var service = await _context.Services
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                    return 0m;
                }

                // دریافت کای فنی
                var technicalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ServiceComponentType.Technical, service.IsHashtagged, currentFinancialYear);

                // دریافت کای حرفه‌ای
                var professionalFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                    ServiceComponentType.Professional, false, currentFinancialYear);

                if (technicalFactor == null || professionalFactor == null)
                {
                    _logger.Warning("🏥 MEDICAL: کای‌های مورد نیاز یافت نشد - TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, CorrelationId: {CorrelationId}",
                        technicalFactor != null, professionalFactor != null, correlationId);
                    
                    // Fallback به قیمت پایه خدمت
                    return service.Price;
                }

                // 🚀 FINANCIAL PRECISION: استفاده از منطق یکسان ServiceCalculationService
                decimal calculatedPrice;
                try
                {
                    // استفاده از ServiceCalculationService برای منطق یکسان
                    if (_serviceCalculationService != null)
                    {
                        calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                            service, _context, DateTime.Now);
                        
                        _logger.Debug("🏥 MEDICAL: محاسبه قیمت تعرفه با ServiceCalculationService - ServiceId: {ServiceId}, Result: {Result}, CorrelationId: {CorrelationId}",
                            serviceId, calculatedPrice, correlationId);
                    }
                    else
                    {
                        // Fallback به منطق قدیمی (ضرب)
                        var basePrice = service.Price;
                        calculatedPrice = basePrice * technicalFactor.Value * professionalFactor.Value;
                        
                        _logger.Debug("🏥 MEDICAL: محاسبه قیمت تعرفه با منطق Fallback - BasePrice: {BasePrice}, TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, Result: {Result}, CorrelationId: {CorrelationId}",
                            basePrice, technicalFactor.Value, professionalFactor.Value, calculatedPrice, correlationId);
                    }
                }
                catch (Exception calcEx)
                {
                    _logger.Warning(calcEx, "🏥 MEDICAL: خطا در ServiceCalculationService، استفاده از منطق Fallback - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                        serviceId, correlationId);
                    
                    // Fallback به منطق قدیمی
                    var basePrice = service.Price;
                    calculatedPrice = basePrice * technicalFactor.Value * professionalFactor.Value;
                }

                // 🚀 FINANCIAL PRECISION: گرد کردن به ریال (بدون اعشار)
                return Math.Round(calculatedPrice, 0, MidpointRounding.AwayFromZero);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت تعرفه - ServiceId: {ServiceId}, CorrelationId: {CorrelationId}", 
                    serviceId, correlationId);
                return 0m;
            }
        }

        /// <summary>
        /// محاسبه مبلغ پایه خدمت بر اساس ServiceComponents و FactorSettings
        /// </summary>
        public async Task<decimal> CalculateServiceBasePriceAsync(int serviceId)
        {
            try
            {
                _logger.Debug("🏥 MEDICAL: شروع محاسبه مبلغ پایه خدمت. ServiceId: {ServiceId}", serviceId);

                // دریافت اطلاعات خدمت از دیتابیس
                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}", serviceId);
                    return 0m;
                }

                // استفاده از ServiceCalculationService برای محاسبه دقیق
                try
                {
                    // ابتدا سعی کنیم از FactorSettings استفاده کنیم
                    if (_serviceCalculationService != null)
                    {
                        var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                            service, _context, DateTime.Now);

                        _logger.Information("🏥 MEDICAL: محاسبه مبلغ پایه با FactorSettings موفق. ServiceId: {ServiceId}, CalculatedPrice: {CalculatedPrice}", 
                            serviceId, calculatedPrice);

                        return calculatedPrice;
                    }
                }
                catch (Exception factorEx)
                {
                    _logger.Warning(factorEx, "🏥 MEDICAL: خطا در محاسبه با FactorSettings، استفاده از روش پایه. ServiceId: {ServiceId}", serviceId);
                }

                // اگر FactorSettings موجود نباشد، از دیتابیس ضرایب را بخوان
                if (service.ServiceComponents != null && service.ServiceComponents.Any())
                {
                    var technicalComponent = service.ServiceComponents
                        .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
                    var professionalComponent = service.ServiceComponents
                        .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                    if (technicalComponent != null && professionalComponent != null)
                    {
                        // دریافت ضرایب از دیتابیس - بدون هاردکد
                        var currentFinancialYear = await GetCurrentFinancialYearAsync(DateTime.Now);
                        
                        // دریافت ضریب فنی از دیتابیس
                        var technicalFactor = await _context.FactorSettings
                            .Where(fs => fs.FactorType == ServiceComponentType.Technical &&
                                        fs.IsHashtagged == service.IsHashtagged &&
                                        fs.FinancialYear == currentFinancialYear &&
                                        fs.IsActive && !fs.IsDeleted &&
                                        !fs.IsFrozen &&
                                        fs.EffectiveFrom <= DateTime.Now &&
                                        (fs.EffectiveTo == null || fs.EffectiveTo >= DateTime.Now))
                            .OrderByDescending(fs => fs.EffectiveFrom)
                            .Select(fs => fs.Value)
                            .FirstOrDefaultAsync();

                        // دریافت ضریب حرفه‌ای از دیتابیس
                        // ✅ FIX: ضریب حرفه‌ای بر اساس نوع خدمت (هشتگ‌دار: 770k، عادی: 1.37M)
                        var professionalFactor = await _context.FactorSettings
                            .Where(fs => fs.FactorType == ServiceComponentType.Professional &&
                                        fs.IsHashtagged == service.IsHashtagged && // ✅ بر اساس نوع خدمت
                                        fs.FinancialYear == currentFinancialYear &&
                                        fs.IsActive && !fs.IsDeleted &&
                                        !fs.IsFrozen &&
                                        fs.EffectiveFrom <= DateTime.Now &&
                                        (fs.EffectiveTo == null || fs.EffectiveTo >= DateTime.Now))
                            .OrderByDescending(fs => fs.EffectiveFrom)
                            .Select(fs => fs.Value)
                            .FirstOrDefaultAsync();

                        if (technicalFactor > 0 && professionalFactor > 0)
                        {
                            // 🚀 FINANCIAL PRECISION: استفاده از منطق یکسان (ضرب به جای جمع)
                            var calculatedPrice = service.Price * technicalFactor * professionalFactor;

                            _logger.Information("🏥 MEDICAL: محاسبه مبلغ پایه با ضرایب دیتابیس. ServiceId: {ServiceId}, TechnicalCoeff: {TechnicalCoeff}, ProfessionalCoeff: {ProfessionalCoeff}, TechnicalFactor: {TechnicalFactor}, ProfessionalFactor: {ProfessionalFactor}, CalculatedPrice: {CalculatedPrice}", 
                                serviceId, technicalComponent.Coefficient, professionalComponent.Coefficient, technicalFactor, professionalFactor, calculatedPrice);

                            return calculatedPrice;
                        }
                    }
                }

                // Fallback به قیمت پایه خدمت
                _logger.Warning("🏥 MEDICAL: استفاده از قیمت پایه خدمت - ServiceId: {ServiceId}, Price: {Price}", serviceId, service.Price);
                return service.Price;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه مبلغ پایه خدمت - ServiceId: {ServiceId}", serviceId);
                return 0m;
            }
        }

        /// <summary>
        /// بررسی تکراری بودن تعرفه
        /// </summary>
        public async Task<bool> IsTariffDuplicateAsync(int insurancePlanId, int? serviceId, bool isAllServices)
        {
            try
            {
                // بررسی duplicate فقط برای تعرفه‌های تکی (نه همه خدمات)
                if (isAllServices || !serviceId.HasValue || serviceId.Value <= 0)
                {
                    return false; // برای "همه خدمات" duplicate check نمی‌کنیم
                }

                var existingTariff = await _context.InsuranceTariffs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.InsurancePlanId == insurancePlanId 
                                           && t.ServiceId == serviceId 
                                           && !t.IsDeleted);

                return existingTariff != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی duplicate تعرفه - PlanId: {PlanId}, ServiceId: {ServiceId}", 
                    insurancePlanId, serviceId);
                return false; // در صورت خطا، اجازه ادامه
            }
        }

        /// <summary>
        /// دریافت سال مالی فعلی
        /// </summary>
        private async Task<int> GetCurrentFinancialYearAsync(DateTime date)
        {
            // منطق سال مالی: از 21 مارس شروع می‌شود
            var financialYear = date.Year;
            if (date.Month >= 3 && date.Day >= 21)
            {
                financialYear = date.Year;
            }
            else if (date.Month >= 3)
            {
                financialYear = date.Year;
            }
            else
            {
                financialYear = date.Year - 1;
            }

            return financialYear;
        }
    }
}
