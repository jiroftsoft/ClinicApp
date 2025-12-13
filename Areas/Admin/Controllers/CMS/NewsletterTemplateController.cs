using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت Template های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class NewsletterTemplateController : BaseCMSController
    {
        private readonly INewsletterTemplateService _templateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public NewsletterTemplateController(
            INewsletterTemplateService templateService,
            ICurrentUserService currentUserService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<NewsletterTemplateController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("درخواست نمایش لیست Template های خبرنامه توسط کاربر {UserId}", _currentUserService.UserId);

                var result = await _templateService.GetTemplatesAsync();

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست Template ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Index"), new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>());
                }

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست Template های خبرنامه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست Template ها");
                return View(GetViewPath("Index"), new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>());
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _templateService.GetTemplateDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            var model = new NewsletterTemplateCreateEditViewModel
            {
                IsActive = true
            };
            return View(GetViewPath("Create"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Create(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Create"), model);
                }

                var result = await _templateService.CreateTemplateAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Create"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Template");
                NotificationHelper.SetError(TempData, "خطا در ایجاد Template");
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _templateService.GetTemplateForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات Template");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Edit(NewsletterTemplateCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _templateService.UpdateTemplateAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Edit"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Template - TemplateId: {TemplateId}", model?.NewsletterTemplateId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی Template");
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _templateService.DeleteTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _templateService.ActivateTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال کردن Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال کردن Template");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _templateService.DeactivateTemplateAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال کردن Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال کردن Template");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Preview

        [HttpGet]
        public async Task<ActionResult> Preview(int id)
        {
            try
            {
                var result = await _templateService.GetTemplateDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                // Render با Sample Data
                var variables = new Dictionary<string, string>
                {
                    { "FullName", "کاربر نمونه" },
                    { "Email", "sample@example.com" }
                };

                var renderResult = await _templateService.RenderTemplateAsync(result.Data.Content, variables);
                if (renderResult.Success)
                {
                    result.Data.Content = renderResult.Data;
                }

                return View(GetViewPath("Preview"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش پیش‌نمایش Template - TemplateId: {TemplateId}", id);
                NotificationHelper.SetError(TempData, "خطا در نمایش پیش‌نمایش");
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

