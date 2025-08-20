using AutoMapper;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Serilog;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Web;
using Unity;
using Unity.AspNet.Mvc;
using Unity.Injection;
using Unity.Lifetime;

namespace ClinicApp
{
    public static class UnityConfig
    {
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        public static IUnityContainer Container => container.Value;

        public static void RegisterTypes(IUnityContainer container)
        {
            // ثبت DbContext
            container.RegisterType<DbContext, ApplicationDbContext>(new PerRequestLifetimeManager());

            // ثبت Identity
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(c =>
                HttpContext.Current.GetOwinContext().Authentication));

            // ثبت سرویس کاربر فعلی با پشتیبانی کامل از HttpContext
            // ثبت سرویس کاربر فعلی با پشتیبانی کامل از تمام وابستگی‌ها
            container.RegisterType<ICurrentUserService, CurrentUserService>(
                new PerRequestLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<HttpContextBase>(),
                    new ResolvedParameter<ApplicationUserManager>(),
                    new ResolvedParameter<ILogger>(),
                    new ResolvedParameter<ApplicationDbContext>()
                )
            );

            // ثبت HttpContextBase
            container.RegisterType<HttpContextBase>(
                new InjectionFactory(c => new HttpContextWrapper(HttpContext.Current))
            );

            // ثبت ApplicationUserManager
            container.RegisterType<ApplicationUserManager>(
                new InjectionFactory(c =>
                {
                    var context = new ApplicationDbContext();
                    var store = new UserStore<ApplicationUser>(context);
                    var userManager = new ApplicationUserManager(store);
                    return userManager;
                })
            );

            // AutoMapper
            var mappingConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });
            container.RegisterInstance(mappingConfig.CreateMapper());

            // Logger
            container.RegisterInstance<Serilog.ILogger>(Log.Logger);

            // سرویس‌ها
            container.RegisterType<IPatientService, PatientService>();
            container.RegisterType<IAuthService, AuthService>();
            container.RegisterType<IIdentityMessageService, AsanakSmsService>();
            container.RegisterType<IDoctorService, DoctorService>();
            container.RegisterType<IClinicService, ClinicService>();
            container.RegisterType<IDepartmentService, DepartmentService>();
            container.RegisterType<IServiceCategoryService, ServiceCategoryService>();
            container.RegisterType<IServiceService, ServiceService>();
            // ثبت تنظیمات سیستم
            container.RegisterType<IAppSettings, AppSettings>(new ContainerControlledLifetimeManager());

            // ثبت سایر کامپوننت‌ها
            container.RegisterType<IPatientService, PatientService>();
            container.RegisterType<IInsuranceService, InsuranceService>();


        }
    }
}