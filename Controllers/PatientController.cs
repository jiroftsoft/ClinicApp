using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// کنترلر مدیریت بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیماران
    /// 2. مدیریت کامل اطلاعات بیمه بیماران
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه
    /// 7. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
    /// 9. امنیت بالا با رعایت استانداردهای سیستم‌های پزشکی
    /// 10. عملکرد بهینه برای محیط‌های Production
    /// </summary>
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [RouteArea("Admin")]
    [RoutePrefix("Patient")]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAppSettings _appSettings;

        public PatientController(
            IPatientService patientService,
            ILogger logger,
            ICurrentUserService currentUserService,
            IAppSettings appSettings)
        {
            _patientService = patientService;
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
        [Route("")]
        [Route("Index")]
        public ActionResult Index()
        {
            _log.Information("بازدید از صفحه اصلی بیماران. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            return View();
        }

        /// <summary>
        /// بارگیری لیست بیماران با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadPatients")]
        public async Task<PartialViewResult> LoadPatients(string searchTerm = "", int page = 1)
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
        /// نمایش جزئیات بیمار
        /// </summary>
        [HttpGet]
        [Route("Details/{id}")]
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

                _log.Information(
                    "دریافت جزئیات بیمار شناسه {PatientId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
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
        /// نمایش فرم ایجاد بیمار جدید
        /// </summary>
        [HttpGet]
        [Route("Create")]
        public async Task<ActionResult> Create()
        {
            _log.Information(
                "درخواست ایجاد بیمار جدید. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var viewModel = new PatientCreateEditViewModel();
                //await PopulateInsuranceList(viewModel);

                _log.Information(
                    "صفحه ایجاد بیمار با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری صفحه ایجاد بیمار. User: {UserName} (Id: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return View(new PatientCreateEditViewModel());
            }
        }

        /// <summary>
        /// پردازش ایجاد بیمار جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<ActionResult> Create(PatientCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد بیمار جدید با کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی بیمه حذف شد - از PatientInsurance استفاده کنید

            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "اعتبارسنجی ایجاد بیمار با کد ملی {NationalCode} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, ModelState.Values.SelectMany(v => v.Errors),
                    _currentUserService.UserName, _currentUserService.UserId);

                // PopulateInsuranceList حذف شد
                return View(model);
            }

            try
            {
                var result = await _patientService.CreatePatientAsync(model);
                if (result.Success)
                {
                    _log.Information(
                        "بیمار جدید با کد ملی {NationalCode} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
                        model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Warning(
                    "ایجاد بیمار با کد ملی {NationalCode} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", result.Message);
                // PopulateInsuranceList حذف شد
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ایجاد بیمار با کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                    model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                // PopulateInsuranceList حذف شد
                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش بیمار
        /// </summary>
        [HttpGet]
        [Route("Edit/{id}")]
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

                var model = result.Data;
                // PopulateInsuranceList حذف شد

                _log.Information(
                    "صفحه ویرایش بیمار شناسه {PatientId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return View(model);
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
        /// پردازش ویرایش بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<ActionResult> Edit(PatientCreateEditViewModel model)
        {
            if (model.PatientId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش بیمار با شناسه نامعتبر. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست ویرایش بیمار شناسه {PatientId} با کد ملی {NationalCode}. User: {UserName} (Id: {UserId})",
                model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی دستی برای بیمه
            //if (model.InsuranceId <= 0)
            //{
            //    // دریافت لیست بیمه‌ها از سرویس
            //    //var insurancesResult = await _insuranceService.GetActiveInsurancesAsync();
            //    InsuranceViewModel freeInsurance = null;

            //    //// بررسی موفقیت عملیات و دسترسی به داده‌ها
            //    //if (insurancesResult.Success && insurancesResult.Data != null)
            //    //{
            //    //    // جستجوی بیمه آزاد در لیست بیمه‌ها
            //    //    freeInsurance = insurancesResult.Data.FirstOrDefault(i => i.Name == SystemConstants.FreeInsuranceName);
            //    //}
            //    //else
            //    //{
            //    //    _log.Warning(
            //    //        "دریافت لیست بیمه‌ها ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
            //    //        insurancesResult.Message, _currentUserService.UserName, _currentUserService.UserId);
            //    //}

            //    if (freeInsurance != null)
            //    {
            //        model.InsuranceId = freeInsurance.InsuranceId;
            //        _log.Information(
            //            "بیمه آزاد به عنوان پیش‌فرض برای بیمار شناسه {PatientId} انتخاب شد.",
            //            model.PatientId);
            //    }
            //    else
            //    {
            //        _log.Error(
            //            "بیمه آزاد در سیستم یافت نشد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
            //            model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

            //        ModelState.AddModelError("InsuranceId", "سیستم به درستی پیکربندی نشده است. لطفاً با پشتیبانی تماس بگیرید.");
            //    }
            //}

            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "اعتبارسنجی ویرایش بیمار شناسه {PatientId} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                    model.PatientId, ModelState.Values.SelectMany(v => v.Errors),
                    _currentUserService.UserName, _currentUserService.UserId);

                // PopulateInsuranceList حذف شد
                return View(model);
            }

            try
            {
                var result = await _patientService.UpdatePatientAsync(model);
                if (result.Success)
                {
                    _log.Information(
                        "بیمار شناسه {PatientId} با کد ملی {NationalCode} با موفقیت ویرایش شد. User: {UserName} (Id: {UserId})",
                        model.PatientId, model.NationalCode, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                _log.Warning(
                    "ویرایش بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                    model.PatientId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", result.Message);
                // PopulateInsuranceList حذف شد
                return View(model);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ویرایش بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                    model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                // PopulateInsuranceList حذف شد
                return View(model);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// پردازش حذف بیمار
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Delete")]
        public async Task<JsonResult> Delete(int id)
        {
            _log.Information(
                "درخواست حذف بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                id, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _patientService.DeletePatientAsync(id);
                if (result.Success)
                {
                    _log.Information(
                        "بیمار شناسه {PatientId} با موفقیت حذف شد. User: {UserName} (Id: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _log.Warning(
                        "حذف بیمار شناسه {PatientId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف بیمار شناسه {PatientId}. User: {UserName} (Id: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// پر کردن لیست بیمه‌ها برای نمایش در ویو
        /// </summary>
        // PopulateInsuranceList حذف شد - از PatientInsurance استفاده کنید

        #endregion
    }
}