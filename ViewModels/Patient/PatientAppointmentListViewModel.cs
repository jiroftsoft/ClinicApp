using System;
using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای نمایش لیست نوبت‌های بیمار
    /// </summary>
    public class PatientAppointmentListViewModel
    {
        public List<PatientAppointmentDto> Appointments { get; set; } = new List<PatientAppointmentDto>();
        public DateTime? StartDateFilter { get; set; }
        public DateTime? EndDateFilter { get; set; }
        public AppointmentStatus? StatusFilter { get; set; }
        public string SearchTerm { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}

