using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Controllers.Base;
using ClinicApp.Interfaces;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;
using System.Collections.Generic;

namespace ClinicApp.Controllers.Api
{
    /// <summary>
    /// API Controller مدیریت بیماران در پذیرش
    /// 
    /// Responsibilities:
    /// 1. جستجوی بیماران
    /// 2. ایجاد بیمار جدید
    /// 3. دریافت جزئیات بیمار
    /// 4. استعلام هویت بیمار
    /// 
    /// Architecture:
    /// ✅ Single Responsibility: فقط Patient
    /// ✅ No Cache: طبق سیاست
    /// ✅ Conditional Authorization
    /// </summary>
    public class PatientController : ReceptionBaseController
    {
        #region Fields

        private readonly IPatientService _patientService;

        #endregion

        #region Constructor

        public PatientController(
            IPatientService patientService,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
        }

        #endregion

        #region Patient Lookup Actions

        /// <summary>
        /// جستجوی بیمار بر اساس کد ملی
        /// </summary>
        /// <param name="nationalCode">کد ملی بیمار</param>
        /// <returns>اطلاعات بیمار</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LookupByNationalCode(string nationalCode)
        {
            using (StartPerformanceMonitoring("LookupByNationalCode"))
            {
                try
                {
                    _logger.Information(
                        "🔍 جستجوی بیمار بر اساس کد ملی. کد ملی: {NationalCode}, کاربر: {UserName}",
                        nationalCode, _currentUserService.UserName);

                    // Security Headers
                    AddSecurityHeaders();

                    // Validation
                    if (string.IsNullOrWhiteSpace(nationalCode))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "کد ملی الزامی است"
                        });
                    }

                    // Call Service
                    var result = await _patientService.GetPatientByNationalCodeAsync(nationalCode);

                    if (result == null)
                    {
                        _logger.Warning("⚠️ بیمار با کد ملی {NationalCode} یافت نشد", nationalCode);
                        return Json(new
                        {
                            success = false,
                            message = "بیمار یافت نشد"
                        });
                    }

                    _logger.Information("✅ بیمار با کد ملی {NationalCode} یافت شد", nationalCode);
                    
                    return SuccessResponse(result, "بیمار با موفقیت یافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "LookupByNationalCode", new { nationalCode });
                }
            }
        }

        /// <summary>
        /// جستجوی بیماران
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>لیست بیماران</returns>
        [HttpGet]
        public async Task<JsonResult> Search(string searchTerm)
        {
            using (StartPerformanceMonitoring("SearchPatients"))
            {
                try
                {
                    _logger.Information("🔍 جستجوی بیماران. عبارت: {SearchTerm}, کاربر: {UserName}",
                        searchTerm, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "عبارت جستجو الزامی است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientService.SearchPatientsAsync(searchTerm, 1, 10);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "خطا در جستجو"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new
                    {
                        success = true,
                        data = result,
                        message = "جستجو با موفقیت انجام شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "SearchPatients", new { searchTerm });
                }
            }
        }

        /// <summary>
        /// جستجوی بیماران با نام
        /// </summary>
        /// <param name="searchTerm">نام بیمار</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد در صفحه</param>
        /// <returns>لیست صفحه‌بندی شده بیماران</returns>
        [HttpGet]
        public async Task<JsonResult> SearchByName(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            using (StartPerformanceMonitoring("SearchPatientsByName"))
            {
                try
                {
                    _logger.Information(
                        "🔍 جستجوی بیماران با نام. نام: {SearchTerm}, صفحه: {PageNumber}, کاربر: {UserName}",
                        searchTerm, pageNumber, _currentUserService.UserName);

                    AddSecurityHeaders();

                    var result = await _patientService.SearchPatientsAsync(searchTerm, 1, 10);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "خطا در جستجو"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new
                    {
                        success = true,
                        data = result,
                        message = "جستجو با موفقیت انجام شد"
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "SearchPatientsByName", 
                        new { searchTerm, pageNumber, pageSize });
                }
            }
        }

        #endregion

        #region Patient Details Actions

        /// <summary>
        /// دریافت جزئیات بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>جزئیات بیمار</returns>
        [HttpGet]
        public async Task<JsonResult> GetDetails(int patientId)
        {
            using (StartPerformanceMonitoring("GetPatientDetails"))
            {
                try
                {
                    _logger.Information("📋 دریافت جزئیات بیمار. شناسه: {PatientId}, کاربر: {UserName}",
                        patientId, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (patientId <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "شناسه بیمار نامعتبر است"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    var result = await _patientService.GetPatientDetailsAsync(patientId);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "بیمار یافت نشد"
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return SuccessResponse(result, "جزئیات بیمار با موفقیت دریافت شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "GetPatientDetails", new { patientId });
                }
            }
        }

        #endregion

        #region Patient Creation Actions

        /// <summary>
        /// ایجاد بیمار جدید
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create(ReceptionFormPatientViewModel model)
        {
            using (StartPerformanceMonitoring("CreatePatient"))
            {
                try
                {
                    _logger.Information("➕ ایجاد بیمار جدید. کد ملی: {NationalCode}, کاربر: {UserName}",
                        model?.NationalCode, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (!ModelState.IsValid)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "اطلاعات ورودی نامعتبر است",
                            errors = ModelState.Values
                        });
                    }

                    // Convert ReceptionFormPatientViewModel to PatientCreateEditViewModel
                    var patientModel = new PatientCreateEditViewModel
                    {
                        FirstName = model.PatientInfo?.FirstName ?? "",
                        LastName = model.PatientInfo?.LastName ?? "",
                        NationalCode = model.PatientInfo?.NationalCode ?? "",
                        PhoneNumber = model.PatientInfo?.PhoneNumber ?? "",
                        BirthDate = model.PatientInfo?.BirthDate,
                        Gender = model.PatientInfo?.Gender ?? ClinicApp.Models.Enums.Gender.Male,
                        Address = model.PatientInfo?.Address ?? ""
                    };
                    
                    var result = await _patientService.CreatePatientAsync(patientModel);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "خطا در ایجاد بیمار"
                        });
                    }

                    _logger.Information("✅ بیمار جدید با موفقیت ایجاد شد");
                    
                    return SuccessResponse(result, "بیمار با موفقیت ایجاد شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "CreatePatient", model);
                }
            }
        }

        /// <summary>
        /// ایجاد بیمار به صورت Inline
        /// </summary>
        /// <param name="model">اطلاعات بیمار</param>
        /// <returns>نتیجه عملیات</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateInline(PatientCreateEditViewModel model)
        {
            using (StartPerformanceMonitoring("CreatePatientInline"))
            {
                try
                {
                    _logger.Information("➕ ایجاد بیمار Inline. کد ملی: {NationalCode}, کاربر: {UserName}",
                        model?.NationalCode, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (!ModelState.IsValid)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "اطلاعات ورودی نامعتبر است"
                        });
                    }

                    var result = await _patientService.CreatePatientAsync(model);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "خطا در ایجاد بیمار"
                        });
                    }

                    return SuccessResponse(result, "بیمار با موفقیت ایجاد شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "CreatePatientInline", model);
                }
            }
        }

        #endregion

        #region Patient Identity Inquiry

        /// <summary>
        /// استعلام هویت بیمار از سیستم خارجی
        /// </summary>
        /// <param name="nationalCode">کد ملی</param>
        /// <param name="birthDate">تاریخ تولد</param>
        /// <returns>نتیجه استعلام</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> InquiryIdentity(string nationalCode, DateTime birthDate)
        {
            using (StartPerformanceMonitoring("InquiryPatientIdentity"))
            {
                try
                {
                    _logger.Information(
                        "🔍 استعلام هویت بیمار. کد ملی: {NationalCode}, تاریخ تولد: {BirthDate}, کاربر: {UserName}",
                        nationalCode, birthDate, _currentUserService.UserName);

                    AddSecurityHeaders();

                    if (string.IsNullOrWhiteSpace(nationalCode))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "کد ملی الزامی است"
                        });
                    }

                    var result = await _patientService.GetPatientByNationalCodeAsync(nationalCode);

                    if (result == null)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "بیمار یافت نشد"
                        });
                    }

                    return SuccessResponse(result, "استعلام با موفقیت انجام شد");
                }
                catch (Exception ex)
                {
                    return HandleReceptionError(ex, "InquiryPatientIdentity", 
                        new { nationalCode, birthDate });
                }
            }
        }

        #endregion
    }
}

