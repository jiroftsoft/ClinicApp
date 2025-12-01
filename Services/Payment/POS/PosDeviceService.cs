using ClinicApp.Interfaces.Payment.POS;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Services.Payment.POS.Drivers;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ClinicApp.Services.Payment.POS
{
    /// <summary>
    /// Service for POS Device Communication
    /// 
    /// Responsibility: Direct communication with physical POS terminal devices
    /// Purpose: Send payment amounts to POS devices and receive transaction responses
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: Only device communication logic
    /// ✅ Separation of Concerns: Device communication separated from business logic
    /// ✅ Provider Agnostic: Supports multiple POS providers via driver pattern
    /// ✅ Error Handling: Comprehensive error handling and logging
    /// 
    /// Current Implementation:
    /// - Base structure with driver factory pattern
    /// - Stub implementation for immediate use
    /// - Ready for real driver implementations (SamanKish, AsanPardakht, etc.)
    /// </summary>
    public class PosDeviceService : IPosDeviceService
    {
        private readonly ILogger _logger;

        public PosDeviceService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Process payment by sending amount to POS terminal device
        /// </summary>
        public async Task<ServiceResult<PosPaymentResponse>> ProcessPaymentAsync(
            PosTerminal terminal, 
            decimal amountIRR, 
            int receptionId)
        {
            try
            {
                _logger.Information("🏥 POS Device: Starting payment process - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}, ReceptionId: {ReceptionId}",
                    terminal?.TerminalId, amountIRR, receptionId);

                // Validation
                if (terminal == null)
                {
                    _logger.Warning("⚠️ POS Device: Terminal is null");
                    return ServiceResult<PosPaymentResponse>.Failed("ترمینال POS یافت نشد");
                }

                if (amountIRR <= 0)
                {
                    _logger.Warning("⚠️ POS Device: Invalid amount - AmountIRR: {AmountIRR}", amountIRR);
                    return ServiceResult<PosPaymentResponse>.Failed("مبلغ پرداخت باید بیشتر از صفر باشد");
                }

                if (!terminal.IsActive)
                {
                    _logger.Warning("⚠️ POS Device: Terminal is not active - TerminalId: {TerminalId}", terminal.TerminalId);
                    return ServiceResult<PosPaymentResponse>.Failed("ترمینال POS فعال نیست");
                }

                // Get appropriate driver based on provider
                var driver = GetDriver(terminal.Provider, terminal);
                if (driver == null)
                {
                    _logger.Warning("⚠️ POS Device: Driver not found for provider - Provider: {Provider}", terminal.Provider);
                    return ServiceResult<PosPaymentResponse>.Failed($"درایور برای ارائه‌دهنده {terminal.Provider} یافت نشد");
                }

                // Connect to device
                _logger.Information("🏥 POS Device: Connecting to terminal - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port}",
                    terminal.TerminalId, terminal.IpAddress, terminal.Port);

                var connectResult = await driver.ConnectAsync(terminal);
                if (!connectResult.Success)
                {
                    _logger.Error("❌ POS Device: Connection failed - TerminalId: {TerminalId}, IP: {IpAddress}, Port: {Port}, Error: {Error}",
                        terminal.TerminalId, terminal.IpAddress, terminal.Port, connectResult.Message);
                    
                    // Build detailed error response with metadata
                    var errorResult = ServiceResult<PosPaymentResponse>.Failed(connectResult.Message);
                    errorResult.Metadata["ConnectionError"] = true;
                    errorResult.Metadata["TerminalId"] = terminal.TerminalId;
                    errorResult.Metadata["IpAddress"] = terminal.IpAddress;
                    errorResult.Metadata["Port"] = terminal.Port?.ToString() ?? "نامشخص";
                    errorResult.Metadata["Provider"] = terminal.Provider.ToString();
                    
                    // Copy metadata from connect result if available
                    if (connectResult.Metadata != null)
                    {
                        foreach (var kvp in connectResult.Metadata)
                        {
                            errorResult.Metadata[kvp.Key] = kvp.Value;
                        }
                    }
                    
                    return errorResult;
                }

                // Send payment amount to device
                _logger.Information("🏥 POS Device: Sending payment amount to device - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}",
                    terminal.TerminalId, amountIRR);

                var paymentResult = await driver.SendPaymentAsync(terminal, amountIRR);
                
                // Disconnect from device
                await driver.DisconnectAsync(terminal);

                if (!paymentResult.Success)
                {
                    _logger.Error("❌ POS Device: Payment processing failed - TerminalId: {TerminalId}, Amount: {Amount}, Error: {Error}",
                        terminal.TerminalId, amountIRR, paymentResult.Message);
                    
                    // Build detailed error response with metadata
                    var errorResult = ServiceResult<PosPaymentResponse>.Failed(paymentResult.Message);
                    errorResult.Metadata["PaymentError"] = true;
                    errorResult.Metadata["TerminalId"] = terminal.TerminalId;
                    errorResult.Metadata["IpAddress"] = terminal.IpAddress;
                    errorResult.Metadata["Port"] = terminal.Port?.ToString() ?? "نامشخص";
                    errorResult.Metadata["Provider"] = terminal.Provider.ToString();
                    errorResult.Metadata["AmountIRR"] = amountIRR.ToString();
                    errorResult.Metadata["ReceptionId"] = receptionId.ToString();
                    
                    // Copy metadata from payment result if available
                    if (paymentResult.Metadata != null)
                    {
                        foreach (var kvp in paymentResult.Metadata)
                        {
                            errorResult.Metadata[kvp.Key] = kvp.Value;
                        }
                    }
                    
                    return errorResult;
                }

                _logger.Information("✅ POS Device: Payment processed successfully - TerminalId: {TerminalId}, RRN: {RRN}, TraceNo: {TraceNo}",
                    terminal.TerminalId, paymentResult.Data.RRN, paymentResult.Data.TraceNo);

                return ServiceResult<PosPaymentResponse>.Successful(new PosPaymentResponse
                {
                    Success = true,
                    RRN = paymentResult.Data.RRN,
                    TraceNo = paymentResult.Data.TraceNo,
                    TerminalId = terminal.TerminalId,
                    CardLast4 = paymentResult.Data.CardLast4,
                    Message = "پرداخت با موفقیت انجام شد"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ POS Device: Unexpected error in payment processing - TerminalId: {TerminalId}, ReceptionId: {ReceptionId}",
                    terminal?.TerminalId, receptionId);
                return ServiceResult<PosPaymentResponse>.Failed("خطا در پردازش پرداخت POS");
            }
        }

        /// <summary>
        /// Get appropriate driver based on POS provider type and protocol
        /// </summary>
        private IPosDeviceDriver GetDriver(PosProviderType provider, PosTerminal terminal = null)
        {
            switch (provider)
            {
                case PosProviderType.SamanKish:
                    // اگر Protocol = SignalR باشد، از SignalR Driver استفاده کن
                    if (terminal != null && terminal.Protocol == PosProtocol.SignalR)
                    {
                        _logger.Information("✅ POS Device: Using SamanKish SignalR driver");
                        return new SamanKishSignalRDriver(_logger);
                    }
                    // در غیر این صورت از TCP/IP Driver استفاده کن
                    _logger.Information("✅ POS Device: Using SamanKish TCP/IP driver");
                    return new SamanKishDriver(_logger);

                case PosProviderType.BehPardakht:
                    _logger.Information("✅ POS Device: Using Behpardakht Melat driver");
                    return new BehpardakhtMelatDriver(_logger);

                case PosProviderType.AsanPardakht:
                    // TODO: Implement AsanPardakhtDriver when SDK is available
                    _logger.Warning("⚠️ POS Device: AsanPardakht driver not yet implemented, using stub");
                    return new StubPosDeviceDriver(_logger);

                case PosProviderType.Fanava:
                case PosProviderType.IranKish:
                case PosProviderType.PardakhtAria:
                case PosProviderType.NadaPay:
                    // TODO: Implement drivers for other providers when needed
                    _logger.Warning("⚠️ POS Device: Driver for provider {Provider} not yet implemented, using stub", provider);
                    return new StubPosDeviceDriver(_logger);

                default:
                    _logger.Warning("⚠️ POS Device: Unknown provider, using stub - Provider: {Provider}", provider);
                    return new StubPosDeviceDriver(_logger);
            }
        }
    }

    #region POS Device Driver Interface and Stub Implementation

    /// <summary>
    /// Interface for POS device drivers
    /// Each provider (SamanKish, AsanPardakht, etc.) will have its own implementation
    /// 
    /// Production Note: Implements IDisposable for proper resource management
    /// </summary>
    public interface IPosDeviceDriver : IDisposable
    {
        Task<ServiceResult> ConnectAsync(PosTerminal terminal);
        Task<ServiceResult<PosPaymentDriverResponse>> SendPaymentAsync(PosTerminal terminal, decimal amountIRR);
        Task<ServiceResult> DisconnectAsync(PosTerminal terminal);
    }

    /// <summary>
    /// Response from POS device driver
    /// </summary>
    public class PosPaymentDriverResponse
    {
        public bool Success { get; set; }
        public string RRN { get; set; }
        public string TraceNo { get; set; }
        public string CardLast4 { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }

    /// <summary>
    /// Stub implementation for POS device driver
    /// Used until real drivers are implemented
    /// 
    /// NOTE: This is a temporary implementation for development/testing
    /// In production, real drivers should be implemented based on provider SDKs
    /// </summary>
    public class StubPosDeviceDriver : IPosDeviceDriver
    {
        private readonly ILogger _logger;

        public StubPosDeviceDriver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceResult> ConnectAsync(PosTerminal terminal)
        {
            _logger.Information("🏥 POS Driver (Stub): Simulating connection - TerminalId: {TerminalId}, IP: {IpAddress}",
                terminal?.TerminalId, terminal?.IpAddress);
            
            // Simulate connection delay
            await Task.Delay(100);
            
            return ServiceResult.Successful();
        }

        public async Task<ServiceResult<PosPaymentDriverResponse>> SendPaymentAsync(PosTerminal terminal, decimal amountIRR)
        {
            _logger.Information("🏥 POS Driver (Stub): Simulating payment - TerminalId: {TerminalId}, AmountIRR: {AmountIRR}",
                terminal?.TerminalId, amountIRR);
            
            // Simulate payment processing delay
            await Task.Delay(500);
            
            // Generate simulated response
            var random = new Random();
            var response = new PosPaymentDriverResponse
            {
                Success = true,
                RRN = $"RRN{DateTime.Now:yyyyMMddHHmmss}{random.Next(1000, 9999)}",
                TraceNo = $"{DateTime.Now:HHmmss}{random.Next(100, 999)}",
                CardLast4 = $"{random.Next(1000, 9999)}",
                Message = "پرداخت با موفقیت انجام شد (شبیه‌سازی)"
            };

            _logger.Warning("⚠️ POS Driver (Stub): Using simulated response - RRN: {RRN}, TraceNo: {TraceNo}",
                response.RRN, response.TraceNo);

            return ServiceResult<PosPaymentDriverResponse>.Successful(response);
        }

        public async Task<ServiceResult> DisconnectAsync(PosTerminal terminal)
        {
            _logger.Information("🏥 POS Driver (Stub): Simulating disconnection - TerminalId: {TerminalId}",
                terminal?.TerminalId);
            
            // Simulate disconnection delay
            await Task.Delay(50);
            
            return ServiceResult.Successful();
        }

        public void Dispose()
        {
            // Stub implementation - no resources to dispose
        }
    }

    #endregion
}

