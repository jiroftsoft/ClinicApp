using System;
using System.Globalization;
using System.Web.Mvc;

namespace ClinicApp.Models.Binders
{
    /// <summary>
    /// Model Binder سفارشی برای Decimal که از InvariantCulture استفاده می‌کند
    /// این کلاس مشکل Culture در Decimal Parsing را حل می‌کند
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (value == null || string.IsNullOrEmpty(value.AttemptedValue))
            {
                return bindingContext.ModelType == typeof(decimal?) ? (decimal?)null : 0m;
            }

            var attemptedValue = value.AttemptedValue.Trim();
            
            // اگر خالی است
            if (string.IsNullOrEmpty(attemptedValue))
            {
                return bindingContext.ModelType == typeof(decimal?) ? (decimal?)null : 0m;
            }

            try
            {
                // استفاده از InvariantCulture برای Decimal Parsing
                // این باعث می‌شود که همیشه از "." به عنوان جداکننده اعشار استفاده شود
                return decimal.Parse(attemptedValue, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                // اگر Parse با InvariantCulture شکست خورد، سعی کن با CurrentCulture
                try
                {
                    return decimal.Parse(attemptedValue, CultureInfo.CurrentCulture);
                }
                catch (FormatException)
                {
                    // اگر هر دو شکست خوردند، سعی کن با Replace کردن جداکننده‌ها
                    var normalizedValue = attemptedValue
                        .Replace(",", ".")  // جایگزینی کاما با نقطه
                        .Replace("٫", ".")  // جایگزینی جداکننده فارسی با نقطه
                        .Replace("٬", "."); // جایگزینی جداکننده فارسی دیگر با نقطه

                    return decimal.Parse(normalizedValue, CultureInfo.InvariantCulture);
                }
            }
            catch (OverflowException)
            {
                // اگر مقدار خیلی بزرگ است
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, 
                    $"مقدار '{attemptedValue}' خارج از محدوده مجاز است.");
                return bindingContext.ModelType == typeof(decimal?) ? (decimal?)null : 0m;
            }
            catch (Exception ex)
            {
                // سایر خطاها
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, 
                    $"خطا در تبدیل مقدار '{attemptedValue}' به عدد اعشاری: {ex.Message}");
                return bindingContext.ModelType == typeof(decimal?) ? (decimal?)null : 0m;
            }
        }
    }
}