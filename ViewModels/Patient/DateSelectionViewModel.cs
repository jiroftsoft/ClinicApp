using System;

namespace ClinicApp.ViewModels.Patient
{
    /// <summary>
    /// ViewModel برای صفحه انتخاب تاریخ
    /// </summary>
    public class DateSelectionViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
    }
}

