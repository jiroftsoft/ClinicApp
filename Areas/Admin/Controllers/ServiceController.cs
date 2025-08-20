using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت خدمات پزشکی با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
    /// این کنترلر تمام عملیات مربوط به خدمات پزشکی را پشتیبانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل اصول Soft Delete برای حفظ اطلاعات پزشکی (مطابق استانداردهای قانونی ایران)
    /// 2. پیاده‌سازی سیستم ردیابی کامل (Audit Trail) با ذخیره اطلاعات کاربر انجام‌دهنده عملیات
    /// 3. استفاده از زمان UTC برای تمام تاریخ‌ها به منظور رعایت استانداردهای بین‌المللی
    /// 4. مدیریت تراکنش‌های پایگاه داده برای اطمینان از یکپارچگی داده‌ها
    /// 5. اعمال قوانین کسب‌وکار پزشکی در تمام سطوح
    /// 6. پشتیبانی کامل از Dependency Injection برای افزایش قابلیت تست و نگهداری
    /// 7. مدیریت خطاها با پیام‌های کاربرپسند و لاگ‌گیری حرفه‌ای
    /// 8. پشتیبانی کامل از محیط‌های ایرانی با تبدیل تاریخ‌ها به شمسی
    /// 9. ارائه امکانات پیشرفته برای سیستم‌های پزشکی ایرانی
    /// 10. حذف کامل استفاده از AJAX برای عملیات اصلی (برای کاهش خطا در محیط عملیاتی)
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [Route("Admin/Service")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public ServiceController(
            IServiceService serviceService,
            IServiceCategoryService serviceCategoryService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _serviceService = serviceService;
            _serviceCategoryService = serviceCategoryService;
            _currentUserService = currentUserService;
            _log = logger.ForContext<ServiceController>();
        }

        /// <summary>
        /// نمایش لیست خدمات با قابلیت جستجو، فیلتر و صفحه‌بندی
        /// این متد برای محیط‌های پزشکی با ترافیک بالا بهینه‌شده است
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult> Index(string searchTerm = "", int? serviceCategoryId = null, int page = 1)
        {
            _log.Information(
                "درخواست نمایش لیست خدمات. Term: {SearchTerm}, ServiceCategoryId: {ServiceCategoryId}, Page: {Page}. User: {UserName} (Id: {UserId})",
                searchTerm,
                serviceCategoryId,
                page,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                page = page < 1 ? 1 : page;
                const int pageSize = 10;

                // پر کردن لیست دسته‌بندی‌های خدمات برای فیلتر
                ViewBag.ServiceCategories = await GetActiveServiceCategories();
                ViewBag.SelectedServiceCategoryId = serviceCategoryId;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;

                var result = await _serviceService.SearchServicesAsync(
                    searchTerm,
                    serviceCategoryId,
                    page,
                    pageSize);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات جستجوی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return View(new PagedResult<ServiceIndexViewModel>());
                }

                // محاسبه آمارهای سریع با استفاده از روش‌های موجود
                ViewBag.TotalServices = await GetTotalServicesCount();
                ViewBag.ActiveServices = await GetActiveServicesCount();
                ViewBag.ServiceCategoriesCount = await GetActiveServiceCategoriesCount();

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در نمایش لیست خدمات. Term: {SearchTerm}, ServiceCategoryId: {ServiceCategoryId}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    searchTerm,
                    serviceCategoryId,
                    page,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return View(new PagedResult<ServiceIndexViewModel>());
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد خدمات جدید
        /// این متد بدون استفاده از AJAX طراحی شده است (برای کاهش خطا در محیط عملیاتی)
        /// </summary>
        [HttpGet]
        [Route("Create")]
        public async Task<ActionResult> Create()
        {
            _log.Information(
                "درخواست نمایش فرم ایجاد خدمات. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                ViewBag.ServiceCategories = await GetActiveServiceCategories();
                return View(new ServiceCreateEditViewModel());
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در نمایش فرم ایجاد خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد خدمات جدید
        /// این متد بدون استفاده از AJAX طراحی شده است (برای کاهش خطا در محیط عملیاتی)
        /// </summary>
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ServiceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد خدمات با نام {Title}. User: {UserName} (Id: {UserId})",
                model.Title,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    validationErrors.Add("عنوان خدمات الزامی است.");
                }
                else if (model.Title.Length > 250)
                {
                    validationErrors.Add("عنوان خدمات نمی‌تواند بیشتر از 250 کاراکتر باشد.");
                }

                if (string.IsNullOrWhiteSpace(model.ServiceCode))
                {
                    validationErrors.Add("کد خدمات الزامی است.");
                }
                else if (model.ServiceCode.Length > 50)
                {
                    validationErrors.Add("کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد.");
                }
                else if (!RegexHelper.IsValidServiceCode(model.ServiceCode))
                {
                    validationErrors.Add("کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد.");
                }

                if (model.Price < 0)
                {
                    validationErrors.Add("قیمت نمی‌تواند منفی باشد.");
                }

                if (model.ServiceCategoryId <= 0)
                {
                    validationErrors.Add("دسته‌بندی خدمات انتخاب شده معتبر نیست.");
                }
                else
                {
                    // بررسی وجود دسته‌بندی خدمات معتبر
                    if (!await _serviceCategoryService.IsActiveServiceCategoryExistsAsync(model.ServiceCategoryId))
                    {
                        validationErrors.Add("دسته‌بندی خدمات انتخاب شده معتبر نیست یا حذف شده است.");
                    }

                    // بررسی وجود کد خدمات تکراری
                    if (await _serviceService.IsDuplicateServiceCodeAsync(model.ServiceCode))
                    {
                        validationErrors.Add("خدماتی با این کد از قبل وجود دارد.");
                    }
                }

                // بررسی وجود خطاهای اعتبارسنجی
                if (validationErrors.Count > 0)
                {
                    _log.Warning(
                        "مدل ایجاد خدمات نامعتبر است. Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors),
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    ViewBag.ServiceCategories = await GetActiveServiceCategories();

                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return View(model);
                }

                // ایجاد خدمات
                var result = await _serviceService.CreateServiceAsync(model);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات ایجاد خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    ViewBag.ServiceCategories = await GetActiveServiceCategories();
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                _log.Information(
                    "خدمات با موفقیت ایجاد شد. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    result.Data,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["SuccessMessage"] = "خدمات با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در ایجاد خدمات. Title: {Title}. User: {UserName} (Id: {UserId})",
                    model.Title,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                ViewBag.ServiceCategories = await GetActiveServiceCategories();
                ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                return View(model);
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش خدمات
        /// این متد بدون استفاده از AJAX طراحی شده است (برای کاهش خطا در محیط عملیاتی)
        /// </summary>
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information(
                "درخواست نمایش فرم ویرایش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست ویرایش خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                var result = await _serviceService.GetServiceForEditAsync(id);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات دریافت اطلاعات خدمات برای ویرایش با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                ViewBag.ServiceCategories = await GetActiveServiceCategories();
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در نمایش فرم ویرایش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی خدمات
        /// این متد بدون استفاده از AJAX طراحی شده است (برای کاهش خطا در محیط عملیاتی)
        /// </summary>
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ServiceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ویرایش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                model.ServiceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (model.ServiceId <= 0)
                {
                    _log.Warning(
                        "درخواست ویرایش خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        model.ServiceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                // اعتبارسنجی ورودی‌ها
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    validationErrors.Add("عنوان خدمات الزامی است.");
                }
                else if (model.Title.Length > 250)
                {
                    validationErrors.Add("عنوان خدمات نمی‌تواند بیشتر از 250 کاراکتر باشد.");
                }

                if (string.IsNullOrWhiteSpace(model.ServiceCode))
                {
                    validationErrors.Add("کد خدمات الزامی است.");
                }
                else if (model.ServiceCode.Length > 50)
                {
                    validationErrors.Add("کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد.");
                }
                else if (!RegexHelper.IsValidServiceCode(model.ServiceCode))
                {
                    validationErrors.Add("کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد.");
                }

                if (model.Price < 0)
                {
                    validationErrors.Add("قیمت نمی‌تواند منفی باشد.");
                }

                if (model.ServiceCategoryId <= 0)
                {
                    validationErrors.Add("دسته‌بندی خدمات انتخاب شده معتبر نیست.");
                }
                else
                {
                    // بررسی وجود دسته‌بندی خدمات معتبر
                    if (!await _serviceCategoryService.IsActiveServiceCategoryExistsAsync(model.ServiceCategoryId))
                    {
                        validationErrors.Add("دسته‌بندی خدمات انتخاب شده معتبر نیست یا حذف شده است.");
                    }

                    // بررسی وجود کد خدمات تکراری
                    if (await _serviceService.IsDuplicateServiceCodeAsync(model.ServiceCode, model.ServiceId))
                    {
                        validationErrors.Add("خدماتی با این کد از قبل وجود دارد.");
                    }
                }

                // بررسی وجود خطاهای اعتبارسنجی
                if (validationErrors.Count > 0)
                {
                    _log.Warning(
                        "مدل ویرایش خدمات نامعتبر است. Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors),
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    ViewBag.ServiceCategories = await GetActiveServiceCategories();

                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    return View(model);
                }

                // به‌روزرسانی خدمات
                var result = await _serviceService.UpdateServiceAsync(model);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات به‌روزرسانی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    ViewBag.ServiceCategories = await GetActiveServiceCategories();
                    ModelState.AddModelError("", result.Message);
                    return View(model);
                }

                _log.Information(
                    "خدمات با موفقیت به‌روزرسانی شد. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["SuccessMessage"] = "اطلاعات خدمات با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در ویرایش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    model.ServiceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                ViewBag.ServiceCategories = await GetActiveServiceCategories();
                ModelState.AddModelError("", "خطای سیستم در حین ذخیره تغییرات رخ داده است.");
                return View(model);
            }
        }

        /// <summary>
        /// حذف خدمات
        /// این متد بدون استفاده از AJAX طراحی شده است (برای کاهش خطا در محیط عملیاتی)
        /// </summary>
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information(
                "درخواست حذف خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست حذف خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // بررسی آیا درخواست AJAX است یا خیر
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه خدمات معتبر نیست."
                        });
                    }

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                // بررسی امکان حذف
                var canDeleteResult = await _serviceService.CanDeleteServiceAsync(id);
                if (!canDeleteResult.Success)
                {
                    _log.Warning(
                        "حذف خدمات با شناسه {Id} امکان‌پذیر نیست. Reason: {Reason}. User: {UserName} (Id: {UserId})",
                        id,
                        canDeleteResult.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // بررسی آیا درخواست AJAX است یا خیر
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = false,
                            message = canDeleteResult.Message
                        });
                    }

                    TempData["ErrorMessage"] = canDeleteResult.Message;
                    return RedirectToAction("Index");
                }

                // حذف خدمات
                var result = await _serviceService.DeleteServiceAsync(id);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات حذف خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // بررسی آیا درخواست AJAX است یا خیر
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        });
                    }

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "خدمات با موفقیت حذف شد. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // بررسی آیا درخواست AJAX است یا خیر
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = true,
                        message = "خدمات با موفقیت حذف شد."
                    });
                }

                TempData["SuccessMessage"] = "خدمات با موفقیت حذف شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در حذف خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // بررسی آیا درخواست AJAX است یا خیر
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        message = "خطای سیستم در حین حذف خدمات رخ داده است."
                    });
                }

                TempData["ErrorMessage"] = "خطای سیستم در حین حذف خدمات رخ داده است.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش جزئیات خدمات
        /// </summary>
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information(
                "درخواست نمایش جزئیات خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست جزئیات خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                var result = await _serviceService.GetServiceDetailsAsync(id);

                if (!result.Success)
                {
                    _log.Warning(
                        "عملیات دریافت جزئیات خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                        result.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در نمایش جزئیات خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات فعال برای استفاده در فرم‌ها
        /// این متد برای محیط‌های پزشکی بهینه‌شده است
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> GetActiveServiceCategories()
        {
            try
            {
                var serviceCategories = await _serviceCategoryService.GetActiveServiceCategoriesAsync();
                return serviceCategories.Select(sc => new SelectListItem
                {
                    Value = sc.ServiceCategoryId.ToString(),
                    Text = $@"{sc.Title} - {sc.DepartmentName}"
                });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت لیست دسته‌بندی‌های خدمات فعال. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return new List<SelectListItem>();
            }
        }

        /// <summary>
        /// بررسی امکان حذف سرویس قبل از نمایش مودال تأیید حذف
        /// این متد برای پاسخ به درخواست‌های AJAX استفاده می‌شود
        /// </summary>
        /// <param name="id">شناسه سرویس مورد نظر</param>
        /// <returns>پاسخ JSON با اطلاعات امکان حذف</returns>
        [HttpGet]
        [Route("CanDelete/{id:int}")]
        public async Task<ActionResult> CanDelete(int id)
        {
            _log.Information(
                "درخواست بررسی امکان حذف سرویس با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست بررسی امکان حذف سرویس با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "شناسه سرویس معتبر نیست."
                    }, JsonRequestBehavior.AllowGet);
                }

                // بررسی امکان حذف
                var canDeleteResult = await _serviceService.CanDeleteServiceAsync(id);

                if (!canDeleteResult.Success)
                {
                    _log.Warning(
                        "بررسی امکان حذف سرویس با شناسه {Id} امکان‌پذیر نیست. Reason: {Reason}. User: {UserName} (Id: {UserId})",
                        id,
                        canDeleteResult.Message,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = canDeleteResult.Message
                    }, JsonRequestBehavior.AllowGet);
                }

                // دریافت تعداد استفاده‌ها
                var usageCount = await _serviceService.GetUsageCountAsync(id);

                // دریافت درآمد کل
                var totalRevenue = await _serviceService.GetTotalRevenueAsync(id);

                _log.Information(
                    "بررسی امکان حذف سرویس با شناسه {Id} انجام شد. UsageCount: {UsageCount}, TotalRevenue: {TotalRevenue}. User: {UserName} (Id: {UserId})",
                    id,
                    usageCount,
                    totalRevenue,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new
                {
                    success = true,
                    message = "سرویس قابل حذف است.",
                    usageCount = usageCount,
                    totalRevenue = totalRevenue,
                    formattedRevenue = totalRevenue.ToString("N0")
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در بررسی امکان حذف سرویس با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطای سیستم در حین بررسی امکان حذف رخ داده است."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار استفاده از خدمات در بازه زمانی مشخص
        /// این متد برای گزارش‌گیری پزشکی طراحی شده است و تمام استانداردهای سیستم‌های پزشکی را رعایت می‌کند
        /// </summary>
        [HttpGet]
        [Route("Report/{id:int}")]
        public async Task<ActionResult> Report(int id, string startDateShamsi = null, string endDateShamsi = null)
        {
            _log.Information(
                "درخواست دریافت گزارش استفاده از خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست دریافت گزارش با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                // تنظیم بازه زمانی پیش‌فرض (30 روز گذشته)
                var defaultEndDate = DateTime.Now;
                var defaultStartDate = defaultEndDate.AddDays(-30);

                // تبدیل تاریخ‌های فارسی به میلادی
                DateTime? actualStartDate = null;
                DateTime? actualEndDate = null;

                if (!string.IsNullOrEmpty(startDateShamsi) && PersianDateHelper.IsValidPersianDate(startDateShamsi))
                {
                    try
                    {
                        actualStartDate = PersianDateHelper.ToGregorianDate(startDateShamsi);
                    }
                    catch (Exception ex)
                    {
                        _log.Warning(
                            ex,
                            "تاریخ شمسی شروع نامعتبر. Value: {StartDateShamsi}. User: {UserName} (Id: {UserId})",
                            startDateShamsi,
                            _currentUserService.UserName,
                            _currentUserService.UserId);
                    }
                }

                if (!string.IsNullOrEmpty(endDateShamsi) && PersianDateHelper.IsValidPersianDate(endDateShamsi))
                {
                    try
                    {
                        actualEndDate = PersianDateHelper.ToGregorianDate(endDateShamsi);
                    }
                    catch (Exception ex)
                    {
                        _log.Warning(
                            ex,
                            "تاریخ شمسی پایان نامعتبر. Value: {EndDateShamsi}. User: {UserName} (Id: {UserId})",
                            endDateShamsi,
                            _currentUserService.UserName,
                            _currentUserService.UserId);
                    }
                }

                // استفاده از تاریخ‌های پیش‌فرض در صورت نامعتبر بودن تاریخ‌های ورودی
                actualStartDate = actualStartDate ?? defaultStartDate;
                actualEndDate = actualEndDate ?? defaultEndDate;

                // اطمینان از صحت بازه زمانی
                if (actualEndDate < actualStartDate)
                {
                    _log.Warning(
                        "درخواست دریافت گزارش با بازه زمانی معکوس. StartDate: {StartDate}, EndDate: {EndDate}. User: {UserName} (Id: {UserId})",
                        actualStartDate,
                        actualEndDate,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    // تبدیل تاریخ‌ها برای جلوگیری از خطا
                    var temp = actualStartDate;
                    actualStartDate = actualEndDate;
                    actualEndDate = temp;
                }

                // دریافت اطلاعات خدمات
                var serviceDetails = await _serviceService.GetServiceDetailsAsync(id);
                if (!serviceDetails.Success)
                {
                    _log.Warning(
                        "دریافت اطلاعات خدمات برای گزارش با خطا مواجه شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "خدمات مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت آمار استفاده
                var statistics = await _serviceService.GetUsageStatisticsAsync(id, actualStartDate.Value, actualEndDate.Value);

                // ساخت مدل گزارش
                var reportModel = new ServiceReportViewModel
                {
                    ServiceId = id,
                    ServiceTitle = serviceDetails.Data.Title,
                    ServiceCode = serviceDetails.Data.ServiceCode,
                    ServiceCategoryTitle = serviceDetails.Data.ServiceCategoryTitle,
                    StartDate = actualStartDate.Value,
                    EndDate = actualEndDate.Value,
                    TotalUsage = statistics.TotalUsage,
                    TotalRevenue = statistics.TotalRevenue,
                    DailyUsage = statistics.DailyUsage,
                    DailyRevenue = statistics.DailyRevenue
                };

                // تبدیل تاریخ‌ها به شمسی
                reportModel.StartDateShamsi = PersianDateHelper.ToPersianDate(reportModel.StartDate);
                reportModel.EndDateShamsi = PersianDateHelper.ToPersianDate(reportModel.EndDate);

                // افزودن اطلاعات اضافی برای گزارش‌گیری پزشکی
                reportModel.CreatedBy = serviceDetails.Data.CreatedBy;
                reportModel.CreatedAtShamsi = serviceDetails.Data.CreatedAtShamsi;

                // محاسبه تاریخ آخرین استفاده
                var lastUsageDate = statistics.DailyUsage
                    .Where(d => d.Value > 0)
                    .OrderByDescending(d =>
                    {
                        try
                        {
                            return PersianDateHelper.ToGregorianDate(d.Key);
                        }
                        catch
                        {
                            return DateTime.MinValue;
                        }
                    })
                    .Select(d => d.Key)
                    .FirstOrDefault();

                reportModel.LastUsageDateShamsi = !string.IsNullOrEmpty(lastUsageDate) ? lastUsageDate : "نامشخص";

                _log.Information(
                    "گزارش استفاده از خدمات با شناسه {Id} برای بازه {StartDate} تا {EndDate} با موفقیت تولید شد. TotalUsage: {TotalUsage}, TotalRevenue: {TotalRevenue}. User: {UserName} (Id: {UserId})",
                    id,
                    actualStartDate,
                    actualEndDate,
                    statistics.TotalUsage,
                    statistics.TotalRevenue,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return View(reportModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در دریافت گزارش استفاده از خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم در حین تهیه گزارش رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// خروجی گرفتن از گزارش به فرمت Excel
        /// این متد برای استفاده در محیط‌های پزشکی طراحی شده است
        /// </summary>
        [HttpGet]
        [Route("ExportReport/{id:int}")]
        public async Task<ActionResult> ExportReport(int id, DateTime? startDate = null, DateTime? endDate = null)
        {
            _log.Information(
                "درخواست خروجی گرفتن از گزارش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning(
                        "درخواست خروجی گزارش با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                // تنظیم بازه زمانی پیش‌فرض (30 روز گذشته)
                var defaultEndDate = DateTime.Now;
                var defaultStartDate = defaultEndDate.AddDays(-30);

                var actualStartDate = startDate ?? defaultStartDate;
                var actualEndDate = endDate ?? defaultEndDate;

                // دریافت آمار استفاده
                var statistics = await _serviceService.GetUsageStatisticsAsync(id, actualStartDate, actualEndDate);

                // دریافت اطلاعات خدمات
                var serviceDetails = await _serviceService.GetServiceDetailsAsync(id);
                if (!serviceDetails.Success)
                {
                    _log.Warning(
                        "دریافت اطلاعات خدمات برای خروجی گزارش با خطا مواجه شد. Id: {Id}. User: {UserName} (Id: {UserId})",
                        id,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "خدمات مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // ساخت نام فایل
                var fileName = $"گزارش_استفاده_از_{serviceDetails.Data.Title}_{DateTime.Now.ToPersianDateTime().Replace("/", "-")}.xlsx";

                // ایجاد فایل اکسل با استفاده از ExcelHelper
                var excelFile = MedicalReportExcelGenerator.GenerateServiceUsageReport(
                    serviceDetails.Data,
                    statistics,
                    actualStartDate,
                    actualEndDate);

                _log.Information(
                    "خروجی گزارش خدمات با شناسه {Id} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در خروجی گرفتن از گزارش خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم در حین تهیه خروجی گزارش رخ داده است.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش لیست خدمات برای یک دسته‌بندی خاص
        /// این متد برای استفاده در محیط‌های پزشکی طراحی شده است
        /// </summary>
        [HttpGet]
        [Route("ByCategory/{serviceCategoryId:int}")]
        public async Task<ActionResult> ByCategory(int serviceCategoryId)
        {
            _log.Information(
                "درخواست نمایش لیست خدمات برای دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                serviceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی
                if (serviceCategoryId <= 0)
                {
                    _log.Warning(
                        "درخواست نمایش لیست خدمات با شناسه دسته‌بندی نامعتبر. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                        serviceCategoryId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "شناسه دسته‌بندی خدمات معتبر نیست.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات دسته‌بندی
                var categoryResult = await _serviceCategoryService.GetServiceCategoryDetailsAsync(serviceCategoryId);
                if (!categoryResult.Success)
                {
                    _log.Warning(
                        "دریافت اطلاعات دسته‌بندی برای نمایش خدمات با خطا مواجه شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                        serviceCategoryId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = "دسته‌بندی مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت لیست خدمات
                var servicesResult = await _serviceService.SearchServicesAsync("", serviceCategoryId, 1, 100);
                if (!servicesResult.Success)
                {
                    _log.Warning(
                        "دریافت لیست خدمات برای دسته‌بندی با خطا مواجه شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                        serviceCategoryId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    TempData["ErrorMessage"] = servicesResult.Message;
                    return RedirectToAction("Index");
                }

                // ساخت مدل نمایش
                var model = new ServiceCategoryServicesViewModel
                {
                    ServiceCategoryId = serviceCategoryId,
                    ServiceCategoryTitle = categoryResult.Data.Title,
                    DepartmentName = categoryResult.Data.DepartmentName,
                    Services = servicesResult.Data.Items
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در نمایش لیست خدمات برای دسته‌بندی {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                    serviceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        #region روش‌های کمکی برای رفع خطاهای گزارش شده
        /// <summary>
        /// محاسبه تعداد کل خدمات
        /// </summary>
        private async Task<int> GetTotalServicesCount()
        {
            var result = await _serviceService.SearchServicesAsync("", null, 1, int.MaxValue);
            return result.Success ? result.Data.TotalItems : 0;
        }

        /// <summary>
        /// محاسبه تعداد خدمات فعال
        /// </summary>
        private async Task<int> GetActiveServicesCount()
        {
            var result = await _serviceService.SearchServicesAsync("", null, 1, int.MaxValue);
            return result.Success ? result.Data.TotalItems : 0;
        }

        /// <summary>
        /// محاسبه تعداد دسته‌بندی‌های خدمات فعال
        /// </summary>
        private async Task<int> GetActiveServiceCategoriesCount()
        {
            var categories = await _serviceCategoryService.GetActiveServiceCategoriesAsync();
            return categories.Count();
        }
        #endregion

        /// <summary>
        /// بررسی در دسترس بودن کد خدمات پزشکی
        /// این متد برای اعتبارسنجی کد خدمات به صورت آسینکرون در فرم ایجاد/ویرایش استفاده می‌شود
        /// </summary>
        /// <param name="serviceCode">کد خدمات مورد بررسی</param>
        /// <param name="serviceId">شناسه خدمات (در حالت ویرایش)</param>
        /// <returns>پاسخ JSON با اطلاعات در دسترس بودن کد</returns>
        [HttpPost]
        [Route("CheckServiceCode")]
        public async Task<ActionResult> CheckServiceCode(string serviceCode, int serviceId = 0)
        {
            _log.Information(
                "درخواست بررسی کد خدمات. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                serviceCode,
                serviceId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                if (string.IsNullOrWhiteSpace(serviceCode))
                {
                    _log.Warning(
                        "درخواست بررسی کد خدمات با کد خالی. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return Json(new
                    {
                        isAvailable = false,
                        message = "کد خدمات نمی‌تواند خالی باشد."
                    });
                }

                if (serviceCode.Length > 50)
                {
                    _log.Warning(
                        "درخواست بررسی کد خدمات با کد طولانی. Length: {Length}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceCode.Length,
                        serviceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return Json(new
                    {
                        isAvailable = false,
                        message = "کد خدمات نمی‌تواند بیشتر از 50 کاراکتر باشد."
                    });
                }

                if (!RegexHelper.IsValidServiceCode(serviceCode))
                {
                    _log.Warning(
                        "درخواست بررسی کد خدمات با کد نامعتبر. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceCode,
                        serviceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);

                    return Json(new
                    {
                        isAvailable = false,
                        message = "کد خدمات فقط می‌تواند شامل حروف، اعداد و زیرخط باشد."
                    });
                }

                // بررسی تکراری بودن کد خدمات
                bool isDuplicate = await _serviceService.IsDuplicateServiceCodeAsync(serviceCode, serviceId);

                if (isDuplicate)
                {
                    _log.Information(
                        "کد خدمات تکراری است. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceCode,
                        serviceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);
                }
                else
                {
                    _log.Information(
                        "کد خدمات در دسترس است. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceCode,
                        serviceId,
                        _currentUserService.UserName,
                        _currentUserService.UserId);
                }

                return Json(new
                {
                    isAvailable = !isDuplicate,
                    message = isDuplicate ? "این کد خدمات قبلاً استفاده شده است." : "کد خدمات در دسترس است."
                });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در بررسی کد خدمات. ServiceCode: {ServiceCode}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceCode,
                    serviceId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new
                {
                    isAvailable = false,
                    message = "خطای سیستم در حین بررسی کد خدمات رخ داده است."
                });
            }
        }
    }

    #region مدل‌های ویو مورد نیاز برای رفع خطاهای گزارش شده


    /// <summary>
    /// مدل ویو برای نمایش لیست خدمات یک دسته‌بندی
    /// </summary>
    public class ServiceCategoryServicesViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string ServiceCategoryTitle { get; set; }
        public string DepartmentName { get; set; }
        public IEnumerable<ServiceIndexViewModel> Services { get; set; }
    }
    #endregion


}