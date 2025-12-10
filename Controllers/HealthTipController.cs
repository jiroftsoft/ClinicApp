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
    /// کنترلر عمومی Health Tips برای نمایش در سایت
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class HealthTipController : Controller
    {
        private readonly IHealthTipService _healthTipService;
        private readonly ILogger _logger;

        public HealthTipController(IHealthTipService healthTipService)
        {
            _healthTipService = healthTipService ?? throw new ArgumentNullException(nameof(healthTipService));
            _logger = Log.ForContext<HealthTipController>();
        }

        /// <summary>
        /// صفحه اصلی Health Tips با دسته‌بندی‌ها
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "category")]
        public async Task<ActionResult> Index(string category = null)
        {
            try
            {
                var tipsResult = await _healthTipService.GetPublicHealthTipsAsync(category, 20);
                var categoriesResult = await _healthTipService.GetCategoriesAsync();

                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<HealthTipCategoryViewModel>();
                ViewBag.SelectedCategory = category;

                if (!tipsResult.Success)
                {
                    return View(new System.Collections.Generic.List<HealthTipPublicViewModel>());
                }

                return View(tipsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه نکات سلامت");
                return View(new System.Collections.Generic.List<HealthTipPublicViewModel>());
            }
        }

        /// <summary>
        /// نمایش یک Health Tip بر اساس Slug
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Details(string slug)
        {
            try
            {
                var result = await _healthTipService.GetBySlugAsync(slug);
                if (!result.Success || result.Data == null)
                {
                    return HttpNotFound();
                }

                // افزایش تعداد بازدید
                await _healthTipService.IncrementViewCountAsync(result.Data.HealthTipId);

                var viewModel = new HealthTipPublicViewModel
                {
                    HealthTipId = result.Data.HealthTipId,
                    Title = result.Data.Title,
                    Summary = result.Data.Summary,
                    Content = result.Data.Content,
                    ImageUrl = result.Data.ImageUrl,
                    ThumbnailUrl = result.Data.ThumbnailUrl,
                    Category = result.Data.Category,
                    CategoryDisplayName = GetCategoryDisplayName(result.Data.Category),
                    Tags = !string.IsNullOrEmpty(result.Data.Tags) 
                        ? result.Data.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new System.Collections.Generic.List<string>(),
                    PublishedAt = result.Data.PublishedAt,
                    ExpiryDate = result.Data.ExpiryDate,
                    ViewCount = result.Data.ViewCount + 1,
                    ShareCount = result.Data.ShareCount,
                    Slug = result.Data.Slug,
                    RelatedLinkUrl = result.Data.RelatedLinkUrl
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات نکته سلامت - Slug: {Slug}", slug);
                return HttpNotFound();
            }
        }

        /// <summary>
        /// جستجوی Health Tips (AJAX)
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

                var result = await _healthTipService.SearchHealthTipsAsync(searchTerm);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی نکات سلامت");
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
                await _healthTipService.IncrementViewCountAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                // خطا در افزایش ViewCount نباید باعث مشکل شود
                return Json(new { success = true });
            }
        }

        /// <summary>
        /// افزایش تعداد اشتراک‌گذاری (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> IncrementShare(int id)
        {
            try
            {
                await _healthTipService.IncrementShareCountAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                // خطا در افزایش ShareCount نباید باعث مشکل شود
                return Json(new { success = true });
            }
        }

        #region Helper Methods

        private string GetCategoryDisplayName(string category)
        {
            return category switch
            {
                "prevention" => "پیشگیری",
                "nutrition" => "تغذیه",
                "exercise" => "ورزش",
                "diseases" => "بیماری‌ها",
                "general" => "عمومی",
                _ => category ?? "عمومی"
            };
        }

        #endregion
    }
}

