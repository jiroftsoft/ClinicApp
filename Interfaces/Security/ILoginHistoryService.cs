using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Security
{
    /// <summary>
    /// سرویس مدیریت تاریخچه ورود کاربران
    /// 
    /// Single Responsibility: مدیریت ثبت و بازیابی تاریخچه ورودها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت ورودهای موفق و ناموفق
    /// 2. ثبت خروج کاربران
    /// 3. دریافت تاریخچه ورود یک کاربر
    /// 4. دریافت ورودهای اخیر
    /// 5. تشخیص فعالیت مشکوک
    /// 6. آمار ورودها
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public interface ILoginHistoryService
    {
        /// <summary>
        /// ثبت ورود موفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="sessionId">شناسه Session (optional)</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> LogLoginAsync(string userId, string ipAddress, string userAgent, string sessionId = null);

        /// <summary>
        /// ثبت ورود ناموفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر (nullable - در صورت عدم شناسایی کاربر)</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="failureReason">دلیل عدم موفقیت</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> LogFailedLoginAsync(string userId, string ipAddress, string userAgent, string failureReason);

        /// <summary>
        /// ثبت خروج کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="sessionId">شناسه Session</param>
        /// <returns>نتیجه عملیات</returns>
        Task<ServiceResult> LogLogoutAsync(string userId, string sessionId);

        /// <summary>
        /// دریافت تاریخچه ورود یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="pageNumber">شماره صفحه (1-based)</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه</param>
        /// <returns>لیست تاریخچه ورودها</returns>
        Task<ServiceResult<List<UserLoginHistory>>> GetUserLoginHistoryAsync(string userId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// دریافت ورودهای اخیر (تمام کاربران)
        /// </summary>
        /// <param name="count">تعداد رکورد</param>
        /// <returns>لیست ورودهای اخیر</returns>
        Task<ServiceResult<List<UserLoginHistory>>> GetRecentLoginsAsync(int count = 50);

        /// <summary>
        /// دریافت یک رکورد تاریخچه ورود بر اساس ID
        /// </summary>
        /// <param name="id">شناسه رکورد</param>
        /// <returns>رکورد تاریخچه ورود</returns>
        Task<ServiceResult<UserLoginHistory>> GetLoginHistoryByIdAsync(int id);

        /// <summary>
        /// تشخیص فعالیت مشکوک برای یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP فعلی</param>
        /// <param name="userAgent">User Agent فعلی</param>
        /// <returns>true اگر فعالیت مشکوک باشد</returns>
        Task<ServiceResult<bool>> IsSuspiciousActivityAsync(string userId, string ipAddress, string userAgent);

        /// <summary>
        /// دریافت آمار ورودها برای یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="days">تعداد روز گذشته (default: 30)</param>
        /// <returns>آمار ورودها</returns>
        Task<ServiceResult<LoginStatistics>> GetLoginStatisticsAsync(string userId, int days = 30);
    }

    /// <summary>
    /// آمار ورودها
    /// </summary>
    public class LoginStatistics
    {
        public int TotalLogins { get; set; }
        public int SuccessfulLogins { get; set; }
        public int FailedLogins { get; set; }
        public int UniqueIpAddresses { get; set; }
        public int UniqueDevices { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string LastLoginIpAddress { get; set; }
        public string LastLoginDevice { get; set; }
    }
}

