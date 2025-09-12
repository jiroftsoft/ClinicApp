using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Core;

/// <summary>
/// مدل درخواست‌های کد تأیید (OTP) برای سیستم ورود بدون رمز عبور
/// این مدل فقط از ITrackable ارث می‌برد و از ISoftDelete استفاده نمی‌کند
/// زیرا در سیستم OTP نیازی به سیستم حذف نرم نیست
/// </summary>
public class OtpRequest : ITrackable
{
    public int OtpRequestId { get; set; }

    [Required, StringLength(20)] // ✅ افزایش طول به ۲۰ کاراکتر
    public string PhoneNumber { get; set; }

    [Required]
    public string OtpCodeHash { get; set; } // هش کد در اینجا ذخیره می‌شود

    public DateTime RequestTime { get; set; } = DateTime.Now;

    public int AttemptCount { get; set; } = 1;

    public bool IsVerified { get; set; } = false;

    // پیاده‌سازی ITrackable با ارزش‌های پیش‌فرض
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string UpdatedByUserId { get; set; }

    public ApplicationUser UpdatedByUser { get; set; }
}
/// <summary>
/// پیکربندی مدل درخواست‌های کد تأیید (OTP) برای Entity Framework
/// توجه: این مدل فقط از ITrackable ارث می‌برد و از ISoftDelete استفاده نمی‌کند
/// زیرا در سیستم OTP نیازی به سیستم حذف نرم نیست
/// </summary>
public class OtpRequestConfig : EntityTypeConfiguration<OtpRequest>
{
    public OtpRequestConfig()
    {
        ToTable("OtpRequests");
        HasKey(o => o.OtpRequestId);

        // ویژگی‌های اصلی
        Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(11)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_PhoneNumber")));

        Property(o => o.OtpCodeHash)
            .IsRequired();
        Property(p => p.PhoneNumber)
            .HasMaxLength(20) // Set the new maximum length
            .IsRequired();    // You can chain other configurations

        Property(o => o.RequestTime)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_RequestTime")));

        Property(o => o.AttemptCount)
            .IsRequired();

        Property(o => o.IsVerified)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_IsVerified")));

        // پیاده‌سازی ITrackable
        Property(o => o.CreatedAt)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_CreatedAt")));

        Property(o => o.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_CreatedByUserId")));

        Property(o => o.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_UpdatedAt")));

        Property(o => o.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_OtpRequest_UpdatedByUserId")));

        // روابط
        HasOptional(o => o.CreatedByUser)
            .WithMany()
            .HasForeignKey(o => o.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(o => o.UpdatedByUser)
            .WithMany()
            .HasForeignKey(o => o.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای بهبود عملکرد
        HasIndex(o => new { o.PhoneNumber, o.IsVerified, o.RequestTime })
            .HasName("IX_OtpRequest_PhoneNumber_IsVerified_RequestTime");
    }
}