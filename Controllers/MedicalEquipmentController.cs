using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی برای نمایش تجهیزات پزشکی
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class MedicalEquipmentController : Controller
    {
        private readonly IMedicalEquipmentService _medicalEquipmentService;
        private readonly ILogger _logger;

        public MedicalEquipmentController(IMedicalEquipmentService medicalEquipmentService)
        {
            _medicalEquipmentService = medicalEquipmentService ?? throw new ArgumentNullException(nameof(medicalEquipmentService));
            _logger = Log.ForContext<MedicalEquipmentController>();
        }

        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "page;category;search")]
        public async Task<ActionResult> Index(string category = null, string search = null, int page = 1)
        {
            try
            {
                var filter = new MedicalEquipmentSearchViewModel
                {
                    Category = category,
                    SearchTerm = search,
                    PageNumber = page,
                    PageSize = 12,
                    IsActive = true
                };

                var result = await _medicalEquipmentService.GetMedicalEquipmentsAsync(filter);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست تجهیزات پزشکی: {ErrorMessage}", result.Message);
                    return View(new PagedResult<MedicalEquipmentIndexViewModel>(new System.Collections.Generic.List<MedicalEquipmentIndexViewModel>(), 0, page, 12));
                }

                ViewBag.Category = category;
                ViewBag.Search = search;
                ViewBag.Categories = new System.Collections.Generic.List<SelectListItem>
                {
                    new SelectListItem { Text = "تصویربرداری", Value = "Imaging" },
                    new SelectListItem { Text = "آزمایشگاه", Value = "Laboratory" },
                    new SelectListItem { Text = "جراحی", Value = "Surgery" },
                    new SelectListItem { Text = "مانیتورینگ", Value = "Monitoring" },
                    new SelectListItem { Text = "درمانی", Value = "Therapy" },
                    new SelectListItem { Text = "تشخیصی", Value = "Diagnostic" },
                    new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                    new SelectListItem { Text = "توانبخشی", Value = "Rehabilitation" }
                };

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تجهیزات پزشکی");
                return View(new PagedResult<MedicalEquipmentIndexViewModel>(new System.Collections.Generic.List<MedicalEquipmentIndexViewModel>(), 0, page, 12));
            }
        }

        [HttpGet]
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Details(string slug)
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                {
                    return RedirectToAction("Index");
                }

                var equipmentResult = await _medicalEquipmentService.GetBySlugAsync(slug);
                if (!equipmentResult.Success || equipmentResult.Data == null)
                {
                    _logger.Warning("تجهیز پزشکی یافت نشد - Slug: {Slug}", slug);
                    TempData["Error"] = "تجهیز پزشکی یافت نشد";
                    return RedirectToAction("Index");
                }

                var equipment = equipmentResult.Data;

                // افزایش تعداد بازدید
                await _medicalEquipmentService.IncrementViewCountAsync(equipment.MedicalEquipmentId);

                // تبدیل به ViewModel
                var detailsResult = await _medicalEquipmentService.GetMedicalEquipmentDetailsAsync(equipment.MedicalEquipmentId);
                if (!detailsResult.Success)
                {
                    TempData["Error"] = "خطا در بارگذاری جزئیات تجهیز پزشکی";
                    return RedirectToAction("Index");
                }

                return View(detailsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تجهیز پزشکی - Slug: {Slug}", slug);
                TempData["Error"] = "خطا در بارگذاری جزئیات تجهیز پزشکی";
                return RedirectToAction("Index");
            }
        }
    }
}

