using System;
using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه انتخاب زمان
    /// </summary>
    public class TimeSlotSelectionViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<AvailableTimeSlotDto> AvailableSlots { get; set; } = new List<AvailableTimeSlotDto>();
        public int AppointmentDuration { get; set; }
    }
}

