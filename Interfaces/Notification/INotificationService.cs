using System.Threading.Tasks;
using ClinicApp.Models.Enums;

namespace ClinicApp.Interfaces.Notification;

/// <summary>
/// سرویس صف اعلان نوبت — Event-based، فقط بعد از Commit فراخوانی شود.
/// مسئول: Enqueue کردن اعلان رزرو/پرداخت و زمان‌بندی یادآوری.
/// </summary>
public interface IAppointmentNotificationQueueService
{
    /// <summary>بعد از Commit رزرو نوبت — اعلان فوری + زمان‌بندی یادآوری 24h, 3h, 30min</summary>
    Task EnqueueAppointmentBookingConfirmationAsync(int appointmentId);

    /// <summary>بعد از Commit پرداخت موفق — اعلان تأیید پرداخت</summary>
    Task EnqueuePaymentConfirmationAsync(int appointmentId);

    /// <summary>ثبت یک یادآوری زمان‌بندی‌شده (از Hangfire Job فراخوانی می‌شود — هر بار یک نوع: 24h، 3h، 30min)</summary>
    Task EnqueueAppointmentReminderAsync(int appointmentId, NotificationType reminderType);
}
