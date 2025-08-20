using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Services; // Namespace containing your AsanakSmsService
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Serilog;

namespace ClinicApp
{
    #region Service Implementations (پیاده‌سازی سرویس‌ها)

    /// <summary>
    /// A production-ready email service using SMTP settings from Web.config.
    /// </summary>
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            try
            {
                var fromAddress = ConfigurationManager.AppSettings["Email:FromAddress"];
                var smtpClient = new SmtpClient(ConfigurationManager.AppSettings["Email:SmtpServer"],
                                                int.Parse(ConfigurationManager.AppSettings["Email:Port"]))
                {
                    Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["Email:Username"],
                        ConfigurationManager.AppSettings["Email:Password"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(fromAddress, message.Destination, message.Subject, message.Body)
                {
                    IsBodyHtml = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                Log.Information("Email sent successfully to {Destination}", message.Destination);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send email to {Destination}", message.Destination);
                // In production, you might want to throw or handle this more gracefully.
            }
        }
    }

    // AsanakSmsService from our previous steps is assumed to be in the ClinicApp.Services namespace.
    // If it's not, copy the complete AsanakSmsService class here.

    #endregion

    #region Identity Managers (مدیران هویت)

    /// <summary>
    /// Manages application users, including validation, password hashing, and service configuration.
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        #region Constructor
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
            this.SmsService = new AsanakSmsService();
        }
        #endregion

        #region Factory Method (OWIN Integration)
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            #region User Validation (اعتبارسنجی نام کاربری)
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };
            #endregion

            #region Password Validation (اعتبارسنجی رمز عبور)
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonLetterOrDigit = false // Special characters not required, but can be enabled
            };
            #endregion

            #region Account Lockout (قفل کردن حساب کاربری)
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10); // Increased for security
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;
            #endregion

            #region Two-Factor Authentication (احراز هویت دو مرحله‌ای)
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "کد امنیتی شما: {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "کد امنیتی",
                BodyFormat = "کد امنیتی شما: {0}"
            });
            #endregion

            #region Message Services (سرویس‌های ارسال پیام)
            //manager.EmailService = new EmailService();
            manager.SmsService = new AsanakSmsService(); // Our production-ready SMS service
            #endregion

            #region Token Provider (تولید توکن امن)
            // This setup ensures that security tokens (for password reset, email confirmation, etc.)
            // work correctly in a multi-server (web farm) production environment.
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            #endregion

            return manager;
        }
        #endregion
    }

    /// <summary>
    /// Manages the application's sign-in process.
    /// </summary>
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        #region Constructor
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }
        #endregion

        #region Overrides & Factory Method
        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
        #endregion
    }

    #endregion

    public static class IdentitySeed
    {
        public static void SeedDefaultData(ApplicationDbContext context)
        {
            SeedRoles(context);
            // ابتدا کاربر ادمین را ایجاد می‌کنیم
            SeedAdminUser(context);

            // سپس بیمه پیش‌فرض را ایجاد می‌کنیم
            SeedDefaultInsurance(context);

        }

        public static void SeedAdminUser(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.UserName == "3020347998"))
            {
                var admin = new ApplicationUser
                {
                    UserName = "3020347998",
                    Email = "admin@clinic.com",
                    FirstName = "مدیر",
                    LastName = "سیستم",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = userManager.Create(admin);

                if (result.Succeeded)
                {
                    userManager.AddToRole(admin.Id, AppRoles.Admin);
                }
                else
                {
                    throw new Exception($"خطا در ایجاد کاربر ادمین: {string.Join(", ", result.Errors)}");
                }
            }
        }

        public static void SeedDefaultInsurance(ApplicationDbContext context)
        {
            // دریافت کاربر ادمین
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == "3020347998");

            if (adminUser == null)
            {
                throw new Exception("کاربر ادمین برای ایجاد بیمه پیش‌فرض یافت نشد.");
            }

            if (!context.Insurances.Any(i => i.Name == "آزاد"))
            {
                var defaultInsurance = new Insurance
                {
                    Name = "آزاد",
                    Description = "بیمه پیش‌فرض برای بیمارانی که بیمه‌ای انتخاب نکرده‌اند",
                    DefaultPatientShare = 100,
                    DefaultInsurerShare = 0,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = adminUser.Id // استفاده از شناسه کاربر ادمین
                };

                context.Insurances.Add(defaultInsurance);
                context.SaveChanges();
            }
        }

        public static void SeedRoles(ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // Create Admin role if it doesn't exist
            if (!roleManager.RoleExists(AppRoles.Admin))
            {
                var role = new IdentityRole(AppRoles.Admin);
                roleManager.Create(role);
            }

            // Create Doctor role if it doesn't exist
            if (!roleManager.RoleExists(AppRoles.Doctor))
            {
                var role = new IdentityRole(AppRoles.Doctor);
                roleManager.Create(role);
            }

            // Create Receptionist role if it doesn't exist
            if (!roleManager.RoleExists(AppRoles.Receptionist))
            {
                var role = new IdentityRole(AppRoles.Receptionist);
                roleManager.Create(role);
            }

            // **ایجاد نقش بیمار (جدید)**
            if (!roleManager.RoleExists(AppRoles.Patient))
            {
                roleManager.Create(new IdentityRole(AppRoles.Patient));
            }
        }
    }
}