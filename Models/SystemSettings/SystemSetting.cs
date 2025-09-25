using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicApp.Models.SystemSettings
{
    /// <summary>
    /// مدل تنظیمات سیستم
    /// </summary>
    [Table("SystemSettings")]
    public class SystemSetting
    {
        /// <summary>
        /// شناسه یکتای تنظیمات
        /// </summary>
        [Key]
        public int SystemSettingId { get; set; }

        /// <summary>
        /// کلید تنظیمات
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; }

        /// <summary>
        /// مقدار تنظیمات
        /// </summary>
        [Required]
        [StringLength(500)]
        public string SettingValue { get; set; }

        /// <summary>
        /// نوع داده تنظیمات
        /// </summary>
        [Required]
        [StringLength(50)]
        public string DataType { get; set; }

        /// <summary>
        /// دسته‌بندی تنظیمات
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        /// <summary>
        /// توضیحات تنظیمات
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// آیا تنظیمات فعال است
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// شناسه کاربر ایجادکننده
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین بروزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر آخرین بروزرسانی‌کننده
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// تاریخ شروع اعتبار
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        /// <summary>
        /// تاریخ پایان اعتبار
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// شناسه کلینیک (برای تنظیمات خاص کلینیک)
        /// </summary>
        public int? ClinicId { get; set; }
    }

    /// <summary>
    /// انواع دسته‌بندی تنظیمات
    /// </summary>
    public static class SettingCategories
    {
        public const string Financial = "Financial";
        public const string Insurance = "Insurance";
        public const string Calculation = "Calculation";
        public const string System = "System";
        public const string UI = "UI";
        public const string Security = "Security";
    }

    /// <summary>
    /// کلیدهای تنظیمات سیستم
    /// </summary>
    public static class SettingKeys
    {
        // تنظیمات مالی
        public const string CurrentFinancialYear = "CurrentFinancialYear";
        public const string FinancialYearStartMonth = "FinancialYearStartMonth";
        public const string FinancialYearStartDay = "FinancialYearStartDay";
        
        // تنظیمات بیمه
        public const string DefaultCoveragePercent = "DefaultCoveragePercent";
        public const string MaxCoveragePercent = "MaxCoveragePercent";
        public const string MinCoveragePercent = "MinCoveragePercent";
        
        // تنظیمات محاسبه
        public const string DefaultTechnicalFactor = "DefaultTechnicalFactor";
        public const string DefaultProfessionalFactor = "DefaultProfessionalFactor";
        public const string CalculationPrecision = "CalculationPrecision";
        
        // تنظیمات سیستم
        public const string SystemName = "SystemName";
        public const string SystemVersion = "SystemVersion";
        public const string MaintenanceMode = "MaintenanceMode";
        
        // تنظیمات امنیت
        public const string SessionTimeout = "SessionTimeout";
        public const string MaxLoginAttempts = "MaxLoginAttempts";
        public const string PasswordExpiryDays = "PasswordExpiryDays";
    }
}
