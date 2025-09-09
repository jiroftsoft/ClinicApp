using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using ClinicApp.Services;
using FluentValidation;
using Serilog;

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
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [RouteArea("Admin")]
    [RoutePrefix("Insurance/Calculation")]
    public class InsuranceCalculationController : Controller
    {
        private readonly IInsuranceCalculationService _insuranceCalculationService;
        private readonly IInsuranceValidationService _insuranceValidationService;
        private readonly IInsuranceCalculationRepository _insuranceCalculationRepository;
        private readonly IMessageNotificationService _messageNotificationService;
        private readonly IValidator<InsuranceCalculationViewModel> _validator;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceCalculationController(
            IInsuranceCalculationService insuranceCalculationService,
            IInsuranceValidationService insuranceValidationService,
            IInsuranceCalculationRepository insuranceCalculationRepository,
            IMessageNotificationService messageNotificationService,
            IValidator<InsuranceCalculationViewModel> validator,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _insuranceCalculationService = insuranceCalculationService ?? throw new ArgumentNullException(nameof(insuranceCalculationService));
            _insuranceValidationService = insuranceValidationService ?? throw new ArgumentNullException(nameof(insuranceValidationService));
            _insuranceCalculationRepository = insuranceCalculationRepository ?? throw new ArgumentNullException(nameof(insuranceCalculationRepository));
            _messageNotificationService = messageNotificationService ?? throw new ArgumentNullException(nameof(messageNotificationService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _log = logger.ForContext<InsuranceCalculationController>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Calculate

        /// <summary>
        /// نمایش صفحه محاسبه بیمه
        /// </summary>
        [HttpGet]
        [Route("Calculate")]
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
        [Route("CalculatePatientShare")]
        public async Task<ActionResult> CalculatePatientShare(InsuranceCalculationViewModel model)
        {
            _log.Information(
                "درخواست محاسبه سهم بیمار. PatientId: {PatientId}, ServiceId: {ServiceId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                model?.PatientId, model?.ServiceId, model?.CalculationDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (!ModelState.IsValid)
                {
                    _log.Warning(
                        "مدل محاسبه بیمه معتبر نیست. PatientId: {PatientId}, ServiceId: {ServiceId}, CalculationDate: {CalculationDate}. User: {UserName} (Id: {UserId})",
                        model?.PatientId, model?.ServiceId, model?.CalculationDate, _currentUserService.UserName, _currentUserService.UserId);

                     return View("Calculate", model);
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
        [Route("CalculateReceptionCosts")]
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
        [Route("CalculateAppointmentCost")]
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
        [Route("ValidateInsurance")]
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
        [Route("CheckServiceCoverage")]
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
        [Route("CheckInsuranceValidity")]
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
                    return Json(new { 
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
        [Route("GetPatientInsurances")]
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
        [Route("GetServices")]
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
        [Route("Index")]
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
        [Route("Details/{id}")]
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
        [Route("Create")]
        public async Task<ActionResult> Create()
        {
            try
            {
                _log.Information("Displaying create insurance calculation form");

                var model = new InsuranceCalculationViewModel
                {
                    CalculationDate = DateTime.Now
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
        [Route("Create")]
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
        [Route("Edit/{id}")]
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
        [Route("Edit/{id}")]
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
        [Route("Delete/{id}")]
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
        [Route("Delete/{id}")]
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
        [Route("UpdateValidity/{id}")]
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
        [Route("Statistics")]
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
                // TODO: پیاده‌سازی بارگذاری داده‌ها از Services
                // فعلاً با داده‌های خالی SelectList ها را ایجاد می‌کنیم
                
                // ایجاد SelectList برای بیماران
                model.PatientSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "انتخاب بیمار" }
                }, "Value", "Text");

                // ایجاد SelectList برای بیمه‌های بیمار
                model.PatientInsuranceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "انتخاب بیمه بیمار" }
                }, "Value", "Text");

                // ایجاد SelectList برای خدمات
                model.ServiceSelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "انتخاب خدمت" }
                }, "Value", "Text");

                // ایجاد SelectList برای دسته‌بندی خدمات
                model.ServiceCategorySelectList = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "انتخاب دسته‌بندی" }
                }, "Value", "Text");

                _log.Information("SelectLists loaded successfully for create insurance calculation form");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error loading SelectLists for create insurance calculation form");
                // در صورت خطا، SelectList های خالی ایجاد می‌کنیم
                model.PatientSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                model.PatientInsuranceSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                model.ServiceSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                model.ServiceCategorySelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
            }
        }

        #endregion
    }
}
