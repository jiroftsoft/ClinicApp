using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using ClinicApp.Controllers.Api;
using ClinicApp.Interfaces.Reception;
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

        public ReceptionPricingService(
            IPricingEngine pricingEngine,
            ApplicationDbContext context,
            ILogger logger)
        {
            _pricingEngine = pricingEngine ?? throw new ArgumentNullException(nameof(pricingEngine));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger.ForContext<ReceptionPricingService>();
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

                var result = new PricingBreakdownDto
                {
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
                    PatientPayableIRRStr = ((decimal)patientPayable).ToIrrString()
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
        /// </summary>
        public async Task RepriceAllAsync(int receptionId)
        {
            try
            {
                _logger.Information("💰 PRICING SERVICE: شروع محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);

                // استفاده از PricingEngine برای Reprice
                await _pricingEngine.RepriceReceptionAsync(receptionId);

                _logger.Information("✅ PRICING SERVICE: محاسبه مجدد پذیرش تکمیل شد - ReceptionId: {ReceptionId}", receptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PRICING SERVICE: خطا در محاسبه مجدد پذیرش - ReceptionId: {ReceptionId}", receptionId);
                throw;
            }
        }
    }
}

