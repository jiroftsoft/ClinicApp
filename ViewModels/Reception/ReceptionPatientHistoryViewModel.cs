using System;
using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    /// <summary>
    /// ViewModel برای نمایش تاریخچه پذیرش‌های بیمار
    /// </summary>
    public class ReceptionPatientHistoryViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string NationalCode { get; set; }
        public List<ReceptionHistoryItem> Receptions { get; set; } = new List<ReceptionHistoryItem>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// آیتم تاریخچه پذیرش
    /// </summary>
    public class ReceptionHistoryItem
    {
        public int ReceptionId { get; set; }
        public string ReceptionNumber { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
    }
}
