using System;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Reception;
using Serilog;

namespace ClinicApp.Services.Reception.EventHandlers
{
    /// <summary>
    /// Handler برای ارسال اعلان push
    /// </summary>
    public class PushNotificationEventHandler : IEventHandler
    {
        private readonly ILogger _logger;

        public PushNotificationEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HandlerResult> HandleAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("🔔 پردازش رویداد ارسال push notification: پذیرش {ReceptionId}", eventData.ReceptionId);

                // TODO: پیاده‌سازی ارسال push notification
                // مثال: ارسال اعلان به اپلیکیشن موبایل، وب، دسکتاپ

                await Task.Delay(200); // شبیه‌سازی ارسال push notification

                _logger.Information("✅ رویداد ارسال push notification پردازش شد: پذیرش {ReceptionId}", eventData.ReceptionId);

                return new HandlerResult
                {
                    HandlerType = nameof(PushNotificationEventHandler),
                    Success = true,
                    ResultData = new { Message = "ارسال push notification تکمیل شد" }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد ارسال push notification: پذیرش {ReceptionId}", eventData.ReceptionId);
                return new HandlerResult
                {
                    HandlerType = nameof(PushNotificationEventHandler),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
