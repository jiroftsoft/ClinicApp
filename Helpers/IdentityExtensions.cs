using System.Security.Claims;
using System.Security.Principal;

namespace ClinicApp.Helpers;

public static class IdentityExtensions
{
    public static string GetFirstName(this IIdentity identity)
    {
        var claim = ((ClaimsIdentity)identity)?.FindFirst("FirstName");
        // Return the value of the claim if it's found, otherwise an empty string.
        return claim?.Value ?? string.Empty;
    }
}