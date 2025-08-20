    using System.Text;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers;

/// <summary>
/// کلاس کمکی برای کار با اعداد فارسی و عربی.
/// </summary>
public static class PersianNumberHelper
{
    /// <summary>
    /// تبدیل رشته حاوی اعداد فارسی یا عربی به اعداد انگلیسی.
    /// مثال: "۱۲۳۴۵" → "12345"
    /// </summary>
    public static string ToEnglishNumbers(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                double digit = char.GetNumericValue(c);
                sb.Append((int)digit);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// فقط اعداد از متن را استخراج کرده و به انگلیسی برمی‌گرداند.
    /// مثال: "شماره ۱۲۳۴ است" → "1234"
    /// </summary>
    public static string ExtractDigits(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(ToEnglishNumbers(input), @"[^\d]", "");
    }

    /// <summary>
    /// بررسی اینکه رشته فقط شامل اعداد (فارسی، عربی یا انگلیسی) باشد.
    /// </summary>
    public static bool IsNumeric(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        return Regex.IsMatch(ToEnglishNumbers(input), @"^\d+$");
    }
}