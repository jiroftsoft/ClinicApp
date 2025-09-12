using ClinicApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Payment.Gateway
{
    #region Payment Gateway ViewModels

    /// <summary>
    /// ViewModel برای ایجاد درگاه پرداخت
    /// </summary>
    public class PaymentGatewayCreateViewModel
    {
        [Required(ErrorMessage = "نام درگاه الزامی است")]
        [StringLength(100, ErrorMessage = "نام درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام درگاه")]
        public string Name { get; set; }

        [Required(ErrorMessage = "نوع درگاه الزامی است")]
        [Display(Name = "نوع درگاه")]
        public PaymentGatewayType GatewayType { get; set; }

        [Required(ErrorMessage = "MerchantId الزامی است")]
        [StringLength(100, ErrorMessage = "MerchantId نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "MerchantId")]
        public string MerchantId { get; set; }

        [Required(ErrorMessage = "ApiKey الزامی است")]
        [StringLength(200, ErrorMessage = "ApiKey نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "ApiKey")]
        public string ApiKey { get; set; }

        [Required(ErrorMessage = "ApiSecret الزامی است")]
        [StringLength(200, ErrorMessage = "ApiSecret نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "ApiSecret")]
        public string ApiSecret { get; set; }

        [Required(ErrorMessage = "آدرس Callback الزامی است")]
        [StringLength(500, ErrorMessage = "آدرس Callback نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Url(ErrorMessage = "آدرس Callback باید معتبر باشد")]
        [Display(Name = "آدرس Callback")]
        public string CallbackUrl { get; set; }

        [StringLength(500, ErrorMessage = "آدرس Webhook نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Url(ErrorMessage = "آدرس Webhook باید معتبر باشد")]
        [Display(Name = "آدرس Webhook")]
        public string WebhookUrl { get; set; }

        [Required(ErrorMessage = "درصد کارمزد الزامی است")]
        [Range(0, 100, ErrorMessage = "درصد کارمزد باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد کارمزد")]
        public decimal FeePercentage { get; set; }

        [Required(ErrorMessage = "کارمزد ثابت الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "کارمزد ثابت باید مثبت باشد")]
        [Display(Name = "کارمزد ثابت")]
        public decimal FixedFee { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        public PaymentGatewayCreateViewModel()
        {
            Name = string.Empty;
            MerchantId = string.Empty;
            ApiKey = string.Empty;
            ApiSecret = string.Empty;
            CallbackUrl = string.Empty;
            WebhookUrl = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای ویرایش درگاه پرداخت
    /// </summary>
    public class PaymentGatewayEditViewModel
    {
        [Required(ErrorMessage = "شناسه درگاه الزامی است")]
        [Display(Name = "شناسه درگاه")]
        public int Id { get; set; }

        [Required(ErrorMessage = "نام درگاه الزامی است")]
        [StringLength(100, ErrorMessage = "نام درگاه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "نام درگاه")]
        public string Name { get; set; }

        [Required(ErrorMessage = "نوع درگاه الزامی است")]
        [Display(Name = "نوع درگاه")]
        public PaymentGatewayType GatewayType { get; set; }

        [Required(ErrorMessage = "MerchantId الزامی است")]
        [StringLength(100, ErrorMessage = "MerchantId نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        [Display(Name = "MerchantId")]
        public string MerchantId { get; set; }

        [Required(ErrorMessage = "ApiKey الزامی است")]
        [StringLength(200, ErrorMessage = "ApiKey نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "ApiKey")]
        public string ApiKey { get; set; }

        [Required(ErrorMessage = "ApiSecret الزامی است")]
        [StringLength(200, ErrorMessage = "ApiSecret نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        [Display(Name = "ApiSecret")]
        public string ApiSecret { get; set; }

        [Required(ErrorMessage = "آدرس Callback الزامی است")]
        [StringLength(500, ErrorMessage = "آدرس Callback نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Url(ErrorMessage = "آدرس Callback باید معتبر باشد")]
        [Display(Name = "آدرس Callback")]
        public string CallbackUrl { get; set; }

        [StringLength(500, ErrorMessage = "آدرس Webhook نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        [Url(ErrorMessage = "آدرس Webhook باید معتبر باشد")]
        [Display(Name = "آدرس Webhook")]
        public string WebhookUrl { get; set; }

        [Required(ErrorMessage = "درصد کارمزد الزامی است")]
        [Range(0, 100, ErrorMessage = "درصد کارمزد باید بین 0 تا 100 باشد")]
        [Display(Name = "درصد کارمزد")]
        public decimal FeePercentage { get; set; }

        [Required(ErrorMessage = "کارمزد ثابت الزامی است")]
        [Range(0, double.MaxValue, ErrorMessage = "کارمزد ثابت باید مثبت باشد")]
        [Display(Name = "کارمزد ثابت")]
        public decimal FixedFee { get; set; }

        [Display(Name = "فعال")]
        public bool IsActive { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool IsDefault { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        public PaymentGatewayEditViewModel()
        {
            Name = string.Empty;
            MerchantId = string.Empty;
            ApiKey = string.Empty;
            ApiSecret = string.Empty;
            CallbackUrl = string.Empty;
            WebhookUrl = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات درگاه پرداخت
    /// </summary>
    public class PaymentGatewayDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public string MerchantId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string CallbackUrl { get; set; }
        public string WebhookUrl { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public string Description { get; set; }
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
        public decimal AverageResponseTime { get; set; }
        public DateTime? LastTransactionDate { get; set; }

        public PaymentGatewayDetailsViewModel()
        {
            Name = string.Empty;
            MerchantId = string.Empty;
            ApiKey = string.Empty;
            ApiSecret = string.Empty;
            CallbackUrl = string.Empty;
            WebhookUrl = string.Empty;
            Description = string.Empty;
            CreatedByUserId = string.Empty;
            UpdatedByUserId = string.Empty;
            CreatedByUserName = string.Empty;
            UpdatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای لیست درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public string MerchantId { get; set; }
        public decimal FeePercentage { get; set; }
        public decimal FixedFee { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }

        // Statistics
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTime { get; set; }

        public PaymentGatewayListViewModel()
        {
            Name = string.Empty;
            MerchantId = string.Empty;
            CreatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewaySearchViewModel
    {
        [Display(Name = "نام درگاه")]
        public string Name { get; set; }

        [Display(Name = "نوع درگاه")]
        public PaymentGatewayType? GatewayType { get; set; }

        [Display(Name = "MerchantId")]
        public string MerchantId { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsActive { get; set; }

        [Display(Name = "پیش‌فرض")]
        public bool? IsDefault { get; set; }

        [Display(Name = "ایجادکننده")]
        public string CreatedByUserId { get; set; }

        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public PaymentGatewaySearchViewModel()
        {
            Name = string.Empty;
            MerchantId = string.Empty;
            CreatedByUserId = string.Empty;
        }
    }

    #endregion

    #region Online Payment ViewModels

    /// <summary>
    /// ViewModel برای ایجاد پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentCreateViewModel
    {
        [Required(ErrorMessage = "شناسه پذیرش الزامی است")]
        [Display(Name = "شناسه پذیرش")]
        public int ReceptionId { get; set; }

        [Display(Name = "شناسه نوبت")]
        public int? AppointmentId { get; set; }

        [Required(ErrorMessage = "شناسه بیمار الزامی است")]
        [Display(Name = "شناسه بیمار")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "نوع پرداخت الزامی است")]
        [Display(Name = "نوع پرداخت")]
        public OnlinePaymentType PaymentType { get; set; }

        [Required(ErrorMessage = "مبلغ پرداخت الزامی است")]
        [Range(0.01, double.MaxValue, ErrorMessage = "مبلغ پرداخت باید بیشتر از صفر باشد")]
        [Display(Name = "مبلغ پرداخت")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "درگاه پرداخت الزامی است")]
        [Display(Name = "درگاه پرداخت")]
        public int PaymentGatewayId { get; set; }

        [Display(Name = "توضیحات")]
        [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Description { get; set; }

        [Display(Name = "آدرس IP کاربر")]
        public string UserIpAddress { get; set; }

        [Display(Name = "User Agent")]
        public string UserAgent { get; set; }

        public OnlinePaymentCreateViewModel()
        {
            Description = string.Empty;
            UserIpAddress = string.Empty;
            UserAgent = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای نمایش جزئیات پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentDetailsViewModel
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public int? AppointmentId { get; set; }
        public int PatientId { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public int PaymentGatewayId { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public string GatewayReferenceCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Description { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public decimal? GatewayFee { get; set; }
        public decimal? NetAmount { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string PaymentGatewayName { get; set; }
        public string CreatedByUserName { get; set; }

        public OnlinePaymentDetailsViewModel()
        {
            PaymentToken = string.Empty;
            GatewayTransactionId = string.Empty;
            GatewayReferenceCode = string.Empty;
            ErrorCode = string.Empty;
            ErrorMessage = string.Empty;
            Description = string.Empty;
            UserIpAddress = string.Empty;
            UserAgent = string.Empty;
            CreatedByUserId = string.Empty;
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            PaymentGatewayName = string.Empty;
            CreatedByUserName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای لیست پرداخت‌های آنلاین
    /// </summary>
    public class OnlinePaymentListViewModel
    {
        public int Id { get; set; }
        public int ReceptionId { get; set; }
        public int PatientId { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string PaymentToken { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string GatewayTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public string ReceptionNumber { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string PaymentGatewayName { get; set; }

        public OnlinePaymentListViewModel()
        {
            PaymentToken = string.Empty;
            GatewayTransactionId = string.Empty;
            ReceptionNumber = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
            PaymentGatewayName = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای جستجوی پرداخت‌های آنلاین
    /// </summary>
    public class OnlinePaymentSearchViewModel
    {
        [Display(Name = "شناسه پذیرش")]
        public int? ReceptionId { get; set; }

        [Display(Name = "شناسه بیمار")]
        public int? PatientId { get; set; }

        [Display(Name = "نوع پرداخت")]
        public OnlinePaymentType? PaymentType { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        public OnlinePaymentStatus? Status { get; set; }

        [Display(Name = "درگاه پرداخت")]
        public PaymentGatewayType? PaymentGatewayType { get; set; }

        [Display(Name = "مبلغ از")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ باید مثبت باشد")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "مبلغ تا")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ باید مثبت باشد")]
        public decimal? MaxAmount { get; set; }

        [Display(Name = "تاریخ از")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "تاریخ تا")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "توکن پرداخت")]
        public string PaymentToken { get; set; }

        [Display(Name = "شناسه تراکنش درگاه")]
        public string GatewayTransactionId { get; set; }

        [Display(Name = "کد مرجع")]
        public string ReferenceCode { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        public OnlinePaymentSearchViewModel()
        {
            PaymentToken = string.Empty;
            GatewayTransactionId = string.Empty;
            ReferenceCode = string.Empty;
            PatientName = string.Empty;
            DoctorName = string.Empty;
        }
    }

    #endregion

    #region Gateway Index ViewModels

    /// <summary>
    /// ViewModel برای صفحه اصلی درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayIndexViewModel
    {
        public List<PaymentGatewayListViewModel> Gateways { get; set; }
        public List<OnlinePaymentListViewModel> RecentPayments { get; set; }
        public PaymentGatewaySearchViewModel SearchModel { get; set; }
        public OnlinePaymentSearchViewModel PaymentSearchModel { get; set; }
        public PaymentGatewayStatisticsViewModel Statistics { get; set; }
        public int TotalGatewayCount { get; set; }
        public int TotalPaymentCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public PaymentGatewayIndexViewModel()
        {
            Gateways = new List<PaymentGatewayListViewModel>();
            RecentPayments = new List<OnlinePaymentListViewModel>();
            SearchModel = new PaymentGatewaySearchViewModel();
            PaymentSearchModel = new OnlinePaymentSearchViewModel();
            Statistics = new PaymentGatewayStatisticsViewModel();
        }
    }

    /// <summary>
    /// ViewModel برای آمار درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayStatisticsViewModel
    {
        public int TotalGateways { get; set; }
        public int ActiveGateways { get; set; }
        public int InactiveGateways { get; set; }
        public int DefaultGateways { get; set; }
        public int TotalOnlinePayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal FailedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AverageResponseTime { get; set; }
        public Dictionary<PaymentGatewayType, int> GatewaysByType { get; set; }
        public Dictionary<OnlinePaymentType, int> PaymentsByType { get; set; }
        public Dictionary<OnlinePaymentStatus, int> PaymentsByStatus { get; set; }

        public PaymentGatewayStatisticsViewModel()
        {
            GatewaysByType = new Dictionary<PaymentGatewayType, int>();
            PaymentsByType = new Dictionary<OnlinePaymentType, int>();
            PaymentsByStatus = new Dictionary<OnlinePaymentStatus, int>();
        }
    }

    #endregion

    #region Gateway Filter ViewModels

    /// <summary>
    /// ViewModel برای فیلترهای درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayFilterViewModel
    {
        public List<PaymentGatewayType> PaymentGateways { get; set; }
        public List<OnlinePaymentType> OnlinePaymentTypes { get; set; }
        public List<OnlinePaymentStatus> OnlinePaymentStatuses { get; set; }

        public PaymentGatewayFilterViewModel()
        {
            PaymentGateways = new List<PaymentGatewayType>();
            OnlinePaymentTypes = new List<OnlinePaymentType>();
            OnlinePaymentStatuses = new List<OnlinePaymentStatus>();
        }
    }

    #endregion

    #region Gateway Lookup ViewModels

    /// <summary>
    /// ViewModel برای Lookup انواع درگاه‌های پرداخت
    /// </summary>
    public class PaymentGatewayTypeLookupViewModel
    {
        public PaymentGatewayType Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public PaymentGatewayTypeLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای Lookup انواع پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentTypeLookupViewModel
    {
        public OnlinePaymentType Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public OnlinePaymentTypeLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    /// <summary>
    /// ViewModel برای Lookup وضعیت‌های پرداخت آنلاین
    /// </summary>
    public class OnlinePaymentStatusLookupViewModel
    {
        public OnlinePaymentStatus Value { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public OnlinePaymentStatusLookupViewModel()
        {
            DisplayName = string.Empty;
            Description = string.Empty;
        }
    }

    #endregion
}
