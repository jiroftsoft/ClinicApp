//using ClinicApp.Helpers;
//using ClinicApp.Interfaces;
//using ClinicApp.Models.Entities;
//using ClinicApp.ViewModels;
//using DocumentFormat.OpenXml.EMMA;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using System.Web.Mvc;

//namespace ClinicApp.Areas.Admin.Controllers
//{
//    /// <summary>
//    /// کنترلر مدیریت بیمه‌ها - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
//    /// 
//    /// ویژگی‌های کلیدی:
//    /// 1. پشتیبانی از ایجاد، ویرایش و حذف نرم بیمه‌ها
//    /// 2. مدیریت تعرفه‌های بیمه‌ای برای خدمات مختلف
//    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
//    /// 4. ارتباط با کاربران ایجاد کننده، ویرایش کننده و حذف کننده برای ردیابی دقیق
//    /// 5. مدیریت کامل تاریخ‌ها و اطلاعات کاربران مرتبط
//    /// 6. پشتیبانی از بیمه آزاد به عنوان پیش‌فرض برای بیماران بدون بیمه
//    /// 7. پشتیبانی از محیط‌های پزشکی ایرانی با تاریخ شمسی و اعداد فارسی
//    /// 8. عدم استفاده از AutoMapper برای کنترل کامل بر روی داده‌ها
//    /// 9. امنیت بالا با رعایت استانداردهای سیستم‌های پزشکی
//    /// 10. عملکرد بهینه برای محیط‌های Production
//    /// </summary>
//    //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
//    [RouteArea("Admin")]
//    [RoutePrefix("Insurance")]
//    public class InsuranceController2 : Controller
//    {
//        private readonly IInsuranceService _insuranceService;
//        private readonly IServiceService _serviceService;
//        private readonly ILogger _log;
//        private const int PageSize = 15;
//        private readonly ICurrentUserService _currentUserService;

//        public InsuranceController2(
//            IInsuranceService insuranceService,
//            IServiceService serviceService,
//            ILogger logger,
//            ICurrentUserService currentUserService)
//        {
//            _insuranceService = insuranceService;
//            _serviceService = serviceService;
//            _log = logger.ForContext<InsuranceController>();
//            _currentUserService = currentUserService;
//        }

//        #region Index & Search

//        /// <summary>
//        /// نمایش صفحه اصلی بیمه‌ها
//        /// </summary>
//        [HttpGet]
//        [Route("")]
//        [Route("Index")]
//        public ActionResult Index()
//        {
//            _log.Information("بازدید از صفحه اصلی بیمه‌ها. User: {UserName} (Id: {UserId})",
//                _currentUserService.UserName, _currentUserService.UserId);

//            return View();
//        }

//        /// <summary>
//        /// بارگیری لیست بیمه‌ها با صفحه‌بندی
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("LoadInsurances")]
//        public async Task<PartialViewResult> LoadInsurances(string searchTerm = "", int page = 1, bool activeOnly = true)
//        {
//            _log.Information(
//                "درخواست لود بیمه‌ها. SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}, ActiveOnly: {ActiveOnly}. User: {UserName} (Id: {UserId})",
//                searchTerm, page, PageSize, activeOnly, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                // دریافت لیست بیمه‌ها از سرویس
//                var result = await _insuranceService.GetActiveInsurancesAsync();

//                // بررسی موفقیت عملیات و داده‌ها
//                if (!result.Success || result.Data == null)
//                {
//                    _log.Warning(
//                        "دریافت لیست بیمه‌ها ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                    return PartialView("_InsuranceListPartial", new PagedResult<InsuranceViewModel>());
//                }

//                // فیلتر بر اساس جستجو
//                var filteredInsurances = result.Data.AsQueryable();

//                if (!string.IsNullOrEmpty(searchTerm))
//                {
//                    filteredInsurances = filteredInsurances.Where(i =>
//                        i.Name.Contains(searchTerm) ||
//                        i.Description.Contains(searchTerm));
//                }

//                if (activeOnly)
//                {
//                    filteredInsurances = filteredInsurances.Where(i => i.IsActive);
//                }

//                // اعمال صفحه‌بندی
//                var pagedResult = new PagedResult<InsuranceViewModel>
//                {
//                    Items = filteredInsurances.Skip((page - 1) * PageSize).Take(PageSize).ToList(),
//                    PageNumber = page,
//                    PageSize = PageSize,
//                    TotalItems = filteredInsurances.Count()
//                };

//                _log.Information(
//                    "لود بیمه‌ها با موفقیت انجام شد. Count: {Count}, Page: {Page}. User: {UserName} (Id: {UserId})",
//                    pagedResult.TotalItems, page, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_InsuranceListPartial", pagedResult);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در لود بیمه‌ها. SearchTerm: {SearchTerm}, Page: {Page}, ActiveOnly: {ActiveOnly}. User: {UserName} (Id: {UserId})",
//                    searchTerm, page, activeOnly, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_InsuranceListPartial", new PagedResult<InsuranceViewModel>());
//            }
//        }

//        #endregion

//        #region Details

//        /// <summary>
//        /// نمایش جزئیات بیمه
//        /// </summary>
//        [HttpGet]
//        [Route("Details/{id}")]
//        public async Task<ActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                _log.Warning(
//                    "درخواست جزئیات بیمه با شناسه خالی. User: {UserName} (Id: {UserId})",
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }

//            _log.Information(
//                "درخواست جزئیات بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceDetailsAsync(id.Value);
//                if (!result.Success || result.Data == null)
//                {
//                    _log.Warning(
//                        "دریافت جزئیات بیمه شناسه {InsuranceId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                    TempData["ErrorMessage"] = "بیمه مورد نظر یافت نشد یا دسترسی لازم را ندارید.";
//                    return RedirectToAction("Index");
//                }

//                _log.Information(
//                    "دریافت جزئیات بیمه شناسه {InsuranceId} با موفقیت انجام شد. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return View(result.Data);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در دریافت جزئیات بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                TempData["ErrorMessage"] = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
//                return RedirectToAction("Index");
//            }
//        }


//        #endregion

//        #region Create

//        /// <summary>
//        /// نمایش فرم ایجاد بیمه جدید
//        /// </summary>
//        [HttpGet]
//        [Route("Create")]
//        public async Task<ActionResult> Create()
//        {
//            _log.Information(
//                "درخواست ایجاد بیمه جدید. User: {UserName} (Id: {UserId})",
//                _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var viewModel = new CreateInsuranceViewModel
//                {
//                    IsActive = true
//                };

//                _log.Information(
//                    "صفحه ایجاد بیمه با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_CreateInsurance", viewModel);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در بارگیری صفحه ایجاد بیمه. User: {UserName} (Id: {UserId})",
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً تلاش کنید." }, JsonRequestBehavior.AllowGet);
//            }
//        }

//        /// <summary>
//        /// پردازش ایجاد بیمه جدید
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("Create")]
//        public async Task<ActionResult> Create(CreateInsuranceViewModel model)
//        {
//            _log.Information(
//                "درخواست ایجاد بیمه جدید با نام {Name}. User: {UserName} (Id: {UserId})",
//                model.Name, _currentUserService.UserName, _currentUserService.UserId);

//            if (!ModelState.IsValid)
//            {
//                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
//                _log.Warning(
//                    "اعتبارسنجی ایجاد بیمه با نام {Name} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
//                    model.Name, errors, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, errors });
//            }

//            try
//            {
//                var result = await _insuranceService.CreateInsuranceAsync(model);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "بیمه جدید با نام {Name} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
//                        model.Name, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = true, message = result.Message });
//                }

//                _log.Warning(
//                    "ایجاد بیمه با نام {Name} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                    model.Name, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در ایجاد بیمه با نام {Name}. User: {UserName} (Id: {UserId})",
//                    model.Name, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
//            }
//        }


//        #endregion

//        #region Edit

//        /// <summary>
//        /// نمایش فرم ویرایش بیمه
//        /// </summary>
//        [HttpGet]
//        [Route("Edit/{id}")]
//        public async Task<ActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                _log.Warning(
//                    "درخواست ویرایش بیمه با شناسه خالی. User: {UserName} (Id: {UserId})",
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }

//            _log.Information(
//                "درخواست ویرایش بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceDetailsAsync(id.Value);
//                if (!result.Success || result.Data == null)
//                {
//                    _log.Warning(
//                        "دریافت اطلاعات بیمه شناسه {InsuranceId} برای ویرایش ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                    return HttpNotFound();
//                }

//                var model = new EditInsuranceViewModel
//                {
//                    InsuranceId = result.Data.InsuranceId,
//                    Name = result.Data.Name,
//                    Description = result.Data.Description,
//                    DefaultPatientShare = result.Data.DefaultPatientShare,
//                    DefaultInsurerShare = result.Data.DefaultInsurerShare,
//                    IsActive = result.Data.IsActive
//                };

//                _log.Information(
//                    "صفحه ویرایش بیمه شناسه {InsuranceId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_EditInsurance", model);

//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در بارگیری صفحه ویرایش بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return HttpNotFound();
//            }
//        }

//        /// <summary>
//        /// پردازش ویرایش بیمه
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("Edit")]
//        public async Task<ActionResult> Edit(EditInsuranceViewModel model)
//        {
//            if (model.InsuranceId <= 0)
//            {
//                _log.Warning(
//                    "درخواست ویرایش بیمه با شناسه نامعتبر. InsuranceId: {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }

//            _log.Information(
//                "درخواست ویرایش بیمه شناسه {InsuranceId} با نام {Name}. User: {UserName} (Id: {UserId})",
//                model.InsuranceId, model.Name, _currentUserService.UserName, _currentUserService.UserId);

//            if (!ModelState.IsValid)
//            {
//                _log.Warning(
//                    "اعتبارسنجی ویرایش بیمه شناسه {InsuranceId} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, ModelState.Values.SelectMany(v => v.Errors),
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return View(model);
//            }

//            try
//            {
//                var result = await _insuranceService.UpdateInsuranceAsync(model);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "بیمه شناسه {InsuranceId} با نام {Name} با موفقیت ویرایش شد. User: {UserName} (Id: {UserId})",
//                        model.InsuranceId, model.Name, _currentUserService.UserName, _currentUserService.UserId);

//                    TempData["SuccessMessage"] = result.Message;
//                    return RedirectToAction("Index");
//                }

//                _log.Warning(
//                    "ویرایش بیمه شناسه {InsuranceId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                ModelState.AddModelError("", result.Message);
//                return View(model);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در ویرایش بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                ModelState.AddModelError("", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
//                return View(model);
//            }
//        }

//        #endregion

//        #region Delete

//        /// <summary>
//        /// پردازش حذف بیمه
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("Delete")]
//        public async Task<JsonResult> Delete(int id)
//        {
//            _log.Information(
//                "درخواست حذف بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.DeleteInsuranceAsync(id);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "بیمه شناسه {InsuranceId} با موفقیت حذف شد. User: {UserName} (Id: {UserId})",
//                        id, _currentUserService.UserName, _currentUserService.UserId);

//                    TempData["SuccessMessage"] = result.Message;
//                }
//                else
//                {
//                    _log.Warning(
//                        "حذف بیمه شناسه {InsuranceId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);
//                }

//                return Json(new { success = result.Success, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در حذف بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
//            }
//        }

//        #endregion

//        #region Tariffs Management

//        /// <summary>
//        /// نمایش صفحه مدیریت تعرفه‌های بیمه
//        /// </summary>
//        [HttpGet]
//        [Route("ManageTariffs/{insuranceId}")]
//        public async Task<ActionResult> ManageTariffs(int insuranceId)
//        {
//            _log.Information(
//                "درخواست مدیریت تعرفه‌های بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                // دریافت اطلاعات بیمه
//                var insuranceResult = await _insuranceService.GetInsuranceDetailsAsync(insuranceId);
//                if (!insuranceResult.Success || insuranceResult.Data == null)
//                {
//                    _log.Warning(
//                        "دریافت اطلاعات بیمه شناسه {InsuranceId} برای مدیریت تعرفه‌ها ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        insuranceId, insuranceResult.Message,
//                        _currentUserService.UserName, _currentUserService.UserId);

//                    return HttpNotFound();
//                }

//                // دریافت تعرفه‌ها
//                var tariffsResult = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
//                var tariffs = tariffsResult.Success && tariffsResult.Data != null ?
//                    tariffsResult.Data : new List<InsuranceTariffViewModel>();

//                // دریافت خدمات فعال
//                var services = await _serviceService.GetActiveServicesAsync() ??
//                    new List<ServiceSelectItem>();

//                // ساخت مدل
//                var model = new InsuranceTariffsViewModel
//                {
//                    InsuranceId = insuranceId,
//                    InsuranceName = insuranceResult.Data.Name,
//                    Tariffs = tariffs,
//                    AvailableServices = services
//                        .Where(s => tariffs.All(t => t.ServiceTitle != s.Title))
//                        .Select(s => new SelectListItem
//                        {
//                            Value = s.ServiceId.ToString(),
//                            Text = s.Title
//                        })
//                        .ToList()
//                };

//                _log.Information(
//                    "صفحه مدیریت تعرفه‌های بیمه شناسه {InsuranceId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return View(model);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در بارگیری صفحه مدیریت تعرفه‌ها برای بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return HttpNotFound();
//            }
//        }

//        /// <summary>
//        /// بارگیری لیست تعرفه‌های بیمه
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("LoadTariffs")]
//        public async Task<PartialViewResult> LoadTariffs(int insuranceId)
//        {
//            _log.Information(
//                "درخواست لود تعرفه‌های بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
//                var tariffs = result.Success && result.Data != null ? 
//                    result.Data : new List<InsuranceTariffViewModel>();

//                _log.Information(
//                    "لود تعرفه‌های بیمه شناسه {InsuranceId} با موفقیت انجام شد. Count: {Count}. User: {UserName} (Id: {UserId})",
//                    insuranceId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_TariffListPartial", tariffs);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در لود تعرفه‌های بیمه شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_TariffListPartial", new List<InsuranceTariffViewModel>());
//            }
//        }

//        /// <summary>
//        /// پردازش افزودن تعرفه جدید
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("AddTariff")]
//        public async Task<ActionResult> AddTariff(CreateInsuranceTariffViewModel model)
//        {
//            _log.Information(
//                "درخواست افزودن تعرفه برای بیمه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//            // اعتبارسنجی سمت سرور
//            if (!ModelState.IsValid)
//            {
//                _log.Warning(
//                    "اعتبارسنجی افزودن تعرفه برای بیمه {InsuranceId} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, ModelState.Values.SelectMany(v => v.Errors),
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "داده‌های ورودی نامعتبر است." });
//            }

//            // اعتبارسنجی سهم‌ها
//            if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
//            {
//                if (Math.Abs(model.PatientShare.Value + model.InsurerShare.Value - 100) > 0.01m)
//                {
//                    _log.Warning(
//                        "جمع سهم‌های بیمار و بیمه برای بیمه {InsuranceId} برابر با 100 نیست. User: {UserName} (Id: {UserId})",
//                        model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
//                }
//            }

//            try
//            {
//                var result = await _insuranceService.CreateInsuranceTariffAsync(model);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "تعرفه برای بیمه {InsuranceId} با موفقیت ایجاد شد. User: {UserName} (Id: {UserId})",
//                        model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = true, message = result.Message });
//                }

//                _log.Warning(
//                    "افزودن تعرفه برای بیمه {InsuranceId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در افزودن تعرفه برای بیمه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
//            }
//        }

//        /// <summary>
//        /// نمایش فرم ایجاد تعرفه جدید
//        /// </summary>
//        [HttpGet]
//        [Route("CreateTariff")]
//        public async Task<ActionResult> CreateTariff(int insuranceId)
//        {
//            _log.Information(
//                "درخواست فرم ایجاد تعرفه جدید برای بیمه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                // دریافت خدمات فعال
//                var services = await _serviceService.GetActiveServicesAsync() ??
//                               new List<ServiceSelectItem>();

//                // دریافت تعرفه‌های موجود
//                var tariffsResult = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
//                var tariffs = tariffsResult.Success && tariffsResult.Data != null ?
//                    tariffsResult.Data : new List<InsuranceTariffViewModel>();

//                // فیلتر خدمات قابل انتخاب
//                var availableServices = services
//                    .Where(s => tariffs.All(t => t.ServiceTitle != s.Title))
//                    .ToList();

//                ViewBag.AvailableServices = availableServices;
//                ViewBag.InsuranceId = insuranceId;

//                _log.Information(
//                    "فرم ایجاد تعرفه جدید برای بیمه {InsuranceId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_AddTariffPartial");
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در بارگیری فرم ایجاد تعرفه جدید برای بیمه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_ErrorPartial", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
//            }
//        }

//        /// <summary>
//        /// نمایش فرم ویرایش تعرفه
//        /// </summary>
//        [HttpGet]
//        [Route("EditTariff/{id}")]
//        public async Task<ActionResult> EditTariff(int id)
//        {
//            _log.Information(
//                "درخواست ویرایش تعرفه با شناسه {TariffId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceTariffForEditAsync(id);
//                if (!result.Success || result.Data == null)
//                {
//                    _log.Warning(
//                        "دریافت تعرفه شناسه {TariffId} برای ویرایش ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                    return PartialView("_ErrorPartial", result.Message);
//                }

//                var model = result.Data;

//                // دریافت خدمات فعال
//                var services = await _serviceService.GetActiveServicesAsync() ??
//                               new List<ServiceSelectItem>();

//                ViewBag.Services = services;

//                _log.Information(
//                    "فرم ویرایش تعرفه شناسه {TariffId} با موفقیت بارگیری شد. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_EditTariffPartial", model);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در بارگیری فرم ویرایش تعرفه شناسه {TariffId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_ErrorPartial", "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
//            }
//        }

//        /// <summary>
//        /// پردازش ویرایش تعرفه
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("UpdateTariff")]
//        public async Task<ActionResult> UpdateTariff(EditInsuranceTariffViewModel model)
//        {
//            _log.Information(
//                "درخواست ویرایش تعرفه {InsuranceTariffId}. User: {UserName} (Id: {UserId})",
//                model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

//            // اعتبارسنجی سمت سرور
//            if (!ModelState.IsValid)
//            {
//                _log.Warning(
//                    "اعتبارسنجی ویرایش تعرفه {InsuranceTariffId} شکست خورد. Errors: {@Errors}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceTariffId, ModelState.Values.SelectMany(v => v.Errors),
//                    _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "داده‌های ورودی نامعتبر است." });
//            }

//            // اعتبارسنجی سهم‌ها
//            if (model.PatientShare.HasValue && model.InsurerShare.HasValue)
//            {
//                if (Math.Abs(model.PatientShare.Value + model.InsurerShare.Value - 100) > 0.01m)
//                {
//                    _log.Warning(
//                        "جمع سهم‌های بیمار و بیمه برای تعرفه {InsuranceTariffId} برابر با 100 نیست. User: {UserName} (Id: {UserId})",
//                        model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = false, message = "جمع سهم بیمار و بیمه باید برابر با 100 درصد باشد." });
//                }
//            }

//            try
//            {
//                var result = await _insuranceService.UpdateInsuranceTariffAsync(model);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "تعرفه {InsuranceTariffId} با موفقیت ویرایش شد. User: {UserName} (Id: {UserId})",
//                        model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = true, message = result.Message });
//                }

//                _log.Warning(
//                    "ویرایش تعرفه {InsuranceTariffId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceTariffId, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در ویرایش تعرفه {InsuranceTariffId}. User: {UserName} (Id: {UserId})",
//                    model.InsuranceTariffId, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
//            }
//        }

//        /// <summary>
//        /// پردازش حذف تعرفه
//        /// </summary>
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        [Route("DeleteTariff")]
//        public async Task<JsonResult> DeleteTariff(int id)
//        {
//            _log.Information(
//                "درخواست حذف تعرفه {InsuranceTariffId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.DeleteInsuranceTariffAsync(id);
//                if (result.Success)
//                {
//                    _log.Information(
//                        "تعرفه {InsuranceTariffId} با موفقیت حذف شد. User: {UserName} (Id: {UserId})",
//                        id, _currentUserService.UserName, _currentUserService.UserId);

//                    return Json(new { success = result.Success, message = result.Message });
//                }

//                _log.Warning(
//                    "حذف تعرفه {InsuranceTariffId} ناموفق بود. Error: {Error}. User: {UserName} (Id: {UserId})",
//                    id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = result.Success, message = result.Message });
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در حذف تعرفه {InsuranceTariffId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return Json(new { success = false, message = "خطای سیستمی رخ داده است. لطفاً بعداً مجدداً تلاش کنید." });
//            }
//        }

//        #endregion

//        #region API Endpoints

//        /// <summary>
//        /// دریافت تعرفه‌های بیمه برای نمایش در صفحه جزئیات
//        /// </summary>
//        [HttpGet]
//        [Route("GetInsuranceTariffs/{insuranceId}")]
//        public async Task<PartialViewResult> GetInsuranceTariffs(int insuranceId)
//        {
//            _log.Information(
//                "درخواست بارگیری تعرفه‌های بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceTariffsAsync(insuranceId);
//                var tariffs = result.Success && result.Data != null ?
//                    result.Data : new List<InsuranceTariffViewModel>();

//                _log.Information(
//                    "تعرفه‌های بیمه با شناسه {InsuranceId} با موفقیت بارگیری شد. Count: {Count}. User: {UserName} (Id: {UserId})",
//                    insuranceId, tariffs.Count, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_InsuranceTariffsListPartial", tariffs);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطا در بارگیری تعرفه‌های بیمه با شناسه {InsuranceId}. User: {UserName} (Id: {UserId})",
//                    insuranceId, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_InsuranceTariffsListPartial", new List<InsuranceTariffViewModel>());
//            }
//        }
//        /// <summary>
//        /// دریافت جزئیات تعرفه بیمه برای ویرایش
//        /// </summary>
//        [HttpGet]
//        [Route("GetInsuranceTariffForEdit/{id}")]
//        public async Task<ActionResult> GetInsuranceTariffForEdit(int id)
//        {
//            _log.Information(
//                "درخواست دریافت جزئیات تعرفه بیمه با شناسه {TariffId}. User: {UserName} (Id: {UserId})",
//                id, _currentUserService.UserName, _currentUserService.UserId);

//            try
//            {
//                var result = await _insuranceService.GetInsuranceTariffForEditAsync(id);
//                if (!result.Success || result.Data == null)
//                {
//                    _log.Warning(
//                        "درخواست جزئیات تعرفه بیمه ناموجود. TariffId: {TariffId}. Error: {Error}. User: {UserName} (Id: {UserId})",
//                        id, result.Message, _currentUserService.UserName, _currentUserService.UserId);

//                    return HttpNotFound();
//                }

//                _log.Information(
//                    "جزئیات تعرفه بیمه با شناسه {TariffId} با موفقیت دریافت شد. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return PartialView("_EditTariffPartial", result.Data);
//            }
//            catch (Exception ex)
//            {
//                _log.Error(
//                    ex,
//                    "خطای سیستمی در دریافت جزئیات تعرفه بیمه شناسه {TariffId}. User: {UserName} (Id: {UserId})",
//                    id, _currentUserService.UserName, _currentUserService.UserId);

//                return HttpNotFound();
//            }
//        }

//        #endregion
//    }
//}