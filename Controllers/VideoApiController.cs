using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// API Controller برای ویدیوها (Public)
    /// طراحی شده بر اساس اصول SRP و Production-Ready
    /// </summary>
    public class VideoApiController : Controller
    {
        private readonly IVideoService _videoService;

        public VideoApiController(IVideoService videoService)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
        }

        /// <summary>
        /// افزایش تعداد بازدید ویدیو
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> IncrementViewCount(int videoId)
        {
            try
            {
                var result = await _videoService.IncrementViewCountAsync(videoId);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "تعداد بازدید به‌روزرسانی شد" });
                }
                
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در به‌روزرسانی تعداد بازدید" });
            }
        }
    }
}

