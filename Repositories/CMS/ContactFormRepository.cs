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
    /// Repository برای عملیات داده‌ای ContactForm
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class ContactFormRepository : IContactFormRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactFormRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ContactForm> GetByIdAsync(int contactFormId)
        {
            return await _context.Set<ContactForm>()
                .Where(c => c.ContactFormId == contactFormId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ContactForm>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<ContactForm>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ContactForm>> GetByStatusAsync(ContactFormStatus status, bool includeDeleted = false)
        {
            var query = _context.Set<ContactForm>()
                .Where(c => c.Status == status);
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ContactForm>> GetByCategoryAsync(ContactFormCategory category, bool includeDeleted = false)
        {
            var query = _context.Set<ContactForm>()
                .Where(c => c.Category == category);
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ContactForm>> GetUnreadAsync(bool includeDeleted = false)
        {
            var query = _context.Set<ContactForm>()
                .Where(c => !c.IsRead);
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Set<ContactForm>()
                .Where(c => !c.IsDeleted && !c.IsRead)
                .CountAsync();
        }

        public async Task<List<ContactForm>> SearchAsync(string searchTerm, ContactFormCategory? category, ContactFormStatus? status, bool? isRead, bool includeDeleted = false)
        {
            var query = _context.Set<ContactForm>().AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.Trim();
                query = query.Where(c => 
                    c.FullName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.PhoneNumber.Contains(search) ||
                    c.Subject.Contains(search) ||
                    c.Message.Contains(search));
            }

            if (category.HasValue)
            {
                query = query.Where(c => c.Category == category.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (isRead.HasValue)
            {
                query = query.Where(c => c.IsRead == isRead.Value);
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public void Add(ContactForm contactForm)
        {
            if (contactForm == null)
                throw new ArgumentNullException(nameof(contactForm));

            _context.Set<ContactForm>().Add(contactForm);
        }

        public void Update(ContactForm contactForm)
        {
            if (contactForm == null)
                throw new ArgumentNullException(nameof(contactForm));

            _context.Entry(contactForm).State = EntityState.Modified;
        }

        public void Delete(ContactForm contactForm)
        {
            if (contactForm == null)
                throw new ArgumentNullException(nameof(contactForm));

            contactForm.IsDeleted = true;
            contactForm.DeletedAt = DateTime.Now;
            _context.Entry(contactForm).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(int contactFormId)
        {
            return await _context.Set<ContactForm>()
                .AnyAsync(c => c.ContactFormId == contactFormId && !c.IsDeleted);
        }
    }
}

