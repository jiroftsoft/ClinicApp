using System.Linq;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers.Validation
{
    public static class IdentityValidators
    {
        public static bool IsValidNationalCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            var code = Regex.Replace(input, @"\D", "");
            if (code.Length != 10) return false;
            if (new[] { "0000000000","1111111111","2222222222","3333333333","4444444444",
                        "5555555555","6666666666","7777777777","8888888888","9999999999" }.Contains(code))
                return false;
            var check = int.Parse(code[9].ToString());
            var sum = Enumerable.Range(0, 9).Sum(i => int.Parse(code[i].ToString()) * (10 - i)) % 11;
            return (sum < 2 && check == sum) || (sum >= 2 && check + sum == 11);
        }

        public static string NormalizeMobile(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var digits = Regex.Replace(input, @"\D", "");
            // به 09xxxxxxxxx تبدیل کن
            if (digits.StartsWith("0098")) digits = "0" + digits.Substring(4);
            if (digits.StartsWith("98")) digits = "0" + digits.Substring(2);
            if (!digits.StartsWith("0")) digits = "0" + digits;
            if (digits.Length != 11) return null;
            return digits;
        }
    }
}
