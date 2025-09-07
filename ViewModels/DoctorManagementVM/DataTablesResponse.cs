using System.Collections.Generic;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class DataTablesResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<object> Data { get; set; } = new List<object>();
        public string Error { get; set; }
    }
}
