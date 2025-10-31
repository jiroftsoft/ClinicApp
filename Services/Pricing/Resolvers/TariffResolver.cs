using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Services.Pricing.Interfaces;
using ClinicApp.Services.Insurance;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Services.Pricing.Resolvers
{
    /// <summary>
    /// Resolver تعرفه خدمت
    /// دو مسیر: (الف) اگر Service.Price ست باشد، همان تعرفه مصوب است؛ (ب) در غیر این‌صورت از ServiceComponent × FactorSetting محاسبه می‌کنیم.
    /// </summary>
    public class TariffResolver : ITariffResolver
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger _log;
        private readonly ServiceCalculationEngine _serviceCalculationEngine;

        public TariffResolver(
            ApplicationDbContext db,
            ILogger log,
            ServiceCalculationEngine serviceCalculationEngine)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _serviceCalculationEngine = serviceCalculationEngine ?? throw new ArgumentNullException(nameof(serviceCalculationEngine));
        }

        public async Task<long> ResolveApprovedTariffAsync(int serviceId, int clinicId, int departmentId, int financialYearId, CancellationToken ct = default)
        {
            try
            {
                _log.Debug("💰 PRICING: شروع محاسبه تعرفه مصوب - ServiceId: {ServiceId}, ClinicId: {ClinicId}, DeptId: {DeptId}, FY: {FinancialYear}",
                    serviceId, clinicId, departmentId, financialYearId);

                // 1) دریافت اطلاعات خدمت
                var service = await _db.Services
                    .AsNoTracking()
                    .Where(s => s.ServiceId == serviceId && !s.IsDeleted && s.IsActive)
                    .Select(s => new
                    {
                        s.ServiceId,
                        s.Price,
                        s.IsHashtagged,
                        s.GroupCode
                    })
                    .FirstOrDefaultAsync(ct);

                if (service == null)
                {
                    _log.Warning("💰 PRICING: خدمت یافت نشد - ServiceId: {ServiceId}", serviceId);
                    throw new InvalidOperationException($"Service {serviceId} not found");
                }

                // 2) مسیر ساده: اگر Service.Price موجود و > 0 باشد، همان تعرفه مصوب است
                if (service.Price > 0)
                {
                    var approved = (long)Math.Round(service.Price, 0, MidpointRounding.AwayFromZero);
                    _log.Information("✅ PRICING: تعرفه مصوب از Service.Price - ServiceId: {ServiceId}, ApprovedTariff: {ApprovedTariff}",
                        serviceId, approved);
                    return approved;
                }

                // 3) مسیر محاسبه: از ServiceCalculationEngine استفاده کن
                var unitPrice = await _serviceCalculationEngine.CalculateUnitPriceIRRAsync(serviceId, financialYearId);
                
                if (unitPrice <= 0)
                {
                    _log.Warning("💰 PRICING: قیمت محاسبه شده نامعتبر است - ServiceId: {ServiceId}, UnitPrice: {UnitPrice}, FY: {FinancialYear}",
                        serviceId, unitPrice, financialYearId);
                    throw new InvalidOperationException($"Calculated unit price is invalid: {unitPrice} for Service {serviceId}");
                }

                var approvedCalculated = (long)Math.Round(unitPrice, 0, MidpointRounding.AwayFromZero);
                _log.Information("✅ PRICING: تعرفه مصوب محاسبه شده - ServiceId: {ServiceId}, ApprovedTariff: {ApprovedTariff}, FY: {FinancialYear}",
                    serviceId, approvedCalculated, financialYearId);
                return approvedCalculated;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ PRICING: خطا در محاسبه تعرفه مصوب - ServiceId: {ServiceId}, ClinicId: {ClinicId}, DeptId: {DeptId}, FY: {FinancialYear}",
                    serviceId, clinicId, departmentId, financialYearId);
                throw;
            }
        }
    }
}
