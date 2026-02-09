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
        
        /// <summary>جستجو بر اساس نام، تخصص یا کد نظام پزشکی (مشابه دکترتو/پذیرش۲۴)</summary>
        public string SearchTerm { get; set; }
        
        // ✅ Pagination Properties
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public AvailableAppointmentsViewModel()
        {
            Doctors = new List<DoctorSearchResultDto>();
            AvailableSlots = new List<AvailableTimeSlotDto>();
            SelectedDate = DateTime.Now;
            PageNumber = 1;
            PageSize = 20;
            TotalCount = 0;
        }
    }
}

