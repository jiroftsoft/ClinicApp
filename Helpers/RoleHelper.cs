using System;
using System.Collections.Generic;
using ClinicApp.Models.Core;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای تبدیل نام نقش‌های انگلیسی به فارسی
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public static class RoleHelper
    {
        /// <summary>
        /// Dictionary برای نگاشت نام نقش‌های انگلیسی به فارسی
        /// </summary>
        private static readonly Dictionary<string, string> RolePersianNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { AppRoles.Admin, "مدیر سیستم" },
            { AppRoles.Doctor, "پزشک" },
            { AppRoles.Receptionist, "منشی" },
            { AppRoles.Patient, "بیمار" },
            { AppRoles.System, "سیستم" }
        };

        /// <summary>
        /// دریافت نام فارسی نقش
        /// </summary>
        /// <param name="roleName">نام نقش به انگلیسی</param>
        /// <returns>نام فارسی نقش یا همان نام انگلیسی در صورت عدم وجود</returns>
        public static string GetPersianName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return "بدون نقش";
            }

            if (RolePersianNames.ContainsKey(roleName))
            {
                return RolePersianNames[roleName];
            }

            // در صورت عدم وجود در Dictionary، همان نام انگلیسی را برمی‌گرداند
            return roleName;
        }

        /// <summary>
        /// تبدیل لیست نقش‌ها به فارسی
        /// </summary>
        /// <param name="roleNames">لیست نام نقش‌ها</param>
        /// <returns>لیست نام‌های فارسی</returns>
        public static List<string> GetPersianNames(List<string> roleNames)
        {
            if (roleNames == null || roleNames.Count == 0)
            {
                return new List<string> { "بدون نقش" };
            }

            var persianNames = new List<string>();
            foreach (var roleName in roleNames)
            {
                persianNames.Add(GetPersianName(roleName));
            }

            return persianNames;
        }

        /// <summary>
        /// تبدیل Dictionary نقش‌ها به فارسی (برای SelectListItem)
        /// </summary>
        /// <param name="roleName">نام نقش انگلیسی</param>
        /// <returns>نام فارسی نقش</returns>
        public static string ToPersian(string roleName)
        {
            return GetPersianName(roleName);
        }
    }
}

