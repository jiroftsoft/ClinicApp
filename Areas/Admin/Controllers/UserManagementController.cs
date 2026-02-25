using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.UserManagement;
using ClinicApp.Models.Core;
using ClinicApp.ViewModels.UserManagement;
using Serilog;

namespace ClinicApp.Areas.Admin.Controllers
{
    /// <summary>
    /// کنترلر مدیریت کاربران سیستم
    /// طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// Architecture Principles Applied:
    /// ✅ Single Responsibility: فقط Routing و Orchestration
    /// ✅ Dependency Inversion: وابستگی به Interface ها
    /// ✅ Clean Architecture: Controller فقط View را مدیریت می‌کند
    /// ✅ Medical Standards: رعایت استانداردهای سیستم‌های پزشکی
    /// ✅ Security: Authorization کامل، Validation کامل
    /// 
    /// Flow: HTTP Request -> Controller -> Service -> Repository -> Database
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class UserManagementController : Controller
    {
        #region Fields and Constructor

        private readonly IUserManagementService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public UserManagementController(
            IUserManagementService userService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<UserManagementController>() ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Index & Listing

        /// <summary>
        /// نمایش لیست کاربران با قابلیت جستجو و فیلتر
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(UserSearchFilter filter = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست لیست کاربران - SearchTerm: {SearchTerm}, IsActive: {IsActive}, Role: {Role}, Page: {Page}. User: {UserId}",
                    filter?.SearchTerm, filter?.IsActive, filter?.RoleName, page, _currentUserService.UserId);

                // ✅ تنظیم مقادیر پیش‌فرض
                filter = filter ?? new UserSearchFilter();
                if (page < 1) page = 1;

                const int pageSize = 20;

                // ✅ دریافت داده از Service
                var result = await _userService.GetUsersAsync(filter, page, pageSize);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست کاربران - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(new UserIndexViewModel());
                }

                _logger.Information("لیست کاربران با موفقیت دریافت شد - Count: {Count}. User: {UserId}",
                    result.Data?.Users?.Count ?? 0, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست کاربران. User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست کاربران");
                return View(new UserIndexViewModel());
            }
        }

        #endregion

        #region DataTables API (سرور-ساید)

        /// <summary>
        /// دریافت داده‌های جدول کاربران برای DataTables (AJAX - سرور-ساید)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OutputCache(Duration = 0, NoStore = true)]
        public async Task<JsonResult> GetUsersData(UserManagementDataTablesRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { draw = 0, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = "درخواست نامعتبر است" });
                }

                var filter = new UserSearchFilter
                {
                    SearchTerm = !string.IsNullOrWhiteSpace(request.FilterSearchTerm) ? request.FilterSearchTerm.Trim() : request.Search?.Value?.Trim(),
                    IsActive = request.FilterIsActive,
                    RoleName = request.FilterRoleName
                };

                var length = request.Length > 0 ? request.Length : 10;
                _logger.Information("DataTables GetUsersData | Draw: {Draw}, Start: {Start}, Length: {Length}, SearchTerm: {SearchTerm}, IsActive: {IsActive}, RoleName: {RoleName}. User: {UserId}",
                    request.Draw, request.Start, length, string.IsNullOrEmpty(filter.SearchTerm) ? "(empty)" : "***", filter.IsActive?.ToString() ?? "all", filter.RoleName ?? "all", _currentUserService.UserId);

                var result = await _userService.GetUsersForDataTablesAsync(filter, request.Start, length);
                if (!result.Success)
                {
                    return Json(new { draw = request.Draw, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = result.Message });
                }

                var (recordsTotal, recordsFiltered, list) = result.Data;
                var data = list.Select(u => new
                {
                    userId = u.UserId,
                    fullName = u.FullName,
                    nationalCodeMasked = u.NationalCodeMasked,
                    email = u.Email ?? "",
                    phoneNumberMasked = u.PhoneNumberMasked,
                    rolesDisplay = u.RolesDisplay,
                    isActive = u.IsActive,
                    isActiveDisplay = u.IsActive ? "فعال" : "غیرفعال",
                    createdAtShamsi = u.CreatedAtShamsi ?? "",
                    actionsHtml = BuildActionsHtml(u),
                    hasDoctorRole = u.Roles != null && u.Roles.Contains(AppRoles.Doctor)
                }).ToList();

                return Json(new { draw = request.Draw, recordsTotal, recordsFiltered, data });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت داده‌های DataTables کاربران. User: {UserId}", _currentUserService.UserId);
                return Json(new { draw = request?.Draw ?? 0, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = "خطا در دریافت داده‌ها" });
            }
        }

        private string BuildActionsHtml(UserListItemViewModel u)
        {
            var detailsUrl = Url.Action("Details", "UserManagement", new { area = "Admin", id = u.UserId });
            var editUrl = Url.Action("Edit", "UserManagement", new { area = "Admin", id = u.UserId });
            var sb = new System.Text.StringBuilder();
            sb.Append("<div class='btn-group' role='group'>");
            sb.Append("<a href='").Append(detailsUrl).Append("' class='btn btn-sm btn-info' title='مشاهده'><i class='fas fa-eye'></i></a>");
            sb.Append("<a href='").Append(editUrl).Append("' class='btn btn-sm btn-warning' title='ویرایش'><i class='fas fa-edit'></i></a>");
            if (u.IsActive)
                sb.Append("<button type='button' class='btn btn-sm btn-secondary btn-deactivate' data-user-id='").Append(u.UserId).Append("' title='غیرفعال'><i class='fas fa-ban'></i></button>");
            else
                sb.Append("<button type='button' class='btn btn-sm btn-success btn-activate' data-user-id='").Append(u.UserId).Append("' title='فعال'><i class='fas fa-check'></i></button>");
            sb.Append("<button type='button' class='btn btn-sm btn-danger btn-delete' data-user-id='").Append(u.UserId).Append("' data-user-name='").Append(u.FullName?.Replace("'", "&#39;") ?? "").Append("' title='حذف'><i class='fas fa-trash'></i></button>");
            sb.Append("</div>");
            return sb.ToString();
        }

        #endregion

        #region Create

        /// <summary>
        /// نمایش فرم ایجاد کاربر جدید
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            try
            {
                _logger.Information("درخواست فرم ایجاد کاربر. User: {UserId}", _currentUserService.UserId);

                // ✅ دریافت نقش‌های موجود
                var rolesResult = await _userService.GetAvailableRolesAsync();
                if (!rolesResult.Success)
                {
                    NotificationHelper.SetError(TempData, "خطا در دریافت لیست نقش‌ها");
                    return RedirectToAction("Index");
                }

                var viewModel = new UserCreateEditViewModel
                {
                    IsActive = true,
                    AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                    {
                        Value = r.RoleName,
                        Text = r.DisplayName
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ایجاد کاربر. User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ایجاد کاربر");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست ایجاد کاربر - NationalCode: {NationalCode}, Email: {Email}. User: {UserId}",
                    MaskNationalCode(model?.NationalCode), MaskEmail(model?.Email), _currentUserService.UserId);

                // ✅ Validation
                if (!ModelState.IsValid)
                {
                    _logger.Warning("Validation failed برای ایجاد کاربر. User: {UserId}", _currentUserService.UserId);
                    
                    // ✅ بارگذاری مجدد نقش‌ها
                    var rolesResult = await _userService.GetAvailableRolesAsync();
                    if (rolesResult.Success)
                    {
                        model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                        {
                            Value = r.RoleName,
                            Text = r.DisplayName,
                            Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                        }).ToList();
                    }

                    NotificationHelper.SetError(TempData,"لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(model);
                }

                // ✅ ایجاد کاربر
                var result = await _userService.CreateUserAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در ایجاد کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);

                    // ✅ بارگذاری مجدد نقش‌ها
                    var rolesResult = await _userService.GetAvailableRolesAsync();
                    if (rolesResult.Success)
                    {
                        model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                        {
                            Value = r.RoleName,
                            Text = r.DisplayName,
                            Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                        }).ToList();
                    }

                    NotificationHelper.SetError(TempData,result.Message);
                    return View(model);
                }

                _logger.Information("کاربر جدید با موفقیت ایجاد شد - UserId: {UserId}, NationalCode: {NationalCode}. CreatedBy: {CreatedBy}",
                    result.Data?.Id, MaskNationalCode(result.Data?.NationalCode), _currentUserService.UserId);

                NotificationHelper.SetSuccess(TempData, "کاربر با موفقیت ایجاد شد.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد کاربر. User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطای سیستمی در ایجاد کاربر رخ داد.");

                // ✅ بارگذاری مجدد نقش‌ها
                var rolesResult = await _userService.GetAvailableRolesAsync();
                if (rolesResult.Success)
                {
                    model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                    {
                        Value = r.RoleName,
                        Text = r.DisplayName,
                        Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                    }).ToList();
                }

                return View(model);
            }
        }

        #endregion

        #region Edit

        /// <summary>
        /// نمایش فرم ویرایش کاربر
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                _logger.Information("درخواست ویرایش کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    id, _currentUserService.UserId);

                if (string.IsNullOrEmpty(id))
                {
                    NotificationHelper.SetError(TempData, "شناسه کاربر معتبر نیست.");
                    return RedirectToAction("Index");
                }

                // ✅ دریافت اطلاعات کاربر
                var result = await _userService.GetUserForEditAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت اطلاعات کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData,result.Message);
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش فرم ویرایش کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    id, _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری فرم ویرایش کاربر");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// به‌روزرسانی کاربر
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserCreateEditViewModel model)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی کاربر - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                    model?.UserId, _currentUserService.UserId);

                // ✅ Validation
                if (!ModelState.IsValid)
                {
                    _logger.Warning("Validation failed برای به‌روزرسانی کاربر. User: {UserId}", _currentUserService.UserId);

                    // ✅ بارگذاری مجدد نقش‌ها
                    var rolesResult = await _userService.GetAvailableRolesAsync();
                    if (rolesResult.Success)
                    {
                        model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                        {
                            Value = r.RoleName,
                            Text = r.DisplayName,
                            Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                        }).ToList();
                    }

                    NotificationHelper.SetError(TempData,"لطفاً تمام فیلدهای الزامی را پر کنید.");
                    return View(model);
                }

                // ✅ به‌روزرسانی کاربر
                var result = await _userService.UpdateUserAsync(model);

                if (!result.Success)
                {
                    _logger.Warning("خطا در به‌روزرسانی کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);

                    // ✅ بارگذاری مجدد نقش‌ها
                    var rolesResult = await _userService.GetAvailableRolesAsync();
                    if (rolesResult.Success)
                    {
                        model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                        {
                            Value = r.RoleName,
                            Text = r.DisplayName,
                            Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                        }).ToList();
                    }

                    NotificationHelper.SetError(TempData, result.Message);
                    return View(model);
                }

                _logger.Information("کاربر با موفقیت به‌روزرسانی شد - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                    model.UserId, _currentUserService.UserId);

                NotificationHelper.SetSuccess(TempData, "کاربر با موفقیت به‌روزرسانی شد.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی کاربر - UserId: {UserId}. UpdatedBy: {UpdatedBy}",
                    model?.UserId, _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطای سیستمی در به‌روزرسانی کاربر رخ داد.");

                // ✅ بارگذاری مجدد نقش‌ها
                var rolesResult = await _userService.GetAvailableRolesAsync();
                if (rolesResult.Success)
                {
                    model.AvailableRoles = rolesResult.Data.Select(r => new SelectListItem
                    {
                        Value = r.RoleName,
                        Text = r.DisplayName,
                        Selected = model.SelectedRoles != null && model.SelectedRoles.Contains(r.RoleName)
                    }).ToList();
                }

                return View(model);
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// نمایش جزئیات کاربر
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            try
            {
                _logger.Information("درخواست جزئیات کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    id, _currentUserService.UserId);

                if (string.IsNullOrEmpty(id))
                {
                    NotificationHelper.SetError(TempData, "شناسه کاربر معتبر نیست.");
                    return RedirectToAction("Index");
                }

                // ✅ دریافت جزئیات کاربر
                var result = await _userService.GetUserDetailsAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت جزئیات کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData,result.Message);
                    return RedirectToAction("Index");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش جزئیات کاربر - UserId: {UserId}. RequestedBy: {RequestedBy}",
                    id, _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری جزئیات کاربر");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// حذف نرم کاربر
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست حذف نرم کاربر - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    id, _currentUserService.UserId);

                if (string.IsNullOrEmpty(id))
                {
                    NotificationHelper.SetError(TempData, "شناسه کاربر معتبر نیست.");
                    return RedirectToAction("Index");
                }

                // پروداکشن درمانی: جلوگیری از حذف حساب خود
                if (string.Equals(id, _currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warning("تلاش برای حذف حساب خود - UserId: {UserId}", _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, "امکان حذف حساب خودتان وجود ندارد.");
                    return RedirectToAction("Index");
                }

                // ✅ حذف نرم
                var result = await _userService.DeleteUserAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("🏥 MEDICAL: خطا در حذف کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }

                _logger.Information("🏥 MEDICAL: کاربر با موفقیت حذف شد (Soft Delete) - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    id, _currentUserService.UserId);

                NotificationHelper.SetSuccess(TempData, "کاربر با موفقیت حذف شد.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در حذف کاربر - UserId: {UserId}. DeletedBy: {DeletedBy}",
                    id, _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطای سیستمی در حذف کاربر رخ داد.");
                return RedirectToAction("Index");
            }
        }

        #endregion

        #region Deleted Users Management

        /// <summary>
        /// نمایش لیست کاربران حذف شده
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> DeletedUsers(UserSearchFilter filter = null, int page = 1)
        {
            try
            {
                _logger.Information("درخواست لیست کاربران حذف شده - SearchTerm: {SearchTerm}, Role: {Role}, Page: {Page}. User: {UserId}",
                    filter?.SearchTerm, filter?.RoleName, page, _currentUserService.UserId);

                // ✅ تنظیم مقادیر پیش‌فرض
                filter = filter ?? new UserSearchFilter();
                if (page < 1) page = 1;

                const int pageSize = 20;

                // ✅ دریافت داده از Service
                var result = await _userService.GetDeletedUsersAsync(filter, page, pageSize);

                if (!result.Success)
                {
                    _logger.Warning("خطا در دریافت لیست کاربران حذف شده - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(new UserIndexViewModel());
                }

                _logger.Information("لیست کاربران حذف شده با موفقیت دریافت شد - Count: {Count}. User: {UserId}",
                    result.Data?.Users?.Count ?? 0, _currentUserService.UserId);

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش لیست کاربران حذف شده. User: {UserId}", _currentUserService.UserId);
                NotificationHelper.SetError(TempData, "خطا در بارگذاری لیست کاربران حذف شده");
                return View(new UserIndexViewModel());
            }
        }

        /// <summary>
        /// بازگردانی کاربر حذف شده (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Restore(string id)
        {
            try
            {
                _logger.Information("درخواست بازگردانی کاربر - UserId: {UserId}. RestoredBy: {RestoredBy}",
                    id, _currentUserService.UserId);

                if (string.IsNullOrEmpty(id))
                {
                    _logger.Warning("شناسه کاربر خالی است - RestoredBy: {RestoredBy}", _currentUserService.UserId);
                    return Json(new { success = false, message = "شناسه کاربر معتبر نیست." });
                }

                var result = await _userService.RestoreUserAsync(id);

                if (result == null)
                {
                    _logger.Error("نتیجه Service null است - UserId: {UserId}", id);
                    return Json(new { success = false, message = "خطای سیستمی رخ داد." });
                }

                if (!result.Success)
                {
                    _logger.Warning("خطا در بازگردانی کاربر - Message: {Message}, Code: {Code}. User: {UserId}",
                        result.Message, result.Code, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message ?? "خطا در بازگردانی کاربر" });
                }

                _logger.Information("کاربر با موفقیت بازگردانی شد - UserId: {UserId}, Message: {Message}. RestoredBy: {RestoredBy}",
                    id, result.Message, _currentUserService.UserId);

                return Json(new { success = true, message = result.Message ?? "کاربر با موفقیت بازگردانی شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازگردانی کاربر - UserId: {UserId}", id);
                return Json(new { success = false, message = "خطای سیستمی رخ داد." });
            }
        }

        #endregion

        #region Role Management

        /// <summary>
        /// اختصاص نقش به کاربر (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AssignRole(string userId, string roleName)
        {
            try
            {
                _logger.Information("درخواست اختصاص نقش - UserId: {UserId}, Role: {Role}. AssignedBy: {AssignedBy}",
                    userId, roleName, _currentUserService.UserId);

                var result = await _userService.AssignRoleAsync(userId, roleName);

                if (!result.Success)
                {
                    _logger.Warning("خطا در اختصاص نقش - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("نقش با موفقیت اختصاص داده شد - UserId: {UserId}, Role: {Role}. AssignedBy: {AssignedBy}",
                    userId, roleName, _currentUserService.UserId);

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اختصاص نقش - UserId: {UserId}, Role: {Role}",
                    userId, roleName);
                return Json(new { success = false, message = "خطای سیستمی رخ داد." });
            }
        }

        /// <summary>
        /// حذف نقش از کاربر (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveRole(string userId, string roleName)
        {
            try
            {
                _logger.Information("درخواست حذف نقش - UserId: {UserId}, Role: {Role}. RemovedBy: {RemovedBy}",
                    userId, roleName, _currentUserService.UserId);

                var result = await _userService.RemoveRoleAsync(userId, roleName);

                if (!result.Success)
                {
                    _logger.Warning("خطا در حذف نقش - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("نقش با موفقیت حذف شد - UserId: {UserId}, Role: {Role}. RemovedBy: {RemovedBy}",
                    userId, roleName, _currentUserService.UserId);

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نقش - UserId: {UserId}, Role: {Role}",
                    userId, roleName);
                return Json(new { success = false, message = "خطای سیستمی رخ داد." });
            }
        }

        #endregion

        #region Activation/Deactivation

        /// <summary>
        /// فعال‌سازی کاربر (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Activate(string id)
        {
            try
            {
                _logger.Information("درخواست فعال‌سازی کاربر - UserId: {UserId}. ActivatedBy: {ActivatedBy}",
                    id, _currentUserService.UserId);

                var result = await _userService.ActivateUserAsync(id);

                if (!result.Success)
                {
                    _logger.Warning("خطا در فعال‌سازی کاربر - Message: {Message}. User: {UserId}",
                        result.Message, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message });
                }

                _logger.Information("کاربر با موفقیت فعال شد - UserId: {UserId}. ActivatedBy: {ActivatedBy}",
                    id, _currentUserService.UserId);

                return Json(new { success = true, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در فعال‌سازی کاربر - UserId: {UserId}", id);
                return Json(new { success = false, message = "خطای سیستمی رخ داد." });
            }
        }

        /// <summary>
        /// غیرفعال‌سازی کاربر (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Deactivate(string id)
        {
            try
            {
                _logger.Information("درخواست غیرفعال‌سازی کاربر - UserId: {UserId}. DeactivatedBy: {DeactivatedBy}",
                    id, _currentUserService.UserId);

                if (string.IsNullOrEmpty(id))
                {
                    _logger.Warning("شناسه کاربر خالی است - DeactivatedBy: {DeactivatedBy}", _currentUserService.UserId);
                    return Json(new { success = false, message = "شناسه کاربر معتبر نیست." });
                }

                // پروداکشن درمانی: جلوگیری از غیرفعال‌سازی حساب خود
                if (string.Equals(id, _currentUserService.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.Warning("تلاش برای غیرفعال‌سازی حساب خود - UserId: {UserId}", _currentUserService.UserId);
                    return Json(new { success = false, message = "امکان غیرفعال‌سازی حساب خودتان وجود ندارد." });
                }

                var result = await _userService.DeactivateUserAsync(id);

                if (result == null)
                {
                    _logger.Error("نتیجه Service null است - UserId: {UserId}", id);
                    return Json(new { success = false, message = "خطای سیستمی رخ داد." });
                }

                if (!result.Success)
                {
                    _logger.Warning("خطا در غیرفعال‌سازی کاربر - Message: {Message}, Code: {Code}. User: {UserId}",
                        result.Message, result.Code, _currentUserService.UserId);
                    return Json(new { success = false, message = result.Message ?? "خطا در غیرفعال‌سازی کاربر" });
                }

                _logger.Information("کاربر با موفقیت غیرفعال شد - UserId: {UserId}, Message: {Message}. DeactivatedBy: {DeactivatedBy}",
                    id, result.Message, _currentUserService.UserId);

                return Json(new { success = true, message = result.Message ?? "کاربر با موفقیت غیرفعال شد." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در غیرفعال‌سازی کاربر - UserId: {UserId}", id);
                return Json(new { success = false, message = "خطای سیستمی رخ داد." });
            }
        }

        #endregion

        #region Validation (AJAX)

        /// <summary>
        /// بررسی معتبر بودن کد ملی (AJAX) — پروداکشن: با AntiForgery و Rate Limit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClinicApp.Filters.UserManagementValidationRateLimit(30, 1)]
        public async Task<JsonResult> ValidateNationalCode(string nationalCode, string excludeUserId = null)
        {
            try
            {
                var result = await _userService.ValidateNationalCodeAsync(nationalCode, excludeUserId);
                return Json(new { valid = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی کد ملی - NationalCode: {NationalCode}", MaskNationalCode(nationalCode));
                return Json(new { valid = false, message = "خطای سیستمی رخ داد." });
            }
        }

        /// <summary>
        /// بررسی معتبر بودن ایمیل (AJAX) — پروداکشن: با AntiForgery و Rate Limit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClinicApp.Filters.UserManagementValidationRateLimit(30, 1)]
        public async Task<JsonResult> ValidateEmail(string email, string excludeUserId = null)
        {
            try
            {
                var result = await _userService.ValidateEmailAsync(email, excludeUserId);
                return Json(new { valid = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی ایمیل - Email: {Email}", MaskEmail(email));
                return Json(new { valid = false, message = "خطای سیستمی رخ داد." });
            }
        }

        #endregion

        #region Logging Helpers (PII Masking برای پروداکشن درمانی)

        private static string MaskNationalCode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "***";
            if (value.Length < 4) return "****";
            return value.Substring(0, 2) + "***" + value.Substring(value.Length - 2);
        }

        private static string MaskEmail(string value)
        {
            if (string.IsNullOrEmpty(value)) return "***";
            var idx = value.IndexOf('@');
            if (idx <= 0) return "***@***";
            return value[0] + "***@" + (idx + 1 < value.Length ? value.Substring(idx + 1) : "***");
        }

        #endregion
    }
}

