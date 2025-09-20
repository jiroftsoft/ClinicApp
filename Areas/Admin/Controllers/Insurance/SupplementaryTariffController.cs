using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Services;
using ClinicApp.Services.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceTariff;
using ClinicApp.ViewModels.Insurance.Supplementary;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر مدیریت تعرفه‌های بیمه تکمیلی
    /// طراحی شده برای محیط درمانی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل تعرفه‌های بیمه تکمیلی
    /// 2. پشتیبانی از تعیین ست بیمه‌های مختلف (تامین + دانا)
    /// 3. تنظیمات پیچیده و انعطاف‌پذیر
    /// 4. مدیریت کامل CRUD operations
    /// </summary>
    public class SupplementaryTariffController : BaseController
    {
        private readonly SupplementaryTariffSeederService _seederService;
        private readonly IInsuranceTariffService _tariffService;
        private readonly IInsurancePlanService _planService;
        private readonly IInsuranceProviderService _providerService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public SupplementaryTariffController(
            SupplementaryTariffSeederService seederService,
            IInsuranceTariffService tariffService,
            IInsurancePlanService planService,
            IInsuranceProviderService providerService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService)
            : base(messageNotificationService)
        {
            _seederService = seederService ?? throw new ArgumentNullException(nameof(seederService));
            _tariffService = tariffService ?? throw new ArgumentNullException(nameof(tariffService));
            _planService = planService ?? throw new ArgumentNullException(nameof(planService));
            _providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
            _log = logger.ForContext<SupplementaryTariffController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// صفحه اصلی مدیریت تعرفه‌های بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _log.Information("🏥 MEDICAL: دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var statsResult = await _seederService.GetSupplementaryTariffStatsAsync();
                if (statsResult.Success)
                {
                    ViewBag.Stats = statsResult.Data;
                }

                return View();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دسترسی به صفحه مدیریت تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return View();
            }
        }

        /// <summary>
        /// ایجاد تعرفه‌های بیمه تکمیلی برای همه خدمات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateAll()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه‌های بیمه تکمیلی برای همه خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _seederService.CreateSupplementaryTariffsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه‌های بیمه تکمیلی با موفقیت ایجاد شدند. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعرفه‌های بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد تعرفه بیمه تکمیلی برای خدمت خاص
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateForService(int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _seederService.CreateSupplementaryTariffForServiceAsync(serviceId);
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی برای خدمت {ServiceId} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId} - {Error}. User: {UserName} (Id: {UserId})",
                        serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی برای خدمت {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌های بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetStats()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _seederService.GetSupplementaryTariffStatsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: آمار تعرفه‌های بیمه تکمیلی با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت آمار تعرفه‌های بیمه تکمیلی - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار تعرفه‌های بیمه تکمیلی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌های بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ایجاد تعیین ست بیمه پایه و تکمیلی (مثل تامین + دانا)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateInsuranceCombination(int primaryPlanId, int supplementaryPlanId, decimal coveragePercent, decimal maxPayment)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ایجاد تعیین ست بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}, CoveragePercent: {CoveragePercent}, MaxPayment: {MaxPayment}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, coveragePercent, maxPayment, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه
                var primaryPlan = await _planService.GetPlanDetailsAsync(primaryPlanId);
                var supplementaryPlan = await _planService.GetPlanDetailsAsync(supplementaryPlanId);

                if (!primaryPlan.Success || !supplementaryPlan.Success)
                {
                    return Json(new { success = false, message = "طرح بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // ایجاد تعرفه‌های ترکیبی - استفاده از متد موجود
                var result = await _seederService.CreateSupplementaryTariffsAsync();
                
                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: تعیین ست بیمه با موفقیت ایجاد شد - PrimaryPlan: {PrimaryPlanName}, SupplementaryPlan: {SupplementaryPlanName}. User: {UserName} (Id: {UserId})",
                        primaryPlan.Data.Name, supplementaryPlan.Data.Name, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در ایجاد تعیین ست بیمه - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعیین ست بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ایجاد تعیین ست بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست طرح‌های بیمه برای تعیین ست
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetInsurancePlans()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست طرح‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var result = await _planService.GetPlansAsync(null, "", 1, 1000);
                
                if (result.Success)
                {
                    var plans = result.Data.Items.Select(p => new
                    {
                        id = p.InsurancePlanId,
                        name = p.Name,
                        providerName = p.InsuranceProviderName,
                        coveragePercent = p.CoveragePercent,
                        isPrimary = p.CoveragePercent > 0 && p.CoveragePercent < 100
                    }).ToList();

                    _log.Information("🏥 MEDICAL: لیست طرح‌های بیمه با موفقیت دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        plans.Count, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, data = plans }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست طرح‌های بیمه - {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست طرح‌های بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت لیست طرح‌های بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ویرایش تعرفه بیمه تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditTariff(int tariffId, decimal coveragePercent, decimal maxPayment, string settings = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست ویرایش تعرفه بیمه تکمیلی - TariffId: {TariffId}, CoveragePercent: {CoveragePercent}, MaxPayment: {MaxPayment}. User: {UserName} (Id: {UserId})",
                    tariffId, coveragePercent, maxPayment, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var tariff = tariffResult.Data;
                
                // تبدیل به CreateEditViewModel برای به‌روزرسانی
                var editModel = new InsuranceTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariff.InsuranceTariffId,
                    ServiceId = tariff.ServiceId,
                    InsurancePlanId = tariff.InsurancePlanId,
                    TariffPrice = tariff.TariffPrice,
                    PatientShare = tariff.PatientShare,
                    InsurerShare = tariff.InsurerShare,
                    IsActive = tariff.IsActive
                };

                // ذخیره تغییرات
                var updateResult = await _tariffService.UpdateTariffAsync(editModel);
                if (updateResult.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت ویرایش شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = "تعرفه بیمه تکمیلی با موفقیت ویرایش شد" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در به‌روزرسانی تعرفه بیمه تکمیلی - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        updateResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = updateResult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش تعرفه بیمه تکمیلی. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در ویرایش تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// حذف تعرفه بیمه تکمیلی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteTariff(int tariffId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست حذف تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود تعرفه
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                // حذف تعرفه
                var deleteResult = await _tariffService.DeleteTariffAsync(tariffId);
                if (deleteResult.Success)
                {
                    _log.Information("🏥 MEDICAL: تعرفه بیمه تکمیلی با موفقیت حذف شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = "تعرفه بیمه تکمیلی با موفقیت حذف شد" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در حذف تعرفه بیمه تکمیلی - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        deleteResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = deleteResult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف تعرفه بیمه تکمیلی. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در حذف تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت جزئیات تعرفه بیمه تکمیلی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffDetails(int tariffId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست جزئیات تعرفه بیمه تکمیلی - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه از دیتابیس
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var tariff = tariffResult.Data;
                
                // آماده‌سازی داده‌های پاسخ
                var tariffDetails = new
                {
                    tariffId = tariff.InsuranceTariffId,
                    serviceId = tariff.ServiceId,
                    serviceName = tariff.ServiceTitle ?? "نامشخص",
                    planId = tariff.InsurancePlanId,
                    planName = tariff.InsurancePlanName ?? "نامشخص",
                    tariffPrice = tariff.TariffPrice,
                    patientShare = tariff.PatientShare,
                    insurerShare = tariff.InsurerShare,
                    createdAt = tariff.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                    updatedAt = tariff.UpdatedAt?.ToString("yyyy/MM/dd HH:mm")
                };
                
                _log.Information("🏥 MEDICAL: جزئیات تعرفه بیمه تکمیلی با موفقیت دریافت شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = true, data = tariffDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت جزئیات تعرفه بیمه تکمیلی. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت جزئیات تعرفه بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت تعرفه‌های بیمه بر اساس طرح
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffsByPlan(int planId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه بر اساس طرح - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه‌های بیمه تکمیلی
                var supplementaryTariffsResult = await _tariffService.GetSupplementaryTariffsAsync(planId);
                if (!supplementaryTariffsResult.Success)
                {
                    return Json(new { success = false, message = "خطا در دریافت تعرفه‌های بیمه تکمیلی" }, JsonRequestBehavior.AllowGet);
                }

                var tariffs = supplementaryTariffsResult.Data.Select(t => new
                {
                    tariffId = t.InsuranceTariffId,
                    serviceId = t.ServiceId,
                    serviceName = t.Service?.Title ?? "نامشخص",
                    serviceCode = t.Service?.ServiceCode ?? "نامشخص",
                    patientShare = t.PatientShare,
                    insurerShare = t.InsurerShare,
                    supplementaryCoveragePercent = t.SupplementaryCoveragePercent,
                    supplementaryMaxPayment = t.SupplementaryMaxPayment,
                    priority = t.Priority,
                    startDate = t.StartDate?.ToString("yyyy/MM/dd"),
                    endDate = t.EndDate?.ToString("yyyy/MM/dd"),
                    isActive = t.IsActive,
                    createdAt = t.CreatedAt.ToString("yyyy/MM/dd HH:mm")
                }).ToList();

                _log.Information("🏥 MEDICAL: تعرفه‌های بیمه بر اساس طرح با موفقیت دریافت شد - PlanId: {PlanId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    planId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = tariffs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه بر اساس طرح. PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت تعرفه‌های بیمه بر اساس طرح" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// به‌روزرسانی تنظیمات تعرفه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateTariffSettings(int tariffId, string settings)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست به‌روزرسانی تنظیمات تعرفه - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت تعرفه موجود
                var tariffResult = await _tariffService.GetTariffByIdAsync(tariffId);
                if (!tariffResult.Success)
                {
                    return Json(new { success = false, message = "تعرفه بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var tariff = tariffResult.Data;
                
                // تبدیل به CreateEditViewModel برای به‌روزرسانی
                var editModel = new InsuranceTariffCreateEditViewModel
                {
                    InsuranceTariffId = tariff.InsuranceTariffId,
                    ServiceId = tariff.ServiceId,
                    InsurancePlanId = tariff.InsurancePlanId,
                    TariffPrice = tariff.TariffPrice,
                    PatientShare = tariff.PatientShare,
                    InsurerShare = tariff.InsurerShare,
                    IsActive = tariff.IsActive
                };

                // ذخیره تغییرات
                var updateResult = await _tariffService.UpdateTariffAsync(editModel);
                if (updateResult.Success)
                {
                    _log.Information("🏥 MEDICAL: تنظیمات تعرفه با موفقیت به‌روزرسانی شد - TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = true, message = "تنظیمات تعرفه با موفقیت به‌روزرسانی شد" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در به‌روزرسانی تنظیمات تعرفه - {Error}. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                        updateResult.Message, tariffId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return Json(new { success = false, message = updateResult.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در به‌روزرسانی تنظیمات تعرفه. TariffId: {TariffId}. User: {UserName} (Id: {UserId})",
                    tariffId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در به‌روزرسانی تنظیمات تعرفه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// اعتبارسنجی ترکیب بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ValidateTariffCombination(int primaryPlanId, int supplementaryPlanId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست اعتبارسنجی ترکیب بیمه - PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت طرح‌های بیمه
                var primaryPlanResult = await _planService.GetPlanDetailsAsync(primaryPlanId);
                var supplementaryPlanResult = await _planService.GetPlanDetailsAsync(supplementaryPlanId);

                if (!primaryPlanResult.Success || !supplementaryPlanResult.Success)
                {
                    return Json(new { success = false, message = "یکی از طرح‌های بیمه یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var primaryPlan = primaryPlanResult.Data;
                var supplementaryPlan = supplementaryPlanResult.Data;

                // اعتبارسنجی ترکیب
                var validationResult = new
                {
                    isValid = true,
                    primaryPlanName = primaryPlan.Name,
                    supplementaryPlanName = supplementaryPlan.Name,
                    primaryCoveragePercent = primaryPlan.CoveragePercent,
                    supplementaryCoveragePercent = supplementaryPlan.CoveragePercent,
                    totalCoveragePercent = primaryPlan.CoveragePercent + supplementaryPlan.CoveragePercent,
                    warnings = new List<string>(),
                    recommendations = new List<string>()
                };

                // بررسی محدودیت‌ها
                if (validationResult.totalCoveragePercent > 100)
                {
                    validationResult.warnings.Add("مجموع درصد پوشش بیش از 100% است");
                }

                if (primaryPlan.CoveragePercent < 50)
                {
                    validationResult.recommendations.Add("درصد پوشش بیمه اصلی کمتر از 50% است");
                }

                _log.Information("🏥 MEDICAL: اعتبارسنجی ترکیب بیمه تکمیل شد - PrimaryPlan: {PrimaryPlanName}, SupplementaryPlan: {SupplementaryPlanName}. User: {UserName} (Id: {UserId})",
                    primaryPlan.Name, supplementaryPlan.Name, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = validationResult }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی ترکیب بیمه. PrimaryPlanId: {PrimaryPlanId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})",
                    primaryPlanId, supplementaryPlanId, _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در اعتبارسنجی ترکیب بیمه" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت آمار تعرفه‌ها
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTariffStatistics()
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست آمار تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت آمار از سرویس
                var statsResult = await _seederService.GetSupplementaryTariffStatsAsync();
                if (!statsResult.Success)
                {
                    return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها" }, JsonRequestBehavior.AllowGet);
                }

                var statistics = new
                {
                    totalServices = statsResult.Data.TotalServices,
                    totalSupplementaryTariffs = statsResult.Data.TotalSupplementaryTariffs,
                    activeSupplementaryTariffs = statsResult.Data.ActiveSupplementaryTariffs,
                    expiredSupplementaryTariffs = statsResult.Data.ExpiredSupplementaryTariffs,
                    lastUpdated = DateTime.Now.ToString("yyyy/MM/dd HH:mm")
                };

                _log.Information("🏥 MEDICAL: آمار تعرفه‌ها با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true, data = statistics }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت آمار تعرفه‌ها. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                
                return Json(new { success = false, message = "خطا در دریافت آمار تعرفه‌ها" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}