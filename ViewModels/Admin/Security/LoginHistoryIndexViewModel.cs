using ClinicApp.Models.Entities.Security;
using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Admin.Security
{
    /// <summary>
    /// ViewModel برای نمایش لیست تاریخچه ورودها
    /// 
    /// Single Responsibility: نمایش داده‌های Login History در Admin Panel
    /// 
    /// طبق: LOGIN_SECURITY_AUDIT_ROADMAP.md
    /// </summary>
    public class LoginHistoryIndexViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserNationalCode { get; set; }
        public DateTime LoginTime { get; set; }
        public string LoginTimeShamsi { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string LogoutTimeShamsi { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceType { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string OSName { get; set; }
        public string OSVersion { get; set; }
        public string Location { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel برای فیلتر تاریخچه ورودها
    /// </summary>
    public class LoginHistoryFilterViewModel
    {
        public string UserId { get; set; }
        public string UserNationalCode { get; set; }
        public string IpAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsSuccessful { get; set; }
        public string DeviceType { get; set; }
        public string BrowserName { get; set; }
        public string OSName { get; set; }
        public string SearchTerm { get; set; }
    }
}

