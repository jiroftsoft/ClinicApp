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
    /// Repository برای عملیات داده‌ای BlogPost
    /// </summary>
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogPostRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BlogPost> GetByIdAsync(int blogPostId)
        {
            return await _context.Set<BlogPost>()
                .Where(b => b.BlogPostId == blogPostId && !b.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BlogPost>> GetPublishedPostsAsync(int count = 10)
        {
            var now = DateTime.Now;
            return await _context.Set<BlogPost>()
                .Where(b => !b.IsDeleted && 
                           b.IsPublished && 
                           (b.PublishedAt == null || b.PublishedAt <= now))
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .ThenByDescending(b => b.DisplayOrder ?? 0)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetFeaturedPostsAsync(int count = 3)
        {
            var now = DateTime.Now;
            return await _context.Set<BlogPost>()
                .Where(b => !b.IsDeleted && 
                           b.IsPublished && 
                           b.IsFeatured &&
                           (b.PublishedAt == null || b.PublishedAt <= now))
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .ThenByDescending(b => b.DisplayOrder ?? 0)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<BlogPost>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(b => !b.IsDeleted);
            }

            return await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> GetByCategoryAsync(string categoryName, int count = 10)
        {
            var now = DateTime.Now;
            return await _context.Set<BlogPost>()
                .Where(b => !b.IsDeleted && 
                           b.IsPublished && 
                           b.CategoryName == categoryName &&
                           (b.PublishedAt == null || b.PublishedAt <= now))
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<BlogPost> GetBySlugAsync(string slug)
        {
            return await _context.Set<BlogPost>()
                .Where(b => b.Slug == slug && !b.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public void Add(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            _context.Set<BlogPost>().Add(blogPost);
        }

        public void Update(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            _context.Entry(blogPost).State = EntityState.Modified;
        }

        public void Delete(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException(nameof(blogPost));

            blogPost.IsDeleted = true;
            blogPost.DeletedAt = DateTime.Now;
            _context.Entry(blogPost).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int blogPostId)
        {
            return await _context.Set<BlogPost>()
                .AnyAsync(b => b.BlogPostId == blogPostId && !b.IsDeleted);
        }
    }
}

