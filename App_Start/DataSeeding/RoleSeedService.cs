using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;

namespace ClinicApp.DataSeeding
{
    /// <summary>
    /// سرویس Seeding نقش‌های سیستم
    /// </summary>
    public class RoleSeedService : BaseSeedService
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        }

        /// <summary>
        /// ایجاد نقش‌های پیش‌فرض سیستم
        /// </summary>
        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("نقش‌های سیستم");

                var roles = new List<string>
                {
                    AppRoles.Admin,
                    AppRoles.Doctor,
                    AppRoles.Receptionist,
                    AppRoles.Patient
                };

                int createdCount = 0;

                foreach (var roleName in roles)
                {
                    if (await CreateRoleIfNotExistsAsync(roleName))
                    {
                        createdCount++;
                    }
                }

                LogSeedSuccess("نقش‌های سیستم", createdCount);
            }
            catch (Exception ex)
            {
                LogSeedError("نقش‌های سیستم", ex);
                throw;
            }
        }

        /// <summary>
        /// ایجاد نقش در صورت عدم وجود
        /// </summary>
        /// <param name="roleName">نام نقش</param>
        /// <returns>true اگر نقش جدید ایجاد شد، false اگر قبلاً وجود داشت</returns>
        private async Task<bool> CreateRoleIfNotExistsAsync(string roleName)
        {
            try
            {
                // بررسی وجود نقش
                if (_roleManager.RoleExists(roleName))
                {
                    _logger.Debug($"نقش '{roleName}' قبلاً وجود دارد");
                    return false;
                }

                _logger.Information($"ایجاد نقش '{roleName}'...");

                // ایجاد نقش جدید
                var role = new IdentityRole(roleName);
                var result = _roleManager.Create(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.Error($"خطا در ایجاد نقش '{roleName}': {errors}");
                    throw new InvalidOperationException($"خطا در ایجاد نقش '{roleName}': {errors}");
                }

                _logger.Information($"✅ نقش '{roleName}' با موفقیت ایجاد شد");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطای غیرمنتظره در ایجاد نقش '{roleName}'");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی: بررسی وجود تمام نقش‌های مورد نیاز
        /// </summary>
        public override async Task<bool> ValidateAsync()
        {
            try
            {
                var requiredRoles = new List<string>
                {
                    AppRoles.Admin,
                    AppRoles.Doctor,
                    AppRoles.Receptionist,
                    AppRoles.Patient
                };

                var allExist = requiredRoles.All(role => _roleManager.RoleExists(role));

                if (!allExist)
                {
                    var missingRoles = requiredRoles
                        .Where(role => !_roleManager.RoleExists(role))
                        .ToList();

                    _logger.Warning($"نقش‌های زیر یافت نشدند: {string.Join(", ", missingRoles)}");
                    return false;
                }

                _logger.Debug("✅ اعتبارسنجی نقش‌های سیستم موفق");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی نقش‌های سیستم");
                return false;
            }
        }
    }
}

