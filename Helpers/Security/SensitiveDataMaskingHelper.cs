using System;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers.Security
{
    /// <summary>
    /// 🔒 Helper برای ماسک کردن داده‌های حساس طبق قرارداد 04-Security-Requirements
    /// 
    /// این کلاس روش‌های امن برای نمایش داده‌های حساس در لاگ‌ها و UI را فراهم می‌کند
    /// </summary>
    public static class SensitiveDataMaskingHelper
    {
        /// <summary>
        /// ماسک کردن کد ملی ایرانی (10 رقم)
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>کد ملی ماسک شده (مثال: 123***7890)</returns>
        public static string MaskNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode))
                return nationalCode;

            // حذف کاراکترهای غیرعددی
            var cleaned = Regex.Replace(nationalCode, @"\D", "");

            if (cleaned.Length != 10)
                return "***";

            // نمایش 3 رقم اول و 4 رقم آخر
            return $"{cleaned.Substring(0, 3)}***{cleaned.Substring(6)}";
        }

        /// <summary>
        /// ماسک کردن شماره موبایل ایرانی (11 رقم)
        /// </summary>
        /// <param name="mobile">شماره موبایل</param>
        /// <returns>شماره موبایل ماسک شده (مثال: 0912***7890)</returns>
        public static string MaskMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return mobile;

            // حذف کاراکترهای غیرعددی
            var cleaned = Regex.Replace(mobile, @"\D", "");

            // تبدیل فرمت‌های مختلف به فرمت استاندارد
            if (cleaned.StartsWith("0098"))
                cleaned = "0" + cleaned.Substring(4);
            else if (cleaned.StartsWith("+989"))
                cleaned = "0" + cleaned.Substring(4);
            else if (cleaned.StartsWith("989"))
                cleaned = "0" + cleaned.Substring(3);
            else if (cleaned.StartsWith("98"))
                cleaned = "0" + cleaned.Substring(2);
            else if (cleaned.Length == 10 && cleaned.StartsWith("9"))
                cleaned = "0" + cleaned;

            if (cleaned.Length != 11 || !cleaned.StartsWith("09"))
                return "***";

            // نمایش 4 رقم اول و 4 رقم آخر
            return $"{cleaned.Substring(0, 4)}***{cleaned.Substring(7)}";
        }

        /// <summary>
        /// ماسک کردن شماره کارت بانکی (16 رقم)
        /// </summary>
        /// <param name="cardNumber">شماره کارت</param>
        /// <returns>شماره کارت ماسک شده (مثال: 6037********1234)</returns>
        public static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return cardNumber;

            // حذف فاصله و خط تیره
            var cleaned = Regex.Replace(cardNumber, @"[\s-]", "");

            if (cleaned.Length != 16)
                return "****************";

            // نمایش 4 رقم اول و 4 رقم آخر
            return $"{cleaned.Substring(0, 4)}********{cleaned.Substring(12)}";
        }

        /// <summary>
        /// ماسک کردن شماره حساب بانکی
        /// </summary>
        /// <param name="accountNumber">شماره حساب</param>
        /// <returns>شماره حساب ماسک شده</returns>
        public static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return accountNumber;

            var cleaned = Regex.Replace(accountNumber, @"\D", "");

            if (cleaned.Length < 8)
                return "***";

            // نمایش 3 رقم اول و 3 رقم آخر
            var start = cleaned.Substring(0, Math.Min(3, cleaned.Length));
            var end = cleaned.Length >= 3 ? cleaned.Substring(cleaned.Length - 3) : "";
            var maskedLength = cleaned.Length - start.Length - end.Length;

            return $"{start}{new string('*', maskedLength)}{end}";
        }

        /// <summary>
        /// ماسک کردن ایمیل (نمایش دامنه کامل، ماسک کردن نام کاربری)
        /// </summary>
        /// <param name="email">ایمیل</param>
        /// <returns>ایمیل ماسک شده (مثال: u***@example.com)</returns>
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 1)
                return $"*@{domain}";

            var maskedUsername = $"{username[0]}{new string('*', username.Length - 1)}";
            return $"{maskedUsername}@{domain}";
        }

        /// <summary>
        /// ماسک کردن متن دلخواه با حفظ طول
        /// </summary>
        /// <param name="text">متن</param>
        /// <param name="visibleStartChars">تعداد کاراکترهای قابل مشاهده در ابتدا</param>
        /// <param name="visibleEndChars">تعداد کاراکترهای قابل مشاهده در انتها</param>
        /// <returns>متن ماسک شده</returns>
        public static string MaskText(string text, int visibleStartChars = 3, int visibleEndChars = 4)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length <= visibleStartChars + visibleEndChars)
                return new string('*', text.Length);

            var start = text.Substring(0, visibleStartChars);
            var end = text.Substring(text.Length - visibleEndChars);
            var maskedLength = text.Length - visibleStartChars - visibleEndChars;

            return $"{start}{new string('*', maskedLength)}{end}";
        }
    }
}

