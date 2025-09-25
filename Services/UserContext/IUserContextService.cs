using System;
using System.Threading.Tasks;

namespace ClinicApp.Services.UserContext
{
    /// <summary>
    /// سرویس مدیریت Context کاربر و کلینیک
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>
        /// دریافت شناسه کلینیک فعلی کاربر
        /// </summary>
        /// <returns>شناسه کلینیک</returns>
        Task<int> GetCurrentClinicIdAsync();

        /// <summary>
        /// دریافت شناسه کاربر فعلی
        /// </summary>
        /// <returns>شناسه کاربر</returns>
        Task<int> GetCurrentUserIdAsync();

        /// <summary>
        /// دریافت نام کاربر فعلی
        /// </summary>
        /// <returns>نام کاربر</returns>
        Task<string> GetCurrentUserNameAsync();

        /// <summary>
        /// بررسی دسترسی کاربر به کلینیک مشخص
        /// </summary>
        /// <param name="clinicId">شناسه کلینیک</param>
        /// <returns>نتیجه بررسی دسترسی</returns>
        Task<bool> HasAccessToClinicAsync(int clinicId);

        /// <summary>
        /// دریافت اطلاعات کامل Context کاربر
        /// </summary>
        /// <returns>اطلاعات Context</returns>
        Task<UserContextInfo> GetUserContextAsync();
    }

    /// <summary>
    /// اطلاعات Context کاربر
    /// </summary>
    public class UserContextInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string[] Roles { get; set; }
        public DateTime LoginTime { get; set; }
        public string SessionId { get; set; }
    }
}
