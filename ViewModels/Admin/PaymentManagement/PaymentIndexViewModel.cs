using System;
using System.Collections.Generic;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Shared;

namespace ClinicApp.ViewModels.Admin.PaymentManagement
{
    /// <summary>
    /// ViewModel برای صفحه Index مدیریت پرداخت‌ها
    /// طراحی شده طبق اصول Strongly-Typed Development
    /// </summary>
    public class PaymentIndexViewModel
    {
        /// <summary>
        /// لیست پرداخت‌ها
        /// </summary>
        public List<PaymentListItemViewModel> Payments { get; set; }

        /// <summary>
        /// فیلتر جستجو
        /// </summary>
        public PaymentSearchFilter Filter { get; set; }

        /// <summary>
        /// اطلاعات Pagination
        /// </summary>
        public PaginationViewModel PagingInfo { get; set; }

        /// <summary>
        /// آمار کلی
        /// </summary>
        public PaymentStatisticsViewModel Statistics { get; set; }

        public PaymentIndexViewModel()
        {
            Payments = new List<PaymentListItemViewModel>();
            Filter = new PaymentSearchFilter();
            PagingInfo = new PaginationViewModel();
            Statistics = new PaymentStatisticsViewModel();
        }
    }

    /// <summary>
    /// ViewModel برای آیتم لیست پرداخت
    /// </summary>
    public class PaymentListItemViewModel
    {
        public int OnlinePaymentId { get; set; }
        public int? AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientNationalCode { get; set; }
        public int? DoctorId { get; set; }
        public string DoctorName { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplay { get; set; }
        public string GatewayName { get; set; }
        public string PaymentToken { get; set; }
        public string GatewayReferenceCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtDisplay { get; set; }
        public DateTime? PaymentCompletionDate { get; set; }
        public string PaymentCompletionDateDisplay { get; set; }
    }

    /// <summary>
    /// ViewModel برای فیلتر جستجو
    /// </summary>
    public class PaymentSearchFilter
    {
        public string SearchTerm { get; set; }
        public OnlinePaymentStatus? Status { get; set; }
        public OnlinePaymentType? PaymentType { get; set; }
        public int? PatientId { get; set; }
        public string PatientName { get; set; }
        public int? DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int? PaymentGatewayId { get; set; }
    }

    /// <summary>
    /// ViewModel برای آمار پرداخت‌ها
    /// </summary>
    public class PaymentStatisticsViewModel
    {
        public int TotalPayments { get; set; }
        public int SuccessfulPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public int CanceledPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SuccessfulAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal FailedAmount { get; set; }
    }
}

