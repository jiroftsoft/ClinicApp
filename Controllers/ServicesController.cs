using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی خدمات - ریدایرکت به صفحه جزئیات MedicalServiceInfo بر اساس ServiceId
    /// لینک‌های صفحه اصلی (بخش خدمات) به /Services/Details/{id} اشاره می‌کنند؛
    /// این کنترلر با یافتن slug مربوطه، کاربر را به /MedicalServiceInfo/Details/{slug} هدایت می‌کند.
    /// </summary>
    public class ServicesController : Controller
    {
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly ILogger _logger;

        public ServicesController(IMedicalServiceInfoService medicalServiceInfoService)
        {
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _logger = Log.ForContext<ServicesController>();
        }

        /// <summary>
        /// جزئیات خدمت بر اساس ServiceId - ریدایرکت به صفحه جزئیات با slug
        /// مسیر درخواستی: /Services/Details/2449
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction("Index", "MedicalServiceInfo");
            }

            try
            {
                var result = await _medicalServiceInfoService.GetByServiceIdAsync(id);
                if (result.Success && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.Slug))
                {
                    return RedirectToAction("Details", "MedicalServiceInfo", new { slug = result.Data.Slug });
                }

                _logger.Warning("MedicalServiceInfo یافت نشد برای ServiceId: {ServiceId}", id);
                NotificationHelper.SetWarning(TempData, "صفحه جزئیات این خدمت در حال حاضر موجود نیست.");
                return RedirectToAction("Index", "MedicalServiceInfo");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در هدایت به جزئیات خدمت - ServiceId: {ServiceId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات خدمت.");
                return RedirectToAction("Index", "MedicalServiceInfo");
            }
        }
    }
}
