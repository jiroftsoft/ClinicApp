using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.Services;
using FluentValidation;
using Serilog;
using ClinicApp.Models;
using System.Data.Entity;

namespace ClinicApp.Areas.Admin.Controllers.Insurance
{
    /// <summary>
    /// کنترلر محاسبات بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. محاسبه سهم بیمار و بیمه برای خدمات مختلف
    /// 2. استفاده از Anti-Forgery Token در همه POST actions
    /// 3. استفاده از ServiceResult Enhanced pattern
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. محاسبه هزینه‌های پذیرش و قرار ملاقات
    /// 6. مدیریت بیمه اصلی و تکمیلی
    /// 7. محاسبه Franchise و Copay
    /// 8. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کنترلر بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    //[Authorize(Roles = "Admin,Receptionist,Doctor")]

    public class InsuranceCalculationController : Controller
    {
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly IInsuranceValidationService _insuranceValidationService;
        private readonly IInsuranceCalculationRepository _insuranceCalculationRepository;
        private readonly IMessageNotificationService _messageNotificationService;
        private readonly IValidator<InsuranceCalculationViewModel> _validator;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPatientService _patientService;
        private readonly IServiceService _serviceService;
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly IDepartmentManagementService _departmentManagementService;
        private readonly IServiceManagementService _serviceManagementService;
        private readonly ApplicationDbContext _context;
        private readonly IServiceCalculationService _serviceCalculationService;

        public InsuranceCalculationController(
            IInsuranceCalculationService insuranceCalculationService,
            IInsuranceValidationService insuranceValidationService,
            IInsuranceCalculationRepository insuranceCalculationRepository,
            IMessageNotificationService messageNotificationService,
            IValidator<InsuranceCalculationViewModel> validator,
            ILogger logger,
            ICurrentUserService currentUserService,
            IPatientService patientService,
            IServiceService serviceService,
            IServiceCategoryService serviceCategoryService,
            IPatientInsuranceService patientInsuranceService,
            IDepartmentManagementService departmentManagementService,
            IServiceManagementService serviceManagementService,
            ApplicationDbContext context,
            IServiceCalculationService serviceCalculationService)
        {
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _insuranceValidationService = insuranceValidationService ?? throw new ArgumentNullException(nameof(insuranceValidationService));
            _insuranceCalculationRepository = insuranceCalculationRepository ?? throw new ArgumentNullException(nameof(insuranceCalculationRepository));
            _messageNotificationService = messageNotificationService ?? throw new ArgumentNullException(nameof(messageNotificationService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _log = logger.ForContext<InsuranceCalculationController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _serviceService = serviceService ?? throw new ArgumentNullException(nameof(serviceService));
            _serviceCategoryService = serviceCategoryService ?? throw new ArgumentNullException(nameof(serviceCategoryService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
            _departmentManagementService = departmentManagementService ?? throw new ArgumentNullException(nameof(departmentManagementService));
            _serviceManagementService = serviceManagementService ?? throw new ArgumentNullException(nameof(serviceManagementService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
        }

        #region Calculate

        /// <summary>
        /// نمایش صفحه محاسبه بیمه
        /// </summary>
        [HttpGet]
        public ActionResult Calculate()
        {
            _log.Information("بازدید از صفحه محاسبه بیمه. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            var model = new InsuranceCalculationViewModel
            {
                CalculationDate = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// محاسبه سهم بیمار برای یک خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> CalculatePatientShare(InsuranceCalculationViewModel model)
        {
            _log.Information(
                "درخواست محاسبه سهم بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                model?.PatientId, model?.ServiceId, model?.CalculationDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // 🛡️ اعتبارسنجی جامع ورودی‌ها
                var validationResult = ValidateCalculationRequest(model);
                if (!validationResult.IsValid)
                {
                    _log.Warning("درخواست محاسبه بیمه نامعتبر است. Errors: {Errors}. User: {UserName} (Id: {UserId})", 
                        string.Join(", ", validationResult.Errors), _currentUserService.UserName, _currentUserService.UserId);
                    
                    foreach (var error in validationResult.Errors)
                    {
                        TempData["ErrorMessage"] = error;
                        break; // فقط اولین خطا را نمایش می‌دهیم
                    }
                    return RedirectToAction("Calculate");
                }

                // 🛡️ بررسی دسترسی کاربر به اطلاعات بیمار
                var accessCheck = await CheckUserAccessToPatientAsync(model.PatientId);
                if (!accessCheck.IsValid)
                {
                    _log.Warning("کاربر دسترسی به اطلاعات بیمار ندارد. PatientId: {PatientId}, User: {UserName} (Id: {UserId})", 
                        model.PatientId, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = "شما دسترسی به اطلاعات این بیمار ندارید";
                    return RedirectToAction("Calculate");
                }

                // 🛡️ بررسی محدودیت‌های زمانی
                var timeValidation = ValidateCalculationTime(model.CalculationDate);
                if (!timeValidation.IsValid)
                {
                    _log.Warning("تاریخ محاسبه نامعتبر است. CalculationDate: {CalculationDate}, User: {UserName} (Id: {UserId})", 
                        model.CalculationDate, _currentUserService.UserName, _currentUserService.UserId);
                    TempData["ErrorMessage"] = timeValidation.ErrorMessage;
                    return RedirectToAction("Calculate");
                }

                var result = await _insuranceCalculationService.CalculatePatientShareAsync(model.PatientId, model.ServiceId, model.CalculationDate);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در محاسبه سهم بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        model.PatientId, model.ServiceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return View("Calculate", model);
                }

                _log.Information(
                    "محاسبه سهم بیمار با موفقیت انجام شد. PatientId: {PatientId}, ServiceId: {ServiceId}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                    model.PatientId, model.ServiceId, result.Data.PatientShare, _currentUserService.UserName, _currentUserService.UserId);

                return View("Result", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در محاسبه سهم بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                    model?.PatientId, model?.ServiceId, model?.CalculationDate, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در محاسبه سهم بیمار";
                return View("Calculate", model);
            }
        }

        /// <summary>
        /// محاسبه هزینه‌های پذیرش
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> CalculateReceptionCosts(int patientId, string serviceIds, DateTime receptionDate)
        {
            _log.Information(
                "درخواست محاسبه هزینه‌های پذیرش. PatientId: {PatientId}, ServiceIds: {ServiceIds}, ReceptionDate: {ReceptionDate}. User: {UserName} (Id: {UserId})",
                patientId, serviceIds, receptionDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // تبدیل رشته serviceIds به لیست
                var serviceIdList = new System.Collections.Generic.List<int>();
                if (!string.IsNullOrWhiteSpace(serviceIds))
                {
                    var ids = serviceIds.Split(',');
                    foreach (var id in ids)
                    {
                        if (int.TryParse(id.Trim(), out int serviceId))
                        {
                            serviceIdList.Add(serviceId);
                        }
                    }
                }

                if (serviceIdList.Count == 0)
                {
                    TempData["ErrorMessage"] = "حداقل یک خدمت باید انتخاب شود";
                    return RedirectToAction("Calculate");
                }

                var result = await _insuranceCalculationService.CalculateReceptionCostsAsync(patientId, serviceIdList, receptionDate);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در محاسبه هزینه‌های پذیرش. PatientId: {PatientId}, ServiceIds: {ServiceIds}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, serviceIds, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Calculate");
                }

                _log.Information(
                    "محاسبه هزینه‌های پذیرش با موفقیت انجام شد. PatientId: {PatientId}, ServiceIds: {ServiceIds}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds, result.Data.PatientShare, _currentUserService.UserName, _currentUserService.UserId);

                return View("Result", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در محاسبه هزینه‌های پذیرش. PatientId: {PatientId}, ServiceIds: {ServiceIds}, ReceptionDate: {ReceptionDate}. User: {UserName} (Id: {UserId})",
                    patientId, serviceIds, receptionDate, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در محاسبه هزینه‌های پذیرش";
                return RedirectToAction("Calculate");
            }
        }

        /// <summary>
        /// محاسبه هزینه قرار ملاقات
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> CalculateAppointmentCost(int patientId, int serviceId, DateTime appointmentDate)
        {
            _log.Information(
                "درخواست محاسبه هزینه قرار ملاقات. PatientId: {PatientId}, ServiceId: {ServiceId}, AppointmentDate: {AppointmentDate}. User: {UserName} (Id: {UserId})",
                patientId, serviceId, appointmentDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceCalculationService.CalculateAppointmentCostAsync(patientId, serviceId, appointmentDate);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در محاسبه هزینه قرار ملاقات. PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Calculate");
                }

                _log.Information(
                    "محاسبه هزینه قرار ملاقات با موفقیت انجام شد. PatientId: {PatientId}, ServiceId: {ServiceId}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, result.Data.PatientShare, _currentUserService.UserName, _currentUserService.UserId);

                return View("Result", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در محاسبه هزینه قرار ملاقات. PatientId: {PatientId}, ServiceId: {ServiceId}, AppointmentDate: {AppointmentDate}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, appointmentDate, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در محاسبه هزینه قرار ملاقات";
                return RedirectToAction("Calculate");
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// اعتبارسنجی کامل بیمه بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> ValidateInsurance(int patientId, int serviceId, DateTime appointmentDate)
        {
            _log.Information(
                "درخواست اعتبارسنجی بیمه بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, AppointmentDate: {AppointmentDate}. User: {UserName} (Id: {UserId})",
                patientId, serviceId, appointmentDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceValidationService.ValidateCompleteInsuranceAsync(patientId, serviceId, appointmentDate);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در اعتبارسنجی بیمه بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Calculate");
                }

                _log.Information(
                    "اعتبارسنجی بیمه بیمار با موفقیت انجام شد. PatientId: {PatientId}, ServiceId: {ServiceId}, IsCovered: {IsCovered}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, result.Data.IsCovered, _currentUserService.UserName, _currentUserService.UserId);

                return View("Validation", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطای سیستمی در اعتبارسنجی بیمه بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, AppointmentDate: {AppointmentDate}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, appointmentDate, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطا در اعتبارسنجی بیمه بیمار";
                return RedirectToAction("Calculate");
            }
        }

        #endregion

        #region AJAX Actions

        /// <summary>
        /// بررسی پوشش بیمه برای خدمت
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<JsonResult> CheckServiceCoverage(int patientId, int serviceId, DateTime appointmentDate)
        {
            try
            {
                var result = await _insuranceValidationService.ValidateCoverageAsync(patientId, serviceId, appointmentDate);
                return Json(new { covered = result.Success && result.Data });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی پوشش بیمه برای خدمت. PatientId: {PatientId}, ServiceId: {ServiceId}", patientId, serviceId);
                return Json(new { covered = false });
            }
        }

        /// <summary>
        /// بررسی اعتبار بیمه بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<JsonResult> CheckInsuranceValidity(int patientId, DateTime checkDate)
        {
            try
            {
                var result = await _insuranceValidationService.ValidatePatientInsurancesExpiryAsync(patientId);
                if (result.Success)
                {
                    var isValid = result.Data.Values.All(x => x);
                    return Json(new { valid = isValid });
                }
                return Json(new { valid = false });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی اعتبار بیمه بیمار. PatientId: {PatientId}", patientId);
                return Json(new { valid = false });
            }
        }

        /// <summary>
        /// محاسبه سریع سهم بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("QuickCalculate")]
        public async Task<JsonResult> QuickCalculate(int patientId, int serviceId, DateTime calculationDate)
        {
            try
            {
                var result = await _insuranceCalculationService.CalculatePatientShareAsync(patientId, serviceId, calculationDate);
                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        patientShare = result.Data.PatientShare,
                        insuranceShare = result.Data.InsuranceShare,
                        totalCost = result.Data.TotalAmount
                    });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در محاسبه سریع سهم بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}", patientId, serviceId);
                return Json(new { success = false, message = "خطا در محاسبه سهم بیمار" });
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار برای AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<JsonResult> GetPatientInsurances(int patientId)
        {
            try
            {
                _log.Information("درخواست دریافت بیمه‌های بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                if (patientId <= 0)
                {
                    _log.Warning("شناسه بیمار نامعتبر. PatientId: {PatientId}", patientId);
                    return Json(new { success = false, message = "شناسه بیمار نامعتبر است" });
                }

                // TODO: پیاده‌سازی دریافت بیمه‌های بیمار
                // فعلاً لیست خالی برمی‌گردانیم
                var selectList = new List<object>();

                _log.Information("بیمه‌های بیمار با موفقیت دریافت شد. PatientId: {PatientId}, Count: {Count}",
                    patientId, selectList.Count);

                return Json(new { success = true, data = selectList });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در دریافت بیمه‌های بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت بیمه‌های بیمار" });
            }
        }

        /// <summary>
        /// دریافت خدمات برای AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<JsonResult> GetServices(int? categoryId = null)
        {
            try
            {
                _log.Information("درخواست دریافت خدمات. CategoryId: {CategoryId}. User: {UserName} (Id: {UserId})",
                    categoryId, _currentUserService.UserName, _currentUserService.UserId);

                // TODO: پیاده‌سازی دریافت خدمات
                // فعلاً لیست خالی برمی‌گردانیم
                var selectList = new List<object>();

                _log.Information("خدمات با موفقیت دریافت شد. CategoryId: {CategoryId}, Count: {Count}",
                    categoryId, selectList.Count);

                return Json(new { success = true, data = selectList });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در دریافت خدمات. CategoryId: {CategoryId}. User: {UserName} (Id: {UserId})",
                    categoryId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = "خطا در دریافت خدمات" });
            }
        }

        #endregion

        #region Management Operations

        /// <summary>
        /// نمایش لیست محاسبات بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Index(InsuranceCalculationIndexPageViewModel model)
        {
            try
            {
                _log.Information("Displaying insurance calculations index page");

                // تنظیم مقادیر پیش‌فرض
                if (model.CurrentPage <= 0) model.CurrentPage = 1;
                if (model.PageSize <= 0) model.PageSize = 10;

                // جستجو و فیلتر
                var searchResult = await _insuranceCalculationService.SearchCalculationsAsync(
                    model.SearchTerm,
                    model.PatientId,
                    model.ServiceId,
                    model.PlanId,
                    model.IsValid,
                    model.FromDate,
                    model.ToDate,
                    model.CurrentPage,
                    model.PageSize);

                if (!searchResult.Success)
                {
                    _messageNotificationService.AddErrorMessage("خطا در دریافت محاسبات بیمه");
                    return View(new InsuranceCalculationIndexPageViewModel());
                }

                // تنظیم نتایج
                model.InsuranceCalculations = searchResult.Data.Items
                    .Select(InsuranceCalculationIndexViewModel.FromEntity)
                    .ToList();
                model.TotalCount = searchResult.Data.TotalCount;

                // بارگذاری SelectLists
                await LoadSelectListsAsync(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error displaying insurance calculations index");
                _messageNotificationService.AddErrorMessage("خطا در نمایش لیست محاسبات بیمه");
                return View(new InsuranceCalculationIndexPageViewModel());
            }
        }

        /// <summary>
        /// نمایش جزئیات محاسبه بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _log.Information("Displaying insurance calculation details for ID: {CalculationId}", id);

                var calculation = await _insuranceCalculationRepository.GetByIdWithDetailsAsync(id);
                if (calculation == null)
                {
                    _messageNotificationService.AddErrorMessage("محاسبه بیمه یافت نشد");
                    return RedirectToAction("Index");
                }

                var viewModel = InsuranceCalculationDetailsViewModel.FromEntity(calculation);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error displaying insurance calculation details for ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در نمایش جزئیات محاسبه بیمه");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد محاسبه بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Create()
        {
            try
            {
                _log.Information("Displaying create insurance calculation form");

                var model = new InsuranceCalculationViewModel
                {
                    CalculationDate = DateTime.Now,
                    CalculationType = "Service" // مقدار پیش‌فرض
                };

                // بارگذاری SelectLists
                await LoadCreateSelectListsAsync(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error displaying create insurance calculation form");
                _messageNotificationService.AddErrorMessage("خطا در نمایش فرم ایجاد محاسبه بیمه");
                return RedirectToAction("Calculate");
            }
        }

        /// <summary>
        /// ایجاد محاسبه بیمه جدید
        /// </summary>
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InsuranceCalculationViewModel model)
        {
            try
            {
                _log.Information("Creating new insurance calculation");

                // اعتبارسنجی
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    // بارگذاری مجدد SelectLists در صورت خطای اعتبارسنجی
                    await LoadCreateSelectListsAsync(model);
                    return View(model);
                }

                // تبدیل به Entity
                var calculation = new InsuranceCalculation
                {
                    PatientId = model.PatientId,
                    ServiceId = model.ServiceId,
                    InsurancePlanId = model.InsurancePlanId,
                    PatientInsuranceId = model.PatientInsuranceId,
                    ServiceAmount = model.ServiceAmount,
                    InsuranceShare = model.InsuranceShare,
                    PatientShare = model.PatientShare,
                    Copay = model.Copay,
                    CoverageOverride = model.CoverageOverride,
                    CoveragePercent = model.CoveragePercent,
                    Deductible = model.Deductible,
                    CalculationDate = model.CalculationDate,
                    CalculationType = model.CalculationType,
                    IsValid = true,
                    Notes = model.Notes,
                    ReceptionId = model.ReceptionId,
                    AppointmentId = model.AppointmentId
                };

                // ذخیره
                var saveResult = await _insuranceCalculationService.SaveCalculationAsync(calculation);
                if (!saveResult.Success)
                {
                    _messageNotificationService.AddErrorMessage("خطا در ذخیره محاسبه بیمه");
                    return View(model);
                }

                _messageNotificationService.AddSuccessMessage("محاسبه بیمه با موفقیت ایجاد شد");
                return RedirectToAction("Details", new { id = saveResult.Data.InsuranceCalculationId });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error creating insurance calculation");
                _messageNotificationService.AddErrorMessage("خطا در ایجاد محاسبه بیمه");
                return View(model);
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش محاسبه بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _log.Information("Displaying edit insurance calculation form for ID: {CalculationId}", id);

                var calculation = await _insuranceCalculationRepository.GetByIdWithDetailsAsync(id);
                if (calculation == null)
                {
                    _messageNotificationService.AddErrorMessage("محاسبه بیمه یافت نشد");
                    return RedirectToAction("Calculate");
                }

                var model = new InsuranceCalculationViewModel
                {
                    PatientId = calculation.PatientId,
                    ServiceId = calculation.ServiceId,
                    InsurancePlanId = calculation.InsurancePlanId,
                    PatientInsuranceId = calculation.PatientInsuranceId,
                    ServiceAmount = calculation.ServiceAmount,
                    InsuranceShare = calculation.InsuranceShare,
                    PatientShare = calculation.PatientShare,
                    Copay = calculation.Copay ?? 0,
                    CoverageOverride = calculation.CoverageOverride,
                    CoveragePercent = calculation.CoveragePercent,
                    Deductible = calculation.Deductible ?? 0,
                    CalculationDate = calculation.CalculationDate,
                    CalculationType = calculation.CalculationType,
                    Notes = calculation.Notes,
                    ReceptionId = calculation.ReceptionId,
                    AppointmentId = calculation.AppointmentId
                };

                // بارگذاری SelectLists
                await LoadCreateSelectListsAsync(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error displaying edit insurance calculation form for ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در نمایش فرم ویرایش محاسبه بیمه");
                return RedirectToAction("Calculate");
            }
        }

        /// <summary>
        /// ویرایش محاسبه بیمه
        /// </summary>
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, InsuranceCalculationViewModel model)
        {
            try
            {
                _log.Information("Updating insurance calculation with ID: {CalculationId}", id);

                // اعتبارسنجی
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    // بارگذاری مجدد SelectLists در صورت خطای اعتبارسنجی
                    await LoadCreateSelectListsAsync(model);
                    return View(model);
                }

                // دریافت محاسبه موجود
                var existingCalculation = await _insuranceCalculationRepository.GetByIdAsync(id);
                if (existingCalculation == null)
                {
                    _messageNotificationService.AddErrorMessage("محاسبه بیمه یافت نشد");
                    return RedirectToAction("Calculate");
                }

                // به‌روزرسانی
                existingCalculation.ServiceAmount = model.ServiceAmount;
                existingCalculation.InsuranceShare = model.InsuranceShare;
                existingCalculation.PatientShare = model.PatientShare;
                existingCalculation.Copay = model.Copay;
                existingCalculation.CoverageOverride = model.CoverageOverride;
                existingCalculation.CoveragePercent = model.CoveragePercent;
                existingCalculation.Deductible = model.Deductible;
                existingCalculation.CalculationDate = model.CalculationDate;
                existingCalculation.CalculationType = model.CalculationType;
                existingCalculation.Notes = model.Notes;

                var updateResult = await _insuranceCalculationRepository.UpdateAsync(existingCalculation);
                if (updateResult == null)
                {
                    _messageNotificationService.AddErrorMessage("خطا در به‌روزرسانی محاسبه بیمه");
                    // بارگذاری مجدد SelectLists در صورت خطا
                    await LoadCreateSelectListsAsync(model);
                    return View(model);
                }

                _messageNotificationService.AddSuccessMessage("محاسبه بیمه با موفقیت به‌روزرسانی شد");
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error updating insurance calculation with ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در به‌روزرسانی محاسبه بیمه");
                // بارگذاری مجدد SelectLists در صورت خطا
                await LoadCreateSelectListsAsync(model);
                return View(model);
            }
        }

        /// <summary>
        /// نمایش فرم حذف محاسبه بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _log.Information("Displaying delete insurance calculation form for ID: {CalculationId}", id);

                var calculation = await _insuranceCalculationRepository.GetByIdWithDetailsAsync(id);
                if (calculation == null)
                {
                    _messageNotificationService.AddErrorMessage("محاسبه بیمه یافت نشد");
                    return RedirectToAction("Calculate");
                }

                var viewModel = InsuranceCalculationDetailsViewModel.FromEntity(calculation);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error displaying delete insurance calculation form for ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در نمایش فرم حذف محاسبه بیمه");
                return RedirectToAction("Calculate");
            }
        }

        /// <summary>
        /// حذف محاسبه بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _log.Information("Deleting insurance calculation with ID: {CalculationId}", id);

                var deleteResult = await _insuranceCalculationService.DeleteCalculationAsync(id);
                if (!deleteResult.Success)
                {
                    _messageNotificationService.AddErrorMessage("خطا در حذف محاسبه بیمه");
                    return RedirectToAction("Calculate");
                }

                _messageNotificationService.AddSuccessMessage("محاسبه بیمه با موفقیت حذف شد");
                return RedirectToAction("Calculate");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error deleting insurance calculation with ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در حذف محاسبه بیمه");
                return RedirectToAction("Calculate");
            }
        }

        /// <summary>
        /// به‌روزرسانی وضعیت اعتبار محاسبه
        /// </summary>
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateValidity(int id, bool isValid)
        {
            try
            {
                _log.Information("Updating validity for insurance calculation with ID: {CalculationId}, IsValid: {IsValid}", id, isValid);

                var updateResult = await _insuranceCalculationService.UpdateCalculationValidityAsync(id, isValid);
                if (!updateResult.Success)
                {
                    _messageNotificationService.AddErrorMessage("خطا در به‌روزرسانی وضعیت اعتبار");
                    return RedirectToAction("Details", new { id = id });
                }

                var message = isValid ? "محاسبه بیمه معتبر شد" : "محاسبه بیمه نامعتبر شد";
                _messageNotificationService.AddSuccessMessage(message);
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error updating validity for insurance calculation with ID: {CalculationId}", id);
                _messageNotificationService.AddErrorMessage("خطا در به‌روزرسانی وضعیت اعتبار");
                return RedirectToAction("Details", new { id = id });
            }
        }

        /// <summary>
        /// دریافت آمار محاسبات بیمه
        /// </summary>
        [HttpGet]

        public async Task<ActionResult> Statistics()
        {
            try
            {
                _log.Information("درخواست دریافت آمار محاسبات بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                var statisticsResult = await _insuranceCalculationService.GetCalculationStatisticsAsync();
                if (!statisticsResult.Success)
                {
                    _log.Warning("خطا در دریافت آمار محاسبات بیمه. Error: {Error}", statisticsResult.Message);
                    _messageNotificationService.AddErrorMessage("خطا در دریافت آمار محاسبات بیمه");
                    return RedirectToAction("Index");
                }

                _log.Information("آمار محاسبات بیمه با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return View(statisticsResult.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در دریافت آمار محاسبات بیمه. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                _messageNotificationService.AddErrorMessage("خطا در دریافت آمار محاسبات بیمه");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// اعتبارسنجی جامع درخواست محاسبه
        /// </summary>
        private (bool IsValid, List<string> Errors) ValidateCalculationRequest(InsuranceCalculationViewModel model)
        {
            var errors = new List<string>();

            try
            {
                // بررسی null بودن مدل
                if (model == null)
                {
                    errors.Add("اطلاعات محاسبه بیمه ارسال نشده است");
                    return (false, errors);
                }

                // بررسی شناسه بیمار
                if (model.PatientId <= 0)
                    errors.Add("بیمار باید انتخاب شود");

                // بررسی شناسه خدمت
                if (model.ServiceId <= 0)
                    errors.Add("خدمت باید انتخاب شود");

                // بررسی تاریخ محاسبه
                if (model.CalculationDate == default(DateTime))
                    errors.Add("تاریخ محاسبه باید انتخاب شود");

                // بررسی محدوده زمانی
                var now = DateTime.Now;
                if (model.CalculationDate > now.AddDays(1))
                    errors.Add("تاریخ محاسبه نمی‌تواند بیش از یک روز در آینده باشد");

                if (model.CalculationDate < now.AddYears(-2))
                    errors.Add("تاریخ محاسبه نمی‌تواند بیش از دو سال گذشته باشد");

                // بررسی محدوده زمانی کاری (8 صبح تا 8 شب)
                var calculationTime = model.CalculationDate.TimeOfDay;
                if (calculationTime < TimeSpan.FromHours(8) || calculationTime > TimeSpan.FromHours(20))
                {
                    _log.Warning("محاسبه در خارج از ساعات کاری انجام شد. Time: {Time}, User: {UserName}", 
                        calculationTime, _currentUserService.UserName);
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعتبارسنجی درخواست محاسبه");
                errors.Add("خطا در اعتبارسنجی درخواست");
                return (false, errors);
            }
        }

        /// <summary>
        /// بررسی دسترسی کاربر به اطلاعات بیمار
        /// </summary>
        private async Task<(bool IsValid, string ErrorMessage)> CheckUserAccessToPatientAsync(int patientId)
        {
            try
            {
                // بررسی نقش کاربر
                var userRoles = _currentUserService.GetUserRoles();
                if (userRoles.Contains("Admin"))
                {
                    return (true, string.Empty); // Admin دسترسی کامل دارد
                }

                // بررسی دسترسی Receptionist و Doctor
                if (userRoles.Contains("Receptionist") || userRoles.Contains("Doctor"))
                {
                    // TODO: پیاده‌سازی بررسی دسترسی بر اساس کلینیک یا بخش
                    // فعلاً دسترسی کامل در نظر گرفته می‌شود
                    return (true, string.Empty);
                }

                return (false, "شما دسترسی لازم برای محاسبه بیمه ندارید");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی دسترسی کاربر به بیمار. PatientId: {PatientId}, User: {UserName}", 
                    patientId, _currentUserService.UserName);
                return (false, "خطا در بررسی دسترسی");
            }
        }

        /// <summary>
        /// اعتبارسنجی زمان محاسبه
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateCalculationTime(DateTime calculationDate)
        {
            try
            {
                var now = DateTime.Now;
                var timeDifference = Math.Abs((calculationDate - now).TotalHours);

                // بررسی محدوده زمانی منطقی
                if (timeDifference > 24 * 365 * 2) // بیش از دو سال
                {
                    return (false, "تاریخ محاسبه خارج از محدوده مجاز است");
                }

                // بررسی تعطیلات رسمی (اختیاری)
                if (IsHoliday(calculationDate))
                {
                    _log.Information("محاسبه در روز تعطیل انجام شد. Date: {Date}, User: {UserName}", 
                        calculationDate, _currentUserService.UserName);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در اعتبارسنجی زمان محاسبه");
                return (false, "خطا در بررسی زمان محاسبه");
            }
        }

        /// <summary>
        /// بررسی تعطیل بودن روز
        /// </summary>
        private bool IsHoliday(DateTime date)
        {
            try
            {
                // TODO: پیاده‌سازی بررسی تعطیلات رسمی ایران
                // فعلاً فقط تعطیلات ثابت بررسی می‌شود
                var persianDate = date.ToString("yyyy/MM/dd"); // TODO: استفاده از PersianDateHelper
                var month = persianDate.Split('/')[1];
                var day = persianDate.Split('/')[2];

                // تعطیلات ثابت
                var fixedHolidays = new[]
                {
                    "01/01", // نوروز
                    "01/02", // نوروز
                    "01/03", // نوروز
                    "01/04", // نوروز
                    "01/12", // روز جمهوری اسلامی
                    "01/13", // روز طبیعت
                    "03/14", // رحلت امام خمینی
                    "03/15", // قیام 15 خرداد
                    "11/22", // پیروزی انقلاب اسلامی
                    "12/29"  // ملی شدن صنعت نفت
                };

                return fixedHolidays.Contains($"{month}/{day}");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی تعطیل بودن روز");
                return false;
            }
        }

        /// <summary>
        /// بارگذاری SelectLists برای فیلترها
        /// </summary>
        private async Task LoadSelectListsAsync(InsuranceCalculationIndexPageViewModel model)
        {
            try
            {
                // TODO: پیاده‌سازی بارگذاری داده‌ها از Services
                // فعلاً با داده‌های خالی SelectList ها را ایجاد می‌کنیم

                // ایجاد SelectList برای بیماران
                model.Patients = new List<PatientLookupViewModel>();
                model.CreatePatientSelectList();

                // ایجاد SelectList برای خدمات
                model.Services = new List<ServiceLookupViewModel>();
                model.CreateServiceSelectList();

                // ایجاد SelectList برای طرح‌های بیمه
                model.InsurancePlans = new List<InsurancePlanLookupViewModel>();
                model.CreateInsurancePlanSelectList();

                // ایجاد SelectList برای وضعیت اعتبار
                model.CreateValiditySelectList();

                _log.Information("SelectLists loaded successfully for insurance calculations index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading SelectLists for insurance calculations index");
                // در صورت خطا، SelectList های خالی ایجاد می‌کنیم
                model.CreatePatientSelectList();
                model.CreateServiceSelectList();
                model.CreateInsurancePlanSelectList();
                model.CreateValiditySelectList();
            }
        }

        /// <summary>
        /// بارگذاری SelectLists برای فرم Create
        /// </summary>
        private async Task LoadCreateSelectListsAsync(InsuranceCalculationViewModel model)
        {
            try
            {
                _log.Information("Loading SelectLists for create insurance calculation form. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);

                // 🏥 Select2 برای بیماران - بدون بارگذاری اولیه (Server-Side Processing)
                // فقط یک placeholder برای Select2
                model.PatientSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "جستجو و انتخاب بیمار..." }
                }, "Value", "Text");

                _log.Information("Patient Select2 initialized for server-side processing");

                // 🏥 بارگذاری انواع محاسبه
                model.CalculationTypeSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "Service", Text = "محاسبه خدمت" },
                    new SelectListItem { Value = "Reception", Text = "محاسبه پذیرش" },
                    new SelectListItem { Value = "Appointment", Text = "محاسبه قرار ملاقات" },
                    new SelectListItem { Value = "Emergency", Text = "محاسبه اورژانس" },
                    new SelectListItem { Value = "Package", Text = "محاسبه پکیج" }
                }, "Value", "Text", model.CalculationType);

                _log.Information("Calculation type SelectList loaded with {Count} options", model.CalculationTypeSelectList.Count());

                // 🏥 بارگذاری بیمه‌های بیمار (خالی - بعد از انتخاب بیمار پر می‌شود)
                model.PatientInsuranceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "ابتدا بیمار را انتخاب کنید" }
                }, "Value", "Text");

                // 🏥 بارگذاری دسته‌بندی خدمات
                var serviceCategories = await _serviceCategoryService.GetActiveServiceCategoriesAsync();
                if (serviceCategories != null && serviceCategories.Any())
                {
                    var categoryItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "انتخاب دسته‌بندی خدمت" }
                    };
                    
                    categoryItems.AddRange(serviceCategories.Select(sc => new SelectListItem
                    {
                        Value = sc.ServiceCategoryId.ToString(),
                        Text = sc.Title
                    }));
                    
                    model.ServiceCategorySelectList = new SelectList(categoryItems, "Value", "Text");
                    _log.Information("Loaded {Count} service categories for SelectList", serviceCategories.Count());
                }
                else
                {
                    _log.Warning("No active service categories found");
                    model.ServiceCategorySelectList = new SelectList(new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "دسته‌بندی خدمتی یافت نشد" }
                    }, "Value", "Text");
                }

                // 🏥 بارگذاری خدمات (خالی - بعد از انتخاب دسته‌بندی پر می‌شود)
                model.ServiceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "ابتدا دسته‌بندی را انتخاب کنید" }
                }, "Value", "Text");

                _log.Information("SelectLists loaded successfully for create insurance calculation form. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading SelectLists for create insurance calculation form. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);
                
                // در صورت خطا، SelectList های خالی با پیام خطا ایجاد می‌کنیم
                model.PatientSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "خطا در بارگذاری بیماران" }
                }, "Value", "Text");
                
                model.PatientInsuranceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "خطا در بارگذاری بیمه‌ها" }
                }, "Value", "Text");
                
                model.ServiceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "خطا در بارگذاری خدمات" }
                }, "Value", "Text");
                
                model.ServiceCategorySelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "خطا در بارگذاری دسته‌بندی‌ها" }
                }, "Value", "Text");
            }
        }

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار برای AJAX (Create Form)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetPatientInsurancesForCreate(int patientId)
        {
            try
            {
                _log.Information("Getting patient insurances for Create form. PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه‌های فعال بیمار از سرویس
                _log.Information("🔍 DEBUG: Starting to load patient insurances for PatientId: {PatientId}", patientId);
                
                var result = await _patientInsuranceService.GetActivePatientInsurancesForLookupAsync(patientId);
                
                _log.Information("🔍 DEBUG: Service result - Success: {Success}, DataCount: {DataCount}, Message: {Message}", 
                    result.Success, result.Data?.Count ?? 0, result.Message);
                
                if (result.Success && result.Data != null && result.Data.Any())
                {
                    var selectItems = result.Data.Select(pi => new SelectListItem
                    {
                        Value = pi.PatientInsuranceId.ToString(),
                        Text = $"{pi.InsuranceProviderName} - {pi.InsurancePlanName} ({pi.PolicyNumber})"
                    }).ToList();

                    _log.Information("✅ SUCCESS: Loaded {Count} patient insurances for PatientId: {PatientId}. Details: {Details}", 
                        selectItems.Count, patientId, string.Join(", ", selectItems.Select(si => si.Text)));

                    return Json(new { success = true, data = selectItems });
                }
                else
                {
                    _log.Warning("⚠️ WARNING: No active patient insurances found for PatientId: {PatientId}. Success: {Success}, Message: {Message}, DataNull: {DataNull}", 
                        patientId, result.Success, result.Message, result.Data == null);
                    
                    var emptyItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "بیمه فعالی برای این بیمار یافت نشد" }
                    };
                    
                    return Json(new { success = true, data = emptyItems });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurances for Create form. PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت بیمه‌های بیمار" });
            }
        }

        /// <summary>
        /// جستجوی بیماران برای Select2 (Server-Side Processing)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> SearchPatientsForSelect2(string q, int page = 1, int pageSize = 20)
        {
            try
            {
                _log.Information("Searching patients for Select2. Query: {Query}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})", 
                    q, page, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _patientService.SearchPatientsForSelect2Async(q, page, pageSize);
                if (result.Success && result.Data != null)
                {
                    var select2Result = new
                    {
                        results = result.Data.Items.Select(p => new
                        {
                            id = p.PatientId,
                            text = $"{p.FirstName} {p.LastName} ({p.NationalCode})",
                            firstName = p.FirstName,
                            lastName = p.LastName,
                            nationalCode = p.NationalCode,
                            phoneNumber = p.PhoneNumber
                        }).ToList(),
                        pagination = new
                        {
                            more = (page * pageSize) < result.Data.TotalItems
                        }
                    };

                    return Json(select2Result);
                }
                else
                {
                    _log.Warning("Failed to search patients for Select2. Query: {Query}. Error: {Error}", 
                        q, result.Message);
                    return Json(new { results = new List<object>(), pagination = new { more = false } });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error searching patients for Select2. Query: {Query}. User: {UserName} (Id: {UserId})", 
                    q, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { results = new List<object>(), pagination = new { more = false } });
            }
        }

        /// <summary>
        /// دریافت خدمات بر اساس دسته‌بندی برای AJAX
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetServicesByCategory(int categoryId)
        {
            try
            {
                _log.Information("Getting services for CategoryId: {CategoryId}. User: {UserName} (Id: {UserId})", 
                    categoryId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت خدمات فعال بر اساس دسته‌بندی از سرویس
                var servicesResult = await _serviceManagementService.GetActiveServicesForLookupAsync(categoryId);
                if (servicesResult.Success && servicesResult.Data != null && servicesResult.Data.Any())
                {
                    var selectItems = servicesResult.Data.Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Title ?? s.Name
                    }).ToList();

                    _log.Information("Successfully loaded {Count} services for CategoryId: {CategoryId}", 
                        selectItems.Count, categoryId);

                    return Json(new { success = true, data = selectItems });
                }
                else
                {
                    _log.Warning("No active services found for CategoryId: {CategoryId}", categoryId);
                    
                    var emptyItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "خدمت فعالی در این دسته‌بندی یافت نشد" }
                    };
                    
                    return Json(new { success = true, data = emptyItems });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting services for CategoryId: {CategoryId}. User: {UserName} (Id: {UserId})", 
                    categoryId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت خدمات" });
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌ها برای سلسله مراتب دسته‌بندی
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetDepartmentsForHierarchy()
        {
            try
            {
                _log.Information("Getting departments for service hierarchy. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);

                // دریافت دپارتمان‌های فعال از دیتابیس
                // TODO: استفاده از شناسه کلینیک واقعی کاربر
                var currentClinicId = 1; // مقدار ثابت برای تست - باید از سرویس کاربر دریافت شود
                
                var departmentsResult = await _departmentManagementService.GetActiveDepartmentsForLookupAsync(currentClinicId);
                if (departmentsResult.Success && departmentsResult.Data != null)
                {
                    var departments = departmentsResult.Data.Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToList();

                    _log.Information("Successfully loaded {Count} departments for hierarchy from database", departments.Count);
                    return Json(new { success = true, data = departments });
                }
                else
                {
                    _log.Warning("Failed to load departments from service. Success: {Success}, Message: {Message}", 
                        departmentsResult.Success, departmentsResult.Message);
                    return Json(new { success = false, error = departmentsResult.Message ?? "خطا در دریافت دپارتمان‌ها" });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting departments for hierarchy. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت دپارتمان‌ها" });
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های دسته‌بندی بر اساس دپارتمان
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetServiceCategoriesByDepartment(int departmentId)
        {
            try
            {
                _log.Information("Getting service categories for DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})", 
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت دسته‌بندی‌های فعال بر اساس دپارتمان
                var serviceCategoriesResult = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(departmentId);
                if (serviceCategoriesResult.Success && serviceCategoriesResult.Data != null && serviceCategoriesResult.Data.Any())
                {
                    var selectItems = serviceCategoriesResult.Data.Select(sc => new SelectListItem
                    {
                        Value = sc.Id.ToString(),
                        Text = sc.Title ?? sc.Name
                    }).ToList();

                    _log.Information("Successfully loaded {Count} service categories for DepartmentId: {DepartmentId}", 
                        selectItems.Count, departmentId);

                    return Json(new { success = true, data = selectItems });
                }
                else
                {
                    _log.Warning("No service categories found for DepartmentId: {DepartmentId}", departmentId);
                    
                    var emptyItems = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "", Text = "سرفصل دسته‌بندی برای این دپارتمان یافت نشد" }
                    };
                    
                    return Json(new { success = true, data = emptyItems });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting service categories for DepartmentId: {DepartmentId}. User: {UserName} (Id: {UserId})", 
                    departmentId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت سرفصل‌های دسته‌بندی" });
            }
        }

        /// <summary>
        /// دریافت لاگ‌های خطای کلاینت برای audit trail
        /// </summary>
        [HttpPost]
        public ActionResult LogClientError(string message, string type, string data, string url, string userAgent)
        {
            try
            {
                _log.Warning("Client Error Log - Message: {Message}, Type: {Type}, Data: {Data}, URL: {Url}, UserAgent: {UserAgent}. User: {UserName} (Id: {UserId})", 
                    message, type, data, url, userAgent, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error logging client error. User: {UserName} (Id: {UserId})", 
                    _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// دریافت اطلاعات بیمه بیمار برای محاسبه
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetPatientInsuranceDetails(int patientInsuranceId)
        {
            try
            {
                _log.Information("Getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                if (patientInsuranceId <= 0)
                {
                    _log.Warning("Invalid PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, error = "شناسه بیمه بیمار نامعتبر است" });
                }

                // دریافت اطلاعات بیمه بیمار از سرویس
                var patientInsuranceResult = await _patientInsuranceService.GetPatientInsuranceDetailsAsync(patientInsuranceId);
                if (patientInsuranceResult.Success && patientInsuranceResult.Data != null)
                {
                    var patientInsurance = patientInsuranceResult.Data;
                    var insuranceDetails = new
                    {
                        patientInsuranceId = patientInsurance.PatientInsuranceId,
                        insurancePlanId = patientInsurance.InsurancePlanId,
                        insurancePlanName = patientInsurance.InsurancePlanName,
                        insuranceProviderName = patientInsurance.InsuranceProviderName,
                        coveragePercent = patientInsurance.CoveragePercent,
                        deductible = patientInsurance.Deductible,
                        policyNumber = patientInsurance.PolicyNumber
                    };

                    _log.Information("Successfully loaded patient insurance details for PatientInsuranceId: {PatientInsuranceId}. CoveragePercent: {CoveragePercent}, Deductible: {Deductible}", 
                        patientInsuranceId, insuranceDetails.coveragePercent, insuranceDetails.deductible);

                    return Json(new { success = true, data = insuranceDetails });
                }
                else
                {
                    _log.Warning("Patient insurance not found for PatientInsuranceId: {PatientInsuranceId}. Success: {Success}, Message: {Message}", 
                        patientInsuranceId, patientInsuranceResult.Success, patientInsuranceResult.Message);
                    return Json(new { success = false, error = patientInsuranceResult.Message ?? "بیمه بیمار یافت نشد" });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت اطلاعات بیمه بیمار" });
            }
        }

        /// <summary>
        /// دریافت جزئیات خدمت (مبلغ و سایر اطلاعات)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> GetServiceDetails(int serviceId)
        {
            try
            {
                _log.Information("Getting service details for ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                if (serviceId <= 0)
                {
                    _log.Warning("Invalid ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})", 
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, error = "شناسه خدمت نامعتبر است" });
                }

                // دریافت اطلاعات خدمت از سرویس
                var serviceResult = await _serviceManagementService.GetServiceDetailsAsync(serviceId);
                if (serviceResult.Success && serviceResult.Data != null)
                {
                    var service = serviceResult.Data;
                    var serviceDetails = new
                    {
                        serviceId = service.ServiceId,
                        amount = service.Price, // مبلغ واقعی از دیتابیس
                        title = service.Title,
                        description = service.Description
                    };

                    _log.Information("Successfully loaded service details for ServiceId: {ServiceId}. Amount: {Amount}", 
                        serviceId, serviceDetails.amount);

                    return Json(new { success = true, data = serviceDetails });
                }
                else
                {
                    _log.Warning("Service not found for ServiceId: {ServiceId}. Success: {Success}, Message: {Message}", 
                        serviceId, serviceResult.Success, serviceResult.Message);
                    return Json(new { success = false, error = serviceResult.Message ?? "خدمت مورد نظر یافت نشد" });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting service details for ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, error = "خطا در دریافت اطلاعات خدمت" });
            }
        }

        #endregion

        #region ServiceComponents Integration (یکپارچگی با ServiceComponents)

        /// <summary>
        /// محاسبه قیمت خدمت با استفاده از ServiceComponents
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateServicePriceWithComponents(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: محاسبه قیمت خدمت با ServiceComponents. ServiceId: {ServiceId}, Date: {Date}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _log.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                        serviceId, _currentUserService.UserName, _currentUserService.UserId);
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var calculatedPrice = _serviceCalculationService.CalculateServicePriceWithFactorSettings(
                    service, _context, calculationDate ?? DateTime.Now);

                var result = new
                {
                    success = true,
                    serviceId = service.ServiceId,
                    serviceTitle = service.Title,
                    serviceCode = service.ServiceCode,
                    calculatedPrice = calculatedPrice,
                    calculationDate = calculationDate ?? DateTime.Now,
                    components = service.ServiceComponents
                        .Where(sc => !sc.IsDeleted && sc.IsActive)
                        .Select(sc => new
                        {
                            sc.ComponentType,
                            ComponentTypeName = sc.ComponentType == ServiceComponentType.Technical ? "فنی" : "حرفه‌ای",
                            sc.Coefficient
                        })
                        .ToList()
                };

                _log.Information("🏥 MEDICAL: محاسبه قیمت خدمت موفق. ServiceId: {ServiceId}, Price: {Price}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// محاسبه جزئیات بیمه با استفاده از ServiceComponents
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateInsuranceWithServiceComponents(int patientId, int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: محاسبه بیمه با ServiceComponents. PatientId: {PatientId}, ServiceId: {ServiceId}, Date: {Date}, User: {UserName} (Id: {UserId})", 
                    patientId, serviceId, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                // محاسبه قیمت خدمت با ServiceComponents
                var servicePriceResult = await CalculateServicePriceWithComponents(serviceId, calculationDate);
                var servicePriceData = servicePriceResult.Data as dynamic;
                if (servicePriceData == null || !servicePriceData.success)
                {
                    return Json(new { success = false, message = "خطا در محاسبه قیمت خدمت" }, JsonRequestBehavior.AllowGet);
                }

                var servicePrice = (decimal)servicePriceData.calculatedPrice;

                // محاسبه بیمه با قیمت محاسبه شده
                var insuranceResult = await _insuranceCalculationService.CalculatePatientShareAsync(patientId, serviceId, calculationDate ?? DateTime.Now);
                if (!insuranceResult.Success)
                {
                    return Json(new { success = false, message = insuranceResult.Message }, JsonRequestBehavior.AllowGet);
                }

                var insuranceData = insuranceResult.Data;
                
                // محاسبه نهایی با قیمت واقعی
                var totalAmount = servicePrice;
                var insuranceCoverage = (totalAmount * insuranceData.CoveragePercent / 100);
                var patientPayment = totalAmount - insuranceCoverage;

                var result = new
                {
                    success = true,
                    patientId = patientId,
                    serviceId = serviceId,
                    servicePrice = servicePrice,
                    totalAmount = totalAmount,
                    coveragePercent = insuranceData.CoveragePercent,
                    insuranceCoverage = insuranceCoverage,
                    patientPayment = patientPayment,
                    deductible = insuranceData.Deductible,
                    insurancePlanName = insuranceData.InsurancePlanName,
                    insuranceProviderName = insuranceData.InsuranceProviderName,
                    policyNumber = insuranceData.PolicyNumber,
                    calculationDate = calculationDate ?? DateTime.Now
                };

                _log.Information("🏥 MEDICAL: محاسبه بیمه با ServiceComponents موفق. PatientId: {PatientId}, ServiceId: {ServiceId}, TotalAmount: {TotalAmount}, InsuranceCoverage: {InsuranceCoverage}, PatientPayment: {PatientPayment}, User: {UserName} (Id: {UserId})", 
                    patientId, serviceId, totalAmount, insuranceCoverage, patientPayment, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در محاسبه بیمه با ServiceComponents. PatientId: {PatientId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت وضعیت ServiceComponents برای خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceComponentsStatus(int serviceId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: دریافت وضعیت ServiceComponents. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var technicalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && sc.IsActive && !sc.IsDeleted);
                var professionalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && sc.IsActive && !sc.IsDeleted);

                var result = new
                {
                    success = true,
                    serviceId = service.ServiceId,
                    serviceTitle = service.Title,
                    serviceCode = service.ServiceCode,
                    hasTechnicalComponent = technicalComponent != null,
                    hasProfessionalComponent = professionalComponent != null,
                    isComplete = technicalComponent != null && professionalComponent != null,
                    technicalCoefficient = technicalComponent?.Coefficient ?? 0,
                    professionalCoefficient = professionalComponent?.Coefficient ?? 0,
                    componentsCount = service.ServiceComponents.Count(sc => !sc.IsDeleted && sc.IsActive)
                };

                _log.Information("🏥 MEDICAL: وضعیت ServiceComponents دریافت شد. ServiceId: {ServiceId}, Complete: {IsComplete}, ComponentsCount: {ComponentsCount}, User: {UserName} (Id: {UserId})", 
                    serviceId, result.isComplete, result.componentsCount, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت وضعیت ServiceComponents. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
