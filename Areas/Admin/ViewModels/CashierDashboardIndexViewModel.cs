using System.Collections.Generic;
using ClinicApp.ViewModels.Admin.PaymentManagement;
using ClinicApp.ViewModels.Payment;
using PaymentStatisticsViewModel = ClinicApp.ViewModels.Admin.PaymentManagement.PaymentStatisticsViewModel;

namespace ClinicApp.Areas.Admin.ViewModels
{
    /// <summary>
    /// مدل نمایش داشبورد صندوق در پنل ادمین — آمار امروز، جلسات باز، اختلاف‌ها، رتبه‌بندی
    /// </summary>
    public class CashierDashboardIndexViewModel
    {
        public CashierDashboardViewModel Dashboard { get; set; } = new CashierDashboardViewModel();
        /// <summary>آمار پرداخت‌های امروز (موفق، در انتظار، ناموفق)</summary>
        public PaymentStatisticsViewModel TodayPaymentStats { get; set; } = new PaymentStatisticsViewModel();
        /// <summary>تعداد جلسات صندوق باز</summary>
        public int OpenSessionsCount { get; set; }
        /// <summary>لیست جلسات باز برای نمایش</summary>
        public List<OpenSessionDisplay> OpenSessions { get; set; } = new List<OpenSessionDisplay>();
        /// <summary>تعداد اختلاف‌های مالی حل‌نشده</summary>
        public int PendingDiscrepancyCount { get; set; }
    }

    /// <summary>
    /// یک ردیف نمایش برای جلسه صندوق باز
    /// </summary>
    public class OpenSessionDisplay
    {
        public int CashSessionId { get; set; }
        public string SessionNumber { get; set; }
        public string UserName { get; set; }
        public string OpenedAtDisplay { get; set; }
    }
}
