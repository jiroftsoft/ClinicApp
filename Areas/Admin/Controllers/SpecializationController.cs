using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.ViewModels.SpecializationManagementVM;
using FluentValidation;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت تخصص‌ها در سیستم کلینیک شفا
    /// مسئولیت: عملیات CRUD تخصص‌ها (ایجاد، ویرایش، حذف، مشاهده)
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicManager")]
    public class SpecializationController : Controller
    {
        private readonly ISpecializationService _specializationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<SpecializationCreateEditViewModel> _createEditValidator;
        private readonly ILogger _logger;

        public SpecializationController(
            ISpecializationService specializationService,
            ICurrentUserService currentUserService,
            IValidator<SpecializationCreateEditViewModel> createEditValidator
            )
        {
            _specializationService = specializationService ?? throw new ArgumentNullException(nameof(specializationService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _createEditValidator = createEditValidator ?? throw new ArgumentNullException(nameof(createEditValidator));
            _logger = Log.ForContext<SpecializationController>();
        }

        #region Index & Listing

        [HttpGet]
        public async Task<ActionResult> Index(SpecializationSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست تخصص‌ها توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                    searchModel = new SpecializationSearchViewModel();

                if (searchModel.PageNumber <= 0) searchModel.PageNumber = 1;
                if (searchModel.PageSize <= 0) searchModel.PageSize = 10;

                // دریافت تخصص‌ها
                var result = await _specializationService.GetAllSpecializationsAsync();

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست تخصص‌ها: {ErrorMessage}", result.Message);
                    TempData["Error"] = result.Message;
                    return View(new SpecializationIndexPageViewModel());
                }

                // اعمال فیلترها
                var filteredSpecializations = ApplyFilters(result.Data, searchModel);

                // مرتب‌سازی
                var sortedSpecializations = ApplySorting(filteredSpecializations, searchModel);

                // صفحه‌بندی
                var totalCount = sortedSpecializations.Count;
                var pagedSpecializations = sortedSpecializations
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToList();

                // تبدیل به ViewModel
                var specializationViewModels = pagedSpecializations.Select(SpecializationIndexViewModel.FromEntity).ToList();

                // ایجاد PagedResult
                var pagedResult = new PagedResult<SpecializationIndexViewModel>(
                    specializationViewModels,
                    totalCount,
                    searchModel.PageNumber,
                    searchModel.PageSize
                );

                // ایجاد آمار
                var statistics = new SpecializationStatisticsViewModel
                {
                    TotalCount = result.Data.Count,
                    ActiveCount = result.Data.Count(s => s.IsActive && !s.IsDeleted),
                    InactiveCount = result.Data.Count(s => !s.IsActive && !s.IsDeleted),
                    DeletedCount = result.Data.Count(s => s.IsDeleted),
                    TodayCount = result.Data.Count(s => s.CreatedAt.Date == DateTime.Today)
                };

                // ایجاد SpecializationIndexPageViewModel
                var pageViewModel = new SpecializationIndexPageViewModel
                {
                    Specializations = specializationViewModels,
                    SearchModel = searchModel,
                    Statistics = statistics,
                    PagedResult = pagedResult
                };

                return View(pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تخصص‌ها");
                TempData["Error"] = "خطا در بارگذاری لیست تخصص‌ها";
                return View(new SpecializationIndexPageViewModel());
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public ActionResult Create()
        {
            try
            {
                var createModel = new SpecializationCreateEditViewModel
                {
                    IsActive = true,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.Now
                };

                return View(createModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد تخصص");
                TempData["Error"] = "خطا در بارگذاری فرم ایجاد تخصص";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SpecializationCreateEditViewModel model)
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

                // تبدیل به Entity
                var specialization = model.ToEntity();

                var result = await _specializationService.CreateSpecializationAsync(specialization);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "تخصص جدید با موفقیت ایجاد شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تخصص جدید");
                TempData["Error"] = "خطا در ایجاد تخصص جدید";
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
                    TempData["Error"] = "شناسه تخصص نامعتبر است.";
                    return RedirectToAction("Index");
                }

                var result = await _specializationService.GetSpecializationByIdAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "تخصص مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var editModel = SpecializationCreateEditViewModel.FromEntity(result.Data);

                return View(editModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش تخصص {SpecializationId}", id);
                TempData["Error"] = "خطا در بارگذاری فرم ویرایش تخصص";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SpecializationCreateEditViewModel model)
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

                // تبدیل به Entity
                var specialization = model.ToEntity();

                var result = await _specializationService.UpdateSpecializationAsync(specialization);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }

                TempData["Success"] = "اطلاعات تخصص با موفقیت به‌روزرسانی شد.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش تخصص {SpecializationId}", model.SpecializationId);
                TempData["Error"] = "خطا در ویرایش تخصص";
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
                    TempData["Error"] = "شناسه تخصص نامعتبر است.";
                    return RedirectToAction("Index");
                }

                var result = await _specializationService.GetSpecializationByIdAsync(id);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "تخصص مورد نظر یافت نشد.";
                    return RedirectToAction("Index");
                }

                var detailsModel = SpecializationDetailsViewModel.FromEntity(result.Data);

                return View(detailsModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات تخصص {SpecializationId}", id);
                TempData["Error"] = "خطا در بارگذاری جزئیات تخصص";
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
                    return Json(new { success = false, message = "شناسه تخصص نامعتبر است." });

                var result = await _specializationService.SoftDeleteSpecializationAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = "تخصص با موفقیت حذف شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تخصص {SpecializationId}", id);
                return Json(new { success = false, message = "خطا در حذف تخصص" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Restore(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "شناسه تخصص نامعتبر است." });

                var result = await _specializationService.RestoreSpecializationAsync(id);

                if (!result.Success)
                    return Json(new { success = false, message = result.Message });

                return Json(new { success = true, message = "تخصص با موفقیت بازیابی شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی تخصص {SpecializationId}", id);
                return Json(new { success = false, message = "خطا در بازیابی تخصص" });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// اعمال فیلترها بر روی لیست تخصص‌ها
        /// </summary>
        private List<Specialization> ApplyFilters(List<Specialization> specializations, SpecializationSearchViewModel searchModel)
        {
            var filtered = specializations.AsQueryable();

            // فیلتر بر اساس عبارت جستجو
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.Trim();
                filtered = filtered.Where(s => 
                    s.Name.Contains(searchTerm) || 
                    s.Description.Contains(searchTerm));
            }

            // فیلتر بر اساس وضعیت فعال/غیرفعال
            if (searchModel.IsActive.HasValue)
            {
                filtered = filtered.Where(s => s.IsActive == searchModel.IsActive.Value);
            }

            // فیلتر بر اساس تعداد پزشکان
            if (searchModel.MinDoctorCount.HasValue)
            {
                // این فیلتر نیاز به پیاده‌سازی جداگانه دارد
                // filtered = filtered.Where(s => s.DoctorSpecializations.Count >= searchModel.MinDoctorCount.Value);
            }

            // فیلتر بر اساس تاریخ ایجاد
            if (searchModel.CreatedFrom.HasValue)
            {
                filtered = filtered.Where(s => s.CreatedAt >= searchModel.CreatedFrom.Value);
            }

            if (searchModel.CreatedTo.HasValue)
            {
                filtered = filtered.Where(s => s.CreatedAt <= searchModel.CreatedTo.Value);
            }

            // فیلتر بر اساس حذف شده‌ها
            if (!searchModel.IncludeDeleted)
            {
                filtered = filtered.Where(s => !s.IsDeleted);
            }

            return filtered.ToList();
        }

        /// <summary>
        /// اعمال مرتب‌سازی بر روی لیست تخصص‌ها
        /// </summary>
        private List<Specialization> ApplySorting(List<Specialization> specializations, SpecializationSearchViewModel searchModel)
        {
            var sorted = specializations.AsQueryable();

            switch (searchModel.SortBy?.ToLower())
            {
                case "name":
                    sorted = searchModel.SortOrder == "desc" 
                        ? sorted.OrderByDescending(s => s.Name)
                        : sorted.OrderBy(s => s.Name);
                    break;
                case "displayorder":
                    sorted = searchModel.SortOrder == "desc" 
                        ? sorted.OrderByDescending(s => s.DisplayOrder)
                        : sorted.OrderBy(s => s.DisplayOrder);
                    break;
                case "createdat":
                    sorted = searchModel.SortOrder == "desc" 
                        ? sorted.OrderByDescending(s => s.CreatedAt)
                        : sorted.OrderBy(s => s.CreatedAt);
                    break;
                case "doctorcount":
                    // این مرتب‌سازی نیاز به پیاده‌سازی جداگانه دارد
                    // sorted = searchModel.SortOrder == "desc" 
                    //     ? sorted.OrderByDescending(s => s.DoctorSpecializations.Count)
                    //     : sorted.OrderBy(s => s.DoctorSpecializations.Count);
                    break;
                default:
                    sorted = sorted.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name);
                    break;
            }

            return sorted.ToList();
        }

        #endregion
    }
}
