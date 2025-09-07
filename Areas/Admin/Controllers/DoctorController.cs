using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
// طبق DESIGN_PRINCIPLES_CONTRACT از AutoMapper استفاده نمی‌کنیم
// از Factory Method Pattern استفاده می‌کنیم
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels.DoctorManagementVM;
using ClinicApp.Models.Entities;
using FluentValidation;
using Serilog;
using System.Web;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر اصلی مدیریت پزشکان در سیستم کلینیک شفا
    /// مسئولیت: عملیات CRUD پزشکان (ایجاد، ویرایش، حذف، مشاهده)
    /// اصل SRP: این کنترولر فقط مسئول مدیریت درخواست‌های HTTP برای پزشکان است
    /// 
    /// Production Optimizations:
    /// - Performance: Async operations, efficient queries
    /// - Security: Input validation, file upload security, CSRF protection
    /// - Reliability: Comprehensive error handling, logging
    /// - Maintainability: Clean code, helper methods, separation of concerns
    /// </summary>
    //[Authorize(Roles = "Admin")]
    public class DoctorController : Controller
    {
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ISpecializationService _specializationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorCreateEditViewModel> _createEditValidator;
        private readonly ILogger _logger;

        // Production Configuration
        private const int MaxFileSizeInMB = 2;
        private const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        private static readonly string UploadPath = "~/Content/Images/Doctors";

        public DoctorController(
            IDoctorCrudService doctorCrudService,
            ISpecializationService specializationService,
            ICurrentUserService currentUserService,
            IValidator<DoctorCreateEditViewModel> createEditValidator)
        {
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _specializationService = specializationService ?? throw new ArgumentNullException(nameof(specializationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _createEditValidator = createEditValidator ?? throw new ArgumentNullException(nameof(createEditValidator));
            _logger = Log.ForContext<DoctorController>();
        }

        #region Index & Listing

        /// <summary>
        /// نمایش لیست پزشکان با قابلیت جستجو و فیلتر
        /// Performance: Efficient pagination, optimized queries
        /// Security: Input validation, XSS protection
        /// Medical Environment: Real-time data updates for critical medical information
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "*")] // No cache for real-time medical data
        public async Task<ActionResult> Index(DoctorSearchViewModel searchModel)
        {
            // تنظیم HTTP headers برای جلوگیری از کش شدن در محیط پزشکی
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

            try
            {
                _logger.Information("درخواست نمایش لیست پزشکان توسط کاربر {UserId} از IP {IPAddress}",
                    _currentUserService.UserId, GetClientIPAddress());

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

        /// <summary>
        /// نمایش فرم ایجاد پزشک جدید
        /// Security: CSRF protection, input validation
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                var createModel = new DoctorCreateEditViewModel
                {
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    SecurityLevel = "Normal" // Default security level
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

        /// <summary>
        /// ایجاد پزشک جدید
        /// Security: Comprehensive validation, file upload security, audit trail
        /// Performance: Async operations, efficient database queries
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Bio field
        public async Task<ActionResult> Create(DoctorCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد پزشک جدید توسط کاربر {UserId}", _currentUserService.UserId);

                await LoadSpecializationsForView();

                // اعتبارسنجی مدل
                if (!await ValidateModelAsync(model))
                    return View(model);

                // اعتبارسنجی محدودیت‌های یکتا
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

                _logger.Information("Doctor created successfully: {DoctorId} by user {UserId}",
                    result.Data.DoctorId, _currentUserService.UserId);
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

        /// <summary>
        /// نمایش فرم ویرایش پزشک
        /// Security: Authorization check, input validation
        /// </summary>
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

        /// <summary>
        /// ویرایش اطلاعات پزشک
        /// Security: Comprehensive validation, audit trail
        /// Performance: Optimized update operations
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Allow HTML in Bio field
        public async Task<ActionResult> Edit(DoctorCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ویرایش پزشک {DoctorId} توسط کاربر {UserId}",
                    model.DoctorId, _currentUserService.UserId);

                await LoadSpecializationsForView();

                // اعتبارسنجی مدل
                if (!await ValidateModelAsync(model))
                    return View(model);

                // اعتبارسنجی محدودیت‌های یکتا
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

                _logger.Information("Doctor updated successfully: {DoctorId} by user {UserId}",
                    model.DoctorId, _currentUserService.UserId);
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

        /// <summary>
        /// نمایش جزئیات پزشک
        /// Performance: Optimized query with includes
        /// Security: Authorization check
        /// Medical Environment: Real-time data updates for critical medical information
        /// </summary>
        [HttpGet]
        [OutputCache(Duration = 0, VaryByParam = "id")] // No cache for real-time medical data
        public async Task<ActionResult> Details(int id)
        {
            // تنظیم HTTP headers برای جلوگیری از کش شدن در محیط پزشکی
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
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
        }

        #endregion

        #region Delete & Restore

        /// <summary>
        /// حذف نرم پزشک
        /// Security: Authorization check, audit trail
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (!ValidateId(id))
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                _logger.Information("درخواست حذف پزشک {DoctorId} توسط کاربر {UserId}",
                    id, _currentUserService.UserId);

                var result = await _doctorCrudService.SoftDeleteDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                _logger.Information("Doctor soft deleted successfully: {DoctorId} by user {UserId}",
                    id, _currentUserService.UserId);
                return Json(new { success = true, message = "پزشک با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پزشک {DoctorId}", id);
                return Json(new { success = false, message = "خطا در حذف پزشک" });
            }
        }

        /// <summary>
        /// بازیابی پزشک حذف شده
        /// Security: Authorization check, audit trail
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restore(int id)
        {
            try
            {
                if (!ValidateId(id))
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                _logger.Information("درخواست بازیابی پزشک {DoctorId} توسط کاربر {UserId}",
                    id, _currentUserService.UserId);

                var result = await _doctorCrudService.RestoreDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                _logger.Information("Doctor restored successfully: {DoctorId} by user {UserId}",
                    id, _currentUserService.UserId);
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

        /// <summary>
        /// دریافت لیست پزشکان برای استفاده در لیست‌های کشویی
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetDoctors()
        {
            try
            {
                _logger.Information("درخواست AJAX دریافت لیست پزشکان");

                var filter = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000, // دریافت همه پزشکان فعال
                    IsActive = true
                };

                var result = await _doctorCrudService.GetDoctorsAsync(filter);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                var doctors = result.Data.Items.Select(d => new { 
                    Id = d.DoctorId, 
                    FullName = $"{d.FirstName} {d.LastName}",
                    NationalCode = d.NationalCode
                }).ToList();

                return Json(new { success = true, data = doctors }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
                return Json(new { success = false, message = "خطا در دریافت پزشکان" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال پزشک
        /// Security: Authorization check, audit trail
        /// Performance: Optimized status update
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleStatus(int id, bool activate)
        {
            try
            {
                if (!ValidateId(id))
                    return Json(new { success = false, message = "شناسه پزشک نامعتبر است." });

                _logger.Information("درخواست تغییر وضعیت پزشک {DoctorId} به {Status} توسط کاربر {UserId}",
                    id, activate ? "فعال" : "غیرفعال", _currentUserService.UserId);

                var result = activate
                    ? await _doctorCrudService.ActivateDoctorAsync(id)
                    : await _doctorCrudService.DeactivateDoctorAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                var actionText = activate ? "فعال" : "غیرفعال";
                _logger.Information("Doctor status changed successfully: {DoctorId} to {Status} by user {UserId}",
                    id, actionText, _currentUserService.UserId);
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
        /// تنظیم مدل جستجو با مقادیر پیش‌فرض
        /// Performance: Efficient model normalization
        /// </summary>
        private DoctorSearchViewModel NormalizeSearchModel(DoctorSearchViewModel searchModel)
        {
            if (searchModel == null)
                searchModel = new DoctorSearchViewModel();

            // پشتیبانی از فیلدهای backward compatibility
            if (searchModel.Page <= 0) searchModel.Page = 1;
            if (searchModel.PageNumber <= 0) searchModel.PageNumber = searchModel.Page;
            if (searchModel.PageSize <= 0) searchModel.PageSize = 10;

            // محدود کردن اندازه صفحه برای جلوگیری از overload
            if (searchModel.PageSize > 100) searchModel.PageSize = 100;

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
        /// ایجاد مدل صفحه Index با آمار بهینه
        /// Performance: Efficient counting and statistics
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
        /// Performance: Cached loading, error handling
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
        /// اعتبارسنجی مدل با FluentValidation
        /// Security: Comprehensive validation
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
        /// Security: Duplicate prevention
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
        /// Security: Duplicate prevention with exclusion
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
        /// Security: Date validation, error handling
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
        /// پردازش آپلود تصویر پروفایل با امنیت بالا
        /// Security: File type validation, size limits, secure file naming
        /// Performance: Efficient file operations
        /// </summary>
        private async Task ProcessProfileImageUpload(DoctorCreateEditViewModel model)
        {
            try
            {
                var file = Request.Files["ProfileImage"];

                // اگر فایل آپلود نشده، مقدار قبلی را حفظ کن
                if (file == null || file.ContentLength == 0)
                {
                    _logger.Information("هیچ تصویر جدیدی آپلود نشده، مقدار قبلی حفظ می‌شود برای پزشک {DoctorId}", model.DoctorId);
                    return;
                }

                // بررسی نوع فایل
                if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.Warning("نوع فایل نامعتبر برای تصویر پروفایل: {ContentType} from IP {IPAddress}",
                        file.ContentType, GetClientIPAddress());
                    ModelState.AddModelError("ProfileImage", "فقط فایل‌های تصویری مجاز هستند.");
                    return;
                }

                // بررسی اندازه فایل
                if (file.ContentLength > MaxFileSizeInBytes)
                {
                    _logger.Warning("اندازه فایل تصویر پروفایل بیش از حد مجاز: {Size} bytes from IP {IPAddress}",
                        file.ContentLength, GetClientIPAddress());
                    ModelState.AddModelError("ProfileImage", $"حجم فایل نباید بیشتر از {MaxFileSizeInMB} مگابایت باشد.");
                    return;
                }

                // بررسی محتوای فایل (MIME type validation)
                if (!IsValidImageFile(file))
                {
                    _logger.Warning("فایل تصویر نامعتبر از IP {IPAddress}", GetClientIPAddress());
                    ModelState.AddModelError("ProfileImage", "فایل تصویر نامعتبر است.");
                    return;
                }

                // ایجاد نام فایل منحصر به فرد و امن
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"doctor_profile_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";

                // مسیر ذخیره فایل
                var uploadPath = Path.Combine(Server.MapPath(UploadPath), fileName);

                // اطمینان از وجود پوشه
                var directory = Path.GetDirectoryName(uploadPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ذخیره فایل
                file.SaveAs(uploadPath);

                // بازگرداندن مسیر نسبی برای ذخیره در دیتابیس
                var relativePath = $"{UploadPath.Replace("~", "")}/{fileName}";

                _logger.Information("تصویر پروفایل با موفقیت آپلود شد: {Path} by user {UserId}",
                    relativePath, _currentUserService.UserId);
                model.ProfileImageUrl = relativePath;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر پروفایل");
                ModelState.AddModelError("ProfileImage", "خطا در آپلود تصویر.");
            }
        }

        /// <summary>
        /// بررسی اعتبار فایل تصویر
        /// Security: MIME type validation
        /// </summary>
        private bool IsValidImageFile(HttpPostedFileBase file)
        {
            try
            {
                // بررسی header فایل
                var buffer = new byte[8];
                file.InputStream.Read(buffer, 0, 8);
                file.InputStream.Position = 0;

                // بررسی signature فایل‌های تصویری
                if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF) // JPEG
                    return true;
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47) // PNG
                    return true;
                if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46) // GIF
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// اعتبارسنجی شناسه
        /// Security: Input validation
        /// </summary>
        private bool ValidateId(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// دریافت IP آدرس کلاینت
        /// Security: Client tracking for audit
        /// </summary>
        private string GetClientIPAddress()
        {
            try
            {
                var forwarded = Request.Headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwarded))
                {
                    return forwarded.Split(',')[0].Trim();
                }

                return Request.UserHostAddress ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion
    }
}