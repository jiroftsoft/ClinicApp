using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using ClinicApp.Extensions;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس کاربر فعلی برای محیط‌های غیر-وب در سیستم‌های پزشکی
    /// این کلاس برای کارهای پس‌زمینه، Jobهای زمان‌بندی شده و سرویس‌های ویندوز طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی کامل از سیستم‌های پزشکی
    /// 2. رعایت استانداردهای امنیتی سیستم‌های پزشکی
    /// 3. مدیریت صحیح کاربر سیستم
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// </summary>
    public class BackgroundCurrentUserService : ICurrentUserService
    {
        private static readonly ILogger _log = Log.ForContext<BackgroundCurrentUserService>();
        private readonly string _systemUserId;
        private readonly bool _isSystemAdmin;
        private readonly ApplicationUserManager _userManager;

        /// <summary>
        /// سازنده کلاس برای محیط‌های غیر-وب
        /// </summary>
        /// <param name="systemUserId">شناسه کاربر سیستم</param>
        /// <param name="isSystemAdmin">آیا کاربر سیستم مدیر است؟</param>
        /// <param name="userManager">مدیریت کاربران</param>
        public BackgroundCurrentUserService(
            string systemUserId,
            bool isSystemAdmin,
            ApplicationUserManager userManager)
        {
            _systemUserId = systemUserId;
            _isSystemAdmin = isSystemAdmin;
            _userManager = userManager;

            _log.Information("BackgroundCurrentUserService با شناسه کاربر سیستم {SystemUserId} راه‌اندازی شد", systemUserId);
        }

        #region Core Properties (ویژگی‌های اصلی)

        public string UserId => _systemUserId;
        public string UserName => "سیستم";
        public string UserFullName => "کاربر سیستم";
        public bool IsAuthenticated => true;
        public bool IsAdmin => _isSystemAdmin;
        public bool IsDoctor => false;
        public bool IsReceptionist => false;
        public bool IsPatient => false;
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        public string PersianDate => DateTimeExtensions.ToPersianDateTime(DateTime.Now);
        public ClaimsPrincipal ClaimsPrincipal => CreateClaimsPrincipal();

        #endregion

        #region Security Methods (روش‌های امنیتی)

        public bool IsInRole(string role)
        {
            try
            {
                if (string.IsNullOrEmpty(role))
                    return false;

                // برای کاربر سیستم، فقط نقش‌های خاص مدیریتی را بررسی می‌کنیم
                if (_isSystemAdmin)
                {
                    return role == AppRoles.Admin;

                }

                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی نقش کاربر در BackgroundCurrentUserService برای نقش {Role}.", role);
                return false;
            }
        }

        public bool HasPermission(string permission)
        {
            try
            {
                // کاربر سیستم به تمام دسترسی‌ها دسترسی دارد
                return _isSystemAdmin;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی به {Permission}.", permission);
                return false;
            }
        }

        public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class
        {
            try
            {
                if (entity == null)
                    return false;

                // کاربر سیستم به تمام موجودیت‌ها دسترسی دارد
                return _isSystemAdmin;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی به موجودیت {EntityType} با دسترسی {Permission}.",
                    typeof(TEntity).Name, permission);
                return false;
            }
        }

        public Task<bool> HasAccessToServiceAsync(int serviceId)
        {
            try
            {
                // کاربر سیستم به تمام خدمات دسترسی دارد
                return Task.FromResult(_isSystemAdmin);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در بررسی دسترسی کاربر سیستم به خدمات {ServiceId}", serviceId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> HasAccessToInsuranceAsync(int insuranceId)
        {
            try
            {
                // کاربر سیستم به تمام بیمه‌ها دسترسی دارد
                return Task.FromResult(_isSystemAdmin);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی به بیمه {InsuranceId}.", insuranceId);
                return Task.FromResult(false);
            }
        }

        public Task<bool> HasAccessToDepartmentAsync(int departmentId)
        {
            try
            {
                // کاربر سیستم به تمام دپارتمان‌ها دسترسی دارد
                return Task.FromResult(_isSystemAdmin);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی به دپارتمان {DepartmentId}.", departmentId);
                return Task.FromResult(false);
            }
        }

        #endregion

        #region Helper Methods (روش‌های کمکی)

        public Task<Doctor> GetDoctorInfoAsync()
        {
            _log.Warning("تلاش برای دریافت اطلاعات پزشک توسط BackgroundCurrentUserService");
            return Task.FromResult<Doctor>(null);
        }

        public Task<Patient> GetPatientInfoAsync()
        {
            _log.Warning("تلاش برای دریافت اطلاعات بیمار توسط BackgroundCurrentUserService");
            return Task.FromResult<Patient>(null);
        }

        public string GetSystemUserId()
        {
            return _systemUserId;
        }

        public Task<List<Department>> GetDoctorActiveDepartmentsAsync()
        {
            _log.Warning("تلاش برای دریافت دپارتمان‌های فعال پزشک توسط BackgroundCurrentUserService");
            return Task.FromResult(new List<Department>());
        }

        public Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync()
        {
            _log.Warning("تلاش برای دریافت دسته‌بندی خدمات مجاز پزشک توسط BackgroundCurrentUserService");
            return Task.FromResult(new List<ServiceCategory>());
        }

        public Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId)
        {
            _log.Warning("تلاش برای بررسی فعالیت پزشک در دپارتمان {DepartmentId} توسط BackgroundCurrentUserService", departmentId);
            return Task.FromResult(false);
        }

        public Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId)
        {
            _log.Warning("تلاش برای بررسی مجوز پزشک برای دسته‌بندی خدمات {ServiceCategoryId} توسط BackgroundCurrentUserService", serviceCategoryId);
            return Task.FromResult(false);
        }

        public Task<string> GetDoctorRoleInDepartmentAsync(int departmentId)
        {
            _log.Warning("تلاش برای دریافت نقش پزشک در دپارتمان {DepartmentId} توسط BackgroundCurrentUserService", departmentId);
            return Task.FromResult<string>(null);
        }

        public string[] GetUserRoles()
        {
            try
            {
                return _isSystemAdmin ? new string[] { AppRoles.Admin } : new string[0];
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت نقش‌های کاربر سیستم");
                return new string[0];
            }
        }

        /// <summary>
        /// دریافت شناسه کاربر جاری
        /// </summary>
        public string GetCurrentUserId()
        {
            return _systemUserId;
        }

        /// <summary>
        /// دریافت نام کاربر جاری
        /// </summary>
        public string GetCurrentUserName()
        {
            return "سیستم";
        }

        public bool IsDevelopmentEnvironment()
        {
            // BackgroundCurrentUserService معمولاً در محیط‌های غیر-وب استفاده می‌شود
            // که می‌تواند محیط توسعه باشد
            _log.Debug("BackgroundCurrentUserService - تشخیص محیط توسعه");
            return true;
        }

        #endregion

        #region Private Helper Methods (روش‌های کمکی خصوصی)

        private ClaimsPrincipal CreateClaimsPrincipal()
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, _systemUserId),
                    new Claim(ClaimTypes.Name, "سیستم"),
                    new Claim("FullName", "کاربر سیستم"),
                    new Claim("SystemUser", "true")
                };

                if (_isSystemAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, AppRoles.Admin));
                    
                }

                var identity = new ClaimsIdentity(claims, "SystemAuth");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ایجاد ClaimsPrincipal برای BackgroundCurrentUserService");
                return null;
            }
        }

        #endregion
    }
}