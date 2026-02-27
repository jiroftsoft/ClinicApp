using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicApp.ViewModels.Admin
{
    /// <summary>
    /// فیلتر داشبورد درآمد — بازه تاریخ شمسی، پزشک، دپارتمان، روش پرداخت
    /// </summary>
    public class RevenueDashboardFilterViewModel
    {
        [Display(Name = "از تاریخ")]
        public string StartDatePersian { get; set; }

        [Display(Name = "تا تاریخ")]
        public string EndDatePersian { get; set; }

        [Display(Name = "پزشک")]
        public int? DoctorId { get; set; }

        [Display(Name = "دپارتمان")]
        public int? DepartmentId { get; set; }

        [Display(Name = "روش پرداخت")]
        public string PaymentMethod { get; set; }

        /// <summary>تاریخ شروع میلادی (پس از پارس)</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>تاریخ پایان میلادی (پس از پارس)</summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// خلاصه KPI داشبورد درآمد
    /// </summary>
    public class RevenueSummaryViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal CashRevenue { get; set; }
        public decimal PosRevenue { get; set; }
        public decimal OnlineRevenue { get; set; }
        public decimal OtherRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int ReceptionCount { get; set; }
        public decimal AverageTransactionAmount { get; set; }
        public decimal GrowthRatePercent { get; set; }
        public decimal PreviousPeriodRevenue { get; set; }
        public string PeriodLabel { get; set; }
    }

    /// <summary>
    /// داده نمودار — برچسب‌ها و مجموعه‌های داده برای Chart.js
    /// </summary>
    public class RevenueChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> DailyValues { get; set; } = new List<decimal>();
        public List<decimal> CashValues { get; set; } = new List<decimal>();
        public List<decimal> PosValues { get; set; } = new List<decimal>();
        public List<decimal> OnlineValues { get; set; } = new List<decimal>();
    }

    /// <summary>
    /// درآمد به تفکیک پزشک
    /// </summary>
    public class DoctorRevenueItemViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
        public decimal PercentShare { get; set; }
    }

    /// <summary>
    /// درآمد به تفکیک روز (روند روزانه)
    /// </summary>
    public class DailyRevenueItemViewModel
    {
        public string DatePersian { get; set; }
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// آیتم انتخاب برای دراپ‌داون‌های داشبورد درآمد (strongly-typed، بدون وابستگی به ViewBag)
    /// </summary>
    public class RevenueDashboardSelectItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public bool Selected { get; set; }
    }

    /// <summary>
    /// مدل اصلی صفحه داشبورد درآمد — تمام داده‌های مهم به‌صورت strongly-typed
    /// </summary>
    public class RevenueDashboardViewModel
    {
        public RevenueDashboardFilterViewModel Filter { get; set; } = new RevenueDashboardFilterViewModel();
        public RevenueSummaryViewModel Summary { get; set; } = new RevenueSummaryViewModel();
        public RevenueChartDataViewModel ChartData { get; set; } = new RevenueChartDataViewModel();
        public List<DoctorRevenueItemViewModel> DoctorRevenues { get; set; } = new List<DoctorRevenueItemViewModel>();
        public List<DailyRevenueItemViewModel> DailyTrend { get; set; } = new List<DailyRevenueItemViewModel>();
        public List<RevenueDetailRowViewModel> DetailRows { get; set; } = new List<RevenueDetailRowViewModel>();

        /// <summary>لیست پزشکان برای فیلتر (strongly-typed)</summary>
        public List<RevenueDashboardSelectItem> Doctors { get; set; } = new List<RevenueDashboardSelectItem>();

        /// <summary>لیست دپارتمان‌ها برای فیلتر (strongly-typed)</summary>
        public List<RevenueDashboardSelectItem> Departments { get; set; } = new List<RevenueDashboardSelectItem>();

        /// <summary>لیست روش‌های پرداخت برای فیلتر (strongly-typed)</summary>
        public List<RevenueDashboardSelectItem> PaymentMethods { get; set; } = new List<RevenueDashboardSelectItem>();
    }

    /// <summary>
    /// یک ردیف جزئیات برای جدول/خروجی Excel
    /// </summary>
    public class RevenueDetailRowViewModel
    {
        public DateTime Date { get; set; }
        public string DatePersian { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public int ReceptionId { get; set; }
    }
}
