using System.Web.Mvc;
using ClinicApp.Services;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Base Controller برای عملیات مشترک Admin Area
    /// </summary>
    public class BaseController : Controller
    {
        private readonly IMessageNotificationService _messageNotificationService;

        public BaseController(IMessageNotificationService messageNotificationService)
        {
            _messageNotificationService = messageNotificationService;
        }

        /// <summary>
        /// پاک کردن پیام‌های اعلان از Session
        /// </summary>
        [HttpPost]
        public ActionResult ClearNotifications()
        {
            try
            {
                _messageNotificationService.ClearMessages();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
