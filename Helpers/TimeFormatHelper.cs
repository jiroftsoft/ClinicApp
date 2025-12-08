using System;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای فرمت کردن زمان به فارسی
    /// </summary>
    public static class TimeFormatHelper
    {
        /// <summary>
        /// تبدیل TimeSpan به فرمت فارسی (قبل از ظهر / بعد از ظهر)
        /// </summary>
        public static string FormatTimeToPersian(TimeSpan time)
        {
            var hour = time.Hours;
            var minute = time.Minutes;

            string period;
            int displayHour;

            if (hour == 0)
            {
                displayHour = 12;
                period = "قبل از ظهر";
            }
            else if (hour < 12)
            {
                displayHour = hour;
                period = "قبل از ظهر";
            }
            else if (hour == 12)
            {
                displayHour = 12;
                period = "بعد از ظهر";
            }
            else
            {
                displayHour = hour - 12;
                period = "بعد از ظهر";
            }

            return $"{displayHour}:{minute:D2} {period}";
        }

        /// <summary>
        /// تبدیل TimeSpan به بازه زمانی فارسی
        /// </summary>
        public static string FormatTimeRangeToPersian(TimeSpan startTime, TimeSpan endTime)
        {
            return $"{FormatTimeToPersian(startTime)} - {FormatTimeToPersian(endTime)}";
        }
    }
}

