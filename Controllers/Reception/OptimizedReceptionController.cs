using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels.Validators;
using FluentValidation;
using Serilog;
using Microsoft.AspNet.Identity;
using ClinicApp.Services;
using ClinicApp.Models;
using System.Data.Entity;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.DTOs.Insurance;

namespace ClinicApp.Controllers.Reception
{
    /// <summary>
    /// کنترلر بهینه‌سازی شده مدیریت پذیرش‌های بیماران
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل پذیرش‌های بیماران (Normal, Emergency, Special, Online)
    /// 2. پشتیبانی از AJAX endpoints برای UI تعاملی
    /// 3. جستجوی سریع بیماران (10k+ بیمار)
    /// 4. مدیریت Lookup Lists (دسته‌بندی‌ها، خدمات، پزشکان)
    /// 5. محاسبات بیمه و پرداخت
    /// 6. استعلام کمکی خارجی (شبکه شمس)
    /// 7. امنیت بالا و مدیریت خطا
    /// 8. لاگ‌گیری حرفه‌ای
    /// 9. رعایت استانداردهای پزشکی ایران
    /// 10. یکپارچه‌سازی با سیستم‌های موجود
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت HTTP requests
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    [Authorize(Roles = "Receptionist,Admin")]
    [RequireHttps] // Force HTTPS in production
    public class OptimizedReceptionController : BaseController
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly IReceptionBusinessRules _businessRules;
        private readonly IReceptionSecurityService _securityService;
        private readonly IReceptionCacheService _cacheService;
        private readonly IReceptionPerformanceOptimizer _performanceOptimizer;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IPatientInsuranceValidationService _patientInsuranceValidationService;
        private readonly IPatientInsuranceManagementService _patientInsuranceManagementService;

        public OptimizedReceptionController(
            IReceptionService receptionService,
            IReceptionBusinessRules businessRules,
            IReceptionSecurityService securityService,
            IReceptionCacheService cacheService,
            IReceptionPerformanceOptimizer performanceOptimizer,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger,
            IServiceCalculationService serviceCalculationService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IPatientInsuranceService patientInsuranceService,
            IPatientInsuranceValidationService patientInsuranceValidationService,
            IPatientInsuranceManagementService patientInsuranceManagementService) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _businessRules = businessRules ?? throw new ArgumentNullException(nameof(businessRules));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _performanceOptimizer = performanceOptimizer ?? throw new ArgumentNullException(nameof(performanceOptimizer));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _patientInsuranceValidationService = patientInsuranceValidationService ?? throw new ArgumentNullException(nameof(patientInsuranceValidationService));
            _patientInsuranceManagementService = patientInsuranceManagementService ?? throw new ArgumentNullException(nameof(patientInsuranceManagementService));
        }

        #endregion

        #region Main Views

        /// <summary>
        /// صفحه اصلی پذیرش
        /// </summary>
        /// <returns>صفحه پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("ورود به صفحه اصلی پذیرش. کاربر: {UserName}", _currentUserService.UserName);

                // Security check
                var canView = await _securityService.CanViewReceptionsListAsync(_currentUserService.UserId);
                if (!canView)
                {
                    _logger.Warning("کاربر {UserName} مجوز مشاهده لیست پذیرش‌ها را ندارد", _currentUserService.UserName);
                    return RedirectToAction("AccessDenied", "Account");
                }

                var model = new ReceptionSearchViewModel
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(7),
                    Status = ReceptionStatus.Pending
                };

                // Get lookup lists from cache
                var lookupLists = await _cacheService.GetLookupListsFromCacheAsync();
                if (lookupLists != null)
                {
                    model.Doctors = lookupLists.Doctors;
                    model.Patients = lookupLists.Patients;
                    model.Services = lookupLists.Services;
                    model.ServiceCategories = lookupLists.ServiceCategories;
                    model.PaymentMethods = lookupLists.PaymentMethods;
                    model.InsuranceProviders = lookupLists.InsuranceProviders;
                }
                else
                {
                    // Load from database if not in cache
                    model = await LoadLookupListsAsync(model);
                }

                _logger.Information("صفحه اصلی پذیرش با موفقیت بارگذاری شد");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه اصلی پذیرش");
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه ایجاد پذیرش
        /// </summary>
        /// <returns>صفحه ایجاد پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                _logger.Information("ورود به صفحه ایجاد پذیرش. کاربر: {UserName}", _currentUserService.UserName);

                // Security check
                var canCreate = await _securityService.CanCreateReceptionAsync(_currentUserService.UserId);
                if (!canCreate)
                {
                    _logger.Warning("کاربر {UserName} مجوز ایجاد پذیرش را ندارد", _currentUserService.UserName);
                    return RedirectToAction("AccessDenied", "Account");
                }

                var model = new ReceptionCreateViewModel
                {
                    ReceptionDate = DateTime.Now,
                    Status = ReceptionStatus.Pending,
                    Type = ReceptionType.Normal,
                    Priority = ReceptionPriority.Normal,
                    IsEmergency = false,
                    IsOnlineReception = false,
                    SelectedServiceIds = new List<int>()
                };

                // Get lookup lists from cache
                var lookupLists = await _cacheService.GetLookupListsFromCacheAsync();
                if (lookupLists != null)
                {
                    model.Doctors = lookupLists.Doctors;
                    model.Patients = lookupLists.Patients;
                    model.Services = lookupLists.Services;
                    model.ServiceCategories = lookupLists.ServiceCategories;
                    model.PaymentMethods = lookupLists.PaymentMethods;
                    model.InsuranceProviders = lookupLists.InsuranceProviders;
                }
                else
                {
                    // Load from database if not in cache
                    model = await LoadLookupListsAsync(model);
                }

                _logger.Information("صفحه ایجاد پذیرش با موفقیت بارگذاری شد");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه ایجاد پذیرش");
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه ویرایش پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>صفحه ویرایش پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _logger.Information("ورود به صفحه ویرایش پذیرش. شناسه: {Id}, کاربر: {UserName}", id, _currentUserService.UserName);

                // Security check
                var canEdit = await _securityService.CanEditReceptionAsync(_currentUserService.UserId, id);
                if (!canEdit)
                {
                    _logger.Warning("کاربر {UserName} مجوز ویرایش پذیرش {Id} را ندارد", _currentUserService.UserName, id);
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Get reception details from cache or database
                var receptionResult = await _receptionService.GetReceptionDetailsAsync(id);
                if (!receptionResult.Success)
                {
                    _logger.Warning("پذیرش با شناسه {Id} یافت نشد", id);
                    return HttpNotFound("پذیرش مورد نظر یافت نشد");
                }

                var reception = receptionResult.Data;
                var model = new ReceptionEditViewModel
                {
                    ReceptionId = reception.ReceptionId,
                    PatientId = reception.PatientId,
                    DoctorId = reception.DoctorId,
                    ReceptionDate = reception.ReceptionDate,
                    Status = reception.Status,
                    Type = reception.Type,
                    Priority = reception.Priority,
                    IsEmergency = reception.IsEmergency,
                    IsOnlineReception = reception.IsOnlineReception,
                    Notes = reception.Notes,
                    SelectedServiceIds = reception.ReceptionItems?.Select(ri => ri.ServiceId).ToList() ?? new List<int>()
                };

                // Get lookup lists from cache
                var lookupLists = await _cacheService.GetLookupListsFromCacheAsync();
                if (lookupLists != null)
                {
                    model.Doctors = lookupLists.Doctors;
                    model.Patients = lookupLists.Patients;
                    model.Services = lookupLists.Services;
                    model.ServiceCategories = lookupLists.ServiceCategories;
                    model.PaymentMethods = lookupLists.PaymentMethods;
                    model.InsuranceProviders = lookupLists.InsuranceProviders;
                }
                else
                {
                    // Load from database if not in cache
                    model = await LoadLookupListsAsync(model);
                }

                _logger.Information("صفحه ویرایش پذیرش با موفقیت بارگذاری شد");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه ویرایش پذیرش. شناسه: {Id}", id);
                return View("Error");
            }
        }

        /// <summary>
        /// صفحه جزئیات پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>صفحه جزئیات پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("ورود به صفحه جزئیات پذیرش. شناسه: {Id}, کاربر: {UserName}", id, _currentUserService.UserName);

                // Security check
                var canView = await _securityService.CanViewReceptionAsync(_currentUserService.UserId, id);
                if (!canView)
                {
                    _logger.Warning("کاربر {UserName} مجوز مشاهده پذیرش {Id} را ندارد", _currentUserService.UserName, id);
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Get reception details from cache or database
                var receptionResult = await _receptionService.GetReceptionDetailsAsync(id);
                if (!receptionResult.Success)
                {
                    _logger.Warning("پذیرش با شناسه {Id} یافت نشد", id);
                    return HttpNotFound("پذیرش مورد نظر یافت نشد");
                }

                var model = receptionResult.Data;
                _logger.Information("صفحه جزئیات پذیرش با موفقیت بارگذاری شد");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری صفحه جزئیات پذیرش. شناسه: {Id}", id);
                return View("Error");
            }
        }

        #endregion

        #region AJAX Endpoints

        /// <summary>
        /// جستجوی پذیرش‌ها (AJAX)
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <returns>نتیجه جستجو</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SearchReceptions(ReceptionSearchViewModel model)
        {
            try
            {
                _logger.Information("جستجوی پذیرش‌ها. معیارها: {@Model}, کاربر: {UserName}", model, _currentUserService.UserName);

                // Security check
                var canView = await _securityService.CanViewReceptionsListAsync(_currentUserService.UserId);
                if (!canView)
                {
                    _logger.Warning("کاربر {UserName} مجوز مشاهده لیست پذیرش‌ها را ندارد", _currentUserService.UserName);
                    return Json(new { success = false, message = "شما مجوز مشاهده لیست پذیرش‌ها را ندارید" });
                }

                // Input validation
                var validationResult = await _securityService.ValidateReceptionInputAsync(model.SearchTerm ?? "");
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی ورودی جستجو ناموفق. خطاها: {@Errors}", validationResult.Errors);
                    return Json(new { success = false, message = "ورودی نامعتبر است", errors = validationResult.Errors });
                }

                // Search receptions
                var searchResult = await _receptionService.SearchReceptionsAsync(model);
                if (!searchResult.Success)
                {
                    _logger.Warning("جستجوی پذیرش‌ها ناموفق. خطا: {Error}", searchResult.Message);
                    return Json(new { success = false, message = searchResult.Message });
                }

                var result = new
                {
                    success = true,
                    data = searchResult.Data,
                    totalCount = searchResult.Data?.TotalCount ?? 0,
                    pageCount = searchResult.Data?.PageCount ?? 0,
                    currentPage = searchResult.Data?.CurrentPage ?? 0
                };

                _logger.Information("جستجوی پذیرش‌ها موفق. تعداد: {Count}", result.totalCount);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها");
                return Json(new { success = false, message = "خطا در جستجوی پذیرش‌ها" });
            }
        }

        /// <summary>
        /// ایجاد پذیرش (AJAX)
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه ایجاد</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateReception(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Information("ایجاد پذیرش جدید. بیمار: {PatientId}, خدمت: {ServiceId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.SelectedServiceIds?.Count ?? 0, model?.DoctorId, _currentUserService.UserName);

                // Security check
                var canCreate = await _securityService.CanCreateReceptionAsync(_currentUserService.UserId);
                if (!canCreate)
                {
                    _logger.Warning("کاربر {UserName} مجوز ایجاد پذیرش را ندارد", _currentUserService.UserName);
                    return Json(new { success = false, message = "شما مجوز ایجاد پذیرش را ندارید" });
                }

                // Business rules validation
                var businessRulesResult = await _businessRules.ValidateReceptionAsync(model);
                if (!businessRulesResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی قوانین کسب‌وکار ناموفق. خطاها: {@Errors}", businessRulesResult.Errors);
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است", errors = businessRulesResult.Errors });
                }

                // Create reception
                var createResult = await _receptionService.CreateReceptionAsync(model);
                if (!createResult.Success)
                {
                    _logger.Warning("ایجاد پذیرش ناموفق. خطا: {Error}", createResult.Message);
                    return Json(new { success = false, message = createResult.Message });
                }

                // Clear related caches
                await _cacheService.ClearReceptionsCacheAsync(new ReceptionSearchCriteria());
                await _cacheService.ClearStatisticsCacheAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "CreateReception", $"ReceptionId:{createResult.Data.ReceptionId}", true);

                var result = new
                {
                    success = true,
                    message = "پذیرش با موفقیت ایجاد شد",
                    data = new
                    {
                        receptionId = createResult.Data.ReceptionId,
                        receptionNumber = createResult.Data.ReceptionNumber,
                        redirectUrl = Url.Action("Details", new { id = createResult.Data.ReceptionId })
                    }
                };

                _logger.Information("ایجاد پذیرش موفق. شناسه: {ReceptionId}", createResult.Data.ReceptionId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پذیرش");
                return Json(new { success = false, message = "خطا در ایجاد پذیرش" });
            }
        }

        /// <summary>
        /// ویرایش پذیرش (AJAX)
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه ویرایش</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditReception(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Information("ویرایش پذیرش. شناسه: {Id}, بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.ReceptionId, model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                // Security check
                var canEdit = await _securityService.CanEditReceptionAsync(_currentUserService.UserId, model.ReceptionId);
                if (!canEdit)
                {
                    _logger.Warning("کاربر {UserName} مجوز ویرایش پذیرش {Id} را ندارد", _currentUserService.UserName, model.ReceptionId);
                    return Json(new { success = false, message = "شما مجوز ویرایش این پذیرش را ندارید" });
                }

                // Business rules validation
                var businessRulesResult = await _businessRules.ValidateReceptionEditAsync(model);
                if (!businessRulesResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی قوانین کسب‌وکار ناموفق. خطاها: {@Errors}", businessRulesResult.Errors);
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است", errors = businessRulesResult.Errors });
                }

                // Update reception
                var updateResult = await _receptionService.UpdateReceptionAsync(model);
                if (!updateResult.Success)
                {
                    _logger.Warning("ویرایش پذیرش ناموفق. خطا: {Error}", updateResult.Message);
                    return Json(new { success = false, message = updateResult.Message });
                }

                // Clear related caches
                await _cacheService.ClearReceptionDetailsCacheAsync(model.ReceptionId);
                await _cacheService.ClearReceptionsCacheAsync(new ReceptionSearchCriteria());
                await _cacheService.ClearStatisticsCacheAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "EditReception", $"ReceptionId:{model.ReceptionId}", true);

                var result = new
                {
                    success = true,
                    message = "پذیرش با موفقیت ویرایش شد",
                    data = new
                    {
                        receptionId = model.ReceptionId,
                        redirectUrl = Url.Action("Details", new { id = model.ReceptionId })
                    }
                };

                _logger.Information("ویرایش پذیرش موفق. شناسه: {ReceptionId}", model.ReceptionId);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پذیرش. شناسه: {Id}", model?.ReceptionId);
                return Json(new { success = false, message = "خطا در ویرایش پذیرش" });
            }
        }

        /// <summary>
        /// حذف پذیرش (AJAX)
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه حذف</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteReception(int id)
        {
            try
            {
                _logger.Information("حذف پذیرش. شناسه: {Id}, کاربر: {UserName}", id, _currentUserService.UserName);

                // Security check
                var canDelete = await _securityService.CanDeleteReceptionAsync(_currentUserService.UserId, id);
                if (!canDelete)
                {
                    _logger.Warning("کاربر {UserName} مجوز حذف پذیرش {Id} را ندارد", _currentUserService.UserName, id);
                    return Json(new { success = false, message = "شما مجوز حذف این پذیرش را ندارید" });
                }

                // Delete reception
                var deleteResult = await _receptionService.DeleteReceptionAsync(id);
                if (!deleteResult.Success)
                {
                    _logger.Warning("حذف پذیرش ناموفق. خطا: {Error}", deleteResult.Message);
                    return Json(new { success = false, message = deleteResult.Message });
                }

                // Clear related caches
                await _cacheService.ClearReceptionDetailsCacheAsync(id);
                await _cacheService.ClearReceptionsCacheAsync(new ReceptionSearchCriteria());
                await _cacheService.ClearStatisticsCacheAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "DeleteReception", $"ReceptionId:{id}", true);

                var result = new
                {
                    success = true,
                    message = "پذیرش با موفقیت حذف شد"
                };

                _logger.Information("حذف پذیرش موفق. شناسه: {Id}", id);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پذیرش. شناسه: {Id}", id);
                return Json(new { success = false, message = "خطا در حذف پذیرش" });
            }
        }

        #endregion

        #region Lookup Endpoints

        /// <summary>
        /// دریافت لیست پزشکان (AJAX)
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        [HttpGet]
        public async Task<JsonResult> GetDoctors()
        {
            try
            {
                _logger.Debug("دریافت لیست پزشکان. کاربر: {UserName}", _currentUserService.UserName);

                // Try to get from cache first
                var cachedDoctors = await _cacheService.GetDoctorsFromCacheAsync();
                if (cachedDoctors != null)
                {
                    _logger.Debug("دریافت لیست پزشکان از Cache موفق. تعداد: {Count}", cachedDoctors.Count);
                    return Json(new { success = true, data = cachedDoctors }, JsonRequestBehavior.AllowGet);
                }

                // Load from database
                var doctorsResult = await _receptionService.GetDoctorsAsync();
                if (!doctorsResult.Success)
                {
                    _logger.Warning("دریافت لیست پزشکان ناموفق. خطا: {Error}", doctorsResult.Message);
                    return Json(new { success = false, message = doctorsResult.Message });
                }

                // Cache the results
                await _cacheService.CacheDoctorsAsync(doctorsResult.Data);

                _logger.Debug("دریافت لیست پزشکان از Database موفق. تعداد: {Count}", doctorsResult.Data.Count);
                return Json(new { success = true, data = doctorsResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان" });
            }
        }

        /// <summary>
        /// دریافت لیست بیماران (AJAX)
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست بیماران</returns>
        [HttpGet]
        public async Task<JsonResult> GetPatients(string searchTerm = "")
        {
            try
            {
                _logger.Debug("دریافت لیست بیماران. عبارت جستجو: {SearchTerm}, کاربر: {UserName}", searchTerm, _currentUserService.UserName);

                // Input validation
                var validationResult = await _securityService.ValidatePatientInputAsync(searchTerm);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی ورودی جستجوی بیماران ناموفق. خطاها: {@Errors}", validationResult.Errors);
                    return Json(new { success = false, message = "ورودی نامعتبر است" });
                }

                // Search patients
                var patientsResult = await _receptionService.SearchPatientsAsync(searchTerm);
                if (!patientsResult.Success)
                {
                    _logger.Warning("جستجوی بیماران ناموفق. خطا: {Error}", patientsResult.Message);
                    return Json(new { success = false, message = patientsResult.Message });
                }

                _logger.Debug("دریافت لیست بیماران موفق. تعداد: {Count}", patientsResult.Data.Count);
                return Json(new { success = true, data = patientsResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست بیماران");
                return Json(new { success = false, message = "خطا در دریافت لیست بیماران" });
            }
        }

        /// <summary>
        /// دریافت لیست خدمات (AJAX)
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        [HttpGet]
        public async Task<JsonResult> GetServices(int categoryId = 0)
        {
            try
            {
                _logger.Debug("دریافت لیست خدمات. دسته‌بندی: {CategoryId}, کاربر: {UserName}", categoryId, _currentUserService.UserName);

                // Try to get from cache first
                var cachedServices = await _cacheService.GetServicesFromCacheAsync(categoryId);
                if (cachedServices != null)
                {
                    _logger.Debug("دریافت لیست خدمات از Cache موفق. تعداد: {Count}", cachedServices.Count);
                    return Json(new { success = true, data = cachedServices }, JsonRequestBehavior.AllowGet);
                }

                // Load from database
                var servicesResult = await _receptionService.GetServicesByCategoryAsync(categoryId);
                if (!servicesResult.Success)
                {
                    _logger.Warning("دریافت لیست خدمات ناموفق. خطا: {Error}", servicesResult.Message);
                    return Json(new { success = false, message = servicesResult.Message });
                }

                // Cache the results
                await _cacheService.CacheServicesAsync(categoryId, servicesResult.Data);

                _logger.Debug("دریافت لیست خدمات از Database موفق. تعداد: {Count}", servicesResult.Data.Count);
                return Json(new { success = true, data = servicesResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست خدمات. دسته‌بندی: {CategoryId}", categoryId);
                return Json(new { success = false, message = "خطا در دریافت لیست خدمات" });
            }
        }

        #endregion

        #region Statistics Endpoints

        /// <summary>
        /// دریافت آمار روزانه (AJAX)
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        [HttpGet]
        public async Task<JsonResult> GetDailyStats(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار روزانه. تاریخ: {Date}, کاربر: {UserName}", date, _currentUserService.UserName);

                // Try to get from cache first
                var cachedStats = await _cacheService.GetDailyStatsFromCacheAsync(date);
                if (cachedStats != null)
                {
                    _logger.Debug("دریافت آمار روزانه از Cache موفق");
                    return Json(new { success = true, data = cachedStats }, JsonRequestBehavior.AllowGet);
                }

                // Load from database
                var statsResult = await _receptionService.GetDailyStatsAsync(date);
                if (!statsResult.Success)
                {
                    _logger.Warning("دریافت آمار روزانه ناموفق. خطا: {Error}", statsResult.Message);
                    return Json(new { success = false, message = statsResult.Message });
                }

                // Cache the results
                await _cacheService.CacheDailyStatsAsync(date, statsResult.Data);

                _logger.Debug("دریافت آمار روزانه از Database موفق");
                return Json(new { success = true, data = statsResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار روزانه. تاریخ: {Date}", date);
                return Json(new { success = false, message = "خطا در دریافت آمار روزانه" });
            }
        }

        /// <summary>
        /// دریافت آمار پزشکان (AJAX)
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار پزشکان</returns>
        [HttpGet]
        public async Task<JsonResult> GetDoctorStats(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار پزشکان. تاریخ: {Date}, کاربر: {UserName}", date, _currentUserService.UserName);

                // Load from database
                var statsResult = await _receptionService.GetDoctorStatsAsync(date);
                if (!statsResult.Success)
                {
                    _logger.Warning("دریافت آمار پزشکان ناموفق. خطا: {Error}", statsResult.Message);
                    return Json(new { success = false, message = statsResult.Message });
                }

                _logger.Debug("دریافت آمار پزشکان موفق. تعداد: {Count}", statsResult.Data.Count);
                return Json(new { success = true, data = statsResult.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشکان. تاریخ: {Date}", date);
                return Json(new { success = false, message = "خطا در دریافت آمار پزشکان" });
            }
        }

        #endregion

        #region Performance Endpoints

        /// <summary>
        /// دریافت اطلاعات عملکرد (AJAX)
        /// </summary>
        /// <returns>اطلاعات عملکرد</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetPerformanceInfo()
        {
            try
            {
                _logger.Debug("دریافت اطلاعات عملکرد. کاربر: {UserName}", _currentUserService.UserName);

                var performanceInfo = await _performanceOptimizer.GetPerformanceInfoAsync();
                var queryStats = await _performanceOptimizer.GetQueryStatisticsAsync();
                var cacheStats = await _performanceOptimizer.GetCacheStatisticsAsync();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        performance = performanceInfo,
                        queries = queryStats,
                        cache = cacheStats
                    }
                };

                _logger.Debug("دریافت اطلاعات عملکرد موفق");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات عملکرد");
                return Json(new { success = false, message = "خطا در دریافت اطلاعات عملکرد" });
            }
        }

        /// <summary>
        /// پاک کردن Cache (AJAX)
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> ClearCache()
        {
            try
            {
                _logger.Information("پاک کردن Cache. کاربر: {UserName}", _currentUserService.UserName);

                var clearResult = await _cacheService.ClearAllCacheAsync();
                if (!clearResult)
                {
                    _logger.Warning("پاک کردن Cache ناموفق");
                    return Json(new { success = false, message = "خطا در پاک کردن Cache" });
                }

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "ClearCache", "All", true);

                _logger.Information("پاک کردن Cache موفق");
                return Json(new { success = true, message = "Cache با موفقیت پاک شد" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache");
                return Json(new { success = false, message = "خطا در پاک کردن Cache" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// بارگذاری لیست‌های Lookup
        /// </summary>
        /// <param name="model">مدل</param>
        /// <returns>مدل با لیست‌های Lookup</returns>
        private async Task<T> LoadLookupListsAsync<T>(T model) where T : class
        {
            try
            {
                _logger.Debug("بارگذاری لیست‌های Lookup");

                // Load doctors
                var doctorsResult = await _receptionService.GetDoctorsAsync();
                if (doctorsResult.Success)
                {
                    // Set doctors property if it exists
                    var doctorsProperty = typeof(T).GetProperty("Doctors");
                    if (doctorsProperty != null)
                    {
                        doctorsProperty.SetValue(model, doctorsResult.Data);
                    }
                }

                // Load service categories
                var categoriesResult = await _receptionService.GetServiceCategoriesAsync();
                if (categoriesResult.Success)
                {
                    // Set service categories property if it exists
                    var categoriesProperty = typeof(T).GetProperty("ServiceCategories");
                    if (categoriesProperty != null)
                    {
                        categoriesProperty.SetValue(model, categoriesResult.Data);
                    }
                }

                // Load services
                var servicesResult = await _receptionService.GetServicesByCategoryAsync(0);
                if (servicesResult.Success)
                {
                    // Set services property if it exists
                    var servicesProperty = typeof(T).GetProperty("Services");
                    if (servicesProperty != null)
                    {
                        servicesProperty.SetValue(model, servicesResult.Data);
                    }
                }

                // Load payment methods
                var paymentMethods = GetPaymentMethodList();
                var paymentMethodsProperty = typeof(T).GetProperty("PaymentMethods");
                if (paymentMethodsProperty != null)
                {
                    paymentMethodsProperty.SetValue(model, paymentMethods);
                }

                _logger.Debug("بارگذاری لیست‌های Lookup موفق");
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری لیست‌های Lookup");
                return model;
            }
        }

        /// <summary>
        /// دریافت لیست روش‌های پرداخت
        /// </summary>
        /// <returns>لیست روش‌های پرداخت</returns>
        private List<ReceptionLookupViewModel> GetPaymentMethodList()
        {
            var paymentMethods = new List<ReceptionLookupViewModel>();
            
            foreach (PaymentMethod method in Enum.GetValues(typeof(PaymentMethod)))
            {
                paymentMethods.Add(new ReceptionLookupViewModel
                {
                    Id = (int)method,
                    Name = method.GetDisplayName(),
                    Code = method.ToString(),
                    IsActive = true
                });
            }

            return paymentMethods;
        }

        #endregion
    }
}
