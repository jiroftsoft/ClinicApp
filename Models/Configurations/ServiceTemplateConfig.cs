using ClinicApp.Models.Entities.Clinic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Configurations
{
    /// <summary>
    /// پیکربندی Entity Framework برای ServiceTemplate
    /// </summary>
    public class ServiceTemplateConfig : EntityTypeConfiguration<ServiceTemplate>
    {
        public ServiceTemplateConfig()
        {
            // جدول و کلید
            ToTable("ServiceTemplates");
            HasKey(st => st.ServiceTemplateId);

            Property(st => st.ServiceTemplateId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // کد خدمت (Unique)
            Property(st => st.ServiceCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_ServiceCode") { IsUnique = true }));

            // نام خدمت
            Property(st => st.ServiceName)
                .IsRequired()
                .HasMaxLength(200);

            // ضرایب ـ با دقت بالاتر (18,4) برای محاسبات مطمئن
            Property(st => st.DefaultTechnicalCoefficient)
                .IsRequired()
                .HasPrecision(18, 4);
            // در صورت نیاز به فیلتر/سورت روی این ستون، این ایندکس را فعال کن:
            //.HasColumnAnnotation("Index",
            //    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_TechnicalCoefficient")));

            Property(st => st.DefaultProfessionalCoefficient)
                .IsRequired()
                .HasPrecision(18, 4);
            // در صورت نیاز به فیلتر/سورت روی این ستون، این ایندکس را فعال کن:
            //.HasColumnAnnotation("Index",
            //    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_ProfessionalCoefficient")));

            // توضیحات
            Property(st => st.Description)
                .IsOptional()
                .HasMaxLength(500);

            // فلگ‌ها و ایندکس‌ها
            Property(st => st.IsHashtagged)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsHashtagged")));

            Property(st => st.IsActive)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsActive")));

            // Soft Delete
            Property(st => st.IsDeleted)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsDeleted")));

            Property(st => st.DeletedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_DeletedAt")));

            Property(st => st.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(450);

            // Audit
            Property(st => st.CreatedAt)
                .IsRequired()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_CreatedAt")));

            // در محیط‌های واقعی بهتره Optional باشه تا عملیات سیستمی بلاک نشه
            Property(st => st.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(450)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_CreatedByUserId")));

            Property(st => st.UpdatedAt)
                .IsOptional()
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_UpdatedAt")));

            Property(st => st.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(450);

            // ایندکس مرکّب پرکاربرد برای لیست‌/جستجو:
            // WHERE IsDeleted = 0 AND IsActive = 1
            Property(st => st.IsDeleted)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsDeleted_IsActive", 1)));
            Property(st => st.IsActive)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsDeleted_IsActive", 2)));

            // روابط کاربران (Cascade off)
            HasOptional(st => st.DeletedByUser).WithMany().HasForeignKey(st => st.DeletedByUserId).WillCascadeOnDelete(false);
            HasOptional(st => st.CreatedByUser).WithMany().HasForeignKey(st => st.CreatedByUserId).WillCascadeOnDelete(false);
            HasOptional(st => st.UpdatedByUser).WithMany().HasForeignKey(st => st.UpdatedByUserId).WillCascadeOnDelete(false);
        }
    }
}