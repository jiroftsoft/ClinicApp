using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Core;

/// <summary>
/// رابط استاندارد برای ردیابی تاریخ ایجاد و به‌روزرسانی
/// </summary>
public interface ITrackable
{
    DateTime CreatedAt { get; set; }
    string CreatedByUserId { get; set; }
    ApplicationUser CreatedByUser { get; set; }
    DateTime? UpdatedAt { get; set; }
    string UpdatedByUserId { get; set; }
    ApplicationUser UpdatedByUser { get; set; }
}
#region ApplicationUser

/// <summary>
/// پیکربندی مدل کاربران سیستم برای Entity Framework
/// این پیکربندی با توجه به استانداردهای سیستم‌های درمانی طراحی شده است
/// </summary>
public class ApplicationUserConfig : EntityTypeConfiguration<ApplicationUser>
{
    public ApplicationUserConfig()
    {
        ToTable("ApplicationUsers");
        HasKey(u => u.Id);



        Property(p => p.Address)
            .IsOptional()
            .HasMaxLength(500);

        // ویژگی‌های اصلی
        Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_FirstName")));

        Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_LastName")));

        Property(u => u.NationalCode)
        .IsRequired()
        .HasMaxLength(10)
        .HasColumnAnnotation("Index",
            new IndexAnnotation(new IndexAttribute("IX_NationalCode") { IsUnique = true }));

        Property(u => u.IsActive)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_IsActive")));

        // پیاده‌سازی ISoftDelete
        Property(u => u.IsDeleted)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_IsDeleted")));

        Property(u => u.DeletedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_DeletedAt")));

        // پیاده‌سازی ITrackable
        Property(u => u.CreatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_CreatedAt")));

        Property(u => u.CreatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_CreatedByUserId")));

        Property(u => u.UpdatedAt)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UpdatedAt")));

        Property(u => u.UpdatedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UpdatedByUserId")));

        Property(u => u.DeletedByUserId)
            .IsOptional()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_DeletedByUserId")));

        Property(p => p.Gender)
            .IsRequired()
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_Patient_Gender")));

        // ایندکس‌های ترکیبی برای بهبود عملکرد در سیستم‌های پزشکی
        Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_UserName") { IsUnique = true }));

        Property(u => u.Email)
            .IsOptional()
            .HasMaxLength(256)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_Email")));
        // ✅ اضافه شده: تنظیمات شماره تلفن همراه
        Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnAnnotation("Index",
                new IndexAnnotation(new IndexAttribute("IX_ApplicationUser_PhoneNumber") { IsUnique = true }));

        HasMany(u => u.Patients)
            .WithRequired(p => p.ApplicationUser)
            .HasForeignKey(p => p.ApplicationUserId)
            .WillCascadeOnDelete(false);

        // ✅ اصلاح شده: رابطه با تاریخچه اطلاع‌رسانی‌ها
        HasMany(u => u.NotificationHistories)
            .WithRequired(n => n.SenderUser)
            .HasForeignKey(n => n.SenderUserId)
            .WillCascadeOnDelete(false);

        HasOptional(u => u.DeletedByUser)
            .WithMany()
            .HasForeignKey(u => u.DeletedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(u => u.CreatedByUser)
            .WithMany()
            .HasForeignKey(u => u.CreatedByUserId)
            .WillCascadeOnDelete(false);

        HasOptional(u => u.UpdatedByUser)
            .WithMany()
            .HasForeignKey(u => u.UpdatedByUserId)
            .WillCascadeOnDelete(false);

        // ایندکس‌های ترکیبی برای گزارش‌گیری و جستجوهای رایج در سیستم‌های پزشکی
        HasIndex(u => new { u.IsActive, u.IsDeleted })
            .HasName("IX_ApplicationUser_IsActive_IsDeleted");
    }
}

#endregion
