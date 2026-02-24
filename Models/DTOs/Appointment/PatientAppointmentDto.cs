using System;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش نوبت‌های بیمار
    /// </summary>
    public class PatientAppointmentDto
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public string MedicalCouncilCode { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } // "07:30 قبل از ظهر"
        public AppointmentStatus Status { get; set; }
        public string StatusDisplay { get; set; } // "رزرو شده"
        public decimal Price { get; set; }
        public string ClinicName { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public bool IsOnlineBooking { get; set; }
        /// <summary>آیا نوبت مشاوره آنلاین تصویری است؟</summary>
        public bool IsOnlineConsultation { get; set; }
        /// <summary>آیا لینک «ورود به مشاوره تصویری» نمایش داده شود؟ (مشاوره آنلاین + ماژول فعال)</summary>
        public bool ShowOnlineConsultationLink { get; set; }
        public int Duration { get; set; } // مدت زمان ویزیت به دقیقه
        public DateTime CreatedAt { get; set; }
        
        // ✅ ENTERPRISE-GRADE: فیلد برای تشخیص نوبت‌های نیازمند پرداخت
        /// <summary>
        /// آیا این نوبت نیاز به پرداخت دارد؟
        /// true = نوبت رزرو شده اما پرداخت نشده (Status = Pending و PaymentTransactionId = null)
        /// </summary>
        public bool RequiresPayment { get; set; }
        
        /// <summary>
        /// شناسه تراکنش پرداخت (اگر پرداخت شده باشد)
        /// </summary>
        public int? PaymentTransactionId { get; set; }
    }
}

