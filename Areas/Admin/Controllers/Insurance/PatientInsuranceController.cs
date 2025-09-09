using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت بیمه‌های بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل بیمه‌های بیماران (Primary و Supplementary)
    /// 2. استفاده از Anti-Forgery Token در همه POST actions
    /// 3. استفاده از ServiceResult Enhanced pattern
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت روابط با Patient و InsurancePlan
    /// 7. مدیریت بیمه اصلی و تکمیلی
    /// 8. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [RouteArea("Admin")]
    [RoutePrefix("Insurance/PatientInsurance")]
    public class PatientInsuranceController : Controller
    {
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public PatientInsuranceController(
            IPatientInsuranceService patientInsuranceService,
            IInsurancePlanService insurancePlanService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _log = logger.ForContext<PatientInsuranceController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Helper Methods

        /// <summary>
        /// بارگیری لیست طرح‌های بیمه فعال برای ViewBag
        /// </summary>
        private async Task LoadInsurancePlansAsync()
        {
            try
            {
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    ViewBag.InsurancePlans = plansResult.Data;
                }
                else
                {
                    _log.Warning("خطا در بارگیری لیست طرح‌های بیمه. Error: {Error}", plansResult.Message);
                    ViewBag.InsurancePlans = new List<object>();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در بارگیری لیست طرح‌های بیمه");
                ViewBag.InsurancePlans = new List<object>();
            }
        }

        #endregion

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی بیمه‌های بیماران
        /// </summary>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<ActionResult> Index(string searchTerm = "", int? providerId = null, int? planId = null, 
            bool? isPrimary = null, bool? isActive = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1)
        {
            _log.Information("بازدید از صفحه اصلی بیمه‌های بیماران. SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, PlanId: {PlanId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, Page: {Page}. User: {UserName} (Id: {UserId})",
                searchTerm, providerId, planId, isPrimary, isActive, page, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var model = new PatientInsuranceIndexPageViewModel
                {
                    SearchTerm = searchTerm,
                    ProviderId = providerId,
                    PlanId = planId,
                    IsPrimary = isPrimary,
                    IsActive = isActive,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CurrentPage = page,
                    PageSize = PageSize
                };

                // بارگیری داده‌ها
                var result = await _patientInsuranceService.GetPatientInsurancesAsync(
                    patientId: null,
                    searchTerm: searchTerm,
                    pageNumber: page,
                    pageSize: PageSize);

                if (result.Success)
                {
                    // تبدیل PatientInsuranceIndexViewModel به PatientInsuranceIndexItemViewModel
                    model.PatientInsurances = result.Data.Items.Select(item => new PatientInsuranceIndexItemViewModel
                    {
                        PatientInsuranceId = item.PatientInsuranceId,
                        PatientId = item.PatientId,
                        PatientFullName = item.PatientName,
                        PatientCode = item.PatientCode,
                        PatientNationalCode = item.PatientNationalCode,
                        InsurancePlanId = item.InsurancePlanId,
                        PolicyNumber = item.PolicyNumber,
                        InsurancePlanName = item.InsurancePlanName,
                        InsuranceProviderName = item.InsuranceProviderName,
                        InsuranceType = item.InsuranceType,
                        IsPrimary = item.IsPrimary,
                        StartDate = item.StartDate,
                        EndDate = item.EndDate,
                        StartDateShamsi = item.StartDateShamsi,
                        EndDateShamsi = item.EndDateShamsi,
                        IsActive = item.IsActive,
                        CoveragePercent = item.CoveragePercent,
                        CreatedAt = item.CreatedAt,
                        CreatedAtShamsi = item.CreatedAtShamsi,
                        CreatedByUserName = item.CreatedByUserName
                    }).ToList();
                    model.TotalCount = result.Data.TotalItems;
                }

                // بارگیری SelectList ها
                await LoadInsurancePlansAsync();
                
                // تنظیم InsurancePlanSelectList
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    model.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text", planId);
                }
                
                // بارگیری Insurance Providers
                var providersResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (providersResult.Success)
                {
                    // استخراج unique providers از plans
                    var providers = providersResult.Data
                        .GroupBy(p => p.InsuranceProviderName)
                        .Select(g => new { Value = g.First().InsurancePlanId, Text = g.Key })
                        .ToList();
                    
                    model.InsuranceProviderSelectList = new SelectList(providers, "Value", "Text", providerId);
                }

                // تنظیم SelectList های دیگر
                model.PrimaryInsuranceSelectList = PatientInsuranceIndexPageViewModel.CreatePrimaryInsuranceSelectList(isPrimary);
                model.ActiveStatusSelectList = PatientInsuranceIndexPageViewModel.CreateActiveStatusSelectList(isActive);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در نمایش صفحه اصلی بیمه‌های بیماران. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در نمایش صفحه اصلی بیمه‌های بیماران";
                return View(new PatientInsuranceIndexPageViewModel());
            }
        }

        /// <summary>
        /// بارگیری لیست بیمه‌های بیماران با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadPatientInsurances")]
        public async Task<PartialViewResult> LoadPatientInsurances(int? patientId = null, string searchTerm = "", int page = 1)
        {
            _log.Information(
                "درخواست لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                patientId, searchTerm, page, PageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientInsuranceService.GetPatientInsurancesAsync(patientId, searchTerm, page, PageSize);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_PatientInsuranceListPartial", new PagedResult<PatientInsuranceIndexViewModel>());
                }

                _log.Information(
                    "لود بیمه‌های بیماران با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    result.Data.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientInsuranceListPartial", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    patientId, searchTerm, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientInsuranceListPartial", new PagedResult<PatientInsuranceIndexViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات بیمه بیمار
        /// </summary>
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information(
                "درخواست جزئیات بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientInsuranceService.GetPatientInsuranceDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت جزئیات بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "جزئیات بیمه بیمار با موفقیت دریافت شد. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    id, result.Data.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت جزئیات بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت جزئیات بیمه بیمار";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد بیمه بیمار
        /// </summary>
        [HttpGet]
        [Route("Create")]
        public async Task<ActionResult> Create(int? patientId = null)
        {
            _log.Information("بازدید از فرم ایجاد بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var model = new PatientInsuranceCreateEditViewModel
                {
                    PatientId = patientId ?? 0,
                    IsActive = true,
                    StartDate = DateTime.Now,
                    IsPrimary = false
                };

                // بارگیری لیست طرح‌های بیمه
                await LoadInsurancePlansAsync();
                
                // تنظیم SelectList ها
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    model.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text");
                }

                // تنظیم PatientSelectList (خالی برای شروع)
                model.PatientSelectList = new SelectList(new List<object>(), "Value", "Text");

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در نمایش فرم ایجاد بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در نمایش فرم ایجاد بیمه بیمار";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد بیمه بیمار جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<ActionResult> Create(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل بیمه بیمار معتبر نیست. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadInsurancePlansAsync();
                    
                    // تنظیم SelectList ها
                    var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                    if (plansResult.Success)
                    {
                        model.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text", model.InsurancePlanId);
                    }

                    // تنظیم PatientSelectList (خالی برای شروع)
                    model.PatientSelectList = new SelectList(new List<object>(), "Value", "Text");

                    return View(model);
                }

                var result = await _patientInsuranceService.CreatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    
                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadInsurancePlansAsync();

                    return View(model);
                }

                _log.Information(
                    "بیمه بیمار جدید با موفقیت ایجاد شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    result.Data, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار جدید با موفقیت ایجاد شد";
                return RedirectToAction("Details", new { id = result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در ایجاد بیمه بیمار";
                
                // بارگیری مجدد لیست طرح‌های بیمه
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    ViewBag.InsurancePlans = plansResult.Data;
                }

                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش بیمه بیمار
        /// </summary>
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information(
                "درخواست فرم ویرایش بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientInsuranceService.GetPatientInsuranceForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت بیمه بیمار برای ویرایش. PatientInsuranceId: {PatientInsuranceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // بارگیری لیست طرح‌های بیمه
                await LoadInsurancePlansAsync();
                
                // تنظیم SelectList ها
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    result.Data.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text", result.Data.InsurancePlanId);
                }

                // تنظیم PatientSelectList (خالی برای شروع)
                result.Data.PatientSelectList = new SelectList(new List<object>(), "Value", "Text");

                _log.Information(
                    "فرم ویرایش بیمه بیمار با موفقیت دریافت شد. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    id, result.Data.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم ویرایش بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت فرم ویرایش بیمه بیمار";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<ActionResult> Edit(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل بیمه بیمار معتبر نیست. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                        model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadInsurancePlansAsync();
                    
                    // تنظیم SelectList ها
                    var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                    if (plansResult.Success)
                    {
                        model.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text", model.InsurancePlanId);
                    }

                    // تنظیم PatientSelectList (خالی برای شروع)
                    model.PatientSelectList = new SelectList(new List<object>(), "Value", "Text");

                    return View(model);
                }

                var result = await _patientInsuranceService.UpdatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    
                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadInsurancePlansAsync();

                    return View(model);
                }

                _log.Information(
                    "بیمه بیمار با موفقیت به‌روزرسانی شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Details", new { id = model.PatientInsuranceId });
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در به‌روزرسانی بیمه بیمار";
                
                // بارگیری مجدد لیست طرح‌های بیمه
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (plansResult.Success)
                {
                    ViewBag.InsurancePlans = plansResult.Data;
                }

                return View(model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// نمایش فرم تأیید حذف بیمه بیمار
        /// </summary>
        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information(
                "درخواست فرم حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientInsuranceService.GetPatientInsuranceDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در دریافت بیمه بیمار برای حذف. PatientInsuranceId: {PatientInsuranceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "فرم حذف بیمه بیمار با موفقیت دریافت شد. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    id, result.Data.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در دریافت فرم حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت فرم حذف بیمه بیمار";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف نرم بیمه بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Delete")]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information(
                "درخواست حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientInsuranceService.SoftDeletePatientInsuranceAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "بیمه بیمار با موفقیت حذف شد. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار با موفقیت حذف شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در حذف بیمه بیمار";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// بررسی وجود شماره بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckPolicyNumberExists")]
        public async Task<JsonResult> CheckPolicyNumberExists(string policyNumber, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesPolicyNumberExistAsync(policyNumber, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود شماره بیمه. PolicyNumber: {PolicyNumber}", policyNumber);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// بررسی وجود بیمه اصلی برای بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckPrimaryInsuranceExists")]
        public async Task<JsonResult> CheckPrimaryInsuranceExists(int patientId, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesPrimaryInsuranceExistAsync(patientId, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود بیمه اصلی برای بیمار. PatientId: {PatientId}", patientId);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// بررسی تداخل تاریخ بیمه‌های بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CheckDateOverlapExists")]
        public async Task<JsonResult> CheckDateOverlapExists(int patientId, DateTime startDate, DateTime endDate, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesDateOverlapExistAsync(patientId, startDate, endDate, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}, StartDate: {StartDate}, EndDate: {EndDate}", patientId, startDate, endDate);
                return Json(new { exists = false });
            }
        }

        /// <summary>
        /// تنظیم بیمه اصلی بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("SetPrimaryInsurance")]
        public async Task<JsonResult> SetPrimaryInsurance(int patientInsuranceId)
        {
            try
            {
                var result = await _patientInsuranceService.SetPrimaryInsuranceAsync(patientInsuranceId);
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsuranceId);
                return Json(new { success = false, message = "خطا در تنظیم بیمه اصلی بیمار" });
            }
        }

        #endregion
    }
}
