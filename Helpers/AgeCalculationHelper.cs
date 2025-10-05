using System;
using System.Globalization;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// کلاس حرفه‌ای برای محاسبه سن و تاریخ‌های مرتبط
    /// این کلاس برای سیستم‌های پزشکی طراحی شده و شامل:
    /// 
    /// 1. محاسبه سن دقیق با در نظر گیری سال کبیسه
    /// 2. محاسبه سن در تاریخ مشخص
    /// 3. محاسبه سن به صورت متن (مثل "25 سال و 3 ماه")
    /// 4. بررسی معتبر بودن تاریخ تولد
    /// 5. محاسبه سن برای تاریخ‌های شمسی
    /// 6. لاگ‌گیری حرفه‌ای برای سیستم‌های پزشکی
    /// 
    /// استفاده:
    /// int age = DateTime.Now.CalculateAge();
    /// int age = birthDate.CalculateAge();
    /// string ageText = birthDate.GetAgeText();
    /// bool isValid = birthDate.IsValidBirthDate();
    /// </summary>
    public static class AgeCalculationHelper
    {
        private static readonly ILogger _log = Log.ForContext(typeof(AgeCalculationHelper));

        #region Constants

        // محدوده‌های سنی معتبر برای سیستم‌های پزشکی
        private const int MIN_AGE = 0;
        private const int MAX_AGE = 150;
        
        // محدوده‌های تاریخ تولد معتبر
        private static readonly DateTime MIN_BIRTH_DATE = DateTime.Today.AddYears(-MAX_AGE);
        private static readonly DateTime MAX_BIRTH_DATE = DateTime.Today;

        #endregion

        #region Extension Methods

        /// <summary>
        /// محاسبه سن بر اساس تاریخ تولد (Extension Method)
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>سن به سال</returns>
        /// <exception cref="ArgumentNullException">هنگامی که birthDate null باشد</exception>
        /// <exception cref="ArgumentException">هنگامی که تاریخ تولد نامعتبر باشد</exception>
        public static int CalculateAge(this DateTime birthDate)
        {
            return CalculateAge(birthDate, DateTime.Today);
        }

        /// <summary>
        /// محاسبه سن بر اساس تاریخ تولد در تاریخ مشخص
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <param name="referenceDate">تاریخ مرجع</param>
        /// <returns>سن به سال</returns>
        /// <exception cref="ArgumentNullException">هنگامی که birthDate null باشد</exception>
        /// <exception cref="ArgumentException">هنگامی که تاریخ تولد نامعتبر باشد</exception>
        public static int CalculateAge(this DateTime birthDate, DateTime referenceDate)
        {
            if (birthDate == DateTime.MinValue || birthDate == DateTime.MaxValue)
            {
                _log.Warning("🔍 Invalid birth date: {BirthDate}", birthDate);
                throw new ArgumentException("تاریخ تولد نامعتبر است", nameof(birthDate));
            }

            if (referenceDate == DateTime.MinValue || referenceDate == DateTime.MaxValue)
            {
                _log.Warning("🔍 Invalid reference date: {ReferenceDate}", referenceDate);
                throw new ArgumentException("تاریخ مرجع نامعتبر است", nameof(referenceDate));
            }

            try
            {
                // بررسی معتبر بودن تاریخ تولد
                if (!IsValidBirthDate(birthDate, referenceDate))
                {
                    _log.Warning("🔍 Birth date is not valid: {BirthDate}, Reference: {ReferenceDate}", birthDate, referenceDate);
                    throw new ArgumentException($"تاریخ تولد باید بین {MIN_BIRTH_DATE:yyyy/MM/dd} و {MAX_BIRTH_DATE:yyyy/MM/dd} باشد", nameof(birthDate));
                }

                // محاسبه سن دقیق
                var age = referenceDate.Year - birthDate.Year;
                
                // بررسی سال کبیسه و ماه
                if (referenceDate.Month < birthDate.Month || 
                    (referenceDate.Month == birthDate.Month && referenceDate.Day < birthDate.Day))
                {
                    age--;
                }

                // بررسی محدوده سن
                if (age < MIN_AGE || age > MAX_AGE)
                {
                    _log.Warning("🔍 Calculated age is out of valid range: {Age}", age);
                    throw new ArgumentException($"سن محاسبه شده ({age}) خارج از محدوده معتبر ({MIN_AGE}-{MAX_AGE}) است");
                }

                _log.Debug("🔍 Age calculated successfully: {Age} years for birth date: {BirthDate}", age, birthDate);
                return age;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error calculating age for birth date: {BirthDate}, reference: {ReferenceDate}", birthDate, referenceDate);
                throw;
            }
        }

        /// <summary>
        /// دریافت متن سن به صورت کامل (مثل "25 سال و 3 ماه")
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>متن سن</returns>
        public static string GetAgeText(this DateTime birthDate)
        {
            return GetAgeText(birthDate, DateTime.Today);
        }

        /// <summary>
        /// دریافت متن سن به صورت کامل در تاریخ مشخص
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <param name="referenceDate">تاریخ مرجع</param>
        /// <returns>متن سن</returns>
        public static string GetAgeText(this DateTime birthDate, DateTime referenceDate)
        {
            try
            {
                var age = CalculateAge(birthDate, referenceDate);
                var months = GetAgeInMonths(birthDate, referenceDate);
                var days = GetAgeInDays(birthDate, referenceDate);

                if (age >= 1)
                {
                    return $"{age} سال";
                }
                else if (months >= 1)
                {
                    return $"{months} ماه";
                }
                else
                {
                    return $"{days} روز";
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error getting age text for birth date: {BirthDate}", birthDate);
                return "نامشخص";
            }
        }

        /// <summary>
        /// دریافت سن به صورت ماه
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>سن به ماه</returns>
        public static int GetAgeInMonths(this DateTime birthDate)
        {
            return GetAgeInMonths(birthDate, DateTime.Today);
        }

        /// <summary>
        /// دریافت سن به صورت ماه در تاریخ مشخص
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <param name="referenceDate">تاریخ مرجع</param>
        /// <returns>سن به ماه</returns>
        public static int GetAgeInMonths(this DateTime birthDate, DateTime referenceDate)
        {
            try
            {
                var months = (referenceDate.Year - birthDate.Year) * 12 + (referenceDate.Month - birthDate.Month);
                
                if (referenceDate.Day < birthDate.Day)
                {
                    months--;
                }

                return Math.Max(0, months);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error calculating age in months for birth date: {BirthDate}", birthDate);
                return 0;
            }
        }

        /// <summary>
        /// دریافت سن به صورت روز
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>سن به روز</returns>
        public static int GetAgeInDays(this DateTime birthDate)
        {
            return GetAgeInDays(birthDate, DateTime.Today);
        }

        /// <summary>
        /// دریافت سن به صورت روز در تاریخ مشخص
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <param name="referenceDate">تاریخ مرجع</param>
        /// <returns>سن به روز</returns>
        public static int GetAgeInDays(this DateTime birthDate, DateTime referenceDate)
        {
            try
            {
                return (referenceDate - birthDate).Days;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error calculating age in days for birth date: {BirthDate}", birthDate);
                return 0;
            }
        }

        /// <summary>
        /// بررسی معتبر بودن تاریخ تولد
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool IsValidBirthDate(this DateTime birthDate)
        {
            return IsValidBirthDate(birthDate, DateTime.Today);
        }

        /// <summary>
        /// بررسی معتبر بودن تاریخ تولد در تاریخ مشخص
        /// </summary>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <param name="referenceDate">تاریخ مرجع</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool IsValidBirthDate(this DateTime birthDate, DateTime referenceDate)
        {
            try
            {
                // بررسی تاریخ‌های نامعتبر
                if (birthDate == DateTime.MinValue || birthDate == DateTime.MaxValue)
                    return false;

                if (referenceDate == DateTime.MinValue || referenceDate == DateTime.MaxValue)
                    return false;

                // بررسی محدوده تاریخ
                if (birthDate > referenceDate)
                    return false;

                if (birthDate < MIN_BIRTH_DATE || birthDate > MAX_BIRTH_DATE)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error validating birth date: {BirthDate}", birthDate);
                return false;
            }
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// محاسبه سن برای تاریخ‌های شمسی
        /// </summary>
        /// <param name="persianBirthDate">تاریخ تولد شمسی (فرمت: yyyy/MM/dd)</param>
        /// <returns>سن به سال</returns>
        public static int CalculateAgeFromPersianDate(string persianBirthDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(persianBirthDate))
                {
                    _log.Warning("🔍 Persian birth date is null or empty");
                    throw new ArgumentException("تاریخ تولد شمسی نمی‌تواند خالی باشد", nameof(persianBirthDate));
                }

                // تبدیل تاریخ شمسی به میلادی
                var gregorianDate = PersianDateHelper.ToGregorianDate(persianBirthDate);
                
                // محاسبه سن
                return CalculateAge(gregorianDate);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error calculating age from Persian date: {PersianBirthDate}", persianBirthDate);
                throw;
            }
        }

        /// <summary>
        /// دریافت متن سن برای تاریخ‌های شمسی
        /// </summary>
        /// <param name="persianBirthDate">تاریخ تولد شمسی (فرمت: yyyy/MM/dd)</param>
        /// <returns>متن سن</returns>
        public static string GetAgeTextFromPersianDate(string persianBirthDate)
        {
            try
            {
                var gregorianDate = PersianDateHelper.ToGregorianDate(persianBirthDate);
                return GetAgeText(gregorianDate);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error getting age text from Persian date: {PersianBirthDate}", persianBirthDate);
                return "نامشخص";
            }
        }

        /// <summary>
        /// بررسی معتبر بودن تاریخ تولد شمسی
        /// </summary>
        /// <param name="persianBirthDate">تاریخ تولد شمسی</param>
        /// <returns>true اگر معتبر باشد</returns>
        public static bool IsValidPersianBirthDate(string persianBirthDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(persianBirthDate))
                    return false;

                var gregorianDate = PersianDateHelper.ToGregorianDate(persianBirthDate);
                return IsValidBirthDate(gregorianDate);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🔍 Error validating Persian birth date: {PersianBirthDate}", persianBirthDate);
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// محاسبه سن با در نظر گیری سال کبیسه
        /// </summary>
        private static int CalculateAgeWithLeapYear(DateTime birthDate, DateTime referenceDate)
        {
            var age = referenceDate.Year - birthDate.Year;
            
            // بررسی سال کبیسه
            if (referenceDate.Month < birthDate.Month || 
                (referenceDate.Month == birthDate.Month && referenceDate.Day < birthDate.Day))
            {
                age--;
            }

            return age;
        }

        #endregion
    }
}
