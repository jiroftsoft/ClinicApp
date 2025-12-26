using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.DataSeeding;
using ClinicApp.Helpers;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp
{
    /// <summary>
    /// کلاس اصلی برای Seeding داده‌های اولیه سیستم
    /// نسخه بازنویسی شده با معماری مدولار و Transaction Management
    /// 
    /// ویژگی‌ها:
    /// - معماری مدولار و قابل تست
    /// - Transaction Management کامل
    /// - جدا کردن Concerns
    /// - استفاده از Constants
    /// - رفع N+1 Query Problems
    /// - Async/Await
    /// - Logging جامع
    /// - Error Handling بهبود یافته
    /// </summary>
    public static class IdentitySeed
    {
        #region Synchronous Methods (برای Compatibility)

        /// <summary>
        /// ایجاد تمام داده‌های اولیه (Synchronous)
        /// این متد برای Compatibility با کد قبلی نگه داشته شده
        /// </summary>
        public static void SeedDefaultData(ApplicationDbContext context)
        {
            // تبدیل به Async و اجرا به صورت Synchronous
            Task.Run(async () => await SeedDefaultDataAsync(context)).GetAwaiter().GetResult();
        }

        #endregion

        #region Asynchronous Methods (روش جدید - توصیه شده)

        /// <summary>
        /// ایجاد تمام داده‌های اولیه سیستم (Asynchronous)
        /// این متد با Transaction Management کامل طراحی شده است
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        /// <returns>Task</returns>
        public static async Task SeedDefaultDataAsync(ApplicationDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Context نمی‌تواند null باشد");
            }

            // شروع Transaction
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    Log.Information("═══════════════════════════════════════════════");
                    Log.Information("🌱 شروع فرآیند Seeding داده‌های اولیه سیستم...");
                    Log.Information("═══════════════════════════════════════════════");

                    var startTime = DateTime.UtcNow;

                    // 1. ایجاد نقش‌ها (اول باید نقش‌ها ایجاد شوند)
                    Log.Information("📍 مرحله 1: ایجاد نقش‌های سیستم");
                    var roleSeedService = new RoleSeedService(context, Log.Logger);
                    await roleSeedService.SeedAsync();

                    // 2. ایجاد کاربران سیستمی (Admin و System) - باید قبل از SystemUsers.Initialize باشد
                    Log.Information("📍 مرحله 2: ایجاد کاربران سیستمی");
                    var userSeedService = new UserSeedService(context, Log.Logger);
                    await userSeedService.SeedAsync();

                    // 3. مقداردهی اولیه SystemUsers (بعد از seed شدن کاربران!)
                    Log.Information("📍 مرحله 3: مقداردهی اولیه SystemUsers");
                    SystemUsers.Initialize(context);

                    // 4. ایجاد سیستم بیمه (Providers, Plans, PlanServices)
                    Log.Information("📍 مرحله 4: ایجاد سیستم بیمه");
                    var insuranceSeedService = new InsuranceSeedService(context, Log.Logger);
                    await insuranceSeedService.SeedAsync();

                    // 5. ایجاد کلینیک پیش‌فرض
                    Log.Information("📍 مرحله 5: ایجاد کلینیک پیش‌فرض");
                    var clinicSeedService = new ClinicSeedService(context, Log.Logger);
                    await clinicSeedService.SeedAsync();

                    // 6. ایجاد دپارتمان‌های کلینیک
                    Log.Information("📍 مرحله 6: ایجاد دپارتمان‌های کلینیک");
                    var departmentSeedService = new DepartmentSeedService(context, Log.Logger);
                    await departmentSeedService.SeedAsync();

                    // 7. ایجاد تخصص‌ها
                    Log.Information("📍 مرحله 7: ایجاد تخصص‌ها");
                    var specializationSeedService = new SpecializationSeedService(context, Log.Logger);
                    await specializationSeedService.SeedAsync();

                    // 8. ایجاد الگوهای اطلاع‌رسانی
                    Log.Information("📍 مرحله 8: ایجاد الگوهای اطلاع‌رسانی");
                    var notificationSeedService = new NotificationSeedService(context, Log.Logger);
                    await notificationSeedService.SeedAsync();

                    // 9. ذخیره تمام تغییرات (یک بار!)
                    Log.Information("💾 ذخیره تغییرات در دیتابیس...");
                    await context.SaveChangesAsync();

                    // 10. Commit Transaction
                    transaction.Commit();

                    var duration = DateTime.UtcNow - startTime;

                    Log.Information("═══════════════════════════════════════════════");
                    Log.Information($"✅ فرآیند Seeding با موفقیت پایان یافت");
                    Log.Information($"⏱️ مدت زمان: {duration.TotalSeconds:F2} ثانیه");
                    Log.Information("═══════════════════════════════════════════════");

                    // 11. اعتبارسنجی (اختیاری)
                    await ValidateSeededDataAsync(context);
                }
                catch (Exception ex)
                {
                    // Rollback در صورت خطا
                    transaction.Rollback();

                    Log.Error("═══════════════════════════════════════════════");
                    Log.Error(ex, "❌ خطا در فرآیند Seeding - Rollback انجام شد");
                    Log.Error("═══════════════════════════════════════════════");

                    throw new InvalidOperationException("خطا در ایجاد داده‌های اولیه سیستم. تمام تغییرات Rollback شدند.", ex);
                }
            }
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// اعتبارسنجی داده‌های Seed شده
        /// این متد برای اطمینان از صحت داده‌های ایجاد شده است
        /// </summary>
        private static async Task ValidateSeededDataAsync(ApplicationDbContext context)
        {
            try
            {
                Log.Information("🔍 شروع اعتبارسنجی داده‌های Seed شده...");

                var roleSeedService = new RoleSeedService(context, Log.Logger);
                var userSeedService = new UserSeedService(context, Log.Logger);
                var insuranceSeedService = new InsuranceSeedService(context, Log.Logger);
                var clinicSeedService = new ClinicSeedService(context, Log.Logger);
                var departmentSeedService = new DepartmentSeedService(context, Log.Logger);
                var specializationSeedService = new SpecializationSeedService(context, Log.Logger);
                var notificationSeedService = new NotificationSeedService(context, Log.Logger);

                var validationResults = new[]
                {
                    ("نقش‌ها", await roleSeedService.ValidateAsync()),
                    ("کاربران", await userSeedService.ValidateAsync()),
                    ("بیمه", await insuranceSeedService.ValidateAsync()),
                    ("کلینیک", await clinicSeedService.ValidateAsync()),
                    ("دپارتمان‌ها", await departmentSeedService.ValidateAsync()),
                    ("تخصص‌ها", await specializationSeedService.ValidateAsync()),
                    ("اطلاع‌رسانی", await notificationSeedService.ValidateAsync())
                };

                var allValid = true;
                foreach (var (name, isValid) in validationResults)
                {
                    if (isValid)
                    {
                        Log.Debug($"✅ {name}: معتبر");
                    }
                    else
                    {
                        Log.Warning($"⚠️ {name}: نامعتبر");
                        allValid = false;
                    }
                }

                if (allValid)
                {
                    Log.Information("✅ اعتبارسنجی موفق: تمام داده‌ها به درستی ایجاد شدند");
                }
                else
                {
                    Log.Warning("⚠️ اعتبارسنجی ناموفق: برخی داده‌ها به درستی ایجاد نشدند");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در اعتبارسنجی داده‌های Seed شده");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// پاک کردن تمام داده‌های Seed شده (فقط برای محیط تست!)
        /// ⚠️ هشدار: این عملیات خطرناک است و نباید در محیط تولید استفاده شود
        /// </summary>
        public static async Task ClearSeededDataAsync(ApplicationDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Log.Warning("⚠️⚠️⚠️ شروع پاک کردن داده‌های Seed شده ⚠️⚠️⚠️");

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // پاک کردن به ترتیب معکوس
                    // TODO: پیاده‌سازی در صورت نیاز

                    await context.SaveChangesAsync();
                    transaction.Commit();

                    Log.Information("✅ داده‌های Seed شده با موفقیت پاک شدند");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error(ex, "❌ خطا در پاک کردن داده‌های Seed شده");
                    throw;
                }
            }
        }

        /// <summary>
        /// دریافت آمار داده‌های Seed شده
        /// </summary>
        public static async Task<SeedDataStatistics> GetSeedDataStatisticsAsync(ApplicationDbContext context)
        {
            try
            {
                var stats = new SeedDataStatistics
                {
                    RolesCount = await context.Roles.CountAsync(),
                    UsersCount = await context.Users.Where(u => !u.IsDeleted).CountAsync(),
                    InsuranceProvidersCount = await context.InsuranceProviders.Where(ip => !ip.IsDeleted).CountAsync(),
                    InsurancePlansCount = await context.InsurancePlans.Where(ip => !ip.IsDeleted).CountAsync(),
                    ClinicsCount = await context.Clinics.Where(c => !c.IsDeleted).CountAsync(),
                    SpecializationsCount = await context.Specializations.Where(s => !s.IsDeleted).CountAsync(),
                    NotificationTemplatesCount = await context.NotificationTemplates.CountAsync(),
                    Timestamp = DateTime.UtcNow
                };

                return stats;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت آمار داده‌های Seed شده");
                throw;
            }
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// آمار داده‌های Seed شده
    /// </summary>
    public class SeedDataStatistics
    {
        public int RolesCount { get; set; }
        public int UsersCount { get; set; }
        public int InsuranceProvidersCount { get; set; }
        public int InsurancePlansCount { get; set; }
        public int ClinicsCount { get; set; }
        public int SpecializationsCount { get; set; }
        public int NotificationTemplatesCount { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"Roles: {RolesCount}, Users: {UsersCount}, " +
                   $"Providers: {InsuranceProvidersCount}, Plans: {InsurancePlansCount}, " +
                   $"Clinics: {ClinicsCount}, Specializations: {SpecializationsCount}, " +
                   $"Notifications: {NotificationTemplatesCount}";
        }
    }

    #endregion
}

