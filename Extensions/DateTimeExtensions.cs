using System;
using ClinicApp.Helpers;

namespace ClinicApp.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToPersianDate(this DateTime date)
            => PersianDateHelper.ToPersianDate(date);

        public static string ToPersianDateTime(this DateTime date, bool includeSeconds = true)
            => PersianDateHelper.ToPersianDateTime(date, includeSeconds);

        // It's also helpful to have overloads for nullable DateTimes
        public static string ToPersianDate(this DateTime? date)
            => date.HasValue ? PersianDateHelper.ToPersianDate(date.Value) : string.Empty;

        public static string ToPersianDateTime(this DateTime? date, bool includeSeconds = true)
            => date.HasValue ? PersianDateHelper.ToPersianDateTime(date.Value, includeSeconds) : string.Empty;

        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی
        /// </summary>
        public static DateTime ToDateTime(this string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                throw new ArgumentException("تاریخ شمسی نمی‌تواند خالی باشد", nameof(persianDate));

            return PersianDateHelper.ToGregorianDate(persianDate);
        }

        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی (nullable)
        /// </summary>
        public static DateTime? ToDateTimeNullable(this string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                return null;

            try
            {
                return PersianDateHelper.ToGregorianDate(persianDate);
            }
            catch
            {
                return null;
            }
        }
    }
}