using System;
using System.Web.Mvc;
using Serilog;
using ClinicApp.Helpers;

namespace ClinicApp.Extensions
{
    /// <summary>
    /// Extension Methods برای Parse کردن تاریخ در Controller
    /// طبق appointment_controller_review.md - فاز 1
    /// حذف کد تکراری Date Parsing (3 مورد → 1 Extension Method)
    /// </summary>
    public static class ControllerDateExtensions
    {
        /// <summary>
        /// تبدیل امن تاریخ شمسی به میلادی با Fallback به Today
        /// پشتیبانی از فرمت‌های مختلف: شمسی (YYYY/MM/DD), timestamp, ISO
        /// </summary>
        /// <param name="controller">Controller instance</param>
        /// <param name="dateString">تاریخ شمسی یا timestamp</param>
        /// <param name="logger">Logger برای ثبت خطاها</param>
        /// <returns>تاریخ میلادی (یا Today در صورت خطا)</returns>
        public static DateTime ParsePersianDateSafe(
            this Controller controller,
            string dateString,
            ILogger logger)
        {
            // خالی → امروز
            if (string.IsNullOrWhiteSpace(dateString))
            {
                logger.Debug("تاریخ خالی، استفاده از امروز");
                return DateTime.Today;
            }

            try
            {
                DateTime parsedDate;

                // ✅ اول: بررسی تاریخ شمسی (YYYY/MM/DD)
                if (dateString.Contains("/") && dateString.Split('/').Length == 3)
                {
                    var parts = dateString.Split('/');
                    var year = int.Parse(parts[0]);
                    var month = int.Parse(parts[1]);
                    var day = int.Parse(parts[2]);

                    var persianCalendar = new System.Globalization.PersianCalendar();
                    parsedDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0).Date;
                    
                    logger.Debug("تاریخ شمسی تبدیل شد: {PersianDate} -> {GregorianDate}",
                        dateString, parsedDate.ToString("yyyy/MM/dd"));
                }
                // ✅ دوم: بررسی timestamp
                else if (long.TryParse(dateString, out long timestamp) && timestamp > 1000000000)
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    
                    if (timestamp > 9999999999)
                    {
                        // milliseconds
                        parsedDate = epoch.AddMilliseconds(timestamp).ToLocalTime().Date;
                    }
                    else
                    {
                        // seconds
                        parsedDate = epoch.AddSeconds(timestamp).ToLocalTime().Date;
                    }
                    
                    logger.Debug("تاریخ از timestamp تبدیل شد: {Timestamp} -> {Date}",
                        timestamp, parsedDate.ToString("yyyy/MM/dd"));
                }
                // ✅ سوم: استفاده از PersianDateHelper
                else
                {
                    parsedDate = PersianDateHelper.ToGregorianDate(dateString).Date;
                    logger.Debug("تاریخ با PersianDateHelper تبدیل شد: {PersianDate} -> {Date}",
                        dateString, parsedDate.ToString("yyyy/MM/dd"));
                }

                // بررسی گذشته نباشد
                if (parsedDate < DateTime.Today)
                {
                    logger.Warning("تاریخ {Date} در گذشته است، استفاده از امروز",
                        parsedDate.ToString("yyyy/MM/dd"));
                    return DateTime.Today;
                }

                return parsedDate;
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "خطا در Parse تاریخ {DateString}, استفاده از امروز",
                    dateString);
                return DateTime.Today;
            }
        }
    }
}

