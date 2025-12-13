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
    /// Repository برای عملیات داده‌ای NewsletterCampaignRecipient
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterCampaignRecipientRepository : INewsletterCampaignRecipientRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterCampaignRecipientRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<NewsletterCampaignRecipient> GetByIdAsync(int recipientId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Include(r => r.Campaign)
                .Include(r => r.Subscription)
                .Where(r => r.NewsletterCampaignRecipientId == recipientId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NewsletterCampaignRecipient>> GetByCampaignIdAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Include(r => r.Subscription)
                .Where(r => r.NewsletterCampaignId == campaignId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<NewsletterCampaignRecipient>> GetBySubscriptionIdAsync(int subscriptionId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Include(r => r.Campaign)
                .Where(r => r.NewsletterSubscriptionId == subscriptionId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<NewsletterCampaignRecipient> GetByCampaignAndSubscriptionAsync(int campaignId, int subscriptionId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Where(r => r.NewsletterCampaignId == campaignId && 
                           r.NewsletterSubscriptionId == subscriptionId)
                .FirstOrDefaultAsync();
        }

        public void Add(NewsletterCampaignRecipient recipient)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));

            _context.Set<NewsletterCampaignRecipient>().Add(recipient);
        }

        public void Update(NewsletterCampaignRecipient recipient)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));

            _context.Entry(recipient).State = EntityState.Modified;
        }

        public async Task BulkInsertAsync(List<NewsletterCampaignRecipient> recipients)
        {
            if (recipients == null || !recipients.Any())
                return;

            _context.Set<NewsletterCampaignRecipient>().AddRange(recipients);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountByCampaignIdAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Where(r => r.NewsletterCampaignId == campaignId)
                .CountAsync();
        }

        public async Task<int> GetOpenedCountByCampaignIdAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Where(r => r.NewsletterCampaignId == campaignId && r.OpenedAt.HasValue)
                .CountAsync();
        }

        public async Task<int> GetClickedCountByCampaignIdAsync(int campaignId)
        {
            return await _context.Set<NewsletterCampaignRecipient>()
                .Where(r => r.NewsletterCampaignId == campaignId && r.ClickedAt.HasValue)
                .CountAsync();
        }
    }
}

