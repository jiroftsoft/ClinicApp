using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers;

public static class IranianNationalCodeValidator
{
    /// <summary>
    /// بررسی صحت کد ملی ایران
    /// </summary>
    /// <param name="nationalCode">کد ملی به صورت رشته</param>
    /// <returns>True اگر معتبر باشد</returns>
    public static bool IsValid(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode))
            return false;

        // نرمال‌سازی اعداد فارسی به انگلیسی
        nationalCode = NormalizeDigits(nationalCode).Trim();

        // باید دقیقا ۱۰ رقم باشد
        if (!Regex.IsMatch(nationalCode, @"^\d{10}$"))
            return false;

        // جلوگیری از کدهای تکراری مثل 1111111111
        var invalidCodes = new[]
        {
            "0000000000", "1111111111", "2222222222", "3333333333",
            "4444444444", "5555555555", "6666666666", "7777777777",
            "8888888888", "9999999999"
        };
        if (invalidCodes.Contains(nationalCode))
            return false;

        var digits = nationalCode.Select(c => c - '0').ToArray();
        var checkDigit = digits[9];
        var sum = 0;

        for (int i = 0; i < 9; i++)
            sum += digits[i] * (10 - i);

        var remainder = sum % 11;
        var expected = remainder < 2 ? remainder : 11 - remainder;

        return checkDigit == expected;
    }

    /// <summary>
    /// تبدیل اعداد فارسی/عربی به انگلیسی
    /// </summary>
    private static string NormalizeDigits(string input)
    {
        var persianDigits = new[] { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
        var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };

        for (int i = 0; i < 10; i++)
        {
            input = input.Replace(persianDigits[i], (char)('0' + i));
            input = input.Replace(arabicDigits[i], (char)('0' + i));
        }
        return input;
    }

  
}
