using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای خلاصه گزارش پذیرش
/// </summary>
public class ReceptionReportSummaryViewModel
{
    [Display(Name = "کل پذیرش‌ها")]
    public int TotalReceptions { get; set; }

    [Display(Name = "کل مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "میانگین مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal AverageAmount { get; set; }

    [Display(Name = "بیشترین مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal MaxAmount { get; set; }

    [Display(Name = "کمترین مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal MinAmount { get; set; }

    [Display(Name = "کل اورژانس")]
    public int TotalEmergency { get; set; }

    [Display(Name = "کل آنلاین")]
    public int TotalOnline { get; set; }

    [Display(Name = "درصد اورژانس")]
    [DisplayFormat(DataFormatString = "{0:P1}")]
    public decimal EmergencyPercentage { get; set; }

    [Display(Name = "درصد آنلاین")]
    [DisplayFormat(DataFormatString = "{0:P1}")]
    public decimal OnlinePercentage { get; set; }

    public ReceptionReportSummaryViewModel()
    {
        // Constructor for initialization if needed
        TotalReceptions = 0;
        TotalAmount = 0;
        AverageAmount = 0;
        MaxAmount = 0;
        MinAmount = 0;
        TotalEmergency = 0;
        TotalOnline = 0;
        EmergencyPercentage = 0;
        OnlinePercentage = 0;
    }
}