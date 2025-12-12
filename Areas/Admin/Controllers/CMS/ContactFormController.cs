using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت فرم تماس
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class ContactFormController : BaseCMSController
    {
        private readonly IContactFormService _contactFormService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ContactFormController(
            IContactFormService contactFormService,
            ICurrentUserService currentUserService)
        {
            _contactFormService = contactFormService ?? throw new ArgumentNullException(nameof(contactFormService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<ContactFormController>();
        }

        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(ContactFormSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست فرم‌های تماس توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new ContactFormSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _contactFormService.GetContactFormsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست فرم‌های تماس: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyResult = new PagedResult<ContactFormIndexViewModel>(new System.Collections.Generic.List<ContactFormIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize);
                    var emptyPageViewModel = new ContactFormIndexPageViewModel
                    {
                        ContactForms = emptyResult,
                        SearchModel = searchModel,
                        UnreadCount = 0,
                        NewCount = 0,
                        InProgressCount = 0,
                        RepliedCount = 0
                    };
                    return View(GetViewPath("Index"), emptyPageViewModel);
                }

                // دریافت آمار
                var unreadCountResult = await _contactFormService.GetUnreadCountAsync();
                var newCountResult = await _contactFormService.GetNewCountAsync();
                var inProgressCountResult = await _contactFormService.GetInProgressCountAsync();
                var repliedCountResult = await _contactFormService.GetRepliedCountAsync();

                var pageViewModel = new ContactFormIndexPageViewModel
                {
                    ContactForms = result.Data,
                    SearchModel = searchModel,
                    UnreadCount = unreadCountResult.Success ? unreadCountResult.Data : 0,
                    NewCount = newCountResult.Success ? newCountResult.Data : 0,
                    InProgressCount = inProgressCountResult.Success ? inProgressCountResult.Data : 0,
                    RepliedCount = repliedCountResult.Success ? repliedCountResult.Data : 0
                };

                // Production-Ready: استفاده از GetViewPath برای جلوگیری از تداخل با Views/ContactForm/Index.cshtml
                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست فرم‌های تماس");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست فرم‌های تماس");
                var emptyResult = new PagedResult<ContactFormIndexViewModel>(new System.Collections.Generic.List<ContactFormIndexViewModel>(), 0, 1, 10);
                var emptyPageViewModel = new ContactFormIndexPageViewModel
                {
                    ContactForms = emptyResult,
                    SearchModel = new ContactFormSearchViewModel { PageNumber = 1, PageSize = 10 },
                    UnreadCount = 0,
                    NewCount = 0,
                    InProgressCount = 0,
                    RepliedCount = 0
                };
                return View(GetViewPath("Index"), emptyPageViewModel);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _contactFormService.GetContactFormDetailsAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                // علامت‌گذاری به عنوان خوانده شده در صورت نیاز
                if (!result.Data.IsRead)
                {
                    await _contactFormService.MarkAsReadAsync(id, _currentUserService.UserId);
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات فرم تماس - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات فرم تماس");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Reply

        [HttpGet]
        public async Task<ActionResult> Reply(int id)
        {
            try
            {
                var detailsResult = await _contactFormService.GetContactFormDetailsAsync(id);
                if (!detailsResult.Success)
                {
                    NotificationHelper.SetError(TempData, detailsResult.Message);
                    return RedirectToAction("Index");
                }

                var replyViewModel = new ContactFormReplyViewModel
                {
                    ContactFormId = detailsResult.Data.ContactFormId,
                    FullName = detailsResult.Data.FullName,
                    Email = detailsResult.Data.Email,
                    PhoneNumber = detailsResult.Data.PhoneNumber,
                    Subject = detailsResult.Data.Subject,
                    Message = detailsResult.Data.Message,
                    Category = detailsResult.Data.Category,
                    SendEmail = true,
                    SendSms = false
                };

                return View(GetViewPath("Reply"), replyViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم پاسخ - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم پاسخ");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reply(ContactFormReplyViewModel model)
        {
            try
            {
                _logger.Information("درخواست پاسخ به فرم تماس - ContactFormId: {ContactFormId}", model.ContactFormId);

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً خطاهای موجود در فرم را برطرف کنید.");
                    return View(GetViewPath("Reply"), model);
                }

                var result = await _contactFormService.ReplyToContactFormAsync(model, _currentUserService.UserId);
                if (!result.Success)
                {
                    _logger.Warning("خطا در ثبت پاسخ: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(GetViewPath("Reply"), model);
                }

                _logger.Information("پاسخ با موفقیت ثبت شد - ContactFormId: {ContactFormId}", model.ContactFormId);
                NotificationHelper.SetSuccess(TempData, "پاسخ با موفقیت ثبت شد");
                return RedirectToAction("Details", new { id = model.ContactFormId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ثبت پاسخ - ContactFormId: {ContactFormId}", model.ContactFormId);
                NotificationHelper.SetError(TempData, "خطا در ثبت پاسخ");
                return View(GetViewPath("Reply"), model);
            }
        }

        #endregion

        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _contactFormService.DeleteContactFormAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "فرم تماس با موفقیت حذف شد");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف فرم تماس - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف فرم تماس");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Mark as Read/Unread

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _contactFormService.MarkAsReadAsync(id, _currentUserService.UserId);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "فرم تماس به عنوان خوانده شده علامت‌گذاری شد");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری به عنوان خوانده شده - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در علامت‌گذاری به عنوان خوانده شده");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarkAsUnread(int id)
        {
            try
            {
                var result = await _contactFormService.MarkAsUnreadAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "فرم تماس به عنوان خوانده نشده علامت‌گذاری شد");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در علامت‌گذاری به عنوان خوانده نشده - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در علامت‌گذاری به عنوان خوانده نشده");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Change Status

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStatus(int id, ContactFormStatus status)
        {
            try
            {
                var result = await _contactFormService.ChangeStatusAsync(id, status);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                }
                else
                {
                    NotificationHelper.SetSuccess(TempData, "وضعیت با موفقیت تغییر کرد");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت - ContactFormId: {ContactFormId}", id);
                NotificationHelper.SetError(TempData, "خطا در تغییر وضعیت");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Get Unread Count (برای Dashboard)

        [HttpGet]
        public async Task<JsonResult> GetUnreadCount()
        {
            try
            {
                var result = await _contactFormService.GetUnreadCountAsync();
                if (result.Success)
                {
                    return Json(new { success = true, count = result.Data }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد خوانده نشده");
                return Json(new { success = false, message = "خطا در دریافت تعداد خوانده نشده" }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}

