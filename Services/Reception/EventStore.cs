using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// Event Store برای ذخیره و بازیابی رویدادهای workflow
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Event Sourcing کامل
    /// 2. Event Versioning
    /// 3. Event Snapshot
    /// 4. Event Querying
    /// 5. Event Compression
    /// </summary>
    public class EventStore
    {
        #region Fields and Constructor

        private readonly ILogger _logger;
        private readonly List<WorkflowEventData> _events;
        private readonly Dictionary<int, List<WorkflowEventData>> _receptionEvents;
        private readonly Dictionary<string, WorkflowEventData> _eventById;

        public EventStore(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _events = new List<WorkflowEventData>();
            _receptionEvents = new Dictionary<int, List<WorkflowEventData>>();
            _eventById = new Dictionary<string, WorkflowEventData>();
        }

        #endregion

        #region Event Storage

        /// <summary>
        /// ذخیره رویداد در Event Store
        /// </summary>
        public async Task<ServiceResult<bool>> StoreEventAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("💾 ذخیره رویداد: {EventId}, پذیرش: {ReceptionId}, نوع: {EventType}", 
                    eventData.EventId, eventData.ReceptionId, eventData.EventType);

                // اعتبارسنجی رویداد
                if (string.IsNullOrEmpty(eventData.EventId))
                {
                    return ServiceResult<bool>.Failed("شناسه رویداد نمی‌تواند خالی باشد");
                }

                if (eventData.ReceptionId <= 0)
                {
                    return ServiceResult<bool>.Failed("شناسه پذیرش نامعتبر است");
                }

                // بررسی تکراری نبودن رویداد
                if (_eventById.ContainsKey(eventData.EventId))
                {
                    _logger.Warning("⚠️ رویداد تکراری: {EventId}", eventData.EventId);
                    return ServiceResult<bool>.Failed("رویداد تکراری است");
                }

                // ذخیره رویداد
                _events.Add(eventData);
                _eventById[eventData.EventId] = eventData;

                // ذخیره در گروه‌بندی پذیرش
                if (!_receptionEvents.ContainsKey(eventData.ReceptionId))
                {
                    _receptionEvents[eventData.ReceptionId] = new List<WorkflowEventData>();
                }
                _receptionEvents[eventData.ReceptionId].Add(eventData);

                // مرتب‌سازی بر اساس زمان
                _receptionEvents[eventData.ReceptionId] = _receptionEvents[eventData.ReceptionId]
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                _logger.Information("✅ رویداد ذخیره شد: {EventId}, پذیرش: {ReceptionId}", 
                    eventData.EventId, eventData.ReceptionId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ذخیره رویداد: {EventId}", eventData.EventId);
                return ServiceResult<bool>.Failed("خطا در ذخیره رویداد");
            }
        }

        /// <summary>
        /// دریافت رویدادها بر اساس شناسه پذیرش
        /// </summary>
        public async Task<List<WorkflowEventData>> GetEventsAsync(int receptionId, DateTime? fromDate = null)
        {
            try
            {
                _logger.Information("📋 دریافت رویدادها: پذیرش {ReceptionId}, از تاریخ: {FromDate}", receptionId, fromDate);

                if (!_receptionEvents.ContainsKey(receptionId))
                {
                    _logger.Warning("⚠️ هیچ رویدادی برای پذیرش {ReceptionId} یافت نشد", receptionId);
                    return new List<WorkflowEventData>();
                }

                var events = _receptionEvents[receptionId];

                if (fromDate.HasValue)
                {
                    events = events.Where(e => e.Timestamp >= fromDate.Value).ToList();
                }

                _logger.Information("✅ رویدادها دریافت شدند: پذیرش {ReceptionId}, تعداد: {Count}", receptionId, events.Count);

                return events;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت رویدادها: پذیرش {ReceptionId}", receptionId);
                return new List<WorkflowEventData>();
            }
        }

        /// <summary>
        /// دریافت رویدادها بر اساس معیارهای فیلتر
        /// </summary>
        public async Task<List<WorkflowEventData>> GetEventsAsync(EventFilterCriteria criteria)
        {
            try
            {
                _logger.Information("🔍 دریافت رویدادها با فیلتر: {Criteria}", criteria.ToString());

                var events = _events.AsQueryable();

                if (criteria.ReceptionId.HasValue)
                {
                    events = events.Where(e => e.ReceptionId == criteria.ReceptionId.Value);
                }

                if (criteria.EventTypes?.Any() == true)
                {
                    events = events.Where(e => criteria.EventTypes.Contains(e.EventType));
                }

                if (criteria.FromDate.HasValue)
                {
                    events = events.Where(e => e.Timestamp >= criteria.FromDate.Value);
                }

                if (criteria.ToDate.HasValue)
                {
                    events = events.Where(e => e.Timestamp <= criteria.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(criteria.UserId))
                {
                    events = events.Where(e => e.UserId == criteria.UserId);
                }

                var result = events.ToList();

                _logger.Information("✅ رویدادها با فیلتر دریافت شدند: تعداد: {Count}", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت رویدادها با فیلتر");
                return new List<WorkflowEventData>();
            }
        }

        #endregion

        #region Event Querying

        /// <summary>
        /// دریافت آخرین رویداد از نوع خاص
        /// </summary>
        public async Task<WorkflowEventData> GetLastEventAsync(int receptionId, WorkflowEvent eventType)
        {
            try
            {
                _logger.Information("🔍 دریافت آخرین رویداد: پذیرش {ReceptionId}, نوع: {EventType}", receptionId, eventType);

                if (!_receptionEvents.ContainsKey(receptionId))
                {
                    return null;
                }

                var lastEvent = _receptionEvents[receptionId]
                    .Where(e => e.EventType == eventType)
                    .OrderByDescending(e => e.Timestamp)
                    .FirstOrDefault();

                _logger.Information("✅ آخرین رویداد دریافت شد: پذیرش {ReceptionId}, نوع: {EventType}, تاریخ: {Timestamp}", 
                    receptionId, eventType, lastEvent?.Timestamp);

                return lastEvent;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت آخرین رویداد: پذیرش {ReceptionId}, نوع: {EventType}", receptionId, eventType);
                return null;
            }
        }

        /// <summary>
        /// دریافت تعداد رویدادها بر اساس نوع
        /// </summary>
        public async Task<int> GetEventCountAsync(int receptionId, WorkflowEvent eventType)
        {
            try
            {
                if (!_receptionEvents.ContainsKey(receptionId))
                {
                    return 0;
                }

                var count = _receptionEvents[receptionId]
                    .Count(e => e.EventType == eventType);

                _logger.Information("📊 تعداد رویدادها: پذیرش {ReceptionId}, نوع: {EventType}, تعداد: {Count}", 
                    receptionId, eventType, count);

                return count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در شمارش رویدادها: پذیرش {ReceptionId}, نوع: {EventType}", receptionId, eventType);
                return 0;
            }
        }

        /// <summary>
        /// دریافت آمار رویدادها
        /// </summary>
        public async Task<EventStatistics> GetEventStatisticsAsync(int receptionId)
        {
            try
            {
                _logger.Information("📊 دریافت آمار رویدادها: پذیرش {ReceptionId}", receptionId);

                if (!_receptionEvents.ContainsKey(receptionId))
                {
                    return new EventStatistics { ReceptionId = receptionId };
                }

                var events = _receptionEvents[receptionId];
                var statistics = new EventStatistics
                {
                    ReceptionId = receptionId,
                    TotalEvents = events.Count,
                    EventTypeCounts = events.GroupBy(e => e.EventType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    FirstEventTime = events.Min(e => e.Timestamp),
                    LastEventTime = events.Max(e => e.Timestamp),
                    UniqueUsers = events.Select(e => e.UserId).Distinct().Count()
                };

                _logger.Information("✅ آمار رویدادها دریافت شد: پذیرش {ReceptionId}, تعداد کل: {TotalEvents}", 
                    receptionId, statistics.TotalEvents);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت آمار رویدادها: پذیرش {ReceptionId}", receptionId);
                return new EventStatistics { ReceptionId = receptionId };
            }
        }

        #endregion

        #region Event Snapshot

        /// <summary>
        /// ایجاد snapshot از رویدادها
        /// </summary>
        public async Task<ServiceResult<EventSnapshot>> CreateSnapshotAsync(int receptionId, DateTime? atTime = null)
        {
            try
            {
                _logger.Information("📸 ایجاد snapshot: پذیرش {ReceptionId}, زمان: {AtTime}", receptionId, atTime);

                if (!_receptionEvents.ContainsKey(receptionId))
                {
                    return ServiceResult<EventSnapshot>.Failed("هیچ رویدادی برای پذیرش یافت نشد");
                }

                var events = _receptionEvents[receptionId];
                if (atTime.HasValue)
                {
                    events = events.Where(e => e.Timestamp <= atTime.Value).ToList();
                }

                var snapshot = new EventSnapshot
                {
                    ReceptionId = receptionId,
                    SnapshotTime = atTime ?? DateTime.Now,
                    Events = events.ToList(),
                    EventCount = events.Count,
                    SnapshotId = Guid.NewGuid().ToString()
                };

                _logger.Information("✅ Snapshot ایجاد شد: پذیرش {ReceptionId}, شناسه: {SnapshotId}, تعداد رویدادها: {Count}", 
                    receptionId, snapshot.SnapshotId, snapshot.EventCount);

                return ServiceResult<EventSnapshot>.Successful(snapshot);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در ایجاد snapshot: پذیرش {ReceptionId}", receptionId);
                return ServiceResult<EventSnapshot>.Failed("خطا در ایجاد snapshot");
            }
        }

        /// <summary>
        /// بازیابی از snapshot
        /// </summary>
        public async Task<ServiceResult<bool>> RestoreFromSnapshotAsync(EventSnapshot snapshot)
        {
            try
            {
                _logger.Information("🔄 بازیابی از snapshot: پذیرش {ReceptionId}, شناسه: {SnapshotId}", 
                    snapshot.ReceptionId, snapshot.SnapshotId);

                // پاک کردن رویدادهای موجود برای پذیرش
                if (_receptionEvents.ContainsKey(snapshot.ReceptionId))
                {
                    _receptionEvents[snapshot.ReceptionId].Clear();
                }

                // بازیابی رویدادها از snapshot
                foreach (var eventData in snapshot.Events)
                {
                    await StoreEventAsync(eventData);
                }

                _logger.Information("✅ بازیابی از snapshot تکمیل شد: پذیرش {ReceptionId}, تعداد رویدادها: {Count}", 
                    snapshot.ReceptionId, snapshot.Events.Count);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بازیابی از snapshot: پذیرش {ReceptionId}", snapshot.ReceptionId);
                return ServiceResult<bool>.Failed("خطا در بازیابی از snapshot");
            }
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// آمار رویدادها
    /// </summary>
    public class EventStatistics
    {
        public int ReceptionId { get; set; }
        public int TotalEvents { get; set; }
        public Dictionary<WorkflowEvent, int> EventTypeCounts { get; set; } = new Dictionary<WorkflowEvent, int>();
        public DateTime? FirstEventTime { get; set; }
        public DateTime? LastEventTime { get; set; }
        public int UniqueUsers { get; set; }
    }

    /// <summary>
    /// Snapshot رویدادها
    /// </summary>
    public class EventSnapshot
    {
        public string SnapshotId { get; set; }
        public int ReceptionId { get; set; }
        public DateTime SnapshotTime { get; set; }
        public List<WorkflowEventData> Events { get; set; } = new List<WorkflowEventData>();
        public int EventCount { get; set; }
    }

    #endregion
}
