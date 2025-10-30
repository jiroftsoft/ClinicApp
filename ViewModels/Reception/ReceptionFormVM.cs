using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "کدملی")]
        [Required(ErrorMessage = "کدملی الزامی است")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کدملی باید 10 رقم باشد")]
        public string NationalCode { get; set; } 

        public int? PatientId { get; set; } 

        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = "نام و نام خانوادگی الزامی است")]
        [StringLength(100, ErrorMessage = "حداکثر 100 کاراکتر")]
        public string FullName { get; set; } 

        [Display(Name = "موبایل")]
        [Required(ErrorMessage = "موبایل الزامی است")]
        [RegularExpression(@"^0\d{10}$", ErrorMessage = "شماره موبایل نامعتبر است")]
        public string Mobile { get; set; } 
    }
    
    public class InsuranceSectionVM { 
        [Display(Name = "بیمه پایه")]
        public int? BasePlanId { get; set; } 

        [Display(Name = "بیمه تکمیلی")]
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
        [Display(Name = "خدمت")]
        [Required(ErrorMessage = "انتخاب خدمت الزامی است")]
        public int? ServiceId { get; set; }

        [Display(Name = "تعداد")]
        [Range(1, int.MaxValue, ErrorMessage = "تعداد باید حداقل 1 باشد")]
        public int Quantity { get; set; } = 1;

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
        [Display(Name = "روش پرداخت")]
        [Required]
        public string Method { get; set; } = "POS"; 

        [Display(Name = "مبلغ (ریال)")]
        [Range(0, double.MaxValue, ErrorMessage = "مبلغ نامعتبر است")]
        public decimal AmountIRR { get; set; } 

        [Display(Name = "RRN")]
        public string RRN { get; set; } 

        [Display(Name = "Trace")]
        public string TraceNo { get; set; } 

        [Display(Name = "Terminal")]
        public string TerminalId { get; set; } 

        [Display(Name = "4 رقم کارت")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "۴ رقم کارت نامعتبر است")]
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
