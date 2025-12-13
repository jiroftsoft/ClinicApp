using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای عملیات داده‌ای NewsletterCampaign
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterCampaignRepository : INewsletterCampaignRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterCampaignRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<NewsletterCampaign> GetByIdAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaign>()
                .Include(c => c.Template)
                .Where(c => c.NewsletterCampaignId == campaignId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NewsletterCampaign>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterCampaign>()
                .Include(c => c.Template)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterCampaign>> GetByStatusAsync(NewsletterCampaignStatus status, bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterCampaign>()
                .Include(c => c.Template)
                .Where(c => c.Status == status);

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterCampaign>> GetScheduledAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterCampaign>()
                .Include(c => c.Template)
                .Where(c => c.Status == NewsletterCampaignStatus.Scheduled && 
                           c.ScheduledAt.HasValue && 
                           c.ScheduledAt.Value <= DateTime.Now);

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query.OrderBy(c => c.ScheduledAt).ToListAsync();
        }

        public async Task<List<NewsletterCampaign>> SearchAsync(string searchTerm, NewsletterCampaignStatus? status, DateTime? fromDate, DateTime? toDate, bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterCampaign>()
                .Include(c => c.Template)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(c => c.Title.Contains(search) ||
                                       c.Subject.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= toDate.Value);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public void Add(NewsletterCampaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            _context.Set<NewsletterCampaign>().Add(campaign);
        }

        public void Update(NewsletterCampaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            _context.Entry(campaign).State = EntityState.Modified;
        }

        public void Delete(NewsletterCampaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            campaign.IsDeleted = true;
            campaign.DeletedAt = DateTime.Now;
            _context.Entry(campaign).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaign>()
                .AnyAsync(c => c.NewsletterCampaignId == campaignId && !c.IsDeleted);
        }
    }
}

