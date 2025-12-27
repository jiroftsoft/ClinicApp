using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.UserManagement;

namespace ClinicApp.Interfaces.UserManagement
{
    /// <summary>
    /// Service Interface برای مدیریت کاربران سیستم
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations کامل
    /// 2. Role Management
    /// 3. Activation/Deactivation
    /// 4. Validation
    /// 5. Statistics
    /// 6. Audit Trail Support
    /// </summary>
    public interface IUserManagementService
    {
        #region CRUD Operations

        /// <summary>
        /// دریافت لیست کاربران با فیلتر و Pagination
        /// </summary>
        Task<ServiceResult<UserIndexViewModel>> GetUsersAsync(
            UserSearchFilter filter,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// دریافت جزئیات کاربر
        /// </summary>
        Task<ServiceResult<UserDetailsViewModel>> GetUserDetailsAsync(string userId);

        /// <summary>
        /// دریافت اطلاعات کاربر برای ویرایش
        /// </summary>
        Task<ServiceResult<UserCreateEditViewModel>> GetUserForEditAsync(string userId);

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        Task<ServiceResult<ApplicationUser>> CreateUserAsync(UserCreateEditViewModel model);

        /// <summary>
        /// به‌روزرسانی کاربر
        /// </summary>
        Task<ServiceResult<ApplicationUser>> UpdateUserAsync(UserCreateEditViewModel model);

        /// <summary>
        /// حذف نرم کاربر
        /// </summary>
        Task<ServiceResult<bool>> DeleteUserAsync(string userId);

        /// <summary>
        /// دریافت لیست کاربران حذف شده با فیلتر و Pagination
        /// </summary>
        Task<ServiceResult<UserIndexViewModel>> GetDeletedUsersAsync(
            UserSearchFilter filter,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// بازگردانی کاربر حذف شده
        /// </summary>
        Task<ServiceResult<bool>> RestoreUserAsync(string userId);

        #endregion

        #region Role Management

        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        Task<ServiceResult<bool>> AssignRoleAsync(string userId, string roleName);

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        Task<ServiceResult<bool>> RemoveRoleAsync(string userId, string roleName);

        /// <summary>
        /// دریافت لیست نقش‌های موجود
        /// </summary>
        Task<ServiceResult<List<RoleViewModel>>> GetAvailableRolesAsync();

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        Task<ServiceResult<List<RoleViewModel>>> GetUserRolesAsync(string userId);

        #endregion

        #region Activation/Deactivation

        /// <summary>
        /// فعال‌سازی کاربر
        /// </summary>
        Task<ServiceResult<bool>> ActivateUserAsync(string userId);

        /// <summary>
        /// غیرفعال‌سازی کاربر
        /// </summary>
        Task<ServiceResult<bool>> DeactivateUserAsync(string userId);

        #endregion

        #region Validation

        /// <summary>
        /// بررسی معتبر بودن کد ملی
        /// </summary>
        Task<ServiceResult<bool>> ValidateNationalCodeAsync(string nationalCode, string excludeUserId = null);

        /// <summary>
        /// بررسی معتبر بودن ایمیل
        /// </summary>
        Task<ServiceResult<bool>> ValidateEmailAsync(string email, string excludeUserId = null);

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت آمار کاربران
        /// </summary>
        Task<ServiceResult<UserStatisticsViewModel>> GetStatisticsAsync();

        #endregion
    }
}

