using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Notification;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Notification;
using ClinicApp.Models.Enums;
using Microsoft.AspNet.Identity;
using Serilog;

namespace ClinicApp.Services.Notification
{
    /// <summary>
    /// پردازش صف اعلان — توسط Hangfire یا Hosted Job فراخوانی شود.
    /// آیتم‌های Queued با ScheduledTime == null یا &lt;= now را ارسال می‌کند و وضعیت را به‌روز می‌کند.
    /// </summary>
    public class NotificationQueueProcessor
    {
        private readonly INotificationQueueRepository _queueRepository;
        private readonly IIdentityMessageService _smsService;
        private readonly ILogger _logger;
        private const int BatchSize = 50;
        private const int MaxRetries = 3;

        public NotificationQueueProcessor(
            INotificationQueueRepository queueRepository,
            IIdentityMessageService smsService,
            ILogger logger)
        {
            _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger?.ForContext<NotificationQueueProcessor>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessPendingAsync()
        {
            var pending = await _queueRepository.GetPendingBatchAsync(BatchSize);
            var scheduledDue = await _queueRepository.GetScheduledDueBatchAsync(BatchSize);

            var toProcess = new List<NotificationQueueItem>();
            toProcess.AddRange(pending);
            foreach (var s in scheduledDue)
            {
                if (toProcess.All(p => p.Id != s.Id))
                    toProcess.Add(s);
            }

            foreach (var item in toProcess)
            {
                await ProcessOneAsync(item);
            }
        }

        private async Task ProcessOneAsync(NotificationQueueItem item)
        {
            try
            {
                if (item.Channel == NotificationChannelType.Sms)
                {
                    if (string.IsNullOrWhiteSpace(item.Recipient))
                    {
                        item.Status = NotificationStatus.Failed;
                        item.ErrorLog = "شماره گیرنده (Recipient) خالی است.";
                        await _queueRepository.UpdateAsync(item);
                        _logger.Warning("اعلان رد شد - Id: {Id}, Type: {Type}, Reason: Recipient empty", item.Id, item.NotificationType);
                        return;
                    }
                }

                item.Status = NotificationStatus.Sending;
                await _queueRepository.UpdateAsync(item);

                if (item.Channel == NotificationChannelType.Sms)
                {
                    var message = new IdentityMessage
                    {
                        Destination = item.Recipient,
                        Body = item.Message ?? ""
                    };
                    await _smsService.SendAsync(message);
                }
                else if (item.Channel == NotificationChannelType.Email)
                {
                    // TODO: تزریق IEmailService جدا برای ارسال ایمیل
                    item.Status = NotificationStatus.Failed;
                    item.ErrorLog = "Email provider not configured";
                    await _queueRepository.UpdateAsync(item);
                    _logger.Warning("ارسال Email از صف پشتیبانی نشد - Id: {Id}. برای فعال‌سازی IEmailService را ثبت کنید.", item.Id);
                    return;
                }

                item.Status = NotificationStatus.Sent;
                item.SentTime = DateTime.UtcNow;
                item.ErrorLog = null;
                await _queueRepository.UpdateAsync(item);
                _logger.Information("اعلان ارسال شد - Id: {Id}, Type: {Type}, Channel: {Channel}, Recipient: {Recipient}, AppointmentId: {AppointmentId}",
                    item.Id, item.NotificationType, item.Channel, MaskPhone(item.Recipient), item.AppointmentId);
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.ErrorLog = ex.Message;
                if (item.RetryCount >= item.MaxRetries)
                {
                    item.Status = NotificationStatus.Failed;
                    _logger.Error(ex, "اعلان پس از {Retries} تلاش ناموفق - Id: {Id}, Type: {Type}, AppointmentId: {AppointmentId}, Error: {Error}",
                        item.MaxRetries, item.Id, item.NotificationType, item.AppointmentId, ex.Message);
                }
                else
                {
                    item.Status = NotificationStatus.Queued;
                    _logger.Warning(ex, "خطا در ارسال اعلان - Id: {Id}, Type: {Type}, Retry: {Retry}/{Max}", item.Id, item.NotificationType, item.RetryCount, item.MaxRetries);
                }
                await _queueRepository.UpdateAsync(item);
            }
        }

        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 5) return "***";
            return phone.Substring(0, 3) + "***" + phone.Substring(phone.Length - 2);
        }
    }
}
