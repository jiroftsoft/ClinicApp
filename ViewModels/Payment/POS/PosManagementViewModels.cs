using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Payment.POS
{
    #region POS Terminal ViewModels

    /// <summary>
    /// ViewModel برای ایجاد ترمینال POS
    /// </summary>
    public class PosTerminalCreateViewModel
    {
        [Required(ErrorMessage = "نام ترمینال الزامی است")]
        [StringLength(100, ErrorMessage = "نام ترمینال نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام ترمینال")]
        public string Name { get; set; }

        [Required(ErrorMessage = "شماره سریال الزامی است")]
        [StringLength(50, ErrorMessage = "شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره سریال")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "نوع ارائه‌دهنده الزامی است")]
        [Display(Name = "نوع ارائه‌دهنده")]
        public PosProviderType ProviderType { get; set; }

        [Required(ErrorMessage = "پروتکل الزامی است")]
        [Display(Name = "پروتکل")]
        public PosProtocol Protocol { get; set; }

        [Required(ErrorMessage = "رشته اتصال الزامی است")]
        [StringLength(500, ErrorMessage = "رشته اتصال نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "رشته اتصال")]
        public string ConnectionString { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        public PosTerminalCreateViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
            ConnectionString = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای ویرایش ترمینال POS
    /// </summary>
    public class PosTerminalEditViewModel
    {
        [Required(ErrorMessage = "شناسه ترمینال الزامی است")]
        [Display(Name = "شناسه ترمینال")]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام ترمینال الزامی است")]
        [StringLength(100, ErrorMessage = "نام ترمینال نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام ترمینال")]
        public string Name { get; set; }

        [Required(ErrorMessage = "شماره سریال الزامی است")]
        [StringLength(50, ErrorMessage = "شماره سریال نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [Display(Name = "شماره سریال")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "نوع ارائه‌دهنده الزامی است")]
        [Display(Name = "نوع ارائه‌دهنده")]
        public PosProviderType ProviderType { get; set; }

        [Required(ErrorMessage = "پروتکل الزامی است")]
        [Display(Name = "پروتکل")]
        public PosProtocol Protocol { get; set; }

        [Required(ErrorMessage = "رشته اتصال الزامی است")]
        [StringLength(500, ErrorMessage = "رشته اتصال نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Display(Name = "رشته اتصال")]
        public string ConnectionString { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        public PosTerminalEditViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
            ConnectionString = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات ترمینال POS
    /// </summary>
    public class PosTerminalDetailsViewModel
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
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public string CreatedByUserName { get; set; }
        public string UpdatedByUserName { get; set; }

        // Statistics
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public DateTime? LastTransactionDate { get; set; }

        public PosTerminalDetailsViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
            ConnectionString = string.Empty;
            Description = string.Empty;
            CreatedByUserId = string.Empty;
            UpdatedByUserId = string.Empty;
            CreatedByUserName = string.Empty;
            UpdatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای لیست ترمینال‌های POS
    /// </summary>
    public class PosTerminalListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public PosProviderType ProviderType { get; set; }
        public PosProtocol Protocol { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }

        // Statistics
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }

        public PosTerminalListViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
            CreatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی ترمینال‌های POS
    /// </summary>
    public class PosTerminalSearchViewModel
    {
        [Display(Name = "نام ترمینال")]
        public string Name { get; set; }

        [Display(Name = "شماره سریال")]
        public string SerialNumber { get; set; }

        [Display(Name = "نوع ارائه‌دهنده")]
        public PosProviderType? ProviderType { get; set; }

        [Display(Name = "پروتکل")]
        public PosProtocol? Protocol { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool? IsDefault { get; set; }

        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public PosTerminalSearchViewModel()
        {
            Name = string.Empty;
            SerialNumber = string.Empty;
        }
    }

    #endregion

    #region Cash Session ViewModels

    /// <summary>
    /// ViewModel برای شروع جلسه نقدی
    /// </summary>
    public class CashSessionStartViewModel
    {
        [Required(ErrorMessage = "مبلغ اولیه نقدی الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ اولیه نقدی باید مثبت باشد")]
        [Display(Name = "مبلغ اولیه نقدی")]
        public decimal InitialCashAmount { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        public CashSessionStartViewModel()
        {
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای پایان جلسه نقدی
    /// </summary>
    public class CashSessionEndViewModel
    {
        [Required(ErrorMessage = "مبلغ نهایی نقدی الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ نهایی نقدی باید مثبت باشد")]
        [Display(Name = "مبلغ نهایی نقدی")]
        public decimal FinalCashAmount { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        public CashSessionEndViewModel()
        {
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات جلسه نقدی
    /// </summary>
    public class CashSessionDetailsViewModel
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public decimal InitialCashAmount { get; set; }
        public decimal FinalCashAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal Difference { get; set; }
        public CashSessionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Description { get; set; }
        public string EndedByUserId { get; set; }
        public string EndedByUserName { get; set; }

        // Statistics
        public int TotalTransactions { get; set; }
        public int CashTransactions { get; set; }
        public int PosTransactions { get; set; }
        public TimeSpan? Duration { get; set; }

        public CashSessionDetailsViewModel()
        {
            SessionNumber = string.Empty;
            UserId = string.Empty;
            UserName = string.Empty;
            Description = string.Empty;
            EndedByUserId = string.Empty;
            EndedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای لیست جلسات نقدی
    /// </summary>
    public class CashSessionListViewModel
    {
        public int Id { get; set; }
        public string SessionNumber { get; set; }
        public string UserName { get; set; }
        public decimal InitialCashAmount { get; set; }
        public decimal FinalCashAmount { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal Difference { get; set; }
        public CashSessionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }

        // Statistics
        public int TotalTransactions { get; set; }

        public CashSessionListViewModel()
        {
            SessionNumber = string.Empty;
            UserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی جلسات نقدی
    /// </summary>
    public class CashSessionSearchViewModel
    {
        [Display(Name = "شماره جلسه")]
        public string SessionNumber { get; set; }

        [Display(Name = "نام کاربر")]
        public string UserName { get; set; }

        [Display(Name = "وضعیت")]
        public CashSessionStatus? Status { get; set; }

        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "مبلغ از")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ باید مثبت باشد")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "مبلغ تا")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ باید مثبت باشد")]
        public decimal? MaxAmount { get; set; }

        public CashSessionSearchViewModel()
        {
            SessionNumber = string.Empty;
            UserName = string.Empty;
        }
    }

    #endregion

    #region POS Index ViewModels

    /// <summary>
    /// ViewModel برای صفحه اصلی POS
    /// </summary>
    public class PosIndexViewModel
    {
        public List<PosTerminalListViewModel> Terminals { get; set; }
        public List<CashSessionListViewModel> ActiveSessions { get; set; }
        public PosTerminalSearchViewModel TerminalSearchModel { get; set; }
        public CashSessionSearchViewModel SessionSearchModel { get; set; }
        public PosStatisticsViewModel Statistics { get; set; }
        public int TotalTerminalCount { get; set; }
        public int TotalSessionCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public PosIndexViewModel()
        {
            Terminals = new List<PosTerminalListViewModel>();
            ActiveSessions = new List<CashSessionListViewModel>();
            TerminalSearchModel = new PosTerminalSearchViewModel();
            SessionSearchModel = new CashSessionSearchViewModel();
            Statistics = new PosStatisticsViewModel();
        }
    }

    /// <summary>
    /// ViewModel برای آمار POS
    /// </summary>
    public class PosStatisticsViewModel
    {
        public int TotalTerminals { get; set; }
        public int ActiveTerminals { get; set; }
        public int InactiveTerminals { get; set; }
        public int DefaultTerminals { get; set; }
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalCashHandled { get; set; }
        public decimal TotalPosAmount { get; set; }
        public decimal TotalCashAmount { get; set; }
        public decimal AverageSessionAmount { get; set; }
        public decimal AverageSessionDuration { get; set; }
        public Dictionary<PosProviderType, int> TerminalsByProvider { get; set; }
        public Dictionary<PosProtocol, int> TerminalsByProtocol { get; set; }
        public Dictionary<CashSessionStatus, int> SessionsByStatus { get; set; }

        public PosStatisticsViewModel()
        {
            TerminalsByProvider = new Dictionary<PosProviderType, int>();
            TerminalsByProtocol = new Dictionary<PosProtocol, int>();
            SessionsByStatus = new Dictionary<CashSessionStatus, int>();
        }
    }

    #endregion

    #region POS Filter ViewModels

    /// <summary>
    /// ViewModel برای فیلترهای POS
    /// </summary>
    public class PosFilterViewModel
    {
        public List<PosProviderType> PosProviders { get; set; }
        public List<PosProtocol> PosProtocols { get; set; }
        public List<CashSessionStatus> CashSessionStatuses { get; set; }
        public List<string> Users { get; set; }

        public PosFilterViewModel()
        {
            PosProviders = new List<PosProviderType>();
            PosProtocols = new List<PosProtocol>();
            CashSessionStatuses = new List<CashSessionStatus>();
            Users = new List<string>();
        }
    }

    #endregion

    #region POS Lookup ViewModels

    /// <summary>
    /// ViewModel برای Lookup ارائه‌دهندگان POS
    /// </summary>
    public class PosProviderLookupViewModel
    {
        public PosProviderType Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public PosProviderLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای Lookup پروتکل‌های POS
    /// </summary>
    public class PosProtocolLookupViewModel
    {
        public PosProtocol Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public PosProtocolLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای Lookup وضعیت‌های جلسه نقدی
    /// </summary>
    public class CashSessionStatusLookupViewModel
    {
        public CashSessionStatus Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public CashSessionStatusLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    #endregion
}
