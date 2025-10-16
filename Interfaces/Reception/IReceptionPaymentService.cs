using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.ViewModels.Reception;

namespace ClinicApp.Interfaces.Reception
{
    /// <summary>
    /// اینترفیس برای سرویس مدیریت پرداخت در ماژول پذیرش
    /// طراحی شده برای محیط پروداکشن با ترافیک بالا
    /// </summary>
    public interface IReceptionPaymentService
    {
        /// <summary>
        /// دریافت اطلاعات پرداخت
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <returns>اطلاعات پرداخت</returns>
        Task<ServiceResult<PaymentInfoViewModel>> GetPaymentInfoAsync(int receptionId);

        /// <summary>
        /// شروع پرداخت آنلاین
        /// </summary>
        /// <param name="paymentRequest">اطلاعات درخواست پرداخت</param>
        /// <returns>نتیجه شروع پرداخت آنلاین</returns>
        Task<ServiceResult<OnlinePaymentResponseViewModel>> StartOnlinePaymentAsync(PaymentRequestViewModel paymentRequest);

        /// <summary>
        /// تکمیل پرداخت نقدی
        /// </summary>
        /// <param name="cashPaymentRequest">اطلاعات درخواست پرداخت نقدی</param>
        /// <returns>نتیجه تکمیل پرداخت نقدی</returns>
        Task<ServiceResult<CashPaymentResponseViewModel>> CompleteCashPaymentAsync(CashPaymentRequestViewModel cashPaymentRequest);
        /// <summary>
        /// پردازش پرداخت آنلاین
        /// </summary>
        /// <param name="model">اطلاعات درخواست پرداخت</param>
        /// <returns>نتیجه پردازش پرداخت آنلاین</returns>
        Task<ServiceResult<OnlinePaymentResponseViewModel>> ProcessOnlinePaymentAsync(PaymentRequestViewModel model);

        /// <summary>
        /// تایید پرداخت آنلاین پس از بازگشت از درگاه
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="paymentGatewayRefId">شناسه مرجع درگاه</param>
        /// <returns>نتیجه تایید پرداخت</returns>
        Task<ServiceResult<PaymentConfirmationViewModel>> ConfirmOnlinePaymentAsync(string transactionId, string paymentGatewayRefId);

        /// <summary>
        /// پردازش پرداخت نقدی
        /// </summary>
        /// <param name="model">اطلاعات درخواست پرداخت نقدی</param>
        /// <returns>نتیجه پردازش پرداخت نقدی</returns>
        Task<ServiceResult<CashPaymentResponseViewModel>> ProcessCashPaymentAsync(CashPaymentRequestViewModel model);

        /// <summary>
        /// لغو پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <param name="reason">دلیل لغو</param>
        /// <returns>نتیجه لغو پرداخت</returns>
        Task<ServiceResult<bool>> CancelPaymentAsync(int paymentId, string reason);

        /// <summary>
        /// دریافت وضعیت پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <returns>وضعیت پرداخت</returns>
        Task<ServiceResult<PaymentStatusViewModel>> GetPaymentStatusAsync(int paymentId);

        /// <summary>
        /// تولید رسید پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GeneratePaymentReceiptAsync(int paymentId);

        /// <summary>
        /// دریافت لیست روش‌های پرداخت فعال
        /// </summary>
        /// <returns>لیست روش‌های پرداخت</returns>
        Task<ServiceResult<List<PaymentMethodViewModel>>> GetPaymentMethodsAsync();

        /// <summary>
        /// دریافت اطلاعات درگاه پرداخت
        /// </summary>
        /// <param name="gatewayCode">کد درگاه</param>
        /// <returns>اطلاعات درگاه پرداخت</returns>
        Task<ServiceResult<PaymentGatewayInfoViewModel>> GetPaymentGatewayInfoAsync(string gatewayCode);

        /// <summary>
        /// دریافت رسید پرداخت
        /// </summary>
        /// <param name="paymentId">شناسه پرداخت</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int paymentId);

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(string transactionId);


        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, int patientId);

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId);

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="includeDetails">شامل جزئیات</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId, bool includeDetails);

        /// <summary>
        /// دریافت رسید پرداخت (overload)
        /// </summary>
        /// <param name="receptionId">شناسه پذیرش</param>
        /// <param name="transactionId">شناسه تراکنش</param>
        /// <param name="includeDetails">شامل جزئیات</param>
        /// <param name="includeHistory">شامل تاریخچه</param>
        /// <returns>رسید پرداخت</returns>
        Task<ServiceResult<PaymentReceiptViewModel>> GetPaymentReceiptAsync(int receptionId, string transactionId, bool includeDetails, bool includeHistory);

    }
}