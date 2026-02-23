using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// صفحهٔ ورود CMS — نمایش گرید ماژول‌ها برای /Admin/CMS
    /// بهینه‌سازی: یک نقطهٔ ورود واحد به مدیریت محتوا
    /// </summary>
    public class CmsHomeController : BaseCMSController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Areas/Admin/Views/CMS/CmsHome/Index.cshtml");
        }
    }
}
