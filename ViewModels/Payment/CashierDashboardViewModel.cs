using System;
using System.Collections.Generic;
using System.Web.Mvc;
using ClinicApp.Models.DTOs.Payment;

namespace ClinicApp.ViewModels.Payment
{
    /// <summary>
    /// ViewModel برای Dashboard اصلی منشی‌ها
    /// 
    /// طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md
    /// </summary>
    public class CashierDashboardViewModel
    {
        /// <summary>
        /// تاریخ انتخاب شده
        /// </summary>
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        /// <summary>
        /// شناسه منشی انتخاب شده
        /// </summary>
        public string SelectedCashierId { get; set; }

        /// <summary>
        /// آمار روزانه
        /// </summary>
        public CashierStatsViewModel DailyStats { get; set; } = new CashierStatsViewModel();

        /// <summary>
        /// لیست منشی‌های برتر
        /// </summary>
        public List<CashierRanking> TopPerformers { get; set; } = new List<CashierRanking>();

        /// <summary>
        /// رتبه منشی فعلی
        /// </summary>
        public CashierRanking CurrentCashierRanking { get; set; }

        /// <summary>
        /// لیست منشی‌ها برای DropDown
        /// </summary>
        public List<SelectListItem> Cashiers { get; set; } = new List<SelectListItem>();
    }
}

