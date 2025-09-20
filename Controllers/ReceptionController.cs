using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Reception;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Validators;
using FluentValidation;
using Serilog;
using Microsoft.AspNet.Identity;
using ClinicApp.Services;
using ClinicApp.Models;
using System.Data.Entity;
using PatientInquiryViewModel = ClinicApp.ViewModels.Reception.PatientInquiryViewModel;
using ClinicApp.Interfaces.Insurance;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر مدیریت پذیرش‌های بیماران - قلب تپنده سیستم کلینیک شفا
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
    /// طبق AI_COMPLIANCE_CONTRACT: قانون 25 - اصل SRP برای کنترلرها
    /// طبق AI_COMPLIANCE_CONTRACT: قانون 26 - جلوگیری از تکرار در کنترلرها
    /// </summary>
    //[Authorize(Roles = "Receptionist,Admin")]
    //[RequireHttps] // Force HTTPS in production
    public class ReceptionController : BaseController
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly IServiceCalculationService _serviceCalculationService;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IPatientInsuranceService _patientInsuranceService;

        public ReceptionController(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            ILogger logger,
            IServiceCalculationService serviceCalculationService,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IPatientInsuranceService patientInsuranceService) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceCalculationService = serviceCalculationService ?? throw new ArgumentNullException(nameof(serviceCalculationService));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _patientInsuranceService = patientInsuranceService ?? throw new ArgumentNullException(nameof(patientInsuranceService));
        }

        #endregion

        #region Main Views

        /// <summary>
        /// صفحه اصلی پذیرش
        /// </summary>
        /// <returns>صفحه پذیرش</returns>
    [HttpGet]
    public ActionResult Index()
    {
        _logger.Information(
            "ورود به صفحه اصلی پذیرش. کاربر: {UserName}",
            _currentUserService.UserName);

        try
        {
            var model = new ReceptionSearchViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                StartDateShamsi = "", // مقدار اولیه خالی
                EndDateShamsi = "", // مقدار اولیه خالی
                StatusList = GetReceptionStatusList(),
                TypeList = GetReceptionTypeList(),
                PaymentMethodList = GetPaymentMethodList(),
                InsuranceList = GetInsuranceList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "خطا در نمایش صفحه اصلی پذیرش. کاربر: {UserName}",
                _currentUserService.UserName);

            TempData["ErrorMessage"] = "خطا در نمایش صفحه پذیرش. لطفاً مجدداً تلاش کنید.";
            return RedirectToAction("Index", "Home");
        }
    }

        /// <summary>
        /// صفحه ایجاد پذیرش جدید
        /// </summary>
        /// <returns>فرم ایجاد پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            _logger.Information(
                "ورود به صفحه ایجاد پذیرش جدید. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                var model = new ReceptionCreateViewModel
                {
                    ReceptionDate = DateTime.Now,
                    ReceptionDateShamsi = "", // مقدار اولیه خالی طبق قرارداد
                    BirthDateShamsiForInquiry = "", // مقدار اولیه خالی طبق قرارداد
                    IsEmergency = false,
                    IsOnlineReception = false
                };

                // ✅ پر کردن Lookup Lists
                await PopulateCreateViewModel(model);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در نمایش صفحه ایجاد پذیرش. کاربر: {UserName}",
                    _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در نمایش فرم پذیرش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// صفحه جزئیات پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>جزئیات پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            _logger.Information(
                "درخواست جزئیات پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "شناسه پذیرش نامعتبر است.";
                    return RedirectToAction("Index");
                }

                var result = await _receptionService.GetReceptionDetailsAsync(id);
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت جزئیات پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در دریافت جزئیات پذیرش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region AJAX Endpoints - Patient Lookup

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی (AJAX)
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>اطلاعات بیمار</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 300, VaryByParam = "nationalCode")] // Cache for 5 minutes
        // Rate limiting will be implemented with custom middleware
        public async Task<JsonResult> LookupPatientByNationalCode(string nationalCode)
        {
            _logger.Information(
                "جستجوی بیمار بر اساس کد ملی. کد ملی: {NationalCode}, کاربر: {UserName}",
                nationalCode, _currentUserService.UserName);

            try
            {
                // Security Headers
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                Response.Headers.Add("X-Frame-Options", "DENY");
                Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

                // Security validation
                if (!ValidateRequiredField(nationalCode, "کد ملی"))
                {
                    return HandleModelStateErrors();
                }

                // Additional security checks
                if (!IsValidNationalCode(nationalCode))
                {
                    return Json(new { success = false, message = "کد ملی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _receptionService.LookupPatientByNationalCodeAsync(nationalCode);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return SuccessResponse(result.Data, "بیمار با موفقیت یافت شد.");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "جستجوی بیمار", _currentUserService.UserName);
            }
        }

        /// <summary>
        /// جستجوی بیماران بر اساس نام (AJAX)
        /// </summary>
        /// <param name="searchTerm">نام بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست بیماران</returns>
        [HttpGet]
        public async Task<JsonResult> SearchPatientsByName(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            _logger.Information(
                "جستجوی بیماران بر اساس نام. نام: {SearchTerm}, صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                searchTerm, pageNumber, pageSize, _currentUserService.UserName);

            try
            {
                if (!ValidateRequiredField(searchTerm, "نام بیمار"))
                {
                    return HandleModelStateErrors();
                }

                var result = await _receptionService.SearchPatientsByNameAsync(searchTerm, pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return Json(new
                {
                    success = true,
                    data = result.Data,
                    message = "جستجوی بیماران با موفقیت انجام شد.",
                    timestamp = DateTime.Now,
                    requestId = Guid.NewGuid().ToString("N").Substring(0, 8)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "جستجوی بیماران", _currentUserService.UserName);
            }
        }

        /// <summary>
        /// ایجاد بیمار جدید در حین پذیرش (AJAX)
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>اطلاعات بیمار جدید</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreatePatientInline(PatientCreateEditViewModel model)
        {
            _logger.Information(
                "ایجاد بیمار جدید در حین پذیرش. نام: {FirstName} {LastName}, کد ملی: {NationalCode}, کاربر: {UserName}",
                model?.FirstName, model?.LastName, model?.NationalCode, _currentUserService.UserName);

            try
            {
                if (!ModelState.IsValid)
                {
                    return HandleModelStateErrors();
                }

                var result = await _receptionService.CreatePatientInlineAsync(model);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return SuccessResponse(result.Data, "بیمار جدید با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "ایجاد بیمار جدید", _currentUserService.UserName);
            }
        }

        #endregion

        #region AJAX Endpoints - Lookup Lists

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات (AJAX)
        /// </summary>
        /// <returns>لیست دسته‌بندی‌های خدمات</returns>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategories()
        {
            _logger.Information(
                "درخواست لیست دسته‌بندی‌های خدمات. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetServiceCategoriesAsync();
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return SuccessResponse(result.Data, "لیست دسته‌بندی‌های خدمات با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "دریافت لیست دسته‌بندی‌های خدمات", _currentUserService.UserName);
            }
        }

        /// <summary>
        /// دریافت لیست خدمات بر اساس دسته‌بندی (AJAX)
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        [HttpGet]
        public async Task<JsonResult> GetServicesByCategory(int categoryId)
        {
            _logger.Information(
                "درخواست لیست خدمات بر اساس دسته‌بندی. شناسه: {CategoryId}, کاربر: {UserName}",
                categoryId, _currentUserService.UserName);

            try
            {
                if (categoryId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دسته‌بندی نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _receptionService.GetServicesByCategoryAsync(categoryId);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست خدمات. دسته‌بندی: {CategoryId}, کاربر: {UserName}",
                    categoryId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست خدمات." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان (AJAX)
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        [HttpGet]
        public async Task<JsonResult> GetDoctors()
        {
            _logger.Information(
                "درخواست لیست پزشکان. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetDoctorsAsync();
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پزشکان. کاربر: {UserName}",
                    _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت دپارتمان‌های پزشک (AJAX)
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>لیست دپارتمان‌های پزشک</returns>
        [HttpGet]
        public async Task<JsonResult> GetDoctorDepartments(int doctorId)
        {
            _logger.Information(
                "درخواست دریافت دپارتمان‌های پزشک. شناسه: {DoctorId}, کاربر: {UserName}",
                doctorId, _currentUserService.UserName);

            try
            {
                if (doctorId <= 0)
                {
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _receptionService.GetDoctorDepartmentsAsync(doctorId);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت دپارتمان‌های پزشک. شناسه: {DoctorId}, کاربر: {UserName}",
                    doctorId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت دپارتمان‌های پزشک." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت سرفصل‌های خدمات بر اساس دپارتمان‌ها (AJAX)
        /// </summary>
        /// <param name="departmentIds">شناسه‌های دپارتمان‌ها (comma-separated)</param>
        /// <returns>لیست سرفصل‌های خدمات</returns>
        [HttpGet]
        public async Task<JsonResult> GetServiceCategoriesByDepartments(string departmentIds)
        {
            _logger.Information(
                "درخواست دریافت سرفصل‌های خدمات بر اساس دپارتمان‌ها. شناسه‌ها: {DepartmentIds}, کاربر: {UserName}",
                departmentIds, _currentUserService.UserName);

            try
            {
                if (string.IsNullOrEmpty(departmentIds))
                {
                    return Json(new { success = false, message = "شناسه‌های دپارتمان الزامی است." }, JsonRequestBehavior.AllowGet);
                }

                var departmentIdList = departmentIds.Split(',').Select(id => int.Parse(id.Trim())).ToList();
                var result = await _receptionService.GetServiceCategoriesByDepartmentsAsync(departmentIdList);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت سرفصل‌های خدمات. دپارتمان‌ها: {DepartmentIds}, کاربر: {UserName}",
                    departmentIds, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت سرفصل‌های خدمات." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region AJAX Endpoints - Reception Operations

        /// <summary>
        /// ایجاد پذیرش جدید (AJAX)
        /// </summary>
        /// <param name="model">اطلاعات پذیرش</param>
        /// <returns>نتیجه ایجاد پذیرش</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateReception(ReceptionCreateViewModel model)
        {
            _logger.Information(
                "ایجاد پذیرش جدید. بیمار: {PatientId}, خدمت: {ServiceId}, پزشک: {DoctorId}, کاربر: {UserName}",
                model?.PatientId, model?.ServiceId, model?.DoctorId, _currentUserService.UserName);

            try
            {
                // اعتبارسنجی با FluentValidation
                var validator = new ReceptionCreateViewModelValidator();
                var validationResult = validator.Validate(model);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors }, JsonRequestBehavior.AllowGet);
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors }, JsonRequestBehavior.AllowGet);
                }

                var result = await _receptionService.CreateReceptionAsync(model);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data, redirectUrl = Url.Action("Details", new { id = result.Data.ReceptionId }) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ایجاد پذیرش. بیمار: {PatientId}, کاربر: {UserName}",
                    model?.PatientId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در ایجاد پذیرش. لطفاً مجدداً تلاش کنید." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ویرایش پذیرش موجود (AJAX)
        /// </summary>
        /// <param name="model">اطلاعات پذیرش برای ویرایش</param>
        /// <returns>نتیجه ویرایش پذیرش</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditReception(ReceptionEditViewModel model)
        {
            _logger.Information(
                "ویرایش پذیرش. شناسه: {ReceptionId}, بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                model?.ReceptionId, model?.PatientId, model?.DoctorId, _currentUserService.UserName);

            try
            {
                // اعتبارسنجی با FluentValidation
                var validator = new ReceptionEditViewModelValidator();
                var validationResult = validator.Validate(model);
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors }, JsonRequestBehavior.AllowGet);
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors }, JsonRequestBehavior.AllowGet);
                }

                var result = await _receptionService.UpdateReceptionAsync(model);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data, message = "پذیرش با موفقیت ویرایش شد." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    model?.ReceptionId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در ویرایش پذیرش. لطفاً مجدداً تلاش کنید." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها (AJAX)
        /// </summary>
        /// <param name="patientId">شناسه بیمار (اختیاری)</param>
        /// <param name="doctorId">شناسه پزشک (اختیاری)</param>
        /// <param name="status">وضعیت پذیرش (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو (اختیاری)</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست پذیرش‌ها</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetReceptions(int? patientId, int? doctorId, string status, string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            _logger.Information(
                "درخواست لیست پذیرش‌ها. بیمار: {PatientId}, پزشک: {DoctorId}, وضعیت: {Status}, جستجو: {SearchTerm}, صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                patientId, doctorId, status, searchTerm, pageNumber, pageSize, _currentUserService.UserName);

            try
            {
                Models.Enums.ReceptionStatus? receptionStatus = null;
                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Models.Enums.ReceptionStatus>(status, out var parsedStatus))
                {
                    receptionStatus = parsedStatus;
                }

                var result = await _receptionService.GetReceptionsAsync(patientId, doctorId, receptionStatus, searchTerm, pageNumber, pageSize);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پذیرش‌ها. کاربر: {UserName}",
                    _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست پذیرش‌ها." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region External Inquiry

        /// <summary>
        /// استعلام هویت بیمار از شبکه شمس (AJAX)
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>نتیجه استعلام</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> InquiryPatientIdentity(string nationalCode, DateTime birthDate)
        {
            _logger.Information(
                "استعلام هویت بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, کاربر: {UserName}",
                nationalCode, birthDate, _currentUserService.UserName);

            try
            {
                if (!ValidateRequiredField(nationalCode, "کد ملی"))
                {
                    return HandleModelStateErrors();
                }

                if (birthDate == default(DateTime))
                {
                    ModelState.AddModelError("birthDate", "تاریخ تولد الزامی است.");
                    return HandleModelStateErrors();
                }

                var result = await _receptionService.InquiryPatientIdentityAsync(nationalCode, birthDate);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return SuccessResponse(result.Data, "استعلام هویت بیمار با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "استعلام هویت بیمار", _currentUserService.UserName);
            }
        }

        /// <summary>
        /// ایجاد پذیرش جدید
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه ایجاد</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ReceptionCreateViewModel model)
        {
            _logger.Information(
                "درخواست ایجاد پذیرش جدید. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                model.PatientId, model.DoctorId, _currentUserService.UserName);

            try
            {
                // ✅ پردازش فیلدهای شمسی طبق قرارداد
                if (!string.IsNullOrEmpty(model.ReceptionDateShamsi))
                {
                    model.ReceptionDate = model.ReceptionDateShamsi.ToDateTime();
                }
                
                if (!string.IsNullOrEmpty(model.BirthDateShamsiForInquiry))
                {
                    model.BirthDateForInquiry = model.BirthDateShamsiForInquiry.ToDateTimeNullable();
                }

                if (!ModelState.IsValid)
                {
                    await PopulateCreateViewModel(model);
                    return View(model);
                }

                var result = await _receptionService.CreateReceptionAsync(model);

                if (!result.Success)
                {
                    await PopulateCreateViewModel(model);
                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }

                _logger.Information(
                    "پذیرش با موفقیت ایجاد شد. شناسه: {ReceptionId}, کاربر: {UserName}",
                    result.Data?.ReceptionId, _currentUserService.UserName);

                TempData["SuccessMessage"] = "پذیرش با موفقیت ایجاد شد.";
                return RedirectToAction("Details", new { id = result.Data?.ReceptionId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ایجاد پذیرش. بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model.PatientId, model.DoctorId, _currentUserService.UserName);

                await PopulateCreateViewModel(model);
                TempData["ErrorMessage"] = "خطا در ایجاد پذیرش. لطفاً مجدداً تلاش کنید.";
                return View(model);
            }
        }

        /// <summary>
        /// صفحه ویرایش پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>فرم ویرایش پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            _logger.Information(
                "درخواست ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetReceptionDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var editModel = new ReceptionEditViewModel
                {
                    ReceptionId = result.Data.ReceptionId,
                    PatientId = result.Data.PatientId,
                    PatientFullName = result.Data.PatientFullName,
                    DoctorId = result.Data.DoctorId,
                    DoctorFullName = result.Data.DoctorFullName,
                    ReceptionDate = DateTime.Parse(result.Data.ReceptionDate),
                    ReceptionDateShamsi = DateTime.Parse(result.Data.ReceptionDate).ToPersianDate(),
                    IsEmergency = result.Data.Type == "اورژانس",
                    Notes = result.Data.Notes,
                    Status = ParseReceptionStatus(result.Data.Status)
                };

                await PopulateEditViewModel(editModel);
                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در نمایش فرم ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در نمایش فرم ویرایش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه ویرایش</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ReceptionEditViewModel model)
        {
            _logger.Information(
                "درخواست ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                model.ReceptionId, _currentUserService.UserName);

            try
            {
                // ✅ پردازش فیلدهای شمسی طبق قرارداد
                if (!string.IsNullOrEmpty(model.ReceptionDateShamsi))
                {
                    model.ReceptionDate = model.ReceptionDateShamsi.ToDateTime();
                }

                if (!ModelState.IsValid)
                {
                    await PopulateEditViewModel(model);
                    return View(model);
                }

                var result = await _receptionService.UpdateReceptionAsync(model);

                if (!result.Success)
                {
                    await PopulateEditViewModel(model);
                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }

                _logger.Information(
                    "پذیرش با موفقیت ویرایش شد. شناسه: {ReceptionId}, کاربر: {UserName}",
                    model.ReceptionId, _currentUserService.UserName);

                TempData["SuccessMessage"] = "پذیرش با موفقیت ویرایش شد.";
                return RedirectToAction("Details", new { id = model.ReceptionId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    model.ReceptionId, _currentUserService.UserName);

                await PopulateEditViewModel(model);
                TempData["ErrorMessage"] = "خطا در ویرایش پذیرش. لطفاً مجدداً تلاش کنید.";
                return View(model);
            }
        }

        /// <summary>
        /// صفحه حذف پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>فرم حذف پذیرش</returns>
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.Information(
                "درخواست حذف پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetReceptionDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در نمایش فرم حذف پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در نمایش فرم حذف. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// حذف پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه حذف</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _logger.Information(
                "درخواست حذف پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                id, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.DeleteReceptionAsync(id);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _logger.Information(
                    "پذیرش با موفقیت حذف شد. شناسه: {ReceptionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                TempData["SuccessMessage"] = "پذیرش با موفقیت حذف شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در حذف پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    id, _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در حذف پذیرش. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>نتایج جستجو</returns>
        [HttpPost]
        public async Task<ActionResult> Search(ReceptionSearchViewModel model, int pageNumber = 1, int pageSize = 20)
        {
            _logger.Information(
                "درخواست جستجوی پذیرش‌ها. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                pageNumber, pageSize, _currentUserService.UserName);

            try
            {
                var result = await _receptionService.GetReceptionsAsync(null, null, null, null, pageNumber, pageSize);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var viewModel = new ReceptionIndexViewModel
                {
                    ReceptionId = 0, // Multiple receptions handled in search results
                    PatientFullName = "نتایج جستجو",
                    DoctorFullName = "",
                    ReceptionDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    TotalAmount = result.Data?.Items?.Sum(r => r.TotalAmount) ?? 0,
                    Status = "نتایج جستجو"
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در جستجوی پذیرش‌ها. صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    pageNumber, pageSize, _currentUserService.UserName);

                TempData["ErrorMessage"] = "خطا در جستجوی پذیرش‌ها. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// دریافت لیست پذیرش‌ها برای AJAX (JSON)
        /// </summary>
        /// <param name="patientNationalCode">کد ملی بیمار</param>
        /// <param name="patientName">نام بیمار</param>
        /// <param name="doctorName">نام پزشک</param>
        /// <param name="status">وضعیت پذیرش</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="type">نوع پذیرش</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>JSON response</returns>
        [HttpGet]
        public async Task<JsonResult> GetReceptions(
            string patientNationalCode = null,
            string patientName = null,
            string doctorName = null,
            string status = null,
            string startDate = null,
            string endDate = null,
            string startDateShamsi = null,
            string endDateShamsi = null,
            string type = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _logger.Information(
                "درخواست AJAX دریافت پذیرش‌ها. RequestId: {RequestId}, صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}, پارامترها: {@Parameters}",
                requestId, pageNumber, pageSize, _currentUserService.UserName, 
                new { patientNationalCode, patientName, doctorName, status, startDate, endDate, type });

            // حذف تست response - فعال کردن منطق اصلی

            try
            {
                // تبدیل پارامترهای جستجو
                DateTime? start = null;
                DateTime? end = null;
                ReceptionStatus? receptionStatus = null;
                ReceptionType? receptionType = null;

                // اولویت با فیلدهای شمسی
                if (!string.IsNullOrEmpty(startDateShamsi))
                {
                    start = startDateShamsi.ToDateTime();
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParse(startDate, out var parsedStart))
                        start = parsedStart;
                }

                if (!string.IsNullOrEmpty(endDateShamsi))
                {
                    end = endDateShamsi.ToDateTime();
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(endDate, out var parsedEnd))
                        end = parsedEnd;
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParse(endDate, out var parsedEnd))
                        end = parsedEnd;
                }

                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<ReceptionStatus>(status, out var parsedStatus))
                        receptionStatus = parsedStatus;
                }

                if (!string.IsNullOrEmpty(type))
                {
                    if (Enum.TryParse<ReceptionType>(type, out var parsedType))
                        receptionType = parsedType;
                }

                // دریافت داده‌ها از سرویس
                // TODO: نیاز به پیاده‌سازی GetReceptionsAsync با پارامترهای بیشتر
                var result = await _receptionService.GetReceptionsAsync(
                    null, // patientId
                    null, // doctorId  
                    receptionStatus,
                    $"{patientNationalCode} {patientName} {doctorName}", // searchTerm
                    pageNumber,
                    pageSize);

                if (!result.Success)
                {
                    _logger.Error("خطا در دریافت پذیرش‌ها از Service. پیام: {Message}, کد خطا: {ErrorCode}, کاربر: {UserName}",
                        result.Message, result.Code, _currentUserService.UserName);
                    
                return Json(new {
                    success = false,
                        message = result.Message ?? "خطا در دریافت لیست پذیرش‌ها",
                        errorCode = result.Code ?? "SERVICE_ERROR",
                    errorId = Guid.NewGuid().ToString("N").Substring(0, 8)
                }, JsonRequestBehavior.AllowGet);
                }

                // تبدیل به فرمت مناسب برای UI
                var responseData = new
                {
                    items = result.Data?.Items?.Select(r => new
                    {
                        receptionId = r.ReceptionId,
                        patientFullName = r.PatientFullName ?? "نامشخص",
                        patientNationalCode = r.PatientNationalCode ?? "نامشخص",
                        doctorFullName = r.DoctorFullName ?? "نامشخص",
                        receptionDate = r.ReceptionDate ?? "نامشخص",
                        status = r.Status ?? "نامشخص",
                        type = r.Type ?? "نامشخص",
                        totalAmount = r.TotalAmount,
                        paidAmount = r.PaidAmount,
                        remainingAmount = r.RemainingAmount,
                        paymentMethod = r.PaymentMethod ?? "نامشخص"
                    }) ?? Enumerable.Empty<object>(),
                    totalCount = result.Data?.TotalItems ?? 0,
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    totalPages = result.Data?.TotalPages ?? 0
                };

                _logger.Information(
                    "دریافت موفق پذیرش‌ها. RequestId: {RequestId}, تعداد: {Count}, صفحه: {PageNumber}, کاربر: {UserName}",
                    requestId, result.Data?.Items?.Count ?? 0, pageNumber, _currentUserService.UserName);

                // ساختار JSON استاندارد برای JavaScript
                var standardResponse = new
                {
                    success = true,
                    data = responseData,
                    message = "لیست پذیرش‌ها با موفقیت دریافت شد.",
                    timestamp = DateTime.UtcNow,
                    requestId = Guid.NewGuid().ToString("N").Substring(0, 8)
                };

                return Json(standardResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت پذیرش‌ها. RequestId: {RequestId}, صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                    requestId, pageNumber, pageSize, _currentUserService.UserName);

                return Json(new {
                    success = false,
                    message = "خطا در دریافت پذیرش‌ها. لطفاً مجدداً تلاش کنید.",
                    errorCode = "UNEXPECTED_ERROR",
                    errorId = Guid.NewGuid().ToString("N").Substring(0, 8)
                }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region Helper Methods

        /// <summary>
        /// پر کردن ViewModel ایجاد با داده‌های مورد نیاز
        /// </summary>
        /// <param name="model">مدل ایجاد</param>
        private async Task PopulateCreateViewModel(ReceptionCreateViewModel model)
        {
            try
            {
                // پیاده‌سازی Lookup Lists
                var lookupLists = await GetReceptionLookupListsAsync();
                
                if (lookupLists.Success)
                {
                    // ✅ بهبود DoctorList
                    model.DoctorList = lookupLists.Data.Doctors?.Select(d => new SelectListItem
                    {
                        Value = d.DoctorId.ToString(),
                        Text = d.FullName,
                        Selected = d.DoctorId == model.DoctorId
                    }).ToList() ?? new List<SelectListItem>();

                    // ✅ بهبود ServiceList
                    model.ServiceList = lookupLists.Data.Services?.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = s.Title,
                        Selected = model.SelectedServiceIds?.Contains(s.ServiceId) ?? false
                    }).ToList() ?? new List<SelectListItem>();

                    // ✅ بهبود PaymentMethodList
                    model.PaymentMethodList = lookupLists.Data.PaymentMethods?.Select(p => new SelectListItem
                    {
                        Value = p.Value.ToString(),
                        Text = p.Text,
                        Selected = p.Value == model.PaymentMethod
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پر کردن ViewModel ایجاد پذیرش");
            }
        }

        /// <summary>
        /// پر کردن ViewModel ویرایش با داده‌های مورد نیاز
        /// </summary>
        /// <param name="model">مدل ویرایش</param>
        private async Task PopulateEditViewModel(ReceptionEditViewModel model)
        {
            try
            {
                // پیاده‌سازی Lookup Lists
                var lookupLists = await GetReceptionLookupListsAsync();
                
                if (lookupLists.Success)
                {
                    model.DoctorList = lookupLists.Data.Doctors.Select(d => new SelectListItem
                    {
                        Value = d.DoctorId.ToString(),
                        Text = d.FullName,
                        Selected = d.DoctorId == model.DoctorId
                    }).ToList();

                    model.ServiceList = lookupLists.Data.Services.Select(s => new SelectListItem
                    {
                        Value = s.ServiceId.ToString(),
                        Text = s.Title,
                        Selected = model.SelectedServiceIds?.Contains(s.ServiceId) ?? false
                    }).ToList();

                    // اضافه کردن PaymentMethodList
                    model.PaymentMethodList = lookupLists.Data.PaymentMethods.Select(p => new SelectListItem
                    {
                        Value = p.Value.ToString(),
                        Text = p.Text,
                        Selected = p.Value == model.PaymentMethod
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پر کردن ViewModel ویرایش پذیرش");
            }
        }

        /// <summary>
        /// بررسی وجود خطا در ModelState
        /// </summary>
        /// <returns>آیا خطا وجود دارد؟</returns>
        private bool HasModelStateErrors()
        {
            return !ModelState.IsValid;
        }

        /// <summary>
        /// دریافت خطاهای ModelState
        /// </summary>
        /// <returns>لیست خطاها</returns>
        private List<string> GetModelStateErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }

        /// <summary>
        /// دریافت لیست وضعیت‌های پذیرش
        /// </summary>
        /// <returns>لیست SelectListItem</returns>
        private List<SelectListItem> GetReceptionStatusList()
        {
            return Enum.GetValues(typeof(Models.Enums.ReceptionStatus))
                .Cast<Models.Enums.ReceptionStatus>()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = GetReceptionStatusDisplayName(status)
                })
                .ToList();
        }

        /// <summary>
        /// دریافت لیست انواع پذیرش
        /// </summary>
        /// <returns>لیست SelectListItem</returns>
        private List<SelectListItem> GetReceptionTypeList()
        {
            return Enum.GetValues(typeof(ReceptionType))
                .Cast<ReceptionType>()
                .Select(type => new SelectListItem
                {
                    Value = ((int)type).ToString(),
                    Text = GetReceptionTypeDisplayName(type)
                })
                .ToList();
        }

        /// <summary>
        /// دریافت لیست روش‌های پرداخت
        /// </summary>
        /// <returns>لیست SelectListItem</returns>
        private List<SelectListItem> GetPaymentMethodList()
        {
            return Enum.GetValues(typeof(PaymentMethod))
                .Cast<PaymentMethod>()
                .Select(method => new SelectListItem
                {
                    Value = ((int)method).ToString(),
                    Text = GetPaymentMethodDisplayName(method)
                })
                .ToList();
        }

        /// <summary>
        /// دریافت لیست بیمه‌ها
        /// </summary>
        /// <returns>لیست بیمه‌ها</returns>
        private List<SelectListItem> GetInsuranceList()
        {
            // TODO: پیاده‌سازی دریافت لیست بیمه‌ها از Repository
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "انتخاب بیمه", Selected = true },
                new SelectListItem { Value = "1", Text = "تأمین اجتماعی", Selected = false },
                new SelectListItem { Value = "2", Text = "خدمات درمانی", Selected = false },
                new SelectListItem { Value = "3", Text = "نیروهای مسلح", Selected = false },
                new SelectListItem { Value = "4", Text = "بیمه تکمیلی", Selected = false }
            };
        }

        /// <summary>
        /// دریافت لیست‌های Lookup برای فرم‌های پذیرش با Caching و Parallel Processing
        /// </summary>
        /// <returns>لیست‌های Lookup</returns>
        private async Task<ServiceResult<ReceptionLookupListsViewModel>> GetReceptionLookupListsAsync()
        {
            try
            {
                // Cache key برای lookup lists
                var cacheKey = "reception_lookup_lists";
                var cachedData = HttpContext.Cache[cacheKey] as ReceptionLookupListsViewModel;
                
                if (cachedData != null)
                {
                    _logger.Debug("استفاده از cached lookup lists");
                    return ServiceResult<ReceptionLookupListsViewModel>.Successful(cachedData);
                }

                var lookupLists = new ReceptionLookupListsViewModel
                {
                    Doctors = new List<ReceptionDoctorLookupViewModel>(),
                    Patients = new List<ReceptionPatientLookupViewModel>(),
                    Services = new List<ReceptionServiceLookupViewModel>(),
                    PaymentMethods = GetPaymentMethodList().Select(p => new ClinicApp.ViewModels.Payment.PaymentMethodLookupViewModel
                    {
                        Value = (ClinicApp.Models.Enums.PaymentMethod)int.Parse(p.Value),
                        Text = p.Text
                    }).ToList()
                };

                // دریافت داده‌ها به صورت موازی برای بهبود Performance
                var doctorsTask = _receptionService.GetDoctorsAsync();
                var serviceCategoriesTask = _receptionService.GetServiceCategoriesAsync();
                var allServicesTask = _receptionService.GetServicesByCategoryAsync(0); // 0 = همه خدمات

                // انتظار برای تکمیل تمام Task ها
                await Task.WhenAll(doctorsTask, serviceCategoriesTask, allServicesTask);

                // پردازش نتایج
                if (doctorsTask.Result.Success)
                {
                    lookupLists.Doctors = doctorsTask.Result.Data;
                }

                if (serviceCategoriesTask.Result.Success)
                {
                    lookupLists.ServiceCategories = serviceCategoriesTask.Result.Data;
                }

                if (allServicesTask.Result.Success)
                {
                    lookupLists.Services = allServicesTask.Result.Data;
                }

                // Cache کردن داده‌ها برای 15 دقیقه
                HttpContext.Cache.Insert(cacheKey, lookupLists, null, 
                    DateTime.Now.AddMinutes(15), TimeSpan.Zero);

                _logger.Debug("Lookup lists cached for 15 minutes");
                return ServiceResult<ReceptionLookupListsViewModel>.Successful(lookupLists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست‌های Lookup");
                return ServiceResult<ReceptionLookupListsViewModel>.Failed("خطا در دریافت لیست‌های Lookup");
            }
        }

        /// <summary>
        /// تبدیل رشته وضعیت به enum
        /// </summary>
        /// <param name="statusString">رشته وضعیت</param>
        /// <returns>enum وضعیت</returns>
        private ReceptionStatus ParseReceptionStatus(string statusString)
        {
            if (string.IsNullOrWhiteSpace(statusString))
                return ReceptionStatus.Pending;

            switch (statusString.ToLower())
            {
                case "تکمیل شده":
                case "completed":
                    return ReceptionStatus.Completed;
                case "در انتظار":
                case "pending":
                    return ReceptionStatus.Pending;
                case "لغو شده":
                case "cancelled":
                    return ReceptionStatus.Cancelled;
                case "عدم حضور":
                case "no show":
                    return ReceptionStatus.NoShow;
                default:
                    return ReceptionStatus.Pending;
            }
        }

        /// <summary>
        /// دریافت نام نمایشی وضعیت پذیرش
        /// </summary>
        /// <param name="status">وضعیت پذیرش</param>
        /// <returns>نام نمایشی</returns>
        private string GetReceptionStatusDisplayName(Models.Enums.ReceptionStatus status)
        {
            return status switch
            {
                Models.Enums.ReceptionStatus.Pending => "در انتظار",
                Models.Enums.ReceptionStatus.InProgress => "در حال انجام",
                Models.Enums.ReceptionStatus.Completed => "تکمیل شده",
                Models.Enums.ReceptionStatus.Cancelled => "لغو شده",
                Models.Enums.ReceptionStatus.NoShow => "عدم حضور",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// دریافت نام نمایشی نوع پذیرش
        /// </summary>
        /// <param name="type">نوع پذیرش</param>
        /// <returns>نام نمایشی</returns>
        private string GetReceptionTypeDisplayName(ReceptionType type)
        {
            return type switch
            {
                ReceptionType.Normal => "عادی",
                ReceptionType.Emergency => "اورژانس",
                ReceptionType.Special => "ویژه",
                ReceptionType.Online => "آنلاین",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// دریافت نام نمایشی روش پرداخت
        /// </summary>
        /// <param name="method">روش پرداخت</param>
        /// <returns>نام نمایشی</returns>
        private string GetPaymentMethodDisplayName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدی",
                PaymentMethod.POS => "پوز",
                PaymentMethod.Online => "آنلاین",
                PaymentMethod.Debt => "بدهی",
                PaymentMethod.Card => "کارت به کارت",
                PaymentMethod.BankTransfer => "حواله بانکی",
                PaymentMethod.Insurance => "بیمه",
                _ => method.ToString()
            };
        }

        /// <summary>
        /// اعتبارسنجی کد ملی ایرانی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <returns>آیا کد ملی معتبر است؟</returns>
        private bool IsValidNationalCode(string nationalCode)
        {
            if (string.IsNullOrWhiteSpace(nationalCode) || nationalCode.Length != 10)
                return false;

            // Check if all characters are digits
            if (!nationalCode.All(char.IsDigit))
                return false;

            // Iranian National Code validation algorithm
            var digits = nationalCode.Select(c => int.Parse(c.ToString())).ToArray();
            
            // Check for invalid patterns (all same digits)
            if (digits.All(d => d == digits[0]))
                return false;

            // Calculate check digit
            var sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += digits[i] * (10 - i);
            }
            
            var remainder = sum % 11;
            var checkDigit = remainder < 2 ? remainder : 11 - remainder;
            
            return checkDigit == digits[9];
        }

        /// <summary>
        /// اعتبارسنجی ورودی‌های امنیتی
        /// </summary>
        /// <param name="input">ورودی</param>
        /// <param name="maxLength">حداکثر طول</param>
        /// <returns>آیا ورودی امن است؟</returns>
        private bool IsSecureInput(string input, int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true;

            if (input.Length > maxLength)
                return false;

            // Check for SQL injection patterns
            var dangerousPatterns = new[] { "'", "\"", ";", "--", "/*", "*/", "xp_", "sp_" };
            return !dangerousPatterns.Any(pattern => input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// اعتبارسنجی تاریخ
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آیا تاریخ معتبر است؟</returns>
        private bool IsValidDate(DateTime date)
        {
            var minDate = DateTime.Now.AddYears(-120);
            var maxDate = DateTime.Now.AddYears(1);
            
            return date >= minDate && date <= maxDate;
        }

        #endregion

        #region Enhanced Error Handling

        /// <summary>
        /// مدیریت پیشرفته خطاها در Controller
        /// </summary>
        /// <param name="ex">خطای رخ داده</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="context">اطلاعات اضافی</param>
        /// <returns>JsonResult با جزئیات خطا</returns>
        private JsonResult HandleEnhancedControllerError(Exception ex, string operationName, object context = null)
        {
            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var errorContext = new
            {
                ErrorId = errorId,
                Operation = operationName,
                UserId = _currentUserService.UserId,
                UserName = _currentUserService.UserName,
                Timestamp = DateTime.UtcNow,
                Context = context
            };

            // تشخیص نوع خطا و مدیریت مناسب
            switch (ex)
            {
                case ArgumentNullException argEx:
                    _logger.Warning("خطای ورودی نامعتبر در {Operation}. خطا: {ErrorId}, ورودی: {ArgumentName}, کاربر: {UserName}",
                        operationName, errorId, argEx.ParamName, _currentUserService.UserName);
                    return Json(new { 
                        success = false, 
                        message = $"ورودی نامعتبر: {argEx.ParamName}",
                        errorCode = "INVALID_INPUT",
                        errorId = errorId
                    });

                case ArgumentException argEx:
                    _logger.Warning("خطای اعتبارسنجی در {Operation}. خطا: {ErrorId}, پیام: {Message}, کاربر: {UserName}",
                        operationName, errorId, argEx.Message, _currentUserService.UserName);
                    return Json(new { 
                        success = false, 
                        message = $"خطای اعتبارسنجی: {argEx.Message}",
                        errorCode = "VALIDATION_ERROR",
                        errorId = errorId
                    });

                case UnauthorizedAccessException authEx:
                    _logger.Warning("خطای دسترسی در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return Json(new { 
                        success = false, 
                        message = "شما مجوز انجام این عملیات را ندارید.",
                        errorCode = "UNAUTHORIZED_ACCESS",
                        errorId = errorId
                    });

                case TimeoutException timeoutEx:
                    _logger.Error(timeoutEx, "خطای زمان‌بندی در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return Json(new { 
                        success = false, 
                        message = "عملیات بیش از حد انتظار طول کشید. لطفاً مجدداً تلاش کنید.",
                        errorCode = "OPERATION_TIMEOUT",
                        errorId = errorId
                    });

                default:
                    _logger.Error(ex, "خطای غیرمنتظره در {Operation}. خطا: {ErrorId}, کاربر: {UserName}",
                        operationName, errorId, _currentUserService.UserName);
                    return Json(new { 
                        success = false, 
                        message = "خطای غیرمنتظره‌ای رخ داده است. لطفاً با پشتیبانی تماس بگیرید.",
                        errorCode = "UNEXPECTED_ERROR",
                        errorId = errorId
                    });
            }
        }

        /// <summary>
        /// اعتبارسنجی پیشرفته ModelState
        /// </summary>
        /// <param name="model">مدل برای اعتبارسنجی</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        private ServiceResult<bool> ValidateModelStateAdvanced(object model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.Warning("خطای اعتبارسنجی ModelState. خطاها: {Errors}, کاربر: {UserName}",
                    string.Join(", ", errors), _currentUserService.UserName);

                return ServiceResult<bool>.Failed(
                    $"خطاهای اعتبارسنجی: {string.Join(", ", errors)}",
                    "MODEL_VALIDATION_FAILED",
                    ErrorCategory.Validation,
                    SecurityLevel.Low);
            }

            return ServiceResult<bool>.Successful(true);
        }

        /// <summary>
        /// مدیریت خطاهای ServiceResult
        /// </summary>
        /// <param name="result">نتیجه Service</param>
        /// <returns>JsonResult مناسب</returns>
        private JsonResult HandleServiceError(ServiceResult result)
        {
            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            _logger.Warning("خطای Service در {Operation}. خطا: {ErrorId}, پیام: {Message}, کاربر: {UserName}",
                result.OperationName ?? "Unknown", errorId, result.Message, _currentUserService.UserName);

            return Json(new { 
                success = false, 
                message = result.Message,
                errorCode = "SERVICE_ERROR",
                errorId = errorId
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// مدیریت خطاهای Exception
        /// </summary>
        /// <param name="ex">خطای رخ داده</param>
        /// <param name="operationName">نام عملیات</param>
        /// <param name="context">اطلاعات اضافی</param>
        /// <returns>JsonResult مناسب</returns>
        private JsonResult HandleException(Exception ex, string operationName, string context = null)
        {
            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            _logger.Error(ex, "خطا در {Operation}. خطا: {ErrorId}, کاربر: {UserName}, Context: {Context}",
                operationName, errorId, _currentUserService.UserName, context);

            return Json(new { 
                success = false, 
                message = "خطای غیرمنتظره‌ای رخ داده است. لطفاً مجدداً تلاش کنید.",
                errorCode = "UNEXPECTED_ERROR",
                errorId = errorId
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// مدیریت خطاهای ModelState
        /// </summary>
        /// <returns>JsonResult مناسب</returns>
        private JsonResult HandleModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            
            _logger.Warning("خطای اعتبارسنجی ModelState. خطاها: {Errors}, کاربر: {UserName}",
                string.Join(", ", errors), _currentUserService.UserName);

            return Json(new { 
                success = false, 
                message = "اطلاعات وارد شده نامعتبر است.",
                errors = errors,
                errorCode = "VALIDATION_ERROR",
                errorId = errorId
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// پاسخ موفقیت‌آمیز استاندارد
        /// </summary>
        /// <param name="data">داده‌های پاسخ</param>
        /// <param name="message">پیام موفقیت</param>
        /// <returns>JsonResult موفق</returns>
        private JsonResult SuccessResponse(object data, string message = "عملیات با موفقیت انجام شد.")
        {
            return Json(new { 
                success = true, 
                data = data,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Insurance Calculation Integration (یکپارچگی با محاسبه بیمه)

        /// <summary>
        /// محاسبه بیمه ترکیبی برای پذیرش بیمار
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculatePatientInsuranceForReception(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی برای پذیرش. PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}, Date: {Date}, User: {UserName} (Id: {UserId})", 
                    patientId, serviceId, serviceAmount, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var effectiveDate = calculationDate ?? DateTime.Now;

                // محاسبه بیمه ترکیبی
                var insuranceResult = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, effectiveDate);

                if (insuranceResult.Success)
                {
                    var result = new
                    {
                        success = true,
                        data = new
                        {
                            patientId = insuranceResult.Data.PatientId,
                            serviceId = insuranceResult.Data.ServiceId,
                            serviceAmount = insuranceResult.Data.ServiceAmount,
                            primaryCoverage = insuranceResult.Data.PrimaryCoverage,
                            primaryCoveragePercent = insuranceResult.Data.PrimaryCoveragePercent,
                            supplementaryCoverage = insuranceResult.Data.SupplementaryCoverage,
                            supplementaryCoveragePercent = insuranceResult.Data.SupplementaryCoveragePercent,
                            finalPatientShare = insuranceResult.Data.FinalPatientShare,
                            totalInsuranceCoverage = insuranceResult.Data.TotalInsuranceCoverage,
                            hasSupplementaryInsurance = insuranceResult.Data.HasSupplementaryInsurance,
                            notes = insuranceResult.Data.Notes,
                            calculationDate = insuranceResult.Data.CalculationDate
                        },
                        message = "محاسبه بیمه ترکیبی برای پذیرش با موفقیت انجام شد"
                    };

                    _logger.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی برای پذیرش موفق. PatientId: {PatientId}, ServiceId: {ServiceId}, PatientShare: {PatientShare}, TotalCoverage: {TotalCoverage}, User: {UserName} (Id: {UserId})", 
                        patientId, serviceId, insuranceResult.Data.FinalPatientShare, insuranceResult.Data.TotalInsuranceCoverage, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی برای پذیرش. PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}, User: {UserName} (Id: {UserId})", 
                        patientId, serviceId, insuranceResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new
                    {
                        success = false,
                        message = insuranceResult.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی برای پذیرش. PatientId: {PatientId}, ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در محاسبه بیمه ترکیبی برای پذیرش"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت اطلاعات بیمه‌های بیمار برای پذیرش
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetPatientInsurancesForReception(int patientId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست اطلاعات بیمه‌های بیمار برای پذیرش. PatientId: {PatientId}, User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var result = await _patientInsuranceService.GetPatientInsurancesByPatientAsync(patientId);

                if (result.Success)
                {
                    _logger.Information("🏥 MEDICAL: اطلاعات بیمه‌های بیمار برای پذیرش دریافت شد. PatientId: {PatientId}, Count: {Count}, User: {UserName} (Id: {UserId})", 
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
                    _logger.Warning("🏥 MEDICAL: خطا در دریافت اطلاعات بیمه‌های بیمار برای پذیرش. PatientId: {PatientId}, Error: {Error}, User: {UserName} (Id: {UserId})", 
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
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات بیمه‌های بیمار برای پذیرش. PatientId: {PatientId}, User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات بیمه‌های بیمار"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region ServiceCalculationService Integration (یکپارچگی با ServiceCalculationService)

        /// <summary>
        /// محاسبه قیمت خدمت با استفاده از ServiceCalculationService
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> CalculateServicePriceWithComponents(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت با ServiceComponents. ServiceId: {ServiceId}, Date: {Date}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    _logger.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
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

                _logger.Information("🏥 MEDICAL: محاسبه قیمت خدمت موفق. ServiceId: {ServiceId}, Price: {Price}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculatedPrice, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در محاسبه قیمت خدمت. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت جزئیات محاسبه خدمت
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetServiceCalculationDetails(int serviceId, DateTime? calculationDate = null)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: دریافت جزئیات محاسبه خدمت. ServiceId: {ServiceId}, Date: {Date}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculationDate, _currentUserService.UserName, _currentUserService.UserId);

                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var calculationDetails = _serviceCalculationService.CalculateServicePriceWithDetails(
                    service, _context, calculationDate ?? DateTime.Now);

                var result = new
                {
                    success = true,
                    serviceId = service.ServiceId,
                    serviceTitle = service.Title,
                    serviceCode = service.ServiceCode,
                    calculationDetails = new
                    {
                        calculationDetails.TotalAmount,
                        calculationDetails.TechnicalAmount,
                        calculationDetails.ProfessionalAmount,
                        calculationDetails.TechnicalPart,
                        calculationDetails.ProfessionalPart,
                        calculationDetails.TechnicalFactor,
                        calculationDetails.ProfessionalFactor,
                        calculationDetails.CalculationDate,
                        calculationDetails.HasDepartmentOverride,
                        calculationDetails.DepartmentId
                    }
                };

                _logger.Information("🏥 MEDICAL: جزئیات محاسبه خدمت دریافت شد. ServiceId: {ServiceId}, TotalAmount: {TotalAmount}, User: {UserName} (Id: {UserId})", 
                    serviceId, calculationDetails.TotalAmount, _currentUserService.UserName, _currentUserService.UserId);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت جزئیات محاسبه خدمت. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// بررسی وضعیت اجزای خدمت
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetServiceComponentsStatus(int serviceId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: بررسی وضعیت اجزای خدمت. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                var service = await _context.Services
                    .Include(s => s.ServiceComponents)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                if (service == null)
                {
                    return Json(new { success = false, message = "خدمت یافت نشد" }, JsonRequestBehavior.AllowGet);
                }

                var technicalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Technical && !sc.IsDeleted && sc.IsActive);
                
                var professionalComponent = service.ServiceComponents
                    .FirstOrDefault(sc => sc.ComponentType == ServiceComponentType.Professional && !sc.IsDeleted && sc.IsActive);

                var status = new
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

                _logger.Information("🏥 MEDICAL: وضعیت اجزای خدمت بررسی شد. ServiceId: {ServiceId}, Complete: {IsComplete}, User: {UserName} (Id: {UserId})", 
                    serviceId, status.isComplete, _currentUserService.UserName, _currentUserService.UserId);

                return Json(status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی وضعیت اجزای خدمت. ServiceId: {ServiceId}, User: {UserName} (Id: {UserId})", 
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}
