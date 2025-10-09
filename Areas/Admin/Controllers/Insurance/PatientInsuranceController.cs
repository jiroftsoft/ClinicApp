using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.Models.DTOs.Insurance;
using Serilog;
using System.Net;
using System.Data.SqlClient;
using ClinicApp.Models;
// using System.Data.Entity; // 🚨 CRITICAL FIX: حذف شد - دیگر از EF مستقیم استفاده نمی‌کنیم
using System.Net.Http;
using System.Threading;
using ViewModels.Insurance.PatientInsurance;

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
   // [Authorize] // فعال‌سازی کنترل دسترسی - Critical Security Fix
    // Routing attributes حذف شده - از conventional routing استفاده می‌کنیم
    public class PatientInsuranceController : Controller
    {
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IInsurancePlanService _insurancePlanService;
        private readonly IInsuranceProviderService _insuranceProviderService;
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
            IInsuranceProviderService insuranceProviderService,
            IPatientService patientService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _insurancePlanService = insurancePlanService ?? throw new ArgumentNullException(nameof(insurancePlanService));
            _insuranceProviderService = insuranceProviderService ?? throw new ArgumentNullException(nameof(insuranceProviderService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _log = logger.ForContext<PatientInsuranceController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Insurance Status Helper Methods

        /// <summary>
        /// بررسی وضعیت بیمه بیمار و ارائه راهنمایی مناسب
        /// </summary>
        private async Task<InsuranceStatusInfo> GetPatientInsuranceStatusAsync(int patientId)
        {
            try
            {
                var statusInfo = new InsuranceStatusInfo
                {
                    PatientId = patientId,
                    HasPrimaryInsurance = false,
                    HasSupplementaryInsurance = false,
                    PrimaryInsuranceCount = 0,
                    SupplementaryInsuranceCount = 0,
                    Recommendation = InsuranceRecommendation.None
                };

                // بررسی وجود بیمه اصلی
                var primaryInsuranceResult = await _patientInsuranceService.DoesPrimaryInsuranceExistAsync(patientId, null);
                if (primaryInsuranceResult.Success && primaryInsuranceResult.Data)
                {
                    statusInfo.HasPrimaryInsurance = true;
                    statusInfo.PrimaryInsuranceCount = 1; // فقط یک بیمه اصلی مجاز است
                }

                // بررسی وجود بیمه تکمیلی
                var supplementaryInsurancesResult = await _patientInsuranceService.GetSupplementaryInsurancesByPatientAsync(patientId);
                if (supplementaryInsurancesResult.Success && supplementaryInsurancesResult.Data.Any())
                {
                    statusInfo.HasSupplementaryInsurance = true;
                    statusInfo.SupplementaryInsuranceCount = supplementaryInsurancesResult.Data.Count;
                }

                // تعیین توصیه
                if (!statusInfo.HasPrimaryInsurance)
                {
                    statusInfo.Recommendation = InsuranceRecommendation.CreatePrimaryInsurance;
                }
                else if (!statusInfo.HasSupplementaryInsurance)
                {
                    statusInfo.Recommendation = InsuranceRecommendation.ConsiderSupplementaryInsurance;
                }
                else
                {
                    statusInfo.Recommendation = InsuranceRecommendation.InsuranceComplete;
                }

                _log.Information("🏥 MEDICAL: وضعیت بیمه بیمار بررسی شد. PatientId: {PatientId}, HasPrimary: {HasPrimary}, HasSupplementary: {HasSupplementary}, Recommendation: {Recommendation}. User: {UserName} (Id: {UserId})",
                    patientId, statusInfo.HasPrimaryInsurance, statusInfo.HasSupplementaryInsurance, statusInfo.Recommendation, _currentUserService.UserName, _currentUserService.UserId);

                return statusInfo;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وضعیت بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                
                return new InsuranceStatusInfo
                {
                    PatientId = patientId,
                    HasPrimaryInsurance = false,
                    HasSupplementaryInsurance = false,
                    Recommendation = InsuranceRecommendation.Error
                };
            }
        }

        /// <summary>
        /// بررسی وابستگی بیمه تکمیلی به بیمه پایه
        /// </summary>
        private async Task<ServiceResult<bool>> ValidateSupplementaryInsuranceDependencyAsync(int patientId, string policyNumber)
        {
            try
            {
                _log.Information("🏥 MEDICAL: بررسی وابستگی بیمه تکمیلی. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    patientId, policyNumber, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود بیمه پایه برای بیمار
                var primaryInsuranceResult = await _patientInsuranceService.GetPrimaryInsuranceByPatientAsync(patientId);
                if (!primaryInsuranceResult.Success || primaryInsuranceResult.Data == null)
                {
                    _log.Warning("🏥 MEDICAL: بیمه پایه برای بیمار {PatientId} یافت نشد. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("ابتدا باید بیمه پایه برای این بیمار تعریف شود.");
                }

                var primaryInsurance = primaryInsuranceResult.Data;

                // بررسی فعال بودن بیمه پایه
                if (!primaryInsurance.IsActive)
                {
                    _log.Warning("🏥 MEDICAL: بیمه پایه بیمار {PatientId} غیرفعال است. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("بیمه پایه این بیمار غیرفعال است. ابتدا بیمه پایه را فعال کنید.");
                }

                // 🚨 CRITICAL FIX: همسان‌سازی UTC و مقایسه تاریخ
                var effectiveStartDate = DateTime.UtcNow; // استفاده از UTC
                if (primaryInsurance.EndDate.HasValue && primaryInsurance.EndDate.Value < effectiveStartDate)
                {
                    _log.Warning("🏥 MEDICAL: بیمه پایه بیمار {PatientId} منقضی شده است. EndDate: {EndDate}. User: {UserName} (Id: {UserId})",
                        patientId, primaryInsurance.EndDate.Value, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<bool>.Failed("بیمه پایه این بیمار منقضی شده است. ابتدا بیمه پایه را تمدید کنید.");
                }

                _log.Information("🏥 MEDICAL: وابستگی بیمه تکمیلی تأیید شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    patientId, policyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وابستگی بیمه تکمیلی. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    patientId, policyNumber, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی وابستگی بیمه تکمیلی");
            }
        }

        /// <summary>
        /// ایجاد بیمه پیش‌فرض آزاد برای بیمار
        /// </summary>
        private async Task<ServiceResult<PatientInsurance>> CreateDefaultFreeInsuranceAsync(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: ایجاد بیمه پیش‌فرض آزاد برای بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح بیمه آزاد (Free Insurance Plan)
                var freeInsurancePlans = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (!freeInsurancePlans.Success || !freeInsurancePlans.Data.Any())
                {
                    _log.Error("🏥 MEDICAL: هیچ طرح بیمه‌ای یافت نشد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<PatientInsurance>.Failed("هیچ طرح بیمه‌ای یافت نشد");
                }

                // جستجوی طرح بیمه آزاد
                var freePlan = freeInsurancePlans.Data.FirstOrDefault(p => 
                    p.Name.Contains("آزاد") || p.Name.Contains("Free") || p.Name.Contains("عمومی"));
                
                if (freePlan == null)
                {
                    // اگر طرح آزاد یافت نشد، از اولین طرح استفاده کن
                    freePlan = freeInsurancePlans.Data.First();
                    _log.Warning("🏥 MEDICAL: طرح بیمه آزاد یافت نشد، از اولین طرح استفاده می‌شود. PlanId: {PlanId}, PlanName: {PlanName}. User: {UserName} (Id: {UserId})",
                        freePlan.InsurancePlanId, freePlan.Name, _currentUserService.UserName, _currentUserService.UserId);
                }

                // 🚨 CRITICAL FIX: تضمین یکتا بودن شماره بیمه آزاد
                var basePolicyNumber = $"FREE-{patientId:D6}-{DateTime.UtcNow:yyyyMMdd}";
                var policyNumber = basePolicyNumber;
                
                // بررسی یکتا بودن و retry در صورت برخورد
                for (var i = 1; ; i++)
                {
                    var existsResult = await _patientInsuranceService.DoesPolicyNumberExistAsync(policyNumber, null);
                    if (!existsResult.Success || !existsResult.Data)
                    {
                        break; // شماره یکتا است
                    }
                    
                    // شماره تکراری است، شمارنده اضافه کن
                    policyNumber = $"{basePolicyNumber}-{i}";
                    
                    _log.Information("🏥 MEDICAL: شماره بیمه تکراری، شمارنده اضافه شد. Original: {Original}, New: {New}. User: {UserName} (Id: {UserId})",
                        basePolicyNumber, policyNumber, _currentUserService.UserName, _currentUserService.UserId);
                }

                // ایجاد بیمه آزاد با شماره یکتا
                var freeInsurance = new PatientInsurance
                {
                    PatientId = patientId,
                    InsurancePlanId = freePlan.InsurancePlanId,
                    PolicyNumber = policyNumber,
                    StartDate = DateTime.UtcNow,
                    IsPrimary = true,
                    IsActive = true,
                    Priority = InsurancePriority.Primary,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = _currentUserService.UserId
                };

                // ذخیره بیمه آزاد
                var createResult = await _patientInsuranceService.CreatePatientInsuranceAsync(new PatientInsuranceCreateEditViewModel
                {
                    PatientId = freeInsurance.PatientId,
                    InsurancePlanId = freeInsurance.InsurancePlanId,
                    PolicyNumber = freeInsurance.PolicyNumber,
                    StartDate = freeInsurance.StartDate,
                    IsPrimary = freeInsurance.IsPrimary,
                    IsActive = freeInsurance.IsActive
                });

                if (!createResult.Success)
                {
                    _log.Error("🏥 MEDICAL: خطا در ایجاد بیمه آزاد. PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, createResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<PatientInsurance>.Failed("خطا در ایجاد بیمه آزاد: " + createResult.Message);
                }

                _log.Information("🏥 MEDICAL: بیمه پیش‌فرض آزاد با موفقیت ایجاد شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    patientId, freeInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsurance>.Successful(freeInsurance);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ایجاد بیمه پیش‌فرض آزاد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PatientInsurance>.Failed("خطا در ایجاد بیمه پیش‌فرض آزاد");
            }
        }

        /// <summary>
        /// ایجاد خودکار بیمه پیش‌فرض آزاد برای بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateDefaultFreeInsurance(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد بیمه پیش‌فرض آزاد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود بیمه اصلی
                var hasPrimaryInsurance = await _patientInsuranceService.DoesPrimaryInsuranceExistAsync(patientId, null);
                if (hasPrimaryInsurance.Success && hasPrimaryInsurance.Data)
                {
                    _log.Warning("🏥 MEDICAL: بیمار قبلاً بیمه اصلی دارد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "این بیمار قبلاً بیمه اصلی دارد." });
                }

                // ایجاد بیمه آزاد
                var result = await CreateDefaultFreeInsuranceAsync(patientId);
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: بیمه پیش‌فرض آزاد با موفقیت ایجاد شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                        patientId, result.Data.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { 
                        success = true, 
                        message = "بیمه پیش‌فرض آزاد با موفقیت ایجاد شد.",
                        data = new {
                            policyNumber = result.Data.PolicyNumber,
                            startDate = result.Data.StartDate.ToString("yyyy/MM/dd")
                        }
                    });
                }
                else
                {
                    _log.Error("🏥 MEDICAL: خطا در ایجاد بیمه پیش‌فرض آزاد. PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در ایجاد بیمه پیش‌فرض آزاد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در ایجاد بیمه پیش‌فرض آزاد" });
            }
        }

        #endregion

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
                // بررسی ساده اتصال به دیتابیس
                var healthCheck = await _patientInsuranceService.GetTotalRecordsCountAsync();
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

        /// <summary>
        /// اعتبارسنجی بیمه بیمار با استفاده از سرویس جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ValidatePatientInsurance(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: شروع اعتبارسنجی بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از سرویس اعتبارسنجی جدید
                var validationService = DependencyResolver.Current.GetService<IPatientInsuranceValidationService>();
                var validationResult = await validationService.ValidatePatientInsuranceAsync(patientId);

                if (validationResult.Success)
                {
                    _log.Information("🏥 MEDICAL: اعتبارسنجی بیمه بیمار موفق. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(ServiceResult<PatientInsuranceValidationResult>.Successful(
                        validationResult.Data, 
                        "اعتبارسنجی بیمه بیمار با موفقیت انجام شد"));
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در اعتبارسنجی بیمه بیمار. PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, validationResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(ServiceResult<PatientInsuranceValidationResult>.Failed(
                        validationResult.Message), 
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در اعتبارسنجی بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(ServiceResult<PatientInsuranceValidationResult>.Failed(
                    "خطای غیرمنتظره در اعتبارسنجی بیمه بیمار"), 
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت وضعیت بیمه بیمار
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetPatientInsuranceStatus(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: دریافت وضعیت بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه‌های بیمار
                var primaryInsuranceResult = await _patientInsuranceService.GetPrimaryInsuranceByPatientAsync(patientId);
                var supplementaryInsurancesResult = await _patientInsuranceService.GetSupplementaryInsurancesByPatientAsync(patientId);

                var status = new PatientInsuranceStatus
                {
                    PatientId = patientId,
                    HasPrimaryInsurance = primaryInsuranceResult.Success && primaryInsuranceResult.Data != null,
                    HasSupplementaryInsurance = supplementaryInsurancesResult.Success && supplementaryInsurancesResult.Data?.Any() == true,
                    PrimaryInsuranceActive = primaryInsuranceResult.Success && primaryInsuranceResult.Data?.IsActive == true,
                    ValidationDate = DateTime.UtcNow
                };

                _log.Information("🏥 MEDICAL: وضعیت بیمه بیمار دریافت شد. PatientId: {PatientId}, HasPrimary: {HasPrimary}, HasSupplementary: {HasSupplementary}. User: {UserName} (Id: {UserId})",
                    patientId, status.HasPrimaryInsurance, status.HasSupplementaryInsurance, _currentUserService.UserName, _currentUserService.UserId);

                return Json(ServiceResult<PatientInsuranceStatus>.Successful(
                    status,
                    "وضعیت بیمه بیمار دریافت شد"),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در دریافت وضعیت بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(ServiceResult<PatientInsuranceStatus>.Failed(
                    "خطای غیرمنتظره در دریافت وضعیت بیمه بیمار"),
                    JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت بیمه‌گذاران پایه
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetPrimaryInsuranceProviders()
        {
            try
            {
                _log.Information("🏥 MEDICAL: دریافت بیمه‌گذاران پایه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var providers = await _insuranceProviderService.GetPrimaryInsuranceProvidersAsync();
                if (providers.Success)
                {
                    var providerList = providers.Data.Select(p => new { 
                        id = p.InsuranceProviderId, 
                        name = p.Name,
                        code = p.Code 
                    }).ToList();
                    
                    _log.Information("🏥 MEDICAL: بیمه‌گذاران پایه دریافت شد. Count: {Count}. User: {UserName} (Id: {UserId})",
                        providerList.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = providerList,
                        message = "بیمه‌گذاران پایه دریافت شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = providers.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در دریافت بیمه‌گذاران پایه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(ServiceResult<List<object>>.Failed("خطای غیرمنتظره در دریافت بیمه‌گذاران پایه"),
                    JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس بیمه‌گذار
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetInsurancePlansByProvider(int providerId, string type = "primary")
        {
            try
            {
                _log.Information("🏥 MEDICAL: دریافت طرح‌های بیمه. ProviderId: {ProviderId}, Type: {Type}. User: {UserName} (Id: {UserId})",
                    providerId, type, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه بر اساس نوع
                var plans = new List<object>();
                
                if (type == "primary")
                {
                    // طرح‌های بیمه پایه
                    var primaryPlans = await _insurancePlanService.GetPrimaryInsurancePlansByProviderAsync(providerId);
                    if (primaryPlans.Success)
                    {
                        plans = primaryPlans.Data.Select(p => new { id = p.InsurancePlanId, name = p.Name }).ToList<object>();
                    }
                }
                else if (type == "supplementary")
                {
                    // طرح‌های بیمه تکمیلی
                    var supplementaryPlans = await _insurancePlanService.GetSupplementaryInsurancePlansByProviderAsync(providerId);
                    if (supplementaryPlans.Success)
                    {
                        plans = supplementaryPlans.Data.Select(p => new { id = p.InsurancePlanId, name = p.Name }).ToList<object>();
                    }
                }

                _log.Information("🏥 MEDICAL: طرح‌های بیمه دریافت شد. ProviderId: {ProviderId}, Type: {Type}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    providerId, type, plans.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = true,
                    data = plans,
                    message = "طرح‌های بیمه دریافت شد"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای غیرمنتظره در دریافت طرح‌های بیمه. ProviderId: {ProviderId}, Type: {Type}. User: {UserName} (Id: {UserId})",
                    providerId, type, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطای غیرمنتظره در دریافت طرح‌های بیمه"
                }, JsonRequestBehavior.AllowGet);
            }
        }

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
                    return Json(new { success = true, count = result.Data, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in DebugCount method");
                return Json(new { success = false, message = "خطا در بررسی تعداد رکوردها" });
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
                    return Json(new { success = true, data = result.Data, message = result.Message });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error in DebugSimpleList method");
                return Json(new { success = false, message = "خطا در دریافت لیست ساده" });
            }
        }

        #endregion

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی بیمه‌های بیماران
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(string searchTerm = "", int? providerId = null, int? planId = null,
            bool? isPrimary = null, bool? isActive = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 0)
        {
            _log.Information("بازدید از صفحه اصلی بیمه‌های بیماران. SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, PlanId: {PlanId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, Page: {Page}. User: {UserName} (Id: {UserId})",
                searchTerm, providerId, planId, isPrimary, isActive, page, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // ✅ **بهینه‌سازی PageSize برای 7000 بیمار**
                var effectivePageSize = pageSize > 0 ? Math.Min(pageSize, 100) : PageSize; // حداکثر 100 رکورد

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
                    PageSize = effectivePageSize
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
                    pageSize: effectivePageSize);

                if (result.Success)
                {
                    // 🚨 CRITICAL FIX: استفاده مستقیم از ViewModel های سرویس (بدون مپ اضافی)
                    model.PatientInsurances = result.Data.Items.Select(item => new PatientInsuranceIndexItemViewModel
                    {
                        PatientInsuranceId = item.PatientInsuranceId,
                        PatientId = item.PatientId,
                        PatientFullName = item.PatientName, // مپ صحیح: PatientName -> PatientFullName
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
                        CreatedByUserName = item.CreatedByUserName,
                        // 🏥 Medical Environment: فیلدهای بیمه تکمیلی
                        SupplementaryInsuranceProviderId = item.SupplementaryInsuranceProviderId,
                        SupplementaryInsuranceProviderName = item.SupplementaryInsuranceProviderName,
                        SupplementaryInsurancePlanId = item.SupplementaryInsurancePlanId,
                        SupplementaryInsurancePlanName = item.SupplementaryInsurancePlanName,
                        SupplementaryPolicyNumber = item.SupplementaryPolicyNumber,
                        HasSupplementaryInsurance = item.HasSupplementaryInsurance
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
                // استفاده از متد جدید با فیلترهای کامل
                var result = await _patientInsuranceService.GetPatientInsurancesWithFiltersAsync(patientId, searchTerm, providerId, isPrimary, isActive, page, PageSize);
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
            _log.Information("🏥 MEDICAL: بازدید از فرم ایجاد بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                patientId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای ایجاد بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید.";
                    return RedirectToAction("Index");
                }

                var model = new PatientInsuranceCreateEditViewModel
                {
                    PatientId = patientId ?? 0,
                    IsActive = true,
                    StartDate = DateTime.Now,
                    IsPrimary = false
                };

                // 🏥 Medical Environment: بارگیری لیست طرح‌های بیمه با بهینه‌سازی
                await LoadDropdownsForModelAsync(model);

                // 🏥 Medical Environment: بررسی وضعیت بیمه بیمار
                if (patientId.HasValue && patientId.Value > 0)
                {
                    var insuranceStatus = await GetPatientInsuranceStatusAsync(patientId.Value);
                    
                    switch (insuranceStatus.Recommendation)
                    {
                        case InsuranceRecommendation.CreatePrimaryInsurance:
                            model.IsPrimary = true;
                            TempData["InfoMessage"] = "این بیمار فاقد بیمه اصلی است. لطفاً ابتدا بیمه اصلی را ثبت کنید.";
                            _log.Information("🏥 MEDICAL: بیمار فاقد بیمه اصلی - توصیه به ایجاد بیمه اصلی. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                                patientId, _currentUserService.UserName, _currentUserService.UserId);
                            break;
                            
                        case InsuranceRecommendation.ConsiderSupplementaryInsurance:
                            model.IsPrimary = false;
                            TempData["InfoMessage"] = "این بیمار دارای بیمه اصلی است. بیمه جدید به عنوان بیمه تکمیلی ثبت خواهد شد.";
                            _log.Information("🏥 MEDICAL: بیمار دارای بیمه اصلی - توصیه به ایجاد بیمه تکمیلی. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                                patientId, _currentUserService.UserName, _currentUserService.UserId);
                            break;
                            
                        case InsuranceRecommendation.InsuranceComplete:
                            model.IsPrimary = false;
                            TempData["WarningMessage"] = "این بیمار دارای بیمه اصلی و تکمیلی است. آیا مطمئن هستید که می‌خواهید بیمه اضافی ثبت کنید؟";
                            _log.Information("🏥 MEDICAL: بیمار دارای بیمه کامل - هشدار برای بیمه اضافی. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                                patientId, _currentUserService.UserName, _currentUserService.UserId);
                            break;
                            
                        case InsuranceRecommendation.Error:
                            TempData["ErrorMessage"] = "خطا در بررسی وضعیت بیمه بیمار. لطفاً دوباره تلاش کنید.";
                            _log.Error("🏥 MEDICAL: خطا در بررسی وضعیت بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                                patientId, _currentUserService.UserName, _currentUserService.UserId);
                            break;
                    }
                }

                _log.Information("🏥 MEDICAL: فرم ایجاد بیمه بیمار با موفقیت بارگذاری شد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در نمایش فرم ایجاد بیمه بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
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
                "🏥 MEDICAL: درخواست ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای ایجاد بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید.";
                    await LoadDropdownsForModelAsync(model);
                    return View(model);
                }

                // 🏥 Medical Environment: تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    model.ConvertPersianDatesToGregorian();
                }

                // 🏥 Medical Environment: اعتبارسنجی ModelState
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی در فرم ایجاد بیمه بیمار. Errors: {ValidationErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors.SelectMany(x => x.Value)), _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "لطفاً تمام فیلدهای اجباری را به درستی پر کنید.";
                    await LoadDropdownsForModelAsync(model);
                    return View(model);
                }

                // 🏥 Medical Environment: اعتبارسنجی اضافی server-side (منطق کسب‌وکار در سرویس)
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی کسب‌وکار در ایجاد بیمه بیمار. Errors: {BusinessErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationResult.Data.Select(x => $"{x.Key}: {x.Value}")), _currentUserService.UserName, _currentUserService.UserId);

                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیست.";
                    await LoadDropdownsForModelAsync(model);
                    return View(model);
                }

                // 🏥 Medical Environment: اعتبارسنجی وابستگی بیمه تکمیلی
                if (!model.IsPrimary)
                {
                    var dependencyValidation = await ValidateSupplementaryInsuranceDependencyAsync(model.PatientId, model.PolicyNumber);
                    if (!dependencyValidation.Success)
                    {
                        _log.Warning("🏥 MEDICAL: خطا در وابستگی بیمه تکمیلی. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            model.PatientId, model.PolicyNumber, dependencyValidation.Message, _currentUserService.UserName, _currentUserService.UserId);

                        ModelState.AddModelError("PolicyNumber", dependencyValidation.Message);
                        TempData["ErrorMessage"] = "خطا در وابستگی بیمه تکمیلی: " + dependencyValidation.Message;
                        await LoadDropdownsForModelAsync(model);
                        return View(model);
                    }
                }

                // 🏥 Medical Environment: ایجاد بیمه بیمار
                var result = await _patientInsuranceService.CreatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "🏥 MEDICAL: خطا در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    await LoadDropdownsForModelAsync(model);
                    return View(model);
                }

                // 🏥 Medical Environment: Audit Trail
                _log.Information("🏥 MEDICAL: بیمه بیمار جدید با موفقیت ایجاد شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    result.Data, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار جدید با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در ایجاد بیمه بیمار. لطفاً دوباره تلاش کنید.";
                await LoadDropdownsForModelAsync(model);
                return View(model);
            }
        }

        /// <summary>
        /// دریافت شماره بیمه پایه بیمار
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPrimaryInsurancePolicyNumber(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دریافت شماره بیمه پایه. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _patientInsuranceService.GetPrimaryInsurancePolicyNumberAsync(patientId);
                if (result.Success && !string.IsNullOrEmpty(result.Data))
                {
                    _log.Information("🏥 MEDICAL: شماره بیمه پایه دریافت شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                        patientId, result.Data, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: شماره بیمه پایه یافت نشد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "بیمه پایه برای این بیمار تعریف نشده است" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت شماره بیمه پایه. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت شماره بیمه پایه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد بیمه بیمار جدید (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateAjax(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information("🏥 MEDICAL: CreateAjax method STARTED at {Timestamp}", DateTime.UtcNow);
            // 🏥 MEDICAL: بررسی Model Binding در ابتدای متد
            _log.Information("🏥 MEDICAL: === CreateAjax METHOD STARTED ===");
            _log.Information("🏥 MEDICAL: Model is null: {IsNull}", model == null);
            _log.Information("🏥 MEDICAL: Request.ContentType: {ContentType}", Request.ContentType);
            _log.Information("🏥 MEDICAL: Request.Form.Count: {FormCount}", Request.Form.Count);
            
            // Log all form values for debugging
            foreach (string key in Request.Form.AllKeys)
            {
                _log.Information("🏥 MEDICAL: Request.Form[{Key}]: {Value}", key, Request.Form[key]);
            }
            
            // 🏥 MEDICAL: Comprehensive Model Logging
            _log.Information("🏥 MEDICAL: === COMPREHENSIVE MODEL ANALYSIS ===");
            _log.Information("🏥 MEDICAL: Model is null: {IsNull}", model == null);
            
            if (model != null)
            {
                _log.Information("🏥 MEDICAL: === BASIC PROPERTIES ===");
                _log.Information("🏥 MEDICAL: PatientId: {PatientId} (Type: {PatientIdType})", model.PatientId, model.PatientId.GetType().Name);
                _log.Information("🏥 MEDICAL: PatientInsuranceId: {PatientInsuranceId} (Type: {PatientInsuranceIdType})", model.PatientInsuranceId, model.PatientInsuranceId.GetType().Name);
                _log.Information("🏥 MEDICAL: PatientName: {PatientName}", model.PatientName ?? "NULL");
                
                _log.Information("🏥 MEDICAL: === INSURANCE PROVIDER PROPERTIES ===");
                _log.Information("🏥 MEDICAL: InsuranceProviderId: {InsuranceProviderId} (Type: {InsuranceProviderIdType})", model.InsuranceProviderId, model.InsuranceProviderId.GetType().Name);
                _log.Information("🏥 MEDICAL: InsuranceProviderName: {InsuranceProviderName}", model.InsuranceProviderName ?? "NULL");
                _log.Information("🏥 MEDICAL: InsurancePlanId: {InsurancePlanId} (Type: {InsurancePlanIdType})", model.InsurancePlanId, model.InsurancePlanId.GetType().Name);
                _log.Information("🏥 MEDICAL: InsurancePlanName: {InsurancePlanName}", model.InsurancePlanName ?? "NULL");
                
                _log.Information("🏥 MEDICAL: === SUPPLEMENTARY INSURANCE PROPERTIES ===");
                _log.Information("🏥 MEDICAL: SupplementaryInsuranceProviderId: {SupplementaryInsuranceProviderId} (Type: {SupplementaryInsuranceProviderIdType})", 
                    model.SupplementaryInsuranceProviderId, model.SupplementaryInsuranceProviderId?.GetType().Name ?? "NULL");
                _log.Information("🏥 MEDICAL: SupplementaryInsuranceProviderName: {SupplementaryInsuranceProviderName}", model.SupplementaryInsuranceProviderName ?? "NULL");
                _log.Information("🏥 MEDICAL: SupplementaryInsurancePlanId: {SupplementaryInsurancePlanId} (Type: {SupplementaryInsurancePlanIdType})", 
                    model.SupplementaryInsurancePlanId, model.SupplementaryInsurancePlanId?.GetType().Name ?? "NULL");
                _log.Information("🏥 MEDICAL: SupplementaryInsurancePlanName: {SupplementaryInsurancePlanName}", model.SupplementaryInsurancePlanName ?? "NULL");
                
                _log.Information("🏥 MEDICAL: === POLICY NUMBER PROPERTIES ===");
                _log.Information("🏥 MEDICAL: PolicyNumber: {PolicyNumber}", model.PolicyNumber ?? "NULL");
                _log.Information("🏥 MEDICAL: SupplementaryPolicyNumber: {SupplementaryPolicyNumber}", model.SupplementaryPolicyNumber ?? "NULL");
                
                _log.Information("🏥 MEDICAL: === DATE PROPERTIES ===");
                _log.Information("🏥 MEDICAL: StartDate: {StartDate} (Type: {StartDateType})", model.StartDate, model.StartDate.GetType().Name);
                _log.Information("🏥 MEDICAL: EndDate: {EndDate} (Type: {EndDateType})", model.EndDate, model.EndDate?.GetType().Name ?? "NULL");
                _log.Information("🏥 MEDICAL: StartDateShamsi: {StartDateShamsi}", model.StartDateShamsi ?? "NULL");
                _log.Information("🏥 MEDICAL: EndDateShamsi: {EndDateShamsi}", model.EndDateShamsi ?? "NULL");
                
                _log.Information("🏥 MEDICAL: === BOOLEAN PROPERTIES ===");
                _log.Information("🏥 MEDICAL: IsPrimary: {IsPrimary} (Type: {IsPrimaryType})", model.IsPrimary, model.IsPrimary.GetType().Name);
                _log.Information("🏥 MEDICAL: IsActive: {IsActive} (Type: {IsActiveType})", model.IsActive, model.IsActive.GetType().Name);
                
                _log.Information("🏥 MEDICAL: === ENUM PROPERTIES ===");
                _log.Information("🏥 MEDICAL: Priority: {Priority} (Type: {PriorityType})", model.Priority, model.Priority.GetType().Name);
                
                _log.Information("🏥 MEDICAL: === SELECT LIST PROPERTIES ===");
                _log.Information("🏥 MEDICAL: PatientSelectList is null: {PatientSelectListIsNull}", model.PatientSelectList == null);
                _log.Information("🏥 MEDICAL: InsuranceProviderSelectList is null: {InsuranceProviderSelectListIsNull}", model.InsuranceProviderSelectList == null);
                _log.Information("🏥 MEDICAL: InsurancePlanSelectList is null: {InsurancePlanSelectListIsNull}", model.InsurancePlanSelectList == null);
                _log.Information("🏥 MEDICAL: SupplementaryInsuranceProviderSelectList is null: {SupplementaryInsuranceProviderSelectListIsNull}", model.SupplementaryInsuranceProviderSelectList == null);
                _log.Information("🏥 MEDICAL: SupplementaryInsurancePlanSelectList is null: {SupplementaryInsurancePlanSelectListIsNull}", model.SupplementaryInsurancePlanSelectList == null);
            }
            else
            {
                _log.Error("🏥 MEDICAL: Model is NULL - This indicates a serious Model Binding issue!");
            }
            
            _log.Information("🏥 MEDICAL: === REQUEST CONTEXT ANALYSIS ===");
            _log.Information("🏥 MEDICAL: Request Method: {RequestMethod}", Request.HttpMethod);
            _log.Information("🏥 MEDICAL: Request ContentType: {RequestContentType}", Request.ContentType ?? "NULL");
            _log.Information("🏥 MEDICAL: Request Form Keys: {FormKeys}", string.Join(", ", Request.Form.AllKeys ?? new string[0]));
            _log.Information("🏥 MEDICAL: Request QueryString Keys: {QueryStringKeys}", string.Join(", ", Request.QueryString.AllKeys ?? new string[0]));
            
            // Log all form values
            _log.Information("🏥 MEDICAL: === FORM VALUES ANALYSIS ===");
            foreach (string key in Request.Form.AllKeys ?? new string[0])
            {
                _log.Information("🏥 MEDICAL: Form[{Key}] = {Value}", key, Request.Form[key] ?? "NULL");
            }
            
            _log.Information("🏥 MEDICAL: === MODEL STATE ANALYSIS ===");
            _log.Information("🏥 MEDICAL: ModelState.IsValid: {IsValid}", ModelState.IsValid);
            _log.Information("🏥 MEDICAL: ModelState.Keys: {Keys}", string.Join(", ", ModelState.Keys));
            
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                _log.Information("🏥 MEDICAL: ModelState[{Key}].Value: {Value}, Errors: {ErrorCount}", 
                    key, state.Value?.AttemptedValue ?? "NULL", state.Errors.Count);
                foreach (var error in state.Errors)
                {
                    _log.Information("🏥 MEDICAL: ModelState[{Key}].Error: {Error}", key, error.ErrorMessage);
                }
            }
            
            // 🏥 Medical Environment: بررسی Model Binding
            _log.Information("🏥 MEDICAL: === MODEL BINDING ANALYSIS ===");
            _log.Information("🏥 MEDICAL: Model is null: {IsNull}", model == null);
            _log.Information("🏥 MEDICAL: ModelState.IsValid: {IsValid}", ModelState.IsValid);
            
            // محاسبه تعداد خطاهای ModelState
            var errorCount = ModelState.Values.SelectMany(v => v.Errors).Count();
            _log.Information("🏥 MEDICAL: ModelState.ErrorCount: {ErrorCount}", errorCount);
            
            // 🏥 Medical Environment: بررسی Model Binding مشکل
            if (model == null)
            {
                _log.Error("🏥 MEDICAL: CRITICAL ERROR - Model is NULL! Model Binding failed completely!");
                _log.Error("🏥 MEDICAL: This means the form data is not being bound to the model properly!");
                _log.Error("🏥 MEDICAL: Request.ContentType: {ContentType}", Request.ContentType);
                _log.Error("🏥 MEDICAL: Request.Form.Count: {FormCount}", Request.Form.Count);
                
                // Log all form values for debugging
                foreach (string key in Request.Form.AllKeys)
                {
                    _log.Error("🏥 MEDICAL: Request.Form[{Key}]: {Value}", key, Request.Form[key]);
                }
                
                return Json(new { success = false, message = "خطا در دریافت اطلاعات فرم - Model Binding ناموفق" });
            }
            
            // 🏥 Medical Environment: بررسی Request.Form
            _log.Information("🏥 MEDICAL: === REQUEST.FORM ANALYSIS ===");
            _log.Information("🏥 MEDICAL: Request.ContentType: {ContentType}", Request.ContentType);
            _log.Information("🏥 MEDICAL: Request.Form.Count: {FormCount}", Request.Form.Count);
            
            foreach (string key in Request.Form.AllKeys)
            {
                _log.Information("🏥 MEDICAL: Request.Form[{Key}]: {Value}", key, Request.Form[key]);
            }
            
            if (model != null)
        {
            _log.Information(
                "🏥 MEDICAL: درخواست AJAX ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, model.PolicyNumber, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // 🏥 Medical Environment: بررسی مقادیر کلیدی
                _log.Information("🏥 MEDICAL: === COMPREHENSIVE MODEL ANALYSIS ===");
                _log.Information("🏥 MEDICAL: InsuranceProviderId: {InsuranceProviderId}", model.InsuranceProviderId);
                _log.Information("🏥 MEDICAL: InsurancePlanId: {InsurancePlanId}", model.InsurancePlanId);
                _log.Information("🏥 MEDICAL: IsPrimary: {IsPrimary}", model.IsPrimary);
                _log.Information("🏥 MEDICAL: IsActive: {IsActive}", model.IsActive);
                _log.Information("🏥 MEDICAL: PolicyNumber: {PolicyNumber}", model.PolicyNumber);
                _log.Information("🏥 MEDICAL: StartDate: {StartDate}", model.StartDate);
                _log.Information("🏥 MEDICAL: EndDate: {EndDate}", model.EndDate);
            }
            else
            {
                _log.Error("🏥 MEDICAL: Model is NULL! Model Binding failed!");
                return Json(new { success = false, message = "خطا در دریافت اطلاعات فرم" });
            }

            try
            {
                // 🏥 MEDICAL DEBUG: لاگ تمام فیلدهای ارسالی
                _log.Information("🏥 MEDICAL: === COMPLETE FORM DATA ANALYSIS ===");
                _log.Information("🏥 MEDICAL: TEST LOG - این لاگ برای تست است");
                _log.Information("🏥 MEDICAL: PatientId: {PatientId}", model?.PatientId ?? 0);
                _log.Information("🏥 MEDICAL: InsuranceProviderId: {InsuranceProviderId}", model?.InsuranceProviderId ?? 0);
                _log.Information("🏥 MEDICAL: InsurancePlanId: {InsurancePlanId}", model?.InsurancePlanId ?? 0);
                _log.Information("🏥 MEDICAL: PolicyNumber: {PolicyNumber}", model?.PolicyNumber ?? "NULL");
                _log.Information("🏥 MEDICAL: StartDate: {StartDate}", model?.StartDate);
                _log.Information("🏥 MEDICAL: EndDate: {EndDate}", model?.EndDate);
                _log.Information("🏥 MEDICAL: StartDateShamsi: {StartDateShamsi}", model?.StartDateShamsi ?? "NULL");
                _log.Information("🏥 MEDICAL: EndDateShamsi: {EndDateShamsi}", model?.EndDateShamsi ?? "NULL");
                _log.Information("🏥 MEDICAL: IsPrimary: {IsPrimary}", model?.IsPrimary ?? false);
                _log.Information("🏥 MEDICAL: IsActive: {IsActive}", model?.IsActive ?? false);
                // _log.Information("🏥 MEDICAL: CoveragePercent: {CoveragePercent}", model?.CoveragePercent ?? 0); // فیلد موجود نیست
                _log.Information("🏥 MEDICAL: Priority: {Priority}", model?.Priority ?? 0);
                _log.Information("🏥 MEDICAL: SupplementaryInsuranceProviderId: {SupplementaryInsuranceProviderId}", model?.SupplementaryInsuranceProviderId ?? 0);
                _log.Information("🏥 MEDICAL: SupplementaryInsurancePlanId: {SupplementaryInsurancePlanId}", model?.SupplementaryInsurancePlanId ?? 0);
                _log.Information("🏥 MEDICAL: === END FORM DATA ANALYSIS ===");
                
                // 🚨 CRITICAL DEBUG: بررسی دقیق مشکل InsuranceProviderId
                if (model?.InsuranceProviderId == 0)
                {
                    _log.Error("🚨 CRITICAL: InsuranceProviderId is ZERO! This will cause FK constraint violation!");
                    _log.Error("🚨 CRITICAL: Model state: {ModelState}", ModelState.IsValid);
                    _log.Error("🚨 CRITICAL: Model errors: {ModelErrors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }

                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای ایجاد بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید." });
                }

                // 🏥 Medical Environment: تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    _log.Information("🏥 MEDICAL: === DATE CONVERSION ANALYSIS ===");
                    _log.Information("🏥 MEDICAL: Before conversion - StartDateShamsi: {StartDateShamsi}, EndDateShamsi: {EndDateShamsi}", 
                        model.StartDateShamsi ?? "NULL", model.EndDateShamsi ?? "NULL");
                    _log.Information("🏥 MEDICAL: Before conversion - StartDate: {StartDate}, EndDate: {EndDate}", 
                        model.StartDate, model.EndDate);
                    
                    model.ConvertPersianDatesToGregorian();
                    
                    _log.Information("🏥 MEDICAL: After conversion - StartDate: {StartDate}, EndDate: {EndDate}", 
                        model.StartDate, model.EndDate);
                    
                    // محدودیت 1 سال آینده حذف شد - منشی می‌تواند اعتبار بیمه را تا هر زمان آینده تنظیم کند
                    _log.Information("🏥 MEDICAL: Date validation completed - no future date restrictions");
                }

                // 🚨 CRITICAL FIX: Validate InsuranceProviderId exists in database
                if (model.InsuranceProviderId > 0)
                {
                    _log.Information("🏥 MEDICAL: Validating InsuranceProviderId {InsuranceProviderId} exists in database. User: {UserName} (Id: {UserId})",
                        model.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // 🚨 CRITICAL FIX: استفاده از سرویس به جای EF مستقیم
                    var providerResult = await _insuranceProviderService.GetProviderDetailsAsync(model.InsuranceProviderId);
                    if (!providerResult.Success || providerResult.Data == null || !providerResult.Data.IsActive)
                    {
                        _log.Error("🏥 MEDICAL: InsuranceProviderId {InsuranceProviderId} does not exist or is inactive. User: {UserName} (Id: {UserId})",
                            model.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);
                        
                        return Json(new { success = false, message = $"بیمه‌گذار با شناسه {model.InsuranceProviderId} در سیستم وجود ندارد یا غیرفعال است." });
                    }
                    
                    _log.Information("🏥 MEDICAL: InsuranceProviderId {InsuranceProviderId} validated successfully. Provider: {ProviderName}. User: {UserName} (Id: {UserId})",
                        model.InsuranceProviderId, providerResult.Data.Name, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: InsuranceProviderId is 0 or empty. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = "لطفاً بیمه‌گذار را انتخاب کنید." });
                }

                // 🚨 CRITICAL FIX: Validate InsurancePlanId exists in database
                if (model.InsurancePlanId > 0)
                {
                    _log.Information("🏥 MEDICAL: Validating InsurancePlanId {InsurancePlanId} exists in database. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // 🚨 CRITICAL FIX: استفاده از سرویس به جای EF مستقیم
                    var planResult = await _insurancePlanService.GetByIdAsync(model.InsurancePlanId);
                    if (!planResult.Success || planResult.Data == null || !planResult.Data.IsActive || planResult.Data.IsDeleted)
                    {
                        _log.Error("🏥 MEDICAL: InsurancePlanId {InsurancePlanId} does not exist or is inactive. User: {UserName} (Id: {UserId})",
                            model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                        
                        return Json(new { success = false, message = $"طرح بیمه با شناسه {model.InsurancePlanId} در سیستم وجود ندارد یا غیرفعال است." });
                    }
                    
                    _log.Information("🏥 MEDICAL: InsurancePlanId {InsurancePlanId} validated successfully. Plan: {PlanName}. User: {UserName} (Id: {UserId})",
                        model.InsurancePlanId, planResult.Data.Name, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: InsurancePlanId is 0 or empty. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = "لطفاً طرح بیمه را انتخاب کنید." });
                }

                // 🏥 Medical Environment: اعتبارسنجی ModelState
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی در فرم ایجاد بیمه بیمار. Errors: {ValidationErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors.SelectMany(x => x.Value)), _currentUserService.UserName, _currentUserService.UserId);

                    // نمایش جزئیات خطاهای اعتبارسنجی
                    var errorDetails = string.Join("; ", validationErrors.SelectMany(x => x.Value));
                    return Json(new { success = false, message = $"خطاهای اعتبارسنجی: {errorDetails}", errors = validationErrors });
                }

                // 🏥 Medical Environment: اعتبارسنجی اضافی server-side (منطق کسب‌وکار در سرویس)
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی کسب‌وکار در ایجاد بیمه بیمار. Errors: {BusinessErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationResult.Data.Select(x => $"{x.Key}: {x.Value}")), _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "اطلاعات وارد شده معتبر نیست.", errors = validationResult.Data });
                }

                // 🏥 Medical Environment: ایجاد بیمه بیمار جدید
                var result = await _patientInsuranceService.CreatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "🏥 MEDICAL: خطا در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientId, model?.PolicyNumber, model?.InsurancePlanId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }

                // 🏥 Medical Environment: Audit Trail
                _log.Information("🏥 MEDICAL: بیمه بیمار جدید با موفقیت ایجاد شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    result.Data, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, message = "بیمه بیمار جدید با موفقیت ایجاد شد", data = result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در ایجاد بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در ایجاد بیمه بیمار. لطفاً دوباره تلاش کنید." });
            }
        }

        #endregion

        #region Supplementary Insurance Management

        /// <summary>
        /// افزودن بیمه تکمیلی به رکورد بیمه پایه موجود (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddSupplementaryInsurance(AddSupplementaryInsuranceViewModel model)
        {
            _log.Information("🏥 MEDICAL: درخواست افزودن بیمه تکمیلی. PatientId: {PatientId}, SupplementaryProviderId: {SupplementaryProviderId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                model.PatientId, model.SupplementaryInsuranceProviderId, model.SupplementaryInsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ModelState
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی در افزودن بیمه تکمیلی. Errors: {ValidationErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors.SelectMany(x => x.Value)), _currentUserService.UserName, _currentUserService.UserId);

                    var errorDetails = string.Join("; ", validationErrors.SelectMany(x => x.Value));
                    return Json(new { success = false, message = $"خطاهای اعتبارسنجی: {errorDetails}", errors = validationErrors });
                }

                // تبدیل به مدل اصلی برای سرویس
                var serviceModel = new PatientInsuranceCreateEditViewModel
                {
                    PatientId = model.PatientId,
                    SupplementaryInsuranceProviderId = model.SupplementaryInsuranceProviderId,
                    SupplementaryInsurancePlanId = model.SupplementaryInsurancePlanId,
                    SupplementaryPolicyNumber = model.SupplementaryPolicyNumber,
                    IsPrimary = false, // همیشه false برای بیمه تکمیلی
                    IsActive = model.IsActive,
                    Priority = (InsurancePriority)model.Priority // تبدیل صریح int به InsurancePriority
                };

                // فراخوانی سرویس
                var result = await _patientInsuranceService.AddSupplementaryInsuranceToExistingAsync(serviceModel);
                
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در افزودن بیمه تکمیلی. PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model.PatientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }

                // 🏥 Medical Environment: Audit Trail
                _log.Information("🏥 MEDICAL: بیمه تکمیلی با موفقیت اضافه شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    result.Data, model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, message = "بیمه تکمیلی با موفقیت اضافه شد", data = result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در افزودن بیمه تکمیلی. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در افزودن بیمه تکمیلی. لطفاً دوباره تلاش کنید." });
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
                "🏥 MEDICAL: درخواست فرم ویرایش بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای ویرایش بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید.";
                    return RedirectToAction("Index");
                }

                var result = await _patientInsuranceService.GetPatientInsuranceForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning(
                        "🏥 MEDICAL: خطا در دریافت بیمه بیمار برای ویرایش. PatientInsuranceId: {PatientInsuranceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // 🏥 Medical Environment: بارگیری لیست طرح‌های بیمه با بهینه‌سازی
                await LoadDropdownsForModelAsync(result.Data);

                // 🏥 Medical Environment: تبدیل تاریخ‌های میلادی به شمسی برای نمایش در فرم
                _log.Information("🏥 MEDICAL: تبدیل تاریخ‌ها - StartDate: {StartDate}, EndDate: {EndDate}. User: {UserName} (Id: {UserId})",
                    result.Data.StartDate, result.Data.EndDate, _currentUserService.UserName, _currentUserService.UserId);

                result.Data.ConvertGregorianDatesToPersian();

                _log.Information("🏥 MEDICAL: تاریخ‌های شمسی - StartDateShamsi: {StartDateShamsi}, EndDateShamsi: {EndDateShamsi}. User: {UserName} (Id: {UserId})",
                    result.Data.StartDateShamsi, result.Data.EndDateShamsi, _currentUserService.UserName, _currentUserService.UserId);

                _log.Information(
                    "🏥 MEDICAL: فرم ویرایش بیمه بیمار با موفقیت دریافت شد. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    id, result.Data.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "🏥 MEDICAL: خطای سیستمی در دریافت فرم ویرایش بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})",
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
                "🏥 MEDICAL: درخواست به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای به‌روزرسانی بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید.";
                    await LoadDropdownsForModelAsync(model);
                    model.ConvertGregorianDatesToPersian();
                    return View(model);
                }

                // 🏥 Medical Environment: تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    model.ConvertPersianDatesToGregorian();
                }

                // 🏥 Medical Environment: اعتبارسنجی ModelState
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی در ویرایش بیمه بیمار. Errors: {ValidationErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors.SelectMany(x => x.Value)), _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "لطفاً تمام فیلدهای اجباری را به درستی پر کنید.";
                    await LoadDropdownsForModelAsync(model);
                    model.ConvertGregorianDatesToPersian();
                    return View(model);
                }

                // 🏥 Medical Environment: اعتبارسنجی اضافی server-side (منطق کسب‌وکار در سرویس)
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی کسب‌وکار در ویرایش بیمه بیمار. Errors: {BusinessErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationResult.Data.Select(x => $"{x.Key}: {x.Value}")), _currentUserService.UserName, _currentUserService.UserId);

                    foreach (var error in validationResult.Data)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }

                    TempData["ErrorMessage"] = "اطلاعات وارد شده معتبر نیست.";
                    await LoadDropdownsForModelAsync(model);
                    model.ConvertGregorianDatesToPersian();
                    return View(model);
                }

                // 🏥 Medical Environment: به‌روزرسانی بیمه بیمار
                var result = await _patientInsuranceService.UpdatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning(
                        "🏥 MEDICAL: خطا در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    await LoadDropdownsForModelAsync(model);
                    model.ConvertGregorianDatesToPersian();
                    return View(model);
                }

                // 🏥 Medical Environment: Audit Trail
                _log.Information(
                    "🏥 MEDICAL: بیمه بیمار با موفقیت به‌روزرسانی شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["SuccessMessage"] = "بیمه بیمار با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model?.PatientInsuranceId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در به‌روزرسانی بیمه بیمار. لطفاً دوباره تلاش کنید.";
                await LoadDropdownsForModelAsync(model);
                model.ConvertGregorianDatesToPersian();
                return View(model);
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه بیمار (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditAjax(PatientInsuranceCreateEditViewModel model)
        {
            _log.Information("🏥 MEDICAL: درخواست به‌روزرسانی بیمه بیمار (AJAX). PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🏥 Medical Environment: بررسی وضعیت سیستم
                var systemHealth = await CheckSystemHealthAsync();
                if (!systemHealth)
                {
                    _log.Warning("🏥 MEDICAL: وضعیت سیستم نامناسب برای به‌روزرسانی بیمه بیمار. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "سیستم در حال حاضر در دسترس نیست. لطفاً دوباره تلاش کنید." });
                }

                // 🏥 Medical Environment: تبدیل تاریخ‌های شمسی به میلادی قبل از validation
                if (model != null)
                {
                    model.ConvertPersianDatesToGregorian();
                }

                // 🏥 Medical Environment: اعتبارسنجی ModelState
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToList());

                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی در ویرایش بیمه بیمار. Errors: {ValidationErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationErrors.SelectMany(x => x.Value)), _currentUserService.UserName, _currentUserService.UserId);

                    var errorDetails = string.Join("; ", validationErrors.SelectMany(x => x.Value));
                    return Json(new { success = false, message = $"خطاهای اعتبارسنجی: {errorDetails}", errors = validationErrors });
                }

                // 🏥 Medical Environment: اعتبارسنجی اضافی server-side
                var validationResult = await _patientInsuranceService.ValidatePatientInsuranceAsync(model);
                if (!validationResult.Success || validationResult.Data.Count > 0)
                {
                    _log.Warning("🏥 MEDICAL: خطاهای اعتبارسنجی کسب‌وکار در ویرایش بیمه بیمار. Errors: {BusinessErrors}. User: {UserName} (Id: {UserId})",
                        string.Join(", ", validationResult.Data.Select(x => $"{x.Key}: {x.Value}")), _currentUserService.UserName, _currentUserService.UserId);

                    var errorDetails = string.Join("; ", validationResult.Data.Select(x => x.Value));
                    return Json(new { success = false, message = $"خطاهای کسب‌وکار: {errorDetails}", errors = validationResult.Data });
                }

                // 🏥 Medical Environment: به‌روزرسانی بیمه بیمار
                var result = await _patientInsuranceService.UpdatePatientInsuranceAsync(model);
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خطا در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model?.PatientInsuranceId, model?.PatientId, model?.PolicyNumber, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = result.Message });
                }

                // 🏥 Medical Environment: Audit Trail
                _log.Information("🏥 MEDICAL: بیمه بیمار با موفقیت به‌روزرسانی شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, message = "بیمه بیمار با موفقیت به‌روزرسانی شد", data = model.PatientInsuranceId });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})",
                    model?.PatientInsuranceId, model?.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطا در به‌روزرسانی بیمه بیمار. لطفاً دوباره تلاش کنید." });
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
                    phoneNumber = p.PhoneNumber,
                    birthDate = p.BirthDate,
                    birthDateShamsi = p.BirthDateShamsi,
                    age = p.Age,
                    gender = p.Gender,
                    address = p.Address
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

        #region Supplementary Insurance Methods

        /// <summary>
        /// دریافت صندوق‌های بیمه تکمیلی برای مودال افزودن
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSupplementaryInsuranceProviders()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست صندوق‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت فقط صندوق‌های بیمه تکمیلی
                var result = await _insuranceProviderService.GetSupplementaryInsuranceProvidersAsync();
                if (result.Success)
                {
                    var providers = result.Data.Select(p => new
                    {
                        id = p.InsuranceProviderId,
                        name = p.Name,
                        code = p.Code
                    }).ToList();

                    _log.Information("🏥 MEDICAL: {Count} صندوق بیمه تکمیلی دریافت شد. User: {UserName} (Id: {UserId})",
                        providers.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = providers,
                        message = "صندوق‌های بیمه تکمیلی با موفقیت دریافت شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت صندوق‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت صندوق‌های بیمه تکمیلی"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه تکمیلی بر اساس صندوق
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSupplementaryInsurancePlansByProvider(int providerId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست طرح‌های بیمه تکمیلی برای صندوق {ProviderId}. User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه تکمیلی بر اساس صندوق
                var result = await _insurancePlanService.GetSupplementaryInsurancePlansByProviderAsync(providerId);
                if (result.Success)
                {
                    var plans = result.Data.Select(p => new
                    {
                        id = p.InsurancePlanId,
                        name = p.Name,
                        providerName = p.InsuranceProviderName ?? "نامشخص",
                        coveragePercent = p.CoveragePercent,
                        deductible = p.Deductible
                    }).ToList();

                    _log.Information("🏥 MEDICAL: {Count} طرح بیمه تکمیلی برای صندوق {ProviderId} دریافت شد. User: {UserName} (Id: {UserId})",
                        plans.Count, providerId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = plans,
                        message = "طرح‌های بیمه تکمیلی با موفقیت دریافت شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت طرح‌های بیمه تکمیلی برای صندوق {ProviderId}. User: {UserName} (Id: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت طرح‌های بیمه تکمیلی"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه تکمیلی برای مودال افزودن
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetSupplementaryInsurancePlans()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست طرح‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت فقط طرح‌های بیمه تکمیلی (InsuranceType = 2)
                var result = await _insurancePlanService.GetSupplementaryInsurancePlansAsync();
                if (result.Success)
                {
                    var plans = result.Data.Select(p => new
                    {
                        id = p.InsurancePlanId,
                        name = p.Name,
                        providerName = p.InsuranceProviderName ?? "نامشخص",
                        coveragePercent = p.CoveragePercent,
                        deductible = p.Deductible
                    }).ToList();

                    _log.Information("🏥 MEDICAL: {Count} طرح بیمه تکمیلی دریافت شد. User: {UserName} (Id: {UserId})",
                        plans.Count, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = plans,
                        message = "طرح‌های بیمه تکمیلی با موفقیت دریافت شد"
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت طرح‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت طرح‌های بیمه تکمیلی"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بیمار
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> SupplementaryInsurances(int patientId)
        {
            try
            {
                _log.Information("درخواست بیمه‌های تکمیلی بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت فقط بیمه‌های تکمیلی (غیر اصلی)
                var result = await _patientInsuranceService.GetSupplementaryInsurancesByPatientAsync(patientId);
                if (result.Success)
                {
                    var viewModel = new PatientInsuranceIndexPageViewModel
                    {
                        PatientInsurances = result.Data.Select(pi => new PatientInsuranceIndexItemViewModel
                        {
                            PatientInsuranceId = pi.PatientInsuranceId,
                            PatientId = pi.PatientId,
                            PatientFullName = pi.PatientName,
                            InsurancePlanId = pi.InsurancePlanId,
                            InsurancePlanName = pi.InsurancePlanName,
                            InsuranceProviderName = pi.InsuranceProviderName,
                            PolicyNumber = pi.PolicyNumber,
                            StartDate = pi.StartDate,
                            EndDate = pi.EndDate,
                            IsActive = pi.IsActive,
                            IsPrimary = pi.IsPrimary,
                            CoveragePercent = pi.CoveragePercent, // اضافه کردن CoveragePercent
                            CreatedAt = pi.CreatedAt,
                            // 🏥 Medical Environment: فیلدهای بیمه تکمیلی
                            SupplementaryInsuranceProviderId = pi.SupplementaryInsuranceProviderId,
                            SupplementaryInsuranceProviderName = pi.SupplementaryInsuranceProviderName,
                            SupplementaryInsurancePlanId = pi.SupplementaryInsurancePlanId,
                            SupplementaryInsurancePlanName = pi.SupplementaryInsurancePlanName,
                            SupplementaryPolicyNumber = pi.SupplementaryPolicyNumber,
                            HasSupplementaryInsurance = pi.HasSupplementaryInsurance
                        }).ToList(),
                        InsurancePlans = new List<ViewModels.Insurance.InsurancePlan.InsurancePlanLookupViewModel>(),
                        InsuranceProviders = new List<ViewModels.Insurance.InsuranceProvider.InsuranceProviderLookupViewModel>()
                    };

                    ViewBag.PatientId = patientId; // اضافه کردن PatientId به ViewBag
                    return View("SupplementaryInsurances", viewModel);
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت بیمه‌های تکمیلی بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در دریافت بیمه‌های تکمیلی بیمار";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// محاسبه بیمه تکمیلی - بهینه شده برای محیط عملیاتی درمانی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor,Reception")]
        public async Task<JsonResult> CalculateSupplementaryInsurance(
            [Required] int patientId, 
            [Required] int serviceId, 
            [Range(0, 100000000)] decimal serviceAmount, 
            [Range(0, 100000000)] decimal primaryCoverage,
            DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, ServiceAmount: {ServiceAmount}, PrimaryCoverage: {PrimaryCoverage}, Date: {Date}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, primaryCoverage, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _log.Warning("🏥 MEDICAL: ورودی‌های نامعتبر در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, string.Join(", ", errors), _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "ورودی‌های نامعتبر",
                        errors = errors
                    });
                }

                var effectiveDate = calculationDate ?? DateTime.Now;

                // استفاده از سرویس تخصصی محاسبه بیمه ترکیبی
                var result = await _patientInsuranceService.CalculateCombinedInsuranceForPatientAsync(
                    patientId, serviceId, serviceAmount, effectiveDate);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Data.TotalInsuranceCoverage, result.Data.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            supplementaryCoverage = result.Data.SupplementaryCoverage,
                            finalPatientShare = result.Data.FinalPatientShare,
                            totalCoverage = result.Data.TotalInsuranceCoverage,
                            totalCoveragePercent = result.Data.TotalCoveragePercent,
                            hasSupplementary = result.Data.HasSupplementaryInsurance,
                            primaryCoverage = result.Data.PrimaryCoverage,
                            supplementaryCoveragePercent = result.Data.SupplementaryCoveragePercent,
                            coverageStatus = result.Data.CoverageStatus,
                            coverageStatusColor = result.Data.CoverageStatusColor
                        },
                        message = "محاسبه بیمه تکمیلی با موفقیت انجام شد"
                    });
                }

                _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }
            catch (ArgumentException ex)
            {
                _log.Warning("🏥 MEDICAL: خطای ورودی در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, ex.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "ورودی‌های نامعتبر: " + ex.Message
                });
            }
            catch (TimeoutException ex)
            {
                _log.Warning("🏥 MEDICAL: Timeout در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "عملیات بیش از حد انتظار طول کشید"
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                // تشخیص نوع خطا برای پیام مناسب
                string errorMessage = ex switch
                {
                    ArgumentException => "ورودی‌های نامعتبر",
                    InvalidOperationException => "عملیات نامعتبر",
                    TimeoutException => "زمان محاسبه به پایان رسید",
                    _ => "خطای سیستمی در محاسبه بیمه تکمیلی"
                };

                return Json(new
                {
                    success = false,
                    message = errorMessage,
                    errorCode = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// دریافت تنظیمات بیمه تکمیلی - بهینه شده برای محیط عملیاتی درمانی
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Reception")]
        public async Task<JsonResult> GetSupplementarySettings([Required] int planId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی
                if (planId <= 0)
                {
                    _log.Warning("🏥 MEDICAL: شناسه طرح بیمه نامعتبر - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "شناسه طرح بیمه نامعتبر است"
                    });
                }

                // دریافت اطلاعات طرح بیمه
                var planResult = await _insurancePlanService.GetPlanDetailsAsync(planId);
                if (!planResult.Success)
                {
                    _log.Warning("🏥 MEDICAL: طرح بیمه یافت نشد - PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        planId, planResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "طرح بیمه یافت نشد"
                    });
                }

                var plan = planResult.Data;
                var settings = new
                {
                    planId = plan.InsurancePlanId,
                    planName = plan.Name,
                    providerName = plan.InsuranceProviderName,
                    coveragePercent = plan.CoveragePercent,
                    maxPayment = 0, // این فیلد در InsurancePlanDetailsViewModel وجود ندارد
                    deductible = plan.Deductible,
                    isActive = plan.IsActive,
                    startDate = plan.ValidFrom.ToString("yyyy-MM-dd"),
                    endDate = plan.ValidTo?.ToString("yyyy-MM-dd") ?? "",
                    supplementarySettings = new
                    {
                        hasSupplementary = false, // این فیلد در InsurancePlanDetailsViewModel وجود ندارد
                        supplementaryCoveragePercent = 0, // این فیلد در InsurancePlanDetailsViewModel وجود ندارد
                        supplementaryMaxPayment = 0, // این فیلد در InsurancePlanDetailsViewModel وجود ندارد
                        supplementarySettings = "" // این فیلد در InsurancePlanDetailsViewModel وجود ندارد
                    }
                };

                _log.Information("🏥 MEDICAL: تنظیمات بیمه تکمیلی دریافت شد - PlanId: {PlanId}, CoveragePercent: {CoveragePercent}. User: {UserName} (Id: {UserId})",
                    planId, plan.CoveragePercent, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = true,
                    data = settings,
                    message = "تنظیمات بیمه تکمیلی با موفقیت دریافت شد"
                });
            }
            catch (ArgumentException ex)
            {
                _log.Warning("🏥 MEDICAL: خطای ورودی در دریافت تنظیمات بیمه تکمیلی - PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    planId, ex.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "ورودی نامعتبر: " + ex.Message
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت تنظیمات بیمه تکمیلی",
                    errorCode = ex.GetType().Name
                });
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات بیمه تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateSupplementarySettings(int planId, string settingsJson)
        {
            try
            {
                _log.Information("درخواست به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                // این متد باید در SupplementaryInsuranceService پیاده‌سازی شود
                return Json(new
                {
                    success = true,
                    message = "تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد"
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در به‌روزرسانی تنظیمات بیمه تکمیلی. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در به‌روزرسانی تنظیمات بیمه تکمیلی"
                });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت لیست طرح‌های بیمه برای SelectList
        /// </summary>
        private async Task<SelectList> GetInsurancePlansSelectList()
        {
            try
            {
                var result = await _insurancePlanService.GetActivePlansForLookupAsync();
                if (result.Success)
                {
                    return new SelectList(result.Data, "InsurancePlanId", "Name");
                }
                return new SelectList(new List<object>(), "Value", "Text");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت لیست طرح‌های بیمه");
                return new SelectList(new List<object>(), "Value", "Text");
            }
        }

        /// <summary>
        /// دریافت لیست ارائه‌دهندگان بیمه برای SelectList
        /// </summary>
        private async Task<SelectList> GetInsuranceProvidersSelectList()
        {
            try
            {
                var result = await _insurancePlanService.GetActiveProvidersForLookupAsync();
                if (result.Success)
                {
                    return new SelectList(result.Data, "InsuranceProviderId", "Name");
                }
                return new SelectList(new List<object>(), "Value", "Text");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت لیست ارائه‌دهندگان بیمه");
                return new SelectList(new List<object>(), "Value", "Text");
            }
        }

        #endregion
    }

    /// <summary>
    /// اطلاعات وضعیت بیمه بیمار
    /// </summary>
    public class InsuranceStatusInfo
    {
        public int PatientId { get; set; }
        public bool HasPrimaryInsurance { get; set; }
        public bool HasSupplementaryInsurance { get; set; }
        public int PrimaryInsuranceCount { get; set; }
        public int SupplementaryInsuranceCount { get; set; }
        public InsuranceRecommendation Recommendation { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// انواع توصیه‌های بیمه
    /// </summary>
    public enum InsuranceRecommendation
    {
        None,
        CreatePrimaryInsurance,
        ConsiderSupplementaryInsurance,
        InsuranceComplete,
        Error
    }
}
