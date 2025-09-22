using System;
using System.Globalization;
using System.Web.Mvc;

namespace ClinicApp.Models.Binders
{
    /// <summary>
    /// Model Binder برای Decimal - حل مشکل Culture در Model Binding
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (value == null || string.IsNullOrEmpty(value.AttemptedValue))
            {
                return null;
            }

            var attemptedValue = value.AttemptedValue;
            
            // حذف کاما و تبدیل به نقطه
            attemptedValue = attemptedValue.Replace(",", "");
            
            // تلاش برای تبدیل با Culture مختلف
            if (decimal.TryParse(attemptedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            
            // تلاش با Culture فارسی
            if (decimal.TryParse(attemptedValue, NumberStyles.Any, new CultureInfo("fa-IR"), out result))
            {
                return result;
            }
            
            // تلاش با Culture انگلیسی
            if (decimal.TryParse(attemptedValue, NumberStyles.Any, new CultureInfo("en-US"), out result))
            {
                return result;
            }
            
            // در صورت عدم موفقیت، اضافه کردن خطا به ModelState
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, 
                $"The value '{attemptedValue}' is not valid for {bindingContext.ModelName}.");
            
            return null;
        }
    }
}
