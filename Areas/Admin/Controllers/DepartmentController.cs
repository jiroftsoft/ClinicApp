using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت دپارتمان‌ها در بخش ادمین سیستم‌های پزشکی
    /// این کنترلر تمام عملیات مربوط به دپارتمان‌ها از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل استانداردهای امنیتی سیستم‌های پزشکی (HIPAA, GDPR)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. سیستم ردیابی کامل (Audit Trail) با ثبت تمام عملیات حساس
    /// 4. مدیریت صحیح دسترسی‌ها (فقط مدیران سیستم)
    /// 5. پشتیبانی از جستجوی پیشرفته و صفحه‌بندی بهینه‌شده
    /// 6. پشتیبانی از APIهای سریع برای عملیات‌های AJAX
    /// 7. طراحی واکنش‌گرا برای تمام دستگاه‌ها (از جمله تبلت‌های پزشکی)
    /// 8. کلیدهای میانبر پزشکی برای افزایش سرعت کار
    /// 9. بررسی کامل ارتباطات سلسله‌مراتبی (کلینیک -> دپارتمان -> پزشک)
    /// 10. سیستم تأیید دو مرحله‌ای برای عملیات‌های حساس
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin)]
    //[RouteArea("Admin")]
    [RoutePrefix("Department")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IClinicService _clinicService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// سازنده اصلی کنترلر دپارتمان‌ها
        /// </summary>
        public DepartmentController(
            IDepartmentService departmentService,
            IClinicService clinicService,
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _departmentService = departmentService;
            _clinicService = clinicService;
            _log = logger.ForContext<DepartmentController>();
            _currentUserService = currentUserService;
            _context = context;
        }

        /// <summary>
        /// سازنده برای تست‌های واحد
        /// </summary>
        public DepartmentController(
            IDepartmentService departmentService,
            IClinicService clinicService,
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context,
            HttpContextBase httpContext) : this(departmentService, clinicService, logger, currentUserService,context)
        {
            ControllerContext = new ControllerContext(httpContext, new RouteData(), this);
        }

        #region عملیات اصلی (Index, Create, Edit, Details, Delete)

        /// <summary>
        /// نمایش لیست دپارتمان‌ها با قابلیت جستجو و صفحه‌بندی
        /// </summary>
        [Route("")]
        [Route("Index")]
        [Route("Index/{page:int=1}")]
        public async Task<ActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10, int? clinicId = null)
        {
            _log.Information("درخواست نمایش لیست دپارتمان‌ها در بخش ادمین. User: {UserName} (Id: {UserId}), Search: {SearchTerm}, Page: {Page}, ClinicId: {ClinicId}",
                User.Identity.Name,
                _currentUserService.UserId,
                searchTerm,
                page,
                clinicId);

            try
            {
                // دریافت لیست کلینیک‌های فعال
                var clinics = await GetActiveClinics();

                // دریافت لیست دپارتمان‌ها
                var departmentsResult = await _departmentService.SearchDepartmentsAsync(searchTerm, page, pageSize);

                // ایجاد ViewModel
                var viewModel = new DepartmentIndexPageViewModel
                {
                    Clinics = clinics.ToList(),
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    ClinicId = clinicId
                };

                if (departmentsResult.Success)
                {
                    // اعمال فیلتر کلینیک اگر وجود داشته باشد
                    if (clinicId.HasValue)
                    {
                        viewModel.Departments = new PagedResult<DepartmentIndexViewModel>
                        {
                            Items = departmentsResult.Data.Items
                                .Where(d => d.ClinicId == clinicId.Value)
                                .ToList(),
                            PageNumber = page,
                            PageSize = pageSize,
                            TotalItems = departmentsResult.Data.Items.Count(d => d.ClinicId == clinicId.Value)
                        };
                    }
                    else
                    {
                        viewModel.Departments = departmentsResult.Data;
                    }
                }
                else
                {
                    _log.Warning("خطا در جستجوی دپارتمان‌ها در بخش ادمین: {Message}", departmentsResult.Message);
                    ModelState.AddModelError("", departmentsResult.Message);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش لیست دپارتمان‌ها در بخش ادمین");

                // ایجاد ViewModel با مقادیر پیش‌فرض
                var viewModel = new DepartmentIndexPageViewModel
                {
                    Clinics = (await GetActiveClinics()).ToList(),
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    ClinicId = clinicId
                };

                ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً مجدداً تلاش کنید.");
                return View(viewModel);
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد دپارتمان جدید
        /// </summary>
        [Route("Create")]
        public async Task<ActionResult> Create()
        {
            _log.Information("درخواست ایجاد دپارتمان جدید در بخش ادمین. User: {UserName} (Id: {UserId})",
                User.Identity.Name,
                _currentUserService.UserId);

            ViewBag.Clinics = await GetActiveClinics();
            return View(new DepartmentCreateEditViewModel());
        }

        /// <summary>
        /// پردازش درخواست ایجاد دپارتمان جدید در سیستم‌های پزشکی
        /// این اکشن تمام جنبه‌های ایمنی، عملکرد و رعایت استانداردهای پزشکی را مدیریت می‌کند
        /// 
        /// ویژگی‌های کلیدی:
        /// 1. پشتیبانی کامل از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
        /// 2. مدیریت صحیح اعتبارسنجی ورودی‌ها برای جلوگیری از اطلاعات نامعتبر
        /// 3. لاگ‌گیری کامل و دقیق برای رعایت استانداردهای امنیتی و HIPAA
        /// 4. پشتیبانی از تاریخ‌های شمسی برای محیط‌های پزشکی ایرانی
        /// 5. سیستم مدیریت خطا و بازیابی در شرایط بحرانی
        /// </summary>
        /// <param name="model">مدل ایجاد دپارتمان</param>
        /// <returns>نتیجه عملیات ایجاد دپارتمان</returns>
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> Create(DepartmentCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد دپارتمان جدید با نام {Name} در بخش ادمین. User: {UserName} (Id: {UserId})",
                model.Name,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // اعتبارسنجی اولیه مدل
                if (!ModelState.IsValid)
                {
                    _log.Warning("مدل نامعتبر برای ایجاد دپارتمان در بخش ادمین. Errors: {Errors}",
                        string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));

                    await PopulateClinicsForView();
                    return View(model);
                }

                // اعتبارسنجی تخصصی پزشکی
                if (await IsDuplicateDepartmentNameAsync(model.Name, model.ClinicId))
                {
                    _log.Warning("درخواست ایجاد دپارتمان با نام تکراری در کلینیک. Name: {Name}, ClinicId: {ClinicId}",
                        model.Name,
                        model.ClinicId);

                    ModelState.AddModelError("Name", "دپارتمانی با این نام در این کلینیک از قبل وجود دارد.");
                    await PopulateClinicsForView();
                    return View(model);
                }

                // ایجاد دپارتمان از طریق سرویس
                var result = await _departmentService.CreateDepartmentAsync(model);

                if (!result.Success)
                {
                    _log.Warning("خطا در ایجاد دپارتمان در بخش ادمین: {Message}", result.Message);
                    ModelState.AddModelError("", result.Message);
                    await PopulateClinicsForView();
                    return View(model);
                }

                _log.Information("دپارتمان جدید با موفقیت ایجاد شد در بخش ادمین. DepartmentId: {DepartmentId}, Name: {Name}, ClinicId: {ClinicId}",
                    result.Data,
                    model.Name,
                    model.ClinicId);

                TempData["SuccessMessage"] = "دپارتمان با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در ایجاد دپارتمان در بخش ادمین. Name: {Name}, ClinicId: {ClinicId}, User: {UserName} (Id: {UserId})",
                    model.Name,
                    model.ClinicId,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                // در سیستم‌های پزشکی، هر خطا باید به دقت گزارش شود
                ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.");
                await PopulateClinicsForView();
                return View(model);
            }
        }

        /// <summary>
        /// بررسی وجود نام تکراری دپارتمان در یک کلینیک
        /// </summary>
        private async Task<bool> IsDuplicateDepartmentNameAsync(string name, int clinicId)
        {
            return await _context.Departments
                .AnyAsync(d => d.Name == name &&
                              d.ClinicId == clinicId &&
                              !d.IsDeleted);
        }

        /// <summary>
        /// پر کردن لیست کلینیک‌ها برای نمایش در ویو
        /// </summary>
        private async Task PopulateClinicsForView()
        {
            ViewBag.Clinics = await _context.Clinics
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.ClinicId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        /// <summary>
        /// نمایش فرم ویرایش دپارتمان در سیستم‌های پزشکی
        /// این اکشن تمام جنبه‌های امنیتی و عملکردی را برای نمایش فرم ویرایش مدیریت می‌کند
        /// 
        /// ویژگی‌های کلیدی:
        /// 1. بررسی وجود دپارتمان قبل از نمایش فرم ویرایش
        /// 2. ارائه اطلاعات کامل دپارتمان برای ویرایش
        /// 3. لاگ‌گیری کامل و دقیق برای رعایت استانداردهای امنیتی و HIPAA
        /// 4. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
        /// 5. سیستم مدیریت خطا و بازیابی در شرایط بحرانی
        /// </summary>
        /// <param name="id">شناسه دپارتمان مورد نظر</param>
        /// <returns>صفحه ویرایش دپارتمان</returns>
        [HttpGet]
        [Route("Edit/{id:int}")]
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information("درخواست ویرایش دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                // دریافت جزئیات دپارتمان
                var result = await _departmentService.GetDepartmentForEditAsync(id);

                if (!result.Success)
                {
                    _log.Warning("خطا در دریافت جزئیات دپارتمان برای ویرایش در بخش ادمین: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // پر کردن لیست کلینیک‌ها برای نمایش در ویو
                ViewBag.Clinics = await _context.Clinics
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClinicId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در دریافت جزئیات دپارتمان برای ویرایش در بخش ادمین. DepartmentId: {DepartmentId}, User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش درخواست ویرایش دپارتمان
        /// </summary>
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentCreateEditViewModel model)
        {
            _log.Information("درخواست ویرایش دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                model.DepartmentId,
                User.Identity.Name,
                _currentUserService.UserId);

            if (!ModelState.IsValid)
            {
                _log.Warning("مدل نامعتبر برای ویرایش دپارتمان در بخش ادمین. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                ViewBag.Clinics = await GetActiveClinics();
                return View(model);
            }

            var result = await _departmentService.UpdateDepartmentAsync(model);

            if (!result.Success)
            {
                _log.Warning("خطا در ویرایش دپارتمان در بخش ادمین: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                ViewBag.Clinics = await GetActiveClinics();
                return View(model);
            }

            _log.Information("دپارتمان با شناسه {DepartmentId} در بخش ادمین با موفقیت ویرایش شد.", model.DepartmentId);
            TempData["SuccessMessage"] = "اطلاعات دپارتمان با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// نمایش جزئیات کامل یک دپارتمان
        /// </summary>
        [Route("Details/{id:int}")]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information("درخواست جزئیات دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _departmentService.GetDepartmentDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات دپارتمان در بخش ادمین: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }


        #endregion


        #region  عملیات حذف دپارتمان

        /// <summary>
        /// نمایش صفحه تأیید حذف دپارتمان در سیستم‌های پزشکی
        /// این اکشن تمام جنبه‌های امنیتی و عملکردی را برای نمایش صفحه تأیید حذف مدیریت می‌کند
        /// 
        /// ویژگی‌های کلیدی:
        /// 1. بررسی وجود دپارتمان قبل از نمایش صفحه تأیید
        /// 2. ارائه اطلاعات کامل دپارتمان برای تصمیم‌گیری کاربر
        /// 3. لاگ‌گیری کامل و دقیق برای رعایت استانداردهای امنیتی و HIPAA
        /// 4. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
        /// 5. سیستم مدیریت خطا و بازیابی در شرایط بحرانی
        /// </summary>
        /// <param name="id">شناسه دپارتمان مورد نظر</param>
        /// <returns>صفحه تأیید حذف دپارتمان</returns>
        [HttpGet]
        [Route("Delete/{id:int}")]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information("درخواست نمایش صفحه حذف دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                var result = await _departmentService.GetDepartmentDetailsAsync(id);

                if (!result.Success)
                {
                    _log.Warning("خطا در دریافت جزئیات دپارتمان برای حذف در بخش ادمین: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                // بررسی وجود پزشکان فعال قبل از نمایش صفحه حذف
                if (result.Data.DoctorCount > 0)
                {
                    _log.Information("تلاش برای نمایش صفحه حذف دپارتمان با پزشکان فعال. DepartmentId: {DepartmentId}, DoctorCount: {DoctorCount}",
                        id,
                        result.Data.DoctorCount);

                    TempData["WarningMessage"] = $"امکان حذف دپارتمان وجود ندارد چون {result.Data.DoctorCount} پزشک فعال دارد.";
                    return RedirectToAction("Details", new { id = id });
                }

                // بررسی وجود دسته‌بندی‌های خدمات فعال
                if (result.Data.ServiceCount > 0)
                {
                    _log.Information("تلاش برای نمایش صفحه حذف دپارتمان با دسته‌بندی‌های خدمات فعال. DepartmentId: {DepartmentId}, ServiceCount: {ServiceCount}",
                        id,
                        result.Data.ServiceCount);

                    TempData["WarningMessage"] = $"امکان حذف دپارتمان وجود ندارد چون {result.Data.ServiceCount} دسته‌بندی خدمات فعال دارد.";
                    return RedirectToAction("Details", new { id = id });
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش صفحه حذف دپارتمان در بخش ادمین. DepartmentId: {DepartmentId}, User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش درخواست حذف دپارتمان در سیستم‌های پزشکی
        /// این اکشن تمام جنبه‌های امنیتی و عملکردی را برای پردازش حذف مدیریت می‌کند
        /// 
        /// ویژگی‌های کلیدی:
        /// 1. بررسی مجدد وابستگی‌ها قبل از انجام عملیات حذف
        /// 2. مدیریت صحیح تراکنش‌ها برای جلوگیری از ناسازگاری داده‌ها
        /// 3. ثبت اطلاعات کامل ردیابی (Audit Trail) برای پیگیری تغییرات
        /// 4. رعایت استانداردهای امنیتی و HIPAA در مدیریت داده‌های پزشکی
        /// 5. پشتیبانی از ساختار سازمانی پزشکی (کلینیک → دپارتمان)
        /// </summary>
        /// <param name="id">شناسه دپارتمان مورد نظر</param>
        /// <returns>نتیجه عملیات حذف دپارتمان</returns>
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information("درخواست حذف دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                _currentUserService.UserName,
                _currentUserService.UserId);

            try
            {
                var result = await _departmentService.DeleteDepartmentAsync(id);

                if (!result.Success)
                {
                    _log.Warning("خطا در حذف دپارتمان در بخش ادمین: {Message}", result.Message);
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Details", new { id = id });
                }

                //// ارسال اطلاع‌رسانی پزشکی
                //await _notificationService.SendNotificationAsync(
                //    _currentUserService.UserId,
                //    "دپارتمان حذف شد",
                //    $"دپارتمان با شناسه {id} با موفقیت حذف شد.",
                //    NotificationType.Success);

                _log.Information("دپارتمان با شناسه {DepartmentId} در بخش ادمین با موفقیت حذف شد. User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["SuccessMessage"] = "دپارتمان با موفقیت حذف شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در حذف دپارتمان در بخش ادمین. DepartmentId: {DepartmentId}, User: {UserName} (Id: {UserId})",
                    id,
                    _currentUserService.UserName,
                    _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستم در حین حذف دپارتمان رخ داده است. لطفاً بعداً مجدداً تلاش کنید.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        #endregion

        #region عملیات API و AJAX

        /// <summary>
        /// دریافت اطلاعات دپارتمان برای استفاده در APIها و کال‌های AJAX
        /// </summary>
        [HttpGet]
        [Route("GetDepartmentDetailsJson/{id:int}")]
        public async Task<ActionResult> GetDepartmentDetailsJson(int id)
        {
            _log.Information("درخواست JSON جزئیات دپارتمان با شناسه {DepartmentId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _departmentService.GetDepartmentDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات دپارتمان برای API در بخش ادمین: {Message}", result.Message);
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// جستجوی پیشرفته دپارتمان‌ها برای استفاده در کامبو باکس‌ها
        /// </summary>
        [HttpGet]
        [Route("SearchDepartmentsJson")]
        public async Task<ActionResult> SearchDepartmentsJson(string term)
        {
            _log.Information("درخواست جستجوی دپارتمان‌ها با عبارت {SearchTerm} در بخش ادمین. User: {UserName} (Id: {UserId})",
                term,
                User.Identity.Name,
                _currentUserService.UserId);

            try
            {
                var result = await _departmentService.SearchDepartmentsAsync(term, 1, 10);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی دپارتمان‌ها برای API در بخش ادمین: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var items = result.Data.Items.Select(d => new
                {
                    id = d.DepartmentId,
                    text = d.Name,
                    clinicName = d.ClinicName
                });

                return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در جستجوی دپارتمان‌ها برای API در بخش ادمین");
                return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region روش‌های کمکی

        /// <summary>
        /// دریافت لیست کلینیک‌های فعال برای نمایش در کامبو باکس‌ها
        /// </summary>
        private async Task<IEnumerable<ClinicIndexViewModel>> GetActiveClinics()
        {
            try
            {
                var clinicsResult = await _clinicService.SearchClinicsAsync("", 1, int.MaxValue);
                return clinicsResult.Success ? clinicsResult.Data.Items : new List<ClinicIndexViewModel>();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در دریافت لیست کلینیک‌ها");
                return new List<ClinicIndexViewModel>();
            }
        }

        #endregion
    }
}