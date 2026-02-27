using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using ClinicApp.Models.DTOs.Payment;
using ClinicApp.Models.Core;

namespace ClinicApp.ViewModels.Payment
{
    /// <summary>
    /// ViewModels برای ماژول گزارشات صندوق
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
    /// </summary>

    #region Enums

    /// <summary>
    /// نوع گزارش
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// گزارش روزانه
        /// </summary>
        [Display(Name = "روزانه")]
        Daily = 1,

        /// <summary>
        /// گزارش ماهانه
        /// </summary>
        [Display(Name = "ماهانه")]
        Monthly = 2,

        /// <summary>
        /// گزارش بازه زمانی
        /// </summary>
        [Display(Name = "بازه زمانی")]
        Range = 3,

        /// <summary>
        /// خلاصه تمام منشی‌ها
        /// </summary>
        [Display(Name = "همه منشی‌ها")]
        AllCashiers = 4,

        /// <summary>
        /// مقایسه منشی‌ها
        /// </summary>
        [Display(Name = "مقایسه")]
        Compare = 5
    }

    #endregion

    #region Filter ViewModel

    /// <summary>
    /// ViewModel برای فیلتر گزارش‌ها
    /// </summary>
    public class CashierReportFilterViewModel
    {
        /// <summary>
        /// از تاریخ
        /// </summary>
        [Display(Name = "از تاریخ")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تا تاریخ
        /// </summary>
        [Display(Name = "تا تاریخ")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// از تاریخ (شمسی) برای ارسال از فرم و نمایش — در کنترلر به StartDate تبدیل می‌شود.
        /// </summary>
        public string StartDateShamsi { get; set; }

        /// <summary>
        /// تا تاریخ (شمسی) برای ارسال از فرم و نمایش — در کنترلر به EndDate تبدیل می‌شود.
        /// </summary>
        public string EndDateShamsi { get; set; }

        /// <summary>
        /// شناسه منشی
        /// </summary>
        [Display(Name = "منشی")]
        public string CashierId { get; set; }

        /// <summary>
        /// نوع گزارش
        /// </summary>
        [Display(Name = "نوع گزارش")]
        public ReportType ReportType { get; set; }

        /// <summary>
        /// سال (برای گزارش ماهانه)
        /// </summary>
        [Display(Name = "سال")]
        public int? Year { get; set; }

        /// <summary>
        /// ماه (برای گزارش ماهانه) - 1 تا 12
        /// </summary>
        [Display(Name = "ماه")]
        [Range(1, 12, ErrorMessage = "ماه باید بین 1 تا 12 باشد")]
        public int? Month { get; set; }
    }

    #endregion

    #region Index ViewModel

    /// <summary>
    /// ViewModel برای صفحه اصلی گزارش‌ها
    /// </summary>
    public class CashierReportIndexViewModel
    {
        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// لیست منشی‌ها برای DropDown
        /// </summary>
        public List<SelectListItem> Cashiers { get; set; }

        /// <summary>
        /// نوع گزارش انتخاب شده
        /// </summary>
        public ReportType SelectedReportType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierReportIndexViewModel()
        {
            Filter = new CashierReportFilterViewModel
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today,
                ReportType = ReportType.Daily
            };
            Cashiers = new List<SelectListItem>();
            SelectedReportType = ReportType.Daily;
        }
    }

    #endregion

    #region Daily Report ViewModel

    /// <summary>
    /// ViewModel استاندارد مالی برای گزارش روزانه منشی
    /// محاسبات در سرویس؛ نمایش و فیلتر در این مدل.
    /// </summary>
    public class CashierDailyReportViewModel
    {
        /// <summary>
        /// گزارش روزانه (داده و محاسبات از Service Layer)
        /// </summary>
        public CashierDailyReport Report { get; set; }

        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// زمان تولید گزارش (UTC) برای Audit و نمایش در footer
        /// </summary>
        public DateTime? GeneratedAtUtc { get; set; }

        /// <summary>
        /// تاریخ گزارش به شمسی برای نمایش در هدر/چاپ
        /// </summary>
        public string ReportDatePersian { get; set; }

        /// <summary>
        /// واحد پول برای نمایش (ریال)
        /// </summary>
        public string CurrencyLabel => "ریال";

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierDailyReportViewModel()
        {
            Report = new CashierDailyReport();
            Filter = new CashierReportFilterViewModel();
        }
    }

    #endregion

    #region Monthly Report ViewModel

    /// <summary>
    /// ViewModel برای گزارش ماهانه
    /// </summary>
    public class CashierMonthlyReportViewModel
    {
        /// <summary>
        /// گزارش ماهانه
        /// </summary>
        public CashierMonthlyReport Report { get; set; }

        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierMonthlyReportViewModel()
        {
            Report = new CashierMonthlyReport();
            Filter = new CashierReportFilterViewModel();
        }
    }

    #endregion

    #region Range Report ViewModel

    /// <summary>
    /// ViewModel برای گزارش بازه زمانی
    /// </summary>
    public class CashierRangeReportViewModel
    {
        /// <summary>
        /// گزارش بازه زمانی (از نوع DailyReport با تجمیع)
        /// </summary>
        public CashierDailyReport Report { get; set; }

        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierRangeReportViewModel()
        {
            Report = new CashierDailyReport();
            Filter = new CashierReportFilterViewModel();
        }
    }

    #endregion

    #region All Cashiers Summary ViewModel

    /// <summary>
    /// ViewModel برای خلاصه تمام منشی‌ها
    /// </summary>
    public class CashierAllCashiersSummaryViewModel
    {
        /// <summary>
        /// لیست خلاصه منشی‌ها
        /// </summary>
        public List<CashierSummary> Summaries { get; set; }

        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// نتیجه صفحه‌بندی شده (در صورت نیاز)
        /// </summary>
        public Interfaces.PagedResult<CashierSummary> PagedResult { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierAllCashiersSummaryViewModel()
        {
            Summaries = new List<CashierSummary>();
            Filter = new CashierReportFilterViewModel();
            PagedResult = null;
        }
    }

    #endregion

    #region Compare Cashiers ViewModel

    /// <summary>
    /// ViewModel برای مقایسه منشی‌ها
    /// </summary>
    public class CashierCompareCashiersViewModel
    {
        /// <summary>
        /// نتیجه مقایسه
        /// </summary>
        public CashierPerformanceComparison Comparison { get; set; }

        /// <summary>
        /// فیلتر گزارش
        /// </summary>
        public CashierReportFilterViewModel Filter { get; set; }

        /// <summary>
        /// لیست منشی‌های موجود برای انتخاب
        /// </summary>
        public List<SelectListItem> AvailableCashiers { get; set; }

        /// <summary>
        /// لیست شناسه‌های منشی‌های انتخاب شده
        /// </summary>
        public List<string> SelectedCashierIds { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CashierCompareCashiersViewModel()
        {
            Comparison = new CashierPerformanceComparison();
            Filter = new CashierReportFilterViewModel();
            AvailableCashiers = new List<SelectListItem>();
            SelectedCashierIds = new List<string>();
        }
    }

    #endregion
}

