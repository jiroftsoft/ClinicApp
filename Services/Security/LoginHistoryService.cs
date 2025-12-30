using ClinicApp.Helpers;
using ClinicApp.Helpers.Security;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Security;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Security;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ClinicApp.Services.Security
{
    /// <summary>
    /// سرویس مدیریت تاریخچه ورود کاربران
    /// 
    /// Single Responsibility: مدیریت ثبت و بازیابی تاریخچه ورودها
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. ثبت ورودهای موفق و ناموفق
    /// 2. ثبت خروج کاربران
    /// 3. دریافت تاریخچه ورود یک کاربر
    /// 4. دریافت ورودهای اخیر
    /// 5. تشخیص فعالیت مشکوک
    /// 6. آمار ورودها
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public class LoginHistoryService : ILoginHistoryService
    {
        #region Fields

        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        #endregion

        #region Constructor

        public LoginHistoryService(
            ApplicationDbContext context,
            ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region LogLoginAsync

        public async Task<ServiceResult> LogLoginAsync(string userId, string ipAddress, string userAgent, string sessionId = null)
        {
            try
            {
                _logger.Information("📝 Logging successful login for UserId: {UserId} | IP: {IpAddress}", userId, ipAddress);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                // Parse UserAgent برای استخراج Device Info
                var deviceInfo = DeviceDetectionHelper.ParseUserAgent(userAgent);

                // ایجاد لاگ
                var loginHistory = new UserLoginHistory
                {
                    UserId = userId,
                    LoginTime = DateTime.Now,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent ?? "Unknown",
                    DeviceType = deviceInfo.DeviceType,
                    BrowserName = deviceInfo.BrowserName,
                    BrowserVersion = deviceInfo.BrowserVersion,
                    OSName = deviceInfo.OSName,
                    OSVersion = deviceInfo.OSVersion,
                    IsSuccessful = true,
                    SessionId = sessionId,
                    CreatedAt = DateTime.Now
                };

                _context.UserLoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();

                _logger.Information("✅ Login history logged successfully - Id: {Id} | UserId: {UserId}", loginHistory.Id, userId);

                return ServiceResult.Successful("ورود با موفقیت ثبت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error logging login history for UserId: {UserId}", userId);
                return ServiceResult.Failed("خطا در ثبت تاریخچه ورود.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region LogFailedLoginAsync

        public async Task<ServiceResult> LogFailedLoginAsync(string userId, string ipAddress, string userAgent, string failureReason)
        {
            try
            {
                _logger.Warning("⚠️ Logging failed login attempt | UserId: {UserId} | IP: {IpAddress} | Reason: {Reason}", 
                    userId ?? "Unknown", ipAddress, failureReason);

                // Parse UserAgent برای استخراج Device Info
                var deviceInfo = DeviceDetectionHelper.ParseUserAgent(userAgent);

                // ایجاد لاگ
                var loginHistory = new UserLoginHistory
                {
                    UserId = userId ?? "UNKNOWN_USER",
                    LoginTime = DateTime.Now,
                    IpAddress = ipAddress ?? "Unknown",
                    UserAgent = userAgent ?? "Unknown",
                    DeviceType = deviceInfo.DeviceType,
                    BrowserName = deviceInfo.BrowserName,
                    BrowserVersion = deviceInfo.BrowserVersion,
                    OSName = deviceInfo.OSName,
                    OSVersion = deviceInfo.OSVersion,
                    IsSuccessful = false,
                    FailureReason = failureReason,
                    CreatedAt = DateTime.Now
                };

                _context.UserLoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();

                _logger.Warning("✅ Failed login history logged - Id: {Id} | UserId: {UserId}", loginHistory.Id, userId ?? "Unknown");

                return ServiceResult.Successful("تلاش ناموفق ورود ثبت شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error logging failed login history | UserId: {UserId}", userId ?? "Unknown");
                return ServiceResult.Failed("خطا در ثبت تاریخچه ورود ناموفق.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region LogLogoutAsync

        public async Task<ServiceResult> LogLogoutAsync(string userId, string sessionId)
        {
            try
            {
                _logger.Information("📝 Logging logout for UserId: {UserId} | SessionId: {SessionId}", userId, sessionId);

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                // پیدا کردن آخرین ورود موفق با این SessionId
                var lastLogin = await _context.UserLoginHistories
                    .Where(l => l.UserId == userId && 
                                l.SessionId == sessionId && 
                                l.IsSuccessful && 
                                l.LogoutTime == null)
                    .OrderByDescending(l => l.LoginTime)
                    .FirstOrDefaultAsync();

                if (lastLogin != null)
                {
                    lastLogin.LogoutTime = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.Information("✅ Logout logged successfully - LoginHistoryId: {Id}", lastLogin.Id);
                    return ServiceResult.Successful("خروج با موفقیت ثبت شد.");
                }
                else
                {
                    _logger.Warning("⚠️ No matching login found for logout | UserId: {UserId} | SessionId: {SessionId}", userId, sessionId);
                    return ServiceResult.Failed("ورود مرتبط یافت نشد.", "NOT_FOUND");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error logging logout for UserId: {UserId}", userId);
                return ServiceResult.Failed("خطا در ثبت خروج.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region GetUserLoginHistoryAsync

        public async Task<ServiceResult<List<UserLoginHistory>>> GetUserLoginHistoryAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult<List<UserLoginHistory>>.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var skip = (pageNumber - 1) * pageSize;

                var loginHistory = await _context.UserLoginHistories
                    .Where(l => l.UserId == userId)
                    .OrderByDescending(l => l.LoginTime)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.Debug("Retrieved login history for UserId: {UserId} | Count: {Count}", userId, loginHistory.Count);

                return ServiceResult<List<UserLoginHistory>>.Successful(loginHistory);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error retrieving login history for UserId: {UserId}", userId);
                return ServiceResult<List<UserLoginHistory>>.Failed("خطا در دریافت تاریخچه ورود.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region GetRecentLoginsAsync

        public async Task<ServiceResult<List<UserLoginHistory>>> GetRecentLoginsAsync(int count = 50)
        {
            try
            {
                if (count < 1 || count > 500) count = 50;

                var recentLogins = await _context.UserLoginHistories
                    .OrderByDescending(l => l.LoginTime)
                    .Take(count)
                    .ToListAsync();

                _logger.Debug("Retrieved recent logins | Count: {Count}", recentLogins.Count);

                return ServiceResult<List<UserLoginHistory>>.Successful(recentLogins);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error retrieving recent logins");
                return ServiceResult<List<UserLoginHistory>>.Failed("خطا در دریافت ورودهای اخیر.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region GetLoginHistoryByIdAsync

        public async Task<ServiceResult<UserLoginHistory>> GetLoginHistoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ServiceResult<UserLoginHistory>.Failed("شناسه نامعتبر است.", "VALIDATION");
                }

                var loginHistory = await _context.UserLoginHistories
                    .Include(l => l.User)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (loginHistory == null)
                {
                    _logger.Warning("Login history not found | Id: {Id}", id);
                    return ServiceResult<UserLoginHistory>.Failed("رکورد مورد نظر یافت نشد.", "NOT_FOUND");
                }

                _logger.Debug("Retrieved login history | Id: {Id} | UserId: {UserId}", id, loginHistory.UserId);

                return ServiceResult<UserLoginHistory>.Successful(loginHistory);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error retrieving login history | Id: {Id}", id);
                return ServiceResult<UserLoginHistory>.Failed("خطا در دریافت رکورد.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region IsSuspiciousActivityAsync

        public async Task<ServiceResult<bool>> IsSuspiciousActivityAsync(string userId, string ipAddress, string userAgent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult<bool>.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                // دریافت 10 ورود موفق اخیر این کاربر
                var previousLogins = await _context.UserLoginHistories
                    .Where(l => l.UserId == userId && l.IsSuccessful)
                    .OrderByDescending(l => l.LoginTime)
                    .Take(10)
                    .ToListAsync();

                // اگر هیچ ورود قبلی نداشته باشد، مشکوک نیست (اولین ورود)
                if (!previousLogins.Any())
                {
                    return ServiceResult<bool>.Successful(false);
                }

                // بررسی IP جدید
                var isNewIp = !previousLogins.Any(l => l.IpAddress == ipAddress);

                // بررسی Device جدید (بر اساس Browser + OS)
                var currentDeviceFingerprint = GetDeviceFingerprint(userAgent);
                var isNewDevice = !previousLogins.Any(l => GetDeviceFingerprint(l.UserAgent) == currentDeviceFingerprint);

                var isSuspicious = isNewIp || isNewDevice;

                if (isSuspicious)
                {
                    _logger.Warning("⚠️ Suspicious activity detected | UserId: {UserId} | NewIP: {NewIP} | NewDevice: {NewDevice}", 
                        userId, isNewIp, isNewDevice);
                }

                return ServiceResult<bool>.Successful(isSuspicious);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error checking suspicious activity for UserId: {UserId}", userId);
                return ServiceResult<bool>.Failed("خطا در بررسی فعالیت مشکوک.", "SYSTEM_ERROR");
            }
        }

        /// <summary>
        /// ایجاد Device Fingerprint از UserAgent
        /// </summary>
        private string GetDeviceFingerprint(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            var deviceInfo = DeviceDetectionHelper.ParseUserAgent(userAgent);
            return $"{deviceInfo.BrowserName}_{deviceInfo.OSName}_{deviceInfo.DeviceType}";
        }

        #endregion

        #region GetLoginStatisticsAsync

        public async Task<ServiceResult<LoginStatistics>> GetLoginStatisticsAsync(string userId, int days = 30)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ServiceResult<LoginStatistics>.Failed("شناسه کاربر الزامی است.", "VALIDATION");
                }

                if (days < 1 || days > 365) days = 30;

                var fromDate = DateTime.Now.AddDays(-days);

                var logins = await _context.UserLoginHistories
                    .Where(l => l.UserId == userId && l.LoginTime >= fromDate)
                    .ToListAsync();

                var statistics = new LoginStatistics
                {
                    TotalLogins = logins.Count,
                    SuccessfulLogins = logins.Count(l => l.IsSuccessful),
                    FailedLogins = logins.Count(l => !l.IsSuccessful),
                    UniqueIpAddresses = logins.Where(l => !string.IsNullOrWhiteSpace(l.IpAddress))
                                             .Select(l => l.IpAddress)
                                             .Distinct()
                                             .Count(),
                    UniqueDevices = logins.Where(l => !string.IsNullOrWhiteSpace(l.UserAgent))
                                         .Select(l => GetDeviceFingerprint(l.UserAgent))
                                         .Distinct()
                                         .Count(),
                    LastLoginTime = logins.Where(l => l.IsSuccessful)
                                         .OrderByDescending(l => l.LoginTime)
                                         .Select(l => (DateTime?)l.LoginTime)
                                         .FirstOrDefault(),
                    LastLoginIpAddress = logins.Where(l => l.IsSuccessful)
                                               .OrderByDescending(l => l.LoginTime)
                                               .Select(l => l.IpAddress)
                                               .FirstOrDefault(),
                    LastLoginDevice = logins.Where(l => l.IsSuccessful)
                                            .OrderByDescending(l => l.LoginTime)
                                            .Select(l => $"{l.BrowserName} on {l.OSName}")
                                            .FirstOrDefault()
                };

                _logger.Debug("Retrieved login statistics for UserId: {UserId} | Total: {Total}", userId, statistics.TotalLogins);

                return ServiceResult<LoginStatistics>.Successful(statistics);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Error retrieving login statistics for UserId: {UserId}", userId);
                return ServiceResult<LoginStatistics>.Failed("خطا در دریافت آمار ورودها.", "SYSTEM_ERROR");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// دریافت IP Address کلاینت
        /// </summary>
        private string GetClientIpAddress()
        {
            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    var ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipAddress))
                    {
                        ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                    return ipAddress ?? "Unknown";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// دریافت User Agent
        /// </summary>
        private string GetUserAgent()
        {
            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    return HttpContext.Current.Request.UserAgent ?? "Unknown";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        #endregion
    }
}

