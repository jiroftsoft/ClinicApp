using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// DTOs برای پیش‌نویس پذیرش
    /// </summary>
    
    public class CreateDraftRequest
    {
        public int? ClinicId { get; set; }
        public int? DepartmentId { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
    }

    public class CreateDraftResponse
    {
        public int ReceptionId { get; set; }
        public string Status { get; set; }
    }

    public class AddItemRequest
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemRequest
    {
        public int ReceptionId { get; set; }
        public int ServiceId { get; set; }
    }

    public class SetInsurancesRequest
    {
        public int ReceptionId { get; set; }
        public int? BasePlanId { get; set; }
        public int? SupplementaryPlanId { get; set; }
    }

    public class ReceptionItemDto
    {
        public int ServiceId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal UnitPriceIRR { get; set; }
        public decimal TotalIRR { get; set; }
    }

    public class TotalsDto
    {
        public decimal Gross { get; set; }
        public decimal Base { get; set; }
        public decimal Supplementary { get; set; }
        public decimal Patient { get; set; }
    }

    public class ItemsAndTotalsDto
    {
        public List<ReceptionItemDto> Items { get; set; } = new List<ReceptionItemDto>();
        public TotalsDto Totals { get; set; } = new TotalsDto();
    }

    public class FinalizePosRequest
    {
        public int ReceptionId { get; set; }
        public decimal AmountIRR { get; set; }
        public string IdempotencyKey { get; set; }
        public PosPaymentDto Pos { get; set; }
    }

    public class FinalizeCashRequest
    {
        public int ReceptionId { get; set; }
        public decimal AmountIRR { get; set; }
        public string IdempotencyKey { get; set; }
        public CashPaymentDto Cash { get; set; }
    }

    public class PosPaymentDto
    {
        public decimal Amount { get; set; }
        public string RRN { get; set; }
        public string TraceNo { get; set; }
        public string TerminalId { get; set; }
        public string CardLast4 { get; set; }
    }

    public class CashPaymentDto
    {
        public decimal Amount { get; set; }
        public int? CashSessionId { get; set; }
    }

    public class FinalizeResponse
    {
        public string Status { get; set; }
        public ReceiptDto Receipt { get; set; }
    }

    public class ReceiptDto
    {
        public string No { get; set; }
        public string PrintedUrl { get; set; }
    }
}
