using ClinicApp.Helpers;
using ClinicApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClinicApp.Interfaces
{
    public interface IReceptionService
    {
        /// <summary>
        /// ایجاد پذیرش جدید.
        /// </summary>
        Task<ServiceResult<int>> CreateReceptionAsync(ReceptionCreateViewModel model, string receptionistId);

        /// <summary>
        /// بروزرسانی اطلاعات یک پذیرش موجود.
        /// </summary>
        Task<ServiceResult<bool>> UpdateReceptionAsync(int receptionId, ReceptionCreateViewModel model, string updatedByUserId);

        /// <summary>
        /// حذف یک پذیرش (در صورت مجاز بودن).
        /// </summary>
        Task<ServiceResult<bool>> DeleteReceptionAsync(int receptionId);

        /// <summary>
        /// دریافت لیست پذیرش‌ها برای نمایش در صفحه اصلی.
        /// </summary>
        Task<ServiceResult<List<ReceptionIndexViewModel>>> GetReceptionsAsync();

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس فیلترهای مختلف (بیمار، پزشک، تاریخ و ...).
        /// </summary>
        Task<ServiceResult<List<ReceptionIndexViewModel>>> SearchReceptionsAsync(string patientName, string doctorName, string dateFrom, string dateTo);

        /// <summary>
        /// دریافت اطلاعات کامل یک پذیرش برای صفحه جزییات.
        /// </summary>
        Task<ServiceResult<ReceptionDetailsViewModel>> GetReceptionDetailsAsync(int receptionId);

        /// <summary>
        /// ثبت یک پرداخت جدید برای پذیرش.
        /// </summary>
        Task<ServiceResult<bool>> AddPaymentAsync(int receptionId, PaymentViewModel payment, string cashierId);

        /// <summary>
        /// دریافت رسید چاپی یک پذیرش.
        /// </summary>
        Task<ServiceResult<ReceiptPrintViewModel>> GetReceiptAsync(int receptionId);

        /// <summary>
        /// ثبت رکورد چاپ قبض در دیتابیس.
        /// </summary>
        Task<ServiceResult<bool>> SaveReceiptPrintAsync(ReceiptPrintViewModel model, string printedByUserId);

        /// <summary>
        /// تغییر وضعیت پذیرش (مثلاً از "در حال انجام" به "تکمیل شده").
        /// </summary>
        Task<ServiceResult<bool>> ChangeReceptionStatusAsync(int receptionId, string newStatus, string updatedByUserId);
    }
}
