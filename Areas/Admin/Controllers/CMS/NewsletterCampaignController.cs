using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت Campaign های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class NewsletterCampaignController : BaseCMSController
    {
        private readonly INewsletterCampaignService _campaignService;
        private readonly INewsletterTemplateService _templateService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public NewsletterCampaignController(
            INewsletterCampaignService campaignService,
            INewsletterTemplateService templateService,
            ICurrentUserService currentUserService)
        {
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<NewsletterCampaignController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(NewsletterCampaignSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست Campaign های خبرنامه توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                {
                    searchModel = new NewsletterCampaignSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                // Parse تاریخ‌های جستجو از hidden inputs
                searchModel.FromDate = this.ParseDateFromHiddenInput("FromDate", _logger);
                searchModel.ToDate = this.ParseDateFromHiddenInput("ToDate", _logger);

                var result = await _campaignService.GetCampaignsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست Campaign ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyResult = new PagedResult<NewsletterCampaignIndexViewModel>(new System.Collections.Generic.List<NewsletterCampaignIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize);
                    var emptyPageViewModel = new NewsletterCampaignIndexPageViewModel
                    {
                        Campaigns = emptyResult,
                        SearchModel = searchModel
                    };
                    return View(GetViewPath("Index"), emptyPageViewModel);
                }

                var pageViewModel = new NewsletterCampaignIndexPageViewModel
                {
                    Campaigns = result.Data,
                    SearchModel = searchModel
                };

                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست Campaign های خبرنامه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست Campaign ها");
                var emptyResult = new PagedResult<NewsletterCampaignIndexViewModel>(new System.Collections.Generic.List<NewsletterCampaignIndexViewModel>(), 0, 1, 10);
                var emptyPageViewModel = new NewsletterCampaignIndexPageViewModel
                {
                    Campaigns = emptyResult,
                    SearchModel = new NewsletterCampaignSearchViewModel { PageNumber = 1, PageSize = 10 }
                };
                return View(GetViewPath("Index"), emptyPageViewModel);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _campaignService.GetCampaignDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات Campaign");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                var templatesResult = await _templateService.GetTemplatesAsync();
                ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();

                var model = new NewsletterCampaignCreateEditViewModel
                {
                    SendToAll = false,
                    SelectedCategories = new System.Collections.Generic.List<Models.Enums.NewsletterCategory>()
                };
                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد Campaign");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Create(NewsletterCampaignCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات فرم نامعتبر است.");
                    return RedirectToAction("Index");
                }
                model.ScheduledAt = this.ParseDateAndTimeFromForm("ScheduledAtDate", "ScheduledAtTime", _logger);

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    var templatesResult = await _templateService.GetTemplatesAsync();
                    ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                    return View(GetViewPath("Create"), model);
                }

                var result = await _campaignService.CreateCampaignAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    var templatesResult = await _templateService.GetTemplatesAsync();
                    ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                    return View(GetViewPath("Create"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد Campaign");
                NotificationHelper.SetError(TempData, "خطا در ایجاد Campaign");
                var templatesResult = await _templateService.GetTemplatesAsync();
                ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                return View(GetViewPath("Create"), model ?? new NewsletterCampaignCreateEditViewModel { SelectedCategories = new System.Collections.Generic.List<Models.Enums.NewsletterCategory>() });
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _campaignService.GetCampaignForEditAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                var templatesResult = await _templateService.GetTemplatesAsync();
                ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات Campaign");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // برای CKEditor
        public async Task<ActionResult> Edit(NewsletterCampaignCreateEditViewModel model)
        {
            try
            {
                if (model == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات فرم نامعتبر است.");
                    return RedirectToAction("Index");
                }
                model.ScheduledAt = this.ParseDateAndTimeFromForm("ScheduledAtDate", "ScheduledAtTime", _logger);

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    var templatesResult = await _templateService.GetTemplatesAsync();
                    ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _campaignService.UpdateCampaignAsync(model);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    var templatesResult = await _templateService.GetTemplatesAsync();
                    ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                    return View(GetViewPath("Edit"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی Campaign - CampaignId: {CampaignId}", model?.NewsletterCampaignId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی Campaign");
                var templatesResult = await _templateService.GetTemplatesAsync();
                ViewBag.Templates = templatesResult.Success ? templatesResult.Data : new System.Collections.Generic.List<NewsletterTemplateIndexViewModel>();
                if (model != null)
                    return View(GetViewPath("Edit"), model);
                return RedirectToAction("Index");
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
                var result = await _campaignService.DeleteCampaignAsync(id);

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
                _logger.Error(ex, "خطا در حذف Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف Campaign");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Send

        [HttpGet]
        public async Task<ActionResult> Send(int id)
        {
            try
            {
                var result = await _campaignService.GetCampaignForSendAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Send"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ارسال Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ارسال");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Send(NewsletterCampaignSendViewModel model)
        {
            try
            {
                if (model == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات فرم نامعتبر است.");
                    return RedirectToAction("Index");
                }
                model.ScheduledAt = this.ParseDateAndTimeFromForm("ScheduledAtDate", "ScheduledAtTime", _logger);

                if (model.ScheduledAt.HasValue && model.ScheduledAt.Value > DateTime.Now)
                {
                    // زمان‌بندی شده
                    var result = await _campaignService.ScheduleCampaignAsync(
                        model.NewsletterCampaignId,
                        model.ScheduledAt.Value,
                        model.SendEmail,
                        model.SendSms);

                    if (result.Success)
                    {
                        NotificationHelper.SetSuccess(TempData, result.Message);
                    }
                    else
                    {
                        NotificationHelper.SetError(TempData, result.Message);
                    }
                }
                else
                {
                    // ارسال فوری
                    var result = await _campaignService.SendCampaignAsync(
                        model.NewsletterCampaignId,
                        model.SendEmail,
                        model.SendSms);

                    if (result.Success)
                    {
                        NotificationHelper.SetSuccess(TempData, result.Message);
                    }
                    else
                    {
                        NotificationHelper.SetError(TempData, result.Message);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال Campaign - CampaignId: {CampaignId}", model?.NewsletterCampaignId);
                NotificationHelper.SetError(TempData, "خطا در ارسال Campaign");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Cancel Schedule

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelSchedule(int id)
        {
            try
            {
                var result = await _campaignService.CancelScheduledCampaignAsync(id);

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
                _logger.Error(ex, "خطا در لغو زمان‌بندی Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در لغو زمان‌بندی");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Retry Send (ارسال مجدد)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RetrySend(int id)
        {
            try
            {
                var result = await _campaignService.RetryCampaignSendAsync(id);
                if (result.Success)
                    NotificationHelper.SetSuccess(TempData, result.Message);
                else
                    NotificationHelper.SetError(TempData, result.Message);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ارسال مجدد Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در ارسال مجدد.");
                return RedirectToAction("Details", new { id });
            }
        }

        #endregion

        #region Statistics

        [HttpGet]
        public async Task<ActionResult> Statistics(int id)
        {
            try
            {
                var result = await _campaignService.GetCampaignStatisticsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Statistics"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش آمار Campaign - CampaignId: {CampaignId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری آمار");
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

