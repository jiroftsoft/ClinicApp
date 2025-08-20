using System.Reflection;

namespace ClinicApp.Helpers;

public static class ApplicationVersion
{
    public static string GetVersion()
    {
        // دریافت نسخه Assembly فعلی
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version == null)
            return "نسخه نامشخص";

        // نسخه را به صورت Major.Minor.Build نمایش بده
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    public static string GetFullVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version == null)
            return "نسخه نامشخص";

        // نسخه کامل شامل Major.Minor.Build.Revision
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}