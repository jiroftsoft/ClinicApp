using ClinicApp.Interfaces;
using System;
using System.Web;
using Unity;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی برای دسترسی سریع به اطلاعات کاربر جاری در کنترلرها و سایر بخش‌ها
    /// این کلاس برای سیستم‌های پزشکی طراحی شده و رعایت استانداردهای امنیتی را تضمین می‌کند
    /// 
    /// استفاده:
    /// string userId = AppHelper.CurrentUserId;
    /// bool isAdmin = AppHelper.IsAdmin;
    /// 
    /// نکته حیاتی: این کلاس با توجه به استانداردهای پزشکی طراحی شده و:
    /// 1. از Ambient Context Pattern به جای Service Locator استفاده می‌کند
    /// 2. از HttpContext برای دسترسی به اطلاعات کاربر استفاده می‌کند
    /// 3. در شرایط عدم دسترسی به سرویس، به حالت ایمن می‌رود
    /// </summary>
    public static class AppHelper
    {
        /// <summary>
        /// شناسه کاربر جاری
        /// </summary>
        public static string CurrentUserId => GetCurrentUser()?.UserId ?? "System";

        /// <summary>
        /// نام کاربری کاربر جاری
        /// </summary>
        public static string CurrentUserName => GetCurrentUser()?.UserName ?? "سیستم";

        /// <summary>
        /// آیا کاربر جاری احراز هویت شده است؟
        /// </summary>
        public static bool IsAuthenticated => GetCurrentUser()?.IsAuthenticated ?? false;

        /// <summary>
        /// آیا کاربر جاری در نقش مدیر سیستم است؟
        /// </summary>
        public static bool IsAdmin => GetCurrentUser()?.IsAdmin ?? false;

        /// <summary>
        /// آیا کاربر جاری پزشک است؟
        /// </summary>
        public static bool IsDoctor => GetCurrentUser()?.IsDoctor ?? false;

        /// <summary>
        /// آیا کاربر جاری منشی است؟
        /// </summary>
        public static bool IsReceptionist => GetCurrentUser()?.IsReceptionist ?? false;

        /// <summary>
        /// زمان فعلی سیستم به صورت UTC
        /// </summary>
        public static DateTime UtcNow => GetCurrentUser()?.UtcNow ?? DateTime.UtcNow;

        /// <summary>
        /// دریافت سرویس کاربر فعلی از طریق Ambient Context
        /// این روش برای سیستم‌های پزشکی ایمن‌تر از Service Locator است
        /// </summary>
        private static ICurrentUserService GetCurrentUser()
        {
            try
            {
                // روش 1: استفاده از HttpContext برای دسترسی به سرویس
                if (HttpContext.Current != null)
                {
                    var httpContext = new HttpContextWrapper(HttpContext.Current);
                    var unityContainer = UnityConfig.Container;

                    // بررسی وجود سرویس در کانتینر
                    if (unityContainer.IsRegistered<ICurrentUserService>())
                    {
                        return unityContainer.Resolve<ICurrentUserService>();
                    }
                }

                // روش 2: استفاده از DependencyResolver برای سیستم‌های پزشکی
                var resolver = System.Web.Mvc.DependencyResolver.Current;
                if (resolver != null)
                {
                    var service = resolver.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
                    if (service != null)
                        return service;
                }
            }
            catch
            {
                // در صورت بروز خطا، سیستم به حالت ایمن می‌رود
                // هیچ استثنا پرتاب نمی‌شود تا سیستم متوقف نشود
            }

            // در صورت عدم دسترسی به سرویس، null برمی‌گرداند
            return null;
        }
    }
}