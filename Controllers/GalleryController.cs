using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی گالری تصاویر برای کاربران سایت
    /// نمایش گالری محیط کلینیک بدون نیاز به ورود به پنل ادمین
    /// </summary>
    public class GalleryController : Controller
    {
        private readonly IHomePageService _homePageService;
        private readonly ILogger _logger;

        /// <summary>
        /// حداکثر تعداد تصاویر در صفحه گالری عمومی
        /// </summary>
        private const int GalleryPageSize = 48;

        public GalleryController(IHomePageService homePageService)
        {
            _homePageService = homePageService ?? throw new ArgumentNullException(nameof(homePageService));
            _logger = Log.ForContext<GalleryController>();
        }

        /// <summary>
        /// صفحه گالری عمومی - نمایش تصاویر محیط کلینیک برای کاربران
        /// مسیر: /Gallery یا /Gallery/Index
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 600, VaryByParam = "none")]
        public async Task<ActionResult> Index()
        {
            try
            {
                var gallery = await _homePageService.GetGallerySectionAsync(GalleryPageSize);
                if (gallery == null)
                {
                    gallery = new GallerySectionViewModel
                    {
                        SectionTitle = "گالری محیط کلینیک",
                        SectionSubtitle = "محیطی آرام و درمانی برای بیماران",
                        Items = new System.Collections.Generic.List<GalleryItemViewModel>()
                    };
                }
                ViewBag.Title = "گالری تصاویر";
                return View(gallery);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صفحه گالری عمومی");
                ViewBag.Title = "گالری تصاویر";
                return View(new GallerySectionViewModel
                {
                    SectionTitle = "گالری محیط کلینیک",
                    SectionSubtitle = "محیطی آرام و درمانی برای بیماران",
                    Items = new System.Collections.Generic.List<GalleryItemViewModel>()
                });
            }
        }
    }
}
