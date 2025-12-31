using System.Collections.Generic;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// ViewModel اصلی برای صفحه پرونده الکترونیک
    /// Single Responsibility: نمایش داده‌های اصلی EMR
    /// </summary>
    public class MedicalRecordIndexViewModel
    {
        public int PatientId { get; set; }
        public string PatientFullName { get; set; }
        public List<MedicalHistoryViewModel> MedicalHistories { get; set; }
        
        public MedicalRecordIndexViewModel()
        {
            MedicalHistories = new List<MedicalHistoryViewModel>();
        }
    }
}

