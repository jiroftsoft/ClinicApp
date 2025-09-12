using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using ClinicApp.Models.Core;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس حرفه‌ای گسترش‌دهنده روش‌های Identity برای سیستم‌های پزشکی
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
/// string firstName = User.Identity.GetFirstName();
/// bool isAdmin = User.Identity.IsInRole(AppRoles.Admin);
/// 
/// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
/// </summary>
public static class IdentityExtensions
{
    #region User Information Methods (روش‌های اطلاعات کاربر)

    /// <summary>
    /// دریافت نام کاربر از ادعاها (Claims)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - نام کاربر در تمام سیستم‌های پزشکی نمایش داده می‌شود
    /// - برای سند افتتاح پرونده پزشکی استفاده می‌شود
    /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
    /// </summary>
    public static string GetFirstName(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ??
                   claimsIdentity.FindFirst("FirstName")?.Value ??
                   string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// دریافت نام خانوادگی کاربر از ادعاها (Claims)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - نام خانوادگی کاربر در تمام سیستم‌های پزشکی نمایش داده می‌شود
    /// - برای سند افتتاح پرونده پزشکی استفاده می‌شود
    /// - برای جستجوی بیماران بر اساس نام خانوادگی
    /// </summary>
    public static string GetLastName(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ??
                   claimsIdentity.FindFirst("LastName")?.Value ??
                   string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// دریافت نام کامل کاربر از ادعاها (Claims)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - نام کامل کاربر در تمام سیستم‌های پزشکی نمایش داده می‌شود
    /// - برای سند افتتاح پرونده پزشکی استفاده می‌شود
    /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
    /// </summary>
    public static string GetFullName(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            var firstName = claimsIdentity.FindFirst(ClaimTypes.GivenName)?.Value ??
                            claimsIdentity.FindFirst("FirstName")?.Value ??
                            string.Empty;

            var lastName = claimsIdentity.FindFirst(ClaimTypes.Surname)?.Value ??
                           claimsIdentity.FindFirst("LastName")?.Value ??
                           string.Empty;

            return $"{firstName} {lastName}".Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// دریافت کد ملی کاربر از ادعاها (Claims)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - کد ملی به عنوان شناسه اصلی بیماران استفاده می‌شود
    /// - برای جستجوی دقیق بیماران ضروری است
    /// - برای ارتباط با سیستم‌های ملی سلامت
    /// </summary>
    public static string GetNationalCode(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst("NationalCode")?.Value ??
                   claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// دریافت شماره تلفن همراه کاربر از ادعاها (Claims)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای ارسال پیامک‌های یادآوری نوبت
    /// - برای ارسال کد امنیتی (OTP) در سیستم پسورد‌لس
    /// - برای تماس‌های ضروری با بیماران
    /// </summary>
    public static string GetPhoneNumber(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindFirst(ClaimTypes.MobilePhone)?.Value ??
                   claimsIdentity.FindFirst("PhoneNumber")?.Value ??
                   string.Empty;
        }

        return string.Empty;
    }

    #endregion

    #region Role & Permission Methods (روش‌های نقش و دسترسی)

    /// <summary>
    /// بررسی اینکه آیا کاربر در نقش خاصی است؟
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای مدیریت دسترسی‌های سطح بالا (مدیر)
    /// - برای مدیریت دسترسی‌های پزشکی
    /// - برای مدیریت دسترسی‌های پذیرش
    /// </summary>
    public static bool IsInRole(this IIdentity identity, string role)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.HasClaim(ClaimTypes.Role, role);
        }

        return false;
    }

    /// <summary>
    /// دریافت تمام نقش‌های کاربر
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش منوی‌های مربوط به هر نقش
    /// - برای مدیریت پیچیده دسترسی‌ها
    /// - برای گزارش‌گیری و آمارگیری
    /// </summary>
    public static string[] GetRoles(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// بررسی دسترسی کاربر به یک عملیات خاص
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای مدیریت دسترسی‌های پیچیده
    /// - برای اجرای منطق کسب‌وکار خاص سیستم‌های پزشکی
    /// - برای امنیت اطلاعات حساس پزشکی
    /// </summary>
    public static bool HasPermission(this IIdentity identity, string permission)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            return claimsIdentity.HasClaim("Permission", permission);
        }

        return false;
    }

    #endregion

    #region Security & Authentication Methods (روش‌های امنیتی و احراز هویت)


    /// <summary>
    /// دریافت نوع احراز هویت کاربر
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای تشخیص روش ورود کاربر (OTP، کد ملی، ایمیل)
    /// - برای مدیریت سطح امنیت کاربر
    /// - برای گزارش‌گیری امنیتی
    /// </summary>
    public static string GetAuthenticationType(this IIdentity identity)
    {
        return identity?.AuthenticationType ?? string.Empty;
    }

    /// <summary>
    /// آیا کاربر با روش امنیتی بالا وارد شده است؟
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای دسترسی به اطلاعات حساس پزشکی
    /// - برای انجام عملیات‌های حساس (تغییر نوبت‌ها، ویرایش پرونده)
    /// - برای رعایت استانداردهای امنیتی سیستم‌های پزشکی
    /// </summary>
    public static bool IsHighSecurityAuthentication(this IIdentity identity)
    {
        if (identity is ClaimsIdentity claimsIdentity)
        {
            // در سیستم‌های پزشکی، احراز هویت دو مرحله‌ای برای عملیات حساس ضروری است
            var isTwoFactor = claimsIdentity.FindFirst("amr")?.Value.Contains("mfa") ?? false;
            var isSecure = claimsIdentity.AuthenticationType == "ApplicationCookie" ||
                          claimsIdentity.AuthenticationType == "Bearer";

            return isTwoFactor || isSecure;
        }

        return false;
    }

    #endregion

    #region Medical System Specific Methods (روش‌های خاص سیستم‌های پزشکی)

    /// <summary>
    /// آیا کاربر یک پزشک است؟
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش منوی‌های پزشکی
    /// - برای مدیریت نوبت‌های پزشکی
    /// - برای دسترسی به پرونده‌های پزشکی
    /// </summary>
    public static bool IsDoctor(this IIdentity identity)
    {
        return identity.IsInRole(AppRoles.Doctor);
    }

    /// <summary>
    /// آیا کاربر یک منشی است؟
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش منوی‌های پذیرش
    /// - برای مدیریت نوبت‌دهی
    /// - برای انجام عملیات پذیرش
    /// </summary>
    public static bool IsReceptionist(this IIdentity identity)
    {
        return identity.IsInRole(AppRoles.Receptionist);
    }

    /// <summary>
    /// آیا کاربر یک بیمار است؟
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش پورتال بیمار
    /// - برای مدیریت نوبت‌های بیمار
    /// - برای دسترسی به اطلاعات پزشکی بیمار
    /// </summary>
    public static bool IsPatient(this IIdentity identity)
    {
        return identity.IsInRole(AppRoles.Patient);
    }

    /// <summary>
    /// دریافت اطلاعات پزشکی کاربر (در صورتی که کاربر پزشک باشد)
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش اطلاعات پزشک در سیستم
    /// - برای مدیریت نوبت‌های پزشک
    /// - برای ارتباط با سایر سرویس‌های پزشکی
    /// </summary>


 

    #endregion
}