using System;
using System.Collections.Generic;

namespace ClinicApp
{
    /// <summary>
    /// ثابت‌های ماژول پذیرش - طراحی شده برای محیط درمانی
    /// مسئولیت: مدیریت تمام ثابت‌های ماژول پذیرش بدون Hard Code
    /// </summary>
    public static class ReceptionConstants
    {
        #region Alert Types

        /// <summary>
        /// انواع هشدارهای پزشکی
        /// </summary>
        public static class AlertTypes
        {
            public const string Emergency = "emergency";
            public const string Warning = "warning";
            public const string Info = "info";
        }

        /// <summary>
        /// اولویت‌های هشدار
        /// </summary>
        public static class AlertPriorities
        {
            public const string High = "high";
            public const string Medium = "medium";
            public const string Low = "low";
        }

        #endregion

        #region Icons

        /// <summary>
        /// آیکون‌های سیستم
        /// </summary>
        public static class Icons
        {
            public const string Ambulance = "fas fa-ambulance";
            public const string Clock = "fas fa-clock";
            public const string Tools = "fas fa-tools";
            public const string History = "fas fa-history";
            public const string UserSearch = "fas fa-user-search";
            public const string Receipt = "fas fa-receipt";
            public const string ChartLine = "fas fa-chart-line";
            public const string Cogs = "fas fa-cogs";
        }

        #endregion

        #region Status Values

        /// <summary>
        /// وضعیت‌های پذیرش
        /// </summary>
        public static class ReceptionStatuses
        {
            public const string Pending = "در انتظار";
            public const string Completed = "تکمیل شده";
            public const string Cancelled = "لغو شده";
        }

        /// <summary>
        /// وضعیت‌های پرداخت
        /// </summary>
        public static class PaymentStatuses
        {
            public const string Pending = "در انتظار پرداخت";
            public const string Paid = "پرداخت شده";
            public const string Partial = "پرداخت جزئی";
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// قوانین کسب‌وکار
        /// </summary>
        public static class BusinessRules
        {
            public const decimal ToleranceAmount = 0.01m;
            public const int DefaultQuantity = 1;
            public const int ReceptionNumberLength = 4;
        }

        /// <summary>
        /// محدودیت‌های اعتبارسنجی
        /// </summary>
        public static class ValidationLimits
        {
            public const int MinPatientId = 1;
            public const decimal MinAmount = 0.01m;
            public const int MaxReceptionItems = 50;
        }

        #endregion

        #region Default Values

        /// <summary>
        /// مقادیر پیش‌فرض
        /// </summary>
        public static class DefaultValues
        {
            public const string ReceptionNumberPrefix = "REC";
            public const string SystemUserId = "SYSTEM";
            public const string DefaultNotes = "";
        }

        #endregion

        #region Sample Data

        /// <summary>
        /// داده‌های نمونه برای تست و نمایش
        /// </summary>
        public static class SampleData
        {
            public static readonly List<AlertSample> MedicalAlerts = new List<AlertSample>
            {
                new AlertSample
                {
                    Type = AlertTypes.Emergency,
                    Title = "بیمار اورژانسی در انتظار",
                    Message = "بیمار با وضعیت اورژانسی در انتظار ویزیت است",
                    Priority = AlertPriorities.High,
                    Icon = Icons.Ambulance
                },
                new AlertSample
                {
                    Type = AlertTypes.Warning,
                    Title = "نوبت‌های عقب‌افتاده",
                    Message = "3 نوبت عقب‌افتاده نیاز به رسیدگی دارد",
                    Priority = AlertPriorities.Medium,
                    Icon = Icons.Clock
                },
                new AlertSample
                {
                    Type = AlertTypes.Info,
                    Title = "تجهیزات پزشکی",
                    Message = "تجهیزات پزشکی نیاز به سرویس دارند",
                    Priority = AlertPriorities.Low,
                    Icon = Icons.Tools
                }
            };

            public static readonly List<QuickActionSample> QuickActions = new List<QuickActionSample>
            {
                new QuickActionSample
                {
                    Title = "سوابق پذیرش",
                    Description = "مشاهده سوابق پذیرش بیماران",
                    Icon = Icons.History,
                    Action = "openReceptionHistory"
                },
                new QuickActionSample
                {
                    Title = "جستجوی بیمار",
                    Description = "جستجوی سریع بیماران",
                    Icon = Icons.UserSearch,
                    Action = "openPatientSearch"
                },
                new QuickActionSample
                {
                    Title = "سوابق پرداخت",
                    Description = "مشاهده سوابق پرداخت‌ها",
                    Icon = Icons.Receipt,
                    Action = "openPaymentHistory"
                },
                new QuickActionSample
                {
                    Title = "گزارش‌های آماری",
                    Description = "مشاهده گزارش‌های آماری",
                    Icon = Icons.ChartLine,
                    Action = "openReports"
                },
                new QuickActionSample
                {
                    Title = "تنظیمات سیستم",
                    Description = "تنظیمات سیستم پذیرش",
                    Icon = Icons.Cogs,
                    Action = "openSettings"
                }
            };
        }

        #endregion
    }

    /// <summary>
    /// نمونه هشدار
    /// </summary>
    public class AlertSample
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; }
        public string Icon { get; set; }
    }

    /// <summary>
    /// نمونه اقدام سریع
    /// </summary>
    public class QuickActionSample
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Action { get; set; }
    }
}
