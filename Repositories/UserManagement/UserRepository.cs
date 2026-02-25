using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.UserManagement;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using EntityFramework.DynamicFilters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;

namespace ClinicApp.Repositories.UserManagement
{
    /// <summary>
    /// Repository پیاده‌سازی برای مدیریت کاربران سیستم
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations کامل
    /// 2. Search & Filter با Pagination
    /// 3. Role Management Integration
    /// 4. Soft Delete Support
    /// 5. Audit Trail Support
    /// 6. Performance Optimization (Include, AsNoTracking)
    /// </summary>
    public class UserRepository : IUserRepository
    {
        #region Fields and Constructor

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(
            ApplicationDbContext context,
            ILogger logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<UserRepository>() ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// دریافت کاربر با شناسه
        /// </summary>
        public async Task<ApplicationUser> GetByIdAsync(string id)
        {
            try
            {
                _logger.Debug("دریافت کاربر با شناسه: {UserId}", id);

                if (string.IsNullOrEmpty(id))
                {
                    _logger.Warning("شناسه کاربر خالی است");
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.CreatedByUser)
                    .Include(u => u.UpdatedByUser)
                    .Include(u => u.DeletedByUser)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    _logger.Warning("کاربر با شناسه {UserId} یافت نشد", id);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربر با شناسه {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربر با کد ملی
        /// </summary>
        public async Task<ApplicationUser> GetByNationalCodeAsync(string nationalCode)
        {
            try
            {
                _logger.Debug("دریافت کاربر با کد ملی: {NationalCode}", nationalCode);

                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    _logger.Warning("کد ملی خالی است");
                    return null;
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.NationalCode == nationalCode && !u.IsDeleted);

                if (user == null)
                {
                    _logger.Warning("کاربر با کد ملی {NationalCode} یافت نشد", nationalCode);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربر با کد ملی {NationalCode}", nationalCode);
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربر با ایمیل
        /// </summary>
        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            try
            {
                _logger.Debug("دریافت کاربر با ایمیل: {Email}", email);

                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.Warning("ایمیل خالی است");
                    return null;
                }

                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

                if (user == null)
                {
                    _logger.Warning("کاربر با ایمیل {Email} یافت نشد", email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربر با ایمیل {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام کاربران (فقط فعال - بدون حذف شده)
        /// </summary>
        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            try
            {
                _logger.Debug("دریافت تمام کاربران");

                return await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام کاربران");
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربران فعال
        /// </summary>
        public async Task<List<ApplicationUser>> GetActiveUsersAsync()
        {
            try
            {
                _logger.Debug("دریافت کاربران فعال");

                return await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted && u.IsActive)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربران فعال");
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربران حذف شده (Soft Delete)
        /// </summary>
        public async Task<List<ApplicationUser>> GetDeletedUsersAsync()
        {
            try
            {
                _logger.Debug("دریافت کاربران حذف شده");

                // ✅ غیرفعال کردن فیلتر سراسری IsDeletedFilter برای دریافت کاربران حذف شده
                _context.DisableFilter("IsDeletedFilter");
                
                try
                {
                    var deletedUsers = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.IsDeleted)
                        .OrderByDescending(u => u.DeletedAt)
                        .ToListAsync();
                    
                    return deletedUsers;
                }
                finally
                {
                    // ✅ فعال کردن مجدد فیلتر
                    _context.EnableFilter("IsDeletedFilter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کاربران حذف شده");
                throw;
            }
        }

        #endregion

        #region Search & Filter

        /// <summary>
        /// جستجو و فیلتر کاربران با Pagination
        /// </summary>
        public async Task<PagedResult<ApplicationUser>> SearchAsync(
            string searchTerm,
            bool? isActive,
            string roleName,
            int pageNumber,
            int pageSize)
        {
            try
            {
                _logger.Debug("جستجوی کاربران - SearchTerm: {SearchTerm}, IsActive: {IsActive}, Role: {Role}, Page: {Page}, Size: {Size}",
                    searchTerm, isActive, roleName, pageNumber, pageSize);

                // ✅ شروع Query
                var query = _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted);

                // ✅ فیلتر Search Term
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(u =>
                        u.FirstName.Contains(term) ||
                        u.LastName.Contains(term) ||
                        u.NationalCode.Contains(term) ||
                        u.Email.Contains(term) ||
                        u.PhoneNumber.Contains(term) ||
                        (u.FirstName + " " + u.LastName).Contains(term));
                }

                // ✅ فیلتر IsActive
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }

                // ✅ فیلتر Role (یک کوئری با JOIN به‌جای دو round-trip)
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role != null)
                        query = query.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
                }

                // ✅ شمارش کل
                var totalCount = await query.CountAsync();

                // ✅ Pagination
                var items = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Information("جستجوی کاربران - {Count} نتیجه از {Total} کل", items.Count, totalCount);

                return new PagedResult<ApplicationUser>(items, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی کاربران");
                throw;
            }
        }

        #endregion

        #region Add/Update/Delete

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        public async Task<ApplicationUser> AddAsync(ApplicationUser user)
        {
            try
            {
                _logger.Debug("افزودن کاربر جدید - NationalCode: {NationalCode}, Email: {Email}",
                    user?.NationalCode, user?.Email);

                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                // ✅ تنظیم Audit Trail
                if (user.CreatedAt == default(DateTime))
                {
                    user.CreatedAt = DateTime.Now;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.Information("کاربر جدید با موفقیت اضافه شد - UserId: {UserId}, NationalCode: {NationalCode}",
                    user.Id, user.NationalCode);

                return user;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن کاربر جدید");
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی کاربر
        /// </summary>
        public async Task UpdateAsync(ApplicationUser user)
        {
            try
            {
                _logger.Debug("به‌روزرسانی کاربر - UserId: {UserId}", user?.Id);

                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                // ✅ تنظیم Audit Trail
                user.UpdatedAt = DateTime.Now;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.Information("کاربر با موفقیت به‌روزرسانی شد - UserId: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی کاربر - UserId: {UserId}", user?.Id);
                throw;
            }
        }

        /// <summary>
        /// حذف نرم کاربر
        /// </summary>
        public async Task<bool> SoftDeleteAsync(string userId, string deletedByUserId)
        {
            try
            {
                _logger.Debug("حذف نرم کاربر - UserId: {UserId}, DeletedBy: {DeletedBy}", userId, deletedByUserId);

                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId));
                }

                var user = await GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("کاربر با شناسه {UserId} یافت نشد", userId);
                    return false;
                }

                // ✅ Soft Delete
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.DeletedByUserId = deletedByUserId;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUserId = deletedByUserId;

                _context.Entry(user).State = EntityState.Modified;
                var saveResult = await _context.SaveChangesAsync();

                // ✅ بررسی پس از ذخیره
                if (saveResult > 0)
                {
                    // ✅ Verify: بررسی مجدد از دیتابیس (با غیرفعال کردن فیلتر)
                    _context.DisableFilter("IsDeletedFilter");
                    try
                    {
                        var verifyUser = await _context.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == userId);
                        
                        if (verifyUser != null && verifyUser.IsDeleted)
                        {
                            _logger.Information("✅ کاربر با موفقیت حذف شد (Soft Delete) - UserId: {UserId}, IsDeleted: {IsDeleted}, DeletedAt: {DeletedAt}", 
                                userId, verifyUser.IsDeleted, verifyUser.DeletedAt);
                            return true;
                        }
                        else
                        {
                            _logger.Error("❌ کاربر حذف نشد! - UserId: {UserId}, IsDeleted: {IsDeleted}", 
                                userId, verifyUser?.IsDeleted ?? false);
                            return false;
                        }
                    }
                    finally
                    {
                        _context.EnableFilter("IsDeletedFilter");
                    }
                }
                else
                {
                    _logger.Warning("⚠️ هیچ تغییری ذخیره نشد - UserId: {UserId}", userId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم کاربر - UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// بازیابی کاربر حذف شده
        /// </summary>
        public async Task<bool> RestoreAsync(string userId, string restoredByUserId)
        {
            try
            {
                _logger.Debug("بازیابی کاربر - UserId: {UserId}, RestoredBy: {RestoredBy}", userId, restoredByUserId);

                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId));
                }

                // ✅ غیرفعال کردن فیلتر برای یافتن کاربر حذف شده
                _context.DisableFilter("IsDeletedFilter");
                try
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted);

                    if (user == null)
                    {
                        _logger.Warning("کاربر حذف شده با شناسه {UserId} یافت نشد", userId);
                        return false;
                    }

                    // ✅ Restore
                    user.IsDeleted = false;
                    user.DeletedAt = null;
                    user.DeletedByUserId = null;
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedByUserId = restoredByUserId;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                finally
                {
                    _context.EnableFilter("IsDeletedFilter");
                }

                _logger.Information("کاربر با موفقیت بازیابی شد - UserId: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی کاربر - UserId: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Role Management

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                _logger.Debug("دریافت نقش‌های کاربر - UserId: {UserId}", userId);

                if (string.IsNullOrEmpty(userId))
                {
                    return new List<string>();
                }

                var roles = await _userManager.GetRolesAsync(userId);
                return roles?.ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش‌های کاربر - UserId: {UserId}", userId);
                return new List<string>();
            }
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر در نقش خاصی است
        /// </summary>
        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            try
            {
                _logger.Debug("بررسی نقش کاربر - UserId: {UserId}, Role: {Role}", userId, roleName);

                if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(roleName))
                {
                    return false;
                }

                return await _userManager.IsInRoleAsync(userId, roleName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی نقش کاربر - UserId: {UserId}, Role: {Role}", userId, roleName);
                return false;
            }
        }

        /// <summary>
        /// دریافت نقش‌های چند کاربر در یک درخواست (رفع N+1)
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetRolesForUserIdsAsync(IEnumerable<string> userIds)
        {
            var userIdsList = userIds?.ToList() ?? new List<string>();
            var result = new Dictionary<string, List<string>>();
            if (userIdsList.Count == 0)
                return result;

            try
            {
                var pairs = await (from ur in _context.UserRoles
                                  where userIdsList.Contains(ur.UserId)
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  select new { ur.UserId, r.Name })
                             .ToListAsync();

                foreach (var g in pairs.GroupBy(x => x.UserId))
                    result[g.Key] = g.Select(x => x.Name).ToList();

                foreach (var id in userIdsList)
                {
                    if (!result.ContainsKey(id))
                        result[id] = new List<string>();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش‌های دسته‌ای کاربران");
                foreach (var id in userIdsList)
                    result[id] = new List<string>();
                return result;
            }
        }

        /// <summary>
        /// تعداد کاربران فعال (غیرحذف‌شده) در یک نقش
        /// </summary>
        public async Task<int> GetActiveUsersCountInRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return 0;
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return 0;
                return await _context.Users
                    .AsNoTracking()
                    .CountAsync(u => !u.IsDeleted && u.Roles.Any(r => r.RoleId == role.Id));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش کاربران نقش {RoleName}", roleName);
                return 0;
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// دریافت تعداد کل کاربران
        /// </summary>
        public async Task<int> GetTotalUsersCountAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کل کاربران");
                return 0;
            }
        }

        /// <summary>
        /// دریافت تعداد کاربران فعال
        /// </summary>
        public async Task<int> GetActiveUsersCountAsync()
        {
            try
            {
                return await _context.Users
                    .AsNoTracking()
                    .Where(u => !u.IsDeleted && u.IsActive)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کاربران فعال");
                return 0;
            }
        }

        /// <summary>
        /// دریافت تعداد کاربران بر اساس نقش
        /// </summary>
        public async Task<Dictionary<string, int>> GetUsersCountByRoleAsync()
        {
            try
            {
                _logger.Debug("دریافت تعداد کاربران بر اساس نقش");

                var result = new Dictionary<string, int>();

                // ✅ دریافت تمام نقش‌ها
                var roles = await _roleManager.Roles.ToListAsync();

                foreach (var role in roles)
                {
                    var count = await _userManager.Users
                        .CountAsync(u => u.Roles.Any(r => r.RoleId == role.Id) && !u.IsDeleted);

                    result[role.Name] = count;
                }

                _logger.Information("تعداد کاربران بر اساس نقش دریافت شد - {Count} نقش", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کاربران بر اساس نقش");
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// دریافت تعداد کاربران حذف شده
        /// </summary>
        public async Task<int> GetDeletedUsersCountAsync()
        {
            try
            {
                _logger.Debug("دریافت تعداد کاربران حذف شده");

                // ✅ غیرفعال کردن فیلتر سراسری IsDeletedFilter برای شمارش کاربران حذف شده
                _context.DisableFilter("IsDeletedFilter");
                
                try
                {
                    var count = await _context.Users
                        .AsNoTracking()
                        .CountAsync(u => u.IsDeleted);

                    _logger.Information("✅ تعداد کاربران حذف شده: {Count}", count);
                    return count;
                }
                finally
                {
                    // ✅ فعال کردن مجدد فیلتر
                    _context.EnableFilter("IsDeletedFilter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد کاربران حذف شده");
                return 0;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// بررسی وجود کاربر با کد ملی
        /// </summary>
        public async Task<bool> ExistsByNationalCodeAsync(string nationalCode, string excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return false;
                }

                var query = _context.Users
                    .AsNoTracking()
                    .Where(u => u.NationalCode == nationalCode && !u.IsDeleted);

                if (!string.IsNullOrEmpty(excludeUserId))
                {
                    query = query.Where(u => u.Id != excludeUserId);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کاربر با کد ملی {NationalCode}", nationalCode);
                return false;
            }
        }

        /// <summary>
        /// بررسی وجود کاربر با ایمیل
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email, string excludeUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return false;
                }

                var query = _context.Users
                    .AsNoTracking()
                    .Where(u => u.Email == email && !u.IsDeleted);

                if (!string.IsNullOrEmpty(excludeUserId))
                {
                    query = query.Where(u => u.Id != excludeUserId);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کاربر با ایمیل {Email}", email);
                return false;
            }
        }

        #endregion
    }
}

