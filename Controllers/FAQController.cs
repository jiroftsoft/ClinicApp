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
    /// کنترلر عمومی FAQ برای نمایش در سایت
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    public class FAQController : Controller
    {
        private readonly IFAQService _faqService;
        private readonly ILogger _logger;

        public FAQController(IFAQService faqService)
        {
            _faqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
            _logger = Log.ForContext<FAQController>();
        }

        /// <summary>
        /// صفحه اصلی FAQ با دسته‌بندی‌ها
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "category")]
        public async Task<ActionResult> Index(string category = null)
        {
            try
            {
                var faqsResult = await _faqService.GetPublicFAQsAsync(category);
                var categoriesResult = await _faqService.GetCategoriesAsync();

                ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new System.Collections.Generic.List<FAQCategoryViewModel>();
                ViewBag.SelectedCategory = category;

                if (!faqsResult.Success)
                {
                    return View(new System.Collections.Generic.List<FAQPublicViewModel>());
                }

                return View(faqsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه FAQ");
                return View(new System.Collections.Generic.List<FAQPublicViewModel>());
            }
        }

        /// <summary>
        /// نمایش یک FAQ بر اساس Slug
        /// </summary>
        [OutputCache(Duration = 600, VaryByParam = "slug")]
        public async Task<ActionResult> Details(string slug)
        {
            try
            {
                var result = await _faqService.GetBySlugAsync(slug);
                if (!result.Success || result.Data == null)
                {
                    return HttpNotFound();
                }

                // افزایش تعداد بازدید
                await _faqService.IncrementViewCountAsync(result.Data.FAQId);

                var viewModel = new FAQPublicViewModel
                {
                    FAQId = result.Data.FAQId,
                    Question = result.Data.Question,
                    Answer = result.Data.Answer,
                    Category = result.Data.Category,
                    CategoryDisplayName = GetCategoryDisplayName(result.Data.Category),
                    Tags = !string.IsNullOrEmpty(result.Data.Tags) 
                        ? result.Data.Tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim())
                            .ToList()
                        : new System.Collections.Generic.List<string>(),
                    RelatedLinkUrl = result.Data.RelatedLinkUrl,
                    ViewCount = result.Data.ViewCount + 1,
                    Slug = result.Data.Slug
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات FAQ - Slug: {Slug}", slug);
                return HttpNotFound();
            }
        }

        /// <summary>
        /// جستجوی FAQ (AJAX)
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

                var result = await _faqService.SearchFAQsAsync(searchTerm);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی FAQ");
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
                await _faqService.IncrementViewCountAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                // خطا در افزایش ViewCount نباید باعث مشکل شود
                return Json(new { success = true });
            }
        }

        #region Helper Methods

        private string GetCategoryDisplayName(string category)
        {
            return category switch
            {
                "appointment" => "نوبت‌دهی",
                "insurance" => "بیمه",
                "services" => "خدمات",
                "costs" => "هزینه‌ها",
                "general" => "عمومی",
                _ => category ?? "عمومی"
            };
        }

        #endregion
    }
}

