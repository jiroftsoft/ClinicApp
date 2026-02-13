using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// DTO برای نمایش یا ارسال یک دارو در تاریخچه پزشکی
    /// </summary>
    public class MedicalHistoryMedicationItemDto
    {
        public int? Id { get; set; }
        public string DrugName { get; set; }
        public string Dosage { get; set; }
        public string DosageUnit { get; set; }
        public string Frequency { get; set; }
        public string Route { get; set; }
        public DateTime? StartDate { get; set; }
        public string StartDateShamsi { get; set; }
        public DateTime? EndDate { get; set; }
        public string EndDateShamsi { get; set; }
        public string Indication { get; set; }
        public string PrescribingDoctor { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
