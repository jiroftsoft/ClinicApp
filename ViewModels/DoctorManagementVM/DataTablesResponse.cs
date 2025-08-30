using System.Collections.Generic;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class DataTablesResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<DoctorAssignmentListItem> Assignments { get; set; } = new List<DoctorAssignmentListItem>();
        public string Error { get; set; }
    }
}
