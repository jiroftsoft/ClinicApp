using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس حرفه‌ای کمکی برای کار با اعداد فارسی و عربی در سیستم‌های پزشکی
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
/// string englishNumbers = PersianNumberHelper.ToEnglishNumbers("۱۲۳۴۵");
/// string persianNumbers = PersianNumberHelper.ToPersianNumbers("12345");
/// bool isValidNationalCode = PersianNumberHelper.IsValidNationalCode("0065831188");
/// 
/// نکته حیاتی: این کلاس برای سیستم‌های پزشکی طراحی شده و تمام نیازهای خاص را پوشش می‌دهد
/// </summary>
public static class PersianNumberHelper
{
    private static readonly ILogger _log = Log.ForContext(typeof(PersianNumberHelper));

    #region Constants and Lookup Tables

    // ارقام فارسی
    private const char FARSI_ZERO = '۰';
    private const char FARSI_ONE = '۱';
    private const char FARSI_TWO = '۲';
    private const char FARSI_THREE = '۳';
    private const char FARSI_FOUR = '۴';
    private const char FARSI_FIVE = '۵';
    private const char FARSI_SIX = '۶';
    private const char FARSI_SEVEN = '۷';
    private const char FARSI_EIGHT = '۸';
    private const char FARSI_NINE = '۹';

    // ارقام عربی
    private const char ARABIC_ZERO = '٠';
    private const char ARABIC_ONE = '١';
    private const char ARABIC_TWO = '٢';
    private const char ARABIC_THREE = '٣';
    private const char ARABIC_FOUR = '٤';
    private const char ARABIC_FIVE = '٥';
    private const char ARABIC_SIX = '٦';
    private const char ARABIC_SEVEN = '٧';
    private const char ARABIC_EIGHT = '٨';
    private const char ARABIC_NINE = '٩';

    // ارقام انگلیسی
    private const char ENGLISH_ZERO = '0';
    private const char ENGLISH_ONE = '1';
    private const char ENGLISH_TWO = '2';
    private const char ENGLISH_THREE = '3';
    private const char ENGLISH_FOUR = '4';
    private const char ENGLISH_FIVE = '5';
    private const char ENGLISH_SIX = '6';
    private const char ENGLISH_SEVEN = '7';
    private const char ENGLISH_EIGHT = '8';
    private const char ENGLISH_NINE = '9';

    // Lookup table برای تبدیل سریع‌تر
    private static readonly char[] PersianToEnglishMap = new char[65536];
    private static readonly char[] EnglishToPersianMap = new char[65536];

    static PersianNumberHelper()
    {
        // مقداردهی اولیه Lookup tables
        InitializeLookupTables();
    }

    private static void InitializeLookupTables()
    {
        // پر کردن جدول تبدیل فارسی به انگلیسی
        for (int i = 0; i < PersianToEnglishMap.Length; i++)
        {
            PersianToEnglishMap[i] = (char)i; // مقدار پیش‌فرض همان کاراکتر است
        }

        PersianToEnglishMap[FARSI_ZERO] = ENGLISH_ZERO;
        PersianToEnglishMap[FARSI_ONE] = ENGLISH_ONE;
        PersianToEnglishMap[FARSI_TWO] = ENGLISH_TWO;
        PersianToEnglishMap[FARSI_THREE] = ENGLISH_THREE;
        PersianToEnglishMap[FARSI_FOUR] = ENGLISH_FOUR;
        PersianToEnglishMap[FARSI_FIVE] = ENGLISH_FIVE;
        PersianToEnglishMap[FARSI_SIX] = ENGLISH_SIX;
        PersianToEnglishMap[FARSI_SEVEN] = ENGLISH_SEVEN;
        PersianToEnglishMap[FARSI_EIGHT] = ENGLISH_EIGHT;
        PersianToEnglishMap[FARSI_NINE] = ENGLISH_NINE;

        PersianToEnglishMap[ARABIC_ZERO] = ENGLISH_ZERO;
        PersianToEnglishMap[ARABIC_ONE] = ENGLISH_ONE;
        PersianToEnglishMap[ARABIC_TWO] = ENGLISH_TWO;
        PersianToEnglishMap[ARABIC_THREE] = ENGLISH_THREE;
        PersianToEnglishMap[ARABIC_FOUR] = ENGLISH_FOUR;
        PersianToEnglishMap[ARABIC_FIVE] = ENGLISH_FIVE;
        PersianToEnglishMap[ARABIC_SIX] = ENGLISH_SIX;
        PersianToEnglishMap[ARABIC_SEVEN] = ENGLISH_SEVEN;
        PersianToEnglishMap[ARABIC_EIGHT] = ENGLISH_EIGHT;
        PersianToEnglishMap[ARABIC_NINE] = ENGLISH_NINE;

        // پر کردن جدول تبدیل انگلیسی به فارسی
        for (int i = 0; i < EnglishToPersianMap.Length; i++)
        {
            EnglishToPersianMap[i] = (char)i; // مقدار پیش‌فرض همان کاراکتر است
        }

        EnglishToPersianMap[ENGLISH_ZERO] = FARSI_ZERO;
        EnglishToPersianMap[ENGLISH_ONE] = FARSI_ONE;
        EnglishToPersianMap[ENGLISH_TWO] = FARSI_TWO;
        EnglishToPersianMap[ENGLISH_THREE] = FARSI_THREE;
        EnglishToPersianMap[ENGLISH_FOUR] = FARSI_FOUR;
        EnglishToPersianMap[ENGLISH_FIVE] = FARSI_FIVE;
        EnglishToPersianMap[ENGLISH_SIX] = FARSI_SIX;
        EnglishToPersianMap[ENGLISH_SEVEN] = FARSI_SEVEN;
        EnglishToPersianMap[ENGLISH_EIGHT] = FARSI_EIGHT;
        EnglishToPersianMap[ENGLISH_NINE] = FARSI_NINE;
    }

    #endregion

    #region Conversion Methods (روش‌های تبدیل)

    /// <summary>
    /// تبدیل رشته حاوی اعداد فارسی یا عربی به اعداد انگلیسی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - کاربران ممکن است از صفحه‌کلید فارسی استفاده کنند
    /// - اطلاعات ممکن است از سیستم‌های قدیمی وارد شود
    /// - برای یکنواختی داده‌ها در پایگاه داده
    /// </summary>
    /// <param name="input">رشته ورودی حاوی اعداد فارسی یا عربی</param>
    /// <returns>رشته با اعداد انگلیسی</returns>
    /// <example>
    /// "شماره تلفن: ۰۹۱۲۳۴۵۶۷۸۹" → "شماره تلفن: 09123456789"
    /// </example>
    public static string ToEnglishNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.Debug("ورودی خالی یا null برای تبدیل به اعداد انگلیسی دریافت شد");
            return input;
        }

        try
        {
            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                result[i] = PersianToEnglishMap[c];
            }
            return new string(result);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در تبدیل اعداد فارسی/عربی به انگلیسی: {Input}", input);
            return input; // در صورت خطا، ورودی اصلی را برمی‌گردانیم
        }
    }

    /// <summary>
    /// تبدیل رشته حاوی اعداد انگلیسی به اعداد فارسی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش اعداد در UI به فارسی
    /// - برای گزارش‌های چاپی به فارسی
    /// - برای ارسال پیامک به فارسی
    /// </summary>
    /// <param name="input">رشته ورودی حاوی اعداد انگلیسی</param>
    /// <returns>رشته با اعداد فارسی</returns>
    /// <example>
    /// "شماره تلفن: 09123456789" → "شماره تلفن: ۰۹۱۲۳۴۵۶۷۸۹"
    /// </example>
    public static string ToPersianNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.Debug("ورودی خالی یا null برای تبدیل به اعداد فارسی دریافت شد");
            return input;
        }

        try
        {
            var result = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                result[i] = EnglishToPersianMap[c];
            }
            return new string(result);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در تبدیل اعداد انگلیسی به فارسی: {Input}", input);
            return input; // در صورت خطا، ورودی اصلی را برمی‌گردانیم
        }
    }

    /// <summary>
    /// تبدیل یک عدد انگلیسی به عدد فارسی با فرمت‌دهی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش اعداد بزرگ در UI
    /// - برای گزارش‌های مالی
    /// - برای نمایش مبالغ در سیستم‌های پزشکی
    /// </summary>
    /// <param name="number">عدد انگلیسی</param>
    /// <param name="format">فرمت عددی (پیش‌فرض: "N0")</param>
    /// <returns>رشته با اعداد فارسی و فرمت‌دهی شده</returns>
    /// <example>
    /// 1234567 → "۱,۲۳۴,۵۶۷"
    /// </example>
    public static string FormatPersianNumber(long number, string format = "N0")
    {
        try
        {
            string formatted = number.ToString(format);
            return ToPersianNumbers(formatted);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در فرمت‌دهی عدد {Number} با فرمت {Format}", number, format);
            return number.ToString();
        }
    }

    /// <summary>
    /// تبدیل یک عدد اعشاری انگلیسی به عدد فارسی با فرمت‌دهی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای نمایش اعداد بزرگ در UI
    /// - برای گزارش‌های مالی
    /// - برای نمایش مبالغ در سیستم‌های پزشکی
    /// </summary>
    /// <param name="number">عدد اعشاری انگلیسی</param>
    /// <param name="format">فرمت عددی (پیش‌فرض: "N2")</param>
    /// <returns>رشته با اعداد فارسی و فرمت‌دهی شده</returns>
    /// <example>
    /// 1234567.89 → "۱,۲۳۴,۵۶۷.۸۹"
    /// </example>
    public static string FormatPersianNumber(decimal number, string format = "N2")
    {
        try
        {
            string formatted = number.ToString(format);
            return ToPersianNumbers(formatted);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در فرمت‌دهی عدد {Number} با فرمت {Format}", number, format);
            return number.ToString();
        }
    }

    #endregion

    #region Extraction and Validation Methods (روش‌های استخراج و اعتبارسنجی)

    /// <summary>
    /// فقط اعداد از متن را استخراج کرده و به انگلیسی برمی‌گرداند
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای استخراج کد ملی از ورودی‌ها
    /// - برای استخراج شماره تلفن از ورودی‌ها
    /// - برای پردازش اطلاعات ورودی کاربر
    /// </summary>
    /// <param name="input">رشته ورودی</param>
    /// <returns>رشته حاوی فقط اعداد انگلیسی</returns>
    /// <example>
    /// "شماره ۱۲۳۴ است" → "1234"
    /// </example>
    public static string ExtractDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.Debug("ورودی خالی یا null برای استخراج اعداد دریافت شد");
            return string.Empty;
        }

        try
        {
            string englishNumbers = ToEnglishNumbers(input);
            return Regex.Replace(englishNumbers, @"[^\d]", "");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در استخراج اعداد از رشته: {Input}", input);
            return string.Empty;
        }
    }

    /// <summary>
    /// بررسی اینکه رشته فقط شامل اعداد (فارسی، عربی یا انگلیسی) باشد
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای اعتبارسنجی فیلدهای عددی
    /// - برای جلوگیری از ورود اطلاعات نادرست
    /// - برای امنیت اطلاعات پزشکی
    /// </summary>
    /// <param name="input">رشته ورودی</param>
    /// <returns>در صورت فقط شامل اعداد بودن true برمی‌گرداند</returns>
    public static bool IsNumeric(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            _log.Debug("ورودی خالی یا null برای بررسی عددی بودن دریافت شد");
            return false;
        }

        try
        {
            string englishNumbers = ToEnglishNumbers(input);
            return Regex.IsMatch(englishNumbers, @"^\d+$");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "خطا در بررسی عددی بودن رشته: {Input}", input);
            return false;
        }
    }

    /// <summary>
    /// بررسی صحت کد ملی ایران
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - کد ملی به عنوان شناسه اصلی بیماران استفاده می‌شود
    /// - برای جستجوی دقیق بیماران ضروری است
    /// - برای ارتباط با سیستم‌های ملی سلامت
    /// </summary>
    /// <param name="nationalCode">کد ملی به صورت رشته</param>
    /// <returns>در صورت معتبر بودن true برمی‌گرداند</returns>
    public static bool IsValidNationalCode(string nationalCode)
    {
        // تمام منطق اعتبارسنجی به کلاس تخصصی آن واگذار می‌شود
        return IranianNationalCodeValidator.IsValid(nationalCode);
    }

    /// <summary>
    /// بررسی صحت شماره تلفن همراه ایران
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای ارسال پیامک‌های یادآوری نوبت
    /// - برای ارسال کد امنیتی (OTP) در سیستم پسورد‌لس
    /// - برای تماس‌های ضروری با بیماران
    /// </summary>
    /// <param name="phoneNumber">شماره تلفن همراه به صورت رشته</param>
    /// <returns>در صورت معتبر بودن true برمی‌گرداند</returns>
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // First, normalize to get rid of Persian digits and non-numeric characters
        string normalized = ExtractDigits(phoneNumber);

        // ✅ Check for both valid formats
        bool isStandardFormat = normalized.StartsWith("09") && normalized.Length == 11;
        bool isInternationalFormat = normalized.StartsWith("989") && normalized.Length == 12;

        return isStandardFormat || isInternationalFormat;
    }

    /// <summary>
    /// بررسی صحت شماره پزشکی
    /// برای سیستم‌های پزشکی بسیار حیاتی است چون:
    /// - برای تأیید هویت پزشکان
    /// - برای ارتباط با سازمان نظام پزشکی
    /// - برای امنیت اطلاعات پزشکی
    /// </summary>
    /// <param name="medicalNumber">شماره پزشکی به صورت رشته</param>
    /// <returns>در صورت معتبر بودن true برمی‌گرداند</returns>
    public static bool IsValidMedicalNumber(string medicalNumber)
    {
        if (string.IsNullOrWhiteSpace(medicalNumber))
            return false;

        // نرمال‌سازی و استخراج اعداد
        medicalNumber = ExtractDigits(medicalNumber);

        // بررسی طول
        if (medicalNumber.Length != 6 && medicalNumber.Length != 8)
            return false;

        // بررسی فرمت
        return Regex.IsMatch(medicalNumber, @"^\d{6,8}$");
    }

    #endregion

    #region Extension Methods (روش‌های افزوده)

    /// <summary>
    /// تبدیل رشته حاوی اعداد فارسی یا عربی به اعداد انگلیسی
    /// برای استفاده راحت‌تر در کدهای دیگر
    /// </summary>
    public static string ToEnglish(this string input)
    {
        return ToEnglishNumbers(input);
    }

    /// <summary>
    /// تبدیل رشته حاوی اعداد انگلیسی به اعداد فارسی
    /// برای استفاده راحت‌تر در کدهای دیگر
    /// </summary>
    public static string ToPersian(this string input)
    {
        return ToPersianNumbers(input);
    }

    /// <summary>
    /// بررسی اینکه آیا رشته فقط شامل اعداد است
    /// برای استفاده راحت‌تر در کدهای دیگر
    /// </summary>
    public static bool IsNumericString(this string input)
    {
        return IsNumeric(input);
    }

    #endregion

    /// <summary>
    /// A general validation for any type of phone number (landline or mobile).
    /// Checks for a reasonable length and numeric format.
    /// </summary>
    public static bool IsValidGeneralPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return true; // Optional fields are valid if empty
        }

        // Normalize the number to only digits
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Check for a reasonable length (e.g., between 8 and 11 digits for Iranian numbers)
        return digitsOnly.Length >= 8 && digitsOnly.Length <= 11;
    }
}