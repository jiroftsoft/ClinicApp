using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ClinicApp.Infrastructure.Hangfire
{
    /// <summary>
    /// تزریق اسکریپت رفع خطای RealtimeGraph (intValue undefined) در داشبورد Hangfire
    /// </summary>
    public class HangfireDashboardFixMiddleware
    {
        private const string FixScriptTag = "<script src=\"/Content/js/hangfire-realtime-fix.js\"></script>";
        private readonly Func<Task> _next;

        public HangfireDashboardFixMiddleware(Func<Task> next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(System.Collections.Generic.IDictionary<string, object> environment)
        {
            var path = environment["owin.RequestPath"] as string;
            var method = environment["owin.RequestMethod"] as string;

            // فقط برای GET به صفحه اصلی داشبورد (بدون زیرمسیر)
            if (!"GET".Equals(method, StringComparison.OrdinalIgnoreCase) ||
                (path != "/hangfire" && path != "/hangfire/"))
            {
                await _next().ConfigureAwait(false);
                return;
            }

            var response = environment["owin.ResponseBody"] as Stream;
            if (response == null)
            {
                await _next().ConfigureAwait(false);
                return;
            }

            var buffer = new MemoryStream();
            environment["owin.ResponseBody"] = buffer;

            await _next().ConfigureAwait(false);

            buffer.Position = 0;
            var contentType = environment["owin.ResponseHeaders"] as System.Collections.Generic.IDictionary<string, string[]>;
            var isHtml = contentType != null &&
                         contentType.TryGetValue("Content-Type", out var ct) &&
                         ct != null && ct.Length > 0 &&
                         ct[0].IndexOf("text/html", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isHtml)
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(response).ConfigureAwait(false);
                environment["owin.ResponseBody"] = response;
                return;
            }

            using (var reader = new StreamReader(buffer, Encoding.UTF8, false, 1024, true))
            {
                var html = await reader.ReadToEndAsync().ConfigureAwait(false);
                if (html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    html.IndexOf("hangfire-realtime-fix.js", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    html = html.Replace("</body>", FixScriptTag + "</body>");
                }

                var bytes = Encoding.UTF8.GetBytes(html);
                environment["owin.ResponseBody"] = response;
                if (contentType != null && contentType.ContainsKey("Content-Length"))
                    contentType["Content-Length"] = new[] { bytes.Length.ToString() };
                await response.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }
    }
}
