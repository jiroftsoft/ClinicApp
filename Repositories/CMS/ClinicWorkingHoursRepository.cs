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
    /// Repository برای عملیات داده‌ای ClinicWorkingHours
    /// طراحی شده بر اساس اصول SRP و Bulletproof
    /// </summary>
    public class ClinicWorkingHoursRepository : IClinicWorkingHoursRepository
    {
        private readonly ApplicationDbContext _context;

        public ClinicWorkingHoursRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ClinicWorkingHours> GetByIdAsync(int clinicWorkingHoursId)
        {
            return await _context.Set<ClinicWorkingHours>()
                .Where(c => c.ClinicWorkingHoursId == clinicWorkingHoursId && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ClinicWorkingHours>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _context.Set<ClinicWorkingHours>()
                .AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            return await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DayOfWeek)
                .ToListAsync();
        }

        public async Task<List<ClinicWorkingHours>> GetActiveWorkingHoursAsync(int? clinicId = null)
        {
            var query = _context.Set<ClinicWorkingHours>()
                .Where(c => !c.IsDeleted && c.IsActive);

            if (clinicId.HasValue)
            {
                query = query.Where(c => c.ClinicId == clinicId || c.ClinicId == null);
            }
            else
            {
                query = query.Where(c => c.ClinicId == null);
            }

            return await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DayOfWeek)
                .ToListAsync();
        }

        public async Task<List<ClinicWorkingHours>> GetByClinicIdAsync(int clinicId)
        {
            return await _context.Set<ClinicWorkingHours>()
                .Where(c => !c.IsDeleted && 
                           c.IsActive && 
                           (c.ClinicId == clinicId || c.ClinicId == null))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.DayOfWeek)
                .ToListAsync();
        }

        public async Task<ClinicWorkingHours> GetByDayOfWeekAsync(int dayOfWeek, int? clinicId = null)
        {
            var query = _context.Set<ClinicWorkingHours>()
                .Where(c => !c.IsDeleted && 
                           c.IsActive && 
                           c.DayOfWeek == dayOfWeek);

            if (clinicId.HasValue)
            {
                query = query.Where(c => c.ClinicId == clinicId || c.ClinicId == null);
            }
            else
            {
                query = query.Where(c => c.ClinicId == null);
            }

            return await query
                .OrderByDescending(c => c.ClinicId.HasValue) // اولویت با ClinicId مشخص
                .FirstOrDefaultAsync();
        }

        public void Add(ClinicWorkingHours clinicWorkingHours)
        {
            if (clinicWorkingHours == null)
                throw new ArgumentNullException(nameof(clinicWorkingHours));

            _context.Set<ClinicWorkingHours>().Add(clinicWorkingHours);
        }

        public void Update(ClinicWorkingHours clinicWorkingHours)
        {
            if (clinicWorkingHours == null)
                throw new ArgumentNullException(nameof(clinicWorkingHours));

            _context.Entry(clinicWorkingHours).State = EntityState.Modified;
        }

        public void Delete(ClinicWorkingHours clinicWorkingHours)
        {
            if (clinicWorkingHours == null)
                throw new ArgumentNullException(nameof(clinicWorkingHours));

            clinicWorkingHours.IsDeleted = true;
            clinicWorkingHours.DeletedAt = DateTime.Now;
            _context.Entry(clinicWorkingHours).State = EntityState.Modified;
        }
    }
}

