using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Constants;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Security;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Security;
using ClinicApp.ViewModels.Admin.Security;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers.Security
{
    /// <summary>
    /// کنترلر مدیریت تاریخچه ورود کاربران
    /// 
    /// Single Responsibility: مدیریت نمایش و فیلتر تاریخچه ورودها
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: فقط مدیریت نمایش Login History
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// ✅ Clean Architecture: Controller فقط View را مدیریت می‌کند
    /// ✅ Security: Authorization کامل
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    [Authorize]
    public class LoginHistoryController : Controller
    {
        #region Fields and Constructor

        private readonly ILoginHistoryService _loginHistoryService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public LoginHistoryController(
            ILoginHistoryService loginHistoryService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _loginHistoryService = loginHistoryService ?? throw new ArgumentNullException(nameof(loginHistoryService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<LoginHistoryController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Index & Listing

        /// <summary>
        /// نمایش لیست تاریخچه ورودها با قابلیت فیلتر و جستجو
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(LoginHistoryFilterViewModel filter = null)
        {
            try
            {
                _logger.Information("درخواست نمایش لیست تاریخچه ورودها توسط کاربر {UserId}", _currentUserService.UserId);

                // ✅ تنظیم فیلتر پیش‌فرض اگر null باشد
                if (filter == null)
                {
                    filter = new LoginHistoryFilterViewModel
                    {
                        StartDate = DateTime.Now.AddDays(-30), // پیش‌فرض: 30 روز گذشته
                        EndDate = DateTime.Now
                    };
                }

                // ✅ Parse تاریخ‌های شمسی از Query String (برای GET request)
                var startDateQuery = Request.QueryString["StartDate"];
                var endDateQuery = Request.QueryString["EndDate"];

                if (!string.IsNullOrEmpty(startDateQuery))
                {
                    var parsedStartDate = this.ParseDateFromHiddenInput("StartDate", _logger);
                    if (parsedStartDate.HasValue)
                    {
                        filter.StartDate = parsedStartDate.Value;
                    }
                }

                if (!string.IsNullOrEmpty(endDateQuery))
                {
                    var parsedEndDate = this.ParseDateFromHiddenInput("EndDate", _logger);
                    if (parsedEndDate.HasValue)
                    {
                        filter.EndDate = parsedEndDate.Value;
                    }
                }

                // ✅ دریافت لیست تاریخچه ورودها
                var result = await GetFilteredLoginHistoryAsync(filter);

                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا در دریافت لیست");
                    return View(GetViewPath("Index"), new PagedResult<LoginHistoryIndexViewModel>(new System.Collections.Generic.List<LoginHistoryIndexViewModel>(), 0, 1, 20));
                }

                // ✅ تبدیل به ViewModel
                var viewModels = result.Data.Select(ConvertToViewModel).ToList();
                var pagedResult = new PagedResult<LoginHistoryIndexViewModel>(viewModels, viewModels.Count, 1, viewModels.Count);

                ViewBag.Filter = filter;
                _logger.Information("لیست تاریخچه ورودها با موفقیت بازیابی شد. Count: {Count}", viewModels.Count);

                return View(GetViewPath("Index"), pagedResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست تاریخچه ورودها");
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست تاریخچه ورودها", "خطا");
                return View(GetViewPath("Index"), new PagedResult<LoginHistoryIndexViewModel>(new System.Collections.Generic.List<LoginHistoryIndexViewModel>(), 0, 1, 20));
            }
        }

        /// <summary>
        /// دریافت لیست فیلتر شده تاریخچه ورودها (AJAX)
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> GetFilteredList(LoginHistoryFilterViewModel filter)
        {
            try
            {
                if (filter == null)
                {
                    filter = new LoginHistoryFilterViewModel
                    {
                        StartDate = DateTime.Now.AddDays(-30),
                        EndDate = DateTime.Now
                    };
                }

                var result = await GetFilteredLoginHistoryAsync(filter);

                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                var viewModels = result.Data.Select(ConvertToViewModel).ToList();

                return Json(new
                {
                    success = true,
                    data = viewModels,
                    total = viewModels.Count
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست فیلتر شده تاریخچه ورودها");
                return Json(new { success = false, message = "خطا در دریافت لیست" });
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات یک ورود
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                _logger.Information("درخواست نمایش جزئیات ورود {Id} توسط کاربر {UserId}", id, _currentUserService.UserId);

                var result = await _loginHistoryService.GetLoginHistoryByIdAsync(id);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message, "خطا");
                    return RedirectToAction("Index");
                }

                var viewModel = ConvertToViewModel(result.Data);

                return View(GetViewPath("Details"), viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات ورود {Id}", id);
                NotificationHelper.SetError(TempData, "خطا در نمایش جزئیات", "خطا");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// دریافت لیست فیلتر شده تاریخچه ورودها
        /// </summary>
        private async Task<ServiceResult<System.Collections.Generic.List<UserLoginHistory>>> GetFilteredLoginHistoryAsync(LoginHistoryFilterViewModel filter)
        {
            try
            {
                // دریافت تمام ورودهای اخیر (برای فیلتر کردن در Memory - در Production باید Repository با فیلتر ایجاد شود)
                var result = await _loginHistoryService.GetRecentLoginsAsync(10000); // حداکثر 10000 رکورد

                if (!result.Success)
                {
                    return ServiceResult<System.Collections.Generic.List<UserLoginHistory>>.Failed(result.Message);
                }

                var filtered = result.Data.AsQueryable();

                // فیلتر بر اساس UserId
                if (!string.IsNullOrWhiteSpace(filter.UserId))
                {
                    filtered = filtered.Where(l => l.UserId == filter.UserId);
                }

                // فیلتر بر اساس IP
                if (!string.IsNullOrWhiteSpace(filter.IpAddress))
                {
                    filtered = filtered.Where(l => l.IpAddress != null && l.IpAddress.Contains(filter.IpAddress));
                }

                // فیلتر بر اساس تاریخ شروع
                if (filter.StartDate.HasValue)
                {
                    filtered = filtered.Where(l => l.LoginTime >= filter.StartDate.Value);
                }

                // فیلتر بر اساس تاریخ پایان
                if (filter.EndDate.HasValue)
                {
                    filtered = filtered.Where(l => l.LoginTime <= filter.EndDate.Value.AddDays(1)); // شامل کل روز
                }

                // فیلتر بر اساس وضعیت موفقیت
                if (filter.IsSuccessful.HasValue)
                {
                    filtered = filtered.Where(l => l.IsSuccessful == filter.IsSuccessful.Value);
                }

                // فیلتر بر اساس نوع دستگاه
                if (!string.IsNullOrWhiteSpace(filter.DeviceType))
                {
                    filtered = filtered.Where(l => l.DeviceType == filter.DeviceType);
                }

                // فیلتر بر اساس مرورگر
                if (!string.IsNullOrWhiteSpace(filter.BrowserName))
                {
                    filtered = filtered.Where(l => l.BrowserName == filter.BrowserName);
                }

                // فیلتر بر اساس سیستم عامل
                if (!string.IsNullOrWhiteSpace(filter.OSName))
                {
                    filtered = filtered.Where(l => l.OSName == filter.OSName);
                }

                // جستجو در UserAgent و IP
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    filtered = filtered.Where(l =>
                        (l.UserAgent != null && l.UserAgent.ToLower().Contains(searchTerm)) ||
                        (l.IpAddress != null && l.IpAddress.Contains(searchTerm)) ||
                        (l.FailureReason != null && l.FailureReason.ToLower().Contains(searchTerm))
                    );
                }

                var filteredList = filtered.OrderByDescending(l => l.LoginTime).ToList();

                return ServiceResult<System.Collections.Generic.List<UserLoginHistory>>.Successful(filteredList);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فیلتر کردن تاریخچه ورودها");
                return ServiceResult<System.Collections.Generic.List<UserLoginHistory>>.Failed("خطا در فیلتر کردن داده‌ها");
            }
        }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        private LoginHistoryIndexViewModel ConvertToViewModel(UserLoginHistory entity)
        {
            return new LoginHistoryIndexViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserFullName = entity.User?.FullName ?? "نامشخص",
                UserNationalCode = entity.User?.NationalCode ?? "نامشخص",
                LoginTime = entity.LoginTime,
                LoginTimeShamsi = entity.LoginTime.ToPersianDateTime(),
                LogoutTime = entity.LogoutTime,
                LogoutTimeShamsi = entity.LogoutTime?.ToPersianDateTime(),
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                DeviceType = entity.DeviceType ?? "نامشخص",
                BrowserName = entity.BrowserName ?? "نامشخص",
                BrowserVersion = entity.BrowserVersion ?? "نامشخص",
                OSName = entity.OSName ?? "نامشخص",
                OSVersion = entity.OSVersion ?? "نامشخص",
                Location = entity.Location,
                IsSuccessful = entity.IsSuccessful,
                FailureReason = entity.FailureReason,
                SessionId = entity.SessionId,
                CreatedAt = entity.CreatedAt
            };
        }

        /// <summary>
        /// دریافت مسیر View با در نظر گیری پوشه Security
        /// </summary>
        /// <param name="viewName">نام View (مثلاً "Index", "Details")</param>
        /// <returns>مسیر کامل View</returns>
        protected string GetViewPath(string viewName)
        {
            // ✅ Controller در پوشه Security است
            return $"~/Areas/Admin/Views/Security/LoginHistory/{viewName}.cshtml";
        }

        #endregion
    }
}

