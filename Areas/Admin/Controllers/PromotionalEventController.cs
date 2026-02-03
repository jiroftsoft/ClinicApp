using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Interfaces.PromotionalEvent;
using ClinicApp.ViewModels.PromotionalEventVM;
using ClinicApp.ViewModels.DoctorManagementVM;
using Serilog;
using MvcSelectListItem = System.Web.Mvc.SelectListItem; // ✅ برای رفع ambiguity

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت ایونت‌های تبلیغاتی
    /// طراحی شده بر اساس اصول SRP و Strongly-Typed
    /// </summary>
    //[Authorize(Roles = "Admin,ClinicAdmin")]
    public class PromotionalEventController : Controller
    {
        private readonly IPromotionalEventService _promotionalEventService;
        private readonly IDoctorCrudService _doctorCrudService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PromotionalEventController(
            IPromotionalEventService promotionalEventService,
            IDoctorCrudService doctorCrudService,
            ICurrentUserService currentUserService)
        {
            _promotionalEventService = promotionalEventService ?? throw new ArgumentNullException(nameof(promotionalEventService));
            _doctorCrudService = doctorCrudService ?? throw new ArgumentNullException(nameof(doctorCrudService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<PromotionalEventController>();
        }

        /// <summary>
        /// Helper method برای برگرداندن View path صحیح
        /// طبق قراردادها و استانداردها - جلوگیری از routing به CMS
        /// </summary>
        /// <param name="viewName">نام View (مثلاً "Index", "Create", "Edit")</param>
        /// <returns>مسیر کامل View</returns>
        protected string GetViewPath(string viewName)
        {
            return $"~/Areas/Admin/Views/PromotionalEvent/{viewName}.cshtml";
        }

        #region Index & Listing

        [HttpGet]
        public async Task<ActionResult> Index(PromotionalEventSearchViewModel searchModel)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست نمایش لیست ایونت‌ها توسط کاربر {UserId}", _currentUserService.UserId);

                if (searchModel == null)
                {
                    searchModel = new PromotionalEventSearchViewModel();
                }

                // Parse تاریخ‌های جستجو از hidden inputs
                // ✅ ENTERPRISE-GRADE: Parse تاریخ‌های جستجو از hidden inputs
                searchModel.FromDate = this.ParseDateFromHiddenInput("FromDate", _logger);
                searchModel.ToDate = this.ParseDateFromHiddenInput("ToDate", _logger);
                
                // ✅ ENTERPRISE-GRADE: تبدیل به UTC برای مقایسه با دیتابیس
                if (searchModel.FromDate.HasValue)
                {
                    searchModel.FromDate = searchModel.FromDate.Value.ToUniversalTime();
                }
                if (searchModel.ToDate.HasValue)
                {
                    searchModel.ToDate = searchModel.ToDate.Value.ToUniversalTime();
                }

                var result = await _promotionalEventService.GetAllAsync(includeDeleted: false);

                if (!result.Success)
                {
                    _logger.Warning("⚠️ PROMOTIONAL EVENT: خطا در دریافت لیست ایونت‌ها: {ErrorMessage}", result.Message);
                    NotificationHelper.SetError(TempData, result.Message);
                    var emptyPageViewModel = new PromotionalEventIndexPageViewModel
                    {
                        Events = new List<PromotionalEventIndexViewModel>(),
                        SearchModel = searchModel,
                        TotalCount = 0,
                        ActiveCount = 0,
                        InactiveCount = 0
                    };
                    return View(GetViewPath("Index"), emptyPageViewModel);
                }

                // فیلتر کردن بر اساس searchModel
                var filteredEvents = result.Data.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
                {
                    var search = searchModel.SearchTerm.Trim();
                    filteredEvents = filteredEvents.Where(e => 
                        e.Title.Contains(search) || 
                        (e.Description != null && e.Description.Contains(search)));
                }

                if (searchModel.IsActive.HasValue)
                {
                    filteredEvents = filteredEvents.Where(e => e.IsActive == searchModel.IsActive.Value);
                }

                if (searchModel.FromDate.HasValue)
                {
                    filteredEvents = filteredEvents.Where(e => e.StartDate >= searchModel.FromDate.Value);
                }

                if (searchModel.ToDate.HasValue)
                {
                    filteredEvents = filteredEvents.Where(e => e.EndDate <= searchModel.ToDate.Value);
                }

                // تبدیل IQueryable به List برای استفاده در متد helper
                var filteredEventsList = filteredEvents
                    .OrderByDescending(e => e.CreatedAt)
                    .ToList();

                // تبدیل به IndexViewModel
                var indexViewModels = filteredEventsList
                    .Select(e => PromotionalEventViewModelFactory.ToIndexViewModel(e))
                    .Where(vm => vm != null)
                    .ToList();

                // بهینه‌سازی: دریافت نام پزشکان در یک بار
                await PopulateDoctorNamesForIndexAsync(filteredEventsList, indexViewModels);

                var totalCount = indexViewModels.Count;
                var activeCount = indexViewModels.Count(e => e.IsActive);
                var inactiveCount = totalCount - activeCount;

                var pageViewModel = new PromotionalEventIndexPageViewModel
                {
                    Events = indexViewModels,
                    SearchModel = searchModel,
                    TotalCount = totalCount,
                    ActiveCount = activeCount,
                    InactiveCount = inactiveCount
                };

                return View(GetViewPath("Index"), pageViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در نمایش لیست ایونت‌ها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست ایونت‌ها");
                var emptyPageViewModel = new PromotionalEventIndexPageViewModel
                {
                    Events = new List<PromotionalEventIndexViewModel>(),
                    SearchModel = searchModel ?? new PromotionalEventSearchViewModel(),
                    TotalCount = 0,
                    ActiveCount = 0,
                    InactiveCount = 0
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
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست نمایش جزئیات ایونت - EventId: {EventId}", id);

                var result = await _promotionalEventService.GetByIdAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }

                var detailsViewModel = PromotionalEventViewModelFactory.ToDetailsViewModel(result.Data);

                // پر کردن DoctorNames از Service
                if (detailsViewModel.IsDoctorSpecific && detailsViewModel.DoctorIds.Any())
                {
                    detailsViewModel.DoctorNames = await GetDoctorNamesAsync(detailsViewModel.DoctorIds);
                }
                else
                {
                    detailsViewModel.DoctorNames = new List<string> { "همه پزشکان" };
                }

                return View(GetViewPath("Details"), detailsViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در نمایش جزئیات ایونت - EventId: {EventId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات ایونت");
                return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
            }
        }

        #endregion

        #region Create

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست نمایش فرم ایجاد ایونت");

                var model = PromotionalEventViewModelFactory.CreateEmpty();

                // دریافت لیست پزشکان برای Multi-Select
                await PopulateAvailableDoctors(model);

                return View(GetViewPath("Create"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در نمایش فرم ایجاد ایونت");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم");
                return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PromotionalEventCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست ایجاد ایونت جدید - Title: {Title}", model?.Title);

                // ✅ ENTERPRISE-GRADE: Parse تاریخ‌ها از hidden inputs (تبدیل شمسی → میلادی)
                var parsedStartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                if (parsedStartDate.HasValue)
                {
                    model.StartDate = parsedStartDate.Value.ToUniversalTime();
                    _logger.Debug("🔄 ENTERPRISE DATE: StartDate converted to UTC - {StartDate}", model.StartDate);
                }
                
                var parsedEndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
                if (parsedEndDate.HasValue)
                {
                    model.EndDate = parsedEndDate.Value.ToUniversalTime();
                    _logger.Debug("🔄 ENTERPRISE DATE: EndDate converted to UTC - {EndDate}", model.EndDate);
                }

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Create"), model);
                }

                // تبدیل ViewModel به Entity
                var entity = PromotionalEventViewModelFactory.ToEntity(model);
                if (entity == null)
                {
                    NotificationHelper.SetError(TempData, "خطا در تبدیل اطلاعات");
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Create"), model);
                }

                var result = await _promotionalEventService.CreateAsync(entity);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Create"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در ایجاد ایونت");
                NotificationHelper.SetError(TempData, "خطا در ایجاد ایونت");
                await PopulateAvailableDoctors(model);
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
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست نمایش فرم ویرایش ایونت - EventId: {EventId}", id);

                var result = await _promotionalEventService.GetByIdAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }

                var model = PromotionalEventViewModelFactory.FromEntity(result.Data);

                // دریافت لیست پزشکان برای Multi-Select
                await PopulateAvailableDoctors(model);

                return View(GetViewPath("Edit"), model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در نمایش فرم ویرایش ایونت - EventId: {EventId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات ایونت");
                return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, PromotionalEventCreateEditViewModel model)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست ویرایش ایونت - EventId: {EventId}, Title: {Title}", id, model?.Title);

                if (id != model.EventId)
                {
                    NotificationHelper.SetError(TempData, "شناسه ایونت نامعتبر است");
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }

                // ✅ ENTERPRISE-GRADE: Parse تاریخ‌ها از hidden inputs (تبدیل شمسی → میلادی)
                var parsedStartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                if (parsedStartDate.HasValue)
                {
                    model.StartDate = parsedStartDate.Value.ToUniversalTime();
                    _logger.Debug("🔄 ENTERPRISE DATE: StartDate converted to UTC - {StartDate}", model.StartDate);
                }
                
                var parsedEndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
                if (parsedEndDate.HasValue)
                {
                    model.EndDate = parsedEndDate.Value.ToUniversalTime();
                    _logger.Debug("🔄 ENTERPRISE DATE: EndDate converted to UTC - {EndDate}", model.EndDate);
                }

                if (!ModelState.IsValid)
                {
                    NotificationHelper.SetError(TempData, "لطفاً تمام فیلدهای الزامی را پر کنید.");
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Edit"), model);
                }

                // تبدیل ViewModel به Entity
                var entity = PromotionalEventViewModelFactory.ToEntity(model);
                if (entity == null)
                {
                    NotificationHelper.SetError(TempData, "خطا در تبدیل اطلاعات");
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Edit"), model);
                }

                var result = await _promotionalEventService.UpdateAsync(id, entity);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    await PopulateAvailableDoctors(model);
                    return View(GetViewPath("Edit"), model);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در ویرایش ایونت - EventId: {EventId}", id);
                NotificationHelper.SetError(TempData, "خطا در ویرایش ایونت");
                await PopulateAvailableDoctors(model);
                return View(GetViewPath("Edit"), model);
            }
        }

        #endregion

        #region Delete

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست نمایش فرم حذف ایونت - EventId: {EventId}", id);

                var result = await _promotionalEventService.GetByIdAsync(id);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }

                var detailsViewModel = PromotionalEventViewModelFactory.ToDetailsViewModel(result.Data);
                return View(GetViewPath("Delete"), detailsViewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در نمایش فرم حذف ایونت - EventId: {EventId}", id);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری اطلاعات ایونت");
                return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.Information("🎁 PROMOTIONAL EVENT: درخواست حذف ایونت - EventId: {EventId}", id);

                var result = await _promotionalEventService.DeleteAsync(id);

                if (result.Success)
                {
                    NotificationHelper.SetSuccess(TempData, result.Message);
                    return RedirectToAction("Index", "PromotionalEvent", new { area = "Admin" });
                }
                else
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Delete", "PromotionalEvent", new { area = "Admin", id = id });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در حذف ایونت - EventId: {EventId}", id);
                NotificationHelper.SetError(TempData, "خطا در حذف ایونت");
                return RedirectToAction("Delete", "PromotionalEvent", new { area = "Admin", id = id });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// پر کردن نام پزشکان برای Index ViewModels (بهینه‌سازی شده)
        /// </summary>
        private async Task PopulateDoctorNamesForIndexAsync(
            List<Models.Entities.PromotionalEvent.PromotionalEvent> entities,
            List<PromotionalEventIndexViewModel> viewModels)
        {
            if (viewModels == null || !viewModels.Any())
                return;

            try
            {
                // جمع‌آوری تمام DoctorIds منحصر به فرد از تمام ایونت‌ها
                var allDoctorIds = new HashSet<int>();
                var entityDoctorIdsMap = new Dictionary<int, List<int>>(); // EventId -> DoctorIds

                foreach (var entity in entities)
                {
                    if (entity.IsDoctorSpecific && !string.IsNullOrWhiteSpace(entity.DoctorIds))
                    {
                        var doctorIds = ParseDoctorIdsFromJson(entity.DoctorIds);
                        if (doctorIds.Any())
                        {
                            entityDoctorIdsMap[entity.EventId] = doctorIds;
                            foreach (var id in doctorIds)
                            {
                                allDoctorIds.Add(id);
                            }
                        }
                    }
                }

                // دریافت نام تمام پزشکان در یک بار
                var doctorNamesDict = new Dictionary<int, string>();
                if (allDoctorIds.Any())
                {
                    var searchModel = new DoctorSearchViewModel
                    {
                        PageNumber = 1,
                        PageSize = 1000,
                        IsActive = true
                    };

                    var doctorsResult = await _doctorCrudService.GetDoctorsAsync(searchModel);
                    if (doctorsResult.Success && doctorsResult.Data != null && doctorsResult.Data.Items != null)
                    {
                        foreach (var doctor in doctorsResult.Data.Items)
                        {
                            if (allDoctorIds.Contains(doctor.DoctorId))
                            {
                                doctorNamesDict[doctor.DoctorId] = doctor.FullName ?? $"پزشک #{doctor.DoctorId}";
                            }
                        }
                    }
                }

                // پر کردن DoctorNames برای هر ViewModel
                foreach (var vm in viewModels)
                {
                    if (vm.IsDoctorSpecific && entityDoctorIdsMap.ContainsKey(vm.EventId))
                    {
                        var doctorIds = entityDoctorIdsMap[vm.EventId];
                        if (doctorIds.Any())
                        {
                            var names = doctorIds
                                .Where(id => doctorNamesDict.ContainsKey(id))
                                .Select(id => doctorNamesDict[id])
                                .OrderBy(name => name)
                                .ToList();

                            vm.DoctorNames = names.Any() ? string.Join(", ", names) : "محدود به پزشکان خاص";
                        }
                        else
                        {
                            vm.DoctorNames = "محدود به پزشکان خاص";
                        }
                    }
                    else
                    {
                        vm.DoctorNames = "همه پزشکان";
                    }
                }

                _logger.Debug("✅ PROMOTIONAL EVENT: نام پزشکان برای {Count} ایونت پر شد", viewModels.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در پر کردن نام پزشکان برای Index");
                foreach (var vm in viewModels)
                {
                    if (string.IsNullOrWhiteSpace(vm.DoctorNames))
                    {
                        vm.DoctorNames = vm.IsDoctorSpecific ? "محدود به پزشکان خاص" : "همه پزشکان";
                    }
                }
            }
        }

        /// <summary>
        /// دریافت نام پزشکان بر اساس لیست ID ها
        /// </summary>
        private async Task<List<string>> GetDoctorNamesAsync(List<int> doctorIds)
        {
            if (doctorIds == null || !doctorIds.Any())
                return new List<string>();

            try
            {
                var searchModel = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000,
                    IsActive = true
                };

                var doctorsResult = await _doctorCrudService.GetDoctorsAsync(searchModel);
                if (doctorsResult.Success && doctorsResult.Data != null && doctorsResult.Data.Items != null)
                {
                    var doctorNames = doctorsResult.Data.Items
                        .Where(d => doctorIds.Contains(d.DoctorId))
                        .Select(d => d.FullName ?? $"پزشک #{d.DoctorId}")
                        .OrderBy(name => name)
                        .ToList();

                    _logger.Debug("✅ PROMOTIONAL EVENT: دریافت {Count} نام پزشک از {Total} ID", doctorNames.Count, doctorIds.Count);
                    return doctorNames;
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت نام پزشکان");
                return doctorIds.Select(id => $"پزشک #{id}").ToList();
            }
        }


        /// <summary>
        /// Parse کردن DoctorIds از JSON string
        /// </summary>
        private List<int> ParseDoctorIdsFromJson(string doctorIdsJson)
        {
            if (string.IsNullOrWhiteSpace(doctorIdsJson))
                return new List<int>();

            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(doctorIdsJson) ?? new List<int>();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ PROMOTIONAL EVENT: خطا در Parse کردن DoctorIds JSON: {Json}", doctorIdsJson);
                return new List<int>();
            }
        }

        /// <summary>
        /// پر کردن لیست پزشکان موجود برای Multi-Select
        /// </summary>
        private async Task PopulateAvailableDoctors(PromotionalEventCreateEditViewModel model)
        {
            try
            {
                // دریافت لیست پزشکان با استفاده از GetDoctorsAsync
                var searchModel = new DoctorSearchViewModel
                {
                    PageNumber = 1,
                    PageSize = 1000, // دریافت حداکثر 1000 پزشک
                    IsActive = true // فقط پزشکان فعال
                };

                var doctorsResult = await _doctorCrudService.GetDoctorsAsync(searchModel);
                if (doctorsResult.Success && doctorsResult.Data != null && doctorsResult.Data.Items != null)
                {
                    model.AvailableDoctors = doctorsResult.Data.Items
                        .Select(d => new MvcSelectListItem
                        {
                            Value = d.DoctorId.ToString(),
                            Text = d.FullName ?? $"پزشک #{d.DoctorId}",
                            Selected = model.SelectedDoctorIds != null && model.SelectedDoctorIds.Contains(d.DoctorId)
                        })
                        .OrderBy(d => d.Text)
                        .ToList();
                }
                else
                {
                    model.AvailableDoctors = new List<MvcSelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ PROMOTIONAL EVENT: خطا در دریافت لیست پزشکان");
                model.AvailableDoctors = new List<MvcSelectListItem>();
            }
        }

        #endregion
    }
}

