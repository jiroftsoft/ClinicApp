using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت خدمات - محیط درمانی با اطمینان 100%
    /// Medical Environment Service Management Controller with 100% Reliability
    /// </summary>
    public class ServiceController : Controller
    {
        #region Dependencies and Constructor

        private readonly IServiceManagementService _serviceManagementService;
        private readonly IDepartmentManagementService _departmentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _log;

        public ServiceController(
            IServiceManagementService serviceManagementService,
            IDepartmentManagementService departmentService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _serviceManagementService = serviceManagementService;
            _departmentService = departmentService;
            _currentUserService = currentUserService;
            _log = logger.ForContext<ServiceController>();
        }

        #endregion

        #region Medical Environment Validation Models

        /// <summary>
        /// نتیجه اعتبارسنجی پزشکی
        /// </summary>
        public class MedicalValidationResult
        {
            public bool IsValid { get; set; }
            public List<MedicalValidationError> Errors { get; set; } = new List<MedicalValidationError>();
        }

        /// <summary>
        /// خطای اعتبارسنجی پزشکی
        /// </summary>
        public class MedicalValidationError
        {
            public string Field { get; set; }
            public string Message { get; set; }

            public MedicalValidationError(string field, string message)
            {
                Field = field;
                Message = message;
            }
        }

        #endregion

        #region Medical Environment Validation Methods

        /// <summary>
        /// اعتبارسنجی کامل خدمت برای محیط پزشکی - با اطمینان 100%
        /// </summary>
        private Task<MedicalValidationResult> ValidateServiceForMedicalEnvironment(ServiceCreateEditViewModel model)
        {
            var result = new MedicalValidationResult { IsValid = true, Errors = new List<MedicalValidationError>() };

            try
            {
                _log.Information("🏥 MEDICAL: شروع اعتبارسنجی خدمت. Title: {Title}, Price: {Price}", 
                    model?.Title, model?.Price);

                // 🔒 1. اعتبارسنجی عنوان
                if (string.IsNullOrWhiteSpace(model?.Title))
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان خدمت الزامی است"));
                    result.IsValid = false;
                }
                else if (model.Title.Length > 250)
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان خدمت نمی‌تواند بیشتر از 250 کاراکتر باشد"));
                    result.IsValid = false;
                }
                else if (model.Title.Length < 3)
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان خدمت باید حداقل 3 کاراکتر باشد"));
                    result.IsValid = false;
                }

                // 🔒 2. اعتبارسنجی کد خدمت - Medical Environment (فقط اعداد)
                if (string.IsNullOrWhiteSpace(model?.ServiceCode))
                {
                    result.Errors.Add(new MedicalValidationError("ServiceCode", "کد خدمت الزامی است"));
                    result.IsValid = false;
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(model.ServiceCode.Trim(), @"^\d+$"))
                {
                    result.Errors.Add(new MedicalValidationError("ServiceCode", "کد خدمت باید فقط شامل اعداد باشد"));
                    result.IsValid = false;
                }
                else if (model.ServiceCode.Trim().Length < 3 || model.ServiceCode.Trim().Length > 10)
                {
                    result.Errors.Add(new MedicalValidationError("ServiceCode", "کد خدمت باید بین 3 تا 10 رقم باشد"));
                    result.IsValid = false;
                }

                // 🔒 3. اعتبارسنجی قیمت - با دقت 100%
                if (model.Price <= 0)
                {
                    result.Errors.Add(new MedicalValidationError("Price", "قیمت خدمت باید بزرگتر از صفر باشد"));
                    result.IsValid = false;
                }
                else if (model.Price > 999999999) // حداکثر 999 میلیون تومان
                {
                    result.Errors.Add(new MedicalValidationError("Price", "قیمت خدمت نمی‌تواند بیشتر از 999,999,999 تومان باشد"));
                    result.IsValid = false;
                }
                else if (model.Price % 1000 != 0) // باید مضرب 1000 باشد
                {
                    result.Errors.Add(new MedicalValidationError("Price", "قیمت خدمت باید مضرب 1000 تومان باشد"));
                    result.IsValid = false;
                }

                // 🔒 4. اعتبارسنجی دسته‌بندی
                if (model.ServiceCategoryId <= 0)
                {
                    result.Errors.Add(new MedicalValidationError("ServiceCategoryId", "انتخاب دسته‌بندی الزامی است"));
                    result.IsValid = false;
                }

                // 🔒 5. اعتبارسنجی توضیحات
                if (!string.IsNullOrWhiteSpace(model?.Description) && model.Description.Length > 1000)
                {
                    result.Errors.Add(new MedicalValidationError("Description", "توضیحات خدمت نمی‌تواند بیشتر از 1000 کاراکتر باشد"));
                    result.IsValid = false;
                }

                if (result.IsValid)
                {
                    _log.Information("🏥 MEDICAL: اعتبارسنجی خدمت موفق. Title: {Title}, Price: {Price}", 
                        model.Title, model.Price);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی خدمت ناموفق. تعداد خطاها: {ErrorCount}", 
                        result.Errors.Count);
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی خدمت");
                result.IsValid = false;
                result.Errors.Add(new MedicalValidationError("", "خطا در اعتبارسنجی داده‌ها"));
                return Task.FromResult(result);
            }
        }

        /// <summary>
        /// اعتبارسنجی کامل دسته‌بندی برای محیط پزشکی - با اطمینان 100%
        /// </summary>
        private Task<MedicalValidationResult> ValidateServiceCategoryForMedicalEnvironment(ServiceCategoryCreateEditViewModel model)
        {
            var result = new MedicalValidationResult { IsValid = true, Errors = new List<MedicalValidationError>() };

            try
            {
                _log.Information("🏥 MEDICAL: شروع اعتبارسنجی دسته‌بندی. Title: {Title}, DepartmentId: {DepartmentId}", 
                    model?.Title, model?.DepartmentId);

                // 🔒 1. اعتبارسنجی عنوان
                if (string.IsNullOrWhiteSpace(model?.Title))
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان دسته‌بندی الزامی است"));
                    result.IsValid = false;
                }
                else if (model.Title.Length > 250)
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان دسته‌بندی نمی‌تواند بیشتر از 250 کاراکتر باشد"));
                    result.IsValid = false;
                }
                else if (model.Title.Length < 3)
                {
                    result.Errors.Add(new MedicalValidationError("Title", "عنوان دسته‌بندی باید حداقل 3 کاراکتر باشد"));
                    result.IsValid = false;
                }

                // 🔒 2. اعتبارسنجی دپارتمان
                if (model.DepartmentId <= 0)
                {
                    result.Errors.Add(new MedicalValidationError("DepartmentId", "انتخاب دپارتمان الزامی است"));
                    result.IsValid = false;
                }

                // 🔒 3. اعتبارسنجی توضیحات
                if (!string.IsNullOrWhiteSpace(model?.Description) && model.Description.Length > 1000)
                {
                    result.Errors.Add(new MedicalValidationError("Description", "توضیحات دسته‌بندی نمی‌تواند بیشتر از 1000 کاراکتر باشد"));
                    result.IsValid = false;
                }

                if (result.IsValid)
                {
                    _log.Information("🏥 MEDICAL: اعتبارسنجی دسته‌بندی موفق. Title: {Title}", 
                        model.Title);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی دسته‌بندی ناموفق. تعداد خطاها: {ErrorCount}", 
                        result.Errors.Count);
                }

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در اعتبارسنجی دسته‌بندی");
                result.IsValid = false;
                result.Errors.Add(new MedicalValidationError("", "خطا در اعتبارسنجی داده‌ها"));
                return Task.FromResult(result);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تنظیم ViewBag.Departments برای DropDownList
        /// </summary>
        private async Task SetDepartmentsViewBag(int selectedDepartmentId = 0)
        {
            try
            {
                var departmentsResult = await _departmentService.GetActiveDepartmentsForLookupAsync(1); // فعلاً کلینیک پیش‌فرض
                if (departmentsResult.Success)
                {
                    ViewBag.Departments = new SelectList(departmentsResult.Data, "Id", "Name", selectedDepartmentId);
                }
                else
                {
                    ViewBag.Departments = new SelectList(new List<LookupItemViewModel>(), "Id", "Name");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در تنظیم ViewBag.Departments");
                ViewBag.Departments = new SelectList(new List<LookupItemViewModel>(), "Id", "Name");
            }
        }

        /// <summary>
        /// تنظیم ViewBag.ServiceCategories برای Medical Environment - با اطمینان 100%
        /// </summary>
        private async Task SetServiceCategoriesViewBagForMedicalEnvironment(int departmentId, int selectedCategoryId = 0)
        {
            try
            {
                _log.Information("🏥 MEDICAL: تنظیم ViewBag.ServiceCategories. DepartmentId: {DepartmentId}, SelectedId: {SelectedId}",
                    departmentId, selectedCategoryId);

                var categoriesResult = await _serviceManagementService.GetActiveServiceCategoriesForLookupAsync(departmentId);
                if (categoriesResult.Success && categoriesResult.Data?.Count > 0)
                {
                    // ایجاد SelectList با اطمینان از انتخاب صحیح
                    var selectList = new SelectList(categoriesResult.Data, "Id", "Title", selectedCategoryId);
                    ViewBag.ServiceCategories = selectList;

                    _log.Information("🏥 MEDICAL: ViewBag.ServiceCategories تنظیم شد. تعداد آیتم‌ها: {Count}, انتخاب شده: {SelectedId}",
                        categoriesResult.Data.Count, selectedCategoryId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: هیچ دسته‌بندی فعالی یافت نشد. DepartmentId: {DepartmentId}", departmentId);
                    ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تنظیم ViewBag.ServiceCategories. DepartmentId: {DepartmentId}", departmentId);
                ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
            }
        }

        /// <summary>
        /// تنظیم ViewBag.ServiceCategories برای Create Service - Medical Environment
        /// </summary>
        private async Task SetServiceCategoriesViewBagForCreate(int selectedCategoryId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: تنظیم ViewBag.ServiceCategories برای Create. SelectedId: {SelectedId}", selectedCategoryId);
                
                // ابتدا اطلاعات ServiceCategory را دریافت کنیم تا departmentId را بدانیم
                var categoryResult = await _serviceManagementService.GetServiceCategoryDetailsAsync(selectedCategoryId);
                if (categoryResult.Success)
                {
                    await SetServiceCategoriesViewBagForMedicalEnvironment(categoryResult.Data.DepartmentId, selectedCategoryId);
                    
                    _log.Information("🏥 MEDICAL: ViewBag.ServiceCategories برای Create تنظیم شد. DepartmentId: {DepartmentId}, SelectedId: {SelectedId}",
                        categoryResult.Data.DepartmentId, selectedCategoryId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی یافت نشد. SelectedId: {SelectedId}", selectedCategoryId);
                    ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تنظیم ViewBag.ServiceCategories برای Create. SelectedId: {SelectedId}", selectedCategoryId);
                ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
            }
        }

        /// <summary>
        /// تنظیم ViewBag.ServiceCategories برای Edit Service - Medical Environment
        /// </summary>
        private async Task SetServiceCategoriesViewBagForEdit(int serviceCategoryId, int selectedCategoryId = 0)
        {
            try
            {
                _log.Information("🏥 MEDICAL: تنظیم ViewBag.ServiceCategories برای Edit. ServiceCategoryId: {ServiceCategoryId}, SelectedId: {SelectedId}", 
                    serviceCategoryId, selectedCategoryId);
                
                // ابتدا اطلاعات ServiceCategory را دریافت کنیم تا departmentId را بدانیم
                var categoryResult = await _serviceManagementService.GetServiceCategoryDetailsAsync(serviceCategoryId);
                if (categoryResult.Success)
                {
                    await SetServiceCategoriesViewBagForMedicalEnvironment(categoryResult.Data.DepartmentId, selectedCategoryId);
                    
                    _log.Information("🏥 MEDICAL: ViewBag.ServiceCategories برای Edit تنظیم شد. DepartmentId: {DepartmentId}, SelectedId: {SelectedId}",
                        categoryResult.Data.DepartmentId, selectedCategoryId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی یافت نشد. ServiceCategoryId: {ServiceCategoryId}", serviceCategoryId);
                    ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تنظیم ViewBag.ServiceCategories برای Edit. ServiceCategoryId: {ServiceCategoryId}", serviceCategoryId);
                ViewBag.ServiceCategories = new SelectList(new List<LookupItemViewModel>(), "Id", "Title");
            }
        }

        #endregion

        #region Service Category Management

        /// <summary>
        /// نمایش لیست دسته‌بندی‌های خدمات
        /// </summary>
        public async Task<ActionResult> Categories(int? departmentId, string searchTerm = "", int page = 1, int pageSize = 10, bool isAjax = false)
        {
            try
            {
                _log.Information("درخواست لیست دسته‌بندی‌های خدمات. DepartmentId: {DepartmentId}, Page: {Page}, User: {UserId}",
                    departmentId, page, _currentUserService.UserId);

                // اگر departmentId مشخص نشده، تمام دسته‌بندی‌ها را نمایش بده
                if (!departmentId.HasValue)
                {
                    return await ShowAllCategories(searchTerm, page, pageSize, isAjax);
                }

                var result = await _serviceManagementService.GetServiceCategoriesAsync(
                    departmentId.Value, searchTerm, page, pageSize);

                if (!result.Success)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["ErrorMessage"] = result.Message;
                    return View("Error");
                }

                // آماده‌سازی ViewBag برای UI
                ViewBag.DepartmentId = departmentId.Value;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                if (isAjax)
                {
                    return PartialView("_CategoriesPartial", result.Data);
                }

                return View("Categories", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در لیست دسته‌بندی‌ها. DepartmentId: {DepartmentId}, User: {UserId}",
                    departmentId, _currentUserService.UserId);

                if (isAjax)
                {
                    return Json(new { success = false, message = "خطای سیستمی رخ داد." }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return View("Error");
            }
        }

        /// <summary>
        /// نمایش تمام دسته‌بندی‌های خدمات (Medical Environment)
        /// </summary>
        private async Task<ActionResult> ShowAllCategories(string searchTerm = "", int page = 1, int pageSize = 10, bool isAjax = false)
        {
            try
            {
                _log.Information("درخواست لیست تمام دسته‌بندی‌های خدمات. Page: {Page}, User: {UserId}",
                    page, _currentUserService.UserId);

                var result = await _serviceManagementService.GetAllServiceCategoriesAsync(searchTerm, page, pageSize);

                if (!result.Success)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["ErrorMessage"] = result.Message;
                    return View("Error");
                }

                // آماده‌سازی ViewBag برای UI
                ViewBag.DepartmentId = null; // نشان‌دهنده نمایش تمام دسته‌بندی‌ها
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                if (isAjax)
                {
                    return PartialView("_CategoriesPartial", result.Data);
                }

                return View("Categories", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در لیست تمام دسته‌بندی‌ها. User: {UserId}",
                    _currentUserService.UserId);

                if (isAjax)
                {
                    return Json(new { success = false, message = "خطای سیستمی رخ داد." }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return View("Error");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد دسته‌بندی خدمات
        /// </summary>
        public async Task<ActionResult> CreateCategory(int departmentId)
        {
            try
            {
                _log.Information("درخواست فرم ایجاد دسته‌بندی. DepartmentId: {DepartmentId}, User: {UserId}",
                    departmentId, _currentUserService.UserId);

                var model = new ServiceCategoryCreateEditViewModel
                {
                    DepartmentId = departmentId,
                    IsActive = true
                };

                // تنظیم ViewBag برای DropDownList
                await SetDepartmentsViewBag(departmentId);

                return View("CreateCategory", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در نمایش فرم ایجاد دسته‌بندی. DepartmentId: {DepartmentId}, User: {UserId}",
                    departmentId, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد.";
                return RedirectToAction("Categories");
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد دسته‌بندی خدمات - Medical Environment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategory(ServiceCategoryCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: پردازش ایجاد دسته‌بندی. Title: {Title}, DepartmentId: {DepartmentId}, User: {UserId}",
                    model?.Title, model?.DepartmentId, _currentUserService.UserId);

                // 🔒 اعتبارسنجی چندلایه - Medical Environment
                var validationResult = ValidateServiceCategoryForMedicalEnvironment(model).Result;
                if (!validationResult.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق. خطاها: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.Message)));
                    
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Field, error.Message);
                    }
                    
                    // تنظیم مجدد ViewBag در صورت خطا
                    await SetDepartmentsViewBag(model.DepartmentId);
                    return View("CreateCategory", model);
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: ModelState نامعتبر. خطاها: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    // تنظیم مجدد ViewBag در صورت خطا
                    await SetDepartmentsViewBag(model.DepartmentId);
                    return View("CreateCategory", model);
                }

                var result = await _serviceManagementService.CreateServiceCategoryAsync(model);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: دسته‌بندی با موفقیت ایجاد شد. Title: {Title}, User: {UserId}",
                        model.Title, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Categories", new { departmentId = model.DepartmentId });
                }

                // مدیریت خطاهای Validation
                if (result.ValidationErrors?.Count > 0)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(error.Field, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }

                // تنظیم مجدد ViewBag در صورت خطا
                await SetDepartmentsViewBag(model.DepartmentId);

                return View("CreateCategory", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد دسته‌بندی. Title: {Title}, User: {UserId}",
                    model?.Title, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.");
                
                // تنظیم مجدد ViewBag در صورت خطا
                if (model?.DepartmentId > 0)
                {
                    await SetDepartmentsViewBag(model.DepartmentId);
                }

                return View("CreateCategory", model);
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش دسته‌بندی خدمات - Medical Environment
        /// </summary>
        public async Task<ActionResult> EditCategory(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست فرم ویرایش دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                // دریافت اطلاعات دسته‌بندی
                var result = await _serviceManagementService.GetServiceCategoryForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی یافت نشد. CategoryId: {CategoryId}, User: {UserId}",
                        id, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Categories");
                }

                var model = result.Data;
                
                // تنظیم ViewBag برای DropDownList
                await SetDepartmentsViewBag(model.DepartmentId);

                _log.Information("🏥 MEDICAL: فرم ویرایش دسته‌بندی آماده شد. Title: {Title}, DepartmentId: {DepartmentId}, User: {UserId}",
                    model.Title, model.DepartmentId, _currentUserService.UserId);

                return View("EditCategory", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ویرایش دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Categories");
            }
        }

        /// <summary>
        /// پردازش فرم ویرایش دسته‌بندی خدمات - Medical Environment با اطمینان 100%
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategory(ServiceCategoryCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: پردازش ویرایش دسته‌بندی. CategoryId: {CategoryId}, Title: {Title}, DepartmentId: {DepartmentId}, User: {UserId}",
                    model?.ServiceCategoryId, model?.Title, model?.DepartmentId, _currentUserService.UserId);

                // 🔒 اعتبارسنجی چندلایه - Medical Environment
                var validationResult = ValidateServiceCategoryForMedicalEnvironment(model).Result;
                if (!validationResult.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق. خطاها: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.Message)));
                    
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Field, error.Message);
                    }
                    
                    // تنظیم مجدد ViewBag در صورت خطا
                    await SetDepartmentsViewBag(model.DepartmentId);
                    return View("EditCategory", model);
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: ModelState نامعتبر. خطاها: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    // تنظیم مجدد ViewBag در صورت خطا
                    await SetDepartmentsViewBag(model.DepartmentId);
                    return View("EditCategory", model);
                }

                var result = await _serviceManagementService.UpdateServiceCategoryAsync(model);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: دسته‌بندی با موفقیت ویرایش شد. CategoryId: {CategoryId}, Title: {Title}, User: {UserId}",
                        model.ServiceCategoryId, model.Title, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Categories", new { departmentId = model.DepartmentId });
                }

                // مدیریت خطاهای Validation
                if (result.ValidationErrors?.Count > 0)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(error.Field, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }

                // تنظیم مجدد ViewBag در صورت خطا
                await SetDepartmentsViewBag(model.DepartmentId);

                return View("EditCategory", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش دسته‌بندی. CategoryId: {CategoryId}, Title: {Title}, User: {UserId}",
                    model?.ServiceCategoryId, model?.Title, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.");
                
                // تنظیم مجدد ViewBag در صورت خطا
                if (model?.DepartmentId > 0)
                {
                    await SetDepartmentsViewBag(model.DepartmentId);
                }

                return View("EditCategory", model);
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// نمایش لیست خدمات یک دسته‌بندی
        /// </summary>
        public async Task<ActionResult> Index(int? serviceCategoryId, string searchTerm = "", int page = 1, int pageSize = 10, bool isAjax = false)
        {
            try
            {
                _log.Information("درخواست لیست خدمات. CategoryId: {CategoryId}, Page: {Page}, User: {UserId}",
                    serviceCategoryId, page, _currentUserService.UserId);

                // اگر serviceCategoryId مشخص نشده، به صفحه انتخاب دسته‌بندی هدایت کن
                if (!serviceCategoryId.HasValue)
                {
                    return RedirectToAction("Categories");
                }

                var result = await _serviceManagementService.GetServicesAsync(
                    serviceCategoryId.Value, searchTerm, page, pageSize);

                if (!result.Success)
                {
                    if (isAjax)
                    {
                        return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["ErrorMessage"] = result.Message;
                    return View("Error");
                }

                ViewBag.ServiceCategoryId = serviceCategoryId.Value;
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;

                if (isAjax)
                {
                    return PartialView("_ServicesPartial", result.Data);
                }

                return View("Index", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای غیرمنتظره در لیست خدمات. CategoryId: {CategoryId}, User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);

                if (isAjax)
                {
                    return Json(new { success = false, message = "خطای سیستمی رخ داد." }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return View("Error");
            }
        }

        /// <summary>
        /// نمایش فرم ایجاد خدمت - Medical Environment
        /// </summary>
        public async Task<ActionResult> Create(int serviceCategoryId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست فرم ایجاد خدمت. CategoryId: {CategoryId}, User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);

                // ابتدا اطلاعات ServiceCategory را دریافت کنیم تا departmentId را بدانیم
                var categoryResult = await _serviceManagementService.GetServiceCategoryDetailsAsync(serviceCategoryId);
                if (!categoryResult.Success)
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی خدمات یافت نشد. CategoryId: {CategoryId}", serviceCategoryId);
                    TempData["ErrorMessage"] = "دسته‌بندی خدمات مورد نظر یافت نشد.";
                    return RedirectToAction("Categories");
                }

                var model = new ServiceCreateEditViewModel
                {
                    ServiceCategoryId = serviceCategoryId,
                    IsActive = true
                };

                // تنظیم ViewBag برای DropDownList - Medical Environment
                await SetServiceCategoriesViewBagForMedicalEnvironment(categoryResult.Data.DepartmentId, serviceCategoryId);

                // اضافه کردن اطلاعات اضافی برای UI
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";
                ViewBag.ServiceCategoryName = categoryResult.Data.Title;
                ViewBag.DepartmentName = categoryResult.Data.DepartmentName;

                _log.Information("🏥 MEDICAL: فرم ایجاد خدمت آماده شد. CategoryId: {CategoryId}, DepartmentId: {DepartmentId}",
                    serviceCategoryId, categoryResult.Data.DepartmentId);

                return View("Create", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ایجاد خدمت. CategoryId: {CategoryId}, User: {UserId}",
                    serviceCategoryId, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index", new { serviceCategoryId });
            }
        }

        /// <summary>
        /// پردازش فرم ایجاد خدمت - Medical Environment با اطمینان 100%
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ServiceCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: پردازش ایجاد خدمت. Title: {Title}, CategoryId: {CategoryId}, User: {UserId}",
                    model?.Title, model?.ServiceCategoryId, _currentUserService.UserId);

                // 🔒 اعتبارسنجی چندلایه - Medical Environment
                var validationResult = ValidateServiceForMedicalEnvironment(model).Result;
                if (!validationResult.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق. خطاها: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.Message)));
                    
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Field, error.Message);
                    }
                    
                    // تنظیم مجدد ViewBag در صورت خطا - Medical Environment
                    await SetServiceCategoriesViewBagForMedicalEnvironment(model.ServiceCategoryId, model.ServiceCategoryId);
                    ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";
                    
                    return View("Create", model);
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: ModelState نامعتبر. خطاها: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    // تنظیم مجدد ViewBag در صورت خطا - Medical Environment
                    await SetServiceCategoriesViewBagForMedicalEnvironment(model.ServiceCategoryId, model.ServiceCategoryId);
                    ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";
                    
                    return View("Create", model);
                }

                var result = await _serviceManagementService.CreateServiceAsync(model);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: خدمت با موفقیت ایجاد شد. Title: {Title}, User: {UserId}",
                        model.Title, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index", new { serviceCategoryId = model.ServiceCategoryId });
                }

                // مدیریت خطاهای Validation
                if (result.ValidationErrors?.Count > 0)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(error.Field, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }

                // تنظیم مجدد ViewBag در صورت خطا
                await SetServiceCategoriesViewBagForMedicalEnvironment(model.ServiceCategoryId, model.ServiceCategoryId);
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";

                return View("Create", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ایجاد خدمت. Title: {Title}, User: {UserId}",
                    model?.Title, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.");
                
                // تنظیم مجدد ViewBag در صورت خطا
                if (model?.ServiceCategoryId > 0)
                {
                    await SetServiceCategoriesViewBagForMedicalEnvironment(model.ServiceCategoryId, model.ServiceCategoryId);
                }
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";

                return View("Create", model);
            }
        }

        /// <summary>
        /// نمایش جزئیات دسته‌بندی خدمات
        /// </summary>
        public async Task<ActionResult> CategoryDetails(int id)
        {
            try
            {
                _log.Information("درخواست جزئیات دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                var result = await _serviceManagementService.GetServiceCategoryDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning("دسته‌بندی یافت نشد. CategoryId: {CategoryId}, User: {UserId}",
                        id, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Categories");
                }

                return View("CategoryDetails", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در نمایش جزئیات دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Categories");
            }
        }

        /// <summary>
        /// حذف نرم دسته‌بندی خدمات - Medical Environment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست حذف دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                var result = await _serviceManagementService.SoftDeleteServiceCategoryAsync(id);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: دسته‌بندی با موفقیت حذف شد. CategoryId: {CategoryId}, User: {UserId}",
                        id, _currentUserService.UserId);

                    return Json(new { success = true, message = result.Message }, JsonRequestBehavior.AllowGet);
                }

                _log.Warning("🏥 MEDICAL: حذف دسته‌بندی ناموفق. CategoryId: {CategoryId}, Message: {Message}, User: {UserId}",
                    id, result.Message, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// تغییر وضعیت دسته‌بندی خدمات - Medical Environment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleServiceCategoryStatus(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست تغییر وضعیت دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                // ابتدا اطلاعات دسته‌بندی را دریافت کنیم
                var categoryResult = await _serviceManagementService.GetServiceCategoryForEditAsync(id);
                if (!categoryResult.Success)
                {
                    _log.Warning("🏥 MEDICAL: دسته‌بندی یافت نشد. CategoryId: {CategoryId}, User: {UserId}",
                        id, _currentUserService.UserId);

                    return Json(new { success = false, message = "دسته‌بندی مورد نظر یافت نشد." });
                }

                var model = categoryResult.Data;
                model.IsActive = !model.IsActive; // تغییر وضعیت

                var result = await _serviceManagementService.UpdateServiceCategoryAsync(model);

                if (result.Success)
                {
                    var statusText = model.IsActive ? "فعال" : "غیرفعال";
                    _log.Information("🏥 MEDICAL: وضعیت دسته‌بندی با موفقیت تغییر کرد. CategoryId: {CategoryId}, Status: {Status}, User: {UserId}",
                        id, statusText, _currentUserService.UserId);

                    return Json(new { success = true, message = $"دسته‌بندی با موفقیت {statusText} شد." });
                }

                _log.Warning("🏥 MEDICAL: تغییر وضعیت دسته‌بندی ناموفق. CategoryId: {CategoryId}, Message: {Message}, User: {UserId}",
                    id, result.Message, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در تغییر وضعیت دسته‌بندی. CategoryId: {CategoryId}, User: {UserId}",
                    id, _currentUserService.UserId);

                return Json(new { success = false, message = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید." });
            }
        }

        #endregion

        #region Service Management

        /// <summary>
        /// نمایش جزئیات خدمت
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _log.Information("درخواست جزئیات خدمت. ServiceId: {ServiceId}, User: {UserId}",
                    id, _currentUserService.UserId);

                var result = await _serviceManagementService.GetServiceDetailsAsync(id);
                if (!result.Success)
                {
                    _log.Warning("خدمت یافت نشد. ServiceId: {ServiceId}, User: {UserId}",
                        id, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                return View("Details", result.Data);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در نمایش جزئیات خدمت. ServiceId: {ServiceId}, User: {UserId}",
                    id, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// نمایش فرم ویرایش خدمت - Medical Environment
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست فرم ویرایش خدمت. ServiceId: {ServiceId}, User: {UserId}",
                    id, _currentUserService.UserId);

                var result = await _serviceManagementService.GetServiceForEditAsync(id);
                if (!result.Success)
                {
                    _log.Warning("🏥 MEDICAL: خدمت یافت نشد. ServiceId: {ServiceId}, User: {UserId}",
                        id, _currentUserService.UserId);
                    
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("Index");
                }

                var model = result.Data;
                
                // تنظیم ViewBag برای DropDownList - Medical Environment
                await SetServiceCategoriesViewBagForEdit(model.ServiceCategoryId, model.ServiceCategoryId);

                // اضافه کردن اطلاعات اضافی برای UI
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";

                _log.Information("🏥 MEDICAL: فرم ویرایش خدمت آماده شد. ServiceId: {ServiceId}, Title: {Title}, User: {UserId}",
                    id, model.Title, _currentUserService.UserId);

                return View("Edit", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در نمایش فرم ویرایش خدمت. ServiceId: {ServiceId}, User: {UserId}",
                    id, _currentUserService.UserId);

                TempData["ErrorMessage"] = "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// پردازش فرم ویرایش خدمت - Medical Environment با اطمینان 100%
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ServiceCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: پردازش ویرایش خدمت. ServiceId: {ServiceId}, Title: {Title}, User: {UserId}",
                    model?.ServiceId, model?.Title, _currentUserService.UserId);

                // 🔒 اعتبارسنجی چندلایه - Medical Environment
                var validationResult = ValidateServiceForMedicalEnvironment(model).Result;
                if (!validationResult.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: اعتبارسنجی ناموفق. خطاها: {Errors}", 
                        string.Join(", ", validationResult.Errors.Select(e => e.Message)));
                    
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.Field, error.Message);
                    }
                    
                    // تنظیم مجدد ViewBag در صورت خطا - Medical Environment
                    await SetServiceCategoriesViewBagForEdit(model.ServiceCategoryId, model.ServiceCategoryId);
                    ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";
                    
                    return View("Edit", model);
                }

                if (!ModelState.IsValid)
                {
                    _log.Warning("🏥 MEDICAL: ModelState نامعتبر. خطاها: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    // تنظیم مجدد ViewBag در صورت خطا - Medical Environment
                    await SetServiceCategoriesViewBagForEdit(model.ServiceCategoryId, model.ServiceCategoryId);
                    ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";
                    
                    return View("Edit", model);
                }

                var result = await _serviceManagementService.UpdateServiceAsync(model);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: خدمت با موفقیت ویرایش شد. ServiceId: {ServiceId}, Title: {Title}, User: {UserId}",
                        model.ServiceId, model.Title, _currentUserService.UserId);

                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Index", new { serviceCategoryId = model.ServiceCategoryId });
                }

                // مدیریت خطاهای Validation
                if (result.ValidationErrors?.Count > 0)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(error.Field, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }

                // تنظیم مجدد ViewBag در صورت خطا
                await SetServiceCategoriesViewBagForEdit(model.ServiceCategoryId, model.ServiceCategoryId);
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";

                return View("Edit", model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در ویرایش خدمت. ServiceId: {ServiceId}, Title: {Title}, User: {UserId}",
                    model?.ServiceId, model?.Title, _currentUserService.UserId);

                ModelState.AddModelError("", "خطای سیستمی رخ داد. لطفاً مجدداً تلاش کنید.");
                
                // تنظیم مجدد ViewBag در صورت خطا
                if (model?.ServiceCategoryId > 0)
                {
                    await SetServiceCategoriesViewBagForEdit(model.ServiceCategoryId, model.ServiceCategoryId);
                }
                ViewBag.CurrentUserName = _currentUserService.UserName ?? "کاربر سیستم";

                return View("Edit", model);
            }
        }

        /// <summary>
        /// بررسی یکتا بودن کد خدمت - Medical Environment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckServiceCode(string serviceCode, int? serviceCategoryId = null, int? excludeServiceId = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: بررسی کد خدمت. ServiceCode: {ServiceCode}, CategoryId: {CategoryId}, ExcludeId: {ExcludeId}, User: {UserId}",
                    serviceCode, serviceCategoryId, excludeServiceId, _currentUserService.UserId);

                // 🔒 اعتبارسنجی اولیه کد خدمت - Medical Environment
                if (string.IsNullOrWhiteSpace(serviceCode))
                {
                    return Json(new { isAvailable = false, message = "کد خدمت الزامی است" });
                }

                // 🔒 اعتبارسنجی الگوی کد خدمت - فقط اعداد برای محیط درمانی
                if (!System.Text.RegularExpressions.Regex.IsMatch(serviceCode.Trim(), @"^\d+$"))
                {
                    return Json(new { isAvailable = false, message = "کد خدمت باید فقط شامل اعداد باشد" });
                }

                // 🔒 اعتبارسنجی طول کد خدمت - Medical Environment
                if (serviceCode.Trim().Length < 3 || serviceCode.Trim().Length > 10)
                {
                    return Json(new { isAvailable = false, message = "کد خدمت باید بین 3 تا 10 رقم باشد" });
                }

                // 🔒 بررسی یکتا بودن کد خدمت
                var isDuplicate = await _serviceManagementService.IsServiceCodeDuplicateAsync(serviceCode.Trim(), serviceCategoryId, excludeServiceId);

                if (isDuplicate)
                {
                    _log.Warning("🏥 MEDICAL: کد خدمت تکراری. ServiceCode: {ServiceCode}, User: {UserId}",
                        serviceCode, _currentUserService.UserId);
                    return Json(new { isAvailable = false, message = "این کد خدمت قبلاً استفاده شده است" });
                }

                _log.Information("🏥 MEDICAL: کد خدمت در دسترس. ServiceCode: {ServiceCode}, User: {UserId}",
                    serviceCode, _currentUserService.UserId);
                return Json(new { isAvailable = true, message = "کد خدمت در دسترس است" });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی کد خدمت. ServiceCode: {ServiceCode}, User: {UserId}",
                    serviceCode, _currentUserService.UserId);
                return Json(new { isAvailable = false, message = "خطا در بررسی کد خدمت" });
            }
        }

        /// <summary>
        /// حذف نرم خدمت - Medical Environment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست حذف خدمت. ServiceId: {ServiceId}, User: {UserId}, RequestMethod: {Method}",
                    id, _currentUserService.UserId, Request.HttpMethod);

                // 🔒 اعتبارسنجی ورودی
                if (id <= 0)
                {
                    _log.Warning("🏥 MEDICAL: شناسه خدمت نامعتبر. ServiceId: {ServiceId}, User: {UserId}",
                        id, _currentUserService.UserId);
                    return Json(new { success = false, message = "شناسه خدمت معتبر نیست." });
                }

                _log.Information("🏥 MEDICAL: فراخوانی سرویس حذف. ServiceId: {ServiceId}, User: {UserId}",
                    id, _currentUserService.UserId);

                var result = await _serviceManagementService.SoftDeleteServiceAsync(id);

                _log.Information("🏥 MEDICAL: نتیجه حذف. ServiceId: {ServiceId}, Success: {Success}, Message: {Message}, User: {UserId}",
                    id, result.Success, result.Message, _currentUserService.UserId);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: خدمت با موفقیت حذف شد. ServiceId: {ServiceId}, User: {UserId}",
                        id, _currentUserService.UserId);

                    return Json(new { success = true, message = result.Message });
                }

                _log.Warning("🏥 MEDICAL: حذف خدمت ناموفق. ServiceId: {ServiceId}, Message: {Message}, User: {UserId}",
                    id, result.Message, _currentUserService.UserId);

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در حذف خدمت. ServiceId: {ServiceId}, User: {UserId}, ExceptionType: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}",
                    id, _currentUserService.UserId, ex.GetType().Name, ex.Message, ex.StackTrace);

                // 🔒 بررسی نوع خطا برای پیام مناسب
                string errorMessage = "خطای سیستمی در حذف خدمت رخ داد.";
                string errorCode = "UNKNOWN_ERROR";
                
                if (ex is System.Data.SqlClient.SqlException sqlEx)
                {
                    errorMessage = "خطای پایگاه داده رخ داد. لطفاً مجدداً تلاش کنید.";
                    errorCode = "DB_ERROR";
                    _log.Error("🏥 MEDICAL: SQL Error Details - Number: {ErrorNumber}, State: {State}, Message: {Message}",
                        sqlEx.Number, sqlEx.State, sqlEx.Message);
                }
                else if (ex is System.Data.Entity.Infrastructure.DbUpdateException dbEx)
                {
                    errorMessage = "خطای به‌روزرسانی پایگاه داده رخ داد.";
                    errorCode = "DB_UPDATE_ERROR";
                    _log.Error("🏥 MEDICAL: DbUpdateException Details - InnerException: {InnerException}",
                        dbEx.InnerException?.Message);
                }
                else if (ex is System.InvalidOperationException)
                {
                    errorMessage = "خطای عملیاتی رخ داد. لطفاً مجدداً تلاش کنید.";
                    errorCode = "INVALID_OPERATION";
                }
                else if (ex is System.ArgumentNullException)
                {
                    errorMessage = "خطای پارامتر ورودی رخ داد.";
                    errorCode = "ARGUMENT_NULL";
                }

                // 🔒 در محیط توسعه، جزئیات بیشتری ارائه دهید
                if (System.Web.HttpContext.Current?.IsDebuggingEnabled == true)
                {
                    errorMessage += $" (خطا: {ex.GetType().Name})";
                }

                return Json(new { 
                    success = false, 
                    message = errorMessage,
                    errorCode = errorCode,
                    exceptionType = ex.GetType().Name
                });
            }
        }

        #endregion
    }
}