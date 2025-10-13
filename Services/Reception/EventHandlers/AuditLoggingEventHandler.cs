using System;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Services.Reception;
using Serilog;

namespace ClinicApp.Services.Reception.EventHandlers
{
    /// <summary>
    /// Handler برای رویداد ثبت audit
    /// </summary>
    public class AuditLoggingEventHandler : IEventHandler
    {
        private readonly ILogger _logger;

        public AuditLoggingEventHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HandlerResult> HandleAsync(WorkflowEventData eventData)
        {
            try
            {
                _logger.Information("📝 پردازش رویداد ثبت audit: پذیرش {ReceptionId}", eventData.ReceptionId);

                // TODO: پیاده‌سازی ثبت audit
                // مثال: ثبت در دیتابیس، ارسال به سیستم audit، ثبت در فایل

                await Task.Delay(50); // شبیه‌سازی پردازش

                _logger.Information("✅ رویداد ثبت audit پردازش شد: پذیرش {ReceptionId}", eventData.ReceptionId);

                return new HandlerResult
                {
                    HandlerType = nameof(AuditLoggingEventHandler),
                    Success = true,
                    ResultData = new { Message = "ثبت audit تکمیل شد" }
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در پردازش رویداد ثبت audit: پذیرش {ReceptionId}", eventData.ReceptionId);
                return new HandlerResult
                {
                    HandlerType = nameof(AuditLoggingEventHandler),
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
