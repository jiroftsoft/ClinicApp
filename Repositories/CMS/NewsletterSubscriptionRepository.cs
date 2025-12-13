using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.Models.Enums;
using Newtonsoft.Json;

namespace ClinicApp.Repositories.CMS
{
    /// <summary>
    /// Repository برای عملیات داده‌ای NewsletterSubscription
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class NewsletterSubscriptionRepository : INewsletterSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterSubscriptionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<NewsletterSubscription> GetByIdAsync(int subscriptionId)
        {
            return await _context.Set<NewsletterSubscription>()
                .Where(n => n.NewsletterSubscriptionId == subscriptionId && !n.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<NewsletterSubscription> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Set<NewsletterSubscription>()
                .Where(n => n.Email == email && !n.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<NewsletterSubscription> GetByVerificationTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return await _context.Set<NewsletterSubscription>()
                .Where(n => n.VerificationToken == token && !n.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<NewsletterSubscription> GetByUnsubscribeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return await _context.Set<NewsletterSubscription>()
                .Where(n => n.UnsubscribeToken == token && !n.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NewsletterSubscription>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterSubscription>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterSubscription>> GetActiveAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterSubscription>()
                .Where(n => n.IsActive);

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterSubscription>> GetActiveAndVerifiedAsync(bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterSubscription>()
                .Where(n => n.IsActive && n.IsVerified);

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterSubscription>> GetByCategoriesAsync(List<NewsletterCategory> categories, bool includeDeleted = false)
        {
            if (categories == null || !categories.Any())
                return new List<NewsletterSubscription>();

            var query = _context.Set<NewsletterSubscription>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            query = query.Where(n => n.IsActive && n.IsVerified);

            var subscriptions = await query.ToListAsync();

            // فیلتر بر اساس Categories (JSON)
            return subscriptions.Where(n =>
            {
                if (string.IsNullOrEmpty(n.Categories))
                    return false;

                try
                {
                    var subscriptionCategories = JsonConvert.DeserializeObject<List<string>>(n.Categories);
                    if (subscriptionCategories == null)
                        return false;

                    return categories.Any(c => subscriptionCategories.Contains(c.ToString()));
                }
                catch
                {
                    return false;
                }
            }).ToList();
        }

        public async Task<List<NewsletterSubscription>> GetBySourceAsync(NewsletterSubscriptionSource source, bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterSubscription>()
                .Where(n => n.Source == source);

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<List<NewsletterSubscription>> SearchAsync(string searchTerm, bool? isActive, bool? isVerified, NewsletterSubscriptionSource? source, NewsletterCategory? category, bool includeDeleted = false)
        {
            var query = _context.Set<NewsletterSubscription>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(n => n.Email.Contains(search) ||
                                        (n.FullName != null && n.FullName.Contains(search)) ||
                                        (n.PhoneNumber != null && n.PhoneNumber.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(n => n.IsActive == isActive.Value);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(n => n.IsVerified == isVerified.Value);
            }

            if (source.HasValue)
            {
                query = query.Where(n => n.Source == source.Value);
            }

            if (category.HasValue)
            {
                var categoryString = category.Value.ToString();
                query = query.Where(n => n.Categories != null && n.Categories.Contains(categoryString));
            }

            return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public void Add(NewsletterSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            _context.Set<NewsletterSubscription>().Add(subscription);
        }

        public void Update(NewsletterSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            _context.Entry(subscription).State = EntityState.Modified;
        }

        public void Delete(NewsletterSubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            subscription.IsDeleted = true;
            subscription.DeletedAt = DateTime.Now;
            _context.Entry(subscription).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Set<NewsletterSubscription>()
                .AnyAsync(n => n.Email == email && !n.IsDeleted);
        }
    }
}

