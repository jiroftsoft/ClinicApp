using System;
using System.Globalization;
using System.Web.Mvc;

namespace ClinicApp.Models.Binders
{
    /// <summary>
    /// Model Binder سفارشی برای TimeSpan که از فرمت HH:mm (24-hour) استفاده می‌کند
    /// این کلاس مشکل Model Binding برای input type="time" را حل می‌کند
    /// </summary>
    public class TimeSpanModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            
            if (value == null || string.IsNullOrEmpty(value.AttemptedValue))
            {
                return bindingContext.ModelType == typeof(TimeSpan?) ? (TimeSpan?)null : TimeSpan.Zero;
            }

            var attemptedValue = value.AttemptedValue.Trim();
            
            // اگر خالی است
            if (string.IsNullOrEmpty(attemptedValue))
            {
                return bindingContext.ModelType == typeof(TimeSpan?) ? (TimeSpan?)null : TimeSpan.Zero;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] 🔍 شروع Parse برای: '{attemptedValue}' (ModelName: {bindingContext.ModelName})");
                
                // ✅ input type="time" همیشه فرمت HH:mm را برمی‌گرداند (24-hour format)
                // مثلاً "12:30" برای 12:30 PM و "00:30" برای 12:30 AM
                // اما TimeSpan.TryParseExact با "HH:mm" ممکن است کار نکند
                // بنابراین ابتدا سعی می‌کنیم با parse دستی
                var parts = attemptedValue.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
                {
                    // ✅ اعتبارسنجی hours و minutes
                    if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                    {
                        var manualResult = new TimeSpan(hours, minutes, 0);
                        System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ✅ Parse موفق (manual HH:mm): '{attemptedValue}' -> {manualResult}");
                        return manualResult;
                    }
                }
                
                // ✅ اگر parse دستی کار نکرد، سعی می‌کنیم با TimeSpan.TryParseExact
                TimeSpan result;
                if (TimeSpan.TryParseExact(attemptedValue, "HH:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ✅ Parse موفق (HH:mm): '{attemptedValue}' -> {result}");
                    return result;
                }
                
                // ✅ اگر فرمت HH:mm کار نکرد، سعی می‌کنیم با فرمت استاندارد parse کنیم
                if (TimeSpan.TryParse(attemptedValue, CultureInfo.InvariantCulture, out result))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ✅ Parse موفق (استاندارد): '{attemptedValue}' -> {result}");
                    return result;
                }
                
                // ✅ اگر هیچکدام کار نکرد، سعی می‌کنیم با فرمت hh:mm tt (12-hour format) parse کنیم
                if (DateTime.TryParseExact(attemptedValue, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    result = dateTime.TimeOfDay;
                    System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ✅ Parse موفق (12-hour): '{attemptedValue}' -> {result}");
                    return result;
                }
                
                System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ❌ Parse ناموفق: '{attemptedValue}'");
                return bindingContext.ModelType == typeof(TimeSpan?) ? (TimeSpan?)null : TimeSpan.Zero;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimeSpanModelBinder] ❌ Exception در Parse: '{attemptedValue}' - {ex.Message}");
                return bindingContext.ModelType == typeof(TimeSpan?) ? (TimeSpan?)null : TimeSpan.Zero;
            }
        }
    }
}

