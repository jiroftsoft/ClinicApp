using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ClinicApp.Models.Enums;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper class برای مدیریت اولویت بیمه
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. تبدیل بین enum و int
    /// 2. دریافت اولویت بعدی برای بیمه تکمیلی
    /// 3. اعتبارسنجی اولویت
    /// 4. دریافت توضیحات اولویت
    /// </summary>
    public static class InsurancePriorityHelper
    {
        /// <summary>
        /// دریافت اولویت بعدی برای بیمه تکمیلی
        /// </summary>
        /// <param name="existingPriorities">لیست اولویت‌های موجود</param>
        /// <returns>اولویت بعدی</returns>
        public static InsurancePriority GetNextSupplementaryPriority(IEnumerable<InsurancePriority> existingPriorities)
        {
            if (existingPriorities == null || !existingPriorities.Any())
            {
                return InsurancePriority.SupplementaryFirst;
            }

            var supplementaryPriorities = existingPriorities
                .Where(p => p != InsurancePriority.Primary)
                .OrderBy(p => (int)p)
                .ToList();

            if (!supplementaryPriorities.Any())
            {
                return InsurancePriority.SupplementaryFirst;
            }

            var maxPriority = supplementaryPriorities.Max(p => (int)p);
            var nextPriority = maxPriority + 1;

            // بررسی محدودیت اولویت
            if (nextPriority > (int)InsurancePriority.SupplementaryNinth)
            {
                throw new InvalidOperationException("حداکثر تعداد بیمه‌های تکمیلی 9 عدد است");
            }

            return (InsurancePriority)nextPriority;
        }

        /// <summary>
        /// دریافت اولویت بر اساس نوع بیمه
        /// </summary>
        /// <param name="isPrimary">آیا بیمه اصلی است؟</param>
        /// <param name="existingPriorities">لیست اولویت‌های موجود</param>
        /// <returns>اولویت مناسب</returns>
        public static InsurancePriority GetPriorityForInsurance(bool isPrimary, IEnumerable<InsurancePriority> existingPriorities = null)
        {
            if (isPrimary)
            {
                return InsurancePriority.Primary;
            }

            return GetNextSupplementaryPriority(existingPriorities ?? new List<InsurancePriority>());
        }

        /// <summary>
        /// بررسی اعتبار اولویت
        /// </summary>
        /// <param name="priority">اولویت برای بررسی</param>
        /// <returns>آیا اولویت معتبر است؟</returns>
        public static bool IsValidPriority(InsurancePriority priority)
        {
            return Enum.IsDefined(typeof(InsurancePriority), priority);
        }

        /// <summary>
        /// دریافت توضیحات اولویت
        /// </summary>
        /// <param name="priority">اولویت</param>
        /// <returns>توضیحات اولویت</returns>
        public static string GetPriorityDescription(InsurancePriority priority)
        {
            var field = typeof(InsurancePriority).GetField(priority.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            
            return attribute?.Description ?? priority.ToString();
        }

        /// <summary>
        /// دریافت لیست اولویت‌های تکمیلی
        /// </summary>
        /// <returns>لیست اولویت‌های تکمیلی</returns>
        public static IEnumerable<InsurancePriority> GetSupplementaryPriorities()
        {
            return Enum.GetValues(typeof(InsurancePriority))
                .Cast<InsurancePriority>()
                .Where(p => p != InsurancePriority.Primary)
                .OrderBy(p => (int)p);
        }

        /// <summary>
        /// تبدیل اولویت به عدد
        /// </summary>
        /// <param name="priority">اولویت</param>
        /// <returns>عدد اولویت</returns>
        public static int ToInt(InsurancePriority priority)
        {
            return (int)priority;
        }

        /// <summary>
        /// تبدیل عدد به اولویت
        /// </summary>
        /// <param name="value">عدد اولویت</param>
        /// <returns>اولویت</returns>
        public static InsurancePriority FromInt(int value)
        {
            if (!Enum.IsDefined(typeof(InsurancePriority), value))
            {
                throw new ArgumentException($"اولویت {value} معتبر نیست");
            }

            return (InsurancePriority)value;
        }

        /// <summary>
        /// بررسی اینکه آیا اولویت مربوط به بیمه اصلی است
        /// </summary>
        /// <param name="priority">اولویت</param>
        /// <returns>آیا اولویت مربوط به بیمه اصلی است؟</returns>
        public static bool IsPrimaryPriority(InsurancePriority priority)
        {
            return priority == InsurancePriority.Primary;
        }

        /// <summary>
        /// بررسی اینکه آیا اولویت مربوط به بیمه تکمیلی است
        /// </summary>
        /// <param name="priority">اولویت</param>
        /// <returns>آیا اولویت مربوط به بیمه تکمیلی است؟</returns>
        public static bool IsSupplementaryPriority(InsurancePriority priority)
        {
            return priority != InsurancePriority.Primary;
        }
    }
}
