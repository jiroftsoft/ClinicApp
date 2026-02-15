using System;
using System.Configuration;
using System.Linq.Expressions;
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
    /// <summary>
    /// راه‌اندازی فوق‌حرفه‌ای Hangfire — صف اعلان و یادآوری نوبت
    /// </summary>
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
                return;

            // ۱) ذخیره‌سازی SQL Server — قبل از هر چیز
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

            // ۲) حل وابستگی از Unity (بدون وابستگی به HttpContext)
            var container = UnityConfig.Container;
            GlobalConfiguration.Configuration.UseActivator(
                new HangfireUnityJobActivator(type => container.Resolve(type, null)));

            // ۳) داشبورد با محدودیت دسترسی (فقط ادمین یا محیط توسعه)
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
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = Math.Max(Environment.ProcessorCount, 2),
                Queues = new[] { "default", "notifications" }
            });

            // ۴) ثبت Recurring Jobها — فقط در Application (نه در محیط تست)
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
        }

        // Helper methods for Recurring Jobs - Hangfire will use JobActivator to resolve dependencies
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
