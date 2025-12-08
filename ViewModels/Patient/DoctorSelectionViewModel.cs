using System.Collections.Generic;
using ClinicApp.Models.DTOs.Appointment;
using ClinicApp.ViewModels.ClinicAdmin;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه انتخاب پزشک
    /// </summary>
    public class DoctorSelectionViewModel
    {
        public List<DoctorSearchResultDto> Doctors { get; set; } = new List<DoctorSearchResultDto>();
        public int? SelectedDepartmentId { get; set; }
        public string SearchTerm { get; set; }
        public List<DepartmentInfo> Departments { get; set; } = new List<DepartmentInfo>();
    }
}

