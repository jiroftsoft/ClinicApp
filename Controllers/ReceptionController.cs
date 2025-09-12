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
using PatientInquiryViewModel = ClinicApp.ViewModels.Reception.PatientInquiryViewModel;

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
    public class ReceptionController : BaseController
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionController(
            IReceptionService receptionService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
                StatusList = GetReceptionStatusList(),
                TypeList = GetReceptionTypeList(),
                PaymentMethodList = GetPaymentMethodList(),
                InsuranceList = new List<SelectListItem>() // TODO: اضافه کردن لیست بیمه‌ها
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
        public ActionResult Create()
        {
            _logger.Information(
                "ورود به صفحه ایجاد پذیرش جدید. کاربر: {UserName}",
                _currentUserService.UserName);

            try
            {
                var model = new ReceptionCreateViewModel
                {
                    ReceptionDate = DateTime.Now,
                    ReceptionDateShamsi = DateTime.Now.ToPersianDateTime(),
                    IsEmergency = false,
                    IsOnlineReception = false
                };

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
        public async Task<JsonResult> LookupPatientByNationalCode(string nationalCode)
        {
            _logger.Information(
                "جستجوی بیمار بر اساس کد ملی. کد ملی: {NationalCode}, کاربر: {UserName}",
                nationalCode, _currentUserService.UserName);

            try
            {
                if (!ValidateRequiredField(nationalCode, "کد ملی"))
                {
                    return HandleModelStateErrors();
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
        /// <param name="name">نام بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">اندازه صفحه</param>
        /// <returns>لیست بیماران</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SearchPatientsByName(string name, int pageNumber = 1, int pageSize = 10)
        {
            _logger.Information(
                "جستجوی بیماران بر اساس نام. نام: {Name}, صفحه: {PageNumber}, اندازه: {PageSize}, کاربر: {UserName}",
                name, pageNumber, pageSize, _currentUserService.UserName);

            try
            {
                if (!ValidateRequiredField(name, "نام بیمار"))
                {
                    return HandleModelStateErrors();
                }

                var result = await _receptionService.SearchPatientsByNameAsync(name, pageNumber, pageSize);
                if (!result.Success)
                {
                    return HandleServiceError(result);
                }

                return SuccessResponse(result.Data, "جستجوی بیماران با موفقیت انجام شد.");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetServicesByCategory(int categoryId)
        {
            _logger.Information(
                "درخواست لیست خدمات بر اساس دسته‌بندی. شناسه: {CategoryId}, کاربر: {UserName}",
                categoryId, _currentUserService.UserName);

            try
            {
                if (categoryId <= 0)
                {
                    return Json(new { success = false, message = "شناسه دسته‌بندی نامعتبر است." });
                }

                var result = await _receptionService.GetServicesByCategoryAsync(categoryId);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست خدمات. دسته‌بندی: {CategoryId}, کاربر: {UserName}",
                    categoryId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست خدمات." });
            }
        }

        /// <summary>
        /// دریافت لیست پزشکان (AJAX)
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پزشکان. کاربر: {UserName}",
                    _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست پزشکان." });
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
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors });
                }

                var result = await _receptionService.CreateReceptionAsync(model);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data, redirectUrl = Url.Action("Details", new { id = result.Data.ReceptionId }) });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ایجاد پذیرش. بیمار: {PatientId}, کاربر: {UserName}",
                    model?.PatientId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در ایجاد پذیرش. لطفاً مجدداً تلاش کنید." });
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
                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است.", errors = errors });
                }

                var result = await _receptionService.UpdateReceptionAsync(model);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data, message = "پذیرش با موفقیت ویرایش شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در ویرایش پذیرش. شناسه: {ReceptionId}, کاربر: {UserName}",
                    model?.ReceptionId, _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در ویرایش پذیرش. لطفاً مجدداً تلاش کنید." });
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
                    return Json(new { success = false, message = result.Message });
                }

                return Json(new { success = true, data = result.Data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "خطا در دریافت لیست پذیرش‌ها. کاربر: {UserName}",
                    _currentUserService.UserName);

                return Json(new { success = false, message = "خطا در دریافت لیست پذیرش‌ها." });
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

        #endregion

        #region Helper Methods

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

        #endregion
    }
}
