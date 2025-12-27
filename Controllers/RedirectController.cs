using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using Serilog;

namespace ClinicApp.Controllers
{
    /// <summary>
    /// Controller برای redirect کردن URL های اشتباه به URL های صحیح
    /// 
    /// اصول طراحی:
    /// 1. Single Responsibility: فقط redirect کردن URL های View به Controller
    /// 2. Security: جلوگیری از Open Redirect و Path Traversal
    /// 3. Logging: ثبت تمام redirect ها برای audit trail
    /// 4. Error Handling: مدیریت خطاها به صورت graceful
    /// 
    /// طبق: DEVELOPMENT_CONTRACT.md, Bugfix-Master-Contract.md
    /// </summary>
    public class RedirectController : BaseController
    {
        #region Constants

        /// <summary>
        /// Area names مجاز برای redirect
        /// </summary>
        private static readonly HashSet<string> AllowedAreas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Admin",
            "Patient"
        };

        /// <summary>
        /// Pattern برای validation URL
        /// فقط حروف، اعداد، خط تیره و خط زیر مجاز هستند
        /// </summary>
        private static readonly Regex ValidUrlPattern = new Regex(@"^[a-zA-Z0-9_\-/]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Default action name
        /// </summary>
        private const string DefaultActionName = "Index";

        /// <summary>
        /// View file extension
        /// </summary>
        private const string ViewFileExtension = ".cshtml";

        /// <summary>
        /// CMS prefix برای Admin Area
        /// </summary>
        private const string CmsPrefix = "CMS";

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor با Dependency Injection
        /// </summary>
        public RedirectController(ILogger logger) : base(logger)
        {
        }

        #endregion

        #region Area Views Redirect

        /// <summary>
        /// Redirect کردن URL های View در Areas به Controller Action
        /// مثال: /Areas/Admin/Views/CMS/ClinicWorkingHours/Index.cshtml -> /Admin/CMS/ClinicWorkingHours
        /// 
        /// Security:
        /// - Validation area name
        /// - Validation path components
        /// - URL encoding
        /// - Path traversal prevention
        /// </summary>
        [HttpGet]
        public ActionResult ViewToController(string area, string path)
        {
            try
            {
                _logger.Information("RedirectController.ViewToController: Area={Area}, Path={Path}", area, path);

                // Validation: بررسی ورودی‌ها
                if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(path))
                {
                    _logger.Warning("RedirectController.ViewToController: Invalid input - Area or Path is empty");
                    return SafeRedirectToHome();
                }

                // Security: بررسی area مجاز
                if (!AllowedAreas.Contains(area))
                {
                    _logger.Warning("RedirectController.ViewToController: Unauthorized area - {Area}", area);
                    return SafeRedirectToHome();
                }

                // Parse و validate path
                var pathParts = ParseAndValidatePath(path);
                if (pathParts == null || pathParts.Count == 0)
                {
                    _logger.Warning("RedirectController.ViewToController: Invalid path - {Path}", path);
                    return SafeRedirectToHome();
                }

                // ساخت redirect URL
                var redirectUrl = BuildAreaRedirectUrl(area, pathParts);
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    _logger.Warning("RedirectController.ViewToController: Failed to build redirect URL");
                    return SafeRedirectToHome();
                }

                // Security: Validate redirect URL
                if (!IsValidRedirectUrl(redirectUrl))
                {
                    _logger.Warning("RedirectController.ViewToController: Invalid redirect URL - {RedirectUrl}", redirectUrl);
                    return SafeRedirectToHome();
                }

                _logger.Information("RedirectController.ViewToController: Redirecting to {RedirectUrl}", redirectUrl);
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RedirectController.ViewToController: Error processing redirect");
                return SafeRedirectToHome();
            }
        }

        #endregion

        #region Non-Area Views Redirect

        /// <summary>
        /// Redirect کردن URL های View (non-Area) به Controller Action
        /// مثال: /Views/Payment/CashierReport/Index.cshtml -> /Payment/CashierReport
        /// 
        /// Security:
        /// - Validation path components
        /// - URL encoding
        /// - Path traversal prevention
        /// - Open redirect prevention
        /// </summary>
        [HttpGet]
        public ActionResult ViewsToController(string path)
        {
            try
            {
                _logger.Information("RedirectController.ViewsToController: Path={Path}", path);

                // Validation: بررسی ورودی
                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.Warning("RedirectController.ViewsToController: Path is empty");
                    return SafeRedirectToHome();
                }

                // Parse و validate path
                var pathParts = ParseAndValidatePath(path);
                if (pathParts == null || pathParts.Count == 0)
                {
                    _logger.Warning("RedirectController.ViewsToController: Invalid path - {Path}", path);
                    return SafeRedirectToHome();
                }

                // ساخت redirect URL
                var redirectUrl = BuildNonAreaRedirectUrl(pathParts);
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    _logger.Warning("RedirectController.ViewsToController: Failed to build redirect URL");
                    return SafeRedirectToHome();
                }

                // Security: Validate redirect URL
                if (!IsValidRedirectUrl(redirectUrl))
                {
                    _logger.Warning("RedirectController.ViewsToController: Invalid redirect URL - {RedirectUrl}", redirectUrl);
                    return SafeRedirectToHome();
                }

                _logger.Information("RedirectController.ViewsToController: Redirecting to {RedirectUrl}", redirectUrl);
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RedirectController.ViewsToController: Error processing redirect");
                return SafeRedirectToHome();
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Parse و validate کردن path
        /// </summary>
        private List<string> ParseAndValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Security: بررسی path برای path traversal
            if (path.Contains("..") || path.Contains("//") || path.Contains("\\"))
            {
                _logger.Warning("RedirectController.ParseAndValidatePath: Path traversal detected - {Path}", path);
                return null;
            }

            // Parse path
            var pathParts = path.Split('/')
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (pathParts.Count == 0)
                return null;

            // حذف پسوند .cshtml از آخرین بخش
            var lastPart = pathParts.Last();
            if (lastPart.EndsWith(ViewFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                lastPart = lastPart.Substring(0, lastPart.Length - ViewFileExtension.Length);
                pathParts[pathParts.Count - 1] = lastPart;
            }

            // Security: Validate هر بخش path
            foreach (var part in pathParts)
            {
                if (!IsValidPathComponent(part))
                {
                    _logger.Warning("RedirectController.ParseAndValidatePath: Invalid path component - {Part}", part);
                    return null;
                }
            }

            return pathParts;
        }

        /// <summary>
        /// بررسی اعتبار یک بخش path
        /// </summary>
        private bool IsValidPathComponent(string component)
        {
            if (string.IsNullOrWhiteSpace(component))
                return false;

            // فقط حروف، اعداد، خط تیره و خط زیر مجاز هستند
            return ValidUrlPattern.IsMatch(component);
        }

        /// <summary>
        /// ساخت redirect URL برای Area views
        /// </summary>
        private string BuildAreaRedirectUrl(string area, List<string> pathParts)
        {
            if (pathParts == null || pathParts.Count == 0)
                return null;

            var sb = new StringBuilder();
            sb.Append("/");
            sb.Append(area);

            // برای Admin Area
            if (area.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                // اگر path با CMS شروع شود
                if (pathParts.Count > 0 && pathParts[0].Equals(CmsPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (pathParts.Count >= 2)
                    {
                        sb.Append("/");
                        sb.Append(CmsPrefix);
                        sb.Append("/");
                        sb.Append(pathParts[1]);

                        var actionName = pathParts.Count > 2 ? pathParts[2] : DefaultActionName;
                        if (!actionName.Equals(DefaultActionName, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append("/");
                            sb.Append(actionName);
                        }
                    }
                    else
                    {
                        return null; // Invalid path
                    }
                }
                else
                {
                    // برای سایر Controllers در Admin (بدون CMS)
                    if (pathParts.Count >= 1)
                    {
                        sb.Append("/");
                        sb.Append(pathParts[0]);

                        var actionName = pathParts.Count > 1 ? pathParts[1] : DefaultActionName;
                        if (!actionName.Equals(DefaultActionName, StringComparison.OrdinalIgnoreCase))
                        {
                            sb.Append("/");
                            sb.Append(actionName);
                        }
                    }
                    else
                    {
                        return null; // Invalid path
                    }
                }
            }
            // برای Patient Area
            else if (area.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                if (pathParts.Count >= 1)
                {
                    sb.Append("/");
                    sb.Append(pathParts[0]);

                    var actionName = pathParts.Count > 1 ? pathParts[1] : DefaultActionName;
                    if (!actionName.Equals(DefaultActionName, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("/");
                        sb.Append(actionName);
                    }
                }
                else
                {
                    return null; // Invalid path
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// ساخت redirect URL برای non-Area views
        /// </summary>
        private string BuildNonAreaRedirectUrl(List<string> pathParts)
        {
            if (pathParts == null || pathParts.Count == 0)
                return null;

            var sb = new StringBuilder();
            sb.Append("/");

            // اگر آخرین بخش Index باشد، آن را حذف می‌کنیم (مطابق با MVC conventions)
            var partsToInclude = pathParts;
            if (pathParts.Last().Equals(DefaultActionName, StringComparison.OrdinalIgnoreCase))
            {
                partsToInclude = pathParts.Take(pathParts.Count - 1).ToList();
            }

            if (partsToInclude.Count == 0)
            {
                return null; // فقط Index بود، باید به Home redirect شود
            }

            sb.Append(string.Join("/", partsToInclude));
            return sb.ToString();
        }

        /// <summary>
        /// بررسی اعتبار redirect URL
        /// Security: جلوگیری از Open Redirect
        /// </summary>
        private bool IsValidRedirectUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // فقط URL های relative مجاز هستند (شروع با /)
            if (!url.StartsWith("/", StringComparison.Ordinal))
                return false;

            // بررسی path traversal
            if (url.Contains("..") || url.Contains("//") || url.Contains("\\"))
                return false;

            // بررسی pattern
            if (!ValidUrlPattern.IsMatch(url))
                return false;

            return true;
        }

        /// <summary>
        /// Safe redirect به Home
        /// </summary>
        private ActionResult SafeRedirectToHome()
        {
            try
            {
                return RedirectToAction(DefaultActionName, "Home");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RedirectController.SafeRedirectToHome: Error redirecting to Home");
                // Fallback: return empty result
                return new EmptyResult();
            }
        }

        #endregion
    }
}
