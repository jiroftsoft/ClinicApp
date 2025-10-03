using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.DataSeeding;
using ClinicApp.Models;
using ClinicApp.Interfaces;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.DataSeeding
{
    /// <summary>
    /// سرویس اصلی برای ایجاد داده‌های اولیه سیستم
    /// </summary>
    public class SystemSeedService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly FactorSettingSeedService _factorSeedService;
        private readonly ServiceSeedService _serviceSeedService;
        private readonly ServiceTemplateSeedService _serviceTemplateSeedService;

        public SystemSeedService(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService,
            FactorSettingSeedService factorSeedService,
            ServiceSeedService serviceSeedService,
            ServiceTemplateSeedService serviceTemplateSeedService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
            _factorSeedService = factorSeedService;
            _serviceSeedService = serviceSeedService;
            _serviceTemplateSeedService = serviceTemplateSeedService;
        }

        /// <summary>
        /// ایجاد تمام داده‌های اولیه سیستم با Transaction Management
        /// </summary>
        public async Task SeedAllDataAsync()
        {
            // شروع Transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _logger.Information("═══════════════════════════════════════════════");
                    _logger.Information("🌱 SYSTEM_SEED: شروع ایجاد داده‌های اولیه سیستم");
                    _logger.Information("═══════════════════════════════════════════════");

                    var startTime = DateTime.UtcNow;

                    // 0. مقداردهی اولیه SystemUsers (اولویت اول)
                    _logger.Information("📍 SYSTEM_SEED: مرحله 0 - مقداردهی اولیه SystemUsers");
                    SystemUsers.Initialize(_context);

                    // 1. ایجاد کای‌های پایه
                    _logger.Information("📍 SYSTEM_SEED: مرحله 1 - ایجاد کای‌های پایه (مصوبه {Year})", 
                        SeedConstants.FactorSettings1404.FinancialYear);
                    await _factorSeedService.SeedFactorSettingsAsync();
                    await _factorSeedService.SeedPreviousYearFactorsAsync();

                    // 2. ایجاد قالب‌های خدمات
                    _logger.Information("📍 SYSTEM_SEED: مرحله 2 - ایجاد قالب‌های خدمات");
                    await _serviceTemplateSeedService.SeedServiceTemplatesAsync();

                    // 3. ایجاد خدمات نمونه
                    _logger.Information("📍 SYSTEM_SEED: مرحله 3 - ایجاد خدمات نمونه");
                    await _serviceSeedService.SeedSampleServicesAsync();

                    // 4. ایجاد اجزای خدمات
                    _logger.Information("📍 SYSTEM_SEED: مرحله 4 - ایجاد اجزای خدمات");
                    await _serviceSeedService.SeedServiceComponentsAsync();

                    // 5. ایجاد خدمات مشترک
                    _logger.Information("📍 SYSTEM_SEED: مرحله 5 - ایجاد خدمات مشترک");
                    await _serviceSeedService.SeedSharedServicesAsync();

                    // 6. اعتبارسنجی (قبل از SaveChanges - از Context.Local)
                    _logger.Information("🔍 SYSTEM_SEED: مرحله 6 - اعتبارسنجی داده‌ها");
                    var factorsValid = await _factorSeedService.ValidateRequiredFactorsAsync();
                    var servicesValid = await _serviceSeedService.ValidateSeededDataAsync();

                    if (!factorsValid || !servicesValid)
                    {
                        _logger.Warning("⚠️ SYSTEM_SEED: اعتبارسنجی ناموفق - Rollback Transaction");
                        transaction.Rollback();
                        throw new InvalidOperationException("اعتبارسنجی داده‌های Seed شده ناموفق بود");
                    }

                    // 7. ذخیره تمام تغییرات (یک بار!)
                    _logger.Information("💾 SYSTEM_SEED: ذخیره تمام تغییرات در دیتابیس...");
                    
                    // Logging ChangeTracker برای تشخیص مشکل
                    _logger.Information("🔍 SYSTEM_SEED: بررسی ChangeTracker...");
                    var addedEntries = _context.ChangeTracker.Entries()
                        .Where(x => x.State == EntityState.Added)
                        .ToList();
                    
                    _logger.Information("📊 SYSTEM_SEED: تعداد Added Entities: {Count}", addedEntries.Count);
                    //foreach (var entry in addedEntries)
                    //{
                    //    var entityType = entry.Entity.GetType().Name;
                    //    var keyValues = string.Join(", ", entry.CurrentValues.PropertyNames
                    //        .Where(p => p.EndsWith("Id"))
                    //        .Select(p => $"{p}:{entry.CurrentValues[p]}"));
                    //    _logger.Information("   - {EntityType}: {KeyValues}", entityType, keyValues);
                    //}
                    
                    await _context.SaveChangesAsync();

                    // 8. محاسبه خودکار قیمت خدمات (بعد از SaveChanges)
                    _logger.Information("📍 SYSTEM_SEED: مرحله 8 - محاسبه خودکار قیمت خدمات");
                    await _serviceSeedService.CalculateAndUpdateServicePricesAsync();

                    // 9. Commit Transaction
                    transaction.Commit();

                    var duration = DateTime.UtcNow - startTime;

                    _logger.Information("═══════════════════════════════════════════════");
                    _logger.Information("✅ SYSTEM_SEED: تمام داده‌های اولیه با موفقیت ایجاد شدند");
                    _logger.Information("⏱️ SYSTEM_SEED: مدت زمان: {Duration:F2} ثانیه", duration.TotalSeconds);
                    _logger.Information("═══════════════════════════════════════════════");
                }
                catch (Exception ex)
                {
                    // Rollback در صورت خطا - با بررسی وضعیت Transaction
                    try
                    {
                        if (transaction != null && transaction.UnderlyingTransaction != null)
                        {
                            transaction.Rollback();
                            _logger.Warning("⚠️ SYSTEM_SEED: Transaction با موفقیت Rollback شد");
                        }
                        else
                        {
                            _logger.Warning("⚠️ SYSTEM_SEED: Transaction قبلاً Rollback شده یا Connection قطع شده");
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.Error(rollbackEx, "❌ SYSTEM_SEED: خطا در Rollback Transaction");
                    }

                    _logger.Error("═══════════════════════════════════════════════");
                    _logger.Error(ex, "❌ SYSTEM_SEED: خطا در ایجاد داده‌های اولیه - Rollback انجام شد");
                    _logger.Error("═══════════════════════════════════════════════");

                    throw new InvalidOperationException("خطا در ایجاد داده‌های اولیه سیستم. تمام تغییرات Rollback شدند.", ex);
                }
            }
        }

        /// <summary>
        /// ایجاد داده‌های اولیه به صورت مرحله‌ای با Structured Logging
        /// </summary>
        public async Task SeedDataStepByStepAsync()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _logger.Information("═══════════════════════════════════════════════");
                    _logger.Information("🔄 SYSTEM_SEED: شروع ایجاد مرحله‌ای داده‌های اولیه");
                    _logger.Information("═══════════════════════════════════════════════");

                    var startTime = DateTime.UtcNow;

                    // مقداردهی SystemUsers
                    _logger.Information("📍 SYSTEM_SEED: مرحله 0 - SystemUsers");
                    SystemUsers.Initialize(_context);

                    // مرحله 1: کای‌ها
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 1/6 - کای‌های پایه");
                    await _factorSeedService.SeedFactorSettingsAsync();
                    await Task.Delay(500); // تاخیر برای نمایش بهتر

                    // مرحله 2: قالب‌های خدمات
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 2/6 - قالب‌های خدمات");
                    await _serviceTemplateSeedService.SeedServiceTemplatesAsync();
                    await Task.Delay(500);

                    // مرحله 3: خدمات
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 3/6 - خدمات نمونه");
                    await _serviceSeedService.SeedSampleServicesAsync();
                    await Task.Delay(500);

                    // مرحله 4: اجزای خدمات
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 4/6 - اجزای خدمات");
                    await _serviceSeedService.SeedServiceComponentsAsync();
                    await Task.Delay(500);

                    // مرحله 5: خدمات مشترک
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 5/6 - خدمات مشترک");
                    await _serviceSeedService.SeedSharedServicesAsync();

                    // ذخیره و Commit
                    _logger.Information("💾 SYSTEM_SEED: ذخیره تغییرات...");
                    await _context.SaveChangesAsync();

                    // مرحله 6: محاسبه قیمت خدمات (بعد از SaveChanges)
                    _logger.Information("🔄 SYSTEM_SEED: مرحله 6/6 - محاسبه قیمت خدمات");
                    await _serviceSeedService.CalculateAndUpdateServicePricesAsync();

                    transaction.Commit();

                    var duration = DateTime.UtcNow - startTime;

                    _logger.Information("═══════════════════════════════════════════════");
                    _logger.Information("✅ SYSTEM_SEED: تمام مراحل با موفقیت تکمیل شد");
                    _logger.Information("⏱️ SYSTEM_SEED: مدت زمان: {Duration:F2} ثانیه", duration.TotalSeconds);
                    _logger.Information("═══════════════════════════════════════════════");
                }
                catch (Exception ex)
                {
                    // Rollback در صورت خطا - با بررسی وضعیت Transaction
                    try
                    {
                        if (transaction != null && transaction.UnderlyingTransaction != null)
                        {
                            transaction.Rollback();
                            _logger.Warning("⚠️ SYSTEM_SEED: Transaction با موفقیت Rollback شد");
                        }
                        else
                        {
                            _logger.Warning("⚠️ SYSTEM_SEED: Transaction قبلاً Rollback شده یا Connection قطع شده");
                        }
                    }
                    catch (Exception rollbackEx)
                    {
                        _logger.Error(rollbackEx, "❌ SYSTEM_SEED: خطا در Rollback Transaction");
                    }

                    _logger.Error(ex, "❌ SYSTEM_SEED: خطا در ایجاد مرحله‌ای - Rollback انجام شد");
                    throw new InvalidOperationException("خطا در ایجاد مرحله‌ای داده‌های اولیه. Rollback انجام شد.", ex);
                }
            }
        }

        /// <summary>
        /// بررسی وضعیت داده‌های اولیه با Structured Logging
        /// </summary>
        public async Task<SeedDataStatus> GetSeedDataStatusAsync()
        {
            try
            {
                _logger.Debug("🔍 SYSTEM_SEED: شروع بررسی وضعیت داده‌های اولیه");

                var status = new SeedDataStatus();

                // بررسی کای‌ها
                status.FactorsExist = await _factorSeedService.ValidateRequiredFactorsAsync();
                _logger.Debug("📊 SYSTEM_SEED: وضعیت کای‌ها: {FactorsExist}", status.FactorsExist);

                // بررسی خدمات
                status.ServicesExist = await _serviceSeedService.ValidateSeededDataAsync();
                _logger.Debug("📊 SYSTEM_SEED: وضعیت خدمات: {ServicesExist}", status.ServicesExist);

                // شمارش رکوردها
                status.FactorSettingsCount = await _context.FactorSettings
                    .Where(f => !f.IsDeleted)
                    .CountAsync();

                status.ServicesCount = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .CountAsync();

                status.SharedServicesCount = await _context.SharedServices
                    .Where(ss => !ss.IsDeleted && ss.IsActive)
                    .CountAsync();

                status.ServiceComponentsCount = await _context.ServiceComponents
                    .Where(sc => !sc.IsDeleted && sc.IsActive)
                    .CountAsync();

                status.ServiceTemplatesCount = await _context.ServiceTemplates
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .CountAsync();

                status.IsComplete = status.FactorsExist && status.ServicesExist;

                _logger.Information("✅ SYSTEM_SEED: وضعیت - کامل: {IsComplete}, کای‌ها: {Factors}, خدمات: {Services}, اجزا: {Components}, مشترک: {Shared}, قالب‌ها: {Templates}",
                    status.IsComplete, status.FactorSettingsCount, status.ServicesCount, 
                    status.ServiceComponentsCount, status.SharedServicesCount, status.ServiceTemplatesCount);

                return status;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SYSTEM_SEED: خطا در بررسی وضعیت داده‌های اولیه");
                return new SeedDataStatus { IsComplete = false };
            }
        }

        /// <summary>
        /// پاک کردن داده‌های اولیه (برای تست) با Transaction - نسخه اجباری
        /// </summary>
        public async Task ClearSeedDataAsync()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _logger.Warning("═══════════════════════════════════════════════");
                    _logger.Warning("⚠️ SYSTEM_SEED: شروع پاک کردن اجباری داده‌های اولیه");
                    _logger.Warning("═══════════════════════════════════════════════");

                    // پاک کردن اجباری تمام خدمات مشترک (بدون شرط CreatedByUserId)
                    var allSharedServices = await _context.SharedServices.ToListAsync();
                    if (allSharedServices.Any())
                    {
                        _context.SharedServices.RemoveRange(allSharedServices);
                        _logger.Warning("🗑️ SYSTEM_SEED: حذف اجباری {Count} خدمت مشترک", allSharedServices.Count);
                    }

                    // پاک کردن اجباری تمام اجزای خدمات
                    var allServiceComponents = await _context.ServiceComponents.ToListAsync();
                    if (allServiceComponents.Any())
                    {
                        _context.ServiceComponents.RemoveRange(allServiceComponents);
                        _logger.Warning("🗑️ SYSTEM_SEED: حذف اجباری {Count} جزء خدمت", allServiceComponents.Count);
                    }

                    // پاک کردن اجباری تمام خدمات
                    var allServices = await _context.Services.ToListAsync();
                    if (allServices.Any())
                    {
                        _context.Services.RemoveRange(allServices);
                        _logger.Warning("🗑️ SYSTEM_SEED: حذف اجباری {Count} خدمت", allServices.Count);
                    }

                    // پاک کردن اجباری تمام قالب‌های خدمات
                    var allTemplates = await _context.ServiceTemplates.ToListAsync();
                    if (allTemplates.Any())
                    {
                        _context.ServiceTemplates.RemoveRange(allTemplates);
                        _logger.Warning("🗑️ SYSTEM_SEED: حذف اجباری {Count} قالب خدمت", allTemplates.Count);
                    }

                    // پاک کردن اجباری تمام کای‌ها
                    var allFactors = await _context.FactorSettings.ToListAsync();
                    if (allFactors.Any())
                    {
                        _context.FactorSettings.RemoveRange(allFactors);
                        _logger.Warning("🗑️ SYSTEM_SEED: حذف اجباری {Count} کای", allFactors.Count);
                    }

                    // ذخیره و Commit
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    _logger.Warning("═══════════════════════════════════════════════");
                    _logger.Warning("✅ SYSTEM_SEED: داده‌های اولیه با موفقیت پاک شدند");
                    _logger.Warning("═══════════════════════════════════════════════");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.Error(ex, "❌ SYSTEM_SEED: خطا در پاک کردن داده‌های اولیه - Rollback انجام شد");
                    throw new InvalidOperationException("خطا در پاک کردن داده‌های اولیه. Rollback انجام شد.", ex);
                }
            }
        }
    }

    /// <summary>
    /// وضعیت داده‌های اولیه سیستم
    /// </summary>
    public class SeedDataStatus
    {
        /// <summary>
        /// آیا کای‌های مورد نیاز وجود دارند؟
        /// </summary>
        public bool FactorsExist { get; set; }

        /// <summary>
        /// آیا خدمات نمونه وجود دارند؟
        /// </summary>
        public bool ServicesExist { get; set; }

        /// <summary>
        /// آیا تمام داده‌های اولیه کامل هستند؟
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// تعداد کای‌های فعال
        /// </summary>
        public int FactorSettingsCount { get; set; }

        /// <summary>
        /// تعداد خدمات فعال
        /// </summary>
        public int ServicesCount { get; set; }

        /// <summary>
        /// تعداد خدمات مشترک فعال
        /// </summary>
        public int SharedServicesCount { get; set; }

        /// <summary>
        /// تعداد اجزای خدمات فعال
        /// </summary>
        public int ServiceComponentsCount { get; set; }

        /// <summary>
        /// تعداد قالب‌های خدمات فعال
        /// </summary>
        public int ServiceTemplatesCount { get; set; }
    }
}
