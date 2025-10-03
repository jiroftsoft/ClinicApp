using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models;
using ClinicApp.Core;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.DataSeeding
{
    /// <summary>
    /// سرویس به‌روزرسانی مقادیر InsuranceType در جدول InsurancePlans
    /// این سرویس برای رفع مشکل مقادیر صفر در فیلد InsuranceType طراحی شده است
    /// </summary>
    public class InsuranceTypeUpdateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public InsuranceTypeUpdateService(ApplicationDbContext context)
        {
            _context = context;
            _logger = Log.ForContext<InsuranceTypeUpdateService>();
        }

        /// <summary>
        /// به‌روزرسانی مقادیر InsuranceType برای تمام طرح‌های بیمه
        /// </summary>
        public async Task<ServiceResult> UpdateInsuranceTypeValuesAsync()
        {
            var correlationId = Guid.NewGuid().ToString();
            var startTime = DateTime.UtcNow;

            _logger.Information("🔧 INSURANCE_TYPE_UPDATE: شروع به‌روزرسانی مقادیر InsuranceType - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // دریافت تمام طرح‌های بیمه فعال
                var allPlans = await _context.InsurancePlans
                    .Where(ip => ip.IsActive && !ip.IsDeleted)
                    .ToListAsync();

                _logger.Information("🔧 INSURANCE_TYPE_UPDATE: تعداد طرح‌های بیمه یافت شده: {Count}", allPlans.Count);

                var updatedCount = 0;
                var errorCount = 0;

                // به‌روزرسانی بیمه‌های پایه
                var primaryPlans = allPlans.Where(p => IsPrimaryInsurance(p.PlanCode)).ToList();
                foreach (var plan in primaryPlans)
                {
                    try
                    {
                        if (plan.InsuranceType != InsuranceType.Primary)
                        {
                            plan.InsuranceType = InsuranceType.Primary;
                            updatedCount++;
                            _logger.Debug("🔧 INSURANCE_TYPE_UPDATE: به‌روزرسانی بیمه پایه - {PlanCode}: {Name}", 
                                plan.PlanCode, plan.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطا در به‌روزرسانی بیمه پایه - {PlanCode}", plan.PlanCode);
                        errorCount++;
                    }
                }

                // به‌روزرسانی بیمه‌های تکمیلی
                var supplementaryPlans = allPlans.Where(p => IsSupplementaryInsurance(p.PlanCode)).ToList();
                foreach (var plan in supplementaryPlans)
                {
                    try
                    {
                        if (plan.InsuranceType != InsuranceType.Supplementary)
                        {
                            plan.InsuranceType = InsuranceType.Supplementary;
                            updatedCount++;
                            _logger.Debug("🔧 INSURANCE_TYPE_UPDATE: به‌روزرسانی بیمه تکمیلی - {PlanCode}: {Name}", 
                                plan.PlanCode, plan.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطا در به‌روزرسانی بیمه تکمیلی - {PlanCode}", plan.PlanCode);
                        errorCount++;
                    }
                }

                // ذخیره تغییرات
                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.Information("🔧 INSURANCE_TYPE_UPDATE: {UpdatedCount} طرح بیمه به‌روزرسانی شد", updatedCount);
                }

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.Information("🔧 INSURANCE_TYPE_UPDATE: عملیات تکمیل شد - CorrelationId: {CorrelationId}, Duration: {Duration}ms, Updated: {UpdatedCount}, Errors: {ErrorCount}",
                    correlationId, duration.TotalMilliseconds, updatedCount, errorCount);

                return ServiceResult.Successful(
                    $"به‌روزرسانی مقادیر InsuranceType با موفقیت انجام شد. تعداد به‌روزرسانی شده: {updatedCount}",
                    "UpdateInsuranceTypeValues",
                    correlationId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🔧 INSURANCE_TYPE_UPDATE: خطای کلی در به‌روزرسانی مقادیر InsuranceType - CorrelationId: {CorrelationId}", correlationId);
                
                return ServiceResult.Failed(
                    "خطا در به‌روزرسانی مقادیر InsuranceType. لطفاً دوباره تلاش کنید.",
                    "UPDATE_INSURANCE_TYPE_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// بررسی اینکه آیا طرح بیمه پایه است یا خیر
        /// </summary>
        private bool IsPrimaryInsurance(string planCode)
        {
            var primaryCodes = new[]
            {
                "FREE_BASIC",
                "SSO_BASIC",
                "SALAMAT_BASIC", 
                "MILITARY_BASIC",
                "KHADAMAT_BASIC",
                "BANK_MELLI_BASIC",
                "BANK_SADERAT_BASIC",
                "BANK_SEPAH_BASIC"
            };

            return primaryCodes.Contains(planCode);
        }

        /// <summary>
        /// بررسی اینکه آیا طرح بیمه تکمیلی است یا خیر
        /// </summary>
        private bool IsSupplementaryInsurance(string planCode)
        {
            var supplementaryCodes = new[]
            {
                "DANA_SUPPLEMENTARY",
                "BIME_MA_SUPPLEMENTARY",
                "BIME_DEY_SUPPLEMENTARY",
                "BIME_ALBORZ_SUPPLEMENTARY",
                "BIME_PASARGAD_SUPPLEMENTARY",
                "BIME_ASIA_SUPPLEMENTARY"
            };

            return supplementaryCodes.Contains(planCode);
        }

        /// <summary>
        /// دریافت آمار به‌روزرسانی
        /// </summary>
        public async Task<ServiceResult<Dictionary<string, object>>> GetUpdateStatisticsAsync()
        {
            try
            {
                var statistics = new Dictionary<string, object>();

                // آمار کلی
                var totalPlans = await _context.InsurancePlans
                    .Where(ip => ip.IsActive && !ip.IsDeleted)
                    .CountAsync();

                var primaryPlans = await _context.InsurancePlans
                    .Where(ip => ip.IsActive && !ip.IsDeleted && ip.InsuranceType == InsuranceType.Primary)
                    .CountAsync();

                var supplementaryPlans = await _context.InsurancePlans
                    .Where(ip => ip.IsActive && !ip.IsDeleted && ip.InsuranceType == InsuranceType.Supplementary)
                    .CountAsync();

                var invalidPlans = await _context.InsurancePlans
                    .Where(ip => ip.IsActive && !ip.IsDeleted && ip.InsuranceType == 0)
                    .CountAsync();

                statistics["TotalPlans"] = totalPlans;
                statistics["PrimaryPlans"] = primaryPlans;
                statistics["SupplementaryPlans"] = supplementaryPlans;
                statistics["InvalidPlans"] = invalidPlans;
                statistics["NeedsUpdate"] = invalidPlans > 0;

                return ServiceResult<Dictionary<string, object>>.Successful(
                    statistics,
                    "آمار به‌روزرسانی InsuranceType با موفقیت دریافت شد.",
                    "GetUpdateStatistics",
                    "System",
                    "System",
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار به‌روزرسانی InsuranceType");
                return ServiceResult<Dictionary<string, object>>.Failed(
                    "خطا در دریافت آمار به‌روزرسانی.",
                    "GET_UPDATE_STATISTICS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }
    }
}
