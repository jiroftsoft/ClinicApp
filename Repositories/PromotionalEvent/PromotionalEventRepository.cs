using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.Models;
using ClinicApp.Models.Entities.PromotionalEvent;
using Newtonsoft.Json;
using Serilog;

namespace ClinicApp.Repositories.PromotionalEvent
{
    /// <summary>
    /// Repository برای عملیات داده‌ای PromotionalEvent
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class PromotionalEventRepository : IPromotionalEventRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public PromotionalEventRepository(ApplicationDbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger?.ForContext<PromotionalEventRepository>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Models.Entities.PromotionalEvent.PromotionalEvent> GetByIdAsync(int eventId)
        {
            try
            {
                _logger.Debug("🎁 PROMOTIONAL EVENT: دریافت ایونت با شناسه {EventId}", eventId);

                var promotionalEvent = await _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .Include(e => e.Appointments)
                    .Where(e => e.EventId == eventId && !e.IsDeleted)
                    .FirstOrDefaultAsync();

                if (promotionalEvent == null)
                {
                    _logger.Warning("⚠️ PROMOTIONAL EVENT: ایونت با شناسه {EventId} یافت نشد", eventId);
                }

                return promotionalEvent;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت با شناسه {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetAllAsync(bool includeDeleted = false)
        {
            try
            {
                _logger.Debug("🎁 PROMOTIONAL EVENT: دریافت تمام ایونت‌ها - IncludeDeleted: {IncludeDeleted}", includeDeleted);

                var query = _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .Include(e => e.Appointments)
                    .AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(e => !e.IsDeleted);
                }

                var result = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                _logger.Information("✅ PROMOTIONAL EVENT: {Count} ایونت دریافت شد", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت تمام ایونت‌ها");
                throw;
            }
        }

        public async Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetActiveEventsAsync(DateTime? appointmentDate = null)
        {
            try
            {
                var now = appointmentDate ?? DateTime.Now;

                _logger.Debug("🎁 PROMOTIONAL EVENT: دریافت ایونت‌های فعال - AppointmentDate: {AppointmentDate}", now);

                var activeEvents = await _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .AsNoTracking() // Read-only query
                    .Where(e => e.IsActive
                        && !e.IsDeleted
                        && e.StartDate <= now
                        && e.EndDate >= now
                        && (e.TotalSlots == null || e.UsedSlots < e.TotalSlots))
                    .OrderBy(e => e.StartDate)
                    .ToListAsync();

                _logger.Information("✅ PROMOTIONAL EVENT: {Count} ایونت فعال یافت شد", activeEvents.Count);

                return activeEvents;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت‌های فعال");
                throw;
            }
        }

        public async Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, bool includeDeleted = false)
        {
            try
            {
                _logger.Debug("🎁 PROMOTIONAL EVENT: دریافت ایونت‌ها در بازه زمانی {StartDate} تا {EndDate}", startDate, endDate);

                var query = _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .AsNoTracking() // Read-only query
                    .Where(e => (e.StartDate <= endDate && e.EndDate >= startDate));

                if (!includeDeleted)
                {
                    query = query.Where(e => !e.IsDeleted);
                }

                var result = await query
                    .OrderBy(e => e.StartDate)
                    .ToListAsync();

                _logger.Information("✅ PROMOTIONAL EVENT: {Count} ایونت در بازه زمانی یافت شد", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت‌ها در بازه زمانی");
                throw;
            }
        }

        public async Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> GetEventsByDoctorAsync(int doctorId, DateTime? appointmentDate = null)
        {
            try
            {
                var now = appointmentDate ?? DateTime.Now;

                _logger.Debug("🎁 PROMOTIONAL EVENT: دریافت ایونت‌های مرتبط با پزشک {DoctorId} - AppointmentDate: {AppointmentDate}", doctorId, now);

                var allActiveEvents = await GetActiveEventsAsync(now);

                var doctorSpecificEvents = new List<Models.Entities.PromotionalEvent.PromotionalEvent>();

                foreach (var evt in allActiveEvents)
                {
                    // اگر ایونت برای همه پزشکان است
                    if (!evt.IsDoctorSpecific)
                    {
                        doctorSpecificEvents.Add(evt);
                        continue;
                    }

                    // بررسی محدودیت پزشک
                    if (!string.IsNullOrWhiteSpace(evt.DoctorIds))
                    {
                        try
                        {
                            var doctorIds = JsonConvert.DeserializeObject<List<int>>(evt.DoctorIds);
                            if (doctorIds != null && doctorIds.Contains(doctorId))
                            {
                                doctorSpecificEvents.Add(evt);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Warning(ex, "⚠️ PROMOTIONAL EVENT: خطا در Parse کردن DoctorIds برای ایونت {EventId}", evt.EventId);
                        }
                    }
                }

                _logger.Information("✅ PROMOTIONAL EVENT: {Count} ایونت مرتبط با پزشک {DoctorId} یافت شد", doctorSpecificEvents.Count, doctorId);

                return doctorSpecificEvents;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت ایونت‌های مرتبط با پزشک {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<List<Models.Entities.PromotionalEvent.PromotionalEvent>> SearchAsync(string searchTerm, bool? isActive = null, DateTime? fromDate = null, DateTime? toDate = null, bool includeDeleted = false)
        {
            try
            {
                _logger.Debug("🎁 PROMOTIONAL EVENT: جستجوی ایونت‌ها - SearchTerm: {SearchTerm}, IsActive: {IsActive}", searchTerm, isActive);

                var query = _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .AsNoTracking() // Read-only query
                    .AsQueryable();

                if (!includeDeleted)
                {
                    query = query.Where(e => !e.IsDeleted);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var search = searchTerm.Trim();
                    query = query.Where(e => e.Title.Contains(search) ||
                                           (e.Description != null && e.Description.Contains(search)));
                }

                if (isActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == isActive.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(e => e.StartDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(e => e.EndDate <= toDate.Value);
                }

                var result = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                _logger.Information("✅ PROMOTIONAL EVENT: {Count} ایونت در جستجو یافت شد", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در جستجوی ایونت‌ها");
                throw;
            }
        }

        public void Add(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent)
        {
            try
            {
                if (promotionalEvent == null)
                    throw new ArgumentNullException(nameof(promotionalEvent));

                _logger.Information("🎁 PROMOTIONAL EVENT: افزودن ایونت جدید - Title: {Title}", promotionalEvent.Title);

                _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>().Add(promotionalEvent);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در افزودن ایونت");
                throw;
            }
        }

        public void Update(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent)
        {
            try
            {
                if (promotionalEvent == null)
                    throw new ArgumentNullException(nameof(promotionalEvent));

                _logger.Information("🎁 PROMOTIONAL EVENT: به‌روزرسانی ایونت - EventId: {EventId}, Title: {Title}", 
                    promotionalEvent.EventId, promotionalEvent.Title);

                _context.Entry(promotionalEvent).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در به‌روزرسانی ایونت - EventId: {EventId}", 
                    promotionalEvent?.EventId);
                throw;
            }
        }

        public void Delete(Models.Entities.PromotionalEvent.PromotionalEvent promotionalEvent)
        {
            try
            {
                if (promotionalEvent == null)
                    throw new ArgumentNullException(nameof(promotionalEvent));

                _logger.Information("🎁 PROMOTIONAL EVENT: حذف نرم ایونت - EventId: {EventId}, Title: {Title}", 
                    promotionalEvent.EventId, promotionalEvent.Title);

                promotionalEvent.IsDeleted = true;
                promotionalEvent.DeletedAt = DateTime.Now;
                _context.Entry(promotionalEvent).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در حذف ایونت - EventId: {EventId}", 
                    promotionalEvent?.EventId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int eventId)
        {
            try
            {
                var exists = await _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .AnyAsync(e => e.EventId == eventId && !e.IsDeleted);

                _logger.Debug("🎁 PROMOTIONAL EVENT: بررسی وجود ایونت - EventId: {EventId}, Exists: {Exists}", eventId, exists);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در بررسی وجود ایونت - EventId: {EventId}", eventId);
                throw;
            }
        }

        public async Task IncrementUsedSlotsAsync(int eventId)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: افزایش تعداد استفاده - EventId: {EventId}", eventId);

                var promotionalEvent = await _context.Set<Models.Entities.PromotionalEvent.PromotionalEvent>()
                    .FirstOrDefaultAsync(e => e.EventId == eventId && !e.IsDeleted);

                if (promotionalEvent == null)
                {
                    _logger.Warning("⚠️ PROMOTIONAL EVENT: ایونت با شناسه {EventId} یافت نشد", eventId);
                    return;
                }

                promotionalEvent.UsedSlots++;
                _context.Entry(promotionalEvent).State = EntityState.Modified;

                _logger.Information("✅ PROMOTIONAL EVENT: تعداد استفاده افزایش یافت - EventId: {EventId}, UsedSlots: {UsedSlots}/{TotalSlots}", 
                    eventId, promotionalEvent.UsedSlots, promotionalEvent.TotalSlots);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در افزایش تعداد استفاده - EventId: {EventId}", eventId);
                throw;
            }
        }
    }
}

