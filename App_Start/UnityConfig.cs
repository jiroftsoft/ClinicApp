using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Infrastructure;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.OTP;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Repositories;
using ClinicApp.Repositories.Payment;
using ClinicApp.Services;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
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
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Repositories.ClinicAdmin;
using ClinicApp.Services.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.ViewModels.SpecializationManagementVM;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.Payment;
using ClinicApp.Models.Core;
using ClinicApp.Repositories.Insurance;
using ClinicApp.Repositories.Payment;
using ClinicApp.Services.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.Services.DataSeeding;
using ClinicApp.Services.UserContext;
using ClinicApp.Services.SystemSettings;
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
                _log.Information("شروع ثبت CurrentUserService...");
                
                // بررسی محیط اجرایی و انتخاب پیاده‌سازی مناسب
                if (HttpContext.Current != null)
                {
                    _log.Information("HttpContext.Current موجود است - استفاده از CurrentUserService");
                    
                    // بررسی احراز هویت
                    bool isAuthenticated = HttpContext.Current.User?.Identity?.IsAuthenticated ?? false;
                    _log.Information("وضعیت احراز هویت: {IsAuthenticated}", isAuthenticated);
                    
                    if (isAuthenticated)
                    {
                        _log.Information("کاربر احراز هویت شده - استفاده از CurrentUserService");
                    }
                    else
                    {
                        _log.Information("کاربر احراز هویت نشده - استفاده از CurrentUserService با پشتیبانی از محیط توسعه");
                    }
                    
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
                    _log.Information("HttpContext.Current موجود نیست - استفاده از BackgroundCurrentUserService برای محیط غیر-وب");

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
                
                _log.Information("CurrentUserService با موفقیت ثبت شد");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ثبت CurrentUserService");
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
                container.RegisterType<IAuthService, AuthService>(new HierarchicalLifetimeManager());
                container.RegisterType<ApplicationUserManager>();

                // ثبت سرویس‌های Seed Data
                container.RegisterType<FactorSettingSeedService>(new PerRequestLifetimeManager());
                container.RegisterType<ServiceSeedService>(new PerRequestLifetimeManager());
                container.RegisterType<ServiceTemplateSeedService>(new PerRequestLifetimeManager());
                container.RegisterType<SystemSeedService>(new PerRequestLifetimeManager());

                // ثبت سرویس مدیریت کای‌ها
                container.RegisterType<IFactorSettingService, FactorSettingService>(new PerRequestLifetimeManager());
                
                // ثبت سرویس مدیریت قالب‌های خدمات
                container.RegisterType<ServiceTemplateService>(new PerRequestLifetimeManager());
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

                // Register Doctor Management Repositories
                container.RegisterType<IDoctorCrudRepository, DoctorCrudRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorDepartmentRepository, DoctorDepartmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorServiceCategoryRepository, DoctorServiceCategoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorScheduleRepository, DoctorScheduleRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorReportingRepository, DoctorReportingRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentRepository, DoctorAssignmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentHistoryRepository, DoctorAssignmentHistoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorDashboardRepository, DoctorDashboardRepository>(new PerRequestLifetimeManager());

                // Register Doctor Management Services
                container.RegisterType<IDoctorCrudService, DoctorCrudService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorDashboardService, DoctorDashboardService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorDepartmentService, DoctorDepartmentService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorServiceCategoryService, DoctorServiceCategoryService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorScheduleService, DoctorScheduleService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentService, DoctorAssignmentService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorReportingService, DoctorReportingService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentHistoryService, DoctorAssignmentHistoryService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های مدیریت نوبت‌دهی
                container.RegisterType<IAppointmentAvailabilityService, AppointmentAvailabilityService>(new PerRequestLifetimeManager());

                // Register Schedule Optimization Service
                container.RegisterType<IScheduleOptimizationService, ScheduleOptimizationService>(new PerRequestLifetimeManager());

                // Register Emergency Booking Service
                container.RegisterType<IEmergencyBookingService, EmergencyBookingService>(new PerRequestLifetimeManager());

                // Register Core Services for Search functionality
                container.RegisterType<IDoctorCrudService, DoctorCrudService>(new PerRequestLifetimeManager());
                container.RegisterType<IDepartmentManagementService, DepartmentManagementService>(new PerRequestLifetimeManager());

                // Register Specialization Management Repositories and Services
                container.RegisterType<ISpecializationRepository, SpecializationRepository>(new PerRequestLifetimeManager());
                container.RegisterType<ISpecializationService, SpecializationService>(new PerRequestLifetimeManager());

                // ثبت Validator برای FluentValidation
                container.RegisterType<IValidator<ClinicCreateEditViewModel>, ClinicCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DepartmentCreateEditViewModel>, DepartmentCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ServiceCategoryCreateEditViewModel>, ServiceCategoryCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ServiceCreateEditViewModel>, ServiceCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorCreateEditViewModel>, DoctorCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorDepartmentViewModel>, DoctorDepartmentViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorServiceCategoryViewModel>, DoctorServiceCategoryViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorScheduleViewModel>, DoctorScheduleViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorAssignmentsViewModel>, DoctorAssignmentsViewModelValidator>(new PerRequestLifetimeManager());
                
                // ثبت Validator برای عملیات انتساب پزشکان
                container.RegisterType<IValidator<DoctorAssignmentOperationViewModel>, DoctorAssignmentOperationViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorAssignmentRemovalViewModel>, DoctorAssignmentRemovalViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<DoctorAssignmentEditViewModel>, DoctorAssignmentEditViewModelValidator>(new PerRequestLifetimeManager());

                // Register Specialization Validators
                container.RegisterType<IValidator<SpecializationCreateEditViewModel>, SpecializationCreateEditViewModelValidator>(new PerRequestLifetimeManager());

                // Register Insurance Module Repositories
                container.RegisterType<IInsuranceProviderRepository, InsuranceProviderRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IInsurancePlanRepository, InsurancePlanRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientInsuranceRepository, PatientInsuranceRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IPlanServiceRepository, PlanServiceRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceCalculationRepository, InsuranceCalculationRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceTariffRepository, InsuranceTariffRepository>(new PerRequestLifetimeManager());

                // Register Insurance Module Services
                container.RegisterType<IInsuranceProviderService, InsuranceProviderService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsurancePlanService, InsurancePlanService>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientInsuranceService, PatientInsuranceService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceCalculationService, InsuranceCalculationService>(new PerRequestLifetimeManager());
                container.RegisterType<ICombinedInsuranceCalculationService, CombinedInsuranceCalculationService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceValidationService, InsuranceValidationService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsurancePlanDependencyService, InsurancePlanDependencyService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceTariffService, InsuranceTariffService>(new PerRequestLifetimeManager());
                container.RegisterType<ITariffDomainValidationService, TariffDomainValidationService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceService, SupplementaryInsuranceService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceCacheService, SupplementaryInsuranceCacheService>(new PerRequestLifetimeManager());
                
                // Register Business Rules Engine
                container.RegisterType<IBusinessRuleEngine, BusinessRuleEngine>(new PerRequestLifetimeManager());
                container.RegisterType<IBusinessRuleRepository, BusinessRuleRepository>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceMonitoringService, SupplementaryInsuranceMonitoringService>(new PerRequestLifetimeManager());

                // Register InsuranceTariff Validators
                container.RegisterType<IValidator<ViewModels.Insurance.InsuranceTariff.InsuranceTariffCreateEditViewModel>, 
                    InsuranceTariffCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Insurance.InsuranceTariff.InsuranceTariffFilterViewModel>, 
                    InsuranceTariffFilterViewModelValidator>(new PerRequestLifetimeManager());

                // Register Message Notification Service
                container.RegisterType<IMessageNotificationService, MessageNotificationService>(new PerRequestLifetimeManager());

                // Register User Context Service
                container.RegisterType<IUserContextService, UserContextService>(new PerRequestLifetimeManager());

                // Register System Settings Service
                container.RegisterType<ISystemSettingService, SystemSettingService>(new PerRequestLifetimeManager());

                // Register Shared Service Management Service
                container.RegisterType<ISharedServiceManagementService, SharedServiceManagementService>(new PerRequestLifetimeManager());

                // Register Service Calculation Service
                container.RegisterType<IServiceCalculationService, ServiceCalculationService>(new PerRequestLifetimeManager());

                // Register External Inquiry and Security Token Services
                container.RegisterType<IExternalInquiryService, ExternalInquiryService>(new PerRequestLifetimeManager());
                container.RegisterType<ISecurityTokenService, SecurityTokenService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های پذیرش
                container.RegisterType<IReceptionRepository, ReceptionRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionService, ReceptionService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های پرداخت
                container.RegisterType<IPaymentTransactionRepository, PaymentTransactionRepository>(new PerRequestLifetimeManager());

                // Register Supplementary Tariff Seeder Service
                container.RegisterType<SupplementaryTariffSeederService>(new PerRequestLifetimeManager());

                // Register Combined Insurance Calculation Test Service
                container.RegisterType<CombinedInsuranceCalculationTestService>(new PerRequestLifetimeManager());

                // Register Insurance Validators
                container.RegisterType<IValidator<InsurancePlanCreateEditViewModel>, InsurancePlanCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<InsuranceCalculationViewModel>, InsuranceCalculationViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<PatientInsuranceCreateEditViewModel>, PatientInsuranceCreateEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<PatientCreateEditViewModel>, PatientCreateEditViewModelValidator>(new PerRequestLifetimeManager());

                // Register Supplementary Insurance Validators
                container.RegisterType<IValidator<ViewModels.Insurance.Supplementary.SupplementaryTariffViewModel>, 
                    ClinicApp.Validators.Insurance.SupplementaryTariffViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Insurance.Supplementary.SupplementarySettings>, 
                    ClinicApp.Validators.Insurance.SupplementarySettingsValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Insurance.Supplementary.SupplementaryCalculationResult>, 
                    ClinicApp.Validators.Insurance.SupplementaryCalculationResultValidator>(new PerRequestLifetimeManager());


                // طبق DESIGN_PRINCIPLES_CONTRACT از AutoMapper استفاده نمی‌کنیم
                // از Factory Method Pattern استفاده می‌کنیم

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