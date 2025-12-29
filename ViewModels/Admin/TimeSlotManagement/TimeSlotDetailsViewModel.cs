using System;
using System.ComponentModel.DataAnnotations;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Models.Entities.Doctor;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Admin.TimeSlotManagement
{
    /// <summary>
    /// ViewModel برای نمایش جزئیات کامل اسلات زمانی
    /// </summary>
    public class TimeSlotDetailsViewModel
    {
        [Display(Name = "شناسه اسلات")]
        public int TimeSlotId { get; set; }

        [Display(Name = "شناسه پزشک")]
        public int DoctorId { get; set; }

        [Display(Name = "نام پزشک")]
        public string DoctorName { get; set; }

        [Display(Name = "کد ملی پزشک")]
        public string DoctorNationalCode { get; set; }

        [Display(Name = "شماره تماس پزشک")]
        public string DoctorPhoneNumber { get; set; }

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

        // اطلاعات نوبت (اگر رزرو شده باشد)
        [Display(Name = "نام بیمار")]
        public string PatientName { get; set; }

        [Display(Name = "کد ملی بیمار")]
        public string PatientNationalCode { get; set; }

        [Display(Name = "شماره تماس بیمار")]
        public string PatientPhoneNumber { get; set; }

        [Display(Name = "تاریخ ایجاد")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاریخ ایجاد (شمسی)")]
        public string CreatedAtShamsi => CreatedAt.ToPersianDateTime();

        [Display(Name = "کاربر ایجاد کننده")]
        public string CreatedByUserName { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "تاریخ آخرین ویرایش (شمسی)")]
        public string UpdatedAtShamsi => UpdatedAt?.ToPersianDateTime();

        [Display(Name = "کاربر آخرین ویرایش کننده")]
        public string UpdatedByUserName { get; set; }

        [Display(Name = "حذف شده")]
        public bool IsDeleted { get; set; }

        [Display(Name = "تاریخ حذف")]
        public DateTime? DeletedAt { get; set; }

        [Display(Name = "تاریخ حذف (شمسی)")]
        public string DeletedAtShamsi => DeletedAt?.ToPersianDateTime();

        [Display(Name = "کاربر حذف کننده")]
        public string DeletedByUserName { get; set; }

        /// <summary>
        /// تبدیل Entity به ViewModel
        /// </summary>
        public static TimeSlotDetailsViewModel FromEntity(DoctorTimeSlot timeSlot)
        {
            if (timeSlot == null) return null;

            return new TimeSlotDetailsViewModel
            {
                TimeSlotId = timeSlot.TimeSlotId,
                DoctorId = timeSlot.DoctorId,
                DoctorName = timeSlot.Doctor != null 
                    ? $"{timeSlot.Doctor.FirstName} {timeSlot.Doctor.LastName}".Trim()
                    : "نامشخص",
                DoctorNationalCode = timeSlot.Doctor?.NationalCode,
                DoctorPhoneNumber = timeSlot.Doctor?.PhoneNumber,
                AppointmentDate = timeSlot.AppointmentDate,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                Duration = timeSlot.Duration,
                Status = timeSlot.Status,
                AppointmentId = timeSlot.AppointmentId,
                PatientName = timeSlot.Appointment?.Patient != null
                    ? $"{timeSlot.Appointment.Patient.FirstName} {timeSlot.Appointment.Patient.LastName}".Trim()
                    : null,
                PatientNationalCode = timeSlot.Appointment?.Patient?.NationalCode,
                PatientPhoneNumber = timeSlot.Appointment?.Patient?.PhoneNumber,
                CreatedAt = timeSlot.CreatedAt,
                CreatedByUserName = timeSlot.CreatedByUser != null
                    ? $"{timeSlot.CreatedByUser.FirstName} {timeSlot.CreatedByUser.LastName}".Trim()
                    : null,
                UpdatedAt = timeSlot.UpdatedAt,
                UpdatedByUserName = timeSlot.UpdatedByUser != null
                    ? $"{timeSlot.UpdatedByUser.FirstName} {timeSlot.UpdatedByUser.LastName}".Trim()
                    : null,
                IsDeleted = timeSlot.IsDeleted,
                DeletedAt = timeSlot.DeletedAt,
                DeletedByUserName = timeSlot.DeletedByUser != null
                    ? $"{timeSlot.DeletedByUser.FirstName} {timeSlot.DeletedByUser.LastName}".Trim()
                    : null
            };
        }
    }
}

