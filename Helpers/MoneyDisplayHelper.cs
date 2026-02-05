using System;
using System.Globalization;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper مخصوص نمایش مبلغ در محیط درمانی (Enterprise).
    /// مسئولیت واحد (SRP): تبدیل و فرمت مبلغ برای نمایش به منشی — ریال به تومان، مبلغ به حروف.
    /// </summary>
    public static class MoneyDisplayHelper
    {
        /// <summary>
        /// تبدیل ریال به تومان (۱ ریال = ۰.۱ تومان؛ نمایش بدون اعشار برای مبالغ صحیح).
        /// </summary>
        public static decimal RialToToman(decimal rial)
        {
            return Math.Floor(rial / 10m);
        }

        /// <summary>
        /// نمایش مبلغ به تومان با ارقام فارسی و واحد «تومان» برای راهنمای منشی.
        /// </summary>
        /// <param name="amountRial">مبلغ به ریال</param>
        /// <returns>مثال: «۵۰,۰۰۰ تومان»</returns>
        public static string FormatTomanDisplay(decimal amountRial)
        {
            var toman = RialToToman(amountRial);
            var culture = CultureInfo.GetCultureInfo("fa-IR");
            var formatted = toman.ToString("N0", culture);
            var persianDigits = PersianNumberHelper.ToPersianNumbers(formatted);
            return $"{persianDigits} تومان";
        }

        /// <summary>
        /// مبلغ به حروف (به تومان) برای راهنمای منشی و کاهش خطای ورود.
        /// </summary>
        /// <param name="amountRial">مبلغ به ریال</param>
        /// <returns>مثال: «پنجاه هزار تومان»</returns>
        public static string AmountToWordsToman(decimal amountRial)
        {
            var toman = (long)RialToToman(amountRial);
            if (toman < 0) return "صفر تومان";
            var words = NumberToWordsFa(toman);
            return string.IsNullOrEmpty(words) ? "صفر تومان" : $"{words} تومان";
        }

        /// <summary>
        /// تبدیل عدد صحیح به حروف فارسی (۰ تا ۹۹۹٬۹۹۹٬۹۹۹).
        /// </summary>
        private static string NumberToWordsFa(long n)
        {
            if (n == 0) return "صفر";

            var parts = new System.Collections.Generic.List<string>();

            // میلیارد
            if (n >= 1_000_000_000)
            {
                parts.Add(NumberToWordsFa(n / 1_000_000_000) + " میلیارد");
                n %= 1_000_000_000;
                if (n > 0) parts.Add("و");
            }

            // میلیون
            if (n >= 1_000_000)
            {
                parts.Add(BlockToWords((int)(n / 1_000_000)) + " میلیون");
                n %= 1_000_000;
                if (n > 0) parts.Add("و");
            }

            // هزار
            if (n >= 1_000)
            {
                parts.Add(BlockToWords((int)(n / 1_000)) + " هزار");
                n %= 1_000;
                if (n > 0) parts.Add("و");
            }

            if (n > 0)
                parts.Add(BlockToWords((int)n));

            return string.Join(" ", parts);
        }

        private static string BlockToWords(int n) // 0..999
        {
            if (n == 0) return "";

            var units = new[] { "صفر", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
            var tens1 = new[] { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
            var tens2 = new[] { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
            var hundreds = new[] { "", "صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };

            var s = "";
            var h = n / 100;
            if (h > 0) s = hundreds[h];
            n %= 100;
            if (n >= 10 && n < 20)
            {
                s += (s.Length > 0 ? " و " : "") + tens1[n - 10];
                return s;
            }
            var t = n / 10;
            var u = n % 10;
            if (t > 0) s += (s.Length > 0 ? " و " : "") + tens2[t];
            if (u > 0) s += (s.Length > 0 ? " و " : "") + units[u];
            return s;
        }
    }
}
