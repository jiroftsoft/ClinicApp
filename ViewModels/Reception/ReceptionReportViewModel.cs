using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.Enums;

namespace ClinicApp.ViewModels.Reception;

/// <summary>
/// ViewModel برای گزارش‌گیری از پذیرش‌ها
/// </summary>
public class ReceptionReportViewModel
{
    [Display(Name = "عنوان گزارش")]
    [Required(ErrorMessage = "عنوان گزارش الزامی است.")]
    [StringLength(200, ErrorMessage = "عنوان گزارش نمی‌تواند بیش از ۲۰۰ کاراکتر باشد.")]
    public string ReportTitle { get; set; }

    [Display(Name = "نوع گزارش")]
    [Required(ErrorMessage = "نوع گزارش الزامی است.")]
    public ReportType ReportType { get; set; }

    [Display(Name = "تاریخ شروع")]
    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Display(Name = "تاریخ پایان")]
    [Required(ErrorMessage = "تاریخ پایان الزامی است.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Display(Name = "گروه‌بندی بر اساس")]
    public string GroupBy { get; set; } = "Date";

    [Display(Name = "فیلتر وضعیت")]
    public ReceptionStatus? StatusFilter { get; set; }

    [Display(Name = "فیلتر نوع")]
    public ReceptionType? TypeFilter { get; set; }

    [Display(Name = "فیلتر پزشک")]
    public int? DoctorFilter { get; set; }

    [Display(Name = "فیلتر بیمه")]
    public int? InsuranceFilter { get; set; }

    [Display(Name = "شامل اورژانس")]
    public bool IncludeEmergency { get; set; } = true;

    [Display(Name = "شامل پذیرش آنلاین")]
    public bool IncludeOnline { get; set; } = true;

    [Display(Name = "فرمت خروجی")]
    public string OutputFormat { get; set; } = "PDF";

    // نتایج گزارش
    public List<ReceptionReportDataViewModel> ReportData { get; set; }
    public ReceptionReportSummaryViewModel Summary { get; set; }

    // لیست‌های کمکی
    public IEnumerable<SelectListItem> ReportTypeList { get; set; }
    public IEnumerable<SelectListItem> GroupByList { get; set; }
    public IEnumerable<SelectListItem> StatusList { get; set; }
    public IEnumerable<SelectListItem> TypeList { get; set; }
    public IEnumerable<SelectListItem> DoctorList { get; set; }
    public IEnumerable<SelectListItem> InsuranceList { get; set; }
    public IEnumerable<SelectListItem> OutputFormatList { get; set; }

    public ReceptionReportViewModel()
    {
        ReportData = new List<ReceptionReportDataViewModel>();
        Summary = new ReceptionReportSummaryViewModel();
        ReportTypeList = new List<SelectListItem>();
        GroupByList = new List<SelectListItem>();
        StatusList = new List<SelectListItem>();
        TypeList = new List<SelectListItem>();
        DoctorList = new List<SelectListItem>();
        InsuranceList = new List<SelectListItem>();
        OutputFormatList = new List<SelectListItem>();
        StartDate = DateTime.Now.AddDays(-30);
        EndDate = DateTime.Now;
        GroupBy = "Date";
        IncludeEmergency = true;
        IncludeOnline = true;
        OutputFormat = "PDF";
        ReportType = ReportType.Daily;
    }
}