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
using ClinicApp.Helpers;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    //[Authorize] // امنیت: فقط کاربران احراز هویت شده
    public class FactorSettingController : BaseController
    {
        private readonly IFactorSettingService _factorSettingService;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public FactorSettingController(
            IFactorSettingService factorSettingService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _factorSettingService = factorSettingService ?? throw new ArgumentNullException(nameof(factorSettingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        // GET: Admin/FactorSetting
        public async Task<ActionResult> Index(int? page, int? pageSize, string searchTerm, ServiceComponentType? factorType, FactorScope? scope, int? financialYear, bool? isActive)
        {
            try
            {
                // تنظیمات پیش‌فرض
                var currentPage = page ?? 1;
                var currentPageSize = Math.Min(pageSize ?? 20, 100); // حداکثر 100 رکورد در صفحه
                
                _logger.Information("دریافت لیست کای‌ها - صفحه: {Page}, اندازه: {PageSize}, جستجو: {SearchTerm}", 
                    currentPage, currentPageSize, searchTerm);

                // دریافت فیلتر شده کای‌ها
                var factors = await _factorSettingService.GetFilteredFactorsAsync(
                    searchTerm, factorType, financialYear, isActive, currentPage, currentPageSize);

                var totalCount = await _factorSettingService.GetFactorsCountAsync(searchTerm, factorType, financialYear, isActive);

                var viewModel = factors.Select(f => new FactorSettingViewModel
                {
                    Id = f.FactorSettingId,
                    Name = $"کای {(f.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {f.FinancialYear}",
                    Description = f.Description,
                    FactorType = f.FactorType,
                    Scope = f.Scope,
                    IsHashtagged = f.IsHashtagged,
                    Value = f.Value,
                    FinancialYear = f.FinancialYear,
                    IsActive = f.IsActive,
                    CreatedAt = f.CreatedAt,
                    CreatedBy = f.CreatedByUserId,
                    UpdatedAt = f.UpdatedAt,
                    UpdatedBy = f.UpdatedByUserId
                }).ToList();

                // اطلاعات Pagination
                ViewBag.CurrentPage = currentPage;
                ViewBag.PageSize = currentPageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / currentPageSize);
                ViewBag.SearchTerm = searchTerm;
                ViewBag.FactorType = factorType;
                ViewBag.Scope = scope;
                ViewBag.FinancialYear = financialYear;
                ViewBag.IsActive = isActive;
                
                // لیست Scope ها برای Dropdown
                ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                    .Cast<FactorScope>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = GetScopeDisplayName(s),
                        Selected = s == scope
                    }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست کای‌ها - صفحه: {Page}, اندازه: {PageSize}", page, pageSize);
                TempData["ErrorMessage"] = "خطا در دریافت لیست کای‌ها";
                return View(new List<FactorSettingViewModel>());
            }
        }

        /// <summary>
        /// دریافت نام نمایشی Scope
        /// </summary>
        private string GetScopeDisplayName(FactorScope scope)
        {
            return scope switch
            {
                FactorScope.General_NoHash => "عمومی بدون هشتگ",
                FactorScope.Hash_1_7 => "هشتگ‌دار (کدهای ۱-۷)",
                FactorScope.Hash_8_9 => "هشتگ‌دار (کدهای ۸-۹)",
                FactorScope.Dent_Technical => "دندانپزشکی فنی",
                FactorScope.Dent_Consumables => "مواد دندانپزشکی",
                FactorScope.Prof_NoHash => "حرفه‌ای بدون هشتگ",
                FactorScope.Prof_Hash => "حرفه‌ای هشتگ‌دار",
                FactorScope.Prof_Dental => "حرفه‌ای دندانپزشکی",
                _ => scope.ToString()
            };
        }

        // GET: Admin/FactorSetting/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var factor = await _factorSettingService.GetFactorByIdAsync(id);
                if (factor == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new FactorSettingViewModel
                {
                    Id = factor.FactorSettingId,
                    Name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                    Description = factor.Description,
                    FactorType = factor.FactorType,
                    Scope = factor.Scope,
                    ScopeDisplayName = GetScopeDisplayName(factor.Scope),
                    IsHashtagged = factor.IsHashtagged,
                    Value = factor.Value,
                    FinancialYear = factor.FinancialYear,
                    IsActive = factor.IsActive,
                    CreatedAt = factor.CreatedAt,
                    CreatedBy = factor.CreatedByUserId,
                    UpdatedAt = factor.UpdatedAt,
                    UpdatedBy = factor.UpdatedByUserId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت جزئیات کای {FactorId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت جزئیات کای";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/FactorSetting/Create
        public ActionResult Create()
        {
            var viewModel = new FactorSettingCreateEditViewModel
            {
                FactorType = ServiceComponentType.Technical, // پیش‌فرض
                Scope = FactorScope.General_NoHash, // پیش‌فرض
                FinancialYear = 1404, // سال مالی جاری
                IsActive = true
            };

            // لیست Scope ها برای Dropdown
            ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                .Cast<FactorScope>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = GetScopeDisplayName(s)
                }).ToList();

            return View(viewModel);
        }

        // POST: Admin/FactorSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FactorSettingCreateEditViewModel model)
        {
            try
            {
                // اعتبارسنجی اولیه
                if (!ModelState.IsValid)
                {
                    _logger.Warning("اعتبارسنجی مدل ناموفق برای ایجاد کای جدید");
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateFactorForCreationAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    _logger.Warning("اعتبارسنجی Business Rules ناموفق: {Errors}", string.Join(", ", validationResult.Errors.Values));
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // بررسی تکراری بودن کای
                var exists = await _factorSettingService.ExistsFactorAsync(model.FactorType, model.FinancialYear, model.IsHashtagged);
                if (exists)
                {
                    ModelState.AddModelError("", $"کای {model.FactorType} برای سال مالی {model.FinancialYear} و وضعیت هشتگ‌دار {(model.IsHashtagged ? "هشتگ‌دار" : "عادی")} قبلاً وجود دارد.");
                    _logger.Warning("کای تکراری: {FactorType} - سال {FinancialYear} - هشتگ‌دار: {IsHashtagged}", 
                        model.FactorType, model.FinancialYear, model.IsHashtagged);
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // ایجاد کای جدید
                var factor = model.ToEntity();
                factor.CreatedAt = DateTime.UtcNow;
                factor.CreatedByUserId = _currentUserService.GetCurrentUserId();

                var createdFactor = await _factorSettingService.CreateFactorAsync(factor);
                
                _logger.Information("کای جدید ایجاد شد: ID={FactorId}, نوع={FactorType}, سال={FinancialYear}, هشتگ‌دار={IsHashtagged}, مقدار={Value}", 
                    createdFactor.FactorSettingId, factor.FactorType, factor.FinancialYear, factor.IsHashtagged, factor.Value);
                
                TempData["SuccessMessage"] = $"کای {factor.FactorType} با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کای جدید - نوع: {FactorType}, سال: {FinancialYear}", model.FactorType, model.FinancialYear);
                TempData["ErrorMessage"] = "خطا در ایجاد کای جدید. لطفاً دوباره تلاش کنید.";
                
                // لیست Scope ها برای Dropdown
                ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                    .Cast<FactorScope>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = GetScopeDisplayName(s)
                    }).ToList();
                
                return View(model);
            }
        }

        // GET: Admin/FactorSetting/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var factor = await _factorSettingService.GetFactorByIdAsync(id);
                if (factor == null)
                {
                    return HttpNotFound();
                }

                var viewModel = FactorSettingCreateEditViewModel.FromEntity(factor);
                
                // لیست Scope ها برای Dropdown
                ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                    .Cast<FactorScope>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = GetScopeDisplayName(s)
                    }).ToList();
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای برای ویرایش {FactorId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت کای برای ویرایش";
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/FactorSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(FactorSettingCreateEditViewModel model)
        {
            try
            {
                // اعتبارسنجی اولیه
                if (!ModelState.IsValid)
                {
                    _logger.Warning("اعتبارسنجی مدل ناموفق برای ویرایش کای {FactorId}", model.Id);
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // بررسی وجود کای
                var existingFactor = await _factorSettingService.GetFactorByIdAsync(model.Id);
                if (existingFactor == null)
                {
                    _logger.Warning("کای با شناسه {FactorId} یافت نشد", model.Id);
                    return HttpNotFound();
                }

                // اعتبارسنجی Business Rules
                var validationResult = await ValidateFactorForEditAsync(model, model.Id);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    _logger.Warning("اعتبارسنجی Business Rules ناموفق برای ویرایش کای {FactorId}: {Errors}", 
                        model.Id, string.Join(", ", validationResult.Errors.Values));
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // بررسی تکراری بودن کای (به جز خود رکورد)
                var exists = await _factorSettingService.ExistsFactorAsync(model.FactorType, model.FinancialYear, model.IsHashtagged);
                if (exists && (existingFactor.FactorType != model.FactorType || 
                               existingFactor.FinancialYear != model.FinancialYear || 
                               existingFactor.IsHashtagged != model.IsHashtagged))
                {
                    ModelState.AddModelError("", $"کای {model.FactorType} برای سال مالی {model.FinancialYear} و وضعیت هشتگ‌دار {(model.IsHashtagged ? "هشتگ‌دار" : "عادی")} قبلاً وجود دارد.");
                    _logger.Warning("کای تکراری برای ویرایش: {FactorType} - سال {FinancialYear} - هشتگ‌دار: {IsHashtagged}", 
                        model.FactorType, model.FinancialYear, model.IsHashtagged);
                    
                    // لیست Scope ها برای Dropdown
                    ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                        .Cast<FactorScope>()
                        .Select(s => new SelectListItem
                        {
                            Value = ((int)s).ToString(),
                            Text = GetScopeDisplayName(s)
                        }).ToList();
                    
                    return View(model);
                }

                // به‌روزرسانی کای
                var factor = model.ToEntity();
                factor.FactorSettingId = existingFactor.FactorSettingId; // حفظ ID اصلی
                factor.CreatedAt = existingFactor.CreatedAt; // حفظ تاریخ ایجاد
                factor.CreatedByUserId = existingFactor.CreatedByUserId; // حفظ ایجادکننده
                factor.UpdatedAt = DateTime.UtcNow;
                factor.UpdatedByUserId = _currentUserService.GetCurrentUserId();

                var updatedFactor = await _factorSettingService.UpdateFactorAsync(factor);
                
                _logger.Information("کای به‌روزرسانی شد: ID={FactorId}, نوع={FactorType}, سال={FinancialYear}, هشتگ‌دار={IsHashtagged}, مقدار={Value}", 
                    updatedFactor.FactorSettingId, factor.FactorType, factor.FinancialYear, factor.IsHashtagged, factor.Value);
                
                TempData["SuccessMessage"] = $"کای {factor.FactorType} با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی کای {FactorId} - نوع: {FactorType}, سال: {FinancialYear}", 
                    model.Id, model.FactorType, model.FinancialYear);
                TempData["ErrorMessage"] = "خطا در به‌روزرسانی کای. لطفاً دوباره تلاش کنید.";
                
                // لیست Scope ها برای Dropdown
                ViewBag.Scopes = Enum.GetValues(typeof(FactorScope))
                    .Cast<FactorScope>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = GetScopeDisplayName(s)
                    }).ToList();
                
                return View(model);
            }
        }

        // GET: Admin/FactorSetting/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var factor = await _factorSettingService.GetFactorByIdAsync(id);
                if (factor == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new FactorSettingViewModel
                {
                    Id = factor.FactorSettingId,
                    Name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                    Description = factor.Description,
                    FactorType = factor.FactorType,
                    Scope = factor.Scope,
                    ScopeDisplayName = GetScopeDisplayName(factor.Scope),
                    IsHashtagged = factor.IsHashtagged,
                    Value = factor.Value,
                    FinancialYear = factor.FinancialYear,
                    IsActive = factor.IsActive,
                    CreatedAt = factor.CreatedAt,
                    CreatedBy = factor.CreatedByUserId,
                    UpdatedAt = factor.UpdatedAt,
                    UpdatedBy = factor.UpdatedByUserId
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای برای حذف {FactorId}", id);
                TempData["ErrorMessage"] = "خطا در دریافت کای برای حذف";
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/FactorSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, string confirmText)
        {
            try
            {
                // بررسی تأیید حذف
                if (confirmText != "حذف")
                {
                    ModelState.AddModelError("", "برای تأیید حذف، کلمه 'حذف' را وارد کنید");
                    return await Delete(id);
                }

                // بررسی وجود کای قبل از حذف
                var factor = await _factorSettingService.GetFactorByIdAsync(id);
                if (factor == null)
                {
                    _logger.Warning("کای با شناسه {FactorId} یافت نشد", id);
                    TempData["ErrorMessage"] = "کای مورد نظر یافت نشد";
                    return RedirectToAction("Index");
                }

                // بررسی استفاده از کای در سیستم
                var isUsed = await _factorSettingService.IsFactorUsedInCalculationsAsync(id);
                if (isUsed)
                {
                    _logger.Warning("کای {FactorId} در حال استفاده است و قابل حذف نیست", id);
                    TempData["ErrorMessage"] = "این کای در حال استفاده است و نمی‌توان آن را حذف کرد";
                    return RedirectToAction("Index");
                }

                // بررسی فریز بودن کای
                if (factor.IsFrozen)
                {
                    _logger.Warning("کای {FactorId} فریز شده است و قابل حذف نیست", id);
                    TempData["ErrorMessage"] = "کای فریز شده قابل حذف نیست";
                    return RedirectToAction("Index");
                }

                await _factorSettingService.DeleteFactorAsync(id);
                _logger.Information("کای حذف شد: ID={FactorId}, نوع={FactorType}, سال={FinancialYear}, مقدار={Value}", 
                    id, factor.FactorType, factor.FinancialYear, factor.Value);
                
                TempData["SuccessMessage"] = $"کای {factor.FactorType} با موفقیت حذف شد";
                return RedirectToAction("DeleteConfirmed");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف کای {FactorId}", id);
                TempData["ErrorMessage"] = "خطا در حذف کای. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/FactorSetting/DeleteConfirmed
        [HttpGet]
        public ActionResult DeleteConfirmed()
        {
            return View();
        }

        // GET: Admin/FactorSetting/GetFactorsByType
        [HttpGet]
        public async Task<JsonResult> GetFactorsByType(ServiceComponentType factorType, int financialYear)
        {
            try
            {
                var factors = await _factorSettingService.GetFactorsByTypeAsync(factorType, financialYear);
                var result = factors.Select(f => new
                {
                    id = f.FactorSettingId,
                    name = $"کای {(f.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {f.FinancialYear}",
                    value = f.Value,
                    isActive = f.IsActive,
                    isHashtagged = f.IsHashtagged,
                    description = f.Description,
                    effectiveFrom = f.EffectiveFrom.ToString("yyyy-MM-dd"),
                    effectiveTo = f.EffectiveTo?.ToString("yyyy-MM-dd")
                }).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌ها بر اساس نوع {FactorType} و سال مالی {FinancialYear}", factorType, financialYear);
                return Json(new { error = "خطا در دریافت کای‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/FactorSetting/GetActiveFactor
        [HttpGet]
        public async Task<JsonResult> GetActiveFactor(ServiceComponentType factorType, int financialYear, bool isHashtagged = false)
        {
            try
            {
                var factor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(factorType, isHashtagged, financialYear);
                if (factor == null)
                {
                    return Json(new { error = "کای فعال یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    id = factor.FactorSettingId,
                    name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                    value = factor.Value,
                    isActive = factor.IsActive,
                    isHashtagged = factor.IsHashtagged,
                    description = factor.Description,
                    effectiveFrom = factor.EffectiveFrom.ToString("yyyy-MM-dd"),
                    effectiveTo = factor.EffectiveTo?.ToString("yyyy-MM-dd")
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای فعال بر اساس نوع {FactorType} و سال مالی {FinancialYear}", factorType, financialYear);
                return Json(new { error = "خطا در دریافت کای فعال" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/FactorSetting/FreezeFinancialYear
        [HttpGet]
        public async Task<ActionResult> FreezeFinancialYear(int financialYear)
        {
            try
            {
                var factors = await _factorSettingService.GetFactorsByFinancialYearAsync(financialYear);
                var activeFactors = factors.Where(f => f.IsActive && !f.IsFrozen).ToList();

                ViewBag.FinancialYear = financialYear;
                ViewBag.ActiveFactorsCount = activeFactors.Count;

                return View(activeFactors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌های سال مالی {FinancialYear} برای فریز", financialYear);
                TempData["ErrorMessage"] = "خطا در دریافت کای‌های سال مالی";
                return RedirectToAction("Index");
            }
        }

        // POST: Admin/FactorSetting/FreezeFinancialYear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> FreezeFinancialYear(int financialYear, string confirmText)
        {
            try
            {
                if (confirmText != "فریز")
                {
                    ModelState.AddModelError("", "برای تأیید فریز کردن، کلمه 'فریز' را وارد کنید");
                    return await FreezeFinancialYear(financialYear);
                }

                var userId = _currentUserService.GetCurrentUserId();
                var result = await _factorSettingService.FreezeFinancialYearFactorsAsync(financialYear, userId);

                if (result)
                {
                    _logger.Information("کای‌های سال مالی {FinancialYear} توسط کاربر {UserId} فریز شدند", financialYear, userId);
                    TempData["SuccessMessage"] = $"کای‌های سال مالی {financialYear} با موفقیت فریز شدند";
                }
                else
                {
                    TempData["ErrorMessage"] = "خطا در فریز کردن کای‌ها";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فریز کردن کای‌های سال مالی {FinancialYear}", financialYear);
                TempData["ErrorMessage"] = "خطا در فریز کردن کای‌ها";
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/FactorSetting/GetFactorStatistics
        [HttpGet]
        public async Task<JsonResult> GetFactorStatistics()
        {
            try
            {
                var currentYear = DateTime.Now.Year + 621; // تبدیل به شمسی
                
                var statistics = new
                {
                    totalFactors = await _factorSettingService.GetFactorsCountAsync(null, null, null, null),
                    activeFactors = await _factorSettingService.GetFactorsCountAsync(null, null, null, true),
                    currentYearFactors = await _factorSettingService.GetFactorsCountAsync(null, null, currentYear, null),
                    technicalFactors = await _factorSettingService.GetFactorsCountAsync(null, ServiceComponentType.Technical, null, null),
                    professionalFactors = await _factorSettingService.GetFactorsCountAsync(null, ServiceComponentType.Professional, null, null),
                    frozenFactors = (await _factorSettingService.GetFrozenFactorsAsync()).Count()
                };

                return Json(statistics, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار کای‌ها");
                return Json(new { error = "خطا در دریافت آمار" }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// اعتبارسنجی Business Rules برای ایجاد کای جدید
        /// </summary>
        private async Task<ValidationResult> ValidateFactorForCreationAsync(FactorSettingCreateEditViewModel model)
        {
            var result = new ValidationResult { IsValid = true, Errors = new Dictionary<string, string>() };

            try
            {
                // اعتبارسنجی سال مالی
                var currentYear = DateTime.Now.Year;
                var persianCurrentYear = currentYear + 621; // تبدیل میلادی به شمسی
                
                if (model.FinancialYear < 1400 || model.FinancialYear > persianCurrentYear + 1)
                {
                    result.IsValid = false;
                    result.Errors["FinancialYear"] = $"سال مالی باید بین 1400 تا {persianCurrentYear + 1} باشد";
                }

                // اعتبارسنجی مقدار کای
                if (model.Value <= 0 || model.Value > 999999.99m)
                {
                    result.IsValid = false;
                    result.Errors["Value"] = "مقدار کای باید بین 0.01 تا 999999.99 باشد";
                }

                // اعتبارسنجی توضیحات
                if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 500)
                {
                    result.IsValid = false;
                    result.Errors["Description"] = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد";
                }

                // اعتبارسنجی Business Rules خاص
                if (model.FactorType == ServiceComponentType.Technical && model.IsHashtagged && model.Value > 50000)
                {
                    result.IsValid = false;
                    result.Errors["Value"] = "کای فنی هشتگ‌دار نمی‌تواند بیش از 50000 باشد";
                }

                if (model.FactorType == ServiceComponentType.Professional && model.Value > 100000)
                {
                    result.IsValid = false;
                    result.Errors["Value"] = "کای حرفه‌ای نمی‌تواند بیش از 100000 باشد";
                }

                // بررسی وجود کای‌های فعال برای سال مالی
                var activeFactorsCount = await _factorSettingService.GetActiveFactorsCountForYearAsync(model.FinancialYear);
                if (activeFactorsCount >= 10) // حداکثر 10 کای فعال در سال
                {
                    result.IsValid = false;
                    result.Errors["FinancialYear"] = $"حداکثر 10 کای فعال برای سال مالی {model.FinancialYear} مجاز است";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی کای جدید");
                result.IsValid = false;
                result.Errors["General"] = "خطا در اعتبارسنجی. لطفاً دوباره تلاش کنید.";
                return result;
            }
        }

        /// <summary>
        /// اعتبارسنجی Business Rules برای ویرایش کای
        /// </summary>
        private async Task<ValidationResult> ValidateFactorForEditAsync(FactorSettingCreateEditViewModel model, int existingFactorId)
        {
            var result = new ValidationResult { IsValid = true, Errors = new Dictionary<string, string>() };

            try
            {
                // اعتبارسنجی‌های مشابه Create
                var createValidation = await ValidateFactorForCreationAsync(model);
                if (!createValidation.IsValid)
                {
                    result.IsValid = false;
                    foreach (var error in createValidation.Errors)
                    {
                        result.Errors[error.Key] = error.Value;
                    }
                }

                // اعتبارسنجی‌های خاص Edit
                var existingFactor = await _factorSettingService.GetFactorByIdAsync(existingFactorId);
                if (existingFactor == null)
                {
                    result.IsValid = false;
                    result.Errors["General"] = "کای مورد نظر یافت نشد";
                    return result;
                }

                // بررسی تغییرات مهم
                if (existingFactor.IsFrozen)
                {
                    result.IsValid = false;
                    result.Errors["General"] = "کای فریز شده قابل ویرایش نیست";
                }

                // بررسی استفاده از کای در سیستم
                var isUsed = await _factorSettingService.IsFactorUsedInCalculationsAsync(existingFactorId);
                if (isUsed && (existingFactor.FactorType != model.FactorType || existingFactor.Value != model.Value))
                {
                    result.IsValid = false;
                    result.Errors["General"] = "کای در حال استفاده است و نمی‌توان نوع یا مقدار آن را تغییر داد";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی ویرایش کای");
                result.IsValid = false;
                result.Errors["General"] = "خطا در اعتبارسنجی. لطفاً دوباره تلاش کنید.";
                return result;
            }
        }

        #endregion

        #region ServiceComponents Integration (یکپارچگی با ServiceComponents)

        /// <summary>
        /// دریافت کای‌های مورد استفاده در ServiceComponents
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetFactorsUsedInServiceComponents()
        {
            try
            {
                _logger.Information("درخواست کای‌های مورد استفاده در ServiceComponents");

                var technicalFactors = await _factorSettingService.GetCurrentYearFactorsAsync();
                var usedFactors = new List<object>();

                foreach (var factor in technicalFactors)
                {
                    var isUsed = await _factorSettingService.IsFactorUsedInCalculationsAsync(factor.FactorSettingId);
                    if (isUsed)
                    {
                        usedFactors.Add(new
                        {
                            id = factor.FactorSettingId,
                            name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                            type = factor.FactorType.ToString(),
                            typeName = factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                            value = factor.Value,
                            isHashtagged = factor.IsHashtagged,
                            financialYear = factor.FinancialYear,
                            isActive = factor.IsActive
                        });
                    }
                }

                _logger.Information("کای‌های مورد استفاده در ServiceComponents: {Count}", usedFactors.Count);

                return Json(new { success = true, factors = usedFactors }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای‌های مورد استفاده در ServiceComponents");
                return Json(new { success = false, message = "خطا در دریافت کای‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی وضعیت کای‌ها برای ServiceComponents
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetFactorsStatusForServiceComponents()
        {
            try
            {
                _logger.Information("بررسی وضعیت کای‌ها برای ServiceComponents");

                var currentYear = 1404; // سال مالی جاری
                var technicalFactor = await _factorSettingService.GetActiveFactorByTypeAsync(ServiceComponentType.Technical, currentYear, false);
                var technicalHashtaggedFactor = await _factorSettingService.GetActiveFactorByTypeAsync(ServiceComponentType.Technical, currentYear, true);
                var professionalFactor = await _factorSettingService.GetActiveFactorByTypeAsync(ServiceComponentType.Professional, currentYear, false);

                var status = new
                {
                    currentYear = currentYear,
                    hasTechnicalFactor = technicalFactor != null,
                    hasTechnicalHashtaggedFactor = technicalHashtaggedFactor != null,
                    hasProfessionalFactor = professionalFactor != null,
                    isComplete = technicalFactor != null && professionalFactor != null,
                    factors = new
                    {
                        technical = technicalFactor != null ? new
                        {
                            id = technicalFactor.FactorSettingId,
                            value = technicalFactor.Value,
                            isActive = technicalFactor.IsActive
                        } : null,
                        technicalHashtagged = technicalHashtaggedFactor != null ? new
                        {
                            id = technicalHashtaggedFactor.FactorSettingId,
                            value = technicalHashtaggedFactor.Value,
                            isActive = technicalHashtaggedFactor.IsActive
                        } : null,
                        professional = professionalFactor != null ? new
                        {
                            id = professionalFactor.FactorSettingId,
                            value = professionalFactor.Value,
                            isActive = professionalFactor.IsActive
                        } : null
                    }
                };

                _logger.Information("وضعیت کای‌ها: Technical={HasTechnical}, Professional={HasProfessional}, Complete={IsComplete}",
                    status.hasTechnicalFactor, status.hasProfessionalFactor, status.isComplete);

                return Json(new { success = true, status = status }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وضعیت کای‌ها برای ServiceComponents");
                return Json(new { success = false, message = "خطا در بررسی وضعیت کای‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت کای فعال بر اساس نوع و وضعیت هشتگ‌دار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetActiveFactorByTypeAndHashtagged(ServiceComponentType factorType, bool isHashtagged, int financialYear = 1404)
        {
            try
            {
                _logger.Information("درخواست کای فعال: Type={FactorType}, Hashtagged={IsHashtagged}, Year={FinancialYear}",
                    factorType, isHashtagged, financialYear);

                var factor = await _factorSettingService.GetActiveFactorByTypeAndHashtaggedAsync(factorType, isHashtagged, financialYear);

                if (factor == null)
                {
                    return Json(new { success = false, message = "کای فعال یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    factor = new
                    {
                        id = factor.FactorSettingId,
                        name = $"کای {(factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای")} {factor.FinancialYear}",
                        type = factor.FactorType.ToString(),
                        typeName = factor.FactorType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                        value = factor.Value,
                        isHashtagged = factor.IsHashtagged,
                        financialYear = factor.FinancialYear,
                        isActive = factor.IsActive,
                        description = factor.Description,
                        effectiveFrom = factor.EffectiveFrom.ToString("yyyy-MM-dd"),
                        effectiveTo = factor.EffectiveTo?.ToString("yyyy-MM-dd")
                    }
                };

                _logger.Information("کای فعال یافت شد: ID={FactorId}, Value={Value}", factor.FactorSettingId, factor.Value);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت کای فعال: Type={FactorType}, Hashtagged={IsHashtagged}, Year={FinancialYear}",
                    factorType, isHashtagged, financialYear);
                return Json(new { success = false, message = "خطا در دریافت کای فعال" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }

    /// <summary>
    /// کلاس کمکی برای نتایج اعتبارسنجی
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}
