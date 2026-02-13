using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر عمومی برای نمایش مطالب آموزشی بیماران
    /// طراحی شده برای دسترسی عمومی بیماران
    /// </summary>
    public class PatientEducationController : Controller
    {
        private readonly IPatientEducationMaterialService _materialService;
        private readonly ILogger _logger;

        public PatientEducationController(IPatientEducationMaterialService materialService)
        {
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _logger = Log.ForContext<PatientEducationController>();
        }

        #region Index - لیست مطالب منتشر شده

        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "category,page")]
        public async Task<ActionResult> Index(PatientEducationCategory? category = null, int page = 1)
        {
            try
            {
                var searchModel = new PatientEducationMaterialSearchViewModel
                {
                    PageNumber = page,
                    PageSize = 12,
                    Category = category,
                    IsPublished = true
                };

                var result = await _materialService.GetMaterialsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست مطالب آموزشی: {ErrorMessage}", result.Message);
                    return View(new PatientEducationMaterialIndexPageViewModel
                    {
                        Materials = new PagedResult<PatientEducationMaterialIndexViewModel>(new System.Collections.Generic.List<PatientEducationMaterialIndexViewModel>(), 0, page, 12),
                        SearchModel = searchModel,
                        Categories = (IEnumerable<PatientEducationCategory>)System.Enum.GetValues(typeof(PatientEducationCategory)),
                        ErrorMessage = result.Message ?? "خطا در بارگذاری مطالب آموزشی"
                    });
                }

                return View(new PatientEducationMaterialIndexPageViewModel
                {
                    Materials = result.Data,
                    SearchModel = searchModel,
                    Categories = (IEnumerable<PatientEducationCategory>)System.Enum.GetValues(typeof(PatientEducationCategory))
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست مطالب آموزشی");
                return View(new PatientEducationMaterialIndexPageViewModel
                {
                    Materials = new PagedResult<PatientEducationMaterialIndexViewModel>(new System.Collections.Generic.List<PatientEducationMaterialIndexViewModel>(), 0, page, 12),
                    SearchModel = new PatientEducationMaterialSearchViewModel { PageNumber = page, PageSize = 12, Category = category, IsPublished = true },
                    Categories = (IEnumerable<PatientEducationCategory>)System.Enum.GetValues(typeof(PatientEducationCategory)),
                    ErrorMessage = "خطا در بارگذاری مطالب آموزشی"
                });
            }
        }

        #endregion

        #region GetMaterialsJson - API برای فیلتر و صفحه‌بندی بدون رفرش

        /// <summary>
        /// برگرداندن لیست مطالب به صورت JSON برای بارگذاری بدون رفرش (فیلتر دسته‌بندی و صفحه‌بندی).
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "category,page")]
        public async Task<ActionResult> GetMaterialsJson(PatientEducationCategory? category = null, int page = 1)
        {
            try
            {
                if (page < 1) page = 1;
                const int pageSize = 12;

                var searchModel = new PatientEducationMaterialSearchViewModel
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    Category = category,
                    IsPublished = true
                };

                var result = await _materialService.GetMaterialsAsync(searchModel);

                if (!result.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message ?? "خطا در دریافت لیست"
                    }, JsonRequestBehavior.AllowGet);
                }

                var data = result.Data;
                var sourceItems = data?.Items ?? Enumerable.Empty<PatientEducationMaterialIndexViewModel>();
                var items = sourceItems.Select(m => new
                {
                    patientEducationMaterialId = m.PatientEducationMaterialId,
                    title = m.Title,
                    description = string.IsNullOrEmpty(m.Description) ? "بدون توضیحات" : m.Description,
                    categoryDisplay = m.CategoryDisplay,
                    thumbnailUrl = m.ThumbnailUrl,
                    imageUrl = m.ImageUrl,
                    fileUrl = m.FileUrl,
                    viewCount = m.ViewCount,
                    downloadCount = m.DownloadCount,
                    detailsUrl = Url.Action("Details", "PatientEducation", new { id = m.PatientEducationMaterialId })
                }).ToList();

                return Json(new
                {
                    success = true,
                    items,
                    totalCount = data?.TotalCount ?? 0,
                    pageNumber = data?.PageNumber ?? 1,
                    pageSize = data?.PageSize ?? pageSize,
                    totalPages = data?.TotalPages ?? 0,
                    hasPreviousPage = data?.HasPreviousPage ?? false,
                    hasNextPage = data?.HasNextPage ?? false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetMaterialsJson");
                return Json(new { success = false, message = "خطا در بارگذاری مطالب" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Details - جزئیات مطلب

        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "id")]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _materialService.GetMaterialDetailsAsync(id);

                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("مطلب آموزشی یافت نشد - MaterialId: {MaterialId}", id);
                    ViewBag.ErrorMessage = "مطلب آموزشی یافت نشد";
                    return View("NotFound");
                }

                if (!result.Data.IsPublished)
                {
                    _logger.Warning("مطلب آموزشی منتشر نشده است - MaterialId: {MaterialId}", id);
                    ViewBag.ErrorMessage = "این مطلب آموزشی در دسترس نیست";
                    return View("NotFound");
                }

                // افزایش تعداد مشاهده
                await _materialService.IncrementViewCountAsync(id);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات مطلب آموزشی - MaterialId: {MaterialId}", id);
                ViewBag.ErrorMessage = "خطا در بارگذاری مطلب آموزشی";
                return View("Error");
            }
        }

        #endregion

        #region Download - دانلود فایل

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Download(int id)
        {
            try
            {
                var result = await _materialService.GetMaterialDetailsAsync(id);

                if (!result.Success || result.Data == null)
                {
                    NotificationHelper.SetError(TempData, "مطلب آموزشی یافت نشد");
                    return RedirectToAction("Index");
                }

                if (!result.Data.IsPublished)
                {
                    NotificationHelper.SetError(TempData, "این مطلب آموزشی در دسترس نیست");
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(result.Data.FileUrl))
                {
                    NotificationHelper.SetError(TempData, "فایل برای دانلود موجود نیست");
                    return RedirectToAction("Details", new { id });
                }

                // افزایش تعداد دانلود
                await _materialService.IncrementDownloadCountAsync(id);

                // دانلود فایل
                var filePath = Server.MapPath(result.Data.FileUrl);
                if (System.IO.File.Exists(filePath))
                {
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    var fileName = result.Data.FileName ?? "document.pdf";
                    return File(fileBytes, System.Web.MimeMapping.GetMimeMapping(fileName), fileName);
                }
                else
                {
                    NotificationHelper.SetError(TempData, "فایل یافت نشد");
                    return RedirectToAction("Details", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دانلود فایل - MaterialId: {MaterialId}", id);
                NotificationHelper.SetError(TempData, "خطا در دانلود فایل");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region GetBySlug - نمایش با Slug

        [HttpGet]
        [OutputCache(Duration = 300, VaryByParam = "slug")]
        public async Task<ActionResult> GetBySlug(string slug)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slug))
                {
                    return RedirectToAction("Index");
                }

                var result = await _materialService.GetMaterialBySlugAsync(slug);

                if (!result.Success || result.Data == null)
                {
                    _logger.Warning("مطلب آموزشی یافت نشد - Slug: {Slug}", slug);
                    ViewBag.ErrorMessage = "مطلب آموزشی یافت نشد";
                    return View("NotFound");
                }

                if (!result.Data.IsPublished)
                {
                    _logger.Warning("مطلب آموزشی منتشر نشده است - Slug: {Slug}", slug);
                    ViewBag.ErrorMessage = "این مطلب آموزشی در دسترس نیست";
                    return View("NotFound");
                }

                // افزایش تعداد مشاهده
                await _materialService.IncrementViewCountAsync(result.Data.PatientEducationMaterialId);

                return View("Details", result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش مطلب آموزشی - Slug: {Slug}", slug);
                ViewBag.ErrorMessage = "خطا در بارگذاری مطلب آموزشی";
                return View("Error");
            }
        }

        #endregion

        #region Helper Methods

        private string GetContentType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                default:
                    return "application/octet-stream";
            }
        }

        #endregion
    }
}

