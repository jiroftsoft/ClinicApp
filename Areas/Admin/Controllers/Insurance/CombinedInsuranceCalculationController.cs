    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ClinicApp.Core;
    using ClinicApp.Interfaces;
    using ClinicApp.Interfaces.Insurance;
    using ClinicApp.Services;
    using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
    using ClinicApp.ViewModels.Insurance.Supplementary;
    using Serilog;

    namespace ClinicApp.Areas.Admin.Controllers.Insurance
    {
        /// <summary>
        /// کنترلر محاسبه بیمه ترکیبی (اصلی + تکمیلی)
        /// طراحی شده برای کلینیک‌های درمانی
        /// </summary>
        //[Authorize] // امنیت: کنترل دسترسی
        public class CombinedInsuranceCalculationController : BaseController
        {
            private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
            private readonly ISupplementaryInsuranceService _supplementaryInsuranceService;
            private readonly ILogger _log;
            private readonly ICurrentUserService _currentUserService;

            public CombinedInsuranceCalculationController(
                ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
                ISupplementaryInsuranceService supplementaryInsuranceService,
                ILogger logger,
                ICurrentUserService currentUserService,
                IMessageNotificationService messageNotificationService)
                : base(messageNotificationService)
            {
                _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
                _supplementaryInsuranceService = supplementaryInsuranceService ?? throw new ArgumentNullException(nameof(supplementaryInsuranceService));
                _log = logger.ForContext<CombinedInsuranceCalculationController>();
                _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            }

            /// <summary>
            /// صفحه اصلی محاسبه بیمه ترکیبی
            /// </summary>
            [HttpGet]
            [OutputCache(Duration = 300)] // Cache برای 5 دقیقه
            public ActionResult Index()
            {
                _log.Information("🏥 MEDICAL: بازدید از صفحه محاسبه بیمه ترکیبی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return View();
            }

            /// <summary>
            /// محاسبه بیمه ترکیبی برای یک خدمت
            /// </summary>
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> CalculateCombinedInsurance(
                [Required] int patientId, 
                [Required] int serviceId, 
                [Range(0, double.MaxValue, ErrorMessage = "مبلغ خدمت نمی‌تواند منفی باشد")] decimal serviceAmount, 
                DateTime? calculationDate = null)
            {
                try
                {
                    // اعتبارسنجی ورودی‌ها
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                        _log.Warning("🏥 MEDICAL: ورودی‌های نامعتبر در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Errors: {Errors}. User: {UserName} (Id: {UserId})",
                            patientId, serviceId, string.Join(", ", errors), _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = "ورودی‌های نامعتبر",
                            errors = errors
                        }, JsonRequestBehavior.AllowGet);
                    }

                    _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                    var effectiveDate = calculationDate ?? DateTime.Now;

                    var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                        patientId, serviceId, serviceAmount, effectiveDate);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                            patientId, serviceId, result.Data.TotalInsuranceCoverage, result.Data.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = "محاسبه بیمه ترکیبی با موفقیت انجام شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                    // تشخیص نوع خطا برای پیام مناسب
                    string errorMessage = ex switch
                    {
                        ArgumentException => "ورودی‌های نامعتبر",
                        InvalidOperationException => "عملیات نامعتبر",
                        TimeoutException => "زمان محاسبه به پایان رسید",
                        _ => "خطای سیستمی در محاسبه بیمه ترکیبی"
                    };

                    return Json(new
                    {
                        success = false,
                        message = errorMessage,
                        errorCode = ex.GetType().Name
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// محاسبه بیمه ترکیبی برای چندین خدمت
            /// </summary>
            [HttpPost]
            [Route("CalculateMultiple")]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> CalculateCombinedInsuranceForServices(
                int patientId, 
                List<int> serviceIds, 
                List<decimal> serviceAmounts, 
                DateTime? calculationDate = null)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}, Date: {Date}. User: {UserName} (Id: {UserId})",
                        patientId, serviceIds?.Count ?? 0, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                    if (serviceIds == null || serviceAmounts == null || serviceIds.Count != serviceAmounts.Count)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "لیست خدمات و مبالغ نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var effectiveDate = calculationDate ?? DateTime.Now;

                    var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceForServicesAsync(
                        patientId, serviceIds, serviceAmounts, effectiveDate);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی برای چندین خدمت موفق - PatientId: {PatientId}, SuccessCount: {SuccessCount}, TotalCount: {TotalCount}. User: {UserName} (Id: {UserId})",
                            patientId, result.Data.Count, serviceIds.Count, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = $"محاسبه بیمه ترکیبی برای {result.Data.Count} خدمت با موفقیت انجام شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            patientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی برای چندین خدمت - PatientId: {PatientId}, ServiceCount: {ServiceCount}. User: {UserName} (Id: {UserId})",
                        patientId, serviceIds?.Count ?? 0, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در محاسبه بیمه ترکیبی برای چندین خدمت"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// دریافت اطلاعات بیمه‌های بیمار
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetPatientInsurances(int patientId)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست دریافت اطلاعات بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    // دریافت اطلاعات بیمه‌های بیمار از Service واقعی
                    var result = await _combinedInsuranceCalculationService.GetPatientInsurancesAsync(patientId);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: دریافت اطلاعات بیمه‌های بیمار موفق - PatientId: {PatientId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                            patientId, result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = $"اطلاعات بیمه‌های بیمار ({result.Data.Count} مورد) دریافت شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در دریافت اطلاعات بیمه‌های بیمار - PatientId: {PatientId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            patientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات بیمه‌های بیمار - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت اطلاعات بیمه‌های بیمار"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            #region Supplementary Insurance Methods

            /// <summary>
            /// محاسبه بیمه تکمیلی برای یک خدمت
            /// </summary>
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> CalculateSupplementaryInsurance(
                int patientId, 
                int serviceId, 
                decimal serviceAmount, 
                decimal primaryCoverage,
                DateTime? calculationDate = null)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, PrimaryCoverage: {PrimaryCoverage}, Date: {Date}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, serviceAmount, primaryCoverage, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                    var effectiveDate = calculationDate ?? DateTime.Now;

                    var result = await _supplementaryInsuranceService.CalculateSupplementaryInsuranceAsync(
                        patientId, serviceId, serviceAmount, primaryCoverage, effectiveDate);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: محاسبه بیمه تکمیلی موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, SupplementaryCoverage: {SupplementaryCoverage}, FinalPatientShare: {FinalPatientShare}. User: {UserName} (Id: {UserId})",
                            patientId, serviceId, result.Data.SupplementaryCoverage, result.Data.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = "محاسبه بیمه تکمیلی با موفقیت انجام شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه تکمیلی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در محاسبه بیمه تکمیلی"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// دریافت تعرفه‌های بیمه تکمیلی برای یک طرح بیمه
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetSupplementaryTariffs(int planId)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست تعرفه‌های بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    var result = await _supplementaryInsuranceService.GetSupplementaryTariffsAsync(planId);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: دریافت تعرفه‌های بیمه تکمیلی موفق - PlanId: {PlanId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                            planId, result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = $"تعرفه‌های بیمه تکمیلی ({result.Data.Count} مورد) دریافت شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در دریافت تعرفه‌های بیمه تکمیلی - PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            planId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت تعرفه‌های بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت تعرفه‌های بیمه تکمیلی"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// دریافت تنظیمات بیمه تکمیلی برای یک طرح بیمه
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetSupplementarySettings(int planId)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    var result = await _supplementaryInsuranceService.GetSupplementarySettingsAsync(planId);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: دریافت تنظیمات بیمه تکمیلی موفق - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                            planId, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = "تنظیمات بیمه تکمیلی دریافت شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در دریافت تنظیمات بیمه تکمیلی - PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            planId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت تنظیمات بیمه تکمیلی"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// به‌روزرسانی تنظیمات بیمه تکمیلی
            /// </summary>
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> UpdateSupplementarySettings(int planId, SupplementarySettings settings)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست به‌روزرسانی تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    var result = await _supplementaryInsuranceService.UpdateSupplementarySettingsAsync(planId, settings);

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: به‌روزرسانی تنظیمات بیمه تکمیلی موفق - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                            planId, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            message = "تنظیمات بیمه تکمیلی با موفقیت به‌روزرسانی شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در به‌روزرسانی تنظیمات بیمه تکمیلی - PlanId: {PlanId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            planId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در به‌روزرسانی تنظیمات بیمه تکمیلی - PlanId: {PlanId}. User: {UserName} (Id: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در به‌روزرسانی تنظیمات بیمه تکمیلی"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// دریافت لیست بیماران برای محاسبه بیمه
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetPatients()
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست لیست بیماران. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    // این متد باید از PatientService استفاده کند
                    var result = await _combinedInsuranceCalculationService.GetActivePatientsAsync();

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: لیست بیماران دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                            result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = $"لیست بیماران ({result.Data.Count} مورد) دریافت شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در دریافت لیست بیماران - Error: {Error}. User: {UserName} (Id: {UserId})",
                            result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست بیماران. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت لیست بیماران"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// دریافت لیست خدمات برای محاسبه بیمه
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetServices()
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست لیست خدمات. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    // این متد باید از ServiceService استفاده کند
                    var result = await _combinedInsuranceCalculationService.GetActiveServicesAsync();

                    if (result.Success)
                    {
                        _log.Information("🏥 MEDICAL: لیست خدمات دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                            result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = true,
                            data = result.Data,
                            message = $"لیست خدمات ({result.Data.Count} مورد) دریافت شد"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: خطا در دریافت لیست خدمات - Error: {Error}. User: {UserName} (Id: {UserId})",
                            result.Message, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new
                        {
                            success = false,
                            message = result.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست خدمات. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در دریافت لیست خدمات"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            #endregion
        }
    }
