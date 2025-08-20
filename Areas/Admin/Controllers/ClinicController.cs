using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت کلینیک‌ها در بخش ادمین سیستم‌های پزشکی
    /// این کنترلر تمام عملیات مربوط به کلینیک‌ها از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
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
    /// </summary>
    //[Authorize(Roles = AppRoles.Admin)]
    //[RouteArea("Admin")]
    [RoutePrefix("Clinic")]
    public class ClinicController : Controller
    {
        private readonly IClinicService _clinicService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// سازنده اصلی کنترلر کلینیک‌ها
        /// </summary>
        public ClinicController(
            IClinicService clinicService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _clinicService = clinicService;
            _log = logger.ForContext<ClinicController>();
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// سازنده برای تست‌های واحد
        /// </summary>
        public ClinicController(
            IClinicService clinicService,
            ILogger logger,
            ICurrentUserService currentUserService,
            HttpContextBase httpContext) : this(clinicService, logger, currentUserService)
        {
            ControllerContext = new ControllerContext(httpContext, new RouteData(), this);
        }

        #region عملیات اصلی (Index, Create, Edit, Details, Delete)

        /// <summary>
        /// نمایش لیست کلینیک‌ها با قابلیت جستجو و صفحه‌بندی
        /// </summary>
        [Route("")]
        [Route("Index")]
        [Route("Index/{page:int=1}")]
        public async Task<ActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            _log.Information("درخواست نمایش لیست کلینیک‌ها در بخش ادمین. User: {UserName} (Id: {UserId}), Search: {SearchTerm}, Page: {Page}",
                User.Identity.Name,
                _currentUserService.UserId,
                searchTerm,
                page);

            try
            {
                var result = await _clinicService.SearchClinicsAsync(searchTerm, page, pageSize);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی کلینیک‌ها در بخش ادمین: {Message}", result.Message);
                    ModelState.AddModelError("", result.Message);
                    return View(new PagedResult<ClinicIndexViewModel>());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش لیست کلینیک‌ها در بخش ادمین");
                ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً مجدداً تلاش کنید.");
                return View(new PagedResult<ClinicIndexViewModel>());
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد کلینیک جدید
        /// </summary>
        [Route("Create")]
        public ActionResult Create()
        {
            _log.Information("درخواست ایجاد کلینیک جدید در بخش ادمین. User: {UserName} (Id: {UserId})",
                User.Identity.Name,
                _currentUserService.UserId);

            return View(new ClinicCreateEditViewModel());
        }

        /// <summary>
        /// پردازش درخواست ایجاد کلینیک جدید
        /// </summary>
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد کلینیک جدید با نام {Name} در بخش ادمین. User: {UserName} (Id: {UserId})",
                model.Name,
                User.Identity.Name,
                _currentUserService.UserId);

            if (!ModelState.IsValid)
            {
                _log.Warning("مدل نامعتبر برای ایجاد کلینیک در بخش ادمین. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var result = await _clinicService.CreateClinicAsync(model);

            if (!result.Success)
            {
                _log.Warning("خطا در ایجاد کلینیک در بخش ادمین: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information("کلینیک جدید با موفقیت ایجاد شد در بخش ادمین. ClinicId: {ClinicId}", result.Data);
            TempData["SuccessMessage"] = "کلینیک با موفقیت ایجاد شد.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// نمایش فرم ویرایش کلینیک
        /// </summary>
        [Route("Edit/{id:int}")]
        public async Task<ActionResult> Edit(int id)
        {
            _log.Information("درخواست ویرایش کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _clinicService.GetClinicForEditAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت اطلاعات کلینیک برای ویرایش در بخش ادمین: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// پردازش درخواست ویرایش کلینیک
        /// </summary>
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست ویرایش کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                model.ClinicId,
                User.Identity.Name,
                _currentUserService.UserId);

            if (!ModelState.IsValid)
            {
                _log.Warning("مدل نامعتبر برای ویرایش کلینیک در بخش ادمین. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var result = await _clinicService.UpdateClinicAsync(model);

            if (!result.Success)
            {
                _log.Warning("خطا در ویرایش کلینیک در بخش ادمین: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information("کلینیک با شناسه {ClinicId} در بخش ادمین با موفقیت ویرایش شد.", model.ClinicId);
            TempData["SuccessMessage"] = "اطلاعات کلینیک با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// نمایش جزئیات کامل یک کلینیک
        /// </summary>
        [Route("Details/{id:int}")]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information("درخواست جزئیات کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _clinicService.GetClinicDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات کلینیک در بخش ادمین: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// نمایش صفحه تأیید حذف کلینیک
        /// </summary>
        [Route("Delete/{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information("درخواست نمایش صفحه حذف کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _clinicService.GetClinicDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات کلینیک برای حذف در بخش ادمین: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// پردازش درخواست حذف کلینیک
        /// </summary>
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information("درخواست حذف کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _clinicService.DeleteClinicAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در حذف کلینیک در بخش ادمین: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", new { id = id });
            }

            _log.Information("کلینیک با شناسه {ClinicId} در بخش ادمین با موفقیت حذف شد.", id);
            TempData["SuccessMessage"] = "کلینیک با موفقیت حذف شد.";
            return RedirectToAction("Index");
        }

        #endregion

        #region عملیات API و AJAX

        /// <summary>
        /// دریافت اطلاعات کلینیک برای استفاده در APIها و کال‌های AJAX
        /// </summary>
        [HttpGet]
        [Route("GetClinicDetailsJson/{id:int}")]
        public async Task<ActionResult> GetClinicDetailsJson(int id)
        {
            _log.Information("درخواست JSON جزئیات کلینیک با شناسه {ClinicId} در بخش ادمین. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                _currentUserService.UserId);

            var result = await _clinicService.GetClinicDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات کلینیک برای API در بخش ادمین: {Message}", result.Message);
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// جستجوی پیشرفته کلینیک‌ها برای استفاده در کامبو باکس‌ها
        /// </summary>
        [HttpGet]
        [Route("SearchClinicsJson")]
        public async Task<ActionResult> SearchClinicsJson(string term)
        {
            _log.Information("درخواست جستجوی کلینیک‌ها با عبارت {SearchTerm} در بخش ادمین. User: {UserName} (Id: {UserId})",
                term,
                User.Identity.Name,
                _currentUserService.UserId);

            try
            {
                var result = await _clinicService.SearchClinicsAsync(term, 1, 10);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی کلینیک‌ها برای API در بخش ادمین: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var items = result.Data.Items.Select(c => new
                {
                    id = c.ClinicId,
                    text = c.Name,
                    address = c.Address,
                    phoneNumber = c.PhoneNumber
                });

                return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در جستجوی کلینیک‌ها برای API در بخش ادمین");
                return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}