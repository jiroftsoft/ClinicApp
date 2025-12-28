using System;
using System.Collections.Generic;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Admin.PaymentManagement
{
    /// <summary>
    /// ViewModel برای صفحه Details پرداخت
    /// طراحی شده طبق اصول Strongly-Typed Development
    /// </summary>
    public class PaymentDetailsViewModel
    {
        public int OnlinePaymentId { get; set; }
        public int? AppointmentId { get; set; }
        public int? ReceptionId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string PatientNationalCode { get; set; }
        public string PatientPhoneNumber { get; set; }
        public int? DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public OnlinePaymentType PaymentType { get; set; }
        public string PaymentTypeDisplay { get; set; }
        public OnlinePaymentStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public decimal Amount { get; set; }
        public string AmountDisplay { get; set; }
        public decimal? GatewayFee { get; set; }
        public string GatewayFeeDisplay { get; set; }
        public decimal? NetAmount { get; set; }
        public string NetAmountDisplay { get; set; }
        public int PaymentGatewayId { get; set; }
        public string GatewayName { get; set; }
        public PaymentGatewayType GatewayType { get; set; }
        public string GatewayTypeDisplay { get; set; }
        public string PaymentToken { get; set; }
        public string GatewayTransactionId { get; set; }
        public string GatewayReferenceCode { get; set; }
        public string InternalTransactionId { get; set; }
        public string PaymentUrl { get; set; }
        public DateTime? PaymentStartDate { get; set; }
        public string PaymentStartDateDisplay { get; set; }
        public DateTime? PaymentCompletionDate { get; set; }
        public string PaymentCompletionDateDisplay { get; set; }
        public DateTime? PaymentExpiryDate { get; set; }
        public string PaymentExpiryDateDisplay { get; set; }
        public string UserIpAddress { get; set; }
        public string UserAgent { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtDisplay { get; set; }
        public string CreatedByUserName { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtDisplay { get; set; }
        public string UpdatedByUserName { get; set; }

        /// <summary>
        /// Timeline رویدادها
        /// </summary>
        public List<PaymentTimelineItemViewModel> Timeline { get; set; }

        /// <summary>
        /// آیا می‌توان Retry کرد؟
        /// </summary>
        public bool CanRetry { get; set; }

        /// <summary>
        /// آیا می‌توان Cancel کرد؟
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// آیا می‌توان Refund کرد؟
        /// </summary>
        public bool CanRefund { get; set; }

        public PaymentDetailsViewModel()
        {
            Timeline = new List<PaymentTimelineItemViewModel>();
        }
    }

    /// <summary>
    /// ViewModel برای آیتم Timeline
    /// </summary>
    public class PaymentTimelineItemViewModel
    {
        public DateTime Date { get; set; }
        public string DateDisplay { get; set; }
        public string Event { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
    }
}

