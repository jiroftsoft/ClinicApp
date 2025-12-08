using System;

namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش اسلات‌های زمانی در دسترس
    /// </summary>
    public class AvailableTimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string DisplayTime { get; set; } // "07:30 قبل از ظهر"
        public string DisplayRange { get; set; } // "07:30 - 07:45 قبل از ظهر"
        public int Duration { get; set; } // مدت زمان به دقیقه
    }
}

