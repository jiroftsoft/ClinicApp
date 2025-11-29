using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Payment;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces.Payment.POS
{
    /// <summary>
    /// Service Interface for POS Device Communication
    /// 
    /// Responsibility: Direct communication with physical POS terminal devices
    /// Purpose: Send payment amounts to POS devices and receive transaction responses
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: Only device communication logic
    /// ✅ Separation of Concerns: Device communication separated from business logic
    /// ✅ High Testability: Interface allows for easy mocking
    /// ✅ Provider Agnostic: Supports multiple POS providers (SamanKish, AsanPardakht, etc.)
    /// </summary>
    public interface IPosDeviceService
    {
        /// <summary>
        /// Process payment by sending amount to POS terminal device
        /// </summary>
        /// <param name="terminal">POS terminal configuration</param>
        /// <param name="amountIRR">Payment amount in IRR (Rials) - Patient's share</param>
        /// <param name="receptionId">Reception ID for tracking</param>
        /// <returns>Payment response from POS device (RRN, TraceNo, CardLast4, etc.)</returns>
        Task<ServiceResult<PosPaymentResponse>> ProcessPaymentAsync(
            PosTerminal terminal, 
            decimal amountIRR, 
            int receptionId);
    }

    /// <summary>
    /// Response from POS device after payment processing
    /// </summary>
    public class PosPaymentResponse
    {
        /// <summary>
        /// Whether payment was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Retrieval Reference Number (RRN) from POS device
        /// </summary>
        public string RRN { get; set; }

        /// <summary>
        /// Trace Number from POS device
        /// </summary>
        public string TraceNo { get; set; }

        /// <summary>
        /// Terminal ID from POS device
        /// </summary>
        public string TerminalId { get; set; }

        /// <summary>
        /// Last 4 digits of card number
        /// </summary>
        public string CardLast4 { get; set; }

        /// <summary>
        /// Response message from POS device
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error code if payment failed
        /// </summary>
        public string ErrorCode { get; set; }
    }
}

