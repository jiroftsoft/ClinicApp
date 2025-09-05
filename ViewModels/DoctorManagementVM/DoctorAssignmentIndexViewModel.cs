using System.Collections.Generic;

namespace ClinicApp.ViewModels.DoctorManagementVM
{
    public class DoctorAssignmentIndexViewModel
    {
        // Page information (replacing ViewBag.Title)
        public string PageTitle { get; set; } = "مدیریت انتسابات کلی پزشکان";
        public string PageSubtitle { get; set; } = "مدیریت عملیات انتساب، انتقال و حذف انتسابات پزشکان";
        
        public AssignmentFilterViewModel Filters { get; set; } = new AssignmentFilterViewModel();
        public AssignmentStatsViewModel Stats { get; set; } = new AssignmentStatsViewModel();
        public List<DoctorAssignmentListItem> Assignments { get; set; } = new List<DoctorAssignmentListItem>();
        public DataTablesRequest DataTablesRequest { get; set; } = new DataTablesRequest();
        
        // Additional properties for better UX
        public bool IsDataLoaded { get; set; }
        public string LastRefreshTime { get; set; }
        public int TotalRecords { get; set; }
        public int FilteredRecords { get; set; }
        
        // Error handling
        public bool HasErrors { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        
        // Loading states
        public bool IsLoading { get; set; }
        public string LoadingMessage { get; set; }
    }

    public class DoctorAssignmentListItem
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorNationalCode { get; set; }
        public string DoctorSpecialization { get; set; }
        public List<DepartmentAssignment> Departments { get; set; } = new List<DepartmentAssignment>();
        public List<ServiceCategoryAssignment> ServiceCategories { get; set; } = new List<ServiceCategoryAssignment>();
        public string Status { get; set; } // active, inactive, pending
        public string AssignmentDate { get; set; }
        public string LastModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        
        // Helper properties for UI
        public string StatusText => GetStatusText(Status);
        public string StatusBadgeClass => GetStatusBadgeClass(Status);
        public string FormattedAssignmentDate => FormatDate(AssignmentDate);
        public string FormattedLastModifiedDate => FormatDate(LastModifiedDate);
        
        private static string GetStatusText(string status)
        {
            return status switch
            {
                "active" => "فعال",
                "inactive" => "غیرفعال",
                "pending" => "انتظار تایید",
                _ => "نامشخص"
            };
        }
        
        private static string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "active" => "bg-success",
                "inactive" => "bg-secondary",
                "pending" => "bg-warning",
                _ => "bg-secondary"
            };
        }
        
        private static string FormatDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return "-";
            
            if (System.DateTime.TryParse(dateString, out var date))
            {
                return date.ToString("yyyy/MM/dd HH:mm");
            }
            
            return dateString;
        }
    }

    public class DepartmentAssignment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Role { get; set; }
        public string StartDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServiceCategoryAssignment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string AuthorizationLevel { get; set; }
        public string GrantedDate { get; set; }
        public string CertificateNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search Search { get; set; } = new Search();
        public List<Order> Order { get; set; } = new List<Order>();
        public List<Column> Columns { get; set; } = new List<Column>();
    }

    public class Search
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

    public class Column
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public Search Search { get; set; } = new Search();
    }
}
