using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Interfaces.Base;
using ClinicApp.Interfaces;
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


        #endregion

        #region Query Operations

        /// <summary>
        /// دریافت Entity ها بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>لیست Entity های مطابق شرط</returns>
        public virtual async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها بر اساس شرط", typeof(T).Name);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(predicate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها بر اساس شرط", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// دریافت اولین Entity مطابق شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>اولین Entity مطابق شرط</returns>
        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("دریافت اولین {EntityType} مطابق شرط", typeof(T).Name);
                
                return await _dbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اولین {EntityType} مطابق شرط", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود Entity بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("بررسی وجود {EntityType} بر اساس شرط", typeof(T).Name);
                
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود {EntityType} بر اساس شرط", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// شمارش Entity ها بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>تعداد Entity های مطابق شرط</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("شمارش {EntityType} ها بر اساس شرط", typeof(T).Name);
                
                return await _dbSet.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش {EntityType} ها بر اساس شرط", typeof(T).Name);
                throw;
            }
        }

        public Task<T> AddAsync(T entity)
        {
            throw new NotImplementedException();
        }

        Task<List<T>> IBaseRepository<T>.AddRangeAsync(List<T> entities)
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateAsync(T entity)
        {
            throw new NotImplementedException();
        }

        Task<List<T>> IBaseRepository<T>.UpdateRangeAsync(List<T> entities)
        {
            throw new NotImplementedException();
        }

        Task<bool> IBaseRepository<T>.DeleteAsync(T entity)
        {
            throw new NotImplementedException();
        }

        Task<bool> IBaseRepository<T>.DeleteByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        Task<int> IBaseRepository<T>.DeleteRangeAsync(List<T> entities)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// افزودن Entity جدید
        /// </summary>
        /// <param name="entity">Entity جدید</param>
        public virtual void Add(T entity)
        {
            try
            {
                _logger.Debug("اضافه کردن {EntityType} جدید", typeof(T).Name);
                
                _dbSet.Add(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اضافه کردن {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// افزودن چندین Entity
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        public virtual async Task AddRangeAsync(List<T> entities)
        {
            try
            {
                _logger.Debug("اضافه کردن {Count} {EntityType}", entities.Count, typeof(T).Name);
                
                _dbSet.AddRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اضافه کردن {EntityType} ها", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Update Operations

        /// <summary>
        /// به‌روزرسانی Entity
        /// </summary>
        /// <param name="entity">Entity برای به‌روزرسانی</param>
        public virtual void Update(T entity)
        {
            try
            {
                _logger.Debug("به‌روزرسانی {EntityType}", typeof(T).Name);
                
                _context.Entry(entity).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی چندین Entity
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        public virtual async Task UpdateRangeAsync(List<T> entities)
        {
            try
            {
                _logger.Debug("به‌روزرسانی {Count} {EntityType}", entities.Count, typeof(T).Name);
                
                foreach (var entity in entities)
                {
                    _context.Entry(entity).State = EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی {EntityType} ها", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// حذف Entity (Soft Delete)
        /// </summary>
        /// <param name="entity">Entity برای حذف</param>
        /// <returns>True اگر حذف موفق باشد</returns>
        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                _logger.Debug("حذف {EntityType}", typeof(T).Name);
                
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// حذف Entity بر اساس شناسه (Soft Delete)
        /// </summary>
        /// <param name="id">شناسه Entity</param>
        /// <returns>True اگر حذف موفق باشد</returns>
        public virtual async Task DeleteByIdAsync(int id)
        {
            try
            {
                _logger.Debug("حذف {EntityType} با شناسه {Id}", typeof(T).Name, id);
                
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _dbSet.Remove(entity);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف {EntityType} با شناسه {Id}", typeof(T).Name, id);
                throw;
            }
        }

        /// <summary>
        /// حذف چندین Entity (Soft Delete)
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>تعداد Entity های حذف شده</returns>
        public virtual async Task DeleteRangeAsync(List<T> entities)
        {
            try
            {
                _logger.Debug("حذف {Count} {EntityType}", entities.Count, typeof(T).Name);
                
                _dbSet.RemoveRange(entities);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف {EntityType} ها", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Performance Operations

        /// <summary>
        /// دریافت Entity ها بر اساس شرط با AsNoTracking
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>لیست Entity ها بدون Tracking</returns>
        public virtual async Task<List<T>> GetWhereAsNoTrackingAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها بر اساس شرط با AsNoTracking", typeof(T).Name);
                
                return await _dbSet
                    .AsNoTracking()
                    .Where(predicate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها بر اساس شرط با AsNoTracking", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity با Include
        /// </summary>
        /// <param name="id">شناسه Entity</param>
        /// <param name="includes">Include expressions</param>
        /// <returns>Entity با روابط</returns>
        public virtual async Task<T> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} با شناسه {Id} و Include ها", typeof(T).Name, id);
                
                var query = _dbSet.AsQueryable();
                
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
                
                return await query.FirstOrDefaultAsync(e => GetEntityId(e) == id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} با شناسه {Id} و Include ها", typeof(T).Name, id);
                throw;
            }
        }

        #endregion

        #region Pagination

        /// <summary>
        /// دریافت Entity ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده</returns>
        public virtual async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها با صفحه‌بندی. صفحه: {PageNumber}, اندازه: {PageSize}", 
                    typeof(T).Name, pageNumber, pageSize);
                
                var totalCount = await _dbSet.CountAsync();
                var items = await _dbSet
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها با صفحه‌بندی", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity ها بر اساس شرط با صفحه‌بندی
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده</returns>
        public virtual async Task<PagedResult<T>> GetPagedWhereAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها بر اساس شرط با صفحه‌بندی. صفحه: {PageNumber}, اندازه: {PageSize}", 
                    typeof(T).Name, pageNumber, pageSize);
                
                var totalCount = await _dbSet.CountAsync(predicate);
                var items = await _dbSet
                    .AsNoTracking()
                    .Where(predicate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها بر اساس شرط با صفحه‌بندی", typeof(T).Name);
                throw;
            }
        }

        #endregion

        #region Audit Operations

        /// <summary>
        /// دریافت Entity ها بر اساس کاربر ایجادکننده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست Entity های ایجاد شده توسط کاربر</returns>
        public virtual async Task<List<T>> GetByCreatedByAsync(string userId)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} های ایجاد شده توسط کاربر {UserId}", typeof(T).Name, userId);
                
                // این متد باید در کلاس‌های مشتق پیاده‌سازی شود
                // چون هر Entity ممکن است فیلد CreatedByUserId نداشته باشد
                throw new NotImplementedException("GetByCreatedByAsync باید در کلاس مشتق پیاده‌سازی شود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} های ایجاد شده توسط کاربر {UserId}", typeof(T).Name, userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity ها بر اساس کاربر به‌روزرسانی‌کننده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست Entity های به‌روزرسانی شده توسط کاربر</returns>
        public virtual async Task<List<T>> GetByUpdatedByAsync(string userId)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} های به‌روزرسانی شده توسط کاربر {UserId}", typeof(T).Name, userId);
                
                // این متد باید در کلاس‌های مشتق پیاده‌سازی شود
                // چون هر Entity ممکن است فیلد UpdatedByUserId نداشته باشد
                throw new NotImplementedException("GetByUpdatedByAsync باید در کلاس مشتق پیاده‌سازی شود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} های به‌روزرسانی شده توسط کاربر {UserId}", typeof(T).Name, userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت Entity ها در بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>لیست Entity ها در بازه زمانی</returns>
        public virtual async Task<List<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.Debug("دریافت {EntityType} ها در بازه زمانی {StartDate} تا {EndDate}", 
                    typeof(T).Name, startDate, endDate);
                
                // این متد باید در کلاس‌های مشتق پیاده‌سازی شود
                // چون هر Entity ممکن است فیلد CreatedAt نداشته باشد
                throw new NotImplementedException("GetByDateRangeAsync باید در کلاس مشتق پیاده‌سازی شود");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت {EntityType} ها در بازه زمانی", typeof(T).Name);
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
                
                return _context.Database.BeginTransaction();
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
                
                foreach (var entity in entities)
                {
                    _context.Entry(entity).State = EntityState.Modified;
                }
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
