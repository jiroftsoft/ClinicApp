using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Core;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Reception;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;
using ClinicApp.Helpers;

namespace ClinicApp.Helpers
{
    /// <summary>
    /// Helper برای مدیریت AJAX endpoints ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت AJAX requests
    /// 2. اعتبارسنجی ورودی‌ها
    /// 3. مدیریت خطاها
    /// 4. بهینه‌سازی عملکرد
    /// 5. مدیریت Cache
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط مدیریت AJAX
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// </summary>
    public class ReceptionAjaxHelper
    {
        #region Fields and Constructor

        private readonly IReceptionService _receptionService;
        private readonly IReceptionBusinessRules _businessRules;
        private readonly IReceptionSecurityService _securityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public ReceptionAjaxHelper(
            IReceptionService receptionService,
            IReceptionBusinessRules businessRules,
            IReceptionSecurityService securityService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _receptionService = receptionService ?? throw new ArgumentNullException(nameof(receptionService));
            _businessRules = businessRules ?? throw new ArgumentNullException(nameof(businessRules));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Search Methods

        /// <summary>
        /// جستجوی پذیرش‌ها
        /// </summary>
        /// <param name="model">مدل جستجو</param>
        /// <returns>نتیجه جستجو</returns>
        public async Task<JsonResult> SearchReceptionsAsync(ReceptionSearchViewModel model)
        {
            try
            {
                _logger.Information("جستجوی پذیرش‌ها. معیارها: {@Model}, کاربر: {UserName}", model, _currentUserService.UserName);

                // Security check
                var canView = await _securityService.CanViewReceptionsListAsync(_currentUserService.UserId);
                if (!canView)
                {
                    _logger.Warning("کاربر {UserName} مجوز مشاهده لیست پذیرش‌ها را ندارد", _currentUserService.UserName);
                    return CreateJsonError("شما مجوز مشاهده لیست پذیرش‌ها را ندارید");
                }

                // Input validation
                var validationResult = await _securityService.ValidateReceptionInputAsync(model.SearchTerm ?? "");
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی ورودی جستجو ناموفق. خطاها: {@Errors}", validationResult.Errors);
                    return CreateJsonValidationError("ورودی نامعتبر است", validationResult.Errors);
                }

                // Search receptions
                var searchResult = await _receptionService.SearchReceptionsAsync(model);
                if (!searchResult.Success)
                {
                    _logger.Warning("جستجوی پذیرش‌ها ناموفق. خطا: {Error}", searchResult.Message);
                    return CreateJsonError(searchResult.Message);
                }

                var result = new
                {
                    success = true,
                    data = searchResult.Data,
                    totalCount = searchResult.Data?.TotalCount ?? 0,
                    pageCount = searchResult.Data?.PageCount ?? 0,
                    currentPage = searchResult.Data?.CurrentPage ?? 0
                };

                _logger.Information("جستجوی پذیرش‌ها موفق. تعداد: {Count}", result.totalCount);
                return CreateJsonSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها");
                return CreateJsonError("خطا در جستجوی پذیرش‌ها");
            }
        }

        /// <summary>
        /// جستجوی بیماران
        /// </summary>
        /// <param name="searchTerm">عبارت جستجو</param>
        /// <returns>نتیجه جستجو</returns>
        public async Task<JsonResult> SearchPatientsAsync(string searchTerm)
        {
            try
            {
                _logger.Debug("جستجوی بیماران. عبارت جستجو: {SearchTerm}, کاربر: {UserName}", searchTerm, _currentUserService.UserName);

                // Input validation
                var validationResult = await _securityService.ValidatePatientInputAsync(searchTerm);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی ورودی جستجوی بیماران ناموفق. خطاها: {@Errors}", validationResult.Errors);
                    return CreateJsonValidationError("ورودی نامعتبر است", validationResult.Errors);
                }

                // Search patients
                var patientsResult = await _receptionService.SearchPatientsAsync(searchTerm);
                if (!patientsResult.Success)
                {
                    _logger.Warning("جستجوی بیماران ناموفق. خطا: {Error}", patientsResult.Message);
                    return CreateJsonError(patientsResult.Message);
                }

                _logger.Debug("جستجوی بیماران موفق. تعداد: {Count}", patientsResult.Data.Count);
                return CreateJsonSuccess(patientsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیماران");
                return CreateJsonError("خطا در جستجوی بیماران");
            }
        }

        #endregion

        #region CRUD Methods

        /// <summary>
        /// ایجاد پذیرش
        /// </summary>
        /// <param name="model">مدل ایجاد پذیرش</param>
        /// <returns>نتیجه ایجاد</returns>
        public async Task<JsonResult> CreateReceptionAsync(ReceptionCreateViewModel model)
        {
            try
            {
                _logger.Information("ایجاد پذیرش جدید. بیمار: {PatientId}, خدمت: {ServiceId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.PatientId, model?.SelectedServiceIds?.Count ?? 0, model?.DoctorId, _currentUserService.UserName);

                // Security check
                var canCreate = await _securityService.CanCreateReceptionAsync(_currentUserService.UserId);
                if (!canCreate)
                {
                    _logger.Warning("کاربر {UserName} مجوز ایجاد پذیرش را ندارد", _currentUserService.UserName);
                    return CreateJsonSecurityError("شما مجوز ایجاد پذیرش را ندارید");
                }

                // Business rules validation
                var businessRulesResult = await _businessRules.ValidateReceptionAsync(model);
                if (!businessRulesResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی قوانین کسب‌وکار ناموفق. خطاها: {@Errors}", businessRulesResult.Errors);
                    return CreateJsonValidationError("اطلاعات وارد شده نامعتبر است", businessRulesResult.Errors);
                }

                // Create reception
                var createResult = await _receptionService.CreateReceptionAsync(model);
                if (!createResult.Success)
                {
                    _logger.Warning("ایجاد پذیرش ناموفق. خطا: {Error}", createResult.Message);
                    return CreateJsonError(createResult.Message);
                }

                // Clear related caches
                await ClearRelatedCachesAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "CreateReception", $"ReceptionId:{createResult.Data.ReceptionId}", true);

                var result = new
                {
                    receptionId = createResult.Data.ReceptionId,
                    receptionNumber = createResult.Data.ReceptionNumber,
                    redirectUrl = $"/Reception/Details/{createResult.Data.ReceptionId}"
                };

                _logger.Information("ایجاد پذیرش موفق. شناسه: {ReceptionId}", createResult.Data.ReceptionId);
                return CreateJsonSuccess(result, "پذیرش با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد پذیرش");
                return CreateJsonError("خطا در ایجاد پذیرش");
            }
        }

        /// <summary>
        /// ویرایش پذیرش
        /// </summary>
        /// <param name="model">مدل ویرایش پذیرش</param>
        /// <returns>نتیجه ویرایش</returns>
        public async Task<JsonResult> UpdateReceptionAsync(ReceptionEditViewModel model)
        {
            try
            {
                _logger.Information("ویرایش پذیرش. شناسه: {Id}, بیمار: {PatientId}, پزشک: {DoctorId}, کاربر: {UserName}",
                    model?.ReceptionId, model?.PatientId, model?.DoctorId, _currentUserService.UserName);

                // Security check
                var canEdit = await _securityService.CanEditReceptionAsync(_currentUserService.UserId, model.ReceptionId);
                if (!canEdit)
                {
                    _logger.Warning("کاربر {UserName} مجوز ویرایش پذیرش {Id} را ندارد", _currentUserService.UserName, model.ReceptionId);
                    return CreateJsonSecurityError("شما مجوز ویرایش این پذیرش را ندارید");
                }

                // Business rules validation
                var businessRulesResult = await _businessRules.ValidateReceptionEditAsync(model);
                if (!businessRulesResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی قوانین کسب‌وکار ناموفق. خطاها: {@Errors}", businessRulesResult.Errors);
                    return CreateJsonValidationError("اطلاعات وارد شده نامعتبر است", businessRulesResult.Errors);
                }

                // Update reception
                var updateResult = await _receptionService.UpdateReceptionAsync(model);
                if (!updateResult.Success)
                {
                    _logger.Warning("ویرایش پذیرش ناموفق. خطا: {Error}", updateResult.Message);
                    return CreateJsonError(updateResult.Message);
                }

                // Clear related caches
                // Cache cleared - no longer needed
                await ClearRelatedCachesAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "EditReception", $"ReceptionId:{model.ReceptionId}", true);

                var result = new
                {
                    receptionId = model.ReceptionId,
                    redirectUrl = $"/Reception/Details/{model.ReceptionId}"
                };

                _logger.Information("ویرایش پذیرش موفق. شناسه: {ReceptionId}", model.ReceptionId);
                return CreateJsonSuccess(result, "پذیرش با موفقیت ویرایش شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ویرایش پذیرش. شناسه: {Id}", model?.ReceptionId);
                return CreateJsonError("خطا در ویرایش پذیرش");
            }
        }

        /// <summary>
        /// حذف پذیرش
        /// </summary>
        /// <param name="id">شناسه پذیرش</param>
        /// <returns>نتیجه حذف</returns>
        public async Task<JsonResult> DeleteReceptionAsync(int id)
        {
            try
            {
                _logger.Information("حذف پذیرش. شناسه: {Id}, کاربر: {UserName}", id, _currentUserService.UserName);

                // Security check
                var canDelete = await _securityService.CanDeleteReceptionAsync(_currentUserService.UserId, id);
                if (!canDelete)
                {
                    _logger.Warning("کاربر {UserName} مجوز حذف پذیرش {Id} را ندارد", _currentUserService.UserName, id);
                    return CreateJsonSecurityError("شما مجوز حذف این پذیرش را ندارید");
                }

                // Delete reception
                var deleteResult = await _receptionService.DeleteReceptionAsync(id);
                if (!deleteResult.Success)
                {
                    _logger.Warning("حذف پذیرش ناموفق. خطا: {Error}", deleteResult.Message);
                    return CreateJsonError(deleteResult.Message);
                }

                // Clear related caches
                // Cache cleared - no longer needed
                await ClearRelatedCachesAsync();

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "DeleteReception", $"ReceptionId:{id}", true);

                _logger.Information("حذف پذیرش موفق. شناسه: {Id}", id);
                return CreateJsonSuccess(null, "پذیرش با موفقیت حذف شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پذیرش. شناسه: {Id}", id);
                return CreateJsonError("خطا در حذف پذیرش");
            }
        }

        #endregion

        #region Lookup Methods

        /// <summary>
        /// دریافت لیست پزشکان
        /// </summary>
        /// <returns>لیست پزشکان</returns>
        public async Task<JsonResult> GetDoctorsAsync()
        {
            try
            {
                _logger.Debug("دریافت لیست پزشکان. کاربر: {UserName}", _currentUserService.UserName);

                // Load from database directly
                var doctorsResult = await _receptionService.GetDoctorsAsync();
                if (!doctorsResult.Success)
                {
                    _logger.Warning("دریافت لیست پزشکان ناموفق. خطا: {Error}", doctorsResult.Message);
                    return CreateJsonError(doctorsResult.Message);
                }

                _logger.Debug("دریافت لیست پزشکان از Database موفق. تعداد: {Count}", doctorsResult.Data.Count);
                return CreateJsonSuccess(doctorsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست پزشکان");
                return CreateJsonError("خطا در دریافت لیست پزشکان");
            }
        }

        /// <summary>
        /// دریافت لیست خدمات
        /// </summary>
        /// <param name="categoryId">شناسه دسته‌بندی</param>
        /// <returns>لیست خدمات</returns>
        public async Task<JsonResult> GetServicesAsync(int categoryId = 0)
        {
            try
            {
                _logger.Debug("دریافت لیست خدمات. دسته‌بندی: {CategoryId}, کاربر: {UserName}", categoryId, _currentUserService.UserName);

                // Try to get from cache first
                // Load from database directly
                var servicesResult = await _receptionService.GetServicesByCategoryAsync(categoryId);
                if (!servicesResult.Success)
                {
                    _logger.Warning("دریافت لیست خدمات ناموفق. خطا: {Error}", servicesResult.Message);
                    return CreateJsonError(servicesResult.Message);
                }

                _logger.Debug("دریافت لیست خدمات از Database موفق. تعداد: {Count}", servicesResult.Data.Count);
                return CreateJsonSuccess(servicesResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست خدمات. دسته‌بندی: {CategoryId}", categoryId);
                return CreateJsonError("خطا در دریافت لیست خدمات");
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی‌های خدمات
        /// </summary>
        /// <returns>لیست دسته‌بندی‌ها</returns>
        public async Task<JsonResult> GetServiceCategoriesAsync()
        {
            try
            {
                _logger.Debug("دریافت لیست دسته‌بندی‌های خدمات. کاربر: {UserName}", _currentUserService.UserName);

                // Try to get from cache first
                // Load from database directly
                var categoriesResult = await _receptionService.GetServiceCategoriesAsync();
                if (!categoriesResult.Success)
                {
                    _logger.Warning("دریافت لیست دسته‌بندی‌های خدمات ناموفق. خطا: {Error}", categoriesResult.Message);
                    return CreateJsonError(categoriesResult.Message);
                }

                _logger.Debug("دریافت لیست دسته‌بندی‌های خدمات از Database موفق. تعداد: {Count}", categoriesResult.Data.Count);
                return CreateJsonSuccess(categoriesResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست دسته‌بندی‌های خدمات");
                return CreateJsonError("خطا در دریافت لیست دسته‌بندی‌های خدمات");
            }
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// دریافت آمار روزانه
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار روزانه</returns>
        public async Task<JsonResult> GetDailyStatsAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار روزانه. تاریخ: {Date}, کاربر: {UserName}", date, _currentUserService.UserName);

                // Try to get from cache first
                // Load from database directly
                var statsResult = await _receptionService.GetDailyStatsAsync(date);
                if (!statsResult.Success)
                {
                    _logger.Warning("دریافت آمار روزانه ناموفق. خطا: {Error}", statsResult.Message);
                    return CreateJsonError(statsResult.Message);
                }

                _logger.Debug("دریافت آمار روزانه از Database موفق");
                return CreateJsonSuccess(statsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار روزانه. تاریخ: {Date}", date);
                return CreateJsonError("خطا در دریافت آمار روزانه");
            }
        }

        /// <summary>
        /// دریافت آمار پزشکان
        /// </summary>
        /// <param name="date">تاریخ</param>
        /// <returns>آمار پزشکان</returns>
        public async Task<JsonResult> GetDoctorStatsAsync(DateTime date)
        {
            try
            {
                _logger.Debug("دریافت آمار پزشکان. تاریخ: {Date}, کاربر: {UserName}", date, _currentUserService.UserName);

                // Load from database
                var statsResult = await _receptionService.GetDoctorStatsAsync(0, date);
                if (!statsResult.Success)
                {
                    _logger.Warning("دریافت آمار پزشکان ناموفق. خطا: {Error}", statsResult.Message);
                    return CreateJsonError(statsResult.Message);
                }

                _logger.Debug("دریافت آمار پزشکان موفق. تعداد: {Count}", statsResult.Data.Count);
                return CreateJsonSuccess(statsResult.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار پزشکان. تاریخ: {Date}", date);
                return CreateJsonError("خطا در دریافت آمار پزشکان");
            }
        }

        #endregion

        #region Performance Methods

        /// <summary>
        /// دریافت اطلاعات عملکرد
        /// </summary>
        /// <returns>اطلاعات عملکرد</returns>
        public async Task<JsonResult> GetPerformanceInfoAsync()
        {
            try
            {
                _logger.Debug("دریافت اطلاعات عملکرد. کاربر: {UserName}", _currentUserService.UserName);

                // Performance monitoring disabled - cache removed
                var performanceInfo = new PerformanceInfo { IsOptimal = true, PerformanceMessage = "Cache removed" };
                var queryStats = new QueryStatistics { TotalQueries = 0 };
                var cacheStats = new CacheStatistics { TotalMemoryUsage = 0 };

                var result = new
                {
                    performance = performanceInfo,
                    queries = queryStats,
                    cache = cacheStats
                };

                _logger.Debug("دریافت اطلاعات عملکرد موفق");
                return CreateJsonSuccess(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات عملکرد");
                return CreateJsonError("خطا در دریافت اطلاعات عملکرد");
            }
        }

        /// <summary>
        /// پاک کردن Cache
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        public async Task<JsonResult> ClearCacheAsync()
        {
            try
            {
                _logger.Information("پاک کردن Cache. کاربر: {UserName}", _currentUserService.UserName);

                // Cache cleared - no longer needed

                // Log security action
                await _securityService.LogSecurityActionAsync(_currentUserService.UserId, "ClearCache", "All", true);

                _logger.Information("پاک کردن Cache موفق");
                return CreateJsonSuccess(null, "Cache با موفقیت پاک شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache");
                return CreateJsonError("خطا در پاک کردن Cache");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ایجاد نتیجه JSON موفق
        /// </summary>
        /// <param name="data">داده</param>
        /// <param name="message">پیام</param>
        /// <returns>نتیجه JSON</returns>
        private JsonResult CreateJsonSuccess(object data = null, string message = "عملیات با موفقیت انجام شد")
        {
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    message = message,
                    data = data
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// ایجاد نتیجه JSON ناموفق
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>نتیجه JSON</returns>
        private JsonResult CreateJsonError(string message = "خطا در انجام عملیات")
        {
            return new JsonResult
            {
                Data = new
                {
                    success = false,
                    message = message
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// ایجاد نتیجه JSON با اعتبارسنجی
        /// </summary>
        /// <param name="message">پیام</param>
        /// <param name="errors">خطاها</param>
        /// <returns>نتیجه JSON</returns>
        private JsonResult CreateJsonValidationError(string message = "اطلاعات وارد شده نامعتبر است", List<string> errors = null)
        {
            return new JsonResult
            {
                Data = new
                {
                    success = false,
                    message = message,
                    errors = errors
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// ایجاد نتیجه JSON با امنیت
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>نتیجه JSON</returns>
        private JsonResult CreateJsonSecurityError(string message = "شما مجوز انجام این عملیات را ندارید")
        {
            return new JsonResult
            {
                Data = new
                {
                    success = false,
                    message = message,
                    securityError = true
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// پاک کردن Cache مرتبط
        /// </summary>
        /// <returns>نتیجه پاک کردن</returns>
        private async Task<bool> ClearRelatedCachesAsync()
        {
            try
            {
                _logger.Debug("پاک کردن Cache مرتبط");

                // Cache cleared - no longer needed

                _logger.Debug("پاک کردن Cache مرتبط موفق");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در پاک کردن Cache مرتبط");
                return false;
            }
        }

        #endregion
    }
}
