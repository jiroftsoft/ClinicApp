using System;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;

namespace ClinicApp.DataSeeding
{
    /// <summary>
    /// سرویس Seeding کاربران سیستمی (Admin و System)
    /// </summary>
    public class UserSeedService : BaseSeedService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
        }

        /// <summary>
        /// ایجاد کاربران پیش‌فرض (Admin و System)
        /// </summary>
        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("کاربران سیستمی");

                // ایجاد کاربر Admin
                await CreateUserIfNotExistsAsync(
                    SeedConstants.AdminUserName,
                    SeedConstants.AdminEmail,
                    SeedConstants.AdminPhoneNumber,
                    "Admin",
                    "System",
                    "Admin"
                );

                // ایجاد کاربر System
                await CreateUserIfNotExistsAsync(
                    SeedConstants.SystemUserName,
                    SeedConstants.SystemEmail,
                    SeedConstants.SystemPhoneNumber,
                    "System",
                    "Shefa Clinic",
                    "Admin"
                );

                LogSeedSuccess("کاربران سیستمی", 2);
            }
            catch (Exception ex)
            {
                LogSeedError("کاربران سیستمی", ex);
                throw;
            }
        }

        /// <summary>
        /// ایجاد کاربر در صورت عدم وجود
        /// </summary>
        private async Task CreateUserIfNotExistsAsync(
            string userName,
            string email,
            string phoneNumber,
            string firstName,
            string lastName,
            string roleName)
        {
            try
            {
                // بررسی وجود کاربر
                var existingUser = await _userManager.FindByNameAsync(userName);
                if (existingUser != null)
                {
                    _logger.Debug($"کاربر '{userName}' قبلاً وجود دارد");
                    return;
                }

                _logger.Information($"ایجاد کاربر '{userName}'...");

                // ایجاد کاربر جدید
                var user = new ApplicationUser
                {
                    UserName = userName,
                    NationalCode = userName, // در سیستم پسورد‌لس، UserName = NationalCode
                    Email = email,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // ایجاد کاربر (بدون پسورد)
                var result = _userManager.Create(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    _logger.Error($"خطا در ایجاد کاربر '{userName}': {errors}");
                    throw new InvalidOperationException($"خطا در ایجاد کاربر '{userName}': {errors}");
                }

                _logger.Information($"✅ کاربر '{userName}' با موفقیت ایجاد شد");

                // دریافت کاربر ایجاد شده
                var createdUser = await _userManager.FindByNameAsync(userName);
                if (createdUser == null)
                {
                    throw new InvalidOperationException($"کاربر '{userName}' بعد از ایجاد یافت نشد");
                }

                // تنظیم CreatedByUserId به خود کاربر
                createdUser.CreatedByUserId = createdUser.Id;

                // اضافه کردن به نقش
                if (!string.IsNullOrEmpty(roleName))
                {
                    var roleResult = await _userManager.AddToRoleAsync(createdUser.Id, roleName);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors);
                        _logger.Warning($"خطا در اضافه کردن کاربر '{userName}' به نقش '{roleName}': {errors}");
                    }
                    else
                    {
                        _logger.Debug($"کاربر '{userName}' به نقش '{roleName}' اضافه شد");
                    }
                }

                // ذخیره تغییرات (فقط یک بار)
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"خطای غیرمنتظره در ایجاد کاربر '{userName}'");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی: بررسی وجود کاربران سیستمی
        /// </summary>
        public override async Task<bool> ValidateAsync()
        {
            try
            {
                var adminExists = await _userManager.FindByNameAsync(SeedConstants.AdminUserName) != null;
                var systemExists = await _userManager.FindByNameAsync(SeedConstants.SystemUserName) != null;

                if (!adminExists)
                {
                    _logger.Warning("کاربر Admin یافت نشد");
                    return false;
                }

                if (!systemExists)
                {
                    _logger.Warning("کاربر System یافت نشد");
                    return false;
                }

                _logger.Debug("✅ اعتبارسنجی کاربران سیستمی موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی کاربران سیستمی");
                return false;
            }
        }
    }
}

