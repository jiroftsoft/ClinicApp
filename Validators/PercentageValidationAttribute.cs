using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClinicApp.Validators
{
    /// <summary>
    /// اعتبارسنجی سفارشی برای درصد (0-100) با پشتیبانی از culture های مختلف
    /// </summary>
    public class PercentageValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true; // Required attribute handles null values

            if (value is decimal decimalValue)
            {
                return decimalValue >= 0 && decimalValue <= 100;
            }

            if (value is string stringValue)
            {
                // Try parsing with different culture settings
                if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result >= 0 && result <= 100;
                }

                if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal result2))
                {
                    return result2 >= 0 && result2 <= 100;
                }

                // Try with comma as decimal separator (Persian culture)
                string normalizedValue = stringValue.Replace(',', '.');
                if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result3))
                {
                    return result3 >= 0 && result3 <= 100;
                }
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"درصد پوشش باید عددی بین 0 تا 100 باشد (حداکثر 2 رقم اعشار)";
        }
    }
}
