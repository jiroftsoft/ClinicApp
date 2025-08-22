using ClinicApp.Helpers;
using ClinicApp.Models.Entities;
using EntityFramework.DynamicFilters;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Serilog; // اضافه کردن این using برای دسترسی به Log
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
            // تنظیمات پیش‌فرض برای عملکرد بهینه در محیط عملیاتی پزشکی
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            // توجه بسیار مهم: این گزینه برای بهبود عملکرد عالی است،
            // اما شما مسئول هستید که به صورت دستی وضعیت موجودیت‌های تغییریافته را مشخص کنید.
            // مثال: context.Entry(entity).State = EntityState.Modified;
            Configuration.AutoDetectChangesEnabled = true;
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

            // اطمینان از اینکه کاربر جاری برای سیستم‌های پزشکی شناخته شده است
            context.EnsureCurrentUser();

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
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<CashSession> CashSessions { get; set; }
        public DbSet<PosTerminal> PosTerminals { get; set; }
        public DbSet<ReceiptPrint> ReceiptPrints { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<DatabaseVersion> DatabaseVersions { get; set; }

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
            // اطمینان از اینکه روابط به درستی پیکربندی شده‌اند

            // 6. فعال‌سازی Dynamic Filters برای سیستم‌های پیچیده پزشکی
            modelBuilder.Filter("ActiveClinics", (Clinic c) => c.IsActive, true);
            modelBuilder.Filter("ActiveDepartments", (Department d) => d.IsActive, true);
            modelBuilder.Filter("ActiveInsurances", (Insurance i) => i.IsActive, true);
            modelBuilder.Filter("ActiveDoctors", (Doctor d) => d.IsActive, true);

            // 7. افزودن پشتیبانی از نسخه‌بندی دیتابیس
            modelBuilder.Entity<DatabaseVersion>()
                .HasKey(dv => dv.VersionId);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            try
            {
                ApplyAuditAndSoftDelete();
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // ثبت جزئیات خطا برای سیستم‌های پزشکی
                var errorMessage = string.Join("; ",
                    ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"Property: {e.PropertyName}, Error: {e.ErrorMessage}"));

                // ✅ رفع خطا: استفاده از Log به جای AuditLogger
                Log.Error(ex, "خطا در ذخیره اطلاعات: {ErrorMessage}", errorMessage);
                throw new Exception($"خطا در ذخیره اطلاعات: {errorMessage}", ex);
            }
            catch (DbUpdateException ex)
            {
                // ✅ رفع خطا: استفاده از Log به جای AuditLogger
                Log.Error(ex, "خطای پایگاه داده رخ داده است");
                HandleDatabaseException(ex);
                throw;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ApplyAuditAndSoftDelete();
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbEntityValidationException ex)
            {
                // ثبت جزئیات خطا برای سیستم‌های پزشکی
                var errorMessage = string.Join("; ",
                    ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"Property: {e.PropertyName}, Error: {e.ErrorMessage}"));

                // ✅ رفع خطا: استفاده از Log به جای AuditLogger
                Log.Error(ex, "خطا در ذخیره اطلاعات: {ErrorMessage}", errorMessage);
                throw new Exception($"خطا در ذخیره اطلاعات: {errorMessage}", ex);
            }
            catch (DbUpdateException ex)
            {
                // ✅ رفع خطا: استفاده از Log به جای AuditLogger
                Log.Error(ex, "خطای پایگاه داده رخ داده است");
                HandleDatabaseException(ex);
                throw;
            }
        }

        #endregion

        #region Transaction Management (سیستم مدیریت تراکنش)

        /// <summary>
        /// ایجاد یک تراکنش جدید برای عملیات‌های اتمیک
        /// </summary>
        public virtual IDbTransaction BeginTransaction()
        {
            var transaction = Database.Connection.BeginTransaction();
            Database.UseTransaction(transaction);
            return transaction;
        }

        /// <summary>
        /// تأیید تراکنش و اعمال تغییرات
        /// </summary>
        public virtual void CommitTransaction(IDbTransaction transaction)
        {
            transaction.Commit();
            transaction.Dispose();
        }

        /// <summary>
        /// لغو تراکنش و بازگشت به حالت قبلی
        /// </summary>
        public virtual void RollbackTransaction(IDbTransaction transaction)
        {
            transaction.Rollback();
            transaction.Dispose();
        }

        #endregion

        #region Patient Data Security (امنیت اطلاعات بیماران)

        /// <summary>
        /// رمزنگاری اطلاعات حساس بیماران قبل از ذخیره در دیتابیس
        /// </summary>
        public void EncryptPatientSensitiveData(Patient patient)
        {
            if (patient == null) return;

            // رمزنگاری کد ملی
            if (!string.IsNullOrEmpty(patient.NationalCode))
            {
                patient.NationalCode = EncryptionService.Encrypt(patient.NationalCode);
            }

            // رمزنگاری شماره تلفن
            if (!string.IsNullOrEmpty(patient.PhoneNumber))
            {
                patient.PhoneNumber = EncryptionService.Encrypt(patient.PhoneNumber);
            }
        }

        /// <summary>
        /// رمزگشایی اطلاعات حساس بیماران پس از دریافت از دیتابیس
        /// </summary>
        public void DecryptPatientSensitiveData(Patient patient)
        {
            if (patient == null) return;

            // رمزگشایی کد ملی
            if (!string.IsNullOrEmpty(patient.NationalCode))
            {
                patient.NationalCode = EncryptionService.Decrypt(patient.NationalCode);
            }

            // رمزگشایی شماره تلفن
            if (!string.IsNullOrEmpty(patient.PhoneNumber))
            {
                patient.PhoneNumber = EncryptionService.Decrypt(patient.PhoneNumber);
            }
        }

        /// <summary>
        /// رمزگشایی اطلاعات حساس بیماران در یک لیست
        /// </summary>
        public void DecryptPatientSensitiveData(IEnumerable<Patient> patients)
        {
            foreach (var patient in patients)
            {
                DecryptPatientSensitiveData(patient);
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

        private void ApplyAuditAndSoftDelete()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted) &&
                            (e.Entity is ITrackable || e.Entity is ISoftDelete));

            var userId = GetCurrentUserId(); // ممکن است null باشد (در زمان Seed)
            var currentDateTime = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                // اعمال قانون حذف نرم (Soft Delete)
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDelete)
                {
                    entry.State = EntityState.Modified; // تغییر وضعیت به ویرایش برای جلوگیری از حذف فیزیکی
                    softDelete.IsDeleted = true;
                    softDelete.DeletedAt = currentDateTime;
                    softDelete.DeletedByUserId = userId;
                }

                // اعمال قوانین ردیابی (Audit Trail)
                if (entry.Entity is ITrackable trackable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        // --- منطق هوشمند برای حالت Add ---

                        // فقط اگر تاریخ ایجاد به صورت دستی تنظیم نشده بود، آن را پر کن
                        if (trackable.CreatedAt == default)
                        {
                            trackable.CreatedAt = currentDateTime;
                        }

                        // *** نکته کلیدی اینجاست ***
                        // فقط اگر کاربر ایجاد کننده به صورت دستی تنظیم نشده بود، آن را پر کن
                        if (string.IsNullOrEmpty(trackable.CreatedByUserId))
                        {
                            trackable.CreatedByUserId = userId;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        // در زمان ویرایش، فیلدهای ویرایش را تنظیم می‌کنیم
                        trackable.UpdatedAt = currentDateTime;
                        trackable.UpdatedByUserId = userId;

                        // --- منطق هوشمند و نهایی برای جلوگیری از تغییر فیلدهای ایجاد ---

                        // 1. تاریخ ایجاد (CreatedAt) پس از ثبت اولیه، هرگز نباید تغییر کند.
                        entry.Property("CreatedAt").IsModified = false;

                        // 2. کاربر ایجاد کننده (CreatedByUserId) نیز نباید تغییر کند،
                        //    مگر اینکه مقدار قبلی آن در دیتابیس null بوده باشد.
                        //    این شرط به فرآیند Seed اجازه می‌دهد مرحله دوم خود را (مقداردهی از null) انجام دهد.
                        var dbValues = entry.GetDatabaseValues();
                        if (dbValues != null && dbValues["CreatedByUserId"] != null)
                        {
                            entry.Property("CreatedByUserId").IsModified = false;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// نسخه حرفه‌ای برای استخراج شناسه‌ی کاربر جاری در تمام محیط‌ها
        /// </summary>
        /// <returns>شناسه‌ی کاربر لاگین کرده، یا شناسه‌ی کاربر 'سیستم' در محیط‌های غیر-وب</returns>
        private string GetCurrentUserId()
        {
            try
            {
                // Thread.CurrentPrincipal در هر دو محیط وب و غیر-وب کار می‌کند و منبع اصلی هویت است.
                // در محیط وب، ASP.NET به صورت خودکار این مقدار را تنظیم می‌کند.
                var identity = Thread.CurrentPrincipal?.Identity;

                if (identity != null && identity.IsAuthenticated)
                {
                    // اگر کاربر لاگین کرده بود، شناسه‌ی او را برمی‌گردانیم
                    return identity.GetUserId();
                }

                // اگر در محیط غیر-وب بودیم (مانند Seed) یا کاربری لاگین نکرده بود،
                // شناسه‌ی کاربر "سیستم" را که در ابتدای برنامه کش شده است، برمی‌گردانیم.
                // این کار از بروز خطای کلید خارجی جلوگیری می‌کند.
                return SystemUsers.SystemUserId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "خطا در دریافت شناسه کاربر جاری. از شناسه‌ی کاربر سیستم به عنوان جایگزین استفاده شد.");
                // در صورت بروز هرگونه خطای پیش‌بینی نشده، باز هم به حالت امن بازمی‌گردیم
                return SystemUsers.SystemUserId;
            }
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
                        case 8115: // Arithmetic overflow error
                            errorMessage.AppendLine("خطا: مقدار وارد شده بیش از حد مجاز است. لطفاً مقدار را کاهش دهید.");
                            break;
                        default:
                            errorMessage.AppendLine($"خطای پایگاه داده (کد: {sqlException.Number})");
                            break;
                    }
                }
            }

            errorMessage.AppendLine($"جزئیات: {ex.Message}");
            errorMessage.AppendLine($"کاربر جاری: {GetCurrentUserId()}");

            // ✅ رفع خطا: استفاده از Log به جای AuditLogger
            Log.Error(ex, "خطای پایگاه داده شناسایی شد: {FriendlyErrorMessage}", errorMessage.ToString());
        }

        /// <summary>
        /// اطمینان از اینکه کاربر جاری برای سیستم‌های پزشکی شناخته شده است
        /// این نسخه کاملاً سازگار با سیستم پسورد‌لس و OTP است
        /// </summary>
        private void EnsureCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "System")
                {
                    // اگر کاربر شناخته نشده بود، سعی می‌کنیم کاربر سیستم را تنظیم کنیم
                    var systemUser = Users.FirstOrDefault(u => u.UserName == "3031945451");

                    if (systemUser == null)
                    {
                        // ایجاد کاربر سیستم اگر وجود نداشت
                        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this));

                        systemUser = new ApplicationUser
                        {
                            UserName = "3031945451", // کد ملی معتبر برای کاربر سیستم
                            Email = "system@clinic.com",
                            FirstName = "سیستم",
                            LastName = "کلینیک شفا",
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        // ایجاد کاربر سیستم بدون نیاز به پسورد (سیستم پسورد‌لس)
                        var result = userManager.Create(systemUser);

                        if (result.Succeeded)
                        {
                            userManager.AddToRole(systemUser.Id, AppRoles.Admin);
                            Log.Information("کاربر سیستم با موفقیت ایجاد شد");
                        }
                        else
                        {
                            Log.Error("خطا در ایجاد کاربر سیستم: {Errors}", string.Join(", ", result.Errors));
                        }
                    }

                    // اطمینان از وجود کاربر ادمین
                    var adminUser = Users.FirstOrDefault(u => u.UserName == "3020347998");
                    if (adminUser == null)
                    {
                        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this));

                        adminUser = new ApplicationUser
                        {
                            UserName = "3020347998", // کد ملی ادمین
                            Email = "admin@clinic.com",
                            FirstName = "مدیر",
                            LastName = "سیستم",
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        // ایجاد کاربر ادمین بدون نیاز به پسورد (سیستم پسورد‌لس)
                        var result = userManager.Create(adminUser);

                        if (result.Succeeded)
                        {
                            userManager.AddToRole(adminUser.Id, AppRoles.Admin);
                            Log.Information("کاربر ادمین با موفقیت ایجاد شد");
                        }
                        else
                        {
                            Log.Error("خطا در ایجاد کاربر ادمین: {Errors}", string.Join(", ", result.Errors));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // در صورت بروز خطا، به حالت ایمن می‌رویم
                Log.Error(ex, "خطا در تضمین کاربر جاری");
            }
        }

        /// <summary>
        /// بررسی مجوز کاربر برای انجام عملیات خاص
        /// </summary>
        public bool HasPermission(string permission)
        {
            var userId = GetCurrentUserId();
            if (userId == "System") return true; // کاربر سیستم دسترسی کامل دارد

            var user = Users
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null) return false;

            // بررسی نقش‌های کاربر
            var userRoleNames = user.Roles
                .Select(r => Roles.FirstOrDefault(role => role.Id == r.RoleId)?.Name)
                .Where(name => name != null)
                .ToList();

            return userRoleNames.Any(roleName =>
                roleName == AppRoles.Admin ||
                roleName == permission);
        }

        /// <summary>
        /// بررسی مجوز کاربر برای انجام عملیات خاص روی موجودیت
        /// </summary>
        public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class
        {
            if (HasPermission(AppRoles.Admin)) return true;

            var userId = GetCurrentUserId();
            if (userId == "System") return true;

            // بررسی مالکیت موجودیت
            if (entity is Patient patient)
            {
                return patient.CreatedByUserId == userId ||
                       patient.ApplicationUserId == userId;
            }

            if (entity is Reception reception)
            {
                return reception.CreatedByUserId == userId ||
                       reception.DoctorId.ToString() == userId;
            }

            // برای سایر موجودیت‌ها
            var trackable = entity as ITrackable;
            if (trackable != null)
            {
                return trackable.CreatedByUserId == userId;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// کلاس کمکی برای مدیریت امنیت داده‌های بیماران
    /// </summary>
    /// <summary>
    /// نسخه نهایی و امن سرویس رمزنگاری با استفاده از IV تصادفی برای هر عملیات
    /// </summary>
    public static class EncryptionService
    {
        // کلید باید حداقل ۳۲ بایت باشد و به صورت امن نگهداری شود (مثلاً در Web.config با بخش encrypted).
        private static readonly string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"] ?? "DefaultSuperSecureKey!@#$12345678";
        private static readonly ILogger _log = Log.ForContext(typeof(EncryptionService));

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    // از کلید اصلی برای ساختن یک کلید ۳۲ بایتی (۲۵۶ بیت) امن استفاده می‌کنیم
                    var keyBytes = new Rfc2898DeriveBytes(EncryptionKey, new byte[8], 1000).GetBytes(32);
                    aes.Key = keyBytes;

                    // ۱. مهم‌ترین بخش: تولید یک IV جدید و کاملاً تصادفی برای این عملیات
                    aes.GenerateIV();
                    var iv = aes.IV;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                    using (var ms = new MemoryStream())
                    {
                        // ۲. ابتدا IV را در ابتدای استریم می‌نویسیم
                        ms.Write(iv, 0, iv.Length);

                        // ۳. سپس داده اصلی را رمز کرده و به ادامه استریم اضافه می‌کنیم
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs, Encoding.UTF8))
                        {
                            sw.Write(plainText);
                        }
                        // نتیجه نهایی: [۱۶ بایت IV] + [بایت‌های داده رمز شده]
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در زمان رمزنگاری داده.");
                // در صورت خطا، یک رشته خالی یا مقدار مشخصی را برگردانید تا از ذخیره داده خام جلوگیری شود.
                return string.Empty;
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                // داده را از Base64 به آرایه بایت برمی‌گردانیم
                var fullCipher = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    var keyBytes = new Rfc2898DeriveBytes(EncryptionKey, new byte[8], 1000).GetBytes(32);
                    aes.Key = keyBytes;

                    // ۱. ۱۶ بایت اول را به عنوان IV استخراج می‌کنیم
                    var iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    // ۲. بقیه بایت‌ها را به عنوان داده اصلی رمز شده در نظر می‌گیریم
                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs, Encoding.UTF8))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (FormatException ex)
            {
                _log.Warning(ex, " تلاش برای رمزگشایی یک رشته که فرمت Base64 ندارد. احتمالاً داده از قبل رمز نشده بوده است.");
                return cipherText; // اگر فرمت اشتباه بود، خود رشته را برمی‌گردانیم
            }
            catch (CryptographicException ex)
            {
                _log.Error(ex, "خطای رمزگشایی. احتمالاً کلید تغییر کرده یا داده با روش قدیمی رمز شده است.");
                // مقدار اصلی را برنگردانید تا از نشت اطلاعات جلوگیری شود
                return "خطا در رمزگشایی";
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در زمان رمزگشایی.");
                return "خطای غیرمنتظره";
            }
        }
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

    /// <summary>
    /// مدل برای مدیریت نسخه‌بندی دیتابیس
    /// این مدل برای سیستم‌های پزشکی حیاتی است چون:
    /// 1. امکان ردیابی تغییرات دیتابیس را فراهم می‌کند
    /// 2. از مشکلات سازگاری نسخه‌ها جلوگیری می‌کند
    /// 3. برای سیستم‌های پزشکی با امنیت بالا طراحی شده است
    /// </summary>
    public class DatabaseVersion
    {
        public int VersionId { get; set; }
        public string VersionNumber { get; set; }
        public string Description { get; set; }
        public DateTime AppliedDate { get; set; }
        public string AppliedByUserId { get; set; }
        public string MigrationScript { get; set; }
    }
}