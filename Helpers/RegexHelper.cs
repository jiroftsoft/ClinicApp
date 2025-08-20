namespace ClinicApp.Helpers;

public static class RegexHelper
{
    public static bool IsValidServiceCode(string serviceCode)
    {
        if (string.IsNullOrWhiteSpace(serviceCode))
            return false;

        // فقط حروف، اعداد و زیرخط مجاز است
        return System.Text.RegularExpressions.Regex.IsMatch(serviceCode, "^[a-zA-Z0-9_]+$");
    }
}