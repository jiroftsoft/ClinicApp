using System;
using System.Collections.Generic;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Payment.POS
{
    #region Request DTOs

    /// <summary>
    /// درخواست ایجاد ترمینال POS
    /// </summary>
    public class CreatePosTerminalRequest
    {
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public PosProviderType ProviderType { get; set; }
        public PosProtocol Protocol { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی ترمینال POS
    /// </summary>
    public class UpdatePosTerminalRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public PosProviderType ProviderType { get; set; }
        public PosProtocol Protocol { get; set; }
        public string ConnectionString { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string UpdatedByUserId { get; set; }
    }

    /// <summary>
    /// درخواست شروع جلسه نقدی
    /// </summary>
    public class StartCashSessionRequest
    {
        public string UserId { get; set; }
        public decimal InitialCashAmount { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// درخواست پایان جلسه نقدی
    /// </summary>
    public class EndCashSessionRequest
    {
        public decimal FinalCashAmount { get; set; }
        public string Description { get; set; }
        public string EndedByUserId { get; set; }
    }

    #endregion

    #region Statistics DTOs

    /// <summary>
    /// آمار ترمینال‌های POS
    /// </summary>
    public class PosTerminalStatistics
    {
        public int TotalTerminals { get; set; }
        public int ActiveTerminals { get; set; }
        public int InactiveTerminals { get; set; }
        public int DefaultTerminals { get; set; }
        public Dictionary<PosProviderType, int> TerminalsByProvider { get; set; }
        public Dictionary<PosProtocol, int> TerminalsByProtocol { get; set; }
        public decimal TotalTransactionAmount { get; set; }
        public int TotalTransactionCount { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    /// <summary>
    /// آمار روزانه POS
    /// </summary>
    public class DailyPosStatistics
    {
        public DateTime Date { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalCashBalance { get; set; }
        public Dictionary<int, decimal> AmountByTerminal { get; set; }
        public Dictionary<string, decimal> AmountByUser { get; set; }
    }

    /// <summary>
    /// موجودی نقدی
    /// </summary>
    public class CashBalance
    {
        public int SessionId { get; set; }
        public string UserId { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal Difference { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// موجودی نقدی روزانه
    /// </summary>
    public class DailyCashBalance
    {
        public DateTime Date { get; set; }
        public int TotalSessions { get; set; }
        public decimal TotalInitialAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalCurrentBalance { get; set; }
        public decimal TotalExpectedBalance { get; set; }
        public decimal TotalDifference { get; set; }
        public Dictionary<string, decimal> BalanceByUser { get; set; }
    }

    #endregion
}
