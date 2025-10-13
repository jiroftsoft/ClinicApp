using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای آمار پزشکان در پذیرش
/// </summary>
public class ReceptionDoctorStatsViewModel
{
    [Display(Name = "شناسه پزشک")]
    public int DoctorId { get; set; }

    [Display(Name = "نام پزشک")]
    public string DoctorName { get; set; }

    [Display(Name = "تخصص")]
    public string Specialty { get; set; }

    [Display(Name = "تاریخ")]
    public DateTime Date { get; set; }

    [Display(Name = "تعداد پذیرش‌ها")]
    public int ReceptionCount { get; set; }

    [Display(Name = "تعداد پذیرش‌ها (سازگاری)")]
    public int ReceptionsCount { get; set; }

    [Display(Name = "تعداد پذیرش‌ها (سازگاری)")]
    public int Count => ReceptionCount;

    [Display(Name = "تعداد پذیرش‌های تکمیل شده")]
    public int CompletedReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های در انتظار")]
    public int PendingReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های لغو شده")]
    public int CancelledReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های در حال انجام")]
    public int InProgressReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های اورژانس")]
    public int EmergencyReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های آنلاین")]
    public int OnlineReceptions { get; set; }

    [Display(Name = "تعداد پذیرش‌های عادی")]
    public int NormalReceptions { get; set; }

    [Display(Name = "مجموع درآمد")]
    public decimal TotalRevenue { get; set; }

    [Display(Name = "میانگین درآمد")]
    public decimal AverageRevenue { get; set; }

    [Display(Name = "میانگین درآمد هر پذیرش")]
    public decimal AverageRevenuePerReception { get; set; }

    [Display(Name = "درآمد نقدی")]
    public decimal CashPayments { get; set; }

    [Display(Name = "درآمد کارتی")]
    public decimal CardPayments { get; set; }

    [Display(Name = "درآمد آنلاین")]
    public decimal OnlinePayments { get; set; }

    [Display(Name = "درآمد بیمه")]
    public decimal InsurancePayments { get; set; }

    [Display(Name = "درصد تکمیل")]
    public decimal CompletionRate { get; set; }

    [Display(Name = "درصد لغو")]
    public decimal CancellationRate { get; set; }

    [Display(Name = "درصد اورژانس")]
    public decimal EmergencyRate { get; set; }

    [Display(Name = "میانگین زمان انتظار")]
    public string AverageWaitingTime { get; set; }

    [Display(Name = "نرخ رضایت")]
    public decimal SatisfactionRate { get; set; }
}