using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Interfaces.CMS
{
    /// <summary>
    /// Interface برای Repository کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP
    /// </summary>
    public interface IBlogPostCommentRepository
    {
        Task<BlogPostComment> GetByIdAsync(int commentId);
        Task<List<BlogPostComment>> GetByBlogPostIdAsync(int blogPostId, bool includeReplies = true);
        Task<List<BlogPostComment>> GetApprovedCommentsAsync(int blogPostId);
        Task<List<BlogPostComment>> GetPendingCommentsAsync();
        Task<List<BlogPostComment>> GetSpamCommentsAsync();
        Task<List<BlogPostComment>> GetReportedCommentsAsync();
        Task<List<BlogPostComment>> GetRepliesAsync(int parentCommentId);
        Task<int> GetCommentCountAsync(int blogPostId, bool approvedOnly = true);
        Task<BlogPostComment> CreateAsync(BlogPostComment comment);
        Task<BlogPostComment> UpdateAsync(BlogPostComment comment);
        Task<bool> DeleteAsync(int commentId);
        Task<bool> ApproveAsync(int commentId);
        Task<bool> RejectAsync(int commentId);
        Task<bool> MarkAsSpamAsync(int commentId);
        Task<bool> MarkAsReportedAsync(int commentId);
        Task<bool> UnmarkAsReportedAsync(int commentId);
    }
}

