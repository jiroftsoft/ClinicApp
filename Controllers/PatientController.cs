using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Interfaces.Insurance;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر مدیریت بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیماران
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 4. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 5. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 6. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
    /// 7. امنیت بالا با رعایت استانداردهای سیستم‌های پزشکی
    /// 8. عملکرد بهینه برای محیط‌های Production
    /// 9. یکپارچه‌سازی با ماژول‌های Insurance، Appointment و Reception
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    //[RouteArea("Admin")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IPatientInsuranceService _patientInsuranceService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public PatientController(
            IPatientService patientService,
            IPatientInsuranceService patientInsuranceService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _patientService = patientService;
            _patientInsuranceService = patientInsuranceService;
            _log = logger.ForContext<PatientController>();
            _currentUserService = currentUserService;
            _appSettings = appSettings;
        }

        private int PageSize => _appSettings.DefaultPageSize;

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی بیماران
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            _log.Information("بازدید از صفحه اصلی بیماران. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            // طبق قرارداد FormStandards - استفاده از Model
            var model = new PatientIndexPageViewModel
            {
                PageTitle = "مدیریت بیماران",
                TotalPatients = 0, // این مقدار از Service دریافت می‌شود
                ActivePatients = 0, // این مقدار از Service دریافت می‌شود
                LastUpdated = DateTime.Now
            };

            return View(model);
        }

        /// <summary>
        /// بارگیری لیست بیماران با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> LoadPatients(string searchTerm = "", int page = 1, string genderFilter = "")
        {
            _log.Information(
                "درخواست لود بیماران. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}. User: {UserName} (Id: {UserId})",
                searchTerm, page, PageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientService.SearchPatientsAsync(searchTerm, page, PageSize);
                if (!result.Success)
                {
                    _log.Warning(
                        "خطا در لود بیماران. SearchTerm: {SearchTerm}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        searchTerm, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_PatientListPartial", new PagedResult<PatientIndexViewModel>());
                }

                _log.Information(
                    "لود بیماران با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    result.Data.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientListPartial", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در لود بیماران. SearchTerm: {SearchTerm}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    searchTerm, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_PatientListPartial", new PagedResult<PatientIndexViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات بیمار - طبق قرارداد FormStandards
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                _log.Warning("درخواست جزئیات بیمار با شناسه خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست جزئیات بیمار با شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientService.GetPatientDetailsAsync(id.Value);
                if (!result.Success)
                {
                    _log.Warning(
                        "دریافت جزئیات بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return HttpNotFound();
                }

                // دریافت بیمه‌های بیمار از طریق PatientService
                var patientInsurancesResult = await _patientService.GetPatientInsurancesAsync(id.Value);
                if (!patientInsurancesResult.Success)
                {
                    _log.Warning(
                        "دریافت بیمه‌های بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, patientInsurancesResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                // دریافت تاریخچه نوبت‌های بیمار
                var patientAppointmentsResult = await _patientService.GetPatientAppointmentsAsync(id.Value);
                if (!patientAppointmentsResult.Success)
                {
                    _log.Warning(
                        "دریافت نوبت‌های بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, patientAppointmentsResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                // دریافت تاریخچه پذیرش‌های بیمار
                var patientReceptionsResult = await _patientService.GetPatientReceptionsAsync(id.Value);
                if (!patientReceptionsResult.Success)
                {
                    _log.Warning(
                        "دریافت پذیرش‌های بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, patientReceptionsResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                _log.Information(
                    "دریافت جزئیات بیمار شناسه {PatientId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // ارسال اطلاعات مرتبط به View از طریق Model
                var patientInsurances = patientInsurancesResult.Success ? 
                    patientInsurancesResult.Data : new List<PatientInsuranceViewModel>();
                var patientAppointments = patientAppointmentsResult.Success ? 
                    patientAppointmentsResult.Data : new List<PatientAppointmentViewModel>();
                var patientReceptions = patientReceptionsResult.Success ? 
                    patientReceptionsResult.Data : new List<PatientReceptionViewModel>();
                
                // اضافه کردن اطلاعات مرتبط به Model
                result.Data.PatientInsurances = patientInsurances.Select(pi => new PatientInsuranceIndexViewModel
                {
                    PatientInsuranceId = pi.PatientInsuranceId,
                    PatientId = pi.PatientId,
                    InsurancePlanId = pi.InsurancePlanId,
                    InsurancePlanName = pi.InsurancePlanName,
                    InsuranceProviderName = pi.InsuranceProviderName,
                    PolicyNumber = pi.PolicyNumber,
                    IsPrimary = pi.IsPrimary,
                    StartDate = pi.StartDate,
                    EndDate = pi.EndDate,
                    StartDateShamsi = pi.StartDateShamsi,
                    EndDateShamsi = pi.EndDateShamsi,
                    IsActive = pi.IsActive
                }).ToList();
                result.Data.PatientAppointments = patientAppointments;
                result.Data.PatientReceptions = patientReceptions;

                // ایجاد PageViewModel طبق قرارداد FormStandards
                var pageViewModel = new PatientDetailsPageViewModel
                {
                    PageTitle = $"جزئیات بیمار: {result.Data.FullName}",
                    PageSubtitle = "مشاهده کامل اطلاعات بیمار",
                    PatientInfo = result.Data,
                    LastUpdated = DateTime.Now
                };

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت جزئیات بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return HttpNotFound();
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد بیمار جدید - طبق قرارداد FormStandards
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            _log.Information(
                "درخواست ایجاد بیمار جدید. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var pageViewModel = new PatientCreateEditPageViewModel
                {
                    PageTitle = "ثبت بیمار جدید",
                    PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید",
                    FormModel = new PatientCreateEditViewModel(),
                    IsEditMode = false,
                    LastUpdated = DateTime.Now
                };

                _log.Information(
                    "صفحه ایجاد بیمار با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری صفحه ایجاد بیمار. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return View(new PatientCreateEditPageViewModel());
            }
        }

        /// <summary>
        /// پردازش ایجاد بیمار جدید - طبق قرارداد FormStandards
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PatientCreateEditPageViewModel pageModel)
        {
            // Defensive Coding: بررسی null بودن pageModel
            if (pageModel?.FormModel == null)
            {
                _log.Warning("درخواست ایجاد بیمار با pageModel یا FormModel خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // لاگ‌گذاری شروع عملیات
            LogOperationStart("ایجاد", pageModel.FormModel.NationalCode, additionalData: new { 
                FirstName = pageModel.FormModel.FirstName,
                LastName = pageModel.FormModel.LastName,
                Gender = pageModel.FormModel.Gender.ToString()
            });

            // اعتبارسنجی اولیه - بیمه از طریق PatientInsurance مدیریت می‌شود

            if (!ModelState.IsValid)
            {
                // لاگ‌گذاری تفصیلی خطاهای اعتبارسنجی
                LogValidationErrors("ایجاد", pageModel.FormModel.NationalCode);

                // اضافه کردن خلاصه خطاها به ModelState برای نمایش به کاربر
                var errorSummary = GetValidationErrorSummary();
                ModelState.AddModelError("", $"خطاهای اعتبارسنجی: {errorSummary}");

                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ثبت بیمار جدید";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید";
                pageModel.IsEditMode = false;
                pageModel.LastUpdated = DateTime.Now;
                
                return View(pageModel);
            }

            try
            {
                var result = await _patientService.CreatePatientAsync(pageModel.FormModel);
                if (result.Success)
                {
                    // لاگ‌گذاری موفقیت
                    LogOperationSuccess("ایجاد", pageModel.FormModel.NationalCode, null, result.Message);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // لاگ‌گذاری شکست
                LogOperationFailure("ایجاد", pageModel.FormModel.NationalCode, errorMessage: result.Message);

                ModelState.AddModelError("", result.Message);
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ثبت بیمار جدید";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید";
                pageModel.IsEditMode = false;
                pageModel.LastUpdated = DateTime.Now;
                
                return View(pageModel);
            }
            catch (DbUpdateException ex)
            {
                // لاگ‌گذاری خطای دیتابیس
                LogOperationFailure("ایجاد", pageModel.FormModel.NationalCode, exception: ex, errorMessage: "خطای دیتابیس");

                ModelState.AddModelError("", "خطا در ذخیره اطلاعات. لطفاً دوباره تلاش کنید.");
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ثبت بیمار جدید";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید";
                pageModel.IsEditMode = false;
                pageModel.LastUpdated = DateTime.Now;
                
                return View(pageModel);
            }
            catch (InvalidOperationException ex)
            {
                // لاگ‌گذاری خطای عملیات نامعتبر
                LogOperationFailure("ایجاد", pageModel.FormModel.NationalCode, exception: ex, errorMessage: "خطای عملیات نامعتبر");

                ModelState.AddModelError("", "عملیات نامعتبر. لطفاً اطلاعات را بررسی کنید.");
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ثبت بیمار جدید";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید";
                pageModel.IsEditMode = false;
                pageModel.LastUpdated = DateTime.Now;
                
                return View(pageModel);
            }
            catch (Exception ex)
            {
                // لاگ‌گذاری خطای سیستمی غیرمنتظره
                LogOperationFailure("ایجاد", pageModel.FormModel.NationalCode, exception: ex, errorMessage: "خطای سیستمی غیرمنتظره");

                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ثبت بیمار جدید";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت وارد کنید";
                pageModel.IsEditMode = false;
                pageModel.LastUpdated = DateTime.Now;
                
                return View(pageModel);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش بیمار - طبق قرارداد FormStandards
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _log.Warning(
                    "درخواست ویرایش بیمار با شناسه خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست ویرایش بیمار با شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientService.GetPatientForEditAsync(id.Value);
                if (!result.Success)
                {
                    _log.Warning(
                        "دریافت اطلاعات بیمار شناسه {PatientId} برای ویرایش ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return HttpNotFound();
                }

                var pageViewModel = new PatientCreateEditPageViewModel
                {
                    PageTitle = "ویرایش اطلاعات بیمار",
                    PageSubtitle = "اطلاعات بیمار را با دقت ویرایش کنید",
                    FormModel = result.Data,
                    IsEditMode = true,
                    LastUpdated = DateTime.Now
                };

                _log.Information(
                    "صفحه ویرایش بیمار شناسه {PatientId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری صفحه ویرایش بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return HttpNotFound();
            }
        }

        /// <summary>
        /// پردازش ویرایش بیمار - طبق قرارداد FormStandards
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(PatientCreateEditPageViewModel pageModel)
        {
            // Defensive Coding: بررسی null بودن pageModel
            if (pageModel?.FormModel == null)
            {
                _log.Warning("درخواست ویرایش بیمار با pageModel یا FormModel خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (pageModel.FormModel.PatientId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش بیمار با شناسه نامعتبر. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    pageModel.FormModel.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // لاگ‌گذاری شروع عملیات
            LogOperationStart("ویرایش", pageModel.FormModel.NationalCode, pageModel.FormModel.PatientId, additionalData: new { 
                FirstName = pageModel.FormModel.FirstName,
                LastName = pageModel.FormModel.LastName,
                Gender = pageModel.FormModel.Gender.ToString()
            });

            if (!ModelState.IsValid)
            {
                // لاگ‌گذاری تفصیلی خطاهای اعتبارسنجی
                LogValidationErrors("ویرایش", pageModel.FormModel.NationalCode, pageModel.FormModel.PatientId);

                // اضافه کردن خلاصه خطاها به ModelState برای نمایش به کاربر
                var errorSummary = GetValidationErrorSummary();
                ModelState.AddModelError("", $"خطاهای اعتبارسنجی: {errorSummary}");

                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ویرایش اطلاعات بیمار";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت ویرایش کنید";
                pageModel.IsEditMode = true;
                pageModel.LastUpdated = DateTime.Now;

                return View(pageModel);
            }

            try
            {
                var result = await _patientService.UpdatePatientAsync(pageModel.FormModel);
                if (result.Success)
                {
                    // لاگ‌گذاری موفقیت
                    LogOperationSuccess("ویرایش", pageModel.FormModel.NationalCode, pageModel.FormModel.PatientId, result.Message);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // لاگ‌گذاری شکست
                LogOperationFailure("ویرایش", pageModel.FormModel.NationalCode, pageModel.FormModel.PatientId, result.Message);

                ModelState.AddModelError("", result.Message);
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ویرایش اطلاعات بیمار";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت ویرایش کنید";
                pageModel.IsEditMode = true;
                pageModel.LastUpdated = DateTime.Now;

                return View(pageModel);
            }
            catch (Exception ex)
            {
                // لاگ‌گذاری خطای سیستمی
                LogOperationFailure("ویرایش", pageModel.FormModel.NationalCode, pageModel.FormModel.PatientId, exception: ex, errorMessage: "خطای سیستمی");

                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                
                // بازگرداندن صفحه با فرم پر شده
                pageModel.PageTitle = "ویرایش اطلاعات بیمار";
                pageModel.PageSubtitle = "اطلاعات بیمار را با دقت ویرایش کنید";
                pageModel.IsEditMode = true;
                pageModel.LastUpdated = DateTime.Now;

                return View(pageModel);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// نمایش صفحه تأیید حذف بیمار - طبق قرارداد FormStandards
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _log.Warning("درخواست حذف بیمار با شناسه خالی. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست صفحه حذف بیمار با شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientService.GetPatientDetailsAsync(id.Value);
                if (!result.Success)
                {
                    _log.Warning(
                        "دریافت جزئیات بیمار شناسه {PatientId} برای حذف ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return HttpNotFound();
                }

                // دریافت بیمه‌های بیمار
                var patientInsurancesResult = await _patientInsuranceService.GetPatientInsurancesAsync(
                    patientId: id.Value, 
                    searchTerm: null,
                    pageNumber: 1, 
                    pageSize: 100);

                if (!patientInsurancesResult.Success)
                {
                    _log.Warning(
                        "دریافت بیمه‌های بیمار شناسه {PatientId} برای حذف ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, patientInsurancesResult.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                _log.Information(
                    "دریافت جزئیات بیمار شناسه {PatientId} برای حذف با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                // ارسال بیمه‌های بیمار به View از طریق Model
                var patientInsurances = patientInsurancesResult.Success ? 
                    patientInsurancesResult.Data.Items : new List<PatientInsuranceIndexViewModel>();
                
                // اضافه کردن بیمه‌ها به Model
                result.Data.PatientInsurances = patientInsurances;

                // ایجاد PageViewModel طبق قرارداد FormStandards
                var pageViewModel = new PatientDeletePageViewModel
                {
                    PageTitle = $"تأیید حذف بیمار: {result.Data.FullName}",
                    PageSubtitle = "این عملیات غیرقابل بازگشت است",
                    PatientInfo = result.Data,
                    LastUpdated = DateTime.Now,
                    DeleteWarning = "آیا از حذف این بیمار اطمینان دارید؟ این عملیات غیرقابل بازگشت است.",
                    ConfirmationMessage = "برای تأیید حذف، دکمه 'حذف' را کلیک کنید."
                };

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت جزئیات بیمار شناسه {PatientId} برای حذف. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return HttpNotFound();
            }
        }

        /// <summary>
        /// پردازش حذف بیمار - طبق قرارداد FormStandards
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, PatientDeletePageViewModel pageModel)
        {
            if (id <= 0)
            {
                _log.Warning("درخواست حذف بیمار با شناسه نامعتبر. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست حذف بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // بررسی وابستگی‌های بیمار قبل از حذف
                var dependencyCheckResult = await _patientService.CheckPatientDependenciesAsync(id);
                if (!dependencyCheckResult.Success)
                {
                    _log.Warning(
                        "امکان حذف بیمار شناسه {PatientId} وجود ندارد. وابستگی‌ها: {Dependencies}. User: {UserName} (Id: {UserId})",
                        id, dependencyCheckResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = dependencyCheckResult.Message;
                    
                    // بازگرداندن صفحه حذف با پیام خطا
                    var patientDetails = await _patientService.GetPatientDetailsAsync(id);
                    if (patientDetails.Success)
                    {
                        pageModel.PatientInfo = patientDetails.Data;
                        pageModel.PageTitle = $"تأیید حذف بیمار: {patientDetails.Data.FullName}";
                        pageModel.PageSubtitle = "این عملیات غیرقابل بازگشت است";
                        pageModel.LastUpdated = DateTime.Now;
                    }
                    
                    return View(pageModel);
                }

                var result = await _patientService.DeletePatientAsync(id);
                if (result.Success)
                {
                    _log.Information(
                        "بیمار شناسه {PatientId} با موفقیت حذف شد. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }
                else
                {
                    _log.Warning(
                        "حذف بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = result.Message;
                    
                    // بازگرداندن صفحه حذف با پیام خطا
                    if (pageModel?.PatientInfo != null)
                    {
                        pageModel.PageTitle = $"تأیید حذف بیمار: {pageModel.PatientInfo.FullName}";
                    }
                    else
                    {
                        pageModel = new PatientDeletePageViewModel
                        {
                            PageTitle = "خطا در حذف بیمار",
                            PageSubtitle = "این عملیات غیرقابل بازگشت است",
                            LastUpdated = DateTime.Now
                        };
                    }
                    
                    return View(pageModel);
                }
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                
                // بازگرداندن صفحه حذف با پیام خطا
                if (pageModel?.PatientInfo != null)
                {
                    pageModel.PageTitle = $"تأیید حذف بیمار: {pageModel.PatientInfo.FullName}";
                }
                else
                {
                    pageModel = new PatientDeletePageViewModel
                    {
                        PageTitle = "خطا در حذف بیمار",
                        PageSubtitle = "این عملیات غیرقابل بازگشت است",
                        LastUpdated = DateTime.Now
                    };
                }
                
                return View(pageModel);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// متد کمکی برای لاگ‌گذاری خطاهای اعتبارسنجی - طبق قرارداد Medical Environment
        /// </summary>
        private void LogValidationErrors(string operation, string nationalCode, int? patientId = null)
        {
            var validationErrors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(error => new
                {
                    Field = x.Key,
                    ErrorMessage = error.ErrorMessage,
                    Exception = error.Exception?.Message,
                    AttemptedValue = x.Value.Value?.AttemptedValue
                }))
                .ToList();

            // خلاصه خطاها برای لاگ اصلی
            var errorSummary = string.Join("; ", validationErrors.Select(e => $"{e.Field}: {e.ErrorMessage}"));

            _log.Warning(
                "اعتبارسنجی {Operation} بیمار {PatientInfo} شکست خورد. " +
                "تعداد خطاها: {ErrorCount}. " +
                "جزئیات خطاها: {ErrorDetails}. " +
                "User: {UserName} (Id: {UserId})",
                operation,
                patientId.HasValue ? $"شناسه {patientId} با کد ملی {nationalCode}" : $"جدید با کد ملی {nationalCode}",
                validationErrors.Count,
                errorSummary,
                _currentUserService.UserName,
                _currentUserService.UserId);

            // لاگ‌گذاری تفصیلی برای هر فیلد - برای ردیابی دقیق
            foreach (var error in validationErrors)
            {
                _log.Warning(
                    "خطای اعتبارسنجی فیلد {Field}: {ErrorMessage}. " +
                    "مقدار وارد شده: {AttemptedValue}. " +
                    "Operation: {Operation}, NationalCode: {NationalCode}, User: {UserName}",
                    error.Field,
                    error.ErrorMessage,
                    error.AttemptedValue ?? "null",
                    operation,
                    nationalCode,
                    _currentUserService.UserName);
            }

            // لاگ‌گذاری برای تحلیل الگوهای خطا
            var fieldErrorCounts = validationErrors
                .GroupBy(e => e.Field)
                .ToDictionary(g => g.Key, g => g.Count());

            _log.Information(
                "آمار خطاهای اعتبارسنجی {Operation}: {FieldErrorStats}. " +
                "NationalCode: {NationalCode}, User: {UserName}",
                operation,
                string.Join(", ", fieldErrorCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}")),
                nationalCode,
                _currentUserService.UserName);
        }

        /// <summary>
        /// متد کمکی برای ایجاد خلاصه خطاهای اعتبارسنجی برای نمایش به کاربر
        /// </summary>
        private string GetValidationErrorSummary()
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors.Select(error => new
                {
                    Field = GetFieldDisplayName(x.Key),
                    ErrorMessage = error.ErrorMessage
                }))
                .ToList();

            if (!errors.Any())
                return "خطای اعتبارسنجی نامشخص";

            return string.Join("؛ ", errors.Select(e => $"{e.Field}: {e.ErrorMessage}"));
        }

        /// <summary>
        /// متد کمکی برای تبدیل نام فیلد به نام نمایشی فارسی
        /// </summary>
        private string GetFieldDisplayName(string fieldName)
        {
            var displayNames = new Dictionary<string, string>
            {
                { "FirstName", "نام" },
                { "LastName", "نام خانوادگی" },
                { "NationalCode", "کد ملی" },
                { "PhoneNumber", "شماره موبایل" },
                { "Gender", "جنسیت" },
                { "BirthDateShamsi", "تاریخ تولد" },
                { "Address", "آدرس" },
                { "Email", "ایمیل" },
                { "PatientCode", "کد بیمار" },
                { "DoctorName", "نام پزشک معالج" }
            };

            return displayNames.ContainsKey(fieldName) ? displayNames[fieldName] : fieldName;
        }

        /// <summary>
        /// بررسی وجود کد ملی در سیستم - برای جلوگیری از تکرار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CheckNationalCodeExists(string nationalCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalCode))
                {
                    return Json(new { exists = false }, JsonRequestBehavior.AllowGet);
                }

                var result = await _patientService.CheckNationalCodeExistsAsync(nationalCode);
                return Json(new { exists = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                    nationalCode, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { exists = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// متد کمکی برای لاگ‌گذاری شروع عملیات - طبق قرارداد Medical Environment
        /// </summary>
        private void LogOperationStart(string operation, string nationalCode, int? patientId = null, object additionalData = null)
        {
            var patientInfo = patientId.HasValue ? 
                $"شناسه {patientId} با کد ملی {nationalCode}" : 
                $"جدید با کد ملی {nationalCode}";

            var additionalInfo = additionalData != null ? 
                $" | اطلاعات اضافی: {System.Text.Json.JsonSerializer.Serialize(additionalData)}" : 
                "";

            _log.Information(
                "شروع عملیات {Operation} بیمار {PatientInfo}. " +
                "User: {UserName} (Id: {UserId}){AdditionalInfo}",
                operation,
                patientInfo,
                _currentUserService.UserName,
                _currentUserService.UserId,
                additionalInfo);
        }

        /// <summary>
        /// متد کمکی برای لاگ‌گذاری موفقیت عملیات - طبق قرارداد Medical Environment
        /// </summary>
        private void LogOperationSuccess(string operation, string nationalCode, int? patientId = null, string resultMessage = null)
        {
            var patientInfo = patientId.HasValue ? 
                $"شناسه {patientId} با کد ملی {nationalCode}" : 
                $"جدید با کد ملی {nationalCode}";

            var message = !string.IsNullOrEmpty(resultMessage) ? 
                $" | پیام: {resultMessage}" : 
                "";

            _log.Information(
                "عملیات {Operation} بیمار {PatientInfo} با موفقیت انجام شد. " +
                "User: {UserName} (Id: {UserId}){Message}",
                operation,
                patientInfo,
                _currentUserService.UserName,
                _currentUserService.UserId,
                message);
        }

        /// <summary>
        /// متد کمکی برای لاگ‌گذاری شکست عملیات - طبق قرارداد Medical Environment
        /// </summary>
        private void LogOperationFailure(string operation, string nationalCode, int? patientId = null, string errorMessage = null, Exception exception = null)
        {
            var patientInfo = patientId.HasValue ? 
                $"شناسه {patientId} با کد ملی {nationalCode}" : 
                $"جدید با کد ملی {nationalCode}";

            var message = !string.IsNullOrEmpty(errorMessage) ? 
                $" | خطا: {errorMessage}" : 
                "";

            if (exception != null)
            {
                _log.Error(
                    exception,
                    "عملیات {Operation} بیمار {PatientInfo} ناموفق بود. " +
                    "User: {UserName} (Id: {UserId}){Message}",
                    operation,
                    patientInfo,
                    _currentUserService.UserName,
                    _currentUserService.UserId,
                    message);
            }
            else
            {
                _log.Warning(
                    "عملیات {Operation} بیمار {PatientInfo} ناموفق بود. " +
                    "User: {UserName} (Id: {UserId}){Message}",
                    operation,
                    patientInfo,
                    _currentUserService.UserName,
                    _currentUserService.UserId,
                    message);
            }
        }

        #endregion
    }
}