using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using ConnectionState = Microsoft.AspNet.SignalR.Client.ConnectionState;

namespace ClinicApp.Services.Payment.POS.Drivers
{
    /// <summary>
    /// Driver for SamanKish POS terminals using SignalR (SSP1126 Web-Based)
    /// 
    /// Protocol: SSP1126 Web-Based with SignalR
    /// Connection: SignalR Hub (SSP1126HUB)
    /// 
    /// Features:
    /// - Real-time communication via SignalR
    /// - Support for multiple clients
    /// - Event-based card swipe detection
    /// - Automatic connection management
    /// 
    /// Configuration:
    /// - SignalR Hub URL: http://localhost:8080/signalr (default)
    /// - Hub Name: SSP1126HUB
    /// - Connection Type: Network (1) or COM (0)
    /// - Account Type: Single (0) or Share (1)
    /// - Language: Farsi (0) or English (1)
    /// 
    /// Flow:
    /// 1. Initial(MediaType, IP, COM, AccountType, Language, Additional)
    /// 2. GetSystemResponse('0' = Success)
    /// 3. SendAmount1Step(Amount, Amounts, Additional, Reference, PurchaseID, TerminalID)
    /// 4. GetCardSwiped (optional - card swipe event)
    /// 5. GetTransactionResponse (final response)
    /// 
    /// Response Codes (from SSP1126 documentation):
    /// - "0" or "00": Success
    /// - Other codes: Various error conditions (see GetErrorMessageFromResponseCode)
    /// 
    /// NOTE: Requires SSP1126SignalRWindowsService to be running
    /// </summary>
    public class SamanKishSignalRDriver : IPosDeviceDriver
    {
        private readonly ILogger _logger;
        private HubConnection _hubConnection;
        private IHubProxy _hubProxy;
        private bool _disposed = false;
        private string _serverMessage = string.Empty;
        private PosPaymentDriverResponse _transactionResponse = null;
        private readonly ManualResetEventSlim _responseWaitHandle = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _transactionWaitHandle = new ManualResetEventSlim(false);
        private readonly object _lockObject = new object();

        // SignalR Configuration
        private const string DefaultSignalRUrl = "http://localhost:8080/signalr";
        private const string HubName = "SSP1126HUB";
        private const int InitializationDelayMs = 1000; // 1 second delay after Initial
        private const int TransactionTimeoutMs = 60000; // 60 seconds for transaction
        private const int ConnectionTimeoutMs = 10000; // 10 seconds for connection

        // Connection Type: 0=COM, 1=Network
        private const int ConnectionTypeNetwork = 1;
        private const int ConnectionTypeCom = 0;

        // Account Type: 0=Single, 1=Share
        private const int AccountTypeSingle = 0;

        // Language: 0=Farsi, 1=English
        private const int LanguageFarsi = 0;

        public SamanKishSignalRDriver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get SignalR Hub URL from configuration or use default
        /// </summary>
        private string GetSignalRUrl()
        {
            try
            {
                // Try to get from app.config or web.config
                var configUrl = ConfigurationManager.AppSettings["SamanKishSignalRUrl"];
                if (!string.IsNullOrWhiteSpace(configUrl))
                {
                    _logger.Debug("🏥 SamanKish SignalR: Using configured URL - {Url}", configUrl);
                    return configUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ SamanKish SignalR: Failed to read SignalR URL from config, using default");
            }

            _logger.Debug("🏥 SamanKish SignalR: Using default URL - {Url}", DefaultSignalRUrl);
            return DefaultSignalRUrl;
        }

        /// <summary>
        /// Connect to SignalR Hub and initialize
        /// </summary>
        private async Task<ServiceResult> ConnectToHubAsync()
        {
            try
            {
                if (_hubConnection != null && _hubConnection.State == ConnectionState.Connected)
                {
                    _logger.Debug("🏥 SamanKish SignalR: Already connected to Hub");
                    return ServiceResult.Successful();
                }

                var hubUrl = GetSignalRUrl();
                _logger.Information("🏥 SamanKish SignalR: Connecting to Hub - {Url}", hubUrl);

                // Create Hub Connection
                _hubConnection = new HubConnection(hubUrl);
                _hubProxy = _hubConnection.CreateHubProxy(HubName);

                // Register client callbacks
                RegisterClientCallbacks();

                // Start connection with LongPolling transport (to avoid "Unknown transport" error)
                // LongPolling is more compatible than WebSocket in some environments
                var startTask = _hubConnection.Start(new LongPollingTransport());
                var timeoutTask = Task.Delay(ConnectionTimeoutMs);
                var completedTask = await Task.WhenAny(startTask, timeoutTask).ConfigureAwait(false);
                
                if (completedTask == timeoutTask)
                {
                    _logger.Error("❌ SamanKish SignalR: Connection timeout");
                    return ServiceResult.Failed("زمان اتصال به SignalR Hub به پایان رسید");
                }
                
                await startTask.ConfigureAwait(false);

                if (_hubConnection.State == ConnectionState.Connected)
                {
                    _logger.Information("✅ SamanKish SignalR: Connected to Hub successfully");
                    return ServiceResult.Successful();
                }
                else
                {
                    _logger.Error("❌ SamanKish SignalR: Connection failed - State: {State}", _hubConnection.State);
                    return ServiceResult.Failed($"اتصال به SignalR Hub ناموفق بود. وضعیت: {_hubConnection.State}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish SignalR: Error connecting to Hub");
                return ServiceResult.Failed($"خطا در اتصال به SignalR Hub: {ex.Message}");
            }
        }

        /// <summary>
        /// Register client callbacks for SignalR Hub
        /// </summary>
        private void RegisterClientCallbacks()
        {
            // GetSystemResponse callback
            _hubProxy.On<string>("GetSystemResponse", (message) =>
            {
                lock (_lockObject)
                {
                    _serverMessage = message;
                    _logger.Information("🏥 SamanKish SignalR: GetSystemResponse received - Message: '{Message}'", message);
                    _responseWaitHandle.Set();
                }
            });

            // GetCardSwiped callback - SignalR Client 2.4.3 sends parameters as IList<object>
            _hubProxy.On<IList<object>>("GetCardSwiped", (parameters) =>
            {
                try
                {
                    var terminalId = parameters != null && parameters.Count > 0 ? parameters[0]?.ToString() : string.Empty;
                    var cardNumberHash = parameters != null && parameters.Count > 1 ? parameters[1]?.ToString() : string.Empty;
                    var cardNumberMask = parameters != null && parameters.Count > 2 ? parameters[2]?.ToString() : string.Empty;
                    var purchaseTypes = parameters != null && parameters.Count > 3 ? parameters[3]?.ToString() : string.Empty;
                    var encryptedNationalCode = parameters != null && parameters.Count > 4 ? parameters[4]?.ToString() : string.Empty;

                    _logger.Information("🏥 SamanKish SignalR: Card Swiped - TerminalId: {TerminalId}, CardMask: {CardMask}",
                        terminalId, cardNumberMask);
                    // Card swiped - transaction is in progress
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "⚠️ SamanKish SignalR: Error parsing GetCardSwiped parameters");
                }
            });

            // GetTransactionResponse callback - SignalR Client 2.4.3 sends parameters as IList<object>
            _hubProxy.On<IList<object>>("GetTransactionResponse", (parameters) =>
            {
                _logger.Information("🔔🔔🔔 SamanKish SignalR: GetTransactionResponse CALLBACK INVOKED - ParametersCount: {Count}",
                    parameters != null ? parameters.Count : 0);
                
                try
                {
                    var terminalId = parameters != null && parameters.Count > 0 ? parameters[0]?.ToString() : string.Empty;
                    var responseCode = parameters != null && parameters.Count > 1 ? parameters[1]?.ToString() : string.Empty;
                    var serialId = parameters != null && parameters.Count > 2 ? parameters[2]?.ToString() : string.Empty;
                    var rrn = parameters != null && parameters.Count > 3 ? parameters[3]?.ToString() : string.Empty;
                    var responseDescription = parameters != null && parameters.Count > 4 ? parameters[4]?.ToString() : string.Empty;
                    var txnDate = parameters != null && parameters.Count > 5 ? parameters[5]?.ToString() : string.Empty;
                    var amount = parameters != null && parameters.Count > 6 ? parameters[6]?.ToString() : string.Empty;
                    var cardNumberMask = parameters != null && parameters.Count > 7 ? parameters[7]?.ToString() : string.Empty;
                    
                    _logger.Information("🔔 SamanKish SignalR: GetTransactionResponse - Parsed Parameters - ResponseCode: {ResponseCode}, RRN: {RRN}",
                        responseCode, rrn);

                    lock (_lockObject)
                    {
                        _logger.Information("🏥 SamanKish SignalR: Transaction Response - TerminalId: {TerminalId}, ResponseCode: {ResponseCode}, RRN: {RRN}, SerialId: {SerialId}",
                            terminalId, responseCode, rrn, serialId);
                        _logger.Information("🏥 SamanKish SignalR: Transaction Response Details - ResponseDescription: '{ResponseDescription}', TxnDate: {TxnDate}, Amount: {Amount}, CardMask: {CardMask}",
                            responseDescription, txnDate, amount, cardNumberMask);

                        // Parse response
                        // ResponseCode "0" or "00" = Success according to SSP1126 documentation
                        var isSuccess = responseCode == "0" || responseCode == "00";
                        
                        // Determine message: Use ResponseDescription if available, otherwise use our error message mapping
                        string finalMessage;
                        if (!string.IsNullOrWhiteSpace(responseDescription))
                        {
                            // Use server's response description (more accurate)
                            finalMessage = responseDescription.Trim();
                            _logger.Information("🏥 SamanKish SignalR: Using ResponseDescription from server: '{Message}'", finalMessage);
                        }
                        else
                        {
                            // Fallback to our error code mapping
                            finalMessage = isSuccess 
                                ? "پرداخت با موفقیت انجام شد" 
                                : GetErrorMessageFromResponseCode(responseCode);
                            _logger.Information("🏥 SamanKish SignalR: Using mapped error message: '{Message}' (ResponseCode: {ResponseCode})", 
                                finalMessage, responseCode);
                        }

                        // Special handling for user cancellation (ResponseCode = "98")
                        if (responseCode == "98")
                        {
                            _logger.Warning("⚠️ SamanKish SignalR: User cancelled transaction - ResponseCode: 98");
                            finalMessage = "عملیات توسط کاربر لغو شد";
                        }

                        _transactionResponse = new PosPaymentDriverResponse
                        {
                            Success = isSuccess,
                            RRN = rrn ?? string.Empty,
                            TraceNo = serialId ?? string.Empty,
                            CardLast4 = ExtractCardLast4(cardNumberMask),
                            Message = finalMessage,
                            ErrorCode = isSuccess ? null : responseCode
                        };

                        if (isSuccess)
                        {
                            _logger.Information("✅ SamanKish SignalR: Transaction successful - RRN: {RRN}, TraceNo: {TraceNo}, CardLast4: {CardLast4}",
                                _transactionResponse.RRN, _transactionResponse.TraceNo, _transactionResponse.CardLast4);
                        }
                        else
                        {
                            _logger.Warning("⚠️ SamanKish SignalR: Transaction failed - ResponseCode: {ResponseCode}, Message: {Message}",
                                responseCode, finalMessage);
                        }

                        _logger.Information("🔔 SamanKish SignalR: Setting _transactionWaitHandle - ResponseCode: {ResponseCode}, Success: {Success}",
                            responseCode, isSuccess);
                        _transactionWaitHandle.Set();
                        _logger.Information("🔔 SamanKish SignalR: _transactionWaitHandle.Set() completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ SamanKish SignalR: Error parsing GetTransactionResponse parameters");
                    lock (_lockObject)
                    {
                        _logger.Warning("⚠️ SamanKish SignalR: Setting _transactionWaitHandle due to exception to avoid deadlock");
                        _transactionWaitHandle.Set(); // Set anyway to avoid deadlock
                    }
                }
            });
        }

        /// <summary>
        /// Extract last 4 digits from masked card number
        /// </summary>
        private string ExtractCardLast4(string cardNumberMask)
        {
            if (string.IsNullOrWhiteSpace(cardNumberMask))
                return string.Empty;

            // Remove non-digit characters and get last 4 digits
            var digits = System.Text.RegularExpressions.Regex.Replace(cardNumberMask, @"[^\d]", "");
            if (digits.Length >= 4)
            {
                return digits.Substring(digits.Length - 4);
            }

            return digits;
        }

        /// <summary>
        /// Get error message from ResponseCode according to SSP1126 documentation
        /// </summary>
        private string GetErrorMessageFromResponseCode(string responseCode)
        {
            if (string.IsNullOrWhiteSpace(responseCode))
                return "خطای نامشخص";

            switch (responseCode)
            {
                case "0":
                case "00":
                    return "تراکنش با موفقیت انجام پذیرفت";
                case "1":
                    return "کارت کشیده شد";
                case "2":
                    return "مبلغ تراکنش نمی‌تواند از حداقل مبلغ کوچکتر باشد";
                case "3":
                    return "عدم ارتباط با دستگاه";
                case "4":
                    return "اطلاعات نامعتبر";
                case "5":
                    return "صفر ریال بدهی";
                case "6":
                    return "خطا در دریافت اطلاعات";
                case "7":
                    return "عدم دسترسی به این عملیات";
                case "8":
                    return "تراکنش یافت نشد";
                case "9":
                    return "ترمینال نامعتبر";
                case "10":
                    return "خطا در پاسخ";
                case "12":
                    return "تراکنش نامعتبر";
                case "13":
                case "79":
                    return "مبلغ نامعتبر";
                case "14":
                    return "خطا در مقداردهی";
                case "20":
                    return "پاسخ نامعتبر";
                case "26":
                    return "خطا در تراکنش";
                case "27":
                    return "این قبض قبلاً پرداخت شده است";
                case "28":
                    return "غیرقابل پرداخت";
                case "30":
                    return "خطا در قالب اطلاعات";
                case "33":
                    return "تاریخ انقضای کارت سپری شده است";
                case "34":
                case "63":
                case "43":
                    return "اخطار امنیتی";
                case "38":
                case "69":
                case "75":
                    return "تعداد دفعات ورود رمز غلط بیش از حد مجاز است";
                case "51":
                    return "موجودی کافی نمی‌باشد";
                case "55":
                    return "رمز کارت نامعتبر است";
                case "57":
                    return "انجام تراکنش مربوطه توسط دارنده کارت مجاز نمی‌باشد";
                case "58":
                    return "انجام تراکنش مربوطه توسط پایانه انجام‌دهنده مجاز نمی‌باشد";
                case "61":
                    return "مبلغ تراکنش بیش از حد مجاز می‌باشد";
                case "68":
                    return "عدم دریافت پاسخ در زمان مناسب";
                case "78":
                    return "کارت غیرفعال می‌باشد";
                case "80":
                case "84":
                case "91":
                    return "عدم پاسخ از سوی صادرکننده کارت";
                case "92":
                    return "مبالغ متفاوت";
                case "96":
                    return "خطای نامشخص";
                case "97":
                    return "عدم ارتباط با مرکز";
                case "98":
                    return "لغو عملیات توسط کاربر";
                case "99":
                    return "عدم دریافت پاسخ در زمان مناسب در کارت";
                default:
                    return $"خطای نامشخص (کد: {responseCode})";
            }
        }

        /// <summary>
        /// Connect to SamanKish POS terminal via SignalR
        /// This method initializes the connection to the SignalR Hub
        /// </summary>
        public async Task<ServiceResult> ConnectAsync(PosTerminal terminal)
        {
            try
            {
                _logger.Information("🏥 SamanKish SignalR: Starting connection - TerminalId: {TerminalId}, IP: {IpAddress}",
                    terminal?.TerminalId, terminal?.IpAddress);

                // Validate terminal configuration
                var validationResult = ValidateTerminalConfiguration(terminal);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Reset state
                lock (_lockObject)
                {
                    _serverMessage = string.Empty;
                    _transactionResponse = null;
                    _responseWaitHandle.Reset();
                    _transactionWaitHandle.Reset();
                }

                // Connect to SignalR Hub
                var connectResult = await ConnectToHubAsync();
                if (!connectResult.Success)
                {
                    return connectResult;
                }

                _logger.Information("✅ SamanKish SignalR: Connection ready - TerminalId: {TerminalId}", terminal.TerminalId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish SignalR: Connection failed - TerminalId: {TerminalId}", terminal?.TerminalId);
                return ServiceResult.Failed($"خطا در اتصال به SignalR Hub: {ex.Message}");
            }
        }

        /// <summary>
        /// Send payment amount to SamanKish POS terminal via SignalR
        /// </summary>
        public async Task<ServiceResult<PosPaymentDriverResponse>> SendPaymentAsync(PosTerminal terminal, decimal amountIRR)
        {
            try
            {
                _logger.Information("🏥 SamanKish SignalR: Starting payment - TerminalId: {TerminalId}, AmountIRR: {AmountIRR:N0}",
                    terminal.TerminalId, amountIRR);

                // Validate amount
                if (amountIRR <= 0)
                {
                    _logger.Error("❌ SamanKish SignalR: Invalid amount - AmountIRR: {AmountIRR}", amountIRR);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (amountIRR > 999999999999) // Max 12 digits
                {
                    _logger.Error("❌ SamanKish SignalR: Amount too large - AmountIRR: {AmountIRR}", amountIRR);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("مبلغ پرداخت بیش از حد مجاز است");
                }

                // Validate terminal configuration
                var validationResult = ValidateTerminalForPayment(terminal);
                if (!validationResult.Success)
                {
                    return ServiceResult<PosPaymentDriverResponse>.Failed(validationResult.Message);
                }

                // Ensure connected to Hub
                if (_hubConnection == null || _hubConnection.State != ConnectionState.Connected)
                {
                    var connectResult = await ConnectToHubAsync();
                    if (!connectResult.Success)
                    {
                        return ServiceResult<PosPaymentDriverResponse>.Failed(connectResult.Message);
                    }
                }

                // Reset state
                lock (_lockObject)
                {
                    _serverMessage = string.Empty;
                    _transactionResponse = null;
                    _responseWaitHandle.Reset();
                    _transactionWaitHandle.Reset();
                }

                // Step 1: Initialize connection
                _logger.Information("🏥 SamanKish SignalR: Initializing - TerminalId: {TerminalId}, IP: {IpAddress}, ConnectionType: {ConnectionType}, AccountType: {AccountType}, Language: {Language}",
                    terminal.TerminalId, terminal.IpAddress, ConnectionTypeNetwork, AccountTypeSingle, LanguageFarsi);

                // Reset wait handle before Initial
                _responseWaitHandle.Reset();
                _serverMessage = string.Empty;

                // ConnectionType: 1 = Network, IP = terminal.IpAddress, Port = null (not used for Network)
                try
                {
                    await _hubProxy.Invoke("Initial", 
                        ConnectionTypeNetwork, 
                        terminal.IpAddress ?? string.Empty, 
                        (object)null, // Port (not used for Network)
                        AccountTypeSingle, 
                        LanguageFarsi, 
                        "0").ConfigureAwait(false);
                    
                    _logger.Information("🏥 SamanKish SignalR: Initial method invoked, waiting for GetSystemResponse...");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ SamanKish SignalR: Error invoking Initial method");
                    return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در فراخوانی متد Initial: {ex.Message}");
                }

                // Wait for GetSystemResponse with timeout
                var initializationTimeout = InitializationDelayMs + 2000; // 3 seconds total
                _logger.Debug("🏥 SamanKish SignalR: Waiting for GetSystemResponse (timeout: {Timeout}ms)...", initializationTimeout);
                
                if (!_responseWaitHandle.Wait(initializationTimeout))
                {
                    _logger.Error("❌ SamanKish SignalR: Initialization timeout after {Timeout}ms - ServerMessage: '{ServerMessage}'", 
                        initializationTimeout, _serverMessage ?? "(empty)");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("زمان انتظار برای پاسخ اولیه به پایان رسید. لطفاً بررسی کنید:\n• Service در حال اجرا است\n• IP Address ترمینال صحیح است\n• دستگاه POS در شبکه قابل دسترس است");
                }

                // Check initialization result
                _logger.Debug("🏥 SamanKish SignalR: GetSystemResponse received - Message: '{Message}'", _serverMessage);
                
                if (string.IsNullOrWhiteSpace(_serverMessage))
                {
                    _logger.Error("❌ SamanKish SignalR: Initialization response is empty");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ اولیه خالی است");
                }

                if (_serverMessage != "0")
                {
                    _logger.Error("❌ SamanKish SignalR: Initialization failed - Response: '{Response}'", _serverMessage);
                    return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در مقداردهی اولیه: {_serverMessage}");
                }

                _logger.Information("✅ SamanKish SignalR: Initialization successful");

                // Step 2: Send payment amount (1 Step Purchase)
                var amountInRials = (long)amountIRR;
                _logger.Information("🏥 SamanKish SignalR: Sending payment - Amount: {Amount:N0} Rials, TerminalId: {TerminalId}", 
                    amountInRials, terminal.TerminalId);

                // Reset transaction wait handle
                _transactionWaitHandle.Reset();
                _transactionResponse = null;

                // SendAmount1Step: Amount, Amounts (null for Single Account), Additional (null), Reference (null), PurchaseID (null), TerminalID
                try
                {
                    await _hubProxy.Invoke("SendAmount1Step",
                        amountInRials.ToString(),
                        (object)null, // Amounts (null for Single Account)
                        (object)null, // Additional
                        (object)null, // Reference
                        (object)null, // PurchaseID
                        terminal.TerminalId ?? string.Empty).ConfigureAwait(false);
                    
                    _logger.Information("🏥 SamanKish SignalR: SendAmount1Step invoked successfully, waiting for card swipe and transaction response...");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "❌ SamanKish SignalR: Error invoking SendAmount1Step");
                    return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در ارسال مبلغ پرداخت: {ex.Message}");
                }

                // Step 3: Wait for transaction response with timeout
                _logger.Information("🏥 SamanKish SignalR: Waiting for transaction response (timeout: {Timeout}ms)...", TransactionTimeoutMs);
                
                var responseReceived = _transactionWaitHandle.Wait(TransactionTimeoutMs);
                _logger.Information("🏥 SamanKish SignalR: Wait completed - ResponseReceived: {ResponseReceived}, HasResponse: {HasResponse}",
                    responseReceived, _transactionResponse != null);
                
                if (!responseReceived)
                {
                    _logger.Error("❌ SamanKish SignalR: Transaction timeout after {Timeout}ms - No response received from device", TransactionTimeoutMs);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("زمان انتظار برای پاسخ تراکنش به پایان رسید. لطفاً کارت را روی دستگاه بکشید یا دوباره تلاش کنید.");
                }

                // Step 4: Return transaction response
                if (_transactionResponse == null)
                {
                    _logger.Error("❌ SamanKish SignalR: Transaction response is null");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ تراکنش دریافت نشد");
                }

                if (!_transactionResponse.Success)
                {
                    var errorMessage = _transactionResponse.Message ?? "پرداخت ناموفق بود";
                    var errorCode = _transactionResponse.ErrorCode ?? "UNKNOWN";
                    
                    _logger.Error("❌ SamanKish SignalR: Payment failed - ErrorCode: {ErrorCode}, Message: {Message}, RRN: {RRN}, TraceNo: {TraceNo}",
                        errorCode, errorMessage, _transactionResponse.RRN, _transactionResponse.TraceNo);
                    
                    // Build detailed error response with metadata
                    var errorResult = ServiceResult<PosPaymentDriverResponse>.Failed(errorMessage);
                    errorResult.Metadata["ErrorCode"] = errorCode;
                    errorResult.Metadata["ResponseCode"] = errorCode;
                    errorResult.Metadata["RRN"] = _transactionResponse.RRN ?? string.Empty;
                    errorResult.Metadata["TraceNo"] = _transactionResponse.TraceNo ?? string.Empty;
                    errorResult.Metadata["CardLast4"] = _transactionResponse.CardLast4 ?? string.Empty;
                    errorResult.Metadata["TerminalId"] = terminal.TerminalId;
                    errorResult.Metadata["AmountIRR"] = amountIRR.ToString();
                    
                    // Special handling for user cancellation
                    if (errorCode == "98")
                    {
                        errorResult.Metadata["IsUserCancellation"] = true;
                        errorResult.Metadata["CanRetry"] = true; // User can retry after cancellation
                    }
                    
                    return errorResult;
                }

                _logger.Information("✅ SamanKish SignalR: Payment successful - RRN: {RRN}, TraceNo: {TraceNo}, CardLast4: {CardLast4}",
                    _transactionResponse.RRN, _transactionResponse.TraceNo, _transactionResponse.CardLast4);

                return ServiceResult<PosPaymentDriverResponse>.Successful(_transactionResponse);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish SignalR: Payment processing failed - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}",
                    terminal?.TerminalId, amountIRR);
                return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در پردازش پرداخت: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect from SignalR Hub
        /// </summary>
        public async Task<ServiceResult> DisconnectAsync(PosTerminal terminal)
        {
            try
            {
                _logger.Information("🏥 SamanKish SignalR: Disconnecting - TerminalId: {TerminalId}", terminal?.TerminalId);

                // Reset state
                lock (_lockObject)
                {
                    _serverMessage = string.Empty;
                    _transactionResponse = null;
                    _responseWaitHandle.Reset();
                    _transactionWaitHandle.Reset();
                }

                // Disconnect from Hub
                if (_hubConnection != null)
                {
                    if (_hubConnection.State == ConnectionState.Connected)
                    {
                        _hubConnection.Stop();
                    }
                    _hubConnection.Dispose();
                    _hubConnection = null;
                    _hubProxy = null;
                }

                _logger.Information("✅ SamanKish SignalR: Disconnected - TerminalId: {TerminalId}", terminal?.TerminalId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish SignalR: Disconnect error - TerminalId: {TerminalId}", terminal?.TerminalId);
                return ServiceResult.Successful(); // Don't fail on disconnect
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate terminal configuration for connection
        /// </summary>
        private ServiceResult ValidateTerminalConfiguration(PosTerminal terminal)
        {
            if (terminal == null)
            {
                return ServiceResult.Failed("ترمینال POS مشخص نشده است");
            }

            if (string.IsNullOrWhiteSpace(terminal.IpAddress))
            {
                return ServiceResult.Failed("آدرس IP ترمینال تنظیم نشده است");
            }

            if (string.IsNullOrWhiteSpace(terminal.TerminalId))
            {
                return ServiceResult.Failed("شماره ترمینال تنظیم نشده است");
            }

            if (string.IsNullOrWhiteSpace(terminal.MerchantId))
            {
                return ServiceResult.Failed("شماره پذیرنده تنظیم نشده است");
            }

            return ServiceResult.Successful();
        }

        /// <summary>
        /// Validate terminal configuration for payment
        /// </summary>
        private ServiceResult ValidateTerminalForPayment(PosTerminal terminal)
        {
            var baseValidation = ValidateTerminalConfiguration(terminal);
            if (!baseValidation.Success)
            {
                return baseValidation;
            }

            if (!terminal.IsActive)
            {
                return ServiceResult.Failed("ترمینال POS غیرفعال است");
            }

            return ServiceResult.Successful();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                if (_hubConnection != null)
                {
                    if (_hubConnection.State == ConnectionState.Connected)
                    {
                        _hubConnection.Stop();
                    }
                    _hubConnection.Dispose();
                    _hubConnection = null;
                    _hubProxy = null;
                }

                _responseWaitHandle?.Dispose();
                _transactionWaitHandle?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "⚠️ SamanKish SignalR: Error during disposal");
            }

            _disposed = true;
        }

        #endregion
    }
}
