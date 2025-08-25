using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس احراز هویت برای سیستم‌های پزشکی ایرانی
    /// این اینترفیس بر اساس استانداردهای سیستم‌های پزشکی ایران طراحی شده و:
    /// 
    /// 1. کاملاً سازگار با سیستم پسورد‌لس و OTP
    /// 2. پشتیبانی کامل از کد ملی در فیلد NationalCode
    /// 3. رعایت اصول امنیتی سیستم‌های پزشکی
    /// 4. قابلیت تست‌پذیری بالا
    /// 5. مدیریت خطاها و لاگ‌گیری حرفه‌ای
    /// 6. پشتیبانی از سیستم حذف نرم و ردیابی
    /// 
    /// نکته حیاتی: در این سیستم، کد ملی در فیلد NationalCode ذخیره می‌شود
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// ارسال کد تأیید (OTP) برای کد ملی مشخص شده.
        /// در سیستم‌های پزشکی ایرانی، کد ملی به عنوان شناسه اصلی استفاده می‌شود.
        /// </summary>
        /// <param name="nationalCode">کد ملی کاربر (به فرمت فارسی یا انگلیسی)</param>
        /// <returns>نتیجه ارسال کد تأیید</returns>
        Task<ServiceResult> SendLoginOtpAsync(string nationalCode);

        /// <summary>
        /// تأیید کد OTP ورود و انجام فرآیند ورود یا هدایت به ثبت‌نام.
        /// </summary>
        /// <param name="nationalCode">کد ملی کاربر (به فرمت فارسی یا انگلیسی)</param>
        /// <param name="otpCode">کد تأیید وارد شده توسط کاربر</param>
        /// <returns>نتیجه تأیید و ورود</returns>
        Task<ServiceResult> VerifyLoginOtpAndSignInAsync(string nationalCode, string otpCode);

        /// <summary>
        /// ورود با استفاده از کد ملی (برای کاربران جدید پس از ثبت‌نام)
        /// </summary>
        /// <param name="nationalCode">کد ملی کاربر</param>
        /// <returns>نتیجه ورود</returns>
        Task<ServiceResult> SignInWithNationalCodeAsync(string nationalCode);

        /// <summary>
        /// خروج کاربر از سیستم.
        /// </summary>
        void SignOut();

        /// <summary>
        /// بررسی وضعیت ورود کاربر.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// دریافت شناسه کاربر جاری.
        /// </summary>
        string GetCurrentUserId();

        // ✅ NEW: Checks if a user exists and is active.
        Task<ServiceResult> CheckUserExistsAsync(string nationalCode);

        // ✅ NEW: Sends an OTP specifically for a registration flow to a new number.
        Task<ServiceResult> SendRegistrationOtpAsync(string nationalCode, string phoneNumber);

        // ✅ NEW: Verifies the registration OTP.
        Task<ServiceResult> VerifyRegistrationOtpAsync(string nationalCode, string phoneNumber, string otpCode);
    }
}