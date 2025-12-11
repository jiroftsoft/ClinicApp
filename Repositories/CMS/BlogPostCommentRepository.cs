using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای مدیریت کامنت‌های مقالات بلاگ
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class BlogPostCommentRepository : IBlogPostCommentRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogPostCommentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BlogPostComment> GetByIdAsync(int commentId)
        {
            return await _context.BlogPostComments
                .Include(c => c.BlogPost)
                .Include(c => c.AuthorUser)
                .Include(c => c.ParentComment)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.BlogPostCommentId == commentId && !c.IsDeleted);
        }

        public async Task<List<BlogPostComment>> GetByBlogPostIdAsync(int blogPostId, bool includeReplies = true)
        {
            var query = _context.BlogPostComments
                .Include(c => c.AuthorUser)
                .Where(c => c.BlogPostId == blogPostId && !c.IsDeleted);

            if (includeReplies)
            {
                query = query.Include(c => c.Replies);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPostComment>> GetApprovedCommentsAsync(int blogPostId)
        {
            return await _context.BlogPostComments
                .Include(c => c.AuthorUser)
                .Include(c => c.Replies)
                .Where(c => c.BlogPostId == blogPostId && c.IsApproved && !c.IsDeleted && !c.IsSpam)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPostComment>> GetPendingCommentsAsync()
        {
            return await _context.BlogPostComments
                .Include(c => c.BlogPost)
                .Include(c => c.AuthorUser)
                .Where(c => !c.IsApproved && !c.IsDeleted && !c.IsSpam)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPostComment>> GetSpamCommentsAsync()
        {
            return await _context.BlogPostComments
                .Include(c => c.BlogPost)
                .Include(c => c.AuthorUser)
                .Where(c => c.IsSpam && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPostComment>> GetReportedCommentsAsync()
        {
            return await _context.BlogPostComments
                .Include(c => c.BlogPost)
                .Include(c => c.AuthorUser)
                .Where(c => c.IsReported && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPostComment>> GetRepliesAsync(int parentCommentId)
        {
            return await _context.BlogPostComments
                .Include(c => c.AuthorUser)
                .Where(c => c.ParentCommentId == parentCommentId && c.IsApproved && !c.IsDeleted && !c.IsSpam)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCommentCountAsync(int blogPostId, bool approvedOnly = true)
        {
            var query = _context.BlogPostComments
                .Where(c => c.BlogPostId == blogPostId && !c.IsDeleted);

            if (approvedOnly)
            {
                query = query.Where(c => c.IsApproved && !c.IsSpam);
            }

            return await query.CountAsync();
        }

        public async Task<BlogPostComment> CreateAsync(BlogPostComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            comment.CreatedAt = DateTime.Now;
            _context.BlogPostComments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<BlogPostComment> UpdateAsync(BlogPostComment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsApproved = true;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsApproved = false;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsSpamAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsSpam = true;
            comment.IsApproved = false;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsReportedAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsReported = true;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnmarkAsReportedAsync(int commentId)
        {
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return false;

            comment.IsReported = false;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

