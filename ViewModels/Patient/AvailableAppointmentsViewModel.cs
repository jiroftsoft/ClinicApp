using System;
using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای نمایش عمومی نوبت‌های موجود
    /// </summary>
    public class AvailableAppointmentsViewModel
    {
        public List<DoctorSearchResultDto> Doctors { get; set; }
        public int? SelectedDoctorId { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<AvailableTimeSlotDto> AvailableSlots { get; set; }

        public AvailableAppointmentsViewModel()
        {
            Doctors = new List<DoctorSearchResultDto>();
            AvailableSlots = new List<AvailableTimeSlotDto>();
            SelectedDate = DateTime.Now;
        }
    }
}

