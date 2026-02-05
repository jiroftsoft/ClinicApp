using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Models.DTOs.Appointment;
using AppointmentEntity = ClinicApp.Models.Entities.Appointment.Appointment;

namespace ClinicApp.Interfaces.Appointment
{
    /// <summary>
    /// Interface برای سرویس رزرو نوبت آنلاین
    /// 
    /// مسئولیت: مدیریت منطق کسب‌وکار رزرو نوبت برای بیماران
    /// 
    /// اصول طراحی:
    /// ✅ Single Responsibility: فقط منطق رزرو نوبت
    /// ✅ Separation of Concerns: جدا از Repository و Controller
    /// ✅ High Testability: Interface قابل Mock کردن
    /// ✅ Production-Ready: آماده برای استفاده در Production
    /// </summary>
    public interface IAppointmentBookingService
    {
        #region Patient Appointments

        /// <summary>
        /// دریافت لیست نوبت‌های بیمار
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="startDate">تاریخ شروع (اختیاری)</param>
        /// <param name="endDate">تاریخ پایان (اختیاری)</param>
        /// <returns>لیست نوبت‌های بیمار</returns>
        Task<ServiceResult<List<PatientAppointmentDto>>> GetPatientAppointmentsAsync(
            int patientId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// دریافت جزئیات یک نوبت
        /// </summary>
        /// <param name="appointmentId">شناسه نوبت</param>
        /// <param name="patientId">شناسه بیمار (برای امنیت)</param>
        /// <returns>جزئیات نوبت</returns>
        Task<ServiceResult<PatientAppointmentDto>> GetAppointmentDetailsAsync(
            int appointmentId,
            int patientId);

        /// <summary>
        /// لغو نوبت
        /// </summary>
        /// <param name="appointmentId">شناسه نوبت</param>
        /// <param name="patientId">شناسه بیمار (برای امنیت)</param>
        /// <returns>نتیجه لغو</returns>
        Task<ServiceResult> CancelAppointmentAsync(int appointmentId, int patientId);

        #endregion

        #region Doctor Selection

        /// <summary>
        /// دریافت لیست پزشکان قابل رزرو
        /// </summary>
        /// <param name="departmentId">شناسه بخش (اختیاری)</param>
        /// <param name="searchTerm">عبارت جستجو (نام، تخصص، کد نظام پزشکی)</param>
        /// <returns>لیست پزشکان</returns>
        Task<ServiceResult<List<DoctorSearchResultDto>>> GetAvailableDoctorsAsync(
            int? departmentId = null,
            string searchTerm = null);

        /// <summary>
        /// دریافت اطلاعات یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>اطلاعات پزشک</returns>
        Task<ServiceResult<DoctorSearchResultDto>> GetDoctorDetailsAsync(int doctorId);

        #endregion

        #region Time Slots

        /// <summary>
        /// دریافت اسلات‌های زمانی در دسترس برای یک پزشک در یک تاریخ مشخص
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="date">تاریخ</param>
        /// <returns>لیست اسلات‌های زمانی</returns>
        Task<ServiceResult<List<AvailableTimeSlotDto>>> GetAvailableTimeSlotsAsync(
            int doctorId,
            DateTime date);

        /// <summary>
        /// دریافت مدت زمان نوبت برای یک پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>مدت زمان نوبت (دقیقه)</returns>
        Task<ServiceResult<int>> GetAppointmentDurationAsync(int doctorId);

        /// <summary>
        /// بررسی دسترسی‌پذیری یک اسلات زمانی
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="appointmentDate">تاریخ نوبت</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <returns>نتیجه بررسی</returns>
        Task<ServiceResult<bool>> CheckSlotAvailabilityAsync(
            int doctorId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime);

        #endregion

        #region Booking

        /// <summary>
        /// رزرو نوبت
        /// </summary>
        /// <param name="request">درخواست رزرو</param>
        /// <returns>نوبت ایجاد شده</returns>
        Task<ServiceResult<AppointmentEntity>> ReserveAppointmentAsync(
            AppointmentBookingRequestDto request);

        /// <summary>
        /// محاسبه قیمت نوبت (شامل تخفیف ایونت تبلیغاتی بر اساس تاریخ نوبت)
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <param name="serviceCategoryId">شناسه دسته‌بندی خدمت (اختیاری)</param>
        /// <param name="appointmentDate">تاریخ نوبت (اختیاری؛ برای اعمال صحیح تخفیف ایونت مثلاً عید نوروز)</param>
        /// <returns>قیمت نوبت</returns>
        Task<ServiceResult<decimal>> GetAppointmentPriceAsync(
            int doctorId,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null);

        /// <summary>
        /// دریافت جزئیات قیمت نوبت (پایه، تخفیف، نهایی) برای نمایش در صفحه انتخاب نوبت
        /// </summary>
        Task<ServiceResult<Models.DTOs.Appointment.AppointmentPriceBreakdownDto>> GetAppointmentPriceBreakdownAsync(
            int doctorId,
            int? serviceCategoryId = null,
            DateTime? appointmentDate = null);

        /// <summary>
        /// بررسی تداخل نوبت‌های بیمار (Double Booking Prevention)
        /// ✅ CRITICAL: با Locking برای جلوگیری از Race Condition
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <param name="appointmentDate">تاریخ نوبت</param>
        /// <param name="startTime">زمان شروع</param>
        /// <param name="endTime">زمان پایان</param>
        /// <returns>true اگر تداخل وجود دارد</returns>
        Task<ServiceResult<bool>> CheckPatientDoubleBookingAsync(
            int patientId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime);

        #endregion
    }
}

