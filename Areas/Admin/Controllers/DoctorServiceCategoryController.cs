using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
// طبق DESIGN_PRINCIPLES_CONTRACT از AutoMapper استفاده نمی‌کنیم
// از Factory Method Pattern استفاده می‌کنیم
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Linq;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت صلاحیت‌های خدماتی پزشکان در سیستم کلینیک شفا
    /// مسئولیت: مدیریت صلاحیت‌های دسترسی پزشکان به دسته‌بندی‌های خدمات
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorServiceCategoryController : Controller
    {
        private readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorServiceCategoryViewModel> _serviceCategoryValidator;
        private readonly ILogger _logger;

        public DoctorServiceCategoryController(
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorServiceCategoryViewModel> serviceCategoryValidator
            )
        {
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _serviceCategoryValidator = serviceCategoryValidator ?? throw new ArgumentNullException(nameof(serviceCategoryValidator));
            _logger = Log.ForContext<DoctorServiceCategoryController>();
        }

        #region Index and List Operations

        /// <summary>
        /// نمایش لیست صلاحیت‌های خدماتی پزشکان
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(int page = 1, int pageSize = 20, int? doctorId = null, int? serviceCategoryId = null, bool? isActive = null)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست صلاحیت‌های خدماتی پزشکان. صفحه: {Page}, اندازه: {PageSize}, کاربر: {UserId}, IP: {IPAddress}", 
                    page, pageSize, _currentUserService.UserId, GetClientIPAddress());

                // اعتبارسنجی پارامترهای ورودی
                if (page <= 0 || pageSize <= 0)
                {
                    _logger.Warning("پارامترهای نامعتبر صفحه. صفحه: {Page}, اندازه: {PageSize}, کاربر: {UserId}", 
                        page, pageSize, _currentUserService.UserId);
                    TempData["Error"] = "پارامترهای صفحه نامعتبر است.";
                    return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>(), PageNumber = page, PageSize = pageSize, TotalItems = 0 });
                }

                // محدود کردن اندازه صفحه برای جلوگیری از overload
                if (pageSize > 100) pageSize = 100;

                // اگر doctorId مشخص نشده، یک پزشک پیش‌فرض انتخاب شود (اولین پزشک فعال)
                int effectiveDoctorId = doctorId.GetValueOrDefault(0);
                if (effectiveDoctorId <= 0)
                {
                    try
                    {
                        var filter = new DoctorSearchViewModel { PageNumber = 1, PageSize = 1, IsActive = true };
                        var doctorsResult = await _doctorCrudService.GetDoctorsAsync(filter);
                        if (doctorsResult.Success && doctorsResult.Data?.Items?.Any() == true)
                        {
                            effectiveDoctorId = doctorsResult.Data.Items.First().DoctorId;
                            _logger.Information("پزشک پیش‌فرض برای نمایش انتخاب شد: {DoctorId}", effectiveDoctorId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "خطا در دریافت پزشک پیش‌فرض برای لیست صلاحیت‌ها");
                    }
                }

                // دریافت لیست صلاحیت‌ها - استفاده از متد موجود (در صورت doctorId=0 ممکن است همه را برگرداند)
                var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(effectiveDoctorId, "", page, pageSize);
                
                if (!result.Success)
                {
                    _logger.Error("خطا در دریافت لیست صلاحیت‌ها. پیام: {ErrorMessage}, کاربر: {UserId}", 
                        result.Message, _currentUserService.UserId);
                    TempData["Error"] = result.Message;
                    return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>(), PageNumber = page, PageSize = pageSize, TotalItems = 0 });
                }

                var data = result.Data ?? new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>(), PageNumber = page, PageSize = pageSize, TotalItems = 0 };
                var items = data.Items ?? new List<DoctorServiceCategoryViewModel>();

                // فیلترهای درون‌حافظه‌ای برای سرفصل و وضعیت
                if (serviceCategoryId.HasValue && serviceCategoryId.Value > 0)
                {
                    items = items.Where(x => x.ServiceCategoryId == serviceCategoryId.Value).ToList();
                }
                if (isActive.HasValue)
                {
                    items = items.Where(x => x.IsActive == isActive.Value).ToList();
                }

                // اگر doctorId مشخص نشده بود و سرویس فقط برای یک پزشک برمی‌گرداند، حداقل داده‌های فیلتر شده را نشان می‌دهیم
                var viewPaged = new PagedResult<DoctorServiceCategoryViewModel>
                {
                    Items = items,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = items.Count
                };

                if (viewPaged.Items == null || !viewPaged.Items.Any())
                {
                    _logger.Warning("داده‌ای برای نمایش وجود ندارد. فیلترها - DoctorId: {DoctorId}, ServiceCategoryId: {ServiceCategoryId}, IsActive: {IsActive}",
                        effectiveDoctorId, serviceCategoryId, isActive);
                }

                _logger.Information("لیست صلاحیت‌های خدماتی پزشکان با موفقیت بارگذاری شد. تعداد: {Count}, صفحه: {Page}, کاربر: {UserId}", 
                    viewPaged.Items.Count, page, _currentUserService.UserId);
                
                return View(viewPaged);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در بارگذاری لیست صلاحیت‌های خدماتی پزشکان. صفحه: {Page}, کاربر: {UserId}", 
                    page, _currentUserService.UserId);
                TempData["Error"] = "خطا در بارگذاری لیست صلاحیت‌ها";
                return View(new PagedResult<DoctorServiceCategoryViewModel> { Items = new List<DoctorServiceCategoryViewModel>(), PageNumber = page, PageSize = pageSize, TotalItems = 0 });
            }
        }

        #endregion

        #region Service Category Permissions

        /// <summary>
        /// نمایش صلاحیت‌های دسته‌بندی خدمات پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ServiceCategoryPermissions(int doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId} توسط کاربر {UserId}", doctorId, _currentUserService.UserId);

                if (doctorId <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index", "Doctor");
                }

                // بررسی وجود پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت صلاحیت‌های دسته‌بندی خدمات
                var permissionsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);
                if (!permissionsResult.Success)
                {
                    TempData["Error"] = permissionsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                // ایجاد ViewModel نهایی
                var viewModel = new DoctorServiceCategoryPermissionsViewModel
                {
                    Doctor = doctorResult.Data,
                    Permissions = permissionsResult.Data.Items?.ToList() ?? new List<DoctorServiceCategoryViewModel>(),
                    Stats = new ServiceCategoryPermissionStatsViewModel
                    {
                        TotalPermissions = permissionsResult.Data.Items?.Count ?? 0,
                        ActivePermissions = permissionsResult.Data.Items?.Count(p => p.IsActive) ?? 0,
                        InactivePermissions = permissionsResult.Data.Items?.Count(p => !p.IsActive) ?? 0,
                        LastPermissionDate = permissionsResult.Data.Items?.Any() == true ? 
                            permissionsResult.Data.Items.Max(p => p.CreatedAt) : (DateTime?)null
                    }
                };

                _logger.Information("صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId} با موفقیت نمایش داده شد. تعداد صلاحیت‌ها: {Count}", 
                    doctorId, viewModel.Permissions.Count);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId}", doctorId);
                TempData["Error"] = "خطا در بارگذاری صلاحیت‌های دسته‌بندی خدمات پزشک";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// اعطای صلاحیت دسته‌بندی خدمات به پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GrantPermission(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId} توسط کاربر {UserId}", 
                    model.ServiceCategoryId, model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعطای صلاحیت
                var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} با موفقیت به پزشک {DoctorId} اعطا شد", model.ServiceCategoryId, model.DoctorId);

                TempData["Success"] = "صلاحیت دسته‌بندی خدمات با موفقیت اعطا شد.";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", model.ServiceCategoryId, model.DoctorId);
                TempData["Error"] = "خطا در اعطای صلاحیت دسته‌بندی خدمات";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// لغو صلاحیت دسته‌بندی خدمات از پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RevokePermission(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId} توسط کاربر {UserId}", 
                    serviceCategoryId, doctorId, _currentUserService.UserId);

                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." });
                }

                // لغو صلاحیت
                var result = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, serviceCategoryId);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات {ServiceCategoryId} با موفقیت از پزشک {DoctorId} لغو شد", serviceCategoryId, doctorId);

                return Json(new { success = true, message = "صلاحیت دسته‌بندی خدمات با موفقیت لغو شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);
                return Json(new { success = false, message = "خطا در لغو صلاحیت دسته‌بندی خدمات" });
            }
        }

        /// <summary>
        /// به‌روزرسانی صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateServiceCategoryPermission(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی صلاحیت دسته‌بندی خدمات پزشک {DoctorId} توسط کاربر {UserId}", 
                    model.DoctorId, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده صحیح نیست.";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                // به‌روزرسانی صلاحیت
                var result = await _doctorServiceCategoryService.UpdateDoctorServiceCategoryAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
                }

                _logger.Information("صلاحیت دسته‌بندی خدمات پزشک {DoctorId} با موفقیت به‌روزرسانی شد", model.DoctorId);

                TempData["Success"] = "صلاحیت دسته‌بندی خدمات با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی صلاحیت دسته‌بندی خدمات پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در به‌روزرسانی صلاحیت دسته‌بندی خدمات";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// اعطای دسته‌ای صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkGrantPermission(int[] doctorIds, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست اعطای دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به {DoctorCount} پزشک توسط کاربر {UserId}", 
                    serviceCategoryId, doctorIds?.Length ?? 0, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "دسته‌بندی خدمات نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                                                 var permissionModel = new DoctorServiceCategoryViewModel
                         {
                             DoctorId = doctorId,
                             ServiceCategoryId = serviceCategoryId,
                             IsActive = true,
                             GrantedDate = DateTime.Now
                         };

                                                 var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(permissionModel);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در اعطای صلاحیت به پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در اعطای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} به پزشک {DoctorId}", serviceCategoryId, doctorId);
                        errors.Add($"خطا در اعطای صلاحیت به پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"صلاحیت دسته‌بندی خدمات به تعداد {successCount} پزشک با موفقیت اعطا شد."
                    : "هیچ صلاحیتی اعطا نشد.";

                if (errors.Any())
                {
                    message += $" خطاها: {string.Join(", ", errors.Take(3))}";
                }

                return Json(new { 
                    success = successCount > 0, 
                    message = message,
                    successCount = successCount,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعطای دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در اعطای دسته‌ای صلاحیت دسته‌بندی خدمات" });
            }
        }

        /// <summary>
        /// لغو دسته‌ای صلاحیت دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkRevokePermission(int[] doctorIds, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست لغو دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از {DoctorCount} پزشک توسط کاربر {UserId}", 
                    serviceCategoryId, doctorIds?.Length ?? 0, _currentUserService.UserId);

                if (doctorIds == null || doctorIds.Length == 0)
                {
                    return Json(new { success = false, message = "هیچ پزشکی انتخاب نشده است." });
                }

                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "دسته‌بندی خدمات نامعتبر است." });
                }

                int successCount = 0;
                var errors = new System.Collections.Generic.List<string>();

                foreach (var doctorId in doctorIds)
                {
                    try
                    {
                                                 var result = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, serviceCategoryId);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            errors.Add($"خطا در لغو صلاحیت از پزشک {doctorId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "خطا در لغو صلاحیت دسته‌بندی خدمات {ServiceCategoryId} از پزشک {DoctorId}", serviceCategoryId, doctorId);
                        errors.Add($"خطا در لغو صلاحیت از پزشک {doctorId}");
                    }
                }

                var message = successCount > 0 
                    ? $"صلاحیت دسته‌بندی خدمات از تعداد {successCount} پزشک با موفقیت لغو شد."
                    : "هیچ صلاحیتی لغو نشد.";

                if (errors.Any())
                {
                    message += $" خطاها: {string.Join(", ", errors.Take(3))}";
                }

                return Json(new { 
                    success = successCount > 0, 
                    message = message,
                    successCount = successCount,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در لغو دسته‌ای صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در لغو دسته‌ای صلاحیت دسته‌بندی خدمات" });
            }
        }

        #endregion

        #region Add and Remove Operations

        /// <summary>
        /// اضافه کردن صلاحیت خدماتی جدید
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddServiceCategory(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست اضافه کردن صلاحیت خدماتی جدید. پزشک: {DoctorId}, دسته‌بندی: {ServiceCategoryId}, کاربر: {UserId}, IP: {IPAddress}", 
                    model.DoctorId, model.ServiceCategoryId, _currentUserService.UserId, GetClientIPAddress());

                // تبدیل تاریخ شمسی به میلادی برای ذخیره در دیتابیس
                if (!string.IsNullOrEmpty(model.GrantedDateString))
                {
                    try
                    {
                        var grantedDate = PersianDateHelper.ConvertPersianToDateTime(model.GrantedDateString);
                        model.GrantedDate = grantedDate;
                        _logger.Information("تاریخ اعطا تبدیل شد: {PersianDate} -> {GregorianDate}", 
                            model.GrantedDateString, grantedDate.ToString("yyyy/MM/dd"));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "فرمت تاریخ اعطا نامعتبر: {PersianDate}, کاربر: {UserId}", 
                            model.GrantedDateString, _currentUserService.UserId);
                        return Json(new { success = false, message = "فرمت تاریخ اعطا نامعتبر است." });
                    }
                }

                if (!string.IsNullOrEmpty(model.ExpiryDateString))
                {
                    try
                    {
                        var expiryDate = PersianDateHelper.ConvertPersianToDateTime(model.ExpiryDateString);
                        model.ExpiryDate = expiryDate;
                        _logger.Information("تاریخ انقضا تبدیل شد: {PersianDate} -> {GregorianDate}", 
                            model.ExpiryDateString, expiryDate.ToString("yyyy/MM/dd"));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "فرمت تاریخ انقضا نامعتبر: {PersianDate}, کاربر: {UserId}", 
                            model.ExpiryDateString, _currentUserService.UserId);
                        return Json(new { success = false, message = "فرمت تاریخ انقضا نامعتبر است." });
                    }
                }

                // اعتبارسنجی مدل
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.Warning("اعتبارسنجی مدل ناموفق. خطاها: {Errors}, کاربر: {UserId}", errors, _currentUserService.UserId);
                    return Json(new { success = false, message = "اطلاعات وارد شده صحیح نیست." });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    _logger.Warning("اعتبارسنجی FluentValidation ناموفق. خطاها: {Errors}, کاربر: {UserId}", errors, _currentUserService.UserId);
                    return Json(new { success = false, message = $"خطا در اعتبارسنجی: {errors}" });
                }

                // اضافه کردن صلاحیت
                var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(model);

                if (!result.Success)
                {
                    _logger.Error("خطا در اضافه کردن صلاحیت خدماتی. پیام: {ErrorMessage}, پزشک: {DoctorId}, دسته‌بندی: {ServiceCategoryId}, کاربر: {UserId}", 
                        result.Message, model.DoctorId, model.ServiceCategoryId, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("صلاحیت خدماتی جدید با موفقیت اضافه شد. پزشک: {DoctorId}, دسته‌بندی: {ServiceCategoryId}, کاربر: {UserId}", 
                    model.DoctorId, model.ServiceCategoryId, _currentUserService.UserId);

                return Json(new { success = true, message = "صلاحیت خدماتی جدید با موفقیت اضافه شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در اضافه کردن صلاحیت خدماتی جدید. پزشک: {DoctorId}, دسته‌بندی: {ServiceCategoryId}, کاربر: {UserId}", 
                    model.DoctorId, model.ServiceCategoryId, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در اضافه کردن صلاحیت خدماتی جدید" });
            }
        }

        /// <summary>
        /// حذف صلاحیت خدماتی
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveServiceCategory(int doctorId, int categoryId)
        {
            try
            {
                _logger.Information("درخواست حذف صلاحیت خدماتی. پزشک: {DoctorId}, دسته‌بندی: {CategoryId}, کاربر: {UserId}, IP: {IPAddress}", 
                    doctorId, categoryId, _currentUserService.UserId, GetClientIPAddress());

                if (doctorId <= 0 || categoryId <= 0)
                {
                    _logger.Warning("پارامترهای ورودی نامعتبر. پزشک: {DoctorId}, دسته‌بندی: {CategoryId}, کاربر: {UserId}", 
                        doctorId, categoryId, _currentUserService.UserId);
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." });
                }

                // حذف صلاحیت
                var result = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, categoryId);

                if (!result.Success)
                {
                    _logger.Error("خطا در حذف صلاحیت خدماتی. پیام: {ErrorMessage}, پزشک: {DoctorId}, دسته‌بندی: {CategoryId}, کاربر: {UserId}", 
                        result.Message, doctorId, categoryId, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("صلاحیت خدماتی با موفقیت حذف شد. پزشک: {DoctorId}, دسته‌بندی: {CategoryId}, کاربر: {UserId}", 
                    doctorId, categoryId, _currentUserService.UserId);

                return Json(new { success = true, message = "صلاحیت خدماتی با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در حذف صلاحیت خدماتی. پزشک: {DoctorId}, دسته‌بندی: {CategoryId}, کاربر: {UserId}", 
                    doctorId, categoryId, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در حذف صلاحیت خدماتی" });
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت لیست صلاحیت‌های دسته‌بندی خدمات پزشک (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctorServiceCategoryPermissions(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت صلاحیت‌های دسته‌بندی خدمات پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطا در دریافت صلاحیت‌های دسته‌بندی خدمات پزشک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات (AJAX)
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست دسته‌بندی‌های خدمات توسط کاربر {UserId}, IP: {IPAddress}", 
                    _currentUserService.UserId, GetClientIPAddress());

                // دریافت لیست دسته‌بندی‌های خدمات
                var result = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دسته‌بندی‌های خدمات. پیام: {ErrorMessage}, کاربر: {UserId}", 
                        result.Message, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var data = result.Data ?? new List<LookupItemViewModel>();
                var categories = data.Select(c => new { Id = c.Id, Name = c.Name }).ToList();

                _logger.Information("لیست دسته‌بندی‌های خدمات با موفقیت ارسال شد. تعداد: {Count}, کاربر: {UserId}", 
                    categories.Count, _currentUserService.UserId);

                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در دریافت لیست دسته‌بندی‌های خدمات. کاربر: {UserId}", 
                    _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت لیست دسته‌بندی‌های خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی دسترسی پزشک به دسته‌بندی خدمات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckDoctorServiceCategoryAccess(int doctorId, int serviceCategoryId)
        {
            try
            {
                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "پارامترهای ورودی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var hasAccess = result.Data?.Items?.Any(permission => 
                    permission.ServiceCategoryId == serviceCategoryId && permission.IsActive) ?? false;

                return Json(new { success = true, hasAccess = hasAccess }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", doctorId, serviceCategoryId);
                return Json(new { success = false, message = "خطا در بررسی دسترسی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategoryDoctors(int serviceCategoryId)
        {
            try
            {
                if (serviceCategoryId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دسته‌بندی خدمات نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                // این متد باید در سرویس پیاده‌سازی شود
                // فعلاً یک نمونه ساده
                return Json(new { success = true, data = new object[0] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات {ServiceCategoryId}", serviceCategoryId);
                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان با صلاحیت دسته‌بندی خدمات" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Duplicate GetServiceCategories method removed - using the improved version above

        #endregion

        #region Assignment Operations

        /// <summary>
        /// نمایش فرم انتساب پزشک به دسته‌بندی خدمات
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> AssignToServiceCategory(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم انتساب پزشک {DoctorId} به دسته‌بندی خدمات", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var doctor = doctorResult.Data;

                var model = new DoctorServiceCategoryViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    IsActive = true,
                    GrantedDate = DateTime.Now
                };

                // دریافت لیست دسته‌بندی‌های خدمات فعال
                var serviceCategoriesResult = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!serviceCategoriesResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دسته‌بندی‌های خدمات");
                    TempData["Error"] = serviceCategoriesResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                ViewBag.ServiceCategories = serviceCategoriesResult.Data.Select(sc => new { Value = sc.Id, Text = sc.Name }).ToList();

                _logger.Information("فرم انتساب پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتساب پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم انتساب";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// پردازش انتساب پزشک به دسته‌بندی خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignToServiceCategory(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتساب پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId}", 
                    model.DoctorId, model.ServiceCategoryId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل انتساب نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("AssignToServiceCategory", new { doctorId = model.DoctorId });
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return RedirectToAction("AssignToServiceCategory", new { doctorId = model.DoctorId });
                }

                // انتساب پزشک به دسته‌بندی خدمات
                var result = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("انتساب پزشک {DoctorId} ناموفق بود: {Message}", model.DoctorId, result.Message);
                    TempData["Error"] = result.Message;
                    return RedirectToAction("AssignToServiceCategory", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتساب پزشک {DoctorId} به دسته‌بندی خدمات {ServiceCategoryId} با موفقیت انجام شد", 
                    model.DoctorId, model.ServiceCategoryId);
                TempData["Success"] = "انتساب پزشک با موفقیت انجام شد";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دسته‌بندی خدمات", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات انتساب";
                return RedirectToAction("AssignToServiceCategory", new { doctorId = model.DoctorId });
            }
        }

        /// <summary>
        /// نمایش فرم انتقال صلاحیت‌های خدماتی پزشک
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> TransferServiceCategory(int? doctorId)
        {
            try
            {
                _logger.Information("درخواست نمایش فرم انتقال صلاحیت‌های خدماتی پزشک {DoctorId}", doctorId);

                if (!doctorId.HasValue || doctorId.Value <= 0)
                {
                    _logger.Warning("شناسه پزشک نامعتبر یا خالی: {DoctorId}", doctorId);
                    TempData["Error"] = "شناسه پزشک نامعتبر است";
                    return RedirectToAction("Index", "Doctor");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId.Value);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId.Value);
                    TempData["Error"] = doctorResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var doctor = doctorResult.Data;

                // دریافت صلاحیت‌های فعلی پزشک
                var permissionsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId.Value, "", 1, 100);
                if (!permissionsResult.Success)
                {
                    _logger.Warning("صلاحیت‌های پزشک {DoctorId} یافت نشد", doctorId);
                    TempData["Error"] = permissionsResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var permissions = permissionsResult.Data;

                // تعیین دسته‌بندی فعلی (اولین دسته‌بندی فعال)
                var currentServiceCategory = permissions.Items.FirstOrDefault(psc => psc.IsActive);
                
                var model = new DoctorServiceCategoryViewModel
                {
                    DoctorId = doctorId.Value,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    ServiceCategoryId = currentServiceCategory?.ServiceCategoryId ?? 0,
                    ServiceCategoryTitle = currentServiceCategory?.ServiceCategoryTitle ?? "بدون صلاحیت",
                    IsActive = true
                };

                // دریافت لیست دسته‌بندی‌های خدمات فعال
                var serviceCategoriesResult = await _doctorServiceCategoryService.GetAllServiceCategoriesAsync();
                if (!serviceCategoriesResult.Success)
                {
                    _logger.Warning("خطا در دریافت لیست دسته‌بندی‌های خدمات");
                    TempData["Error"] = serviceCategoriesResult.Message;
                    return RedirectToAction("Index", "Doctor");
                }

                var availableServiceCategories = serviceCategoriesResult.Data
                    .Where(sc => sc.Id != currentServiceCategory?.ServiceCategoryId)
                    .Select(sc => new System.Web.Mvc.SelectListItem 
                    { 
                        Value = sc.Id.ToString(), 
                        Text = sc.Name 
                    })
                    .ToList();

                var viewModel = new DoctorServiceCategoryAssignFormViewModel
                {
                    Doctor = doctor,
                    Assignment = model,
                    AvailableServiceCategories = availableServiceCategories
                };

                _logger.Information("فرم انتقال صلاحیت‌های خدماتی پزشک {DoctorId} با موفقیت نمایش داده شد", doctorId.Value);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم انتقال صلاحیت‌های خدماتی پزشک {DoctorId}", doctorId?.ToString() ?? "null");
                TempData["Error"] = "خطا در بارگذاری فرم انتقال";
                return RedirectToAction("Index", "Doctor");
            }
        }

        /// <summary>
        /// پردازش انتقال صلاحیت‌های خدماتی پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TransferServiceCategory(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست انتقال صلاحیت‌های خدماتی پزشک {DoctorId} از دسته‌بندی {FromServiceCategoryId} به دسته‌بندی {ToServiceCategoryId}", 
                    model.DoctorId, model.ServiceCategoryId, model.ServiceCategoryId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("مدل انتقال نامعتبر برای پزشک {DoctorId}", model.DoctorId);
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است";
                    return RedirectToAction("TransferServiceCategory", new { doctorId = model.DoctorId });
                }

                // حذف از دسته‌بندی فعلی
                var revokeResult = await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(model.DoctorId, model.ServiceCategoryId);
                if (!revokeResult.Success)
                {
                    _logger.Warning("خطا در حذف از دسته‌بندی فعلی: {Message}", revokeResult.Message);
                    TempData["Error"] = $"خطا در حذف از دسته‌بندی فعلی: {revokeResult.Message}";
                    return RedirectToAction("TransferServiceCategory", new { doctorId = model.DoctorId });
                }

                // انتساب به دسته‌بندی جدید
                var assignModel = new DoctorServiceCategoryViewModel
                {
                    DoctorId = model.DoctorId,
                    ServiceCategoryId = model.ServiceCategoryId,
                    IsActive = true,
                    GrantedDate = DateTime.Now,
                    AuthorizationLevel = model.AuthorizationLevel ?? "پزشک عادی"
                };

                var assignResult = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(assignModel);
                if (!assignResult.Success)
                {
                    _logger.Warning("خطا در انتساب به دسته‌بندی جدید: {Message}", assignResult.Message);
                    TempData["Error"] = $"خطا در انتساب به دسته‌بندی جدید: {assignResult.Message}";
                    return RedirectToAction("TransferServiceCategory", new { doctorId = model.DoctorId });
                }

                _logger.Information("انتقال صلاحیت‌های خدماتی پزشک {DoctorId} با موفقیت انجام شد", model.DoctorId);
                TempData["Success"] = "انتقال صلاحیت‌های خدماتی با موفقیت انجام شد";
                return RedirectToAction("ServiceCategoryPermissions", new { doctorId = model.DoctorId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتقال صلاحیت‌های خدماتی پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در انجام عملیات انتقال";
                return RedirectToAction("TransferServiceCategory", new { doctorId = model.DoctorId });
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// نمایش جزئیات صلاحیت خدماتی پزشک
        /// </summary>
        [NonAction]
        public async Task<ActionResult> Details(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", doctorId, serviceCategoryId);

                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    TempData["Error"] = "پارامترهای ورودی نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات صلاحیت
                var permissionResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);
                if (!permissionResult.Success)
                {
                    TempData["Error"] = "صلاحیت مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var permission = permissionResult.Data.Items?.FirstOrDefault(p => p.ServiceCategoryId == serviceCategoryId);
                if (permission == null)
                {
                    TempData["Error"] = "صلاحیت مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var viewModel = new DoctorServiceCategoryViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    ServiceCategoryId = serviceCategoryId,
                    ServiceCategoryTitle = permission.ServiceCategoryTitle,
                    DepartmentId = permission.DepartmentId,
                    DepartmentName = permission.DepartmentName,
                    AuthorizationLevel = permission.AuthorizationLevel,
                    IsActive = permission.IsActive,
                    GrantedDate = permission.GrantedDate,
                    ExpiryDate = permission.ExpiryDate,
                    CertificateNumber = permission.CertificateNumber,
                    Notes = permission.Notes,
                    CreatedAt = permission.CreatedAt,
                    CreatedBy = permission.CreatedBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", doctorId, serviceCategoryId);
                TempData["Error"] = "خطا در بارگذاری جزئیات صلاحیت";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش صلاحیت خدماتی پزشک
        /// </summary>
        [NonAction]
        public async Task<ActionResult> Edit(int doctorId, int serviceCategoryId)
        {
            try
            {
                _logger.Information("درخواست ویرایش صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", doctorId, serviceCategoryId);

                if (doctorId <= 0 || serviceCategoryId <= 0)
                {
                    TempData["Error"] = "پارامترهای ورودی نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات پزشک
                var doctorResult = await _doctorCrudService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success || doctorResult.Data == null)
                {
                    TempData["Error"] = "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                // دریافت اطلاعات صلاحیت
                var permissionResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);
                if (!permissionResult.Success)
                {
                    TempData["Error"] = "صلاحیت مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var permission = permissionResult.Data.Items?.FirstOrDefault(p => p.ServiceCategoryId == serviceCategoryId);
                if (permission == null)
                {
                    TempData["Error"] = "صلاحیت مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var viewModel = new DoctorServiceCategoryViewModel
                {
                    DoctorId = doctorId,
                    DoctorName = doctorResult.Data.FullName,
                    ServiceCategoryId = serviceCategoryId,
                    ServiceCategoryTitle = permission.ServiceCategoryTitle,
                    DepartmentId = permission.DepartmentId,
                    DepartmentName = permission.DepartmentName,
                    AuthorizationLevel = permission.AuthorizationLevel,
                    IsActive = permission.IsActive,
                    GrantedDate = permission.GrantedDate,
                    ExpiryDate = permission.ExpiryDate,
                    CertificateNumber = permission.CertificateNumber,
                    Notes = permission.Notes,
                    CreatedAt = permission.CreatedAt,
                    CreatedBy = permission.CreatedBy
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", doctorId, serviceCategoryId);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش ویرایش صلاحیت خدماتی پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DoctorServiceCategoryViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", model.DoctorId, model.ServiceCategoryId);

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "اطلاعات وارد شده نامعتبر است.";
                    return View(model);
                }

                // اعتبارسنجی با FluentValidation
                var validationResult = await _serviceCategoryValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    TempData["Error"] = $"خطا در اعتبارسنجی: {errors}";
                    return View(model);
                }

                // به‌روزرسانی صلاحیت
                var result = await _doctorServiceCategoryService.UpdateDoctorServiceCategoryAsync(model);
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId} با موفقیت ویرایش شد", model.DoctorId, model.ServiceCategoryId);
                TempData["Success"] = "صلاحیت خدماتی با موفقیت ویرایش شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش صلاحیت خدماتی پزشک {DoctorId} در دسته‌بندی {ServiceCategoryId}", model.DoctorId, model.ServiceCategoryId);
                TempData["Error"] = "خطا در ویرایش صلاحیت خدماتی";
                return View(model);
            }
        }

        // Duplicate methods removed - using the improved versions above

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت صلاحیت خدماتی بر اساس AssignmentId
        /// </summary>
        private async Task<DoctorServiceCategoryViewModel> GetServiceCategoryByAssignmentIdAsync(string assignmentId)
        {
            try
            {
                _logger.Information("جستجو برای AssignmentId: {AssignmentId}", assignmentId);
                
                // استخراج DoctorId و ServiceCategoryId از AssignmentId
                var parts = assignmentId?.Split('_');
                if (parts?.Length == 2 && int.TryParse(parts[0], out int doctorId) && int.TryParse(parts[1], out int serviceCategoryId))
                {
                    _logger.Information("AssignmentId پارس شد: DoctorId={DoctorId}, ServiceCategoryId={ServiceCategoryId}", doctorId, serviceCategoryId);
                    
                    // دریافت صلاحیت‌های این پزشک خاص
                    var result = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, 100);
                    
                    if (result?.Data?.Items != null)
                    {
                        _logger.Information("تعداد صلاحیت‌های یافت شده برای پزشک {DoctorId}: {Count}", doctorId, result.Data.Items.Count());
                        
                        var foundItem = result.Data.Items.FirstOrDefault(p => p.DoctorId == doctorId && p.ServiceCategoryId == serviceCategoryId);
                        
                        if (foundItem != null)
                        {
                            _logger.Information("صلاحیت خدماتی یافت شد: AssignmentId={AssignmentId}", foundItem.AssignmentId);
                            return foundItem;
                        }
                        else
                        {
                            _logger.Warning("صلاحیت خدماتی با DoctorId={DoctorId} و ServiceCategoryId={ServiceCategoryId} یافت نشد", doctorId, serviceCategoryId);
                        }
                    }
                    else
                    {
                        _logger.Warning("هیچ صلاحیت خدماتی برای پزشک {DoctorId} یافت نشد", doctorId);
                    }
                }
                else
                {
                    _logger.Error("AssignmentId نامعتبر: {AssignmentId}", assignmentId);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت صلاحیت خدماتی بر اساس AssignmentId: {AssignmentId}", assignmentId);
                return null;
            }
        }

        #endregion

        #region Details and Edit Operations

        /// <summary>
        /// نمایش جزئیات صلاحیت خدماتی پزشک
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات صلاحیت خدماتی {AssignmentId} توسط کاربر {UserId}, IP: {IPAddress}", 
                    assignmentId, _currentUserService.UserId, GetClientIPAddress());

                if (string.IsNullOrEmpty(assignmentId))
                {
                    _logger.Warning("شناسه صلاحیت نامعتبر. AssignmentId: {AssignmentId}, کاربر: {UserId}", 
                        assignmentId, _currentUserService.UserId);
                    TempData["Error"] = "شناسه صلاحیت نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت جزئیات صلاحیت - استفاده از متد کمکی
                var permission = await GetServiceCategoryByAssignmentIdAsync(assignmentId);
                
                if (permission == null)
                {
                    _logger.Error("صلاحیت خدماتی یافت نشد. AssignmentId: {AssignmentId}, پیام: {ErrorMessage}, کاربر: {UserId}", 
                        assignmentId, "صلاحیت خدماتی یافت نشد", _currentUserService.UserId);
                    TempData["Error"] = "صلاحیت خدماتی یافت نشد.";
                    return RedirectToAction("Index");
                }
                _logger.Information("جزئیات صلاحیت خدماتی {AssignmentId} با موفقیت نمایش داده شد. کاربر: {UserId}", 
                    assignmentId, _currentUserService.UserId);

                return View(permission);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در نمایش جزئیات صلاحیت خدماتی {AssignmentId}. کاربر: {UserId}", 
                    assignmentId, _currentUserService.UserId);
                TempData["Error"] = "خطا در بارگذاری جزئیات صلاحیت خدماتی";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش صلاحیت خدماتی پزشک
        /// طبق DESIGN_PRINCIPLES_CONTRACT: لاگ‌گیری جامع برای محیط پزشکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(string assignmentId)
        {
            try
            {
                _logger.Information("درخواست ویرایش صلاحیت خدماتی {AssignmentId} توسط کاربر {UserId}, IP: {IPAddress}", 
                    assignmentId, _currentUserService.UserId, GetClientIPAddress());

                if (string.IsNullOrEmpty(assignmentId))
                {
                    _logger.Warning("شناسه صلاحیت نامعتبر. AssignmentId: {AssignmentId}, کاربر: {UserId}", 
                        assignmentId, _currentUserService.UserId);
                    TempData["Error"] = "شناسه صلاحیت نامعتبر است.";
                    return RedirectToAction("Index");
                }

                // دریافت صلاحیت برای ویرایش - استفاده از متد کمکی
                var permission = await GetServiceCategoryByAssignmentIdAsync(assignmentId);
                
                if (permission == null)
                {
                    _logger.Error("صلاحیت خدماتی برای ویرایش یافت نشد. AssignmentId: {AssignmentId}, پیام: {ErrorMessage}, کاربر: {UserId}", 
                        assignmentId, "صلاحیت خدماتی یافت نشد", _currentUserService.UserId);
                    TempData["Error"] = "صلاحیت خدماتی یافت نشد.";
                    return RedirectToAction("Index");
                }

                _logger.Information("فرم ویرایش صلاحیت خدماتی {AssignmentId} با موفقیت نمایش داده شد. کاربر: {UserId}", 
                    assignmentId, _currentUserService.UserId);

                return View(permission);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطای غیرمنتظره در نمایش فرم ویرایش صلاحیت خدماتی {AssignmentId}. کاربر: {UserId}", 
                    assignmentId, _currentUserService.UserId);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش صلاحیت خدماتی";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت IP آدرس کلاینت برای لاگ‌گیری و امنیت
        /// طبق DESIGN_PRINCIPLES_CONTRACT: ردیابی کامل برای محیط پزشکی
        /// </summary>
        private string GetClientIPAddress()
        {
            try
            {
                var forwarded = Request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwarded))
                {
                    return forwarded.Split(',')[0].Trim();
                }
                return Request.UserHostAddress ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در دریافت IP آدرس کلاینت");
                return "Unknown";
            }
        }

        #endregion
    }
}
