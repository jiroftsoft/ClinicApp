using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClinicApp.Services.Notification;

/// <summary>
/// جایگزینی متغیرهای قالب با فرمت {{VariableName}}
/// </summary>
public static class NotificationTemplateEngine
{
    private static readonly Regex PlaceholderRegex = new Regex(
        @"\{\{(\w+)\}\}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Render(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template) || variables == null)
            return template ?? "";

        return PlaceholderRegex.Replace(template, m =>
        {
            var key = m.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? (value ?? "") : m.Value;
        });
    }
}
