using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Services.Reception;
using Serilog;

namespace ClinicApp.Services.Reception.EventHandlers
{
    /// <summary>
    /// Handler برای ارسال اعلان پیامک
    /// </summary>
    public class SmsNotificationEventHandler : IEventHandler
    {
        private readonly ILogger _logger;

        public SmsNotificationEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HandlerResult> HandleAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("📱 پردازش رویداد ارسال پیامک: پذیرش {ReceptionId}", eventData.ReceptionId);

                // TODO: پیاده‌سازی ارسال پیامک
                // مثال: ارسال پیامک تایید، اطلاع‌رسانی، کد تایید

                await Task.Delay(250); // شبیه‌سازی ارسال پیامک

                _logger.Information("✅ رویداد ارسال پیامک پردازش شد: پذیرش {ReceptionId}", eventData.ReceptionId);

                return new HandlerResult
                {
                    HandlerType = nameof(SmsNotificationEventHandler),
                    Success = true,
                    ResultData = new { Message = "ارسال پیامک تکمیل شد" }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد ارسال پیامک: پذیرش {ReceptionId}", eventData.ReceptionId);
                return new HandlerResult
                {
                    HandlerType = nameof(SmsNotificationEventHandler),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
