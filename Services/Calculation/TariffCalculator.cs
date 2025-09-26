using System;

namespace ClinicApp.Services.Calculation
{
    /// <summary>
    /// ماشین حساب تعرفه بیمه - منبع حقیقت برای تمام محاسبات
    /// استراتژی رندینگ استاندارد: MidpointRounding.AwayFromZero
    /// </summary>
    public static class TariffCalculator
    {
        /// <summary>
        /// استراتژی رندینگ استاندارد برای تمام محاسبات
        /// </summary>
        public const MidpointRounding ROUNDING_STRATEGY = MidpointRounding.AwayFromZero;

        /// <summary>
        /// تعداد ارقام اعشار برای مقادیر پولی (تومان)
        /// </summary>
        public const int CURRENCY_DECIMAL_PLACES = 0;

        /// <summary>
        /// تعداد ارقام اعشار برای درصدها
        /// </summary>
        public const int PERCENTAGE_DECIMAL_PLACES = 2;

        /// <summary>
        /// رند کردن مقدار پولی به تومان (بدون اعشار)
        /// </summary>
        /// <param name="value">مقدار ورودی</param>
        /// <returns>مقدار رند شده</returns>
        public static decimal RoundCurrency(decimal value)
        {
            return Math.Round(value, CURRENCY_DECIMAL_PLACES, ROUNDING_STRATEGY);
        }

        /// <summary>
        /// رند کردن درصد به 2 رقم اعشار
        /// </summary>
        /// <param name="value">مقدار ورودی</param>
        /// <returns>مقدار رند شده</returns>
        public static decimal RoundPercentage(decimal value)
        {
            return Math.Round(value, PERCENTAGE_DECIMAL_PLACES, ROUNDING_STRATEGY);
        }

        /// <summary>
        /// محاسبه سهم بیمار بر اساس درصد
        /// </summary>
        /// <param name="tariffPrice">قیمت تعرفه</param>
        /// <param name="patientSharePercent">درصد سهم بیمار</param>
        /// <returns>سهم بیمار (رند شده)</returns>
        public static decimal CalculatePatientShare(decimal tariffPrice, decimal patientSharePercent)
        {
            if (tariffPrice <= 0)
                return 0;

            var share = tariffPrice * (patientSharePercent / 100m);
            return RoundCurrency(share);
        }

        /// <summary>
        /// محاسبه سهم بیمه بر اساس درصد
        /// </summary>
        /// <param name="tariffPrice">قیمت تعرفه</param>
        /// <param name="insurerSharePercent">درصد سهم بیمه</param>
        /// <returns>سهم بیمه (رند شده)</returns>
        public static decimal CalculateInsurerShare(decimal tariffPrice, decimal insurerSharePercent)
        {
            if (tariffPrice <= 0)
                return 0;

            var share = tariffPrice * (insurerSharePercent / 100m);
            return RoundCurrency(share);
        }

        /// <summary>
        /// محاسبه درصد سهم بیمار
        /// </summary>
        /// <param name="tariffPrice">قیمت تعرفه</param>
        /// <param name="patientShare">سهم بیمار</param>
        /// <returns>درصد سهم بیمار (رند شده)</returns>
        public static decimal CalculatePatientSharePercent(decimal tariffPrice, decimal patientShare)
        {
            if (tariffPrice <= 0)
                return 0;

            var percent = (patientShare / tariffPrice) * 100m;
            return RoundPercentage(percent);
        }

        /// <summary>
        /// محاسبه درصد سهم بیمه
        /// </summary>
        /// <param name="tariffPrice">قیمت تعرفه</param>
        /// <param name="insurerShare">سهم بیمه</param>
        /// <returns>درصد سهم بیمه (رند شده)</returns>
        public static decimal CalculateInsurerSharePercent(decimal tariffPrice, decimal insurerShare)
        {
            if (tariffPrice <= 0)
                return 0;

            var percent = (insurerShare / tariffPrice) * 100m;
            return RoundPercentage(percent);
        }

        /// <summary>
        /// اعتبارسنجی جمع درصدها
        /// </summary>
        /// <param name="patientSharePercent">درصد سهم بیمار</param>
        /// <param name="insurerSharePercent">درصد سهم بیمه</param>
        /// <returns>true اگر جمع ≤ 100 باشد</returns>
        public static bool IsValidPercentageSum(decimal patientSharePercent, decimal insurerSharePercent)
        {
            return (patientSharePercent + insurerSharePercent) <= 100m;
        }

        /// <summary>
        /// محاسبه کامل تعرفه با اعتبارسنجی
        /// </summary>
        /// <param name="tariffPrice">قیمت تعرفه</param>
        /// <param name="patientSharePercent">درصد سهم بیمار</param>
        /// <param name="insurerSharePercent">درصد سهم بیمه</param>
        /// <returns>نتیجه محاسبه</returns>
        public static TariffCalculationResult CalculateTariff(decimal tariffPrice, decimal? patientSharePercent = null, decimal? insurerSharePercent = null)
        {
            var result = new TariffCalculationResult
            {
                TariffPrice = RoundCurrency(tariffPrice),
                IsValid = true
            };

            // اگر درصدها ارائه شده باشند
            if (patientSharePercent.HasValue && insurerSharePercent.HasValue)
            {
                // اعتبارسنجی جمع درصدها
                if (!IsValidPercentageSum(patientSharePercent.Value, insurerSharePercent.Value))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "مجموع درصدها نمی‌تواند بیش از 100 باشد.";
                    return result;
                }

                // محاسبه سهم‌ها بر اساس درصدها
                result.PatientShare = CalculatePatientShare(tariffPrice, patientSharePercent.Value);
                result.InsurerShare = CalculateInsurerShare(tariffPrice, insurerSharePercent.Value);
                result.PatientSharePercent = patientSharePercent.Value;
                result.InsurerSharePercent = insurerSharePercent.Value;
            }
            else
            {
                // محاسبه درصدها بر اساس سهم‌های موجود
                result.PatientSharePercent = CalculatePatientSharePercent(tariffPrice, 0);
                result.InsurerSharePercent = CalculateInsurerSharePercent(tariffPrice, 0);
            }

            return result;
        }
    }

    /// <summary>
    /// نتیجه محاسبه تعرفه
    /// </summary>
    public class TariffCalculationResult
    {
        public decimal TariffPrice { get; set; }
        public decimal PatientShare { get; set; }
        public decimal InsurerShare { get; set; }
        public decimal PatientSharePercent { get; set; }
        public decimal InsurerSharePercent { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
