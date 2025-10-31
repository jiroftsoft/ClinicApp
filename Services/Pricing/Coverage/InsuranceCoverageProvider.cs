using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Services.Pricing.Interfaces;
using ClinicApp.Services.Pricing.Models;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.Services.Pricing.Coverage
{
    /// <summary>
    /// Provider قواعد پوشش بیمه
    /// این پیاده‌سازی از InsuranceTariff و InsurancePlan برای دریافت قواعد پوشش استفاده می‌کند
    /// </summary>
    public class InsuranceCoverageProvider : IInsuranceCoverageProvider
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger _log;

        public InsuranceCoverageProvider(ApplicationDbContext db, ILogger log)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<CoverageRule> GetPrimaryRuleAsync(int insurancePlanId, int serviceId, int departmentId, int doctorId, int fy, CancellationToken ct = default)
        {
            return await GetRuleCoreAsync(insurancePlanId, serviceId, departmentId, doctorId, fy, isSupplementary: false, ct);
        }

        public async Task<CoverageRule> GetSupplementaryRuleAsync(int insurancePlanId, int serviceId, int departmentId, int doctorId, int fy, CancellationToken ct = default)
        {
            return await GetRuleCoreAsync(insurancePlanId, serviceId, departmentId, doctorId, fy, isSupplementary: true, ct);
        }

        private async Task<CoverageRule> GetRuleCoreAsync(int planId, int serviceId, int departmentId, int doctorId, int fy, bool isSupplementary, CancellationToken ct)
        {
            try
            {
                _log.Debug("🏥 PRICING: دریافت قاعده پوشش - PlanId: {PlanId}, ServiceId: {ServiceId}, IsSupplementary: {IsSupplementary}",
                    planId, serviceId, isSupplementary);

                // 1) دریافت InsuranceTariff برای این خدمت و طرح
                var tariff = await _db.InsuranceTariffs
                    .AsNoTracking()
                    .Where(t => t.InsurancePlanId == planId &&
                               t.ServiceId == serviceId &&
                               !t.IsDeleted &&
                               t.IsActive &&
                               t.InsuranceType == (isSupplementary ? InsuranceType.Supplementary : InsuranceType.Primary))
                    .FirstOrDefaultAsync(ct);

                if (tariff != null)
                {
                    // ✅ اگر InsuranceTariff موجود است، از آن استفاده کن
                    var coveragePercent = isSupplementary && tariff.SupplementaryCoveragePercent.HasValue
                        ? tariff.SupplementaryCoveragePercent.Value
                        : (tariff.PatientShare.HasValue || tariff.InsurerShare.HasValue)
                            ? CalculateCoverageFromShares(tariff.PatientShare, tariff.InsurerShare)
                            : await GetCoverageFromPlanAsync(planId, ct) ?? 0m;

                    var capValue = isSupplementary && tariff.SupplementaryMaxPayment.HasValue
                        ? (long?)tariff.SupplementaryMaxPayment.Value
                        : null;

                    _log.Information("✅ PRICING: قاعده پوشش از InsuranceTariff - PlanId: {PlanId}, ServiceId: {ServiceId}, CoveragePercent: {CoveragePercent}, Cap: {Cap}",
                        planId, serviceId, coveragePercent, capValue);

                    return new CoverageRule
                    {
                        IsCovered = coveragePercent > 0,
                        CoveragePercent = coveragePercent,
                        PerVisitCapIRR = capValue,
                        RuleName = $"تعرفه {tariff.InsuranceTariffId}"
                    };
                }

                // 2) Fallback: دریافت پوشش پیش‌فرض از InsurancePlan
                var plan = await _db.InsurancePlans
                    .AsNoTracking()
                    .Where(p => p.InsurancePlanId == planId && !p.IsDeleted && p.IsActive)
                    .Select(p => new
                    {
                        p.CoveragePercent,
                        p.Deductible,
                        p.InsuranceType
                    })
                    .FirstOrDefaultAsync(ct);

                if (plan == null)
                {
                    _log.Warning("⚠️ PRICING: طرح بیمه یافت نشد - PlanId: {PlanId}", planId);
                    return CoverageRule.None();
                }

                // ✅ اگر طرح بیمه تکمیلی نیست، پوشش صفر است
                if (isSupplementary && plan.InsuranceType != InsuranceType.Supplementary)
                {
                    _log.Warning("⚠️ PRICING: طرح بیمه تکمیلی نیست - PlanId: {PlanId}, Type: {Type}", planId, plan.InsuranceType);
                    return CoverageRule.None();
                }

                _log.Information("✅ PRICING: قاعده پوشش از InsurancePlan - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}",
                    planId, plan.CoveragePercent);

                return new CoverageRule
                {
                    IsCovered = plan.CoveragePercent > 0,
                    CoveragePercent = plan.CoveragePercent,
                    PerVisitCapIRR = null, // InsurancePlan سقف ندارد (باید در InsuranceTariff تعریف شود)
                    RuleName = "پوشش پیش‌فرض طرح بیمه"
                };
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ PRICING: خطا در دریافت قاعده پوشش - PlanId: {PlanId}, ServiceId: {ServiceId}, IsSupplementary: {IsSupplementary}",
                    planId, serviceId, isSupplementary);
                return CoverageRule.None();
            }
        }

        /// <summary>
        /// محاسبه درصد پوشش از سهم‌های بیمار و بیمه
        /// </summary>
        private decimal CalculateCoverageFromShares(decimal? patientShare, decimal? insurerShare)
        {
            if (!patientShare.HasValue && !insurerShare.HasValue)
                return 0m;

            var total = (patientShare ?? 0m) + (insurerShare ?? 0m);
            if (total <= 0)
                return 0m;

            var coverage = (insurerShare ?? 0m) / total * 100m;
            return Math.Round(coverage, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// دریافت درصد پوشش پیش‌فرض از طرح بیمه
        /// </summary>
        private async Task<decimal?> GetCoverageFromPlanAsync(int planId, CancellationToken ct)
        {
            var plan = await _db.InsurancePlans
                .AsNoTracking()
                .Where(p => p.InsurancePlanId == planId && !p.IsDeleted && p.IsActive)
                .Select(p => (decimal?)p.CoveragePercent)
                .FirstOrDefaultAsync(ct);

            return plan;
        }
    }
}
