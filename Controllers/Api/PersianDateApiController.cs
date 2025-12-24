using System;
using System.Web.Mvc;
using ClinicApp.Helpers;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller برای مدیریت تاریخ شمسی
    /// طبق استانداردهای فرم‌های درمانی سطح سازمانی
    /// </summary>
    [RoutePrefix("api/persian-date")]
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class PersianDateApiController : Controller
    {
        /// <summary>
        /// دریافت تاریخ امروز شمسی از سرور
        /// GET: /api/persian-date/today
        /// 
        /// این endpoint برای اطمینان از صحت تاریخ امروز در client-side استفاده می‌شود
        /// </summary>
        /// <returns>JSON response با تاریخ امروز شمسی</returns>
        [HttpGet]
        [Route("today")]
        [AllowAnonymous]
        public JsonResult GetToday()
        {
            try
            {
                // ✅ استفاده از DateTime.Today برای فقط تاریخ (بدون زمان)
                var today = DateTime.Today;
                var persianToday = PersianDateHelper.ToPersianDate(today);
                
                // ✅ بررسی صحت تبدیل
                if (string.IsNullOrEmpty(persianToday) || persianToday == "0000/00/00")
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطا در محاسبه تاریخ امروز"
                    }, JsonRequestBehavior.AllowGet);
                }

                // ✅ تبدیل به میلادی برای استفاده در DatePicker
                var gregorianToday = today.ToString("yyyy-MM-dd");

                return Json(new
                {
                    success = true,
                    persianDate = persianToday,
                    gregorianDate = gregorianToday,
                    timestamp = (long)(today - new DateTime(1970, 1, 1)).TotalSeconds
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // ✅ Logging خطا
                Serilog.Log.Error(ex, "خطا در دریافت تاریخ امروز شمسی");
                
                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت تاریخ امروز"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

