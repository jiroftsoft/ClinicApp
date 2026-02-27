using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Server;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using ClinicApp.Infrastructure.Hangfire;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Models;
using ClinicApp.Repositories.CMS;
using ClinicApp.Repositories.Notification;
using ClinicApp.Services;
using ClinicApp.Services.CMS;
using ClinicApp.Services.Notification;
using Serilog;
using Unity;
using Unity.Lifetime;

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

            // تزریق اسکریپت رفع خطای RealtimeGraph (statistics.intValue undefined) در داشبورد
            app.Use((context, next) => new HangfireDashboardFixMiddleware(next).Invoke(context.Environment));
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

        /// <summary>
        /// Creates a child Unity container with Transient overrides for types that are PerRequest in the root.
        /// Hangfire jobs run outside HTTP context; resolving PerRequest in background threads throws InvalidOperationException.
        /// Caller must dispose the returned container after use.
        /// </summary>
        private static IUnityContainer CreateHangfireScopeContainer()
        {
            var parent = UnityConfig.Container;
            var child = parent.CreateChildContainer();
            var transient = new TransientLifetimeManager();

            child.RegisterType<DbContext, ApplicationDbContext>(transient);
            child.RegisterType<ApplicationDbContext>(transient);
            child.RegisterType<INotificationQueueRepository, NotificationQueueRepository>(transient);
            child.RegisterType<IAppointmentNotificationQueueService, NotificationService>(transient);
            child.RegisterType<NotificationQueueProcessor>(transient);
            child.RegisterType<AppointmentReminderScheduler>(transient);

            return child;
        }

        /// <summary>
        /// اسکوپ Unity برای Job ارسال کمپین خبرنامه (بدون HttpContext).
        /// </summary>
        private static IUnityContainer CreateNewsletterScopeContainer()
        {
            var parent = UnityConfig.Container;
            var child = parent.CreateChildContainer();
            var transient = new TransientLifetimeManager();

            child.RegisterType<DbContext, ApplicationDbContext>(transient);
            child.RegisterType<ApplicationDbContext>(transient);
            child.RegisterInstance<ICurrentUserService>(new NewsletterJobUserStub());
            child.RegisterType<IChannelConfigRepository, ClinicApp.Repositories.CMS.ChannelConfigRepository>(transient);
            child.RegisterType<IChannelConfigProvider, ClinicApp.Services.CMS.ChannelConfigProviderService>(transient);
            child.RegisterType<INewsletterCampaignRepository, NewsletterCampaignRepository>(transient);
            child.RegisterType<INewsletterCampaignRecipientRepository, NewsletterCampaignRecipientRepository>(transient);
            child.RegisterType<INewsletterSubscriptionRepository, NewsletterSubscriptionRepository>(transient);
            child.RegisterType<INewsletterEmailService, NewsletterEmailService>(transient);
            child.RegisterType<INewsletterSmsService, NewsletterSmsService>(transient);
            child.RegisterType<INewsletterCampaignService, NewsletterCampaignService>(transient);

            return child;
        }

        /// <summary>
        /// Job ارسال واقعی کمپین خبرنامه (ایمیل/SMS) و به‌روزرسانی وضعیت به Sent/Failed و SentCount.
        /// از SendCampaignAsync با BackgroundJob.Enqueue فراخوانی می‌شود.
        /// در صورت هرگونه خطا، وضعیت کمپین به ناموفق تغییر می‌کند (ضدگلوله برای پروداکشن).
        /// </summary>
        public static async Task ProcessCampaignSendQueue(int campaignId, bool sendEmail, bool sendSms)
        {
            var scope = CreateNewsletterScopeContainer();
            try
            {
                var service = scope.Resolve<INewsletterCampaignService>();
                await service.ProcessCampaignSendQueueAsync(campaignId, sendEmail, sendSms);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در Job ارسال کمپین خبرنامه - CampaignId: {CampaignId}", campaignId);
                try
                {
                    var failScope = CreateNewsletterScopeContainer();
                    try
                    {
                        var failService = failScope.Resolve<INewsletterCampaignService>();
                        await failService.MarkCampaignAsFailedAsync(campaignId, ex.Message);
                    }
                    finally
                    {
                        failScope.Dispose();
                    }
                }
                catch (Exception markEx)
                {
                    Log.Error(markEx, "خطا در MarkCampaignAsFailedAsync پس از خطای Job - CampaignId: {CampaignId}", campaignId);
                }
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        // Recurring Jobs: resolve from a child container with Transient overrides (no HTTP context in background)
        public static async Task ProcessNotificationQueue()
        {
            var scope = CreateHangfireScopeContainer();
            try
            {
                var processor = scope.Resolve<NotificationQueueProcessor>();
                await processor.ProcessPendingAsync();
            }
            finally
            {
                scope.Dispose();
            }
        }

        public static async Task Schedule24HourReminders()
        {
            var scope = CreateHangfireScopeContainer();
            try
            {
                var scheduler = scope.Resolve<AppointmentReminderScheduler>();
                await scheduler.Schedule24HourRemindersAsync();
            }
            finally
            {
                scope.Dispose();
            }
        }

        public static async Task Schedule3HourReminders()
        {
            var scope = CreateHangfireScopeContainer();
            try
            {
                var scheduler = scope.Resolve<AppointmentReminderScheduler>();
                await scheduler.Schedule3HourRemindersAsync();
            }
            finally
            {
                scope.Dispose();
            }
        }

        public static async Task Schedule30MinuteReminders()
        {
            var scope = CreateHangfireScopeContainer();
            try
            {
                var scheduler = scope.Resolve<AppointmentReminderScheduler>();
                await scheduler.Schedule30MinuteRemindersAsync();
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
