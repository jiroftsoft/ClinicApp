using System;
using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.ViewModels.DoctorManagementVM;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه جزئیات پزشک (Patient Portal)
    /// </summary>
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }
        public DoctorManagementVM.DoctorDetailsViewModel Doctor { get; set; }
        public DoctorSearchResultDto Schedule { get; set; }
        public DoctorScheduleDisplayDto ScheduleDetails { get; set; }
        public List<AvailableTimeSlotDto> AvailableSlots { get; set; }
        public DateTime SelectedDate { get; set; }
        
        // آمار پزشک
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public decimal AverageRating { get; set; }
        public int ExperienceYears { get; set; }
    }
}
