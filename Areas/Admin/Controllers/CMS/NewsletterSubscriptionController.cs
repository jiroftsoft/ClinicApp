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

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت اشتراک‌های خبرنامه
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class NewsletterSubscriptionController : BaseCMSController
    {
        private readonly INewsletterSubscriptionService _subscriptionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public NewsletterSubscriptionController(
            INewsletterSubscriptionService subscriptionService,
            ICurrentUserService currentUserService)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<NewsletterSubscriptionController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(NewsletterSubscriptionSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست اشتراک‌های خبرنامه توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                {
                    searchModel = new NewsletterSubscriptionSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _subscriptionService.GetSubscriptionsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست اشتراک‌ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyResult = new PagedResult<NewsletterSubscriptionIndexViewModel>(new List<NewsletterSubscriptionIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize);
                    var emptyPageViewModel = new NewsletterSubscriptionIndexPageViewModel
                    {
                        Subscriptions = emptyResult,
                        SearchModel = searchModel,
                        TotalCount = 0,
                        ActiveCount = 0,
                        VerifiedCount = 0,
                        UnsubscribedCount = 0
                    };
                    return View(GetViewPath("Index"), emptyPageViewModel);
                }

                // دریافت آمار
                var activeCountResult = await _subscriptionService.GetActiveCountAsync();
                var verifiedCountResult = await _subscriptionService.GetVerifiedCountAsync();

                var pageViewModel = new NewsletterSubscriptionIndexPageViewModel
                {
                    Subscriptions = result.Data,
                    SearchModel = searchModel,
                    TotalCount = result.Data.TotalCount,
                    ActiveCount = activeCountResult.Success ? activeCountResult.Data : 0,
                    VerifiedCount = verifiedCountResult.Success ? verifiedCountResult.Data : 0,
                    UnsubscribedCount = 0 // TODO: بعداً پیاده‌سازی می‌شود
                };

                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست اشتراک‌های خبرنامه");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست اشتراک‌ها");
                var emptyResult = new PagedResult<NewsletterSubscriptionIndexViewModel>(new List<NewsletterSubscriptionIndexViewModel>(), 0, 1, 10);
                var emptyPageViewModel = new NewsletterSubscriptionIndexPageViewModel
                {
                    Subscriptions = emptyResult,
                    SearchModel = new NewsletterSubscriptionSearchViewModel { PageNumber = 1, PageSize = 10 },
                    TotalCount = 0,
                    ActiveCount = 0,
                    VerifiedCount = 0,
                    UnsubscribedCount = 0
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
                var result = await _subscriptionService.GetSubscriptionDetailsAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات اشتراک - SubscriptionId: {SubscriptionId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات اشتراک");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            var model = new NewsletterSubscriptionCreateEditViewModel
            {
                Source = NewsletterSubscriptionSource.Admin,
                IsActive = true,
                SelectedCategories = new List<NewsletterCategory>()
            };
            return View(GetViewPath("Create"), model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NewsletterSubscriptionCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Create"), model);
                }

                var result = await _subscriptionService.CreateSubscriptionByAdminAsync(model);

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
                _logger.Error(ex, "خطا در ایجاد اشتراک");
                NotificationHelper.SetError(TempData, "خطا در ایجاد اشتراک");
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
                // TODO: بعداً GetSubscriptionForEditAsync اضافه می‌شود
                var detailsResult = await _subscriptionService.GetSubscriptionDetailsAsync(id);
                if (!detailsResult.Success)
                {
                    NotificationHelper.SetError(TempData, detailsResult.Message);
                    return RedirectToAction("Index");
                }

                var details = detailsResult.Data;
                var model = new NewsletterSubscriptionCreateEditViewModel
                {
                    NewsletterSubscriptionId = details.NewsletterSubscriptionId,
                    Email = details.Email,
                    FullName = details.FullName,
                    PhoneNumber = details.PhoneNumber,
                    SelectedCategories = details.Categories ?? new List<NewsletterCategory>(),
                    Source = details.Source,
                    IsActive = details.IsActive
                };

                return View(GetViewPath("Edit"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش اشتراک - SubscriptionId: {SubscriptionId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات اشتراک");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(NewsletterSubscriptionCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _subscriptionService.UpdateSubscriptionAsync(model);

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
                _logger.Error(ex, "خطا در به‌روزرسانی اشتراک - SubscriptionId: {SubscriptionId}", model?.NewsletterSubscriptionId);
                NotificationHelper.SetError(TempData, "خطا در به‌روزرسانی اشتراک");
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
                var result = await _subscriptionService.DeleteSubscriptionAsync(id);

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
                _logger.Error(ex, "خطا در حذف اشتراک - SubscriptionId: {SubscriptionId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف اشتراک");
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
                var result = await _subscriptionService.ActivateSubscriptionAsync(id);

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
                _logger.Error(ex, "خطا در فعال کردن اشتراک - SubscriptionId: {SubscriptionId}", id);
                NotificationHelper.SetError(TempData, "خطا در فعال کردن اشتراک");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _subscriptionService.DeactivateSubscriptionAsync(id);

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
                _logger.Error(ex, "خطا در غیرفعال کردن اشتراک - SubscriptionId: {SubscriptionId}", id);
                NotificationHelper.SetError(TempData, "خطا در غیرفعال کردن اشتراک");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Export/Import

        [HttpGet]
        public ActionResult Import()
        {
            return View(GetViewPath("Import"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Import(System.Web.HttpPostedFileBase file)
        {
            try
            {
                // TODO: پیاده‌سازی Import از Excel
                NotificationHelper.SetInfo(TempData, "Import از Excel در حال پیاده‌سازی است");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Import اشتراک‌ها");
                NotificationHelper.SetError(TempData, "خطا در Import اشتراک‌ها");
                return RedirectToAction("Import");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Export(NewsletterSubscriptionSearchViewModel searchModel)
        {
            try
            {
                // TODO: پیاده‌سازی Export به Excel
                NotificationHelper.SetInfo(TempData, "Export به Excel در حال پیاده‌سازی است");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Export اشتراک‌ها");
                NotificationHelper.SetError(TempData, "خطا در Export اشتراک‌ها");
                return RedirectToAction("Index");
            }
        }

        #endregion
    }
}

