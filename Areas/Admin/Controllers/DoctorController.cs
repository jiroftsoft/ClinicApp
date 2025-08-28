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

                // بارگذاری تخصص‌های فعال برای فیلتر
                var specializationsResult = await _specializationService.GetActiveSpecializationsAsync();
                if (specializationsResult.Success)
                {
                    ViewBag.Specializations = specializationsResult.Data;
                }
                else
                {
                    _logger.Warning("خطا در بارگذاری تخصص‌ها: {Error}", specializationsResult.Message);
                    ViewBag.Specializations = new System.Collections.Generic.List<ClinicApp.Models.Entities.Specialization>();
                }

                var result = await _doctorCrudService.GetDoctorsAsync(searchModel);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست پزشکان: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new DoctorIndexPageViewModel());
                }

                // ایجاد DoctorIndexPageViewModel
                var pageViewModel = new DoctorIndexPageViewModel
                {
                    Doctors = result.Data,
                    SearchModel = searchModel,
                    TotalCount = result.Data.TotalItems,
                    ActiveCount = result.Data.Items?.Count(d => d.IsActive) ?? 0,
                    InactiveCount = result.Data.Items?.Count(d => !d.IsActive) ?? 0,
                    TodayCount = result.Data.Items?.Count(d => d.CreatedAt.Date == DateTime.Today) ?? 0
                };

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

                // بارگذاری تخصص‌های فعال
                var specializationsResult = await _specializationService.GetActiveSpecializationsAsync();
                if (specializationsResult.Success)
                {
                    ViewBag.Specializations = specializationsResult.Data;
                }
                else
                {
                    _logger.Warning("خطا در بارگذاری تخصص‌ها: {Error}", specializationsResult.Message);
                    ViewBag.Specializations = new List<Models.Entities.Specialization>();
                }

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
                // بارگذاری تخصص‌ها برای نمایش مجدد در صورت خطا
                await LoadSpecializationsForView();

                if (!ModelState.IsValid)
                {
                    _logger.Warning("ModelState validation failed for doctor creation: {@Errors}", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(model);
                }

                var validationResult = await _createEditValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        _logger.Warning("Validation error for {Property}: {Error}", error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                // بررسی تکراری نبودن کد ملی
                if (!string.IsNullOrEmpty(model.NationalCode))
                {
                    var existingDoctor = await _doctorCrudService.GetDoctorByNationalCodeAsync(model.NationalCode);
                    if (existingDoctor.Success && existingDoctor.Data != null)
                    {
                        ModelState.AddModelError("NationalCode", "پزشکی با این کد ملی قبلاً ثبت شده است.");
                        return View(model);
                    }
                }

                // بررسی تکراری نبودن شماره نظام پزشکی
                if (!string.IsNullOrEmpty(model.MedicalCouncilCode))
                {
                    var existingDoctor = await _doctorCrudService.GetDoctorByMedicalCouncilCodeAsync(model.MedicalCouncilCode);
                    if (existingDoctor.Success && existingDoctor.Data != null)
                    {
                        ModelState.AddModelError("MedicalCouncilCode", "پزشکی با این شماره نظام پزشکی قبلاً ثبت شده است.");
                        return View(model);
                    }
                }

                // تبدیل تاریخ شمسی به میلادی
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
                        return View(model);
                    }
                }

                // پردازش آپلود تصویر پروفایل
                var profileImageUrl = await ProcessProfileImageUpload();
                if (!string.IsNullOrEmpty(profileImageUrl))
                {
                    model.ProfileImageUrl = profileImageUrl;
                }

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



        /// <summary>
        /// پیش‌نمایش اطلاعات پزشک
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Preview(DoctorCreateEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "لطفاً خطاهای فرم را برطرف کنید" });
                }

                var validationResult = await _createEditValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // بارگذاری نام تخصص‌ها
                var specializations = new List<string>();
                if (model.SelectedSpecializationIds != null && model.SelectedSpecializationIds.Any())
                {
                    var specializationsResult = await _specializationService.GetSpecializationsByIdsAsync(model.SelectedSpecializationIds);
                    if (specializationsResult.Success)
                    {
                        specializations = specializationsResult.Data.Select(s => s.Name).ToList();
                    }
                }

                var previewHtml = GeneratePreviewHtml(model, specializations);
                return Json(new { success = true, data = previewHtml });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پیش‌نمایش پزشک");
                return Json(new { success = false, message = "خطا در پیش‌نمایش" });
            }
        }

        /// <summary>
        /// تولید HTML پیش‌نمایش
        /// </summary>
        private string GeneratePreviewHtml(DoctorCreateEditViewModel model, List<string> specializations)
        {
            return $@"
                <div class='row'>
                    <div class='col-md-6'>
                        <h6><i class='fas fa-user text-primary'></i> اطلاعات پایه</h6>
                        <table class='table table-sm'>
                            <tr><td><strong>نام:</strong></td><td>{model.FirstName} {model.LastName}</td></tr>
                            <tr><td><strong>کد ملی:</strong></td><td>{model.NationalCode}</td></tr>
                            <tr><td><strong>شماره نظام پزشکی:</strong></td><td>{model.MedicalCouncilCode}</td></tr>
                            <tr><td><strong>جنسیت:</strong></td><td>{(model.Gender == Gender.Male ? "مرد" : "زن")}</td></tr>
                            <tr><td><strong>تلفن:</strong></td><td>{model.PhoneNumber}</td></tr>
                            <tr><td><strong>ایمیل:</strong></td><td>{model.Email ?? "-"}</td></tr>
                        </table>
                    </div>
                    <div class='col-md-6'>
                        <h6><i class='fas fa-stethoscope text-primary'></i> اطلاعات حرفه‌ای</h6>
                        <table class='table table-sm'>
                            <tr><td><strong>مدرک:</strong></td><td>{GetDegreeDisplayName(model.Degree)}</td></tr>
                            <tr><td><strong>سال فارغ‌التحصیلی:</strong></td><td>{(model.GraduationYear.HasValue ? model.GraduationYear.Value.ToString() : "-")}</td></tr>
                            <tr><td><strong>دانشگاه:</strong></td><td>{model.University ?? "-"}</td></tr>
                            <tr><td><strong>تجربه:</strong></td><td>{(model.ExperienceYears.HasValue ? model.ExperienceYears.Value.ToString() : "0")} سال</td></tr>
                            <tr><td><strong>تخصص‌ها:</strong></td><td>{string.Join(", ", specializations)}</td></tr>
                            <tr><td><strong>وضعیت:</strong></td><td>{(model.IsActive ? "فعال" : "غیرفعال")}</td></tr>
                        </table>
                    </div>
                </div>
                <div class='row mt-3'>
                    <div class='col-12'>
                        <h6><i class='fas fa-address-book text-primary'></i> آدرس‌ها</h6>
                        <p><strong>آدرس منزل:</strong> {model.HomeAddress ?? "-"}</p>
                        <p><strong>آدرس مطب:</strong> {model.OfficeAddress ?? "-"}</p>
                    </div>
                </div>
                {(string.IsNullOrEmpty(model.Bio) ? "" : $@"
                <div class='row mt-3'>
                    <div class='col-12'>
                        <h6><i class='fas fa-info-circle text-primary'></i> بیوگرافی</h6>
                        <p>{model.Bio}</p>
                    </div>
                </div>")}";
        }

        /// <summary>
        /// دریافت نام نمایشی مدرک تحصیلی
        /// </summary>
        private string GetDegreeDisplayName(Degree degree)
        {
            switch (degree)
            {
                case Degree.GeneralPhysician: return "پزشک عمومی";
                case Degree.Specialist: return "متخصص";
                case Degree.SubSpecialist: return "فوق تخصص";
                case Degree.Dentist: return "دندانپزشک";
                case Degree.Pharmacist: return "داروساز";
                default: return "نامشخص";
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

                // بارگذاری تخصص‌های فعال
                var specializationsResult = await _specializationService.GetActiveSpecializationsAsync();
                if (specializationsResult.Success)
                {
                    ViewBag.Specializations = specializationsResult.Data;
                }
                else
                {
                    _logger.Warning("خطا در بارگذاری تخصص‌ها: {Error}", specializationsResult.Message);
                    ViewBag.Specializations = new System.Collections.Generic.List<ClinicApp.Models.Entities.Specialization>();
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
                // بارگذاری تخصص‌ها برای نمایش مجدد در صورت خطا
                await LoadSpecializationsForView();

                if (!ModelState.IsValid)
                {
                    _logger.Warning("ModelState validation failed for doctor edit: {@Errors}", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return View(model);
                }

                var validationResult = await _createEditValidator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        _logger.Warning("Validation error for {Property}: {Error}", error.PropertyName, error.ErrorMessage);
                    }
                    return View(model);
                }

                // بررسی تکراری نبودن شماره نظام پزشکی (به جز خودش)
                if (!string.IsNullOrEmpty(model.MedicalCouncilCode))
                {
                    var existingDoctor = await _doctorCrudService.GetDoctorByMedicalCouncilCodeAsync(model.MedicalCouncilCode);
                    if (existingDoctor.Success && existingDoctor.Data != null && existingDoctor.Data.DoctorId != model.DoctorId)
                    {
                        ModelState.AddModelError("MedicalCouncilCode", "پزشکی با این شماره نظام پزشکی قبلاً ثبت شده است.");
                        return View(model);
                    }
                }

                // تبدیل تاریخ شمسی به میلادی
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
                        return View(model);
                    }
                }

                // پردازش آپلود تصویر پروفایل
                var profileImageUrl = await ProcessProfileImageUpload();
                if (!string.IsNullOrEmpty(profileImageUrl))
                {
                    model.ProfileImageUrl = profileImageUrl;
                }

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

        #region Helper Methods

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
                    ViewBag.Specializations = new System.Collections.Generic.List<ClinicApp.Models.Entities.Specialization>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بارگذاری تخصص‌ها");
                ViewBag.Specializations = new System.Collections.Generic.List<ClinicApp.Models.Entities.Specialization>();
            }
        }

        /// <summary>
        /// پردازش آپلود تصویر پروفایل
        /// </summary>
        private async Task<string> ProcessProfileImageUpload()
        {
            try
            {
                var file = Request.Files["ProfileImage"];
                if (file == null || file.ContentLength == 0)
                    return null;

                // بررسی نوع فایل
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    _logger.Warning("نوع فایل نامعتبر برای تصویر پروفایل: {ContentType}", file.ContentType);
                    return null;
                }

                // بررسی اندازه فایل (حداکثر 2MB)
                if (file.ContentLength > 2 * 1024 * 1024)
                {
                    _logger.Warning("اندازه فایل تصویر پروفایل بیش از حد مجاز: {Size} bytes", file.ContentLength);
                    return null;
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
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پردازش آپلود تصویر پروفایل");
                return null;
            }
        }

        #endregion
    }
}

