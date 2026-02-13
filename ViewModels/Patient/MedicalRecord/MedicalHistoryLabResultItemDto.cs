using System;

namespace ClinicApp.ViewModels.Patient.MedicalRecord
{
    /// <summary>
    /// DTO برای نمایش یا ارسال یک نتیجه آزمایش در تاریخچه پزشکی
    /// </summary>
    public class MedicalHistoryLabResultItemDto
    {
        public int? Id { get; set; }
        public string LabName { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public DateTime? LabDate { get; set; }
        public string LabDateShamsi { get; set; }
        public string ReferenceRange { get; set; }
    }
}
