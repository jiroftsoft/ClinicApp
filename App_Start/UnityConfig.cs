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
using ClinicApp.Services.Reception;
using ClinicApp.Services.Idempotency;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Validators;
using ClinicApp.ViewModels.ClinicAdmin;
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
using ClinicApp.Interfaces.Payment.Reporting;
using ClinicApp.Models.Core;
using ClinicApp.Repositories.Insurance;
using ClinicApp.Repositories.Payment;
using ClinicApp.Repositories.Payment.POS;
using ClinicApp.Services.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Finance;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.Services.DataSeeding;
using ClinicApp.Services.UserContext;
using ClinicApp.Services.SystemSettings;
using ClinicApp.Services.Triage;
using ClinicApp.Services;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Repositories.Patient;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Repositories.Reception;
using ClinicApp.Services.Finance;
using ClinicApp.Services.Reception;
using ClinicApp.Services.Reception;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Services.Payment.POS;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Services.Appointment;
using ClinicApp.Repositories.Appointment;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Repositories.CMS;
using ClinicApp.Services.CMS;
using ClinicApp.Services.Payment;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Repositories.Notification;
using ClinicApp.Services.Notification;

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

                // ثبت TimeProvider برای مدیریت زمان
                container.RegisterType<ITimeProvider, DefaultTimeProvider>(new ContainerControlledLifetimeManager());

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

                // پروداکشن درمانی: PerRequest تا هر درخواست UserManager با DbContext همان درخواست استفاده شود
                container.RegisterType<ApplicationUserManager>(new PerRequestLifetimeManager(),
                    new InjectionFactory(c =>
                    {
                        var context = c.Resolve<ApplicationDbContext>();
                        var store = new UserStore<ApplicationUser>(context);
                        var userManager = new ApplicationUserManager(store);

                        // پیکربندی UserManager برای سیستم پزشکی
                        ConfigureUserManager(userManager);

                        return userManager;
                    }));

                // ✅ CRITICAL FIX: Use PerRequestLifetimeManager to get fresh AuthenticationManager per request
                // HierarchicalLifetimeManager was causing stale/cached AuthenticationManager from Application_Start
                container.RegisterType<IAuthenticationManager>(new PerRequestLifetimeManager(),
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
                return null;
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
                container.RegisterType<Interfaces.Repositories.IPatientRepository, PatientRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientDashboardService, PatientDashboardService>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientSettingsService, PatientSettingsService>(new PerRequestLifetimeManager());

                // ✅ EMR Module - Medical Record Service & Repository
                container.RegisterType<Interfaces.Repositories.IMedicalRecordRepository, Repositories.Patient.MedicalRecordRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientMedicalRecordService, Services.Patient.MedicalRecordService>(new PerRequestLifetimeManager());

                // ✅ Document Upload Service (for EMR attachments)
                container.RegisterType<IDocumentUploadService, DocumentUploadService>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionWorkflowService, ReceptionWorkflowService>(new PerRequestLifetimeManager());
                container.RegisterType<IDepartmentManagementService, DepartmentManagementService>(new HierarchicalLifetimeManager());
                container.RegisterType<IServiceCategoryService, ServiceCategoryService>(new HierarchicalLifetimeManager());
                container.RegisterType<IServiceService, ServiceService>(new HierarchicalLifetimeManager());
                // پروداکشن: PerRequest تا هر درخواست AuthService با UserManager/DbContext همان درخواست استفاده شود (جلوگیری از DbContext disposed)
                container.RegisterType<IAuthService, AuthService>(new PerRequestLifetimeManager());

                // ✅ ثبت User Management Repository و Service
                container.RegisterType<Interfaces.UserManagement.IUserRepository, Repositories.UserManagement.UserRepository>(new PerRequestLifetimeManager());
                container.RegisterType<Interfaces.UserManagement.IUserManagementService, Services.UserManagement.UserManagementService>(new PerRequestLifetimeManager());

                // ✅ ثبت User Profile Service (برای ویرایش پروفایل خود کاربر)
                container.RegisterType<IUserProfileService, UserProfileService>(new PerRequestLifetimeManager());

                // پروداکشن درمانی: PerRequest تا هر درخواست RoleManager با DbContext همان درخواست استفاده شود
                container.RegisterType<RoleManager<IdentityRole>>(new PerRequestLifetimeManager(),
                    new InjectionFactory(c =>
                    {
                        var context = c.Resolve<ApplicationDbContext>();
                        var roleStore = new RoleStore<IdentityRole>(context);
                        return new RoleManager<IdentityRole>(roleStore);
                    }));

                // Register HomePage Service
                // Note: IAboutPageService و IStoryService optional هستند
                // Unity به صورت خودکار optional parameters را resolve می‌کند اگر در container ثبت شده باشند
                container.RegisterType<IHomePageService, HomePageService>(new PerRequestLifetimeManager());

                // Image Upload Service
                container.RegisterType<IImageUploadService, ImageUploadService>(new PerRequestLifetimeManager());
                container.RegisterType<IDocumentUploadService, DocumentUploadService>(new PerRequestLifetimeManager());
                // ApplicationUserManager قبلاً با PerRequest+Factory در بخش Identity ثبت شده؛ ثبت مجدد حذف شد تا از تداخل جلوگیری شود

                // ثبت سرویس‌های تریاژ
                container.RegisterType<ITriageService, TriageService>(new PerRequestLifetimeManager());
                container.RegisterType<ITriageQueueService, TriageQueueService>(new PerRequestLifetimeManager());

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

                // ✅ CRITICAL FIX: Session-only OTP Store (Simple & Reliable)
                // Database persistence is handled by AuthService using its own context
                container.RegisterType<IOtpStateStore, ClinicApp.Services.HttpSessionOtpStateStore>(new PerRequestLifetimeManager());
                container.RegisterType<IClientInfoProvider, HttpContextClientInfoProvider>(new PerRequestLifetimeManager());
                container.RegisterType<IRateLimiter, MemoryCacheRateLimiter>(new ContainerControlledLifetimeManager()); // Singleton
                // This tells Unity: "When you need an IAppSettings, don't use a constructor.
                // Instead, call AppSettings.Instance to get the existing singleton."
                container.RegisterType<IAppSettings>(new InjectionFactory(c => AppSettings.Instance));

                container.RegisterType<IAuthSettings, AuthSettingsFromConfig>(new ContainerControlledLifetimeManager());
                //================================================================================================================
                container.RegisterType<IClinicManagementService, ClinicManagementService>(new PerRequestLifetimeManager());
                container.RegisterType<IClinicRepository, ClinicRepository>(new PerRequestLifetimeManager());

                // Clinic Bank Account Management
                container.RegisterType<IClinicBankAccountService, ClinicBankAccountService>(new PerRequestLifetimeManager());
                container.RegisterType<IClinicBankAccountRepository, ClinicBankAccountRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDepartmentRepository, DepartmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IServiceCategoryRepository, ServiceCategoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IServiceRepository, ServiceRepository>(new PerRequestLifetimeManager());

                // Register Service Management services
                container.RegisterType<IServiceManagementService, ServiceManagementService>(new PerRequestLifetimeManager());

                // ========== ثبت Repository های CMS ==========
                container.RegisterType<IBlogPostRepository, BlogPostRepository>(new PerRequestLifetimeManager());
                container.RegisterType<ISliderRepository, SliderRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IAnnouncementRepository, AnnouncementRepository>(new PerRequestLifetimeManager());
                container.RegisterType<ITestimonialRepository, TestimonialRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IGalleryItemRepository, GalleryItemRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IVideoRepository, VideoRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IFAQRepository, FAQRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IAboutPageRepository, AboutPageRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IHealthTipRepository, HealthTipRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceInfoRepository, InsuranceInfoRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IMedicalServiceInfoRepository, MedicalServiceInfoRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IEmergencyContactRepository, EmergencyContactRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IClinicWorkingHoursRepository, ClinicWorkingHoursRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IMedicalEquipmentRepository, MedicalEquipmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IContactFormRepository, ContactFormRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientEducationMaterialRepository, PatientEducationMaterialRepository>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterSubscriptionRepository, NewsletterSubscriptionRepository>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterTemplateRepository, NewsletterTemplateRepository>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterCampaignRepository, NewsletterCampaignRepository>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterCampaignRecipientRepository, NewsletterCampaignRecipientRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IStoryRepository, StoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IFooterSettingsRepository, FooterSettingsRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IFooterLinkRepository, FooterLinkRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IFooterSocialRepository, FooterSocialRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IFooterCertificationRepository, FooterCertificationRepository>(new PerRequestLifetimeManager());

                // ========== ثبت Service های CMS ==========
                container.RegisterType<IBlogPostService, BlogPostService>(new PerRequestLifetimeManager());

                // BlogPost Comment & Like Services
                container.RegisterType<IBlogPostCommentRepository, BlogPostCommentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IBlogPostCommentService, BlogPostCommentService>(new PerRequestLifetimeManager());
                container.RegisterType<IBlogPostLikeRepository, BlogPostLikeRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IBlogPostLikeService, BlogPostLikeService>(new PerRequestLifetimeManager());
                container.RegisterType<ISliderService, SliderService>(new PerRequestLifetimeManager());
                container.RegisterType<IAnnouncementService, AnnouncementService>(new PerRequestLifetimeManager());
                container.RegisterType<ITestimonialService, TestimonialService>(new PerRequestLifetimeManager());
                container.RegisterType<IGalleryService, GalleryService>(new PerRequestLifetimeManager());
                container.RegisterType<IVideoService, VideoService>(new PerRequestLifetimeManager());
                container.RegisterType<IVideoUploadService, VideoUploadService>(new PerRequestLifetimeManager());
                // IDocumentUploadService already registered in Image Upload Service section (line 346)
                container.RegisterType<IFAQService, FAQService>(new PerRequestLifetimeManager());
                container.RegisterType<IAboutPageService, AboutPageService>(new PerRequestLifetimeManager());
                container.RegisterType<IHealthTipService, HealthTipService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceInfoService, InsuranceInfoService>(new PerRequestLifetimeManager());
                container.RegisterType<IMedicalServiceInfoService, MedicalServiceInfoService>(new PerRequestLifetimeManager());
                container.RegisterType<IEmergencyContactService, EmergencyContactService>(new PerRequestLifetimeManager());
                container.RegisterType<IClinicWorkingHoursService, ClinicWorkingHoursService>(new PerRequestLifetimeManager());
                container.RegisterType<IMedicalEquipmentService, MedicalEquipmentService>(new PerRequestLifetimeManager());
                container.RegisterType<IContactFormService, ContactFormService>(new PerRequestLifetimeManager());
                container.RegisterType<IPatientEducationMaterialService, PatientEducationMaterialService>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterSubscriptionService, NewsletterSubscriptionService>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterTemplateService, NewsletterTemplateService>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterCampaignService, NewsletterCampaignService>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterEmailService, NewsletterEmailService>(new PerRequestLifetimeManager());
                container.RegisterType<INewsletterSmsService, NewsletterSmsService>(new PerRequestLifetimeManager());
                container.RegisterType<IStoryService, StoryService>(new PerRequestLifetimeManager());
                container.RegisterType<IFooterService, FooterService>(new PerRequestLifetimeManager());

                // Register Doctor Management Repositories
                container.RegisterType<IDoctorCrudRepository, DoctorCrudRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorDepartmentRepository, DoctorDepartmentRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorServiceCategoryRepository, DoctorServiceCategoryRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorScheduleRepository, DoctorScheduleRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorTimeSlotRepository, DoctorTimeSlotRepository>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<ApplicationDbContext>(),
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );
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
                container.RegisterType<IDoctorTimeSlotService, DoctorTimeSlotService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentService, DoctorAssignmentService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorReportingService, DoctorReportingService>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorAssignmentHistoryService, DoctorAssignmentHistoryService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های مدیریت نوبت‌دهی
                container.RegisterType<IAppointmentAvailabilityService, AppointmentAvailabilityService>(new PerRequestLifetimeManager());

                // Register Appointment Repository
                container.RegisterType<IAppointmentRepository, AppointmentRepository>(new PerRequestLifetimeManager());

                // Register Appointment Booking Service
                container.RegisterType<IAppointmentBookingService, AppointmentBookingService>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.Appointment.IDoctorMappingService, ClinicApp.Services.Appointment.DoctorMappingService>(new PerRequestLifetimeManager());
                // گزارش نوبت‌های رزرو شده توسط بیماران (منشی)
                container.RegisterType<IPatientBookedAppointmentsReportService, PatientBookedAppointmentsReportService>(new PerRequestLifetimeManager());

                // مشاوره آنلاین تصویری (Jitsi)
                container.RegisterType<IOnlineConsultationRoomRepository, OnlineConsultationRoomRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IOnlineConsultationService, OnlineConsultationService>(new PerRequestLifetimeManager());

                // ✅ ثبت Promotional Event Repository و Service (برای تخفیف‌های تبلیغاتی)
                container.RegisterType<Interfaces.PromotionalEvent.IPromotionalEventRepository, Repositories.PromotionalEvent.PromotionalEventRepository>(new PerRequestLifetimeManager());
                container.RegisterType<Interfaces.PromotionalEvent.IPromotionalEventService, Services.PromotionalEvent.PromotionalEventService>(new PerRequestLifetimeManager());
                container.RegisterType<Interfaces.PromotionalEvent.IPromotionalEventSmsService, Services.PromotionalEvent.PromotionalEventSmsService>(new PerRequestLifetimeManager());

                // Register Schedule Optimization Service and Strategies
                container.RegisterType<IScheduleOptimizationService, ScheduleOptimizationService>(new PerRequestLifetimeManager());

                // Register Schedule Optimization Strategies
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.IWorkloadAnalyzer,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.WorkloadAnalyzer>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.IBreakTimeOptimizer,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.BreakTimeOptimizer>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.IPriorityManager,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.PriorityManager>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.IPatientDistributor,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.PatientDistributor>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.IEmergencySlotManager,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.EmergencySlotManager>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Interfaces.ClinicAdmin.ScheduleOptimization.ICostAnalyzer,
                    ClinicApp.Services.ClinicAdmin.ScheduleOptimization.Strategies.CostAnalyzer>(new PerRequestLifetimeManager());

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
                container.RegisterType<IValidator<ClinicBankAccountCreateEditViewModel>, ClinicBankAccountCreateEditViewModelValidator>(new PerRequestLifetimeManager());
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
                container.RegisterType<IInsuranceStatusCheckerService, InsuranceStatusCheckerService>(new PerRequestLifetimeManager()); // ✅ کامپوننت قابل استفاده مجدد
                container.RegisterType<IInsuranceCalculationService, InsuranceCalculationService>(new PerRequestLifetimeManager());
                container.RegisterType<ICombinedInsuranceCalculationService, CombinedInsuranceCalculationService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceValidationService, InsuranceValidationService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsurancePlanDependencyService, InsurancePlanDependencyService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceTariffService, InsuranceTariffService>(new PerRequestLifetimeManager());
                container.RegisterType<IInsuranceTariffCalculationService, InsuranceTariffCalculationService>(new PerRequestLifetimeManager());
                container.RegisterType<ITariffDomainValidationService, TariffDomainValidationService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceService, SupplementaryInsuranceService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceCacheService, SupplementaryInsuranceCacheService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryCombinationService, SupplementaryCombinationService>(new PerRequestLifetimeManager());
                container.RegisterType<ISupplementaryInsuranceCalculationService, CorrectSupplementaryInsuranceCalculationService>(new PerRequestLifetimeManager());

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

                // ✅ صف اعلان نوبت (Appointment Notification Queue) — برای AppointmentBookingService و Dashboard زنجیره وابستگی
                container.RegisterType<INotificationQueueRepository, NotificationQueueRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IAppointmentNotificationQueueService, NotificationService>(new PerRequestLifetimeManager());
                // ✅ Hangfire Jobs: پردازش صف اعلان و زمان‌بندی یادآوری نوبت
                container.RegisterType<NotificationQueueProcessor>(new PerRequestLifetimeManager());
                container.RegisterType<AppointmentReminderScheduler>(new PerRequestLifetimeManager());

                // Register User Context Service
                container.RegisterType<IUserContextService, UserContextService>(new PerRequestLifetimeManager());

                // Register Reception Services
                container.RegisterType<IReceptionPatientService, ReceptionPatientService>(new PerRequestLifetimeManager());

                // Register Insurance Plan Suggestion Service (برای پیشنهاد پلن‌های پیش‌فرض)
                container.RegisterType<InsurancePlanSuggestionService, InsurancePlanSuggestionService>(new PerRequestLifetimeManager());

                // Register System Settings Service
                container.RegisterType<ISystemSettingService, SystemSettingService>(new PerRequestLifetimeManager());

                // Register Patient Insurance Management Service
                container.RegisterType<IPatientInsuranceManagementService, PatientInsuranceManagementService>(new PerRequestLifetimeManager());

                // Register Patient Insurance Validation Service
                container.RegisterType<IPatientInsuranceValidationService, PatientInsuranceValidationService>(new PerRequestLifetimeManager());

                // Register Shared Service Management Service
                container.RegisterType<ISharedServiceManagementService, SharedServiceManagementService>(new PerRequestLifetimeManager());

                // Register Service Calculation Service
                container.RegisterType<IServiceCalculationService, ServiceCalculationService>(new PerRequestLifetimeManager());

                // Register Service Calculation Engine
                container.RegisterType<ServiceCalculationEngine, ServiceCalculationEngine>(new PerRequestLifetimeManager());

                // Register Pricing Module
                container.RegisterType<ClinicApp.Services.Pricing.Interfaces.ITariffResolver, ClinicApp.Services.Pricing.Resolvers.TariffResolver>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Services.Pricing.Interfaces.IInsuranceCoverageProvider, ClinicApp.Services.Pricing.Coverage.InsuranceCoverageProvider>(new PerRequestLifetimeManager());
                container.RegisterType<ClinicApp.Services.Pricing.Interfaces.IPricingEngine, ClinicApp.Services.Pricing.Engines.PricingEngine>(new PerRequestLifetimeManager());

                // ✅ Register Reception Pricing Service
                container.RegisterType<ClinicApp.Interfaces.Reception.IReceptionPricingService, ClinicApp.Services.Reception.ReceptionPricingService>(new PerRequestLifetimeManager());

                // Register External Inquiry and Security Token Services
                container.RegisterType<IExternalInquiryService, ExternalInquiryService>(new PerRequestLifetimeManager());
                container.RegisterType<ISecurityTokenService, SecurityTokenService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های پذیرش
                container.RegisterType<IReceptionRepository, ReceptionRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionService, ReceptionService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های تخصصی محاسبات پذیرش
                container.RegisterType<IReceptionCalculationService, ReceptionCalculationService>(new PerRequestLifetimeManager());

                // ثبت سرویس مدیریت فرم پذیرش
                container.RegisterType<IReceptionFormService, ReceptionFormService>(new PerRequestLifetimeManager());

                // ثبت سرویس مدیریت سایدبار پذیرش
                container.RegisterType<IReceptionSidebarService, ReceptionSidebarService>(new PerRequestLifetimeManager());

                // ثبت سرویس دامنه پذیرش
                container.RegisterType<IReceptionDomainService, ReceptionDomainService>(new PerRequestLifetimeManager());

                // ثبت سرویس ناوبری پذیرش
                container.RegisterType<IReceptionNavigationService, ReceptionNavigationService>(new PerRequestLifetimeManager());
                container.RegisterType<ReceptionInsuranceAutoService, ReceptionInsuranceAutoService>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionDepartmentDoctorService, Services.Reception.ReceptionDepartmentDoctorService>(new PerRequestLifetimeManager());

                // ثبت ReceptionFacade - Orchestrator نازک
                container.RegisterType<IReceptionFacade, ReceptionFacade>(new PerRequestLifetimeManager());

                // ثبت FinancialYearService
                container.RegisterType<IFinancialYearService, DbFinancialYearService>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionServiceManagementService, Services.Reception.ReceptionServiceManagementService>(new PerRequestLifetimeManager());
                container.RegisterType<IReceptionPaymentService, Services.Reception.ReceptionPaymentService>(new PerRequestLifetimeManager());

                // ثبت Repository های تخصصی پذیرش
                container.RegisterType<IClinicManagementRepository, Repositories.Reception.ClinicManagementRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IDoctorManagementRepository, Repositories.Reception.DoctorManagementRepository>(new PerRequestLifetimeManager());
                container.RegisterType<IShiftManagementRepository, Repositories.Reception.ShiftManagementRepository>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های شیفت کاری
                container.RegisterType<ShiftHelperService>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های پرداخت
                container.RegisterType<IPaymentTransactionRepository, PaymentTransactionRepository>(new PerRequestLifetimeManager());

                // ثبت ریپازیتوری‌های Payment (برای PaymentService)
                container.RegisterType<Interfaces.Payment.Gateway.IPaymentGatewayRepository, Repositories.Payment.Gateway.PaymentGatewayRepository>(new PerRequestLifetimeManager());
                container.RegisterType<Interfaces.Payment.IOnlinePaymentRepository, Repositories.Payment.OnlinePaymentRepository>(new PerRequestLifetimeManager());

                // ثبت سرویس‌های پرداخت آنلاین
                container.RegisterType<Interfaces.Payment.Gateway.IPaymentGatewayService, Services.Payment.Gateway.PaymentGatewayService>(new PerRequestLifetimeManager());

                // ✅ ثبت Gateway Drivers (ZarinPal)
                container.RegisterType<Interfaces.Payment.Gateway.Drivers.IGatewayDriver, Services.Payment.Gateway.Drivers.ZarinPalDriver>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(new ResolvedParameter<Serilog.ILogger>())
                );

                // ✅ ثبت Gateway Driver Factory (Factory Pattern)
                container.RegisterType<Interfaces.Payment.Gateway.Drivers.IGatewayDriverFactory, Services.Payment.Gateway.Drivers.GatewayDriverFactory>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<Interfaces.Payment.Gateway.Drivers.IGatewayDriver>(), // ZarinPal Driver
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );

                // ✅ ثبت WebPaymentService (یکپارچه‌سازی با Gateway Driver Factory)
                container.RegisterType<Interfaces.Payment.Web.IWebPaymentService, Services.Payment.Web.WebPaymentService>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<Interfaces.Payment.Gateway.IPaymentGatewayRepository>(),
                        new ResolvedParameter<Interfaces.Payment.IOnlinePaymentRepository>(),
                        new ResolvedParameter<Interfaces.Payment.IPaymentTransactionRepository>(),
                        new ResolvedParameter<Interfaces.Payment.IPaymentService>(),
                        new ResolvedParameter<Interfaces.Payment.Gateway.Drivers.IGatewayDriverFactory>(), // ✅ Gateway Driver Factory
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );

                // ثبت سرویس اصلی پرداخت (IPaymentService)
                container.RegisterType<Interfaces.Payment.IPaymentService, Services.Payment.PaymentService>(new PerRequestLifetimeManager());

                // ✅ ثبت Payment Security Service (Enterprise-Grade)
                container.RegisterType<Interfaces.Payment.Security.IPaymentSecurityService, Services.Payment.Security.PaymentSecurityService>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<Interfaces.Payment.IOnlinePaymentRepository>(),
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );

                // ✅ ثبت Payment Management (Admin)
                container.RegisterType<Interfaces.Payment.Management.IPaymentManagementRepository, Repositories.Payment.Management.PaymentManagementRepository>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<ApplicationDbContext>(),
                        new ResolvedParameter<Interfaces.Payment.IOnlinePaymentRepository>(),
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );
                container.RegisterType<Interfaces.Payment.Management.IPaymentManagementService, Services.Payment.Management.PaymentManagementService>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<Interfaces.Payment.Management.IPaymentManagementRepository>(),
                        new ResolvedParameter<Interfaces.Payment.Web.IWebPaymentService>(),
                        new ResolvedParameter<Interfaces.Payment.IOnlinePaymentRepository>(),
                        new ResolvedParameter<ICurrentUserService>(),
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );

                // ثبت ریپازیتوری‌های POS
                container.RegisterType<IPosTerminalRepository, PosTerminalRepository>(new PerRequestLifetimeManager());
                container.RegisterType<ICashSessionRepository, CashSessionRepository>(new PerRequestLifetimeManager());
                // ثبت سرویس مدیریت POS
                container.RegisterType<IPosManagementService, PosManagementService>(new PerRequestLifetimeManager());

                // ثبت سرویس ارتباط با دستگاه کارت‌خوان POS
                container.RegisterType<IPosDeviceService, PosDeviceService>(new PerRequestLifetimeManager());
                container.RegisterType<PosPaymentOrchestrator, PosPaymentOrchestrator>(new PerRequestLifetimeManager());

                // ثبت ماژول پرداخت POS (Production-Ready)
                container.RegisterType<IPosPaymentService, PosPaymentService>(new PerRequestLifetimeManager());
                container.RegisterType<PosPaymentConfigurationService, PosPaymentConfigurationService>(new PerRequestLifetimeManager());

                // ========== ثبت سرویس‌های جدید برای Audit Trail و Performance - 1404/10/05 ==========
                container.RegisterType<ICashierReportService, CashierReportService>(new PerRequestLifetimeManager());
                container.RegisterType<ICashSessionAuditService, CashSessionAuditService>(new PerRequestLifetimeManager());
                container.RegisterType<IPaymentReconciliationService, PaymentReconciliationService>(new PerRequestLifetimeManager());
                container.RegisterType<ICashierPerformanceService, CashierPerformanceService>(new PerRequestLifetimeManager());

                // ========== ثبت سرویس‌های جدید برای Security & Login Audit - 2025-01-XX ==========
                container.RegisterType<Interfaces.Security.ILoginHistoryService, Services.Security.LoginHistoryService>(
                    new PerRequestLifetimeManager(),
                    new InjectionConstructor(
                        new ResolvedParameter<ApplicationDbContext>(),
                        new ResolvedParameter<Serilog.ILogger>()
                    )
                );

                // ثبت سرویس گزارش‌گیری پرداخت‌ها
                container.RegisterType<Interfaces.Payment.Reporting.IPaymentReportingService, Services.Payment.Reporting.PaymentReportingService>(new PerRequestLifetimeManager());

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

                // Register Payment Transaction Validators
                container.RegisterType<IValidator<ViewModels.Payment.PaymentTransactionCreateViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.PaymentTransactionCreateViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.PaymentTransactionEditViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.PaymentTransactionEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.PaymentTransactionSearchViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.PaymentTransactionSearchViewModelValidator>(new PerRequestLifetimeManager());

                // Register POS Validators
                container.RegisterType<IValidator<ViewModels.Payment.POS.PosTerminalCreateViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.PosTerminalCreateViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.POS.PosTerminalEditViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.PosTerminalEditViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.POS.PosTerminalSearchViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.PosTerminalSearchViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.POS.CashSessionStartViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.CashSessionStartViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.POS.CashSessionEndViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.CashSessionEndViewModelValidator>(new PerRequestLifetimeManager());
                container.RegisterType<IValidator<ViewModels.Payment.POS.CashSessionSearchViewModel>,
                    ClinicApp.ViewModels.Validators.Payment.POS.CashSessionSearchViewModelValidator>(new PerRequestLifetimeManager());


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

                // 🚀 P0 FIX: ثبت سرویس Idempotency
                container.RegisterType<IIdempotencyService, InMemoryIdempotencyService>(new PerRequestLifetimeManager());

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