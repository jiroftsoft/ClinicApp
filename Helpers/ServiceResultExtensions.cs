using System;
using System.Configuration;
using System.Web;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// ✅ گام 8 - Extensions برای ServiceResult: افزودن جزئیات Debug (فقط در Dev)
    /// </summary>
    public static class ServiceResultExtensions
    {
        /// <summary>
        /// افزودن جزئیات Exception به ServiceResult (فقط در Development)
        /// در Production هرگز جزئیات Exception به کلاینت افشا نمی‌شود
        /// </summary>
        public static ServiceResult WithExceptionDev(this ServiceResult result, Exception ex)
        {
            if (result == null) return null;
            if (ex == null) return result;

            if (IsDevelopment())
            {
                // Metadata امن برای Dev
                result.Metadata["Exception"] = ex.Message;
                result.Metadata["Source"] = ex.Source;
                result.Metadata["StackTrace"] = ex.StackTrace;
                
                // لاگ جزئیات برای Dev
                Log.Debug(ex, "Exception details added to ServiceResult - Code: {Code}, Message: {Message}", 
                    result.Code, result.Message);
            }

            return result;
        }

        /// <summary>
        /// ✅ افزودن Code به ServiceResult (استفاده از Metadata + Reflection)
        /// </summary>
        public static ServiceResult WithCode(this ServiceResult result, string code)
        {
            if (result == null) return result;
            // ذخیره Code در Metadata
            result.Metadata["Code"] = code;
            // سعی کن Code property را set کن (با Reflection)
            try
            {
                var prop = typeof(ServiceResult).GetProperty("Code", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(result, code, null);
                }
            }
            catch
            {
                // اگر Reflection خطا داد، فقط Metadata استفاده می‌شود
            }
            return result;
        }

        /// <summary>
        /// ✅ افزودن Code به ServiceResult&lt;T&gt; (استفاده از Metadata + Reflection)
        /// </summary>
        public static ServiceResult<T> WithCode<T>(this ServiceResult<T> result, string code)
        {
            if (result == null) return result;
            // ذخیره Code در Metadata
            result.Metadata["Code"] = code;
            // سعی کن Code property را set کن (با Reflection)
            try
            {
                var prop = typeof(ServiceResult<T>).GetProperty("Code", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(result, code, null);
                }
            }
            catch
            {
                // اگر Reflection خطا داد، فقط Metadata استفاده می‌شود
            }
            return result;
        }

        /// <summary>
        /// Overload برای ServiceResult&lt;T&gt;
        /// </summary>
        public static ServiceResult<T> WithExceptionDev<T>(this ServiceResult<T> result, Exception ex)
        {
            if (result == null) return null;
            if (ex == null) return result;

            if (IsDevelopment())
            {
                // Metadata امن برای Dev
                result.Metadata["Exception"] = ex.Message;
                result.Metadata["Source"] = ex.Source;
                result.Metadata["StackTrace"] = ex.StackTrace;
                
                // لاگ جزئیات برای Dev
                Log.Debug(ex, "Exception details added to ServiceResult<T> - Code: {Code}, Message: {Message}", 
                    result.Code, result.Message);
            }

            return result;
        }

        /// <summary>
        /// بررسی اینکه آیا در محیط Development هستیم یا نه
        /// </summary>
        private static bool IsDevelopment()
        {
            try
            {
                // اولویت با appSettings: Environment=Development/Production/...
                var env = ConfigurationManager.AppSettings["Environment"];
                if (!string.IsNullOrWhiteSpace(env) && env.Equals("Development", StringComparison.OrdinalIgnoreCase))
                    return true;

#if DEBUG
                return true;
#else
                // خط دفاعی: اگر debugger فعال است
                if (HttpContext.Current != null && HttpContext.Current.IsDebuggingEnabled)
                    return true;
                
                return false;
#endif
            }
            catch
            {
                // در صورت خطا، امن‌تر است که Production فرض کنیم
                return false;
            }
        }
    }
}

