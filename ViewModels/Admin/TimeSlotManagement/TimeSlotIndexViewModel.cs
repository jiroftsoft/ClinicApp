using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Admin.TimeSlotManagement
{
    /// <summary>
    /// ViewModel برای نمایش اسلات زمانی در لیست
    /// </summary>
    public class TimeSlotIndexViewModel
    {
        [Display(Name = "شناسه اسلات")]
        public int TimeSlotId { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "تاریخ نوبت")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "تاریخ نوبت (شمسی)")]
        public string AppointmentDateShamsi => AppointmentDate.ToPersianDate();

        [Display(Name = "روز هفته")]
        public string DayOfWeekName => PersianDateHelper.GetPersianDayOfWeekName(AppointmentDate.DayOfWeek);

        [Display(Name = "زمان شروع")]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "زمان شروع (متن)")]
        public string StartTimeText => StartTime.ToString(@"hh\:mm");

        [Display(Name = "زمان پایان")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "زمان پایان (متن)")]
        public string EndTimeText => EndTime.ToString(@"hh\:mm");

        [Display(Name = "مدت زمان (دقیقه)")]
        public int Duration { get; set; }

        [Display(Name = "وضعیت")]
        public AppointmentStatus Status { get; set; }

        [Display(Name = "نام وضعیت")]
        public string StatusName => Status.GetDisplayName();

        [Display(Name = "شناسه نوبت")]
        public int? AppointmentId { get; set; }

        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi => CreatedAt.ToPersianDateTime();

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        public static TimeSlotIndexViewModel FromEntity(DoctorTimeSlot timeSlot)
        {
            if (timeSlot == null) return null;

            return new TimeSlotIndexViewModel
            {
                TimeSlotId = timeSlot.TimeSlotId,
                DoctorId = timeSlot.DoctorId,
                DoctorName = timeSlot.Doctor != null 
                    ? $"{timeSlot.Doctor.FirstName} {timeSlot.Doctor.LastName}".Trim()
                    : "نامشخص",
                AppointmentDate = timeSlot.AppointmentDate,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                Duration = timeSlot.Duration,
                Status = timeSlot.Status,
                AppointmentId = timeSlot.AppointmentId,
                PatientName = timeSlot.Appointment?.Patient != null
                    ? $"{timeSlot.Appointment.Patient.FirstName} {timeSlot.Appointment.Patient.LastName}".Trim()
                    : null,
                CreatedAt = timeSlot.CreatedAt,
                IsDeleted = timeSlot.IsDeleted
            };
        }
    }
}

