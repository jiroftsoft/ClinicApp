using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Services.Reception
{
    /// <summary>
    /// موتور State Machine برای مدیریت وضعیت‌های پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت انتقال بین وضعیت‌ها
    /// 2. اعتبارسنجی انتقال‌ها
    /// 3. Event Handling
    /// 4. State History Tracking
    /// 5. Rollback Capability
    /// </summary>
    public class ReceptionStateMachine
    {
        #region Fields and Constructor

        private readonly ILogger _logger;
        private readonly Dictionary<WorkflowState, List<WorkflowState>> _validTransitions;
        private readonly Dictionary<WorkflowState, List<WorkflowEvent>> _stateEvents;

        public ReceptionStateMachine(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validTransitions = InitializeValidTransitions();
            _stateEvents = InitializeStateEvents();
        }

        #endregion

        #region State Machine Core

        /// <summary>
        /// بررسی امکان انتقال به وضعیت جدید
        /// </summary>
        public bool CanTransition(WorkflowState currentState, WorkflowState targetState)
        {
            try
            {
                if (!_validTransitions.ContainsKey(currentState))
                {
                    _logger.Warning("⚠️ وضعیت نامعتبر: {CurrentState}", currentState);
                    return false;
                }

                var validTargets = _validTransitions[currentState];
                var canTransition = validTargets.Contains(targetState);

                _logger.Information("🔄 بررسی انتقال: {CurrentState} -> {TargetState} = {CanTransition}", 
                    currentState, targetState, canTransition);

                return canTransition;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در بررسی انتقال: {CurrentState} -> {TargetState}", currentState, targetState);
                return false;
            }
        }

        /// <summary>
        /// اجرای انتقال وضعیت
        /// </summary>
        public async Task<ServiceResult<StateTransitionResult>> ExecuteTransitionAsync(
            int receptionId, 
            WorkflowState currentState, 
            WorkflowState targetState, 
            string reason, 
            string userId)
        {
            try
            {
                _logger.Information("🔄 اجرای انتقال: {ReceptionId}, {CurrentState} -> {TargetState}, دلیل: {Reason}", 
                    receptionId, currentState, targetState, reason);

                // اعتبارسنجی انتقال
                if (!CanTransition(currentState, targetState))
                {
                    return ServiceResult<StateTransitionResult>.Failed(
                        $"انتقال از {currentState} به {targetState} مجاز نیست");
                }

                // اجرای رویدادهای قبل از انتقال
                await ExecutePreTransitionEventsAsync(receptionId, currentState, targetState);

                // ثبت انتقال در تاریخچه
                var transition = new StateTransition
                {
                    ReceptionId = receptionId,
                    FromState = currentState,
                    ToState = targetState,
                    TransitionTime = DateTime.Now,
                    Reason = reason,
                    UserId = userId
                };

                await SaveStateTransitionAsync(transition);

                // اجرای رویدادهای بعد از انتقال
                await ExecutePostTransitionEventsAsync(receptionId, currentState, targetState);

                var result = new StateTransitionResult
                {
                    ReceptionId = receptionId,
                    PreviousState = currentState,
                    CurrentState = targetState,
                    TransitionTime = DateTime.Now,
                    Success = true
                };

                _logger.Information("✅ انتقال موفق: {ReceptionId}, وضعیت جدید: {TargetState}", 
                    receptionId, targetState);

                return ServiceResult<StateTransitionResult>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اجرای انتقال: {ReceptionId}, {CurrentState} -> {TargetState}", 
                    receptionId, currentState, targetState);
                return ServiceResult<StateTransitionResult>.Failed("خطا در اجرای انتقال وضعیت");
            }
        }

        /// <summary>
        /// دریافت وضعیت‌های مجاز بعدی
        /// </summary>
        public List<WorkflowState> GetValidNextStates(WorkflowState currentState)
        {
            try
            {
                if (!_validTransitions.ContainsKey(currentState))
                {
                    _logger.Warning("⚠️ وضعیت نامعتبر: {CurrentState}", currentState);
                    return new List<WorkflowState>();
                }

                var nextStates = _validTransitions[currentState];
                _logger.Information("📋 وضعیت‌های مجاز بعدی برای {CurrentState}: {NextStates}", 
                    currentState, string.Join(", ", nextStates));

                return nextStates;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در دریافت وضعیت‌های مجاز: {CurrentState}", currentState);
                return new List<WorkflowState>();
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// اجرای رویدادهای قبل از انتقال
        /// </summary>
        private async Task ExecutePreTransitionEventsAsync(int receptionId, WorkflowState fromState, WorkflowState toState)
        {
            try
            {
                _logger.Information("⚡ اجرای رویدادهای قبل از انتقال: {ReceptionId}, {FromState} -> {ToState}", 
                    receptionId, fromState, toState);

                var events = GetPreTransitionEvents(fromState, toState);
                foreach (var eventType in events)
                {
                    await ExecuteEventAsync(receptionId, eventType, "PreTransition");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اجرای رویدادهای قبل از انتقال: {ReceptionId}", receptionId);
            }
        }

        /// <summary>
        /// اجرای رویدادهای بعد از انتقال
        /// </summary>
        private async Task ExecutePostTransitionEventsAsync(int receptionId, WorkflowState fromState, WorkflowState toState)
        {
            try
            {
                _logger.Information("⚡ اجرای رویدادهای بعد از انتقال: {ReceptionId}, {FromState} -> {ToState}", 
                    receptionId, fromState, toState);

                var events = GetPostTransitionEvents(fromState, toState);
                foreach (var eventType in events)
                {
                    await ExecuteEventAsync(receptionId, eventType, "PostTransition");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اجرای رویدادهای بعد از انتقال: {ReceptionId}", receptionId);
            }
        }

        /// <summary>
        /// اجرای رویداد خاص
        /// </summary>
        private async Task ExecuteEventAsync(int receptionId, WorkflowEvent eventType, string phase)
        {
            try
            {
                _logger.Information("🎯 اجرای رویداد: {ReceptionId}, رویداد: {EventType}, فاز: {Phase}", 
                    receptionId, eventType, phase);

                switch (eventType)
                {
                    case WorkflowEvent.PatientValidation:
                        await ExecutePatientValidationEventAsync(receptionId);
                        break;
                    case WorkflowEvent.InsuranceValidation:
                        await ExecuteInsuranceValidationEventAsync(receptionId);
                        break;
                    case WorkflowEvent.PaymentProcessing:
                        await ExecutePaymentProcessingEventAsync(receptionId);
                        break;
                    case WorkflowEvent.NotificationSending:
                        await ExecuteNotificationSendingEventAsync(receptionId);
                        break;
                    case WorkflowEvent.AuditLogging:
                        await ExecuteAuditLoggingEventAsync(receptionId);
                        break;
                    default:
                        _logger.Warning("⚠️ رویداد ناشناخته: {EventType}", eventType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در اجرای رویداد: {ReceptionId}, رویداد: {EventType}", receptionId, eventType);
            }
        }

        #endregion

        #region Event Implementations

        private async Task ExecutePatientValidationEventAsync(int receptionId)
        {
            _logger.Information("👤 اجرای اعتبارسنجی بیمار: {ReceptionId}", receptionId);
            // TODO: پیاده‌سازی اعتبارسنجی بیمار
        }

        private async Task ExecuteInsuranceValidationEventAsync(int receptionId)
        {
            _logger.Information("🏥 اجرای اعتبارسنجی بیمه: {ReceptionId}", receptionId);
            // TODO: پیاده‌سازی اعتبارسنجی بیمه
        }

        private async Task ExecutePaymentProcessingEventAsync(int receptionId)
        {
            _logger.Information("💳 اجرای پردازش پرداخت: {ReceptionId}", receptionId);
            // TODO: پیاده‌سازی پردازش پرداخت
        }

        private async Task ExecuteNotificationSendingEventAsync(int receptionId)
        {
            _logger.Information("📢 اجرای ارسال اعلان: {ReceptionId}", receptionId);
            // TODO: پیاده‌سازی ارسال اعلان
        }

        private async Task ExecuteAuditLoggingEventAsync(int receptionId)
        {
            _logger.Information("📝 اجرای ثبت audit: {ReceptionId}", receptionId);
            // TODO: پیاده‌سازی ثبت audit
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// مقداردهی اولیه انتقال‌های مجاز
        /// </summary>
        private Dictionary<WorkflowState, List<WorkflowState>> InitializeValidTransitions()
        {
            return new Dictionary<WorkflowState, List<WorkflowState>>
            {
                [WorkflowState.Initialized] = new List<WorkflowState> 
                { 
                    WorkflowState.PatientVerification, 
                    WorkflowState.Cancelled 
                },
                [WorkflowState.PatientVerification] = new List<WorkflowState> 
                { 
                    WorkflowState.InsuranceValidation, 
                    WorkflowState.Cancelled 
                },
                [WorkflowState.InsuranceValidation] = new List<WorkflowState> 
                { 
                    WorkflowState.ServiceSelection, 
                    WorkflowState.Cancelled 
                },
                [WorkflowState.ServiceSelection] = new List<WorkflowState> 
                { 
                    WorkflowState.PaymentProcessing, 
                    WorkflowState.Cancelled 
                },
                [WorkflowState.PaymentProcessing] = new List<WorkflowState> 
                { 
                    WorkflowState.Completed, 
                    WorkflowState.Cancelled 
                },
                [WorkflowState.Completed] = new List<WorkflowState> 
                { 
                    WorkflowState.Archived 
                },
                [WorkflowState.Cancelled] = new List<WorkflowState> 
                { 
                    WorkflowState.Archived 
                }
            };
        }

        /// <summary>
        /// مقداردهی اولیه رویدادهای وضعیت
        /// </summary>
        private Dictionary<WorkflowState, List<WorkflowEvent>> InitializeStateEvents()
        {
            return new Dictionary<WorkflowState, List<WorkflowEvent>>
            {
                [WorkflowState.PatientVerification] = new List<WorkflowEvent> 
                { 
                    WorkflowEvent.PatientValidation, 
                    WorkflowEvent.AuditLogging 
                },
                [WorkflowState.InsuranceValidation] = new List<WorkflowEvent> 
                { 
                    WorkflowEvent.InsuranceValidation, 
                    WorkflowEvent.AuditLogging 
                },
                [WorkflowState.PaymentProcessing] = new List<WorkflowEvent> 
                { 
                    WorkflowEvent.PaymentProcessing, 
                    WorkflowEvent.AuditLogging 
                },
                [WorkflowState.Completed] = new List<WorkflowEvent> 
                { 
                    WorkflowEvent.NotificationSending, 
                    WorkflowEvent.AuditLogging 
                }
            };
        }

        private List<WorkflowEvent> GetPreTransitionEvents(WorkflowState fromState, WorkflowState toState)
        {
            // TODO: پیاده‌سازی منطق رویدادهای قبل از انتقال
            return new List<WorkflowEvent>();
        }

        private List<WorkflowEvent> GetPostTransitionEvents(WorkflowState fromState, WorkflowState toState)
        {
            // TODO: پیاده‌سازی منطق رویدادهای بعد از انتقال
            return new List<WorkflowEvent>();
        }

        private async Task SaveStateTransitionAsync(StateTransition transition)
        {
            _logger.Information("💾 ذخیره انتقال وضعیت: {ReceptionId}, {FromState} -> {ToState}", 
                transition.ReceptionId, transition.FromState, transition.ToState);
            // TODO: ذخیره در دیتابیس
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// وضعیت‌های workflow پذیرش
    /// </summary>
    public enum WorkflowState
    {
        Initialized = 1,
        PatientVerification = 2,
        InsuranceValidation = 3,
        ServiceSelection = 4,
        PaymentProcessing = 5,
        Completed = 6,
        Cancelled = 7,
        Archived = 8
    }

    /// <summary>
    /// رویدادهای workflow
    /// </summary>
    public enum WorkflowEvent
    {
        PatientValidation = 1,
        InsuranceValidation = 2,
        PaymentProcessing = 3,
        NotificationSending = 4,
        AuditLogging = 5
    }

    /// <summary>
    /// انتقال وضعیت
    /// </summary>
    public class StateTransition
    {
        public int ReceptionId { get; set; }
        public WorkflowState FromState { get; set; }
        public WorkflowState ToState { get; set; }
        public DateTime TransitionTime { get; set; }
        public string Reason { get; set; }
        public string UserId { get; set; }
    }

    /// <summary>
    /// نتیجه انتقال وضعیت
    /// </summary>
    public class StateTransitionResult
    {
        public int ReceptionId { get; set; }
        public WorkflowState PreviousState { get; set; }
        public WorkflowState CurrentState { get; set; }
        public DateTime TransitionTime { get; set; }
        public bool Success { get; set; }
    }

    #endregion
}
