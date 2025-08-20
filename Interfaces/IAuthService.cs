using ClinicApp.Services;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{



    /// <summary>
    /// اینترفیس سرویس احراز هویت برای مدیریت فرآیندهای ورود و ثبت‌نام.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// یک کد تایید (OTP) برای شماره موبایل مشخص شده ارسال می‌کند.
        /// </summary>
        /// <param name="phoneNumber">شماره موبایل مقصد.</param>
        /// <returns>True در صورت ارسال موفق، در غیر این صورت False.</returns>
        Task<bool> SendLoginOtpAsync(string phoneNumber);

        /// <summary>
        /// کد OTP وارد شده توسط کاربر را تایید می‌کند.
        /// اگر کاربر وجود داشته باشد، او را وارد سیستم می‌کند.
        /// </summary>
        /// <param name="phoneNumber">شماره موبایلی که کد برای آن ارسال شده.</param>
        /// <param name="otpCode">کد تایید وارد شده توسط کاربر.</param>
        /// <returns>
        /// <see cref="SignInStatus.Success"/>: اگر کاربر با موفقیت وارد شد.
        /// <see cref="SignInStatus.RequiresVerification"/>: اگر کد صحیح است ولی کاربر جدید است و باید ثبت‌نام کند.
        /// <see cref="SignInStatus.Failure"/>: اگر کد نامعتبر یا منقضی شده است.
        /// </returns>
        Task<SignInStatus> VerifyLoginOtpAndSignInAsync(string phoneNumber, string otpCode);
    }
}

