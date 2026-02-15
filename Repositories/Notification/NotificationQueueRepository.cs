using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;
using ClinicApp.Models;

namespace ClinicApp.Repositories.Notification;

public class NotificationQueueRepository : INotificationQueueRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationQueueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationQueueItem> AddAsync(NotificationQueueItem item)
    {
        _context.NotificationQueue.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<NotificationQueueItem> GetByIdAsync(long id)
    {
        return await _context.NotificationQueue.FindAsync(id);
    }

    public async Task<NotificationQueueItem> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.NotificationQueue
            .FirstOrDefaultAsync(q => q.IdempotencyKey == idempotencyKey && q.Status != NotificationStatus.Canceled);
    }

    public async Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, params NotificationStatus[] statuses)
    {
        if (statuses == null || statuses.Length == 0)
            return await _context.NotificationQueue.AnyAsync(q => q.IdempotencyKey == idempotencyKey);

        return await _context.NotificationQueue
            .AnyAsync(q => q.IdempotencyKey == idempotencyKey && statuses.Contains(q.Status));
    }

    public async Task<List<NotificationQueueItem>> GetPendingBatchAsync(int maxCount)
    {
        var cutoff = System.DateTime.UtcNow.AddMinutes(5);
        return await _context.NotificationQueue
            .Where(q => q.Status == NotificationStatus.Queued && (q.ScheduledTime == null || q.ScheduledTime <= cutoff))
            .OrderBy(q => q.CreatedAt)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task<List<NotificationQueueItem>> GetScheduledDueBatchAsync(int maxCount)
    {
        var now = System.DateTime.UtcNow;
        return await _context.NotificationQueue
            .Where(q => q.Status == NotificationStatus.Scheduled && q.ScheduledTime.HasValue && q.ScheduledTime <= now)
            .OrderBy(q => q.ScheduledTime)
            .Take(maxCount)
            .ToListAsync();
    }

    public async Task UpdateAsync(NotificationQueueItem item)
    {
        _context.Entry(item).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
