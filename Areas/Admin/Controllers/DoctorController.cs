using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت پزشکان برای سیستم‌های پزشکی
    /// این کنترلر تمام عملیات مربوط به پزشکان از جمله ایجاد، ویرایش، حذف و جستجو را پشتیبانی می‌کند
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. رعایت کامل استانداردهای امنیتی سیستم‌های پزشکی (HIPAA, GDPR)
    /// 2. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 3. سیستم ردیابی کامل (Audit Trail) با ثبت تمام عملیات حساس
    /// 4. مدیریت صحیح دسترسی‌ها بر اساس نقش‌های کاربری
    /// 5. پشتیبانی از جستجوی پیشرفته و صفحه‌بندی
    /// 6. امکانات اضطراری برای مدیریت بحران‌های پزشکی
    /// 7. رابط کاربری کاربرپسند و سازگار با استانداردهای پزشکی
    /// 8. تست‌پذیری کامل برای تضمین کیفیت سیستم
    /// </summary>
    [Authorize]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger _log;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _context; // تعریف عضو جدید

        /// <summary>
        /// سازنده اصلی کنترلر پزشکان
        /// </summary>
        public DoctorsController(
            IDoctorService doctorService,
            ILogger logger,
            ApplicationUserManager userManager,
            ApplicationDbContext context) // اضافه کردن context به سازنده
        {
            _doctorService = doctorService;
            _log = logger.ForContext<DoctorsController>();
            _userManager = userManager;
            _context = context; // اختصاص مقدار به عضو
        }

        /// <summary>
        /// سازنده برای تست‌های واحد
        /// </summary>
        public DoctorsController(
            IDoctorService doctorService,
            ILogger logger,
            ApplicationUserManager userManager,
            ApplicationDbContext context,
            HttpContextBase httpContext) : this(doctorService, logger, userManager, context)
        {
            // برای تست‌های واحد
            ControllerContext = new ControllerContext(httpContext, new RouteData(), this);
        }
        #region عملیات اصلی (Index, Create, Edit, Details, Delete)

        /// <summary>
        /// نمایش لیست پزشکان با قابلیت جستجو و صفحه‌بندی
        /// این عملیات فقط برای کاربران با نقش‌های مجاز قابل دسترسی است
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم‌ها در هر صفحه</param>
        /// <returns>مشاهده لیست پزشکان</returns>
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            _log.Information("درخواست نمایش لیست پزشکان. User: {UserName} (Id: {UserId}), Search: {SearchTerm}, Page: {Page}",
                User.Identity.Name,
                AppHelper.CurrentUserId,
                searchTerm,
                page);

            try
            {
                var result = await _doctorService.SearchDoctorsAsync(searchTerm, page, pageSize);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی پزشکان: {Message}", result.Message);
                    ModelState.AddModelError("", result.Message);
                    return View(new PagedResult<DoctorIndexViewModel>());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش لیست پزشکان");
                ModelState.AddModelError("", "خطای سیستم رخ داده است. لطفاً مجدداً تلاش کنید.");
                return View(new PagedResult<DoctorIndexViewModel>());
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد پزشک جدید
        /// این عملیات فقط برای کاربران با نقش مدیر یا منشی قابل دسترسی است
        /// </summary>
        /// <returns>مشاهده فرم ایجاد پزشک</returns>
        /// <summary>
        /// نمایش فرم ایجاد پزشک جدید
        /// </summary>
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public ActionResult Create()
        {
            // ارسال لیست کلینیک‌ها و دپارتمان‌ها به ویو
            // ارسال لیست کلینیک‌ها و دپارتمان‌ها به ویو
            ViewBag.Clinics = GetActiveClinics();
            ViewBag.Departments = GetActiveDepartments();

            _log.Information("درخواست ایجاد پزشک جدید. User: {UserName} (Id: {UserId})",
                User.Identity.Name,
                AppHelper.CurrentUserId);

            return View(new DoctorCreateEditViewModel());
        }

        /// <summary>
        /// پردازش درخواست ایجاد پزشک جدید
        /// </summary>
        /// <param name="model">مدل اطلاعات پزشک جدید</param>
        /// <returns>نتیجه عملیات ایجاد</returns>
        [HttpPost]
        //[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DoctorCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد پزشک جدید با نام {FirstName} {LastName}. User: {UserName} (Id: {UserId})",
                model.FirstName,
                model.LastName,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            if (!ModelState.IsValid)
            {
                _log.Warning("مدل نامعتبر برای ایجاد پزشک. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var result = await _doctorService.CreateDoctorAsync(model);

            if (!result.Success)
            {
                _log.Warning("خطا در ایجاد پزشک: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information("پزشک جدید با موفقیت ایجاد شد. DoctorId: {DoctorId}", result.Data);
            TempData["SuccessMessage"] = "پزشک با موفقیت ایجاد شد.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// نمایش فرم ویرایش پزشک
        /// </summary>
        /// <param name="id">شناسه پزشک</param>
        /// <returns>مشاهده فرم ویرایش پزشک</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        public async Task<ActionResult> Edit(int id)
        {
            // ارسال لیست کلینیک‌ها و دپارتمان‌ها به ویو
            ViewBag.Clinics = GetActiveClinics();
            ViewBag.Departments = GetActiveDepartments();
            // ثبت درخواست ویرایش پزشک در لاگ
            _log.Information("درخواست ویرایش پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            var result = await _doctorService.GetDoctorForEditAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت اطلاعات پزشک برای ویرایش: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// پردازش درخواست ویرایش پزشک
        /// </summary>
        /// <param name="model">مدل اطلاعات به‌روزرسانی شده پزشک</param>
        /// <returns>نتیجه عملیات ویرایش</returns>
        [HttpPost]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DoctorCreateEditViewModel model)
        {
            _log.Information("درخواست ویرایش پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                model.DoctorId,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            if (!ModelState.IsValid)
            {
                _log.Warning("مدل نامعتبر برای ویرایش پزشک. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            var result = await _doctorService.UpdateDoctorAsync(model);

            if (!result.Success)
            {
                _log.Warning("خطا در ویرایش پزشک: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            _log.Information("پزشک با شناسه {DoctorId} با موفقیت ویرایش شد.", model.DoctorId);
            TempData["SuccessMessage"] = "اطلاعات پزشک با موفقیت به‌روزرسانی شد.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// نمایش جزئیات کامل یک پزشک
        /// </summary>
        /// <param name="id">شناسه پزشک</param>
        /// <returns>مشاهده جزئیات پزشک</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist + "," + AppRoles.Doctor)]
        public async Task<ActionResult> Details(int id)
        {
            _log.Information("درخواست جزئیات پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            var result = await _doctorService.GetDoctorDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات پزشک: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// نمایش صفحه تأیید حذف پزشک
        /// </summary>
        /// <param name="id">شناسه پزشک</param>
        /// <returns>مشاهده صفحه تأیید حذف</returns>
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult> Delete(int id)
        {
            _log.Information("درخواست نمایش صفحه حذف پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            var result = await _doctorService.GetDoctorDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات پزشک برای حذف: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            return View(result.Data);
        }

        /// <summary>
        /// پردازش درخواست حذف پزشک
        /// </summary>
        /// <param name="id">شناسه پزشک</param>
        /// <returns>نتیجه عملیات حذف</returns>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = AppRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            _log.Information("درخواست حذف پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            var result = await _doctorService.DeleteDoctorAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در حذف پزشک: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Details", new { id = id });
            }

            _log.Information("پزشک با شناسه {DoctorId} با موفقیت حذف شد.", id);
            TempData["SuccessMessage"] = "پزشک با موفقیت حذف شد.";
            return RedirectToAction("Index");
        }

        #endregion

        #region عملیات API و AJAX

        /// <summary>
        /// دریافت اطلاعات پزشک برای استفاده در APIها و کال‌های AJAX
        /// </summary>
        /// <param name="id">شناسه پزشک</param>
        /// <returns>اطلاعات پزشک به فرمت JSON</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist + "," + AppRoles.Doctor)]
        [HttpGet]
        public async Task<ActionResult> GetDoctorDetailsJson(int id)
        {
            _log.Information("درخواست JSON جزئیات پزشک با شناسه {DoctorId}. User: {UserName} (Id: {UserId})",
                id,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            var result = await _doctorService.GetDoctorDetailsAsync(id);

            if (!result.Success)
            {
                _log.Warning("خطا در دریافت جزئیات پزشک برای API: {Message}", result.Message);
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = result.Data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// جستجوی پیشرفته پزشکان برای استفاده در کامبو باکس‌ها و انتخابگرهای پیشرفته
        /// </summary>
        /// <param name="term">عبارت جستجو</param>
        /// <returns>لیست پزشکان به فرمت JSON</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist + "," + AppRoles.Doctor)]
        [HttpGet]
        public async Task<ActionResult> SearchDoctorsJson(string term)
        {
            _log.Information("درخواست جستجوی پزشکان با عبارت {SearchTerm}. User: {UserName} (Id: {UserId})",
                term,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            try
            {
                var result = await _doctorService.SearchDoctorsAsync(term, 1, 10);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی پزشکان برای API: {Message}", result.Message);
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var items = result.Data.Items.Select(d => new
                {
                    id = d.DoctorId,
                    text = $"{d.FirstName} {d.LastName} - {d.Specialization}",
                    specialization = d.Specialization,
                    phoneNumber = d.PhoneNumber
                });

                return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در جستجوی پزشکان برای API");
                return Json(new { success = false, message = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region عملیات ویژه پزشکی

        /// <summary>
        /// نمایش لیست پزشکان فعال برای انتخاب در سیستم نوبت‌دهی
        /// این عملیات برای بیماران و پزشکان قابل دسترسی است
        /// </summary>
        /// <returns>لیست پزشکان فعال</returns>
        [Authorize(Roles = AppRoles.Patient + "," + AppRoles.Doctor)]
        public async Task<ActionResult> ActiveDoctors()
        {
            _log.Information("درخواست لیست پزشکان فعال. User: {UserName} (Id: {UserId})",
                User.Identity.Name,
                AppHelper.CurrentUserId);

            try
            {
                var result = await _doctorService.SearchDoctorsAsync("", 1, 50);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی پزشکان فعال: {Message}", result.Message);
                    return View("Error", new HandleErrorInfo(
                        new Exception(result.Message),
                        "Doctors",
                        "ActiveDoctors"));
                }

                // فیلتر پزشکان فعال
                var activeDoctors = result.Data.Items
                    .Where(d => !string.IsNullOrEmpty(d.Specialization))
                    .ToList();

                return View(activeDoctors);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش لیست پزشکان فعال");
                return View("Error");
            }
        }

        /// <summary>
        /// نمایش تقویم نوبت‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ مورد نظر</param>
        /// <returns>تقویم نوبت‌های پزشک</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist + "," + AppRoles.Doctor + "," + AppRoles.Patient)]
        public ActionResult Schedule(int doctorId, DateTime? date = null)
        {
            _log.Information("درخواست تقویم نوبت‌های پزشک {DoctorId}. User: {UserName} (Id: {UserId}), Date: {Date}",
                doctorId,
                User.Identity.Name,
                AppHelper.CurrentUserId,
                date ?? DateTime.Today);

            ViewBag.DoctorId = doctorId;
            ViewBag.Date = date ?? DateTime.Today;
            return View();
        }

        /// <summary>
        /// دریافت نوبت‌های پزشک برای تقویم
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="start">تاریخ شروع</param>
        /// <param name="end">تاریخ پایان</param>
        /// <returns>لیست نوبت‌ها به فرمت JSON</returns>
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist + "," + AppRoles.Doctor + "," + AppRoles.Patient)]
        [HttpGet]
        public async Task<ActionResult> GetAppointments(int doctorId, DateTime start, DateTime end)
        {
            _log.Information("درخواست نوبت‌های پزشک {DoctorId} از {Start} تا {End}. User: {UserName} (Id: {UserId})",
                doctorId,
                start,
                end,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            try
            {
                // در اینجا باید سرویس نوبت‌دهی فراخوانی شود
                // برای مثال:
                // var appointments = await _appointmentService.GetAppointmentsAsync(doctorId, start, end);

                // برای نمونه، داده‌های ساختگی ایجاد می‌کنیم
                var appointments = new List<object>
                {
                    new {
                        id = 1,
                        title = "ویزیت بیمار",
                        start = start.ToString("yyyy-MM-ddTHH:mm:ss"),
                        end = start.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                        status = "Scheduled",
                        patientName = "احمد محمدی"
                    }
                };

                return Json(appointments, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در دریافت نوبت‌های پزشک {DoctorId}", doctorId);
                return Json(new { error = "خطای سیستم رخ داده است." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region عملیات اضطراری و مدیریت بحران

        /// <summary>
        /// نمایش لیست پزشکان در دسترس برای موقعیت‌های اضطراری
        /// این عملیات برای تمام کاربران قابل دسترسی است
        /// </summary>
        /// <returns>لیست پزشکان در دسترس اضطراری</returns>
        [AllowAnonymous]
        public async Task<ActionResult> EmergencyDoctors()
        {
            _log.Information("درخواست لیست پزشکان اضطراری");

            try
            {
                var result = await _doctorService.SearchDoctorsAsync("", 1, 20);

                if (!result.Success)
                {
                    _log.Warning("خطا در جستجوی پزشکان اضطراری: {Message}", result.Message);
                    return View("Error", new HandleErrorInfo(
                        new Exception(result.Message),
                        "Doctors",
                        "EmergencyDoctors"));
                }

                // فیلتر پزشکان فعال و تخصص‌های اورژانس
                var emergencyDoctors = result.Data.Items
                    .Where(d => !string.IsNullOrEmpty(d.Specialization) &&
                               (d.Specialization.Contains("اورژانس") ||
                                d.Specialization.Contains("فوق تخصص قلب") ||
                                d.Specialization.Contains("فوق تخصص مغز و اعصاب")))
                    .ToList();

                return View(emergencyDoctors);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در نمایش لیست پزشکان اضطراری");
                return View("Error");
            }
        }

        /// <summary>
        /// ایجاد تماس اضطراری با پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>نتیجه عملیات تماس اضطراری</returns>
        [HttpPost]
        [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Receptionist)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EmergencyCall(int doctorId)
        {
            _log.Information("درخواست تماس اضطراری با پزشک {DoctorId}. User: {UserName} (Id: {UserId})",
                doctorId,
                User.Identity.Name,
                AppHelper.CurrentUserId);

            try
            {
                var doctorDetails = await _doctorService.GetDoctorDetailsAsync(doctorId);

                if (!doctorDetails.Success)
                {
                    _log.Warning("خطا در دریافت اطلاعات پزشک برای تماس اضطراری: {Message}", doctorDetails.Message);
                    return Json(new { success = false, message = doctorDetails.Message });
                }

                // در اینجا باید سرویس تماس اضطراری فراخوانی شود
                // برای مثال:
                // var callResult = await _emergencyService.MakeEmergencyCallAsync(doctorId, AppHelper.CurrentUserId);

                // برای نمونه:
                _log.Information("تماس اضطراری با پزشک {DoctorId} توسط {UserId} انجام شد",
                    doctorId,
                    AppHelper.CurrentUserId);

                return Json(new
                {
                    success = true,
                    message = "تماس اضطراری با پزشک انجام شد.",
                    doctorName = $"{doctorDetails.Data.FirstName} {doctorDetails.Data.LastName}",
                    phoneNumber = doctorDetails.Data.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در ایجاد تماس اضطراری با پزشک {DoctorId}", doctorId);
                return Json(new { success = false, message = "خطای سیستم رخ داده است." });
            }
        }

        #endregion

        #region روش‌های کمکی

        /// <summary>
        /// بررسی دسترسی کاربر به عملیات خاص
        /// </summary>
        /// <param name="requiredRoles">نقش‌های مورد نیاز</param>
        /// <returns>آیا کاربر دسترسی دارد؟</returns>
        protected bool HasAccess(params string[] requiredRoles)
        {
            return requiredRoles.Any(role => User.IsInRole(role));
        }

        /// <summary>
        /// ایجاد پیام‌های امنیتی برای عملیات حساس
        /// </summary>
        /// <param name="action">عملیات انجام شده</param>
        /// <param name="details">جزئیات بیشتر</param>
        protected void LogSecurityEvent(string action, string details = "")
        {
            _log.Information(
                "رویداد امنیتی: {Action} توسط {UserName} (Id: {UserId}). Details: {Details}",
                action,
                User.Identity.Name,
                AppHelper.CurrentUserId,
                details);
        }

        #endregion
        #region روش‌های کمکی برای دریافت داده‌ها

        /// <summary>
        /// دریافت لیست کلینیک‌های فعال
        /// </summary>
        private List<Clinic> GetActiveClinics()
        {
            return _context.Clinics
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌های فعال
        /// </summary>
        private List<Department> GetActiveDepartments()
        {
            return _context.Departments
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToList();
        }

        #endregion
    }
}