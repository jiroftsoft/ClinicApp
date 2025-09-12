using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClinicApp.Models;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;
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
        // SeedDefaultInsurance حذف شد - مدل قدیمی Insurance حذف شده است
        
        // سیستم بیمه جدید
        SeedInsuranceProviders(context);
        SeedInsurancePlans(context);
        SeedPlanServices(context);
        
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




    // SeedDefaultInsurance حذف شد - مدل قدیمی Insurance حذف شده است

    /// <summary>
    /// ایجاد ارائه‌دهندگان بیمه پیش‌فرض برای سیستم کلینیک شفا
    /// این متد به صورت ایمن از کاربر ادمین استفاده می‌کند و از ایجاد تکراری جلوگیری می‌کند
    /// </summary>
    public static void SeedInsuranceProviders(ApplicationDbContext context)
    {
        try
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
            if (adminUser == null)
            {
                Log.Error("کاربر ادمین برای ایجاد ارائه‌دهندگان بیمه یافت نشد.");
                throw new Exception("کاربر ادمین برای ایجاد ارائه‌دهندگان بیمه یافت نشد.");
            }

            // لیست ارائه‌دهندگان بیمه پیش‌فرض
            var insuranceProviders = new List<InsuranceProvider>
            {
                new InsuranceProvider
                {
                    Name = "تأمین اجتماعی",
                    Code = "SSO",
                    ContactInfo = "تلفن: 021-88888888، آدرس: تهران، خیابان ولیعصر",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new InsuranceProvider
                {
                    Name = "بیمه آزاد",
                    Code = "FREE",
                    ContactInfo = "پرداخت کامل توسط بیمار",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new InsuranceProvider
                {
                    Name = "بیمه نیروهای مسلح",
                    Code = "MILITARY",
                    ContactInfo = "تلفن: 021-77777777، آدرس: تهران، خیابان آزادی",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new InsuranceProvider
                {
                    Name = "بیمه سلامت",
                    Code = "HEALTH",
                    ContactInfo = "تلفن: 021-66666666، آدرس: تهران، خیابان کریمخان",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                new InsuranceProvider
                {
                    Name = "بیمه تکمیلی",
                    Code = "SUPPLEMENTARY",
                    ContactInfo = "بیمه تکمیلی برای پوشش اضافی",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                }
            };

            // افزودن ارائه‌دهندگان بیمه جدید (در صورت عدم وجود)
            foreach (var provider in insuranceProviders)
            {
                if (!context.InsuranceProviders.Any(ip => ip.Code == provider.Code))
                {
                    context.InsuranceProviders.Add(provider);
                    Log.Information("ارائه‌دهنده بیمه '{Name}' با کد '{Code}' ایجاد شد", provider.Name, provider.Code);
                }
            }

            context.SaveChanges();
            Log.Information("ارائه‌دهندگان بیمه پیش‌فرض با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در ایجاد ارائه‌دهندگان بیمه پیش‌فرض");
            throw;
        }
    }

    /// <summary>
    /// ایجاد طرح‌های بیمه پیش‌فرض برای سیستم کلینیک شفا
    /// این متد به صورت ایمن از کاربر ادمین استفاده می‌کند و از ایجاد تکراری جلوگیری می‌کند
    /// </summary>
    public static void SeedInsurancePlans(ApplicationDbContext context)
    {
        try
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
            if (adminUser == null)
            {
                Log.Error("کاربر ادمین برای ایجاد طرح‌های بیمه یافت نشد.");
                throw new Exception("کاربر ادمین برای ایجاد طرح‌های بیمه یافت نشد.");
            }

            // دریافت ارائه‌دهندگان بیمه
            var freeProvider = context.InsuranceProviders.FirstOrDefault(ip => ip.Code == "FREE");
            var ssoProvider = context.InsuranceProviders.FirstOrDefault(ip => ip.Code == "SSO");
            var militaryProvider = context.InsuranceProviders.FirstOrDefault(ip => ip.Code == "MILITARY");
            var healthProvider = context.InsuranceProviders.FirstOrDefault(ip => ip.Code == "HEALTH");
            var supplementaryProvider = context.InsuranceProviders.FirstOrDefault(ip => ip.Code == "SUPPLEMENTARY");

            if (freeProvider == null || ssoProvider == null || militaryProvider == null || healthProvider == null || supplementaryProvider == null)
            {
                Log.Error("ارائه‌دهندگان بیمه یافت نشدند. ابتدا SeedInsuranceProviders را اجرا کنید.");
                throw new Exception("ارائه‌دهندگان بیمه یافت نشدند. ابتدا SeedInsuranceProviders را اجرا کنید.");
            }

            // لیست طرح‌های بیمه پیش‌فرض
            var insurancePlans = new List<InsurancePlan>
            {
                // طرح بیمه آزاد
                new InsurancePlan
                {
                    InsuranceProviderId = freeProvider.InsuranceProviderId,
                    PlanCode = "FREE_BASIC",
                    Name = "بیمه آزاد پایه",
                    CoveragePercent = 0, // بیمار 100% پرداخت می‌کند
                    Deductible = 0,
                    ValidFrom = DateTime.UtcNow.AddYears(-10),
                    ValidTo = DateTime.UtcNow.AddYears(10),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                // طرح تأمین اجتماعی
                new InsurancePlan
                {
                    InsuranceProviderId = ssoProvider.InsuranceProviderId,
                    PlanCode = "SSO_STANDARD",
                    Name = "تأمین اجتماعی استاندارد",
                    CoveragePercent = 70, // بیمه 70% پرداخت می‌کند
                    Deductible = 100000, // فرانشیز 100 هزار تومان
                    ValidFrom = DateTime.UtcNow.AddYears(-5),
                    ValidTo = DateTime.UtcNow.AddYears(5),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                // طرح نیروهای مسلح
                new InsurancePlan
                {
                    InsuranceProviderId = militaryProvider.InsuranceProviderId,
                    PlanCode = "MILITARY_PREMIUM",
                    Name = "نیروهای مسلح پریمیوم",
                    CoveragePercent = 80, // بیمه 80% پرداخت می‌کند
                    Deductible = 50000, // فرانشیز 50 هزار تومان
                    ValidFrom = DateTime.UtcNow.AddYears(-5),
                    ValidTo = DateTime.UtcNow.AddYears(5),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                // طرح بیمه سلامت
                new InsurancePlan
                {
                    InsuranceProviderId = healthProvider.InsuranceProviderId,
                    PlanCode = "HEALTH_BASIC",
                    Name = "بیمه سلامت پایه",
                    CoveragePercent = 60, // بیمه 60% پرداخت می‌کند
                    Deductible = 150000, // فرانشیز 150 هزار تومان
                    ValidFrom = DateTime.UtcNow.AddYears(-3),
                    ValidTo = DateTime.UtcNow.AddYears(7),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                },
                // طرح بیمه تکمیلی
                new InsurancePlan
                {
                    InsuranceProviderId = supplementaryProvider.InsuranceProviderId,
                    PlanCode = "SUPPLEMENTARY_PLUS",
                    Name = "بیمه تکمیلی پلاس",
                    CoveragePercent = 90, // بیمه 90% پرداخت می‌کند
                    Deductible = 25000, // فرانشیز 25 هزار تومان
                    ValidFrom = DateTime.UtcNow.AddYears(-2),
                    ValidTo = DateTime.UtcNow.AddYears(8),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = adminUser.Id
                }
            };

            // افزودن طرح‌های بیمه جدید (در صورت عدم وجود)
            foreach (var plan in insurancePlans)
            {
                if (!context.InsurancePlans.Any(ip => ip.PlanCode == plan.PlanCode))
                {
                    context.InsurancePlans.Add(plan);
                    Log.Information("طرح بیمه '{Name}' با کد '{PlanCode}' ایجاد شد", plan.Name, plan.PlanCode);
                }
            }

            context.SaveChanges();
            Log.Information("طرح‌های بیمه پیش‌فرض با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در ایجاد طرح‌های بیمه پیش‌فرض");
            throw;
        }
    }

    /// <summary>
    /// ایجاد خدمات تحت پوشش طرح‌های بیمه پیش‌فرض برای سیستم کلینیک شفا
    /// این متد به صورت ایمن از کاربر ادمین استفاده می‌کند و از ایجاد تکراری جلوگیری می‌کند
    /// </summary>
    public static void SeedPlanServices(ApplicationDbContext context)
    {
        try
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");
            if (adminUser == null)
            {
                Log.Error("کاربر ادمین برای ایجاد خدمات تحت پوشش یافت نشد.");
                throw new Exception("کاربر ادمین برای ایجاد خدمات تحت پوشش یافت نشد.");
            }

            // دریافت طرح‌های بیمه
            var freePlan = context.InsurancePlans.FirstOrDefault(ip => ip.PlanCode == "FREE_BASIC");
            var ssoPlan = context.InsurancePlans.FirstOrDefault(ip => ip.PlanCode == "SSO_STANDARD");
            var militaryPlan = context.InsurancePlans.FirstOrDefault(ip => ip.PlanCode == "MILITARY_PREMIUM");
            var healthPlan = context.InsurancePlans.FirstOrDefault(ip => ip.PlanCode == "HEALTH_BASIC");
            var supplementaryPlan = context.InsurancePlans.FirstOrDefault(ip => ip.PlanCode == "SUPPLEMENTARY_PLUS");

            if (freePlan == null || ssoPlan == null || militaryPlan == null || healthPlan == null || supplementaryPlan == null)
            {
                Log.Error("طرح‌های بیمه یافت نشدند. ابتدا SeedInsurancePlans را اجرا کنید.");
                throw new Exception("طرح‌های بیمه یافت نشدند. ابتدا SeedInsurancePlans را اجرا کنید.");
            }

            // دریافت دسته‌بندی‌های خدمات موجود (از ماژول‌های تعریف شده)
            var serviceCategories = context.ServiceCategories.Where(sc => sc.IsActive && !sc.IsDeleted).ToList();
            
            if (!serviceCategories.Any())
            {
                Log.Warning("هیچ دسته‌بندی خدمتی یافت نشد. لطفاً ابتدا دسته‌بندی‌های خدمات را تعریف کنید.");
                return; // از اجرای ادامه متد جلوگیری می‌کنیم
            }

            // ایجاد خدمات تحت پوشش برای تمام دسته‌بندی‌های موجود
            var planServices = new List<PlanService>();
            
            // تعریف پوشش‌های مختلف برای هر طرح بیمه
            var planCoverages = new Dictionary<string, (int copay, int coverage)>
            {
                { "FREE_BASIC", (0, 0) },        // بیمه آزاد: بیمار 100% پرداخت می‌کند
                { "SSO_STANDARD", (30, 70) },    // تأمین اجتماعی: بیمه 70%، بیمار 30%
                { "MILITARY_PREMIUM", (20, 80) }, // نیروهای مسلح: بیمه 80%، بیمار 20%
                { "HEALTH_BASIC", (40, 60) },    // بیمه سلامت: بیمه 60%، بیمار 40%
                { "SUPPLEMENTARY_PLUS", (10, 90) } // بیمه تکمیلی: بیمه 90%، بیمار 10%
            };

            var plans = new[] { freePlan, ssoPlan, militaryPlan, healthPlan, supplementaryPlan };

            // برای هر طرح بیمه و هر دسته‌بندی خدمت، یک PlanService ایجاد می‌کنیم
            foreach (var plan in plans)
            {
                var coverage = planCoverages[plan.PlanCode];
                
                foreach (var category in serviceCategories)
                {
                    // تنظیم پوشش بر اساس نوع خدمت (اختیاری)
                    var adjustedCopay = coverage.copay;
                    var adjustedCoverage = coverage.coverage;
                    
                    // برای آزمایش‌ها و تصویربرداری، پوشش کمی کمتر
                    if (category.Title.Contains("آزمایش") || category.Title.Contains("تصویر"))
                    {
                        adjustedCopay += 10;
                        adjustedCoverage -= 10;
                    }

                    planServices.Add(new PlanService
                    {
                        InsurancePlanId = plan.InsurancePlanId,
                        ServiceCategoryId = category.ServiceCategoryId,
                        Copay = adjustedCopay,
                        CoverageOverride = adjustedCoverage,
                        IsCovered = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = adminUser.Id
                    });
                }
            }

            // افزودن خدمات تحت پوشش جدید (در صورت عدم وجود)
            foreach (var planService in planServices)
            {
                if (!context.PlanServices.Any(ps => ps.InsurancePlanId == planService.InsurancePlanId && 
                                                   ps.ServiceCategoryId == planService.ServiceCategoryId))
                {
                    context.PlanServices.Add(planService);
                    Log.Information("خدمت تحت پوشش برای طرح '{PlanId}' و دسته '{CategoryId}' ایجاد شد", 
                        planService.InsurancePlanId, planService.ServiceCategoryId);
                }
            }

            context.SaveChanges();
            Log.Information("خدمات تحت پوشش طرح‌های بیمه پیش‌فرض با موفقیت ایجاد شدند");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "خطا در ایجاد خدمات تحت پوشش طرح‌های بیمه پیش‌فرض");
            throw;
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