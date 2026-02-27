using System;
using System.Threading.Tasks;
using ClinicApp.Models.DTOs.Appointment;

namespace ClinicApp.Interfaces
{
    /// <summary>
    /// سرویس گزارش «نوبت‌های رزرو شده توسط بیماران» — مسئولیت واحد: تهیه داده برای گزارش منشی.
    /// </summary>
    public interface IPatientBookedAppointmentsReportService
    {
        /// <summary>
        /// دریافت لیست نوبت‌های رزرو شده توسط بیماران در بازه و نوع ویزیت اختیاری.
        /// visitType: "all" | "inperson" | "online"
        /// </summary>
        Task<PatientBookedAppointmentsReportResult> GetReportAsync(DateTime? fromDate, DateTime? toDate, string visitType = "all");
    }

    /// <summary>
    /// خروجی گزارش نوبت‌های رزرو شده توسط بیماران.
    /// </summary>
    public class PatientBookedAppointmentsReportResult
    {
        public System.Collections.Generic.List<PatientBookedAppointmentReportItemDto> Items { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        /// <summary>all | inperson | online</summary>
        public string VisitType { get; set; }
        public int TotalCount => Items?.Count ?? 0;
    }
}
