using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using ClinicApp.Helpers;
using ClinicApp.Models;
using Serilog;

namespace ClinicApp.Infrastructure;

/// <summary>
/// این کلاس مسئولیت مدیریت و اجرای خودکار مایگریشن‌های دیتابیس را بر عهده دارد.
/// این سیستم بر اساس فایل‌های SQL که به صورت Embedded Resource در پروژه قرار دارند، کار می‌کند.
/// </summary>
public class DatabaseMigrationManager
{
    private readonly ApplicationDbContext _context;

    public DatabaseMigrationManager(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// مایگریشن‌های جدید را پیدا کرده و آن‌ها را در یک تراکنش امن اجرا می‌کند.
    /// </summary>
    public void ApplyMigrations()
    {
        Log.Information("شروع فرآیند بررسی و اعمال مایگریشن‌های دیتابیس...");

        // ۱. دریافت لیست تمام نسخه‌هایی که قبلاً در دیتابیس اعمال شده‌اند.
        var appliedVersions = _context.DatabaseVersions.AsNoTracking().Select(v => v.VersionNumber).ToList();

        // ۲. پیدا کردن تمام فایل‌های مایگریشن SQL از درون اسمبلی پروژه.
        var allMigrations = GetAllMigrationsFromAssembly();

        // ۳. فیلتر کردن مایگریشن‌هایی که جدید هستند و هنوز اعمال نشده‌اند.
        var pendingMigrations = allMigrations
            .Where(m => !appliedVersions.Contains(m.VersionNumber))
            .OrderBy(m => m.VersionNumber) // مرتب‌سازی بر اساس شماره نسخه برای اجرای به ترتیب
            .ToList();

        if (!pendingMigrations.Any())
        {
            Log.Information("دیتابیس به‌روز است. هیچ مایگریشن جدیدی یافت نشد.");
            return;
        }

        Log.Information("{Count} مایگریشن جدید برای اجرا یافت شد.", pendingMigrations.Count);

        // ۴. اجرای تمام مایگریشن‌های جدید در یک تراکنش (Transaction)
        // این تضمین می‌کند که اگر یکی از اسکریپت‌ها خطا دهد، تمام تغییرات لغو (Rollback) می‌شود.
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                foreach (var migration in pendingMigrations)
                {
                    Log.Information("در حال اجرای مایگریشن: {VersionNumber} - {Description}", migration.VersionNumber, migration.Description);

                    // اجرای اسکریپت SQL
                    _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, migration.Script);

                    // ثبت رکورد اجرای موفق این مایگریشن در جدول تاریخچه (DatabaseVersions)
                    var dbVersion = new DatabaseVersion
                    {
                        VersionNumber = migration.VersionNumber,
                        Description = migration.Description,
                        AppliedDate = DateTime.UtcNow,
                        AppliedByUserId = SystemUsers.SystemUserId, // استفاده از کاربر سیستم
                        MigrationScript = migration.Script // ذخیره متن اسکریپت برای حسابرسی
                    };
                    _context.DatabaseVersions.Add(dbVersion);
                    _context.SaveChanges();
                }

                transaction.Commit(); // اگر همه چیز موفق بود، تراکنش را نهایی کن
                Log.Information("تمام مایگریشن‌های جدید با موفقیت اعمال شدند.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "خطای بحرانی در زمان اجرای مایگریشن‌ها. تمام تغییرات لغو (Rollback) شد.");
                transaction.Rollback(); // اگر خطایی رخ داد، تراکنش را لغو کن
                throw; // پرتاب مجدد خطا برای متوقف کردن برنامه و اطلاع‌رسانی
            }
        }
    }

    /// <summary>
    /// این متد تمام فایل‌های SQL که به عنوان Embedded Resource علامت‌گذاری شده‌اند را از پوشه Migrations پیدا می‌کند.
    /// </summary>
    private List<Migration> GetAllMigrationsFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        // نام پوشه "Migrations" در اینجا مشخص می‌شود
        var resourceNames = assembly.GetManifestResourceNames().Where(n => n.Contains(".Migrations."));
        var migrations = new List<Migration>();

        foreach (var resourceName in resourceNames)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var script = reader.ReadToEnd();
                var fileName = Path.GetFileNameWithoutExtension(resourceName).Split('.').Last();
                var parts = fileName.Split(new[] { '_' }, 2);

                migrations.Add(new Migration
                {
                    VersionNumber = parts[0],
                    Description = parts.Length > 1 ? parts[1].Replace("_", " ") : "No Description",
                    Script = script
                });
            }
        }
        return migrations;
    }

    // کلاس کمکی داخلی برای نگهداری اطلاعات هر مایگریشن
    private class Migration
    {
        public string VersionNumber { get; set; }
        public string Description { get; set; }
        public string Script { get; set; }
    }
}