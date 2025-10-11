using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ClinicApp.Core;

namespace ClinicApp.Interfaces.Base
{
    /// <summary>
    /// Base Repository Interface برای تمام Repository ها
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
    public interface IBaseRepository<T> where T : class
    {
        #region Core CRUD Operations

        /// <summary>
        /// دریافت Entity بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه Entity</param>
        /// <returns>Entity مورد نظر</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// دریافت تمام Entity های فعال
        /// </summary>
        /// <returns>لیست Entity های فعال</returns>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// دریافت Entity ها بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>لیست Entity های مطابق شرط</returns>
        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// دریافت اولین Entity مطابق شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>اولین Entity مطابق شرط</returns>
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// بررسی وجود Entity بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// شمارش Entity ها بر اساس شرط
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>تعداد Entity های مطابق شرط</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        #endregion

        #region Create Operations

        /// <summary>
        /// افزودن Entity جدید
        /// </summary>
        /// <param name="entity">Entity جدید</param>
        /// <returns>Entity افزوده شده</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// افزودن چندین Entity
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>لیست Entity های افزوده شده</returns>
        Task<List<T>> AddRangeAsync(List<T> entities);

        #endregion

        #region Update Operations

        /// <summary>
        /// به‌روزرسانی Entity
        /// </summary>
        /// <param name="entity">Entity برای به‌روزرسانی</param>
        /// <returns>Entity به‌روزرسانی شده</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// به‌روزرسانی چندین Entity
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>لیست Entity های به‌روزرسانی شده</returns>
        Task<List<T>> UpdateRangeAsync(List<T> entities);

        #endregion

        #region Delete Operations

        /// <summary>
        /// حذف Entity (Soft Delete)
        /// </summary>
        /// <param name="entity">Entity برای حذف</param>
        /// <returns>True اگر حذف موفق باشد</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// حذف Entity بر اساس شناسه (Soft Delete)
        /// </summary>
        /// <param name="id">شناسه Entity</param>
        /// <returns>True اگر حذف موفق باشد</returns>
        Task<bool> DeleteByIdAsync(int id);

        /// <summary>
        /// حذف چندین Entity (Soft Delete)
        /// </summary>
        /// <param name="entities">لیست Entity ها</param>
        /// <returns>تعداد Entity های حذف شده</returns>
        Task<int> DeleteRangeAsync(List<T> entities);

        #endregion

        #region Performance Operations

        /// <summary>
        /// دریافت Entity ها با AsNoTracking
        /// </summary>
        /// <returns>لیست Entity ها بدون Tracking</returns>
        Task<List<T>> GetAllAsNoTrackingAsync();

        /// <summary>
        /// دریافت Entity ها بر اساس شرط با AsNoTracking
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <returns>لیست Entity ها بدون Tracking</returns>
        Task<List<T>> GetWhereAsNoTrackingAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// دریافت Entity با Include
        /// </summary>
        /// <param name="id">شناسه Entity</param>
        /// <param name="includes">Include expressions</param>
        /// <returns>Entity با روابط</returns>
        Task<T> GetByIdWithIncludesAsync(int id, params Expression<Func<T, object>>[] includes);

        #endregion

        #region Pagination

        /// <summary>
        /// دریافت Entity ها با صفحه‌بندی
        /// </summary>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده</returns>
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// دریافت Entity ها بر اساس شرط با صفحه‌بندی
        /// </summary>
        /// <param name="predicate">شرط جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده</returns>
        Task<PagedResult<T>> GetPagedWhereAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);

        #endregion

        #region Audit Operations

        /// <summary>
        /// دریافت Entity ها بر اساس کاربر ایجادکننده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست Entity های ایجاد شده توسط کاربر</returns>
        Task<List<T>> GetByCreatedByAsync(string userId);

        /// <summary>
        /// دریافت Entity ها بر اساس کاربر به‌روزرسانی‌کننده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست Entity های به‌روزرسانی شده توسط کاربر</returns>
        Task<List<T>> GetByUpdatedByAsync(string userId);

        /// <summary>
        /// دریافت Entity ها در بازه زمانی
        /// </summary>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <returns>لیست Entity ها در بازه زمانی</returns>
        Task<List<T>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        #endregion
    }
}