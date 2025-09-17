using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
        /// ایجاد تمام داده‌های اولیه سیستم
        /// </summary>
        public async Task SeedAllDataAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد داده‌های اولیه سیستم");

                // 0. مقداردهی اولیه SystemUsers (اولویت اول)
                _logger.Information("مرحله 0: مقداردهی اولیه SystemUsers");
                SystemUsers.Initialize(_context);

                // 1. ایجاد کای‌های پایه
                _logger.Information("مرحله 1: ایجاد کای‌های پایه");
                await _factorSeedService.SeedFactorSettingsAsync();
                await _factorSeedService.SeedPreviousYearFactorsAsync();

                // 2. ایجاد قالب‌های خدمات (بهترین روش)
                _logger.Information("مرحله 2: ایجاد قالب‌های خدمات");
                await _serviceTemplateSeedService.SeedServiceTemplatesAsync();

                // 3. ایجاد خدمات نمونه
                _logger.Information("مرحله 3: ایجاد خدمات نمونه");
                await _serviceSeedService.SeedSampleServicesAsync();

                // 4. ایجاد اجزای خدمات
                _logger.Information("مرحله 4: ایجاد اجزای خدمات");
                await _serviceSeedService.SeedServiceComponentsAsync();

                // 5. ایجاد خدمات مشترک
                _logger.Information("مرحله 5: ایجاد خدمات مشترک");
                await _serviceSeedService.SeedSharedServicesAsync();

                // 6. اعتبارسنجی
                _logger.Information("مرحله 6: اعتبارسنجی داده‌ها");
                var factorsValid = await _factorSeedService.ValidateRequiredFactorsAsync();
                var servicesValid = await _serviceSeedService.ValidateSeededDataAsync();

                if (factorsValid && servicesValid)
                {
                    _logger.Information("✅ تمام داده‌های اولیه با موفقیت ایجاد شدند");
                }
                else
                {
                    _logger.Warning("⚠️ برخی از داده‌های اولیه به درستی ایجاد نشدند");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد داده‌های اولیه سیستم");
                throw;
            }
        }

        /// <summary>
        /// ایجاد داده‌های اولیه به صورت مرحله‌ای
        /// </summary>
        public async Task SeedDataStepByStepAsync()
        {
            try
            {
                _logger.Information("شروع ایجاد داده‌های اولیه به صورت مرحله‌ای");

                // مرحله 1: کای‌ها
                _logger.Information("🔄 مرحله 1/4: ایجاد کای‌های پایه");
                await _factorSeedService.SeedFactorSettingsAsync();
                await Task.Delay(1000); // تاخیر برای نمایش بهتر

                // مرحله 2: خدمات
                _logger.Information("🔄 مرحله 2/4: ایجاد خدمات نمونه");
                await _serviceSeedService.SeedSampleServicesAsync();
                await Task.Delay(1000);

                // مرحله 3: اجزای خدمات
                _logger.Information("🔄 مرحله 3/4: ایجاد اجزای خدمات");
                await _serviceSeedService.SeedServiceComponentsAsync();
                await Task.Delay(1000);

                // مرحله 4: خدمات مشترک
                _logger.Information("🔄 مرحله 4/4: ایجاد خدمات مشترک");
                await _serviceSeedService.SeedSharedServicesAsync();

                _logger.Information("✅ تمام مراحل با موفقیت تکمیل شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد داده‌های اولیه به صورت مرحله‌ای");
                throw;
            }
        }

        /// <summary>
        /// بررسی وضعیت داده‌های اولیه
        /// </summary>
        public async Task<SeedDataStatus> GetSeedDataStatusAsync()
        {
            try
            {
                var status = new SeedDataStatus();

                // بررسی کای‌ها
                status.FactorsExist = await _factorSeedService.ValidateRequiredFactorsAsync();

                // بررسی خدمات
                status.ServicesExist = await _serviceSeedService.ValidateSeededDataAsync();

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

                status.IsComplete = status.FactorsExist && status.ServicesExist;

                return status;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت داده‌های اولیه");
                return new SeedDataStatus { IsComplete = false };
            }
        }

        /// <summary>
        /// پاک کردن داده‌های اولیه (برای تست)
        /// </summary>
        public async Task ClearSeedDataAsync()
        {
            try
            {
                _logger.Warning("شروع پاک کردن داده‌های اولیه");

                // پاک کردن خدمات مشترک
                var sharedServices = await _context.SharedServices
                    .Where(ss => ss.CreatedByUserId == "system-seed")
                    .ToListAsync();
                _context.SharedServices.RemoveRange(sharedServices);

                // پاک کردن اجزای خدمات
                var serviceComponents = await _context.ServiceComponents
                    .Where(sc => sc.CreatedByUserId == "system-seed")
                    .ToListAsync();
                _context.ServiceComponents.RemoveRange(serviceComponents);

                // پاک کردن خدمات
                var services = await _context.Services
                    .Where(s => s.CreatedByUserId == "system-seed")
                    .ToListAsync();
                _context.Services.RemoveRange(services);

                // پاک کردن کای‌ها
                var factors = await _context.FactorSettings
                    .Where(f => f.CreatedByUserId == "system-seed" || f.CreatedByUserId == _currentUserService.UserId)
                    .ToListAsync();
                _context.FactorSettings.RemoveRange(factors);

                await _context.SaveChangesAsync();
                _logger.Information("داده‌های اولیه با موفقیت پاک شدند");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن داده‌های اولیه");
                throw;
            }
        }
    }

    /// <summary>
    /// وضعیت داده‌های اولیه
    /// </summary>
    public class SeedDataStatus
    {
        public bool FactorsExist { get; set; }
        public bool ServicesExist { get; set; }
        public bool IsComplete { get; set; }
        public int FactorSettingsCount { get; set; }
        public int ServicesCount { get; set; }
        public int SharedServicesCount { get; set; }
        public int ServiceComponentsCount { get; set; }
    }
}
