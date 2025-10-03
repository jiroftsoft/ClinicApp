using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Insurance;
using Serilog;

namespace ClinicApp.DataSeeding
{
    /// <summary>
    /// سرویس Seeding سیستم بیمه
    /// شامل: ارائه‌دهندگان بیمه، طرح‌های بیمه، و خدمات تحت پوشش
    /// </summary>
    public class InsuranceSeedService : BaseSeedService
    {
        public InsuranceSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// ایجاد تمام داده‌های بیمه
        /// </summary>
        public override async Task SeedAsync()
        {
            try
            {
                // بارگذاری کاربر Admin
                await LoadSystemUsersAsync();

                // 1. ایجاد ارائه‌دهندگان بیمه
                await SeedInsuranceProvidersAsync();

                // 2. ایجاد طرح‌های بیمه
                await SeedInsurancePlansAsync();

                // 3. ایجاد خدمات تحت پوشش
                await SeedPlanServicesAsync();
            }
            catch (Exception ex)
            {
                LogSeedError("سیستم بیمه", ex);
                throw;
            }
        }

        #region ارائه‌دهندگان بیمه (Insurance Providers)

        /// <summary>
        /// ایجاد ارائه‌دهندگان بیمه پیش‌فرض
        /// </summary>
        private async Task SeedInsuranceProvidersAsync()
        {
            try
            {
                LogSeedStart("ارائه‌دهندگان بیمه");

                var adminUser = await GetAdminUserAsync();

                // تعریف ارائه‌دهندگان (14 بیمه)
                var providers = new List<InsuranceProvider>
                {
                    // بیمه آزاد
                    CreateProvider(SeedConstants.InsuranceProviders.FreeCode, SeedConstants.InsuranceProviders.FreeName, 
                        SeedConstants.InsuranceProviders.FreeContactInfo, adminUser.Id),
                    
                    // بیمه‌های پایه
                    CreateProvider(SeedConstants.InsuranceProviders.SSOCode, SeedConstants.InsuranceProviders.SSOName, 
                        SeedConstants.InsuranceProviders.SSOContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.HealthCode, SeedConstants.InsuranceProviders.HealthName, 
                        SeedConstants.InsuranceProviders.HealthContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.MilitaryCode, SeedConstants.InsuranceProviders.MilitaryName, 
                        SeedConstants.InsuranceProviders.MilitaryContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.MedicalServicesCode, SeedConstants.InsuranceProviders.MedicalServicesName, 
                        SeedConstants.InsuranceProviders.MedicalServicesContactInfo, adminUser.Id),
                    
                    // بانک‌ها
                    CreateProvider(SeedConstants.InsuranceProviders.BankMelliCode, SeedConstants.InsuranceProviders.BankMelliName, 
                        SeedConstants.InsuranceProviders.BankMelliContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BankSaderatCode, SeedConstants.InsuranceProviders.BankSaderatName, 
                        SeedConstants.InsuranceProviders.BankSaderatContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BankSepahCode, SeedConstants.InsuranceProviders.BankSepahName, 
                        SeedConstants.InsuranceProviders.BankSepahContactInfo, adminUser.Id),
                    
                    // بیمه‌های تکمیلی
                    CreateProvider(SeedConstants.InsuranceProviders.DanaCode, SeedConstants.InsuranceProviders.DanaName, 
                        SeedConstants.InsuranceProviders.DanaContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BimeMaCode, SeedConstants.InsuranceProviders.BimeMaName, 
                        SeedConstants.InsuranceProviders.BimeMaContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BimeDeyCode, SeedConstants.InsuranceProviders.BimeDeyName, 
                        SeedConstants.InsuranceProviders.BimeDeyContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BimeAlborzCode, SeedConstants.InsuranceProviders.BimeAlborzName, 
                        SeedConstants.InsuranceProviders.BimeAlborzContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BimePasargadCode, SeedConstants.InsuranceProviders.BimePasargadName, 
                        SeedConstants.InsuranceProviders.BimePasargadContactInfo, adminUser.Id),
                    CreateProvider(SeedConstants.InsuranceProviders.BimeAsiaCode, SeedConstants.InsuranceProviders.BimeAsiaName, 
                        SeedConstants.InsuranceProviders.BimeAsiaContactInfo, adminUser.Id)
                };

                // فیلتر تکراری‌ها (رفع N+1 Problem با Expression)
                var newProviders = await FilterExistingItemsAsync<InsuranceProvider, string>(
                    providers,
                    p => p.Code,
                    _context.InsuranceProviders.Where(ip => !ip.IsDeleted),
                    ip => ip.Code  // Expression<Func<>> برای IQueryable
                );

                // اضافه کردن
                AddRangeIfAny(newProviders, "ارائه‌دهنده بیمه");

                LogSeedSuccess("ارائه‌دهندگان بیمه", newProviders.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("ارائه‌دهندگان بیمه", ex);
                throw;
            }
        }

        /// <summary>
        /// ایجاد یک InsuranceProvider
        /// </summary>
        private InsuranceProvider CreateProvider(string code, string name, string contactInfo, string userId)
        {
            return new InsuranceProvider
            {
                Code = code,
                Name = name,
                ContactInfo = contactInfo,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }

        #endregion

        #region طرح‌های بیمه (Insurance Plans)

        /// <summary>
        /// ایجاد طرح‌های بیمه پیش‌فرض
        /// </summary>
        private async Task SeedInsurancePlansAsync()
        {
            try
            {
                LogSeedStart("طرح‌های بیمه");

                var adminUser = await GetAdminUserAsync();

                // دریافت ارائه‌دهندگان
                var providers = await _context.InsuranceProviders
                    .Where(ip => !ip.IsDeleted)
                    .ToDictionaryAsync(ip => ip.Code, ip => ip.InsuranceProviderId);

                if (!providers.Any())
                {
                    _logger.Warning("هیچ ارائه‌دهنده بیمه‌ای یافت نشد. ابتدا SeedInsuranceProviders را اجرا کنید");
                    return;
                }

                // تعریف طرح‌ها (14 طرح مطابق واقعیت ایران)
                var plans = new List<InsurancePlan>
                {
                    // بیمه آزاد (100% بیمار)
                    CreatePlan(providers[SeedConstants.InsuranceProviders.FreeCode], 
                        SeedConstants.InsurancePlans.FreeBasicCode, SeedConstants.InsurancePlans.FreeBasicName,
                        SeedConstants.InsurancePlans.FreeBasicCoveragePercent, SeedConstants.InsurancePlans.FreeBasicDeductible, adminUser.Id),
                    
                    // بیمه‌های پایه (70% بیمه، 30% بیمار)
                    CreatePlan(providers[SeedConstants.InsuranceProviders.SSOCode], 
                        SeedConstants.InsurancePlans.SSOBasicCode, SeedConstants.InsurancePlans.SSOBasicName,
                        SeedConstants.InsurancePlans.SSOBasicCoveragePercent, SeedConstants.InsurancePlans.SSOBasicDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.HealthCode], 
                        SeedConstants.InsurancePlans.HealthBasicCode, SeedConstants.InsurancePlans.HealthBasicName,
                        SeedConstants.InsurancePlans.HealthBasicCoveragePercent, SeedConstants.InsurancePlans.HealthBasicDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.MilitaryCode], 
                        SeedConstants.InsurancePlans.MilitaryBasicCode, SeedConstants.InsurancePlans.MilitaryBasicName,
                        SeedConstants.InsurancePlans.MilitaryBasicCoveragePercent, SeedConstants.InsurancePlans.MilitaryBasicDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.MedicalServicesCode], 
                        SeedConstants.InsurancePlans.MedicalServicesBasicCode, SeedConstants.InsurancePlans.MedicalServicesBasicName,
                        SeedConstants.InsurancePlans.MedicalServicesBasicCoveragePercent, SeedConstants.InsurancePlans.MedicalServicesBasicDeductible, adminUser.Id),
                    
                    // بانک‌ها (70% بیمه، 30% بیمار)
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BankMelliCode], 
                        SeedConstants.InsurancePlans.BankMelliBasicCode, SeedConstants.InsurancePlans.BankMelliBasicName,
                        SeedConstants.InsurancePlans.BankMelliBasicCoveragePercent, SeedConstants.InsurancePlans.BankMelliBasicDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BankSaderatCode], 
                        SeedConstants.InsurancePlans.BankSaderatBasicCode, SeedConstants.InsurancePlans.BankSaderatBasicName,
                        SeedConstants.InsurancePlans.BankSaderatBasicCoveragePercent, SeedConstants.InsurancePlans.BankSaderatBasicDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BankSepahCode], 
                        SeedConstants.InsurancePlans.BankSepahBasicCode, SeedConstants.InsurancePlans.BankSepahBasicName,
                        SeedConstants.InsurancePlans.BankSepahBasicCoveragePercent, SeedConstants.InsurancePlans.BankSepahBasicDeductible, adminUser.Id),
                    
                    // بیمه‌های تکمیلی (100% پوشش)
                    CreatePlan(providers[SeedConstants.InsuranceProviders.DanaCode], 
                        SeedConstants.InsurancePlans.DanaSupplementaryCode, SeedConstants.InsurancePlans.DanaSupplementaryName,
                        SeedConstants.InsurancePlans.DanaSupplementaryCoveragePercent, SeedConstants.InsurancePlans.DanaSupplementaryDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BimeMaCode], 
                        SeedConstants.InsurancePlans.BimeMaSupplementaryCode, SeedConstants.InsurancePlans.BimeMaSupplementaryName,
                        SeedConstants.InsurancePlans.BimeMaSupplementaryCoveragePercent, SeedConstants.InsurancePlans.BimeMaSupplementaryDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BimeDeyCode], 
                        SeedConstants.InsurancePlans.BimeDeySupplementaryCode, SeedConstants.InsurancePlans.BimeDeySupplementaryName,
                        SeedConstants.InsurancePlans.BimeDeySupplementaryCoveragePercent, SeedConstants.InsurancePlans.BimeDeySupplementaryDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BimeAlborzCode], 
                        SeedConstants.InsurancePlans.BimeAlborzSupplementaryCode, SeedConstants.InsurancePlans.BimeAlborzSupplementaryName,
                        SeedConstants.InsurancePlans.BimeAlborzSupplementaryCoveragePercent, SeedConstants.InsurancePlans.BimeAlborzSupplementaryDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BimePasargadCode], 
                        SeedConstants.InsurancePlans.BimePasargadSupplementaryCode, SeedConstants.InsurancePlans.BimePasargadSupplementaryName,
                        SeedConstants.InsurancePlans.BimePasargadSupplementaryCoveragePercent, SeedConstants.InsurancePlans.BimePasargadSupplementaryDeductible, adminUser.Id),
                    CreatePlan(providers[SeedConstants.InsuranceProviders.BimeAsiaCode], 
                        SeedConstants.InsurancePlans.BimeAsiaSupplementaryCode, SeedConstants.InsurancePlans.BimeAsiaSupplementaryName,
                        SeedConstants.InsurancePlans.BimeAsiaSupplementaryCoveragePercent, SeedConstants.InsurancePlans.BimeAsiaSupplementaryDeductible, adminUser.Id)
                };

                // فیلتر تکراری‌ها (رفع N+1 Problem با Expression)
                var newPlans = await FilterExistingItemsAsync<InsurancePlan, string>(
                    plans,
                    p => p.PlanCode,
                    _context.InsurancePlans.Where(ip => !ip.IsDeleted),
                    ip => ip.PlanCode  // Expression<Func<>> برای IQueryable
                );

                // اضافه کردن
                AddRangeIfAny(newPlans, "طرح بیمه");

                LogSeedSuccess("طرح‌های بیمه", newPlans.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("طرح‌های بیمه", ex);
                throw;
            }
        }

        /// <summary>
        /// ایجاد یک InsurancePlan
        /// </summary>
        private InsurancePlan CreatePlan(
            int providerId, 
            string planCode, 
            string name, 
            int coveragePercent, 
            decimal deductible, 
            string userId)
        {
            return new InsurancePlan
            {
                InsuranceProviderId = providerId,
                PlanCode = planCode,
                Name = name,
                CoveragePercent = coveragePercent,
                Deductible = deductible,
                ValidFrom = DateTime.UtcNow.AddYears(-SeedConstants.PastYearsForValidFrom),
                ValidTo = DateTime.UtcNow.AddYears(SeedConstants.FutureYearsForValidTo),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }

        #endregion

        #region خدمات تحت پوشش (Plan Services)

        /// <summary>
        /// ایجاد خدمات تحت پوشش برای طرح‌های بیمه
        /// </summary>
        private async Task SeedPlanServicesAsync()
        {
            try
            {
                LogSeedStart("خدمات تحت پوشش");

                var adminUser = await GetAdminUserAsync();

                // دریافت طرح‌های بیمه
                var plans = await _context.InsurancePlans
                    .Where(ip => !ip.IsDeleted)
                    .ToListAsync();

                if (!plans.Any())
                {
                    _logger.Warning("هیچ طرح بیمه‌ای یافت نشد. ابتدا SeedInsurancePlans را اجرا کنید");
                    return;
                }

                // دریافت دسته‌بندی‌های خدمات
                var serviceCategories = await _context.ServiceCategories
                    .Where(sc => sc.IsActive && !sc.IsDeleted)
                    .ToListAsync();

                if (!serviceCategories.Any())
                {
                    _logger.Warning("هیچ دسته‌بندی خدماتی یافت نشد. PlanServices ایجاد نمی‌شود");
                    return;
                }

                // تنظیمات پوشش برای هر طرح
                var coverageSettings = GetCoverageSettingsByPlanCode();

                // ایجاد PlanServices
                var planServices = new List<PlanService>();

                foreach (var plan in plans)
                {
                    if (!coverageSettings.ContainsKey(plan.PlanCode))
                    {
                        _logger.Warning($"تنظیمات پوشش برای طرح '{plan.PlanCode}' یافت نشد");
                        continue;
                    }

                    var settings = coverageSettings[plan.PlanCode];

                    foreach (var category in serviceCategories)
                    {
                        var planService = CreatePlanService(
                            plan.InsurancePlanId,
                            category.ServiceCategoryId,
                            settings.PatientShare,
                            settings.Coverage,
                            adminUser.Id
                        );

                        planServices.Add(planService);
                    }
                }

                // فیلتر تکراری‌ها
                var existingPlanServices = await _context.PlanServices
                    .Where(ps => !ps.IsDeleted)
                    .Select(ps => new { ps.InsurancePlanId, ps.ServiceCategoryId })
                    .ToListAsync();

                var newPlanServices = planServices
                    .Where(ps => !existingPlanServices.Any(eps => 
                        eps.InsurancePlanId == ps.InsurancePlanId && 
                        eps.ServiceCategoryId == ps.ServiceCategoryId))
                    .ToList();

                // اضافه کردن
                AddRangeIfAny(newPlanServices, "خدمت تحت پوشش");

                LogSeedSuccess("خدمات تحت پوشش", newPlanServices.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("خدمات تحت پوشش", ex);
                throw;
            }
        }

        /// <summary>
        /// دریافت تنظیمات پوشش برای هر طرح بیمه (مطابق واقعیت ایران)
        /// </summary>
        private Dictionary<string, (int PatientShare, int Coverage)> GetCoverageSettingsByPlanCode()
        {
            return new Dictionary<string, (int PatientShare, int Coverage)>
            {
                // بیمه آزاد (100% بیمار، 0% بیمه)
                { SeedConstants.InsurancePlans.FreeBasicCode, 
                    (SeedConstants.CoverageSettings.FreePatientShare, SeedConstants.InsurancePlans.FreeBasicCoveragePercent) },
                
                // بیمه‌های پایه (30% بیمار، 70% بیمه)
                { SeedConstants.InsurancePlans.SSOBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.SSOBasicCoveragePercent) },
                { SeedConstants.InsurancePlans.HealthBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.HealthBasicCoveragePercent) },
                { SeedConstants.InsurancePlans.MilitaryBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.MilitaryBasicCoveragePercent) },
                { SeedConstants.InsurancePlans.MedicalServicesBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.MedicalServicesBasicCoveragePercent) },
                
                // بانک‌ها (30% بیمار، 70% بیمه)
                { SeedConstants.InsurancePlans.BankMelliBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.BankMelliBasicCoveragePercent) },
                { SeedConstants.InsurancePlans.BankSaderatBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.BankSaderatBasicCoveragePercent) },
                { SeedConstants.InsurancePlans.BankSepahBasicCode, 
                    (SeedConstants.CoverageSettings.PrimaryPatientShare, SeedConstants.InsurancePlans.BankSepahBasicCoveragePercent) },
                
                // بیمه‌های تکمیلی (0% بیمار، 100% پوشش)
                { SeedConstants.InsurancePlans.DanaSupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.DanaSupplementaryCoveragePercent) },
                { SeedConstants.InsurancePlans.BimeMaSupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.BimeMaSupplementaryCoveragePercent) },
                { SeedConstants.InsurancePlans.BimeDeySupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.BimeDeySupplementaryCoveragePercent) },
                { SeedConstants.InsurancePlans.BimeAlborzSupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.BimeAlborzSupplementaryCoveragePercent) },
                { SeedConstants.InsurancePlans.BimePasargadSupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.BimePasargadSupplementaryCoveragePercent) },
                { SeedConstants.InsurancePlans.BimeAsiaSupplementaryCode, 
                    (SeedConstants.CoverageSettings.SupplementaryPatientShare, SeedConstants.InsurancePlans.BimeAsiaSupplementaryCoveragePercent) }
            };
        }

        /// <summary>
        /// ایجاد یک PlanService
        /// </summary>
        private PlanService CreatePlanService(
            int planId, 
            int categoryId, 
            int patientSharePercent, 
            int coverageOverride, 
            string userId)
        {
            return new PlanService
            {
                InsurancePlanId = planId,
                ServiceCategoryId = categoryId,
                PatientSharePercent = patientSharePercent,
                CoverageOverride = coverageOverride,
                IsCovered = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }

        #endregion

        #region Validation

        /// <summary>
        /// اعتبارسنجی سیستم بیمه
        /// </summary>
        public override async Task<bool> ValidateAsync()
        {
            try
            {
                var providersExist = await _context.InsuranceProviders
                    .AnyAsync(ip => !ip.IsDeleted);

                var plansExist = await _context.InsurancePlans
                    .AnyAsync(ip => !ip.IsDeleted);

                if (!providersExist)
                {
                    _logger.Warning("هیچ ارائه‌دهنده بیمه‌ای یافت نشد");
                    return false;
                }

                if (!plansExist)
                {
                    _logger.Warning("هیچ طرح بیمه‌ای یافت نشد");
                    return false;
                }

                _logger.Debug("✅ اعتبارسنجی سیستم بیمه موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی سیستم بیمه");
                return false;
            }
        }

        #endregion
    }
}

