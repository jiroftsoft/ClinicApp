using ClinicApp.Models.Entities.Clinic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace ClinicApp.Models.Configurations
{
    public class ServiceTemplateConfig : EntityTypeConfiguration<ServiceTemplate>
    {
        public ServiceTemplateConfig()
        {
            ToTable("ServiceTemplates");
            HasKey(st => st.ServiceTemplateId);

            Property(st => st.ServiceTemplateId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // کد خدمت (Unique)
            Property(st => st.ServiceCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(
                        new IndexAttribute("IX_ServiceTemplate_ServiceCode") { IsUnique = true }
                    ));

            // نام خدمت
            Property(st => st.ServiceName)
                .IsRequired()
                .HasMaxLength(200);

            // ضرایب: سبک‌تر و کافی
            Property(st => st.DefaultTechnicalCoefficient)
                .IsRequired()
                .HasPrecision(8, 4);

            Property(st => st.DefaultProfessionalCoefficient)
                .IsRequired()
                .HasPrecision(8, 4);

            // توضیحات
            Property(st => st.Description)
                .IsOptional()
                .HasMaxLength(500);

            // فلگ‌ها
            Property(st => st.IsHashtagged).IsRequired();
            Property(st => st.IsActive).IsRequired();

            // Soft Delete
            Property(st => st.IsDeleted).IsRequired();

            Property(st => st.DeletedAt)
                .IsOptional();

            // طول‌های سازگار با سایر مدل‌ها (Identity قدیمی)
            Property(st => st.DeletedByUserId)
                .IsOptional()
                .HasMaxLength(128);

            // Audit
            Property(st => st.CreatedAt)
                .IsRequired();

            Property(st => st.CreatedByUserId)
                .IsOptional()
                .HasMaxLength(128);

            Property(st => st.UpdatedAt)
                .IsOptional();

            Property(st => st.UpdatedByUserId)
                .IsOptional()
                .HasMaxLength(128);

            // روابط کاربران (Cascade off)
            HasOptional(st => st.DeletedByUser).WithMany().HasForeignKey(st => st.DeletedByUserId).WillCascadeOnDelete(false);
            HasOptional(st => st.CreatedByUser).WithMany().HasForeignKey(st => st.CreatedByUserId).WillCascadeOnDelete(false);
            HasOptional(st => st.UpdatedByUser).WithMany().HasForeignKey(st => st.UpdatedByUserId).WillCascadeOnDelete(false);

            // ✅ تنها یک ایندکس مرکّب (برای لیست‌های رایج)
            // WHERE IsDeleted = 0 AND IsActive = 1
            Property(st => st.IsDeleted)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute("IX_ServiceTemplate_IsDeleted_IsActive", 1)
                    }));

            Property(st => st.IsActive)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute("IX_ServiceTemplate_IsDeleted_IsActive", 2)
                    }));

            // در صورت نیاز: ایندکس تکی IsHashtagged (اگر فیلتر پرتکرار دارید)
            Property(st => st.IsHashtagged)
                .HasColumnAnnotation("Index",
                    new IndexAnnotation(new IndexAttribute("IX_ServiceTemplate_IsHashtagged")));
        }
    }
}
