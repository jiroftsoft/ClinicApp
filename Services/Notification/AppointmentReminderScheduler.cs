using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Models;
using ClinicApp.Infrastructure;
using ClinicApp.Models.Enums;
using Serilog;
using AppointmentNotificationType = ClinicApp.Models.Enums.NotificationType;

namespace ClinicApp.Services.Notification
{
    /// <summary>
    /// زمان‌بندی یادآوری نوبت — توسط Hangfire Recurring Job فراخوانی شود (مثلاً هر ۱۵ دقیقه).
    /// نوبت‌های Scheduled را در بازه 24h، 3h، 30min پیدا می‌کند و یک یادآوری برای هر کدام Enqueue می‌کند.
    /// </summary>
    public class AppointmentReminderScheduler
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentNotificationQueueService _notificationService;
        private readonly ITimeProvider _timeProvider;
        private readonly ILogger _logger;

        public AppointmentReminderScheduler(
            ApplicationDbContext context,
            IAppointmentNotificationQueueService notificationService,
            ITimeProvider timeProvider,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger?.ForContext<AppointmentReminderScheduler>() ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// نوبت‌هایی که در 23 تا 25 ساعت آینده هستند → Enqueue یادآوری 24h
        /// </summary>
        public async Task Schedule24HourRemindersAsync()
        {
            var now = _timeProvider.GetIranNow();
            var from = now.AddHours(23);
            var to = now.AddHours(25);
            var appointmentIds = await GetUpcomingAppointmentIdsAsync(from, to);
            foreach (var id in appointmentIds)
            {
                try
                {
                    await _notificationService.EnqueueAppointmentReminderAsync(id, AppointmentNotificationType.AppointmentReminder24h);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "خطا در Enqueue یادآوری 24h - AppointmentId: {AppointmentId}", id);
                }
            }
            if (appointmentIds.Count > 0)
                _logger.Information("یادآوری 24h برای {Count} نوبت زمان‌بندی شد", appointmentIds.Count);
        }

        /// <summary>
        /// نوبت‌هایی که در 2.5 تا 3.5 ساعت آینده هستند → Enqueue یادآوری 3h
        /// </summary>
        public async Task Schedule3HourRemindersAsync()
        {
            var now = _timeProvider.GetIranNow();
            var from = now.AddHours(2.5);
            var to = now.AddHours(3.5);
            var appointmentIds = await GetUpcomingAppointmentIdsAsync(from, to);
            foreach (var id in appointmentIds)
            {
                try
                {
                    await _notificationService.EnqueueAppointmentReminderAsync(id, AppointmentNotificationType.AppointmentReminder3h);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "خطا در Enqueue یادآوری 3h - AppointmentId: {AppointmentId}", id);
                }
            }
            if (appointmentIds.Count > 0)
                _logger.Information("یادآوری 3h برای {Count} نوبت زمان‌بندی شد", appointmentIds.Count);
        }

        /// <summary>
        /// نوبت‌هایی که در 25 تا 35 دقیقه آینده هستند → Enqueue یادآوری 30min
        /// </summary>
        public async Task Schedule30MinuteRemindersAsync()
        {
            var now = _timeProvider.GetIranNow();
            var from = now.AddMinutes(25);
            var to = now.AddMinutes(35);
            var appointmentIds = await GetUpcomingAppointmentIdsAsync(from, to);
            foreach (var id in appointmentIds)
            {
                try
                {
                    await _notificationService.EnqueueAppointmentReminderAsync(id, AppointmentNotificationType.AppointmentReminder30min);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "خطا در Enqueue یادآوری 30min - AppointmentId: {AppointmentId}", id);
                }
            }
            if (appointmentIds.Count > 0)
                _logger.Information("یادآوری 30min برای {Count} نوبت زمان‌بندی شد", appointmentIds.Count);
        }

        private async Task<List<int>> GetUpcomingAppointmentIdsAsync(DateTime from, DateTime to)
        {
            return await _context.Appointments
                .Where(a => !a.IsDeleted &&
                            a.Status == AppointmentStatus.Scheduled &&
                            a.PatientId != null &&
                            a.AppointmentDate >= from &&
                            a.AppointmentDate < to)
                .Select(a => a.AppointmentId)
                .ToListAsync();
        }
    }
}
