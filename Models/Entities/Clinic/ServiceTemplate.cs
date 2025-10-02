using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicApp.Models.Core;

namespace ClinicApp.Models.Entities.Clinic;

/// <summary>
/// مدل قالب خدمات - برای مدیریت مقادیر پیش‌فرض اجزای خدمات
/// 
/// این مدل بهترین روش برای مدیریت مقادیر پیش‌فرض است چون:
/// 1. انعطاف‌پذیری: تغییرات بدون تغییر کد
/// 2. قابلیت نگهداری: مدیریت آسان
/// 3. مقیاس‌پذیری: پشتیبانی از خدمات جدید
/// 4. ردیابی تغییرات: امکان Audit Trail
/// </summary>
public class ServiceTemplate : ISoftDelete, ITrackable
{
    /// <summary>
    /// شناسه قالب خدمت
    /// </summary>
    public int ServiceTemplateId { get; set; }

    /// <summary>
    /// کد خدمت (مطابق با مصوبه 1404)
    /// </summary>
    [Required(ErrorMessage = "کد خدمت الزامی است.")]
    [StringLength(50, ErrorMessage = "کد خدمت نمی‌تواند بیش از 50 کاراکتر باشد.")]
    [Index("IX_ServiceTemplate_ServiceCode", IsUnique = true)]
    public string ServiceCode { get; set; }

    /// <summary>
    /// نام خدمت
    /// </summary>
    [Required(ErrorMessage = "نام خدمت الزامی است.")]
    [StringLength(200, ErrorMessage = "نام خدمت نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string ServiceName { get; set; }

    /// <summary>
    /// ضریب فنی پیش‌فرض
    /// </summary>
    /// <summary>ضریب فنی پیش‌فرض</summary>
    [Required]
    [Range(typeof(decimal), "0", "999999.9999", ErrorMessage = "ضریب فنی باید بین 0 تا 999999.9999 باشد.")]
    [Column(TypeName = "decimal")] // Precision در Fluent تعیین می‌شود
    public decimal DefaultTechnicalCoefficient { get; set; }

    /// <summary>
    /// ضریب حرفه‌ای پیش‌فرض
    /// </summary>
    /// <summary>ضریب حرفه‌ای پیش‌فرض</summary>
    [Required]
    [Range(typeof(decimal), "0", "999999.9999", ErrorMessage = "ضریب حرفه‌ای باید بین 0 تا 999999.9999 باشد.")]
    [Column(TypeName = "decimal")]
    public decimal DefaultProfessionalCoefficient { get; set; }

    /// <summary>
    /// توضیحات قالب
    /// </summary>
    [StringLength(500, ErrorMessage = "توضیحات نمی‌تواند بیش از 500 کاراکتر باشد.")]
    public string Description { get; set; }

    /// <summary>
    /// آیا هشتگ‌دار است؟ (برای تشخیص نوع کای فنی)
    /// </summary>
    public bool IsHashtagged { get; set; } = false;

    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    #region پیاده‌سازی ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedByUserId { get; set; }
    
    /// <summary>
    /// کاربری که این رکورد را حذف کرده است
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedByUserId { get; set; }
    
    /// <summary>
    /// کاربری که این رکورد را ایجاد کرده است
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }
    
    /// <summary>
    /// کاربری که این رکورد را آخرین بار به‌روزرسانی کرده است
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion
}
