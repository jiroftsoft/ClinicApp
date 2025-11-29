using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.ClinicAdmin;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller برای مدیریت حساب بانکی کلینیک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. CRUD Operations برای حساب بانکی
    /// 2. مدیریت رابطه One-to-One با Clinic
    /// 3. Validation کامل شماره شبا
    /// 4. استفاده از ServiceResult Pattern
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط HTTP handling و ViewModel mapping
    /// ✅ Separation of Concerns: منطق کسب‌وکار در Service Layer
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Controller فقط HTTP handling
    /// </summary>
    public class ClinicBankAccountController : ClinicApp.Controllers.BaseController
    {
        private readonly IClinicBankAccountService _service;
        private readonly IClinicManagementService _clinicService;

        public ClinicBankAccountController(
            IClinicBankAccountService service,
            IClinicManagementService clinicService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _clinicService = clinicService ?? throw new ArgumentNullException(nameof(clinicService));
        }

        #region Index (List)

        /// <summary>
        /// نمایش لیست حساب‌های بانکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            using (StartPerformanceMonitoring("ClinicBankAccountIndex"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست نمایش لیست حساب‌های بانکی. User: {UserId}",
                        _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    var result = await _service.GetAllAsync();

                    if (!result.Success)
                    {
                        AddError(result.Message);
                        return View(new System.Collections.Generic.List<ClinicBankAccountIndexViewModel>());
                    }

                    _logger.Information("🏥 MEDICAL: {Count} حساب بانکی برای نمایش آماده شد. User: {UserId}",
                        result.Data?.Count ?? 0, _currentUserService?.UserId ?? "Anonymous");

                    return View(result.Data);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش لیست حساب‌های بانکی. User: {UserId}",
                        _currentUserService?.UserId ?? "Anonymous");
                    return HandleError(ex, "خطا در نمایش لیست حساب‌های بانکی");
                }
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات حساب بانکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountDetails"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست نمایش جزئیات حساب بانکی: {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    if (id <= 0)
                    {
                        AddError("شناسه حساب بانکی نامعتبر است.");
                        return RedirectToAction("Index");
                    }

                    var result = await _service.GetByIdAsync(id);

                    if (!result.Success)
                    {
                        AddError(result.Message);
                        return RedirectToAction("Index");
                    }

                    _logger.Information("🏥 MEDICAL: جزئیات حساب بانکی {Id} با موفقیت نمایش داده شد. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");

                    return View(result.Data);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش جزئیات حساب بانکی {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");
                    return HandleError(ex, "خطا در نمایش جزئیات حساب بانکی");
                }
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد حساب بانکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create(int? clinicId = null)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountCreateGet"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست نمایش فرم ایجاد حساب بانکی. ClinicId: {ClinicId}, User: {UserId}",
                        clinicId, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    var model = new ClinicBankAccountCreateEditViewModel
                    {
                        ClinicId = clinicId ?? 0,
                        IsActive = true,
                        IsDefault = true
                    };

                    // بارگذاری لیست کلینیک‌ها برای Dropdown
                    await LoadClinicsForDropdown();

                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ایجاد حساب بانکی. User: {UserId}",
                        _currentUserService?.UserId ?? "Anonymous");
                    return HandleError(ex, "خطا در نمایش فرم ایجاد حساب بانکی");
                }
            }
        }

        /// <summary>
        /// ایجاد حساب بانکی جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClinicBankAccountCreateEditViewModel model)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountCreatePost"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست ایجاد حساب بانکی برای کلینیک: {ClinicId}. User: {UserId}",
                        model.ClinicId, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    if (!ModelState.IsValid)
                    {
                        await LoadClinicsForDropdown();
                        return View(model);
                    }

                    var result = await _service.CreateAsync(model);

                    if (!result.Success)
                    {
                        AddErrors(result.ValidationErrors);
                        AddError(result.Message);
                        await LoadClinicsForDropdown();
                        return View(model);
                    }

                    AddSuccess("حساب بانکی با موفقیت ایجاد شد.");
                    _logger.Information("🏥 MEDICAL: حساب بانکی با شناسه {Id} با موفقیت ایجاد شد. User: {UserId}",
                        result.Data, _currentUserService?.UserId ?? "Anonymous");

                    return RedirectToAction("Details", new { id = result.Data });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در ایجاد حساب بانکی. User: {UserId}",
                        _currentUserService?.UserId ?? "Anonymous");
                    AddError("خطا در ایجاد حساب بانکی.");
                    await LoadClinicsForDropdown();
                    return View(model);
                }
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش حساب بانکی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountEditGet"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست نمایش فرم ویرایش حساب بانکی: {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    if (id <= 0)
                    {
                        AddError("شناسه حساب بانکی نامعتبر است.");
                        return RedirectToAction("Index");
                    }

                    var result = await _service.GetForEditAsync(id);

                    if (!result.Success)
                    {
                        AddError(result.Message);
                        return RedirectToAction("Index");
                    }

                    await LoadClinicsForDropdown();

                    return View(result.Data);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ویرایش حساب بانکی {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");
                    return HandleError(ex, "خطا در نمایش فرم ویرایش حساب بانکی");
                }
            }
        }

        /// <summary>
        /// به‌روزرسانی حساب بانکی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ClinicBankAccountCreateEditViewModel model)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountEditPost"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست به‌روزرسانی حساب بانکی: {Id}. User: {UserId}",
                        model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    if (!ModelState.IsValid)
                    {
                        await LoadClinicsForDropdown();
                        return View(model);
                    }

                    var result = await _service.UpdateAsync(model);

                    if (!result.Success)
                    {
                        AddErrors(result.ValidationErrors);
                        AddError(result.Message);
                        await LoadClinicsForDropdown();
                        return View(model);
                    }

                    AddSuccess("حساب بانکی با موفقیت به‌روزرسانی شد.");
                    _logger.Information("🏥 MEDICAL: حساب بانکی {Id} با موفقیت به‌روزرسانی شد. User: {UserId}",
                        model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                    return RedirectToAction("Details", new { id = model.ClinicBankAccountId });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در به‌روزرسانی حساب بانکی {Id}. User: {UserId}",
                        model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");
                    AddError("خطا در به‌روزرسانی حساب بانکی.");
                    await LoadClinicsForDropdown();
                    return View(model);
                }
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف حساب بانکی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            using (StartPerformanceMonitoring("ClinicBankAccountDelete"))
            {
                try
                {
                    _logger.Information("🏥 MEDICAL: درخواست حذف حساب بانکی: {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");

                    AddSecurityHeaders();

                    if (id <= 0)
                    {
                        return Json(new { success = false, message = "شناسه حساب بانکی نامعتبر است." });
                    }

                    var result = await _service.DeleteAsync(id);

                    if (!result.Success)
                    {
                        return Json(new { success = false, message = result.Message });
                    }

                    _logger.Information("🏥 MEDICAL: حساب بانکی {Id} با موفقیت حذف شد. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");

                    return Json(new { success = true, message = "حساب بانکی با موفقیت حذف شد." });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "🏥 MEDICAL: خطا در حذف حساب بانکی {Id}. User: {UserId}",
                        id, _currentUserService?.UserId ?? "Anonymous");
                    return Json(new { success = false, message = "خطا در حذف حساب بانکی." });
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// بارگذاری لیست کلینیک‌ها برای Dropdown
        /// </summary>
        private async Task LoadClinicsForDropdown()
        {
            try
            {
                var clinicsResult = await _clinicService.GetActiveClinicsForLookupAsync();
                if (clinicsResult.Success && clinicsResult.Data != null)
                {
                    ViewBag.Clinics = new System.Web.Mvc.SelectList(
                        clinicsResult.Data,
                        "Id",
                        "Name"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ خطا در بارگذاری لیست کلینیک‌ها");
            }
        }

        #endregion
    }
}

