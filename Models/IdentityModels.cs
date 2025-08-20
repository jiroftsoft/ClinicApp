using ClinicApp.Models.Entities;
using EntityFramework.DynamicFilters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Serilog;
using System;
using System.Collections;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ClinicApp.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        #region Constructors & Factory

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            // تنظیمات پیش‌فرض برای عملکرد بهتر در محیط عملیاتی پزشکی
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.ValidateOnSaveEnabled = true;
            Configuration.UseDatabaseNullSemantics = true;

            // راه‌حل صحیح برای تنظیم CommandTimeout
            Database.CommandTimeout = 180; // 3 دقیقه برای عملیات‌های سنگین
        }

        public static ApplicationDbContext Create()
        {
            var context = new ApplicationDbContext();

            // راه‌حل صحیح برای تنظیم CommandTimeout در متد Create
            context.Database.CommandTimeout = 180; // 3 دقیقه برای عملیات‌های سنگین
            return context;
        }

        #endregion

        #region DbSet Definitions

        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<InsuranceTariff> InsuranceTariffs { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Reception> Receptions { get; set; }
        public DbSet<ReceptionItem> ReceptionItems { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        // Payment مدل قدیمی حذف شده است
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } // مدل جدید تراکنش‌های مالی
        public DbSet<CashSession> CashSessions { get; set; } // شیفت‌های صندوق
        public DbSet<PosTerminal> PosTerminals { get; set; } // دستگاه‌های پوز
        public DbSet<ReceiptPrint> ReceiptPrints { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        #endregion

        #region Core Overrides (سیستم‌های حیاتی)

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 1. فعال‌سازی پشتیبانی کامل از فارسی (Unicode) برای سیستم‌های پزشکی ایرانی
            modelBuilder.Properties<string>()
                .Configure(p => p.IsUnicode(true));

            // 2. غیرفعال‌سازی کلاسیک Conventionهای ناکارآمد
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // 3. تنظیم کانفیگ‌های موجودیت‌ها
            RegisterEntityConfigurations(modelBuilder);

            // 4. اعمال فیلتر سراسری Soft Delete برای سیستم‌های پزشکی
            // حذف فیزیکی اطلاعات پزشکی مجاز نیست
            modelBuilder.Filter("IsDeletedFilter", (ISoftDelete d) => d.IsDeleted, false);

            // 5. تنظیم روابط موجودیت‌ها برای سیستم‌های پزشکی

            // 6. فعال‌سازی Dynamic Filters برای سیستم‌های پیچیده پزشکی
            modelBuilder.Filter("ActiveClinics", (Clinic c) => c.IsActive, true);
            modelBuilder.Filter("ActiveDepartments", (Department d) => d.IsActive, true);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            try
            {
                ApplyAuditInformation();
                return base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // ثبت خطا در سیستم مانیتورینگ پزشکی
                HandleDatabaseException(ex);
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ApplyAuditInformation();
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // ثبت خطا در سیستم مانیتورینگ پزشکی
                HandleDatabaseException(ex);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods (سیستم‌های داخلی)

        private void RegisterEntityConfigurations(DbModelBuilder modelBuilder)
        {
            var configTypes = typeof(ClinicConfig).Assembly.GetTypes()
                .Where(t => t.BaseType != null &&
                            t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var type in configTypes)
            {
                dynamic config = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(config);
            }
        }



        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ITrackable &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            var userId = GetCurrentUserId();

            foreach (var entry in entries)
            {
                var trackable = (ITrackable)entry.Entity;

                // اطمینان از وجود کاربر معتبر برای سیستم‌های پزشکی
                string validUserId = string.IsNullOrEmpty(userId) ? "System" : userId;

                if (entry.State == EntityState.Added)
                {
                    trackable.CreatedAt = DateTime.UtcNow;
                    trackable.CreatedByUserId = validUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    trackable.UpdatedAt = DateTime.UtcNow;
                    trackable.UpdatedByUserId = validUserId;
                }
            }
        }

        private string GetCurrentUserId()
        {
            // استخراج ایمن شناسه کاربر فعلی برای سیستم‌های پزشکی
            try
            {
                // روش اول: برای محیط‌های وب
                if (System.Web.HttpContext.Current != null)
                {
                    var httpContext = System.Web.HttpContext.Current;
                    if (httpContext.User != null &&
                        httpContext.User.Identity != null &&
                        httpContext.User.Identity.IsAuthenticated)
                    {
                        return httpContext.User.Identity.GetUserId();
                    }
                }

                // روش دوم: برای محیط‌های غیر-وب یا سرویس‌های پس‌زمینه
                var httpContextStorage = HttpContextStorage.Current;
                if (httpContextStorage != null)
                {
                    if (httpContextStorage.User != null &&
                        httpContextStorage.User.Identity != null &&
                        httpContextStorage.User.Identity.IsAuthenticated)
                    {
                        return httpContextStorage.User.Identity.GetUserId();
                    }
                }

                // روش سوم: استفاده از Thread.CurrentPrincipal برای تمام محیط‌ها
                if (Thread.CurrentPrincipal != null)
                {
                    var principal = Thread.CurrentPrincipal;
                    if (principal.Identity != null && principal.Identity.IsAuthenticated)
                    {
                        return principal.Identity.GetUserId();
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت بروز خطا، سیستم به حالت ایمن می‌رود
                // این خطا را برای مانیتورینگ ثبت می‌کنیم
                Log.Error(ex, "خطا در دریافت شناسه کاربر جاری");
                return "System";
            }

            // اگر کاربری لاگین نکرده بود یا در محیط غیر-وب بودیم
            return "System";
        }

        private void HandleDatabaseException(DbUpdateException ex)
        {
            // سیستم پیشرفته مدیریت خطا برای محیط عملیاتی پزشکی
            var errorMessage = new System.Text.StringBuilder();
            errorMessage.AppendLine("خطای پایگاه داده رخ داده است:");

            if (ex.InnerException != null)
            {
                var sqlException = ex.InnerException.InnerException as System.Data.SqlClient.SqlException;
                if (sqlException != null)
                {
                    switch (sqlException.Number)
                    {
                        case 2627: // Unique constraint violation
                            errorMessage.AppendLine("خطا: رکورد تکراری ثبت شده است. لطفاً مطمئن شوید که نام دپارتمان در کلینیک مورد نظر تکراری نباشد.");
                            break;
                        case 547: // Foreign key violation
                            errorMessage.AppendLine("خطا: ارجاع به رکوردی که وجود ندارد. لطفاً مطمئن شوید کلینیک انتخاب شده فعال است.");
                            break;
                        case 208: // Invalid object name
                            errorMessage.AppendLine("خطا: جدول مورد نظر یافت نشد. لطفاً با پشتیبانی سیستم تماس بگیرید.");
                            break;
                        case 8152: // String or binary data would be truncated
                            errorMessage.AppendLine("خطا: داده وارد شده طولانی‌تر از حد مجاز است. لطفاً اطلاعات را کوتاه‌تر کنید.");
                            break;
                        default:
                            errorMessage.AppendLine($"خطای پایگاه داده (کد: {sqlException.Number})");
                            break;
                    }
                }
            }

            errorMessage.AppendLine($"جزئیات: {ex.Message}");

            Log.Error(ex, "خطای پایگاه داده شناسایی شد: {FriendlyErrorMessage}", errorMessage.ToString());
        }

        #endregion
    }

    /// <summary>
    /// کلاس کمکی برای دسترسی ایمن به HttpContext در تمام محیط‌ها
    /// این کلاس برای سیستم‌های پزشکی حیاتی است چون:
    /// 1. امکان دسترسی به HttpContext در سرویس‌های پس‌زمینه را فراهم می‌کند
    /// 2. از خطا در محیط‌های غیر-وب جلوگیری می‌کند
    /// 3. برای سیستم‌های پزشکی با امنیت بالا طراحی شده است
    /// </summary>
    public static class HttpContextStorage
    {
        private static readonly string HttpContextKey = "HttpContext.Current";

        public static System.Web.HttpContextBase Current
        {
            get
            {
                try
                {
                    // ابتدا از HttpContext فعلی استفاده می‌کنیم
                    if (System.Web.HttpContext.Current != null)
                        return new System.Web.HttpContextWrapper(System.Web.HttpContext.Current);

                    // سپس از OWIN Context استفاده می‌کنیم
                    var owinContext = GetOwinContext();
                    if (owinContext != null && owinContext.Environment != null)
                    {
                        var environment = owinContext.Environment;
                        if (environment.ContainsKey(HttpContextKey) && environment[HttpContextKey] is System.Web.HttpContextBase)
                            return (System.Web.HttpContextBase)environment[HttpContextKey];
                    }
                }
                catch
                {
                    // در صورت بروز خطا، به حالت ایمن می‌رویم
                }

                return null;
            }
        }

        private static IOwinContext GetOwinContext()
        {
            try
            {
                var context = System.Web.HttpContext.Current?.GetOwinContext();
                return context;
            }
            catch
            {
                return null;
            }
        }
    }
}