using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// Base Controller برای همه CMS Controllers
    /// این کلاس متد GetViewPath را فراهم می‌کند تا مشکل case-sensitivity در MVC View resolution حل شود
    /// </summary>
    public abstract class BaseCMSController : Controller
    {
        /// <summary>
        /// Helper method برای برگرداندن View path صحیح
        /// این کار برای حل مشکل case-sensitivity در MVC View resolution است
        /// </summary>
        /// <param name="viewName">نام View (مثلاً "Index", "Create", "Edit")</param>
        /// <returns>مسیر کامل View</returns>
        protected string GetViewPath(string viewName)
        {
            // نام Controller را از نوع کلاس فعلی استخراج می‌کنیم
            string controllerName = GetType().Name.Replace("Controller", "");
            return $"~/Areas/Admin/Views/CMS/{controllerName}/{viewName}.cshtml";
        }
    }
}

