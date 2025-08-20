using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNet.Identity;
using RestSharp;
using Serilog;

namespace ClinicApp.Services
{
    /// <summary>
    /// سرویس ارسال پیامک از طریق آسانک، سازگار با ASP.NET Identity.
    /// - اعتبارسنجی و نرمال‌سازی شماره (E.164)
    /// - Retry با Backoff نمایی و Jitter
    /// - Timeout و CancellationToken
    /// - لاگ‌گیری ساخت‌یافته با Serilog
    /// - خواندن امن تنظیمات از Web.config
    /// </summary>
    public sealed class AsanakSmsService : IIdentityMessageService
    {
        private static readonly ILogger _log = Log.ForContext<AsanakSmsService>();

        // تنظیمات
        private readonly string _username;
        private readonly string _password;
        private readonly string _sourceNumber;
        private readonly bool _enabled;
        private readonly int _timeoutMs;
        private readonly int _maxRetries;
        private readonly int _retryBaseDelayMs;

        // Endpoint (می‌توانید برای محیط‌های مختلف تغییر دهید)
        private const string BaseUrl = "https://panel.asanak.com";
        private const string SendPath = "/webservice/v1rest/sendsms";

        // الگو برای تبدیل اعداد فارسی/عربی به انگلیسی
        private static readonly Regex PersianArabicDigits = new Regex("[\u06F0-\u06F9\u0660-\u0669]", RegexOptions.Compiled);
        // حذف هر چیزی جز + و ارقام
        private static readonly Regex NonDigitExceptPlus = new Regex(@"(?!^\+)[^\d]", RegexOptions.Compiled);

        public AsanakSmsService()
        {
            _username = ConfigurationManager.AppSettings["Asanak:Username"];
            _password = ConfigurationManager.AppSettings["Asanak:Password"];
            _sourceNumber = ConfigurationManager.AppSettings["Asanak:SourceNumber"];

            // پیش‌فرض‌ها با قابلیت override از طریق Web.config
            _enabled = GetBool("Asanak:Enabled", defaultValue: true);
            _timeoutMs = GetInt("Asanak:TimeoutMs", defaultValue: 15000);
            _maxRetries = GetInt("Asanak:MaxRetries", defaultValue: 3);
            _retryBaseDelayMs = GetInt("Asanak:RetryBaseDelayMs", defaultValue: 400);
        }

        /// <summary>
        /// متد استاندارد Identity برای ارسال پیامک (تأیید شماره، کد 2FA و...).
        /// </summary>
        public async Task SendAsync(IdentityMessage message)
        {
            // در سناریوهایی که پیامک موقتاً غیر فعال شده است (محیط توسعه/تست)
            if (!_enabled)
            {
                _log.Information("Asanak SMS sending is DISABLED via config. Destination: {Destination}, Body: {Body}", message?.Destination, message?.Body);
                return;
            }

            if (message == null)
            {
                _log.Warning("IdentityMessage is null. SMS not sent.");
                return;
            }

            // اعتبارسنجی پایه
            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
            {
                _log.Error("Asanak credentials are missing in Web.config. SMS to {Destination} aborted.", message.Destination);
                return;
            }

            if (string.IsNullOrWhiteSpace(_sourceNumber))
            {
                _log.Error("Asanak SourceNumber is missing in Web.config. SMS to {Destination} aborted.", message.Destination);
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Destination) || string.IsNullOrWhiteSpace(message.Body))
            {
                _log.Warning("Destination or Body is empty. Destination: {Destination}, BodyLength: {Len}", message.Destination, message.Body?.Length ?? 0);
                return;
            }

            // نرمال‌سازی شماره‌ها
            string destination = NormalizeMsisdn(message.Destination);
            string source = NormalizeMsisdn(_sourceNumber);

            if (!IsLikelyPhone(destination))
            {
                _log.Warning("Destination MSISDN seems invalid after normalization. Original: {Original}, Normalized: {Normalized}", message.Destination, destination);
                return;
            }

            // برخی پنل‌ها محدودیت طول دارند؛ فقط هشدار می‌دهیم (برش نمی‌زنیم تا مسئولیت متن دست‌نخورده بماند)
            if (message.Body.Length > 1000)
            {
                _log.Warning("SMS body is quite long ({Len} chars). Destination: {Destination}", message.Body.Length, destination);
            }

            // ارسال با Retry
            var rnd = new Random();
            Exception lastEx = null;

            for (int attempt = 1; attempt <= Math.Max(1, _maxRetries); attempt++)
            {
                using var cts = new CancellationTokenSource(_timeoutMs);
                try
                {
                    var response = await SendInternalAsync(source, destination, message.Body, cts.Token);

                    if (response.IsSuccessful && (int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                    {
                        _log.Information("SMS sent via Asanak. Attempt: {Attempt}, To: {Destination}, Status: {Status}, Length: {Len}, Content: {Content}",
                            attempt, destination, response.StatusCode, message.Body.Length, Truncate(response.Content, 500));
                        return;
                    }

                    _log.Error("Asanak SMS failed. Attempt: {Attempt}, To: {Destination}, Status: {Status}, Error: {Error}, Content: {Content}",
                        attempt, destination, response.StatusCode, response.ErrorMessage, Truncate(response.Content, 800));

                    // اگر تلاش بعدی داریم، Backoff
                    if (attempt < _maxRetries)
                    {
                        int delay = ComputeBackoffDelay(attempt, _retryBaseDelayMs, rnd);
                        await Task.Delay(delay, CancellationToken.None);
                    }
                }
                catch (OperationCanceledException ocex)
                {
                    lastEx = ocex;
                    _log.Error(ocex, "Asanak SMS timeout/canceled. Attempt: {Attempt}, To: {Destination}", attempt, destination);
                    if (attempt < _maxRetries)
                    {
                        int delay = ComputeBackoffDelay(attempt, _retryBaseDelayMs, rnd);
                        await Task.Delay(delay, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _log.Error(ex, "Asanak SMS unexpected error. Attempt: {Attempt}, To: {Destination}", attempt, destination);
                    if (attempt < _maxRetries)
                    {
                        int delay = ComputeBackoffDelay(attempt, _retryBaseDelayMs, rnd);
                        await Task.Delay(delay, CancellationToken.None);
                    }
                }
            }

            // اگر بعد از همه تلاش‌ها ناموفق بود
            _log.Fatal(lastEx, "Asanak SMS permanently failed after {Retries} attempts. To: {Destination}", _maxRetries, message.Destination);
        }

        /// <summary>
        /// ارسال واقعی درخواست به آسانک (با RestSharp)
        /// </summary>
        private async Task<RestResponse> SendInternalAsync(string source, string destination, string body, CancellationToken ct)
        {
            // RestSharp مدرن: استفاده از RestClientOptions
            var options = new RestClientOptions(BaseUrl)
            {
                MaxTimeout = _timeoutMs, // ms
                ThrowOnAnyError = false
            };

            var client = new RestClient(options);
            var request = new RestRequest(SendPath, Method.Post);

            // برخی پنل‌ها روی encoding حساس هستند؛ UTF-8
            request.AddHeader("Accept", "application/json, text/plain, */*");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

            // پارامترهای موردنیاز آسانک
            request.AddParameter("username", _username);
            request.AddParameter("password", _password);
            request.AddParameter("source", source);
            request.AddParameter("destination", destination);
            request.AddParameter("message", body);

            // در صورت نیاز می‌توانید پارامترهای دیگری مانند flash، udh، encoding اضافه کنید.

            return await client.ExecuteAsync(request, ct);
        }

        // ===== Helpers =====

        private static int ComputeBackoffDelay(int attempt, int baseDelayMs, Random rnd)
        {
            // backoff نمایی + jitter برای جلوگیری از هم‌زمانی درخواست‌ها
            double exp = Math.Pow(2, attempt - 1);
            int jitter = rnd.Next(0, baseDelayMs);
            // سقف محافظه‌کارانه
            int delay = (int)Math.Min(15000, exp * baseDelayMs + jitter);
            return delay;
        }

        private static string NormalizeMsisdn(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // تبدیل ارقام فارسی/عربی به انگلیسی
            string converted = PersianArabicDigits.Replace(input, m =>
            {
                char ch = m.Value[0];
                // فارسی ۰x06F0..۰x06F9  | عربی ٠x0660..٩x0669
                int digit = ch >= '\u06F0' && ch <= '\u06F9' ? ch - '\u06F0' : ch - '\u0660';
                return digit.ToString(CultureInfo.InvariantCulture);
            });

            // حذف هر چیزی جز ارقام؛ + فقط اگر ابتدای رشته باشد مجاز است
            converted = NonDigitExceptPlus.Replace(converted, string.Empty).Trim();

            // سناریوهای رایج ایران:
            // 0098xxxxxxxxxx  -> +98xxxxxxxxxx
            if (converted.StartsWith("0098")) converted = "+98" + converted.Substring(4);

            // 098xxxxxxxxxx -> +98xxxxxxxxxx
            if (converted.StartsWith("098")) converted = "+98" + converted.Substring(3);

            // 98xxxxxxxxxx -> +98xxxxxxxxxx
            if (converted.StartsWith("98") && !converted.StartsWith("+")) converted = "+98" + converted.Substring(2);

            // 0xxxxxxxxxx -> +98xxxxxxxxxx (شماره موبایل داخلی ایران)
            if (converted.StartsWith("0") && converted.Length >= 10)
            {
                converted = "+98" + converted.Substring(1);
            }

            // اگر بدون + و شبیه 9********* بود (ایران)، به +98 تبدیل کنیم
            if (!converted.StartsWith("+") && converted.Length == 10 && converted.StartsWith("9"))
            {
                converted = "+98" + converted;
            }

            return converted;
        }

        private static bool IsLikelyPhone(string msisdn)
        {
            if (string.IsNullOrWhiteSpace(msisdn)) return false;
            // شکل ساده E.164: + و 8 تا 15 رقم
            return Regex.IsMatch(msisdn, @"^\+\d{8,15}$");
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
            return s.Substring(0, max) + "…";
        }

        private static int GetInt(string key, int defaultValue)
        {
            var val = ConfigurationManager.AppSettings[key];
            return int.TryParse(val, out var n) ? n : defaultValue;
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            var val = ConfigurationManager.AppSettings[key];
            return bool.TryParse(val, out var b) ? b : defaultValue;
        }
    }
}
