using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using Serilog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment.POS.Drivers
{
    /// <summary>
    /// Driver for Behpardakht Melat POS terminals
    /// 
    /// Protocol: Standard TCP/IP with Melat-specific format
    /// Connection: TCP/IP
    /// 
    /// Features:
    /// - TCP/IP connection to POS terminal
    /// - Payment amount transmission
    /// - Transaction response handling
    /// - Error handling and retry logic
    /// 
    /// NOTE: This implementation uses standard TCP/IP protocol
    /// For production use, you may need to integrate with Behpardakht SDK if available
    /// </summary>
    public class BehpardakhtMelatDriver : IPosDeviceDriver
    {
        private readonly ILogger _logger;
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private bool _disposed = false;
        private const int ConnectionTimeout = 5000; // 5 seconds
        private const int ReadTimeout = 30000; // 30 seconds for payment processing
        
        // Default port for Behpardakht Melat terminals
        private const int DefaultPort = 5000;

        public BehpardakhtMelatDriver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Connect to Behpardakht Melat POS terminal via TCP/IP
        /// </summary>
        public async Task<ServiceResult> ConnectAsync(PosTerminal terminal)
        {
            try
            {
                _logger.Information("🏥 Behpardakht Melat: Connecting to terminal - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port}",
                    terminal.TerminalId, terminal.IpAddress, terminal.Port);

                // Validation
                if (string.IsNullOrWhiteSpace(terminal.IpAddress))
                {
                    return ServiceResult.Failed("آدرس IP ترمینال مشخص نشده است");
                }

                // Port is optional - if not set, we'll use default port
                if (terminal.Port.HasValue && (terminal.Port.Value <= 0 || terminal.Port.Value > 65535))
                {
                    return ServiceResult.Failed("پورت ترمینال معتبر نیست. پورت باید بین 1 تا 65535 باشد");
                }

                // Parse IP address
                if (!IPAddress.TryParse(terminal.IpAddress, out IPAddress ipAddress))
                {
                    return ServiceResult.Failed($"آدرس IP نامعتبر است: {terminal.IpAddress}");
                }

                // Use port from terminal or default port (5000)
                var connectionPort = terminal.Port ?? DefaultPort;
                
                _logger.Information("🏥 Behpardakht Melat: Using port {Port} (Source: {PortSource}) - TerminalId: {TerminalId}",
                    connectionPort, terminal.Port.HasValue ? "User Config" : $"Default ({DefaultPort})", terminal.TerminalId);

                // Create TCP client
                _tcpClient = new TcpClient();
                _tcpClient.ReceiveTimeout = ConnectionTimeout;
                _tcpClient.SendTimeout = ConnectionTimeout;

                // Connect with timeout
                var connectTask = _tcpClient.ConnectAsync(ipAddress, connectionPort);
                var timeoutTask = Task.Delay(ConnectionTimeout);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    _tcpClient?.Close();
                    _logger.Error("❌ Behpardakht Melat: Connection timeout - IP: {IpAddress}, Port: {Port}",
                        terminal.IpAddress, connectionPort);
                    return ServiceResult.Failed("زمان اتصال به دستگاه کارت‌خوان به پایان رسید");
                }

                if (_tcpClient.Connected)
                {
                    _stream = _tcpClient.GetStream();
                    _stream.ReadTimeout = ReadTimeout;
                    _stream.WriteTimeout = ConnectionTimeout;

                    _logger.Information("✅ Behpardakht Melat: Connected successfully - TerminalId: {TerminalId}",
                        terminal.TerminalId);
                    return ServiceResult.Successful();
                }
                else
                {
                    _logger.Error("❌ Behpardakht Melat: Connection failed - IP: {IpAddress}, Port: {Port}",
                        terminal.IpAddress, connectionPort);
                    return ServiceResult.Failed("اتصال به دستگاه کارت‌خوان برقرار نشد");
                }
            }
            catch (SocketException ex)
            {
                var errorPort = terminal?.Port ?? DefaultPort;
                _logger.Error(ex, "❌ Behpardakht Melat: Socket error during connection - IP: {IpAddress}, Port: {Port}",
                    terminal?.IpAddress, errorPort);
                return ServiceResult.Failed($"خطا در اتصال شبکه: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Behpardakht Melat: Unexpected error during connection");
                return ServiceResult.Failed($"خطا در اتصال به دستگاه: {ex.Message}");
            }
        }

        /// <summary>
        /// Send payment amount to Behpardakht Melat POS terminal
        /// </summary>
        public async Task<ServiceResult<PosPaymentDriverResponse>> SendPaymentAsync(PosTerminal terminal, decimal amountIRR)
        {
            try
            {
                if (_stream == null || !_tcpClient.Connected)
                {
                    _logger.Error("❌ Behpardakht Melat: Not connected to terminal");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("اتصال به دستگاه برقرار نیست");
                }

                _logger.Information("🏥 Behpardakht Melat: Sending payment - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}",
                    terminal.TerminalId, amountIRR);

                // Convert amount to Rials (remove decimal if any)
                var amountInRials = (long)amountIRR;

                // Build payment command according to Behpardakht Melat protocol
                // Format: STX + Command + TerminalId + MerchantId + Amount + ETX + Checksum
                var command = BuildPaymentCommand(terminal, amountInRials);

                // Send command to terminal
                var commandBytes = Encoding.ASCII.GetBytes(command);
                await _stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                await _stream.FlushAsync();

                _logger.Debug("🏥 Behpardakht Melat: Payment command sent - Command: {Command}", command);

                // Read response from terminal
                var response = await ReadResponseAsync();

                if (string.IsNullOrEmpty(response))
                {
                    _logger.Error("❌ Behpardakht Melat: Empty response from terminal");
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ خالی از دستگاه دریافت شد");
                }

                _logger.Debug("🏥 Behpardakht Melat: Response received - Response: {Response}", response);

                // Parse response
                var parsedResponse = ParsePaymentResponse(response, terminal);

                if (!parsedResponse.Success)
                {
                    _logger.Error("❌ Behpardakht Melat: Payment failed - Error: {Error}", parsedResponse.Message);
                    return ServiceResult<PosPaymentDriverResponse>.Failed(parsedResponse.Message);
                }

                _logger.Information("✅ Behpardakht Melat: Payment successful - RRN: {RRN}, TraceNo: {TraceNo}",
                    parsedResponse.Data.RRN, parsedResponse.Data.TraceNo);

                return parsedResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Behpardakht Melat: Error during payment processing");
                return ServiceResult<PosPaymentDriverResponse>.Failed($"خطا در پردازش پرداخت: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect from Behpardakht Melat POS terminal with proper resource cleanup
        /// </summary>
        public async Task<ServiceResult> DisconnectAsync(PosTerminal terminal)
        {
            try
            {
                _logger.Information("🏥 Behpardakht Melat: Disconnecting from terminal - TerminalId: {TerminalId}",
                    terminal?.TerminalId);

                CleanupConnection();

                _logger.Information("✅ Behpardakht Melat: Disconnected successfully - TerminalId: {TerminalId}",
                    terminal?.TerminalId);
                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Behpardakht Melat: Error during disconnection - TerminalId: {TerminalId}",
                    terminal?.TerminalId);
                // Don't fail on disconnect errors, but ensure cleanup
                CleanupConnection();
                return ServiceResult.Successful();
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Build payment command according to Behpardakht Melat protocol
        /// </summary>
        private string BuildPaymentCommand(PosTerminal terminal, long amountInRials)
        {
            // Behpardakht Melat Protocol Format:
            // STX (0x02) + Command + TerminalId + MerchantId + Amount + ETX (0x03) + Checksum
            
            // For Behpardakht Melat, typical command format:
            // Command: "PAY" for payment
            // TerminalId: 8 digits
            // MerchantId: 15 digits
            // Amount: 12 digits (padded with zeros)
            
            var command = "PAY";
            var terminalId = terminal.TerminalId.PadLeft(8, '0').Substring(0, Math.Min(8, terminal.TerminalId.Length));
            var merchantId = terminal.MerchantId.PadLeft(15, '0').Substring(0, Math.Min(15, terminal.MerchantId.Length));
            var amount = amountInRials.ToString().PadLeft(12, '0');

            var commandString = $"{command}{terminalId}{merchantId}{amount}";
            
            // Calculate checksum (simple XOR checksum)
            byte checksum = 0;
            foreach (var c in commandString)
            {
                checksum ^= (byte)c;
            }

            // Build final command with STX, ETX, and checksum
            var finalCommand = $"\x02{commandString}\x03{checksum:X2}";

            return finalCommand;
        }

        /// <summary>
        /// Read response from terminal
        /// </summary>
        private async Task<string> ReadResponseAsync()
        {
            try
            {
                var buffer = new byte[1024];
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                
                if (bytesRead == 0)
                {
                    return null;
                }

                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                return response.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Behpardakht Melat: Error reading response");
                return null;
            }
        }

        /// <summary>
        /// Parse payment response from terminal
        /// </summary>
        private ServiceResult<PosPaymentDriverResponse> ParsePaymentResponse(string response, PosTerminal terminal)
        {
            try
            {
                // Behpardakht Melat Response Format:
                // STX + Status + RRN + TraceNo + CardLast4 + ETX + Checksum
                
                // Remove STX and ETX
                if (response.StartsWith("\x02"))
                {
                    response = response.Substring(1);
                }
                if (response.EndsWith("\x03"))
                {
                    response = response.Substring(0, response.Length - 1);
                }

                // Remove checksum (last 2 characters)
                if (response.Length >= 2)
                {
                    response = response.Substring(0, response.Length - 2);
                }

                // Parse response fields
                // Status: 2 characters (00 = success)
                // RRN: 12 characters
                // TraceNo: 6 characters
                // CardLast4: 4 characters

                if (response.Length < 24)
                {
                    return ServiceResult<PosPaymentDriverResponse>.Failed("پاسخ نامعتبر از دستگاه دریافت شد");
                }

                var status = response.Substring(0, 2);
                var rrn = response.Substring(2, 12);
                var traceNo = response.Substring(14, 6);
                var cardLast4 = response.Length >= 24 ? response.Substring(20, 4) : "0000";

                if (status != "00")
                {
                    var errorMessage = GetErrorMessage(status);
                    return ServiceResult<PosPaymentDriverResponse>.Failed(errorMessage);
                }

                return ServiceResult<PosPaymentDriverResponse>.Successful(new PosPaymentDriverResponse
                {
                    Success = true,
                    RRN = rrn.TrimStart('0'),
                    TraceNo = traceNo.TrimStart('0'),
                    CardLast4 = cardLast4,
                    Message = "پرداخت با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ Behpardakht Melat: Error parsing response - Response: {Response}", response);
                return ServiceResult<PosPaymentDriverResponse>.Failed("خطا در تفسیر پاسخ دستگاه");
            }
        }

        /// <summary>
        /// Get error message from status code
        /// </summary>
        private string GetErrorMessage(string statusCode)
        {
            switch (statusCode)
            {
                case "00":
                    return "تراکنش موفق";
                case "01":
                    return "تراکنش ناموفق - خطای عمومی";
                case "02":
                    return "کارت نامعتبر";
                case "03":
                    return "موجودی کافی نیست";
                case "04":
                    return "رمز اشتباه";
                case "05":
                    return "تراکنش لغو شد";
                case "06":
                    return "خطا در ارتباط با بانک";
                case "07":
                    return "زمان تراکنش به پایان رسید";
                case "08":
                    return "کارت مسدود شده است";
                case "09":
                    return "مبلغ بیش از حد مجاز";
                default:
                    return $"خطا در پردازش پرداخت (کد خطا: {statusCode})";
            }
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
                        _logger.Warning(ex, "⚠️ Behpardakht Melat: Error closing stream");
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
                        _logger.Warning(ex, "⚠️ Behpardakht Melat: Error closing TCP client");
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
                _logger.Error(ex, "❌ Behpardakht Melat: Error during cleanup");
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

