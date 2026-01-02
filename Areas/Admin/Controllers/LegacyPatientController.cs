using ClinicApp.Helpers;
using ClinicApp.Models;
using ClinicApp.Services.Patient;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// مدیریت بیماران Legacy و ارسال خوش‌آمدگویی
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class LegacyPatientController : Controller
    {
        private readonly LegacyPatientWelcomeService _welcomeService;

        public LegacyPatientController()
        {
            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(context));
            _welcomeService = new LegacyPatientWelcomeService(context, userManager);
        }

        // GET: Admin/LegacyPatient
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// API: دریافت آمار بیماران Legacy
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStatistics()
        {
            var result = await _welcomeService.GetLegacyPatientsStatisticsAsync();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// API: ارسال پیامک خوش‌آمدگویی به بیماران Legacy
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendWelcomeNotifications()
        {
            var result = await _welcomeService.SendWelcomeNotificationsAsync();
            return Json(result);
        }
    }
}

