using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;
using ClinicApp.Services;
using ClinicApp.ViewModels;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using Serilog;
using System.Data.Entity;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت اجزای خدمات - محیط درمانی با اطمینان 100%
    /// Service Component Management Controller for Medical Environment with 100% Reliability
    /// </summary>
    //[Authorize]
    public class ServiceComponentController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly IServiceService _serviceService;

        public ServiceComponentController(
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService,
            IServiceCalculationService serviceCalculationService,
            IServiceService serviceService)
            : base(messageNotificationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
        }

        #region Index and List Operations

        /// <summary>
        /// نمایش لیست اجزای خدمات
        /// </summary>
        public async Task<ActionResult> Index(int? page, int? pageSize, string searchTerm, int? serviceId, ServiceComponentType? componentType, bool? isActive)
        {
            try
            {
                var currentPage = page ?? 1;
                var currentPageSize = Math.Min(pageSize ?? 20, 100);

                _logger.Information("🏥 MEDICAL: دریافت لیست اجزای خدمات - صفحه: {Page}, اندازه: {PageSize}, جستجو: {SearchTerm}", 
                    currentPage, currentPageSize, searchTerm);

                // بررسی اولیه تعداد کل ServiceComponents
                var totalServiceComponents = await _context.ServiceComponents.CountAsync();
                var activeServiceComponents = await _context.ServiceComponents.Where(sc => !sc.IsDeleted).CountAsync();
                var activeServiceComponentsWithService = await _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Where(sc => !sc.IsDeleted && sc.Service != null)
                    .CountAsync();

                _logger.Information("🏥 MEDICAL: آمار ServiceComponents - کل: {Total}, فعال: {Active}, با Service: {WithService}", 
                    totalServiceComponents, activeServiceComponents, activeServiceComponentsWithService);

                var query = _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Where(sc => !sc.IsDeleted);

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(sc => sc.Service.Title.Contains(searchTerm) || 
                                            sc.Description.Contains(searchTerm));
                }

                if (serviceId.HasValue)
                {
                    query = query.Where(sc => sc.ServiceId == serviceId.Value);
                }

                if (componentType.HasValue)
                {
                    query = query.Where(sc => sc.ComponentType == componentType.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(sc => sc.IsActive == isActive.Value);
                }

                var totalCount = await query.CountAsync();
                _logger.Information("🏥 MEDICAL: تعداد کل نتایج بعد از فیلتر: {TotalCount}", totalCount);

                var serviceComponents = await query
                    .OrderBy(sc => sc.ServiceComponentId)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                _logger.Information("🏥 MEDICAL: تعداد نتایج بارگذاری شده: {LoadedCount}", serviceComponents.Count);

                // بررسی ServiceComponents بارگذاری شده
                foreach (var sc in serviceComponents.Take(3))
                {
                    _logger.Information("🏥 MEDICAL: ServiceComponent - ID: {Id}, ServiceId: {ServiceId}, Service: {ServiceTitle}, ComponentType: {ComponentType}", 
                        sc.ServiceComponentId, sc.ServiceId, sc.Service?.Title ?? "NULL", sc.ComponentType);
                }

                var viewModel = serviceComponents.Select(sc => new ServiceComponentViewModel
                {
                    ServiceComponentId = sc.ServiceComponentId,
                    ServiceId = sc.ServiceId,
                    ServiceTitle = sc.Service?.Title ?? "نامشخص",
                    ServiceCode = sc.Service?.ServiceCode ?? "نامشخص",
                    ComponentType = sc.ComponentType,
                    ComponentTypeName = sc.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                    Coefficient = sc.Coefficient,
                    Description = sc.Description,
                    IsActive = sc.IsActive,
                    CreatedAt = sc.CreatedAt,
                    CreatedBy = sc.CreatedByUserId,
                    UpdatedAt = sc.UpdatedAt,
                    UpdatedBy = sc.UpdatedByUserId
                }).ToList();

                // ایجاد PagedResult
                var pagedResult = new PagedResult<ServiceComponentViewModel>(viewModel, totalCount, currentPage, currentPageSize);

                // اطلاعات Pagination
                ViewBag.CurrentPage = currentPage;
                ViewBag.PageSize = currentPageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.ServiceId = serviceId;
                ViewBag.ComponentType = componentType;
                ViewBag.IsActive = isActive;

                // لیست خدمات برای فیلتر
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}"
                }).ToList(), "Value", "Text");

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست اجزای خدمات");
                TempData["ErrorMessage"] = "خطا در دریافت لیست اجزای خدمات";
                
                // ایجاد PagedResult خالی
                var emptyPagedResult = new PagedResult<ServiceComponentViewModel>(new List<ServiceComponentViewModel>(), 0, 1, 20);
                
                return View(emptyPagedResult);
            }
        }

        #endregion

        #region Debug Operations

        /// <summary>
        /// متد Debug برای بررسی وضعیت ServiceComponents - محیط درمانی
        /// </summary>
        public async Task<ActionResult> DebugServiceComponents()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: شروع Debug ServiceComponents");

                // بررسی اولیه
                var totalServiceComponents = await _context.ServiceComponents.CountAsync();
                var activeServiceComponents = await _context.ServiceComponents.Where(sc => !sc.IsDeleted).CountAsync();
                var activeAndActiveServiceComponents = await _context.ServiceComponents
                    .Where(sc => !sc.IsDeleted && sc.IsActive)
                    .CountAsync();

                // بررسی Services
                var totalServices = await _context.Services.CountAsync();
                var activeServices = await _context.Services.Where(s => !s.IsDeleted && s.IsActive).CountAsync();

                // بررسی ServiceComponents با Service
                var serviceComponentsWithService = await _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Where(sc => !sc.IsDeleted && sc.Service != null)
                    .CountAsync();

                // نمونه‌ای از ServiceComponents
                var sampleServiceComponents = await _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Where(sc => !sc.IsDeleted)
                    .Take(5)
                    .Select(sc => new
                    {
                        sc.ServiceComponentId,
                        sc.ServiceId,
                        sc.ComponentType,
                        sc.Coefficient,
                        sc.IsActive,
                        sc.IsDeleted,
                        ServiceTitle = sc.Service != null ? sc.Service.Title : "NULL",
                        ServiceCode = sc.Service != null ? sc.Service.ServiceCode : "NULL"
                    })
                    .ToListAsync();

                var debugInfo = new
                {
                    ServiceComponents = new
                    {
                        Total = totalServiceComponents,
                        Active = activeServiceComponents,
                        ActiveAndActive = activeAndActiveServiceComponents,
                        WithService = serviceComponentsWithService
                    },
                    Services = new
                    {
                        Total = totalServices,
                        Active = activeServices
                    },
                    SampleServiceComponents = sampleServiceComponents,
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                _logger.Information("🏥 MEDICAL: Debug ServiceComponents تکمیل شد");

                return Json(new
                {
                    success = true,
                    message = "Debug ServiceComponents با موفقیت انجام شد",
                    data = debugInfo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در Debug ServiceComponents");
                return Json(new
                {
                    success = false,
                    message = $"خطا: {ex.Message}",
                    details = ex.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Details Operations

        /// <summary>
        /// نمایش جزئیات جزء خدمت
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var serviceComponent = await _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Include(sc => sc.CreatedByUser)
                    .Include(sc => sc.UpdatedByUser)
                    .FirstOrDefaultAsync(sc => sc.ServiceComponentId == id && !sc.IsDeleted);

                if (serviceComponent == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new ServiceComponentDetailsViewModel
                {
                    ServiceComponentId = serviceComponent.ServiceComponentId,
                    ServiceId = serviceComponent.ServiceId,
                    ServiceTitle = serviceComponent.Service.Title,
                    ServiceCode = serviceComponent.Service.ServiceCode,
                    ComponentType = serviceComponent.ComponentType,
                    ComponentTypeName = serviceComponent.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                    Coefficient = serviceComponent.Coefficient,
                    Description = serviceComponent.Description,
                    IsActive = serviceComponent.IsActive,
                    CreatedAt = serviceComponent.CreatedAt,
                    CreatedBy = serviceComponent.CreatedByUserId,
                    CreatedByName = serviceComponent.CreatedByUser?.UserName ?? "سیستم",
                    UpdatedAt = serviceComponent.UpdatedAt,
                    UpdatedBy = serviceComponent.UpdatedByUserId,
                    UpdatedByName = serviceComponent.UpdatedByUser?.UserName ?? "سیستم"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات جزء خدمت {ServiceComponentId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات جزء خدمت";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// نمایش فرم ایجاد جزء خدمت
        /// </summary>
        public async Task<ActionResult> Create(int? serviceId)
        {
            try
            {
                var viewModel = new ServiceComponentCreateEditViewModel
                {
                    ServiceId = serviceId ?? 0,
                    ComponentType = ServiceComponentType.Technical,
                    IsActive = true
                };

                // لیست خدمات
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}",
                    Selected = s.ServiceId == serviceId
                }).ToList(), "Value", "Text", serviceId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد جزء خدمت");
                TempData["ErrorMessage"] = "خطا در نمایش فرم ایجاد جزء خدمت";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد جزء خدمت جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ServiceComponentCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // بارگذاری مجدد لیست خدمات
                    var services = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                        .ToListAsync();

                    ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = $"{s.ServiceCode} - {s.Title}",
                        Selected = s.ServiceId == model.ServiceId
                    }).ToList(), "Value", "Text", model.ServiceId);

                    return View(model);
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateServiceComponentAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    var services = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                        .ToListAsync();

                    ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = $"{s.ServiceCode} - {s.Title}",
                        Selected = s.ServiceId == model.ServiceId
                    }).ToList(), "Value", "Text", model.ServiceId);

                    return View(model);
                }

                var serviceComponent = new ServiceComponent
                {
                    ServiceId = model.ServiceId,
                    ComponentType = model.ComponentType,
                    Coefficient = model.Coefficient,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.GetCurrentUserId()
                };

                _context.ServiceComponents.Add(serviceComponent);
                await _context.SaveChangesAsync();

                _logger.Information("جزء خدمت جدید ایجاد شد - شناسه: {ServiceComponentId}, خدمت: {ServiceId}, نوع: {ComponentType}", 
                    serviceComponent.ServiceComponentId, model.ServiceId, model.ComponentType);

                // به‌روزرسانی قیمت خدمت بر اساس ServiceComponents جدید
                try
                {
                    var updateResult = await _serviceService.UpdateServicePriceAsync(model.ServiceId);
                    if (updateResult.Success)
                    {
                        _logger.Information("قیمت خدمت بعد از ایجاد ServiceComponent به‌روزرسانی شد - ServiceId: {ServiceId}, NewPrice: {NewPrice}", 
                            model.ServiceId, updateResult.Data);
                    }
                    else
                    {
                        _logger.Warning("خطا در به‌روزرسانی قیمت خدمت بعد از ایجاد ServiceComponent - ServiceId: {ServiceId}, Error: {Error}", 
                            model.ServiceId, updateResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "خطا در به‌روزرسانی قیمت خدمت بعد از ایجاد ServiceComponent - ServiceId: {ServiceId}", model.ServiceId);
                }

                TempData["SuccessMessage"] = "جزء خدمت با موفقیت ایجاد شد";
                return RedirectToAction("Details", new { id = serviceComponent.ServiceComponentId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد جزء خدمت");
                TempData["ErrorMessage"] = "خطا در ایجاد جزء خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Edit Operations

        /// <summary>
        /// نمایش فرم ویرایش جزء خدمت
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var serviceComponent = await _context.ServiceComponents
                    .Include(sc => sc.Service)
                    .Include(sc => sc.Service.ServiceCategory)
                    .Include(sc => sc.Service.ServiceCategory.Department)
                    .Include(sc => sc.Service.ServiceCategory.Department.Clinic)
                    .FirstOrDefaultAsync(sc => sc.ServiceComponentId == id && !sc.IsDeleted);

                if (serviceComponent == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new ServiceComponentCreateEditViewModel
                {
                    ServiceComponentId = serviceComponent.ServiceComponentId,
                    ServiceId = serviceComponent.ServiceId,
                    ComponentType = serviceComponent.ComponentType,
                    Coefficient = serviceComponent.Coefficient,
                    Description = serviceComponent.Description,
                    IsActive = serviceComponent.IsActive
                };

                // لیست خدمات
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}",
                    Selected = s.ServiceId == serviceComponent.ServiceId
                }).ToList(), "Value", "Text", serviceComponent.ServiceId);

                // اطلاعات سلسله‌مراتبی برای Navigation
                if (serviceComponent.Service != null)
                {
                    ViewBag.ServiceTitle = serviceComponent.Service.Title;
                    ViewBag.ServiceCode = serviceComponent.Service.ServiceCode;
                    ViewBag.ServiceId = serviceComponent.Service.ServiceId;

                    if (serviceComponent.Service.ServiceCategory != null)
                    {
                        ViewBag.ServiceCategoryId = serviceComponent.Service.ServiceCategory.ServiceCategoryId;
                        ViewBag.ServiceCategoryName = serviceComponent.Service.ServiceCategory.Title;

                        if (serviceComponent.Service.ServiceCategory.Department != null)
                        {
                            ViewBag.DepartmentId = serviceComponent.Service.ServiceCategory.Department.DepartmentId;
                            ViewBag.DepartmentName = serviceComponent.Service.ServiceCategory.Department.Name;

                            if (serviceComponent.Service.ServiceCategory.Department.Clinic != null)
                            {
                                ViewBag.ClinicId = serviceComponent.Service.ServiceCategory.Department.Clinic.ClinicId;
                                ViewBag.ClinicName = serviceComponent.Service.ServiceCategory.Department.Clinic.Name;
                            }
                        }
                    }
                }

                // نام نوع جزء برای نمایش
                ViewBag.ComponentTypeName = serviceComponent.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش جزء خدمت {ServiceComponentId}", id);
                TempData["ErrorMessage"] = "خطا در نمایش فرم ویرایش جزء خدمت";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش جزء خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ServiceComponentCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var services = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                        .ToListAsync();

                    ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = $"{s.ServiceCode} - {s.Title}",
                        Selected = s.ServiceId == model.ServiceId
                    }).ToList(), "Value", "Text", model.ServiceId);

                    return View(model);
                }

                var serviceComponent = await _context.ServiceComponents
                    .FirstOrDefaultAsync(sc => sc.ServiceComponentId == model.ServiceComponentId && !sc.IsDeleted);

                if (serviceComponent == null)
                {
                    return HttpNotFound();
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateServiceComponentAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    var services = await _context.Services
                        .Where(s => !s.IsDeleted && s.IsActive)
                        .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                        .ToListAsync();

                    ViewBag.Services = new SelectList(services.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = $"{s.ServiceCode} - {s.Title}",
                        Selected = s.ServiceId == model.ServiceId
                    }).ToList(), "Value", "Text", model.ServiceId);

                    return View(model);
                }

                // به‌روزرسانی فیلدها
                serviceComponent.ServiceId = model.ServiceId;
                serviceComponent.ComponentType = model.ComponentType;
                serviceComponent.Coefficient = model.Coefficient;
                serviceComponent.Description = model.Description;
                serviceComponent.IsActive = model.IsActive;
                serviceComponent.UpdatedAt = DateTime.UtcNow;
                serviceComponent.UpdatedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("جزء خدمت ویرایش شد - شناسه: {ServiceComponentId}, خدمت: {ServiceId}, نوع: {ComponentType}", 
                    model.ServiceComponentId, model.ServiceId, model.ComponentType);

                // به‌روزرسانی قیمت خدمت بر اساس ServiceComponents ویرایش شده
                try
                {
                    var updateResult = await _serviceService.UpdateServicePriceAsync(model.ServiceId);
                    if (updateResult.Success)
                    {
                        _logger.Information("قیمت خدمت بعد از ویرایش ServiceComponent به‌روزرسانی شد - ServiceId: {ServiceId}, NewPrice: {NewPrice}", 
                            model.ServiceId, updateResult.Data);
                    }
                    else
                    {
                        _logger.Warning("خطا در به‌روزرسانی قیمت خدمت بعد از ویرایش ServiceComponent - ServiceId: {ServiceId}, Error: {Error}", 
                            model.ServiceId, updateResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "خطا در به‌روزرسانی قیمت خدمت بعد از ویرایش ServiceComponent - ServiceId: {ServiceId}", model.ServiceId);
                }

                TempData["SuccessMessage"] = "جزء خدمت با موفقیت ویرایش شد";
                return RedirectToAction("Details", new { id = serviceComponent.ServiceComponentId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش جزء خدمت {ServiceComponentId}", model.ServiceComponentId);
                TempData["ErrorMessage"] = "خطا در ویرایش جزء خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// حذف نرم جزء خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var serviceComponent = await _context.ServiceComponents
                    .FirstOrDefaultAsync(sc => sc.ServiceComponentId == id && !sc.IsDeleted);

                if (serviceComponent == null)
                {
                    return HttpNotFound();
                }

                // حذف نرم
                serviceComponent.IsDeleted = true;
                serviceComponent.DeletedAt = DateTime.UtcNow;
                serviceComponent.DeletedByUserId = _currentUserService.GetCurrentUserId();

                await _context.SaveChangesAsync();

                _logger.Information("جزء خدمت حذف شد - شناسه: {ServiceComponentId}", id);

                // به‌روزرسانی قیمت خدمت بر اساس ServiceComponents باقی‌مانده
                try
                {
                    var updateResult = await _serviceService.UpdateServicePriceAsync(serviceComponent.ServiceId);
                    if (updateResult.Success)
                    {
                        _logger.Information("قیمت خدمت بعد از حذف ServiceComponent به‌روزرسانی شد - ServiceId: {ServiceId}, NewPrice: {NewPrice}", 
                            serviceComponent.ServiceId, updateResult.Data);
                    }
                    else
                    {
                        _logger.Warning("خطا در به‌روزرسانی قیمت خدمت بعد از حذف ServiceComponent - ServiceId: {ServiceId}, Error: {Error}", 
                            serviceComponent.ServiceId, updateResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "خطا در به‌روزرسانی قیمت خدمت بعد از حذف ServiceComponent - ServiceId: {ServiceId}", serviceComponent.ServiceId);
                }

                TempData["SuccessMessage"] = "جزء خدمت با موفقیت حذف شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف جزء خدمت {ServiceComponentId}", id);
                TempData["ErrorMessage"] = "خطا در حذف جزء خدمت: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// دریافت اجزای یک خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceComponents(int serviceId)
        {
            try
            {
                var components = await _context.ServiceComponents
                    .Where(sc => sc.ServiceId == serviceId && !sc.IsDeleted && sc.IsActive)
                    .Select(sc => new
                    {
                        sc.ServiceComponentId,
                        sc.ComponentType,
                        ComponentTypeName = sc.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                        sc.Coefficient,
                        sc.Description
                    })
                    .ToListAsync();

                return Json(components, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اجزای خدمت {ServiceId}", serviceId);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه قیمت خدمت بر اساس اجزای آن
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateServicePrice(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                    service, _context, calculationDate ?? DateTime.Now);

                var result = new
                {
                    success = true,
                    serviceId = service.ServiceId,
                    serviceTitle = service.Title,
                    calculatedPrice = calculatedPrice,
                    calculationDate = calculationDate ?? DateTime.Now,
                    components = service.ServiceComponents
                        .Where(sc => !sc.IsDeleted && sc.IsActive)
                        .Select(sc => new
                        {
                            sc.ComponentType,
                            ComponentTypeName = sc.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                            sc.Coefficient
                        })
                        .ToList()
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در محاسبه قیمت خدمت {ServiceId}", serviceId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی Business Rules برای جزء خدمت
        /// </summary>
        private async Task<ServiceComponentValidationResult> ValidateServiceComponentAsync(ServiceComponentCreateEditViewModel model)
        {
            var result = new ServiceComponentValidationResult { IsValid = true, Errors = new Dictionary<string, string>() };

            try
            {
                // بررسی وجود خدمت
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId && !s.IsDeleted);

                if (service == null)
                {
                    result.IsValid = false;
                    result.Errors["ServiceId"] = "خدمت انتخاب شده یافت نشد";
                }

                // بررسی وجود جزء مشابه
                var existingComponent = await _context.ServiceComponents
                    .FirstOrDefaultAsync(sc => sc.ServiceId == model.ServiceId && 
                                             sc.ComponentType == model.ComponentType && 
                                             !sc.IsDeleted &&
                                             (model.ServiceComponentId == 0 || sc.ServiceComponentId != model.ServiceComponentId));

                if (existingComponent != null)
                {
                    result.IsValid = false;
                    result.Errors["ComponentType"] = $"جزء {model.ComponentType} برای این خدمت از قبل موجود است";
                }

                // اعتبارسنجی مقدار ضریب
                if (model.Coefficient < 0.01m || model.Coefficient > 999999.99m)
                {
                    result.IsValid = false;
                    result.Errors["Coefficient"] = "ضریب باید بین 0.01 تا 999999.99 باشد";
                }

                // اعتبارسنجی توضیحات
                if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500)
                {
                    result.IsValid = false;
                    result.Errors["Description"] = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی جزء خدمت");
                result.IsValid = false;
                result.Errors["General"] = "خطا در اعتبارسنجی";
                return result;
            }
        }

        #endregion
    }

    #region Helper Classes

    /// <summary>
    /// نتیجه اعتبارسنجی برای اجزای خدمات
    /// </summary>
    public class ServiceComponentValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }

    #endregion
}
