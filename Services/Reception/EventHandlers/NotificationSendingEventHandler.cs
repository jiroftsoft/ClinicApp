using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Services.Reception;
using Serilog;

namespace ClinicApp.Services.Reception.EventHandlers
{
    /// <summary>
    /// Handler برای رویداد ارسال اعلان
    /// </summary>
    public class NotificationSendingEventHandler : IEventHandler
    {
        private readonly ILogger _logger;

        public NotificationSendingEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HandlerResult> HandleAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("📢 پردازش رویداد ارسال اعلان: پذیرش {ReceptionId}", eventData.ReceptionId);

                // TODO: پیاده‌سازی ارسال اعلان
                // مثال: ارسال ایمیل، پیامک، push notification

                await Task.Delay(100); // شبیه‌سازی پردازش

                _logger.Information("✅ رویداد ارسال اعلان پردازش شد: پذیرش {ReceptionId}", eventData.ReceptionId);

                return new HandlerResult
                {
                    HandlerType = nameof(NotificationSendingEventHandler),
                    Success = true,
                    ResultData = new { Message = "ارسال اعلان تکمیل شد" }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد ارسال اعلان: پذیرش {ReceptionId}", eventData.ReceptionId);
                return new HandlerResult
                {
                    HandlerType = nameof(NotificationSendingEventHandler),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
