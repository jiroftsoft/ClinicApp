using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Repositories.Base
{
    /// <summary>
    /// Base Repository Pattern برای تمام Repository ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations استاندارد
    /// 2. Performance Optimization
    /// 3. Error Handling
    /// 4. Logging Integration
    /// 5. Transaction Management
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط دسترسی به داده
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    /// <typeparam name="T">نوع Entity</typeparam>
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        #region Fields and Constructor

        protected readonly ApplicationDbContext _context;
        protected readonly ILogger _logger;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbSet = _context.Set<T>();
        }

        #endregion

        #region Core CRUD Operations

        /// <summary>
        /// دریافت Entity با شناسه
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>Entity</returns>
        public virtual async Task<T> GetByIdAsync(int id)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} با شناسه {Id}", typeof(T).Name, id);
                
                var entity = await _dbSet.FindAsync(id);
                
                if (entity == null)
                {
                    _logger.Warning("{EntityType} با شناسه {Id} یافت نشد", typeof(T).Name, id);
                }
                
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} با شناسه {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام Entity ها
        /// </summary>
        /// <returns>لیست Entity ها</returns>
        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                _logger.Debug("دریافت تمام {EntityType} ها", typeof(T).Name);
                
                return await _dbSet
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام {EntityType} ها", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// اضافه کردن Entity جدید
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity اضافه شده</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                _logger.Debug("اضافه کردن {EntityType} جدید", typeof(T).Name);
                
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                
                _logger.Information("{EntityType} با موفقیت اضافه شد", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اضافه کردن {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی Entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity به‌روزرسانی شده</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                _logger.Debug("به‌روزرسانی {EntityType}", typeof(T).Name);
                
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                
                _logger.Information("{EntityType} با موفقیت به‌روزرسانی شد", typeof(T).Name);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// حذف Entity
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>نتیجه حذف</returns>
        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.Debug("حذف {EntityType} با شناسه {Id}", typeof(T).Name, id);
                
                var entity = await GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.Warning("{EntityType} با شناسه {Id} یافت نشد", typeof(T).Name, id);
                    return false;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                
                _logger.Information("{EntityType} با شناسه {Id} با موفقیت حذف شد", typeof(T).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف {EntityType} با شناسه {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود Entity
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>نتیجه بررسی</returns>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                _logger.Debug("بررسی وجود {EntityType} با شناسه {Id}", typeof(T).Name, id);
                
                return await _dbSet.FindAsync(id) != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود {EntityType} با شناسه {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// شمارش Entity ها
        /// </summary>
        /// <returns>تعداد Entity ها</returns>
        public virtual async Task<int> CountAsync()
        {
            try
            {
                _logger.Debug("شمارش {EntityType} ها", typeof(T).Name);
                
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش {EntityType} ها", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Performance Optimization Methods

        /// <summary>
        /// دریافت Entity ها با AsNoTracking
        /// </summary>
        /// <returns>لیست Entity ها</returns>
        public virtual async Task<List<T>> GetAllAsNoTrackingAsync()
        {
            try
            {
                _logger.Debug("دریافت تمام {EntityType} ها با AsNoTracking", typeof(T).Name);
                
                return await _dbSet
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام {EntityType} ها با AsNoTracking", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست Entity ها</returns>
        public virtual async Task<List<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها با صفحه‌بندی. صفحه: {PageNumber}, اندازه: {PageSize}", 
                    typeof(T).Name, pageNumber, pageSize);
                
                return await _dbSet
                    .AsNoTracking()
                    .Skip(pageNumber * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها با صفحه‌بندی", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity ها با مرتب‌سازی
        /// </summary>
        /// <param name="orderBy">فیلد مرتب‌سازی</param>
        /// <param name="isDescending">نزولی</param>
        /// <returns>لیست Entity ها</returns>
        public virtual async Task<List<T>> GetOrderedAsync(string orderBy, bool isDescending = false)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها با مرتب‌سازی. فیلد: {OrderBy}, نزولی: {IsDescending}", 
                    typeof(T).Name, orderBy, isDescending);
                
                var query = _dbSet.AsNoTracking();
                
                // TODO: Implement dynamic ordering
                // This would require reflection or expression trees
                
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها با مرتب‌سازی", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Transaction Management Methods

        /// <summary>
        /// شروع Transaction
        /// </summary>
        /// <returns>Transaction</returns>
        public virtual async Task<System.Data.Entity.DbContextTransaction> BeginTransactionAsync()
        {
            try
            {
                _logger.Debug("شروع Transaction برای {EntityType}", typeof(T).Name);
                
                return await _context.Database.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع Transaction برای {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        /// <returns>تعداد رکوردهای تأثیرپذیر</returns>
        public virtual async Task<int> SaveChangesAsync()
        {
            try
            {
                _logger.Debug("ذخیره تغییرات برای {EntityType}", typeof(T).Name);
                
                var result = await _context.SaveChangesAsync();
                
                _logger.Information("{Count} رکورد برای {EntityType} ذخیره شد", result, typeof(T).Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات برای {EntityType}", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Bulk Operations Methods

        /// <summary>
        /// اضافه کردن دسته‌ای Entity ها
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>نتیجه اضافه کردن</returns>
        public virtual async Task<bool> AddBulkAsync(List<T> entities)
        {
            try
            {
                _logger.Debug("اضافه کردن دسته‌ای {Count} {EntityType}", entities.Count, typeof(T).Name);
                
                _dbSet.AddRange(entities);
                await _context.SaveChangesAsync();
                
                _logger.Information("{Count} {EntityType} با موفقیت اضافه شد", entities.Count, typeof(T).Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اضافه کردن دسته‌ای {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی دسته‌ای Entity ها
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        public virtual async Task<bool> UpdateBulkAsync(List<T> entities)
        {
            try
            {
                _logger.Debug("به‌روزرسانی دسته‌ای {Count} {EntityType}", entities.Count, typeof(T).Name);
                
                _dbSet.UpdateRange(entities);
                await _context.SaveChangesAsync();
                
                _logger.Information("{Count} {EntityType} با موفقیت به‌روزرسانی شد", entities.Count, typeof(T).Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی دسته‌ای {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// حذف دسته‌ای Entity ها
        /// </summary>
        /// <param name="ids">لیست شناسه‌ها</param>
        /// <returns>نتیجه حذف</returns>
        public virtual async Task<bool> DeleteBulkAsync(List<int> ids)
        {
            try
            {
                _logger.Debug("حذف دسته‌ای {Count} {EntityType}", ids.Count, typeof(T).Name);
                
                var entities = await _dbSet.Where(e => ids.Contains(GetEntityId(e))).ToListAsync();
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync();
                
                _logger.Information("{Count} {EntityType} با موفقیت حذف شد", entities.Count, typeof(T).Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف دسته‌ای {EntityType}", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت شناسه Entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>شناسه</returns>
        protected virtual int GetEntityId(T entity)
        {
            // This method should be overridden in derived classes
            // to provide the correct ID property
            throw new NotImplementedException("GetEntityId method must be overridden in derived classes");
        }

        /// <summary>
        /// بررسی اعتبار Entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        protected virtual bool ValidateEntity(T entity)
        {
            if (entity == null)
            {
                _logger.Warning("Entity {EntityType} نمی‌تواند null باشد", typeof(T).Name);
                return false;
            }
            
            return true;
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// آزادسازی منابع
        /// </summary>
        public virtual void Dispose()
        {
            _context?.Dispose();
        }

        #endregion
    }
}
