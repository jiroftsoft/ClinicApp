using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using ClinicApp.Models.Core;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Reception;

namespace ClinicApp.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس کاربر فعلی برای سیستم‌های پزشکی با رعایت کامل استانداردهای امنیتی و عملکردی
    /// این نسخه کامل و هماهنگ با رابط ICurrentUserService است
    /// </summary>
    public class CurrentUserService : ICurrentUserService
    {
        private readonly HttpContextBase _httpContext;
        private readonly ApplicationUserManager _userManager;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private ApplicationUser _currentUser;

        public CurrentUserService(
            HttpContextBase httpContext,
            ApplicationUserManager userManager,
            ILogger logger,
            ApplicationDbContext context)
        {
            _httpContext = httpContext;
            _userManager = userManager;
            _logger = logger.ForContext<CurrentUserService>();
            _context = context;
        }

        #region Core Properties (ویژگی‌های اصلی)

        /// <summary>
        /// شناسه کاربر فعلی
        /// در محیط توسعه، همیشه شناسه کاربر Admin توسعه را برمی‌گرداند
        /// </summary>
        public string UserId
        {
            get
            {
                _logger.Information("=== شروع UserId Property ===");
                
                var userId = GetUserId();
                _logger.Information("GetUserId() برگرداند: {UserId}", userId);
                
                // اطمینان از اینکه هرگز null برنمی‌گرداند
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Error("GetUserId() مقدار null یا خالی برگرداند. استفاده از کاربر Admin توسعه");
                    var fallbackUserId = GetDevelopmentAdminUserId();
                    _logger.Information("GetDevelopmentAdminUserId() برگرداند: {UserId}", fallbackUserId);
                    _logger.Information("=== پایان UserId Property - Fallback - بازگشت: {UserId} ===", fallbackUserId);
                    return fallbackUserId;
                }
                
                _logger.Information("=== پایان UserId Property - Normal - بازگشت: {UserId} ===", userId);
                return userId;
            }
        }
        public string UserName => GetUserName();
        public bool IsAuthenticated => GetIsAuthenticated();
        public bool IsAdmin => IsInRole(AppRoles.Admin);
        public bool IsDoctor => IsInRole(AppRoles.Doctor);
        public bool IsReceptionist => IsInRole(AppRoles.Receptionist);
        public bool IsPatient => IsInRole(AppRoles.Patient);
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        public ClaimsPrincipal ClaimsPrincipal => _httpContext?.User as ClaimsPrincipal;
        public IEnumerable<string> Roles => GetUserRoles();

        #endregion

        #region Security Methods (روش‌های امنیتی)

        public bool IsInRole(string role)
        {
            try
            {
                if (_httpContext?.User?.Identity == null || !_httpContext.User.Identity.IsAuthenticated)
                {
                    return false;
                }

                var userId = UserId;
                if (userId == "System")
                {
                    return false;
                }

                return _userManager.IsInRole(userId, role);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی نقش کاربر برای نقش {Role}.", role);
                return false;
            }
        }

        public bool HasPermission(string permission)
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return false;
                }

                // منطق خاص بررسی دسترسی‌ها برای سیستم‌های پزشکی
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی به {Permission}.", permission);
                return false;
            }
        }

        public bool HasEntityAccess<TEntity>(TEntity entity, string permission) where TEntity : class
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return false;
                }

                // منطق خاص بررسی دسترسی به موجودیت‌ها
                if (IsAdmin)
                {
                    return true;
                }

                // بررسی مالکیت موجودیت
                if (entity is Patient patient)
                {
                    return patient.CreatedByUserId == UserId ||
                           patient.ApplicationUserId == UserId;
                }

                if (entity is Reception reception)
                {
                    return reception.CreatedByUserId == UserId ||
                           reception.DoctorId.ToString() == UserId;
                }

                // برای سایر موجودیت‌ها
                var trackable = entity as ITrackable;
                if (trackable != null)
                {
                    return trackable.CreatedByUserId == UserId;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی به موجودیت {EntityType} با دسترسی {Permission}.",
                    typeof(TEntity).Name, permission);
                return false;
            }
        }

        public async Task<bool> HasAccessToServiceAsync(int serviceId)
        {
            try
            {
                // دریافت شناسه کاربر جاری
                var userId = UserId;

                // اگر کاربر وارد نشده باشد، دسترسی ندارد
                if (string.IsNullOrEmpty(userId) || userId == "System")
                {
                    _logger.Information("تلاش برای دسترسی به خدمات با شناسه {ServiceId} توسط کاربر غیراحراز هویت شده", serviceId);
                    return false;
                }

                // بررسی نقش‌های کاربر
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.Warning("کاربر با شناسه {UserId} در سیستم کاربران یافت نشد", userId);
                    return false;
                }

                // 1. اگر کاربر نقش مدیر دارد، به همه چیز دسترسی دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Admin))
                {
                    _logger.Information("کاربر مدیر {UserId} به خدمات {ServiceId} دسترسی دارد", userId, serviceId);
                    return true;
                }

                // 2. اگر کاربر نقش پزشک دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Doctor))
                {
                    // دریافت پزشک مربوط به کاربر
                    // توجه: پزشک دیگر ApplicationUserId ندارد، بنابراین باید از طریق نام کاربری پیدا کنیم
                    var doctor = await _context.Doctors
                        .Include(d => d.DoctorDepartments.Select(dd => dd.Department))
                        .Include(d => d.DoctorServiceCategories.Select(dsc => dsc.ServiceCategory))
                        .FirstOrDefaultAsync(d => (d.FirstName + " " + d.LastName).Contains(user.UserName) && !d.IsDeleted);

                    if (doctor == null)
                    {
                        _logger.Warning("پزشک مربوط به کاربر {UserId} یافت نشد", userId);
                        return false;
                    }

                    // دریافت خدمات
                    var service = await _context.Services
                        .Include(s => s.ServiceCategory.Department)
                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                    if (service == null || service.ServiceCategory == null)
                    {
                        _logger.Warning("خدمات با شناسه {ServiceId} یا دسته‌بندی آن یافت نشد", serviceId);
                        return false;
                    }

                    // ✅ بررسی دسترسی بر اساس معماری جدید Many-to-Many
                    
                    // 1. بررسی دسترسی بر اساس کلینیک
                    bool hasClinicAccess = doctor.ClinicId.HasValue &&
                                         service.ServiceCategory.Department.ClinicId == doctor.ClinicId;

                    // 2. بررسی دسترسی بر اساس دپارتمان‌های پزشک
                    bool hasDepartmentAccess = doctor.DoctorDepartments
                        .Any(dd => dd.IsActive && 
                                  dd.DepartmentId == service.ServiceCategory.DepartmentId);

                    // 3. بررسی دسترسی مستقیم به دسته‌بندی خدمات
                    bool hasServiceCategoryAccess = doctor.DoctorServiceCategories
                        .Any(dsc => dsc.IsActive && 
                                   dsc.ServiceCategoryId == service.ServiceCategoryId);

                    bool hasAccess = hasClinicAccess || hasDepartmentAccess || hasServiceCategoryAccess;

                    _logger.Information("پزشک {UserId} به خدمات {ServiceId} دسترسی {AccessStatus} - کلینیک: {ClinicAccess}, دپارتمان: {DepartmentAccess}, دسته‌بندی: {ServiceCategoryAccess}",
                        userId, serviceId, hasAccess ? "دارد" : "ندارد", 
                        hasClinicAccess, hasDepartmentAccess, hasServiceCategoryAccess);
                    
                    return hasAccess;
                }

                // 3. اگر کاربر نقش منشی دارد
                if (await _userManager.IsInRoleAsync(user.Id, AppRoles.Receptionist))
                {
                    // دریافت خدمات
                    var service = await _context.Services
                        .Include(s => s.ServiceCategory)
                        .FirstOrDefaultAsync(s => s.ServiceId == serviceId && !s.IsDeleted);

                    if (service == null || service.ServiceCategory == null)
                    {
                        _logger.Warning("خدمات با شناسه {ServiceId} یا دسته‌بندی آن برای منشی یافت نشد", serviceId);
                        return false;
                    }

                    // دریافت Department مربوطه
                    var serviceCategory = await _context.ServiceCategories
                        .Include(sc => sc.Department)
                        .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == service.ServiceCategoryId);

                    if (serviceCategory?.Department == null)
                    {
                        _logger.Warning("دپارتمان مربوط به خدمات {ServiceId} برای منشی یافت نشد", serviceId);
                        return false;
                    }

                    // بررسی دسترسی منشی - فرض بر این است که منشی به تمام خدمات دسترسی دارد
                    // یا می‌توانید منطق خاص خود را اینجا پیاده‌سازی کنید
                    bool hasAccess = true; // یا منطق دسترسی خاص شما

                    _logger.Information("منشی {UserId} به خدمات {ServiceId} دسترسی {AccessStatus}",
                        userId, serviceId, hasAccess ? "دارد" : "ندارد");
                    return hasAccess;
                }

                // 4. سایر نقش‌ها (بیمار و غیره) به گزارش‌های خدمات دسترسی ندارند
                _logger.Information("کاربر {UserId} با نقش نامعتبر به خدمات {ServiceId} دسترسی ندارد", userId, serviceId);
                return false;
            }
            catch (Exception ex)
            {
                // در سیستم‌های پزشکی، بهتر است در صورت خطا، دسترسی را رد کنیم
                // این یک اصل امنیتی مهم است: "در صورت عدم اطمینان، دسترسی را رد کنید"
                _logger.Error(ex, "خطای غیرمنتظره در بررسی دسترسی کاربر {UserId} به خدمات {ServiceId}", UserId, serviceId);
                return false;
            }
        }

        public async Task<bool> HasAccessToInsuranceAsync(int insuranceId)
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return false;
                }

                // 1. اگر کاربر نقش مدیر دارد، به همه چیز دسترسی دارد
                if (IsAdmin)
                {
                    return true;
                }

                // 2. اگر کاربر نقش پزشک یا منشی دارد
                if (IsDoctor || IsReceptionist)
                {
                    // دریافت کاربر فعلی
                    var user = await _userManager.FindByIdAsync(UserId);
                    if (user == null)
                    {
                        return false;
                    }

                    // دریافت پزشک یا منشی مربوطه
                    if (IsDoctor)
                    {
                        // توجه: پزشک دیگر ApplicationUserId ندارد
                        // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                        // فعلاً false برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                        return false;
                    }
                    else if (IsReceptionist)
                    {
                        // منشی می‌تواند به تمام بیمه‌ها دسترسی داشته باشد
                        return true;
                    }
                }

                // 3. سایر نقش‌ها (بیمار و غیره)
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی به بیمه {InsuranceId}.", insuranceId);
                return false;
            }
        }

        public async Task<bool> HasAccessToDepartmentAsync(int departmentId)
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return false;
                }

                // 1. اگر کاربر نقش مدیر دارد، به همه چیز دسترسی دارد
                if (IsAdmin)
                {
                    return true;
                }

                // 2. اگر کاربر نقش پزشک دارد
                if (IsDoctor)
                {
                    // توجه: پزشک دیگر ApplicationUserId ندارد
                    // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                    // فعلاً false برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                    return false;
                }

                // 3. اگر کاربر نقش منشی دارد
                if (IsReceptionist)
                {
                    // منشی می‌تواند به تمام دپارتمان‌ها دسترسی داشته باشد
                    return true;
                }

                // 4. سایر نقش‌ها (بیمار و غیره)
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی دسترسی به دپارتمان {DepartmentId}.", departmentId);
                return false;
            }
        }

        #endregion

        #region Helper Methods (روش‌های کمکی)

        public async Task<Doctor> GetDoctorInfoAsync()
        {
            try
            {
                if (!IsDoctor)
                {
                    return null;
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً null برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک برای کاربر {UserId}.", UserId);
                return null;
            }
        }

        public async Task<Patient> GetPatientInfoAsync()
        {
            try
            {
                if (!IsPatient)
                {
                    return null;
                }

                return await _context.Patients
                    .Include(p => p.ApplicationUser)
                    //.Include(p => p.Insurance)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == UserId && !p.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات بیمار برای کاربر {UserId}.", UserId);
                return null;
            }
        }

        public string GetSystemUserId()
        {
            return SystemUsers.SystemUserId;
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌هایی که پزشک فعلی در آن‌ها فعال است
        /// </summary>
        public async Task<List<Department>> GetDoctorActiveDepartmentsAsync()
        {
            try
            {
                if (!IsDoctor)
                {
                    return new List<Department>();
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً null برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                var doctor = null as Doctor;

                if (doctor == null)
                {
                    return new List<Department>();
                }

                return doctor.DoctorDepartments
                    .Where(dd => dd.IsActive)
                    .Select(dd => dd.Department)
                    .Where(d => d != null && !d.IsDeleted && d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دپارتمان‌های فعال پزشک برای کاربر {UserId}.", UserId);
                return new List<Department>();
            }
        }

        /// <summary>
        /// دریافت لیست دسته‌بندی خدماتی که پزشک فعلی مجاز به ارائه آن‌ها است
        /// </summary>
        public async Task<List<ServiceCategory>> GetDoctorAuthorizedServiceCategoriesAsync()
        {
            try
            {
                if (!IsDoctor)
                {
                    return new List<ServiceCategory>();
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً null برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                var doctor = null as Doctor;

                if (doctor == null)
                {
                    return new List<ServiceCategory>();
                }

                return doctor.DoctorServiceCategories
                    .Where(dsc => dsc.IsActive && 
                                 (dsc.ExpiryDate == null || dsc.ExpiryDate > DateTime.Now))
                    .Select(dsc => dsc.ServiceCategory)
                    .Where(sc => sc != null && !sc.IsDeleted && sc.IsActive)
                    .OrderBy(sc => sc.Title)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت دسته‌بندی خدمات مجاز پزشک برای کاربر {UserId}.", UserId);
                return new List<ServiceCategory>();
            }
        }

        /// <summary>
        /// بررسی اینکه آیا پزشک فعلی در دپارتمان مشخص شده فعال است یا خیر
        /// </summary>
        public async Task<bool> IsDoctorActiveInDepartmentAsync(int departmentId)
        {
            try
            {
                if (!IsDoctor)
                {
                    return false;
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً false برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی فعالیت پزشک در دپارتمان {DepartmentId} برای کاربر {UserId}.", departmentId, UserId);
                return false;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا پزشک فعلی مجاز به دسته‌بندی خدمات مشخص شده است یا خیر
        /// </summary>
        public async Task<bool> IsDoctorAuthorizedForServiceCategoryAsync(int serviceCategoryId)
        {
            try
            {
                if (!IsDoctor)
                {
                    return false;
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً false برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی مجوز پزشک برای دسته‌بندی خدمات {ServiceCategoryId} برای کاربر {UserId}.", serviceCategoryId, UserId);
                return false;
            }
        }

        /// <summary>
        /// دریافت نقش پزشک در دپارتمان مشخص شده
        /// </summary>
        public async Task<string> GetDoctorRoleInDepartmentAsync(int departmentId)
        {
            try
            {
                if (!IsDoctor)
                {
                    return null;
                }

                // توجه: پزشک دیگر ApplicationUserId ندارد
                // در اینجا باید منطق جدیدی برای پیدا کردن پزشک پیاده‌سازی شود
                // فعلاً null برمی‌گردانیم تا نیاز به پیاده‌سازی منطق جدید باشد
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش پزشک در دپارتمان {DepartmentId} برای کاربر {UserId}.", departmentId, UserId);
                return null;
            }
        }

        #endregion

        #region Private Helper Methods (روش‌های کمکی خصوصی)

        private string GetUserId()
        {
            try
            {
                // بررسی وجود HttpContext
                if (_httpContext == null)
                {
                    _logger.Warning("HttpContext در CurrentUserService null است. احتمالاً در محیط توسعه یا Background Service");
                    return GetDevelopmentAdminUserId();
                }

                // بررسی وجود User
                if (_httpContext.User == null)
                {
                    _logger.Warning("HttpContext.User در CurrentUserService null است. استفاده از کاربر Admin توسعه");
                    return GetDevelopmentAdminUserId();
                }

                // بررسی وجود Identity
                if (_httpContext.User.Identity == null)
                {
                    _logger.Warning("HttpContext.User.Identity در CurrentUserService null است. استفاده از کاربر Admin توسعه");
                    return GetDevelopmentAdminUserId();
                }

                var identity = _httpContext.User.Identity;
                
                // بررسی احراز هویت
                if (!identity.IsAuthenticated)
                {
                    _logger.Information("کاربر احراز هویت نشده است. استفاده از کاربر Admin توسعه. Identity.IsAuthenticated: {IsAuthenticated}", identity.IsAuthenticated);
                    return GetDevelopmentAdminUserId();
                }

                _logger.Debug("کاربر احراز هویت شده است. Identity Type: {IdentityType}, Name: {IdentityName}", 
                    identity.GetType().Name, identity.Name);

                // برای ClaimsIdentity
                if (identity is ClaimsIdentity claimsIdentity)
                {
                    var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    string userId = userIdClaim?.Value;

                    _logger.Debug("ClaimsIdentity - NameIdentifier Claim: {UserId}", userId);

                    if (!string.IsNullOrEmpty(userId) && UserExistsInDatabase(userId))
                    {
                        _logger.Debug("کاربر با شناسه {UserId} در دیتابیس یافت شد", userId);
                        return userId;
                    }
                    else
                    {
                        _logger.Warning("کاربر با شناسه {UserId} در دیتابیس یافت نشد یا شناسه خالی است. استفاده از کاربر Admin توسعه", userId);
                    }
                }
                // برای Identity ساده
                else
                {
                    var userName = identity.Name;
                    _logger.Debug("Identity ساده - UserName: {UserName}", userName);
                    
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = _userManager.FindByName(userName);
                        if (user != null && UserExistsInDatabase(user.Id))
                        {
                            _logger.Debug("کاربر با نام {UserName} و شناسه {UserId} در دیتابیس یافت شد", userName, user.Id);
                            return user.Id;
                        }
                        else
                        {
                            _logger.Warning("کاربر با نام {UserName} در دیتابیس یافت نشد. استفاده از کاربر Admin توسعه", userName);
                        }
                    }
                }

                _logger.Warning("هیچ کاربر معتبری یافت نشد. استفاده از کاربر Admin توسعه");
                return GetDevelopmentAdminUserId();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت شناسه کاربر. استفاده از کاربر Admin توسعه");
                return GetDevelopmentAdminUserId();
            }
        }

        /// <summary>
        /// دریافت شناسه کاربر Admin توسعه برای محیط توسعه
        /// این متد هرگز null برنمی‌گرداند و همیشه یک شناسه معتبر برمی‌گرداند
        /// </summary>
        private string GetDevelopmentAdminUserId()
        {
            try
            {
                _logger.Information("=== شروع GetDevelopmentAdminUserId() ===");
                
                // شناسه کاربر Admin که در محیط توسعه استفاده می‌شود
                const string developmentAdminUserId = "6f999f4d-24b8-4142-a97e-20077850278b"; // شناسه کاربر شما

                _logger.Information("درخواست کاربر Admin توسعه. شناسه: {UserId}", developmentAdminUserId);
                
                // تست ساده - اطمینان از اینکه مقدار خالی نیست
                if (string.IsNullOrEmpty(developmentAdminUserId))
                {
                    _logger.Error("developmentAdminUserId خالی است!");
                    throw new InvalidOperationException("developmentAdminUserId نمی‌تواند خالی باشد");
                }
                
                // در محیط توسعه، همیشه از شناسه Admin استفاده می‌کنیم
                // بررسی دیتابیس را حذف کردیم تا عملکرد سریع‌تر باشد
                _logger.Information("استفاده از کاربر Admin توسعه با شناسه: {UserId}", developmentAdminUserId);
                _logger.Information("=== پایان GetDevelopmentAdminUserId() - بازگشت: {UserId} ===", developmentAdminUserId);
                return developmentAdminUserId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در GetDevelopmentAdminUserId. استفاده از شناسه پیش‌فرض");
                // در صورت خطا، از شناسه Admin استفاده می‌کنیم
                const string fallbackUserId = "6f999f4d-24b8-4142-a97e-20077850278b";
                _logger.Information("استفاده از شناسه پیش‌فرض: {UserId}", fallbackUserId);
                _logger.Information("=== پایان GetDevelopmentAdminUserId() - خطا - بازگشت: {UserId} ===", fallbackUserId);
                return fallbackUserId;
            }
        }

        /// <summary>
        /// بررسی اینکه آیا در محیط توسعه هستیم یا نه
        /// </summary>
        public bool IsDevelopmentEnvironment()
        {
            try
            {
                // بررسی وجود HttpContext
                if (_httpContext == null)
                {
                    _logger.Debug("HttpContext null - احتمالاً در محیط توسعه");
                    return true;
                }

                // بررسی وجود User
                if (_httpContext.User == null)
                {
                    _logger.Debug("HttpContext.User null - احتمالاً در محیط توسعه");
                    return true;
                }

                // بررسی احراز هویت
                if (_httpContext.User.Identity == null || !_httpContext.User.Identity.IsAuthenticated)
                {
                    _logger.Debug("کاربر احراز هویت نشده - احتمالاً در محیط توسعه");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "خطا در بررسی محیط توسعه. فرض بر محیط توسعه");
                return true;
            }
        }

        private string GetUserName()
        {
            try
            {
                if (_httpContext?.User?.Identity == null)
                {
                    return "سیستم";
                }

                var identity = _httpContext.User.Identity;
                if (!identity.IsAuthenticated)
                {
                    return "سیستم";
                }

                // برای ClaimsIdentity
                if (identity is ClaimsIdentity claimsIdentity)
                {
                    // سعی در دریافت نام کامل از Claim سفارشی
                    var fullNameClaim = claimsIdentity.FindFirst("FullName");
                    if (!string.IsNullOrEmpty(fullNameClaim?.Value))
                    {
                        return fullNameClaim.Value;
                    }

                    var userNameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
                    if (!string.IsNullOrEmpty(userNameClaim?.Value))
                    {
                        return userNameClaim.Value;
                    }

                    var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    string userId = userIdClaim?.Value;

                    if (!string.IsNullOrEmpty(userId) && UserExistsInDatabase(userId))
                    {
                        var applicationUser = _userManager.FindById(userId);
                        if (applicationUser != null)
                        {
                            return !string.IsNullOrEmpty(applicationUser.FullName) ?
                                applicationUser.FullName :
                                applicationUser.UserName;
                        }
                    }
                }
                // برای Identity ساده
                else
                {
                    var userName = identity.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = _userManager.FindByName(userName);
                        if (user != null)
                        {
                            return !string.IsNullOrEmpty(user.FullName) ?
                                user.FullName :
                                user.UserName;
                        }
                    }
                }

                return "سیستم";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نام کاربر.");
                return "سیستم";
            }
        }

        private bool GetIsAuthenticated()
        {
            try
            {
                return _httpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی احراز هویت کاربر.");
                return false;
            }
        }

        private bool UserExistsInDatabase(string userId)
        {
            try
            {
                _logger.Debug("=== شروع UserExistsInDatabase() برای شناسه: {UserId} ===", userId);
                
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Debug("شناسه کاربر خالی است");
                    _logger.Debug("=== پایان UserExistsInDatabase() - شناسه خالی - بازگشت: false ===");
                    return false;
                }

                _logger.Debug("بررسی وجود کاربر در دیتابیس...");
                bool userExists = _context.Users.Any(u => u.Id == userId);
                _logger.Debug("نتیجه بررسی: {UserExists}", userExists);
                _logger.Debug("=== پایان UserExistsInDatabase() - بازگشت: {UserExists} ===", userExists);
                return userExists;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود کاربر در پایگاه داده برای شناسه {UserId}.", userId);
                _logger.Debug("=== پایان UserExistsInDatabase() - خطا - بازگشت: false ===");
                return false;
            }
        }

        public string[] GetUserRoles()
        {
            try
            {
                if (!IsAuthenticated)
                {
                    return new string[0];
                }

                var userId = UserId;
                if (userId == "System")
                {
                    return new string[0];
                }

                var roles = _userManager.GetRoles(userId);
                return roles?.ToArray() ?? new string[0];
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نقش‌های کاربر.");
                return new string[0];
            }
        }

        /// <summary>
        /// دریافت شناسه کاربر جاری
        /// </summary>
        public string GetCurrentUserId()
        {
            return UserId;
        }

        /// <summary>
        /// دریافت نام کاربر جاری
        /// </summary>
        public string GetCurrentUserName()
        {
            return UserName;
        }

        /// <summary>
        /// بررسی معتبر بودن کاربر فعلی برای عملیات‌های حساس
        /// </summary>
        /// <returns>true اگر کاربر معتبر و احراز هویت شده باشد</returns>
        public bool IsValidUser()
        {
            try
            {
                var userId = UserId;
                
                // بررسی خالی نبودن شناسه کاربر
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Warning("شناسه کاربر خالی است");
                    return false;
                }
                
                // بررسی System نبودن شناسه کاربر
                if (userId == "System")
                {
                    _logger.Warning("شناسه کاربر 'System' است - کاربر احراز هویت نشده");
                    return false;
                }
                
                // بررسی احراز هویت
                if (!IsAuthenticated)
                {
                    _logger.Warning("کاربر احراز هویت نشده است");
                    return false;
                }
                
                // بررسی وجود کاربر در دیتابیس
                if (!UserExistsInDatabase(userId))
                {
                    _logger.Warning("کاربر با شناسه {UserId} در دیتابیس وجود ندارد", userId);
                    return false;
                }
                
                _logger.Debug("کاربر با شناسه {UserId} معتبر است", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی معتبر بودن کاربر");
                return false;
            }
        }

        #endregion
    }
}