using ClinicApp.Helpers;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// Interface برای سرویس مدیریت و تست توکن امنیتی KeyA
    /// این سرویس برای بررسی ارتباط با توکن سخت‌افزاری طراحی شده است
    /// </summary>
    public interface ISecurityTokenService
    {
        /// <summary>
        /// بررسی وجود توکن امنیتی در سیستم
        /// </summary>
        /// <param name="tokenId">شناسه توکن (پیش‌فرض: TR127256)</param>
        /// <returns>وضعیت توکن</returns>
        Task<ServiceResult<object>> CheckTokenPresenceAsync(string tokenId = "TR127256");

        /// <summary>
        /// تست ارتباط با توکن امنیتی
        /// </summary>
        /// <param name="tokenId">شناسه توکن (پیش‌فرض: TR127256)</param>
        /// <returns>نتیجه تست</returns>
        Task<ServiceResult<object>> TestTokenConnectionAsync(string tokenId = "TR127256");
    }
}
