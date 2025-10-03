using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.DataSeeding;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;
using Serilog;


namespace ClinicApp.Services.DataSeeding
{
    /// <summary>
    /// سرویس ایجاد داده‌های اولیه برای تنظیمات کای‌ها
    /// </summary>
    public class FactorSettingSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public FactorSettingSeedService(
            ApplicationDbContext context, 
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// ایجاد کای‌های پایه برای سال مالی 1404 با استفاده از Constants
        /// </summary>
        public async Task SeedFactorSettingsAsync()
        {
            try
            {
                _logger.Information("═══════════════════════════════════════════════");
                _logger.Information("🌱 FACTOR_SEED: شروع ایجاد کای‌های پایه سال مالی {Year}", 
                    SeedConstants.FactorSettings1404.FinancialYear);
                _logger.Information("═══════════════════════════════════════════════");

                var currentYear = SeedConstants.FactorSettings1404.FinancialYear;
                var existingFactors = await _context.FactorSettings
                    .Where(f => f.FinancialYear == currentYear && !f.IsDeleted)
                    .ToListAsync();

                if (existingFactors.Any())
                {
                    _logger.Information("✅ FACTOR_SEED: کای‌های سال مالی {Year} قبلاً ایجاد شده‌اند ({Count} کای)", 
                        currentYear, existingFactors.Count);
                    return;
                }

                var factorSettings = new List<FactorSetting>
                {
                    // کای‌های فنی - خدمات عادی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Scope = FactorScope.General_NoHash,
                        Value = SeedConstants.FactorSettings1404.TechnicalNormal,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = $"کای فنی پایه برای کلیه خدمات (مصوبه {currentYear} - {SeedConstants.FactorSettings1404.TechnicalNormal:N0} ریال)"
                    },

                    // کای‌های فنی - خدمات هشتگ‌دار کدهای ۱ تا ۷ (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = true,
                        Scope = FactorScope.Hash_1_7,
                        Value = SeedConstants.FactorSettings1404.TechnicalHash_1_7,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی برای خدمات هشتگ‌دار کدهای ۱ تا ۷ (مصوبه 1404 - 2,750,000 ریال)"
                    },

                    // کای‌های فنی - خدمات هشتگ‌دار کدهای ۸ و ۹ (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = true,
                        Scope = FactorScope.Hash_8_9,
                        Value = SeedConstants.FactorSettings1404.TechnicalHash_8_9,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی برای خدمات هشتگ‌دار کدهای ۸ و ۹ (مصوبه 1404 - 2,600,000 ریال)"
                    },

                    // کای‌های فنی - دندانپزشکی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Scope = FactorScope.Dent_Technical,
                        Value = SeedConstants.FactorSettings1404.TechnicalDental,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی دندانپزشکی (مصوبه 1404 - 1,900,000 ریال)"
                    },

                    // کای‌های فنی - مواد و لوازم دندانپزشکی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Scope = FactorScope.Dent_Consumables,
                        Value = SeedConstants.FactorSettings1404.TechnicalDentalConsumables,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی مواد و لوازم مصرفی دندانپزشکی (مصوبه 1404 - 1,000,000 ریال)"
                    },

                    // کای‌های حرفه‌ای - خدمات عادی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = false,
                        Scope = FactorScope.Prof_NoHash,
                        Value = SeedConstants.FactorSettings1404.ProfessionalNormal,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای پایه برای کلیه خدمات (مصوبه 1404 - 1,370,000 ریال)"
                    },

                    // کای‌های حرفه‌ای - خدمات هشتگ‌دار (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = true,
                        Scope = FactorScope.Prof_Hash,
                        Value = SeedConstants.FactorSettings1404.ProfessionalHash,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای برای خدمات هشتگ‌دار (مصوبه 1404 - 770,000 ریال)"
                    },

                    // کای‌های حرفه‌ای - دندانپزشکی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = false,
                        Scope = FactorScope.Prof_Dental, // دندانپزشکی بدون هشتگ - Scope جداگانه
                        Value = SeedConstants.FactorSettings1404.ProfessionalDental,
                        EffectiveFrom = SeedConstants.FactorSettings1404.EffectiveFrom,
                        EffectiveTo = SeedConstants.FactorSettings1404.EffectiveTo,
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای دندانپزشکی (مصوبه 1404 - 850,000 ریال)"
                    }
                };

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();

                _logger.Information("📍 FACTOR_SEED: اضافه کردن {Count} کای به دیتابیس", factorSettings.Count);

                foreach (var factor in factorSettings)
                {
                    factor.CreatedAt = DateTime.UtcNow;
                    factor.CreatedByUserId = systemUserId;
                    
                    // اگر فریز شده است، FrozenByUserId را نیز تنظیم می‌کنیم
                    if (factor.IsFrozen && !string.IsNullOrEmpty(factor.FrozenByUserId))
                    {
                        factor.FrozenByUserId = systemUserId;
                    }
                    
                    _context.FactorSettings.Add(factor);
                    _logger.Debug("📌 FACTOR_SEED: کای {Type} - {IsHashtagged} - {Value:N0} ریال", 
                        factor.FactorType, 
                        factor.IsHashtagged ? "هشتگ‌دار" : "عادی", 
                        factor.Value);
                }

                // حذف SaveChangesAsync - انجام می‌شود در SystemSeedService
                _logger.Information("✅ FACTOR_SEED: کای‌های سال مالی {Year} آماده ذخیره‌سازی ({Count} کای)", 
                    currentYear, factorSettings.Count);
                _logger.Information("═══════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACTOR_SEED: خطا در ایجاد کای‌های پایه");
                throw;
            }
        }

        /// <summary>
        /// ایجاد کای‌های سال مالی قبلی (1403) برای تست با Structured Logging
        /// </summary>
        public async Task SeedPreviousYearFactorsAsync()
        {
            try
            {
                _logger.Information("🔄 FACTOR_SEED: بررسی کای‌های سال قبل...");

                var previousYear = SeedConstants.FactorSettings1404.FinancialYear - 1;
                var existingFactors = await _context.FactorSettings
                    .Where(f => f.FinancialYear == previousYear && !f.IsDeleted)
                    .ToListAsync();

                if (existingFactors.Any())
                {
                    _logger.Information("✅ FACTOR_SEED: کای‌های سال مالی {Year} قبلاً ایجاد شده‌اند", previousYear);
                    return;
                }

                _logger.Information("📍 FACTOR_SEED: ایجاد کای‌های سال مالی {Year} (Frozen)", previousYear);

                var factorSettings = new List<FactorSetting>
                {
                    // کای‌های فنی - خدمات عادی (سال قبل)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Value = 2.8m, // کای فنی پایه سال قبل
                        EffectiveFrom = new DateTime(2024, 3, 21),
                        EffectiveTo = new DateTime(2025, 3, 20),
                        FinancialYear = previousYear,
                        IsActiveForCurrentYear = false,
                        IsFrozen = true, // سال قبل فریز شده
                        FrozenAt = DateTime.UtcNow,
                        FrozenByUserId = "system-seed",
                        IsActive = true,
                        Description = "کای فنی پایه برای خدمات عادی - سال مالی 1403 (فریز شده)"
                    },

                    // کای‌های حرفه‌ای - خدمات عادی (سال قبل)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = false,
                        Value = 11.5m, // کای حرفه‌ای پایه سال قبل
                        EffectiveFrom = new DateTime(2024, 3, 21),
                        EffectiveTo = new DateTime(2025, 3, 20),
                        FinancialYear = previousYear,
                        IsActiveForCurrentYear = false,
                        IsFrozen = true, // سال قبل فریز شده
                        FrozenAt = DateTime.UtcNow,
                        FrozenByUserId = "system-seed",
                        IsActive = true,
                        Description = "کای حرفه‌ای پایه برای خدمات عادی - سال مالی 1403 (فریز شده)"
                    }
                };

                // دریافت کاربر معتبر برای Seed
                var systemUserId = await GetValidUserIdForSeedAsync();

                foreach (var factor in factorSettings)
                {
                    factor.CreatedAt = DateTime.UtcNow;
                    factor.CreatedByUserId = systemUserId;
                    
                    // اگر فریز شده است، FrozenByUserId را نیز تنظیم می‌کنیم
                    if (factor.IsFrozen && !string.IsNullOrEmpty(factor.FrozenByUserId))
                    {
                        factor.FrozenByUserId = systemUserId;
                    }
                    
                    _context.FactorSettings.Add(factor);
                }

                // حذف SaveChangesAsync - انجام می‌شود در SystemSeedService
                _logger.Information("✅ FACTOR_SEED: کای‌های سال مالی {Year} آماده ذخیره‌سازی ({Count} کای - Frozen)", 
                    previousYear, factorSettings.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACTOR_SEED: خطا در ایجاد کای‌های سال قبلی");
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربر معتبر برای عملیات Seed
        /// </summary>
        private async Task<string> GetValidUserIdForSeedAsync()
        {
            try
            {
                // ابتدا سعی می‌کنیم کاربر فعلی را دریافت کنیم
                var currentUserId = _currentUserService.GetCurrentUserId();
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    // بررسی وجود کاربر در دیتابیس
                    var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId);
                    if (userExists)
                    {
                        _logger.Information("استفاده از کاربر فعلی برای Seed: {UserId}", currentUserId);
                        return currentUserId;
                    }
                }

                // اگر کاربر فعلی وجود ندارد، کاربر سیستم را پیدا می‌کنیم
                var systemUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == "3031945451" || u.UserName == "system");
                
                if (systemUser != null)
                {
                    _logger.Information("استفاده از کاربر سیستم برای Seed: {UserId}", systemUser.Id);
                    return systemUser.Id;
                }

                // اگر کاربر سیستم وجود ندارد، کاربر ادمین را پیدا می‌کنیم
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == "3020347998" || u.UserName == "admin");
                
                if (adminUser != null)
                {
                    _logger.Information("استفاده از کاربر ادمین برای Seed: {UserId}", adminUser.Id);
                    return adminUser.Id;
                }

                // در نهایت، از شناسه پیش‌فرض استفاده می‌کنیم
                var fallbackUserId = "6f999f4d-24b8-4142-a97e-20077850278b";
                _logger.Warning("هیچ کاربر معتبری یافت نشد. استفاده از شناسه پیش‌فرض: {UserId}", fallbackUserId);
                return fallbackUserId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربر معتبر برای Seed");
                // در صورت خطا، از شناسه پیش‌فرض استفاده می‌کنیم
                return "6f999f4d-24b8-4142-a97e-20077850278b";
            }
        }

        /// <summary>
        /// دریافت سال مالی جاری (فارسی)
        /// </summary>
        private int GetCurrentPersianYear()
        {
            // برای تست، سال 1404 را برمی‌گردانیم
            // در تولید باید از PersianCalendar استفاده شود
            return 1404;
        }

        /// <summary>
        /// بررسی وجود کای‌های مورد نیاز با Logging دقیق و ضدگلوله‌سازی
        /// </summary>
        public async Task<bool> ValidateRequiredFactorsAsync()
        {
            try
            {
                _logger.Information("🔍 FACTOR_VALIDATION: شروع اعتبارسنجی کای‌های مورد نیاز");

                var currentYear = GetCurrentPersianYear();
                _logger.Information("📅 FACTOR_VALIDATION: سال مالی جاری: {Year}", currentYear);

                // بررسی Context.Local اول
                var localFactorsCount = _context.FactorSettings.Local
                    .Count(f => f.FinancialYear == currentYear && f.IsActive && !f.IsDeleted);

                _logger.Information("📊 FACTOR_VALIDATION: Context.Local - تعداد کای‌ها: {Count}", localFactorsCount);

                // اگر Local خالی است، از DB بخوان
                List<FactorSetting> factors;
                if (localFactorsCount == 0)
                {
                    _logger.Information("⚠️ FACTOR_VALIDATION: Context.Local خالی است - بررسی دیتابیس...");
                    factors = await _context.FactorSettings
                        .Where(f => f.FinancialYear == currentYear && f.IsActive && !f.IsDeleted)
                        .ToListAsync();
                    _logger.Information("📊 FACTOR_VALIDATION: Database - تعداد کای‌ها: {Count}", factors.Count);
                }
                else
                {
                    factors = _context.FactorSettings.Local
                        .Where(f => f.FinancialYear == currentYear && f.IsActive && !f.IsDeleted)
                        .ToList();
                }

                if (!factors.Any())
                {
                    _logger.Error("❌ FACTOR_VALIDATION: هیچ کای‌ای برای سال مالی {Year} یافت نشد!", currentYear);
                    return false;
                }

                // بررسی کای‌های مورد نیاز
                var requiredFactors = new[]
                {
                    new { Type = ServiceComponentType.Technical, IsHashtagged = false, Name = "کای فنی عادی" },
                    new { Type = ServiceComponentType.Technical, IsHashtagged = true, Name = "کای فنی هشتگ‌دار" },
                    new { Type = ServiceComponentType.Professional, IsHashtagged = false, Name = "کای حرفه‌ای عادی" },
                    new { Type = ServiceComponentType.Professional, IsHashtagged = true, Name = "کای حرفه‌ای هشتگ‌دار" }
                };

                var missingFactors = new List<string>();
                var foundFactors = new List<string>();

                foreach (var required in requiredFactors)
                {
                    var exists = factors.Any(f => f.FactorType == required.Type && f.IsHashtagged == required.IsHashtagged);
                    
                    if (exists)
                    {
                        var factor = factors.First(f => f.FactorType == required.Type && f.IsHashtagged == required.IsHashtagged);
                        foundFactors.Add($"{required.Name}: {factor.Value:N0} ریال");
                        _logger.Information("✅ FACTOR_VALIDATION: {Name} = {Value:N0} ریال", required.Name, factor.Value);
                    }
                    else
                    {
                        missingFactors.Add(required.Name);
                        _logger.Error("❌ FACTOR_VALIDATION: {Name} یافت نشد!", required.Name);
                    }
                }

                if (missingFactors.Any())
                {
                    _logger.Error("❌ FACTOR_VALIDATION: {Count} کای مورد نیاز یافت نشد: {Missing}",
                        missingFactors.Count, string.Join(", ", missingFactors));
                    return false;
                }

                _logger.Information("✅ FACTOR_VALIDATION: همه کای‌های مورد نیاز موجود هستند:");
                foreach (var found in foundFactors)
                {
                    _logger.Information("   - {Factor}", found);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ FACTOR_VALIDATION: خطا در اعتبارسنجی کای‌ها");
                return false;
            }
        }
    }
}
