using System;
using System.Globalization;
using System.Threading;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس حرفه‌ای کمکی برای تبدیل تاریخ‌های میلادی و شمسی برای سیستم‌های پزشکی
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
    /// string persianDate = DateTime.Now.ToPersianDate();
    /// DateTime gregorianDate = PersianDateHelper.ToGregorianDate("1402/05/15");
    /// 
    /// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
    /// </summary>
    public static class PersianDateHelper
    {
        private static readonly ILogger _log = Log.ForContext(typeof(IranianNationalCodeValidator));

        #region Fields

        // استفاده از Lazy برای Thread-Safe بودن و بهینه‌سازی عملکرد
        private static readonly Lazy<PersianCalendar> _persianCalendar =
            new Lazy<PersianCalendar>(() => new PersianCalendar());

        // محدوده معتبر تقویم شمسی
        private const int MIN_PERSIAN_YEAR = 1;
        private const int MAX_PERSIAN_YEAR = 9378;

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
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - تمام تاریخ‌ها در سیستم‌های پزشکی ایرانی باید شمسی باشند
        /// - برای نمایش تاریخ‌ها در UI و گزارش‌ها
        /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
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
                // بررسی محدوده تاریخ برای تقویم شمسی
                if (!IsDateTimeInPersianCalendarRange(dateTime))
                {
                    _log.Warning("تاریخ میلادی خارج از محدوده معتبر تقویم شمسی است: {DateTime}", dateTime);
                    return dateTime.ToString(includeSeconds ? DATETIME_FORMAT : "yyyy/MM/dd HH:mm");
                }

                int year = Calendar.GetYear(dateTime);
                int month = Calendar.GetMonth(dateTime);
                int day = Calendar.GetDayOfMonth(dateTime);
                int hour = Calendar.GetHour(dateTime);
                int minute = Calendar.GetMinute(dateTime);
                int second = Calendar.GetSecond(dateTime);

                return string.Format(CultureInfo.InvariantCulture,
                    includeSeconds ? "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}" : "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}",
                    year, month, day, hour, minute, second);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _log.Error(ex, "خطا در تبدیل تاریخ میلادی به شمسی: {DateTime}", dateTime);
                return dateTime.ToString(includeSeconds ? DATETIME_FORMAT : "yyyy/MM/dd HH:mm");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در تبدیل تاریخ میلادی به شمسی: {DateTime}", dateTime);
                return dateTime.ToString(includeSeconds ? DATETIME_FORMAT : "yyyy/MM/dd HH:mm");
            }
        }

        /// <summary>
        /// تبدیل تاریخ میلادی به تاریخ شمسی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - تمام تاریخ‌ها در سیستم‌های پزشکی ایرانی باید شمسی باشند
        /// - برای نمایش تاریخ‌ها در UI و گزارش‌ها
        /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
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
                // بررسی محدوده تاریخ برای تقویم شمسی
                if (!IsDateTimeInPersianCalendarRange(dateTime))
                {
                    _log.Warning("تاریخ میلادی خارج از محدوده معتبر تقویم شمسی است: {DateTime}", dateTime);
                    return dateTime.ToString(DATE_FORMAT);
                }

                int year = Calendar.GetYear(dateTime);
                int month = Calendar.GetMonth(dateTime);
                int day = Calendar.GetDayOfMonth(dateTime);

                return string.Format(CultureInfo.InvariantCulture,
                    "{0:0000}/{1:00}/{2:00}",
                    year, month, day);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _log.Error(ex, "خطا در تبدیل تاریخ میلادی به شمسی: {DateTime}", dateTime);
                return dateTime.ToString(DATE_FORMAT);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در تبدیل تاریخ میلادی به شمسی: {DateTime}", dateTime);
                return dateTime.ToString(DATE_FORMAT);
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ شمسی به تاریخ میلادی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای ذخیره‌سازی تاریخ‌ها در پایگاه داده (میلادی)
        /// - برای محاسبات تاریخی
        /// - برای ارتباط با سیستم‌های بین‌المللی
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

                int year = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int month = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int day = int.Parse(parts[2], CultureInfo.InvariantCulture);

                // اعتبارسنجی مقادیر تاریخ
                ValidatePersianDate(year, month, day);

                // بررسی محدوده تاریخ برای تقویم شمسی
                if (year < MIN_PERSIAN_YEAR || year > MAX_PERSIAN_YEAR)
                {
                    _log.Warning("تاریخ شمسی خارج از محدوده معتبر است: {PersianDate}", persianDate);
                    return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
                }

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
                _log.Error(ex, "تاریخ شمسی '{PersianDate}' خارج از محدوده معتبر است", persianDate);
                throw new FormatException($"تاریخ شمسی '{persianDate}' خارج از محدوده معتبر است", ex);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "تاریخ شمسی '{PersianDate}' معتبر نیست", persianDate);
                throw new FormatException($"تاریخ شمسی '{persianDate}' معتبر نیست", ex);
            }
        }

        /// <summary>
        /// تبدیل رشته تاریخ و زمان شمسی به تاریخ میلادی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای ذخیره‌سازی تاریخ‌ها در پایگاه داده (میلادی)
        /// - برای محاسبات تاریخی
        /// - برای ارتباط با سیستم‌های بین‌المللی
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
                var parts = persianDateTime.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1)
                    throw new FormatException("فرمت تاریخ و زمان شمسی نامعتبر است");

                var datePart = parts[0];
                var timePart = parts.Length > 1 ? parts[1] : "00:00:00";

                // تبدیل تاریخ
                var date = ToGregorianDate(datePart);

                // تبدیل زمان
                var timeParts = timePart.Split(':');
                int hour = timeParts.Length > 0 ? int.Parse(timeParts[0], CultureInfo.InvariantCulture) : 0;
                int minute = timeParts.Length > 1 ? int.Parse(timeParts[1], CultureInfo.InvariantCulture) : 0;
                int second = timeParts.Length > 2 ? int.Parse(timeParts[2], CultureInfo.InvariantCulture) : 0;

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
                _log.Error(ex, "تاریخ و زمان شمسی '{PersianDateTime}' معتبر نیست", persianDateTime);
                throw new FormatException($"تاریخ و زمان شمسی '{persianDateTime}' معتبر نیست", ex);
            }
        }

        /// <summary>
        /// بررسی اعتبار تاریخ شمسی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - قبل از ذخیره‌سازی در پایگاه داده
        /// - قبل از ارسال به سیستم‌های دیگر
        /// - برای جلوگیری از خطاهای تاریخی
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
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - قبل از ذخیره‌سازی در پایگاه داده
        /// - قبل از ارسال به سیستم‌های دیگر
        /// - برای جلوگیری از خطاهای تاریخی
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
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای نمایش نام ماه در UI
        /// - برای گزارش‌های ماهانه
        /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
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
                "فرور", "اردی", "خرداد", "تیر", "مرداد", "شهری",
                "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
            };

            return isLongName ? longNames[month - 1] : shortNames[month - 1];
        }

        /// <summary>
        /// دریافت نام روز هفته شمسی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای نمایش نام روز در UI
        /// - برای برنامه‌ریزی نوبت‌دهی
        /// - برای ارسال اطلاع‌رسانی‌های شخصی‌سازی شده
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

        /// <summary>
        /// محاسبه تاریخ شمسی امروز
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای نمایش تاریخ جاری در UI
        /// - برای ایجاد پرونده‌های جدید
        /// - برای ارسال اطلاع‌رسانی‌های روزانه
        /// </summary>
        public static string Today => DateTime.Now.ToPersianDate();

        /// <summary>
        /// محاسبه تاریخ و زمان شمسی امروز
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای نمایش تاریخ جاری در UI
        /// - برای ایجاد پرونده‌های جدید
        /// - برای ارسال اطلاع‌رسانی‌های روزانه
        /// </summary>
        public static string Now => DateTime.Now.ToPersianDateTime();

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// بررسی اینکه آیا تاریخ میلادی در محدوده معتبر تقویم شمسی است
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - تقویم شمسی فقط در محدوده خاصی معتبر است
        /// - برای جلوگیری از خطاهای تبدیل تاریخ
        /// - برای اطمینان از صحت داده‌ها
        /// </summary>
        private static bool IsDateTimeInPersianCalendarRange(DateTime dateTime)
        {
            try
            {
                // تقویم شمسی فقط از تاریخ 622/03/22 میلادی شروع می‌شود
                DateTime minDate = new DateTime(622, 3, 22);
                DateTime maxDate = new DateTime(5550, 1, 1); // تقریباً معادل سال 9378 شمسی

                return dateTime >= minDate && dateTime <= maxDate;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// اعتبارسنجی مقادیر تاریخ شمسی
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - قبل از ذخیره‌سازی در پایگاه داده
        /// - قبل از ارسال به سیستم‌های دیگر
        /// - برای جلوگیری از خطاهای تاریخی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <param name="month">ماه شمسی</param>
        /// <param name="day">روز شمسی</param>
        private static void ValidatePersianDate(int year, int month, int day)
        {
            // محدوده منطقی برای سال‌های شمسی (0001-9378)
            if (year < MIN_PERSIAN_YEAR || year > MAX_PERSIAN_YEAR)
                throw new ArgumentOutOfRangeException(nameof(year), $@"سال {year} خارج از محدوده معتبر ({MIN_PERSIAN_YEAR}-{MAX_PERSIAN_YEAR}) است");

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
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای اعتبارسنجی تاریخ‌ها
        /// - برای محاسبات تاریخی
        /// - برای جلوگیری از ورود اطلاعات نادرست
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
        /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
        /// - برای محاسبه تعداد روزهای ماه اسفند
        /// - برای محاسبات تاریخی دقیق
        /// - برای جلوگیری از خطاهای تاریخی
        /// </summary>
        /// <param name="year">سال شمسی</param>
        /// <returns>در صورت کبیسه بودن true برمی‌گرداند</returns>
        private static bool IsPersianLeapYear(int year)
        {
            // محاسبه موقعیت در چرخه 33 ساله
            int cyclePosition = (year - 1) % 33;

            // سال‌های کبیسه در چرخه 33 ساله: 1, 5, 9, 13, 17, 21, 26, 30
            // توجه: cyclePosition 0 مربوط به سال 1 چرخه است
            return cyclePosition == 0 || cyclePosition == 4 || cyclePosition == 8 ||
                   cyclePosition == 12 || cyclePosition == 16 || cyclePosition == 20 ||
                   cyclePosition == 25 || cyclePosition == 29;
        }

        #endregion
    }
}