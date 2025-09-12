using System;
using ClinicApp.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.Models.Enums;

namespace ClinicApp.Models.Entities.Doctor;

/// <summary>
/// موجودیت تاریخچه انتسابات پزشکان
/// این موجودیت برای ردیابی کامل تمام تغییرات انتسابات پزشکان طراحی شده است
/// 
/// ویژگی‌های کلیدی:
/// 1. ردیابی کامل تمام عملیات انتساب (ایجاد، ویرایش، حذف، انتقال)
/// 2. ذخیره جزئیات کامل هر عملیات برای حسابرسی
/// 3. ارتباط با موجودیت‌های مرتبط (پزشک، دپارتمان، کاربر)
/// 4. پشتیبانی از سیستم حذف نرم برای حفظ اطلاعات تاریخی
/// 5. ردیابی کامل برای سیستم‌های پزشکی
/// 
/// نکته حیاتی: این موجودیت برای رعایت استانداردهای پزشکی و حسابرسی طراحی شده است
/// </summary>
public class DoctorAssignmentHistory : ITrackable, ISoftDelete
{
    /// <summary>
    /// شناسه یکتای تاریخچه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پزشک مرتبط
    /// </summary>
    [Required(ErrorMessage = "شناسه پزشک الزامی است.")]
    public int DoctorId { get; set; }

    /// <summary>
    /// نوع عملیات انجام شده
    /// مثال: "Create", "Update", "Delete", "Transfer", "Assign", "Remove"
    /// </summary>
    [Required(ErrorMessage = "نوع عملیات الزامی است.")]
    [MaxLength(50, ErrorMessage = "نوع عملیات نمی‌تواند بیش از 50 کاراکتر باشد.")]
    public string ActionType { get; set; }

    /// <summary>
    /// عنوان عملیات برای نمایش
    /// مثال: "انتساب به دپارتمان اورژانس", "انتقال به دپارتمان داخلی"
    /// </summary>
    [Required(ErrorMessage = "عنوان عملیات الزامی است.")]
    [MaxLength(200, ErrorMessage = "عنوان عملیات نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string ActionTitle { get; set; }

    /// <summary>
    /// توضیحات تفصیلی عملیات
    /// شامل جزئیات کامل تغییرات انجام شده
    /// </summary>
    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string ActionDescription { get; set; }

    /// <summary>
    /// تاریخ و زمان انجام عملیات
    /// </summary>
    [Required(ErrorMessage = "تاریخ عملیات الزامی است.")]
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// شناسه دپارتمان مرتبط (اختیاری)
    /// برای عملیات‌هایی که مربوط به دپارتمان خاصی هستند
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// نام دپارتمان در زمان عملیات
    /// برای حفظ اطلاعات تاریخی حتی اگر دپارتمان حذف شود
    /// </summary>
    [MaxLength(200, ErrorMessage = "نام دپارتمان نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string DepartmentName { get; set; }

    /// <summary>
    /// صلاحیت‌های خدماتی مرتبط
    /// ذخیره به صورت JSON string برای حفظ اطلاعات تاریخی
    /// </summary>
    [MaxLength(2000, ErrorMessage = "صلاحیت‌های خدماتی نمی‌تواند بیش از 2000 کاراکتر باشد.")]
    public string ServiceCategories { get; set; }

    /// <summary>
    /// شناسه کاربر انجام‌دهنده عملیات
    /// </summary>
    [Required(ErrorMessage = "شناسه کاربر انجام‌دهنده الزامی است.")]
    [MaxLength(128, ErrorMessage = "شناسه کاربر نمی‌تواند بیش از 128 کاراکتر باشد.")]
    public string PerformedByUserId { get; set; }

    /// <summary>
    /// نام کاربر انجام‌دهنده
    /// برای حفظ اطلاعات تاریخی
    /// </summary>
    [MaxLength(200, ErrorMessage = "نام کاربر نمی‌تواند بیش از 200 کاراکتر باشد.")]
    public string PerformedByUserName { get; set; }

    /// <summary>
    /// یادداشت‌های اضافی
    /// برای توضیحات تکمیلی یا دلایل انجام عملیات
    /// </summary>
    [MaxLength(1000, ErrorMessage = "یادداشت نمی‌تواند بیش از 1000 کاراکتر باشد.")]
    public string Notes { get; set; }

    /// <summary>
    /// داده‌های قبلی (قبل از تغییر)
    /// ذخیره به صورت JSON string برای مقایسه
    /// </summary>
    [MaxLength(4000, ErrorMessage = "داده‌های قبلی نمی‌تواند بیش از 4000 کاراکتر باشد.")]
    public string PreviousData { get; set; }

    /// <summary>
    /// داده‌های جدید (بعد از تغییر)
    /// ذخیره به صورت JSON string برای مقایسه
    /// </summary>
    [MaxLength(4000, ErrorMessage = "داده‌های جدید نمی‌تواند بیش از 4000 کاراکتر باشد.")]
    public string NewData { get; set; }

    /// <summary>
    /// سطح اهمیت عملیات
    /// برای فیلتر کردن عملیات‌های مهم
    /// </summary>
    [Required(ErrorMessage = "سطح اهمیت الزامی است.")]
    public AssignmentHistoryImportance Importance { get; set; } = AssignmentHistoryImportance.Normal;

    #region پیاده‌سازی ISoftDelete (سیستم حذف نرم)
    /// <summary>
    /// نشان‌دهنده وضعیت حذف شدن رکورد
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ و زمان حذف
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// شناسه کاربر حذف‌کننده
    /// </summary>
    [MaxLength(128, ErrorMessage = "شناسه کاربر حذف‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
    public string DeletedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر حذف‌کننده
    /// </summary>
    public virtual ApplicationUser DeletedByUser { get; set; }
    #endregion

    #region پیاده‌سازی ITrackable (مدیریت ردیابی)
    /// <summary>
    /// تاریخ و زمان ایجاد رکورد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// شناسه کاربر ایجاد‌کننده
    /// </summary>
    [MaxLength(128, ErrorMessage = "شناسه کاربر ایجاد‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ایجاد‌کننده
    /// </summary>
    public virtual ApplicationUser CreatedByUser { get; set; }

    /// <summary>
    /// تاریخ و زمان آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// شناسه کاربر ویرایش‌کننده
    /// </summary>
    [MaxLength(128, ErrorMessage = "شناسه کاربر ویرایش‌کننده نمی‌تواند بیش از 128 کاراکتر باشد.")]
    public string UpdatedByUserId { get; set; }

    /// <summary>
    /// ارجاع به کاربر ویرایش‌کننده
    /// </summary>
    public virtual ApplicationUser UpdatedByUser { get; set; }
    #endregion

    #region روابط
    /// <summary>
    /// ارجاع به پزشک مرتبط
    /// </summary>
    public virtual Doctor Doctor { get; set; }

    /// <summary>
    /// ارجاع به دپارتمان مرتبط
    /// </summary>
    public virtual Department Department { get; set; }

    /// <summary>
    /// ارجاع به کاربر انجام‌دهنده
    /// </summary>
    public virtual ApplicationUser PerformedByUser { get; set; }
    #endregion
}

/// <summary>
/// پیکربندی مدل تاریخچه انتسابات پزشکان برای Entity Framework
/// </summary>
public class DoctorAssignmentHistoryConfig : EntityTypeConfiguration<DoctorAssignmentHistory>
{
    public DoctorAssignmentHistoryConfig()
    {
        ToTable("DoctorAssignmentHistories");
        HasKey(h => h.Id);

        // ویژگی‌های اصلی
        Property(h => h.ActionType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_ActionType")));

        Property(h => h.ActionTitle)
            .IsRequired()
            .HasMaxLength(200);

        Property(h => h.ActionDescription)
            .IsOptional()
            .HasMaxLength(1000);

        Property(h => h.ActionDate)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_ActionDate")));

        Property(h => h.DepartmentName)
            .IsOptional()
            .HasMaxLength(200);

        Property(h => h.ServiceCategories)
            .IsOptional()
            .HasMaxLength(2000);

        Property(h => h.PerformedByUserId)
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_PerformedByUserId")));

        Property(h => h.PerformedByUserName)
            .IsOptional()
            .HasMaxLength(200);

        Property(h => h.Notes)
            .IsOptional()
            .HasMaxLength(1000);

        Property(h => h.PreviousData)
            .IsOptional()
            .HasMaxLength(4000);

        Property(h => h.NewData)
            .IsOptional()
            .HasMaxLength(4000);

        Property(h => h.Importance)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_Importance")));

        // پیاده‌سازی ISoftDelete
        Property(h => h.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_IsDeleted")));

        Property(h => h.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_DeletedAt")));

        Property(h => h.DeletedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_DeletedByUserId")));

        // پیاده‌سازی ITrackable
        Property(h => h.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_CreatedAt")));

        Property(h => h.CreatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_CreatedByUserId")));

        Property(h => h.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_UpdatedAt")));

        Property(h => h.UpdatedByUserId)
            .IsOptional()
            .HasMaxLength(128)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_DoctorAssignmentHistory_UpdatedByUserId")));

        // روابط
        HasRequired(h => h.Doctor)
            .WithMany()
            .HasForeignKey(h => h.DoctorId)
            .WillCascadeOnDelete(false);

        HasOptional(h => h.Department)
            .WithMany()
            .HasForeignKey(h => h.DepartmentId)
            .WillCascadeOnDelete(false);

        HasRequired(h => h.PerformedByUser)
            .WithMany()
            .HasForeignKey(h => h.PerformedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(h => h.CreatedByUser)
            .WithMany()
            .HasForeignKey(h => h.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(h => h.UpdatedByUser)
            .WithMany()
            .HasForeignKey(h => h.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(h => h.DeletedByUser)
            .WithMany()
            .HasForeignKey(h => h.DeletedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(h => new { h.DoctorId, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_DoctorId_ActionDate");

        HasIndex(h => new { h.ActionType, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_ActionType_ActionDate");

        HasIndex(h => new { h.DepartmentId, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_DepartmentId_ActionDate");

        HasIndex(h => new { h.PerformedByUserId, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_PerformedByUserId_ActionDate");

        HasIndex(h => new { h.Importance, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_Importance_ActionDate");

        HasIndex(h => new { h.IsDeleted, h.ActionDate })
            .HasName("IX_DoctorAssignmentHistory_IsDeleted_ActionDate");
    }
}