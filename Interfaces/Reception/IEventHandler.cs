using System.Threading.Tasks;
using ClinicApp.Services.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// Interface برای Event Handler ها
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// پردازش رویداد
        /// </summary>
        /// <param name="eventData">داده‌های رویداد</param>
        /// <returns>نتیجه پردازش</returns>
        Task<HandlerResult> HandleAsync(WorkflowEventData eventData);
    }
}
