using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی Insurance Info برای نمایش در سایت
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class InsuranceInfoController : Controller
    {
        private readonly IInsuranceInfoService _insuranceInfoService;
        private readonly ILogger _logger;

        public InsuranceInfoController(IInsuranceInfoService insuranceInfoService)
        {
            _insuranceInfoService = insuranceInfoService ?? throw new ArgumentNullException(nameof(insuranceInfoService));
            _logger = Log.ForContext<InsuranceInfoController>();
        }

        /// <summary>
        /// صفحه اصلی Insurance Info با فیلتر نوع بیمه
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "insuranceType")]
        public async Task<ActionResult> Index(string insuranceType = null)
        {
            try
            {
                var insurancesResult = await _insuranceInfoService.GetPublicInsuranceInfosAsync(insuranceType);
                var typesResult = await _insuranceInfoService.GetInsuranceTypesAsync();

                var viewModel = new InsuranceInfoIndexPageViewModel
                {
                    InsuranceInfos = insurancesResult.Success && insurancesResult.Data != null 
                        ? insurancesResult.Data 
                        : new System.Collections.Generic.List<InsuranceInfoPublicViewModel>(),
                    InsuranceTypes = typesResult.Success && typesResult.Data != null 
                        ? typesResult.Data 
                        : new System.Collections.Generic.List<InsuranceInfoTypeViewModel>(),
                    SelectedType = insuranceType
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه اطلاعات بیمه");
                return View(new InsuranceInfoIndexPageViewModel
                {
                    InsuranceInfos = new System.Collections.Generic.List<InsuranceInfoPublicViewModel>(),
                    InsuranceTypes = new System.Collections.Generic.List<InsuranceInfoTypeViewModel>(),
                    SelectedType = insuranceType
                });
            }
        }

        /// <summary>
        /// نمایش یک Insurance Info بر اساس Slug
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Details(string slug)
        {
            try
            {
                var result = await _insuranceInfoService.GetBySlugAsync(slug);
                if (!result.Success || result.Data == null)
                {
                    return HttpNotFound();
                }

                // افزایش تعداد بازدید
                await _insuranceInfoService.IncrementViewCountAsync(result.Data.InsuranceInfoId);

                var viewModel = new InsuranceInfoPublicViewModel
                {
                    InsuranceInfoId = result.Data.InsuranceInfoId,
                    InsuranceName = result.Data.InsuranceName,
                    InsuranceType = result.Data.InsuranceType,
                    TypeDisplayName = GetTypeDisplayName(result.Data.InsuranceType),
                    Description = result.Data.Description,
                    FullDescription = result.Data.FullDescription,
                    LogoUrl = result.Data.LogoUrl,
                    ThumbnailUrl = result.Data.ThumbnailUrl,
                    ContactPhone = result.Data.ContactPhone,
                    WebsiteUrl = result.Data.WebsiteUrl,
                    Address = result.Data.Address,
                    CoveragePercentage = result.Data.CoveragePercentage,
                    ViewCount = result.Data.ViewCount + 1,
                    Slug = result.Data.Slug
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اطلاعات بیمه - Slug: {Slug}", slug);
                return HttpNotFound();
            }
        }

        /// <summary>
        /// جستجوی Insurance Info (AJAX)
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

                var result = await _insuranceInfoService.SearchInsuranceInfosAsync(searchTerm);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی اطلاعات بیمه");
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
                await _insuranceInfoService.IncrementViewCountAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                // خطا در افزایش ViewCount نباید باعث مشکل شود
                return Json(new { success = true });
            }
        }

        #region Helper Methods

        private string GetTypeDisplayName(string insuranceType)
        {
            return insuranceType switch
            {
                "basic" => "بیمه پایه",
                "supplementary" => "بیمه تکمیلی",
                "private" => "بیمه خصوصی",
                "government" => "بیمه دولتی",
                _ => insuranceType ?? "عمومی"
            };
        }

        #endregion
    }
}

