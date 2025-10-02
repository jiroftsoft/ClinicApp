using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Enums;
using ClinicApp.Helpers;
using ClinicApp.Models.Statistics;
using ClinicApp.Interfaces;

namespace ClinicApp.ViewModels.Payment
{
    /// <summary>
    /// ViewModel برای ایجاد تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionCreateViewModel
    {
        [Required(ErrorMessage = "شناسه پذیرش الزامی است")]
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Required(ErrorMessage = "مبلغ پرداخت الزامی است")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ پرداخت باید بیشتر از صفر باشد")]
        [Display(Name = "مبلغ پرداخت")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "روش پرداخت الزامی است")]
        [Display(Name = "روش پرداخت")]
        public PaymentMethod Method { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "یادداشت‌ها")]
        [StringLength(500, ErrorMessage = "یادداشت‌ها نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Notes { get; set; }

        [Display(Name = "شماره مرجع")]
        [StringLength(100, ErrorMessage = "شماره مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string ReferenceNumber { get; set; }

        [Display(Name = "شناسه ترمینال POS")]
        public int? PosTerminalId { get; set; }

        [Display(Name = "شناسه درگاه پرداخت")]
        public int? PaymentGatewayId { get; set; }

        [Display(Name = "شناسه جلسه نقدی")]
        public int? CashSessionId { get; set; }

        [Display(Name = "شناسه تراکنش")]
        [StringLength(100, ErrorMessage = "شناسه تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string TransactionId { get; set; }

        [Display(Name = "کد مرجع")]
        [StringLength(100, ErrorMessage = "کد مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string ReferenceCode { get; set; }

        [Display(Name = "شماره رسید")]
        [StringLength(100, ErrorMessage = "شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string ReceiptNo { get; set; }

        public PaymentTransactionCreateViewModel()
        {
            Description = string.Empty;
            Notes = string.Empty;
            ReferenceNumber = string.Empty;
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
        }

        /// <summary>
        /// ✅ (Factory Method) یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public PaymentTransaction ToEntity()
        {
            return new PaymentTransaction
            {
                ReceptionId = this.ReceptionId,
                Amount = this.Amount,
                Method = this.Method,
                Description = this.Description,
                PosTerminalId = this.PosTerminalId ?? 0,
                PaymentGatewayId = this.PaymentGatewayId ?? 0,
                CashSessionId = this.CashSessionId ?? 0,
                TransactionId = this.TransactionId,
                ReferenceCode = this.ReferenceCode,
                ReceiptNo = this.ReceiptNo,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی Entity می‌سازد.
        /// </summary>
        public static PaymentTransactionCreateViewModel FromEntity(PaymentTransaction entity)
        {
            if (entity == null) return null;

            return new PaymentTransactionCreateViewModel
            {
                ReceptionId = entity.ReceptionId,
                Amount = entity.Amount,
                Method = entity.Method,
                Description = entity.Description,
                PosTerminalId = entity.PosTerminalId,
                PaymentGatewayId = entity.PaymentGatewayId,
                CashSessionId = entity.CashSessionId,
                TransactionId = entity.TransactionId,
                ReferenceCode = entity.ReferenceCode,
                ReceiptNo = entity.ReceiptNo
            };
        }
    }

    /// <summary>
    /// ViewModel برای ویرایش تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionEditViewModel
    {
        [Required(ErrorMessage = "شناسه تراکنش الزامی است")]
        [Display(Name = "شناسه تراکنش")]
        public int PaymentTransactionId { get; set; }


        [Required(ErrorMessage = "شناسه پذیرش الزامی است")]
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Required(ErrorMessage = "مبلغ پرداخت الزامی است")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ پرداخت باید بیشتر از صفر باشد")]
        [Display(Name = "مبلغ پرداخت")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "روش پرداخت الزامی است")]
        [Display(Name = "روش پرداخت")]
        public ClinicApp.Models.Enums.PaymentMethod Method { get; set; }

        [Required(ErrorMessage = "وضعیت پرداخت الزامی است")]
        [Display(Name = "وضعیت پرداخت")]
        public ClinicApp.Models.Enums.PaymentStatus Status { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "شناسه ترمینال POS")]
        public int? PosTerminalId { get; set; }

        [Display(Name = "شناسه درگاه پرداخت")]
        public int? PaymentGatewayId { get; set; }

        [Display(Name = "شناسه جلسه نقدی")]
        public int? CashSessionId { get; set; }

        [Display(Name = "شناسه تراکنش")]
        [StringLength(100, ErrorMessage = "شناسه تراکنش نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string TransactionId { get; set; }

        [Display(Name = "کد مرجع")]
        [StringLength(100, ErrorMessage = "کد مرجع نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string ReferenceCode { get; set; }

        [Display(Name = "شماره رسید")]
        [StringLength(100, ErrorMessage = "شماره رسید نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string ReceiptNo { get; set; }

        public PaymentTransactionEditViewModel()
        {
            Description = string.Empty;
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
        }

        /// <summary>
        /// ✅ (Factory Method) یک Entity جدید از روی ViewModel می‌سازد.
        /// </summary>
        public PaymentTransaction ToEntity()
        {
            return new PaymentTransaction
            {
                PaymentTransactionId = this.PaymentTransactionId,
                ReceptionId = this.ReceptionId,
                Amount = this.Amount,
                Method = this.Method,
                Status = this.Status,
                Description = this.Description,
                PosTerminalId = this.PosTerminalId ?? 0,
                PaymentGatewayId = this.PaymentGatewayId ?? 0,
                CashSessionId = this.CashSessionId ?? 0,
                TransactionId = this.TransactionId,
                ReferenceCode = this.ReferenceCode,
                ReceiptNo = this.ReceiptNo,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی Entity می‌سازد.
        /// </summary>
        public static PaymentTransactionEditViewModel FromEntity(PaymentTransaction entity)
        {
            if (entity == null) return null;

            return new PaymentTransactionEditViewModel
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
                ReceiptNo = entity.ReceiptNo
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات تراکنش پرداخت
    /// </summary>
    public class PaymentTransactionDetailsViewModel
    {
        public int PaymentTransactionId { get; set; }
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public ClinicApp.Models.Enums.PaymentMethod Method { get; set; }
        public ClinicApp.Models.Enums.PaymentStatus Status { get; set; }
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

        public PaymentTransactionDetailsViewModel()
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
        public static PaymentTransactionDetailsViewModel FromEntity(PaymentTransaction entity)
        {
            if (entity == null) return null;

            return new PaymentTransactionDetailsViewModel
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
                PosTerminalName = entity.PosTerminal?.Name ?? string.Empty,
                PaymentGatewayName = entity.PaymentGateway?.Name ?? string.Empty,
                CashSessionNumber = entity.CashSession?.SessionNumber ?? string.Empty,
                CreatedByUserName = entity.CreatedByUser?.UserName ?? string.Empty,
                UpdatedByUserName = entity.UpdatedByUser?.UserName ?? string.Empty
            };
        }
    }

    /// <summary>
    /// ViewModel برای نمایش تراکنش پرداخت (ساده)
    /// </summary>
    public class PaymentTransactionViewModel
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }

        public PaymentTransactionViewModel()
        {
            PaymentMethod = string.Empty;
            Status = string.Empty;
            ReferenceNumber = string.Empty;
            Notes = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای لیست تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionListViewModel
    {
        public int PaymentTransactionId { get; set; }
        public int Id { get; set; } // برای سازگاری
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public ClinicApp.Models.Enums.PaymentMethod Method { get; set; }
        public ClinicApp.Models.Enums.PaymentMethod PaymentMethod { get; set; } // برای سازگاری
        public ClinicApp.Models.Enums.PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public string ReferenceCode { get; set; }
        public string ReceiptNo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TransactionDate { get; set; } // برای سازگاری
        public string Description { get; set; }
        public string CreatedByUserName { get; set; }

        // Navigation Properties
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }

        public PaymentTransactionListViewModel()
        {
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
            CreatedByUserName = string.Empty;
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            Description = string.Empty;
        }

        /// <summary>
        /// ✅ (Factory Method) یک ViewModel جدید از روی Entity می‌سازد.
        /// </summary>
        public static PaymentTransactionListViewModel FromEntity(PaymentTransaction entity)
        {
            if (entity == null) return null;

            return new PaymentTransactionListViewModel
            {
                PaymentTransactionId = entity.PaymentTransactionId,
                ReceptionId = entity.ReceptionId,
                Amount = entity.Amount,
                Method = entity.Method,
                Status = entity.Status,
                TransactionId = entity.TransactionId,
                ReferenceCode = entity.ReferenceCode,
                ReceiptNo = entity.ReceiptNo,
                CreatedAt = entity.CreatedAt,
                CreatedByUserName = entity.CreatedByUser?.UserName ?? string.Empty,
                ReceptionNumber = entity.Reception?.ReceptionNumber ?? string.Empty,
                PatientName = entity.Reception?.Patient?.FirstName + " " + entity.Reception?.Patient?.LastName ?? string.Empty,
                DoctorName = entity.Reception?.Doctor?.FirstName + " " + entity.Reception?.Doctor?.LastName ?? string.Empty
            };
        }

        /// <summary>
        /// ✅ (Factory Method) لیست ViewModels از روی لیست Entities می‌سازد.
        /// </summary>
        public static List<PaymentTransactionListViewModel> FromEntities(IEnumerable<PaymentTransaction> entities)
        {
            if (entities == null) return new List<PaymentTransactionListViewModel>();

            return entities.Select(FromEntity).ToList();
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی تراکنش‌های پرداخت
    /// </summary>
    public class PaymentTransactionSearchViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int? ReceptionId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "مبلغ از")]
        public decimal? AmountFrom { get; set; }

        [Display(Name = "مبلغ تا")]
        public decimal? AmountTo { get; set; }

        [Display(Name = "حداقل مبلغ")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "حداکثر مبلغ")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "روش پرداخت")]
        public ClinicApp.Models.Enums.PaymentMethod? Method { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public ClinicApp.Models.Enums.PaymentStatus? Status { get; set; }

        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "شناسه ترمینال POS")]
        public int? PosTerminalId { get; set; }

        [Display(Name = "شناسه درگاه پرداخت")]
        public int? PaymentGatewayId { get; set; }

        [Display(Name = "شناسه جلسه نقدی")]
        public int? CashSessionId { get; set; }

        [Display(Name = "شماره تراکنش")]
        public string TransactionId { get; set; }

        [Display(Name = "کد مرجع")]
        public string ReferenceCode { get; set; }

        [Display(Name = "شماره رسید")]
        public string ReceiptNo { get; set; }

        public PaymentTransactionSearchViewModel()
        {
            TransactionId = string.Empty;
            ReferenceCode = string.Empty;
            ReceiptNo = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای پرداخت‌های پذیرش
    /// </summary>
    public class ReceptionPaymentViewModel
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }

        public ReceptionPaymentViewModel()
        {
            PaymentMethod = string.Empty;
            Status = string.Empty;
            ReferenceNumber = string.Empty;
            Notes = string.Empty;
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
    /// ViewModel برای نمایش روش پرداخت
    /// </summary>
    public class PaymentMethodLookupViewModel
    {
        public ClinicApp.Models.Enums.PaymentMethod Value { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        public PaymentMethodLookupViewModel()
        {
            Text = string.Empty;
            Description = string.Empty;
            DisplayName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای صفحه اصلی پرداخت‌ها
    /// </summary>
    public class PaymentIndexViewModel
    {
        public List<PaymentTransactionListViewModel> Transactions { get; set; } = new List<PaymentTransactionListViewModel>();
        public PaymentTransactionSearchViewModel SearchModel { get; set; } = new PaymentTransactionSearchViewModel();
        public PagedResult<PaymentTransactionListViewModel> PagedResult { get; set; }
        public PaymentStatistics Statistics { get; set; }
        public List<PaymentMethodLookupViewModel> PaymentMethods { get; set; } = new List<PaymentMethodLookupViewModel>();
        public List<CashSessionLookupViewModel> CashSessions { get; set; } = new List<CashSessionLookupViewModel>();
        
        // Properties for pagination
        public int TotalCount => PagedResult?.TotalItems ?? 0;
        public int PageNumber => PagedResult?.PageNumber ?? 1;
        public int PageSize => PagedResult?.PageSize ?? 20;
        public int TotalPages => PagedResult?.TotalPages ?? 0;
    }
}
