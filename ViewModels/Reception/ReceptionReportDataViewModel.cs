using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای داده‌های گزارش پذیرش
/// </summary>
public class ReceptionReportDataViewModel
{
    [Display(Name = "تاریخ")]
    public string Date { get; set; }

    [Display(Name = "تعداد پذیرش")]
    public int ReceptionCount { get; set; }

    [Display(Name = "مجموع مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "میانگین مبلغ")]
    [DisplayFormat(DataFormatString = "{0:N0}")]
    public decimal AverageAmount { get; set; }

    [Display(Name = "تعداد اورژانس")]
    public int EmergencyCount { get; set; }

    [Display(Name = "تعداد آنلاین")]
    public int OnlineCount { get; set; }

    [Display(Name = "تعداد پرداخت نقدی")]
    public int CashPaymentCount { get; set; }

    [Display(Name = "تعداد پرداخت کارتی")]
    public int CardPaymentCount { get; set; }

    [Display(Name = "تعداد پرداخت بیمه")]
    public int InsurancePaymentCount { get; set; }

    public ReceptionReportDataViewModel()
    {
        // Constructor for initialization if needed
        Date = DateTime.Now.ToString("yyyy-MM-dd");
        ReceptionCount = 0;
        TotalAmount = 0;
        AverageAmount = 0;
        EmergencyCount = 0;
        OnlineCount = 0;
        CashPaymentCount = 0;
        CardPaymentCount = 0;
        InsurancePaymentCount = 0;
    }
}