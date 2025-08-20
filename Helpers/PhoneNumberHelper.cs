using System.Text.RegularExpressions;

namespace ClinicApp.Helpers;

public static class PhoneNumberHelper
{
    private static readonly Regex PersianArabicDigits =
        new Regex("[\u06F0-\u06F9\u0660-\u0669]", RegexOptions.Compiled);

    private static readonly Regex NonDigitExceptPlus = new Regex(@"(?!^\+)[^\d]", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes various Iranian mobile number formats to the E.164 standard (e.g., +98912...).
    /// </summary>
    public static string NormalizeToE164(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return phoneNumber;

        // Convert Persian/Arabic digits to English
        string converted = PersianArabicDigits.Replace(phoneNumber, m =>
            ((char)(m.Value[0] - (m.Value[0] >= '\u06F0' ? '\u06F0' : '\u0660'))).ToString());

        // Remove all non-digit characters except for a leading '+'
        converted = NonDigitExceptPlus.Replace(converted, string.Empty).Trim();

        if (converted.StartsWith("0098")) converted = "+98" + converted.Substring(4);
        else if (converted.StartsWith("098")) converted = "+98" + converted.Substring(3);
        else if (converted.StartsWith("98") && !converted.StartsWith("+")) converted = "+98" + converted.Substring(2);
        else if (converted.StartsWith("0")) converted = "+98" + converted.Substring(1);
        else if (converted.Length == 10 && converted.StartsWith("9")) converted = "+98" + converted;

        return converted;
    }
}