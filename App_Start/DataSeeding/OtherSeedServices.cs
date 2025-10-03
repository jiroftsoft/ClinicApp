using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.DataSeeding
{
    #region DepartmentSeedService

    /// <summary>
    /// سرویس Seeding دپارتمان‌های کلینیک (40 دپارتمان)
    /// </summary>
    public class DepartmentSeedService : BaseSeedService
    {
        public DepartmentSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("دپارتمان‌های کلینیک");

                // بررسی وجود کلینیک
                var clinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.Name == SeedConstants.DefaultClinic.Name && !c.IsDeleted);

                if (clinic == null)
                {
                    _logger.Warning("DATA_SEED: کلینیک پیش‌فرض یافت نشد. ابتدا ClinicSeedService را اجرا کنید");
                    return;
                }

                var adminUser = await GetAdminUserAsync();

                // ایجاد لیست دپارتمان‌ها (40 دپارتمان)
                var departments = SeedConstants.Departments.DefaultDepartments
                    .Select(d => new Department
                    {
                        Name = d.Name,
                        Description = d.Description,
                        ClinicId = clinic.ClinicId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = adminUser.Id
                    })
                    .ToList();

                // فیلتر تکراری‌ها (رفع N+1 Problem با Expression)
                var newDepartments = await FilterExistingItemsAsync<Department, string>(
                    departments,
                    d => d.Name,
                    _context.Departments.Where(d => !d.IsDeleted && d.ClinicId == clinic.ClinicId),
                    d => d.Name  // Expression<Func<>> برای IQueryable
                );

                // اضافه کردن
                AddRangeIfAny(newDepartments, "دپارتمان");

                LogSeedSuccess("دپارتمان‌های کلینیک", newDepartments.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("دپارتمان‌های کلینیک", ex);
                throw;
            }
        }

        public override async Task<bool> ValidateAsync()
        {
            var count = await _context.Departments
                .Where(d => !d.IsDeleted)
                .CountAsync();
            
            if (count == 0)
            {
                _logger.Warning("هیچ دپارتمانی یافت نشد");
                return false;
            }
            
            _logger.Debug($"✅ تعداد {count} دپارتمان موجود است");
            return true;
        }
    }

    #endregion

    #region ClinicSeedService

    /// <summary>
    /// سرویس Seeding کلینیک پیش‌فرض
    /// </summary>
    public class ClinicSeedService : BaseSeedService
    {
        public ClinicSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("کلینیک پیش‌فرض");

                var adminUser = await GetAdminUserAsync();

                // بررسی وجود کلینیک
                var clinicExists = await _context.Clinics
                    .AnyAsync(c => c.Name == SeedConstants.DefaultClinic.Name && !c.IsDeleted);

                if (clinicExists)
                {
                    _logger.Debug("کلینیک پیش‌فرض قبلاً وجود دارد");
                    return;
                }

                // ایجاد کلینیک
                var clinic = new Clinic
                {
                    Name = SeedConstants.DefaultClinic.Name,
                    Address = SeedConstants.DefaultClinic.Address,
                    PhoneNumber = SeedConstants.DefaultClinic.PhoneNumber,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                };

                _context.Clinics.Add(clinic);
                LogSeedSuccess("کلینیک پیش‌فرض", 1);
            }
            catch (Exception ex)
            {
                LogSeedError("کلینیک پیش‌فرض", ex);
                throw;
            }
        }

        public override async Task<bool> ValidateAsync()
        {
            var exists = await _context.Clinics
                .AnyAsync(c => c.Name == SeedConstants.DefaultClinic.Name && !c.IsDeleted);
            
            if (!exists)
            {
                _logger.Warning("کلینیک پیش‌فرض یافت نشد");
            }
            
            return exists;
        }
    }

    #endregion

    #region SpecializationSeedService

    /// <summary>
    /// سرویس Seeding تخصص‌ها
    /// </summary>
    public class SpecializationSeedService : BaseSeedService
    {
        public SpecializationSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("تخصص‌ها");

                var adminUser = await GetAdminUserAsync();

                // ایجاد لیست تخصص‌ها
                var specializations = SeedConstants.Specializations.DefaultSpecializations
                    .Select(s => new Specialization
                    {
                        Name = s.Name,
                        Description = s.Description,
                        DisplayOrder = s.DisplayOrder,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = adminUser.Id
                    })
                    .ToList();

                // فیلتر تکراری‌ها (رفع N+1 Problem با Expression)
                var newSpecializations = await FilterExistingItemsAsync<Specialization, string>(
                    specializations,
                    s => s.Name,
                    _context.Specializations.Where(s => !s.IsDeleted),
                    s => s.Name  // Expression<Func<>> برای IQueryable
                );

                // اضافه کردن
                AddRangeIfAny(newSpecializations, "تخصص");

                LogSeedSuccess("تخصص‌ها", newSpecializations.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("تخصص‌ها", ex);
                throw;
            }
        }

        public override async Task<bool> ValidateAsync()
        {
            var count = await _context.Specializations
                .Where(s => !s.IsDeleted)
                .CountAsync();
            
            if (count == 0)
            {
                _logger.Warning("هیچ تخصصی یافت نشد");
                return false;
            }
            
            _logger.Debug($"✅ تعداد {count} تخصص موجود است");
            return true;
        }
    }

    #endregion

    #region NotificationSeedService

    /// <summary>
    /// سرویس Seeding الگوهای اطلاع‌رسانی
    /// </summary>
    public class NotificationSeedService : BaseSeedService
    {
        public NotificationSeedService(ApplicationDbContext context, ILogger logger) 
            : base(context, logger)
        {
        }

        public override async Task SeedAsync()
        {
            try
            {
                LogSeedStart("الگوهای اطلاع‌رسانی");

                var adminUser = await GetAdminUserAsync();

                // تعریف الگوها
                var templates = new List<NotificationTemplate>
                {
                    CreateTemplate(
                        SeedConstants.NotificationKeys.Registration,
                        "ثبت‌نام در کلینیک",
                        "پیام تأیید ثبت‌نام بیمار جدید",
                        "سلام {0} عزیز،\n\nشما با موفقیت در کلینیک شفا ثبت‌نام کردید.\nلطفاً برای دریافت نوبت به سایت مراجعه کنید.\n\nکلینیک شفا",
                        adminUser.Id
                    ),
                    CreateTemplate(
                        SeedConstants.NotificationKeys.AppointmentConfirmation,
                        "تأیید نوبت",
                        "پیام تأیید نوبت برای بیمار",
                        "سلام {0} عزیز،\n\nنوبت شما در تاریخ {1} ساعت {2} تأیید شد.\nلطفاً 10 دقیقه قبل از موعد حضور یابید.\n\nکلینیک شفا",
                        adminUser.Id
                    ),
                    CreateTemplate(
                        SeedConstants.NotificationKeys.AppointmentReminder,
                        "یادآوری نوبت",
                        "پیام یادآوری نوبت 24 ساعت قبل از موعد",
                        "سلام {0} عزیز،\n\nفردا در تاریخ {1} ساعت {2} نوبت شما در کلینیک شفا است.\nلطفاً 10 دقیقه قبل از موعد حضور یابید.\n\nکلینیک شفا",
                        adminUser.Id
                    ),
                    CreateTemplate(
                        SeedConstants.NotificationKeys.BirthdayWish,
                        "تبریک تولد",
                        "پیام تبریک تولد برای بیمار",
                        "سلام {0} عزیز،\n\nتولدت مبارک! 🎉\nامیدواریم سالی پر از سلامتی و شادی داشته باشید.\n\nبا افتخار، کلینیک شفا",
                        adminUser.Id
                    ),
                    CreateTemplate(
                        SeedConstants.NotificationKeys.PaymentConfirmation,
                        "تأیید پرداخت",
                        "پیام تأیید پرداخت برای بیمار",
                        "سلام {0} عزیز،\n\nپرداخت شما به مبلغ {1} ریال در تاریخ {2} تأیید شد.\nرسید پرداخت در پیشنهاد بیمار موجود است.\n\nکلینیک شفا",
                        adminUser.Id
                    )
                };

                // فیلتر تکراری‌ها - Key خودش Primary Key است
                // دریافت Key های موجود
                var existingKeys = await _context.NotificationTemplates
                    .Select(t => t.Key)
                    .ToListAsync();

                // فیلتر کردن Template های جدید (فقط آنهایی که وجود ندارند)
                var newTemplates = templates
                    .Where(t => !existingKeys.Contains(t.Key))
                    .ToList();

                _logger.Debug("DATA_SEED: تعداد {ExistingCount} Template موجود، {NewCount} Template جدید", 
                    existingKeys.Count, newTemplates.Count);

                // اضافه کردن
                AddRangeIfAny(newTemplates, "الگوی اطلاع‌رسانی");

                LogSeedSuccess("الگوهای اطلاع‌رسانی", newTemplates.Count);
            }
            catch (Exception ex)
            {
                LogSeedError("الگوهای اطلاع‌رسانی", ex);
                throw;
            }
        }

        private NotificationTemplate CreateTemplate(
            string key, 
            string title, 
            string description, 
            string template, 
            string userId)
        {
            return new NotificationTemplate
            {
                Key = key,
                Title = title,
                Description = description,
                ChannelType = NotificationChannelType.Sms,
                PersianTemplate = template,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
        }

        public override async Task<bool> ValidateAsync()
        {
            var count = await _context.NotificationTemplates.CountAsync();
            
            if (count == 0)
            {
                _logger.Warning("DATA_SEED: هیچ الگوی اطلاع‌رسانی یافت نشد");
                return false;
            }

            // بررسی وجود الگوهای اصلی
            var requiredKeys = new[]
            {
                SeedConstants.NotificationKeys.Registration,
                SeedConstants.NotificationKeys.AppointmentConfirmation,
                SeedConstants.NotificationKeys.AppointmentReminder,
                SeedConstants.NotificationKeys.BirthdayWish,
                SeedConstants.NotificationKeys.PaymentConfirmation
            };

            var existingKeys = await _context.NotificationTemplates
                .Where(t => requiredKeys.Contains(t.Key))
                .Select(t => t.Key)
                .ToListAsync();

            var allExist = requiredKeys.All(key => existingKeys.Contains(key));
            
            if (!allExist)
            {
                var missing = requiredKeys.Except(existingKeys);
                _logger.Warning("DATA_SEED: الگوهای اطلاع‌رسانی ناقص هستند. مفقود: {MissingKeys}", 
                    string.Join(", ", missing));
                return false;
            }
            
            _logger.Debug("DATA_SEED: تعداد {Count} الگوی اطلاع‌رسانی موجود و معتبر است", count);
            return true;
        }
    }

    #endregion
}

