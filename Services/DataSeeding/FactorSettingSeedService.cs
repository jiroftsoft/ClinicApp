using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        /// ایجاد کای‌های پایه برای سال مالی 1404
        /// </summary>
        public async Task SeedFactorSettingsAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد کای‌های پایه برای سال مالی 1404");

                var currentYear = GetCurrentPersianYear();
                var existingFactors = await _context.FactorSettings
                    .Where(f => f.FinancialYear == currentYear && !f.IsDeleted)
                    .ToListAsync();

                if (existingFactors.Any())
                {
                    _logger.Information($"کای‌های سال مالی {currentYear} قبلاً ایجاد شده‌اند");
                    return;
                }

                var factorSettings = new List<FactorSetting>
                {
                    // کای‌های فنی - خدمات عادی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Value = 4350000m, // کای فنی پایه - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21), // شروع سال 1404
                        EffectiveTo = new DateTime(2026, 3, 20), // پایان سال 1404
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی پایه برای کلیه خدمات (مصوبه 1404 - 4,350,000 ریال)"
                    },

                    // کای‌های فنی - خدمات کد ۷ (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = true,
                        Value = 2750000m, // کای فنی کد ۷ - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی برای خدمات کد ۷ (مصوبه 1404 - 2,750,000 ریال)"
                    },

                    // کای‌های فنی - خدمات کدهای ۸ و ۹ (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = true,
                        Value = 2600000m, // کای فنی کدهای ۸ و ۹ - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای فنی برای خدمات کدهای ۸ و ۹ (مصوبه 1404 - 2,600,000 ریال)"
                    },

                    // کای‌های فنی - دندانپزشکی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Technical,
                        IsHashtagged = false,
                        Value = 1900000m, // کای فنی دندانپزشکی - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
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
                        Value = 1000000m, // کای فنی مواد دندانپزشکی - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
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
                        Value = 1370000m, // کای حرفه‌ای پایه - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای پایه برای کلیه خدمات (مصوبه 1404 - 1,370,000 ریال)"
                    },

                    // کای‌های حرفه‌ای - ویزیت سرپایی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = true,
                        Value = 770000m, // کای حرفه‌ای ویزیت سرپایی - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای برای ویزیت سرپایی (مصوبه 1404 - 770,000 ریال)"
                    },

                    // کای‌های حرفه‌ای - دندانپزشکی (مصوبه 1404)
                    new FactorSetting
                    {
                        FactorType = ServiceComponentType.Professional,
                        IsHashtagged = false,
                        Value = 850000m, // کای حرفه‌ای دندانپزشکی - مصوبه 1404
                        EffectiveFrom = new DateTime(2025, 3, 21),
                        EffectiveTo = new DateTime(2026, 3, 20),
                        FinancialYear = currentYear,
                        IsActiveForCurrentYear = true,
                        IsFrozen = false,
                        IsActive = true,
                        Description = "کای حرفه‌ای دندانپزشکی (مصوبه 1404 - 850,000 ریال)"
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

                await _context.SaveChangesAsync();
                _logger.Information($"کای‌های پایه برای سال مالی {currentYear} با موفقیت ایجاد شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کای‌های پایه");
                throw;
            }
        }

        /// <summary>
        /// ایجاد کای‌های سال مالی قبلی (1403) برای تست
        /// </summary>
        public async Task SeedPreviousYearFactorsAsync()
        {
            try
            {
                var previousYear = GetCurrentPersianYear() - 1;
                var existingFactors = await _context.FactorSettings
                    .Where(f => f.FinancialYear == previousYear && !f.IsDeleted)
                    .ToListAsync();

                if (existingFactors.Any())
                {
                    _logger.Information($"کای‌های سال مالی {previousYear} قبلاً ایجاد شده‌اند");
                    return;
                }

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

                await _context.SaveChangesAsync();
                _logger.Information($"کای‌های سال مالی {previousYear} با موفقیت ایجاد شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کای‌های سال مالی قبلی");
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
        /// بررسی وجود کای‌های مورد نیاز
        /// </summary>
        public async Task<bool> ValidateRequiredFactorsAsync()
        {
            var currentYear = GetCurrentPersianYear();
            var requiredFactors = new[]
            {
                new { Type = ServiceComponentType.Technical, IsHashtagged = false },
                new { Type = ServiceComponentType.Technical, IsHashtagged = true },
                new { Type = ServiceComponentType.Professional, IsHashtagged = false },
                new { Type = ServiceComponentType.Professional, IsHashtagged = true }
            };

            foreach (var required in requiredFactors)
            {
                var exists = await _context.FactorSettings
                    .AnyAsync(f => f.FinancialYear == currentYear 
                                && f.FactorType == required.Type 
                                && f.IsHashtagged == required.IsHashtagged 
                                && f.IsActive 
                                && !f.IsDeleted);

                if (!exists)
                {
                    _logger.Warning($"کای {required.Type} برای خدمات {(required.IsHashtagged ? "هشتگ‌دار" : "عادی")} یافت نشد");
                    return false;
                }
            }

            return true;
        }
    }
}
