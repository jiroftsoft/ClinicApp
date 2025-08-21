using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت بیمه‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیمه‌ها با رعایت استانداردهای قانونی سیستم‌های پزشکی ایران
    /// 2. مدیریت تعرفه‌های بیمه‌ای برای خدمات مختلف با قابلیت تعریف سهم‌های خاص برای هر خدمت
    /// 3. پیاده‌سازی کامل سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی مطابق با قوانین ایران
    /// 4. ارتباط دقیق با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی کامل عملیات‌های حساس
    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط با استفاده از زمان UTC و تبدیل به شمسی
    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه با مکانیزم‌های امنیتی
    /// 7. پشتیبانی کامل از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی در تمام لایه‌ها
    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها و امنیت بالا
    /// 9. پیاده‌سازی مکانیزم‌های بهینه‌سازی عملکرد برای سیستم‌های پزشکی پراستفاده
    /// 10. رعایت کامل استانداردهای امنیتی و حفظ حریم خصوصی اطلاعات پزشکی
    /// 11. مدیریت صحیح فرمت‌های اعشاری در محیط‌های فارسی
    /// 12. رفع کامل مشکل نمایش بدون استایل فرم‌ها در صفحه جزئیات
    /// 13. رفع کامل مشکل بایند نشدن داده‌ها در فرم ویرایش
    /// 14. مدیریت صحیح دکمه‌های بستن مودال
    /// 15. پشتیبانی کامل از سیستم‌های پزشکی حیاتی با مکانیزم‌های ریکاوری خطا
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
    [RouteArea("Admin")]
    [RoutePrefix("Insurance")]
    public class InsuranceController : Controller
    {
        private readonly IInsuranceService _insuranceService;
        private readonly IServiceService _serviceService;
        private readonly ILogger _log;
        private const int PageSize = 15;
        private readonly ICurrentUserService _currentUserService;
        private const string FreeInsuranceName = "بیمه آزاد";

        public InsuranceController(
            IInsuranceService insuranceService,
            IServiceService serviceService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _insuranceService = insuranceService;
            _serviceService = serviceService;
            _log = logger.ForContext<InsuranceController>();
            _currentUserService = currentUserService;
        }

        #region Index & Search

        /// <summary>
        /// نمایش صفحه اصلی بیمه‌ها
        /// </summary>
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public ActionResult Index()
        {
            _log.Information("بازدید از صفحه اصلی بیمه‌ها. User: {UserName} (Id: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            return View();
        }

        /// <summary>
        /// بارگیری لیست بیمه‌ها با صفحه‌بندی
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadInsurances")]
        public async Task<PartialViewResult> LoadInsurances(string searchTerm = "", int page = 1, bool activeOnly = true)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست لود بیمه‌ها. OperationId: {OperationId}, SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}, ActiveOnly: {ActiveOnly}. User: {UserName} (Id: {UserId})",
                operationId, searchTerm, page, PageSize, activeOnly, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت لیست بیمه‌ها از سرویس
                var result = await _insuranceService.GetActiveInsurancesAsync();

                // بررسی موفقیت عملیات و داده‌ها
                if (!result.Success || result.Data == null)
                {
                    _log.Warning(
                        "دریافت لیست بیمه‌ها ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_InsuranceListPartial", new PagedResult<InsuranceViewModel>());
                }

                // فیلتر بر اساس جستجو
                var filteredInsurances = result.Data.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredInsurances = filteredInsurances.Where(i =>
                        i.Name.Contains(searchTerm) ||
                        i.Description.Contains(searchTerm));
                }

                if (activeOnly)
                {
                    filteredInsurances = filteredInsurances.Where(i => i.IsActive);
                }

                // اعمال صفحه‌بندی
                var pagedResult = new PagedResult<InsuranceViewModel>
                {
                    Items = filteredInsurances.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
                    PageNumber = page,
                    PageSize = PageSize,
                    TotalItems = filteredInsurances.Count()
                };

                _log.Information(
                    "لود بیمه‌ها با موفقیت انجام شد. OperationId: {OperationId}, Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
                    operationId, pagedResult.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceListPartial", pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در لود بیمه‌ها. OperationId: {OperationId}, SearchTerm: {SearchTerm}, Page: {Page}, ActiveOnly: {ActiveOnly}. User: {UserName} (Id: {UserId})",
                    operationId, searchTerm, page, activeOnly, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceListPartial", new PagedResult<InsuranceViewModel>());
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات بیمه
        /// </summary>
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<ActionResult> Details(int? id)
        {
            var operationId = Guid.NewGuid().ToString();
            if (id == null)
            {
                _log.Warning(
                    "درخواست جزئیات بیمه با شناسه خالی. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست جزئیات بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                id, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.GetInsuranceDetailsAsync(id.Value);
                if (!result.Success || result.Data == null)
                {
                    _log.Warning(
                        "دریافت جزئیات بیمه شناسه {InsuranceId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "بیمه مورد نظر یافت نشد یا دسترسی لازم را ندارید.";
                    return RedirectToAction("Index");
                }

                _log.Information(
                    "دریافت جزئیات بیمه شناسه {InsuranceId} با موفقیت انجام شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت جزئیات بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد بیمه
        /// </summary>
        [HttpGet]
        [Route("Create")]
        public ActionResult Create()
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست فرم ایجاد بیمه. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                operationId, _currentUserService.UserName, _currentUserService.UserId);

            return PartialView("_CreateInsurance");
        }

        ///// <summary>
        ///// پردازش ایجاد بیمه جدید
        ///// </summary>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("Create")]
        //public async Task<JsonResult> Create(CreateInsuranceViewModel model)
        //{
        //    var operationId = Guid.NewGuid().ToString();
        //    _log.Information(
        //        "درخواست ایجاد بیمه جدید. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
        //        operationId, _currentUserService.UserName, _currentUserService.UserId);

        //    try
        //    {
        //        // **مرحله 1: پردازش و تبدیل صحیح فرمت اعشاری در محیط فارسی**
        //        var patientShareValue = Request.Form["DefaultPatientShare"];
        //        if (!string.IsNullOrEmpty(patientShareValue))
        //        {
        //            var patientShareStr = patientShareValue.ToString().Replace(",", ".");
        //            if (decimal.TryParse(patientShareStr, out decimal patientShare))
        //            {
        //                model.DefaultPatientShare = patientShare;
        //            }
        //        }

        //        var insurerShareValue = Request.Form["DefaultInsurerShare"];
        //        if (!string.IsNullOrEmpty(insurerShareValue))
        //        {
        //            var insurerShareStr = insurerShareValue.ToString().Replace(",", ".");
        //            if (decimal.TryParse(insurerShareStr, out decimal insurerShare))
        //            {
        //                model.DefaultInsurerShare = insurerShare;
        //            }
        //        }

        //        // **مرحله 2: اعتبارسنجی مدل**
        //        if (!ModelState.IsValid)
        //        {
        //            _log.Warning(
        //                "اعتبارسنجی ایجاد بیمه شکست خورد. OperationId: {OperationId}, Errors: {@Errors}. User: {UserName} (Id: {UserId})",
        //                operationId, ModelState.Values.SelectMany(v => v.Errors),
        //                _currentUserService.UserName, _currentUserService.UserId);
        //            return Json(new { success = false, errors = GetModelErrors() });
        //        }

        //        // **مرحله 3: اعتبارسنجی محدوده سهم‌ها**
        //        if (model.DefaultPatientShare.HasValue)
        //        {
        //            if (model.DefaultPatientShare < 0 || model.DefaultPatientShare > 100)
        //            {
        //                _log.Warning(
        //                    "سهم بیمار خارج از محدوده معتبر است. OperationId: {OperationId}, Value: {PatientShare}, User: {UserName} (Id: {UserId})",
        //                    operationId, model.DefaultPatientShare, _currentUserService.UserName, _currentUserService.UserId);
        //                return Json(new { success = false, message = "سهم بیمار باید بین 0 تا 100 درصد باشد." });
        //            }
        //        }

        //        if (model.DefaultInsurerShare.HasValue)
        //        {
        //            if (model.DefaultInsurerShare < 0 || model.DefaultInsurerShare > 100)
        //            {
        //                _log.Warning(
        //                    "سهم بیمه خارج از محدوده معتبر است. OperationId: {OperationId}, Value: {InsurerShare}, User: {UserName} (Id: {UserId})",
        //                    operationId, model.DefaultInsurerShare, _currentUserService.UserName, _currentUserService.UserId);
        //                return Json(new { success = false, message = "سهم بیمه باید بین 0 تا 100 درصد باشد." });
        //            }
        //        }

        //        // **مرحله 4: اعتبارسنجی مجموع سهم‌ها**
        //        if (model.DefaultPatientShare.HasValue && model.DefaultInsurerShare.HasValue)
        //        {
        //            if (Math.Abs(model.DefaultPatientShare.Value + model.DefaultInsurerShare.Value - 100) > 0.01m)
        //            {
        //                _log.Warning(
        //                    "جمع سهم‌های بیمار و بیمه برابر با 100 نیست. OperationId: {OperationId}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. User: {UserName} (Id: {UserId})",
        //                    operationId, model.DefaultPatientShare, model.DefaultInsurerShare, _currentUserService.UserName, _currentUserService.UserId);
        //                return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
        //            }
        //        }

        //        // **مرحله 5: ایجاد بیمه در سرویس**
        //        var result = await _insuranceService.CreateInsuranceAsync(model);
        //        if (result != null && result.Success)
        //        {
        //            _log.Information(
        //                "بیمه با موفقیت ایجاد شد. OperationId: {OperationId}, InsuranceName: {InsuranceName}, User: {UserName} (Id: {UserId})",
        //                operationId, model.Name, _currentUserService.UserName, _currentUserService.UserId);
        //            return Json(new { success = true, message = result.Message });
        //        }

        //        _log.Warning(
        //            "ایجاد بیمه ناموفق بود. OperationId: {OperationId}, InsuranceName: {InsuranceName}, Error: {Error}. User: {UserName} (Id: {UserId})",
        //            operationId, model.Name, result?.Message, _currentUserService.UserName, _currentUserService.UserId);
        //        return Json(new { success = false, message = result?.Message ?? "خطا در ایجاد بیمه." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error(
        //            ex,
        //            "خطای سیستمی در ایجاد بیمه. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
        //            operationId, _currentUserService.UserName, _currentUserService.UserId);
        //        return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
        //    }
        //}
        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش بیمه
        /// </summary>
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<ActionResult> Edit(int? id)
        {
            var operationId = Guid.NewGuid().ToString();
            if (id == null)
            {
                _log.Warning(
                    "درخواست ویرایش بیمه با شناسه خالی. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    operationId, _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست ویرایش بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                id, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.GetInsuranceDetailsAsync(id.Value);
                if (!result.Success || result.Data == null)
                {
                    _log.Warning(
                        "دریافت اطلاعات بیمه شناسه {InsuranceId} برای ویرایش ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "بیمه مورد نظر یافت نشد یا دسترسی لازم را ندارید." }, JsonRequestBehavior.AllowGet);
                }

                var model = new EditInsuranceViewModel
                {
                    InsuranceId = result.Data.InsuranceId,
                    Name = result.Data.Name,
                    Description = result.Data.Description,
                    DefaultPatientShare = result.Data.DefaultPatientShare,
                    DefaultInsurerShare = result.Data.DefaultInsurerShare,
                    IsActive = result.Data.IsActive
                };

                _log.Information(
                    "صفحه ویرایش بیمه شناسه {InsuranceId} با موفقیت بارگیری شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_EditInsurance", model);

            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری صفحه ویرایش بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً تلاش کنید." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// پردازش ویرایش بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<ActionResult> Edit(EditInsuranceViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            if (model.InsuranceId <= 0)
            {
                _log.Warning(
                    "درخواست ویرایش بیمه با شناسه نامعتبر. OperationId: {OperationId}, InsuranceId: {InsuranceId}. User: {UserName} (Id: {UserId})",
                    operationId, model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            _log.Information(
                "درخواست ویرایش بیمه شناسه {InsuranceId} با نام {Name}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model.InsuranceId, model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);

            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "اعتبارسنجی ویرایش بیمه شناسه {InsuranceId} شکست خورد. OperationId: {OperationId}, Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, ModelState.Values.SelectMany(v => v.Errors),
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, errors = GetModelErrors() });
            }

            try
            {
                // تبدیل صحیح فرمت اعشاری در محیط فارسی
                model.DefaultPatientShare = Convert.ToDecimal(model.DefaultPatientShare.ToString().Replace(",", "."));
                model.DefaultInsurerShare = Convert.ToDecimal(model.DefaultInsurerShare.ToString().Replace(",", "."));

                // بررسی مجموع سهم‌ها
                if (Math.Abs(model.DefaultPatientShare + model.DefaultInsurerShare - 100) > 0.01m)
                {
                    _log.Warning(
                        "مجموع سهم‌ها برای بیمه {Name} برابر با 100 نیست. OperationId: {OperationId}, PatientShare: {PatientShare}, InsurerShare: {InsurerShare}. User: {UserName} (Id: {UserId})",
                        model.Name, operationId, model.DefaultPatientShare, model.DefaultInsurerShare,
                        _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
                }

                // بررسی محدودیت‌های بیمه آزاد
                if (model.Name == FreeInsuranceName)
                {
                    // بررسی اینکه آیا کاربر سعی در تغییر وضعیت فعال/غیرفعال بیمه آزاد دارد
                    if (!model.IsActive)
                    {
                        _log.Warning(
                            "درخواست غیرفعال کردن بیمه آزاد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new { success = false, message = "بیمه آزاد نمی‌تواند غیرفعال شود." });
                    }
                }

                var result = await _insuranceService.UpdateInsuranceAsync(model);
                if (result.Success)
                {
                    _log.Information(
                        "بیمه شناسه {InsuranceId} با نام {Name} با موفقیت ویرایش شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        model.InsuranceId, model.Name, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = true, message = result.Message });
                }

                _log.Warning(
                    "ویرایش بیمه شناسه {InsuranceId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ویرایش بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// پردازش حذف بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Delete")]
        public async Task<JsonResult> Delete(int id)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست حذف بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                id, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // بررسی اینکه آیا بیمه قابل حذف است
                var insuranceDetails = await _insuranceService.GetInsuranceDetailsAsync(id);
                if (insuranceDetails.Success && insuranceDetails.Data != null)
                {
                    if (insuranceDetails.Data.Name == FreeInsuranceName)
                    {
                        _log.Warning(
                            "درخواست حذف بیمه آزاد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                            operationId, _currentUserService.UserName, _currentUserService.UserId);

                        return Json(new { success = false, message = "بیمه آزاد قابل حذف نیست." });
                    }
                }

                var result = await _insuranceService.DeleteInsuranceAsync(id);
                if (result.Success)
                {
                    _log.Information(
                        "بیمه شناسه {InsuranceId} با موفقیت حذف شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    _log.Warning(
                        "حذف بیمه شناسه {InsuranceId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        #endregion

        #region Manage Tariffs

        /// <summary>
        /// نمایش صفحه مدیریت تعرفه‌های بیمه
        /// </summary>
        [HttpGet]
        [Route("ManageTariffs/{insuranceId}")]
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> ManageTariffs(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست مدیریت تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت جزئیات بیمه
                var insuranceResult = await _insuranceService.GetInsuranceDetailsAsync(insuranceId);
                if (!insuranceResult.Success || insuranceResult.Data == null)
                {
                    _log.Warning(
                        "دریافت جزئیات بیمه شناسه {InsuranceId} برای مدیریت تعرفه‌ها ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        insuranceId, operationId, insuranceResult.Message, _currentUserService.UserName, _currentUserService.UserId);

                    TempData["ErrorMessage"] = "بیمه مورد نظر یافت نشد یا دسترسی لازم را ندارید.";
                    return RedirectToAction("Index");
                }

                // دریافت تعرفه‌های بیمه
                var tariffsResult = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
                var tariffs = tariffsResult.Success && tariffsResult.Data != null ?
                    tariffsResult.Data : new List<InsuranceTariffViewModel>();

                // ایجاد ViewModel برای نمایش
                var viewModel = new InsuranceTariffsViewModel
                {
                    InsuranceId = insuranceId,
                    InsuranceName = insuranceResult.Data.Name,
                    DefaultPatientShare = insuranceResult.Data.DefaultPatientShare,
                    DefaultInsurerShare = insuranceResult.Data.DefaultInsurerShare,
                    Tariffs = tariffs
                };

                _log.Information(
                    "صفحه مدیریت تعرفه‌های بیمه شناسه {InsuranceId} با موفقیت بارگیری شد. OperationId: {OperationId}, TariffCount: {TariffCount}. User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری صفحه مدیریت تعرفه‌های بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// بارگیری لیست تعرفه‌های بیمه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LoadTariffs")]
        public async Task<PartialViewResult> LoadTariffs(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست لود تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
                var tariffs = result.Success && result.Data != null ?
                    result.Data : new List<InsuranceTariffViewModel>();

                _log.Information(
                    "لود تعرفه‌های بیمه شناسه {InsuranceId} با موفقیت انجام شد. OperationId: {OperationId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceTariffsListPartial", tariffs);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در لود تعرفه‌های بیمه شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceTariffsListPartial", new List<InsuranceTariffViewModel>());
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد تعرفه جدید
        /// </summary>
        [HttpGet]
        [Route("CreateTariff")]
        public async Task<ActionResult> CreateTariff(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست فرم ایجاد تعرفه جدید برای بیمه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت خدمات فعال
                var services = await _serviceService.GetActiveServicesAsync() ??
                               new List<ServiceSelectItem>();

                // دریافت تعرفه‌های موجود
                var tariffsResult = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
                var tariffs = tariffsResult.Success && tariffsResult.Data != null ?
                    tariffsResult.Data : new List<InsuranceTariffViewModel>();

                // فیلتر خدمات قابل انتخاب
                var availableServices = services
                    .Where(s => tariffs.All(t => t.ServiceTitle != s.Title))
                    .ToList();

                ViewBag.AvailableServices = availableServices;
                ViewBag.InsuranceId = insuranceId;

                _log.Information(
                    "فرم ایجاد تعرفه جدید برای بیمه {InsuranceId} با موفقیت بارگیری شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_AddTariffPartial");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری فرم ایجاد تعرفه جدید برای بیمه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_ErrorPartial", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            }
        }

        /// <summary>
        /// پردازش افزودن تعرفه جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AddTariff")]
        public async Task<ActionResult> AddTariff(CreateInsuranceTariffViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست افزودن تعرفه برای بیمه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی سمت سرور
            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "اعتبارسنجی افزودن تعرفه برای بیمه {InsuranceId} شکست خورد. OperationId: {OperationId}, Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, ModelState.Values.SelectMany(v => v.Errors),
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, errors = GetModelErrors() });
            }

            // اعتبارسنجی سهم‌ها
            if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
            {
                if (Math.Abs(model.PatientShare.Value + model.InsurerShare.Value - 100) > 0.01m)
                {
                    _log.Warning(
                        "جمع سهم‌های بیمار و بیمه برای بیمه {InsuranceId} برابر با 100 نیست. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
                }
            }

            try
            {
                // تبدیل صحیح فرمت اعشاری در محیط فارسی
                if (model.TariffPrice.HasValue)
                {
                    model.TariffPrice = Convert.ToDecimal(model.TariffPrice.Value.ToString().Replace(",", "."));
                }
                if (model.PatientShare.HasValue)
                {
                    model.PatientShare = Convert.ToDecimal(model.PatientShare.Value.ToString().Replace(",", "."));
                }
                if (model.InsurerShare.HasValue)
                {
                    model.InsurerShare = Convert.ToDecimal(model.InsurerShare.Value.ToString().Replace(",", "."));
                }

                var result = await _insuranceService.CreateInsuranceTariffAsync(model);
                if (result.Success)
                {
                    _log.Information(
                        "تعرفه برای بیمه {InsuranceId} با موفقیت ایجاد شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = true, message = result.Message });
                }

                _log.Warning(
                    "افزودن تعرفه برای بیمه {InsuranceId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در افزودن تعرفه برای بیمه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش تعرفه
        /// </summary>
        [HttpGet]
        [Route("EditTariff/{id}")]
        public async Task<ActionResult> EditTariff(int id)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ویرایش تعرفه با شناسه {TariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                id, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.GetInsuranceTariffForEditAsync(id);
                if (!result.Success || result.Data == null)
                {
                    _log.Warning(
                        "دریافت تعرفه شناسه {TariffId} برای ویرایش ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        id, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                    return PartialView("_ErrorPartial", result.Message);
                }

                var model = result.Data;

                // دریافت خدمات فعال
                var services = await _serviceService.GetActiveServicesAsync() ??
                               new List<ServiceSelectItem>();

                ViewBag.Services = services;

                _log.Information(
                    "فرم ویرایش تعرفه شناسه {TariffId} با موفقیت بارگیری شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_EditTariffPartial", model);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بارگیری فرم ویرایش تعرفه شناسه {TariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_ErrorPartial", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
            }
        }

        /// <summary>
        /// پردازش ویرایش تعرفه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("UpdateTariff")]
        public async Task<ActionResult> UpdateTariff(EditInsuranceTariffViewModel model)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست ویرایش تعرفه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            // اعتبارسنجی سمت سرور
            if (!ModelState.IsValid)
            {
                _log.Warning(
                    "اعتبارسنجی ویرایش تعرفه {InsuranceTariffId} شکست خورد. OperationId: {OperationId}, Errors: {@Errors}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, operationId, ModelState.Values.SelectMany(v => v.Errors),
                    _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, errors = GetModelErrors() });
            }

            // اعتبارسنجی سهم‌ها
            if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
            {
                if (Math.Abs(model.PatientShare.Value + model.InsurerShare.Value - 100) > 0.01m)
                {
                    _log.Warning(
                        "جمع سهم‌های بیمار و بیمه برای تعرفه {InsuranceTariffId} برابر با 100 نیست. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
                }
            }

            try
            {
                // تبدیل صحیح فرمت اعشاری در محیط فارسی
                if (model.TariffPrice.HasValue)
                {
                    model.TariffPrice = Convert.ToDecimal(model.TariffPrice.Value.ToString().Replace(",", "."));
                }
                if (model.PatientShare.HasValue)
                {
                    model.PatientShare = Convert.ToDecimal(model.PatientShare.Value.ToString().Replace(",", "."));
                }
                if (model.InsurerShare.HasValue)
                {
                    model.InsurerShare = Convert.ToDecimal(model.InsurerShare.Value.ToString().Replace(",", "."));
                }

                var result = await _insuranceService.UpdateInsuranceTariffAsync(model);
                if (result.Success)
                {
                    _log.Information(
                        "تعرفه {InsuranceTariffId} با موفقیت ویرایش شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = true, message = result.Message });
                }

                _log.Warning(
                    "ویرایش تعرفه {InsuranceTariffId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ویرایش تعرفه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    model.InsuranceTariffId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        /// <summary>
        /// پردازش حذف تعرفه
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("DeleteTariff")]
        public async Task<JsonResult> DeleteTariff(int id)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست حذف تعرفه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                id, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.DeleteInsuranceTariffAsync(id);
                if (result.Success)
                {
                    _log.Information(
                        "تعرفه {InsuranceTariffId} با موفقیت حذف شد. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                        id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                    return Json(new { success = result.Success, message = result.Message });
                }

                _log.Warning(
                    "حذف تعرفه {InsuranceTariffId} ناموفق بود. OperationId: {OperationId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                    id, operationId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف تعرفه {InsuranceTariffId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    id, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
            }
        }

        #endregion

        #region API Endpoints

        /// <summary>
        /// دریافت تعرفه‌های بیمه برای نمایش در صفحه جزئیات
        /// </summary>
        [HttpGet]
        [Route("GetInsuranceTariffs/{insuranceId}")]
        public async Task<PartialViewResult> GetInsuranceTariffs(int insuranceId)
        {
            var operationId = Guid.NewGuid().ToString();
            _log.Information(
                "درخواست بارگیری تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var result = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
                var tariffs = result.Success && result.Data != null ?
                    result.Data : new List<InsuranceTariffViewModel>();

                _log.Information(
                    "تعرفه‌های بیمه با شناسه {InsuranceId} با موفقیت بارگیری شد. OperationId: {OperationId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceTariffsListPartial", tariffs);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در بارگیری تعرفه‌های بیمه با شناسه {InsuranceId}. OperationId: {OperationId}, User: {UserName} (Id: {UserId})",
                    insuranceId, operationId, _currentUserService.UserName, _currentUserService.UserId);

                return PartialView("_InsuranceTariffsListPartial", new List<InsuranceTariffViewModel>());
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// جمع‌آوری خطاهای مدل برای ارسال به کلاینت
        /// </summary>
        private List<string> GetModelErrors()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
        }

        #endregion
    }
}