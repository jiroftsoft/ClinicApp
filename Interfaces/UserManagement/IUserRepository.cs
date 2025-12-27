using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Models.Core;

namespace ClinicApp.Interfaces.UserManagement
{
    /// <summary>
    /// Repository Interface برای مدیریت کاربران سیستم
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations کامل
    /// 2. Search & Filter با Pagination
    /// 3. Role Management
    /// 4. Soft Delete Support
    /// 5. Audit Trail Support
    /// 6. Performance Optimization
    /// </summary>
    public interface IUserRepository
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت کاربر با شناسه
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>ApplicationUser یا null</returns>
        Task<ApplicationUser> GetByIdAsync(string id);

        /// <summary>
        /// دریافت کاربر با کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>ApplicationUser یا null</returns>
        Task<ApplicationUser> GetByNationalCodeAsync(string nationalCode);

        /// <summary>
        /// دریافت کاربر با ایمیل
        /// </summary>
        /// <param name="email">ایمیل</param>
        /// <returns>ApplicationUser یا null</returns>
        Task<ApplicationUser> GetByEmailAsync(string email);

        /// <summary>
        /// دریافت تمام کاربران (فقط فعال)
        /// </summary>
        /// <returns>لیست کاربران</returns>
        Task<List<ApplicationUser>> GetAllAsync();

        /// <summary>
        /// دریافت کاربران فعال
        /// </summary>
        /// <returns>لیست کاربران فعال</returns>
        Task<List<ApplicationUser>> GetActiveUsersAsync();

        /// <summary>
        /// دریافت کاربران حذف شده (Soft Delete)
        /// </summary>
        /// <returns>لیست کاربران حذف شده</returns>
        Task<List<ApplicationUser>> GetDeletedUsersAsync();

        #endregion

        #region Search & Filter

        /// <summary>
        /// جستجو و فیلتر کاربران با Pagination
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو (نام، نام خانوادگی، کد ملی، ایمیل)</param>
        /// <param name="isActive">فیلتر وضعیت فعال (null = همه)</param>
        /// <param name="roleName">فیلتر نقش (null = همه)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد در هر صفحه</param>
        /// <returns>نتیجه صفحه‌بندی شده</returns>
        Task<PagedResult<ApplicationUser>> SearchAsync(
            string searchTerm,
            bool? isActive,
            string roleName,
            int pageNumber,
            int pageSize);

        #endregion

        #region Add/Update/Delete

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        /// <param name="user">کاربر جدید</param>
        /// <returns>کاربر اضافه شده</returns>
        Task<ApplicationUser> AddAsync(ApplicationUser user);

        /// <summary>
        /// به‌روزرسانی کاربر
        /// </summary>
        /// <param name="user">کاربر برای به‌روزرسانی</param>
        /// <returns>کاربر به‌روزرسانی شده</returns>
        Task UpdateAsync(ApplicationUser user);

        /// <summary>
        /// حذف نرم کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="deletedByUserId">شناسه کاربر حذف‌کننده</param>
        /// <returns>True اگر موفق باشد</returns>
        Task<bool> SoftDeleteAsync(string userId, string deletedByUserId);

        /// <summary>
        /// بازیابی کاربر حذف شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="restoredByUserId">شناسه کاربر بازیابی‌کننده</param>
        /// <returns>True اگر موفق باشد</returns>
        Task<bool> RestoreAsync(string userId, string restoredByUserId);

        #endregion

        #region Role Management

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست نام نقش‌ها</returns>
        Task<List<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// بررسی اینکه آیا کاربر در نقش خاصی است
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>True اگر کاربر در این نقش باشد</returns>
        Task<bool> IsInRoleAsync(string userId, string roleName);

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت تعداد کل کاربران
        /// </summary>
        /// <returns>تعداد کل کاربران</returns>
        Task<int> GetTotalUsersCountAsync();

        /// <summary>
        /// دریافت تعداد کاربران فعال
        /// </summary>
        /// <returns>تعداد کاربران فعال</returns>
        Task<int> GetActiveUsersCountAsync();

        /// <summary>
        /// دریافت تعداد کاربران بر اساس نقش
        /// </summary>
        /// <returns>Dictionary: نام نقش → تعداد کاربران</returns>
        Task<Dictionary<string, int>> GetUsersCountByRoleAsync();

        /// <summary>
        /// دریافت تعداد کاربران حذف شده
        /// </summary>
        /// <returns>تعداد کاربران حذف شده</returns>
        Task<int> GetDeletedUsersCountAsync();

        #endregion

        #region Validation

        /// <summary>
        /// بررسی وجود کاربر با کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <param name="excludeUserId">شناسه کاربری که باید از بررسی حذف شود (برای Edit)</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsByNationalCodeAsync(string nationalCode, string excludeUserId = null);

        /// <summary>
        /// بررسی وجود کاربر با ایمیل
        /// </summary>
        /// <param name="email">ایمیل</param>
        /// <param name="excludeUserId">شناسه کاربری که باید از بررسی حذف شود (برای Edit)</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsByEmailAsync(string email, string excludeUserId = null);

        #endregion
    }
}

