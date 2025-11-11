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
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using Serilog;
using System.Data.Entity;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت خدمات مشترک - محیط درمانی با اطمینان 100%
    /// Shared Service Management Controller for Medical Environment with 100% Reliability
    /// 
    /// اصول طراحی:
    /// ✅ Single Responsibility: فقط مدیریت خدمات مشترک
    /// ✅ Clean Architecture: جداسازی کامل concerns
    /// ✅ Medical Environment: فیلترها و اعتبارسنجی‌های مخصوص محیط درمانی
    /// ✅ Error Handling: مدیریت کامل خطاها
    /// ✅ Logging: ثبت کامل عملیات
    /// </summary>
    //[Authorize] // امنیت: فقط کاربران احراز هویت شده
    public class SharedServiceController : BaseController
    {
        #region Dependencies and Constructor

        private readonly ISharedServiceManagementService _sharedServiceManagementService;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public SharedServiceController(
            ISharedServiceManagementService sharedServiceManagementService,
            IServiceCalculationService serviceCalculationService,
            ApplicationDbContext context,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _sharedServiceManagementService = sharedServiceManagementService ?? throw new ArgumentNullException(nameof(sharedServiceManagementService));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #endregion

        #region Index and List Operations

        /// <summary>
        /// نمایش لیست خدمات مشترک
        /// </summary>
        public async Task<ActionResult> Index(int? page, int? pageSize, string searchTerm, int? departmentId, int? serviceId, bool? isActive)
        {
            try
            {
                var currentPage = page ?? 1;
                var currentPageSize = Math.Min(pageSize ?? 20, 100);

                _logger.Information("🏥 MEDICAL: دریافت لیست خدمات مشترک - صفحه: {Page}, اندازه: {PageSize}, جستجو: {SearchTerm}, دپارتمان: {DepartmentId}, خدمت: {ServiceId}, فعال: {IsActive}, کاربر: {UserName} (Id: {UserId})",
                    currentPage, currentPageSize, searchTerm, departmentId, serviceId, isActive, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت خدمات مشترک با فیلتر
                var query = _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .Include(ss => ss.CreatedByUser)
                    .Include(ss => ss.UpdatedByUser)
                    .Where(ss => !ss.IsDeleted);

                // اعمال فیلترها
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(ss => 
                        ss.Service.Title.Contains(searchTerm) ||
                        ss.Service.ServiceCode.Contains(searchTerm) ||
                        ss.Department.Name.Contains(searchTerm) ||
                        ss.DepartmentSpecificNotes.Contains(searchTerm));
                }

                if (departmentId.HasValue)
                {
                    query = query.Where(ss => ss.DepartmentId == departmentId.Value);
                }

                if (serviceId.HasValue)
                {
                    query = query.Where(ss => ss.ServiceId == serviceId.Value);
                }

                if (isActive.HasValue)
                {
                    query = query.Where(ss => ss.IsActive == isActive.Value);
                }

                // شمارش کل رکوردها
                var totalCount = await query.CountAsync();

                // دریافت رکوردهای صفحه فعلی
                var sharedServices = await query
                    .OrderBy(ss => ss.SharedServiceId)
                    .Skip((currentPage - 1) * currentPageSize)
                    .Take(currentPageSize)
                    .ToListAsync();

                // تبدیل به ViewModel
                var viewModel = sharedServices.Select(SharedServiceIndexViewModel.FromEntity).ToList();

                // ایجاد PagedResult
                var pagedResult = new PagedResult<SharedServiceIndexViewModel>(viewModel, totalCount, currentPage, currentPageSize);

                // ایجاد Filter ViewModel
                var filter = new SharedServiceFilterViewModel
                {
                    SearchTerm = searchTerm,
                    DepartmentId = departmentId,
                    ServiceId = serviceId,
                    IsActive = isActive,
                    PageSize = currentPageSize,
                    PageNumber = currentPage
                };

                // تنظیم فیلترها
                await SetupFilterOptions(filter);

                // محاسبه آمار کلی
                var statistics = await CalculateStatistics();

                // ایجاد Page ViewModel
                var pageViewModel = new SharedServiceIndexPageViewModel(pagedResult, filter, statistics);

                _logger.Information("🏥 MEDICAL: دریافت لیست خدمات مشترک موفق - تعداد: {Count}, صفحه: {Page}, کاربر: {UserName} (Id: {UserId})",
                    totalCount, currentPage, _currentUserService.UserName, _currentUserService.UserId);

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست خدمات مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در دریافت لیست خدمات مشترک";

                return View(new SharedServiceIndexPageViewModel());
            }
        }

        /// <summary>
        /// نمایش جزئیات خدمت مشترک
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: دریافت جزئیات خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .Include(ss => ss.CreatedByUser)
                    .Include(ss => ss.UpdatedByUser)
                    .Include(ss => ss.DeletedByUser)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == id && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = "خدمت مشترک مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var viewModel = SharedServiceDetailsViewModel.FromEntity(sharedService);

                _logger.Information("🏥 MEDICAL: دریافت جزئیات خدمت مشترک موفق - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت جزئیات خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات خدمت مشترک";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create Operations

        /// <summary>
        /// نمایش فرم ایجاد خدمت مشترک جدید
        /// </summary>
        public async Task<ActionResult> Create()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: نمایش فرم ایجاد خدمت مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var viewModel = new SharedServiceCreateEditViewModel();
                await SetupCreateEditViewBags(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ایجاد خدمت مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در نمایش فرم ایجاد خدمت مشترک";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد خدمت مشترک جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SharedServiceCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: پردازش ایجاد خدمت مشترک - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل نامعتبر برای ایجاد خدمت مشترک - کاربر: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    await SetupCreateEditViewBags(model);
                    return View(model);
                }

                // بررسی تکراری نبودن
                var isDuplicate = _sharedServiceManagementService.IsServiceInDepartment(model.ServiceId, model.DepartmentId);
                if (isDuplicate)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک تکراری - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                        model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);
                    ModelState.AddModelError("", "این خدمت قبلاً در این دپارتمان تعریف شده است");
                    await SetupCreateEditViewBags(model);
                    return View(model);
                }

                // ایجاد خدمت مشترک
                var result = await _sharedServiceManagementService.AddServiceToDepartment(
                    model.ServiceId, 
                    model.DepartmentId, 
                    _currentUserService.UserId);

                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: ایجاد خدمت مشترک موفق - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                        model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["SuccessMessage"] = "خدمت مشترک با موفقیت ایجاد شد";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: ایجاد خدمت مشترک ناموفق - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, خطا: {Error}, کاربر: {UserName} (Id: {UserId})",
                        model.ServiceId, model.DepartmentId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    ModelState.AddModelError("", result.Message);
                    await SetupCreateEditViewBags(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در ایجاد خدمت مشترک - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در ایجاد خدمت مشترک";
                await SetupCreateEditViewBags(model);
                return View(model);
            }
        }

        #endregion

        #region Edit Operations

        /// <summary>
        /// نمایش فرم ویرایش خدمت مشترک
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: نمایش فرم ویرایش خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .Include(ss => ss.CreatedByUser)
                    .Include(ss => ss.UpdatedByUser)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == id && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد برای ویرایش - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = "خدمت مشترک مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var viewModel = SharedServiceCreateEditViewModel.FromEntity(sharedService);
                await SetupCreateEditViewBags(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ویرایش خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در نمایش فرم ویرایش خدمت مشترک";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ویرایش خدمت مشترک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SharedServiceCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: پردازش ویرایش خدمت مشترک - Id: {Id}, ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    model.SharedServiceId, model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    _logger.Warning("🏥 MEDICAL: مدل نامعتبر برای ویرایش خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        model.SharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    await SetupCreateEditViewBags(model);
                    return View(model);
                }

                // بررسی تکراری نبودن (به جز خود رکورد)
                var isDuplicate = _sharedServiceManagementService.IsServiceInDepartment(model.ServiceId, model.DepartmentId);
                if (isDuplicate)
                {
                    // بررسی اینکه آیا این تکراری مربوط به خود رکورد است یا نه
                    var existingSharedService = await _context.SharedServices
                        .FirstOrDefaultAsync(ss => ss.ServiceId == model.ServiceId && 
                                                  ss.DepartmentId == model.DepartmentId && 
                                                  ss.SharedServiceId != model.SharedServiceId && 
                                                  !ss.IsDeleted);
                    
                    if (existingSharedService != null)
                    {
                        _logger.Warning("🏥 MEDICAL: خدمت مشترک تکراری در ویرایش - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                            model.ServiceId, model.DepartmentId, _currentUserService.UserName, _currentUserService.UserId);
                        ModelState.AddModelError("", "این خدمت قبلاً در این دپارتمان تعریف شده است");
                        await SetupCreateEditViewBags(model);
                        return View(model);
                    }
                }

                // به‌روزرسانی خدمت مشترک
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == model.SharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد برای به‌روزرسانی - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        model.SharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = "خدمت مشترک مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                // به‌روزرسانی فیلدها
                sharedService.ServiceId = model.ServiceId;
                sharedService.DepartmentId = model.DepartmentId;
                sharedService.IsActive = model.IsActive;
                sharedService.DepartmentSpecificNotes = model.DepartmentSpecificNotes;
                sharedService.UpdatedAt = DateTime.Now;
                sharedService.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _logger.Information("🏥 MEDICAL: ویرایش خدمت مشترک موفق - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    model.SharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                TempData["SuccessMessage"] = "خدمت مشترک با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در ویرایش خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    model.SharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در ویرایش خدمت مشترک";
                await SetupCreateEditViewBags(model);
                return View(model);
            }
        }

        #endregion

        #region Delete Operations

        /// <summary>
        /// نمایش فرم تأیید حذف خدمت مشترک
        /// </summary>
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: نمایش فرم حذف خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == id && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد برای حذف - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = "خدمت مشترک مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var viewModel = SharedServiceDetailsViewModel.FromEntity(sharedService);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم حذف خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در نمایش فرم حذف خدمت مشترک";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش حذف خدمت مشترک
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: پردازش حذف خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // ابتدا باید serviceId و departmentId را از sharedServiceId پیدا کنم
                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == id && !ss.IsDeleted);

                if (sharedService == null)
                {
                    TempData["ErrorMessage"] = "خدمت مشترک مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                var result = await _sharedServiceManagementService.RemoveServiceFromDepartment(
                    sharedService.ServiceId, sharedService.DepartmentId, _currentUserService.UserId);

                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: حذف خدمت مشترک موفق - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["SuccessMessage"] = "خدمت مشترک با موفقیت حذف شد";
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: حذف خدمت مشترک ناموفق - Id: {Id}, خطا: {Error}, کاربر: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در حذف خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                TempData["ErrorMessage"] = "خطا در حذف خدمت مشترک";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Operations

        /// <summary>
        /// فعال/غیرفعال کردن خدمت مشترک
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ToggleActive(int id, bool isActive)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: تغییر وضعیت فعال خدمت مشترک - Id: {Id}, فعال: {IsActive}, کاربر: {UserName} (Id: {UserId})",
                    id, isActive, _currentUserService.UserName, _currentUserService.UserId);

                var sharedService = await _context.SharedServices
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == id && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد برای تغییر وضعیت - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "خدمت مشترک یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                sharedService.IsActive = isActive;
                sharedService.UpdatedAt = DateTime.Now;
                sharedService.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _logger.Information("🏥 MEDICAL: تغییر وضعیت فعال خدمت مشترک موفق - Id: {Id}, فعال: {IsActive}, کاربر: {UserName} (Id: {UserId})",
                    id, isActive, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, message = $"خدمت مشترک با موفقیت {(isActive ? "فعال" : "غیرفعال")} شد" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در تغییر وضعیت فعال خدمت مشترک - Id: {Id}, کاربر: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در تغییر وضعیت خدمت مشترک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار خدمات مشترک
        /// </summary>
        [HttpGet]
        public JsonResult GetStatistics()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: دریافت آمار خدمات مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var statisticsResult = _sharedServiceManagementService.GetSharedServiceStatistics();
                
                if (!statisticsResult.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت آمار خدمات مشترک - {Message}, کاربر: {UserName} (Id: {UserId})",
                        statisticsResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = statisticsResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var statistics = statisticsResult.Data;

                _logger.Information("🏥 MEDICAL: دریافت آمار خدمات مشترک موفق - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = statistics }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار خدمات مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت آمار خدمات مشترک" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه قیمت خدمت مشترک (برای Create)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateSharedServicePriceForCreate(int serviceId, int departmentId, decimal? overrideTechnicalFactor = null, decimal? overrideProfessionalFactor = null)
        {
            try
            {
                Console.WriteLine($"🚀 [CONTROLLER] شروع محاسبه قیمت خدمت مشترک (Create)");
                Console.WriteLine($"🚀 [CONTROLLER] ServiceId: {serviceId}, DepartmentId: {departmentId}");
                Console.WriteLine($"🚀 [CONTROLLER] Override Technical: {overrideTechnicalFactor}, Override Professional: {overrideProfessionalFactor}");
                Console.WriteLine($"🚀 [CONTROLLER] User: {_currentUserService.UserName} (Id: {_currentUserService.UserId})");

                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت مشترک (Create) - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, departmentId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از سرویس یکپارچه محاسبه
                var calculationResult = await _serviceCalculationService.CalculateSharedServicePriceAsync(
                    serviceId, departmentId, _context, overrideTechnicalFactor, overrideProfessionalFactor);

                Console.WriteLine($"📊 [CONTROLLER] نتیجه محاسبه: Success = {calculationResult.Success}");

                if (!calculationResult.Success)
                {
                    Console.WriteLine($"❌ [CONTROLLER] خطا در محاسبه: {calculationResult.Message}");
                    _logger.Warning("🏥 MEDICAL: خطا در محاسبه قیمت - ServiceId: {ServiceId}, Message: {Message}, کاربر: {UserName} (Id: {UserId})",
                        serviceId, calculationResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = calculationResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    serviceId = calculationResult.Details.ServiceId,
                    serviceTitle = calculationResult.Details.ServiceTitle,
                    serviceCode = calculationResult.Details.ServiceCode,
                    departmentId = departmentId,
                    calculatedPrice = calculationResult.CalculatedPrice,
                    technicalComponent = calculationResult.Details.TechnicalAmount,  // ✅ مبلغ فنی
                    professionalComponent = calculationResult.Details.ProfessionalAmount,  // ✅ مبلغ حرفه‌ای
                    technicalCoefficient = calculationResult.Details.TechnicalPart,  // ضریب فنی
                    professionalCoefficient = calculationResult.Details.ProfessionalPart,  // ضریب حرفه‌ای
                    technicalFactor = calculationResult.Details.TechnicalFactor,
                    professionalFactor = calculationResult.Details.ProfessionalFactor,
                    calculationFormula = calculationResult.CalculationFormula,
                    hasOverride = calculationResult.HasOverride,
                    financialYear = calculationResult.FinancialYear
                };

                Console.WriteLine($"✅ [CONTROLLER] محاسبه موفق:");
                Console.WriteLine($"   📊 ServiceId: {result.serviceId}");
                Console.WriteLine($"   📊 ServiceTitle: {result.serviceTitle}");
                Console.WriteLine($"   📊 CalculatedPrice: {result.calculatedPrice:N0}");
                Console.WriteLine($"   📊 Technical Component: {result.technicalComponent}");
                Console.WriteLine($"   📊 Professional Component: {result.professionalComponent}");
                Console.WriteLine($"   📊 Technical Factor: {result.technicalFactor:N0}");
                Console.WriteLine($"   📊 Professional Factor: {result.professionalFactor:N0}");
                Console.WriteLine($"   📊 Formula: {result.calculationFormula}");
                Console.WriteLine($"   📊 Has Override: {result.hasOverride}");
                Console.WriteLine($"   📊 Financial Year: {result.financialYear}");

                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت مشترک (Create) موفق - ServiceId: {ServiceId}, Price: {Price}, Formula: {Formula}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, calculationResult.CalculatedPrice, calculationResult.CalculationFormula, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CONTROLLER] خطا در محاسبه قیمت خدمت مشترک (Create)");
                Console.WriteLine($"❌ [CONTROLLER] ServiceId: {serviceId}, DepartmentId: {departmentId}");
                Console.WriteLine($"❌ [CONTROLLER] Error: {ex.Message}");
                Console.WriteLine($"❌ [CONTROLLER] Stack Trace: {ex.StackTrace}");
                
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت مشترک (Create) - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, departmentId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه قیمت خدمت مشترک (برای Edit/Details)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateSharedServicePrice(int sharedServiceId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت مشترک - SharedServiceId: {SharedServiceId}, کاربر: {UserName} (Id: {UserId})",
                    sharedServiceId, _currentUserService.UserName, _currentUserService.UserId);

                var sharedService = await _context.SharedServices
                    .Include(ss => ss.Service)
                    .Include(ss => ss.Department)
                    .FirstOrDefaultAsync(ss => ss.SharedServiceId == sharedServiceId && !ss.IsDeleted);

                if (sharedService == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت مشترک یافت نشد - SharedServiceId: {SharedServiceId}, کاربر: {UserName} (Id: {UserId})",
                        sharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "خدمت مشترک یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var calculationResult = await _serviceCalculationService.CalculateSharedServicePriceAsync(
                    sharedService.ServiceId, sharedService.DepartmentId, _context, 
                    sharedService.OverrideTechnicalFactor, sharedService.OverrideProfessionalFactor);
                
                if (!calculationResult.Success)
                {
                    return Json(new { success = false, message = calculationResult.Message }, JsonRequestBehavior.AllowGet);
                }
                
                var calculatedPrice = calculationResult.CalculatedPrice;

                var result = new
                {
                    success = true,
                    sharedServiceId = sharedService.SharedServiceId,
                    serviceTitle = sharedService.Service.Title,
                    serviceCode = sharedService.Service.ServiceCode,
                    departmentName = sharedService.Department.Name,
                    calculatedPrice = calculatedPrice,
                    hasOverride = sharedService.OverrideTechnicalFactor.HasValue || sharedService.OverrideProfessionalFactor.HasValue,
                    overrideTechnicalFactor = sharedService.OverrideTechnicalFactor,
                    overrideProfessionalFactor = sharedService.OverrideProfessionalFactor
                };

                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت مشترک موفق - SharedServiceId: {SharedServiceId}, Price: {Price}, کاربر: {UserName} (Id: {UserId})",
                    sharedServiceId, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت مشترک - SharedServiceId: {SharedServiceId}, کاربر: {UserName} (Id: {UserId})",
                    sharedServiceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی وضعیت خدمت در دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> CheckServiceInDepartment(int serviceId, int departmentId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: بررسی وضعیت خدمت در دپارتمان - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, departmentId, _currentUserService.UserName, _currentUserService.UserId);

                var isShared = await _serviceCalculationService.IsServiceSharedInDepartmentAsync(serviceId, departmentId, _context);

                var result = new
                {
                    success = true,
                    serviceId = serviceId,
                    departmentId = departmentId,
                    isShared = isShared
                };

                _logger.Information("🏥 MEDICAL: بررسی وضعیت خدمت در دپارتمان موفق - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, IsShared: {IsShared}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, departmentId, isShared, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی وضعیت خدمت در دپارتمان - ServiceId: {ServiceId}, DepartmentId: {DepartmentId}, کاربر: {UserName} (Id: {UserId})",
                    serviceId, departmentId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// محاسبه آمار کلی خدمات مشترک
        /// </summary>
        private async Task<SharedServiceStatisticsViewModel> CalculateStatistics()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: شروع محاسبه آمار کلی خدمات مشترک - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // آمار کلی خدمات مشترک
                var totalSharedServices = await _context.SharedServices
                    .Where(ss => !ss.IsDeleted)
                    .CountAsync();

                var activeSharedServices = await _context.SharedServices
                    .Where(ss => !ss.IsDeleted && ss.IsActive)
                    .CountAsync();

                var inactiveSharedServices = totalSharedServices - activeSharedServices;

                // آمار دپارتمان‌ها
                var totalDepartments = await _context.Departments
                    .Where(d => !d.IsDeleted)
                    .CountAsync();

                // آمار خدمات
                var totalServices = await _context.Services
                    .Where(s => !s.IsDeleted)
                    .CountAsync();

                // آمار Override
                var servicesWithOverride = await _context.SharedServices
                    .Where(ss => !ss.IsDeleted && 
                                (ss.OverrideTechnicalFactor.HasValue || ss.OverrideProfessionalFactor.HasValue))
                    .CountAsync();

                var servicesWithoutOverride = totalSharedServices - servicesWithOverride;

                // محاسبه میانگین
                var averageServicesPerDepartment = totalDepartments > 0 ? (double)totalSharedServices / totalDepartments : 0;

                // محاسبه درصدها
                var activeServicesPercentage = totalSharedServices > 0 ? (double)activeSharedServices / totalSharedServices * 100 : 0;
                var inactiveServicesPercentage = totalSharedServices > 0 ? (double)inactiveSharedServices / totalSharedServices * 100 : 0;

                var statistics = new SharedServiceStatisticsViewModel
                {
                    TotalSharedServices = totalSharedServices,
                    ActiveSharedServices = activeSharedServices,
                    InactiveSharedServices = inactiveSharedServices,
                    AverageServicesPerDepartment = averageServicesPerDepartment,
                    TotalDepartments = totalDepartments,
                    TotalServices = totalServices,
                    ServicesWithOverride = servicesWithOverride,
                    ServicesWithoutOverride = servicesWithoutOverride,
                    ActiveServicesPercentage = activeServicesPercentage,
                    InactiveServicesPercentage = inactiveServicesPercentage
                };

                _logger.Information("🏥 MEDICAL: محاسبه آمار کلی موفق - کل: {Total}, فعال: {Active}, غیرفعال: {Inactive}, میانگین: {Average}, کاربر: {UserName} (Id: {UserId})",
                    totalSharedServices, activeSharedServices, inactiveSharedServices, averageServicesPerDepartment, 
                    _currentUserService.UserName, _currentUserService.UserId);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه آمار کلی - کاربر: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                // بازگشت آمار پیش‌فرض در صورت خطا
                return new SharedServiceStatisticsViewModel();
            }
        }

        #endregion

        #region Helper Methods


        /// <summary>
        /// تنظیم گزینه‌های فیلتر
        /// </summary>
        private async Task SetupFilterOptions(SharedServiceFilterViewModel filter)
        {
            try
            {
                // دریافت دپارتمان‌ها
                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToListAsync();

                filter.Departments = departments.Select(d => new System.Web.Mvc.SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name,
                    Selected = filter.DepartmentId.HasValue && d.DepartmentId == filter.DepartmentId.Value
                }).ToList();

                // دریافت خدمات
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                filter.Services = services.Select(s => new System.Web.Mvc.SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}",
                    Selected = filter.ServiceId.HasValue && s.ServiceId == filter.ServiceId.Value
                }).ToList();

                // به‌روزرسانی انتخاب‌های فیلتر
                filter.UpdateSelectedValues();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم گزینه‌های فیلتر");
            }
        }

        /// <summary>
        /// تنظیم ViewBag برای فیلترها (برای سازگاری با کد قدیمی)
        /// </summary>
        private async Task SetupFilterViewBags(int? departmentId, int? serviceId)
        {
            try
            {
                // دریافت دپارتمان‌ها
                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToListAsync();

                ViewBag.Departments = departments.Select(d => new System.Web.Mvc.SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name,
                    Selected = departmentId.HasValue && d.DepartmentId == departmentId.Value
                }).ToList();

                // دریافت خدمات
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                ViewBag.Services = services.Select(s => new System.Web.Mvc.SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}",
                    Selected = serviceId.HasValue && s.ServiceId == serviceId.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ViewBag برای فیلترها");
            }
        }

        /// <summary>
        /// تنظیم ViewBag برای فرم‌های ایجاد و ویرایش
        /// </summary>
        private async Task SetupCreateEditViewBags(SharedServiceCreateEditViewModel model)
        {
            try
            {
                // دریافت دپارتمان‌ها
                var departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Select(d => new { d.DepartmentId, d.Name })
                    .ToListAsync();

                model.Departments = departments.Select(d => new System.Web.Mvc.SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name,
                    Selected = d.DepartmentId == model.DepartmentId
                }).ToList();

                // دریافت خدمات
                var services = await _context.Services
                    .Where(s => !s.IsDeleted && s.IsActive)
                    .Select(s => new { s.ServiceId, s.ServiceCode, s.Title })
                    .ToListAsync();

                model.Services = services.Select(s => new System.Web.Mvc.SelectListItem
                {
                    Value = s.ServiceId.ToString(),
                    Text = $"{s.ServiceCode} - {s.Title}",
                    Selected = s.ServiceId == model.ServiceId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم ViewBag برای فرم‌های ایجاد و ویرایش");
            }
        }

        #endregion
    }
}
