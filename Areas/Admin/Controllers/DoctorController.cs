using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر اصلی مدیریت پزشکان در سیستم کلینیک شفا
    /// مسئولیت: عملیات CRUD پزشکان (ایجاد، ویرایش، حذف، مشاهده)
    /// </summary>
    [Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorController : Controller
    {
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorCreateEditViewModel> _createEditValidator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorController(
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService,
            IValidator<DoctorCreateEditViewModel> createEditValidator,
            IMapper mapper)
        {
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _createEditValidator = createEditValidator ?? throw new ArgumentNullException(nameof(createEditValidator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorController>();
        }

        #region Index & Listing

        [HttpGet]
        public async Task<ActionResult> Index(DoctorSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست پزشکان توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                    searchModel = new DoctorSearchViewModel();

                if (searchModel.PageNumber <= 0) searchModel.PageNumber = 1;
                if (searchModel.PageSize <= 0) searchModel.PageSize = 10;

                var result = await _doctorCrudService.GetDoctorsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست پزشکان: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new DoctorIndexViewModel());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست پزشکان");
                TempData["Error"] = "خطا در بارگذاری لیست پزشکان";
                return View(new DoctorIndexViewModel());
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var createModel = new DoctorCreateEditViewModel
                {
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                return View(createModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد پزشک");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد پزشک";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DoctorCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var validationResult = await _createEditValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                var result = await _doctorCrudService.CreateDoctorAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "پزشک جدید با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پزشک جدید");
                TempData["Error"] = "خطا در ایجاد پزشک جدید";
                return View(model);
            }
        }

        #endregion

        #region Edit

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index");
                }

                var result = await _doctorCrudService.GetDoctorForEditAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش پزشک {DoctorId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش پزشک";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DoctorCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                var validationResult = await _createEditValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                var result = await _doctorCrudService.UpdateDoctorAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اطلاعات پزشک با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در ویرایش پزشک";
                return View(model);
            }
        }

        #endregion

        #region Details

        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["Error"] = "شناسه پزشک نامعتبر است.";
                    return RedirectToAction("Index");
                }

                var result = await _doctorCrudService.GetDoctorDetailsAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات پزشک {DoctorId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات پزشک";
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete & Restore

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                var result = await _doctorCrudService.SoftDeleteDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = "پزشک با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پزشک {DoctorId}", id);
                return Json(new { success = false, message = "خطا در حذف پزشک" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restore(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                var result = await _doctorCrudService.RestoreDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = "پزشک با موفقیت بازیابی شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی پزشک {DoctorId}", id);
                return Json(new { success = false, message = "خطا در بازیابی پزشک" });
            }
        }

        #endregion
    }
}
