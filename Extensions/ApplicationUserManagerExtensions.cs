using System;
using System.Data.Entity;
using System.Threading.Tasks;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities;

namespace ClinicApp.Extensions;

/// <summary>
/// این کلاس شامل متدهای گسترشی برای ApplicationUserManager است
/// این متدها برای پشتیبانی از کد ملی در سیستم پزشکی "شفا" طراحی شده‌اند
/// </summary>
public static class ApplicationUserManagerExtensions
{
    /// <summary>
    /// یافتن کاربر بر اساس کد ملی
    /// </summary>
    /// <param name="manager">مدیر کاربران</param>
    /// <param name="nationalCode">کد ملی کاربر</param>
    /// <returns>کاربر مطابق با کد ملی یا null در صورت عدم وجود</returns>
    public static async Task<ApplicationUser> FindByNationalCodeAsync(this ApplicationUserManager manager, string nationalCode)
    {
        try
        {
            // نرمال‌سازی کد ملی
            nationalCode = nationalCode?.Trim();

            // اگر کد ملی خالی باشد، null برمی‌گردانیم
            if (string.IsNullOrWhiteSpace(nationalCode))
            {
                return null;
            }

            // یافتن کاربر بر اساس کد ملی
            var user = await manager.Users
                .FirstOrDefaultAsync(u => u.NationalCode == nationalCode);

            return user;
        }
        catch (Exception ex)
        {
            // در محیط‌های پزشکی، نمی‌خواهیم خطا را به کاربر نشان دهیم
            // فقط در صورت نیاز می‌توانیم آن را لاگ کنیم
            return null;
        }
    }

    /// <summary>
    /// بررسی وجود کاربر با کد ملی مشخص
    /// </summary>
    /// <param name="manager">مدیر کاربران</param>
    /// <param name="nationalCode">کد ملی کاربر</param>
    /// <returns>درست در صورت وجود کاربر</returns>
    public static async Task<bool> ExistsByNationalCodeAsync(this ApplicationUserManager manager, string nationalCode)
    {
        // نرمال‌سازی کد ملی
        nationalCode = nationalCode?.Trim();

        // اگر کد ملی خالی باشد، false برمی‌گردانیم
        if (string.IsNullOrWhiteSpace(nationalCode))
        {
            return false;
        }

        // بررسی وجود کاربر
        var exists = await manager.Users
            .AnyAsync(u => u.NationalCode == nationalCode);

        return exists;
    }
}