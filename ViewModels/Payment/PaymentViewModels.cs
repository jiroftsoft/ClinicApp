using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClinicApp.ViewModels.Payment
{
    /// <summary>
    /// ViewModel عمومی برای تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionViewModel
    {
        public int PaymentTransactionId { get; set; }
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string Description { get; set; }
        public int? PosTerminalId { get; set; }
        public int? PaymentGatewayId { get; set; }
        public int? CashSessionId { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string PosTerminalName { get; set; }
        public string PaymentGatewayName { get; set; }
        public string CashSessionNumber { get; set; }
        public string CreatedByUserName { get; set; }
        public string UpdatedByUserName { get; set; }

        public PaymentTransactionViewModel()
        {
            Description = string.Empty;
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
            CreatedByUserId = string.Empty;
            UpdatedByUserId = string.Empty;
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            PosTerminalName = string.Empty;
            PaymentGatewayName = string.Empty;
            CashSessionNumber = string.Empty;
            CreatedByUserName = string.Empty;
            UpdatedByUserName = string.Empty;
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی Entity می‌سازد.
        /// </summary>
        public static PaymentTransactionViewModel FromEntity(PaymentTransaction entity)
        {
            if (entity == null) return null;

            return new PaymentTransactionViewModel
            {
                PaymentTransactionId = entity.PaymentTransactionId,
                ReceptionId = entity.ReceptionId,
                Amount = entity.Amount,
                Method = entity.Method,
                Status = entity.Status,
                Description = entity.Description,
                PosTerminalId = entity.PosTerminalId,
                PaymentGatewayId = entity.PaymentGatewayId,
                CashSessionId = entity.CashSessionId,
                TransactionId = entity.TransactionId,
                ReferenceCode = entity.ReferenceCode,
                ReceiptNo = entity.ReceiptNo,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedAt = entity.CreatedAt,
                UpdatedByUserId = entity.UpdatedByUserId,
                UpdatedAt = entity.UpdatedAt,
                // Navigation properties will be set by the service layer
                ReceptionNumber = entity.Reception?.ReceptionNumber ?? string.Empty,
                PatientName = entity.Reception?.Patient?.FirstName + " " + entity.Reception?.Patient?.LastName ?? string.Empty,
                DoctorName = entity.Reception?.Doctor?.FirstName + " " + entity.Reception?.Doctor?.LastName ?? string.Empty,
                PosTerminalName = entity.PosTerminal?.Title ?? string.Empty,
                PaymentGatewayName = entity.PaymentGateway?.Name ?? string.Empty,
                CashSessionNumber = entity.CashSession?.CashSessionId.ToString() ?? string.Empty,
                CreatedByUserName = entity.CreatedByUser?.UserName ?? string.Empty,
                UpdatedByUserName = entity.UpdatedByUser?.UserName ?? string.Empty
            };
        }

        /// <summary>
        /// ✅ (Factory Method) لیست ViewModels از روی لیست Entities می‌سازد.
        /// </summary>
        public static List<PaymentTransactionViewModel> FromEntities(IEnumerable<PaymentTransaction> entities)
        {
            if (entities == null) return new List<PaymentTransactionViewModel>();

            return entities.Select(FromEntity).ToList();
        }
    }

    /// <summary>
    /// ViewModel برای پرداخت‌های پذیرش
    /// </summary>
    public class ReceptionPaymentViewModel
    {
        public int ReceptionId { get; set; }
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<PaymentTransactionViewModel> Transactions { get; set; }

        public ReceptionPaymentViewModel()
        {
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            Transactions = new List<PaymentTransactionViewModel>();
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی Entity می‌سازد.
        /// </summary>
        public static ReceptionPaymentViewModel FromEntity(ClinicApp.Models.Entities.Reception.Reception entity)
        {
            if (entity == null) return null;

            var transactions = entity.Transactions?.Select(PaymentTransactionViewModel.FromEntity).ToList() ?? new List<PaymentTransactionViewModel>();
            var paidAmount = transactions.Where(t => t.Status == PaymentStatus.Success).Sum(t => t.Amount);

            return new ReceptionPaymentViewModel
            {
                ReceptionId = entity.ReceptionId,
                ReceptionNumber = entity.ReceptionNumber,
                PatientName = entity.Patient?.FirstName + " " + entity.Patient?.LastName ?? string.Empty,
                DoctorName = entity.Doctor?.FirstName + " " + entity.Doctor?.LastName ?? string.Empty,
                TotalAmount = entity.TotalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = entity.TotalAmount - paidAmount,
                Transactions = transactions
            };
        }
    }

    /// <summary>
    /// ViewModel برای رسید پرداخت
    /// </summary>
    public class PaymentReceiptViewModel
    {
        public int PaymentTransactionId { get; set; }
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public decimal Amount { get; set; }
        public ClinicApp.Models.Enums.PaymentMethod Method { get; set; }
        public ClinicApp.Models.Enums.PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public string ReceiptNumber => ReceiptNo;
        public string TransactionNumber => TransactionId;
        public string ClinicName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }

        public PaymentReceiptViewModel()
        {
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
            ClinicName = string.Empty;
            CreatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای آمار پرداخت‌ها
    /// </summary>
    public class PaymentStatisticsViewModel
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal PosAmount { get; set; }
        public decimal OnlineAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public int PendingTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<ClinicApp.Models.Enums.PaymentMethod, int> TransactionsByMethod { get; set; }
        public Dictionary<ClinicApp.Models.Enums.PaymentStatus, int> TransactionsByStatus { get; set; }

        public PaymentStatisticsViewModel()
        {
            TransactionsByMethod = new Dictionary<ClinicApp.Models.Enums.PaymentMethod, int>();
            TransactionsByStatus = new Dictionary<ClinicApp.Models.Enums.PaymentStatus, int>();
        }
    }

    /// <summary>
    /// ViewModel برای فیلتر پرداخت‌ها
    /// </summary>
    public class PaymentFilterViewModel
    {
        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "روش پرداخت")]
        public ClinicApp.Models.Enums.PaymentMethod? Method { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public ClinicApp.Models.Enums.PaymentStatus? Status { get; set; }

        [Display(Name = "مبلغ از")]
        public decimal? AmountFrom { get; set; }

        [Display(Name = "مبلغ تا")]
        public decimal? AmountTo { get; set; }

        public List<PaymentMethodLookupViewModel> PaymentMethods { get; set; }
        public List<PaymentStatusLookupViewModel> PaymentStatuses { get; set; }
        public List<PaymentGatewayLookupViewModel> PaymentGateways { get; set; }
        public List<PosProviderType> PosProviders { get; set; }
        public List<PosProtocol> PosProtocols { get; set; }

        public PaymentFilterViewModel()
        {
            PaymentMethods = new List<PaymentMethodLookupViewModel>();
            PaymentStatuses = new List<PaymentStatusLookupViewModel>();
            PaymentGateways = new List<PaymentGatewayLookupViewModel>();
            PosProviders = new List<PosProviderType>();
            PosProtocols = new List<PosProtocol>();
        }
    }

    /// <summary>
    /// ViewModel برای نمایش روش پرداخت
    /// </summary>
    public class PaymentMethodLookupViewModel
    {
        public ClinicApp.Models.Enums.PaymentMethod PaymentMethod { get; set; }
        public ClinicApp.Models.Enums.PaymentMethod Value => PaymentMethod;
        public string Name { get; set; }
        public string Text => Name;
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
        public bool RequiresConfirmation { get; set; }

        public PaymentMethodLookupViewModel()
        {
            Name = string.Empty;
            Description = string.Empty;
            DisplayName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش وضعیت پرداخت
    /// </summary>
    public class PaymentStatusLookupViewModel
    {
        public ClinicApp.Models.Enums.PaymentStatus Value { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        public PaymentStatusLookupViewModel()
        {
            Text = string.Empty;
            Description = string.Empty;
            DisplayName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش درگاه پرداخت
    /// </summary>
    public class PaymentGatewayLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ClinicApp.Models.Enums.PaymentGatewayType GatewayType { get; set; }

        public PaymentGatewayLookupViewModel()
        {
            Name = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش ترمینال POS
    /// </summary>
    public class PosTerminalLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ClinicApp.Models.Enums.PosProviderType ProviderType { get; set; }
        public string SerialNumber { get; set; }

        public PosTerminalLookupViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جلسه نقدی
    /// </summary>
    public class CashSessionLookupViewModel
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; }
        public ClinicApp.Models.Enums.CashSessionStatus Status { get; set; }
        public string UserName { get; set; }

        public CashSessionLookupViewModel()
        {
            SessionNumber = string.Empty;
            UserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای صفحه اصلی پرداخت‌ها
    /// </summary>
    public class PaymentIndexViewModel
    {
        public List<PaymentTransactionListViewModel> Transactions { get; set; }
        public PaymentStatisticsViewModel Statistics { get; set; }
        public PaymentFilterViewModel FilterModel { get; set; }
        public PaymentTransactionListViewModel.PaymentTransactionSearchViewModel SearchModel { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public PaymentIndexViewModel()
        {
            Transactions = new List<PaymentTransactionListViewModel>();
            Statistics = new PaymentStatisticsViewModel();
            FilterModel = new PaymentFilterViewModel();
            SearchModel = new PaymentTransactionListViewModel.PaymentTransactionSearchViewModel();
        }
    }
}
