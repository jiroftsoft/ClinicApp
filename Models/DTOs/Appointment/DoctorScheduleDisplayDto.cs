using System;
using System.Collections.Generic;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش برنامه کاری پزشک در Patient Portal
    /// طراحی شده با رعایت SRP و اصول Clean Code
    /// </summary>
    public class DoctorScheduleDisplayDto
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentDuration { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// لیست روزهای کاری هفتگی
        /// </summary>
        public List<WorkDayDisplayDto> WorkDays { get; set; } = new List<WorkDayDisplayDto>();
    }

    /// <summary>
    /// DTO برای نمایش روز کاری
    /// </summary>
    public class WorkDayDisplayDto
    {
        public int WorkDayId { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; }
        public string DayNameShort { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// لیست بازه‌های زمانی کاری
        /// </summary>
        public List<TimeRangeDisplayDto> TimeRanges { get; set; } = new List<TimeRangeDisplayDto>();
    }

    /// <summary>
    /// DTO برای نمایش بازه زمانی
    /// </summary>
    public class TimeRangeDisplayDto
    {
        public int TimeRangeId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string DisplayTime { get; set; }
        public string DisplayRange { get; set; }
        public bool IsActive { get; set; }
    }
}

