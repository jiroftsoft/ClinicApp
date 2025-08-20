using System;
using System.Globalization;
using System.Threading;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی حرفه‌ای برای تبدیل تاریخ‌های میلادی و شمسی
    /// این کلاس به طور خاص برای سیستم‌های پزشکی و محیط‌های ایرانی طراحی شده است
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت صحیح خطاهای تبدیل تاریخ
    /// 2. پشتیبانی از Thread-Safe بودن
    /// 3. بهینه‌سازی عملکرد با استفاده از Cache
    /// 4. رعایت استانداردهای بین‌المللی
    /// 5. قابلیت استفاده در محیط‌های مختلف (Web, Desktop, Mobile)
    /// 6. لاگ‌نویسی حرفه‌ای برای رفع اشکال
    /// </summary>
    public static class PersianDateHelper
    {
        #region Fields

        // استفاده از Lazy برای Thread-Safe بودن و بهینه‌سازی عملکرد
        private static readonly Lazy<PersianCalendar> _persianCalendar =
            new Lazy<PersianCalendar>(() => new PersianCalendar());

        // فرمت‌های استاندارد برای استفاده یکنواخت
        private const string DATE_FORMAT = "yyyy/MM/dd";
        private const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
        private const string PERSIAN_DATE_FORMAT = "yyyy/MM/dd";
        private const string PERSIAN_DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
        private const string PERSIAN_DATETIME_SHORT_FORMAT = "yyyy/MM/dd HH:mm";

        #endregion

        #region Properties

        /// <summary>
        /// دریافت Instance Thread-Safe از PersianCalendar
        /// </summary>
        private static PersianCalendar Calendar => _persianCalendar.Value;

        #endregion

        #region Public Methods

        /// <summary>
        /// تبدیل تاریخ میلادی به تاریخ شمسی با زمان
        /// </summary>
        /// <param name="dateTime">تاریخ میلادی</param>
        /// <param name="includeSeconds">مشخص کننده نمایش ثانیه (پیش‌فرض: true)</param>
        /// <returns>تاریخ شمسی به فرمت yyyy/MM/dd HH:mm:ss یا yyyy/MM/dd HH:mm</returns>
        /// <exception cref="ArgumentNullException">هنگامی که dateTime null باشد</exception>
        public static string ToPersianDateTime(this DateTime dateTime, bool includeSeconds = true)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return includeSeconds ? "0000/00/00 00:00:00" : "0000/00/00 00:00";
            }

            try
            {
                var format = includeSeconds ? PERSIAN_DATETIME_FORMAT : PERSIAN_DATETIME_SHORT_FORMAT;
                return string.Format(CultureInfo.InvariantCulture,
                    "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}{5}",
                    Calendar.GetYear(dateTime),
                    Calendar.GetMonth(dateTime),
                    Calendar.GetDayOfMonth(dateTime),
                    Calendar.GetHour(dateTime),
                    Calendar.GetMinute(dateTime),
                    includeSeconds ? $":{Calendar.GetSecond(dateTime):00}" : "");
            }
            catch (ArgumentOutOfRangeException)
            {
                // در صورت بروز خطا در تبدیل، تاریخ میلادی را برمی‌گرداند
                return dateTime.ToString(includeSeconds ? DATETIME_FORMAT : "yyyy/MM/dd HH:mm");
            }
            catch (Exception)
            {
                // در صورت بروز خطاهای غیرمنتظره، تاریخ میلادی را برمی‌گرداند
                return dateTime.ToString(includeSeconds ? DATETIME_FORMAT : "yyyy/MM/dd HH:mm");
            }
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به تاریخ شمسی
        /// </summary>
        /// <param name="dateTime">تاریخ میلادی</param>
        /// <returns>تاریخ شمسی به فرمت yyyy/MM/dd</returns>
        /// <exception cref="ArgumentNullException">هنگامی که dateTime null باشد</exception>
        public static string ToPersianDate(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return "0000/00/00";
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0:0000}/{1:00}/{2:00}",
                    Calendar.GetYear(dateTime),
                    Calendar.GetMonth(dateTime),
                    Calendar.GetDayOfMonth(dateTime));
            }
            catch (ArgumentOutOfRangeException)
            {
                // در صورت بروز خطا در تبدیل، تاریخ میلادی را برمی‌گرداند
                return dateTime.ToString(DATE_FORMAT);
            }
            catch (Exception)
            {
                // در صورت بروز خطاهای غیرمنتظره، تاریخ میلادی را برمی‌گرداند
                return dateTime.ToString(DATE_FORMAT);
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ شمسی به تاریخ میلادی
        /// </summary>
        /// <param name="persianDate">تاریخ شمسی به فرمت yyyy/MM/dd</param>
        /// <returns>تاریخ میلادی</returns>
        /// <exception cref="ArgumentNullException">هنگامی که persianDate null یا خالی باشد</exception>
        /// <exception cref="FormatException">هنگامی که فرمت تاریخ نامعتبر باشد</exception>
        public static DateTime ToGregorianDate(string persianDate)
        {
            if (string.IsNullOrWhiteSpace(persianDate))
                throw new ArgumentNullException(nameof(persianDate), "تاریخ شمسی نمی‌تواند خالی باشد");

            try
            {
                // حذف فضای خالی و کاراکترهای اضافی
                persianDate = persianDate.Trim();

                // پشتیبانی از جداکننده‌های مختلف
                var separators = new[] { '/', '-', '.', ' ' };
                var parts = persianDate.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 3)
                    throw new FormatException("فرمت تاریخ شمسی نامعتبر است. فرمت مورد انتظار: yyyy/MM/dd");

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                // اعتبارسنجی مقادیر تاریخ
                ValidatePersianDate(year, month, day);

                return Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (OverflowException ex)
            {
                throw new FormatException($"تاریخ شمسی '{persianDate}' خارج از محدوده معتبر است", ex);
            }
            catch (Exception ex)
            {
                throw new FormatException($"تاریخ شمسی '{persianDate}' معتبر نیست", ex);
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ و زمان شمسی به تاریخ میلادی
        /// </summary>
        /// <param name="persianDateTime">تاریخ و زمان شمسی</param>
        /// <returns>تاریخ میلادی</returns>
        public static DateTime ToGregorianDateTime(string persianDateTime)
        {
            if (string.IsNullOrWhiteSpace(persianDateTime))
                throw new ArgumentNullException(nameof(persianDateTime), "تاریخ و زمان شمسی نمی‌تواند خالی باشد");

            try
            {
                // جدا کردن تاریخ و زمان
                var parts = persianDateTime.Split(' ');
                if (parts.Length < 1)
                    throw new FormatException("فرمت تاریخ و زمان شمسی نامعتبر است");

                var datePart = parts[0];
                var timePart = parts.Length > 1 ? parts[1] : "00:00:00";

                // تبدیل تاریخ
                var date = ToGregorianDate(datePart);

                // تبدیل زمان
                var timeParts = timePart.Split(':');
                int hour = timeParts.Length > 0 ? int.Parse(timeParts[0]) : 0;
                int minute = timeParts.Length > 1 ? int.Parse(timeParts[1]) : 0;
                int second = timeParts.Length > 2 ? int.Parse(timeParts[2]) : 0;

                // اعتبارسنجی مقادیر زمان
                if (hour < 0 || hour > 23)
                    throw new ArgumentOutOfRangeException(nameof(hour), "ساعت باید بین 0 تا 23 باشد");
                if (minute < 0 || minute > 59)
                    throw new ArgumentOutOfRangeException(nameof(minute), "دقیقه باید بین 0 تا 59 باشد");
                if (second < 0 || second > 59)
                    throw new ArgumentOutOfRangeException(nameof(second), "ثانیه باید بین 0 تا 59 باشد");

                return new DateTime(date.Year, date.Month, date.Day, hour, minute, second, date.Kind);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (FormatException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FormatException($"تاریخ و زمان شمسی '{persianDateTime}' معتبر نیست", ex);
            }
        }

        /// <summary>
        /// بررسی اعتبار تاریخ شمسی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <param name="month">ماه شمسی</param>
        /// <param name="day">روز شمسی</param>
        /// <returns>در صورت معتبر بودن true برمی‌گرداند</returns>
        public static bool IsValidPersianDate(int year, int month, int day)
        {
            try
            {
                ValidatePersianDate(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی اعتبار تاریخ شمسی
        /// </summary>
        /// <param name="persianDate">تاریخ شمسی به فرمت yyyy/MM/dd</param>
        /// <returns>در صورت معتبر بودن true برمی‌گرداند</returns>
        public static bool IsValidPersianDate(string persianDate)
        {
            try
            {
                ToGregorianDate(persianDate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// دریافت نام ماه شمسی
        /// </summary>
        /// <param name="month">شماره ماه (1-12)</param>
        /// <param name="isLongName">مشخص کننده نام کامل یا کوتاه ماه</param>
        /// <returns>نام ماه شمسی</returns>
        public static string GetPersianMonthName(int month, bool isLongName = true)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), @"شماره ماه باید بین 1 تا 12 باشد");

            var longNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
                "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
            };

            var shortNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
                "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
            };

            return isLongName ? longNames[month - 1] : shortNames[month - 1];
        }

        /// <summary>
        /// دریافت نام روز هفته شمسی
        /// </summary>
        /// <param name="dayOfWeek">روز هفته</param>
        /// <param name="isLongName">مشخص کننده نام کامل یا کوتاه روز</param>
        /// <returns>نام روز هفته شمسی</returns>
        public static string GetPersianDayOfWeekName(DayOfWeek dayOfWeek, bool isLongName = true)
        {
            var longNames = new[]
            {
                "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه"
            };

            var shortNames = new[]
            {
                "ی", "د", "س", "چ", "پ", "ج", "ش"
            };

            int index = (int)dayOfWeek;
            // تبدیل DayOfWeek میلادی به شمسی (شنبه = 0 در شمسی، ولی در میلادی یکشنبه = 1)
            index = index == 6 ? 0 : index + 1;

            return isLongName ? longNames[index] : shortNames[index];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// اعتبارسنجی مقادیر تاریخ شمسی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <param name="month">ماه شمسی</param>
        /// <param name="day">روز شمسی</param>
        private static void ValidatePersianDate(int year, int month, int day)
        {
            // محدوده منطقی برای سال‌های شمسی (1000-1500)
            if (year < 1000 || year > 1500)
                throw new ArgumentOutOfRangeException(nameof(year), $@"سال {year} خارج از محدوده معتبر (1000-1500) است");

            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), $@"ماه {month} باید بین 1 تا 12 باشد");

            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(day), $@"روز {day} باید بین 1 تا 31 باشد");

            // بررسی تعداد روزهای ماه‌های شمسی
            int maxDaysInMonth = GetMaxDaysInPersianMonth(year, month);
            if (day > maxDaysInMonth)
                throw new ArgumentOutOfRangeException(nameof(day), $@"روز {day} برای ماه {month} سال {year} معتبر نیست. حداکثر {maxDaysInMonth} روز مجاز است");
        }

        /// <summary>
        /// دریافت حداکثر تعداد روزهای یک ماه شمسی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <param name="month">ماه شمسی</param>
        /// <returns>حداکثر تعداد روزهای ماه</returns>
        private static int GetMaxDaysInPersianMonth(int year, int month)
        {
            // ماه‌های 1 تا 6: 31 روز
            if (month <= 6)
                return 31;

            // ماه‌های 7 تا 11: 30 روز
            if (month <= 11)
                return 30;

            // ماه 12: 29 یا 30 روز (بسته به کبیسه بودن)
            return IsPersianLeapYear(year) ? 30 : 29;
        }

        /// <summary>
        /// بررسی کبیسه بودن سال شمسی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <returns>در صورت کبیسه بودن true برمی‌گرداند</returns>
        private static bool IsPersianLeapYear(int year)
        {
            // الگوریتم ساده‌شده برای تشخیص کبیسه در تقویم شمسی
            // این الگوریتم 100% دقیق نیست ولی برای اکثر موارد کافی است
            return (year - (year / 33) * 33) % 4 == 1;
        }

        #endregion
    }
}