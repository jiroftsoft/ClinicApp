# 📘 راهنمای کامل نصب و کانفیگ Hangfire در ASP.NET MVC5

## 📋 فهرست مطالب

1. [پیش‌نیازها](#پیش‌نیازها)
2. [نصب پکیج‌های NuGet](#نصب-پکیج‌های-nuget)
3. [ساختار فایل‌ها](#ساختار-فایل‌ها)
4. [کانفیگ OWIN Startup](#کانفیگ-owin-startup)
5. [کانفیگ SQL Server Storage](#کانفیگ-sql-server-storage)
6. [کانفیگ Unity DI Integration](#کانفیگ-unity-di-integration)
7. [کانفیگ Dashboard با Authorization](#کانفیگ-dashboard-با-authorization)
8. [کانفیگ Background Job Server](#کانفیگ-background-job-server)
9. [ثبت Recurring Jobs](#ثبت-recurring-jobs)
10. [Database Migration](#database-migration)
11. [تست و بررسی](#تست-و-بررسی)
12. [عیب‌یابی](#عیب‌یابی)
13. [بهترین Practices](#بهترین-practices)

---

## 🎯 پیش‌نیازها

### الزامات سیستم

- ✅ **.NET Framework 4.8**
- ✅ **ASP.NET MVC5**
- ✅ **OWIN** (Microsoft.Owin.Host.SystemWeb)
- ✅ **Unity Container** (برای Dependency Injection)
- ✅ **SQL Server** (برای Job Storage)
- ✅ **Visual Studio 2019+** یا **Visual Studio 2022**

### پکیج‌های OWIN موجود

قبل از نصب Hangfire، اطمینان حاصل کنید که این پکیج‌ها نصب هستند:

```xml
<package id="Microsoft.Owin" version="4.2.3" />
<package id="Microsoft.Owin.Host.SystemWeb" version="4.2.3" />
<package id="Owin" version="1.0" />
```

---

## 📦 نصب پکیج‌های NuGet

### ⚠️ نکته مهم

**برای پروژه‌های MVC5 (.NET Framework) از نسخه 1.7.34 استفاده کنید!**

نسخه‌های 1.8.x برای ASP.NET Core هستند و با MVC5 سازگار نیستند.

### روش 1: Package Manager Console

```powershell
# نصب پکیج اصلی Hangfire (meta-package)
Install-Package Hangfire -Version 1.7.34

# نصب پکیج SQL Server Storage
Install-Package Hangfire.SqlServer -Version 1.7.34
```

### روش 2: NuGet Package Manager UI

1. **Solution Explorer** → راست‌کلیک روی پروژه → **Manage NuGet Packages**
2. جستجوی `Hangfire` → انتخاب نسخه **1.7.34**
3. نصب `Hangfire` و `Hangfire.SqlServer`

### پکیج‌های نصب شده

بعد از نصب، در `packages.config` باید این خطوط وجود داشته باشند:

```xml
<package id="Hangfire" version="1.7.34" targetFramework="net48" />
<package id="Hangfire.Core" version="1.7.34" targetFramework="net48" />
<package id="Hangfire.SqlServer" version="1.7.34" targetFramework="net48" />
```

### ❌ پکیج‌های غیرضروری (حذف کنید)

این پکیج‌ها برای ASP.NET Core هستند و در MVC5 نیاز نیستند:

```xml
<!-- ❌ حذف کنید -->
<package id="Hangfire.AspNetCore" />
<package id="Hangfire.NetCore" />
<package id="Hangfire.Owin" /> <!-- این پکیج وجود ندارد! -->
```

---

## 📁 ساختار فایل‌ها

بعد از نصب، این فایل‌ها را ایجاد کنید:

```
ClinicApp/
├── App_Start/
│   └── Startup.Hangfire.cs          # کانفیگ اصلی Hangfire
├── Infrastructure/
│   └── Hangfire/
│       ├── HangfireUnityJobActivator.cs      # Unity DI Integration
│       └── HangfireAuthorizationFilter.cs    # Dashboard Authorization
└── Startup.cs                        # OWIN Startup (قبلاً موجود)
```

---

## ⚙️ کانفیگ OWIN Startup

### 1. فایل `Startup.cs`

اطمینان حاصل کنید که `ConfigureHangfire` فراخوانی می‌شود:

```csharp
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClinicApp.Startup))]
namespace ClinicApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigureHangfire(app); // ✅ فراخوانی Hangfire
        }
    }
}
```

---

## 🔧 کانفیگ SQL Server Storage

### فایل `App_Start/Startup.Hangfire.cs`

```csharp
using System;
using System.Configuration;
using System.Web.Hosting;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;
using ClinicApp.Infrastructure.Hangfire;
using Unity;

namespace ClinicApp
{
    /// <summary>
    /// راه‌اندازی Hangfire — صف اعلان و یادآوری نوبت
    /// </summary>
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            // 1️⃣ بررسی Connection String
            var connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // در Production باید خطا بدهد
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found!");
            }

            // 2️⃣ کانفیگ SQL Server Storage
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionString, 
                new SqlServerStorageOptions
                {
                    // Timeout برای دسته‌ای از دستورات
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    
                    // Timeout برای Jobهای نامرئی
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    
                    // فاصله زمانی polling برای صف‌ها
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    
                    // استفاده از Isolation Level توصیه شده
                    UseRecommendedIsolationLevel = true,
                    
                    // استفاده از Page Locks برای Dequeue
                    UsePageLocksOnDequeue = true,
                    
                    // غیرفعال کردن Global Locks (برای Performance بهتر)
                    DisableGlobalLocks = true
                });

            // ادامه در بخش‌های بعدی...
        }
    }
}
```

### تنظیمات `SqlServerStorageOptions`

| تنظیم | مقدار پیش‌فرض | توصیه | توضیح |
|-------|---------------|-------|-------|
| `CommandBatchMaxTimeout` | 30s | 5min | Timeout برای دسته دستورات |
| `SlidingInvisibilityTimeout` | 5min | 5min | Timeout برای Jobهای نامرئی |
| `QueuePollInterval` | 15s | 15s | فاصله polling صف‌ها |
| `UseRecommendedIsolationLevel` | false | true | Isolation Level بهینه |
| `UsePageLocksOnDequeue` | false | true | استفاده از Page Locks |
| `DisableGlobalLocks` | false | true | غیرفعال کردن Global Locks |

---

## 🔌 کانفیگ Unity DI Integration

### 1. فایل `Infrastructure/Hangfire/HangfireUnityJobActivator.cs`

```csharp
using System;
using Hangfire;

namespace ClinicApp.Infrastructure.Hangfire
{
    /// <summary>
    /// فعال‌سازی Jobهای Hangfire از طریق Unity Container
    /// در محیط Background فاقد HttpContext، مستقیم از Container حل وابستگی می‌شود
    /// </summary>
    public class HangfireUnityJobActivator : JobActivator
    {
        private readonly Func<Type, object> _resolver;

        public HangfireUnityJobActivator(Func<Type, object> resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public override object ActivateJob(Type jobType)
        {
            return _resolver(jobType);
        }
    }
}
```

### 2. ثبت در `Startup.Hangfire.cs`

```csharp
// بعد از UseSqlServerStorage

// 3️⃣ کانفیگ Unity Job Activator
var container = UnityConfig.Container;
GlobalConfiguration.Configuration.UseActivator(
    new HangfireUnityJobActivator(type => container.Resolve(type, null)));
```

### ✅ مزایای Unity Integration

- ✅ حل وابستگی خودکار برای Jobها
- ✅ استفاده از همان Container که در MVC استفاده می‌شود
- ✅ پشتیبانی از Lifetime Management (Singleton, Transient, etc.)
- ✅ امکان استفاده از Interface ها در Jobها

---

## 🔐 کانفیگ Dashboard با Authorization

### 1. فایل `Infrastructure/Hangfire/HangfireAuthorizationFilter.cs`

```csharp
using System;
using System.Web;
using Hangfire.Dashboard;
using ClinicApp.Models.Core;

namespace ClinicApp.Infrastructure.Hangfire
{
    /// <summary>
    /// محدودیت دسترسی به داشبورد Hangfire
    /// فقط نقش ادمین یا در محیط Development بدون لاگین (localhost)
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly bool _allowAnonymousInDevelopment;

        public HangfireAuthorizationFilter(bool allowAnonymousInDevelopment = false)
        {
            _allowAnonymousInDevelopment = allowAnonymousInDevelopment;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
                return false;

            // در محیط Development می‌توان دسترسی بدون لاگین را مجاز کرد
            // (فقط روی localhost توصیه می‌شود)
            if (_allowAnonymousInDevelopment && IsLocalRequest(httpContext))
                return true;

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            // فقط کاربران با نقش ادمین
            return httpContext.User.IsInRole(AppRoles.Admin);
        }

        private static bool IsLocalRequest(HttpContext httpContext)
        {
            try
            {
                return httpContext.Request.IsLocal;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

### 2. ثبت Dashboard در `Startup.Hangfire.cs`

```csharp
// بعد از UseActivator

// 4️⃣ کانفیگ Dashboard با Authorization
var dashboardPath = "/hangfire";
var isDevelopment = ConfigurationManager.AppSettings["Environment"]?
    .Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

var options = new DashboardOptions
{
    // فیلتر دسترسی
    Authorization = new[] { new HangfireAuthorizationFilter(isDevelopment) },
    
    // عنوان داشبورد
    DashboardTitle = "صف اعلان و Jobs — کلینیک شفا",
    
    // مسیر بازگشت به اپلیکیشن
    AppPath = "~/",
    
    // عدم نمایش Connection String (امنیت)
    DisplayStorageConnectionString = false
};

app.UseHangfireDashboard(dashboardPath, options);
```

### 🔒 تنظیمات امنیتی

| تنظیم | مقدار | توضیح |
|-------|-------|-------|
| `Authorization` | `HangfireAuthorizationFilter` | فیلتر دسترسی |
| `DisplayStorageConnectionString` | `false` | عدم نمایش Connection String |
| `DashboardTitle` | Custom | عنوان داشبورد |

---

## 🚀 کانفیگ Background Job Server

```csharp
// بعد از UseHangfireDashboard

// 5️⃣ کانفیگ Background Job Server
app.UseHangfireServer(new BackgroundJobServerOptions
{
    // تعداد Worker Threads
    // پیش‌فرض: Environment.ProcessorCount * 5
    WorkerCount = Math.Max(Environment.ProcessorCount, 2),
    
    // صف‌های پردازش
    Queues = new[] { "default", "notifications" },
    
    // نام Server (اختیاری)
    ServerName = Environment.MachineName
});
```

### تنظیمات `BackgroundJobServerOptions`

| تنظیم | مقدار پیش‌فرض | توصیه | توضیح |
|-------|---------------|-------|-------|
| `WorkerCount` | `ProcessorCount * 5` | `Max(ProcessorCount, 2)` | تعداد Worker Threads |
| `Queues` | `["default"]` | `["default", "notifications"]` | لیست صف‌ها |
| `ServerName` | `GUID` | `MachineName` | نام Server |

---

## ⏰ ثبت Recurring Jobs

### روش صحیح برای Hangfire 1.7.34

در نسخه 1.7.34، باید از **static methods** استفاده کنید:

```csharp
// بعد از UseHangfireServer

// 6️⃣ ثبت Recurring Jobها — فقط در Application (نه در محیط تست)
if (HostingEnvironment.IsHosted)
{
    // پردازش صف اعلان — هر ۱ دقیقه
    RecurringJob.AddOrUpdate(
        "notification-queue-processor",
        () => ProcessNotificationQueue(),
        "*/1 * * * *", // هر دقیقه
        TimeZoneInfo.Local);

    // یادآوری ۲۴ ساعت قبل — هر ۱۵ دقیقه اسکن
    RecurringJob.AddOrUpdate(
        "reminder-24h",
        () => Schedule24HourReminders(),
        "*/15 * * * *",
        TimeZoneInfo.Local);

    // یادآوری ۳ ساعت قبل
    RecurringJob.AddOrUpdate(
        "reminder-3h",
        () => Schedule3HourReminders(),
        "*/15 * * * *",
        TimeZoneInfo.Local);

    // یادآوری ۳۰ دقیقه قبل
    RecurringJob.AddOrUpdate(
        "reminder-30min",
        () => Schedule30MinuteReminders(),
        "*/15 * * * *",
        TimeZoneInfo.Local);
}
```

### Helper Methods

```csharp
// Helper methods برای Recurring Jobs
// Hangfire از JobActivator برای resolve کردن dependencies استفاده می‌کند

public static async Task ProcessNotificationQueue()
{
    var container = UnityConfig.Container;
    var processor = container.Resolve<NotificationQueueProcessor>();
    await processor.ProcessPendingAsync();
}

public static async Task Schedule24HourReminders()
{
    var container = UnityConfig.Container;
    var scheduler = container.Resolve<AppointmentReminderScheduler>();
    await scheduler.Schedule24HourRemindersAsync();
}

public static async Task Schedule3HourReminders()
{
    var container = UnityConfig.Container;
    var scheduler = container.Resolve<AppointmentReminderScheduler>();
    await scheduler.Schedule3HourRemindersAsync();
}

public static async Task Schedule30MinuteReminders()
{
    var container = UnityConfig.Container;
    var scheduler = container.Resolve<AppointmentReminderScheduler>();
    await scheduler.Schedule30MinuteRemindersAsync();
}
```

### ⚠️ نکات مهم

1. **استفاده از Static Methods**: در Hangfire 1.7.34، Recurring Jobs باید static باشند
2. **Unity Container**: هر helper method باید container را resolve کند
3. **Async Support**: می‌توانید از `async Task` استفاده کنید
4. **HostingEnvironment.IsHosted**: فقط در محیط Application اجرا می‌شود (نه در Unit Tests)

### Cron Expression Examples

| Expression | توضیح |
|------------|-------|
| `*/1 * * * *` | هر 1 دقیقه |
| `*/15 * * * *` | هر 15 دقیقه |
| `0 * * * *` | هر ساعت |
| `0 0 * * *` | هر روز در نیمه شب |
| `0 0 * * 0` | هر یکشنبه در نیمه شب |

---

## 🗄️ Database Migration

### ایجاد جداول Hangfire

Hangfire به صورت خودکار جداول را ایجاد می‌کند. اما می‌توانید به صورت دستی هم انجام دهید:

#### روش 1: خودکار (توصیه می‌شود)

هنگام اولین اجرای اپلیکیشن، Hangfire به صورت خودکار جداول را ایجاد می‌کند.

#### روش 2: دستی با SQL Script

```sql
-- اجرای این اسکریپت در SQL Server
-- Hangfire جداول زیر را ایجاد می‌کند:

-- HangFire.[Schema]
-- HangFire.[Set]
-- HangFire.[Hash]
-- HangFire.[List]
-- HangFire.[Job]
-- HangFire.[State]
-- HangFire.[JobParameter]
-- HangFire.[JobQueue]
-- HangFire.[Server]
-- HangFire.[Counter]
-- HangFire.[AggregatedCounter]
```

### بررسی جداول

```sql
-- بررسی وجود Schema
SELECT * FROM sys.schemas WHERE name = 'HangFire'

-- بررسی جداول
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'HangFire'
```

---

## ✅ تست و بررسی

### 1. Build پروژه

```powershell
# در Package Manager Console
Build-Solution
```

### 2. اجرای پروژه

1. **F5** برای اجرای پروژه
2. بررسی **Output Window** برای خطاهای Hangfire

### 3. بررسی Dashboard

1. مراجعه به: `http://localhost:port/hangfire`
2. باید Dashboard Hangfire نمایش داده شود
3. بررسی **Recurring Jobs** — باید 4 job ثبت شده باشند

### 4. بررسی Database

```sql
-- بررسی Jobهای ثبت شده
SELECT * FROM HangFire.[Set] WHERE [Key] LIKE 'recurring-jobs:%'

-- بررسی Server
SELECT * FROM HangFire.[Server]

-- بررسی Jobهای در حال اجرا
SELECT * FROM HangFire.[Job] WHERE StateName = 'Processing'
```

### 5. تست Recurring Job

```csharp
// در یک Controller یا Console App
using Hangfire;

// اجرای دستی یک Job
RecurringJob.Trigger("notification-queue-processor");
```

---

## 🐛 عیب‌یابی

### خطای 1: "Unable to find version '1.8.23' of package 'Hangfire.Owin'"

**علت**: پکیج `Hangfire.Owin` وجود ندارد!

**راه‌حل**:
1. حذف `Hangfire.Owin` از `packages.config`
2. حذف Reference از `.csproj`
3. استفاده از نسخه **1.7.34**

### خطای 2: "The type or namespace name 'Hangfire' could not be found"

**علت**: پکیج‌ها restore نشده‌اند

**راه‌حل**:
```powershell
# Restore NuGet Packages
Update-Package -reinstall
```

### خطای 3: "CS1503: Argument 1: cannot convert from 'string' to 'Expression'"

**علت**: استفاده از signature نادرست `RecurringJob.AddOrUpdate`

**راه‌حل**: استفاده از static helper methods (مطابق مستند بالا)

### خطای 4: Dashboard نمایش داده نمی‌شود

**علت**: مشکل در Authorization یا Route

**راه‌حل**:
1. بررسی `Authorization` filter
2. بررسی Route در `Startup.cs`
3. بررسی لاگ‌های OWIN

### خطای 5: Jobs اجرا نمی‌شوند

**علت**: مشکل در JobActivator یا Unity Container

**راه‌حل**:
1. بررسی ثبت Services در Unity
2. بررسی `HangfireUnityJobActivator`
3. بررسی لاگ‌های Hangfire

---

## 🎯 بهترین Practices

### 1. امنیت

- ✅ **همیشه** Authorization Filter استفاده کنید
- ✅ **هرگز** Connection String را در Dashboard نمایش ندهید
- ✅ در Production، فقط Admin دسترسی داشته باشد

### 2. Performance

- ✅ استفاده از `DisableGlobalLocks = true`
- ✅ تنظیم `QueuePollInterval` مناسب
- ✅ استفاده از صف‌های جداگانه برای Jobهای مختلف

### 3. Monitoring

- ✅ بررسی Dashboard به صورت منظم
- ✅ مانیتورینگ Failed Jobs
- ✅ استفاده از Serilog برای لاگ‌گیری

### 4. Error Handling

```csharp
// استفاده از Automatic Retry
[AutomaticRetry(Attempts = 3)]
public static async Task ProcessNotificationQueue()
{
    // ...
}
```

### 5. Testing

```csharp
// در Unit Tests، Hangfire را Mock کنید
if (!HostingEnvironment.IsHosted)
{
    // Skip Hangfire configuration
    return;
}
```

---

## 📊 خلاصه کانفیگ نهایی

### فایل کامل `Startup.Hangfire.cs`

```csharp
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Hosting;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Server;
using Hangfire.SqlServer;
using Owin;
using ClinicApp.Infrastructure.Hangfire;
using ClinicApp.Services.Notification;
using Unity;

namespace ClinicApp
{
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            var connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                return;

            // 1. SQL Server Storage
            GlobalConfiguration.Configuration.UseSqlServerStorage(
                connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

            // 2. Unity DI
            var container = UnityConfig.Container;
            GlobalConfiguration.Configuration.UseActivator(
                new HangfireUnityJobActivator(type => container.Resolve(type, null)));

            // 3. Dashboard
            var dashboardPath = "/hangfire";
            var isDevelopment = ConfigurationManager.AppSettings["Environment"]?
                .Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;

            var options = new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter(isDevelopment) },
                DashboardTitle = "صف اعلان و Jobs — کلینیک شفا",
                AppPath = "~/",
                DisplayStorageConnectionString = false
            };

            app.UseHangfireDashboard(dashboardPath, options);

            // 4. Background Job Server
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Math.Max(Environment.ProcessorCount, 2),
                Queues = new[] { "default", "notifications" }
            });

            // 5. Recurring Jobs
            if (HostingEnvironment.IsHosted)
            {
                RecurringJob.AddOrUpdate(
                    "notification-queue-processor",
                    () => ProcessNotificationQueue(),
                    "*/1 * * * *",
                    TimeZoneInfo.Local);

                RecurringJob.AddOrUpdate(
                    "reminder-24h",
                    () => Schedule24HourReminders(),
                    "*/15 * * * *",
                    TimeZoneInfo.Local);

                RecurringJob.AddOrUpdate(
                    "reminder-3h",
                    () => Schedule3HourReminders(),
                    "*/15 * * * *",
                    TimeZoneInfo.Local);

                RecurringJob.AddOrUpdate(
                    "reminder-30min",
                    () => Schedule30MinuteReminders(),
                    "*/15 * * * *",
                    TimeZoneInfo.Local);
            }
        }

        // Helper Methods
        public static async Task ProcessNotificationQueue()
        {
            var container = UnityConfig.Container;
            var processor = container.Resolve<NotificationQueueProcessor>();
            await processor.ProcessPendingAsync();
        }

        public static async Task Schedule24HourReminders()
        {
            var container = UnityConfig.Container;
            var scheduler = container.Resolve<AppointmentReminderScheduler>();
            await scheduler.Schedule24HourRemindersAsync();
        }

        public static async Task Schedule3HourReminders()
        {
            var container = UnityConfig.Container;
            var scheduler = container.Resolve<AppointmentReminderScheduler>();
            await scheduler.Schedule3HourRemindersAsync();
        }

        public static async Task Schedule30MinuteReminders()
        {
            var container = UnityConfig.Container;
            var scheduler = container.Resolve<AppointmentReminderScheduler>();
            await scheduler.Schedule30MinuteRemindersAsync();
        }
    }
}
```

---

## 📚 منابع و مراجع

- [Hangfire Documentation](https://docs.hangfire.io/)
- [Hangfire GitHub](https://github.com/HangfireIO/Hangfire)
- [OWIN Documentation](https://owin.org/)
- [Unity Container](https://github.com/unitycontainer/unity)

---

## ✅ چک‌لیست نهایی

- [ ] پکیج‌های Hangfire 1.7.34 نصب شده‌اند
- [ ] فایل `Startup.Hangfire.cs` ایجاد شده
- [ ] فایل `HangfireUnityJobActivator.cs` ایجاد شده
- [ ] فایل `HangfireAuthorizationFilter.cs` ایجاد شده
- [ ] `ConfigureHangfire` در `Startup.cs` فراخوانی می‌شود
- [ ] Connection String در `Web.config` تنظیم شده
- [ ] Dashboard در `/hangfire` قابل دسترسی است
- [ ] Recurring Jobs ثبت شده‌اند
- [ ] Database جداول Hangfire را دارد
- [ ] Authorization Filter کار می‌کند

---

**تاریخ ایجاد**: 2024  
**نسخه**: 1.0  
**نویسنده**: ClinicApp Development Team

