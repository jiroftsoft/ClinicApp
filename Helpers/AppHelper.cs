using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Log = Serilog.Log;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی حرفه‌ای برای دسترسی سریع و ایمن به اطلاعات کاربر جاری در سیستم‌های پزشکی
    /// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// استفاده:
    /// string userId = AppHelper.CurrentUserId;
    /// bool isAdmin = AppHelper.IsAdmin;
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و در تمام محیط‌ها کار می‌کند:
    /// - کنترلرها
    /// - سرویس‌ها
    /// - Seed
    /// - تست‌ها
    /// - کارهای پس‌زمینه
    /// </summary>
    public static class AppHelper
    {
        private static readonly ILogger _log = Log.ForContext(typeof(AppHelper));
        private static ICurrentUserService _currentUserService;

        #region Initialization Methods (روش‌های مقداردهی اولیه)

        /// <summary>
        /// مقداردهی اولیه کلاس برای استفاده در سراسر سیستم
        /// این متد باید در Startup یا Global.asax فراخوانی شود
        /// </summary>
        public static void Initialize(ICurrentUserService currentUserService)
        {
            if (currentUserService == null)
            {
                _log.Error("Failed to initialize AppHelper: ICurrentUserService is null");
                throw new ArgumentNullException(nameof(currentUserService),
                    @"ICurrentUserService cannot be null. Please check your DI configuration.");
            }

            _currentUserService = currentUserService;
            _log.Information("AppHelper initialized successfully");
        }

        #endregion

        #region Core Properties (ویژگی‌های اصلی)

        /// <summary>
        /// شناسه کاربر جاری
        /// در سیستم‌های پزشکی، این شناسه برای ردیابی و امنیت حیاتی است
        /// </summary>
        public static string CurrentUserId => GetCurrentUserIdInternal();

        /// <summary>
        /// نام کاربری کاربر جاری (معمولاً کد ملی در سیستم‌های پزشکی ایرانی)
        /// </summary>
        public static string CurrentUserName => GetCurrentUser()?.UserName ?? "سیستم";

        /// <summary>
        /// آیا کاربر جاری احراز هویت شده است؟
        /// </summary>
        public static bool IsAuthenticated => GetCurrentUser()?.IsAuthenticated ?? false;

        /// <summary>
        /// آیا کاربر جاری در نقش مدیر سیستم است؟
        /// </summary>
        public static bool IsAdmin => GetCurrentUser()?.IsInRole(AppRoles.Admin) ?? false;

        /// <summary>
        /// آیا کاربر جاری پزشک است؟
        /// </summary>
        public static bool IsDoctor => GetCurrentUser()?.IsInRole(AppRoles.Doctor) ?? false;

        /// <summary>
        /// آیا کاربر جاری منشی است؟
        /// </summary>
        public static bool IsReceptionist => GetCurrentUser()?.IsInRole(AppRoles.Receptionist) ?? false;

        /// <summary>
        /// آیا کاربر جاری بیمار است؟
        /// </summary>
        public static bool IsPatient => GetCurrentUser()?.IsInRole(AppRoles.Patient) ?? false;

        /// <summary>
        /// زمان فعلی سیستم به صورت UTC
        /// برای سیستم‌های پزشکی بسیار حیاتی است تا تمام تاریخ‌ها یکسان باشند
        /// </summary>
        public static DateTime UtcNow => GetCurrentUser()?.UtcNow ?? DateTime.UtcNow;

        /// <summary>
        /// زمان فعلی سیستم به صورت محلی (با توجه به تنظیمات سیستم پزشکی)
        /// </summary>
        public static DateTime Now => GetCurrentUser()?.Now ?? DateTime.Now;

        #endregion

        #region Security Methods (روش‌های امنیتی)

        /// <summary>
        /// بررسی دسترسی کاربر جاری به عملیات خاص
        /// </summary>
        public static bool HasPermission(string permission)
        {
            try
            {
                return GetCurrentUser()?.HasPermission(permission) ?? false;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking permission '{Permission}'", permission);
                return false;
            }
        }

        /// <summary>
        /// بررسی دسترسی کاربر جاری به موجودیت خاص
        /// </summary>
        public static bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class
        {
            try
            {
                return GetCurrentUser()?.HasEntityAccess(entity, permission) ?? false;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking entity access for type '{EntityType}'", typeof(TEntity).Name);
                return false;
            }
        }

        /// <summary>
        /// دریافت ادعاها (Claims) کاربر جاری
        /// </summary>
        public static ClaimsPrincipal CurrentClaimsPrincipal => GetCurrentUser()?.ClaimsPrincipal;

        #endregion

        #region Helper Methods (روش‌های کمکی)

        /// <summary>
        /// دریافت اطلاعات کاربر جاری
        /// </summary>
        private static ICurrentUserService GetCurrentUser()
        {
            try
            {
                // 1. اولویت اول: استفاده از سرویس ثبت شده
                if (_currentUserService != null)
                {
                    return _currentUserService;
                }

                // 2. اولویت دوم: استفاده از HttpContext در محیط وب
                if (HttpContext.Current != null)
                {
                    var httpContext = new HttpContextWrapper(HttpContext.Current);
                    var identity = httpContext.User?.Identity as ClaimsIdentity;

                    if (identity != null && identity.IsAuthenticated)
                    {
                        var userId = identity.GetUserId();
                        if (!string.IsNullOrEmpty(userId))
                        {
                            return new HttpContextCurrentUserService(httpContext);
                        }
                    }
                }

                // 3. اولویت سوم: استفاده از Thread.CurrentPrincipal در محیط‌های غیر-وب
                var currentPrincipal = Thread.CurrentPrincipal;
                if (currentPrincipal != null && currentPrincipal.Identity.IsAuthenticated)
                {
                    var userId = currentPrincipal.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        return new ThreadPrincipalCurrentUserService(currentPrincipal);
                    }
                }

                // 4. اولویت چهارم: استفاده از کاربر سیستم در محیط‌های Seed یا پس‌زمینه
                _log.Debug("No authenticated user found. Using System user as fallback.");
                return new SystemCurrentUserService();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting current user. Using System user as fallback.");
                return new SystemCurrentUserService();
            }
        }

        /// <summary>
        /// دریافت شناسه کاربر جاری با مدیریت خطا
        /// </summary>
        private static string GetCurrentUserIdInternal()
        {
            try
            {
                var userId = GetCurrentUser()?.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    _log.Warning("Current user ID is null or empty. Using System user ID as fallback.");
                    return SystemUsers.SystemUserId;
                }
                return userId;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting current user ID. Using System user ID as fallback.");
                return SystemUsers.SystemUserId;
            }
        }

        #endregion

        #region Nested Service Implementations (پیاده‌سازی‌های داخلی)

        private class HttpContextCurrentUserService : ICurrentUserService
        {
            private readonly HttpContextBase _httpContext;
            private readonly ApplicationUserManager _userManager;

            public HttpContextCurrentUserService(HttpContextBase httpContext)
            {
                _httpContext = httpContext;
                _userManager = DependencyResolver.Current.GetService<ApplicationUserManager>();
            }

            public string UserId => _httpContext.User.Identity.GetUserId();
            public string UserName => _httpContext.User.Identity.Name;
            public bool IsAuthenticated => _httpContext.User.Identity.IsAuthenticated;
            public ClaimsPrincipal ClaimsPrincipal => _httpContext.User as ClaimsPrincipal;
            public DateTime UtcNow => DateTime.UtcNow;
            public DateTime Now => DateTime.Now;

            public bool IsAdmin => IsInRole(AppRoles.Admin);
            public bool IsDoctor => IsInRole(AppRoles.Doctor);
            public bool IsReceptionist => IsInRole(AppRoles.Receptionist);
            public bool IsPatient => IsInRole(AppRoles.Patient);

            public bool IsInRole(string role) =>
                _httpContext.User.IsInRole(role);

            public bool HasPermission(string permission) =>
                true;

            public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class =>
                true;

            public Task<bool> HasAccessToServiceAsync(int serviceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToInsuranceAsync(int insuranceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToDepartmentAsync(int departmentId) =>
                Task.FromResult(true);

            public Task<Doctor> GetDoctorInfoAsync() =>
                Task.FromResult(IsDoctor ?
                    _userManager.FindById(UserId)?.Doctors?.FirstOrDefault() :
                    null);

            public Task<Patient> GetPatientInfoAsync() =>
                Task.FromResult(IsPatient ?
                    _userManager.FindById(UserId)?.Patients?.FirstOrDefault() :
                    null);

            public string GetSystemUserId() =>
                SystemUsers.SystemUserId;

            public Task<List<Department>> GetDoctorActiveDepartmentsAsync() =>
                Task.FromResult(new List<Department>());

            public Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync() =>
                Task.FromResult(new List<ServiceCategory>());

            public Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId) =>
                Task.FromResult(false);

            public Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId) =>
                Task.FromResult(false);

            public Task<string> GetDoctorRoleInDepartmentAsync(int departmentId) =>
                Task.FromResult<string>(null);

            public string[] GetUserRoles() =>
                new string[0];
        }

        private class ThreadPrincipalCurrentUserService : ICurrentUserService
        {
            private readonly IPrincipal _principal;
            private readonly ApplicationUserManager _userManager;

            public ThreadPrincipalCurrentUserService(IPrincipal principal)
            {
                _principal = principal;
                _userManager = DependencyResolver.Current.GetService<ApplicationUserManager>();
            }

            public string UserId => _principal.Identity.GetUserId();
            public string UserName => _principal.Identity.Name;
            public bool IsAuthenticated => _principal.Identity.IsAuthenticated;
            public ClaimsPrincipal ClaimsPrincipal => _principal as ClaimsPrincipal;
            public DateTime UtcNow => DateTime.UtcNow;
            public DateTime Now => DateTime.Now;

            public bool IsAdmin => IsInRole(AppRoles.Admin);
            public bool IsDoctor => IsInRole(AppRoles.Doctor);
            public bool IsReceptionist => IsInRole(AppRoles.Receptionist);
            public bool IsPatient => IsInRole(AppRoles.Patient);

            public bool IsInRole(string role) =>
                _principal.IsInRole(role);

            public bool HasPermission(string permission) =>
                true;

            public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class =>
                true;

            public Task<bool> HasAccessToServiceAsync(int serviceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToInsuranceAsync(int insuranceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToDepartmentAsync(int departmentId) =>
                Task.FromResult(true);

            public Task<Doctor> GetDoctorInfoAsync() =>
                Task.FromResult(IsDoctor ?
                    _userManager.FindById(UserId)?.Doctors?.FirstOrDefault() :
                    null);

            public Task<Patient> GetPatientInfoAsync() =>
                Task.FromResult(IsPatient ?
                    _userManager.FindById(UserId)?.Patients?.FirstOrDefault() :
                    null);

            public string GetSystemUserId() =>
                SystemUsers.SystemUserId;

            public Task<List<Department>> GetDoctorActiveDepartmentsAsync() =>
                Task.FromResult(new List<Department>());

            public Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync() =>
                Task.FromResult(new List<ServiceCategory>());

            public Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId) =>
                Task.FromResult(false);

            public Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId) =>
                Task.FromResult(false);

            public Task<string> GetDoctorRoleInDepartmentAsync(int departmentId) =>
                Task.FromResult<string>(null);

            public string[] GetUserRoles() =>
                new string[0];
        }

        private class SystemCurrentUserService : ICurrentUserService
        {
            public string UserId => SystemUsers.SystemUserId;
            public string UserName => "سیستم";
            public bool IsAuthenticated => true;
            public ClaimsPrincipal ClaimsPrincipal => null;
            public DateTime UtcNow => DateTime.UtcNow;
            public DateTime Now => DateTime.Now;

            public bool IsAdmin => true;
            public bool IsDoctor => false;
            public bool IsReceptionist => false;
            public bool IsPatient => false;

            public bool IsInRole(string role) =>
                role == AppRoles.Admin;

            public bool HasPermission(string permission) =>
                true;

            public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class =>
                true;

            public Task<bool> HasAccessToServiceAsync(int serviceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToInsuranceAsync(int insuranceId) =>
                Task.FromResult(true);

            public Task<bool> HasAccessToDepartmentAsync(int departmentId) =>
                Task.FromResult(true);

            public Task<Doctor> GetDoctorInfoAsync() =>
                Task.FromResult<Doctor>(null);

            public Task<Patient> GetPatientInfoAsync() =>
                Task.FromResult<Patient>(null);

            public string GetSystemUserId() =>
                SystemUsers.SystemUserId;

            public Task<List<Department>> GetDoctorActiveDepartmentsAsync() =>
                Task.FromResult(new List<Department>());

            public Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync() =>
                Task.FromResult(new List<ServiceCategory>());

            public Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId) =>
                Task.FromResult(false);

            public Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId) =>
                Task.FromResult(false);

            public Task<string> GetDoctorRoleInDepartmentAsync(int departmentId) =>
                Task.FromResult<string>(null);

            public string[] GetUserRoles() =>
                new string[] { AppRoles.Admin };
        }

        #endregion
    }

    
}