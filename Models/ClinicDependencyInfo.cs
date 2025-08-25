using System.Collections.Generic;

namespace ClinicApp.Models
{
    /// <summary>
    /// 🏥 MEDICAL: اطلاعات وابستگی‌های کلینیک برای اعتبارسنجی حذف
    /// </summary>
    public class ClinicDependencyInfo
    {
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        
        // وابستگی‌های مستقیم
        public int ActiveDepartmentCount { get; set; }
        public int TotalDepartmentCount { get; set; }
        
        // وابستگی‌های غیرمستقیم (از طریق دپارتمان‌ها)
        public int ActiveServiceCategoryCount { get; set; }
        public int TotalServiceCategoryCount { get; set; }
        public int ActiveServiceCount { get; set; }
        public int TotalServiceCount { get; set; }
        public int ActiveDoctorCount { get; set; }
        public int TotalDoctorCount { get; set; }
        
        // جزئیات وابستگی‌ها
        public List<DepartmentDependencyInfo> Departments { get; set; } = new List<DepartmentDependencyInfo>();
        
        // نتیجه بررسی
        public bool CanBeDeleted => ActiveDepartmentCount == 0 && 
                                   ActiveServiceCategoryCount == 0 && 
                                   ActiveServiceCount == 0 && 
                                   ActiveDoctorCount == 0;
        
        public string DeletionErrorMessage
        {
            get
            {
                if (CanBeDeleted) return null;
                
                var reasons = new List<string>();
                
                if (ActiveDepartmentCount > 0)
                    reasons.Add($"{ActiveDepartmentCount} دپارتمان فعال");
                
                if (ActiveServiceCategoryCount > 0)
                    reasons.Add($"{ActiveServiceCategoryCount} دسته‌بندی خدمت فعال");
                
                if (ActiveServiceCount > 0)
                    reasons.Add($"{ActiveServiceCount} خدمت فعال");
                
                if (ActiveDoctorCount > 0)
                    reasons.Add($"{ActiveDoctorCount} پزشک فعال");
                
                return $"امکان حذف کلینیک '{ClinicName}' وجود ندارد زیرا دارای {string.Join("، ", reasons)} است.";
            }
        }
        
        public string SummaryMessage
        {
            get
            {
                return $"کلینیک '{ClinicName}' دارای:\n" +
                       $"• {TotalDepartmentCount} دپارتمان ({ActiveDepartmentCount} فعال)\n" +
                       $"• {TotalServiceCategoryCount} دسته‌بندی خدمت ({ActiveServiceCategoryCount} فعال)\n" +
                       $"• {TotalServiceCount} خدمت ({ActiveServiceCount} فعال)\n" +
                       $"• {TotalDoctorCount} پزشک ({ActiveDoctorCount} فعال)";
            }
        }
    }
    
    /// <summary>
    /// 🏥 MEDICAL: اطلاعات وابستگی‌های دپارتمان
    /// </summary>
    public class DepartmentDependencyInfo
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCategoryCount { get; set; }
        public int ServiceCount { get; set; }
        public int DoctorCount { get; set; }
    }
}
