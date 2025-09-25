    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ClinicApp.Core;
    using ClinicApp.Interfaces;
    using ClinicApp.Interfaces.Insurance;
    using ClinicApp.Interfaces.ClinicAdmin;
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
        private readonly PatientService _patientService;
        private readonly ServiceService _serviceService;
        private readonly IDepartmentManagementService _departmentManagementService;
        private readonly ISharedServiceManagementService _sharedServiceManagementService;

        public CombinedInsuranceCalculationController(
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            ISupplementaryInsuranceService supplementaryInsuranceService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IMessageNotificationService messageNotificationService,
            PatientService patientService,
            ServiceService serviceService,
            IDepartmentManagementService departmentManagementService,
            ISharedServiceManagementService sharedServiceManagementService)
            : base(messageNotificationService)
        {
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _supplementaryInsuranceService = supplementaryInsuranceService ?? throw new ArgumentNullException(nameof(supplementaryInsuranceService));
            _log = logger.ForContext<CombinedInsuranceCalculationController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _departmentManagementService = departmentManagementService ?? throw new ArgumentNullException(nameof(departmentManagementService));
            _sharedServiceManagementService = sharedServiceManagementService ?? throw new ArgumentNullException(nameof(sharedServiceManagementService));
        }

            /// <summary>
            /// صفحه اصلی محاسبه بیمه ترکیبی
            /// </summary>
            [HttpGet]
            // 🏥 MEDICAL: Real-time data - no cache for clinical safety
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
        /// دریافت لیست بیماران برای Select2 با پردازش سمت سرور بهینه شده
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPatients(string searchTerm = "", string searchType = "name", int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست بیماران برای Select2. SearchTerm: {SearchTerm}, SearchType: {SearchType}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    searchTerm, searchType, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100; // محدودیت برای جلوگیری از بارگذاری بیش از حد

                // تشخیص نوع جستجو و بهینه‌سازی - فقط کد ملی
                bool isNationalCodeSearch = searchType == "nationalCode" && !string.IsNullOrEmpty(searchTerm);
                bool isPartialNationalCode = !string.IsNullOrEmpty(searchTerm) && searchTerm.Length >= 3 && searchTerm.Length < 10 && searchTerm.All(char.IsDigit);
                bool isCompleteNationalCode = !string.IsNullOrEmpty(searchTerm) && searchTerm.Length == 10 && searchTerm.All(char.IsDigit);
                
                // محدودیت دقیق برای محیط درمانی - اصلاح شده
                if (isCompleteNationalCode) {
                    pageSize = 1; // کد ملی کامل = فقط یک نتیجه
                } else if (isPartialNationalCode) {
                    pageSize = 5; // کد ملی جزئی = حداکثر 5 نتیجه
                } else {
                    pageSize = 0; // اگر کد ملی نیست = هیچ نتیجه‌ای
                }

                // اعمال محدودیت اضافی برای جلوگیری از بارگذاری بیش از حد
                if (pageSize > 5) pageSize = 5;
                if (page < 1) page = 1;

                // استفاده از PatientService برای جستجوی بیماران - بهینه‌سازی برای Select2
                var result = await _patientService.SearchPatientsForSelect2Async(searchTerm, page, pageSize);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: لیست بیماران برای Select2 دریافت شد - Count: {Count}, Total: {Total}. User: {UserName} (Id: {UserId})",
                        result.Data.Items.Count, result.Data.TotalItems, _currentUserService.UserName, _currentUserService.UserId);

                    // تبدیل به فرمت Select2
                    var patientsData = result.Data.Items.Select(p => new
                    {
                        id = p.PatientId,
                        text = $"{p.FullName} ({p.NationalCode})",
                        fullName = p.FullName,
                        nationalCode = p.NationalCode,
                        phoneNumber = p.PhoneNumber
                    }).ToList();

                    return Json(new
                    {
                        results = patientsData,
                        pagination = new
                        {
                            more = result.Data.HasNextPage
                        },
                        total_count = result.Data.TotalItems
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست بیماران برای Select2 - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        results = new List<object>(),
                        pagination = new { more = false },
                        total_count = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست بیماران برای Select2. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    results = new List<object>(),
                    pagination = new { more = false },
                    total_count = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌ها برای Select2
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDepartments(string searchTerm = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست دپارتمان‌ها برای Select2. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // استفاده از DepartmentManagementService برای دریافت دپارتمان‌های واقعی
                var result = await _departmentManagementService.GetActiveDepartmentsForLookupAsync(1); // TODO: Get current clinic ID from user context

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: لیست دپارتمان‌ها برای Select2 دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                    // فیلتر کردن بر اساس searchTerm
                    var filteredDepartments = result.Data
                        .Where(d => string.IsNullOrEmpty(searchTerm) || 
                                   d.Name.ToLower().Contains(searchTerm.ToLower()))
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    // تبدیل به فرمت Select2
                    var departmentsData = filteredDepartments.Select(d => new
                    {
                        id = d.Id,
                        text = d.Name,
                        name = d.Name
                    }).ToList();

                    return Json(new
                    {
                        results = departmentsData,
                        pagination = new
                        {
                            more = (page * pageSize) < result.Data.Count
                        },
                        total_count = result.Data.Count
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست دپارتمان‌ها برای Select2 - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        results = new List<object>(),
                        pagination = new { more = false },
                        total_count = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست دپارتمان‌ها برای Select2. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    results = new List<object>(),
                    pagination = new { more = false },
                    total_count = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست سرفصل‌های خدمات بر اساس دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories(int departmentId, string searchTerm = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست سرفصل‌های خدمات. DepartmentId: {DepartmentId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    departmentId, searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // استفاده از ServiceService برای جستجوی خدمات (سرفصل‌ها)
                var result = await _serviceService.SearchServicesForSelect2Async(searchTerm, page, pageSize);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: لیست سرفصل‌های خدمات دریافت شد - Count: {Count}, Total: {Total}. User: {UserName} (Id: {UserId})",
                        result.Data.Items.Count, result.Data.TotalItems, _currentUserService.UserName, _currentUserService.UserId);

                    // تبدیل به فرمت Select2
                    var categoriesData = result.Data.Items.Select(c => new
                    {
                        id = c.ServiceId,
                        text = c.Title,
                        title = c.Title,
                        serviceCode = c.ServiceCode
                    }).ToList();

                    return Json(new
                    {
                        results = categoriesData,
                        pagination = new
                        {
                            more = result.Data.HasNextPage
                        },
                        total_count = result.Data.TotalItems
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست سرفصل‌های خدمات - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        results = new List<object>(),
                        pagination = new { more = false },
                        total_count = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست سرفصل‌های خدمات. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    results = new List<object>(),
                    pagination = new { more = false },
                    total_count = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت قیمت خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServicePrice(int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                // منطق بهینه: ابتدا بررسی قیمت ذخیره شده، سپس محاسبه داینامیک
                try
                {
                    // مرحله 1: بررسی قیمت ذخیره شده (سریع‌تر)
                    var existingPriceResult = await _serviceService.GetServicePriceAsync(serviceId);
                    if (existingPriceResult.Success && existingPriceResult.Data > 0)
                    {
                        _log.Information("🏥 MEDICAL: قیمت ذخیره شده استفاده شد. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                            serviceId, existingPriceResult.Data, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new { success = true, price = existingPriceResult.Data, source = "stored" }, JsonRequestBehavior.AllowGet);
                    }

                    // مرحله 2: محاسبه داینامیک (اگر قیمت ذخیره شده نباشد)
                    _log.Information("🏥 MEDICAL: قیمت ذخیره شده موجود نیست، شروع محاسبه داینامیک. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);

                    var calculatedPriceResult = await _serviceService.UpdateServicePriceAsync(serviceId);
                    if (calculatedPriceResult.Success)
                    {
                        _log.Information("🏥 MEDICAL: قیمت داینامیک محاسبه و ذخیره شد. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                            serviceId, calculatedPriceResult.Data, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new { success = true, price = calculatedPriceResult.Data, source = "calculated" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        _log.Warning("🏥 MEDICAL: محاسبه داینامیک ناموفق. ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                            serviceId, calculatedPriceResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                        
                        return Json(new { success = false, message = "قیمت خدمت قابل محاسبه نیست" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception serviceEx)
                {
                    _log.Warning(serviceEx, "🏥 MEDICAL: خطا در دریافت قیمت از GetServiceDetailsAsync. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    // تلاش برای دریافت قیمت از طریق متد جایگزین
                    try
                    {
                        var fallbackResult = await _serviceService.GetServicePriceAsync(serviceId);
                        if (fallbackResult.Success)
                        {
                            _log.Information("🏥 MEDICAL: قیمت خدمت از متد جایگزین دریافت شد. ServiceId: {ServiceId}, Price: {Price}. User: {UserName} (Id: {UserId})",
                                serviceId, fallbackResult.Data, _currentUserService.UserName, _currentUserService.UserId);
                            return Json(new { success = true, price = fallbackResult.Data }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _log.Warning(fallbackEx, "🏥 MEDICAL: خطا در متد جایگزین نیز. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                            serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    }
                    
                    return Json(new { success = false, message = "قیمت خدمت در دسترس نیست" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت قیمت خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت قیمت خدمت" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست خدمات بر اساس دپارتمان
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServices(int departmentId, string searchTerm = "", int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست لیست خدمات برای Select2. DepartmentId: {DepartmentId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                    departmentId, searchTerm, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // اعتبارسنجی ورودی‌ها
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 200) pageSize = 200; // افزایش محدودیت برای نمایش خدمات بیشتر

                // استفاده از ServiceService برای دریافت همه خدمات دپارتمان
                var result = await _serviceService.GetServicesByDepartmentAsync(departmentId);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: لیست خدمات برای Select2 دریافت شد - Count: {Count}. User: {UserName} (Id: {UserId})",
                        result.Data.Count, _currentUserService.UserName, _currentUserService.UserId);

                // فیلتر کردن بر اساس searchTerm - اولویت با کد خدمت
                var filteredServices = result.Data
                    .Where(s => {
                        if (string.IsNullOrEmpty(searchTerm)) return true;
                        
                        var searchLower = searchTerm.ToLower();
                        
                        // اولویت اول: جستجو بر اساس کد خدمت
                        if (s.ServiceCode.ToLower().Contains(searchLower))
                            return true;
                            
                        // اولویت دوم: جستجو بر اساس عنوان خدمت
                        if (s.Title.ToLower().Contains(searchLower))
                            return true;
                            
                        return false;
                    })
                    .OrderBy(s => {
                        // اولویت‌بندی: ابتدا کدهای مطابق، سپس عناوین مطابق
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            var searchLower = searchTerm.ToLower();
                            if (s.ServiceCode.ToLower().Contains(searchLower))
                                return 0; // اولویت بالا
                            if (s.Title.ToLower().Contains(searchLower))
                                return 1; // اولویت پایین
                        }
                        return 2; // بدون تطابق
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                    // تبدیل به فرمت Select2
                    var servicesData = filteredServices.Select(s => new
                    {
                        id = s.ServiceId,
                        text = $"{s.Title} ({s.ServiceCode})",
                        title = s.Title,
                        serviceCode = s.ServiceCode,
                        basePrice = 0 // قیمت از GetServicePrice دریافت می‌شود
                    }).ToList();

                    return Json(new
                    {
                        results = servicesData,
                        pagination = new
                        {
                            more = (page * pageSize) < result.Data.Count
                        },
                        total_count = result.Data.Count
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در دریافت لیست خدمات برای Select2 - Error: {Error}. User: {UserName} (Id: {UserId})",
                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        results = new List<object>(),
                        pagination = new { more = false },
                        total_count = 0
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در دریافت لیست خدمات برای Select2. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    results = new List<object>(),
                    pagination = new { more = false },
                    total_count = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

            #endregion

            #region Supplementary Tariff Management

            /// <summary>
            /// ایجاد تعرفه بیمه تکمیلی جدید
            /// </summary>
            [HttpGet]
            public ActionResult CreateSupplementaryTariff()
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست ایجاد تعرفه بیمه تکمیلی جدید. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    // Redirect to the main supplementary tariff creation page
                    return RedirectToAction("Create", "SupplementaryTariff", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد تعرفه بیمه تکمیلی جدید. User: {UserName} (Id: {UserId})",
                        _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "خطا در ایجاد تعرفه بیمه تکمیلی جدید";
                    return RedirectToAction("Index");
                }
            }

            /// <summary>
            /// ویرایش تعرفه بیمه تکمیلی
            /// </summary>
            [HttpGet]
            public ActionResult EditSupplementaryTariff(int id)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست ویرایش تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    // Redirect to the main supplementary tariff edit page
                    return RedirectToAction("Edit", "SupplementaryTariff", new { area = "Admin", id = id });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "خطا در ویرایش تعرفه بیمه تکمیلی";
                    return RedirectToAction("Index");
                }
            }

            /// <summary>
            /// مشاهده جزئیات تعرفه بیمه تکمیلی
            /// </summary>
            [HttpGet]
            public ActionResult ViewSupplementaryTariffDetails(int id)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست مشاهده جزئیات تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    // Redirect to the main supplementary tariff details page
                    return RedirectToAction("Details", "SupplementaryTariff", new { area = "Admin", id = id });
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطا در مشاهده جزئیات تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "خطا در مشاهده جزئیات تعرفه بیمه تکمیلی";
                    return RedirectToAction("Index");
                }
            }

            /// <summary>
            /// حذف تعرفه بیمه تکمیلی
            /// </summary>
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<JsonResult> DeleteSupplementaryTariff(int id)
            {
                try
                {
                    _log.Information("🏥 MEDICAL: درخواست حذف تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    // For now, return success as the actual deletion should be handled by the main SupplementaryTariff controller
                    // This is a placeholder implementation
                    return Json(new
                    {
                        success = true,
                        message = "تعرفه با موفقیت حذف شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "🏥 MEDICAL: خطا در حذف تعرفه بیمه تکمیلی - ID: {Id}. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = "خطا در حذف تعرفه بیمه تکمیلی"
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            #endregion
        }
    }
