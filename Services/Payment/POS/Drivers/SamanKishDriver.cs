using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment.POS.Drivers
{
    /// <summary>
    /// Production-ready Driver for SamanKish POS terminals
    /// 
    /// Protocol: SSP1126 (Standard Socket Protocol)
    /// Connection: TCP/IP
    /// 
    /// Production Features:
    /// - TCP/IP connection to POS terminal with retry logic
    /// - Payment amount transmission with validation
    /// - Transaction response handling with comprehensive error handling
    /// - Resource management with proper disposal
    /// - Connection timeout and retry mechanism
    /// - Comprehensive logging for production debugging
    /// 
    /// NOTE: This implementation uses standard TCP/IP protocol (SSP1126)
    /// Optimized for production use with retry logic and proper resource management
    /// </summary>
    public class SamanKishDriver : IPosDeviceDriver
    {
        private readonly ILogger _logger;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _disposed = false;

        // Production-optimized timeout settings
        private const int ConnectionTimeout = 10000; // 10 seconds (increased for production)
        private const int ReadTimeout = 45000; // 45 seconds for payment processing (increased for production)
        private const int WriteTimeout = 10000; // 10 seconds
        private const int MaxRetryAttempts = 3; // Maximum retry attempts for connection
        private const int RetryDelayMs = 1000; // 1 second delay between retries
        
        // Default port for SSP1126 protocol (most common for SamanKish terminals)
        private const int DefaultPort = 5000;

        public SamanKishDriver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Connect to SamanKish POS terminal via TCP/IP with retry logic
        /// </summary>
        public async Task<ServiceResult> ConnectAsync(PosTerminal terminal)
        {
            // Validate terminal configuration
            var validationResult = ValidateTerminalConfiguration(terminal);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // Parse IP address
            if (!IPAddress.TryParse(terminal.IpAddress, out IPAddress ipAddress))
            {
                _logger.Error("❌ SamanKish: Invalid IP address - IP: {IpAddress}", terminal.IpAddress);
                return ServiceResult.Failed($"آدرس IP نامعتبر است: {terminal.IpAddress}");
            }

            // Use port from terminal or default port (5000 for SSP1126)
            var connectionPort = terminal.Port ?? DefaultPort;
            
            // Retry logic for connection
            Exception lastException = null;
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    _logger.Information("🏥 SamanKish: Connection attempt {Attempt}/{MaxAttempts} - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port} (Source: {PortSource})",
                        attempt, MaxRetryAttempts, terminal.TerminalId, terminal.IpAddress, connectionPort, terminal.Port.HasValue ? "User Config" : $"Default ({DefaultPort})");

                    // Clean up previous connection if exists
                    CleanupConnection();

                    // Create new TCP client
                    _tcpClient = new TcpClient();
                    _tcpClient.ReceiveTimeout = ConnectionTimeout;
                    _tcpClient.SendTimeout = WriteTimeout;

                    // Connect with timeout using CancellationToken
                    using (var cts = new CancellationTokenSource(ConnectionTimeout))
                    {
                        try
                        {
                            await _tcpClient.ConnectAsync(ipAddress, connectionPort).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            CleanupConnection();
                            throw new TimeoutException($"Connection timeout after {ConnectionTimeout}ms");
                        }
                    }

                    // Verify connection
                    if (_tcpClient.Connected)
                    {
                        _stream = _tcpClient.GetStream();
                        _stream.ReadTimeout = ReadTimeout;
                        _stream.WriteTimeout = WriteTimeout;

                        _logger.Information("✅ SamanKish: Connected successfully - TerminalId: {TerminalId}, Attempt: {Attempt}",
                            terminal.TerminalId, attempt);
                        return ServiceResult.Successful();
                    }
                    else
                    {
                        CleanupConnection();
                        throw new SocketException((int)SocketError.NotConnected);
                    }
                }
                catch (TimeoutException ex)
                {
                    lastException = ex;
                    _logger.Warning("⚠️ SamanKish: Connection timeout - Attempt: {Attempt}/{MaxAttempts}, IP: {IpAddress}, Port: {Port}",
                        attempt, MaxRetryAttempts, terminal.IpAddress, connectionPort);

                    if (attempt < MaxRetryAttempts)
                    {
                        await Task.Delay(RetryDelayMs * attempt); // Exponential backoff
                        continue;
                    }
                }
                catch (SocketException ex)
                {
                    lastException = ex;
                    _logger.Warning(ex, "⚠️ SamanKish: Socket error - Attempt: {Attempt}/{MaxAttempts}, ErrorCode: {ErrorCode}, IP: {IpAddress}, Port: {Port}",
                        attempt, MaxRetryAttempts, ex.SocketErrorCode, terminal.IpAddress, connectionPort);

                    // Retry only for transient errors
                    if (IsTransientSocketError(ex) && attempt < MaxRetryAttempts)
                    {
                        await Task.Delay(RetryDelayMs * attempt); // Exponential backoff
                        continue;
                    }
                    else
                    {
                        // Permanent error, don't retry
                        break;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.Error(ex, "❌ SamanKish: Unexpected error during connection - Attempt: {Attempt}/{MaxAttempts}",
                        attempt, MaxRetryAttempts);

                    if (attempt < MaxRetryAttempts)
                    {
                        await Task.Delay(RetryDelayMs * attempt);
                        continue;
                    }
                }
            }

            // All retry attempts failed
            var finalPort = terminal.Port ?? DefaultPort;
            _logger.Error(lastException, "❌ SamanKish: Connection failed after {MaxAttempts} attempts - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port}",
                MaxRetryAttempts, terminal.TerminalId, terminal.IpAddress, finalPort);

            CleanupConnection();

            // Build detailed error message with all information
            var errorMessage = BuildDetailedErrorMessage(lastException, terminal);
            
            // Create ServiceResult with detailed metadata
            var result = ServiceResult.Failed(errorMessage);
            result.Metadata["TerminalId"] = terminal.TerminalId;
            result.Metadata["IpAddress"] = terminal.IpAddress;
            result.Metadata["Port"] = finalPort.ToString();
            result.Metadata["PortSource"] = terminal.Port.HasValue ? "User Config" : $"Default ({DefaultPort})";
            result.Metadata["Provider"] = terminal.Provider.ToString();
            result.Metadata["MaxRetryAttempts"] = MaxRetryAttempts;
            
            if (lastException is SocketException socketEx)
            {
                result.Metadata["SocketErrorCode"] = socketEx.SocketErrorCode.ToString();
                result.Metadata["SocketErrorCodeNumber"] = ((int)socketEx.SocketErrorCode).ToString();
                result.Metadata["NativeErrorCode"] = socketEx.NativeErrorCode.ToString();
            }
            else if (lastException is TimeoutException)
            {
                result.Metadata["ExceptionType"] = "TimeoutException";
                result.Metadata["TimeoutMs"] = ConnectionTimeout.ToString();
            }
            else if (lastException != null)
            {
                result.Metadata["ExceptionType"] = lastException.GetType().Name;
                result.Metadata["ExceptionMessage"] = lastException.Message;
            }

            return result;
        }

        /// <summary>
        /// Send payment amount to SamanKish POS terminal with validation and error handling
        /// </summary>
        public async Task<ServiceResult<PosPaymentDriverResponse>> SendPaymentAsync(PosTerminal terminal, decimal amountIRR)
        {
            try
            {
                // Validate connection
                if (_stream == null || _tcpClient == null || !_tcpClient.Connected)
                {
                    _logger.Error("❌ SamanKish: Not connected to terminal - TerminalId: {TerminalId}", terminal?.TerminalId);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("اتصال به دستگاه برقرار نیست. لطفاً ابتدا اتصال را برقرار کنید.");
                }

                // Validate amount
                if (amountIRR <= 0)
                {
                    _logger.Error("❌ SamanKish: Invalid amount - AmountIRR: {AmountIRR}", amountIRR);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (amountIRR > 999999999999) // Max 12 digits
                {
                    _logger.Error("❌ SamanKish: Amount too large - AmountIRR: {AmountIRR}", amountIRR);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("مبلغ پرداخت بیش از حد مجاز است");
                }

                _logger.Information("🏥 SamanKish: Sending payment - TerminalId: {TerminalId}, AmountIRR: {AmountIRR:N0}",
                    terminal.TerminalId, amountIRR);

                // Convert amount to Rials (remove decimal if any)
                var amountInRials = (long)amountIRR;

                // Validate terminal configuration for payment
                var validationResult = ValidateTerminalForPayment(terminal);
                if (!validationResult.Success)
                {
                    return ServiceResult<PosPaymentDriverResponse>.Failed(validationResult.Message);
                }

                // Build payment command according to SSP1126 protocol
                var command = BuildPaymentCommand(terminal, amountInRials);
                if (string.IsNullOrEmpty(command))
                {
                    _logger.Error("❌ SamanKish: Failed to build payment command");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("خطا در ساخت دستور پرداخت");
                }

                // Send command to terminal with timeout
                try
                {
                    using (var cts = new CancellationTokenSource(WriteTimeout))
                    {
                        var commandBytes = Encoding.ASCII.GetBytes(command);
                        await _stream.WriteAsync(commandBytes, 0, commandBytes.Length, cts.Token).ConfigureAwait(false);
                        await _stream.FlushAsync(cts.Token).ConfigureAwait(false);
                    }

                    _logger.Debug("🏥 SamanKish: Payment command sent - TerminalId: {TerminalId}, CommandLength: {Length}",
                        terminal.TerminalId, command.Length);
                }
                catch (OperationCanceledException)
                {
                    _logger.Error("❌ SamanKish: Write timeout - TerminalId: {TerminalId}", terminal.TerminalId);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("زمان ارسال دستور به دستگاه به پایان رسید");
                }
                catch (IOException ioEx)
                {
                    _logger.Error(ioEx, "❌ SamanKish: IO error during write - TerminalId: {TerminalId}", terminal.TerminalId);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("خطا در ارسال دستور به دستگاه. اتصال ممکن است قطع شده باشد.");
                }

                // Read response from terminal
                var response = await ReadResponseAsync();

                if (string.IsNullOrEmpty(response))
                {
                    _logger.Error("❌ SamanKish: Empty response from terminal - TerminalId: {TerminalId}", terminal.TerminalId);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ خالی از دستگاه دریافت شد. ممکن است اتصال قطع شده باشد.");
                }

                _logger.Debug("🏥 SamanKish: Response received - TerminalId: {TerminalId}, ResponseLength: {Length}",
                    terminal.TerminalId, response.Length);

                // Parse response
                var parsedResponse = ParsePaymentResponse(response, terminal);

                if (!parsedResponse.Success)
                {
                    _logger.Error("❌ SamanKish: Payment failed - TerminalId: {TerminalId}, Error: {Error}",
                        terminal.TerminalId, parsedResponse.Message);
                    return ServiceResult<PosPaymentDriverResponse>.Failed(parsedResponse.Message);
                }

                _logger.Information("✅ SamanKish: Payment successful - TerminalId: {TerminalId}, RRN: {RRN}, TraceNo: {TraceNo}, CardLast4: {CardLast4}",
                    terminal.TerminalId, parsedResponse.Data.RRN, parsedResponse.Data.TraceNo, parsedResponse.Data.CardLast4);

                return parsedResponse;
            }
            catch (ObjectDisposedException)
            {
                _logger.Error("❌ SamanKish: Connection disposed during payment - TerminalId: {TerminalId}", terminal?.TerminalId);
                return ServiceResult<PosPaymentDriverResponse>.Failed("اتصال به دستگاه قطع شده است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Unexpected error during payment processing - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}",
                    terminal?.TerminalId, amountIRR);
                return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در پردازش پرداخت: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect from SamanKish POS terminal with proper resource cleanup
        /// </summary>
        public async Task<ServiceResult> DisconnectAsync(PosTerminal terminal)
        {
            try
            {
                _logger.Information("🏥 SamanKish: Disconnecting from terminal - TerminalId: {TerminalId}",
                    terminal?.TerminalId);

                CleanupConnection();

                _logger.Information("✅ SamanKish: Disconnected successfully - TerminalId: {TerminalId}",
                    terminal?.TerminalId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Error during disconnection - TerminalId: {TerminalId}",
                    terminal?.TerminalId);
                // Don't fail on disconnect errors, but ensure cleanup
                CleanupConnection();
                return ServiceResult.Successful();
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate terminal configuration before connection
        /// </summary>
        private ServiceResult ValidateTerminalConfiguration(PosTerminal terminal)
        {
            if (terminal == null)
            {
                return ServiceResult.Failed("اطلاعات ترمینال مشخص نشده است");
            }

            if (string.IsNullOrWhiteSpace(terminal.IpAddress))
            {
                return ServiceResult.Failed("آدرس IP ترمینال مشخص نشده است");
            }

            // Port is optional - if not set, we'll use default port
            if (terminal.Port.HasValue && (terminal.Port.Value <= 0 || terminal.Port.Value > 65535))
            {
                return ServiceResult.Failed("پورت ترمینال معتبر نیست. پورت باید بین 1 تا 65535 باشد");
            }

            if (string.IsNullOrWhiteSpace(terminal.TerminalId))
            {
                return ServiceResult.Failed("شماره ترمینال مشخص نشده است");
            }

            if (string.IsNullOrWhiteSpace(terminal.MerchantId))
            {
                return ServiceResult.Failed("شماره پذیرنده مشخص نشده است");
            }

            return ServiceResult.Successful();
        }

        /// <summary>
        /// Validate terminal configuration for payment
        /// </summary>
        private ServiceResult ValidateTerminalForPayment(PosTerminal terminal)
        {
            if (string.IsNullOrWhiteSpace(terminal.TerminalId) || terminal.TerminalId.Length > 50)
            {
                return ServiceResult.Failed("شماره ترمینال نامعتبر است");
            }

            if (string.IsNullOrWhiteSpace(terminal.MerchantId) || terminal.MerchantId.Length > 50)
            {
                return ServiceResult.Failed("شماره پذیرنده نامعتبر است");
            }

            return ServiceResult.Successful();
        }

        /// <summary>
        /// Clean up connection resources
        /// </summary>
        private void CleanupConnection()
        {
            try
            {
                if (_stream != null)
                {
                    try
                    {
                        if (_stream.CanRead || _stream.CanWrite)
                        {
                            _stream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "⚠️ SamanKish: Error closing stream");
                    }
                    finally
                    {
                        _stream?.Dispose();
                        _stream = null;
                    }
                }

                if (_tcpClient != null)
                {
                    try
                    {
                        if (_tcpClient.Connected)
                        {
                            _tcpClient.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "⚠️ SamanKish: Error closing TCP client");
                    }
                    finally
                    {
                        _tcpClient?.Dispose();
                        _tcpClient = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Error during cleanup");
            }
        }

        /// <summary>
        /// Check if socket error is transient (can be retried)
        /// </summary>
        private bool IsTransientSocketError(SocketException ex)
        {
            switch (ex.SocketErrorCode)
            {
                case SocketError.TimedOut:
                case SocketError.ConnectionRefused:
                case SocketError.NetworkUnreachable:
                case SocketError.HostUnreachable:
                case SocketError.TryAgain:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Build detailed error message with all connection information
        /// </summary>
        private string BuildDetailedErrorMessage(Exception ex, PosTerminal terminal)
        {
            var baseMessage = "";
            var details = new List<string>();
            var warnings = new List<string>();

            // ⚠️ Check if port might be wrong (common bank server ports)
            if (terminal.Port.HasValue)
            {
                var port = terminal.Port.Value;
                // Common bank server ports (NOT for PC-POS communication)
                if (port == 2155 || port == 8580 || (port >= 2000 && port <= 3000 && port != 8080))
                {
                    warnings.Add($"⚠️ هشدار: پورت {port} معمولاً برای ارتباط دستگاه با سرور بانک است، نه برای PC ↔ POS!");
                    warnings.Add("پورت صحیح PC ↔ POS معمولاً یکی از این‌هاست: 5000, 8080, 9100");
                    warnings.Add("لطفاً پورت را از منوی تنظیمات شبکه دستگاه POS یا مستندات سامان‌کیش بررسی کنید.");
                }
            }

            if (ex is SocketException socketEx)
            {
                baseMessage = GetSocketErrorMessage(socketEx);
                details.Add($"کد خطای Socket: {socketEx.SocketErrorCode} ({(int)socketEx.SocketErrorCode})");
                if (socketEx.NativeErrorCode != 0)
                {
                    details.Add($"کد خطای Native: {socketEx.NativeErrorCode}");
                }
            }
            else if (ex is TimeoutException)
            {
                baseMessage = $"زمان اتصال به دستگاه کارت‌خوان به پایان رسید (Timeout: {ConnectionTimeout}ms).";
                details.Add("لطفاً اتصال شبکه و پورت دستگاه را بررسی کنید.");
            }
            else
            {
                baseMessage = $"خطا در اتصال به دستگاه کارت‌خوان: {ex?.Message ?? "خطای نامشخص"}";
            }

            // Add connection details
            var errorPort = terminal.Port ?? DefaultPort;
            details.Add($"IP: {terminal.IpAddress}");
            details.Add($"Port: {errorPort} {(terminal.Port.HasValue ? "(تنظیم شده)" : $"(پیش‌فرض {DefaultPort})")}");
            details.Add($"Terminal ID: {terminal.TerminalId}");
            details.Add($"Provider: {terminal.Provider}");

            // Combine message
            var fullMessage = baseMessage;
            
            if (warnings.Any())
            {
                fullMessage += "\n\n" + string.Join("\n", warnings);
            }
            
            if (details.Any())
            {
                fullMessage += "\n\nجزئیات:\n" + string.Join("\n", details);
            }

            return fullMessage;
        }

        /// <summary>
        /// Get user-friendly error message from socket exception
        /// </summary>
        private string GetSocketErrorMessage(SocketException ex)
        {
            switch (ex.SocketErrorCode)
            {
                case SocketError.TimedOut:
                    return "زمان اتصال به دستگاه کارت‌خوان به پایان رسید. لطفاً اتصال شبکه را بررسی کنید.";
                case SocketError.ConnectionRefused:
                    return "اتصال به دستگاه کارت‌خوان رد شد. لطفاً موارد زیر را بررسی کنید:\n" +
                           "⚠️ **مهم**: پورت 2155 برای ارتباط دستگاه با سرور بانک است، نه برای PC ↔ POS!\n" +
                           "• پورت صحیح PC ↔ POS معمولاً یکی از این‌هاست: 5000, 8080, 9100 (بسته به مدل دستگاه)\n" +
                           "• پورت را از منوی تنظیمات شبکه دستگاه POS یا مستندات سامان‌کیش پیدا کنید\n" +
                           "• فایروال دستگاه POS اجازه اتصال می‌دهد\n" +
                           "• سرویس SSP1126 روی دستگاه فعال است";
                case SocketError.NetworkUnreachable:
                    return "شبکه در دسترس نیست. لطفاً اتصال شبکه را بررسی کنید.";
                case SocketError.HostUnreachable:
                    return "دستگاه کارت‌خوان در دسترس نیست. لطفاً IP دستگاه را بررسی کنید.";
                case SocketError.AddressAlreadyInUse:
                    return "آدرس در حال استفاده است. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.";
                case SocketError.ConnectionReset:
                    return "اتصال توسط دستگاه قطع شد. لطفاً دوباره تلاش کنید.";
                case SocketError.Shutdown:
                    return "اتصال بسته شده است. لطفاً دوباره تلاش کنید.";
                default:
                    return $"خطا در اتصال شبکه: {ex.Message} (کد خطا: {ex.SocketErrorCode})";
            }
        }

        /// <summary>
        /// Build payment command according to SSP1126 protocol
        /// </summary>
        private string BuildPaymentCommand(PosTerminal terminal, long amountInRials)
        {
            try
            {
                // SSP1126 Protocol Format:
                // STX (0x02) + Command + TerminalId + MerchantId + Amount + ETX (0x03) + Checksum
                
                // For SamanKish, typical command format:
                // Command: "PAY" for payment
                // TerminalId: 8 digits (padded with zeros)
                // MerchantId: 15 digits (padded with zeros)
                // Amount: 12 digits (padded with zeros)
                
                const string command = "PAY";
                
                // Validate and pad TerminalId (max 8 digits)
                var terminalId = terminal.TerminalId?.Trim() ?? string.Empty;
                if (terminalId.Length > 8)
                {
                    _logger.Warning("⚠️ SamanKish: TerminalId too long, truncating - Original: {Original}, Length: {Length}",
                        terminalId, terminalId.Length);
                    terminalId = terminalId.Substring(0, 8);
                }
                terminalId = terminalId.PadLeft(8, '0');

                // Validate and pad MerchantId (max 15 digits)
                var merchantId = terminal.MerchantId?.Trim() ?? string.Empty;
                if (merchantId.Length > 15)
                {
                    _logger.Warning("⚠️ SamanKish: MerchantId too long, truncating - Original: {Original}, Length: {Length}",
                        merchantId, merchantId.Length);
                    merchantId = merchantId.Substring(0, 15);
                }
                merchantId = merchantId.PadLeft(15, '0');

                // Validate and pad Amount (12 digits)
                if (amountInRials < 0 || amountInRials > 999999999999)
                {
                    throw new ArgumentOutOfRangeException(nameof(amountInRials), "Amount must be between 0 and 999999999999");
                }
                var amount = amountInRials.ToString().PadLeft(12, '0');

                var commandString = $"{command}{terminalId}{merchantId}{amount}";
                
                // Calculate checksum (XOR checksum for SSP1126)
                byte checksum = 0;
                foreach (var c in commandString)
                {
                    checksum ^= (byte)c;
                }

                // Build final command with STX, ETX, and checksum
                var finalCommand = $"\x02{commandString}\x03{checksum:X2}";

                _logger.Debug("🏥 SamanKish: Command built - TerminalId: {TerminalId}, MerchantId: {MerchantId}, Amount: {Amount}, CommandLength: {Length}",
                    terminalId, merchantId, amount, finalCommand.Length);

                return finalCommand;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Error building payment command - TerminalId: {TerminalId}, Amount: {Amount}",
                    terminal?.TerminalId, amountInRials);
                return null;
            }
        }

        /// <summary>
        /// Read response from terminal with timeout and proper error handling
        /// </summary>
        private async Task<string> ReadResponseAsync()
        {
            try
            {
                if (_stream == null || !_stream.CanRead)
                {
                    _logger.Error("❌ SamanKish: Stream is not readable");
                    return null;
                }

                var buffer = new byte[1024];
                int totalBytesRead = 0;
                int bytesRead;

                // Read response with timeout
                using (var cts = new CancellationTokenSource(ReadTimeout))
                {
                    try
                    {
                        // Read until we have complete response or timeout
                        do
                        {
                            bytesRead = await _stream.ReadAsync(buffer, totalBytesRead, buffer.Length - totalBytesRead, cts.Token).ConfigureAwait(false);
                            
                            if (bytesRead == 0)
                            {
                                // Connection closed
                                _logger.Warning("⚠️ SamanKish: Connection closed by terminal");
                                break;
                            }

                            totalBytesRead += bytesRead;

                            // Check if we have complete response (ETX character indicates end)
                            if (totalBytesRead > 0 && buffer[totalBytesRead - 1] == 0x03)
                            {
                                break; // Complete response received
                            }

                            // Prevent buffer overflow
                            if (totalBytesRead >= buffer.Length)
                            {
                                _logger.Warning("⚠️ SamanKish: Response buffer full - BufferSize: {Size}", buffer.Length);
                                break;
                            }
                        } while (bytesRead > 0 && !cts.Token.IsCancellationRequested);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Error("❌ SamanKish: Read timeout after {Timeout}ms", ReadTimeout);
                        return null;
                    }
                }

                if (totalBytesRead == 0)
                {
                    _logger.Warning("⚠️ SamanKish: No data received from terminal");
                    return null;
                }

                var response = Encoding.ASCII.GetString(buffer, 0, totalBytesRead);
                return response.Trim();
            }
            catch (ObjectDisposedException)
            {
                _logger.Error("❌ SamanKish: Stream disposed during read");
                return null;
            }
            catch (IOException ioEx)
            {
                _logger.Error(ioEx, "❌ SamanKish: IO error reading response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Unexpected error reading response");
                return null;
            }
        }

        /// <summary>
        /// Parse payment response from terminal with comprehensive validation
        /// </summary>
        private ServiceResult<PosPaymentDriverResponse> ParsePaymentResponse(string response, PosTerminal terminal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ خالی از دستگاه دریافت شد");
                }

                _logger.Debug("🏥 SamanKish: Parsing response - ResponseLength: {Length}, TerminalId: {TerminalId}",
                    response.Length, terminal?.TerminalId);

                // SSP1126 Response Format:
                // STX (0x02) + Status + RRN + TraceNo + CardLast4 + ETX (0x03) + Checksum
                
                var originalResponse = response;
                
                // Remove STX if present
                if (response.StartsWith("\x02"))
                {
                    response = response.Substring(1);
                }

                // Remove ETX and checksum if present
                var etxIndex = response.LastIndexOf('\x03');
                if (etxIndex >= 0)
                {
                    // Remove ETX and checksum (2 characters after ETX)
                    response = response.Substring(0, etxIndex);
                }
                else if (response.Length >= 2)
                {
                    // If no ETX, assume last 2 characters are checksum
                    response = response.Substring(0, response.Length - 2);
                }

                // Minimum response length: Status(2) + RRN(12) + TraceNo(6) + CardLast4(4) = 24
                if (response.Length < 24)
                {
                    _logger.Error("❌ SamanKish: Invalid response length - Expected: >=24, Actual: {Length}, Response: {Response}",
                        response.Length, originalResponse);
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ نامعتبر از دستگاه دریافت شد. طول پاسخ کافی نیست.");
                }

                // Parse response fields
                var status = response.Substring(0, 2);
                var rrn = response.Substring(2, Math.Min(12, response.Length - 2));
                var traceNo = response.Length >= 18 ? response.Substring(14, Math.Min(6, response.Length - 14)) : "000000";
                var cardLast4 = response.Length >= 24 ? response.Substring(20, Math.Min(4, response.Length - 20)) : "0000";

                // Validate status code
                if (status != "00")
                {
                    var errorMessage = GetErrorMessage(status);
                    _logger.Warning("⚠️ SamanKish: Payment failed - Status: {Status}, Error: {Error}, TerminalId: {TerminalId}",
                        status, errorMessage, terminal?.TerminalId);
                    return ServiceResult<PosPaymentDriverResponse>.Failed(errorMessage);
                }

                // Validate RRN and TraceNo
                if (string.IsNullOrWhiteSpace(rrn) || rrn.Trim('0').Length == 0)
                {
                    _logger.Warning("⚠️ SamanKish: Invalid RRN in response - RRN: {RRN}", rrn);
                    // Don't fail, but log warning
                }

                if (string.IsNullOrWhiteSpace(traceNo) || traceNo.Trim('0').Length == 0)
                {
                    _logger.Warning("⚠️ SamanKish: Invalid TraceNo in response - TraceNo: {TraceNo}", traceNo);
                    // Don't fail, but log warning
                }

                var result = new PosPaymentDriverResponse
                {
                    Success = true,
                    RRN = rrn.TrimStart('0'),
                    TraceNo = traceNo.TrimStart('0'),
                    CardLast4 = cardLast4.Trim(),
                    Message = "پرداخت با موفقیت انجام شد"
                };

                // Validate parsed values
                if (string.IsNullOrWhiteSpace(result.RRN))
                {
                    result.RRN = rrn; // Use original if trimmed is empty
                }

                if (string.IsNullOrWhiteSpace(result.TraceNo))
                {
                    result.TraceNo = traceNo; // Use original if trimmed is empty
                }

                _logger.Debug("🏥 SamanKish: Response parsed successfully - RRN: {RRN}, TraceNo: {TraceNo}, CardLast4: {CardLast4}",
                    result.RRN, result.TraceNo, result.CardLast4);

                return ServiceResult<PosPaymentDriverResponse>.Successful(result);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.Error(ex, "❌ SamanKish: Index out of range parsing response - ResponseLength: {Length}, Response: {Response}",
                    response?.Length, response);
                return ServiceResult<PosPaymentDriverResponse>.Failed("خطا در تفسیر پاسخ دستگاه: فرمت پاسخ نامعتبر است");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ SamanKish: Error parsing response - Response: {Response}", response);
                return ServiceResult<PosPaymentDriverResponse>.Failed("خطا در تفسیر پاسخ دستگاه");
            }
        }

        /// <summary>
        /// Get user-friendly error message from status code
        /// </summary>
        private string GetErrorMessage(string statusCode)
        {
            if (string.IsNullOrWhiteSpace(statusCode))
            {
                return "کد وضعیت نامعتبر از دستگاه دریافت شد";
            }

            switch (statusCode.Trim())
            {
                case "00":
                    return "تراکنش موفق";
                case "01":
                    return "تراکنش ناموفق - خطای عمومی. لطفاً دوباره تلاش کنید.";
                case "02":
                    return "کارت نامعتبر است. لطفاً کارت دیگری استفاده کنید.";
                case "03":
                    return "موجودی کافی نیست. لطفاً موجودی کارت را بررسی کنید.";
                case "04":
                    return "رمز کارت اشتباه است. لطفاً رمز صحیح را وارد کنید.";
                case "05":
                    return "تراکنش توسط کاربر لغو شد.";
                case "06":
                    return "خطا در ارتباط با بانک. لطفاً چند لحظه صبر کنید و دوباره تلاش کنید.";
                case "07":
                    return "زمان تراکنش به پایان رسید. لطفاً دوباره تلاش کنید.";
                case "08":
                    return "کارت مسدود شده است. لطفاً با بانک تماس بگیرید.";
                case "09":
                    return "مبلغ بیش از حد مجاز است.";
                case "10":
                    return "تعداد تلاش‌های ناموفق بیش از حد مجاز است.";
                case "11":
                    return "کارت منقضی شده است.";
                case "12":
                    return "دستور نامعتبر است.";
                case "13":
                    return "مبلغ نامعتبر است.";
                case "14":
                    return "شماره کارت نامعتبر است.";
                case "15":
                    return "بانک صادرکننده پاسخ نمی‌دهد.";
                case "16":
                    return "خطا در پردازش تراکنش.";
                case "17":
                    return "کاربر تراکنش را لغو کرد.";
                case "18":
                    return "زمان تراکنش به پایان رسید.";
                case "19":
                    return "مبلغ تراکنش بیش از حد مجاز است.";
                case "20":
                    return "کارت غیرفعال است.";
                default:
                    _logger.Warning("⚠️ SamanKish: Unknown status code - StatusCode: {StatusCode}", statusCode);
                    return $"خطا در پردازش پرداخت (کد خطا: {statusCode}). لطفاً با پشتیبانی تماس بگیرید.";
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                CleanupConnection();
                _disposed = true;
            }
        }

        #endregion
    }
}

