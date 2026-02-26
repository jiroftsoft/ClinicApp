using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه انتخاب تاریخ (Context-Aware Booking Step)
    /// </summary>
    public class DateSelectionViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        /// <summary>کد نظام پزشکی</summary>
        public string MedicalCouncilCode { get; set; }
        /// <summary>متن اولین نوبت خالی (مثلاً: پنجشنبه 7 اسفند — 15:40)</summary>
        public string FirstAvailableSlotText { get; set; }
        /// <summary>آیا مشاوره آنلاین دارد</summary>
        public bool HasOnlineConsultation { get; set; }
        /// <summary>تاریخ‌های دارای نوبت (برای کارت تاریخ و پیشنهاد سریع)</summary>
        public List<AvailableDateInfo> AvailableDates { get; set; } = new List<AvailableDateInfo>();
    }
}

