using System;
using System.Text.RegularExpressions;

namespace ClinicApp.Helpers.Security
{
    /// <summary>
    /// Helper برای تشخیص Device, Browser, OS از UserAgent
    /// 
    /// Single Responsibility: Parse UserAgent string و استخراج اطلاعات Device
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public static class DeviceDetectionHelper
    {
        /// <summary>
        /// Parse UserAgent و استخراج اطلاعات Device
        /// </summary>
        /// <param name="userAgent">UserAgent string</param>
        /// <returns>DeviceInfo object</returns>
        public static DeviceInfo ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return new DeviceInfo
                {
                    DeviceType = "Unknown",
                    BrowserName = "Unknown",
                    BrowserVersion = "Unknown",
                    OSName = "Unknown",
                    OSVersion = "Unknown"
                };
            }

            var deviceInfo = new DeviceInfo
            {
                DeviceType = DetectDeviceType(userAgent),
                BrowserName = DetectBrowser(userAgent),
                BrowserVersion = DetectBrowserVersion(userAgent),
                OSName = DetectOS(userAgent),
                OSVersion = DetectOSVersion(userAgent)
            };

            return deviceInfo;
        }

        /// <summary>
        /// تشخیص نوع دستگاه (Mobile, Desktop, Tablet)
        /// </summary>
        private static string DetectDeviceType(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            // Tablet detection
            if (userAgent.Contains("tablet") || 
                userAgent.Contains("ipad") || 
                (userAgent.Contains("android") && !userAgent.Contains("mobile")))
            {
                return "Tablet";
            }

            // Mobile detection
            if (userAgent.Contains("mobile") || 
                userAgent.Contains("android") || 
                userAgent.Contains("iphone") || 
                userAgent.Contains("ipod") || 
                userAgent.Contains("blackberry") || 
                userAgent.Contains("windows phone"))
            {
                return "Mobile";
            }

            // Desktop (default)
            return "Desktop";
        }

        /// <summary>
        /// تشخیص نام مرورگر
        /// </summary>
        private static string DetectBrowser(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("edg/"))
                return "Edge";
            if (userAgent.Contains("chrome") && !userAgent.Contains("edg"))
                return "Chrome";
            if (userAgent.Contains("firefox"))
                return "Firefox";
            if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
                return "Safari";
            if (userAgent.Contains("opera") || userAgent.Contains("opr/"))
                return "Opera";
            if (userAgent.Contains("msie") || userAgent.Contains("trident"))
                return "IE";

            return "Unknown";
        }

        /// <summary>
        /// تشخیص نسخه مرورگر
        /// </summary>
        private static string DetectBrowserVersion(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            // Chrome
            var chromeMatch = Regex.Match(userAgent, @"chrome/([\d.]+)", RegexOptions.IgnoreCase);
            if (chromeMatch.Success && !userAgent.Contains("edg"))
                return chromeMatch.Groups[1].Value;

            // Edge
            var edgeMatch = Regex.Match(userAgent, @"edg/([\d.]+)", RegexOptions.IgnoreCase);
            if (edgeMatch.Success)
                return edgeMatch.Groups[1].Value;

            // Firefox
            var firefoxMatch = Regex.Match(userAgent, @"firefox/([\d.]+)", RegexOptions.IgnoreCase);
            if (firefoxMatch.Success)
                return firefoxMatch.Groups[1].Value;

            // Safari
            var safariMatch = Regex.Match(userAgent, @"version/([\d.]+).*safari", RegexOptions.IgnoreCase);
            if (safariMatch.Success)
                return safariMatch.Groups[1].Value;

            // Opera
            var operaMatch = Regex.Match(userAgent, @"(?:opera|opr)/([\d.]+)", RegexOptions.IgnoreCase);
            if (operaMatch.Success)
                return operaMatch.Groups[1].Value;

            // IE
            var ieMatch = Regex.Match(userAgent, @"(?:msie |rv:)([\d.]+)", RegexOptions.IgnoreCase);
            if (ieMatch.Success)
                return ieMatch.Groups[1].Value;

            return "Unknown";
        }

        /// <summary>
        /// تشخیص سیستم عامل
        /// </summary>
        private static string DetectOS(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("windows"))
                return "Windows";
            if (userAgent.Contains("mac os x") || userAgent.Contains("macintosh"))
                return "macOS";
            if (userAgent.Contains("android"))
                return "Android";
            if (userAgent.Contains("iphone") || userAgent.Contains("ipad") || userAgent.Contains("ipod"))
                return "iOS";
            if (userAgent.Contains("linux"))
                return "Linux";
            if (userAgent.Contains("ubuntu"))
                return "Ubuntu";

            return "Unknown";
        }

        /// <summary>
        /// تشخیص نسخه سیستم عامل
        /// </summary>
        private static string DetectOSVersion(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            // Windows
            var windowsMatch = Regex.Match(userAgent, @"windows nt ([\d.]+)", RegexOptions.IgnoreCase);
            if (windowsMatch.Success)
            {
                var version = windowsMatch.Groups[1].Value;
                // Map Windows NT versions to friendly names
                return MapWindowsVersion(version);
            }

            // macOS
            var macMatch = Regex.Match(userAgent, @"mac os x ([\d_]+)", RegexOptions.IgnoreCase);
            if (macMatch.Success)
                return macMatch.Groups[1].Value.Replace("_", ".");

            // Android
            var androidMatch = Regex.Match(userAgent, @"android ([\d.]+)", RegexOptions.IgnoreCase);
            if (androidMatch.Success)
                return androidMatch.Groups[1].Value;

            // iOS
            var iosMatch = Regex.Match(userAgent, @"os ([\d_]+)", RegexOptions.IgnoreCase);
            if (iosMatch.Success && (userAgent.Contains("iphone") || userAgent.Contains("ipad")))
                return iosMatch.Groups[1].Value.Replace("_", ".");

            return "Unknown";
        }

        /// <summary>
        /// Map Windows NT version to friendly name
        /// </summary>
        private static string MapWindowsVersion(string ntVersion)
        {
            switch (ntVersion)
            {
                case "10.0":
                    return "10/11";
                case "6.3":
                    return "8.1";
                case "6.2":
                    return "8";
                case "6.1":
                    return "7";
                case "6.0":
                    return "Vista";
                default:
                    return ntVersion;
            }
        }
    }

    /// <summary>
    /// اطلاعات Device استخراج شده از UserAgent
    /// </summary>
    public class DeviceInfo
    {
        public string DeviceType { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }
    }
}

