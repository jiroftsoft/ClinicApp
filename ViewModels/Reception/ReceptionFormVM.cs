using System.Collections.Generic;

namespace ClinicApp.ViewModels.Reception
{
    public class ReceptionFormVM
    {
        public PatientSectionVM Patient { get; set; } = new PatientSectionVM();
        public InsuranceSectionVM Insurance { get; set; } = new InsuranceSectionVM();
        public ClinicDepartmentSectionVM ClinicDept { get; set; } = new ClinicDepartmentSectionVM();
        public ServicePickerSectionVM ServicePicker { get; set; } = new ServicePickerSectionVM();
        public TotalsVM Totals { get; set; } = new TotalsVM();
        public PaymentSectionVM Payment { get; set; } = new PaymentSectionVM();
        public SidebarVM Sidebar { get; set; } = new SidebarVM();
        public BootstrapVM Bootstrap { get; set; } = new BootstrapVM();
    }

    public class PatientSectionVM { 
        public string NationalCode { get; set; } 
        public int? PatientId { get; set; } 
        public string FullName { get; set; } 
        public string Mobile { get; set; } 
    }
    
    public class InsuranceSectionVM { 
        public int? BasePlanId { get; set; } 
        public int? SupplementaryPlanId { get; set; } 
        public string BasePlanTitle { get; set; } 
        public string SuppPlanTitle { get; set; } 
    }
    
    public class ClinicDepartmentSectionVM { 
        public int? ClinicId { get; set; } 
        public int? DepartmentId { get; set; } 
        public int? DoctorId { get; set; } 
    }
    
    public class ServicePickerSectionVM { 
        public List<ReceptionItemVM> SelectedItems { get; set; } = new List<ReceptionItemVM>(); 
    }
    
    public class ReceptionItemVM { 
        public int ServiceId { get; set; } 
        public string Code { get; set; } 
        public string Name { get; set; } 
        public int Qty { get; set; } 
        public decimal UnitPriceIRR { get; set; } 
        public decimal TotalIRR { get; set; } 
    }
    
    public class TotalsVM { 
        public decimal Gross { get; set; } 
        public decimal BaseInsurance { get; set; } 
        public decimal Supplementary { get; set; } 
        public decimal PatientPayable { get; set; } 
    }
    
    public class PaymentSectionVM { 
        public string Method { get; set; } = "POS"; 
        public decimal AmountIRR { get; set; } 
        public string RRN { get; set; } 
        public string TraceNo { get; set; } 
        public string TerminalId { get; set; } 
        public string CardLast4 { get; set; } 
        public int? CashSessionId { get; set; } 
    }
    
    public class SidebarVM { 
        public List<string> QuickTips { get; set; } = new List<string>(); 
    }
    
    public class BootstrapVM { 
        public int FinancialYear { get; set; } = 1404; 
    }
}
