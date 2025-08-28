using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities;
using FluentValidation;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر اصلی مدیریت پزشکان در سیستم کلینیک شفا
    /// مسئولیت: عملیات CRUD پزشکان (ایجاد، ویرایش، حذف، مشاهده)
    /// اصل SRP: این کنترولر فقط مسئول مدیریت درخواست‌های HTTP برای پزشکان است
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class DoctorController : Controller
    {
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ISpecializationService _specializationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorCreateEditViewModel> _createEditValidator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorController(
            IDoctorCrudService doctorCrudService,
            ISpecializationService specializationService,
            ICurrentUserService currentUserService,
            IValidator<DoctorCreateEditViewModel> createEditValidator,
            IMapper mapper)
        {
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _specializationService = specializationService ?? throw new ArgumentNullException(nameof(specializationService));
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

                // تنظیم مدل جستجو
                searchModel = NormalizeSearchModel(searchModel);

                // بارگذاری تخصص‌های فعال برای فیلتر
                await LoadSpecializationsForView();

                var result = await _doctorCrudService.GetDoctorsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست پزشکان: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new DoctorIndexPageViewModel());
                }

                // ایجاد DoctorIndexPageViewModel
                var pageViewModel = CreateIndexPageViewModel(result.Data, searchModel);

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست پزشکان");
                TempData["Error"] = "خطا در بارگذاری لیست پزشکان";
                return View(new DoctorIndexPageViewModel());
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                var createModel = new DoctorCreateEditViewModel
                {
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await LoadSpecializationsForView();
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
                await LoadSpecializationsForView();

                if (!await ValidateModelAsync(model))
                    return View(model);

                if (!await ValidateUniqueConstraintsAsync(model))
                    return View(model);

                // تبدیل تاریخ شمسی به میلادی
                if (!ConvertPersianDate(model))
                    return View(model);

                // پردازش آپلود تصویر پروفایل
                await ProcessProfileImageUpload(model);

                var result = await _doctorCrudService.CreateDoctorAsync(model);

                if (!result.Success)
                {
                    _logger.Error("Failed to create doctor: {Error}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("Doctor created successfully: {DoctorId}", result.Data.DoctorId);
                TempData["Success"] = "پزشک جدید با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پزشک جدید");
                TempData["Error"] = "خطا در ایجاد پزشک جدید";
                await LoadSpecializationsForView();
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
                if (!ValidateId(id))
                    return RedirectToAction("Index");

                var result = await _doctorCrudService.GetDoctorForEditAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "پزشک مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                await LoadSpecializationsForView();
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
                await LoadSpecializationsForView();

                if (!await ValidateModelAsync(model))
                    return View(model);

                if (!await ValidateUniqueConstraintsForEditAsync(model))
                    return View(model);

                // تبدیل تاریخ شمسی به میلادی
                if (!ConvertPersianDate(model))
                    return View(model);

                // پردازش آپلود تصویر پروفایل
                await ProcessProfileImageUpload(model);

                var result = await _doctorCrudService.UpdateDoctorAsync(model);

                if (!result.Success)
                {
                    _logger.Error("Failed to update doctor: {Error}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                _logger.Information("Doctor updated successfully: {DoctorId}", model.DoctorId);
                TempData["Success"] = "اطلاعات پزشک با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پزشک {DoctorId}", model.DoctorId);
                TempData["Error"] = "خطا در ویرایش پزشک";
                await LoadSpecializationsForView();
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
                if (!ValidateId(id))
                    return RedirectToAction("Index");

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
                if (!ValidateId(id))
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
                if (!ValidateId(id))
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

        #region AJAX Operations

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleStatus(int id, bool activate)
        {
            try
            {
                if (!ValidateId(id))
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                var result = activate 
                    ? await _doctorCrudService.ActivateDoctorAsync(id)
                    : await _doctorCrudService.DeactivateDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                var actionText = activate ? "فعال" : "غیرفعال";
                return Json(new { success = true, message = $"پزشک با موفقیت {actionText} شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تغییر وضعیت پزشک {DoctorId} به {Status}", id, activate);
                return Json(new { success = false, message = "خطا در تغییر وضعیت پزشک" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تنظیم مدل جستجو
        /// </summary>
        private DoctorSearchViewModel NormalizeSearchModel(DoctorSearchViewModel searchModel)
        {
            if (searchModel == null)
                searchModel = new DoctorSearchViewModel();

            // پشتیبانی از فیلدهای backward compatibility
            if (searchModel.Page <= 0) searchModel.Page = 1;
            if (searchModel.PageNumber <= 0) searchModel.PageNumber = searchModel.Page;
            if (searchModel.PageSize <= 0) searchModel.PageSize = 10;

            // تبدیل Status به IsActive
            if (!string.IsNullOrEmpty(searchModel.Status))
            {
                if (bool.TryParse(searchModel.Status, out bool isActive))
                {
                    searchModel.IsActive = isActive;
                }
            }

            return searchModel;
        }

        /// <summary>
        /// ایجاد مدل صفحه Index
        /// </summary>
        private DoctorIndexPageViewModel CreateIndexPageViewModel(PagedResult<DoctorIndexViewModel> data, DoctorSearchViewModel searchModel)
        {
            return new DoctorIndexPageViewModel
            {
                Doctors = data,
                SearchModel = searchModel,
                TotalCount = data.TotalItems,
                ActiveCount = data.Items?.Count(d => d.IsActive) ?? 0,
                InactiveCount = data.Items?.Count(d => !d.IsActive) ?? 0,
                TodayCount = data.Items?.Count(d => d.CreatedAt.Date == DateTime.Today) ?? 0
            };
        }

        /// <summary>
        /// بارگذاری تخصص‌های فعال برای ViewBag
        /// </summary>
        private async Task LoadSpecializationsForView()
        {
            try
            {
                var specializationsResult = await _specializationService.GetActiveSpecializationsAsync();
                if (specializationsResult.Success)
                {
                    ViewBag.Specializations = specializationsResult.Data;
                }
                else
                {
                    _logger.Warning("خطا در بارگذاری تخصص‌ها: {Error}", specializationsResult.Message);
                    ViewBag.Specializations = new List<Specialization>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری تخصص‌ها");
                ViewBag.Specializations = new List<Specialization>();
            }
        }

        /// <summary>
        /// اعتبارسنجی مدل
        /// </summary>
        private async Task<bool> ValidateModelAsync(DoctorCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.Warning("ModelState validation failed: {@Errors}", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return false;
            }

            var validationResult = await _createEditValidator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    _logger.Warning("Validation error for {Property}: {Error}", error.PropertyName, error.ErrorMessage);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// اعتبارسنجی محدودیت‌های یکتا برای ایجاد
        /// </summary>
        private async Task<bool> ValidateUniqueConstraintsAsync(DoctorCreateEditViewModel model)
        {
            // بررسی تکراری نبودن کد ملی
            if (!string.IsNullOrEmpty(model.NationalCode))
            {
                var existingDoctor = await _doctorCrudService.GetDoctorByNationalCodeAsync(model.NationalCode);
                if (existingDoctor.Success && existingDoctor.Data != null)
                {
                    ModelState.AddModelError("NationalCode", "پزشکی با این کد ملی قبلاً ثبت شده است.");
                    return false;
                }
            }

            // بررسی تکراری نبودن شماره نظام پزشکی
            if (!string.IsNullOrEmpty(model.MedicalCouncilCode))
            {
                var existingDoctor = await _doctorCrudService.GetDoctorByMedicalCouncilCodeAsync(model.MedicalCouncilCode);
                if (existingDoctor.Success && existingDoctor.Data != null)
                {
                    ModelState.AddModelError("MedicalCouncilCode", "پزشکی با این شماره نظام پزشکی قبلاً ثبت شده است.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// اعتبارسنجی محدودیت‌های یکتا برای ویرایش
        /// </summary>
        private async Task<bool> ValidateUniqueConstraintsForEditAsync(DoctorCreateEditViewModel model)
        {
            // بررسی تکراری نبودن شماره نظام پزشکی (به جز خودش)
            if (!string.IsNullOrEmpty(model.MedicalCouncilCode))
            {
                var existingDoctor = await _doctorCrudService.GetDoctorByMedicalCouncilCodeAsync(model.MedicalCouncilCode);
                if (existingDoctor.Success && existingDoctor.Data != null && existingDoctor.Data.DoctorId != model.DoctorId)
                {
                    ModelState.AddModelError("MedicalCouncilCode", "پزشکی با این شماره نظام پزشکی قبلاً ثبت شده است.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// تبدیل تاریخ شمسی به میلادی
        /// </summary>
        private bool ConvertPersianDate(DoctorCreateEditViewModel model)
        {
            if (!string.IsNullOrEmpty(model.DateOfBirthShamsi))
            {
                try
                {
                    model.DateOfBirth = PersianDateHelper.ToGregorianDate(model.DateOfBirthShamsi);
                }
                catch (Exception ex)
                {
                    _logger.Warning("خطا در تبدیل تاریخ تولد: {Error}", ex.Message);
                    ModelState.AddModelError("DateOfBirthShamsi", "تاریخ تولد وارد شده معتبر نیست.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// پردازش آپلود تصویر پروفایل
        /// </summary>
        private async Task ProcessProfileImageUpload(DoctorCreateEditViewModel model)
        {
            try
            {
                var file = Request.Files["ProfileImage"];
                if (file == null || file.ContentLength == 0)
                    return;

                // بررسی نوع فایل
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.Warning("نوع فایل نامعتبر برای تصویر پروفایل: {ContentType}", file.ContentType);
                    return;
                }

                // بررسی اندازه فایل (حداکثر 2MB)
                if (file.ContentLength > 2 * 1024 * 1024)
                {
                    _logger.Warning("اندازه فایل تصویر پروفایل بیش از حد مجاز: {Size} bytes", file.ContentLength);
                    return;
                }

                // ایجاد نام فایل منحصر به فرد
                var fileName = $"doctor_profile_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")}{Path.GetExtension(file.FileName)}";
                
                // مسیر ذخیره فایل
                var uploadPath = Path.Combine(Server.MapPath("~/Content/Images/Doctors"), fileName);
                
                // اطمینان از وجود پوشه
                var directory = Path.GetDirectoryName(uploadPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ذخیره فایل
                file.SaveAs(uploadPath);

                // بازگرداندن مسیر نسبی برای ذخیره در دیتابیس
                var relativePath = $"/Content/Images/Doctors/{fileName}";
                
                _logger.Information("تصویر پروفایل با موفقیت آپلود شد: {Path}", relativePath);
                model.ProfileImageUrl = relativePath;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر پروفایل");
            }
        }

        /// <summary>
        /// اعتبارسنجی شناسه
        /// </summary>
        private bool ValidateId(int id)
        {
            return id > 0;
        }

        #endregion
    }
}

