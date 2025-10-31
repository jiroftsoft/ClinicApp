using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Generic;
using ClinicApp.Controllers.Api;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Extensions;
using ClinicApp.Services.Pricing.Interfaces;
using ClinicApp.Services.Pricing.Models;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// ✅ سرویس محاسبه قیمت‌گذاری پذیرش
    /// Thin-Wrapper برای PricingEngine به منظور ارائه خروجی متحدالشکل برای UI
    /// </summary>
    public class ReceptionPricingService : IReceptionPricingService
    {
        private readonly IPricingEngine _pricingEngine;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly IFactorSettingService _factorSettingService;

        public ReceptionPricingService(
            IPricingEngine pricingEngine,
            ApplicationDbContext context,
            ILogger logger,
            IFactorSettingService factorSettingService)
        {
            _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<ReceptionPricingService>();
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
        }

        /// <summary>
        /// محاسبه جزئیات قیمت یک آیتم
        /// </summary>
        public async Task<PricingBreakdownDto> PriceItemAsync(int receptionId, int receptionItemId)
        {
            try
            {
                _logger.Information("💰 PRICING SERVICE: محاسبه جزئیات قیمت - ReceptionId: {ReceptionId}, ReceptionItemId: {ReceptionItemId}", 
                    receptionId, receptionItemId);

                // دریافت ReceptionItem با Reception
                var item = await _context.ReceptionItems
                    .Include(i => i.Reception)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.ReceptionItemId == receptionItemId && 
                                             i.ReceptionId == receptionId && 
                                             !i.IsDeleted);

                if (item == null)
                {
                    _logger.Warning("⚠️ PRICING SERVICE: آیتم یافت نشد - ReceptionId: {ReceptionId}, ReceptionItemId: {ReceptionItemId}", 
                        receptionId, receptionItemId);
                    throw new InvalidOperationException($"آیتم با شناسه {receptionItemId} یافت نشد");
                }

                var reception = item.Reception;
                if (reception == null)
                {
                    _logger.Warning("⚠️ PRICING SERVICE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    throw new InvalidOperationException($"پذیرش با شناسه {receptionId} یافت نشد");
                }

                // استفاده از SnapshotJson برای محاسبات
                long unitPrice = (long)item.UnitPrice;
                long gross = unitPrice * item.Quantity;
                long baseCovered = 0;
                long suppCovered = 0;
                long patientPayable = (long)item.PatientShareAmount;

                // تلاش برای استخراج سهم‌ها از SnapshotJson
                if (!string.IsNullOrEmpty(item.SnapshotJson))
                {
                    try
                    {
                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
                        if (snapshot != null)
                        {
                            if (snapshot.PrimaryPays != null)
                                baseCovered = (long)snapshot.PrimaryPays;
                            if (snapshot.SupplementaryPays != null)
                                suppCovered = (long)snapshot.SupplementaryPays;
                            if (snapshot.PatientShare != null)
                                patientPayable = (long)snapshot.PatientShare;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "⚠️ PRICING SERVICE: خطا در parse کردن SnapshotJson - ReceptionItemId: {ReceptionItemId}", 
                            receptionItemId);
                    }
                }

                // اگر SnapshotJson خالی است یا مقدار ندارد، از مقادیر entity استفاده کن
                if (baseCovered == 0 && suppCovered == 0)
                {
                    var insurerShare = (long)item.InsurerShareAmount;
                    // برآورد: اگر BasePlanId وجود دارد، احتمالاً بخشی از InsurerShare مربوط به Base است
                    if (reception.BasePlanId.HasValue && reception.SupplementaryPlanId.HasValue)
                    {
                        // تقسیم 50/50 (یا می‌توانیم از QuoteAsync استفاده کنیم برای محاسبه دقیق‌تر)
                        baseCovered = insurerShare / 2;
                        suppCovered = insurerShare - baseCovered;
                    }
                    else if (reception.BasePlanId.HasValue)
                    {
                        baseCovered = insurerShare;
                    }
                    else if (reception.SupplementaryPlanId.HasValue)
                    {
                        suppCovered = insurerShare;
                    }
                }

                // ایجاد Notes از SnapshotJson
                var notes = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(item.SnapshotJson))
                {
                    try
                    {
                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
                        if (snapshot != null)
                        {
                            if (snapshot.BaseInsuranceCoverage != null)
                                notes.Add($"پوشش پایه: {snapshot.BaseInsuranceCoverage:F1}%");
                            if (snapshot.SupplementaryCoverage != null)
                                notes.Add($"پوشش تکمیلی: {snapshot.SupplementaryCoverage:F1}%");
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                // ✅ ساخت CoverageDetails برای UI (badge + highlight + modal)
                var coverageDetails = BuildCoverageDetails(
                    gross: gross,
                    baseCovered: baseCovered,
                    suppCovered: suppCovered,
                    patientPayable: patientPayable,
                    reception: reception,
                    snapshotJson: item.SnapshotJson
                );

                var result = new PricingBreakdownDto
                {
                    ReceptionItemId = item.ReceptionItemId,  // ✅ اضافه شده برای RepriceAll
                    ServiceId = item.ServiceId,
                    Quantity = item.Quantity,
                    UnitPriceIRR = unitPrice,
                    GrossIRR = gross,
                    BaseCoveredIRR = baseCovered,
                    SuppCoveredIRR = suppCovered,
                    PatientPayableIRR = patientPayable,
                    Notes = notes.ToArray(),
                    UnitPriceIRRStr = ((decimal)unitPrice).ToIrrString(),
                    GrossIRRStr = ((decimal)gross).ToIrrString(),
                    BaseCoveredIRRStr = ((decimal)baseCovered).ToIrrString(),
                    SuppCoveredIRRStr = ((decimal)suppCovered).ToIrrString(),
                    PatientPayableIRRStr = ((decimal)patientPayable).ToIrrString(),
                    Coverage = coverageDetails
                };

                _logger.Information("✅ PRICING SERVICE: جزئیات قیمت محاسبه شد - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}", 
                    gross, baseCovered, suppCovered, patientPayable);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING SERVICE: خطا در محاسبه جزئیات قیمت - ReceptionId: {ReceptionId}, ReceptionItemId: {ReceptionItemId}", 
                    receptionId, receptionItemId);
                throw;
            }
        }

        /// <summary>
        /// محاسبه جمع‌های پذیرش (مجموع همه آیتم‌ها)
        /// </summary>
        public async Task<ReceptionTotalsDto> CalculateTotalsAsync(int receptionId)
        {
            try
            {
                _logger.Information("💰 PRICING SERVICE: محاسبه جمع‌های پذیرش - ReceptionId: {ReceptionId}", receptionId);

                // دریافت Reception با آیتم‌ها
                var reception = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && !r.IsDeleted);

                if (reception == null)
                {
                    _logger.Warning("⚠️ PRICING SERVICE: پذیرش یافت نشد - ReceptionId: {ReceptionId}", receptionId);
                    throw new InvalidOperationException($"پذیرش با شناسه {receptionId} یافت نشد");
                }

                var items = reception.ReceptionItems.Where(i => !i.IsDeleted).ToList();

                // محاسبه جمع‌ها از مقادیر entity
                long gross = 0;
                long baseCovered = 0;
                long suppCovered = 0;
                long patientPayable = 0;

                foreach (var item in items)
                {
                    var itemGross = (long)(item.UnitPrice * item.Quantity);
                    gross += itemGross;

                    // استخراج سهم‌ها از SnapshotJson
                    long itemBaseCovered = 0;
                    long itemSuppCovered = 0;

                    if (!string.IsNullOrEmpty(item.SnapshotJson))
                    {
                        try
                        {
                            var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(item.SnapshotJson);
                            if (snapshot != null)
                            {
                                if (snapshot.PrimaryPays != null)
                                    itemBaseCovered = (long)snapshot.PrimaryPays;
                                if (snapshot.SupplementaryPays != null)
                                    itemSuppCovered = (long)snapshot.SupplementaryPays;
                            }
                        }
                        catch
                        {
                            // Ignore
                        }
                    }

                    // Fallback: برآورد از InsurerShareAmount
                    if (itemBaseCovered == 0 && itemSuppCovered == 0)
                    {
                        var insurerShare = (long)item.InsurerShareAmount;
                        if (reception.BasePlanId.HasValue && reception.SupplementaryPlanId.HasValue)
                        {
                            itemBaseCovered = insurerShare / 2;
                            itemSuppCovered = insurerShare - itemBaseCovered;
                        }
                        else if (reception.BasePlanId.HasValue)
                        {
                            itemBaseCovered = insurerShare;
                        }
                        else if (reception.SupplementaryPlanId.HasValue)
                        {
                            itemSuppCovered = insurerShare;
                        }
                    }

                    baseCovered += itemBaseCovered;
                    suppCovered += itemSuppCovered;
                    patientPayable += (long)item.PatientShareAmount;
                }

                var result = new ReceptionTotalsDto
                {
                    GrossIRR = gross,
                    BaseCoveredIRR = baseCovered,
                    SuppCoveredIRR = suppCovered,
                    PatientPayableIRR = patientPayable,
                    GrossIRRStr = ((decimal)gross).ToIrrString(),
                    BaseCoveredIRRStr = ((decimal)baseCovered).ToIrrString(),
                    SuppCoveredIRRStr = ((decimal)suppCovered).ToIrrString(),
                    PatientPayableIRRStr = ((decimal)patientPayable).ToIrrString()
                };

                _logger.Information("✅ PRICING SERVICE: جمع‌های پذیرش محاسبه شد - Gross: {Gross}, Base: {Base}, Supp: {Supp}, Patient: {Patient}", 
                    gross, baseCovered, suppCovered, patientPayable);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING SERVICE: خطا در محاسبه جمع‌های پذیرش - ReceptionId: {ReceptionId}", receptionId);
                throw;
            }
        }

        /// <summary>
        /// محاسبه مجدد همه آیتم‌های پذیرش
        /// ✅ بهبود یافته: برگرداندن totals و pricings برای UI
        /// </summary>
        public async Task<(ReceptionTotalsDto totals, List<PricingBreakdownDto> pricings)> RepriceAllAsync(int receptionId)
        {
            try
            {
                _logger.Information("💰 PRICING SERVICE: شروع محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);

                // استفاده از PricingEngine برای Reprice
                await _pricingEngine.RepriceReceptionAsync(receptionId);

                _logger.Information("✅ PRICING SERVICE: محاسبه مجدد پذیرش تکمیل شد - ReceptionId: {ReceptionId}", receptionId);

                // ✅ دریافت Reception با آیتم‌ها برای محاسبه totals و pricings
                var reception = await _context.Receptions
                    .Include(r => r.ReceptionItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReceptionId == receptionId && !r.IsDeleted);

                if (reception == null)
                {
                    _logger.Warning("⚠️ PRICING SERVICE: پذیرش یافت نشد پس از Reprice - ReceptionId: {ReceptionId}", receptionId);
                    throw new InvalidOperationException($"پذیرش با شناسه {receptionId} یافت نشد");
                }

                // ✅ محاسبه pricings برای همه آیتم‌ها
                var pricings = new List<PricingBreakdownDto>();
                foreach (var item in reception.ReceptionItems.Where(i => !i.IsDeleted))
                {
                    try
                    {
                        var pricing = await PriceItemAsync(receptionId, item.ReceptionItemId);
                        pricings.Add(pricing);
                    }
                    catch (Exception itemEx)
                    {
                        _logger.Warning(itemEx, "⚠️ PRICING SERVICE: خطا در محاسبه قیمت آیتم - ReceptionItemId: {ReceptionItemId}", 
                            item.ReceptionItemId);
                        // ادامه با آیتم بعدی
                    }
                }

                // ✅ محاسبه totals
                var totals = await CalculateTotalsAsync(receptionId);

                _logger.Information("✅ PRICING SERVICE: Reprice کامل شد - ReceptionId: {ReceptionId}, ItemsCount: {Count}, Gross: {Gross}", 
                    receptionId, pricings.Count, totals.GrossIRR);

                return (totals, pricings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING SERVICE: خطا در محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);
                throw;
            }
        }

        /// <summary>
        /// ✅ بررسی وجود تعیین‌ست بیمه‌ای برای خدمت
        /// </summary>
        public async Task<(bool ok, string code, string message, object meta)> CheckInsuranceSetAsync(
            int serviceId, 
            int? departmentId, 
            int? doctorId, 
            int financialYearId, 
            int? basePlanId, 
            int? suppPlanId)
        {
            try
            {
                _logger.Information("🔍 PRICING SERVICE: بررسی تعیین‌ست بیمه‌ای - ServiceId: {ServiceId}, BasePlanId: {BasePlanId}, SuppPlanId: {SuppPlanId}", 
                    serviceId, basePlanId, suppPlanId);

                var missing = new List<string>();
                var meta = new Dictionary<string, object>
                {
                    ["serviceId"] = serviceId,
                    ["financialYearId"] = financialYearId
                };

                // ✅ بررسی FactorSetting برای سال مالی (استفاده از IFactorSettingService)
                try
                {
                    var techFactor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(
                        ServiceComponentType.Technical, 
                        false, // فرض: خدمت non-hashtagged برای بررسی کلی
                        financialYearId);

                    if (techFactor == null)
                    {
                        missing.Add("FactorSetting (Technical)");
                        meta["missingFactorSetting"] = true;
                    }
                }
                catch (Exception factorEx)
                {
                    _logger.Warning(factorEx, "⚠️ PRICING SERVICE: خطا در بررسی FactorSetting - FinancialYearId: {FinancialYearId}", 
                        financialYearId);
                    missing.Add("FactorSetting (Technical)");
                    meta["missingFactorSetting"] = true;
                }

                // ✅ بررسی InsuranceTariff برای بیمه پایه (در صورت وجود)
                if (basePlanId.HasValue)
                {
                    var baseTariff = await _context.InsuranceTariffs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.ServiceId == serviceId &&
                                                 t.InsurancePlanId == basePlanId.Value &&
                                                 t.InsuranceType == Models.Entities.Insurance.InsuranceType.Primary &&
                                                 t.IsActive &&
                                                 !t.IsDeleted);

                    if (baseTariff == null)
                    {
                        missing.Add("BASE");
                        meta["missingBase"] = true;
                    }
                }

                // ✅ بررسی InsuranceTariff برای بیمه تکمیلی (در صورت وجود)
                if (suppPlanId.HasValue)
                {
                    var suppTariff = await _context.InsuranceTariffs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.ServiceId == serviceId &&
                                                 t.InsurancePlanId == suppPlanId.Value &&
                                                 t.InsuranceType == Models.Entities.Insurance.InsuranceType.Supplementary &&
                                                 t.IsActive &&
                                                 !t.IsDeleted);

                    if (suppTariff == null)
                    {
                        missing.Add("SUPP");
                        meta["missingSupp"] = true;
                    }
                }

                if (missing.Any())
                {
                    var missingList = string.Join(" و ", missing);
                    meta["missing"] = missingList;
                    meta["createTariffUrl"] = $"/InsuranceTariff/Create?serviceId={serviceId}&planId={(basePlanId ?? suppPlanId)}";

                    _logger.Warning("⚠️ PRICING SERVICE: تعیین‌ست بیمه‌ای ناقص - ServiceId: {ServiceId}, Missing: {Missing}", 
                        serviceId, missingList);

                    return (false, "INSURANCE_SET_MISSING", 
                        $"برای این خدمت تعیین‌ست بیمه‌ای یافت نشد. ({missingList})", 
                        meta);
                }

                _logger.Information("✅ PRICING SERVICE: تعیین‌ست بیمه‌ای موجود است - ServiceId: {ServiceId}", serviceId);
                return (true, "SUCCESS", "تعیین‌ست بیمه‌ای موجود است", meta);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING SERVICE: خطا در بررسی تعیین‌ست بیمه‌ای - ServiceId: {ServiceId}", serviceId);
                return (false, "UNHANDLED", "خطا در بررسی تعیین‌ست بیمه‌ای: " + ex.Message, new Dictionary<string, object> { ["exception"] = ex.Message });
            }
        }

        /// <summary>
        /// ✅ ساخت CoverageDetails از مقادیر محاسبه شده
        /// </summary>
        private CoverageDetailsDto BuildCoverageDetails(
            long gross,
            long baseCovered,
            long suppCovered,
            long patientPayable,
            Models.Entities.Reception.Reception reception,
            string snapshotJson)
        {
            var coverage = new CoverageDetailsDto();

            // ✅ State: Full اگر patientPayable = 0، Partial اگر بخشی پوشش، None اگر هیچ پوششی نیست
            if (patientPayable == 0 && (baseCovered > 0 || suppCovered > 0))
            {
                coverage.State = CoverageState.Full;
            }
            else if (baseCovered > 0 || suppCovered > 0)
            {
                coverage.State = CoverageState.Partial;
            }
            else
            {
                coverage.State = CoverageState.None;
            }

            // ✅ Segments: ساخت لیست پرداخت‌کنندگان
            if (baseCovered > 0)
            {
                bool capHit = false;
                long? capRemaining = null;

                // استخراج اطلاعات سقف از SnapshotJson
                if (!string.IsNullOrEmpty(snapshotJson))
                {
                    try
                    {
                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(snapshotJson);
                        if (snapshot != null)
                        {
                            // بررسی اینکه آیا سقف اعمال شده است
                            if (snapshot.BaseInsuranceCoverage != null)
                            {
                                var coveragePercent = (decimal)snapshot.BaseInsuranceCoverage;
                                var expectedCoverage = gross * (coveragePercent / 100m);
                                if (baseCovered < expectedCoverage)
                                {
                                    capHit = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                coverage.Segments.Add(new CoverageSegmentDto
                {
                    Payer = "BASE",
                    AmountIRR = baseCovered,
                    Reason = capHit ? CoverageReasonCode.BaseCapReached : CoverageReasonCode.BaseCovered,
                    Note = capHit ? "پوشش پایه تا سقف اعمال شد" : "پوشش توسط بیمه پایه"
                });
            }

            if (suppCovered > 0)
            {
                bool capHit = false;

                // استخراج اطلاعات سقف از SnapshotJson
                if (!string.IsNullOrEmpty(snapshotJson))
                {
                    try
                    {
                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(snapshotJson);
                        if (snapshot != null)
                        {
                            if (snapshot.SupplementaryCoverage != null)
                            {
                                var coveragePercent = (decimal)snapshot.SupplementaryCoverage;
                                var remainderAfterBase = gross - baseCovered;
                                var expectedCoverage = remainderAfterBase * (coveragePercent / 100m);
                                if (suppCovered < expectedCoverage)
                                {
                                    capHit = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                coverage.Segments.Add(new CoverageSegmentDto
                {
                    Payer = "SUPP",
                    AmountIRR = suppCovered,
                    Reason = capHit ? CoverageReasonCode.SuppCapReached : CoverageReasonCode.SuppCovered,
                    Note = capHit ? "پوشش تکمیلی تا سقف اعمال شد" : "پوشش توسط بیمه تکمیلی"
                });
            }

            if (patientPayable > 0)
            {
                bool excluded = false;
                long? franchise = null;

                // بررسی فرانشیز از SnapshotJson
                if (!string.IsNullOrEmpty(snapshotJson))
                {
                    try
                    {
                        var snapshot = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(snapshotJson);
                        if (snapshot != null)
                        {
                            // اگر FranchisePercent وجود دارد، احتمالاً فرانشیز اعمال شده
                            if (snapshot.FranchisePercent != null || snapshot.FranchiseIRR != null)
                            {
                                franchise = snapshot.FranchiseIRR != null ? (long)snapshot.FranchiseIRR : null;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                }

                coverage.Segments.Add(new CoverageSegmentDto
                {
                    Payer = "PATIENT",
                    AmountIRR = patientPayable,
                    Reason = franchise.HasValue ? CoverageReasonCode.FranchiseApplied : 
                             (baseCovered == 0 && suppCovered == 0 ? CoverageReasonCode.NotInCoverage : CoverageReasonCode.None),
                    Note = franchise.HasValue ? $"فرانشیز بر عهده بیمار: {((decimal)franchise.Value).ToIrrString()}" :
                           (baseCovered == 0 && suppCovered == 0 ? "این خدمت در شمول پوشش نیست" : "سهم باقیمانده بیمار")
                });

                if (franchise.HasValue)
                {
                    coverage.FranchiseIRR = franchise.Value;
                }
            }

            // ✅ Warnings: پیام‌های قابل نمایش
            if (baseCovered == 0 && suppCovered == 0 && gross > 0)
            {
                coverage.Warnings.Add("این خدمت در شمول پوشش نیست.");
            }

            return coverage;
        }
    }
}

