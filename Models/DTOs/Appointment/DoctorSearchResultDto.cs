namespace ClinicApp.Models.DTOs.Appointment
{
    /// <summary>
    /// DTO برای نمایش نتایج جستجوی پزشک
    /// </summary>
    public class DoctorSearchResultDto
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string MedicalCouncilCode { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool HasActiveSchedule { get; set; }
        public string ScheduleInfo { get; set; } // "شنبه تا چهارشنبه - 07:30 تا 12:00"
        public decimal? BasePrice { get; set; }
    }
}

