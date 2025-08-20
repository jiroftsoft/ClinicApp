using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers;

/// <summary>
/// کنترلر مدیریت دسته‌بندی‌های خدمات پزشکی با رعایت کامل استانداردهای سیستم‌های پزشکی و امنیت اطلاعات
/// این کنترلر تمام عملیات مربوط به دسته‌بندی‌های خدمات پزشکی را پشتیبانی می‌کند
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
/// </summary>
//[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
[RoutePrefix("ServiceCategories")]
public class ServiceCategoryController : Controller
{
    private readonly IServiceCategoryService _serviceCategoryService;
    private readonly IDepartmentService _departmentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _log;

    public ServiceCategoryController(
        IServiceCategoryService serviceCategoryService,
        IDepartmentService departmentService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _serviceCategoryService = serviceCategoryService;
        _departmentService = departmentService;
        _currentUserService = currentUserService;
        _log = logger.ForContext<ServiceCategoryController>();
    }


    /// <summary>
    /// نمایش لیست دسته‌بندی‌های خدمات با قابلیت جستجو، فیلتر و صفحه‌بندی
    /// </summary>
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> Index(string searchTerm = "", int? departmentId = null, int page = 1)
    {
        _log.Information(
            "درخواست نمایش لیست دسته‌بندی‌های خدمات. Term: {SearchTerm}, DepartmentId: {DepartmentId}, Page: {Page}. User: {UserName} (Id: {UserId})",
            searchTerm,
            departmentId,
            page,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            page = page < 1 ? 1 : page;
            const int pageSize = 10;

            // دریافت لیست دپارتمان‌ها
            var departments = await GetActiveDepartments();

            // دریافت نتایج جستجو
            var result = await _serviceCategoryService.SearchServiceCategoriesAsync(
                searchTerm,
                departmentId,
                page,
                pageSize);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات جستجوی دسته‌بندی‌های خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = result.Message;
                return View(CreateViewModel(null, departments, departmentId, searchTerm, page));
            }

            // محاسبه آمار
            var totalCategories = result.Data.TotalItems;
            var activeCategories = result.Data.Items.Count(x => x.IsActive);

            return View(CreateViewModel(result.Data, departments, departmentId, searchTerm, page, totalCategories, activeCategories));
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در نمایش لیست دسته‌بندی‌های خدمات. Term: {SearchTerm}, DepartmentId: {DepartmentId}, Page: {Page}. User: {UserName} (Id: {UserId})",
                searchTerm,
                departmentId,
                page,
                _currentUserService.UserName,
                _currentUserService.UserId);

            TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
            return View(CreateViewModel(null, await GetActiveDepartments(), departmentId, searchTerm, page));
        }
    }

    /// <summary>
    /// ایجاد مدل ویو برای صفحه Index
    /// </summary>
    private ServiceCategoryIndexViewModel CreateViewModel(
        PagedResult<ServiceCategoryIndexItemViewModel> serviceCategories,
        IEnumerable<SelectListItem> departments,
        int? selectedDepartmentId,
        string searchTerm,
        int currentPage,
        int totalCategories = 0,
        int activeCategories = 0)
    {
        return new ServiceCategoryIndexViewModel
        {
            ServiceCategories = serviceCategories ?? new PagedResult<ServiceCategoryIndexItemViewModel>(),
            Departments = departments,
            SelectedDepartmentId = selectedDepartmentId,
            SearchTerm = searchTerm,
            CurrentPage = currentPage,
            TotalCategories = totalCategories,
            ActiveCategories = activeCategories
        };
    }
    /// <summary>
    /// نمایش فرم ایجاد دسته‌بندی خدمات جدید
    /// این متد بر اساس نوع درخواست (عادی یا AJAX) عمل می‌کند
    /// </summary>
    [HttpGet]
    [Route("Create")]
    public async Task<ActionResult> Create(bool isPartial = false)
    {
        _log.Information(
            "درخواست نمایش فرم ایجاد دسته‌بندی خدمات. IsPartial: {IsPartial}. User: {UserName} (Id: {UserId})",
            isPartial,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // بررسی وجود کاربر
            if (string.IsNullOrEmpty(_currentUserService.UserId))
            {
                _log.Warning("کاربر غیرمجاز در تلاش برای دسترسی به فرم ایجاد. User: {UserName}",
                    _currentUserService.UserName);

                return RedirectToAction("Login", "Account");
            }

            ViewBag.Departments = await GetActiveDepartments();

            // اگر درخواست از طریق AJAX باشد، پارشال ویو را برمی‌گردانیم
            if (isPartial || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreatePartial", new ServiceCategoryCreateEditViewModel { IsActive = true });
            }

            // در غیر این صورت ویوی کامل را برمی‌گردانیم
            return View(new ServiceCategoryCreateEditViewModel { IsActive = true });
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در نمایش فرم ایجاد دسته‌بندی خدمات. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "خطای سیستم رخ داده است." });
            }

            TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// ایجاد دسته‌بندی خدمات جدید
    /// این متد برای پاسخ به درخواست‌های AJAX نیز طراحی شده است
    /// </summary>
    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(ServiceCategoryCreateEditViewModel model)
    {
        _log.Information(
            "درخواست ایجاد دسته‌بندی خدمات با نام {Title}. User: {UserName} (Id: {UserId})",
            model.Title,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // بررسی وجود کاربر
            if (string.IsNullOrEmpty(_currentUserService.UserId))
            {
                _log.Warning("کاربر غیرمجاز در تلاش برای ایجاد دسته‌بندی. User: {UserName}",
                    _currentUserService.UserName);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "کاربر شناسایی نشد. لطفاً مجدداً وارد سیستم شوید." });
                }

                return RedirectToAction("Login", "Account");
            }

            // ایجاد دسته‌بندی خدمات
            var result = await _serviceCategoryService.CreateServiceCategoryAsync(model);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات ایجاد دسته‌بندی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // اگر درخواست از طریق AJAX باشد، پاسخ JSON برمی‌گردانیم
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = result.Message });
                }

                ViewBag.Departments = await GetActiveDepartments();
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information(
                "دسته‌بندی خدمات با موفقیت ایجاد شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                result.Data,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // اگر درخواست از طریق AJAX باشد، پاسخ موفقیت‌آمیز JSON برمی‌گردانیم
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index") });
            }

            TempData["SuccessMessage"] = "دسته‌بندی خدمات با موفقیت ایجاد شد.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در ایجاد دسته‌بندی خدمات. Title: {Title}. User: {UserName} (Id: {UserId})",
                model.Title,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // اگر درخواست از طریق AJAX باشد، پاسخ خطا JSON برمی‌گردانیم
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "خطای سیستم رخ داده است." });
            }

            ViewBag.Departments = await GetActiveDepartments();
            ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            return View(model);
        }
    }

    /// <summary>
    /// نمایش فرم ویرایش دسته‌بندی خدمات
    /// </summary>
    [HttpGet]
    [Route("Edit/{id:int}")]
    public async Task<ActionResult> Edit(int id)
    {
        _log.Information(
            "درخواست نمایش فرم ویرایش دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            id,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (id <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "شناسه دسته‌بندی خدمات معتبر نیست.";
                return RedirectToAction("Index");
            }

            var result = await _serviceCategoryService.GetServiceCategoryForEditAsync(id);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات دریافت اطلاعات دسته‌بندی خدمات برای ویرایش با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            ViewBag.Departments = await GetActiveDepartments();
            return View(result.Data);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در نمایش فرم ویرایش دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// به‌روزرسانی دسته‌بندی خدمات
    /// </summary>
    [HttpPost]
    [Route("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Edit(ServiceCategoryCreateEditViewModel model)
    {
        _log.Information(
            "درخواست ویرایش دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            model.ServiceCategoryId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (model.ServiceCategoryId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش دسته‌بندی خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                    model.ServiceCategoryId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "شناسه دسته‌بندی خدمات معتبر نیست.";
                return RedirectToAction("Index");
            }

            // اعتبارسنجی مدل
            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "مدل ویرایش دسته‌بندی خدمات نامعتبر است. Errors: {Errors}. User: {UserName} (Id: {UserId})",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)),
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                ViewBag.Departments = await GetActiveDepartments();
                return View(model);
            }

            // به‌روزرسانی دسته‌بندی خدمات
            var result = await _serviceCategoryService.UpdateServiceCategoryAsync(model);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات به‌روزرسانی دسته‌بندی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                ViewBag.Departments = await GetActiveDepartments();
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information(
                "دسته‌بندی خدمات با موفقیت به‌روزرسانی شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                model.ServiceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            TempData["SuccessMessage"] = "اطلاعات دسته‌بندی خدمات با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در ویرایش دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                model.ServiceCategoryId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            ViewBag.Departments = await GetActiveDepartments();
            ModelState.AddModelError("", "خطای سیستم در حین ذخیره تغییرات رخ داده است.");
            return View(model);
        }
    }

    /// <summary>
    /// حذف دسته‌بندی خدمات با پشتیبانی کامل از درخواست‌های AJAX
    /// این متد بر اساس نوع درخواست (عادی یا AJAX) عمل می‌کند
    /// </summary>
    [HttpPost]
    [Route("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Delete(int id)
    {
        _log.Information(
            "درخواست حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            id,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (id <= 0)
            {
                _log.Warning(
                    "درخواست حذف دسته‌بندی خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // بررسی نوع درخواست
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "شناسه دسته‌بندی خدمات معتبر نیست." });
                }

                TempData["ErrorMessage"] = "شناسه دسته‌بندی خدمات معتبر نیست.";
                return RedirectToAction("Index");
            }

            // بررسی امکان حذف
            var canDeleteResult = await _serviceCategoryService.CanDeleteServiceCategoryAsync(id);
            if (!canDeleteResult.Success)
            {
                _log.Warning(
                    "حذف دسته‌بندی خدمات با شناسه {Id} امکان‌پذیر نیست. Reason: {Reason}. User: {UserName} (Id: {UserId})",
                    id,
                    canDeleteResult.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // بررسی نوع درخواست
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = canDeleteResult.Message });
                }

                TempData["ErrorMessage"] = canDeleteResult.Message;
                return RedirectToAction("Index");
            }

            // حذف دسته‌بندی خدمات
            var result = await _serviceCategoryService.DeleteServiceCategoryAsync(id);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات حذف دسته‌بندی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // بررسی نوع درخواست
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = result.Message });
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            _log.Information(
                "دسته‌بندی خدمات با موفقیت حذف شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // بررسی نوع درخواست
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = "دسته‌بندی خدمات با موفقیت حذف شد.",
                    redirectUrl = Url.Action("Index")
                });
            }

            TempData["SuccessMessage"] = "دسته‌بندی خدمات با موفقیت حذف شد.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // بررسی نوع درخواست
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = false,
                    message = "خطای سیستم در حین حذف دسته‌بندی خدمات رخ داده است."
                });
            }

            TempData["ErrorMessage"] = "خطای سیستم در حین حذف دسته‌بندی خدمات رخ داده است.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// نمایش جزئیات دسته‌بندی خدمات
    /// </summary>
    [HttpGet]
    [Route("Details/{id:int}")]
    public async Task<ActionResult> Details(int id)
    {
        _log.Information(
            "درخواست نمایش جزئیات دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            id,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (id <= 0)
            {
                _log.Warning(
                    "درخواست جزئیات دسته‌بندی خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "شناسه دسته‌بندی خدمات معتبر نیست.";
                return RedirectToAction("Index");
            }

            var result = await _serviceCategoryService.GetServiceCategoryDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات دریافت جزئیات دسته‌بندی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
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
                "خطا در نمایش جزئیات دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// دریافت لیست دپارتمان‌های فعال برای استفاده در فرم‌ها
    /// </summary>
    private async Task<IEnumerable<SelectListItem>> GetActiveDepartments()
    {
        try
        {
            var departments = await _departmentService.GetActiveDepartmentsAsync();
            return departments.Select(d => new SelectListItem
            {
                Value = d.DepartmentId.ToString(),
                Text = $@"{d.Name} - {d.ClinicName}"
            });
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دپارتمان‌های فعال. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName,
                _currentUserService.UserId);

            return new List<SelectListItem>();
        }
    }

    /// <summary>
    /// بررسی امکان حذف دسته‌بندی خدمات قبل از نمایش صفحه حذف
    /// </summary>
    [HttpGet]
    [Route("CanDelete/{id:int}")]
    public async Task<ActionResult> CanDelete(int id)
    {
        _log.Information(
            "درخواست بررسی امکان حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
            id,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (id <= 0)
            {
                _log.Warning(
                    "درخواست بررسی امکان حذف دسته‌بندی خدمات با شناسه نامعتبر. Id: {Id}. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "شناسه دسته‌بندی خدمات معتبر نیست." }, JsonRequestBehavior.AllowGet);
            }

            var result = await _serviceCategoryService.CanDeleteServiceCategoryAsync(id);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                serviceCount = await _serviceCategoryService.GetActiveServiceCountAsync(id)
            }, JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در بررسی امکان حذف دسته‌بندی خدمات با شناسه {Id}. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
        }
    }

    /// <summary>
    /// دریافت لیست دسته‌بندی‌های خدمات برای یک دپارتمان خاص (برای استفاده در APIهای جاوااسکریپت)
    /// </summary>
    [HttpGet]
    [Route("GetByDepartment/{departmentId:int}")]
    public async Task<ActionResult> GetByDepartment(int departmentId)
    {
        _log.Information(
            "درخواست دریافت لیست دسته‌بندی‌های خدمات برای دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
            departmentId,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی
            if (departmentId <= 0)
            {
                _log.Warning(
                    "درخواست دریافت لیست دسته‌بندی‌ها با شناسه دپارتمان نامعتبر. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    departmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "شناسه دپارتمان معتبر نیست." }, JsonRequestBehavior.AllowGet);
            }

            var categories = await _serviceCategoryService.GetActiveServiceCategoriesByDepartmentAsync(departmentId);

            var result = categories.Select(c => new
            {
                id = c.ServiceCategoryId,
                text = c.Title
            });

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در دریافت لیست دسته‌بندی‌های خدمات برای دپارتمان {DepartmentId}. User: {UserName} (Id: {UserId})",
                departmentId,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
        }
    }

    /// <summary>
    /// ایجاد سریع دسته‌بندی خدمات (برای استفاده در APIهای جاوااسکریپت)
    /// </summary>
    [HttpPost]
    [Route("QuickCreate")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> QuickCreate(ServiceCategoryQuickCreateViewModel model)
    {
        _log.Information(
            "درخواست ایجاد سریع دسته‌بندی خدمات با نام {Title}. User: {UserName} (Id: {UserId})",
            model.Title,
            _currentUserService.UserName,
            _currentUserService.UserId);

        try
        {
            // اعتبارسنجی ورودی‌ها
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                _log.Warning(
                    "درخواست ایجاد سریع دسته‌بندی خدمات با عنوان خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "عنوان دسته‌بندی الزامی است." }, JsonRequestBehavior.AllowGet);
            }

            if (model.Title.Length > 200)
            {
                _log.Warning(
                    "درخواست ایجاد سریع دسته‌بندی خدمات با عنوان طولانی. Length: {Length}. User: {UserName} (Id: {UserId})",
                    model.Title.Length,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "عنوان دسته‌بندی نمی‌تواند بیشتر از 200 کاراکتر باشد." }, JsonRequestBehavior.AllowGet);
            }

            if (model.DepartmentId <= 0)
            {
                _log.Warning(
                    "درخواست ایجاد سریع دسته‌بندی خدمات با دپارتمان نامعتبر. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    model.DepartmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "دپارتمان انتخاب شده معتبر نیست." }, JsonRequestBehavior.AllowGet);
            }

            // بررسی وجود دپارتمان معتبر
            if (!await _departmentService.IsActiveDepartmentExistsAsync(model.DepartmentId))
            {
                _log.Warning(
                    "درخواست ایجاد سریع دسته‌بندی خدمات با دپارتمان غیرموجود. DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    model.DepartmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "دپارتمان انتخاب شده معتبر نیست یا حذف شده است." }, JsonRequestBehavior.AllowGet);
            }

            // بررسی وجود دسته‌بندی تکراری در دپارتمان
            if (await _serviceCategoryService.IsDuplicateServiceCategoryNameAsync(model.Title, model.DepartmentId))
            {
                _log.Warning(
                    "درخواست ایجاد سریع دسته‌بندی خدمات با نام تکراری. Title: {Title}, DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})",
                    model.Title,
                    model.DepartmentId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = "دسته‌بندی خدماتی با این نام در این دپارتمان از قبل وجود دارد." }, JsonRequestBehavior.AllowGet);
            }

            // ساخت مدل برای ایجاد
            var createModel = new ServiceCategoryCreateEditViewModel
            {
                Title = model.Title,
                DepartmentId = model.DepartmentId,
                IsActive = true
            };

            // ایجاد دسته‌بندی خدمات
            var result = await _serviceCategoryService.CreateServiceCategoryAsync(createModel);

            if (!result.Success)
            {
                _log.Warning(
                    "عملیات ایجاد سریع دسته‌بندی خدمات با خطا مواجه شد. Message: {Message}. User: {UserName} (Id: {UserId})",
                    result.Message,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }

            _log.Information(
                "دسته‌بندی خدمات با موفقیت ایجاد شد. ServiceCategoryId: {ServiceCategoryId}. User: {UserName} (Id: {UserId})",
                result.Data,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // دریافت اطلاعات کامل دسته‌بندی برای پاسخ
            var categoryDetails = await _serviceCategoryService.GetServiceCategoryDetailsAsync(result.Data);
            var category = categoryDetails.Data;

            return Json(new
            {
                success = true,
                id = result.Data,
                text = category.Title,
                departmentName = category.DepartmentName
            }, JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            _log.Error(
                ex,
                "خطا در ایجاد سریع دسته‌بندی خدمات. Title: {Title}. User: {UserName} (Id: {UserId})",
                model.Title,
                _currentUserService.UserName,
                _currentUserService.UserId);

            return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
        }
    }
}