using System;
using System.Linq;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس کمکی برای Masking داده‌های حساس در لاگ‌ها
    /// 
    /// ✅ هدف: جلوگیری از نشت اطلاعات شخصی (PII) در فایل‌های Log
    /// 
    /// در سیستم‌های پزشکی، لاگ کردن اطلاعات زیر غیرقانونی است:
    /// - کد ملی (National Code) = SSN در ایران
    /// - شماره موبایل
    /// - شماره کارت بانکی
    /// - کد OTP
    /// 
    /// قوانین:
    /// - GDPR (اروپا)
    /// - قانون حمایت از اطلاعات شخصی (ایران)
    /// - HIPAA (آمریکا - استاندارد جهانی برای سیستم‌های پزشکی)
    /// 
    /// طبق: BEAST MODE AUDIT - Issue #4
    /// </summary>
    public static class MaskHelper
    {
        /// <summary>
        /// Mask کردن کد ملی برای لاگ
        /// ورودی: 1234567890
        /// خروجی: 1234****90
        /// 
        /// ✅ 4 رقم اول + 4 ستاره + 2 رقم آخر
        /// </summary>
        /// <param name="nationalCode">کد ملی (10 رقمی)</param>
        /// <returns>کد ملی Mask شده</returns>
        public static string MaskNationalCode(string nationalCode)
        {
            if (string.IsNullOrEmpty(nationalCode))
                return "****";

            // حذف فاصله‌ها و کاراکترهای اضافی
            nationalCode = nationalCode.Trim();

            // اگر کمتر از 4 کاراکتر باشد، کلاً Mask کن
            if (nationalCode.Length < 4)
                return "****";

            // اگر کمتر از 6 کاراکتر باشد، فقط اول را نشان بده
            if (nationalCode.Length < 6)
                return nationalCode.Substring(0, 2) + "****";

            // حالت استاندارد: 4 اول + 4 ستاره + 2 آخر
            return nationalCode.Substring(0, 4) + "****" + nationalCode.Substring(nationalCode.Length - 2);
        }

        /// <summary>
        /// Mask کردن شماره موبایل برای لاگ
        /// ورودی: 09123456789
        /// خروجی: 0912***6789
        /// 
        /// ✅ 4 رقم اول + 3 ستاره + 4 رقم آخر
        /// </summary>
        /// <param name="phoneNumber">شماره موبایل (11 رقمی)</param>
        /// <returns>شماره Mask شده</returns>
        public static string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return "****";

            // حذف فاصله‌ها و کاراکترهای اضافی
            phoneNumber = phoneNumber.Trim().Replace(" ", "").Replace("-", "");

            // اگر کمتر از 8 کاراکتر باشد، کلاً Mask کن
            if (phoneNumber.Length < 8)
                return "****";

            // حالت استاندارد: 4 اول + 3 ستاره + 4 آخر
            return phoneNumber.Substring(0, 4) + "***" + phoneNumber.Substring(phoneNumber.Length - 4);
        }

        /// <summary>
        /// Mask کردن ایمیل برای لاگ
        /// ورودی: user@example.com
        /// خروجی: us**@example.com
        /// 
        /// ✅ 2 کاراکتر اول + ** + @ + domain کامل
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <returns>ایمیل Mask شده</returns>
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return "****";

            var parts = email.Split('@');
            if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]))
                return "****";

            var localPart = parts[0];
            var domain = parts[1];

            // اگر local part کمتر از 2 کاراکتر باشد
            if (localPart.Length < 2)
                return "**@" + domain;

            // 2 کاراکتر اول + ** + domain
            return localPart.Substring(0, 2) + "**@" + domain;
        }

        /// <summary>
        /// Mask کردن شماره کارت بانکی برای لاگ
        /// ورودی: 6037997012345678
        /// خروجی: 6037****5678
        /// 
        /// ✅ 4 رقم اول + 4 ستاره + 4 رقم آخر
        /// </summary>
        /// <param name="cardNumber">شماره کارت (16 رقمی)</param>
        /// <returns>شماره کارت Mask شده</returns>
        public static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "****";

            // حذف فاصله‌ها و کاراکترهای اضافی
            cardNumber = cardNumber.Trim().Replace(" ", "").Replace("-", "");

            // اگر کمتر از 8 کاراکتر باشد، کلاً Mask کن
            if (cardNumber.Length < 8)
                return "****";

            // حالت استاندارد: 4 اول + 4 ستاره + 4 آخر
            return cardNumber.Substring(0, 4) + "****" + cardNumber.Substring(cardNumber.Length - 4);
        }

        /// <summary>
        /// Mask کردن کد OTP برای لاگ
        /// ⚠️ هرگز کد OTP را لاگ نکنید!
        /// این متد فقط برای موارد ضروری است
        /// 
        /// ورودی: 123456
        /// خروجی: ******
        /// </summary>
        /// <param name="otpCode">کد OTP</param>
        /// <returns>کامل Mask شده</returns>
        public static string MaskOtpCode(string otpCode)
        {
            // ❌ هرگز حتی بخشی از OTP را نشان نده
            return "******";
        }

        /// <summary>
        /// Mask کردن آدرس IP برای لاگ (برای Privacy بیشتر)
        /// ورودی: 192.168.1.100
        /// خروجی: 192.168.***.***
        /// 
        /// ✅ 2 بخش اول + *** برای بقیه
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>IP Mask شده</returns>
        public static string MaskIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "***";

            // برای IPv4
            if (ipAddress.Contains("."))
            {
                var parts = ipAddress.Split('.');
                if (parts.Length == 4)
                {
                    return $"{parts[0]}.{parts[1]}.***.***";
                }
            }

            // برای IPv6 یا فرمت‌های دیگر
            if (ipAddress.Contains(":"))
            {
                var parts = ipAddress.Split(':');
                if (parts.Length >= 2)
                {
                    return $"{parts[0]}:{parts[1]}:***";
                }
            }

            return "***";
        }

        /// <summary>
        /// Mask کردن نام کاربری برای لاگ
        /// ورودی: محمدرضا
        /// خروجی: محم***
        /// 
        /// ✅ 3 کاراکتر اول + ***
        /// </summary>
        /// <param name="username">نام کاربری یا نام</param>
        /// <returns>نام Mask شده</returns>
        public static string MaskUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return "***";

            username = username.Trim();

            if (username.Length <= 3)
                return "***";

            return username.Substring(0, 3) + "***";
        }

        /// <summary>
        /// Mask کردن خودکار بر اساس نوع داده
        /// این متد تشخیص می‌دهد که ورودی چه نوع داده‌ای است
        /// </summary>
        /// <param name="data">داده ورودی</param>
        /// <param name="dataType">نوع داده (اختیاری)</param>
        /// <returns>داده Mask شده</returns>
        public static string MaskAuto(string data, string dataType = null)
        {
            if (string.IsNullOrEmpty(data))
                return "****";

            // اگر نوع مشخص شده، از آن استفاده کن
            if (!string.IsNullOrEmpty(dataType))
            {
                switch (dataType.ToLower())
                {
                    case "nationalcode":
                    case "national_code":
                        return MaskNationalCode(data);
                    case "phone":
                    case "phonenumber":
                    case "mobile":
                        return MaskPhoneNumber(data);
                    case "email":
                        return MaskEmail(data);
                    case "card":
                    case "cardnumber":
                        return MaskCardNumber(data);
                    case "otp":
                        return MaskOtpCode(data);
                    case "ip":
                    case "ipaddress":
                        return MaskIpAddress(data);
                    default:
                        return MaskUsername(data);
                }
            }

            // تشخیص خودکار بر اساس الگو
            data = data.Trim();

            // کد ملی (10 رقمی)
            if (data.Length == 10 && data.All(char.IsDigit))
                return MaskNationalCode(data);

            // شماره موبایل (11 رقمی که با 09 شروع شود)
            if (data.Length == 11 && data.StartsWith("09") && data.All(char.IsDigit))
                return MaskPhoneNumber(data);

            // شماره کارت (16 رقمی)
            if (data.Length == 16 && data.All(char.IsDigit))
                return MaskCardNumber(data);

            // ایمیل
            if (data.Contains("@") && data.Contains("."))
                return MaskEmail(data);

            // IP
            if (data.Contains(".") || data.Contains(":"))
                return MaskIpAddress(data);

            // پیش‌فرض
            return MaskUsername(data);
        }
    }
}

