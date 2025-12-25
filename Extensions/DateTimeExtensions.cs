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

        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی (alias برای ToDateTime)
        /// </summary>
        public static DateTime ToDateTimeFromPersian(this string persianDate)
        {
            return ToDateTime(persianDate);
        }

        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی (nullable) (alias برای ToDateTimeNullable)
        /// </summary>
        public static DateTime? ToDateTimeFromPersianNullable(this string persianDate)
        {
            return ToDateTimeNullable(persianDate);
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به رشته شمسی
        /// </summary>
        public static string ToPersianDateString(this DateTime date)
        {
            return PersianDateHelper.ToPersianDate(date);
        }

        #region Start/End Operations - عملیات ابتدا و انتها

        /// <summary>
        /// Gets the start of the day (00:00:00.000)
        /// دریافت ابتدای روز
        /// </summary>
        public static DateTime StartOfDay(this DateTime date)
        {
            return date.Date;
        }

        /// <summary>
        /// Gets the end of the day (23:59:59.999)
        /// دریافت انتهای روز
        /// </summary>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// Gets the start of the week
        /// دریافت ابتدای هفته
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Saturday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets the end of the week
        /// دریافت انتهای هفته
        /// </summary>
        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Saturday)
        {
            return date.StartOfWeek(startOfWeek).AddDays(7).AddTicks(-1);
        }

        /// <summary>
        /// Gets the start of the month
        /// دریافت ابتدای ماه
        /// </summary>
        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Gets the end of the month
        /// دریافت انتهای ماه
        /// </summary>
        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.StartOfMonth().AddMonths(1).AddTicks(-1);
        }

        #endregion

        #region Relative Time - زمان نسبی

        /// <summary>
        /// Converts DateTime to relative time string (e.g., "2 hours ago")
        /// تبدیل تاریخ به زمان نسبی (مثلاً "2 ساعت پیش")
        /// </summary>
        public static string ToRelativeTime(this DateTime date)
        {
            var timeSpan = DateTime.Now - date;

            if (timeSpan.TotalSeconds < 60)
                return "همین الان";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} دقیقه پیش";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعت پیش";

            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} روز پیش";

            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} هفته پیش";

            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} ماه پیش";

            return $"{(int)(timeSpan.TotalDays / 365)} سال پیش";
        }

        #endregion

        #region Validation & Comparison - اعتبارسنجی و مقایسه

        /// <summary>
        /// Checks if date is between two dates (inclusive)
        /// بررسی قرار گرفتن تاریخ بین دو تاریخ
        /// </summary>
        public static bool IsBetween(this DateTime date, DateTime start, DateTime end)
        {
            return date >= start && date <= end;
        }

        /// <summary>
        /// Checks if the date is a weekend (Friday in Persian calendar)
        /// بررسی آیا تاریخ آخر هفته است
        /// </summary>
        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Friday;
        }

        /// <summary>
        /// Checks if the date is a workday
        /// بررسی آیا تاریخ روز کاری است
        /// </summary>
        public static bool IsWorkday(this DateTime date)
        {
            return !date.IsWeekend();
        }

        #endregion
    }
}