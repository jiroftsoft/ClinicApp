using System.Collections.Generic;

namespace ClinicApp.ViewModels.Diagnostics
{
    /// <summary>
    /// ViewModel برای صفحه Diagnostics - نمایش اطلاعات Authentication
    /// </summary>
    public class AuthDiagnosticsViewModel
    {
        // Basic Auth Info
        public bool RequestIsAuthenticated { get; set; }
        public bool UserIdentityIsAuthenticated { get; set; }
        public string UserIdentityName { get; set; }
        public string UserId { get; set; }

        // CurrentUserService Info
        public string CurrentUserServiceUserId { get; set; }
        public bool CurrentUserServiceIsAuthenticated { get; set; }
        public bool CurrentUserServiceIsPatient { get; set; }
        public bool CurrentUserServiceIsAdmin { get; set; }
        public bool CurrentUserServiceIsDoctor { get; set; }

        // Claims
        public List<ClaimInfo> Claims { get; set; }

        // Database Roles
        public object DatabaseRoles { get; set; }

        // Patient Record
        public object PatientRecord { get; set; }

        // Cookies
        public List<CookieInfo> Cookies { get; set; }
    }

    public class ClaimInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class CookieInfo
    {
        public string Key { get; set; }
        public bool HasValue { get; set; }
    }
}

