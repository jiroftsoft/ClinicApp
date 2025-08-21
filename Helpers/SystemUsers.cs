using System.Data.Entity;
using System.Linq;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Helpers;

/// <summary>
/// این کلاس شناسه‌ی کاربران کلیدی سیستم را پس از شروع برنامه کش می‌کند.
/// این کار از کوئری‌های تکراری به دیتابیس در متدهایی مانند GetCurrentUserId جلوگیری می‌کند.
/// </summary>
public static class SystemUsers
{
    public static string SystemUserId { get; private set; }
    public static string AdminUserId { get; private set; }

    public static void Initialize(ApplicationDbContext context)
    {
        Log.Information("در حال بارگذاری و کش کردن شناسه‌ی کاربران سیستمی...");

        // یوزرنیم‌ها باید با مقادیر استفاده شده در IdentitySeed.cs یکسان باشند
        var systemUser = context.Users.AsNoTracking().FirstOrDefault(u => u.UserName == "3031945451");
        if (systemUser != null)
        {
            SystemUserId = systemUser.Id;
            Log.Information("شناسه‌ی کاربر 'سیستم' با موفقیت کش شد: {UserId}", SystemUserId);
        }
        else
        {
            Log.Warning("کاربر 'سیستم' در دیتابیس یافت نشد. ممکن است عملیات‌های پس‌زمینه با خطا مواجه شوند.");
        }

        var adminUser = context.Users.AsNoTracking().FirstOrDefault(u => u.UserName == "3020347998");
        if (adminUser != null)
        {
            AdminUserId = adminUser.Id;
            Log.Information("شناسه‌ی کاربر 'ادمین' با موفقیت کش شد: {UserId}", AdminUserId);
        }
        else
        {
            Log.Warning("کاربر 'ادمین' در دیتابیس یافت نشد.");
        }
    }
}