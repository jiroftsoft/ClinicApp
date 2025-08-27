using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Serilog;

namespace ClinicApp;

public static class IdentitySeed
{
    public static void SeedDefaultData(ApplicationDbContext context)
    {
        Log.Information("شروع فرآیند سیدینگ داده‌های اولیه...");
        SeedRoles(context);
        SeedAdminUser(context);
        SeedDefaultInsurance(context);
        SeedDefaultClinic(context);
        SeedSpecializations(context);
        SeedNotificationTemplates(context);
        Log.Information("فرآیند سیدینگ با موفقیت پایان یافت.");
    }

    public static void SeedAdminUser(ApplicationDbContext context)
    {
        var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

        // Create the Admin user
        CreateUserIfNotExists(userManager, context, new ApplicationUser
        {
            UserName = "3020347998",
            // ✅ The NationalCode property is now explicitly set to match the UserName.
            NationalCode = "3020347998",
            Email = "admin@clinic.com",
            PhoneNumber = "09136381995",
            PhoneNumberConfirmed = true,
            FirstName = "Admin",
            LastName = "System"
        }, "Admin");

        // Create the System user
        CreateUserIfNotExists(userManager, context, new ApplicationUser
        {
            UserName = "3031945451",
            // ✅ The NationalCode property is now explicitly set.
            NationalCode = "3031945451",
            Email = "system@clinic.com",
            PhoneNumber = "09022487373",
            PhoneNumberConfirmed = true,
            FirstName = "System",
            LastName = "Shefa Clinic"
        }, "Admin"); // The "System" user is also given the "Admin" role for full permissions.
    }
    /// <summary>
    /// متد کمکی برای ایجاد کاربر در سیستم بدون پسورد
    /// </summary>
    private static void CreateUserIfNotExists(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ApplicationUser user, string role)
    {
        if (userManager.FindByName(user.UserName) == null)
        {
            // مرحله ۱: INSERT کاربر (بدون پسورد)
            user.IsActive = true;
            user.CreatedAt = DateTime.UtcNow;

            var result = userManager.Create(user); // <<-- تنها تغییر اینجاست: پارامتر پسورد حذف شد
            if (result.Succeeded)
            {
                Log.Information("کاربر '{UserName}' با موفقیت ایجاد شد.", user.UserName);

                // مرحله ۲: UPDATE کاربر و تنظیم CreatedByUserId به خودش
                var createdUser = userManager.FindByName(user.UserName);
                createdUser.CreatedByUserId = createdUser.Id;
                context.Entry(createdUser).State = EntityState.Modified;
                context.SaveChanges();

                if (!string.IsNullOrEmpty(role))
                {
                    userManager.AddToRole(createdUser.Id, role);
                }
            }
            else
            {
                Log.Error("خطا در ایجاد کاربر '{UserName}': {Errors}", user.UserName, string.Join(", ", result.Errors));
            }
        }
    }




    public static void SeedDefaultInsurance(ApplicationDbContext context)
    {
        var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
        if (adminUser == null) throw new Exception("کاربر ادمین یافت نشد.");

        if (!context.Insurances.Any(i => i.Name == "آزاد"))
        {
            var defaultInsurance = new Insurance
            {
                Name = "آزاد",
                Description = "بیمه پیش‌فرض",
                DefaultPatientShare = 100,
                DefaultInsurerShare = 0,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUser.Id
            };
            context.Insurances.Add(defaultInsurance);
            context.SaveChanges();
            Log.Information("بیمه پیش‌فرض 'آزاد' ایجاد شد");
        }
    }

    public static void SeedDefaultClinic(ApplicationDbContext context)
    {
        var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
        if (adminUser == null) throw new Exception("کاربر ادمین یافت نشد.");

        if (!context.Clinics.Any(c => c.Name == "کلینیک شفا"))
        {
            var defaultClinic = new Clinic
            {
                Name = "کلینیک شفا",
                Address = "جیرفت، خیابان آزادی، کوچه 12",
                PhoneNumber = "034-12345678",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUser.Id
            };
            context.Clinics.Add(defaultClinic);
            context.SaveChanges();
            Log.Information("کلینیک پیش‌فرض 'کلینیک شفا' ایجاد شد");
        }
    }

    public static void SeedRoles(ApplicationDbContext context)
    {
        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        var roles = new List<string> { AppRoles.Admin, AppRoles.Doctor, AppRoles.Receptionist, AppRoles.Patient };
        foreach (var roleName in roles)
        {
            if (!roleManager.RoleExists(roleName))
            {
                var role = new IdentityRole(roleName);
                var result = roleManager.Create(role);
                if (result.Succeeded) Log.Information("نقش '{RoleName}' ایجاد شد", roleName);
                else Log.Error("خطا در ایجاد نقش '{RoleName}': {Errors}", roleName, string.Join(", ", result.Errors));
            }
        }
    }
    /// <summary>
    /// ایجاد الگوهای پیش‌فرض اطلاع‌رسانی برای سیستم کلینیک شفا
    /// این متد به صورت ایمن از کاربر ادمین استفاده می‌کند و از ایجاد تکراری جلوگیری می‌کند
    /// </summary>
    public static void SeedNotificationTemplates(ApplicationDbContext context)
    {
        try
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
            if (adminUser == null)
            {
                Log.Error("کاربر ادمین برای ایجاد الگوهای اطلاع‌رسانی یافت نشد.");
                throw new Exception("کاربر ادمین برای ایجاد الگوهای اطلاع‌رسانی یافت نشد.");
            }

            // لیست الگوهای پیش‌فرض
            var templates = new List<NotificationTemplate>
            {
                new NotificationTemplate
                {
                    Key = NotificationTemplates.Registration,
                    Title = "ثبت‌نام در کلینیک",
                    Description = "پیام تأیید ثبت‌نام بیمار جدید",
                    ChannelType = NotificationChannelType.Sms,
                    PersianTemplate = "سلام {0} عزیز،\n\nشما با موفقیت در کلینیک شفا ثبت‌نام کردید.\nلطفاً برای دریافت نوبت به سایت مراجعه کنید.\n\nکلینیک شفا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new NotificationTemplate
                {
                    Key = NotificationTemplates.AppointmentConfirmation,
                    Title = "تأیید نوبت",
                    Description = "پیام تأیید نوبت برای بیمار",
                    ChannelType = NotificationChannelType.Sms,
                    PersianTemplate = "سلام {0} عزیز،\n\nنوبت شما در تاریخ {1} ساعت {2} تأیید شد.\nلطفاً 10 دقیقه قبل از موعد حضور یابید.\n\nکلینیک شفا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new NotificationTemplate
                {
                    Key = NotificationTemplates.AppointmentReminder,
                    Title = "یادآوری نوبت",
                    Description = "پیام یادآوری نوبت 24 ساعت قبل از موعد",
                    ChannelType = NotificationChannelType.Sms,
                    PersianTemplate = "سلام {0} عزیز،\n\nفردا در تاریخ {1} ساعت {2} نوبت شما در کلینیک شفا است.\nلطفاً 10 دقیقه قبل از موعد حضور یابید.\n\nکلینیک شفا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new NotificationTemplate
                {
                    Key = NotificationTemplates.BirthdayWish,
                    Title = "تبریک تولد",
                    Description = "پیام تبریک تولد برای بیمار",
                    ChannelType = NotificationChannelType.Sms,
                    PersianTemplate = "سلام {0} عزیز،\n\nتولدت مبارک! 🎉\nامیدواریم سالی پر از سلامتی و شادی داشته باشید.\n\nبا افتخار، کلینیک شفا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new NotificationTemplate
                {
                    Key = NotificationTemplates.PaymentConfirmation,
                    Title = "تأیید پرداخت",
                    Description = "پیام تأیید پرداخت برای بیمار",
                    ChannelType = NotificationChannelType.Sms,
                    PersianTemplate = "سلام {0} عزیز،\n\nپرداخت شما به مبلغ {1} ریال در تاریخ {2} تأیید شد.\nرسید پرداخت در پیشنهاد بیمار موجود است.\n\nکلینیک شفا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                }
            };

            // افزودن الگوهای جدید (در صورت عدم وجود)
            foreach (var template in templates)
            {
                if (!context.NotificationTemplates.Any(t => t.Key == template.Key))
                {
                    context.NotificationTemplates.Add(template);
                    Log.Information("الگوی اطلاع‌رسانی '{Key}' ایجاد شد", template.Key);
                }
            }

            context.SaveChanges();
            Log.Information("الگوهای اطلاع‌رسانی پیش‌فرض با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در ایجاد الگوهای اطلاع‌رسانی");
            throw;
        }
    }

    /// <summary>
    /// ایجاد تخصص‌های پیش‌فرض برای سیستم کلینیک شفا
    /// این متد به صورت ایمن از کاربر ادمین استفاده می‌کند و از ایجاد تکراری جلوگیری می‌کند
    /// </summary>
    public static void SeedSpecializations(ApplicationDbContext context)
    {
        try
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
            if (adminUser == null)
            {
                Log.Error("کاربر ادمین برای ایجاد تخصص‌ها یافت نشد.");
                throw new Exception("کاربر ادمین برای ایجاد تخصص‌ها یافت نشد.");
            }

            // لیست تخصص‌های پیش‌فرض
            var specializations = new List<Specialization>
            {
                new Specialization
                {
                    Name = "قلب و عروق",
                    Description = "متخصص قلب و عروق - تشخیص و درمان بیماری‌های قلبی و عروقی",
                    IsActive = true,
                    DisplayOrder = 1,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "داخلی",
                    Description = "متخصص داخلی - تشخیص و درمان بیماری‌های داخلی",
                    IsActive = true,
                    DisplayOrder = 2,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "جراحی عمومی",
                    Description = "جراح عمومی - انجام اعمال جراحی عمومی",
                    IsActive = true,
                    DisplayOrder = 3,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "اورتوپدی",
                    Description = "متخصص ارتوپدی - تشخیص و درمان بیماری‌های استخوان و مفاصل",
                    IsActive = true,
                    DisplayOrder = 4,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "نورولوژی",
                    Description = "متخصص مغز و اعصاب - تشخیص و درمان بیماری‌های عصبی",
                    IsActive = true,
                    DisplayOrder = 5,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "پزشکی خانواده",
                    Description = "پزشک خانواده - مراقبت‌های اولیه و جامع خانواده",
                    IsActive = true,
                    DisplayOrder = 6,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "پزشکی عمومی",
                    Description = "پزشک عمومی - مراقبت‌های اولیه و عمومی",
                    IsActive = true,
                    DisplayOrder = 7,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "دندانپزشکی",
                    Description = "دندانپزشک - تشخیص و درمان بیماری‌های دهان و دندان",
                    IsActive = true,
                    DisplayOrder = 8,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "داروسازی",
                    Description = "داروساز - مشاوره دارویی و تجویز دارو",
                    IsActive = true,
                    DisplayOrder = 9,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new Specialization
                {
                    Name = "پزشکی ورزشی",
                    Description = "متخصص پزشکی ورزشی - مراقبت از ورزشکاران و آسیب‌های ورزشی",
                    IsActive = true,
                    DisplayOrder = 10,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                }
            };

            // افزودن تخصص‌های جدید (در صورت عدم وجود)
            foreach (var specialization in specializations)
            {
                if (!context.Specializations.Any(s => s.Name == specialization.Name))
                {
                    context.Specializations.Add(specialization);
                    Log.Information("تخصص '{Name}' ایجاد شد", specialization.Name);
                }
            }

            context.SaveChanges();
            Log.Information("تخصص‌های پیش‌فرض با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در ایجاد تخصص‌های پیش‌فرض");
            throw;
        }
    }

}