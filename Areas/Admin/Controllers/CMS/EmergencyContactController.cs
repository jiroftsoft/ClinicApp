using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.CMS;
using ClinicApp.Models;
using ClinicApp.Models.Entities.CMS;
using ClinicApp.ViewModels.CMS;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.CMS
{
    /// <summary>
    /// کنترلر مدیریت تماس‌های اضطراری (Emergency Contact)
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class EmergencyContactController : BaseCMSController
    {
        private readonly IEmergencyContactService _emergencyContactService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public EmergencyContactController(
            IEmergencyContactService emergencyContactService,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
        {
            _emergencyContactService = emergencyContactService ?? throw new ArgumentNullException(nameof(emergencyContactService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = Log.ForContext<EmergencyContactController>();
        }


        #region Index & Listing

        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> Index(EmergencyContactSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست تماس‌های اضطراری توسط کاربر {UserId}", _currentUserService.UserId);

                // تنظیم مدل جستجو
                if (searchModel == null)
                {
                    searchModel = new EmergencyContactSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 10
                    };
                }

                var result = await _emergencyContactService.GetEmergencyContactsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست تماس‌های اضطراری: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(GetViewPath("Index"), new PagedResult<EmergencyContactIndexViewModel>(new System.Collections.Generic.List<EmergencyContactIndexViewModel>(), 0, searchModel.PageNumber, searchModel.PageSize));
                }

                // بارگذاری انواع تماس برای فیلتر
                var contactTypes = await _context.Set<EmergencyContact>()
                    .Where(e => !e.IsDeleted)
                    .Select(e => e.ContactType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                ViewBag.ContactTypes = contactTypes.Select(t => new SelectListItem
                {
                    Value = t,
                    Text = GetTypeDisplayName(t)
                }).ToList();

                return View(GetViewPath("Index"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تماس‌های اضطراری");
                TempData["Error"] = "خطا در بارگذاری لیست تماس‌های اضطراری";
                return View(GetViewPath("Index"), new PagedResult<EmergencyContactIndexViewModel>(new System.Collections.Generic.List<EmergencyContactIndexViewModel>(), 0, 1, 10));
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var result = await _emergencyContactService.GetEmergencyContactDetailsAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View(GetViewPath("Details"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                ViewBag.ContactTypes = new SelectList(new[] { 
                    new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                    new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                    new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                    new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                    new SelectListItem { Text = "پلیس", Value = "Police" },
                    new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                    new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                }, "Value", "Text");

                var model = new EmergencyContactCreateEditViewModel
                {
                    IsActive = true,
                    IsAlwaysVisible = false,
                    DisplayOrder = 0
                };

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد تماس اضطراری");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(EmergencyContactCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد تماس اضطراری جدید توسط کاربر {UserId}", _currentUserService.UserId);

                if (!ModelState.IsValid)
                {
                    ViewBag.ContactTypes = new SelectList(new[] { 
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                        new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                        new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                        new SelectListItem { Text = "پلیس", Value = "Police" },
                        new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                        new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                    }, "Value", "Text", model.ContactType);
                    return View(GetViewPath("Create"), model);
                }

                var result = await _emergencyContactService.CreateEmergencyContactAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد تماس اضطراری: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    ViewBag.ContactTypes = new SelectList(new[] { 
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                        new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                        new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                        new SelectListItem { Text = "پلیس", Value = "Police" },
                        new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                        new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                    }, "Value", "Text", model.ContactType);
                    return View(GetViewPath("Create"), model);
                }

                _logger.Information("تماس اضطراری با موفقیت ایجاد شد - EmergencyContactId: {EmergencyContactId}", result.Data.EmergencyContactId);
                TempData["Success"] = "تماس اضطراری با موفقیت ایجاد شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تماس اضطراری");
                TempData["Error"] = "خطا در ایجاد تماس اضطراری";
                return View(GetViewPath("Create"), model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var result = await _emergencyContactService.GetEmergencyContactForEditAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index");
                }

                ViewBag.ContactTypes = new SelectList(new[] { 
                    new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                    new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                    new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                    new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                    new SelectListItem { Text = "پلیس", Value = "Police" },
                    new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                    new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                }, "Value", "Text", result.Data.ContactType);

                return View(GetViewPath("Edit"), result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmergencyContactCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", model.EmergencyContactId);

                if (!ModelState.IsValid)
                {
                    ViewBag.ContactTypes = new SelectList(new[] { 
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                        new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                        new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                        new SelectListItem { Text = "پلیس", Value = "Police" },
                        new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                        new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                    }, "Value", "Text", model.ContactType);
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _emergencyContactService.UpdateEmergencyContactAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی تماس اضطراری: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    
                    ViewBag.ContactTypes = new SelectList(new[] { 
                        new SelectListItem { Text = "اورژانس", Value = "Emergency" },
                        new SelectListItem { Text = "آمبولانس", Value = "Ambulance" },
                        new SelectListItem { Text = "مرکز مسمومیت", Value = "Poison Control" },
                        new SelectListItem { Text = "آتش‌نشانی", Value = "Fire" },
                        new SelectListItem { Text = "پلیس", Value = "Police" },
                        new SelectListItem { Text = "بیمارستان", Value = "Hospital" },
                        new SelectListItem { Text = "کلینیک", Value = "Clinic" }
                    }, "Value", "Text", model.ContactType);
                    return View(GetViewPath("Edit"), model);
                }

                _logger.Information("تماس اضطراری با موفقیت به‌روزرسانی شد - EmergencyContactId: {EmergencyContactId}", model.EmergencyContactId);
                TempData["Success"] = "تماس اضطراری با موفقیت به‌روزرسانی شد";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", model.EmergencyContactId);
                TempData["Error"] = "خطا در به‌روزرسانی تماس اضطراری";
                return View(GetViewPath("Edit"), model);
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
                _logger.Information("درخواست حذف تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);

                var result = await _emergencyContactService.DeleteEmergencyContactAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تماس اضطراری با موفقیت حذف شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در حذف تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Activate/Deactivate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var result = await _emergencyContactService.ActivateEmergencyContactAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تماس اضطراری با موفقیت فعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در فعال‌سازی تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var result = await _emergencyContactService.DeactivateEmergencyContactAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = "تماس اضطراری با موفقیت غیرفعال شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در غیرفعال‌سازی تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Always Visible

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetAlwaysVisible(int id, bool isAlwaysVisible)
        {
            try
            {
                var result = await _emergencyContactService.SetAlwaysVisibleAsync(id, isAlwaysVisible);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                }
                else
                {
                    TempData["Success"] = isAlwaysVisible ? "تماس اضطراری به عنوان همیشه قابل مشاهده تنظیم شد" : "تماس اضطراری از حالت همیشه قابل مشاهده خارج شد";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تنظیم وضعیت همیشه قابل مشاهده تماس اضطراری - EmergencyContactId: {EmergencyContactId}", id);
                TempData["Error"] = "خطا در تنظیم وضعیت همیشه قابل مشاهده تماس اضطراری";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Helper Methods

        private string GetTypeDisplayName(string contactType)
        {
            return contactType switch
            {
                "Emergency" => "اورژانس",
                "Ambulance" => "آمبولانس",
                "Poison Control" => "مرکز مسمومیت",
                "Fire" => "آتش‌نشانی",
                "Police" => "پلیس",
                "Hospital" => "بیمارستان",
                "Clinic" => "کلینیک",
                _ => contactType ?? "عمومی"
            };
        }

        #endregion
    }
}

