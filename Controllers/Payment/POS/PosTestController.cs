using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Services.Payment.POS;
using Serilog;

namespace ClinicApp.Controllers.Payment.POS
{
    /// <summary>
    /// کنترلر تست دستگاه POS
    /// 
    /// هدف: تست و بررسی عملکرد دستگاه POS قبل از استفاده در ماژول پذیرش
    /// 
    /// قابلیت‌ها:
    /// - تست اتصال به دستگاه POS
    /// - تست ارسال مبلغ به دستگاه
    /// - نمایش لاگ‌های کامل
    /// - نمایش جزئیات ترمینال
    /// </summary>
    [RoutePrefix("PosTest")]
    [Route("{action=Index}")]
    public class PosTestController : BaseController
    {
        private readonly IPosManagementService _posManagementService;
        private readonly IPosDeviceService _posDeviceService;
        private readonly PosPaymentOrchestrator _paymentOrchestrator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public PosTestController(
            IPosManagementService posManagementService,
            IPosDeviceService posDeviceService,
            PosPaymentOrchestrator paymentOrchestrator,
            ICurrentUserService currentUserService,
            ILogger logger) : base(currentUserService, logger)
        {
            _posManagementService = posManagementService ?? throw new ArgumentNullException(nameof(posManagementService));
            _posDeviceService = posDeviceService ?? throw new ArgumentNullException(nameof(posDeviceService));
            _paymentOrchestrator = paymentOrchestrator ?? throw new ArgumentNullException(nameof(paymentOrchestrator));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger.ForContext<PosTestController>();
        }

        /// <summary>
        /// صفحه اصلی تست POS
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                _logger.Information("🔍 POS Test: صفحه تست باز شد - کاربر: {UserName}", _currentUserService?.UserName ?? "Unknown");

                // دریافت لیست ترمینال‌های فعال
                var terminalsResult = await _posManagementService.GetActivePosTerminalsAsync();
                var terminals = terminalsResult.Success && terminalsResult.Data != null
                    ? terminalsResult.Data.ToList()
                    : new List<PosTerminal>();

                // دریافت ترمینال پیش‌فرض
                var defaultTerminalResult = await _posManagementService.GetDefaultPosTerminalAsync();
                var defaultTerminal = defaultTerminalResult.Success && defaultTerminalResult.Data != null
                    ? defaultTerminalResult.Data
                    : null;

                ViewBag.Terminals = terminals;
                ViewBag.DefaultTerminal = defaultTerminal;
                ViewBag.TerminalCount = terminals.Count;

                return View();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Test: خطا در بارگذاری صفحه تست");
                AddError("خطا در بارگذاری صفحه تست");
                return View();
            }
        }

        /// <summary>
        /// تست اتصال به دستگاه POS (GET - فقط برای نمایش پیام)
        /// </summary>
        [HttpGet]
        [Route("TestConnection")]
        public ActionResult TestConnection()
        {
            return Content("این action فقط از طریق AJAX (POST request) قابل دسترسی است. لطفاً از صفحه اصلی تست استفاده کنید.", "text/html; charset=utf-8");
        }

        /// <summary>
        /// تست اتصال به دستگاه POS (POST - برای AJAX)
        /// </summary>
        [HttpPost]
        [Route("TestConnection")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> TestConnection(int terminalId, decimal? testAmount = null)
        {
            try
            {
                _logger.Information("🔍 POS Test: شروع تست اتصال - TerminalId: {TerminalId}, User: {UserName}",
                    terminalId, _currentUserService?.UserName ?? "Unknown");

                // دریافت ترمینال
                var terminalResult = await _posManagementService.GetPosTerminalAsync(terminalId);
                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    _logger.Warning("⚠️ POS Test: ترمینال یافت نشد - TerminalId: {TerminalId}", terminalId);
                    return Json(ServiceResult.Failed("ترمینال POS یافت نشد"));
                }

                var terminal = terminalResult.Data;

                // بررسی تنظیمات ترمینال
                var validationErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(terminal.TerminalId))
                    validationErrors.Add("شماره ترمینال تنظیم نشده است");

                if (string.IsNullOrWhiteSpace(terminal.MerchantId))
                    validationErrors.Add("شماره پذیرنده تنظیم نشده است");

                if (string.IsNullOrWhiteSpace(terminal.IpAddress))
                    validationErrors.Add("آدرس IP تنظیم نشده است");

                // Port is optional - if not set, driver will use default port (5000
                // if (!terminal.Port.HasValue || terminal.Port.Value <= 0)
                //     validationErrors.Add("پورت تنظیم نشده است");

                if (!terminal.IsActive)
                    validationErrors.Add("ترمینال غیرفعال است");

                if (validationErrors.Any())
                {
                    _logger.Warning("⚠️ POS Test: تنظیمات ترمینال ناقص است - TerminalId: {TerminalId}, Errors: {Errors}",
                        terminal.TerminalId, string.Join(", ", validationErrors));
                    return Json(ServiceResult.Failed($"تنظیمات ترمینال ناقص است: {string.Join("; ", validationErrors)}"));
                }

                // تست اتصال
                _logger.Information("🔍 POS Test: تلاش برای اتصال - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port}",
                    terminal.TerminalId, terminal.IpAddress, terminal.Port);

                // استفاده از Driver برای تست اتصال
                var driver = GetDriver(terminal.Provider);
                if (driver == null)
                {
                    _logger.Warning("⚠️ POS Test: درایور یافت نشد - Provider: {Provider}", terminal.Provider);
                    return Json(ServiceResult.Failed($"درایور برای ارائه‌دهنده {terminal.Provider} یافت نشد"));
                }

                try
                {
                    var connectResult = await driver.ConnectAsync(terminal);
                    if (!connectResult.Success)
                    {
                        _logger.Warning("⚠️ POS Test: اتصال با پورت فعلی ناموفق - TerminalId: {TerminalId}, Port: {Port}, Error: {Error}",
                            terminal.TerminalId, terminal.Port ?? 5000, connectResult.Message);

                        // اگر پورت تنظیم نشده یا اتصال ناموفق بود، پورت‌های رایج را خودکار تست کن
                        // همیشه پورت‌های رایج را تست کن (حتی اگر پورت تنظیم شده باشد)
                        _logger.Information("🔍 POS Test: شروع تست خودکار پورت‌های رایج - IP: {IpAddress}", terminal.IpAddress);
                        
                        // لیست گسترده‌تر پورت‌های رایج برای تست (بدون نیاز به تنظیم پورت)
                        var commonPorts = new[] { 
                            5000, 8080, 9100, 4000,  // پورت‌های رایج SSP1126
                            3000, 6000, 7000, 9000,  // پورت‌های جایگزین
                            2000, 3001, 5001, 6001,  // پورت‌های اضافی
                            23, 80, 443, 8081, 9090  // پورت‌های استاندارد
                        };
                            var testedPorts = new List<object>();
                            int? foundPort = null;

                            foreach (var testPort in commonPorts)
                            {
                                // اگر پورت فعلی را قبلاً تست کردیم، skip کن (اما اگر null بود، همه را تست کن)
                                if (terminal.Port.HasValue && terminal.Port.Value == testPort)
                                {
                                    _logger.Debug("⏭️ POS Test: پورت {Port} قبلاً تست شد، skip می‌شود", testPort);
                                    continue;
                                }

                                try
                                {
                                    var testTerminal = new PosTerminal
                                    {
                                        IpAddress = terminal.IpAddress,
                                        Port = testPort,
                                        TerminalId = terminal.TerminalId,
                                        MerchantId = terminal.MerchantId,
                                        Provider = terminal.Provider,
                                        Protocol = terminal.Protocol,
                                        IsActive = true
                                    };

                                    var testDriver = GetDriver(testTerminal.Provider);
                                    if (testDriver == null)
                                        continue;

                                    using (testDriver)
                                    {
                                        var testResult = await testDriver.ConnectAsync(testTerminal);
                                        if (testResult.Success)
                                        {
                                            _logger.Information("✅ POS Test: پورت {Port} موفق - IP: {IpAddress}", testPort, terminal.IpAddress);
                                            foundPort = testPort;
                                            testedPorts.Add(new { port = testPort, status = "موفق", message = "اتصال برقرار شد" });
                                            await testDriver.DisconnectAsync(testTerminal);
                                            break;
                                        }
                                        else
                                        {
                                            testedPorts.Add(new { port = testPort, status = "ناموفق", message = testResult.Message });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Warning(ex, "⚠️ POS Test: خطا در تست پورت {Port}", testPort);
                                    testedPorts.Add(new { port = testPort, status = "خطا", message = ex.Message });
                                }
                            }

                            if (foundPort.HasValue)
                            {
                                var successResult = ServiceResult<object>.Successful(new
                                {
                                    connectionStatus = "موفق (پورت خودکار پیدا شد)",
                                    foundPort = foundPort.Value,
                                    message = $"اتصال با پورت {foundPort.Value} برقرار شد. لطفاً این پورت را در تنظیمات ترمینال ذخیره کنید.",
                                    testedPorts = testedPorts,
                                    originalError = connectResult.Message
                                });
                                successResult.Metadata["AutoDetectedPort"] = foundPort.Value;
                                successResult.Metadata["TestedPorts"] = testedPorts;
                                return Json(successResult);
                            }
                            else
                            {
                                var errorResult = ServiceResult.Failed(
                                    $"اتصال با پورت فعلی ناموفق بود و هیچ یک از پورت‌های رایج (5000, 8080, 9100, 4000) نیز کار نکردند.\n\nخطای اصلی: {connectResult.Message}\n\nلطفاً:\n• پورت را از منوی تنظیمات شبکه دستگاه POS بررسی کنید\n• مطمئن شوید سرویس SSP1126 روی دستگاه فعال است\n• فایروال دستگاه POS را بررسی کنید");
                                errorResult.Metadata["TestedPorts"] = testedPorts;
                                errorResult.Metadata["OriginalError"] = connectResult.Message;
                                return Json(errorResult);
                            }

                        // اگر تست خودکار انجام شد و پورتی پیدا نشد، خطا را برگردان
                        _logger.Error("❌ POS Test: اتصال ناموفق - TerminalId: {TerminalId}, Error: {Error}",
                            terminal.TerminalId, connectResult.Message);
                        return Json(ServiceResult.Failed($"خطا در اتصال به دستگاه: {connectResult.Message}"));
                    }

                    _logger.Information("✅ POS Test: اتصال موفق - TerminalId: {TerminalId}", terminal.TerminalId);

                    // اگر مبلغ تستی داده شده، تست پرداخت هم انجام شود
                    if (testAmount.HasValue && testAmount.Value > 0)
                    {
                        _logger.Information("🔍 POS Test: شروع تست پرداخت - TerminalId: {TerminalId}, Amount: {Amount}",
                            terminal.TerminalId, testAmount.Value);

                        var paymentResult = await driver.SendPaymentAsync(terminal, testAmount.Value);
                        await driver.DisconnectAsync(terminal);

                        if (!paymentResult.Success)
                        {
                            _logger.Error("❌ POS Test: تست پرداخت ناموفق - TerminalId: {TerminalId}, Error: {Error}",
                                terminal.TerminalId, paymentResult.Message);
                            return Json(ServiceResult.Failed($"تست اتصال موفق بود، اما تست پرداخت ناموفق: {paymentResult.Message}"));
                        }

                        _logger.Information("✅ POS Test: تست پرداخت موفق - TerminalId: {TerminalId}, RRN: {RRN}, TraceNo: {TraceNo}",
                            terminal.TerminalId, paymentResult.Data.RRN, paymentResult.Data.TraceNo);

                        return Json(ServiceResult<object>.Successful(new
                        {
                            connectionStatus = "موفق",
                            paymentStatus = "موفق",
                            rrn = paymentResult.Data.RRN,
                            traceNo = paymentResult.Data.TraceNo,
                            cardLast4 = paymentResult.Data.CardLast4,
                            message = paymentResult.Data.Message,
                            terminalInfo = new
                            {
                                terminalId = terminal.TerminalId,
                                merchantId = terminal.MerchantId,
                                ipAddress = terminal.IpAddress,
                                port = terminal.Port,
                                provider = terminal.Provider.ToString()
                            }
                        }));
                    }

                    // فقط تست اتصال
                    await driver.DisconnectAsync(terminal);

                    return Json(ServiceResult<object>.Successful(new
                    {
                        connectionStatus = "موفق",
                        message = "اتصال به دستگاه با موفقیت برقرار شد",
                        terminalInfo = new
                        {
                            terminalId = terminal.TerminalId,
                            merchantId = terminal.MerchantId,
                            ipAddress = terminal.IpAddress,
                            port = terminal.Port,
                            provider = terminal.Provider.ToString()
                        }
                    }));
                }
                finally
                {
                    driver?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Test: خطای غیرمنتظره در تست اتصال - TerminalId: {TerminalId}", terminalId);
                return Json(ServiceResult.Failed($"خطا در تست اتصال: {ex.Message}"));
            }
        }

        /// <summary>
        /// تست پرداخت کامل (GET - فقط برای نمایش پیام)
        /// </summary>
        [HttpGet]
        [Route("TestPayment")]
        public ActionResult TestPayment()
        {
            return Content("این action فقط از طریق AJAX (POST request) قابل دسترسی است. لطفاً از صفحه اصلی تست استفاده کنید.", "text/html; charset=utf-8");
        }

        /// <summary>
        /// تست پرداخت کامل (POST - برای AJAX)
        /// </summary>
        [HttpPost]
        [Route("TestPayment")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> TestPayment(int terminalId, decimal amount)
        {
            try
            {
                _logger.Information("🔍 POS Test: شروع تست پرداخت - TerminalId: {TerminalId}, Amount: {Amount}, User: {UserName}",
                    terminalId, amount, _currentUserService?.UserName ?? "Unknown");

                if (amount <= 0)
                {
                    return Json(ServiceResult.Failed("مبلغ باید بیشتر از صفر باشد"));
                }

                // دریافت ترمینال
                var terminalResult = await _posManagementService.GetPosTerminalAsync(terminalId);
                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    _logger.Warning("⚠️ POS Test: ترمینال یافت نشد - TerminalId: {TerminalId}", terminalId);
                    return Json(ServiceResult.Failed("ترمینال POS یافت نشد"));
                }

                var terminal = terminalResult.Data;

                // استفاده از PosPaymentOrchestrator برای تست پرداخت (Production-Ready)
                var paymentResult = await _paymentOrchestrator.ProcessPaymentAsync(
                    receptionId: 0, // ReceptionId = 0 برای تست
                    amountIRR: amount,
                    terminalId: terminalId,
                    userId: _currentUserService?.UserId);

                if (!paymentResult.Success)
                {
                    _logger.Error("❌ POS Test: تست پرداخت ناموفق - TerminalId: {TerminalId}, Error: {Error}",
                        terminal.TerminalId, paymentResult.Message);
                    return Json(ServiceResult.Failed(paymentResult.Message));
                }

                _logger.Information("✅ POS Test: تست پرداخت موفق - TerminalId: {TerminalId}, RRN: {RRN}, TraceNo: {TraceNo}",
                    terminal.TerminalId, paymentResult.RRN, paymentResult.TraceNo);

                return Json(ServiceResult<object>.Successful(new
                {
                    success = true,
                    rrn = paymentResult.RRN,
                    traceNo = paymentResult.TraceNo,
                    terminalId = paymentResult.TerminalId,
                    cardLast4 = paymentResult.CardLast4,
                    message = paymentResult.Message,
                    amount = amount,
                    terminalInfo = new
                    {
                        terminalId = terminal.TerminalId,
                        merchantId = terminal.MerchantId,
                        ipAddress = terminal.IpAddress,
                        port = terminal.Port,
                        provider = terminal.Provider.ToString()
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Test: خطای غیرمنتظره در تست پرداخت - TerminalId: {TerminalId}", terminalId);
                return Json(ServiceResult.Failed($"خطا در تست پرداخت: {ex.Message}"));
            }
        }

        /// <summary>
        /// تست سریع اتصال فقط با IP (تست خودکار پورت‌های رایج)
        /// </summary>
        [HttpPost]
        [Route("QuickTestByIp")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> QuickTestByIp(string ipAddress, PosProviderType? providerType = null)
        {
            try
            {
                _logger.Information("🔍 POS Quick Test: شروع تست سریع با IP - IP: {IpAddress}, Provider: {Provider}", 
                    ipAddress, providerType);

                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    return Json(ServiceResult.Failed("آدرس IP الزامی است"));
                }

                // پورت‌های رایج برای تست
                var commonPorts = new[] { 5000, 8080, 9100, 4000 };
                var results = new List<object>();
                int? foundPort = null;

                // تست هر پورت
                foreach (var port in commonPorts)
                {
                    try
                    {
                        _logger.Information("🔍 POS Quick Test: تست پورت {Port} - IP: {IpAddress}", port, ipAddress);

                        // ایجاد یک ترمینال موقت برای تست
                        var testTerminal = new PosTerminal
                        {
                            IpAddress = ipAddress,
                            Port = port,
                            TerminalId = "TEST",
                            MerchantId = "TEST",
                            Provider = providerType ?? PosProviderType.SamanKish,
                            Protocol = PosProtocol.Tcp,
                            IsActive = true
                        };

                        // استفاده از Driver برای تست اتصال
                        var driver = GetDriver(testTerminal.Provider);
                        if (driver == null)
                        {
                            results.Add(new { port, status = "خطا", message = "درایور یافت نشد" });
                            continue;
                        }

                        using (driver)
                        {
                            var connectResult = await driver.ConnectAsync(testTerminal);
                            if (connectResult.Success)
                            {
                                _logger.Information("✅ POS Quick Test: پورت {Port} موفق - IP: {IpAddress}", port, ipAddress);
                                foundPort = port;
                                results.Add(new { port, status = "موفق", message = "اتصال برقرار شد" });
                                await driver.DisconnectAsync(testTerminal);
                                break; // اولین پورت موفق کافی است
                            }
                            else
                            {
                                _logger.Warning("⚠️ POS Quick Test: پورت {Port} ناموفق - IP: {IpAddress}, Error: {Error}", 
                                    port, ipAddress, connectResult.Message);
                                results.Add(new { port, status = "ناموفق", message = connectResult.Message });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "⚠️ POS Quick Test: خطا در تست پورت {Port} - IP: {IpAddress}", port, ipAddress);
                        results.Add(new { port, status = "خطا", message = ex.Message });
                    }
                }

                if (foundPort.HasValue)
                {
                    return Json(ServiceResult<object>.Successful(new
                    {
                        success = true,
                        foundPort = foundPort.Value,
                        message = $"پورت {foundPort.Value} برای اتصال به دستگاه مناسب است",
                        allResults = results
                    }));
                }
                else
                {
                    var errorResult = ServiceResult<object>.Failed(
                        "هیچ یک از پورت‌های رایج (5000, 8080, 9100, 4000) برای اتصال مناسب نیستند. لطفاً پورت را از منوی تنظیمات دستگاه POS بررسی کنید.",
                        "NO_PORT_FOUND");
                    errorResult.Metadata["allResults"] = results;
                    return Json(errorResult);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Quick Test: خطا در تست سریع - IP: {IpAddress}", ipAddress);
                return Json(ServiceResult.Failed($"خطا در تست سریع: {ex.Message}"));
            }
        }

        /// <summary>
        /// دریافت اطلاعات ترمینال
        /// </summary>
        [HttpGet]
        [Route("GetTerminalInfo")]
        public async Task<JsonResult> GetTerminalInfo(int terminalId)
        {
            try
            {
                var terminalResult = await _posManagementService.GetPosTerminalAsync(terminalId);
                if (!terminalResult.Success || terminalResult.Data == null)
                {
                    return Json(ServiceResult.Failed("ترمینال POS یافت نشد"), JsonRequestBehavior.AllowGet);
                }

                var terminal = terminalResult.Data;

                return Json(ServiceResult<object>.Successful(new
                {
                    id = terminal.PosTerminalId,
                    title = terminal.Title,
                    terminalId = terminal.TerminalId,
                    merchantId = terminal.MerchantId,
                    serialNumber = terminal.SerialNumber,
                    ipAddress = terminal.IpAddress,
                    port = terminal.Port,
                    macAddress = terminal.MacAddress,
                    provider = terminal.Provider.ToString(),
                    protocol = terminal.Protocol.ToString(),
                    isActive = terminal.IsActive,
                    isDefault = terminal.IsDefault,
                    createdAt = terminal.CreatedAt,
                    updatedAt = terminal.UpdatedAt
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Test: خطا در دریافت اطلاعات ترمینال - TerminalId: {TerminalId}", terminalId);
                return Json(ServiceResult.Failed($"خطا در دریافت اطلاعات ترمینال: {ex.Message}"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// دریافت Driver مناسب بر اساس Provider
        /// </summary>
        private IPosDeviceDriver GetDriver(PosProviderType provider)
        {
            switch (provider)
            {
                case PosProviderType.SamanKish:
                    return new Services.Payment.POS.Drivers.SamanKishDriver(_logger);

                case PosProviderType.BehPardakht:
                    return new Services.Payment.POS.Drivers.BehpardakhtMelatDriver(_logger);

                default:
                    _logger.Warning("⚠️ POS Test: درایور برای Provider {Provider} یافت نشد، استفاده از Stub", provider);
                    return new Services.Payment.POS.StubPosDeviceDriver(_logger);
            }
        }
    }
}

