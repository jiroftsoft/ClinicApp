using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// رابط قوانین کسب‌وکار - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت قوانین کسب‌وکار پویا
    /// 2. اعتبارسنجی چندمرحله‌ای
    /// 3. پشتیبانی از قوانین پیچیده
    /// 4. مدیریت خطاها و استثناها
    /// 5. یکپارچه‌سازی با سیستم امنیت
    /// </summary>
    /// <typeparam name="T">نوع مدل برای اعتبارسنجی</typeparam>
    public interface IBusinessRule<T>
    {
        /// <summary>
        /// اعتبارسنجی قانون کسب‌وکار
        /// </summary>
        /// <param name="model">مدل برای اعتبارسنجی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<ValidationResult> ValidateAsync(T model);
    }

    /// <summary>
    /// نتیجه اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// آیا اعتبارسنجی موفق بوده است؟
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// لیست خطاهای اعتبارسنجی
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// پیام کلی اعتبارسنجی
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کد خطا
        /// </summary>
        public string ErrorCode { get; set; }
    }
}
