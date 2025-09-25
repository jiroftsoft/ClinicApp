using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using ClinicApp.Services.UserContext;
using Serilog;

namespace ClinicApp.Services.UserContext
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت Context کاربر و کلینیک
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly ILogger _logger;

        public UserContextService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت شناسه کلینیک فعلی کاربر
        /// </summary>
        public async Task<int> GetCurrentClinicIdAsync()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت ClinicId - CorrelationId: {CorrelationId}", correlationId);

                // دریافت از Claims
                var claimsIdentity = HttpContext.Current?.User?.Identity as System.Security.Claims.ClaimsIdentity;
                var clinicIdClaim = claimsIdentity?.FindFirst("ClinicId");
                if (clinicIdClaim != null && int.TryParse(clinicIdClaim.Value, out int clinicId))
                {
                    _logger.Information("🏥 MEDICAL: ClinicId از Claims دریافت شد - ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                        clinicId, correlationId);
                    return clinicId;
                }

                // دریافت از Session
                if (HttpContext.Current?.Session?["CurrentClinicId"] != null)
                {
                    var sessionClinicId = HttpContext.Current.Session["CurrentClinicId"];
                    if (int.TryParse(sessionClinicId?.ToString(), out int sessionClinicIdValue))
                    {
                        _logger.Information("🏥 MEDICAL: ClinicId از Session دریافت شد - ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                            sessionClinicIdValue, correlationId);
                        return sessionClinicIdValue;
                    }
                }

                // مقدار پیش‌فرض برای محیط توسعه
                _logger.Warning("🏥 MEDICAL: ClinicId یافت نشد - استفاده از مقدار پیش‌فرض - CorrelationId: {CorrelationId}", correlationId);
                return 1; // مقدار پیش‌فرض برای محیط توسعه
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت ClinicId");
                return 1; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// دریافت شناسه کاربر فعلی
        /// </summary>
        public async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت UserId - CorrelationId: {CorrelationId}", correlationId);

                // دریافت از Claims
                var claimsIdentity = HttpContext.Current?.User?.Identity as System.Security.Claims.ClaimsIdentity;
                var userIdClaim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    _logger.Information("🏥 MEDICAL: UserId از Claims دریافت شد - UserId: {UserId}, CorrelationId: {CorrelationId}", 
                        userId, correlationId);
                    return userId;
                }

                // دریافت از Session
                if (HttpContext.Current?.Session?["CurrentUserId"] != null)
                {
                    var sessionUserId = HttpContext.Current.Session["CurrentUserId"];
                    if (int.TryParse(sessionUserId?.ToString(), out int sessionUserIdValue))
                    {
                        _logger.Information("🏥 MEDICAL: UserId از Session دریافت شد - UserId: {UserId}, CorrelationId: {CorrelationId}", 
                            sessionUserIdValue, correlationId);
                        return sessionUserIdValue;
                    }
                }

                _logger.Warning("🏥 MEDICAL: UserId یافت نشد - استفاده از مقدار پیش‌فرض - CorrelationId: {CorrelationId}", correlationId);
                return 1; // مقدار پیش‌فرض برای محیط توسعه
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت UserId");
                return 1; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// دریافت نام کاربر فعلی
        /// </summary>
        public async Task<string> GetCurrentUserNameAsync()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت UserName - CorrelationId: {CorrelationId}", correlationId);

                // دریافت از Claims
                var claimsIdentity = HttpContext.Current?.User?.Identity as System.Security.Claims.ClaimsIdentity;
                var userNameClaim = claimsIdentity?.FindFirst(ClaimTypes.Name);
                if (!string.IsNullOrEmpty(userNameClaim?.Value))
                {
                    _logger.Information("🏥 MEDICAL: UserName از Claims دریافت شد - UserName: {UserName}, CorrelationId: {CorrelationId}", 
                        userNameClaim.Value, correlationId);
                    return userNameClaim.Value;
                }

                // دریافت از Session
                if (HttpContext.Current?.Session?["CurrentUserName"] != null)
                {
                    var sessionUserName = HttpContext.Current.Session["CurrentUserName"]?.ToString();
                    if (!string.IsNullOrEmpty(sessionUserName))
                    {
                        _logger.Information("🏥 MEDICAL: UserName از Session دریافت شد - UserName: {UserName}, CorrelationId: {CorrelationId}", 
                            sessionUserName, correlationId);
                        return sessionUserName;
                    }
                }

                _logger.Warning("🏥 MEDICAL: UserName یافت نشد - استفاده از مقدار پیش‌فرض - CorrelationId: {CorrelationId}", correlationId);
                return "SystemUser"; // مقدار پیش‌فرض برای محیط توسعه
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت UserName");
                return "SystemUser"; // مقدار پیش‌فرض در صورت خطا
            }
        }

        /// <summary>
        /// بررسی دسترسی کاربر به کلینیک مشخص
        /// </summary>
        public async Task<bool> HasAccessToClinicAsync(int clinicId)
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var currentClinicId = await GetCurrentClinicIdAsync();
                
                _logger.Information("🏥 MEDICAL: بررسی دسترسی به کلینیک - CurrentClinicId: {CurrentClinicId}, RequestedClinicId: {RequestedClinicId}, CorrelationId: {CorrelationId}", 
                    currentClinicId, clinicId, correlationId);

                // بررسی دسترسی مستقیم
                if (currentClinicId == clinicId)
                {
                    _logger.Information("🏥 MEDICAL: دسترسی مستقیم تأیید شد - CorrelationId: {CorrelationId}", correlationId);
                    return true;
                }

                // بررسی دسترسی از طریق نقش‌ها (برای آینده)
                // TODO: پیاده‌سازی بررسی دسترسی از طریق نقش‌ها
                
                _logger.Warning("🏥 MEDICAL: دسترسی به کلینیک رد شد - CorrelationId: {CorrelationId}", correlationId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی دسترسی به کلینیک");
                return false;
            }
        }

        /// <summary>
        /// دریافت اطلاعات کامل Context کاربر
        /// </summary>
        public async Task<UserContextInfo> GetUserContextAsync()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _logger.Information("🏥 MEDICAL: دریافت اطلاعات کامل Context - CorrelationId: {CorrelationId}", correlationId);

                var userId = await GetCurrentUserIdAsync();
                var userName = await GetCurrentUserNameAsync();
                var clinicId = await GetCurrentClinicIdAsync();

                var contextInfo = new UserContextInfo
                {
                    UserId = userId,
                    UserName = userName,
                    ClinicId = clinicId,
                    ClinicName = "کلینیک شفا", // TODO: دریافت از دیتابیس
                    Roles = new[] { "Admin", "MedicalStaff" }, // TODO: دریافت از Claims یا دیتابیس
                    LoginTime = DateTime.UtcNow, // TODO: دریافت از Session
                    SessionId = HttpContext.Current?.Session?.SessionID ?? "Unknown"
                };

                _logger.Information("🏥 MEDICAL: اطلاعات Context دریافت شد - UserId: {UserId}, UserName: {UserName}, ClinicId: {ClinicId}, CorrelationId: {CorrelationId}", 
                    userId, userName, clinicId, correlationId);

                return contextInfo;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات Context");
                return new UserContextInfo
                {
                    UserId = 1,
                    UserName = "SystemUser",
                    ClinicId = 1,
                    ClinicName = "کلینیک شفا",
                    Roles = new[] { "Admin" },
                    LoginTime = DateTime.UtcNow,
                    SessionId = "Unknown"
                };
            }
        }
    }
}
