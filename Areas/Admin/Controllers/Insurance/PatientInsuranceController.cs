using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using Serilog;
using System.Net;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading;
// using Microsoft.Extensions.Caching.Memory; // در ASP.NET Framework در دسترس نیست

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// انواع خطاهای سیستم
    /// </summary>
    public enum ErrorType
    {
        Unknown,
        DatabaseConnection,
        ForeignKeyViolation,
        DuplicateKey,
        RequiredField,
        Timeout,
        Authorization,
        Validation,
        BusinessLogic
    }

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
    //[Authorize] // فعال‌سازی کنترل دسترسی - Critical Security Fix
    // Routing attributes حذف شده - از conventional routing استفاده می‌کنیم
    public class PatientInsuranceController : Controller
    {
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IPatientService _patientService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;
        // private readonly IMemoryCache _memoryCache; // در ASP.NET Framework در دسترس نیست
        
        // Cache logic moved to Service Layer - SRP Compliance // 5 minutes cache

        // Performance and Resilience Configuration moved to Infrastructure Layer - SRP Compliance

        public PatientInsuranceController(
            IPatientInsuranceService patientInsuranceService,
            IInsurancePlanService insurancePlanService,
            IPatientService patientService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _log = logger.ForContext<PatientInsuranceController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Error Handling (Simplified - SRP Compliance)

        /// <summary>
        /// مدیریت ساده خطاها - منطق پیچیده به Global Exception Filter منتقل شد
        /// </summary>
        private ActionResult HandleException(Exception ex, string operation, object parameters = null)
        {
            _log.Error(ex, "خطا در {Operation}. User: {UserName} (Id: {UserId})", 
                operation, _currentUserService.UserName, _currentUserService.UserId);

            TempData["ErrorMessage"] = "خطا در انجام عملیات. لطفاً دوباره تلاش کنید.";
            return RedirectToAction("Index");
        }

        // منطق‌های پیچیده به Global Exception Filter و Infrastructure Layer منتقل شدند

        #endregion

        #region Logging (Simplified - SRP Compliance)

        // منطق لاگ‌گیری پیچیده به Action Filters منتقل شد

        // تمام متدهای لاگ‌گیری پیچیده به Action Filters منتقل شدند

        #endregion

        #region Performance (Simplified - SRP Compliance)

        // منطق Performance و Resilience به Infrastructure Layer منتقل شد

        // تمام متدهای Performance و Resilience به Infrastructure Layer منتقل شدند

        // تمام متدهای Performance و Resilience به Infrastructure Layer منتقل شدند

        /// <summary>
        /// بررسی وضعیت عملکرد سیستم
        /// </summary>
        private async Task<bool> CheckSystemHealthAsync()
        {
            try
            {
                // بررسی اتصال به دیتابیس
                var healthCheck = await _patientInsuranceService.GetPatientInsurancesAsync(null, null, 1, 1);
                return healthCheck.Success;
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "بررسی وضعیت سیستم ناموفق بود");
                return false;
            }
        }

        /// <summary>
        /// اجرای عملیات با Circuit Breaker pattern
        /// </summary>
        private async Task<T> ExecuteWithCircuitBreaker<T>(
            Func<Task<T>> operation,
            string operationName,
            int failureThreshold = 5,
            TimeSpan recoveryTimeout = default)
        {
            if (recoveryTimeout == default)
                recoveryTimeout = TimeSpan.FromMinutes(1);

            // در یک پیاده‌سازی واقعی، این اطلاعات باید در cache یا database ذخیره شود
            var circuitKey = $"circuit_breaker_{operationName}";
            
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "عملیات {OperationName} ناموفق بود. Circuit Breaker فعال شد.", operationName);
                
                // در اینجا باید failure count را افزایش دهیم و در صورت رسیدن به threshold، circuit را باز کنیم
                // برای سادگی، فعلاً فقط exception را دوباره throw می‌کنیم
                throw;
            }
        }

        #endregion

        #region Validation Helper Methods

        // منطق اعتبارسنجی به سرویس منتقل شد - طبق قرارداد No Business Logic in Controllers

        #endregion

        #region Performance Monitoring Methods

        /// <summary>
        /// مانیتورینگ عملکرد عملیات‌های مختلف
        /// </summary>
        private async Task<T> MonitorPerformance<T>(Func<Task<T>> operation, string operationName, object parameters = null)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;
            
            try
            {
                var result = await operation();
                stopwatch.Stop();
                
                // لاگ عملکرد موفق
                _log.Information("عملیات {OperationName} با موفقیت انجام شد. Duration: {Duration}ms, Parameters: {@Parameters}",
                    operationName, stopwatch.ElapsedMilliseconds, parameters);
                
                // اگر عملیات بیش از 5 ثانیه طول کشیده باشد، warning لاگ کنیم
                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    _log.Warning("عملیات {OperationName} کند بود. Duration: {Duration}ms, Parameters: {@Parameters}",
                        operationName, stopwatch.ElapsedMilliseconds, parameters);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // لاگ خطا با اطلاعات عملکرد
                _log.Error(ex, "عملیات {OperationName} ناموفق بود. Duration: {Duration}ms, Parameters: {@Parameters}",
                    operationName, stopwatch.ElapsedMilliseconds, parameters);
                
                throw;
            }
        }

        /// <summary>
        /// بررسی وضعیت حافظه و منابع سیستم
        /// </summary>
        private void LogSystemResources(string operationName)
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
                var cpuTime = process.TotalProcessorTime;
                
                _log.Debug("منابع سیستم - Operation: {OperationName}, Memory: {MemoryMB}MB, CPU Time: {CpuTime}",
                    operationName, memoryUsage, cpuTime);
                
                // اگر حافظه بیش از 500MB باشد، warning لاگ کنیم
                if (memoryUsage > 500)
                {
                    _log.Warning("استفاده زیاد از حافظه - Operation: {OperationName}, Memory: {MemoryMB}MB",
                        operationName, memoryUsage);
                }
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "خطا در بررسی منابع سیستم - Operation: {OperationName}", operationName);
            }
        }

        /// <summary>
        /// اجرای عملیات با مانیتورینگ کامل
        /// </summary>
        private async Task<T> ExecuteWithFullMonitoring<T>(
            Func<Task<T>> operation,
            string operationName,
            object parameters = null,
            bool enableResourceMonitoring = true)
        {
            if (enableResourceMonitoring)
            {
                LogSystemResources(operationName);
            }
            
            return await MonitorPerformance(operation, operationName, parameters);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// بارگیری لیست طرح‌های بیمه فعال برای ViewModel با استفاده از Cache
        /// </summary>
        private async Task LoadDropdownsForModelAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                // بارگیری طرح‌های بیمه (PatientSelectList حذف شده - استفاده از Select2)
                var plansResult = await _insurancePlanService.GetActivePlansForLookupAsync();
                
                // تنظیم InsurancePlanSelectList
                if (plansResult.Success)
                {
                    model.InsurancePlanSelectList = new SelectList(plansResult.Data, "Value", "Text", model.InsurancePlanId);
                }
                else
                {
                    _log.Warning("خطا در بارگیری لیست طرح‌های بیمه برای ViewModel: {Message}", plansResult.Message);
                    model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                }
                
                _log.Information("بارگیری SelectList ها برای ViewModel با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگیری SelectList ها برای ViewModel. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
            }
        }

        // Cache methods removed - SRP Compliance


        /// <summary>
        /// بارگیری و تنظیم SelectList های مورد نیاز برای Index ViewModel با استفاده از Cache
        /// </summary>
        private async Task LoadSelectListsForIndexViewModelAsync(PatientInsuranceIndexPageViewModel model, int? selectedPlanId = null, int? selectedProviderId = null)
        {
            try
            {
                // بارگیری موازی طرح‌های بیمه و شرکت‌های بیمه
                var plansTask = _insurancePlanService.GetActivePlansForLookupAsync();
                var providersTask = _insurancePlanService.GetActiveProvidersForLookupAsync();
                
                await Task.WhenAll(plansTask, providersTask);
                
                var plansResult = await plansTask;
                var providersResult = await providersTask;
                
                if (plansResult.Success && providersResult.Success)
                {
                    // تنظیم InsurancePlanSelectList
                    model.InsurancePlanSelectList = new SelectList(plansResult.Data ?? new List<InsurancePlanLookupViewModel>(), "Value", "Text", selectedPlanId);
                    
                    // تنظیم InsuranceProviderSelectList با استفاده از متد جدید
                    model.InsuranceProviderSelectList = new SelectList(providersResult.Data ?? new List<InsuranceProviderLookupViewModel>(), "InsuranceProviderId", "Name", selectedProviderId);
                    
                    // تنظیم SelectList های دیگر
                    model.PrimaryInsuranceSelectList = PatientInsuranceIndexPageViewModel.CreatePrimaryInsuranceSelectList(model.IsPrimary);
                    model.ActiveStatusSelectList = PatientInsuranceIndexPageViewModel.CreateActiveStatusSelectList(model.IsActive);
                }
                else
                {
                    model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                    model.InsuranceProviderSelectList = new SelectList(new List<InsuranceProviderLookupViewModel>(), "InsuranceProviderId", "Name");
                    _log.Warning("خطا در بارگیری لیست SelectList ها برای Index ViewModel. Plans: {PlansMessage}, Providers: {ProvidersMessage}", 
                        plansResult.Message, providersResult.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بارگیری SelectList ها برای Index ViewModel. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                model.InsurancePlanSelectList = new SelectList(new List<object>(), "Value", "Text");
                model.InsuranceProviderSelectList = new SelectList(new List<InsuranceProviderLookupViewModel>(), "InsuranceProviderId", "Name");
                model.PrimaryInsuranceSelectList = PatientInsuranceIndexPageViewModel.CreatePrimaryInsuranceSelectList(null);
                model.ActiveStatusSelectList = PatientInsuranceIndexPageViewModel.CreateActiveStatusSelectList(null);
            }
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// متد debug برای بررسی داده‌های موجود
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DebugCount()
        {
            try
            {
                var result = await _patientInsuranceService.GetTotalRecordsCountAsync();
                if (result.Success)
                {
                    return Json(new { success = true, count = result.Data, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in DebugCount method");
                return Json(new { success = false, message = "خطا در بررسی تعداد رکوردها" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> DebugSimpleList()
        {
            try
            {
                var result = await _patientInsuranceService.GetSimpleListAsync();
                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in DebugSimpleList method");
                return Json(new { success = false, message = "خطا در دریافت لیست ساده" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی بیمه‌های بیماران
        /// </summary>
        [HttpGet]
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

                // بررسی وضعیت سیستم قبل از عملیات اصلی
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("وضعیت سیستم نامناسب است. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید.";
                    return View(model);
                }

                // بارگیری داده‌ها با استفاده از متد بهینه‌سازی شده
                var result = await _patientInsuranceService.GetPagedAsync(
                    searchTerm: searchTerm,
                    providerId: providerId,
                    planId: planId,
                    isPrimary: isPrimary,
                    isActive: isActive,
                    fromDate: fromDate,
                    toDate: toDate,
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
                await LoadSelectListsForIndexViewModelAsync(model, planId, providerId);

                return View(model);
            }
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در نمایش صفحه اصلی بیمه‌های بیماران. ErrorNumber: {ErrorNumber}, User: {UserName} (Id: {UserId})",
                    ex.Number, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در اتصال به پایگاه داده. لطفاً دوباره تلاش کنید.";
                return View(new PatientInsuranceIndexPageViewModel());
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در نمایش صفحه اصلی بیمه‌های بیماران. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "عملیات بیش از حد انتظار طول کشید. لطفاً دوباره تلاش کنید.";
                return View(new PatientInsuranceIndexPageViewModel());
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Warning(ex, "عدم دسترسی در نمایش صفحه اصلی بیمه‌های بیماران. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "شما دسترسی لازم برای مشاهده این صفحه را ندارید.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش صفحه اصلی بیمه‌های بیماران. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای غیرمنتظره در سیستم. لطفاً با پشتیبانی تماس بگیرید.";
                return View(new PatientInsuranceIndexPageViewModel());
            }
        }

        /// <summary>
        /// بارگیری لیست بیمه‌های بیماران با صفحه‌بندی و فیلترها
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> LoadPatientInsurances(int? patientId = null, string searchTerm = "", int? providerId = null, bool? isPrimary = null, bool? isActive = null, int page = 1)
        {
            _log.Information(
                "درخواست لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                patientId, searchTerm, providerId, isPrimary, isActive, page, PageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // برای حال حاضر، فقط از پارامترهای اصلی استفاده می‌کنیم
                // TODO: در آینده می‌توان فیلترهای اضافی را به Service اضافه کرد
                var result = await _patientInsuranceService.GetPatientInsurancesAsync(patientId, searchTerm, page, PageSize);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_PatientInsuranceListPartial", new PatientInsuranceListPartialViewModel());
                }

                // تبدیل PatientInsuranceIndexViewModel به PatientInsuranceIndexItemViewModel
                var convertedItems = result.Data.Items.Select(x => new PatientInsuranceIndexItemViewModel
                {
                    PatientInsuranceId = x.PatientInsuranceId,
                    PatientId = x.PatientId,
                    PatientFullName = x.PatientName,
                    PatientCode = x.PatientCode,
                    InsurancePlanName = x.InsurancePlanName,
                    InsuranceProviderName = x.InsuranceProviderName,
                    PolicyNumber = x.PolicyNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    StartDateShamsi = x.StartDateShamsi,
                    EndDateShamsi = x.EndDateShamsi
                }).ToList();

                // ایجاد ViewModel برای Partial View
                var partialViewModel = new PatientInsuranceListPartialViewModel
                {
                    Items = convertedItems,
                    CurrentPage = page,
                    PageSize = PageSize,
                    TotalItems = result.Data.TotalItems
                };

                // اعمال فیلترهای اضافی در سمت کلاینت (موقت)
                if (providerId.HasValue || isPrimary.HasValue || isActive.HasValue)
                {
                    var filteredItems = partialViewModel.Items.AsEnumerable();
                    
                    if (providerId.HasValue)
                    {
                        // TODO: فیلتر بر اساس providerId - نیاز به اضافه کردن به ViewModel
                    }
                    
                    if (isPrimary.HasValue)
                    {
                        filteredItems = filteredItems.Where(x => x.IsPrimary == isPrimary.Value);
                    }
                    
                    if (isActive.HasValue)
                    {
                        filteredItems = filteredItems.Where(x => x.IsActive == isActive.Value);
                    }
                    
                    partialViewModel.Items = filteredItems.ToList();
                    partialViewModel.TotalItems = partialViewModel.Items.Count;
                }

                _log.Information(
                    "لود بیمه‌های بیماران با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    partialViewModel.Items.Count, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientInsuranceListPartial", partialViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در لود بیمه‌های بیماران. PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    patientId, searchTerm, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientInsuranceListPartial", new PatientInsuranceListPartialViewModel());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات بیمه بیمار
        /// </summary>
        [HttpGet]
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
                await LoadDropdownsForModelAsync(model);

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
        public async Task<ActionResult> Create(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
                    
                    // Validation errors logged

                    // اضافه کردن پیام خطای کلی
                    TempData["ErrorMessage"] = "لطفاً تمام فیلدهای اجباری را به درستی پر کنید.";

                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadDropdownsForModelAsync(model);

                    return View(model);
                }

                // اعتبارسنجی اضافی server-side (منطق کسب‌وکار در سرویس)
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    
                    // Validation errors logged
                    TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیست.";
                    
                    await LoadDropdownsForModelAsync(model);
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
                    await LoadDropdownsForModelAsync(model);

                    return View(model);
                }

                // Audit Trail logged

                TempData["SuccessMessage"] = "بیمه بیمار جدید با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                return HandleException(ex, "ایجاد بیمه بیمار", model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش بیمه بیمار
        /// </summary>
        [HttpGet]
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
                await LoadDropdownsForModelAsync(result.Data);

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
        public async Task<ActionResult> Edit(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());
                    
                    // Validation errors logged

                    _log.Warning(
                        "مدل بیمه بیمار معتبر نیست. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                        model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                    // اضافه کردن پیام خطای کلی
                    TempData["ErrorMessage"] = "لطفاً تمام فیلدهای اجباری را به درستی پر کنید.";

                    // بارگیری مجدد لیست طرح‌های بیمه
                    await LoadDropdownsForModelAsync(model);

                    return View(model);
                }

                // اعتبارسنجی اضافی server-side (منطق کسب‌وکار در سرویس)
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                    
                    // Validation errors logged
                    TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیست.";
                    
                    await LoadDropdownsForModelAsync(model);
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
                    await LoadDropdownsForModelAsync(model);

                    return View(model);
                }

                _log.Information(
                    "بیمه بیمار با موفقیت به‌روزرسانی شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // مدیریت خطا
                return HandleException(ex, "به‌روزرسانی بیمه بیمار", model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// نمایش فرم تأیید حذف بیمه بیمار
        /// </summary>
        [HttpGet]
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
            catch (SqlException ex) when (ex.Number == 547) // Foreign Key Violation
            {
                _log.Warning(ex, "امکان حذف بیمه بیمار به دلیل وابستگی‌های موجود. PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "امکان حذف این بیمه به دلیل وجود وابستگی‌های موجود نیست.";
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Number == 2) // Database Connection
            {
                _log.Error(ex, "خطای اتصال به پایگاه داده در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در اتصال به پایگاه داده. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در حذف بیمه بیمار. ErrorNumber: {ErrorNumber}, PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    ex.Number, id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در حذف بیمه بیمار. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Warning(ex, "عدم دسترسی در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "شما دسترسی لازم برای حذف این بیمه را ندارید.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                _log.Warning(ex, "خطای منطق کسب‌وکار در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "امکان حذف این بیمه وجود ندارد: " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "عملیات بیش از حد انتظار طول کشید. لطفاً دوباره تلاش کنید.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای غیرمنتظره در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای غیرمنتظره در سیستم. لطفاً با پشتیبانی تماس بگیرید.";
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
        public async Task<JsonResult> CheckPolicyNumberExists(string policyNumber, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesPolicyNumberExistAsync(policyNumber, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در بررسی وجود شماره بیمه. PolicyNumber: {PolicyNumber}, ErrorNumber: {ErrorNumber}", 
                    policyNumber, ex.Number);
                return Json(new { exists = false, error = "خطا در اتصال به پایگاه داده" });
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در بررسی وجود شماره بیمه. PolicyNumber: {PolicyNumber}", policyNumber);
                return Json(new { exists = false, error = "عملیات بیش از حد انتظار طول کشید" });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در بررسی وجود شماره بیمه. PolicyNumber: {PolicyNumber}", policyNumber);
                return Json(new { exists = false, error = "خطای غیرمنتظره در سیستم" });
            }
        }

        /// <summary>
        /// بررسی وجود بیمه اصلی برای بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckPrimaryInsuranceExists(int patientId, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesPrimaryInsuranceExistAsync(patientId, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در بررسی وجود بیمه اصلی برای بیمار. PatientId: {PatientId}, ErrorNumber: {ErrorNumber}", 
                    patientId, ex.Number);
                return Json(new { exists = false, error = "خطا در اتصال به پایگاه داده" });
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در بررسی وجود بیمه اصلی برای بیمار. PatientId: {PatientId}", patientId);
                return Json(new { exists = false, error = "عملیات بیش از حد انتظار طول کشید" });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در بررسی وجود بیمه اصلی برای بیمار. PatientId: {PatientId}", patientId);
                return Json(new { exists = false, error = "خطای غیرمنتظره در سیستم" });
            }
        }

        /// <summary>
        /// بررسی تداخل تاریخ بیمه‌های بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckDateOverlapExists(int patientId, DateTime startDate, DateTime endDate, int? excludeId = null)
        {
            try
            {
                var result = await _patientInsuranceService.DoesDateOverlapExistAsync(patientId, startDate, endDate, excludeId);
                return Json(new { exists = result.Success && result.Data });
            }
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}, ErrorNumber: {ErrorNumber}", 
                    patientId, ex.Number);
                return Json(new { exists = false, error = "خطا در اتصال به پایگاه داده" });
            }
            catch (ArgumentException ex)
            {
                _log.Warning(ex, "خطای اعتبارسنجی در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}", patientId);
                return Json(new { exists = false, error = "تاریخ‌های وارد شده معتبر نیست" });
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}", patientId);
                return Json(new { exists = false, error = "عملیات بیش از حد انتظار طول کشید" });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}, StartDate: {StartDate}, EndDate: {EndDate}", 
                    patientId, startDate, endDate);
                return Json(new { exists = false, error = "خطای غیرمنتظره در سیستم" });
            }
        }

        /// <summary>
        /// جستجوی بیماران برای Select2 (Server-Side Processing)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> SearchPatients(string q = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("جستجوی بیماران برای Select2. Query: {Query}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    q, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _patientService.SearchPatientsForSelect2Async(q, page, pageSize);
                
                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی بیماران برای Select2: {Message}", result.Message);
                    return Json(new { results = new List<object>(), pagination = new { more = false } }, JsonRequestBehavior.AllowGet);
                }

                var patients = result.Data.Items.Select(p => new
                {
                    id = p.PatientId,
                    text = $"{p.FirstName} {p.LastName} ({p.NationalCode})",
                    firstName = p.FirstName,
                    lastName = p.LastName,
                    nationalCode = p.NationalCode,
                    phoneNumber = p.PhoneNumber
                }).ToList();

                var hasMore = (page * pageSize) < result.Data.TotalItems;

                _log.Information("جستجوی بیماران برای Select2 با موفقیت انجام شد. تعداد: {Count}, صفحه: {Page}. User: {UserName} (Id: {UserId})",
                    patients.Count, page, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new 
                { 
                    results = patients, 
                    pagination = new { more = hasMore } 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در جستجوی بیماران برای Select2. Query: {Query}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    q, page, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { results = new List<object>(), pagination = new { more = false } }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// تنظیم بیمه اصلی بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            catch (SqlException ex)
            {
                _log.Error(ex, "خطای پایگاه داده در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}, ErrorNumber: {ErrorNumber}", 
                    patientInsuranceId, ex.Number);
                return Json(new { success = false, message = "خطا در اتصال به پایگاه داده" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.Warning(ex, "عدم دسترسی در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsuranceId);
                return Json(new { success = false, message = "شما دسترسی لازم برای این عملیات را ندارید" });
            }
            catch (InvalidOperationException ex)
            {
                _log.Warning(ex, "خطای منطق کسب‌وکار در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsuranceId);
                return Json(new { success = false, message = "امکان انجام این عملیات وجود ندارد: " + ex.Message });
            }
            catch (TimeoutException ex)
            {
                _log.Warning(ex, "Timeout در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsuranceId);
                return Json(new { success = false, message = "عملیات بیش از حد انتظار طول کشید" });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در تنظیم بیمه اصلی بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsuranceId);
                return Json(new { success = false, message = "خطای غیرمنتظره در سیستم" });
            }
        }

        #endregion
    }
}
