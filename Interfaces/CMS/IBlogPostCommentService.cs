using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای سرویس مدیریت کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface IBlogPostCommentService
    {
        Task<ServiceResult<BlogPostCommentViewModel>> CreateCommentAsync(BlogPostCommentCreateViewModel model);
        Task<ServiceResult<BlogPostCommentViewModel>> GetCommentByIdAsync(int commentId);
        Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetCommentsByBlogPostIdAsync(int blogPostId, int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<bool>> ApproveCommentAsync(int commentId);
        Task<ServiceResult<bool>> RejectCommentAsync(int commentId);
        Task<ServiceResult<bool>> DeleteCommentAsync(int commentId);
        Task<ServiceResult<bool>> MarkAsSpamAsync(int commentId);
        Task<ServiceResult<bool>> MarkAsReportedAsync(int commentId);
        Task<ServiceResult<bool>> UnmarkAsReportedAsync(int commentId);
        Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetPendingCommentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetSpamCommentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<PagedResult<BlogPostCommentViewModel>>> GetReportedCommentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<int>> GetCommentCountAsync(int blogPostId, bool approvedOnly = true);
    }
}

