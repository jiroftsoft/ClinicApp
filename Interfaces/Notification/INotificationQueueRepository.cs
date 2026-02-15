using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Notification;

public interface INotificationQueueRepository
{
    Task<NotificationQueueItem> AddAsync(NotificationQueueItem item);
    Task<NotificationQueueItem> GetByIdAsync(long id);
    Task<NotificationQueueItem> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<bool> ExistsByIdempotencyKeyAsync(string idempotencyKey, params NotificationStatus[] statuses);
    Task<List<NotificationQueueItem>> GetPendingBatchAsync(int maxCount);
    Task<List<NotificationQueueItem>> GetScheduledDueBatchAsync(int maxCount);
    Task UpdateAsync(NotificationQueueItem item);
}
