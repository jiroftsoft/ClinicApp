using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using Serilog;

namespace ClinicApp.DataSeeding
{
    /// <summary>
    /// کلاس پایه برای تمام سرویس‌های Seeding
    /// این کلاس متدهای مشترک و Helper Methods را فراهم می‌کند
    /// </summary>
    public abstract class BaseSeedService
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger _logger;

        protected ApplicationUser _adminUser;
        protected ApplicationUser _systemUser;
        protected bool _systemUsersLoaded = false;

        protected BaseSeedService(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region دریافت کاربران سیستمی (Get System Users)

        /// <summary>
        /// بارگذاری کاربران سیستمی (Admin و System)
        /// این متد فقط یک بار اجرا می‌شود و نتیجه کش می‌شود
        /// </summary>
        protected virtual async Task LoadSystemUsersAsync()
        {
            if (_systemUsersLoaded)
            {
                _logger.Debug("DATA_SEED: کاربران سیستمی قبلاً بارگذاری شده‌اند (از Cache استفاده می‌شود)");
                return;
            }

            _logger.Debug("DATA_SEED: شروع بارگذاری کاربران سیستمی...");

            _adminUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == SeedConstants.AdminUserName && !u.IsDeleted);

            _systemUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == SeedConstants.SystemUserName && !u.IsDeleted);

            if (_adminUser == null)
            {
                _logger.Error("DATA_SEED: کاربر ادمین یافت نشد. UserName: {AdminUserName}", 
                    SeedConstants.AdminUserName);
                throw new InvalidOperationException(
                    $"کاربر ادمین با نام کاربری '{SeedConstants.AdminUserName}' یافت نشد");
            }

            if (_systemUser == null)
            {
                _logger.Error("DATA_SEED: کاربر سیستم یافت نشد. UserName: {SystemUserName}", 
                    SeedConstants.SystemUserName);
                throw new InvalidOperationException(
                    $"کاربر سیستم با نام کاربری '{SeedConstants.SystemUserName}' یافت نشد");
            }

            _systemUsersLoaded = true;
            _logger.Debug("DATA_SEED: کاربران سیستمی با موفقیت بارگذاری شدند. AdminId: {AdminId}, SystemId: {SystemId}", 
                _adminUser.Id, _systemUser.Id);
        }

        /// <summary>
        /// دریافت کاربر ادمین
        /// </summary>
        protected virtual async Task<ApplicationUser> GetAdminUserAsync()
        {
            if (!_systemUsersLoaded)
            {
                await LoadSystemUsersAsync();
            }
            return _adminUser;
        }

        /// <summary>
        /// دریافت کاربر سیستم
        /// </summary>
        protected virtual async Task<ApplicationUser> GetSystemUserAsync()
        {
            if (!_systemUsersLoaded)
            {
                await LoadSystemUsersAsync();
            }
            return _systemUser;
        }

        #endregion

        #region بررسی تکراری نبودن (Check Duplicates)

        /// <summary>
        /// فیلتر کردن آیتم‌های تکراری به صورت Batch
        /// این متد برای جلوگیری از N+1 Query Problem طراحی شده است
        /// </summary>
        /// <typeparam name="TEntity">نوع Entity</typeparam>
        /// <typeparam name="TKey">نوع کلید</typeparam>
        /// <param name="newItems">لیست آیتم‌های جدید</param>
        /// <param name="keySelector">Selector برای دریافت کلید از آیتم جدید (در Memory)</param>
        /// <param name="existingItemsQuery">Query برای دریافت آیتم‌های موجود</param>
        /// <param name="existingKeySelector">Selector برای دریافت کلید از آیتم موجود (در Database - باید Expression باشد)</param>
        /// <returns>لیست آیتم‌های جدیدی که تکراری نیستند</returns>
        protected virtual async Task<List<TEntity>> FilterExistingItemsAsync<TEntity, TKey>(
            List<TEntity> newItems,
            Func<TEntity, TKey> keySelector,
            IQueryable<TEntity> existingItemsQuery,
            Expression<Func<TEntity, TKey>> existingKeySelector)
        {
            if (newItems == null || !newItems.Any())
            {
                return new List<TEntity>();
            }

            try
            {
                _logger.Debug("DATA_SEED: شروع فیلتر کردن آیتم‌های تکراری. تعداد آیتم‌های جدید: {NewItemsCount}", 
                    newItems.Count);

                // دریافت کلیدهای آیتم‌های جدید
                var newKeys = newItems.Select(keySelector).ToList();

                // یک بار Query برای دریافت کلیدهای موجود (رفع N+1 Problem)
                var existingKeys = await existingItemsQuery
                    .Select(existingKeySelector)
                    .ToListAsync();

                _logger.Debug("DATA_SEED: تعداد آیتم‌های موجود در Database: {ExistingItemsCount}", 
                    existingKeys.Count);

                // فیلتر کردن در Memory
                var nonDuplicateItems = newItems
                    .Where(item => !existingKeys.Contains(keySelector(item)))
                    .ToList();

                if (nonDuplicateItems.Count < newItems.Count)
                {
                    var duplicateCount = newItems.Count - nonDuplicateItems.Count;
                    _logger.Debug("DATA_SEED: تعداد {DuplicateCount} آیتم تکراری شناسایی و فیلتر شدند. " +
                        "آیتم‌های جدید: {NonDuplicateCount}", 
                        duplicateCount, nonDuplicateItems.Count);
                }
                else
                {
                    _logger.Debug("DATA_SEED: هیچ آیتم تکراری یافت نشد. تمام {Count} آیتم جدید هستند", 
                        nonDuplicateItems.Count);
                }

                return nonDuplicateItems;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DATA_SEED: خطا در فیلتر کردن آیتم‌های تکراری. " +
                    "تعداد آیتم‌های جدید: {NewItemsCount}", newItems.Count);
                throw;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا آیتمی با کلید مشخص وجود دارد یا نه
        /// </summary>
        protected virtual async Task<bool> ExistsAsync<TEntity, TKey>(
            IQueryable<TEntity> query,
            Expression<Func<TEntity, TKey>> keySelector,
            TKey key)
        {
            try
            {
                // دریافت تمام کلیدها و بررسی در Memory
                // این روش برای تعداد کم Entity مناسب است
                var keys = await query.Select(keySelector).ToListAsync();
                return keys.Contains(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود آیتم. Key: {@Key}", key);
                throw;
            }
        }

        #endregion

        #region عملیات Seed اصلی (Main Seed Operations)

        /// <summary>
        /// متد اصلی برای Seeding
        /// این متد باید در کلاس‌های فرزند پیاده‌سازی شود
        /// </summary>
        public abstract Task SeedAsync();

        /// <summary>
        /// اعتبارسنجی داده‌های Seed شده
        /// این متد برای بررسی صحت داده‌های ایجاد شده است
        /// </summary>
        public virtual async Task<bool> ValidateAsync()
        {
            // پیاده‌سازی پیش‌فرض
            return await Task.FromResult(true);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تنظیم Audit Trail برای Entity
        /// </summary>
        protected virtual void SetAuditTrail<TEntity>(TEntity entity, string userId) 
            where TEntity : class
        {
            var auditableEntity = entity as ITrackable;
            if (auditableEntity != null)
            {
                auditableEntity.CreatedAt = DateTime.UtcNow;
                auditableEntity.CreatedByUserId = userId;
            }
        }

        /// <summary>
        /// تنظیم Audit Trail برای لیستی از Entity ها
        /// </summary>
        protected virtual void SetAuditTrailForRange<TEntity>(IEnumerable<TEntity> entities, string userId) 
            where TEntity : class
        {
            foreach (var entity in entities)
            {
                SetAuditTrail(entity, userId);
            }
        }

        /// <summary>
        /// اضافه کردن لیست Entity ها به Context (با Structured Logging)
        /// </summary>
        protected virtual void AddRangeIfAny<TEntity>(List<TEntity> entities, string operationName) 
            where TEntity : class
        {
            if (entities == null || !entities.Any())
            {
                _logger.Debug("DATA_SEED: هیچ {OperationName} جدیدی برای اضافه کردن وجود ندارد", operationName);
                return;
            }

            _context.Set<TEntity>().AddRange(entities);
            _logger.Information("DATA_SEED: تعداد {Count} {OperationName} جدید به Context اضافه شد", 
                entities.Count, operationName);
        }

        /// <summary>
        /// لاگ شروع عملیات Seeding (Structured Logging)
        /// </summary>
        protected virtual void LogSeedStart(string entityName)
        {
            _logger.Information("🌱 DATA_SEED: شروع Seeding {EntityName}...", entityName);
        }

        /// <summary>
        /// لاگ پایان موفق عملیات Seeding (Structured Logging)
        /// </summary>
        protected virtual void LogSeedSuccess(string entityName, int count)
        {
            _logger.Information("✅ DATA_SEED: Seeding {EntityName} موفق. تعداد: {Count} آیتم جدید", 
                entityName, count);
        }

        /// <summary>
        /// لاگ خطا در عملیات Seeding (Structured Logging)
        /// </summary>
        protected virtual void LogSeedError(string entityName, Exception ex)
        {
            _logger.Error(ex, "❌ DATA_SEED: خطا در Seeding {EntityName}. نوع خطا: {ExceptionType}", 
                entityName, ex.GetType().Name);
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// بررسی null بودن و پرتاب Exception
        /// </summary>
        protected virtual void ThrowIfNull<T>(T obj, string paramName) where T : class
        {
            if (obj == null)
            {
                var error = $"پارامتر '{paramName}' نمی‌تواند null باشد";
                _logger.Error(error);
                throw new ArgumentNullException(paramName, error);
            }
        }

        /// <summary>
        /// بررسی خالی بودن رشته و پرتاب Exception
        /// </summary>
        protected virtual void ThrowIfNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var error = $"پارامتر '{paramName}' نمی‌تواند خالی باشد";
                _logger.Error(error);
                throw new ArgumentException(error, paramName);
            }
        }

        #endregion
    }
}

