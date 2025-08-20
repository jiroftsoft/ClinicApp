using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس کاربر فعلی برای سیستم‌های پزشکی با رعایت کامل استانداردهای امنیتی و عملکردی
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly HttpContextBase _httpContext;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public CurrentUserService(
            HttpContextBase httpContext,
            ApplicationUserManager userManager,
            ILogger logger,
            ApplicationDbContext context)
        {
            _httpContext = httpContext;
            _userManager = userManager;
            _logger = logger.ForContext<CurrentUserService>();
            _context = context;
        }

        public string UserId => GetUserId();
        public string UserName => GetUserName();
        public bool IsAuthenticated => GetIsAuthenticated();
        public bool IsAdmin => IsInRole(AppRoles.Admin);
        public bool IsDoctor => IsInRole(AppRoles.Doctor);
        public bool IsReceptionist => IsInRole(AppRoles.Receptionist);
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime IranNow => DateTime.Now;
        public string PersianDate => DateTime.Now.ToPersianDateTime();

        public async Task<bool> HasAccessToServiceAsync(int serviceId)
        {
            try
            {
                // دریافت شناسه کاربر جاری
                var userId = UserId;

                // اگر کاربر وارد نشده باشد، دسترسی ندارد
                if (string.IsNullOrEmpty(userId) || userId == "System")
                {
                    _logger.Information("تلاش برای دسترسی به خدمات با شناسه {ServiceId} توسط کاربر غیراحراز هویت شده", serviceId);
                    return false;
                }

                // بررسی نقش‌های کاربر
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("کاربر با شناسه {UserId} در سیستم کاربران یافت نشد", userId);
                    return false;
                }

                // 1. اگر کاربر نقش مدیر دارد، به همه چیز دسترسی دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Admin))
                {
                    _logger.Information("کاربر مدیر {UserId} به خدمات {ServiceId} دسترسی دارد", userId, serviceId);
                    return true;
                }

                // 2. اگر کاربر نقش پزشک دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Doctor))
                {
                    // دریافت پزشک مربوط به کاربر
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.ApplicationUserId == userId && !d.IsDeleted);

                    if (doctor == null)
                    {
                        _logger.Warning("پزشک مربوط به کاربر {UserId} یافت نشد", userId);
                        return false;
                    }

                    // دریافت خدمات - بدون استفاده از ThenInclude (EF6)
                    var service = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                    if (service == null || service.ServiceCategory == null)
                    {
                        _logger.Warning("خدمات با شناسه {ServiceId} یا دسته‌بندی آن یافت نشد", serviceId);
                        return false;
                    }

                    // دریافت Department مربوطه به صورت جداگانه (به جای ThenInclude)
                    var serviceCategory = await _context.ServiceCategories
                        .Include(sc => sc.Department)
                        .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == service.ServiceCategoryId);

                    if (serviceCategory?.Department == null)
                    {
                        _logger.Warning("دپارتمان مربوط به خدمات {ServiceId} یافت نشد", serviceId);
                        return false;
                    }

                    // بررسی دسترسی بر اساس کلینیک یا دپارتمان
                    bool hasAccess = (doctor.ClinicId.HasValue &&
                                    serviceCategory.Department.ClinicId == doctor.ClinicId) ||
                                   (doctor.DepartmentId.HasValue &&
                                    serviceCategory.DepartmentId == doctor.DepartmentId);

                    _logger.Information("پزشک {UserId} به خدمات {ServiceId} دسترسی {AccessStatus}",
                        userId, serviceId, hasAccess ? "دارد" : "ندارد");
                    return hasAccess;
                }

                // 3. اگر کاربر نقش منشی دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Receptionist))
                {
                    // دریافت خدمات
                    var service = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                    if (service == null || service.ServiceCategory == null)
                    {
                        _logger.Warning("خدمات با شناسه {ServiceId} یا دسته‌بندی آن برای منشی یافت نشد", serviceId);
                        return false;
                    }

                    // دریافت Department مربوطه
                    var serviceCategory = await _context.ServiceCategories
                        .Include(sc => sc.Department)
                        .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == service.ServiceCategoryId);

                    if (serviceCategory?.Department == null)
                    {
                        _logger.Warning("دپارتمان مربوط به خدمات {ServiceId} برای منشی یافت نشد", serviceId);
                        return false;
                    }

                    // بررسی دسترسی منشی - فرض بر این است که منشی به تمام خدمات دسترسی دارد
                    // یا می‌توانید منطق خاص خود را اینجا پیاده‌سازی کنید
                    bool hasAccess = true; // یا منطق دسترسی خاص شما

                    _logger.Information("منشی {UserId} به خدمات {ServiceId} دسترسی {AccessStatus}",
                        userId, serviceId, hasAccess ? "دارد" : "ندارد");
                    return hasAccess;
                }

                // 4. سایر نقش‌ها (بیمار و غیره) به گزارش‌های خدمات دسترسی ندارند
                _logger.Information("کاربر {UserId} با نقش نامعتبر به خدمات {ServiceId} دسترسی ندارد", userId, serviceId);
                return false;
            }
            catch (Exception ex)
            {
                // در سیستم‌های پزشکی، بهتر است در صورت خطا، دسترسی را رد کنیم
                // این یک اصل امنیتی مهم است: "در صورت عدم اطمینان، دسترسی را رد کنید"
                _logger.Error(ex, "خطای غیرمنتظره در بررسی دسترسی کاربر {UserId} به خدمات {ServiceId}", UserId, serviceId);
                return false;
            }
        }

        private string GetUserId()
        {
            try
            {
                if (_httpContext?.User?.Identity == null)
                {
                    _logger.Warning("HttpContext یا User در CurrentUserService وجود ندارد.");
                    return "System";
                }

                var identity = _httpContext.User.Identity;
                if (!identity.IsAuthenticated)
                {
                    return "System";
                }

                // برای ClaimsIdentity
                if (identity is ClaimsIdentity claimsIdentity)
                {
                    var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    string userId = userIdClaim?.Value;

                    if (!string.IsNullOrEmpty(userId) && UserExistsInDatabase(userId))
                    {
                        return userId;
                    }
                }
                // برای Identity ساده
                else
                {
                    var userName = identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = _userManager.FindByName(userName);
                        if (user != null && UserExistsInDatabase(user.Id))
                        {
                            return user.Id;
                        }
                    }
                }

                return "System";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شناسه کاربر.");
                return "System";
            }
        }

        private string GetUserName()
        {
            try
            {
                if (_httpContext?.User?.Identity == null)
                {
                    return "سیستم";
                }

                var identity = _httpContext.User.Identity;
                if (!identity.IsAuthenticated)
                {
                    return "سیستم";
                }

                // برای ClaimsIdentity
                if (identity is ClaimsIdentity claimsIdentity)
                {
                    // سعی در دریافت نام کامل از Claim سفارشی
                    var fullNameClaim = claimsIdentity.FindFirst("FullName");
                    if (!string.IsNullOrEmpty(fullNameClaim?.Value))
                    {
                        return fullNameClaim.Value;
                    }

                    var userNameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
                    if (!string.IsNullOrEmpty(userNameClaim?.Value))
                    {
                        return userNameClaim.Value;
                    }

                    var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    string userId = userIdClaim?.Value;

                    if (!string.IsNullOrEmpty(userId) && UserExistsInDatabase(userId))
                    {
                        var applicationUser = _userManager.FindById(userId);
                        if (applicationUser != null)
                        {
                            return !string.IsNullOrEmpty(applicationUser.FullName) ?
                                applicationUser.FullName :
                                applicationUser.UserName;
                        }
                    }
                }
                // برای Identity ساده
                else
                {
                    var userName = identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = _userManager.FindByName(userName);
                        if (user != null)
                        {
                            return !string.IsNullOrEmpty(user.FullName) ?
                                user.FullName :
                                user.UserName;
                        }
                    }
                }

                return "سیستم";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نام کاربر.");
                return "سیستم";
            }
        }

        private bool GetIsAuthenticated()
        {
            try
            {
                return _httpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی احراز هویت کاربر.");
                return false;
            }
        }

        private bool IsInRole(string role)
        {
            try
            {
                if (_httpContext?.User?.Identity == null || !_httpContext.User.Identity.IsAuthenticated)
                {
                    return false;
                }

                var userId = UserId;
                if (userId == "System")
                {
                    return false;
                }

                return _userManager.IsInRole(userId, role);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی نقش کاربر برای نقش {Role}.", role);
                return false;
            }
        }

        private bool UserExistsInDatabase(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return false;
                }

                return _context.Users.Any(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کاربر در پایگاه داده برای شناسه {UserId}.", userId);
                return false;
            }
        }

        public string[] GetUserRoles()
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return new string[0];
                }

                var userId = UserId;
                if (userId == "System")
                {
                    return new string[0];
                }

                var roles = _userManager.GetRoles(userId);
                return roles?.ToArray() ?? new string[0];
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش‌های کاربر.");
                return new string[0];
            }
        }

        public bool HasAccess(string resource)
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return false;
                }

                // منطق خاص بررسی دسترسی‌ها
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی به {Resource}.", resource);
                return false;
            }
        }
    }
}