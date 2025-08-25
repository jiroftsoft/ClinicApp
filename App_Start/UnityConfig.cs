using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Infrastructure;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.OTP;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Repositories;
using ClinicApp.Services;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Validators;
using FluentValidation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Serilog;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Security.Claims;
using System.Web;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace ClinicApp
{
    /// <summary>
    /// کلاس حرفه‌ای تنظیم Dependency Injection برای سیستم‌های پزشکی
    /// این کلاس با توجه به استانداردهای سیستم‌های پزشکی طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از محیط‌های وب و غیر-وب
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// استفاده:
    /// UnityConfig.RegisterTypes(Container);
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
    /// </summary>
    public static class UnityConfig
    {
        private static readonly ILogger _log = Log.ForContext(typeof(IranianNationalCodeValidator));
        private static Lazy<IUnityContainer> _container =
            new Lazy<IUnityContainer>(() =>
            {
                try
                {
                    var container = new UnityContainer();
                    RegisterTypes(container);
                    _log.Information("Unity Container با موفقیت راه‌اندازی شد");
                    return container;
                }
                catch (Exception ex)
                {
                    _log.Fatal(ex, "خطا در راه‌اندازی Unity Container");
                    throw;
                }
            });

        public static IUnityContainer Container => _container.Value;

        public static void RegisterTypes(IUnityContainer container)
        {
            try
            {
                // ثبت DbContext با مدیریت صحیح Lifetime
                container.RegisterType<DbContext, ApplicationDbContext>(new PerRequestLifetimeManager());
                container.RegisterType<ApplicationDbContext>(new PerRequestLifetimeManager());

                // ثبت Identity با پشتیبانی از محیط‌های مختلف
                RegisterIdentityServices(container);

                // ثبت سرویس کاربر فعلی با پشتیبانی کامل از تمام محیط‌ها
                RegisterCurrentUserService(container);

                // ثبت AutoMapper با پشتیبانی از مپینگ‌های پزشکی
                RegisterAutoMapper(container);

                // ثبت Logger با پشتیبانی از محیط‌های مختلف
                RegisterLogger(container);

                // ثبت تنظیمات سیستم با پشتیبانی از محیط‌های مختلف
                RegisterAppSettings(container);

                // ثبت سرویس‌های پزشکی
                RegisterMedicalServices(container);

                // ثبت سایر کامپوننت‌های حیاتی
                RegisterOtherComponents(container);
            }
            catch (Exception ex)
            {
                _log.Fatal(ex, "خطا در ثبت وابستگی‌ها در Unity Container");
                throw;
            }
        }

        private static void RegisterIdentityServices(IUnityContainer container)
        {
            try
            {
                // ثبت UserStore و ApplicationUserManager
                container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();

                container.RegisterType<ApplicationUserManager>(new HierarchicalLifetimeManager(),
                    new InjectionFactory(c =>
                    {
                        var context = c.Resolve<ApplicationDbContext>();
                        var store = new UserStore<ApplicationUser>(context);
                        var userManager = new ApplicationUserManager(store);

                        // پیکربندی UserManager برای سیستم پزشکی
                        ConfigureUserManager(userManager);

                        return userManager;
                    }));

                // ثبت AuthenticationManager با پشتیبانی از محیط‌های مختلف
                container.RegisterType<IAuthenticationManager>(new HierarchicalLifetimeManager(),
                    new InjectionFactory(c => GetAuthenticationManager()));
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت سرویس‌های Identity");
                throw;
            }
        }

        private static void ConfigureUserManager(ApplicationUserManager userManager)
        {
            // پیکربندی UserManager برای سیستم‌های پزشکی
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // پیکربندی پسورد برای سیستم‌های پزشکی
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = false
            };

            userManager.SmsService = new AsanakSmsService();
            var dataProtectionProvider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("ClinicApp");

            // *** کد اصلاح شده اینجاست ***
            // ابتدا یک IDataProtector با هدف مشخص می‌سازیم
            var dataProtector = dataProtectionProvider.Create("ASP.NET Identity");
            // سپس آن را به سازنده کلاس پاس می‌دهیم
            userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtector);
        }

        private static IAuthenticationManager GetAuthenticationManager()
        {
            try
            {
                // بررسی اینکه آیا در محیط وب هستیم
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.GetOwinContext().Authentication;
                }

                // برای محیط‌های غیر-وب، یک AuthenticationManager مجازی برمی‌گردانیم
                _log.Warning("در حال استفاده از AuthenticationManager مجازی برای محیط غیر-وب");
                return new FakeAuthenticationManager();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت AuthenticationManager");
                throw;
            }
        }

        private static void RegisterCurrentUserService(IUnityContainer container)
        {
            try
            {
                // بررسی محیط اجرایی و انتخاب پیاده‌سازی مناسب
                if (HttpContext.Current != null)
                {
                    // در محیط وب، از CurrentUserService استفاده می‌کنیم
                    container.RegisterType<ICurrentUserService, CurrentUserService>(
                        new PerRequestLifetimeManager(),
                        new InjectionConstructor(
                            new ResolvedParameter<HttpContextBase>(),
                            new ResolvedParameter<ApplicationUserManager>(),
                            new ResolvedParameter<Serilog.ILogger>(),
                            new ResolvedParameter<ApplicationDbContext>()
                        )
                    );
                }
                else
                {
                    // در محیط‌های غیر-وب، از BackgroundCurrentUserService استفاده می‌کنیم
                    _log.Information("در حال استفاده از BackgroundCurrentUserService برای محیط غیر-وب");

                    string systemUserId = SystemUsers.SystemUserId;
                    bool isSystemAdmin = true; // یا منطق خاص خودتان

                    container.RegisterType<ICurrentUserService, BackgroundCurrentUserService>(
                        new ContainerControlledLifetimeManager(),
                        new InjectionConstructor(
                            systemUserId,
                            isSystemAdmin,
                            new ResolvedParameter<ApplicationUserManager>()
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت CurrentUserService");
                throw;
            }
        }

        private static void RegisterAutoMapper(IUnityContainer container)
        {
            try
            {
                var mappingConfig = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new MappingProfile());
                });

                var mapper = mappingConfig.CreateMapper();
                container.RegisterInstance<IMapper>(mapper);
                _log.Information("AutoMapper با موفقیت راه‌اندازی شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در راه‌اندازی AutoMapper");
                throw;
            }
        }

        private static void RegisterLogger(IUnityContainer container)
        {
            try
            {
                // ثبت ILogger<T> برای تمام کلاس‌ها

                // ثبت ILogger عمومی برای موارد خاص
                container.RegisterInstance<Serilog.ILogger>(Log.Logger);

                _log.Information("Logger با موفقیت راه‌اندازی شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در راه‌اندازی Logger");
                throw;
            }
        }

        private static void RegisterAppSettings(IUnityContainer container)
        {
            try
            {
                // ثبت IAppSettings با استفاده از AppHelper
                container.RegisterType<IAppSettings>(new ContainerControlledLifetimeManager(),
                    new InjectionFactory(c => AppSettings.Instance));

                // مقداردهی اولیه AppHelper
                var appSettings = AppSettings.Instance;
                _log.Information("تنظیمات سیستم با موفقیت بارگذاری شدند");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگذاری تنظیمات سیستم");
                throw;
            }
        }

        private static void RegisterMedicalServices(IUnityContainer container)
        {
            try
            {
                // ثبت سرویس‌های پزشکی با پشتیبانی از سیستم حذف نرم
                container.RegisterType<IPatientService, PatientService>(new HierarchicalLifetimeManager());
                container.RegisterType<IDepartmentManagementService, DepartmentManagementService>(new HierarchicalLifetimeManager());
                container.RegisterType<IServiceCategoryService, ServiceCategoryService>(new HierarchicalLifetimeManager());
                container.RegisterType<IServiceService, ServiceService>(new HierarchicalLifetimeManager());
                container.RegisterType<IInsuranceService, InsuranceService>(new HierarchicalLifetimeManager());
                container.RegisterType<IAuthService, AuthService>(new HierarchicalLifetimeManager());
                container.RegisterType<ApplicationUserManager>();
                // ثبت سرویس‌های ارتباطی پزشکی
                container.RegisterType<IIdentityMessageService, AsanakSmsService>(new HierarchicalLifetimeManager());

                container.RegisterType<IOtpStateStore, HttpSessionOtpStateStore>(new PerRequestLifetimeManager());
                container.RegisterType<IClientInfoProvider, HttpContextClientInfoProvider>(new PerRequestLifetimeManager());
                container.RegisterType<IRateLimiter, MemoryCacheRateLimiter>(new ContainerControlledLifetimeManager()); // Singleton
                // This tells Unity: "When you need an IAppSettings, don't use a constructor.
                // Instead, call AppSettings.Instance to get the existing singleton."
                container.RegisterType<IAppSettings>(new InjectionFactory(c => AppSettings.Instance));

                container.RegisterType<IAuthSettings, AuthSettingsFromConfig>(new ContainerControlledLifetimeManager());
                //================================================================================================================
                container.RegisterType<IClinicManagementService, ClinicManagementService>(new PerRequestLifetimeManager());
                container.RegisterType<IClinicRepository, ClinicRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDepartmentRepository, DepartmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IServiceCategoryRepository, ServiceCategoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IServiceRepository, ServiceRepository>(new PerRequestLifetimeManager());

                // Register Service Management services
                container.RegisterType<IServiceManagementService, ServiceManagementService>(new PerRequestLifetimeManager());

                // ثبت Validator برای FluentValidation
                container.RegisterType<IValidator<ClinicCreateEditViewModel>, ClinicCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DepartmentCreateEditViewModel>, DepartmentCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ServiceCategoryCreateEditViewModel>, ServiceCategoryCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ServiceCreateEditViewModel>, ServiceCreateEditViewModelValidator>(new PerRequestLifetimeManager());

                _log.Information("سرویس‌های پزشکی با موفقیت ثبت شدند");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت سرویس‌های پزشکی");
                throw;
            }
        }

        private static void RegisterOtherComponents(IUnityContainer container)
        {
            try
            {
                // ثبت سایر کامپوننت‌های حیاتی
                //container.RegisterType<IPaymentService, PaymentService>(new HierarchicalLifetimeManager());
                //container.RegisterType<INotificationService, NotificationService>(new HierarchicalLifetimeManager());
                //container.RegisterType<IAppointmentService, AppointmentService>(new HierarchicalLifetimeManager());
                //container.RegisterType<ICashSessionService, CashSessionService>(new HierarchicalLifetimeManager());

                _log.Information("سایر کامپوننت‌ها با موفقیت ثبت شدند");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت سایر کامپوننت‌ها");
                throw;
            }
        }


    }
}