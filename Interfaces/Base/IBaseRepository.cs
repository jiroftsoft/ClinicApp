using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Base
{
    /// <summary>
    /// Base Interface برای تمام Repository ها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations استاندارد
    /// 2. Performance Optimization
    /// 3. Transaction Management
    /// 4. Bulk Operations
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط دسترسی به داده
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    /// <typeparam name="T">نوع Entity</typeparam>
    public interface IBaseRepository<T> : IDisposable where T : class
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت Entity با شناسه
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>Entity</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// دریافت تمام Entity ها
        /// </summary>
        /// <returns>لیست Entity ها</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// اضافه کردن Entity جدید
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity اضافه شده</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// به‌روزرسانی Entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>Entity به‌روزرسانی شده</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// حذف Entity
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// بررسی وجود Entity
        /// </summary>
        /// <param name="id">شناسه</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// شمارش Entity ها
        /// </summary>
        /// <returns>تعداد Entity ها</returns>
        Task<int> CountAsync();

        #endregion

        #region Performance Optimization Methods

        /// <summary>
        /// دریافت Entity ها با AsNoTracking
        /// </summary>
        /// <returns>لیست Entity ها</returns>
        Task<List<T>> GetAllAsNoTrackingAsync();

        /// <summary>
        /// دریافت Entity ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست Entity ها</returns>
        Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// دریافت Entity ها با مرتب‌سازی
        /// </summary>
        /// <param name="orderBy">فیلد مرتب‌سازی</param>
        /// <param name="isDescending">نزولی</param>
        /// <returns>لیست Entity ها</returns>
        Task<List<T>> GetOrderedAsync(string orderBy, bool isDescending = false);

        #endregion

        #region Transaction Management Methods

        /// <summary>
        /// شروع Transaction
        /// </summary>
        /// <returns>Transaction</returns>
        Task<System.Data.Entity.DbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        /// <returns>تعداد رکوردهای تأثیرپذیر</returns>
        Task<int> SaveChangesAsync();

        #endregion

        #region Bulk Operations Methods

        /// <summary>
        /// اضافه کردن دسته‌ای Entity ها
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>نتیجه اضافه کردن</returns>
        Task<bool> AddBulkAsync(List<T> entities);

        /// <summary>
        /// به‌روزرسانی دسته‌ای Entity ها
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>نتیجه به‌روزرسانی</returns>
        Task<bool> UpdateBulkAsync(List<T> entities);

        /// <summary>
        /// حذف دسته‌ای Entity ها
        /// </summary>
        /// <param name="ids">لیست شناسه‌ها</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> DeleteBulkAsync(List<int> ids);

        #endregion
    }
}
