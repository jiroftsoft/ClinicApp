using System.Collections.Generic;

namespace ClinicApp.ViewModels.InsuranceTypeUpdate
{
    /// <summary>
    /// ViewModel برای صفحه مدیریت به‌روزرسانی InsuranceType
    /// این ViewModel به صورت Strongly Typed طراحی شده است
    /// </summary>
    public class InsuranceTypeUpdateIndexViewModel
    {
        /// <summary>
        /// آمار کلی طرح‌های بیمه
        /// </summary>
        public InsuranceTypeStatistics Statistics { get; set; }

        /// <summary>
        /// آیا نیاز به به‌روزرسانی وجود دارد
        /// </summary>
        public bool NeedsUpdate { get; set; }

        /// <summary>
        /// پیام موفقیت (اختیاری)
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// پیام خطا (اختیاری)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// پیام هشدار (اختیاری)
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// آیا عملیات در حال انجام است
        /// </summary>
        public bool IsProcessing { get; set; }

        /// <summary>
        /// شناسه همبستگی برای ردیابی عملیات
        /// </summary>
        public string CorrelationId { get; set; }

        public InsuranceTypeUpdateIndexViewModel()
        {
            Statistics = new InsuranceTypeStatistics();
            NeedsUpdate = false;
            IsProcessing = false;
        }
    }

    /// <summary>
    /// آمار طرح‌های بیمه
    /// </summary>
    public class InsuranceTypeStatistics
    {
        /// <summary>
        /// تعداد کل طرح‌های بیمه
        /// </summary>
        public int TotalPlans { get; set; }

        /// <summary>
        /// تعداد بیمه‌های پایه
        /// </summary>
        public int PrimaryPlans { get; set; }

        /// <summary>
        /// تعداد بیمه‌های تکمیلی
        /// </summary>
        public int SupplementaryPlans { get; set; }

        /// <summary>
        /// تعداد طرح‌های نامعتبر (نیاز به به‌روزرسانی)
        /// </summary>
        public int InvalidPlans { get; set; }

        /// <summary>
        /// درصد طرح‌های پایه
        /// </summary>
        public decimal PrimaryPercentage => TotalPlans > 0 ? (decimal)PrimaryPlans / TotalPlans * 100 : 0;

        /// <summary>
        /// درصد طرح‌های تکمیلی
        /// </summary>
        public decimal SupplementaryPercentage => TotalPlans > 0 ? (decimal)SupplementaryPlans / TotalPlans * 100 : 0;

        /// <summary>
        /// درصد طرح‌های نامعتبر
        /// </summary>
        public decimal InvalidPercentage => TotalPlans > 0 ? (decimal)InvalidPlans / TotalPlans * 100 : 0;

        /// <summary>
        /// آیا همه طرح‌ها معتبر هستند
        /// </summary>
        public bool AllPlansValid => InvalidPlans == 0;

        /// <summary>
        /// خلاصه آمار
        /// </summary>
        public string Summary => $"کل: {TotalPlans} | پایه: {PrimaryPlans} | تکمیلی: {SupplementaryPlans} | نامعتبر: {InvalidPlans}";
    }

    /// <summary>
    /// ViewModel برای عملیات به‌روزرسانی
    /// </summary>
    public class InsuranceTypeUpdateOperationViewModel
    {
        /// <summary>
        /// شناسه عملیات
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// نوع عملیات
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// وضعیت عملیات
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// زمان شروع
        /// </summary>
        public System.DateTime StartTime { get; set; }

        /// <summary>
        /// زمان پایان
        /// </summary>
        public System.DateTime? EndTime { get; set; }

        /// <summary>
        /// مدت زمان اجرا (میلی‌ثانیه)
        /// </summary>
        public long? Duration { get; set; }

        /// <summary>
        /// تعداد رکوردهای به‌روزرسانی شده
        /// </summary>
        public int UpdatedRecords { get; set; }

        /// <summary>
        /// تعداد خطاها
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// آیا عملیات موفق بوده است
        /// </summary>
        public bool IsSuccess => Status == "Success";

        /// <summary>
        /// آیا عملیات در حال انجام است
        /// </summary>
        public bool IsInProgress => Status == "InProgress";

        /// <summary>
        /// آیا عملیات با خطا مواجه شده است
        /// </summary>
        public bool HasError => Status == "Error";
    }
}
