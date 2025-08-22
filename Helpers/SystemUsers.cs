using System;
using System.Data.Entity;
using System.Linq;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Serilog;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس حرفه‌ای برای مدیریت شناسه‌های کاربران سیستمی در سیستم‌های پزشکی
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
/// SystemUsers.Initialize(context);
/// string systemUserId = SystemUsers.SystemUserId;
/// 
/// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
/// </summary>
public static class SystemUsers
{
    #region Constants

    /// <summary>
    /// نام کاربری کاربر سیستم
    /// این نام کاربری باید در همه محیط‌ها یکسان باشد
    /// </summary>
    public const string SystemUserName = "system";

    /// <summary>
    /// کد ملی کاربر سیستم (برای سیستم پسورد‌لس)
    /// این کد ملی باید در همه محیط‌ها یکسان باشد
    /// </summary>
    public const string SystemUserNationalCode = "3031945451";

    /// <summary>
    /// نام کاربری کاربر ادمین
    /// این نام کاربری باید در همه محیط‌ها یکسان باشد
    /// </summary>
    public const string AdminUserName = "admin";

    /// <summary>
    /// کد ملی کاربر ادمین (برای سیستم پسورد‌لس)
    /// این کد ملی باید در همه محیط‌ها یکسان باشد
    /// </summary>
    public const string AdminUserNationalCode = "3020347998";

    #endregion

    #region Properties

    /// <summary>
    /// شناسه کاربر سیستم
    /// این شناسه پس از فراخوانی متد Initialize تنظیم می‌شود
    /// </summary>
    public static string SystemUserId { get; private set; }

    /// <summary>
    /// شناسه کاربر ادمین
    /// این شناسه پس از فراخوانی متد Initialize تنظیم می‌شود
    /// </summary>
    public static string AdminUserId { get; private set; }

    /// <summary>
    /// بررسی آماده‌بودن سیستم
    /// </summary>
    public static bool IsInitialized { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// مقداردهی اولیه کلاس با کش کردن شناسه‌های کاربران سیستمی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - از کوئری‌های تکراری به دیتابیس جلوگیری می‌کند
    /// - برای عملیات پس‌زمینه ضروری است
    /// - برای سیستم حذف نرم و ردیابی حیاتی است
    /// </summary>
    /// <param name="context">DbContext سیستم</param>
    public static void Initialize(ApplicationDbContext context)
    {
        if (IsInitialized)
        {
            Log.Debug("SystemUsers قبلاً مقداردهی شده است. نیازی به مقداردهی مجدد نیست.");
            return;
        }

        try
        {
            Log.Information("در حال بارگذاری و کش کردن شناسه‌ی کاربران سیستمی...");

            // پاک کردن مقادیر قبلی
            SystemUserId = null;
            AdminUserId = null;
            IsInitialized = false;

            // دریافت کاربران سیستمی
            LoadSystemUsers(context);
            LoadAdminUsers(context);

            // بررسی کامل بودن اطلاعات
            ValidateSystemUsers();

            // علامت‌گذاری به عنوان مقداردهی شده
            IsInitialized = true;

            Log.Information("SystemUsers با موفقیت مقداردهی شد.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در مقداردهی اولیه SystemUsers");
            throw;
        }
    }

    /// <summary>
    /// بررسی اینکه آیا شناسه داده شده متعلق به کاربر سیستم است
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای مدیریت عملیات پس‌زمینه
    /// - برای امنیت سیستم
    /// - برای ردیابی تغییرات
    /// </summary>
    public static bool IsSystemUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return string.Equals(userId, SystemUserId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// بررسی اینکه آیا شناسه داده شده متعلق به کاربر ادمین است
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای مدیریت دسترسی‌ها
    /// - برای امنیت سیستم
    /// - برای ردیابی تغییرات
    /// </summary>
    public static bool IsAdminUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return string.Equals(userId, AdminUserId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// بررسی اینکه آیا کاربر فعلی یکی از کاربران سیستمی است
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای مدیریت دسترسی‌ها
    /// - برای امنیت سیستم
    /// - برای ردیابی تغییرات
    /// </summary>
    public static bool IsSystemOrAdminUser(string userId)
    {
        return IsSystemUser(userId) || IsAdminUser(userId);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// بارگذاری کاربران سیستمی
    /// </summary>
    private static void LoadSystemUsers(ApplicationDbContext context)
    {
        try
        {
            // در سیستم‌های پزشکی ایرانی، کاربر سیستم معمولاً با کد ملی شناسایی می‌شود
            var systemUser = context.Users
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.UserName == SystemUserName ||
                    u.UserName == SystemUserNationalCode);

            if (systemUser != null)
            {
                SystemUserId = systemUser.Id;
                Log.Information("شناسه‌ی کاربر 'سیستم' با موفقیت کش شد: {UserId}", SystemUserId);
            }
            else
            {
                Log.Warning("کاربر 'سیستم' در دیتابیس یافت نشد. ممکن است عملیات‌های پس‌زمینه با خطا مواجه شوند.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در بارگذاری کاربر سیستم");
            // ادامه فرآیند، ممکن است در محیط‌های تست یا توسعه کاربر سیستم وجود نداشته باشد
        }
    }

    /// <summary>
    /// بارگذاری کاربران ادمین
    /// </summary>
    private static void LoadAdminUsers(ApplicationDbContext context)
    {
        try
        {
            // در سیستم‌های پزشکی ایرانی، کاربر ادمین معمولاً با کد ملی شناسایی می‌شود
            var adminUser = context.Users
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.UserName == AdminUserName ||
                    u.UserName == AdminUserNationalCode);

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
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در بارگذاری کاربر ادمین");
            // ادامه فرآیند، ممکن است در محیط‌های تست یا توسعه کاربر ادمین وجود نداشته باشد
        }
    }

    /// <summary>
    /// اعتبارسنجی شناسه‌های کاربران سیستمی
    /// </summary>
    private static void ValidateSystemUsers()
    {
        if (string.IsNullOrWhiteSpace(SystemUserId))
        {
            Log.Warning("هشدار: شناسه کاربر سیستم مقداردهی نشده است. ممکن است عملیات‌های پس‌زمینه با خطا مواجه شوند.");
        }
        else
        {
            Log.Debug("شناسه کاربر سیستم معتبر است: {SystemUserId}", SystemUserId);
        }

        if (string.IsNullOrWhiteSpace(AdminUserId))
        {
            Log.Warning("هشدار: شناسه کاربر ادمین مقداردهی نشده است.");
        }
        else
        {
            Log.Debug("شناسه کاربر ادمین معتبر است: {AdminUserId}", AdminUserId);
        }
    }

    #endregion
}