using System.Threading.Tasks;
using ClinicApp.Helpers;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای سرویس مدیریت لایک‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface IBlogPostLikeService
    {
        Task<ServiceResult<bool>> ToggleLikeAsync(int blogPostId, string userId = null, string guestIdentifier = null, string ipAddress = null, string userAgent = null);
        Task<ServiceResult<int>> GetLikeCountAsync(int blogPostId);
        Task<ServiceResult<bool>> HasUserLikedAsync(int blogPostId, string userId = null, string guestIdentifier = null);
        Task<ServiceResult<bool>> UnlikeAsync(int blogPostId, string userId = null, string guestIdentifier = null);
    }
}

