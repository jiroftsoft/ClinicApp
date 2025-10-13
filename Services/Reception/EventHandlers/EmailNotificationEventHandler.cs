using System;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Reception;
using Serilog;

namespace ClinicApp.Services.Reception.EventHandlers
{
    /// <summary>
    /// Handler برای ارسال اعلان ایمیل
    /// </summary>
    public class EmailNotificationEventHandler : IEventHandler
    {
        private readonly ILogger _logger;

        public EmailNotificationEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HandlerResult> HandleAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("📧 پردازش رویداد ارسال ایمیل: پذیرش {ReceptionId}", eventData.ReceptionId);

                // TODO: پیاده‌سازی ارسال ایمیل
                // مثال: ارسال ایمیل تایید، اطلاع‌رسانی، گزارش

                await Task.Delay(300); // شبیه‌سازی ارسال ایمیل

                _logger.Information("✅ رویداد ارسال ایمیل پردازش شد: پذیرش {ReceptionId}", eventData.ReceptionId);

                return new HandlerResult
                {
                    HandlerType = nameof(EmailNotificationEventHandler),
                    Success = true,
                    ResultData = new { Message = "ارسال ایمیل تکمیل شد" }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد ارسال ایمیل: پذیرش {ReceptionId}", eventData.ReceptionId);
                return new HandlerResult
                {
                    HandlerType = nameof(EmailNotificationEventHandler),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
