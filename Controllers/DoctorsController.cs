using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی صفحه پزشکان (مسیر /Doctors).
    /// طبق SRP فقط مسئول ریدایرکت به صفحه لیست پزشکان در ناحیه Patient است.
    /// </summary>
    public class DoctorsController : Controller
    {
        /// <summary>
        /// صفحه اصلی پزشکان: ریدایرکت به لیست پزشکان قابل نوبت‌گیری در ناحیه Patient.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Available", "Appointment", new { area = "Patient" });
        }
    }
}
