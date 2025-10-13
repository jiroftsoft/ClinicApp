using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Reception.EventHandlers;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// موتور مدیریت رویدادها برای workflow پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت رویدادهای همزمان و ناهمزمان
    /// 2. Event Sourcing و Event Store
    /// 3. Event Replay و Rollback
    /// 4. Event Filtering و Routing
    /// 5. Event Monitoring و Analytics
    /// </summary>
    public class ReceptionEventHandler
    {
        #region Fields and Constructor

        private readonly ILogger _logger;
        private readonly Dictionary<WorkflowEvent, List<IEventHandler>> _eventHandlers;
        private readonly Dictionary<WorkflowEvent, List<IEventHandler>> _asyncEventHandlers;
        private readonly EventStore _eventStore;

        public ReceptionEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventHandlers = InitializeEventHandlers();
            _asyncEventHandlers = InitializeAsyncEventHandlers();
            _eventStore = new EventStore(logger);
        }

        #endregion

        #region Event Processing

        /// <summary>
        /// پردازش رویداد workflow
        /// </summary>
        public async Task<ServiceResult<EventProcessingResult>> ProcessEventAsync(
            int receptionId, 
            WorkflowEvent eventType, 
            object eventData, 
            string userId)
        {
            try
            {
                _logger.Information("🎯 پردازش رویداد: {ReceptionId}, رویداد: {EventType}, کاربر: {UserId}", 
                    receptionId, eventType, userId);

                var eventProcessingResult = new EventProcessingResult
                {
                    ReceptionId = receptionId,
                    EventType = eventType,
                    ProcessedAt = DateTime.Now,
                    Success = true,
                    HandlerResults = new List<HandlerResult>()
                };

                // ایجاد رویداد
                var workflowEvent = new WorkflowEventData
                {
                    ReceptionId = receptionId,
                    EventType = eventType,
                    EventData = eventData,
                    UserId = userId,
                    Timestamp = DateTime.Now,
                    EventId = Guid.NewGuid().ToString()
                };

                // ذخیره رویداد در Event Store
                await _eventStore.StoreEventAsync(workflowEvent);

                // پردازش همزمان
                var syncResult = await ProcessSynchronousEventAsync(workflowEvent);
                eventProcessingResult.HandlerResults.AddRange(syncResult);

                // پردازش ناهمزمان
                var asyncResult = await ProcessAsynchronousEventAsync(workflowEvent);
                eventProcessingResult.HandlerResults.AddRange(asyncResult);

                // بررسی نتایج
                var failedHandlers = eventProcessingResult.HandlerResults.Where(r => !r.Success).ToList();
                if (failedHandlers.Any())
                {
                    eventProcessingResult.Success = false;
                    eventProcessingResult.Errors = failedHandlers.Select(h => h.ErrorMessage).ToList();
                    
                    _logger.Warning("⚠️ برخی handler ها ناموفق بودند: {ReceptionId}, رویداد: {EventType}, خطاها: {ErrorCount}", 
                        receptionId, eventType, failedHandlers.Count);
                }

                _logger.Information("✅ پردازش رویداد تکمیل شد: {ReceptionId}, رویداد: {EventType}, موفق: {Success}", 
                    receptionId, eventType, eventProcessingResult.Success);

                return ServiceResult<EventProcessingResult>.Successful(eventProcessingResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد: {ReceptionId}, رویداد: {EventType}", receptionId, eventType);
                return ServiceResult<EventProcessingResult>.Failed("خطا در پردازش رویداد");
            }
        }

        /// <summary>
        /// پردازش همزمان رویداد
        /// </summary>
        private async Task<List<HandlerResult>> ProcessSynchronousEventAsync(WorkflowEventData workflowEvent)
        {
            var results = new List<HandlerResult>();

            try
            {
                if (!_eventHandlers.ContainsKey(workflowEvent.EventType))
                {
                    _logger.Warning("⚠️ هیچ handler همزمانی برای رویداد {EventType} تعریف نشده", workflowEvent.EventType);
                    return results;
                }

                var handlers = _eventHandlers[workflowEvent.EventType];
                foreach (var handler in handlers)
                {
                    try
                    {
                        _logger.Information("🔄 اجرای handler همزمان: {ReceptionId}, رویداد: {EventType}, handler: {HandlerType}", 
                            workflowEvent.ReceptionId, workflowEvent.EventType, handler.GetType().Name);

                        var result = await handler.HandleAsync(workflowEvent);
                        results.Add(result);

                        _logger.Information("✅ handler همزمان اجرا شد: {ReceptionId}, handler: {HandlerType}, موفق: {Success}", 
                            workflowEvent.ReceptionId, handler.GetType().Name, result.Success);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ خطا در اجرای handler همزمان: {ReceptionId}, handler: {HandlerType}", 
                            workflowEvent.ReceptionId, handler.GetType().Name);

                        results.Add(new HandlerResult
                        {
                            HandlerType = handler.GetType().Name,
                            Success = false,
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش همزمان رویداد: {ReceptionId}", workflowEvent.ReceptionId);
            }

            return results;
        }

        /// <summary>
        /// پردازش ناهمزمان رویداد
        /// </summary>
        private async Task<List<HandlerResult>> ProcessAsynchronousEventAsync(WorkflowEventData workflowEvent)
        {
            var results = new List<HandlerResult>();

            try
            {
                if (!_asyncEventHandlers.ContainsKey(workflowEvent.EventType))
                {
                    _logger.Warning("⚠️ هیچ handler ناهمزمانی برای رویداد {EventType} تعریف نشده", workflowEvent.EventType);
                    return results;
                }

                var handlers = _asyncEventHandlers[workflowEvent.EventType];
                var tasks = handlers.Select(async handler =>
                {
                    try
                    {
                        _logger.Information("🔄 اجرای handler ناهمزمان: {ReceptionId}, رویداد: {EventType}, handler: {HandlerType}", 
                            workflowEvent.ReceptionId, workflowEvent.EventType, handler.GetType().Name);

                        var result = await handler.HandleAsync(workflowEvent);
                        
                        _logger.Information("✅ handler ناهمزمان اجرا شد: {ReceptionId}, handler: {HandlerType}, موفق: {Success}", 
                            workflowEvent.ReceptionId, handler.GetType().Name, result.Success);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ خطا در اجرای handler ناهمزمان: {ReceptionId}, handler: {HandlerType}", 
                            workflowEvent.ReceptionId, handler.GetType().Name);

                        return new HandlerResult
                        {
                            HandlerType = handler.GetType().Name,
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                    }
                });

                var handlerResults = await Task.WhenAll(tasks);
                results.AddRange(handlerResults);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش ناهمزمان رویداد: {ReceptionId}", workflowEvent.ReceptionId);
            }

            return results;
        }

        #endregion

        #region Event Replay

        /// <summary>
        /// بازپخش رویدادها برای یک پذیرش
        /// </summary>
        public async Task<ServiceResult<EventReplayResult>> ReplayEventsAsync(int receptionId, DateTime? fromDate = null)
        {
            try
            {
                _logger.Information("🔄 بازپخش رویدادها: {ReceptionId}, از تاریخ: {FromDate}", receptionId, fromDate);

                var events = await _eventStore.GetEventsAsync(receptionId, fromDate);
                var replayResult = new EventReplayResult
                {
                    ReceptionId = receptionId,
                    ReplayedEvents = new List<EventReplayItem>(),
                    Success = true
                };

                foreach (var eventData in events)
                {
                    try
                    {
                        _logger.Information("🔄 بازپخش رویداد: {ReceptionId}, رویداد: {EventType}, تاریخ: {Timestamp}", 
                            eventData.ReceptionId, eventData.EventType, eventData.Timestamp);

                        var processResult = await ProcessEventAsync(
                            eventData.ReceptionId, 
                            eventData.EventType, 
                            eventData.EventData, 
                            eventData.UserId);

                        replayResult.ReplayedEvents.Add(new EventReplayItem
                        {
                            EventId = eventData.EventId,
                            EventType = eventData.EventType,
                            Timestamp = eventData.Timestamp,
                            Success = processResult.Success,
                            ErrorMessage = processResult.Success ? null : processResult.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "❌ خطا در بازپخش رویداد: {ReceptionId}, رویداد: {EventType}", 
                            eventData.ReceptionId, eventData.EventType);

                        replayResult.ReplayedEvents.Add(new EventReplayItem
                        {
                            EventId = eventData.EventId,
                            EventType = eventData.EventType,
                            Timestamp = eventData.Timestamp,
                            Success = false,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                _logger.Information("✅ بازپخش رویدادها تکمیل شد: {ReceptionId}, تعداد رویدادها: {EventCount}", 
                    receptionId, replayResult.ReplayedEvents.Count);

                return ServiceResult<EventReplayResult>.Successful(replayResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بازپخش رویدادها: {ReceptionId}", receptionId);
                return ServiceResult<EventReplayResult>.Failed("خطا در بازپخش رویدادها");
            }
        }

        #endregion

        #region Event Filtering

        /// <summary>
        /// فیلتر کردن رویدادها بر اساس معیارهای مختلف
        /// </summary>
        public async Task<ServiceResult<List<WorkflowEventData>>> FilterEventsAsync(EventFilterCriteria criteria)
        {
            try
            {
                _logger.Information("🔍 فیلتر رویدادها: معیارها: {Criteria}", criteria.ToString());

                var events = await _eventStore.GetEventsAsync(criteria);
                var filteredEvents = ApplyEventFilters(events, criteria);

                _logger.Information("✅ فیلتر رویدادها تکمیل شد: تعداد: {Count}", filteredEvents.Count);

                return ServiceResult<List<WorkflowEventData>>.Successful(filteredEvents);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در فیلتر رویدادها");
                return ServiceResult<List<WorkflowEventData>>.Failed("خطا در فیلتر رویدادها");
            }
        }

        /// <summary>
        /// اعمال فیلترهای رویداد
        /// </summary>
        private List<WorkflowEventData> ApplyEventFilters(List<WorkflowEventData> events, EventFilterCriteria criteria)
        {
            var filteredEvents = events.AsQueryable();

            if (criteria.ReceptionId.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.ReceptionId == criteria.ReceptionId.Value);
            }

            if (criteria.EventTypes?.Any() == true)
            {
                filteredEvents = filteredEvents.Where(e => criteria.EventTypes.Contains(e.EventType));
            }

            if (criteria.FromDate.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.Timestamp >= criteria.FromDate.Value);
            }

            if (criteria.ToDate.HasValue)
            {
                filteredEvents = filteredEvents.Where(e => e.Timestamp <= criteria.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(criteria.UserId))
            {
                filteredEvents = filteredEvents.Where(e => e.UserId == criteria.UserId);
            }

            return filteredEvents.ToList();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// مقداردهی اولیه handler های همزمان
        /// </summary>
        private Dictionary<WorkflowEvent, List<IEventHandler>> InitializeEventHandlers()
        {
            return new Dictionary<WorkflowEvent, List<IEventHandler>>
            {
                [WorkflowEvent.PatientValidation] = new List<IEventHandler>
                {
                    new PatientValidationEventHandler(_logger),
                    new AuditLoggingEventHandler(_logger)
                },
                [WorkflowEvent.InsuranceValidation] = new List<IEventHandler>
                {
                    new InsuranceValidationEventHandler(_logger),
                    new AuditLoggingEventHandler(_logger)
                },
                [WorkflowEvent.PaymentProcessing] = new List<IEventHandler>
                {
                    new PaymentProcessingEventHandler(_logger),
                    new AuditLoggingEventHandler(_logger)
                },
                [WorkflowEvent.NotificationSending] = new List<IEventHandler>
                {
                    new NotificationSendingEventHandler(_logger)
                },
                [WorkflowEvent.AuditLogging] = new List<IEventHandler>
                {
                    new AuditLoggingEventHandler(_logger)
                }
            };
        }

        /// <summary>
        /// مقداردهی اولیه handler های ناهمزمان
        /// </summary>
        private Dictionary<WorkflowEvent, List<IEventHandler>> InitializeAsyncEventHandlers()
        {
            return new Dictionary<WorkflowEvent, List<IEventHandler>>
            {
                [WorkflowEvent.NotificationSending] = new List<IEventHandler>
                {
                    new EmailNotificationEventHandler(_logger),
                    new SmsNotificationEventHandler(_logger),
                    new PushNotificationEventHandler(_logger)
                },
                [WorkflowEvent.AuditLogging] = new List<IEventHandler>
                {
                    new AuditLoggingEventHandler(_logger),
                    new AnalyticsEventHandler(_logger)
                }
            };
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// داده‌های رویداد workflow
    /// </summary>
    public class WorkflowEventData
    {
        public string EventId { get; set; }
        public int ReceptionId { get; set; }
        public WorkflowEvent EventType { get; set; }
        public object EventData { get; set; }
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// نتیجه پردازش رویداد
    /// </summary>
    public class EventProcessingResult
    {
        public int ReceptionId { get; set; }
        public WorkflowEvent EventType { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool Success { get; set; }
        public List<HandlerResult> HandlerResults { get; set; } = new List<HandlerResult>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// نتیجه handler
    /// </summary>
    public class HandlerResult
    {
        public string HandlerType { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public object ResultData { get; set; }
    }

    /// <summary>
    /// نتیجه بازپخش رویدادها
    /// </summary>
    public class EventReplayResult
    {
        public int ReceptionId { get; set; }
        public List<EventReplayItem> ReplayedEvents { get; set; } = new List<EventReplayItem>();
        public bool Success { get; set; }
    }

    /// <summary>
    /// آیتم بازپخش رویداد
    /// </summary>
    public class EventReplayItem
    {
        public string EventId { get; set; }
        public WorkflowEvent EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// معیارهای فیلتر رویداد
    /// </summary>
    public class EventFilterCriteria
    {
        public int? ReceptionId { get; set; }
        public List<WorkflowEvent> EventTypes { get; set; } = new List<WorkflowEvent>();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string UserId { get; set; }
    }

    #endregion
}
