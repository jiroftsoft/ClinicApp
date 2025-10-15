using System;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel تخصصی برای نمایش اطلاعات پذیرش
    /// </summary>
    public class ReceptionInformationViewModel
    {
        /// <summary>
        /// شناسه پذیرش
        /// </summary>
        public int? ReceptionId { get; set; }

        /// <summary>
        /// شناسه بیمار
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// نوع پذیرش
        /// </summary>
        public ReceptionType ReceptionType { get; set; } = ReceptionType.Normal;

        /// <summary>
        /// وضعیت پذیرش
        /// </summary>
        public ReceptionStatus Status { get; set; } = ReceptionStatus.Pending;

        /// <summary>
        /// اولویت پذیرش
        /// </summary>
        public AppointmentPriority Priority { get; set; } = AppointmentPriority.Normal;

        /// <summary>
        /// آیا اورژانس است؟
        /// </summary>
        public bool IsEmergency { get; set; } = false;

        /// <summary>
        /// آیا پذیرش آنلاین است؟
        /// </summary>
        public bool IsOnlineReception { get; set; } = false;

        /// <summary>
        /// تاریخ پذیرش
        /// </summary>
        public DateTime ReceptionDate { get; set; }

        /// <summary>
        /// تاریخ پذیرش (شمسی)
        /// </summary>
        public string ReceptionDateShamsi { get; set; }

        /// <summary>
        /// زمان پذیرش
        /// </summary>
        public string ReceptionTime { get; set; }

        /// <summary>
        /// تاریخ و زمان پذیرش (ترکیبی)
        /// </summary>
        public DateTime ReceptionDateTime => ReceptionDate;

        /// <summary>
        /// ایجاد کننده
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// نمایش نوع پذیرش
        /// </summary>
        public string ReceptionTypeDisplay
        {
            get
            {
                return ReceptionType switch
                {
                    ReceptionType.Normal => "عادی",
                    ReceptionType.Emergency => "اورژانس",
                    ReceptionType.Special => "ویژه",
                    ReceptionType.Online => "آنلاین",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// نمایش وضعیت پذیرش
        /// </summary>
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    ReceptionStatus.Pending => "در انتظار",
                    ReceptionStatus.Confirmed => "تأیید شده",
                    ReceptionStatus.InProgress => "در حال انجام",
                    ReceptionStatus.Completed => "تکمیل شده",
                    ReceptionStatus.Cancelled => "لغو شده",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// نمایش اولویت
        /// </summary>
        public string PriorityDisplay
        {
            get
            {
                return Priority switch
                {
                    AppointmentPriority.Low => "کم",
                    AppointmentPriority.Normal => "عادی",
                    AppointmentPriority.High => "بالا",
                    AppointmentPriority.Critical => "بحرانی",
                    _ => "نامشخص"
                };
            }
        }

        /// <summary>
        /// نمایش تاریخ و زمان پذیرش (فرمات شده)
        /// </summary>
        public string ReceptionDateTimeDisplay => $"{ReceptionDateShamsi} - {ReceptionTime}";

        /// <summary>
        /// آیا پذیرش اورژانس است؟
        /// </summary>
        public bool IsUrgent => IsEmergency || Priority == AppointmentPriority.Critical;

        /// <summary>
        /// آیا پذیرش عادی است؟
        /// </summary>
        public bool IsNormal => !IsEmergency && Priority == AppointmentPriority.Normal;

        /// <summary>
        /// نمایش وضعیت کلی
        /// </summary>
        public string OverallStatus
        {
            get
            {
                if (IsUrgent) return "فوری";
                if (IsNormal) return "عادی";
                return "ویژه";
            }
        }
    }
}
