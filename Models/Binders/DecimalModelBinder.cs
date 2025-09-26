using System;
using System.Globalization;
using System.Web.Mvc;

namespace ClinicApp.Models.Binders
{
    /// <summary>
    /// ModelBinder برای تمیزسازی و استانداردسازی اعداد decimal
    /// سازگار با فرهنگ‌های مختلف و اعداد فارسی
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (valueResult == null || string.IsNullOrEmpty(valueResult.AttemptedValue))
            {
                return null;
            }

            var attemptedValue = valueResult.AttemptedValue;
            
            // 🔍 MEDICAL: Clean the value for cross-culture compatibility
            var cleanValue = CleanNumericValue(attemptedValue);
            
            if (decimal.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            // If parsing fails, add model error
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, 
                $"مقدار '{attemptedValue}' یک عدد معتبر نیست.");
            
            return null;
        }

        /// <summary>
        /// تمیزسازی مقدار عددی برای سازگاری با فرهنگ‌های مختلف
        /// </summary>
        private string CleanNumericValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // تبدیل اعداد فارسی به انگلیسی
            var persianToEnglish = value
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
                .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
                .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
                .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');

            // حذف کاما و فاصله (separators)
            var withoutSeparators = persianToEnglish.Replace(",", "").Replace(" ", "");

            // اگر جداکننده اعشار / است، آن را به . تبدیل کن
            var withDotDecimal = withoutSeparators.Replace("/", ".");

            return withDotDecimal;
        }
    }

    /// <summary>
    /// ModelBinder برای nullable decimal
    /// </summary>
    public class NullableDecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (valueResult == null || string.IsNullOrEmpty(valueResult.AttemptedValue))
            {
                return null;
            }

            var attemptedValue = valueResult.AttemptedValue;
            
            // 🔍 MEDICAL: Clean the value for cross-culture compatibility
            var cleanValue = CleanNumericValue(attemptedValue);
            
            if (decimal.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            // If parsing fails, add model error
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, 
                $"مقدار '{attemptedValue}' یک عدد معتبر نیست.");
            
            return null;
        }

        /// <summary>
        /// تمیزسازی مقدار عددی برای سازگاری با فرهنگ‌های مختلف
        /// </summary>
        private string CleanNumericValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // تبدیل اعداد فارسی به انگلیسی
            var persianToEnglish = value
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
                .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
                .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
                .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');

            // حذف کاما و فاصله (separators)
            var withoutSeparators = persianToEnglish.Replace(",", "").Replace(" ", "");

            // اگر جداکننده اعشار / است، آن را به . تبدیل کن
            var withDotDecimal = withoutSeparators.Replace("/", ".");

            return withDotDecimal;
        }
    }
}