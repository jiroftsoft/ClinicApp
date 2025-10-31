using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using ClinicApp.Services.Pricing.Interfaces;
using ClinicApp.Services.Pricing.Models;
using ClinicApp.Interfaces.Finance;
using ClinicApp.Models;
using ClinicApp.Models.Enums;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Pricing.Engines
{
    /// <summary>
    /// موتور محاسبه قیمت‌گذاری
    /// این Engine از TariffResolver و InsuranceCoverageProvider برای محاسبه دقیق سهم‌های بیمه استفاده می‌کند
    /// </summary>
    public class PricingEngine : IPricingEngine
    {
        private readonly ITariffResolver _tariff;
        private readonly IInsuranceCoverageProvider _coverage;
        private readonly IFinancialYearService _fyService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _log;

        public PricingEngine(
            ITariffResolver tariff,
            IInsuranceCoverageProvider coverage,
            IFinancialYearService fyService,
            ApplicationDbContext context,
            ILogger log)
        {
            _tariff = tariff ?? throw new ArgumentNullException(nameof(tariff));
            _coverage = coverage ?? throw new ArgumentNullException(nameof(coverage));
            _fyService = fyService ?? throw new ArgumentNullException(nameof(fyService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task<QuoteResultDto> QuoteAsync(QuoteRequestDto r, CancellationToken ct = default)
        {
            try
            {
                _log.Information("💰 PRICING: شروع پیش‌محاسبه - ServiceId: {ServiceId}, ClinicId: {ClinicId}, DeptId: {DeptId}, DoctorId: {DoctorId}",
                    r.ServiceId, r.ClinicId, r.DepartmentId, r.DoctorId);

                // 1) دریافت سال مالی فعال
                var fy = r.FinancialYearId ?? _fyService.GetCurrentYear();
                
                // 2) محاسبه تعرفه مصوب
                var approved = await _tariff.ResolveApprovedTariffAsync(r.ServiceId, r.ClinicId, r.DepartmentId, fy, ct);
                
                if (approved <= 0)
                {
                    _log.Warning("💰 PRICING: تعرفه مصوب نامعتبر است - ServiceId: {ServiceId}, ApprovedTariff: {ApprovedTariff}",
                        r.ServiceId, approved);
                    throw new InvalidOperationException($"تعرفه مصوب برای خدمت {r.ServiceId} نامعتبر است: {approved}");
                }

                long primaryPays = 0, suppPays = 0;
                long patientAfterPrimary = approved;

                // 3) PRIMARY: محاسبه سهم بیمه پایه
                CoverageRule primaryRule = CoverageRule.None();
                if (r.Primary?.InsurancePlanId.HasValue == true)
                {
                    primaryRule = await _coverage.GetPrimaryRuleAsync(
                        r.Primary.InsurancePlanId.Value,
                        r.ServiceId,
                        r.DepartmentId,
                        r.DoctorId,
                        fy,
                        ct);

                    if (primaryRule.IsCovered && approved > 0)
                    {
                        // اعمال سقف پایه (اگر وجود دارد)
                        var allowed = ApplyCap(approved, primaryRule.PerVisitCapIRR);
                        
                        // محاسبه سهم پایه (درصد × مبلغ مجاز)
                        primaryPays = Round(allowed * (primaryRule.CoveragePercent / 100m));
                        
                        // محدود کردن سهم پایه به مبلغ مجاز
                        primaryPays = Math.Min(primaryPays, allowed);
                        
                        patientAfterPrimary = approved - primaryPays;
                        if (patientAfterPrimary < 0)
                            patientAfterPrimary = 0;

                        _log.Information("✅ PRICING: سهم بیمه پایه محاسبه شد - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, Approved: {Approved}, PrimaryPays: {PrimaryPays}, PatientAfterPrimary: {PatientAfterPrimary}",
                            r.Primary.InsurancePlanId.Value, primaryRule.CoveragePercent, approved, primaryPays, patientAfterPrimary);
                    }
                    else
                    {
                        _log.Information("⚠️ PRICING: بیمه پایه پوشش ندارد - PlanId: {PlanId}, IsCovered: {IsCovered}",
                            r.Primary.InsurancePlanId.Value, primaryRule.IsCovered);
                    }
                }

                // 4) SUPPLEMENTARY: محاسبه سهم بیمه تکمیلی (از باقیمانده بعد از پایه)
                CoverageRule suppRule = CoverageRule.None();
                if (r.Supplementary?.InsurancePlanId.HasValue == true && patientAfterPrimary > 0)
                {
                    suppRule = await _coverage.GetSupplementaryRuleAsync(
                        r.Supplementary.InsurancePlanId.Value,
                        r.ServiceId,
                        r.DepartmentId,
                        r.DoctorId,
                        fy,
                        ct);

                    if (suppRule.IsCovered && patientAfterPrimary > 0)
                    {
                        // اعمال سقف تکمیلی (اگر وجود دارد)
                        var allowed = ApplyCap(patientAfterPrimary, suppRule.PerVisitCapIRR);
                        
                        // محاسبه سهم تکمیلی (درصد × مبلغ مجاز)
                        var suppByPercent = Round(allowed * (suppRule.CoveragePercent / 100m));
                        
                        // محدود کردن سهم تکمیلی به مبلغ باقیمانده
                        suppPays = Math.Min(patientAfterPrimary, suppByPercent);
                        
                        // محدود کردن به سقف تکمیلی (اگر وجود دارد)
                        if (suppRule.PerVisitCapIRR.HasValue)
                        {
                            suppPays = Math.Min(suppPays, suppRule.PerVisitCapIRR.Value);
                        }

                        _log.Information("✅ PRICING: سهم بیمه تکمیلی محاسبه شد - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}%, PatientAfterPrimary: {PatientAfterPrimary}, SuppPays: {SuppPays}",
                            r.Supplementary.InsurancePlanId.Value, suppRule.CoveragePercent, patientAfterPrimary, suppPays);
                    }
                    else
                    {
                        _log.Information("⚠️ PRICING: بیمه تکمیلی پوشش ندارد یا باقیمانده صفر است - PlanId: {PlanId}, IsCovered: {IsCovered}, PatientAfterPrimary: {PatientAfterPrimary}",
                            r.Supplementary.InsurancePlanId.Value, suppRule.IsCovered, patientAfterPrimary);
                    }
                }

                // 5) محاسبه سهم بیمار نهایی
                var patientFinal = approved - primaryPays - suppPays;
                if (patientFinal < 0)
                    patientFinal = 0;

                // 6) ایجاد نتیجه
                var result = new QuoteResultDto
                {
                    ServiceId = r.ServiceId,
                    ApprovedTariff = approved,
                    Primary = new CoverageBreakdownDto
                    {
                        PlanId = r.Primary?.InsurancePlanId,
                        IsCovered = primaryRule.IsCovered,
                        CoveragePercent = primaryRule.CoveragePercent,
                        CapApplied = primaryRule.PerVisitCapIRR.HasValue,
                        CapValue = primaryRule.PerVisitCapIRR,
                        Pays = primaryPays,
                        CoverageRuleName = primaryRule.RuleName,
                        CoveragePercentStr = $"{primaryRule.CoveragePercent:F1}%",
                        CapValueStr = primaryRule.PerVisitCapIRR.HasValue ? ((decimal)primaryRule.PerVisitCapIRR.Value).ToIrrString() : "—",
                        PaysStr = ((decimal)primaryPays).ToIrrString()
                    },
                    Supplementary = new CoverageBreakdownDto
                    {
                        PlanId = r.Supplementary?.InsurancePlanId,
                        IsCovered = suppRule.IsCovered,
                        CoveragePercent = suppRule.CoveragePercent,
                        CapApplied = suppRule.PerVisitCapIRR.HasValue,
                        CapValue = suppRule.PerVisitCapIRR,
                        Pays = suppPays,
                        CoverageRuleName = suppRule.RuleName,
                        CoveragePercentStr = $"{suppRule.CoveragePercent:F1}%",
                        CapValueStr = suppRule.PerVisitCapIRR.HasValue ? ((decimal)suppRule.PerVisitCapIRR.Value).ToIrrString() : "—",
                        PaysStr = ((decimal)suppPays).ToIrrString()
                    },
                    PatientInitialCoinsurance = approved - primaryPays,
                    PatientFinal = patientFinal,
                    RoundingPolicy = "AwayFromZero",
                    ApprovedTariffStr = ((decimal)approved).ToIrrString(),
                    PatientInitialCoinsuranceStr = ((decimal)(approved - primaryPays)).ToIrrString(),
                    PatientFinalStr = ((decimal)patientFinal).ToIrrString()
                };

                result.Notes.Add($"سال مالی: {fy}");
                if (primaryRule.IsCovered)
                    result.Notes.Add($"پوشش پایه: {primaryRule.CoveragePercent:F1}%");
                if (suppRule.IsCovered)
                    result.Notes.Add($"پوشش تکمیلی: {suppRule.CoveragePercent:F1}%");
                if (primaryRule.PerVisitCapIRR.HasValue)
                    result.Notes.Add($"سقف پایه: {((decimal)primaryRule.PerVisitCapIRR.Value).ToIrrString()}");
                if (suppRule.PerVisitCapIRR.HasValue)
                    result.Notes.Add($"سقف تکمیلی: {((decimal)suppRule.PerVisitCapIRR.Value).ToIrrString()}");

                _log.Information("✅ PRICING: پیش‌محاسبه تکمیل شد - ServiceId: {ServiceId}, Approved: {Approved}, Primary: {Primary}, Supp: {Supp}, Patient: {Patient}",
                    r.ServiceId, approved, primaryPays, suppPays, patientFinal);

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ PRICING: خطا در پیش‌محاسبه - ServiceId: {ServiceId}, ClinicId: {ClinicId}, DeptId: {DeptId}",
                    r?.ServiceId, r?.ClinicId, r?.DepartmentId);
                throw;
            }
        }

        public async Task RepriceReceptionAsync(int receptionId, CancellationToken ct = default)
        {
            try
            {
                _log.Information("💰 PRICING: شروع محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);

                // دریافت Reception با آیتم‌ها
                var reception = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && r.Status == ReceptionStatus.Pending);

                if (reception == null)
                {
                    _log.Warning("💰 PRICING: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    throw new InvalidOperationException($"پذیرش {receptionId} یافت نشد");
                }

                var fy = reception.FinancialYear;
                if (fy == 0)
                {
                    _log.Warning("⚠️ PRICING: سال مالی برای پذیرش {ReceptionId} یافت نشد. استفاده از سال مالی فعال.", receptionId);
                    fy = _fyService.GetCurrentYear();
                    if (fy == 0)
                    {
                        _log.Error("❌ PRICING: سال مالی فعال یافت نشد. محاسبه مجدد ناموفق.");
                        throw new InvalidOperationException("سال مالی فعال یافت نشد.");
                    }
                    reception.FinancialYear = fy; // به‌روزرسانی پذیرش با سال مالی فعال
                }
                
                // محاسبه مجدد هر آیتم
                foreach (var item in reception.ReceptionItems.Where(i => !i.IsDeleted))
                {
                    try
                    {
                        // دریافت اطلاعات خدمت
                        var service = await _context.Services
                            .AsNoTracking()
                            .Where(s => s.ServiceId == item.ServiceId && !s.IsDeleted && s.IsActive)
                            .FirstOrDefaultAsync(ct);

                        if (service == null)
                            continue;

                        // محاسبه تعرفه مصوب
                        var approved = await _tariff.ResolveApprovedTariffAsync(
                            item.ServiceId,
                            reception.ClinicId,
                            reception.DepartmentId,
                            fy,
                            ct);

                        if (approved <= 0)
                            continue;

                        var total = approved * item.Quantity;

                        // محاسبه سهم‌ها
                        long primaryPays = 0, suppPays = 0;
                        decimal primaryCoveragePercent = 0m, suppCoveragePercent = 0m;
                        CoverageRule primaryRule = CoverageRule.None();
                        CoverageRule suppRule = CoverageRule.None();

                        if (reception.BasePlanId.HasValue)
                        {
                            primaryRule = await _coverage.GetPrimaryRuleAsync(
                                reception.BasePlanId.Value,
                                item.ServiceId,
                                reception.DepartmentId,
                                reception.DoctorId,
                                fy,
                                ct);
                            primaryCoveragePercent = primaryRule.CoveragePercent;

                            if (primaryRule.IsCovered)
                            {
                                // اعمال سقف هر واحد (PerVisitCapIRR) و سپس ضرب در تعداد
                                var unitAllowed = ApplyCap(approved, primaryRule.PerVisitCapIRR);
                                var unitPrimaryPays = Round(unitAllowed * (primaryRule.CoveragePercent / 100m));
                                primaryPays = unitPrimaryPays * item.Quantity;
                                primaryPays = Math.Min(primaryPays, total);
                            }
                        }

                        var patientAfterPrimary = total - primaryPays;
                        if (patientAfterPrimary < 0)
                            patientAfterPrimary = 0;

                        if (reception.SupplementaryPlanId.HasValue && patientAfterPrimary > 0)
                        {
                            suppRule = await _coverage.GetSupplementaryRuleAsync(
                                reception.SupplementaryPlanId.Value,
                                item.ServiceId,
                                reception.DepartmentId,
                                reception.DoctorId,
                                fy,
                                ct);
                            suppCoveragePercent = suppRule.CoveragePercent;

                            if (suppRule.IsCovered)
                            {
                                // اعمال سقف هر واحد (PerVisitCapIRR) و سپس ضرب در تعداد
                                var unitAllowedAfterPrimary = patientAfterPrimary / item.Quantity;
                                var unitAllowed = ApplyCap(unitAllowedAfterPrimary, suppRule.PerVisitCapIRR);
                                var unitSuppPays = Round(unitAllowed * (suppRule.CoveragePercent / 100m));
                                suppPays = Math.Min(unitSuppPays * item.Quantity, patientAfterPrimary);
                            }
                        }

                        var patientFinal = total - primaryPays - suppPays;
                        if (patientFinal < 0)
                            patientFinal = 0;

                        // به‌روزرسانی آیتم
                        item.UnitPrice = approved;
                        item.PatientShareAmount = patientFinal;
                        item.InsurerShareAmount = primaryPays + suppPays;

                        // به‌روزرسانی SnapshotJson با مقادیر واقعی از CoverageRule
                        try
                        {
                            var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson ?? "{}");
                            if (snapshot != null)
                            {
                                snapshot.UnitPrice = approved;
                                snapshot.GrossAmount = total;
                                snapshot.BaseInsuranceCoverage = primaryCoveragePercent;
                                snapshot.SupplementaryCoverage = suppCoveragePercent;
                                snapshot.PatientShare = patientFinal;
                                snapshot.InsurerShare = primaryPays + suppPays;
                                snapshot.PrimaryPays = primaryPays;
                                snapshot.SupplementaryPays = suppPays;
                                snapshot.BasePlanId = reception.BasePlanId;
                                snapshot.SupplementaryPlanId = reception.SupplementaryPlanId;
                                snapshot.RepricedAt = DateTime.Now;
                                
                                item.SnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(snapshot);
                            }
                        }
                        catch (Exception snapEx)
                        {
                            _log.Warning(snapEx, "⚠️ PRICING: خطا در به‌روزرسانی SnapshotJson هنگام Reprice - ItemId: {ItemId}", item.ReceptionItemId);
                        }
                    }
                    catch (Exception itemEx)
                    {
                        _log.Error(itemEx, "⚠️ PRICING: خطا در محاسبه مجدد آیتم - ReceptionId: {ReceptionId}, ItemId: {ItemId}",
                            receptionId, item.ReceptionItemId);
                    }
                }

                await _context.SaveChangesAsync(ct);

                // بازمحاسبه مجموع‌ها
                await RecalculateReceptionTotalsAsync(receptionId, ct);

                _log.Information("✅ PRICING: محاسبه مجدد پذیرش تکمیل شد - ReceptionId: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ PRICING: خطا در محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);
                throw;
            }
        }

        /// <summary>
        /// بازمحاسبه مجموع‌های پذیرش
        /// </summary>
        private async Task RecalculateReceptionTotalsAsync(int receptionId, CancellationToken ct)
        {
            var reception = await _context.Receptions
                .Include(r => r.ReceptionItems)
                .FirstOrDefaultAsync(r => r.ReceptionId == receptionId, ct);

            if (reception == null)
                return;

            var items = reception.ReceptionItems.Where(i => !i.IsDeleted).ToList();
            
            reception.TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity);
            reception.InsurerShareAmount = items.Sum(i => i.InsurerShareAmount);
            reception.PatientCoPay = items.Sum(i => i.PatientShareAmount);

            await _context.SaveChangesAsync(ct);
        }

        private static long Round(decimal v)
            => (long)Math.Round(v, 0, MidpointRounding.AwayFromZero);

        private static long ApplyCap(long amount, long? cap)
        {
            if (cap.HasValue && cap.Value >= 0)
                return Math.Min(amount, cap.Value);
            return amount;
        }
    }
}
