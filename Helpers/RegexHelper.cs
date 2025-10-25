using System.Linq;
using System.Text.RegularExpressions;
using ClinicApp.Helpers;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس حرفه‌ای برای اعتبارسنجی الگوهای مختلف در سیستم‌های پزشکی
/// 
/// ویژگی‌های کلیدی:
/// 1. اعتبارسنجی کد ملی ایرانی با چک‌سام
/// 2. اعتبارسنجی شماره موبایل ایرانی
/// 3. اعتبارسنجی ایمیل
/// 4. اعتبارسنجی کدپستی
/// 5. اعتبارسنجی تاریخ شمسی
/// 6. نرمال‌سازی ارقام فارسی/عربی
/// </summary>
public static class RegexHelper
{
    #region Service Code Validation

    public static bool IsValidServiceCode(string serviceCode)
    {
        if (string.IsNullOrWhiteSpace(serviceCode))
            return false;

        // فقط حروف، اعداد و زیرخط مجاز است
        return Regex.IsMatch(serviceCode, "^[a-zA-Z0-9_]+$");
    }

    #endregion

    #region Iranian National Code Validation

    /// <summary>
    /// اعتبارسنجی کد ملی ایرانی با چک‌سام ۱۰رقمی
    /// </summary>
    public static bool IsValidNationalCode(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode) || !Regex.IsMatch(nationalCode, @"^\d{10}$"))
            return false;

        var d = nationalCode.Select(c => c - '0').ToArray();
        
        // بررسی تکراری بودن ارقام
        if (Enumerable.Range(0, 10).All(i => d[i] == d[0]))
            return false;

        // محاسبه چک‌سام
        var s = Enumerable.Range(0, 9).Sum(i => d[i] * (10 - i));
        var r = s % 11;
        
        return (r < 2 && d[9] == r) || (r >= 2 && d[9] == (11 - r));
    }

    /// <summary>
    /// نرمال‌سازی کد ملی (تبدیل ارقام فارسی/عربی به انگلیسی)
    /// </summary>
    public static string NormalizeNationalCode(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode))
            return nationalCode;

        return nationalCode
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2')
            .Replace('۳', '3').Replace('۴', '4').Replace('۵', '5')
            .Replace('۶', '6').Replace('۷', '7').Replace('۸', '8')
            .Replace('۹', '9')
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
            .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
            .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8')
            .Replace('٩', '9');
    }

    #endregion

    #region Iranian Mobile Number Validation

    /// <summary>
    /// اعتبارسنجی شماره موبایل ایرانی
    /// </summary>
    public static bool IsValidMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return false;

        // نرمال‌سازی
        var normalized = NormalizeMobile(mobile);
        
        // الگوهای موبایل ایرانی
        var patterns = new[]
        {
            @"^09\d{9}$",           // 09xxxxxxxxx
            @"^\+989\d{9}$",         // +989xxxxxxxxx
            @"^00989\d{9}$"          // 00989xxxxxxxxx
        };

        return patterns.Any(pattern => Regex.IsMatch(normalized, pattern));
    }

    /// <summary>
    /// نرمال‌سازی شماره موبایل به فرمت 09xxxxxxxxx
    /// </summary>
    public static string NormalizeMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return mobile;

        // حذف فاصله و خط تیره
        var cleaned = mobile.Replace(" ", "").Replace("-", "");
        
        // تبدیل ارقام فارسی/عربی
        cleaned = NormalizeNationalCode(cleaned);

        // تبدیل به فرمت استاندارد
        if (cleaned.StartsWith("0098"))
            return "0" + cleaned.Substring(4);
        else if (cleaned.StartsWith("+989"))
            return "0" + cleaned.Substring(4);
        else if (cleaned.StartsWith("989"))
            return "0" + cleaned.Substring(3);
        else if (cleaned.StartsWith("98"))
            return "0" + cleaned.Substring(2);
        else if (cleaned.Length == 10 && cleaned.StartsWith("9"))
            return "0" + cleaned;

        return cleaned;
    }

    /// <summary>
    /// ماسک کردن شماره موبایل برای privacy
    /// </summary>
    public static string MaskMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile) || mobile.Length < 4)
            return mobile;

        var normalized = NormalizeMobile(mobile);
        if (normalized.Length == 11 && normalized.StartsWith("09"))
            return normalized.Substring(0, 4) + "***" + normalized.Substring(7);

        return mobile;
    }

    #endregion

    #region Email Validation

    /// <summary>
    /// اعتبارسنجی ایمیل
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Postal Code Validation

    /// <summary>
    /// اعتبارسنجی کدپستی ایرانی (۵رقمی)
    /// </summary>
    public static bool IsValidPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        var normalized = NormalizeNationalCode(postalCode);
        return Regex.IsMatch(normalized, @"^\d{5}$");
    }

    #endregion

    #region Persian Date Validation

    /// <summary>
    /// اعتبارسنجی تاریخ شمسی (فرمت: 1403/01/01)
    /// </summary>
    public static bool IsValidPersianDate(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            return false;

        var normalized = NormalizeNationalCode(persianDate);
        return Regex.IsMatch(normalized, @"^\d{4}/\d{2}/\d{2}$");
    }

    /// <summary>
    /// اعتبارسنجی تاریخ شمسی با فرمت‌های مختلف
    /// </summary>
    public static bool IsValidPersianDateFlexible(string persianDate)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            return false;

        var normalized = NormalizeNationalCode(persianDate);
        var patterns = new[]
        {
            @"^\d{4}/\d{1,2}/\d{1,2}$",  // 1403/1/1
            @"^\d{4}-\d{1,2}-\d{1,2}$",  // 1403-1-1
            @"^\d{4}\d{2}\d{2}$"         // 14030101
        };

        return patterns.Any(pattern => Regex.IsMatch(normalized, pattern));
    }

    #endregion

    #region General Validation

    /// <summary>
    /// نرمال‌سازی ارقام فارسی/عربی به انگلیسی
    /// </summary>
    public static string ToEnglishDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2')
            .Replace('۳', '3').Replace('۴', '4').Replace('۵', '5')
            .Replace('۶', '6').Replace('۷', '7').Replace('۸', '8')
            .Replace('۹', '9')
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
            .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
            .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8')
            .Replace('٩', '9');
    }

    /// <summary>
    /// نرمال‌سازی ارقام انگلیسی به فارسی
    /// </summary>
    public static string ToPersianDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return input
            .Replace('0', '۰').Replace('1', '۱').Replace('2', '۲')
            .Replace('3', '۳').Replace('4', '۴').Replace('5', '۵')
            .Replace('6', '۶').Replace('7', '۷').Replace('8', '۸')
            .Replace('9', '۹');
    }

    #endregion
}