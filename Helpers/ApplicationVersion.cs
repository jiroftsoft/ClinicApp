using ClinicApp.Models;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس حرفه‌ای برای مدیریت نسخه‌بندی در سیستم‌های پزشکی
/// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
/// 
/// 1. کاملاً سازگار با Semantic Versioning 2.0.0
/// 2. پشتیبانی از محیط‌های مختلف (ویندوز، لینوکس، داکر)
/// 3. رعایت استانداردهای امنیتی سیستم‌های پزشکی
/// 4. ارائه اطلاعات کامل برای ردیابی و امنیت
/// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
/// 6. پشتیبانی از نسخه‌بندی پایگاه داده
/// 
/// استفاده:
/// string version = ApplicationVersion.GetVersion();
/// string fullInfo = ApplicationVersion.GetFullVersionInfo();
/// 
/// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و اطلاعات کاملی را ارائه می‌دهد
/// </summary>
public static class ApplicationVersion
{
    private static readonly ILogger _log = Log.ForContext(typeof(ApplicationVersion));
    private static readonly Lazy<VersionInfo> _versionInfo = new Lazy<VersionInfo>(GetVersionInfoInternal);

    #region Public Methods (روش‌های عمومی)

    /// <summary>   
    /// دریافت نسخه اصلی برنامه به فرمت Major.Minor
    /// برای نمایش در UI و گزارش‌ها استفاده می‌شود
    /// </summary>
    public static string GetVersion()
    {
        return _versionInfo.Value.MainVersion;
    }

    /// <summary>
    /// دریافت نسخه کامل برنامه شامل Major.Minor.Build.Revision
    /// برای استفاده در گزارش‌های فنی و عیب‌یابی
    /// </summary>
    public static string GetFullVersion()
    {
        return _versionInfo.Value.FullVersion;
    }

    /// <summary>
    /// دریافت اطلاعات کامل نسخه شامل نسخه، تاریخ انتشار و محیط اجرا
    /// برای استفاده در صفحه درباره (About) سیستم
    /// </summary>
    public static string GetFullVersionInfo()
    {
        var info = _versionInfo.Value;
        return $"{info.MainVersion} (ساخت: {info.BuildNumber}) - {info.ReleaseDate:yyyy/MM/dd HH:mm}\n" +
               $"پایگاه داده: {info.DatabaseVersion}\n" +
               $"محیط اجرا: {info.RuntimeEnvironment}";
    }

    /// <summary>
    /// دریافت تاریخ انتشار نسخه
    /// برای سیستم‌های پزشکی بسیار حیاتی است
    /// </summary>
    public static DateTime GetReleaseDate()
    {
        return _versionInfo.Value.ReleaseDate;
    }

    /// <summary>
    /// دریافت شماره ساخت (Build Number)
    /// برای ردیابی دقیق تغییرات در سیستم‌های پزشکی
    /// </summary>
    public static string GetBuildNumber()
    {
        return _versionInfo.Value.BuildNumber;
    }

    /// <summary>
    /// دریافت اطلاعات محیط اجرای برنامه
    /// برای عیب‌یابی و پشتیبانی سیستم‌های پزشکی
    /// </summary>
    public static string GetRuntimeEnvironment()
    {
        return _versionInfo.Value.RuntimeEnvironment;
    }

    /// <summary>
    /// دریافت نسخه پایگاه داده
    /// برای سیستم‌های پزشکی که نسخه پایگاه داده با نسخه برنامه متفاوت است
    /// </summary>
    public static string GetDatabaseVersion()
    {
        return _versionInfo.Value.DatabaseVersion;
    }

    /// <summary>
    /// بررسی آیا نسخه فعلی به روز است یا خیر
    /// برای سیستم‌های پزشکی که نیاز به به‌روزرسانی‌های امنیتی فوری دارند
    /// </summary>
    public static bool IsVersionUpToDate()
    {
        return _versionInfo.Value.IsUpToDate;
    }

    #endregion

    #region Private Helper Methods (روش‌های کمکی خصوصی)

    private static VersionInfo GetVersionInfoInternal()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var informationalVersion = GetInformationalVersion(assembly);
            var buildDate = GetBuildDate(assembly);

            // اطلاعات پایگاه داده
            string dbVersion = "نامشخص";
            bool isUpToDate = true;

            try
            {
                using var context = new ApplicationDbContext();
                var latestVersion = context.DatabaseVersions
                    .OrderByDescending(v => v.AppliedDate)
                    .FirstOrDefault();

                if (latestVersion != null)
                {
                    dbVersion = latestVersion.VersionNumber;
                    isUpToDate = dbVersion == GetExpectedDatabaseVersion();
                }
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "Cannot access database version. This is expected during initial setup or migrations.");
                dbVersion = "در دسترس نیست (در حال راه‌اندازی اولیه یا مهاجرت)";
            }

            // محیط اجرا
            string runtimeEnvironment = DetermineRuntimeEnvironment();

            return new VersionInfo
            {
                MainVersion = version != null ? $"{version.Major}.{version.Minor}" : "نامشخص",
                FullVersion = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "نامشخص",
                BuildNumber = informationalVersion,
                ReleaseDate = buildDate,
                DatabaseVersion = dbVersion,
                RuntimeEnvironment = runtimeEnvironment,
                IsUpToDate = isUpToDate
            };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error getting application version information");

            // در صورت بروز خطا، اطلاعات پایه برمی‌گردانیم
            return new VersionInfo
            {
                MainVersion = "خطا در دریافت نسخه",
                FullVersion = "خطا در دریافت نسخه",
                BuildNumber = "نامشخص",
                ReleaseDate = DateTime.MinValue,
                DatabaseVersion = "نامشخص",
                RuntimeEnvironment = DetermineRuntimeEnvironment(),
                IsUpToDate = false
            };
        }
    }

    private static string GetInformationalVersion(Assembly assembly)
    {
        try
        {
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute?.InformationalVersion ?? "نامشخص";
        }
        catch
        {
            return "نامشخص";
        }
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        try
        {
            // روش 1: استفاده از AssemblyFileVersion
            var attribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            if (attribute != null && DateTime.TryParseExact(
                attribute.Version.Substring(0, 8),
                "yyyyMMdd",
                null,
                System.Globalization.DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }

            // روش 2: استفاده از زمان ایجاد فایل
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && File.Exists(location))
            {
                return File.GetLastWriteTime(location);
            }

            // روش 3: تخمین زمان بر اساس AssemblyFileVersion
            if (attribute != null)
            {
                var parts = attribute.Version.Split('.');
                if (parts.Length >= 4 &&
                    int.TryParse(parts[2], out int year) &&
                    int.TryParse(parts[3], out int day))
                {
                    // فرض بر این است که سال از 2000 شروع می‌شود
                    year += 2000;
                    return new DateTime(year, 1, 1).AddDays(day - 1);
                }
            }

            return DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Error getting build date");
            return DateTime.UtcNow;
        }
    }

    private static string DetermineRuntimeEnvironment()
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            // تشخیص محیط اجرا
            bool isDocker = File.Exists("/.dockerenv") ||
                            Directory.Exists("/proc/self/cgroup") &&
                            File.ReadAllText("/proc/self/cgroup").Contains("docker");

            bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix;
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            string osInfo = isDocker ? "Docker" :
                            isLinux ? "Linux" :
                            isWindows ? "Windows" :
                            "نامشخص";

            return $"{environment} - {osInfo}";
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Error determining runtime environment");
            return "محیط اجرا نامشخص";
        }
    }

    private static string GetExpectedDatabaseVersion()
    {
        // این متد باید بر اساس نیازهای خاص پروژه پیاده‌سازی شود
        // به عنوان مثال، می‌تواند از یک فایل کانفیگ یا ثابت خوانده شود
        return "1.0.0";
    }

    #endregion

    #region Helper Classes (کلاس‌های کمکی)

    private class VersionInfo
    {
        public string MainVersion { get; set; }
        public string FullVersion { get; set; }
        public string BuildNumber { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string DatabaseVersion { get; set; }
        public string RuntimeEnvironment { get; set; }
        public bool IsUpToDate { get; set; }
    }

    #endregion
}