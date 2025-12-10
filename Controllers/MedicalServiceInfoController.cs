using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی Medical Service Info برای نمایش در سایت
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class MedicalServiceInfoController : Controller
    {
        private readonly IMedicalServiceInfoService _medicalServiceInfoService;
        private readonly ILogger _logger;

        public MedicalServiceInfoController(IMedicalServiceInfoService medicalServiceInfoService)
        {
            _medicalServiceInfoService = medicalServiceInfoService ?? throw new ArgumentNullException(nameof(medicalServiceInfoService));
            _logger = Log.ForContext<MedicalServiceInfoController>();
        }

        /// <summary>
        /// صفحه اصلی Medical Service Info با فیلتر دسته‌بندی
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "serviceCategoryId")]
        public async Task<ActionResult> Index(int? serviceCategoryId = null)
        {
            try
            {
                var servicesResult = await _medicalServiceInfoService.GetPublicServiceInfosAsync(serviceCategoryId);

                ViewBag.SelectedCategoryId = serviceCategoryId;

                if (!servicesResult.Success)
                {
                    return View(new System.Collections.Generic.List<MedicalServiceInfoPublicViewModel>());
                }

                return View(servicesResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه خدمات پزشکی");
                return View(new System.Collections.Generic.List<MedicalServiceInfoPublicViewModel>());
            }
        }

        /// <summary>
        /// نمایش یک Medical Service Info بر اساس Slug
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Details(string slug)
        {
            try
            {
                var result = await _medicalServiceInfoService.GetBySlugAsync(slug);
                if (!result.Success || result.Data == null)
                {
                    return HttpNotFound();
                }

                // افزایش تعداد بازدید
                await _medicalServiceInfoService.IncrementViewCountAsync(result.Data.MedicalServiceInfoId);

                var viewModel = new MedicalServiceInfoPublicViewModel
                {
                    MedicalServiceInfoId = result.Data.MedicalServiceInfoId,
                    ServiceId = result.Data.ServiceId,
                    ServiceTitle = result.Data.Service.Title,
                    ServiceCode = result.Data.Service.ServiceCode,
                    ServiceCategoryName = result.Data.Service.ServiceCategory?.Title ?? "بدون دسته‌بندی",
                    Description = result.Data.Description,
                    FullDescription = result.Data.FullDescription,
                    Features = !string.IsNullOrEmpty(result.Data.Features) 
                        ? result.Data.Features.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(f => f.Trim())
                            .ToList()
                        : new System.Collections.Generic.List<string>(),
                    ImageUrl = result.Data.ImageUrl,
                    ThumbnailUrl = result.Data.ThumbnailUrl,
                    VideoUrl = result.Data.VideoUrl,
                    Price = result.Data.Price ?? result.Data.Service.Price,
                    ServicePrice = result.Data.Service.Price,
                    InsuranceCoverage = !string.IsNullOrEmpty(result.Data.InsuranceCoverage) 
                        ? result.Data.InsuranceCoverage.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList()
                        : new System.Collections.Generic.List<string>(),
                    EstimatedDuration = result.Data.EstimatedDuration,
                    RequiredDocuments = !string.IsNullOrEmpty(result.Data.RequiredDocuments) 
                        ? result.Data.RequiredDocuments.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => d.Trim())
                            .ToList()
                        : new System.Collections.Generic.List<string>(),
                    ViewCount = result.Data.ViewCount + 1,
                    Slug = result.Data.Slug,
                    RelatedLinkUrl = result.Data.RelatedLinkUrl
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات خدمت پزشکی - Slug: {Slug}", slug);
                return HttpNotFound();
            }
        }

        /// <summary>
        /// جستجوی Medical Service Info (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Search(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = false, message = "لطفاً کلمه‌ای برای جستجو وارد کنید" });
                }

                var result = await _medicalServiceInfoService.SearchServiceInfosAsync(searchTerm);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی خدمات پزشکی");
                return Json(new { success = false, message = "خطا در جستجو" });
            }
        }

        /// <summary>
        /// افزایش تعداد بازدید (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> IncrementView(int id)
        {
            try
            {
                await _medicalServiceInfoService.IncrementViewCountAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                // خطا در افزایش ViewCount نباید باعث مشکل شود
                return Json(new { success = true });
            }
        }
    }
}

